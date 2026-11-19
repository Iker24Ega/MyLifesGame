using System;
using System.Collections.Generic;

namespace MyLifesGame.Motor
{
    /// <summary>Roles que el jugador ha dado a los edificios de su ciudad (uno por solar; "" = sin rol).</summary>
    [Serializable]
    public class DatosCiudad
    {
        public List<string> roles = new List<string>();
        public int edificioActual = -1; // dónde está el avatar
    }

    /// <summary>Todo lo que se guarda en disco.</summary>
    [Serializable]
    public class DatosPartida
    {
        public int version = 1;
        public DatosJornada jornada = new DatosJornada();
        public DatosProgreso progreso = new DatosProgreso();
        public DatosCiudad ciudad = new DatosCiudad();
        public float minutoJuego;
        public float velocidad = 1f;
        public long guardadoUtcTicks;
        public string accionEnCurso = "";
        public float inicioAccion;
        public bool conAmigos;
    }
}
