
using UnityEngine;
using UnityEngine.UI;

public class SakeroSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SakeroOption
    {
        public GameObject prefab;
        public Sprite cursorSprite;
    }

    [Header("Sakero Options")]
    [SerializeField] private SakeroOption[] sakeroOptions;

    [Header("Spawn Position")]
    [SerializeField] private Transform spawnPoint;

    [Header("Custom Cursor")]
    [SerializeField] private Image cursorImage;

    private const string SelectedSakeroKey = "SelectedSakero";

    private void Start()
    {
        SpawnSakero();
    }

    private void SpawnSakero()
    {
        if (sakeroOptions == null || sakeroOptions.Length == 0)
        {
            Debug.LogError("No Sakero options assigned!");
            return;
        }

        int selectedIndex = PlayerPrefs.GetInt(
            SelectedSakeroKey, 0
        );

        selectedIndex = Mathf.Clamp(
            selectedIndex, 0, sakeroOptions.Length - 1
        );

        SakeroOption selected = sakeroOptions[selectedIndex];

        if (selected.prefab == null)
        {
            Debug.LogError("Selected Sakero prefab is missing!");
            return;
        }

        // Update custom cursor
        if (cursorImage != null)
            cursorImage.sprite = selected.cursorSprite;

        // Spawn Sakero
        Transform parent = spawnPoint != null
            ? spawnPoint
            : transform;

        GameObject sakero = Instantiate(selected.prefab, parent);

        sakero.transform.localPosition = new Vector3(0f, 1f, 0f);
        sakero.transform.localRotation = Quaternion.identity;

        // Initialize upgrade UI
        SakeroCombatUpgrade upgrade =
            sakero.GetComponent<SakeroCombatUpgrade>();

        SakeroCombatUpgradeUI upgradeUI =
            FindAnyObjectByType<SakeroCombatUpgradeUI>();

        if (upgradeUI != null)
            upgradeUI.Initialize(upgrade);
    }
}
