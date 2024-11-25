using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Guide : MonoBehaviour
{
    public GameObject OldFolder;
    public GameObject NewFolder;
    public GameObject quitButton;
    public Button Return;               // 戻るボタン

    private void Start()
    {
        // 説明文のImageを非表示にする
        NewFolder.SetActive(false);

        // 戻るボタンにリスナーを追加して、説明文を表示する
        Return.onClick.AddListener(ShowExplanationImage);

        // Closeボタンにリスナーを追加して、説明文を非表示にする
        Return.onClick.AddListener(HideExplanationImage);
    }

    // 説明文を表示するメソッド
    public void ShowExplanationImage()
    {
        OldFolder.SetActive(false);
        NewFolder.SetActive(true);  // Imageを表示
    }

    // 説明文を非表示にするメソッド
    public void HideExplanationImage()
    {
        OldFolder.SetActive(true);
        NewFolder.SetActive(false); // Imageを非表示
    }
}
