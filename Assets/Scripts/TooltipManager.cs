using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private Canvas canvas;

    [Header("Settings")]
    [SerializeField] private float maxWidth = 300f;
    [SerializeField] private Vector2 offset = new Vector2(15f, -15f);

    private Vector3 originalPosition;
    private object currentTooltipOwner;

    public bool IsUpgradeTooltipActive { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        originalPosition = tooltipRect.localPosition;
        HideTooltipImmediately();
    }

    private void Update()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                Input.mousePosition,
                canvas.worldCamera,
                out Vector2 localPoint))
            {
                float height = tooltipRect.rect.height;

                tooltipRect.localPosition = localPoint + new Vector2(
                    offset.x,
                    height * 0.5f + Mathf.Abs(offset.y)
                );
            }
        }
    }

    public void ShowTooltip(string title, string description, object owner)
    {
        ShowTooltipInternal(title, description, owner, false);
    }

    public void ShowUpgradeTooltip(string title, string description, object owner)
    {
        ShowTooltipInternal(title, description, owner, true);
    }

    private void ShowTooltipInternal(
        string title,
        string description,
        object owner,
        bool isUpgradeTooltip)
    {
        if (tooltipPanel == null)
            return;

        currentTooltipOwner = owner;
        IsUpgradeTooltipActive = isUpgradeTooltip;

        titleText.text = title;
        descriptionText.text = description;

        layoutElement.preferredWidth = maxWidth;

        tooltipPanel.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
    }

    public void HideTooltip(object owner)
    {
        if (currentTooltipOwner != owner)
            return;

        currentTooltipOwner = null;
        IsUpgradeTooltipActive = false;

        HideTooltipImmediately();
    }

    private void HideTooltipImmediately()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
            tooltipRect.localPosition = originalPosition;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}