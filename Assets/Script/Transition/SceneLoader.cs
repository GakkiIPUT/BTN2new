using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;// ロードするシーンの名前
    public GameObject loadingUI;// ロードの進捗状況を表示するUIなど
    public Button[] loadButtons;// 複数のボタンを扱うための配列
    private AsyncOperation async;// ロードの進捗状況を管理するための変数

    //// ロードにかける最低時間（秒）
    //[SerializeField] protected float minimumLoadTime = 1f;
    //// ボタンの無効時間
    //[SerializeField] protected float buttonCooldownTime = 5f;

    // ロードを開始するメソッド
    public void StartLoad()
    {
        StartCoroutine(Load());
        DisableButtons();
    }

    // コルーチンを使用してロードを実行するメソッド
    private IEnumerator Load()
    {
        PlayerPrefs.Save();
        loadingUI.SetActive(true);// ロード画面を表示する
        async = SceneManager.LoadSceneAsync(sceneName);// シーンを非同期でロードする
        async.allowSceneActivation = false;// ロードが完了してもすぐにシーンを有効にしない（進行状況は0.9で止まる）
        float elapsedTime = 0f;// ロード開始からの経過時間を計測
        while (!async.isDone)// ロードが完了するか、最低ロード時間が経過するまで待機する
        {
            elapsedTime += UnityEngine.Time.deltaTime;
            if (async.progress >= 0.9f && elapsedTime >= 1f)
            {
                async.allowSceneActivation = true;
            }
            yield return null;
        }
    // ロード画面を非表示にする
    loadingUI.SetActive(false);
    }
    private void DisableButtons()
    {
        foreach (Button button in loadButtons)
        {
            if (button != null)
            {
                button.interactable = false;
            }
        }
    }
}