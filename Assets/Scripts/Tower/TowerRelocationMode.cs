
using System;
using UnityEngine;
using UnityEngine.UI;

public class TowerRelocationMode : MonoBehaviour
{
    public static TowerRelocationMode Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private Image buttonImage;

    [Header("Button Colors")]
    [SerializeField] private Color offColor = new Color32(158, 27, 40, 255);
    [SerializeField] private Color onColor = new Color32(40, 158, 27, 255);

    public bool IsRelocationMode { get; private set; }

    public event Action<bool> OnModeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleMode);

        UpdateButton();
    }

    private void OnDestroy()
    {
        if (toggleButton != null)
            toggleButton.onClick.RemoveListener(ToggleMode);

        if (Instance == this)
            Instance = null;
    }

    public void ToggleMode()
    {
        SetMode(!IsRelocationMode);
    }

    public void SetMode(bool enabled)
    {
        if (IsRelocationMode == enabled)
            return;

        IsRelocationMode = enabled;

        UpdateButton();
        OnModeChanged?.Invoke(IsRelocationMode);

        Debug.Log($"Tower Relocation Mode: {(enabled ? "ON" : "OFF")}");
    }

    private void UpdateButton()
    {
        if (buttonImage != null)
        {
            buttonImage.color = IsRelocationMode
                ? onColor
                : offColor;
        }
    }
}
