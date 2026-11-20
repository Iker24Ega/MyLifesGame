using UnityEditor;

/// <summary>
/// Ajustes de importación de los personajes (Resources/Personajes): esqueleto genérico, normales suavizadas
/// (para un aspecto más redondeado, como las imágenes de referencia) y animaciones en bucle.
/// </summary>
public class ImportarPersonajes : AssetPostprocessor
{
    private bool EsPersonaje { get { return assetPath.Replace('\\', '/').Contains("/Resources/Personajes/"); } }

    private void OnPreprocessModel()
    {
        if (!EsPersonaje) return;
        var mi = (ModelImporter)assetImporter;
        mi.animationType = ModelImporterAnimationType.Generic;
        mi.importAnimation = true;
        mi.importNormals = ModelImporterNormals.Calculate;
        mi.normalSmoothingAngle = 75f;
        mi.importBlendShapes = false;
    }

    private void OnPreprocessAnimation()
    {
        if (!EsPersonaje) return;
        var mi = (ModelImporter)assetImporter;
        ModelImporterClipAnimation[] clips = mi.defaultClipAnimations;
        foreach (ModelImporterClipAnimation c in clips)
        {
            bool unaVez = c.name.Contains("Death") || c.name.Contains("Roll") || c.name.Contains("Hit");
            c.loopTime = !unaVez;
            c.loopPose = !unaVez;
        }
        mi.clipAnimations = clips;
    }
}
