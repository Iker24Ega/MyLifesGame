using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Fotos de revisión de los modelos generados: cada modelo de Resources/Modelos3D se coloca lejos de la escena,
/// se fotografía desde 4 ángulos (como se verá en la ciudad: 3/4 desde arriba) y se guarda en
/// Automatizacion/revision/&lt;nombre&gt;.png. Se lanza con Automatizacion/revisar_modelos.txt o desde el menú.
/// </summary>
public static class RevisarModelos3D
{
    [MenuItem("My Life's Game/Fotos de revisión de modelos 3D")]
    public static void Fotografiar()
    {
        string carpeta = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Automatizacion", "revision"));
        Directory.CreateDirectory(carpeta);
        string raizAssets = ImportarModelos3D.Destino;
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { raizAssets });
        var escenario = new GameObject("__Revision");
        escenario.transform.position = new Vector3(0, -2000, 0);
        var luzGO = new GameObject("Luz");
        luzGO.transform.SetParent(escenario.transform, false);
        var luz = luzGO.AddComponent<Light>();
        luz.type = LightType.Directional;
        luz.intensity = 1.3f;
        luz.color = new Color(1f, 0.95f, 0.88f);
        luz.shadows = LightShadows.Soft;
        luzGO.transform.rotation = Quaternion.Euler(50, -30, 0);
        var camGO = new GameObject("Cam");
        camGO.transform.SetParent(escenario.transform, false);
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.93f, 0.9f, 0.85f);
        cam.fieldOfView = 30;
        var rt = new RenderTexture(512, 512, 24);
        cam.targetTexture = rt;
        int n = 0;
        try
        {
            foreach (string g in guids)
            {
                string ruta = AssetDatabase.GUIDToAssetPath(g);
                string nombre = Path.GetFileNameWithoutExtension(ruta);
                var raiz = Modelos3D.Crear(nombre, escenario.transform, 10f, Modelos3D.Medida.Alto);
                if (raiz == null) continue;
                var hoja = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
                Vector3 centro = escenario.transform.position + Vector3.up * 5f;
                for (int i = 0; i < 4; i++)
                {
                    float ang = 45f + i * 90f;
                    Vector3 dir = Quaternion.Euler(30f, ang, 0) * Vector3.back;
                    cam.transform.position = centro + dir * 26f;
                    cam.transform.LookAt(centro);
                    cam.Render();
                    RenderTexture.active = rt;
                    var t = new Texture2D(512, 512, TextureFormat.RGB24, false);
                    t.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
                    t.Apply();
                    hoja.SetPixels((i % 2) * 512, (1 - i / 2) * 512, 512, 512, t.GetPixels());
                    Object.DestroyImmediate(t);
                }
                RenderTexture.active = null;
                File.WriteAllBytes(Path.Combine(carpeta, nombre + ".png"), hoja.EncodeToPNG());
                Object.DestroyImmediate(hoja);
                Object.DestroyImmediate(raiz);
                n++;
            }
        }
        finally
        {
            cam.targetTexture = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(escenario);
        }
        File.WriteAllText(Path.Combine(carpeta, "_hecho.txt"), System.DateTime.Now + " | " + n + " modelos\n");
    }
}
