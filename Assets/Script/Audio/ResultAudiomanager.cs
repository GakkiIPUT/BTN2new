using UnityEngine;

public class ResultAudiomanager : MonoBehaviour
{
    [SerializeField] AudioSource se;
    [SerializeField] AudioSource win;
    [SerializeField] AudioSource lose;
    private void Start()
    {
        win.PlayOneShot(win.clip);
    }

    public void CliskSE()
    {
        se.PlayOneShot(se.clip);
    }
}
