using UnityEngine;
using UnityEngine.UI;

public class PromoteManager : MonoBehaviour
{
    [SerializeField] private GameObject player1PromotePanel;// ����I�v�V������\������p�l��
    [SerializeField] private GameObject player2PromotePanel;
    [SerializeField] private Button player1PromoteButton;// ���肷��{�^��
    [SerializeField] private Button player1NoPromoteButton;
    [SerializeField] private Button player2PromoteButton;// ���肵�Ȃ��{�^��
    [SerializeField] private Button player2NoPromoteButton;

    [SerializeField] private GameTime gameTime;  // Time�N���X�̎Q�Ƃ�ǉ�

    private UnitController selectedUnit;  // ����Ώۂ̋�
    private GameSystem gameSystem;
    private Camera player1Camera;
    private Camera player2Camera;

    private AudioClip SE;
    //--------�����ݒ菈���J�n-----------
    private void Start()
    {
        InitializeCameras();
        gameSystem = UnityEngine.Object.FindAnyObjectByType<GameSystem>();

        // �{�^���Ƀ��X�i�[��ǉ�
        player1PromoteButton.onClick.AddListener(() => OnPromoteButtonPressed(true, 0));
        player1NoPromoteButton.onClick.AddListener(() => OnPromoteButtonPressed(false, 0));
        player2PromoteButton.onClick.AddListener(() => OnPromoteButtonPressed(true, 1));
        player2NoPromoteButton.onClick.AddListener(() => OnPromoteButtonPressed(false, 1));

        // �p�l�����\���ɂ���
        player1PromotePanel.SetActive(false);
        player2PromotePanel.SetActive(false);
    }
    void InitializeCameras()
    {
        player1Camera = GameObject.Find("Player1Setting").GetComponent<Camera>();
        player2Camera = GameObject.Find("Player2Setting").GetComponent<Camera>();
        player1Camera.rect = new Rect(0, 0, 0.5f, 1);
        player2Camera.rect = new Rect(0.5f, 0, 0.5f, 1);
    }
    //--------�����ݒ菈���I���----------

    //--------���菈���J�n-----------
    public void ShowPromoteOptions(UnitController unit, int currentPlayer)// ����I�v�V������\������
    {
        gameSystem.ClearCursors();
        Debug.Log("a");

        selectedUnit = unit;  // ����Ώۂ̋���L�^
        if (currentPlayer == 0) // �v���C���[1�̏ꍇ
        {
            player1PromotePanel.SetActive(true);// �p�l����\��
        }
        else // �v���C���[2�̏ꍇ
        {
            player2PromotePanel.SetActive(true);// �p�l����\��
        }
        UnityEngine.Time.timeScale = 0; //�Q�[���̎��Ԃ��~
        gameSystem.DisableInput();// ���͂𖳌��ɂ���
        gameSystem.ClearCursors();
        Debug.Log("b");
    }

    // ����/����Ȃ��{�^���������ꂽ���̏���
    // �{�^���������ꂽ�Ƃ��ɓ��͂�L���ɖ߂�
    private void OnPromoteButtonPressed(bool promote, int currentPlayer)
    {
        if (selectedUnit != null)
        {
            if (promote)
            {
                selectedUnit.Promote();  // ��������s
            }
            HidePromoteOptions(currentPlayer);

            //// ���͂�L���ɖ߂�
            //GameSystem gameSystem = UnityEngine.Object.FindAnyObjectByType<GameSystem>();
            if (gameSystem != null)
            {
                UnityEngine.Time.timeScale = 1; // �Q�[���̎��Ԃ��ĊJ
                //gameTime.ResetBattleTime();         // ���Ԃ����Z�b�g
                gameSystem.EnableInput();  // ���͂��ēx�L���ɂ���
                gameSystem.StartOuteNextTurn();
                if (!gameSystem.skipActivateText)
                {
                    gameSystem.ActivateText();
                }
                gameSystem.skipActivateText = false; // �t���O�����Z�b�g���Ď��^�[���ōĂѓ���\��
                gameSystem.DeselectBoardPieces();
                //gameSystem.EndTurn();  // �^�[����؂�ւ���
            }
        }
    }
    public void HidePromoteOptions(int currentPlayer)
    {
        if (currentPlayer == 0)
        {
            player1PromotePanel.SetActive(false);
        }
        else
        {
            player2PromotePanel.SetActive(false);
        }
    }    //--------���菈���I���----------
}