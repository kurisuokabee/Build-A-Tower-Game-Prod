
using UnityEngine;

public class FlailWeaponController : MonoBehaviour
{
    [Header("Weapon References")]
    [SerializeField] private Transform flailPivot;
    [SerializeField] private Transform firstFlail;
    [SerializeField] private Transform firstBall;
    [SerializeField] private Transform firstSword;
    [SerializeField] private Transform secondFlail;
    [SerializeField] private Transform secondBall;
    [SerializeField] private Transform secondSword;

    [Header("Hitbox References")]
    [SerializeField] private FlailWeaponHitbox firstBallHitbox;
    [SerializeField] private FlailWeaponHitbox firstSwordHitbox;
    [SerializeField] private FlailWeaponHitbox secondBallHitbox;
    [SerializeField] private FlailWeaponHitbox secondSwordHitbox;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float chargingRotationSpeed = 720f;

    [Header("Charging Flail")]
    [SerializeField] private float spinDuration = 2f;

    [Header("Roulette Flail")]
    [SerializeField] private float minimumDamageMultiplier = 0.1f;
    [SerializeField] private float maximumDamageMultiplier = 3f;

    private Quaternion firstRestRotation;
    private Quaternion secondRestRotation;

    private float currentAngle;
    private float phaseTimer;
    private bool isSpinPhase;

    private void Awake()
    {
        if (firstFlail != null)
            firstRestRotation = firstFlail.localRotation;

        if (secondFlail != null)
            secondRestRotation = secondFlail.localRotation;

        // Automatically find hitboxes if they were not assigned.
        if (firstBallHitbox == null && firstBall != null)
            firstBallHitbox = firstBall.GetComponent<FlailWeaponHitbox>();

        if (firstSwordHitbox == null && firstSword != null)
            firstSwordHitbox = firstSword.GetComponent<FlailWeaponHitbox>();

        if (secondBallHitbox == null && secondBall != null)
            secondBallHitbox = secondBall.GetComponent<FlailWeaponHitbox>();

        if (secondSwordHitbox == null && secondSword != null)
            secondSwordHitbox = secondSword.GetComponent<FlailWeaponHitbox>();

        if (flailPivot == null || firstFlail == null)
        {
            Debug.LogError(
                "FlailWeaponController is missing its Pivot or FirstFlail.",
                this
            );
        }

        CheckHitbox(firstBallHitbox, "First Ball");
        CheckHitbox(firstSwordHitbox, "First Sword");
        CheckHitbox(secondBallHitbox, "Second Ball");
        CheckHitbox(secondSwordHitbox, "Second Sword");
    }

    private void CheckHitbox(
        FlailWeaponHitbox hitbox,
        string weaponName)
    {
        if (hitbox == null)
        {
            Debug.LogWarning(
                $"FlailWeaponController is missing the {weaponName} hitbox.",
                this
            );
        }
    }

    public void Tick(
        bool isCharging,
        bool hasSecondWeapon,
        bool useSword,
        bool isRoulette,
        float damage)
    {
        if (flailPivot == null || firstFlail == null)
            return;

        RefreshWeaponVisuals(hasSecondWeapon, useSword);

        ConfigureHitboxes(damage, isRoulette);

        if (isCharging)
        {
            UpdateChargingFlail(hasSecondWeapon);
        }
        else
        {
            UpdateContinuousRotation(hasSecondWeapon);
        }
    }

    private void ConfigureHitboxes(
        float damage,
        bool isRoulette)
    {
        ConfigureHitbox(firstBallHitbox, damage, isRoulette);
        ConfigureHitbox(firstSwordHitbox, damage, isRoulette);
        ConfigureHitbox(secondBallHitbox, damage, isRoulette);
        ConfigureHitbox(secondSwordHitbox, damage, isRoulette);
    }

    private void ConfigureHitbox(
        FlailWeaponHitbox hitbox,
        float damage,
        bool isRoulette)
    {
        if (hitbox == null)
            return;

        hitbox.Configure(
            damage,
            isRoulette,
            minimumDamageMultiplier,
            maximumDamageMultiplier
        );
    }

    private void RefreshWeaponVisuals(
        bool hasSecondWeapon,
        bool useSword)
    {
        if (firstFlail != null)
            firstFlail.gameObject.SetActive(true);

        if (secondFlail != null)
            secondFlail.gameObject.SetActive(hasSecondWeapon);

        SetActive(firstBall, !useSword);
        SetActive(firstSword, useSword);

        SetActive(secondBall, !useSword);
        SetActive(secondSword, useSword);
    }

    private void SetActive(Transform target, bool active)
    {
        if (target != null && target.gameObject.activeSelf != active)
            target.gameObject.SetActive(active);
    }

    private void UpdateContinuousRotation(bool hasSecondWeapon)
    {
        float rotationThisFrame = rotationSpeed * Time.deltaTime;

        currentAngle += rotationThisFrame;

        RotateWeapons(hasSecondWeapon);
    }

    private void UpdateChargingFlail(bool hasSecondWeapon)
    {
        if (!isSpinPhase)
        {
            float rotationThisFrame = rotationSpeed * Time.deltaTime;

            currentAngle += rotationThisFrame;

            RotateWeapons(hasSecondWeapon);

            // Two full revolutions before rapid spinning.
            if (currentAngle >= 720f)
            {
                currentAngle = 0f;
                isSpinPhase = true;
                phaseTimer = 0f;

                Debug.Log(
                    "[Flail] Charging complete. Rapid spin started.",
                    this
                );
            }
        }
        else
        {
            phaseTimer += Time.deltaTime;

            float rotationThisFrame =
                -chargingRotationSpeed * Time.deltaTime;

            currentAngle += rotationThisFrame;

            RotateWeapons(hasSecondWeapon);

            if (phaseTimer >= spinDuration)
            {
                isSpinPhase = false;
                phaseTimer = 0f;
                currentAngle = 0f;

                Debug.Log(
                    "[Flail] Rapid spin complete. Charging started.",
                    this
                );
            }
        }
    }

    private void RotateWeapons(bool hasSecondWeapon)
    {
        Quaternion rotation =
            Quaternion.Euler(0f, 0f, currentAngle);

        if (firstFlail != null)
        {
            firstFlail.localRotation =
                firstRestRotation * rotation;
        }

        if (secondFlail != null && hasSecondWeapon)
        {
            // Both flails rotate in the same direction.
            secondFlail.localRotation =
                secondRestRotation * rotation;
        }
    }

    public void ResetWeapons()
    {
        currentAngle = 0f;
        phaseTimer = 0f;
        isSpinPhase = false;

        if (firstFlail != null)
            firstFlail.localRotation = firstRestRotation;

        if (secondFlail != null)
            secondFlail.localRotation = secondRestRotation;
    }

    private void OnDisable()
    {
        ResetWeapons();
    }
}
