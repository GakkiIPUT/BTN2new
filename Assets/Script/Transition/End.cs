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

        // �{�^���Ƀ��X�i�[��ǉ�
        yesButton.onClick.AddListener(QuitGameYes);
        noButton.onClick.AddListener(QuitGameNo);
    }

    // �N���b�N���ꂽ�Ƃ��ɌĂяo����郁�\�b�h
    public void OnQuitButtonClick()
    {
        quitTitleObj.SetActive(false);
        quitPanel.SetActive(true);
        quitButton.SetActive(false);
    }

    // Yes�{�^�����N���b�N���ꂽ�Ƃ��ɌĂяo����郁�\�b�h
    public void QuitGameYes()
    {
        Application.Quit();
        //Debug.Log("�I��邺�I");
    }

    // No�{�^�����N���b�N���ꂽ�Ƃ��ɌĂяo����郁�\�b�h
    public void QuitGameNo()
    {
        quitTitleObj.SetActive(true);
        quitPanel.SetActive(false);
        quitButton.SetActive(true);
    }
}
