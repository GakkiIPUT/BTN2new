using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Guide : MonoBehaviour
{
    public GameObject OldFolder;
    public GameObject NewFolder;
    public GameObject quitButton;
    public Button Return;               // �߂�{�^��

    private void Start()
    {
        // ��������Image���\���ɂ���
        NewFolder.SetActive(false);

        // �߂�{�^���Ƀ��X�i�[��ǉ����āA��������\������
        Return.onClick.AddListener(ShowExplanationImage);

        // Close�{�^���Ƀ��X�i�[��ǉ����āA���������\���ɂ���
        Return.onClick.AddListener(HideExplanationImage);
    }

    // ��������\�����郁�\�b�h
    public void ShowExplanationImage()
    {
        OldFolder.SetActive(false);
        NewFolder.SetActive(true);  // Image��\��
    }

    // ���������\���ɂ��郁�\�b�h
    public void HideExplanationImage()
    {
        OldFolder.SetActive(true);
        NewFolder.SetActive(false); // Image���\��
    }
}
