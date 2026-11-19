using System;
using System.Collections.Generic;
using System.Globalization;

namespace MyLifesGame.Motor
{
    public enum AccionRapida { Agua, ComerBien, Ducha }
    public enum SensibilidadPantallas { Nada, Algo, Mucho }

    /// <summary>Una línea del registro del día: qué pasó y cómo quedaron las áreas.</summary>
    [Serializable]
    public class Evento
    {
        public int minutoInicio;
        public int minutoFin;
        public string texto;
        public string nota;
        public float[] valores; // Físico, Mente, Social, Energía, Ánimo
    }

    [Serializable]
    public class BloqueMental
    {
        public int inicio;
        public int fin;
    }

    /// <summary>Todo lo que hay que guardar de la jornada para poder seguir otro día.</summary>
    [Serializable]
    public class DatosJornada
    {
        public float[] areas = { 50, 50, 50, 75, 50 };
        public int minutoActual;
        public int proximaRevision;
        public List<BloqueMental> bloquesMentales = new List<BloqueMental>();
        public int minutosMentalesHoy;
        public int vasosAgua;
        public int comidasBuenas;
        public bool duchaHecha;
        public int finUltimaAccionSueno = -100000;
        public int[] ultimoCuidado = new int[5];
        public int diasSeguidosEntrenando;
        public int ultimoDiaEntreno = -10;
        public int pasosHoy;
        public bool diaPesado;
        public bool checkInHecho;
        public List<int> focos = new List<int>();
        public SensibilidadPantallas pantallas = SensibilidadPantallas.Algo;
        public int metaAgua = Ajustes.MetaAguaPorDefecto;
        public List<Evento> eventos = new List<Evento>();
    }

    /// <summary>Lo que ha cambiado al terminar una acción, para la pantalla de Resultado.</summary>
    public class ResultadoAccion
    {
        public DefinicionAccion accion;
        public int minutos;
        public TramoDosis tramo;
        public bool posterior;
        public readonly float[] cambios = new float[5];
        public readonly List<string> notas = new List<string>();
        public int xp;
        public int monedas;
        public readonly List<string> mensajes = new List<string>(); // recompensas, misiones y logros
        public bool DosisOptima { get { return tramo == TramoDosis.ZonaOptima && !posterior; } }
    }

    /// <summary>
    /// El día a día del jugador. Aplica las reglas del equilibrio (docs/fase-1-acciones-y-curvas.md, v0.4):
    /// dosis, reglas 1 a 7, fatiga mental, desgaste de Energía, check-in, minipreguntas, pasos y sueño.
    /// </summary>
    public class Jornada
    {
        public DatosJornada Datos { get; private set; }
        public EstadoAreas Estado { get; private set; }
        public int MinutoActual { get { return Datos.minutoActual; } }
        public List<Evento> Eventos { get { return Datos.eventos; } }
        public int Dia { get { return Datos.minutoActual / 1440 + 1; } }

        private readonly TablaAcciones tabla;

        /// <summary>Retoma una jornada guardada.</summary>
        public Jornada(TablaAcciones tabla, DatosJornada datos)
        {
            this.tabla = tabla;
            Datos = datos;
            Estado = new EstadoAreas(datos.areas);
        }

        /// <summary>Empieza una partida nueva.</summary>
        public static Jornada Nueva(TablaAcciones tabla, float[] valoresIniciales, int minutoDespertar)
        {
            var d = new DatosJornada();
            d.areas = (float[])valoresIniciales.Clone();
            d.minutoActual = minutoDespertar;
            d.proximaRevision = minutoDespertar + Ajustes.MinutosEntreRevisiones;
            for (int i = 0; i < 5; i++) d.ultimoCuidado[i] = minutoDespertar;
            var j = new Jornada(tabla, d);
            j.Registrar(minutoDespertar, minutoDespertar, "Empieza tu partida", "");
            return j;
        }

        // ---------------------------------------------------------------- tiempo

        /// <summary>Hace pasar el tiempo: desgaste de Energía, desgaste tras 48 h sin cuidar un área y revisiones cada 6 h.</summary>
        public void Avanzar(int hasta)
        {
            while (Datos.minutoActual < hasta)
            {
                int siguiente = Math.Min(hasta, Datos.proximaRevision);
                int minutos = siguiente - Datos.minutoActual;
                Estado.Sumar(Area.Energia, -Ajustes.DesgasteEnergiaPorHora * minutos / 60f);
                DesgastePorDescuido(Area.Fisico, Ajustes.DesgasteFisicoPorDia, minutos, siguiente);
                DesgastePorDescuido(Area.Mente, Ajustes.DesgasteMentePorDia, minutos, siguiente);
                DesgastePorDescuido(Area.Social, Ajustes.DesgasteSocialPorDia, minutos, siguiente);
                Datos.minutoActual = siguiente;
                if (Datos.minutoActual == Datos.proximaRevision)
                {
                    RevisarEquilibrio();
                    Datos.proximaRevision += Ajustes.MinutosEntreRevisiones;
                }
            }
        }

        // Regla 3: solo baja si llevas más de 48 h sin cuidar el área, y nunca por debajo de 10.
        private void DesgastePorDescuido(Area area, float porDia, int minutos, int hasta)
        {
            int limite = Datos.ultimoCuidado[(int)area] + Ajustes.MinutosGracia;
            int minutosDescuidados = Math.Min(minutos, hasta - limite);
            if (minutosDescuidados <= 0) return;
            if (Estado[area] <= Ajustes.MinimoPorDesgaste) return;
            float baja = porDia * minutosDescuidados / Ajustes.MinutosDespiertoPorDia;
            Estado[area] = Math.Max(Ajustes.MinimoPorDesgaste, Estado[area] - baja);
        }

        /// <summary>Tiempo que pasa en pausa (ausencias): no hay desgaste ni castigo.</summary>
        public void SaltarTiempo(int minutos)
        {
            if (minutos <= 0) return;
            Datos.minutoActual += minutos;
            Datos.proximaRevision += minutos;
            for (int i = 0; i < 5; i++) Datos.ultimoCuidado[i] += minutos;
        }

        // ---------------------------------------------------------------- acciones

        /// <summary>Hace una acción con cronómetro y devuelve qué ha cambiado y por qué.</summary>
        public ResultadoAccion HacerAccion(string idAccion, int inicio, int minutos, bool conAmigos = false)
        {
            Avanzar(inicio);
            ResultadoAccion r = CalcularAccion(idAccion, inicio, minutos, conAmigos, 1f);
            Avanzar(inicio + minutos);
            Registrar(inicio, inicio + minutos, r.accion.nombre + " " + Duracion(minutos) + (conAmigos ? " (con amigos)" : ""),
                string.Join("; ", r.notas));
            return r;
        }

        /// <summary>Lo que olvidaste iniciar: cuenta al 70 % y sin bonus de dosis óptima.</summary>
        public ResultadoAccion RegistrarPosterior(string idAccion, int minutos)
        {
            ResultadoAccion r = CalcularAccion(idAccion, Datos.minutoActual - minutos, minutos, false, Ajustes.FactorRegistroPosterior);
            r.posterior = true;
            r.notas.Add("Registrado después: cuenta un poco menos");
            Registrar(Datos.minutoActual, Datos.minutoActual, r.accion.nombre + " " + Duracion(minutos) + " (registrado después)",
                string.Join("; ", r.notas));
            return r;
        }

        private ResultadoAccion CalcularAccion(string idAccion, int inicio, int minutos, bool conAmigos, float factor)
        {
            DefinicionAccion accion = tabla.Buscar(idAccion);
            var r = new ResultadoAccion { accion = accion, minutos = minutos, tramo = accion.Tramo(minutos) };

            // Regla 2: el efecto depende de cómo estás al empezar
            bool sinEnergia = accion.esfuerzo && Estado[Area.Energia] < Ajustes.LimitePoco;
            bool animoBajo = accion.ayudaAnimoBajo && Estado[Area.Animo] < Ajustes.LimitePoco;
            if (sinEnergia) r.notas.Add("Ibas sin energía: rinde menos");
            if (animoBajo) r.notas.Add("Lo necesitabas: te ha sentado mejor");

            // Día pesado (check-in "vengo arrastrando algo")
            bool pesado = Datos.diaPesado && accion.esfuerzo;
            if (pesado) r.notas.Add("Día pesado: cansa un poco más");

            // Regla 4: descanso activo
            bool descansoActivo = accion.desconexion &&
                MinutosMentalesEntre(inicio - Ajustes.VentanaDescansoActivoMin, inicio) >= Ajustes.MinutosMentalesParaDescansoActivo;
            if (descansoActivo) r.notas.Add("Desconectar tras trabajar o estudiar recarga más");

            // Regla 5: días seguidos entrenando
            bool cansancioAcumulado = false;
            if (accion.id == "entrenar")
            {
                int dia = inicio / 1440;
                if (dia == Datos.ultimoDiaEntreno + 1) Datos.diasSeguidosEntrenando++;
                else if (dia != Datos.ultimoDiaEntreno) Datos.diasSeguidosEntrenando = 1;
                Datos.ultimoDiaEntreno = dia;
                if (Datos.diasSeguidosEntrenando >= Ajustes.DiasSeguidosParaCansancio)
                {
                    cansancioAcumulado = true;
                    r.notas.Add("Llevas " + Datos.diasSeguidosEntrenando + " días seguidos: tu cuerpo pide descanso");
                }
            }

            bool fatigaAvisada = false;
            for (int m = 0; m < minutos; m++)
            {
                Efectos tasa = m < accion.demasiadoDesde ? accion.efectoHora : accion.efectoHoraDemasiado;
                foreach (Area area in Areas.Todas)
                {
                    float v = tasa.Get(area) / 60f;
                    if (area == Area.Social && conAmigos) v += accion.socialConAmigosPorHora / 60f;
                    if (sinEnergia)
                    {
                        if (area == Area.Energia && v < 0) v *= Ajustes.FactorCosteSinEnergia;
                        else if (v > 0) v *= Ajustes.FactorBeneficioSinEnergia;
                    }
                    if (pesado && area == Area.Energia && v < 0) v *= Ajustes.FactorDiaPesado;
                    if (animoBajo && area == Area.Animo && v > 0) v *= Ajustes.FactorAnimoSiAnimoBajo;
                    if (cansancioAcumulado && area == Area.Fisico && v > 0) v *= Ajustes.FactorFisicoDiasSeguidos;
                    if (accion.mental && area == Area.Mente && Datos.minutosMentalesHoy + m >= Ajustes.MinutosFatigaMental)
                    {
                        v *= Ajustes.FactorFatigaMental;
                        if (!fatigaAvisada) { r.notas.Add("Mucho esfuerzo mental hoy: rinde menos"); fatigaAvisada = true; }
                    }
                    if (descansoActivo && area == Area.Energia && v > 0 && m < Ajustes.MinutosConBonusDescansoActivo)
                        v += v * Ajustes.ExtraDescansoActivo;
                    r.cambios[(int)area] += v * factor;
                }
            }
            if (cansancioAcumulado) r.cambios[(int)Area.Energia] -= Ajustes.EnergiaExtraDiasSeguidos;

            if (r.tramo == TramoDosis.ZonaOptima && factor >= 1f) r.notas.Add("Dosis óptima");
            if (r.tramo == TramoDosis.Demasiado) r.notas.Add("Te has pasado: cansa más de lo que aporta");

            foreach (Area area in Areas.Todas)
            {
                Estado.Sumar(area, r.cambios[(int)area]);
                if (r.cambios[(int)area] > 0.5f) Datos.ultimoCuidado[(int)area] = Math.Max(Datos.ultimoCuidado[(int)area], inicio + minutos);
            }

            if (accion.mental)
            {
                Datos.bloquesMentales.Add(new BloqueMental { inicio = inicio, fin = inicio + minutos });
                Datos.minutosMentalesHoy += minutos;
            }
            if (accion.afectaSueno) Datos.finUltimaAccionSueno = Math.Max(Datos.finUltimaAccionSueno, inicio + minutos);
            return r;
        }

        /// <summary>
        /// Minipregunta "¿Qué tal ha ido?" (9.1): -1 = peor, 0 = normal, 1 = mejor.
        /// Peor: la acción ha costado más Energía; mejor: ha costado menos.
        /// </summary>
        public void Valorar(ResultadoAccion r, int valoracion, string motivo)
        {
            if (valoracion == 0) return;
            float coste = Math.Max(0f, -r.cambios[(int)Area.Energia]);
            float ajuste = 2f + coste * 0.5f;
            float delta = valoracion < 0 ? -ajuste : ajuste;
            Estado.Sumar(Area.Energia, delta);
            r.cambios[(int)Area.Energia] += delta;
            string texto = valoracion < 0 ? "Hoy te ha costado más: lo tengo en cuenta" : "Te ha ido mejor de lo normal";
            if (!string.IsNullOrEmpty(motivo)) texto += " (" + motivo + ")";
            r.notas.Add(texto);
            Registrar(Datos.minutoActual, Datos.minutoActual, "¿Qué tal ha ido? " + (valoracion < 0 ? "Peor" : "Mejor"), texto);
        }

        /// <summary>Hábitos de un toque, sin cronómetro (regla 7).</summary>
        public void HacerAccionRapida(AccionRapida tipo)
        {
            switch (tipo)
            {
                case AccionRapida.Agua:
                    if (Datos.vasosAgua < Datos.metaAgua) Estado.Sumar(Area.Energia, 1f);
                    Datos.vasosAgua++;
                    Registrar(Datos.minutoActual, Datos.minutoActual, "Vaso de agua (" + Datos.vasosAgua + "/" + Datos.metaAgua + ")", "");
                    break;
                case AccionRapida.ComerBien:
                    if (Datos.comidasBuenas < Ajustes.MaxComidasBuenas)
                    {
                        Estado.Sumar(Area.Energia, 3f);
                        Estado.Sumar(Area.Animo, 1f);
                    }
                    Datos.comidasBuenas++;
                    Registrar(Datos.minutoActual, Datos.minutoActual, "Comer bien", "");
                    break;
                case AccionRapida.Ducha:
                    if (!Datos.duchaHecha)
                    {
                        Estado.Sumar(Area.Animo, 2f);
                        Estado.Sumar(Area.Energia, 2f);
                    }
                    Datos.duchaHecha = true;
                    Registrar(Datos.minutoActual, Datos.minutoActual, "Ducha", "");
                    break;
            }
        }

        /// <summary>Suma pasos dados fuera de las acciones (sin doble conteo). Aplica solo la diferencia.</summary>
        public void SumarPasos(int pasos)
        {
            if (pasos <= 0) return;
            float[] antes = EfectoPasos(Datos.pasosHoy);
            Datos.pasosHoy += pasos;
            float[] despues = EfectoPasos(Datos.pasosHoy);
            Estado.Sumar(Area.Fisico, despues[0] - antes[0]);
            Estado.Sumar(Area.Energia, despues[1] - antes[1]);
            if (despues[0] - antes[0] > 0.01f) Datos.ultimoCuidado[(int)Area.Fisico] = Datos.minutoActual;
        }

        private static float[] EfectoPasos(int pasos)
        {
            float miles = pasos / 1000f;
            float extraZonaBuena = Limitar(miles - 6f, 0f, 6f);
            float fisico = Math.Min(miles, 6f) + extraZonaBuena;
            float energia = extraZonaBuena * 0.5f;
            if (pasos > 18000) energia -= 5f;
            return new[] { fisico, energia };
        }

        /// <summary>Check-in de la mañana. caraSueno y sentir van de 0 (peor) a 4 (mejor).</summary>
        public void CheckIn(int caraSueno, int sentir, List<Area> focos, bool arrastroAlgo)
        {
            Estado[Area.Energia] = Ajustes.EnergiaSegunSueno[Clamp(caraSueno, 0, 4)];
            Estado.Sumar(Area.Animo, Ajustes.AnimoSegunSentir[Clamp(sentir, 0, 4)]);
            Datos.focos.Clear();
            foreach (Area a in focos) Datos.focos.Add((int)a);
            Datos.diaPesado = arrastroAlgo;
            Datos.checkInHecho = true;
            Registrar(Datos.minutoActual, Datos.minutoActual, "Check-in de la mañana", arrastroAlgo ? "Día pesado: el esfuerzo cansa un poco más" : "");
        }

        /// <summary>Dormir: recarga Energía según las horas y la regla 6. Al despertar empieza un día nuevo.</summary>
        public void Dormir(int minutoAcostarse, float horas)
        {
            Avanzar(minutoAcostarse);
            var notas = new List<string>();
            Estado.Sumar(Area.Energia, Ajustes.RecargaSuenoPorHora * Math.Min(horas, 10f));

            float tope = Ajustes.Maximo;
            if (horas < 6f) { tope = 60f; Estado.Sumar(Area.Animo, -5f); notas.Add("Has dormido poco"); }
            else if (horas >= 7f && horas <= 9f) Estado.Sumar(Area.Animo, 5f);
            else if (horas > 10f) { Estado.Sumar(Area.Animo, -3f); notas.Add("Has dormido de más"); }

            bool pantallas = minutoAcostarse - Datos.finUltimaAccionSueno <= 60;
            if (pantallas && Datos.pantallas != SensibilidadPantallas.Nada)
            {
                tope = Math.Min(tope, Datos.pantallas == SensibilidadPantallas.Algo ? 85f : 75f);
                notas.Add("Pantallas antes de dormir: te levantas con menos energía");
            }
            if (Estado[Area.Energia] > tope) Estado[Area.Energia] = tope;

            int despertar = minutoAcostarse + (int)(horas * 60);
            Registrar(minutoAcostarse, despertar, "Dormir " + horas.ToString("0.#", CultureInfo.InvariantCulture) + " h",
                string.Join("; ", notas));

            // Durante el sueño no hay desgaste: el tiempo dormido no consume el periodo de gracia.
            int dormido = despertar - minutoAcostarse;
            for (int i = 0; i < 5; i++) Datos.ultimoCuidado[i] += dormido;
            Datos.minutoActual = despertar;
            Datos.proximaRevision = despertar + Ajustes.MinutosEntreRevisiones;
            Datos.bloquesMentales.Clear();
            Datos.minutosMentalesHoy = 0;
            Datos.vasosAgua = 0;
            Datos.comidasBuenas = 0;
            Datos.duchaHecha = false;
            Datos.pasosHoy = 0;
            Datos.diaPesado = false;
            Datos.checkInHecho = false;
            Datos.focos.Clear();
        }

        /// <summary>Añade una línea al registro con el estado actual.</summary>
        public void Anotar(string texto, string nota = "")
        {
            Registrar(Datos.minutoActual, Datos.minutoActual, texto, nota);
        }

        /// <summary>Se llama cuando alguien se registra en el registro (para el diario de partida).</summary>
        public event Action<Evento> AlRegistrar;

        // Regla 1: cada 6 h despierto, el Ánimo sube si las otras áreas están bien y baja por cada una en "Poco".
        private void RevisarEquilibrio()
        {
            var bajas = new List<string>();
            foreach (Area a in new[] { Area.Fisico, Area.Mente, Area.Social, Area.Energia })
                if (Estado[a] < Ajustes.LimitePoco) bajas.Add(Areas.Nombre(a));

            string nota;
            if (bajas.Count == 0)
            {
                Estado.Sumar(Area.Animo, Ajustes.AnimoPorEquilibrio);
                nota = "Tu equilibrio te sube el ánimo (+3)";
            }
            else
            {
                float baja = Ajustes.AnimoPorAreaBaja * bajas.Count;
                Estado.Sumar(Area.Animo, -baja);
                nota = "Tener " + string.Join(" y ", bajas) + " bajo te pesa (−" + baja + ")";
            }
            Registrar(Datos.minutoActual, Datos.minutoActual, "Revisión del equilibrio", nota);
        }

        private int MinutosMentalesEntre(int desde, int hasta)
        {
            int total = 0;
            foreach (BloqueMental b in Datos.bloquesMentales)
                total += Math.Max(0, Math.Min(b.fin, hasta) - Math.Max(b.inicio, desde));
            return total;
        }

        private void Registrar(int inicio, int fin, string texto, string nota)
        {
            var e = new Evento { minutoInicio = inicio, minutoFin = fin, texto = texto, nota = nota, valores = Estado.Copia() };
            Datos.eventos.Add(e);
            if (Datos.eventos.Count > 200) Datos.eventos.RemoveAt(0);
            if (AlRegistrar != null) AlRegistrar(e);
        }

        private static float Limitar(float v, float min, float max)
        {
            return v < min ? min : (v > max ? max : v);
        }

        private static int Clamp(int v, int min, int max)
        {
            return v < min ? min : (v > max ? max : v);
        }

        public static string Hora(int minuto)
        {
            int h = (minuto / 60) % 24;
            int m = minuto % 60;
            return h.ToString("00") + ":" + m.ToString("00");
        }

        public static string Duracion(int minutos)
        {
            if (minutos < 60) return minutos + " min";
            int h = minutos / 60, m = minutos % 60;
            return m == 0 ? h + " h" : h + " h " + m.ToString("00");
        }
    }
}
