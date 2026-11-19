using System;
using System.Collections.Generic;
using System.Globalization;

namespace MyLifesGame.Motor
{
    public enum AccionRapida { Agua, ComerBien, Ducha }
    public enum SensibilidadPantallas { Nada, Algo, Mucho }

    /// <summary>Una línea del registro del día: qué pasó y cómo quedaron las áreas.</summary>
    public class Evento
    {
        public int minutoInicio;
        public int minutoFin;
        public string texto;
        public string nota;
        public float[] valores; // Físico, Mente, Social, Energía, Ánimo
    }

    /// <summary>Lo que ha cambiado al terminar una acción, para la pantalla de Resultado.</summary>
    public class ResultadoAccion
    {
        public DefinicionAccion accion;
        public int minutos;
        public TramoDosis tramo;
        public readonly float[] cambios = new float[5];
        public readonly List<string> notas = new List<string>();
        public bool DosisOptima { get { return tramo == TramoDosis.ZonaOptima; } }
    }

    /// <summary>
    /// Un día del jugador. Aplica las reglas del equilibrio (docs/fase-1-acciones-y-curvas.md, v0.4):
    /// dosis, regla 1 (Ánimo por equilibrio), regla 2 (según cómo estás), regla 4 (descanso activo),
    /// regla 6 (pantallas antes de dormir), fatiga mental, desgaste de Energía, acciones rápidas, pasos y sueño.
    /// Las reglas de varios días (3 y 5) llegarán con el guardado.
    /// </summary>
    public class Jornada
    {
        public EstadoAreas Estado { get; private set; }
        public int MinutoActual { get; private set; }
        public List<Evento> Eventos { get; private set; }
        public SensibilidadPantallas Pantallas = SensibilidadPantallas.Algo;
        public int MetaAgua = Ajustes.MetaAguaPorDefecto;

        private readonly TablaAcciones tabla;
        private int proximaRevision;
        private readonly List<int[]> bloquesMentales = new List<int[]>(); // [inicio, fin) en minutos
        private int minutosMentalesHoy;
        private int vasosAgua;
        private int comidasBuenas;
        private bool duchaHecha;
        private int finUltimaAccionQueAfectaSueno = int.MinValue / 2;

        public Jornada(TablaAcciones tabla, EstadoAreas inicio, int minutoDespertar)
        {
            this.tabla = tabla;
            Estado = inicio;
            MinutoActual = minutoDespertar;
            Eventos = new List<Evento>();
            proximaRevision = minutoDespertar + Ajustes.MinutosEntreRevisiones;
            Registrar(minutoDespertar, minutoDespertar, "Check-in", "");
        }

        /// <summary>Hace pasar el tiempo: desgaste de Energía y revisiones del equilibrio cada 6 h.</summary>
        public void Avanzar(int hasta)
        {
            while (MinutoActual < hasta)
            {
                int siguiente = Math.Min(hasta, proximaRevision);
                Estado.Sumar(Area.Energia, -Ajustes.DesgasteEnergiaPorHora * (siguiente - MinutoActual) / 60f);
                MinutoActual = siguiente;
                if (MinutoActual == proximaRevision)
                {
                    RevisarEquilibrio();
                    proximaRevision += Ajustes.MinutosEntreRevisiones;
                }
            }
        }

        /// <summary>Hace una acción con cronómetro y devuelve qué ha cambiado y por qué.</summary>
        public ResultadoAccion HacerAccion(string idAccion, int inicio, int minutos, bool conAmigos = false)
        {
            Avanzar(inicio);
            DefinicionAccion accion = tabla.Buscar(idAccion);
            var r = new ResultadoAccion { accion = accion, minutos = minutos, tramo = accion.Tramo(minutos) };

            // Regla 2: el efecto depende de cómo estás al empezar
            bool sinEnergia = accion.esfuerzo && Estado[Area.Energia] < Ajustes.LimitePoco;
            bool animoBajo = accion.ayudaAnimoBajo && Estado[Area.Animo] < Ajustes.LimitePoco;
            if (sinEnergia) r.notas.Add("Ibas sin energía: rinde menos");
            if (animoBajo) r.notas.Add("Lo necesitabas: te ha sentado mejor");

            // Regla 4: descanso activo
            bool descansoActivo = accion.desconexion &&
                MinutosMentalesEntre(inicio - Ajustes.VentanaDescansoActivoMin, inicio) >= Ajustes.MinutosMentalesParaDescansoActivo;
            if (descansoActivo) r.notas.Add("Desconectar tras trabajar o estudiar recarga más");

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
                    if (animoBajo && area == Area.Animo && v > 0) v *= Ajustes.FactorAnimoSiAnimoBajo;
                    if (accion.mental && area == Area.Mente && minutosMentalesHoy + m >= Ajustes.MinutosFatigaMental)
                    {
                        v *= Ajustes.FactorFatigaMental;
                        if (!fatigaAvisada) { r.notas.Add("Mucho esfuerzo mental hoy: rinde menos"); fatigaAvisada = true; }
                    }
                    if (descansoActivo && area == Area.Energia && v > 0 && m < Ajustes.MinutosConBonusDescansoActivo)
                        v += v * Ajustes.ExtraDescansoActivo;
                    r.cambios[(int)area] += v;
                }
            }

            if (r.tramo == TramoDosis.ZonaOptima) r.notas.Add("Dosis óptima");
            if (r.tramo == TramoDosis.Demasiado) r.notas.Add("Te has pasado: cansa más de lo que aporta");

            foreach (Area area in Areas.Todas) Estado.Sumar(area, r.cambios[(int)area]);

            if (accion.mental)
            {
                bloquesMentales.Add(new[] { inicio, inicio + minutos });
                minutosMentalesHoy += minutos;
            }
            if (accion.afectaSueno) finUltimaAccionQueAfectaSueno = inicio + minutos;

            Avanzar(inicio + minutos);
            Registrar(inicio, inicio + minutos, accion.nombre + " " + Duracion(minutos) + (conAmigos ? " (con amigos)" : ""),
                string.Join("; ", r.notas));
            return r;
        }

        /// <summary>Hábitos de un toque, sin cronómetro (regla 7).</summary>
        public void HacerAccionRapida(AccionRapida tipo, int veces = 1)
        {
            for (int i = 0; i < veces; i++)
            {
                switch (tipo)
                {
                    case AccionRapida.Agua:
                        if (vasosAgua < MetaAgua) Estado.Sumar(Area.Energia, 1f);
                        vasosAgua++;
                        break;
                    case AccionRapida.ComerBien:
                        if (comidasBuenas < Ajustes.MaxComidasBuenas)
                        {
                            Estado.Sumar(Area.Energia, 3f);
                            Estado.Sumar(Area.Animo, 1f);
                        }
                        comidasBuenas++;
                        break;
                    case AccionRapida.Ducha:
                        if (!duchaHecha)
                        {
                            Estado.Sumar(Area.Animo, 2f);
                            Estado.Sumar(Area.Energia, 2f);
                        }
                        duchaHecha = true;
                        break;
                }
            }
        }

        /// <summary>Pasos dados fuera de las acciones con cronómetro (sin doble conteo).</summary>
        public void RegistrarPasos(int pasos)
        {
            float miles = pasos / 1000f;
            float extraZonaBuena = Limitar(miles - 6f, 0f, 6f);
            float fisico = Math.Min(miles, 6f) + extraZonaBuena;
            float energia = extraZonaBuena * 0.5f;
            string nota = "";
            if (pasos > 18000) { energia -= 5f; nota = "Muchísimos pasos: cansancio"; }
            Estado.Sumar(Area.Fisico, fisico);
            Estado.Sumar(Area.Energia, energia);
            Registrar(MinutoActual, MinutoActual, pasos.ToString("N0", CultureInfo.GetCultureInfo("es-ES")) + " pasos fuera de acciones", nota);
        }

        /// <summary>Dormir: recarga Energía según las horas y la regla 6 (pantallas antes de dormir).</summary>
        public void Dormir(int minutoAcostarse, float horas)
        {
            Avanzar(minutoAcostarse);
            var notas = new List<string>();
            Estado.Sumar(Area.Energia, Ajustes.RecargaSuenoPorHora * Math.Min(horas, 10f));

            float tope = Ajustes.Maximo;
            if (horas < 6f) { tope = 60f; Estado.Sumar(Area.Animo, -5f); notas.Add("Has dormido poco"); }
            else if (horas >= 7f && horas <= 9f) Estado.Sumar(Area.Animo, 5f);
            else if (horas > 10f) { Estado.Sumar(Area.Animo, -3f); notas.Add("Has dormido de más"); }

            bool pantallas = minutoAcostarse - finUltimaAccionQueAfectaSueno <= 60;
            if (pantallas && Pantallas != SensibilidadPantallas.Nada)
            {
                tope = Math.Min(tope, Pantallas == SensibilidadPantallas.Algo ? 85f : 75f);
                notas.Add("Pantallas antes de dormir: te levantas con menos energía");
            }
            if (Estado[Area.Energia] > tope) Estado[Area.Energia] = tope;

            Registrar(minutoAcostarse, minutoAcostarse + (int)(horas * 60), "Dormir " + horas.ToString("0.#", CultureInfo.InvariantCulture) + " h",
                string.Join("; ", notas));
        }

        /// <summary>Añade una línea al registro con el estado actual (por ejemplo, tras acciones rápidas).</summary>
        public void Anotar(string texto, string nota = "")
        {
            Registrar(MinutoActual, MinutoActual, texto, nota);
        }

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
            Registrar(MinutoActual, MinutoActual, "Revisión del equilibrio", nota);
        }

        private int MinutosMentalesEntre(int desde, int hasta)
        {
            int total = 0;
            foreach (var b in bloquesMentales)
                total += Math.Max(0, Math.Min(b[1], hasta) - Math.Max(b[0], desde));
            return total;
        }

        private void Registrar(int inicio, int fin, string texto, string nota)
        {
            Eventos.Add(new Evento { minutoInicio = inicio, minutoFin = fin, texto = texto, nota = nota, valores = Estado.Copia() });
        }

        private static float Limitar(float v, float min, float max)
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
