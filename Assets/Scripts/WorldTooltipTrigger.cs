
using UnityEngine;

public class WorldTooltipTrigger : MonoBehaviour
{
    [Header("Tooltip Content")]
    [SerializeField] private string tooltipTitle;

    [TextArea(2, 5)]
    [SerializeField] private string tooltipDescription;

    [Header("Tooltip Settings")]
    [SerializeField] private float showDelay = 0.5f;

    private Collider2D objectCollider;
    private float hoverTimer;
    private bool isHovered;
    private bool tooltipShown;

    private void Awake()
    {
        objectCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (objectCollider == null || Camera.main == null)
            return;

        Vector2 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        bool hovering = objectCollider.OverlapPoint(mousePosition);

        if (hovering)
        {
            if (!isHovered)
            {
                isHovered = true;
                hoverTimer = 0f;
            }

            if (!tooltipShown)
            {
                hoverTimer += Time.deltaTime;

                if (hoverTimer >= showDelay)
                {
                    tooltipShown = true;

                    if (TooltipManager.Instance != null)
                    {
                        TooltipManager.Instance.ShowTooltip(
                            tooltipTitle,
                            tooltipDescription,
                            this
                        );
                    }
                }
            }
        }
        else
        {
            ResetTooltip();
        }
    }

    private void ResetTooltip()
    {
        isHovered = false;
        tooltipShown = false;
        hoverTimer = 0f;

        if (TooltipManager.Instance != null)
            TooltipManager.Instance.HideTooltip(this);
    }

    private void OnDisable()
    {
        ResetTooltip();
    }
}
