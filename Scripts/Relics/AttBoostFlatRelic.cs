public class AttBoostFlatRelic : BaseRelic
{
    private int bonus = 5;
    private static int attBoostFlatRelicCount = 0;
    protected override int CurrentRelicCount
    {
        get => attBoostFlatRelicCount;
        set => attBoostFlatRelicCount = value;
    }

    protected override string TooltipDescription => "+5 Attack Power.";

    protected override void ApplyEffect()
    {
        PlayerController.attackFlatBonuses.Add(bonus);
        PlayerController.attackBonus += bonus;
    }
}
