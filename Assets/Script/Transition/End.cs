using UnityEngine;
using UnityEngine.UI;

public class End : MonoBehaviour
{
    public GameObject quitTitleObj;
    public GameObject quitPanel;
    public GameObject quitButton;
    public Button yesButton;
    public Button noButton;

    private void Start()
    {
        quitTitleObj.SetActive(true);
        quitPanel.SetActive(false);

        // ボタンにリスナーを追加
        yesButton.onClick.AddListener(QuitGameYes);
        noButton.onClick.AddListener(QuitGameNo);
    }

    // クリックされたときに呼び出されるメソッド
    public void OnQuitButtonClick()
    {
        quitTitleObj.SetActive(false);
        quitPanel.SetActive(true);
        quitButton.SetActive(false);
    }

    // Yesボタンがクリックされたときに呼び出されるメソッド
    public void QuitGameYes()
    {
        Application.Quit();
        //Debug.Log("終わるぜ！");
    }

    // Noボタンがクリックされたときに呼び出されるメソッド
    public void QuitGameNo()
    {
        quitTitleObj.SetActive(true);
        quitPanel.SetActive(false);
        quitButton.SetActive(true);
    }
}
