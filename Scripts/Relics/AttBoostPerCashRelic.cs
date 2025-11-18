using UnityEngine;

public class AttBoostPerCashRelic : BaseRelic
{
    public static int attBoostPerCashRelicCount = 0;
    //private static int lastBonusGiven = 0;

    protected override int CurrentRelicCount
    {
        get => attBoostPerCashRelicCount;
        set => attBoostPerCashRelicCount = value;
    }

    protected override string TooltipDescription => "Gain +1 ATT for each 1$ you currently own.";

    protected override void ApplyEffect()
    {
        //Effetto dinamico nel PlayerController
    }
}
