
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardPopupManager : MonoBehaviour
{
    public static RewardPopupManager Instance { get; private set; }

    [Header("Popup")]
    [SerializeField] private RewardPopup popupPrefab;
    [SerializeField] private float popupDelay = 0.3f;

    private readonly Queue<RewardData> rewardQueue = new Queue<RewardData>();
    private Coroutine popupCoroutine;

    private struct RewardData
    {
        public Vector3 position;
        public string message;
        public Color color;

        public RewardData(Vector3 position, string message, Color color)
        {
            this.position = position;
            this.message = message;
            this.color = color;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ShowReward(Vector3 position, string message, Color color)
    {
        rewardQueue.Enqueue(new RewardData(position, message, color));

        if (popupCoroutine == null)
            popupCoroutine = StartCoroutine(ProcessRewardQueue());
    }

    private IEnumerator ProcessRewardQueue()
    {
        while (rewardQueue.Count > 0)
        {
            RewardData reward = rewardQueue.Dequeue();

            GameObject popupObject = ObjectPoolManager.Instance.GetObject(
                popupPrefab.gameObject,
                reward.position,
                Quaternion.identity
            );

            popupObject.transform.position = reward.position;

            RewardPopup popup = popupObject.GetComponent<RewardPopup>();
            popup.Show(reward.message, reward.color);

            yield return new WaitForSeconds(popupDelay);
        }

        popupCoroutine = null;
    }
}
