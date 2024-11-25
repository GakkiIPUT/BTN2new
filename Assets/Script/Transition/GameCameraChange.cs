using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    // Player1��UI�v�f
    public GameObject quitPanel1;
    public Button settingsButton1;      // Player1�̐ݒ�{�^��
    public Button Return1;
    public Button quitButton1;
    public Button guideButton1; // �Q�[���K�C�h�\���{�^��
    public Camera mainCamera1;
    public Camera guideCamera1;

    // Player2��UI�v�f
    public GameObject quitPanel2;
    public Button settingsButton2;      // Player2�̐ݒ�{�^��
    public Button Return2;
    public Button quitButton2;
    public Button guideButton2; // �Q�[���K�C�h�\���{�^��
    public Camera mainCamera2;
    public Camera guideCamera2;

    private void Start()
    {
        // �����ݒ�
        SetupCameraView();

        // �{�^���ɃN���b�N�C�x���g��o�^
        settingsButton1.onClick.AddListener(OpenSettings1); // Player1�ݒ��ʂ��J��
        Return1.onClick.AddListener(CloseSettings1);
        quitButton1.onClick.AddListener(ToggleCamera1);
        guideButton1.onClick.AddListener(ShowGuide1);

        settingsButton2.onClick.AddListener(OpenSettings2); // Player2�ݒ��ʂ��J��
        Return2.onClick.AddListener(CloseSettings2);
        quitButton2.onClick.AddListener(ToggleCamera2);
        guideButton2.onClick.AddListener(ShowGuide2);
        // �ݒ��ʂ��\���ɂ��Ă���
        quitPanel1.SetActive(false);
        quitPanel2.SetActive(false);
    }

    // �J�����r���[�̏����z�u
    private void SetupCameraView()
    {
        mainCamera1.rect = new Rect(0, 0, 0.5f, 1);
        mainCamera2.rect = new Rect(0.5f, 0, 0.5f, 1);

        guideCamera1.enabled = false;
        guideCamera2.enabled = false;
    }

    // Player1�̐ݒ��ʂ��J��
    private void OpenSettings1()
    {
        quitPanel1.SetActive(true);
        mainCamera1.enabled = true;
        guideCamera1.enabled = true;
        FindAnyObjectByType<GameSystem>().DisableInput(); // �Q�[�����͂𖳌���
    }

    // Player2�̐ݒ��ʂ��J��
    private void OpenSettings2()
    {
        quitPanel2.SetActive(true);
        mainCamera2.enabled = true;
        guideCamera2.enabled = true;
        FindAnyObjectByType<GameSystem>().DisableInput(); // �Q�[�����͂𖳌���
    }

    // Player1�̐ݒ��ʂ����
    private void CloseSettings1()
    {
        quitPanel1.SetActive(false);
        mainCamera1.enabled = true;   // ���C���J������L����
        guideCamera1.enabled = false; // �K�C�h�J�����𖳌���
        Debug.Log("Player1: �ݒ��ʂ���A���C���J�����ɖ߂�܂���");
        FindAnyObjectByType<GameSystem>().EnableInput();  // �Q�[�����͂��ĊJ
    }

    // Player2�̐ݒ��ʂ����
    private void CloseSettings2()
    {
        quitPanel2.SetActive(false);
        mainCamera2.enabled = true;   // ���C���J������L����
        guideCamera2.enabled = false; // �K�C�h�J�����𖳌���
        Debug.Log("Player2: �ݒ��ʂ���A���C���J�����ɖ߂�܂���");
        FindAnyObjectByType<GameSystem>().EnableInput();  // �Q�[�����͂��ĊJ
    }
    // Player1�̃J�����؂�ւ�
    private void ToggleCamera1()
    {
        bool isMainCameraActive = mainCamera1.enabled;
        mainCamera1.enabled = !isMainCameraActive;
        guideCamera1.enabled = isMainCameraActive;
    }

    // Player2�̃J�����؂�ւ�
    private void ToggleCamera2()
    {
        bool isMainCameraActive = mainCamera2.enabled;
        mainCamera2.enabled = !isMainCameraActive;
        guideCamera2.enabled = isMainCameraActive;
    }

    // Player1�̃Q�[���K�C�h��\��
    private void ShowGuide1()
    {
        guideCamera1.enabled = true;
        mainCamera1.enabled = false;
    }

    // Player2�̃Q�[���K�C�h��\��
    private void ShowGuide2()
    {
        guideCamera2.enabled = true;
        mainCamera2.enabled = false;
    }
}