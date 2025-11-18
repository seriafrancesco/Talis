using UnityEngine;

public class HpRegenAfterTurnRelic : BaseRelic
{
    private static int relicCount = 0;
    static public int healAmount;

    protected override int CurrentRelicCount
    {
        get => relicCount;
        set => relicCount = value;
    }

    protected override string TooltipDescription => $"Regenerate 1 HP after your turn ends.";

    protected override void ApplyEffect()
    {
        // Aggiungi questa reliquia a una lista se vuoi usarla nel sistema di attacco
        healAmount += 1;
    }
}
