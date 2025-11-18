using UnityEngine;

public class CursedShieldRelic : BaseRelic
{
    public static int relicCount = 0;
    public static int shieldBonusPerRelic = 5;

    protected override int CurrentRelicCount
    {
        get => relicCount;
        set => relicCount = value;
    }

    protected override string TooltipDescription =>
        "When shielding yourself: +5 Shield Power, but -1 Max HP.";

    protected override void ApplyEffect()
    {
        // Logica interamente in PlayerController
    }
}