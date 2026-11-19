namespace MyLifesGame.Motor
{
    /// <summary>
    /// Números de las reglas del equilibrio (Fase 1, v0.4). Son hipótesis de juego:
    /// se ajustan jugando. Más adelante pasarán a un archivo de datos.
    /// </summary>
    public static class Ajustes
    {
        // Áreas
        public const float Minimo = 0f;
        public const float Maximo = 100f;
        public const float LimitePoco = 30f;   // por debajo: "Poco"
        public const float LimiteAlto = 80f;   // por encima: "Alto"

        // Energía: batería del día
        public const float DesgasteEnergiaPorHora = 1.5f;

        // Regla 1: el Ánimo sale del equilibrio
        public const int MinutosEntreRevisiones = 360;
        public const float AnimoPorEquilibrio = 3f;
        public const float AnimoPorAreaBaja = 3f;

        // Regla 2: el efecto depende de cómo estás
        public const float FactorBeneficioSinEnergia = 0.5f;
        public const float FactorCosteSinEnergia = 1.5f;
        public const float FactorAnimoSiAnimoBajo = 1.5f;

        // Regla 3: periodo de gracia de 48 h y desgaste después (por día despierto, unos 16 h)
        public const int MinutosGracia = 48 * 60;
        public const float MinutosDespiertoPorDia = 16 * 60;
        public const float DesgasteFisicoPorDia = 8f;
        public const float DesgasteMentePorDia = 6f;
        public const float DesgasteSocialPorDia = 10f;
        public const float MinimoPorDesgaste = 10f;

        // Regla 4: descanso activo
        public const int VentanaDescansoActivoMin = 180;
        public const int MinutosMentalesParaDescansoActivo = 120;
        public const int MinutosConBonusDescansoActivo = 30;
        public const float ExtraDescansoActivo = 0.5f; // recarga 1,5 veces

        // Regla 5: días seguidos entrenando
        public const int DiasSeguidosParaCansancio = 4;
        public const float FactorFisicoDiasSeguidos = 0.75f;
        public const float EnergiaExtraDiasSeguidos = 5f;

        // Fatiga mental
        public const int MinutosFatigaMental = 360;
        public const float FactorFatigaMental = 0.5f;

        // Acciones rápidas
        public const int MetaAguaPorDefecto = 8;
        public const int MaxComidasBuenas = 3;

        // Sueño
        public const float RecargaSuenoPorHora = 10f;

        // Check-in
        public static readonly float[] EnergiaSegunSueno = { 40f, 50f, 60f, 75f, 85f };
        public static readonly float[] AnimoSegunSentir = { -10f, -5f, 0f, 5f, 10f };
        public const float FactorDiaPesado = 1.25f;

        // Registro posterior (lo que olvidaste iniciar)
        public const float FactorRegistroPosterior = 0.7f;

        // Ausencia: a partir de aquí sale la pantalla de regreso
        public const int MinutosAusenciaParaRegreso = 6 * 60;
    }
}
