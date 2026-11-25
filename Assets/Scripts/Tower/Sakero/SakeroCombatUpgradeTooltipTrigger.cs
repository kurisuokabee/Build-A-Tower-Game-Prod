
using UnityEngine;
using UnityEngine.EventSystems;

public class SakeroCombatUpgradeTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public void OnPointerEnter(PointerEventData eventData)
    {   
        SakeroCombatUpgrade upgrade = FindAnyObjectByType<SakeroCombatUpgrade>();

        if (upgrade == null || TooltipManager.Instance == null)
            return;

        string description =
            $"+{upgrade.DamagePerUpgrade:F1} Damage\n" +
            $"+{upgrade.AttacksPerSecondPerUpgrade:F2} Attacks Speed";

        TooltipManager.Instance.ShowTooltip("Sakero Combat Upgrade", description, this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
            TooltipManager.Instance.HideTooltip(this);
    }
}
