using System;
using System.Collections.Generic;

namespace MyLifesGame.Motor
{
    [Serializable]
    public class MisionDia
    {
        public string accionId;
        public int minutos;
        public string titulo;
        public int area;
        public int xp;
        public bool sugerida;
        public string motivo;
        public int minutosHechos;
        public bool hecha;
    }

    [Serializable]
    public class DatosProgreso
    {
        public int xp;
        public int monedas;
        public int accionesTerminadas;
        public int dosisOptimas;
        public List<string> logros = new List<string>();
        public List<string> ropaComprada = new List<string> { "teal" };
        public string ropaPuesta = "teal";
        public List<MisionDia> misiones = new List<MisionDia>();
        public int diaMisiones = -1;
    }

    /// <summary>
    /// XP, nivel, monedas, misiones del día, logros y ropa.
    /// ⚠ Las recompensas están aparcadas en el diseño: estos números son provisionales para la demo.
    /// </summary>
    public class Progreso
    {
        public const int XpPorNivel = 300;
        public DatosProgreso Datos { get; private set; }

        public Progreso(DatosProgreso datos) { Datos = datos; }

        public int Nivel { get { return 1 + Datos.xp / XpPorNivel; } }
        public int XpEnNivel { get { return Datos.xp % XpPorNivel; } }
        public bool Tiene(string logro) { return Datos.logros.Contains(logro); }

        /// <summary>Recompensas, misiones y logros al terminar una acción. Rellena r.xp, r.monedas y r.mensajes.</summary>
        public void AlTerminarAccion(ResultadoAccion r, Jornada jornada)
        {
            DefinicionAccion a = r.accion;
            float xp;
            int normal = Math.Min(r.minutos, a.demasiadoDesde);
            int demasiado = r.minutos - normal;
            if (r.tramo == TramoDosis.Poco) xp = r.minutos * 0.5f;
            else xp = normal + demasiado * 0.25f;
            if (r.posterior) xp *= Ajustes.FactorRegistroPosterior;
            r.xp = (int)Math.Round(xp);
            r.monedas = r.xp / 2;
            Datos.xp += r.xp;
            Datos.monedas += r.monedas;
            Datos.accionesTerminadas++;
            if (r.DosisOptima) Datos.dosisOptimas++;

            // Misiones
            foreach (MisionDia m in Datos.misiones)
            {
                if (m.hecha || m.accionId != a.id) continue;
                m.minutosHechos += r.minutos;
                if (m.minutosHechos >= m.minutos)
                {
                    m.hecha = true;
                    Datos.xp += m.xp;
                    Datos.monedas += m.xp / 2;
                    r.mensajes.Add("Misión cumplida: " + m.titulo + " (+" + m.xp + " XP)");
                    Desbloquear("mision", r.mensajes);
                }
            }

            // Logros
            Desbloquear("primera_accion", r.mensajes);
            if (r.DosisOptima) Desbloquear("dosis_optima", r.mensajes);
            if (Datos.dosisOptimas >= 3) Desbloquear("constancia", r.mensajes);
            if (a.id == "quedar" && r.minutos >= 60) Desbloquear("socializador", r.mensajes);
            RevisarLogrosDeEstado(jornada, r.mensajes);
        }

        /// <summary>Logros que dependen del estado (agua, pasos, equilibrio).</summary>
        public void RevisarLogrosDeEstado(Jornada jornada, List<string> mensajes)
        {
            if (jornada.Datos.vasosAgua >= jornada.Datos.metaAgua) Desbloquear("hidratado", mensajes);
            if (jornada.Datos.pasosHoy >= 6000) Desbloquear("paseante", mensajes);
            bool todas = true;
            foreach (Area area in Areas.Todas) if (jornada.Estado[area] < Ajustes.LimitePoco) todas = false;
            if (todas) Desbloquear("equilibrio", mensajes);
        }

        public bool Desbloquear(string id, List<string> mensajes)
        {
            if (Datos.logros.Contains(id)) return false;
            Datos.logros.Add(id);
            Logro l = Catalogos.BuscarLogro(id);
            if (mensajes != null && l != null) mensajes.Add("¡Logro desbloqueado! " + l.nombre);
            return true;
        }

        /// <summary>Genera las misiones del día: según los focos del check-in y el área más baja.</summary>
        public void GenerarMisiones(Jornada jornada)
        {
            if (Datos.diaMisiones == jornada.Dia && Datos.misiones.Count > 0) return;
            Datos.diaMisiones = jornada.Dia;
            Datos.misiones.Clear();
            var usadas = new HashSet<string>();

            // 1. Área más baja (sin contar Ánimo, que sale del equilibrio)
            Area masBaja = Area.Fisico;
            foreach (Area a in new[] { Area.Fisico, Area.Mente, Area.Social, Area.Energia })
                if (jornada.Estado[a] < jornada.Estado[masBaja]) masBaja = a;
            Anadir(masBaja, true, "Sugerida: tu " + Areas.Nombre(masBaja) + " está bajo", usadas);

            // 2. Focos elegidos en el check-in
            foreach (int f in jornada.Datos.focos) Anadir((Area)f, false, "Hoy cuidas " + Areas.Nombre((Area)f), usadas);

            // 3. Completar hasta 4 con áreas variadas
            foreach (Area a in new[] { Area.Fisico, Area.Mente, Area.Social, Area.Animo, Area.Energia })
            {
                if (Datos.misiones.Count >= 4) break;
                Anadir(a, false, "", usadas);
            }
        }

        private void Anadir(Area area, bool sugerida, string motivo, HashSet<string> usadas)
        {
            if (Datos.misiones.Count >= 4) return;
            foreach (PlantillaMision p in Catalogos.Misiones)
            {
                if (p.area != area || usadas.Contains(p.accionId)) continue;
                usadas.Add(p.accionId);
                Datos.misiones.Add(new MisionDia
                {
                    accionId = p.accionId, minutos = p.minutos, titulo = p.titulo, area = (int)p.area,
                    xp = p.xp, sugerida = sugerida, motivo = motivo
                });
                return;
            }
        }

        // ---------------------------------------------------------------- ropa

        public bool Comprada(string prenda) { return Datos.ropaComprada.Contains(prenda); }

        /// <summary>Compra una prenda. Devuelve un texto con el resultado.</summary>
        public string Comprar(Prenda p, List<string> mensajes)
        {
            if (Comprada(p.id)) return "Ya la tienes";
            if (Nivel < p.nivelMinimo) return "Necesitas nivel " + p.nivelMinimo;
            if (Datos.monedas < p.precio) return "Te faltan " + (p.precio - Datos.monedas) + " monedas";
            Datos.monedas -= p.precio;
            Datos.ropaComprada.Add(p.id);
            Datos.ropaPuesta = p.id;
            Desbloquear("estilo", mensajes);
            return "¡Comprada y puesta!";
        }

        public void Poner(Prenda p)
        {
            if (Comprada(p.id)) Datos.ropaPuesta = p.id;
        }
    }
}
