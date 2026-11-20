using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Automatización para trabajar sin clics (solo en el editor):
/// - Refresca y compila solo cada pocos segundos, aunque Unity no tenga el foco.
/// - Si aparece Automatizacion/capturas.txt, entra en Play y la demo hace capturas de todas las pantallas.
/// - Deja el estado en Automatizacion/estado.txt (para que Claude lo lea).
/// </summary>
[InitializeOnLoad]
public static class AutomatizacionClaude
{
    private static double proximo;

    private static string Carpeta
    {
        get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Automatizacion")); }
    }

    static AutomatizacionClaude()
    {
        PlayerSettings.runInBackground = true; // que la demo no se pause si Unity no tiene el foco
        EditorApplication.update += Tick;
        EscribirEstado("cargado");
    }

    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup < proximo) return;
        proximo = EditorApplication.timeSinceStartup + 4;
        try
        {
            Directory.CreateDirectory(Carpeta);
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            string parar = Path.Combine(Carpeta, "parar.txt");
            if (File.Exists(parar))
            {
                File.Delete(parar);
                if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            string capturas = Path.Combine(Carpeta, "capturas.txt");
            if (File.Exists(capturas))
            {
                string enCurso = Path.Combine(Carpeta, "capturas_en_curso.txt");
                if (File.Exists(enCurso)) File.Delete(enCurso);
                File.Move(capturas, enCurso);
                EscribirEstado("entrando en Play para capturas");
                EditorApplication.isPlaying = true;
                return;
            }

            string web = Path.Combine(Carpeta, "build_web.txt");
            if (File.Exists(web))
            {
                File.Delete(web);
                CompilarWeb();
                return;
            }

            AssetDatabase.Refresh();
            EscribirEstado("refresco");
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Automatización] " + e.Message);
        }
    }

    /// <summary>
    /// Versión web (WebGL) para probar en el móvil. Sale en la carpeta BuildWeb/ junto al proyecto (no se sube a git).
    /// Usa compresión Gzip con descompresión de respaldo, para que funcione en cualquier alojamiento estático.
    /// </summary>
    [MenuItem("My Life's Game/Compilar versión web")]
    public static void CompilarWeb()
    {
        string salida = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "BuildWeb"));
        EscribirEstado("compilando web");
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.productName = "My Life's Game";
        var opciones = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/SampleScene.unity" },
            locationPathName = salida,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };
        var informe = BuildPipeline.BuildPlayer(opciones);
        string resumen = informe.summary.result + " | " + (informe.summary.totalSize / (1024 * 1024)) + " MB | " + informe.summary.totalTime;
        File.WriteAllText(Path.Combine(Carpeta, "build_web_resultado.txt"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + resumen + "\n");
        EscribirEstado("web terminada: " + informe.summary.result);
    }

    private static void EscribirEstado(string evento)
    {
        try
        {
            Directory.CreateDirectory(Carpeta);
            File.WriteAllText(Path.Combine(Carpeta, "estado.txt"),
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + evento +
                " | compilacionFallida=" + EditorUtility.scriptCompilationFailed +
                " | jugando=" + EditorApplication.isPlaying + "\n");
        }
        catch (Exception) { }
    }
}
