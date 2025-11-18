using UnityEngine;

public class FreeRerollRelic : BaseRelic
{
    private static int freeRerollRelicCount = 0;

    protected override int CurrentRelicCount
    {
        get => freeRerollRelicCount;
        set => freeRerollRelicCount = value;
    }

    protected override string TooltipDescription => "Gain +1 free reroll in the Shop.";

    protected override void ApplyEffect()
    {
        PlayerController.maxFreeRerolls += 1;
        PlayerController.currentFreeRerolls += 1;
    }
}
