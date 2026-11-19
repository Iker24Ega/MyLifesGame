namespace MyLifesGame.Motor
{
    /// <summary>Las cinco áreas de la vida del jugador.</summary>
    public enum Area { Fisico, Mente, Social, Energia, Animo }

    /// <summary>Tramo en el que está un área: Poco (&lt; 30), Zona buena (30-80) o Alto (&gt; 80).</summary>
    public enum TramoArea { Poco, ZonaBuena, Alto }

    public static class Areas
    {
        public static readonly Area[] Todas = { Area.Fisico, Area.Mente, Area.Social, Area.Energia, Area.Animo };

        public static string Nombre(Area area)
        {
            switch (area)
            {
                case Area.Fisico: return "Físico";
                case Area.Mente: return "Mente";
                case Area.Social: return "Social";
                case Area.Energia: return "Energía";
                default: return "Ánimo";
            }
        }
    }
}
