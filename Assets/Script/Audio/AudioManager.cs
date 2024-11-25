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
        // �X���C�_�[�̏����l�� PlayerPrefs ����ǂݍ���
        float savedBgmValue = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float savedSeValue = PlayerPrefs.GetFloat("SEVolume", 1f);

        bgmSlider1.value = savedBgmValue;
        bgmSlider2.value = savedBgmValue;
        seSlider1.value = savedSeValue;
        seSlider2.value = savedSeValue;

        // �I�[�f�B�I�~�L�T�[�ɏ����l��ݒ�
        audioMixer.SetFloat("BGM", savedBgmValue * 80 - 80f);
        audioMixer.SetFloat("SE", savedSeValue * 80 - 80f);

        // �X���C�_�[�̒l���ς�����Ƃ��ɉ��ʂ�ύX���郊�X�i�[��ǉ�
        bgmSlider1.onValueChanged.AddListener((value) =>
        {
            value = Mathf.Clamp01(value);
            float decibel = 20f * Mathf.Log10(value);
            decibel = Mathf.Clamp(decibel, -80f, 0f);
            audioMixer.SetFloat("BGM", decibel);

            // �X���C�_�[2�̒l�𓯊�
            bgmSlider2.value = value;
            PlayerPrefs.SetFloat("BGMVolume", value); // �ۑ�
        });

        bgmSlider2.onValueChanged.AddListener((value) =>
        {
            value = Mathf.Clamp01(value);
            float decibel = 20f * Mathf.Log10(value);
            decibel = Mathf.Clamp(decibel, -80f, 0f);
            audioMixer.SetFloat("BGM", decibel);

            // �X���C�_�[1�̒l�𓯊�
            bgmSlider1.value = value;
            PlayerPrefs.SetFloat("BGMVolume", value); // �ۑ�
        });

        seSlider1.onValueChanged.AddListener((value) =>
        {
            value = Mathf.Clamp01(value);
            float decibel = 20f * Mathf.Log10(value);
            decibel = Mathf.Clamp(decibel, -80f, 0f);
            audioMixer.SetFloat("SE", decibel);

            // �X���C�_�[2�̒l�𓯊�
            seSlider2.value = value;
            PlayerPrefs.SetFloat("SEVolume", value); // �ۑ�
        });

        seSlider2.onValueChanged.AddListener((value) =>
        {
            value = Mathf.Clamp01(value);
            float decibel = 20f * Mathf.Log10(value);
            decibel = Mathf.Clamp(decibel, -80f, 0f);
            audioMixer.SetFloat("SE", decibel);

            // �X���C�_�[1�̒l�𓯊�
            seSlider1.value = value;
            PlayerPrefs.SetFloat("SEVolume", value); // �ۑ�
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
