using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    // Player1のUI要素
    public GameObject quitPanel1;
    public Button settingsButton1;      // Player1の設定ボタン
    public Button Return1;
    public Button quitButton1;
    public Button guideButton1; // ゲームガイド表示ボタン
    public Camera mainCamera1;
    public Camera guideCamera1;

    // Player2のUI要素
    public GameObject quitPanel2;
    public Button settingsButton2;      // Player2の設定ボタン
    public Button Return2;
    public Button quitButton2;
    public Button guideButton2; // ゲームガイド表示ボタン
    public Camera mainCamera2;
    public Camera guideCamera2;

    private void Start()
    {
        // 初期設定
        SetupCameraView();

        // ボタンにクリックイベントを登録
        settingsButton1.onClick.AddListener(OpenSettings1); // Player1設定画面を開く
        Return1.onClick.AddListener(CloseSettings1);
        quitButton1.onClick.AddListener(ToggleCamera1);
        guideButton1.onClick.AddListener(ShowGuide1);

        settingsButton2.onClick.AddListener(OpenSettings2); // Player2設定画面を開く
        Return2.onClick.AddListener(CloseSettings2);
        quitButton2.onClick.AddListener(ToggleCamera2);
        guideButton2.onClick.AddListener(ShowGuide2);
        // 設定画面を非表示にしておく
        quitPanel1.SetActive(false);
        quitPanel2.SetActive(false);
    }

    // カメラビューの初期配置
    private void SetupCameraView()
    {
        mainCamera1.rect = new Rect(0, 0, 0.5f, 1);
        mainCamera2.rect = new Rect(0.5f, 0, 0.5f, 1);

        guideCamera1.enabled = false;
        guideCamera2.enabled = false;
    }

    // Player1の設定画面を開く
    private void OpenSettings1()
    {
        quitPanel1.SetActive(true);
        mainCamera1.enabled = true;
        guideCamera1.enabled = true;
        FindAnyObjectByType<GameSystem>().DisableInput(); // ゲーム入力を無効化
    }

    // Player2の設定画面を開く
    private void OpenSettings2()
    {
        quitPanel2.SetActive(true);
        mainCamera2.enabled = true;
        guideCamera2.enabled = true;
        FindAnyObjectByType<GameSystem>().DisableInput(); // ゲーム入力を無効化
    }

    // Player1の設定画面を閉じる
    private void CloseSettings1()
    {
        quitPanel1.SetActive(false);
        mainCamera1.enabled = true;   // メインカメラを有効化
        guideCamera1.enabled = false; // ガイドカメラを無効化
        Debug.Log("Player1: 設定画面を閉じ、メインカメラに戻りました");
        FindAnyObjectByType<GameSystem>().EnableInput();  // ゲーム入力を再開
    }

    // Player2の設定画面を閉じる
    private void CloseSettings2()
    {
        quitPanel2.SetActive(false);
        mainCamera2.enabled = true;   // メインカメラを有効化
        guideCamera2.enabled = false; // ガイドカメラを無効化
        Debug.Log("Player2: 設定画面を閉じ、メインカメラに戻りました");
        FindAnyObjectByType<GameSystem>().EnableInput();  // ゲーム入力を再開
    }
    // Player1のカメラ切り替え
    private void ToggleCamera1()
    {
        bool isMainCameraActive = mainCamera1.enabled;
        mainCamera1.enabled = !isMainCameraActive;
        guideCamera1.enabled = isMainCameraActive;
    }

    // Player2のカメラ切り替え
    private void ToggleCamera2()
    {
        bool isMainCameraActive = mainCamera2.enabled;
        mainCamera2.enabled = !isMainCameraActive;
        guideCamera2.enabled = isMainCameraActive;
    }

    // Player1のゲームガイドを表示
    private void ShowGuide1()
    {
        guideCamera1.enabled = true;
        mainCamera1.enabled = false;
    }

    // Player2のゲームガイドを表示
    private void ShowGuide2()
    {
        guideCamera2.enabled = true;
        mainCamera2.enabled = false;
    }
}