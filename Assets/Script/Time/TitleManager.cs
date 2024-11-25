using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [SerializeField, Header("一手Dropdown")]
    private TMP_Dropdown[] titleTimeDropdown;
    [SerializeField, Header("持ち時間")]
    private TMP_Dropdown[] titleTime2Dropdown;

    [SerializeField, Header("一手Image")]
    private Image[] titleTimeHighlight; // 光るエフェクト用のUIオブジェクトの配列
    [SerializeField, Header("持ち時間Image")]
    private Image[] titleTime2Highlight; // 光るエフェクト用のUIオブジェクトの配列

    private int[] titleTimePresetTimes = new int[] { 30, 60, 300 };
    private int[] titleTime2PresetTimes = new int[] { 600, 1800, 3600 };

    private void Start()
    {
        // 複数のDropdownを初期化し、リスナーを設定
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

        // 最初はすべてのハイライトを非表示
        SetHighlightsActive(titleTimeHighlight, false);
        SetHighlightsActive(titleTime2Highlight, false);

        PlayerPrefs.Save();
    }

    private void InitializeDropdown(TMP_Dropdown dropdown, int[] presetTimes)
    {
        dropdown.options.Clear();
        dropdown.options.Add(new TMP_Dropdown.OptionData("時間選択"));

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

            // ログを出力
            Debug.Log($"TitleTime1: {FormatTimeOption(selectedTime1)} が選択されました。");

            // titleTime2Dropdownを選択していない状態（インデックス0）にリセット
            foreach (var dropdown in titleTime2Dropdown)
            {
                dropdown.value = 0;
                dropdown.RefreshShownValue();
            }

            // 他のtitleTimeDropdownも選択状態を同期
            foreach (var dropdown in titleTimeDropdown)
            {
                if (dropdown.value != selectedIndex)
                {
                    dropdown.value = selectedIndex;
                    dropdown.RefreshShownValue();
                }
            }

            // titleTimeHighlight を有効にし、titleTime2Highlight を無効にする
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

            // ログを出力
            Debug.Log($"TitleTime2: {FormatTimeOption(selectedTime2)} が選択されました。");

            // titleTimeDropdownを選択していない状態（インデックス0）にリセット
            foreach (var dropdown in titleTimeDropdown)
            {
                dropdown.value = 0;
                dropdown.RefreshShownValue();
            }

            // 他のtitleTime2Dropdownも選択状態を同期
            foreach (var dropdown in titleTime2Dropdown)
            {
                if (dropdown.value != selectedIndex)
                {
                    dropdown.value = selectedIndex;
                    dropdown.RefreshShownValue();
                }
            }

            // titleTime2Highlight を有効にし、titleTimeHighlight を無効にする
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
            ? (remainingSeconds > 0 ? $"{minutes} 分 {remainingSeconds} 秒" : $"{minutes} 分")
            : $"{seconds} 秒";
    }

    private void SetHighlightsActive(Image[] highlights, bool isActive)
    {
        foreach (Image highlight in highlights)
        {
            highlight.gameObject.SetActive(isActive);
        }
    }
}
