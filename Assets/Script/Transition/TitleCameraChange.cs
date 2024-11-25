using UnityEngine;
using UnityEngine.UI;

public class CameraChange : MonoBehaviour
{
    public GameObject quitPanel;
    public Button Return;               // 戻るボタン
    public Button quitButton;           // カメラを切り替えるボタン
    public Camera mainCamera;           // メインカメラ
    public Camera guideCamera;          // 説明用のカメラ

    private void Start()
    {
        // 説明文のImageを非表示にする
        quitPanel.SetActive(false);

        // 説明用カメラを無効化（メインカメラのみ有効にしておく）
        guideCamera.enabled = false;

        // 戻るボタンにリスナーを追加して、説明文を表示する
        Return.onClick.AddListener(ShowExplanationImage);

        // Closeボタンにリスナーを追加して、説明文を非表示にする
        Return.onClick.AddListener(HideExplanationImage);

        // カメラを切り替えるボタンにリスナーを追加
        quitButton.onClick.AddListener(SwitchCamera);
        Return.onClick.AddListener(SwitchCamera);
    }

    // 説明文を表示するメソッド
    public void ShowExplanationImage()
    {
        quitPanel.SetActive(true);  // Imageを表示
    }

    // 説明文を非表示にするメソッド
    public void HideExplanationImage()
    {
        quitPanel.SetActive(false); // Imageを非表示
    }

    // カメラを切り替えるメソッド
    public void SwitchCamera()
    {

        // メインカメラと説明用カメラの有効/無効を切り替える
        if (mainCamera.enabled)
        {
            Debug.Log("移動したよ");
            mainCamera.enabled = false;
            guideCamera.enabled = true;  // 説明用のカメラを有効化
        }
        else
        {
            Debug.Log("帰って来たよ");
            mainCamera.enabled = true;
            guideCamera.enabled = false; // メインカメラを有効化
        }
    }
}