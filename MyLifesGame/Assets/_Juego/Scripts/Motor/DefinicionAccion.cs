using System;
using System.Collections.Generic;

namespace MyLifesGame.Motor
{
    /// <summary>Puntos por hora que una acción suma o resta en cada área.</summary>
    [Serializable]
    public class Efectos
    {
        public float fisico;
        public float mente;
        public float social;
        public float energia;
        public float animo;

        public float Get(Area area)
        {
            switch (area)
            {
                case Area.Fisico: return fisico;
                case Area.Mente: return mente;
                case Area.Social: return social;
                case Area.Energia: return energia;
                default: return animo;
            }
        }
    }

    /// <summary>Tramo de duración de una acción, como la barra del cronómetro.</summary>
    public enum TramoDosis { Poco, ZonaOptima, Larga, Demasiado }

    /// <summary>Una acción del juego (Entrenar, Estudiar...). Se carga desde Datos/acciones.json.</summary>
    [Serializable]
    public class DefinicionAccion
    {
        public string id;
        public string nombre;
        public string lugar;

        public int zonaMin;          // minutos: inicio de la zona óptima
        public int zonaMax;          // minutos: fin de la zona óptima
        public int demasiadoDesde;   // minutos: a partir de aquí, "Demasiado"

        public bool esfuerzo;        // regla 2: rinde menos sin energía
        public bool desconexion;     // regla 4: recarga más tras trabajo o estudio
        public bool mental;          // cuenta como esfuerzo mental (descanso activo y fatiga)
        public bool ayudaAnimoBajo;  // regla 2: sube más el Ánimo si está bajo
        public float socialConAmigosPorHora; // interruptor "con amigos"
        public bool afectaSueno;     // regla 6: hecha en la última hora antes de dormir, se descansa peor

        public Efectos efectoHora;           // en zona óptima
        public Efectos efectoHoraDemasiado;  // a partir de "Demasiado"

        public TramoDosis Tramo(int minutos)
        {
            if (minutos < zonaMin) return TramoDosis.Poco;
            if (minutos <= zonaMax) return TramoDosis.ZonaOptima;
            if (minutos <= demasiadoDesde) return TramoDosis.Larga;
            return TramoDosis.Demasiado;
        }
    }

    [Serializable]
    public class TablaAcciones
    {
        public List<DefinicionAccion> acciones = new List<DefinicionAccion>();

        public DefinicionAccion Buscar(string id)
        {
            foreach (var a in acciones)
                if (a.id == id) return a;
            throw new ArgumentException("No existe la acción: " + id);
        }
    }
}
