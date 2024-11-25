using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [SerializeField, Header("���Dropdown")]
    private TMP_Dropdown[] titleTimeDropdown;
    [SerializeField, Header("��������")]
    private TMP_Dropdown[] titleTime2Dropdown;

    [SerializeField, Header("���Image")]
    private Image[] titleTimeHighlight; // ����G�t�F�N�g�p��UI�I�u�W�F�N�g�̔z��
    [SerializeField, Header("��������Image")]
    private Image[] titleTime2Highlight; // ����G�t�F�N�g�p��UI�I�u�W�F�N�g�̔z��

    private int[] titleTimePresetTimes = new int[] { 30, 60, 300 };
    private int[] titleTime2PresetTimes = new int[] { 600, 1800, 3600 };

    private void Start()
    {
        // ������Dropdown�����������A���X�i�[��ݒ�
        foreach (TMP_Dropdown dropdown in titleTimeDropdown)
        {
            InitializeDropdown(dropdown, titleTimePresetTimes);
            dropdown.onValueChanged.AddListener((int index) => OnTitleTimeDropdownChanged(index, titleTimeDropdown));
        }

        foreach (TMP_Dropdown dropdown in titleTime2Dropdown)
        {
            InitializeDropdown(dropdown, titleTime2PresetTimes);
            dropdown.onValueChanged.AddListener((int index) => OnTitleTime2DropdownChanged(index, titleTime2Dropdown));
        }

        // �ŏ��͂��ׂẴn�C���C�g���\��
        SetHighlightsActive(titleTimeHighlight, false);
        SetHighlightsActive(titleTime2Highlight, false);

        PlayerPrefs.Save();
    }

    private void InitializeDropdown(TMP_Dropdown dropdown, int[] presetTimes)
    {
        dropdown.options.Clear();
        dropdown.options.Add(new TMP_Dropdown.OptionData("���ԑI��"));

        foreach (int time in presetTimes)
        {
            string timeOption = FormatTimeOption(time);
            dropdown.options.Add(new TMP_Dropdown.OptionData(timeOption));
        }

        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }

    private void OnTitleTimeDropdownChanged(int selectedIndex, TMP_Dropdown[] dropdowns)
    {
        if (selectedIndex > 0)
        {
            int selectedTime1 = titleTimePresetTimes[selectedIndex - 1];
            PlayerPrefs.SetInt("SelectedBattleTime1", selectedTime1);
            PlayerPrefs.SetString("SelectedTimeType", "TitleTime1");
            PlayerPrefs.Save();

            // ���O���o��
            Debug.Log($"TitleTime1: {FormatTimeOption(selectedTime1)} ���I������܂����B");

            // titleTime2Dropdown��I�����Ă��Ȃ���ԁi�C���f�b�N�X0�j�Ƀ��Z�b�g
            foreach (var dropdown in titleTime2Dropdown)
            {
                dropdown.value = 0;
                dropdown.RefreshShownValue();
            }

            // ����titleTimeDropdown���I����Ԃ𓯊�
            foreach (var dropdown in titleTimeDropdown)
            {
                if (dropdown.value != selectedIndex)
                {
                    dropdown.value = selectedIndex;
                    dropdown.RefreshShownValue();
                }
            }

            // titleTimeHighlight ��L���ɂ��AtitleTime2Highlight �𖳌��ɂ���
            SetHighlightsActive(titleTimeHighlight, true);
            SetHighlightsActive(titleTime2Highlight, false);
        }
        else
        {
            SetHighlightsActive(titleTimeHighlight, false);
            SetHighlightsActive(titleTime2Highlight, false);
        }
    }

    private void OnTitleTime2DropdownChanged(int selectedIndex, TMP_Dropdown[] dropdowns)
    {
        if (selectedIndex > 0)
        {
            int selectedTime2 = titleTime2PresetTimes[selectedIndex - 1];
            PlayerPrefs.SetInt("SelectedBattleTime2", selectedTime2);
            PlayerPrefs.SetString("SelectedTimeType", "TitleTime2");
            PlayerPrefs.Save();

            // ���O���o��
            Debug.Log($"TitleTime2: {FormatTimeOption(selectedTime2)} ���I������܂����B");

            // titleTimeDropdown��I�����Ă��Ȃ���ԁi�C���f�b�N�X0�j�Ƀ��Z�b�g
            foreach (var dropdown in titleTimeDropdown)
            {
                dropdown.value = 0;
                dropdown.RefreshShownValue();
            }

            // ����titleTime2Dropdown���I����Ԃ𓯊�
            foreach (var dropdown in titleTime2Dropdown)
            {
                if (dropdown.value != selectedIndex)
                {
                    dropdown.value = selectedIndex;
                    dropdown.RefreshShownValue();
                }
            }

            // titleTime2Highlight ��L���ɂ��AtitleTimeHighlight �𖳌��ɂ���
            SetHighlightsActive(titleTime2Highlight, true);
            SetHighlightsActive(titleTimeHighlight, false);
        }
        else
        {
            SetHighlightsActive(titleTime2Highlight, false);
            SetHighlightsActive(titleTimeHighlight, false);
        }
    }

    private string FormatTimeOption(int seconds)
    {
        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        return minutes > 0
            ? (remainingSeconds > 0 ? $"{minutes} �� {remainingSeconds} �b" : $"{minutes} ��")
            : $"{seconds} �b";
    }

    private void SetHighlightsActive(Image[] highlights, bool isActive)
    {
        foreach (Image highlight in highlights)
        {
            highlight.gameObject.SetActive(isActive);
        }
    }
}
