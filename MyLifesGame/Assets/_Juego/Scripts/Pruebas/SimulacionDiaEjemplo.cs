using System.Text;
using MyLifesGame.Motor;
using UnityEngine;

/// <summary>
/// Al pulsar Play, simula los días de ejemplo de la Fase 1 con el motor del equilibrio
/// y escribe el resultado en la consola. No hace falta ponerlo en ninguna escena.
/// </summary>
public static class SimulacionDiaEjemplo
{
    // Ponlo a true para volver a ver la simulación en la consola al pulsar Play.
    private static readonly bool Activa = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AlPulsarPlay()
    {
        if (!Activa) return;
        TextAsset json = Resources.Load<TextAsset>("acciones");
        if (json == null)
        {
            Debug.LogError("No encuentro la tabla de acciones (Assets/_Juego/Datos/Resources/acciones.json).");
            return;
        }
        TablaAcciones tabla = JsonUtility.FromJson<TablaAcciones>(json.text);
        Debug.Log("Motor del equilibrio cargado: " + tabla.acciones.Count + " acciones.");
        Debug.Log(ComoTabla("Día A: día completo y equilibrado", DiaA(tabla)));
        Debug.Log(ComoTabla("Día B: jornada de 8 h y gimnasio por la tarde", DiaB(tabla)));
    }

    private static int H(int horas, int minutos = 0) { return horas * 60 + minutos; }

    private static Jornada DiaA(TablaAcciones tabla)
    {
        // Check-in: he dormido bien (Energía 75), me siento bien (Ánimo 55). Social 25: varios días sin quedar.
        var dia = Jornada.Nueva(tabla, new float[] { 55, 50, 25, 75, 55 }, H(8));
        dia.Avanzar(H(8, 15));
        dia.HacerAccionRapida(AccionRapida.Ducha);
        dia.HacerAccionRapida(AccionRapida.ComerBien);
        dia.HacerAccionRapida(AccionRapida.Agua);
        dia.HacerAccionRapida(AccionRapida.Agua);
        dia.Anotar("Ducha, desayuno y 2 vasos de agua");
        dia.HacerAccion("trabajar", H(9), 240);
        dia.HacerAccion("pasear", H(13), 30);
        dia.Avanzar(H(14));
        dia.HacerAccionRapida(AccionRapida.ComerBien);
        dia.HacerAccionRapida(AccionRapida.Agua);
        dia.HacerAccionRapida(AccionRapida.Agua);
        dia.Anotar("Comida y 2 vasos de agua");
        dia.HacerAccion("estudiar", H(15), 120);
        dia.HacerAccion("entrenar", H(18), 65);
        dia.HacerAccion("quedar", H(20), 120);
        dia.HacerAccion("videojuegos", H(22), 45);
        dia.Avanzar(H(23));
        dia.HacerAccionRapida(AccionRapida.ComerBien);
        dia.HacerAccionRapida(AccionRapida.Agua);
        dia.HacerAccionRapida(AccionRapida.Agua);
        dia.Anotar("Cena y 2 vasos de agua");
        dia.SumarPasos(7500);
        dia.Anotar("7.500 pasos fuera de acciones");
        dia.Dormir(H(23), 8);
        return dia;
    }

    private static Jornada DiaB(TablaAcciones tabla)
    {
        var dia = Jornada.Nueva(tabla, new float[] { 55, 50, 25, 75, 55 }, H(8));
        dia.Avanzar(H(8, 15));
        dia.HacerAccionRapida(AccionRapida.Ducha);
        dia.HacerAccionRapida(AccionRapida.ComerBien);
        dia.HacerAccionRapida(AccionRapida.Agua);
        dia.HacerAccionRapida(AccionRapida.Agua);
        dia.Anotar("Ducha, desayuno y 2 vasos de agua");
        dia.HacerAccion("trabajar", H(9), 480);
        dia.HacerAccion("entrenar", H(18), 65);
        return dia;
    }

    private static string ComoTabla(string titulo, Jornada dia)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<b>" + titulo + "</b>");
        sb.AppendLine("Hora  | Físico Mente Social Energía Ánimo | Qué pasa");
        foreach (Evento e in dia.Eventos)
        {
            string hora = e.minutoFin > e.minutoInicio
                ? Jornada.Hora(e.minutoInicio) + "-" + Jornada.Hora(e.minutoFin)
                : Jornada.Hora(e.minutoInicio);
            sb.Append(hora.PadRight(11)).Append(" | ");
            foreach (float v in e.valores) sb.Append(Mathf.RoundToInt(v).ToString().PadLeft(4)).Append("  ");
            sb.Append("| ").Append(e.texto);
            if (!string.IsNullOrEmpty(e.nota)) sb.Append("  → ").Append(e.nota);
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
