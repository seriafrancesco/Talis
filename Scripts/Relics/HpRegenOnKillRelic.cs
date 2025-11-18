using UnityEngine;

public class HpRegenOnKillRelic : BaseRelic
{
    public static int hpRegenOnKillRelicCount = 0;

    protected override int CurrentRelicCount
    {
        get => hpRegenOnKillRelicCount;
        set => hpRegenOnKillRelicCount = value;
    }

    protected override string TooltipDescription => "Regenerate 10% Max HP when killing an enemy.";

    protected override void ApplyEffect()
    {
        //In EnemyController
    }
}
