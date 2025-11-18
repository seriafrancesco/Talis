using UnityEngine;

public class AttBoostEvery10TurnsRelic : BaseRelic
{
    public static int relicCount = 0;

    protected override int CurrentRelicCount
    {
        get => relicCount;
        set => relicCount = value;
    }

    protected override string TooltipDescription => "Gain +24 ATT every 10 turns in the same floor.";

    protected override void ApplyEffect()
    {
        // L'effetto viene controllato direttamente nel PlayerController ogni turno.
    }
}
