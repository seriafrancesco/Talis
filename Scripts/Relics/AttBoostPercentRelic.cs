public class AttBoostPercentRelic : BaseRelic
{
    private float bonus = 0.25f;
    private static int attBoostPercentRelics = 0;
    protected override int CurrentRelicCount
    {
        get => attBoostPercentRelics;
        set => attBoostPercentRelics = value;
    }

    protected override string TooltipDescription => "+25% Attack Power";

    protected override void ApplyEffect()
    {
        PlayerController.attackMultBonuses.Add(1 + bonus); // es. 1.1
    }
}
