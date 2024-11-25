using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

/*
   ������ǉ��E�\���X�V
   ��N���b�N
   ������I������
   �⏕���\�b�h
   ������N���b�N
*/

public class CapturedPieceManager : MonoBehaviour
{
    [SerializeField] private AbsorptionPieceManager absorptionPieceManager; // �z�����̎Q�Ƃ�ǉ�

    [SerializeField] private Transform player1CapturedArea; // Player1�̋��̈ʒu
    [SerializeField] private Transform player2CapturedArea; // Player2�̋��̈ʒu

    [SerializeField] private GameObject[] kanjiPrefabs; // ��̊����v���t�@�u�i7�j
    [SerializeField] private GameObject[] numberPrefabs; // �����̃v���t�@�u�i10�j

    [SerializeField] private GameObject selectionHighlightPrefab;// ��I��p�̃n�C���C�g�v���t�@�u
    // �e�v���C���[���ƂɁA��̎�ނƂ��̐����Ǘ����鎫��
    private Dictionary<int, Dictionary<UnitType, int>> capturedPieces = new Dictionary<int, Dictionary<UnitType, int>>();

    private GameSystem gameSystem;// GameSystem ���Q��

    private GameObject currentHighlight; // ���݂̑I���n�C���C�g
    public UnitType selectedPieceType = UnitType.None;// ���ݑI�𒆂̋�̎��

    //--------�����������J�n-----------
    void Start()
    {
        // GameSystem ���Q��
        gameSystem = FindAnyObjectByType<GameSystem>();
        absorptionPieceManager = FindAnyObjectByType<AbsorptionPieceManager>();
        if (gameSystem == null)
        {
            UnityEngine.Debug.LogError("GameSystem ��������܂���ł����B");
        }
        // �v���C���[���Ƃ̋���������
        for (int i = 0; i < 2; i++)
        {
            capturedPieces[i] = new Dictionary<UnitType, int>();
            foreach (UnitType type in System.Enum.GetValues(typeof(UnitType)))
            {
                if (type != UnitType.None)
                    capturedPieces[i][type] = 0;
            }
        }
    }
    //--------�����������I���----------

    //--------������ǉ��E�\���X�V�����J�n-----------
    // ������ɒǉ����郁�\�b�h
    public void AddCapturedPiece(UnitType type, int player)
    {
        // �J�E���g��1�������₷
        if (capturedPieces[player][type] >= 0)
        {
            capturedPieces[player][type]++;
            //Debug.Log($"{type} ���ߊl����܂����B���݂̐�: {capturedPieces[player][type]}");
        }
        // ���ɕ\�����邽�߂̏���
        UpdateCapturedPieceDisplay(player);
    }

    // ���̕\�����X�V���郁�\�b�h
    private void UpdateCapturedPieceDisplay(int player)
    {
        // ���̈ʒu���擾
        Transform capturedArea = player == 0 ? player1CapturedArea : player2CapturedArea;

        // �����̋�\�����폜
        foreach (Transform child in capturedArea)
        {
            Destroy(child.gameObject);
        }

        // ��̎�ނƂ��̐������X�g�����A�w�肳�ꂽ���ŕ\��
        List<(UnitType, int)> pieceList = GetSortedPieceList(player);

        // ��̕\���ʒu
        Vector3 currentPos = Vector3.zero;
        int pieceCount = 0; // ��̐����J�E���g

        foreach (var (type, count) in pieceList)
        {
            if (count > 0)
            {
                // ��̊����v���t�@�u�𐶐�
                GameObject kanjiPrefab = GetKanjiPrefab(type);
                if (kanjiPrefab == null) continue;

                GameObject newPiece = Instantiate(kanjiPrefab, capturedArea);
                newPiece.name = type.ToString();
                newPiece.transform.localPosition = currentPos;

                // ���Collider���Ȃ���Βǉ�����
                if (newPiece.GetComponent<Collider>() == null)
                {
                    newPiece.AddComponent<BoxCollider>();
                }
                // ��N���b�N���ꂽ�Ƃ��̃C�x���g��ǉ�
                newPiece.AddComponent<CapturedPieceClickHandler>().Init(this, type, player);

                // ���1���傫���ꍇ�͊������̃v���t�@�u��ǉ�
                if (count > 1)
                {
                    GameObject numberPrefab = Instantiate(GetNumberPrefab(count), newPiece.transform);
                    numberPrefab.name = "Count";
                    numberPrefab.transform.localPosition = new Vector3(-0.002f, -0.005f, 0.002f); // ��̉E���ɔz�u
                }

                // ���̋�̕\���ʒu���X�V
                currentPos.x += -0.62f; // �������ɋl�߂Ĕz�u
                pieceCount++;

                // ���s�����i4���Ƃɉ��s�j
                if (pieceCount % 4 == 0)
                {
                    currentPos.x = 0;
                    currentPos.z += 0.8f; // �c�����ɏ���������
                }
            }
        }
    }

    public void ReduceCapturedPieceCount(UnitType type, int player)
    {
        if (capturedPieces[player][type] > 0)
        {
            capturedPieces[player][type]--;
            UpdateCapturedPieceDisplay(player);
        }
    }

    // �w�肳�ꂽ���ŋ����בւ��郁�\�b�h
    private List<(UnitType, int)> GetSortedPieceList(int player)
    {
        return new List<(UnitType, int)>
        {
            (UnitType.huhyou, capturedPieces[player][UnitType.huhyou]),
            (UnitType.kyousya, capturedPieces[player][UnitType.kyousya]),
            (UnitType.keima, capturedPieces[player][UnitType.keima]),
            (UnitType.ginsyou, capturedPieces[player][UnitType.ginsyou]),
            (UnitType.kinsyou, capturedPieces[player][UnitType.kinsyou]),
            (UnitType.hisya, capturedPieces[player][UnitType.hisya]),
            (UnitType.kakugyou, capturedPieces[player][UnitType.kakugyou])
        };
    }
    //--------������ǉ��E�\���X�V�����I���----------

    //--------��N���b�N�����J�n-----------
    // ��N���b�N���ꂽ���̏���
    public void OnCapturedPieceClicked(UnitType type, int player)
    {
        absorptionPieceManager.AbsorDeselectPiece();
        DeselectPiece();
        // ���݂̃v���C���[�̃^�[�����ǂ����m�F
        if (gameSystem.GetCurrentPlayer() != player)
        {
            return;
        }
        gameSystem.DeselectBoardPieces();

        // ��̑I��
        selectedPieceType = type;

        // ��̈ʒu�Ƀn�C���C�g��\��
        GameObject clickedPiece = GameObject.Find(type.ToString()); // ��̖��O�ŃI�u�W�F�N�g��T��
        if (clickedPiece != null)
        {
            ShowHighlight(clickedPiece.transform.position); // ��̈ʒu�Ƀn�C���C�g��\��
        }

        // ���u����ꏊ�ɃJ�[�\����\��
        gameSystem.DisplayPlaceableCursors();
    }
    // �n�C���C�g����̈ʒu�ɕ\�����郁�\�b�h
    public void ShowHighlight(Vector3 position)
    {
        ClearHighlight(); // �����̃n�C���C�g���N���A
        AbsorptionPieceManager absorptionPieceManager = GetComponent<AbsorptionPieceManager>();
        absorptionPieceManager.ClearHighlight();

        // ���W�𒲐�����i�w��̃I�t�Z�b�g��������j
        Vector3 adjustedPosition = new Vector3(
            position.x - 0.22f,  // x �� -0.22 ����
            position.y - 2.7f,   // y �� -2.7 ����
            position.z - 0.86f   // z �� +0.86 ����
        );

        // �n�C���C�g�v���t�@�u�𐶐�
        currentHighlight = Instantiate(selectionHighlightPrefab, adjustedPosition, Quaternion.identity);
        currentHighlight.transform.localPosition = adjustedPosition;
    }
    // �I����Ԃ�����
    public void ClearHighlight()
    {
        if (currentHighlight != null)
        {
            Destroy(currentHighlight);
        }
    }
    //--------��N���b�N�����I���----------

    //--------������I�����������J�n-----------
    public void DeselectPiece()
    {
        // ������̑I��������
        ClearSelectedPiece();
        //Debug.Log("��̑I�����������܂����B");

        // GameSystem �� ClearCursors ���\�b�h���Ăяo��
        if (gameSystem != null)
        {
            gameSystem.ClearCursors();  // �J�[�\�����N���A
            if (gameSystem.GetSelectUnit() != null)// �����Տ�̋�̑I��������
            {
                gameSystem.GetSelectUnit().Selected(false);  // �I��������
                gameSystem.ClearSelectUnit();  // GameSystem �̑I����Ԃ�����
            }
        }
    }
    // �I�𒆂̋���������郁�\�b�h
    public void ClearSelectedPiece()
    {
        selectedPieceType = UnitType.None;

        //Debug.Log("�I�𒆂̋��������܂����B");
        ClearHighlight();
    }

    //--------��I�����������I���----------

    //--------�⏕���\�b�h�J�n-----------
    public GameObject GetKanjiPrefab(UnitType type)// ��̎�ނɉ����Ċ����v���t�@�u���擾���郁�\�b�h
    {
        switch (type)
        {
            case UnitType.huhyou: return kanjiPrefabs[0]; // ��
            case UnitType.kyousya: return kanjiPrefabs[1]; // ��
            case UnitType.keima: return kanjiPrefabs[2]; // �j
            case UnitType.ginsyou: return kanjiPrefabs[3]; // ��
            case UnitType.kinsyou: return kanjiPrefabs[4]; // ��
            case UnitType.hisya: return kanjiPrefabs[5]; // ��
            case UnitType.kakugyou: return kanjiPrefabs[6]; // �p
            default: return null;
        }
    }
    // �����v���t�@�u���擾���郁�\�b�h
    private GameObject GetNumberPrefab(int count)
    {
        if (count < 1 || count > 10) return null;
        return numberPrefabs[count - 1];
    }
    //--------�⏕���\�b�h�I���----------

    //--------������N���b�N�����J�n-----------
    public class CapturedPieceClickHandler : MonoBehaviour
    {
        private CapturedPieceManager capturedPieceManager;
        private UnitType pieceType;
        private int player;

        // ���������\�b�h
        public void Init(CapturedPieceManager manager, UnitType type, int playerIndex)
        {
            capturedPieceManager = manager;
            pieceType = type;
            player = playerIndex;
        }

        // ��N���b�N���ꂽ�Ƃ��ɌĂ΂�郁�\�b�h
        public void OnMouseUpAsButton()
        {
            if (capturedPieceManager != null)
            {
                // ��N���b�N���ꂽ�Ƃ��̏���
                capturedPieceManager.OnCapturedPieceClicked(pieceType, player);

                // �n�C���C�g��\���i�������ꂽ��̈ʒu�Ɂj
                Vector3 highlightPosition = transform.position; // ��̈ʒu���擾
                capturedPieceManager.ShowHighlight(highlightPosition);
            }
        }

    }
    // ���ݑI������Ă����̎�ނ��擾���郁�\�b�h
    public UnitType GetSelectedPieceType()
    {
        return selectedPieceType;
    }
    //--------������N���b�N�����I���----------
}
