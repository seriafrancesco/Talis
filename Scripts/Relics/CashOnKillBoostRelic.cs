using UnityEngine;

public class CashOnKillBoostRelic : BaseRelic
{
    private static int cashBoostFlatRelic = 0;
    private int bonus = 2;

    protected override int CurrentRelicCount
    {
        get => cashBoostFlatRelic;
        set => cashBoostFlatRelic = value;
    }

    protected override string TooltipDescription => "+2$ when killing an enemy.";

    protected override void ApplyEffect()
    {
        PlayerController.cashForKill += bonus;
    }
}
