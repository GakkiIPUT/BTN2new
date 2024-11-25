using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class Result : MonoBehaviour
{
    public Button returnButton1;
    public Button returnButton2;

    // プレイヤー1のキャンバス内のテキスト参照
    public TMP_Text player1MovesText;        // 手数
    public TMP_Text totalTimeText_Player1;    // 試合時間
    public TMP_Text winnerText_Player1;       // 勝敗結果

    // プレイヤー2のキャンバス内のテキスト参照
    public TMP_Text player2MovesText;        // 手数
    public TMP_Text totalTimeText_Player2;    // 試合時間
    public TMP_Text winnerText_Player2;       // 勝敗結果

    // 盤面表示用のUI参照
    public RawImage finalBoardImage1;  // 盤面画像を表示するRawImage
    public Button showFinalBoardButton1;  // 盤面画像を表示するボタン
    public RawImage finalBoardImage2;  
    public Button showFinalBoardButton2;

    void Start()
    {
        DisplayResult();
        // 各ボタンにクリックイベントを追加
        showFinalBoardButton1.onClick.AddListener(() => DisplayFinalBoard(finalBoardImage1, "BothCameras.png", returnButton1));
        showFinalBoardButton2.onClick.AddListener(() => DisplayFinalBoard(finalBoardImage2, "BothCameras.png", returnButton2));

        // 戻るボタンのクリックイベントにDisplayResultメソッドを設定
        returnButton1.onClick.AddListener(() => HideFinalBoardAndShowResult(finalBoardImage1, returnButton1));
        returnButton2.onClick.AddListener(() => HideFinalBoardAndShowResult(finalBoardImage2, returnButton2));
    }

    void DisplayResult()
    {
        // PlayerPrefsから各情報を取得
        int player1Moves = PlayerPrefs.GetInt("Player1MoveCount", 0);
        int player2Moves = PlayerPrefs.GetInt("Player2MoveCount", 0);
        int totalTime = PlayerPrefs.GetInt("TotalTime", 0);
        string winner = PlayerPrefs.GetString("Winner", "引き分け");

        // プレイヤー1の結果を表示
        player1MovesText.text = "あなたが差した手数: " + player1Moves + "手";
        totalTimeText_Player1.text = "試合時間: " + FormatTime(totalTime);
        winnerText_Player1.text = winner == "Player1" ? "WIN！" : "Lose...";

        // プレイヤー2の結果を表示
        player2MovesText.text = "あなたが差した手数: " + player2Moves + "手";
        totalTimeText_Player2.text = "試合時間: " + FormatTime(totalTime);
        winnerText_Player2.text = winner == "Player2" ? "WIN！" : "Lose...";
    }
    // 秒数を「秒・分・時間」の形式にフォーマット
    string FormatTime(int seconds)
    {
        if (seconds >= 3600)
        {
            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            return $"{hours}時間 {minutes}分";
        }
        else if (seconds >= 60)
        {
            int minutes = seconds / 60;
            int secs = seconds % 60;
            return $"{minutes}分 {secs}秒";
        }
        else
        {
            return $"{seconds}秒";
        }
    }
    void DisplayFinalBoard(RawImage finalBoardImage, string fileName, Button returnButton)
    {
        string path = Application.persistentDataPath + "/" + fileName;

        if (File.Exists(path))
        {
            // 画像を読み込み、テクスチャとして適用
            byte[] imageData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);
            finalBoardImage.texture = texture;
            finalBoardImage.gameObject.SetActive(true);  // RawImageを表示
            returnButton.gameObject.SetActive(true); // 戻るボタンを表示
        }
        else
        {
            Debug.LogWarning("最終盤面の画像が見つかりません: " + path);
        }
    }
    void HideFinalBoardAndShowResult(RawImage finalBoardImage, Button returnButton)
    {
        finalBoardImage.gameObject.SetActive(false);  // 盤面画像を非表示
        returnButton.gameObject.SetActive(false);     // 戻るボタンを非表示
        DisplayResult();                              // リザルト表示を更新
    }
}

