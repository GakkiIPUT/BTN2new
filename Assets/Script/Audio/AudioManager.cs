using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] AudioSource se;
    [SerializeField] AudioSource startse;
    [SerializeField] Slider bgmSlider1;
    [SerializeField] Slider seSlider1;
    [SerializeField] Slider bgmSlider2;
    [SerializeField] Slider seSlider2;

    private void Start()
    {
        // スライダーの初期値を PlayerPrefs から読み込む
        float savedBgmValue = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float savedSeValue = PlayerPrefs.GetFloat("SEVolume", 1f);

        bgmSlider1.value = savedBgmValue;
        bgmSlider2.value = savedBgmValue;
        seSlider1.value = savedSeValue;
        seSlider2.value = savedSeValue;

        // オーディオミキサーに初期値を設定
        audioMixer.SetFloat("BGM", savedBgmValue * 80 - 80f);
        audioMixer.SetFloat("SE", savedSeValue * 80 - 80f);

        // スライダーの値が変わったときに音量を変更するリスナーを追加
        bgmSlider1.onValueChanged.AddListener((value) =>
        {
            value = Mathf.Clamp01(value);
            float decibel = 20f * Mathf.Log10(value);
            decibel = Mathf.Clamp(decibel, -80f, 0f);
            audioMixer.SetFloat("BGM", decibel);

            // スライダー2の値を同期
            bgmSlider2.value = value;
            PlayerPrefs.SetFloat("BGMVolume", value); // 保存
        });

        bgmSlider2.onValueChanged.AddListener((value) =>
        {
            value = Mathf.Clamp01(value);
            float decibel = 20f * Mathf.Log10(value);
            decibel = Mathf.Clamp(decibel, -80f, 0f);
            audioMixer.SetFloat("BGM", decibel);

            // スライダー1の値を同期
            bgmSlider1.value = value;
            PlayerPrefs.SetFloat("BGMVolume", value); // 保存
        });

        seSlider1.onValueChanged.AddListener((value) =>
        {
            value = Mathf.Clamp01(value);
            float decibel = 20f * Mathf.Log10(value);
            decibel = Mathf.Clamp(decibel, -80f, 0f);
            audioMixer.SetFloat("SE", decibel);

            // スライダー2の値を同期
            seSlider2.value = value;
            PlayerPrefs.SetFloat("SEVolume", value); // 保存
        });

        seSlider2.onValueChanged.AddListener((value) =>
        {
            value = Mathf.Clamp01(value);
            float decibel = 20f * Mathf.Log10(value);
            decibel = Mathf.Clamp(decibel, -80f, 0f);
            audioMixer.SetFloat("SE", decibel);

            // スライダー1の値を同期
            seSlider1.value = value;
            PlayerPrefs.SetFloat("SEVolume", value); // 保存
        });
    }
    public void ClickSe()
    {
        se.PlayOneShot(se.clip);
    }
    public void StartSe()
    {
        startse.PlayOneShot(startse.clip);
    }
}
