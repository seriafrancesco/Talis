using UnityEngine;

public class BonusSelectableDiceRelic : BaseRelic
{
    private static int extraDiceCount = 0;

    protected override int CurrentRelicCount
    {
        get => extraDiceCount;
        set => extraDiceCount = value;
    }

    protected override string TooltipDescription => "Gain +1 selectable dice";

    protected override void ApplyEffect()
    {
        DiceRoller.selectableDices += 1;
    }
}
