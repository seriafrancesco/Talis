using UnityEngine;

public class MaxHpBoostOnKillRelic : BaseRelic
{
    public static int relicCount = 0;

    protected override int CurrentRelicCount
    {
        get => relicCount;
        set => relicCount = value;
    }

    protected override string TooltipDescription => "Gain +2 Max HP when you kill an enemy.";

    protected override void ApplyEffect()
    {
        // Nessun effetto immediato: trigger esterno (su kill)
    }
}
