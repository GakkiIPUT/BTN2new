using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Player1Handicap : MonoBehaviour
{
    [SerializeField, Header("Handicap ドロップダウン")]
    private TMP_Dropdown HandicapDropdown;

    //public TMP_Text displayText;  // 選択内容を表示するためのテキスト

    private Dictionary<int, List<(int row, int col)>> handicapSettings;
    public List<(int row, int col)> CurrentHandicapSetting { get; private set; } = new List<(int row, int col)>();

    private void Awake()
    {
        InitializeHandicapSettings();
        SetupDropdown();
        HandicapDropdown.onValueChanged.AddListener(OnHandicapSelected);
    }

    private void Start()
    {
        // 初期選択された値に基づき、駒落ち設定を反映
        OnHandicapSelected(HandicapDropdown.value);
    }

    private void InitializeHandicapSettings()
    {
        handicapSettings = new Dictionary<int, List<(int, int)>>()
        {
            { 0, new List<(int, int)> { } }, // なし
            { 1, new List<(int, int)> { (8, 8) } }, //左側(角のある側)の香
            { 2, new List<(int, int)> { (7, 7) } }, // 角落ち
            { 3, new List<(int, int)> { (1, 7) } }, //飛車落ち
            { 4, new List<(int, int)> { (1, 7), (8, 8) } }, //飛車と左側(角のある側)の香
            { 5, new List<(int, int)> { (7, 7), (1, 7) } }, //飛車と角
            { 6, new List<(int, int)> { (7, 7), (1, 7), (8, 8), (0, 8) }}, //飛車と角、両方の香
            { 7, new List<(int, int)> { (7, 7), (1, 7), (8, 8), (0, 8), (7, 8), (1, 8) }}, //飛車と角、両方の桂と香
        };
        //Debug.Log("Handicap - 駒落ち設定を初期化");
    }

    private void SetupDropdown()
    {
        HandicapDropdown.ClearOptions();
        HandicapDropdown.AddOptions(new List<string> { "無し", "香落ち(左側の香)", "角落ち", "飛車落ち", "飛車香落ち(飛車と左側の香)", "二枚落ち(飛車と角)", "四枚落ち(飛車と角、両方の香)", "六枚落ち(飛車と角、両方の桂と香)" });
        //Debug.Log("Handicap - ドロップダウンにオプションを設定");
    }

    private void OnHandicapSelected(int index)
    {
        if (handicapSettings.ContainsKey(index))
        {
            CurrentHandicapSetting = handicapSettings[index];
            PlayerPrefs.SetInt("HandicapSetting1", index); // インデックスを保存

            // 駒落ちの位置情報を文字列で保存
            string positions = string.Join(";", CurrentHandicapSetting.Select(pos => $"{pos.row},{pos.col}"));
            PlayerPrefs.SetString("HandicapPositions1", positions);

            //Debug.Log("駒落ち設定が変更されました" + index + ", 設定内容 " + positions);
            //displayText.text = ("駒落ち設定が変更されました1" + index + ", 設定内容 " + positions);
        }
    }
}
