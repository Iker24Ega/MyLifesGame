using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MyLifesGame.Motor;
using UnityEngine;

/// <summary>
/// Recorrido automático para capturas (lo activa el editor con Automatizacion/capturas.txt).
/// Usa una partida nueva en memoria (no toca la partida guardada), recorre todas las pantallas,
/// guarda una captura de cada una en Automatizacion/capturas/ y sale del modo Play.
/// </summary>
public class CapturadorPantallas : MonoBehaviour
{
    private string carpetaCapturas;

    public static string Carpeta
    {
        get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Automatizacion")); }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Detectar()
    {
#if UNITY_EDITOR
        Juego.ModoCapturas = File.Exists(Path.Combine(Carpeta, "capturas_en_curso.txt"));
#endif
    }

    private IEnumerator Start()
    {
        Juego j = GetComponent<Juego>();
        carpetaCapturas = Path.Combine(Carpeta, "capturas");
        Directory.CreateDirectory(carpetaCapturas);
        foreach (string f in Directory.GetFiles(carpetaCapturas, "*.png")) File.Delete(f);
        Debug.Log("[Capturas] Empieza el recorrido.");

        yield return Esperar(1.5f);
        yield return Captura("00a_bienvenida");
        j.Bienvenida.IrAPaso(1);
        yield return Esperar(0.5f);
        yield return Captura("00b_sudadera");
        j.Bienvenida.IrAPaso(2);
        yield return Esperar(0.5f);
        yield return Captura("00c_pregunta");
        j.Bienvenida.IrAPaso(7);
        yield return Esperar(0.5f);
        yield return Captura("00d_resumen");
        j.TerminarBienvenida(new float[] { 50, 50, 40, 75, 50 }, "teal");
        yield return Esperar(0.6f);
        yield return Captura("01_checkin");
        // Secuencia del despertar (para revisar la animación en la cama)
        string[] letras = { "b", "c", "d", "e", "f", "g", "h", "i" };
        for (int k = 0; k < letras.Length; k++)
        {
            yield return Esperar(1.5f);
            Avatar3D av = null;
            foreach (Avatar3D a3 in UnityEngine.Object.FindObjectsByType<Avatar3D>(FindObjectsSortMode.None)) if (a3.Cama != null) av = a3;
            File.AppendAllText(Path.Combine(carpetaCapturas, "checkin_clips.txt"),
                letras[k] + ": " + (av != null ? av.ClipActual : "?") + "\n");
            yield return Captura("01" + letras[k] + "_checkin");
        }

        j.CompletarCheckIn(3, 3, new List<Area> { Area.Fisico, Area.Social }, false);
        j.AsignarRol(0, "gimnasio");
        j.AsignarRol(2, "cafeteria");
        j.AsignarRol(3, "biblioteca");
        j.AsignarRol(4, "parque");
        for (int i = 0; i < 7; i++) j.SumarPasos(1000);
        for (int i = 0; i < 3; i++) j.HacerRapida(AccionRapida.Agua);
        j.HacerRapida(AccionRapida.ComerBien);

        j.IrAPestana(0);
        yield return Esperar(1.5f);
        yield return Captura("02_ciudad");
        j.CapturaTocarEdificio(5);
        yield return Esperar(0.5f);
        yield return Captura("03_ciudad_construir");
        j.CapturaTocarEdificio(0);
        yield return Esperar(0.5f);
        yield return Captura("04_ciudad_ir");
        j.IrAPestana(1);
        yield return Esperar(0.5f);
        yield return Captura("05_hoy");
        j.IrAEquilibrio();
        yield return Esperar(0.5f);
        yield return Captura("06_equilibrio");
        j.IrAHacer();
        yield return Esperar(0.5f);
        yield return Captura("07_hacer");
        j.EmpezarAccion(j.Tabla.Buscar("entrenar"));
        j.MinutoJuego += 45;
        yield return Esperar(0.5f);
        yield return Captura("08_cronometro");
        j.MinutoJuego += 20;
        j.TerminarAccion();
        yield return Esperar(0.5f);
        yield return Captura("09_resultado");
        j.IrAPestana(3);
        yield return Esperar(0.5f);
        yield return Captura("10_avatar");
        j.IrAPestana(4);
        yield return Esperar(0.5f);
        yield return Captura("11_album");
        j.SimularAusencia(6 * 60);
        yield return Esperar(0.5f);
        yield return Captura("12_regreso");
        j.TerminarRegreso(new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, int>>());
        string[] extra = { "estudiar", "meditar", "quedar" };
        for (int k = 0; k < extra.Length; k++)
        {
            j.EmpezarAccion(j.Tabla.Buscar(extra[k]));
            j.MinutoJuego += 20;
            yield return Esperar(0.6f);
            yield return Captura((13 + k) + "_cronometro_" + extra[k]);
            j.CancelarAccion();
            yield return Esperar(0.2f);
        }

        File.WriteAllText(Path.Combine(Carpeta, "capturas_hechas.txt"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        string enCurso = Path.Combine(Carpeta, "capturas_en_curso.txt");
        if (File.Exists(enCurso)) File.Delete(enCurso);
        Debug.Log("[Capturas] Recorrido terminado.");
        yield return Esperar(0.5f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private static IEnumerator Esperar(float segundos)
    {
        float fin = Time.realtimeSinceStartup + segundos;
        while (Time.realtimeSinceStartup < fin) yield return null;
    }

    private IEnumerator Captura(string nombre)
    {
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(carpetaCapturas, nombre + ".png"));
        yield return null;
        yield return null;
    }
}
