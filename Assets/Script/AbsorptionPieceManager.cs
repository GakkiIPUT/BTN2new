using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AbsorptionPieceManager : MonoBehaviour
{
    [SerializeField] private CapturedPieceManager capturedPieceManager; // �ʏ���̎Q�Ƃ�ǉ�

    [SerializeField] private Transform player1AbsorArea; // Player1�̋z�����̈ʒu 
    [SerializeField] private Transform player2AbsorArea; // Player2�̋z�����̈ʒu 

    [SerializeField] private GameObject[] kanjiPrefabs; // ��̊����v���t�@�u�i7�j
    [SerializeField] private GameObject[] numberPrefabs; // �����̃v���t�@�u�i10�j

    // ��I��p�̃n�C���C�g�v���t�@�u
    [SerializeField] private GameObject selectionHighlightPrefab;
    private GameObject currentHighlight; // ���݂̑I���n�C���C�g

    // �e�v���C���[���ƂɁA��̎�ނƂ��̐����Ǘ����鎫��
    private Dictionary<int, Dictionary<UnitType, int>> absorptionPices = new Dictionary<int, Dictionary<UnitType, int>>();

    // ���ݑI�𒆂̋�̎��
    public UnitType selectedPieceType = UnitType.None;

    // GameSystem ���Q��
    private GameSystem gameSystem;

    // ���\�����̊����̐F
    public Material absorPiece;

    // �{�^������Ăяo�����߂̑I���������\�b�h

    //--------�����������J�n-----------
    void Start()
    {
        // GameSystem ���Q��
        gameSystem = FindAnyObjectByType<GameSystem>();
        capturedPieceManager = FindAnyObjectByType<CapturedPieceManager>();

        // �v���C���[���Ƃ̋���������
        for (int i = 0; i < 2; i++)
        {
            absorptionPices[i] = new Dictionary<UnitType, int>();
            foreach (UnitType type in System.Enum.GetValues(typeof(UnitType)))
            {
                if (type != UnitType.None)
                    absorptionPices[i][type] = 0;
            }
        }
    }
    //--------�����������I���----------

    //--------������ǉ��E�\���X�V�����J�n-----------
    // ������ɒǉ����郁�\�b�h
    public void AddAbsorptionPiece(UnitType type, int player2)
    {
        // �J�E���g��1�������₷
        if (absorptionPices[player2][type] >= 0)
        {
            absorptionPices[player2][type]++;
            //Debug.Log($"{type} ���ߊl����܂����B���݂̐�: {capturedPieces[player][type]}");
        }
        // ���ɕ\�����邽�߂̏���
        UpdateAbsorPieceDisplay(player2);
    }

    private void UpdateAbsorPieceDisplay(int player)
    {
        // ���̈ʒu���擾
        Transform absorArea = player == 0 ? player1AbsorArea : player2AbsorArea;

        // �����̋�\�����폜
        foreach (Transform child2 in absorArea)
        {
            Destroy(child2.gameObject);
        }

        // ��̎�ނƂ��̐������X�g�����A�w�肳�ꂽ���ŕ\��
        List<(UnitType, int)> pieceList2 = GetSortedPieceList(player);

        // ��̕\���ʒu
        Vector3 currentPos2 = Vector3.zero;
        int pieceCount2 = 0;

        foreach (var (type, count) in pieceList2)
        {
            if (count > 0)
            {
                // ��̊����v���t�@�u�𐶐�
                GameObject kanjiPrefab = GetKanjiPrefab(type);
                if (kanjiPrefab == null) continue;

                GameObject newPiece2 = Instantiate(kanjiPrefab, absorArea);
                newPiece2.GetComponent<MeshRenderer>().material = absorPiece;
                newPiece2.name = type.ToString();
                newPiece2.transform.localPosition = currentPos2;

                // ���Collider���Ȃ���Βǉ�����
                if (newPiece2.GetComponent<Collider>() == null)
                {
                    newPiece2.AddComponent<BoxCollider>();
                }
                // ��N���b�N���ꂽ�Ƃ��̃C�x���g��ǉ�
                newPiece2.AddComponent<CapturedPieceClickHandler>().Init(this, type, player);

                // ���1���傫���ꍇ�͊������̃v���t�@�u��ǉ�
                if (count > 1)
                {
                    GameObject numberPrefab2 = Instantiate(GetNumberPrefab(count), newPiece2.transform);
                    numberPrefab2.name = "Count";
                    numberPrefab2.transform.localPosition = new Vector3(-0.002f, -0.004f, 0.002f); // ��̉E���ɔz�u
                }

                // ���̋�̕\���ʒu���X�V
                currentPos2.x += -0.62f;
                pieceCount2++;

                // ���s�����i4���Ƃɉ��s�j
                if (pieceCount2 % 4 == 0)
                {
                    currentPos2.x = 0;
                    currentPos2.z += 0.8f;
                }
            }
        }
    }

    public void ReduceAbsorPieceCount(UnitType type, int player)
    {
        if (absorptionPices[player][type] > 0)
        {
            absorptionPices[player][type]--;
            UpdateAbsorPieceDisplay(player);
        }
    }

    // �w�肳�ꂽ���ŋ����בւ��郁�\�b�h
    private List<(UnitType, int)> GetSortedPieceList(int player)
    {
        return new List<(UnitType, int)>
        {
            (UnitType.huhyou, absorptionPices[player][UnitType.huhyou]),
            (UnitType.kyousya, absorptionPices[player][UnitType.kyousya]),
            (UnitType.keima, absorptionPices[player][UnitType.keima]),
            (UnitType.ginsyou, absorptionPices[player][UnitType.ginsyou]),
            (UnitType.kinsyou, absorptionPices[player][UnitType.kinsyou]),
            (UnitType.hisya, absorptionPices[player][UnitType.hisya]),
            (UnitType.kakugyou, absorptionPices[player][UnitType.kakugyou])
        };
    }
    //--------������ǉ��E�\���X�V�����I���----------


    // ��̎�ނɉ����Ċ����v���t�@�u���擾���郁�\�b�h
    public GameObject GetKanjiPrefab(UnitType type)
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


    //--------��N���b�N�����J�n-----------
    // ��N���b�N���ꂽ���̏���
    public void OnAbsorptionPieceClicked(UnitType type, int player)
    {
        capturedPieceManager.DeselectPiece();
        AbsorDeselectPiece();
        // ���݂̃v���C���[�̃^�[�����ǂ����m�F
        if (gameSystem.GetCurrentPlayer() != player)
        {
            return;
        }

        // ��̑I��
        selectedPieceType = type;

        // ��̈ʒu�Ƀn�C���C�g��\��
        GameObject clickedPiece = GameObject.Find(type.ToString()); // ��̖��O�ŃI�u�W�F�N�g��T��
        if (clickedPiece != null)
        {
            ShowHighlight(clickedPiece.transform.position); // ��̈ʒu�Ƀn�C���C�g��\��
        }

        // ���u����ꏊ�ɃJ�[�\����\��
        gameSystem.DisplayPlaceableCursorsAbsor();
    }

    // �n�C���C�g����̈ʒu�ɕ\�����郁�\�b�h
    public void ShowHighlight(Vector3 position)
    {
        ClearHighlight(); // �����̃n�C���C�g���N���A
        CapturedPieceManager capturedPieceManager = GetComponent<CapturedPieceManager>();
        capturedPieceManager.ClearHighlight();

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
    public void AbsorDeselectPiece()
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
    // �����v���t�@�u���擾���郁�\�b�h
    private GameObject GetNumberPrefab(int count)
    {
        if (count < 1 || count > 10) return null;
        return numberPrefabs[count - 1];
    }
    //--------�⏕���\�b�h�I���----------

    //--------CapturedPieceClickHandler�N���X�J�n-----------
    public class CapturedPieceClickHandler : MonoBehaviour
    {
        private AbsorptionPieceManager absorptionPieceManager;
        private UnitType pieceType;
        private int player;

        // ���������\�b�h
        public void Init( AbsorptionPieceManager manager, UnitType type, int playerIndex)
        {
            absorptionPieceManager = manager;
            pieceType = type;
            player = playerIndex;
        }

        // ��N���b�N���ꂽ�Ƃ��ɌĂ΂�郁�\�b�h
        public void OnMouseUpAsButton()
        {
            if (absorptionPieceManager != null)
            {
                // ��N���b�N���ꂽ�Ƃ��̏���
                absorptionPieceManager.OnAbsorptionPieceClicked(pieceType, player);

                // �n�C���C�g��\���i�������ꂽ��̈ʒu�Ɂj
                Vector3 highlightPosition = transform.position; // ��̈ʒu���擾
                absorptionPieceManager.ShowHighlight(highlightPosition);
            }
        }

    }
    // ���ݑI������Ă����̎�ނ��擾���郁�\�b�h
    public UnitType GetSelectedPieceType()
    {
        return selectedPieceType;
    }
    //--------CapturedPieceClickHandler�N���X�I���----------

}
