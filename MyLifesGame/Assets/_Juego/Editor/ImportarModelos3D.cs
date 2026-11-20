using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Trae al proyecto los modelos 3D generados (Flow → TRELLIS → Mixamo) sin clics:
/// copia lo nuevo de <c>_descargas/modelos3d/</c> a <c>Assets/_Juego/Arte/Resources/Modelos3D/</c>
/// (respetando subcarpetas) y ajusta la importación. Los .glb los importa glTFast; los .fbx (personaje
/// rigeado y animaciones de Mixamo) usan esqueleto humanoide para poder compartir animaciones.
/// Deja un resumen en Automatizacion/modelos3d.txt.
/// </summary>
public class ImportarModelos3D : AssetPostprocessor
{
    public const string Destino = "Assets/_Juego/Arte/Resources/Modelos3D";
    private static readonly string[] Extensiones = { ".glb", ".gltf", ".fbx", ".png", ".jpg", ".jpeg" };

    private static string Origen
    {
        get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "_descargas", "modelos3d")); }
    }

    /// <summary>Copia los archivos nuevos o cambiados. Devuelve cuántos ha copiado.</summary>
    [MenuItem("My Life's Game/Importar modelos 3D nuevos")]
    public static int CopiarNuevos()
    {
        string origen = Origen;
        if (!Directory.Exists(origen)) return 0;
        string destinoAbs = Path.GetFullPath(Path.Combine(Application.dataPath, "..", Destino));
        int copiados = 0;
        string registro = "";
        foreach (string f in Directory.GetFiles(origen, "*", SearchOption.AllDirectories))
        {
            string ext = Path.GetExtension(f).ToLowerInvariant();
            if (Array.IndexOf(Extensiones, ext) < 0) continue;
            string rel = f.Substring(origen.Length).TrimStart('/', '\\');
            // Las imágenes sueltas de referencia no se importan; solo las texturas de suelo y las de los modelos
            bool esImagen = ext == ".png" || ext == ".jpg" || ext == ".jpeg";
            if (esImagen && !rel.ToLowerInvariant().Contains("textura")) continue;
            string dst = Path.Combine(destinoAbs, rel);
            if (File.Exists(dst) && File.GetLastWriteTimeUtc(dst) >= File.GetLastWriteTimeUtc(f)) continue;
            Directory.CreateDirectory(Path.GetDirectoryName(dst));
            File.Copy(f, dst, true);
            copiados++;
            registro += "  + " + rel.Replace('\\', '/') + "\n";
        }
        if (copiados > 0)
        {
            string carpeta = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Automatizacion"));
            Directory.CreateDirectory(carpeta);
            File.AppendAllText(Path.Combine(carpeta, "modelos3d.txt"),
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | copiados " + copiados + "\n" + registro);
        }
        return copiados;
    }

    /// <summary>
    /// Mixamo solo acepta FBX/OBJ: exporta el personaje generado (GLB) a _descargas/para_mixamo/personaje_base.fbx
    /// con el exportador FBX de Unity (paquete com.unity.formats.fbx, llamado por reflexión para que el proyecto
    /// compile aunque no esté instalado). Se hace solo una vez, cuando aparece el GLB del personaje.
    /// </summary>
    [MenuItem("My Life's Game/Exportar personaje para Mixamo")]
    public static string ExportarParaMixamo()
    {
        string glb = null;
        foreach (string c in new[] { Destino + "/personaje_base.glb", Destino + "/personaje/personaje_base.glb" })
            if (File.Exists(Path.Combine(Application.dataPath, "..", c))) glb = c;
        if (glb == null) return "no hay personaje_base.glb";
        string salida = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "_descargas", "para_mixamo", "personaje_base.fbx"));
        if (File.Exists(salida)) return "ya exportado";
        Type exportador = Type.GetType("UnityEditor.Formats.Fbx.Exporter.ModelExporter, Unity.Formats.Fbx.Editor");
        if (exportador == null) return "falta el paquete com.unity.formats.fbx";
        var modelo = AssetDatabase.LoadAssetAtPath<GameObject>(glb);
        if (modelo == null) return "el GLB aún no está importado (¿falta glTFast?)";
        Directory.CreateDirectory(Path.GetDirectoryName(salida));
        GameObject copia = UnityEngine.Object.Instantiate(modelo);
        try
        {
            var metodo = exportador.GetMethod("ExportObject", new[] { typeof(string), typeof(UnityEngine.Object) });
            metodo.Invoke(null, new object[] { salida, copia });
        }
        finally { UnityEngine.Object.DestroyImmediate(copia); }
        return File.Exists(salida) ? "exportado " + salida : "falló la exportación";
    }

    // Subir el número fuerza a Unity a reimportar estos modelos con los ajustes nuevos
    public override uint GetVersion() { return 2; }

    private bool EsNuestro { get { return assetPath.Replace('\\', '/').StartsWith(Destino + "/"); } }

    private void OnPreprocessModel()
    {
        if (!EsNuestro) return;
        var m = (ModelImporter)assetImporter;
        string nombre = Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();
        bool personaje = assetPath.ToLowerInvariant().Contains("/personaje/");
        m.globalScale = 1f;
        m.importCameras = false;
        m.importLights = false;
        m.isReadable = personaje; // el avatar mide su malla al crearse (en la versión web hace falta poder leerla)
        m.meshCompression = ModelImporterMeshCompression.Medium; // la web pesa menos
        m.importNormals = ModelImporterNormals.Import;
        m.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        if (personaje)
        {
            // Humanoide: el personaje rigeado da el avatar y las animaciones de Mixamo lo reutilizan
            m.animationType = ModelImporterAnimationType.Human;
            bool soloAnimacion = !nombre.Contains("rig") && !nombre.Contains("base");
            m.importAnimation = soloAnimacion;
            m.avatarSetup = soloAnimacion ? ModelImporterAvatarSetup.CopyFromOther : ModelImporterAvatarSetup.CreateFromThisModel;
            if (soloAnimacion)
            {
                string rutaRig = Path.GetDirectoryName(assetPath).Replace('\\', '/') + "/personaje_rig.fbx";
                Avatar avatar = Buscar<Avatar>(AssetDatabase.LoadAllAssetsAtPath(rutaRig));
                if (avatar != null) m.sourceAvatar = avatar;
                else m.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            }
        }
        else
        {
            m.animationType = ModelImporterAnimationType.None;
            m.importAnimation = false;
        }
    }

    private void OnPreprocessAnimation()
    {
        if (!EsNuestro) return;
        var m = (ModelImporter)assetImporter;
        string nombre = Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();
        bool bucle = !(nombre.Contains("cheer") || nombre.Contains("wav") || nombre.Contains("celebr") || nombre.Contains("salud"));
        var clips = m.defaultClipAnimations;
        foreach (var c in clips)
        {
            c.loopTime = bucle;
            c.lockRootPositionXZ = true; // el avatar camina por código, la animación no lo desplaza
            c.keepOriginalPositionXZ = true;
            c.lockRootHeightY = true;
            c.keepOriginalPositionY = true;
            c.lockRootRotation = true;
            c.keepOriginalOrientation = true;
            c.name = Path.GetFileNameWithoutExtension(assetPath);
        }
        m.clipAnimations = clips;
    }

    private void OnPreprocessTexture()
    {
        if (assetPath.Replace('\\', '/').Contains("/Resources/Sprites/"))
        {
            // Imágenes de Flow recortadas que se colocan en la ciudad
            var ts = (TextureImporter)assetImporter;
            ts.textureType = TextureImporterType.Default;
            ts.alphaIsTransparency = true;
            ts.alphaSource = TextureImporterAlphaSource.FromInput;
            ts.wrapMode = TextureWrapMode.Clamp;
            ts.mipmapEnabled = true;
            ts.maxTextureSize = 512;
            ts.npotScale = TextureImporterNPOTScale.None;
            return;
        }
        if (!EsNuestro) return;
        var t = (TextureImporter)assetImporter;
        if (assetPath.ToLowerInvariant().Contains("textura"))
        {
            t.wrapMode = TextureWrapMode.Repeat; // texturas de suelo que se repiten
            t.mipmapEnabled = true;
            t.maxTextureSize = 1024;
        }
        else t.maxTextureSize = 1024;
    }

    private static T Buscar<T>(UnityEngine.Object[] objs) where T : UnityEngine.Object
    {
        foreach (var o in objs) if (o is T r) return r;
        return null;
    }
}
