using UnityEngine;

public class ShieldBoostFlatRelic : BaseRelic
{
    private int bonus = 5;
    private static int shieldBoostFlatRelicCount = 0;

    protected override int CurrentRelicCount
    {
        get => shieldBoostFlatRelicCount;
        set => shieldBoostFlatRelicCount = value;
    }

    protected override string TooltipDescription => "+5 Shield Power";

    protected override void ApplyEffect()
    {
        PlayerController.shieldFlatBonuses.Add(bonus);
        PlayerController.shieldBonus += bonus; // se usi anche una variabile cumulativa
    }
}
