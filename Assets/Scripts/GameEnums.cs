namespace GameEnum {
    public enum TroopTypes
    {
        Swordsman,
        Archer,
        AntiHorse,
        SwordsmanPlus,
        Knight,
        Crossbowman,
        AntiHorsePlus,
        KnightPlus
    }

    public enum TroopSpecialCharacteristics
    {
        Charge,
        DamageReflect,
        UseRadius,
        CrossbowFireReload
    }

    public enum GameSides
    {
        Flemish,
        French
    }
    public enum BattlePhase
    {
        MovementAttackPhase,
        DeployPhase
    }

    public enum DeckVisibility
    {
        Hidden,
        FullVisible,
        LowVisible
    }
}
