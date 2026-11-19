namespace MyLifesGame.Motor
{
    /// <summary>Valores actuales de las cinco áreas (0-100).</summary>
    public class EstadoAreas
    {
        private readonly float[] valores = new float[5];

        public EstadoAreas(float fisico, float mente, float social, float energia, float animo)
        {
            valores[(int)Area.Fisico] = fisico;
            valores[(int)Area.Mente] = mente;
            valores[(int)Area.Social] = social;
            valores[(int)Area.Energia] = energia;
            valores[(int)Area.Animo] = animo;
        }

        public float this[Area area]
        {
            get { return valores[(int)area]; }
            set { valores[(int)area] = Limitar(value); }
        }

        public void Sumar(Area area, float cantidad)
        {
            this[area] = this[area] + cantidad;
        }

        public TramoArea Tramo(Area area)
        {
            float v = this[area];
            if (v < Ajustes.LimitePoco) return TramoArea.Poco;
            if (v > Ajustes.LimiteAlto) return TramoArea.Alto;
            return TramoArea.ZonaBuena;
        }

        public float[] Copia()
        {
            return (float[])valores.Clone();
        }

        private static float Limitar(float v)
        {
            if (v < Ajustes.Minimo) return Ajustes.Minimo;
            if (v > Ajustes.Maximo) return Ajustes.Maximo;
            return v;
        }
    }
}
