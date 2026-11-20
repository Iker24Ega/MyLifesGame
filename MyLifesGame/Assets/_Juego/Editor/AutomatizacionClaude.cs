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
    private static bool mixamoIntentado;
    private static UnityEditor.PackageManager.Requests.AddRequest peticionPaquete;

    /// <summary>Instala un paquete de Unity por nombre (la versión la elige el gestor de paquetes). Resultado en paquete_resultado.txt.</summary>
    private static void InstalarPaquete(string nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return;
        EscribirEstado("instalando paquete " + nombre);
        peticionPaquete = UnityEditor.PackageManager.Client.Add(nombre);
    }

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

            string revisar = Path.Combine(Carpeta, "revisar_modelos.txt");
            if (File.Exists(revisar))
            {
                File.Delete(revisar);
                EscribirEstado("fotografiando modelos");
                RevisarModelos3D.Fotografiar();
                return;
            }

            string paquete = Path.Combine(Carpeta, "paquete.txt");
            if (File.Exists(paquete))
            {
                string nombre = File.ReadAllText(paquete).Trim();
                File.Delete(paquete);
                InstalarPaquete(nombre);
                return;
            }
            if (peticionPaquete != null)
            {
                if (!peticionPaquete.IsCompleted) return;
                string res = peticionPaquete.Status == UnityEditor.PackageManager.StatusCode.Success
                    ? "OK " + peticionPaquete.Result.packageId
                    : "ERROR " + (peticionPaquete.Error != null ? peticionPaquete.Error.message : "?");
                File.WriteAllText(Path.Combine(Carpeta, "paquete_resultado.txt"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + res + "\n");
                peticionPaquete = null;
            }

            ImportarModelos3D.CopiarNuevos();
            AssetDatabase.Refresh();
            if (!mixamoIntentado)
            {
                string r = ImportarModelos3D.ExportarParaMixamo();
                if (r.StartsWith("exportado") || r == "ya exportado") mixamoIntentado = true;
                if (r.StartsWith("exportado")) File.WriteAllText(Path.Combine(Carpeta, "mixamo.txt"), r + "\n");
            }
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
        PlayerSettings.WebGL.nameFilesAsHashes = true; // nombres nuevos en cada versión: el móvil no se queda con la caché vieja
        PlayerSettings.stripEngineCode = true; // link.xml conserva los colisionadores; sin recorte la versión web desborda la pila
        PlayerSettings.productName = "My Life's Game";
        var opciones = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/SampleScene.unity" },
            locationPathName = salida,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };
        var informe = BuildPipeline.BuildPlayer(opciones);
        AjustarIndiceWeb(Path.Combine(salida, "index.html"));
        string resumen = informe.summary.result + " | " + (informe.summary.totalSize / (1024 * 1024)) + " MB | " + informe.summary.totalTime;
        File.WriteAllText(Path.Combine(Carpeta, "build_web_resultado.txt"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + resumen + "\n");
        EscribirEstado("web terminada: " + informe.summary.result);
    }

    /// <summary>Retoca la página de la versión web: título, guardado persistente, nitidez en móvil y tamaño de móvil en el ordenador.</summary>
    private static void AjustarIndiceWeb(string ruta)
    {
        if (!File.Exists(ruta)) return;
        string s = File.ReadAllText(ruta);
        s = s.Replace("<title>Unity Web Player | My Life's Game</title>", "<title>My Life's Game</title>");
        s = s.Replace("<html lang=\"en-us\">", "<html lang=\"es\">");
        s = s.Replace("// config.autoSyncPersistentDataPath = true;", "config.autoSyncPersistentDataPath = true; // la partida se guarda en el navegador");
        s = s.Replace("// config.devicePixelRatio = 1;", "config.devicePixelRatio = Math.min(window.devicePixelRatio || 1, 2);");
        s = s.Replace("fullscreenContainer.style.width = \"960px\";", "fullscreenContainer.style.width = \"432px\"; canvas.width = 432; canvas.height = 768;");
        s = s.Replace("fullscreenContainer.style.height = \"600px\";", "fullscreenContainer.style.height = \"768px\";");
        File.WriteAllText(ruta, s);
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
