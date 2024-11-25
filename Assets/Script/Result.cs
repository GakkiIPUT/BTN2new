using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class Result : MonoBehaviour
{
    public Button returnButton1;
    public Button returnButton2;

    // �v���C���[1�̃L�����o�X���̃e�L�X�g�Q��
    public TMP_Text player1MovesText;        // �萔
    public TMP_Text totalTimeText_Player1;    // ��������
    public TMP_Text winnerText_Player1;       // ���s����

    // �v���C���[2�̃L�����o�X���̃e�L�X�g�Q��
    public TMP_Text player2MovesText;        // �萔
    public TMP_Text totalTimeText_Player2;    // ��������
    public TMP_Text winnerText_Player2;       // ���s����

    // �Ֆʕ\���p��UI�Q��
    public RawImage finalBoardImage1;  // �Ֆʉ摜��\������RawImage
    public Button showFinalBoardButton1;  // �Ֆʉ摜��\������{�^��
    public RawImage finalBoardImage2;  
    public Button showFinalBoardButton2;

    void Start()
    {
        DisplayResult();
        // �e�{�^���ɃN���b�N�C�x���g��ǉ�
        showFinalBoardButton1.onClick.AddListener(() => DisplayFinalBoard(finalBoardImage1, "BothCameras.png", returnButton1));
        showFinalBoardButton2.onClick.AddListener(() => DisplayFinalBoard(finalBoardImage2, "BothCameras.png", returnButton2));

        // �߂�{�^���̃N���b�N�C�x���g��DisplayResult���\�b�h��ݒ�
        returnButton1.onClick.AddListener(() => HideFinalBoardAndShowResult(finalBoardImage1, returnButton1));
        returnButton2.onClick.AddListener(() => HideFinalBoardAndShowResult(finalBoardImage2, returnButton2));
    }

    void DisplayResult()
    {
        // PlayerPrefs����e�����擾
        int player1Moves = PlayerPrefs.GetInt("Player1MoveCount", 0);
        int player2Moves = PlayerPrefs.GetInt("Player2MoveCount", 0);
        int totalTime = PlayerPrefs.GetInt("TotalTime", 0);
        string winner = PlayerPrefs.GetString("Winner", "��������");

        // �v���C���[1�̌��ʂ�\��
        player1MovesText.text = "���Ȃ����������萔: " + player1Moves + "��";
        totalTimeText_Player1.text = "��������: " + FormatTime(totalTime);
        winnerText_Player1.text = winner == "Player1" ? "WIN�I" : "Lose...";

        // �v���C���[2�̌��ʂ�\��
        player2MovesText.text = "���Ȃ����������萔: " + player2Moves + "��";
        totalTimeText_Player2.text = "��������: " + FormatTime(totalTime);
        winnerText_Player2.text = winner == "Player2" ? "WIN�I" : "Lose...";
    }
    // �b�����u�b�E���E���ԁv�̌`���Ƀt�H�[�}�b�g
    string FormatTime(int seconds)
    {
        if (seconds >= 3600)
        {
            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            return $"{hours}���� {minutes}��";
        }
        else if (seconds >= 60)
        {
            int minutes = seconds / 60;
            int secs = seconds % 60;
            return $"{minutes}�� {secs}�b";
        }
        else
        {
            return $"{seconds}�b";
        }
    }
    void DisplayFinalBoard(RawImage finalBoardImage, string fileName, Button returnButton)
    {
        string path = Application.persistentDataPath + "/" + fileName;

        if (File.Exists(path))
        {
            // �摜��ǂݍ��݁A�e�N�X�`���Ƃ��ēK�p
            byte[] imageData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);
            finalBoardImage.texture = texture;
            finalBoardImage.gameObject.SetActive(true);  // RawImage��\��
            returnButton.gameObject.SetActive(true); // �߂�{�^����\��
        }
        else
        {
            Debug.LogWarning("�ŏI�Ֆʂ̉摜��������܂���: " + path);
        }
    }
    void HideFinalBoardAndShowResult(RawImage finalBoardImage, Button returnButton)
    {
        finalBoardImage.gameObject.SetActive(false);  // �Ֆʉ摜���\��
        returnButton.gameObject.SetActive(false);     // �߂�{�^�����\��
        DisplayResult();                              // ���U���g�\�����X�V
    }
}

