using UnityEngine;

/// <summary>
/// Sonidos suaves generados por código (sin archivos): toque de botón, aviso, éxito y fin de acción.
/// Ondas senoidales con envolvente corta, tipo "juego de móvil amable".
/// </summary>
public static class Sonidos
{
    public static bool Activos = true;
    private static AudioSource fuente;
    private static AudioClip toque, aviso, exito, fin, moneda;
    private const int Frecuencia = 44100;

    private static void Preparar()
    {
        if (fuente != null) return;
        var go = new GameObject("Sonidos");
        Object.DontDestroyOnLoad(go);
        fuente = go.AddComponent<AudioSource>();
        fuente.playOnAwake = false;
        fuente.volume = 0.5f;
        toque = Clip("toque", new[] { 660f }, 0.07f, 0.35f);
        aviso = Clip("aviso", new[] { 784f, 1046f }, 0.12f, 0.35f);
        exito = Clip("exito", new[] { 523f, 659f, 784f, 1046f }, 0.11f, 0.4f);
        fin = Clip("fin", new[] { 659f, 523f }, 0.13f, 0.35f);
        moneda = Clip("moneda", new[] { 988f, 1318f }, 0.08f, 0.35f);
    }

    /// <summary>Crea un clip con una nota por paso (arpegio), cada una con ataque rápido y caída suave.</summary>
    private static AudioClip Clip(string nombre, float[] notas, float duracionNota, float volumen)
    {
        int porNota = Mathf.RoundToInt(duracionNota * Frecuencia);
        int cola = Mathf.RoundToInt(0.12f * Frecuencia);
        var datos = new float[porNota * notas.Length + cola];
        for (int n = 0; n < notas.Length; n++)
        {
            int inicio = n * porNota;
            int largo = porNota + cola;
            for (int i = 0; i < largo && inicio + i < datos.Length; i++)
            {
                float t = i / (float)Frecuencia;
                float ataque = Mathf.Clamp01(t / 0.006f);
                float caida = Mathf.Exp(-t * 18f);
                float onda = Mathf.Sin(2f * Mathf.PI * notas[n] * t) + 0.25f * Mathf.Sin(4f * Mathf.PI * notas[n] * t);
                datos[inicio + i] += onda * ataque * caida * volumen;
            }
        }
        AudioClip c = AudioClip.Create(nombre, datos.Length, 1, Frecuencia, false);
        c.SetData(datos, 0);
        return c;
    }

    private static void Sonar(AudioClip c, float tono = 1f)
    {
        if (!Activos) return;
        Preparar();
        if (c == null) { Preparar(); return; }
        fuente.pitch = tono;
        fuente.PlayOneShot(c);
    }

    public static void Toque() { Preparar(); Sonar(toque, Random.Range(0.97f, 1.03f)); }
    public static void Aviso() { Preparar(); Sonar(aviso); }
    public static void Exito() { Preparar(); Sonar(exito); }
    public static void Fin() { Preparar(); Sonar(fin); }
    public static void Moneda() { Preparar(); Sonar(moneda); }
}
