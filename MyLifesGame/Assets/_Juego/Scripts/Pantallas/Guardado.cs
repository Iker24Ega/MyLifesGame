using System;
using System.IO;
using MyLifesGame.Motor;
using UnityEngine;

/// <summary>Guarda y carga la partida en un archivo JSON en el almacenamiento de la app.</summary>
public static class Guardado
{
    public static string Ruta { get { return Path.Combine(Application.persistentDataPath, "partida.json"); } }

    public static void Guardar(DatosPartida datos)
    {
        try
        {
            datos.guardadoUtcTicks = DateTime.UtcNow.Ticks;
            File.WriteAllText(Ruta, JsonUtility.ToJson(datos));
        }
        catch (Exception e)
        {
            Debug.LogWarning("No se pudo guardar la partida: " + e.Message);
        }
    }

    public static DatosPartida Cargar()
    {
        try
        {
            if (!File.Exists(Ruta)) return null;
            DatosPartida d = JsonUtility.FromJson<DatosPartida>(File.ReadAllText(Ruta));
            if (d == null || d.jornada == null || d.jornada.areas == null || d.jornada.areas.Length != 5) return null;
            return d;
        }
        catch (Exception e)
        {
            Debug.LogWarning("No se pudo cargar la partida (se empieza una nueva): " + e.Message);
            return null;
        }
    }

    public static void Borrar()
    {
        try { if (File.Exists(Ruta)) File.Delete(Ruta); }
        catch (Exception e) { Debug.LogWarning("No se pudo borrar la partida: " + e.Message); }
    }
}
