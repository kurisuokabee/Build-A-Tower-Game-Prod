using UnityEngine;
using UnityEngine.UI;

public class AudioSliders : MonoBehaviour
{
    [SerializeField] Slider master;
    [SerializeField] Slider music;
    [SerializeField] Slider sfx;

    void Start()
    {
        AudioManager.Instance.RegisterSliders(master, music, sfx);
    }
}
