namespace MyLifesGame.Motor
{
    public enum TipoEstadoAvatar { EnForma, Normal, Cansado, Descuidado }

    /// <summary>Cómo se ve el avatar según tus áreas (sin cambiar nunca su cuerpo, solo postura, luz y entorno).</summary>
    public static class EstadoAvatar
    {
        public static TipoEstadoAvatar Calcular(EstadoAreas e, bool vieneDeAusencia, out string causa)
        {
            if (vieneDeAusencia) { causa = "Varias horas sin entrar"; return TipoEstadoAvatar.Descuidado; }
            if (e[Area.Energia] < Ajustes.LimitePoco) { causa = "Poca energía"; return TipoEstadoAvatar.Cansado; }
            bool todas = true;
            foreach (Area a in Areas.Todas) if (e[a] < Ajustes.LimitePoco) todas = false;
            if (todas && e[Area.Fisico] >= 65f && e[Area.Energia] >= 50f) { causa = "Buen equilibrio y en forma"; return TipoEstadoAvatar.EnForma; }
            causa = todas ? "Día equilibrado" : "Alguna área en Poco";
            return TipoEstadoAvatar.Normal;
        }

        public static string Nombre(TipoEstadoAvatar t)
        {
            switch (t)
            {
                case TipoEstadoAvatar.EnForma: return "En forma";
                case TipoEstadoAvatar.Cansado: return "Cansado";
                case TipoEstadoAvatar.Descuidado: return "Descuidado";
                default: return "Normal";
            }
        }
    }
}
