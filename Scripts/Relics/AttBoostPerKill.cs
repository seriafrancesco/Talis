using UnityEngine;

public class AttBoostPerKillRelic : BaseRelic
{
    public static int attBoostPerKillRelicCount = 0;
    public static int bonus = 3;

    protected override int CurrentRelicCount
    {
        get => attBoostPerKillRelicCount;
        set => attBoostPerKillRelicCount = value;
    }

    protected override string TooltipDescription => "Gain +3 ATT for each enemy defeated.";

    protected override void ApplyEffect()
    {
        // L'effetto viene applicato da EnemyController al momento della kill
    }
}