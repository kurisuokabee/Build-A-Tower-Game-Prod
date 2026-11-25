using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Tooltip Content")]
    [SerializeField] private string tooltipTitle;

    [TextArea(2, 5)]
    [SerializeField] private string tooltipDescription;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager.Instance == null)
            return;

        

        TooltipManager.Instance.ShowTooltip(
            tooltipTitle,
            tooltipDescription,
            this
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance == null)
            return;

        
        TooltipManager.Instance.HideTooltip(this);
    }
}