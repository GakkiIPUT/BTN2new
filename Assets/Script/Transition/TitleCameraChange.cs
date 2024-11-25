using UnityEngine;
using UnityEngine.UI;

public class CameraChange : MonoBehaviour
{
    public GameObject quitPanel;
    public Button Return;               // �߂�{�^��
    public Button quitButton;           // �J������؂�ւ���{�^��
    public Camera mainCamera;           // ���C���J����
    public Camera guideCamera;          // �����p�̃J����

    private void Start()
    {
        // ��������Image���\���ɂ���
        quitPanel.SetActive(false);

        // �����p�J�����𖳌����i���C���J�����̂ݗL���ɂ��Ă����j
        guideCamera.enabled = false;

        // �߂�{�^���Ƀ��X�i�[��ǉ����āA��������\������
        Return.onClick.AddListener(ShowExplanationImage);

        // Close�{�^���Ƀ��X�i�[��ǉ����āA���������\���ɂ���
        Return.onClick.AddListener(HideExplanationImage);

        // �J������؂�ւ���{�^���Ƀ��X�i�[��ǉ�
        quitButton.onClick.AddListener(SwitchCamera);
        Return.onClick.AddListener(SwitchCamera);
    }

    // ��������\�����郁�\�b�h
    public void ShowExplanationImage()
    {
        quitPanel.SetActive(true);  // Image��\��
    }

    // ���������\���ɂ��郁�\�b�h
    public void HideExplanationImage()
    {
        quitPanel.SetActive(false); // Image���\��
    }

    // �J������؂�ւ��郁�\�b�h
    public void SwitchCamera()
    {

        // ���C���J�����Ɛ����p�J�����̗L��/������؂�ւ���
        if (mainCamera.enabled)
        {
            Debug.Log("�ړ�������");
            mainCamera.enabled = false;
            guideCamera.enabled = true;  // �����p�̃J������L����
        }
        else
        {
            Debug.Log("�A���ė�����");
            mainCamera.enabled = true;
            guideCamera.enabled = false; // ���C���J������L����
        }
    }
}