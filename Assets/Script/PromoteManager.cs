using UnityEngine;
using UnityEngine.UI;

public class PromoteManager : MonoBehaviour
{
    [SerializeField] private GameObject player1PromotePanel;// 成りオプションを表示するパネル
    [SerializeField] private GameObject player2PromotePanel;
    [SerializeField] private Button player1PromoteButton;// 成りするボタン
    [SerializeField] private Button player1NoPromoteButton;
    [SerializeField] private Button player2PromoteButton;// 成りしないボタン
    [SerializeField] private Button player2NoPromoteButton;

    [SerializeField] private GameTime gameTime;  // Timeクラスの参照を追加

    private UnitController selectedUnit;  // 成り対象の駒
    private GameSystem gameSystem;
    private Camera player1Camera;
    private Camera player2Camera;

    private AudioClip SE;
    //--------初期設定処理開始-----------
    private void Start()
    {
        InitializeCameras();
        gameSystem = UnityEngine.Object.FindAnyObjectByType<GameSystem>();

        // ボタンにリスナーを追加
        player1PromoteButton.onClick.AddListener(() => OnPromoteButtonPressed(true, 0));
        player1NoPromoteButton.onClick.AddListener(() => OnPromoteButtonPressed(false, 0));
        player2PromoteButton.onClick.AddListener(() => OnPromoteButtonPressed(true, 1));
        player2NoPromoteButton.onClick.AddListener(() => OnPromoteButtonPressed(false, 1));

        // パネルを非表示にする
        player1PromotePanel.SetActive(false);
        player2PromotePanel.SetActive(false);
    }
    void InitializeCameras()
    {
        player1Camera = GameObject.Find("Player1Setting").GetComponent<Camera>();
        player2Camera = GameObject.Find("Player2Setting").GetComponent<Camera>();
        player1Camera.rect = new Rect(0, 0, 0.5f, 1);
        player2Camera.rect = new Rect(0.5f, 0, 0.5f, 1);
    }
    //--------初期設定処理終わり----------

    //--------成り処理開始-----------
    public void ShowPromoteOptions(UnitController unit, int currentPlayer)// 成りオプションを表示する
    {
        gameSystem.ClearCursors();
        Debug.Log("a");

        selectedUnit = unit;  // 成り対象の駒を記録
        if (currentPlayer == 0) // プレイヤー1の場合
        {
            player1PromotePanel.SetActive(true);// パネルを表示
        }
        else // プレイヤー2の場合
        {
            player2PromotePanel.SetActive(true);// パネルを表示
        }
        UnityEngine.Time.timeScale = 0; //ゲームの時間を停止
        gameSystem.DisableInput();// 入力を無効にする
        gameSystem.ClearCursors();
        Debug.Log("b");
    }

    // 成り/成らないボタンが押された時の処理
    // ボタンが押されたときに入力を有効に戻す
    private void OnPromoteButtonPressed(bool promote, int currentPlayer)
    {
        if (selectedUnit != null)
        {
            if (promote)
            {
                selectedUnit.Promote();  // 成りを実行
            }
            HidePromoteOptions(currentPlayer);

            //// 入力を有効に戻す
            //GameSystem gameSystem = UnityEngine.Object.FindAnyObjectByType<GameSystem>();
            if (gameSystem != null)
            {
                UnityEngine.Time.timeScale = 1; // ゲームの時間を再開
                //gameTime.ResetBattleTime();         // 時間をリセット
                gameSystem.EnableInput();  // 入力を再度有効にする
                gameSystem.StartOuteNextTurn();
                if (!gameSystem.skipActivateText)
                {
                    gameSystem.ActivateText();
                }
                gameSystem.skipActivateText = false; // フラグをリセットして次ターンで再び動作可能に
                gameSystem.DeselectBoardPieces();
                //gameSystem.EndTurn();  // ターンを切り替える
            }
        }
    }
    public void HidePromoteOptions(int currentPlayer)
    {
        if (currentPlayer == 0)
        {
            player1PromotePanel.SetActive(false);
        }
        else
        {
            player2PromotePanel.SetActive(false);
        }
    }    //--------成り処理終わり----------
}