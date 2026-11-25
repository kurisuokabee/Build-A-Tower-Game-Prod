
using UnityEngine;

public class ParticleFollowSakero : MonoBehaviour
{
    private Transform sakero;

    private Vector3 initialOffset;

    private void Start()
    {
        sakero = transform.parent;

        initialOffset = transform.localPosition;

        transform.SetParent(null);
    }

    private void LateUpdate()
    {
        if (sakero == null)
            return;

        transform.position = sakero.position + initialOffset;
    }
}
