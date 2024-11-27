using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine.UI;
public class GameSystem : MonoBehaviour
{
    //-------�Q�[���ݒ�J�n--------
    //�V�[���J��
    public string sceneName;
    //�V�[���J�ڏI��
    private float TotalElapsedTime; // �����̌o�ߎ���
    private bool isGameActive = false; // �Q�[�����i�s�����ǂ����̃t���O

    const int PlayerMax = 2; // �v���C���[��
    int currentPlayer;// ���݂̃v���C���[ (0�������A1���G)
    [SerializeField, Tooltip("�ŏ��̎�Ԃ������_���ɂ��邩�ǂ���")]
    bool randomizeStartingPlayer= false;

    [SerializeField] private bool turnChange = true; // �^�[�����̃t���O
    public int[] playerMoveCounts = new int[PlayerMax];// 2�l�̃v���C���[���̎�̃J�E���g
    public int TotalTime = 0;

    int kingMoveCount = 0;// ���������Ȃ�������
    public bool isKingInCheck = false;// ���肩�ǂ����̃t���O
    [SerializeField] Vector2Int Player0KingPos; // ���肪���������Ƃ��̉����̈ʒu
    [SerializeField] Vector2Int Player1KingPos;�@// ���肪���������Ƃ��̉����̈ʒu
    int maxMoveCount = 1;// ���������Ȃ������ꍇ�̍ő�J�E���g
    [SerializeField, Header("����Image")]
    public GameObject[] OuteEffect; // ����G�t�F�N�g�p��UI�I�u�W�F�N�g�̔z��
    private bool isCheckTriggeredNextTurn = false; // �����ԃt���O��ǉ�
    private bool startOuteNextTurnDisabled = false; // �������t���O

    int boardWidth;
    int boardHeight;

    public Material BaseMaterial;  // ���̘g�̐F
    public Material HoheiMaterial;  // �����̕����}�e���A��

    private bool isInputDisabled = false;// ���͖����t���O

    private AudioClip SE;

    public void DisableInput()
    { isInputDisabled = true; }// ���͂𖳌��ɂ���

    public void EnableInput()
    { isInputDisabled = false; }// ���͂�L���ɖ߂�

    [SerializeField] private GameTime gametime;  // �Q�[���̃v���C����
    [SerializeField] GameObject PrefabTile; // �^�C���̃v���n�u��
    [SerializeField] List<GameObject> PrefabUnits; // ��̃v���n�u��
    private Player2Handicap handicap2; //���
    private Player1Handicap handicap1;

    int[,] boardsetting =  // ��̏����z�u
    {
        { 14, 0, 11, 0, 0, 0, 1, 0, 4 },
        { 15, 12, 11, 0, 0, 0, 1, 3, 5 },
        { 16, 0, 11, 0, 0, 0, 1, 0, 6 },
        { 17, 0, 11, 0, 0, 0, 1, 0, 7 },
        { 18, 0, 11, 0, 0, 0, 1, 0, 9 },
        { 17, 0, 11, 0, 0, 0, 1, 0, 7 },
        { 16, 0, 11, 0, 0, 0, 1, 0, 6 },
        { 15, 13, 11, 0, 0, 0, 1, 2, 5 },
        { 14, 0, 11, 0, 0, 0, 1, 0, 4 },

        // �f�o�b�O��Ɨp
        //{  0, 0, 0, 0, 0, 2, 1, 0, 0 },
        //{  0, 0, 0, 0, 0, 7, 1, 0, 0 },
        //{  0, 0, 0, 0, 0, 7, 1, 0, 0 },
        //{  0, 0, 0, 0, 0, 5, 1, 0, 0 },
        //{ 13, 0, 0, 0, 0, 5, 1, 0, 3 },
        //{  0, 0, 0, 0, 0, 4, 1, 0, 0 },
        //{  0, 0, 0, 0, 0, 4, 1, 0, 0 },
        //{  0, 0, 0, 0, 0, 6, 1, 0, 0 },
        //{  0, 0, 0, 0, 0, 6, 1, 0, 0 },
    };

    Dictionary<Vector2Int, GameObject> tiles;
    UnitController[,] units;

    //���
    private void ApplyHandicap()
    {
        int handicapIndex2 = PlayerPrefs.GetInt("HandicapSetting2", 0); // �f�t�H���g�́u�Ȃ��v
        int handicapIndex1 = PlayerPrefs.GetInt("HandicapSetting1", 0); // �f�t�H���g�́u�Ȃ��v
        string handicapPositions2 = PlayerPrefs.GetString("HandicapPositions2", ""); // �f�t�H���g�͋󕶎���
        string handicapPositions1 = PlayerPrefs.GetString("HandicapPositions1", ""); // �f�t�H���g�͋󕶎���

        // ����ݒ肪����ꍇ�A�ʒu�����擾���Ĕ��f
        if (!string.IsNullOrEmpty(handicapPositions2) || !string.IsNullOrEmpty(handicapPositions1))
        {
            var positions2 = ParsePositions(handicapPositions2);
            var positions1 = ParsePositions(handicapPositions1);
            foreach (var (row, col) in positions2)
            {
                boardsetting[row, col] = 0; // �f�[�^�Ƃ��Ă̋���폜

                // ���GameObject���폜
                if (units[row, col] != null)
                {
                    Destroy(units[row, col].gameObject);
                    units[row, col] = null;
                }
            }
            foreach (var (row, col) in positions1)
            {
                boardsetting[row, col] = 0; // �f�[�^�Ƃ��Ă̋���폜

                // ���GameObject���폜
                if (units[row, col] != null)
                {
                    Destroy(units[row, col].gameObject);
                    units[row, col] = null;
                }
            }
            Debug.Log("����ݒ肪���f����܂���2" + handicapIndex2 + ", �ݒ���e " + handicapPositions2);
            Debug.Log("����ݒ肪���f����܂���1" + handicapIndex1 + ", �ݒ���e " + handicapPositions1);
        }
    }

    private List<(int row, int col)> ParsePositions(string positions)
    {
        var result = new List<(int row, int col)>();
        foreach (var pos in positions.Split(';'))
        {
            var values = pos.Split(',');
            if (values.Length == 2 && int.TryParse(values[0], out int row) && int.TryParse(values[1], out int col))
            {
                result.Add((row, col));
            }
        }
        return result;
    }

    public int GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public GameObject GetTile(Vector2Int position)
    {
        if (tiles.TryGetValue(position, out GameObject tile))
        {
            return tile;
        }
        return null;
    }

    [SerializeField] List<GameObject> TurnText;
    [SerializeField] List<GameObject> TurnText_always;
    //-------�Q�[���ݒ�I���--------


    //-------��̊Ǘ��J�n--------
    private Dictionary<UnitType, int> capturedUnits;// �������̎�ނƐ����Ǘ����邽�߂̎���

    [SerializeField] private PromoteManager promoteManager; // ����̊Ǘ�
    [SerializeField] private CapturedPieceManager capturedPieceManager; // ��鏈���̊Ǘ�
    [SerializeField] private AbsorptionPieceManager absorptionPieceManager;

    public UnitController selectUnit; // �I�𒆂̋�
    public UnitController GetSelectUnit()//GetSelectUnit���\�b�h��ǉ�
    {
        return selectUnit;  // ���ݑI������Ă��郆�j�b�g��Ԃ�
    }
    public void ClearSelectUnit()//ClearSelectUnit���\�b�h��ǉ�
    {
        selectUnit = null;  // �I��������
    }
    public List<UnitController> absorptionHistory = new List<UnitController>();
    private UnitController currentReleaseUnit;
    private UnitController selectedReleaseUnit; // �I�𒆂̕��o��

    Dictionary<GameObject, Vector2Int> movableTiles;// �ړ��\�^�C��
    Dictionary<GameObject, Vector2Int> absorbableTiles;// �z���\�^�C��
    Dictionary<GameObject, Vector2Int> relesableTiles;// ���o�\�^�C��

    [SerializeField] GameObject prefabCursor;// �ړ��͈͂̃v���n�u
    [SerializeField] GameObject absorption;// �z���͈͂̃v���n�u
    [SerializeField] GameObject releseCursor;// ���o�\�̃v���n�u

    List<GameObject> cursors;// �ړ��͈͂̃I�u�W�F�N�g
    List<GameObject> absorptioncursors;// �z���͈̓I�u�W�F�N�g
    List<GameObject> relesecursors;// ���o�͈̓I�u�W�F�N�g

    [SerializeField] GameObject absorEffect; // �G�t�F�N�g�̃v���n�u
    [SerializeField] GameObject fillingEffect;
    [SerializeField] GameObject filledEffect;
    private Color newColor;
    private Material newMaterial;
    [SerializeField] List<Material> newMaterials;
    [SerializeField] GameObject releaseEffect;
    private GameObject currentAbsorEffect; // �C���X�^���X�������G�t�F�N�g�̎Q��
    public bool skipActivateText = false;

    public UnitController absorUnit;
    public int Turn;// �o�߃^�[��
    public int absorTurn;// �z�������^�[��
    //-------��̊Ǘ��I���--------


    //-------�J�����ݒ�J�n--------
    public Camera player1Camera;
    public Camera player2Camera;
    public int totalWidth = 1920;  // �S��ʕ�
    public int totalHeight = 540;  // ����
    public int playerWidth = 960;  // �e�v���C���[�̕�
    public int playerHeight = 540;  // ����
    //-------�J�����ݒ�I���--------


    //-------�Q�[���̏����������J�n--------
    void Start()
    {
        sceneName = "Result";
        PlayerPrefs.DeleteKey("Player1MoveCount");
        PlayerPrefs.DeleteKey("Player2MoveCount");
        PlayerPrefs.DeleteKey("Winner");
        PlayerPrefs.DeleteKey("TotalTime");
        InitializeCameras();
        InitializeBoard();
        InitializeUnits();
        InitializeKingPositions();
        TotalElapsedTime = 0f;
        isGameActive = true; // �Q�[���i�s���t���O�𗧂Ă�
        ApplyHandicap(); //���
        // �Q�[���J�n���� OuteEffect �̊e�I�u�W�F�N�g���A�N�e�B�u�ɂ���
        foreach (GameObject highlight in OuteEffect)
        {
            highlight.SetActive(false);
        }

        foreach (GameObject text in TurnText)
        {
            text.SetActive(false);
        }

        // �Q�[���J�n���̕\��
        if (currentPlayer == 0)
        {
            TurnText_always[0].GetComponentInChildren<TextMeshProUGUI>().text = "���Ȃ��̃^�[��"; // Player1
            TurnText_always[1].GetComponentInChildren<TextMeshProUGUI>().text = "����̃^�[��"; // Player2
        }
        if (currentPlayer == 1)
        {
            TurnText_always[0].GetComponentInChildren<TextMeshProUGUI>().text = "����̃^�[��"; //Player1
            TurnText_always[1].GetComponentInChildren<TextMeshProUGUI>().text = "���Ȃ��̃^�[��"; //Player2
        }
        ActivateText();
        Invoke("DeactivateText", 1.5f);
    }

    void InitializeCameras() // �J����
    {
        player1Camera = GameObject.Find("Player1").GetComponent<Camera>();
        player2Camera = GameObject.Find("Player2").GetComponent<Camera>();
        player1Camera.rect = new Rect(0, 0, 0.5f, 1);
        player2Camera.rect = new Rect(0.5f, 0, 0.5f, 1);
    }

    void InitializeBoard() // �{�[�h
    {
        if (randomizeStartingPlayer) { currentPlayer = UnityEngine.Random.Range(0, 2);}//�����_���^�[��
        else { currentPlayer = 0; }
        boardWidth = boardsetting.GetLength(0);// �{�[�h�̃T�C�Y
        boardHeight = boardsetting.GetLength(1);
        tiles = new Dictionary<Vector2Int, GameObject>();// �t�B�[���h������
        units = new UnitController[boardWidth, boardHeight];
        movableTiles = new Dictionary<GameObject, Vector2Int>();// �ړ��\����
        cursors = new List<GameObject>();
        absorbableTiles = new Dictionary<GameObject, Vector2Int>(); // �z���\����
        absorptioncursors = new List<GameObject>();
        relesableTiles = new Dictionary<GameObject, Vector2Int>();// ���o�\����
        relesecursors = new List<GameObject>();
    }

    void InitializeUnits() //��
    {
        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardHeight; j++)
            {
                CreateTileAndUnit(i, j);
            }
        }
        capturedUnits = new Dictionary<UnitType, int>();
        // ���ׂĂ̎�ނ̋��0�ŏ�����
        foreach (UnitType type in Enum.GetValues(typeof(UnitType)))
        {
            if (type != UnitType.None)
                capturedUnits[type] = 0;
        }
    }

    void CreateTileAndUnit(int i, int j) // ��ƔՂ̐���
    {
        float tileSpacingX = 0.5f, tileSpacingZ = 0.6f;// �^�C���ƃ��j�b�g�̃|�W�V����
        float x = (i - boardWidth / 2) * tileSpacingX;
        float y = (j - boardHeight / 2) * tileSpacingZ;
        Vector3 pos = new Vector3(x, 0, y);
        GameObject tile = Instantiate(PrefabTile, pos, Quaternion.identity); // �^�C���쐬
        Vector2Int idx = new Vector2Int(i, j);// �^�C���̃C���f�b�N�X
        tiles.Add(idx, tile);
        movableTiles.Add(tile, idx);// �ړ��\���������Ō��߂�

        int type = boardsetting[i, j] % 10; // ��쐬
        int player = boardsetting[i, j] / 10;
        if (type == 0) return;

        pos.y = 0.7f;
        GameObject prefab = PrefabUnits[type - 1];
        GameObject unit = Instantiate(prefab, pos, Quaternion.Euler(-90, player * 180, 0));
        UnitController unitctrl = unit.GetComponent<UnitController>();
        unitctrl.Init(player, type, tile, idx);
        units[i, j] = unitctrl;
    }
    //-------�Q�[���̏����������I���--------


    //-------�^�[���������J�n--------
    void Update()
    {
        if (isInputDisabled) return;
        HandlePlayerInput();
        if (Input.GetMouseButtonUp(1)){ DeselectBoardPieces();capturedPieceManager.DeselectPiece(); absorptionPieceManager.AbsorDeselectPiece(); }
        if (isGameActive){ TotalElapsedTime += Time.deltaTime;}
    }

    void HandlePlayerInput()�@// �v���C���[����
    {
        if (Input.GetMouseButtonUp(0))
        {
            Vector3 mousePos = Input.mousePosition;
            Ray ray = (currentPlayer == 0 ? player1Camera : player2Camera).ScreenPointToRay(mousePos);
            HandleRaycast(ray);
        }
    }

    void HandleRaycast(Ray ray) // ���C�̐���
    {
        GameObject tile = null;
        UnitController unit = null;

        foreach (RaycastHit hit in Physics.RaycastAll(ray))
        {
            UnitController hitunit = hit.transform.GetComponent<UnitController>();

            if (hitunit != null && hitunit.Player != currentPlayer)
            {
                //Debug.Log("����̃^�[���̂��߁A���I���ł��܂���");
                return;
            }
            if (hitunit != null && FieldStatus.Captured == hitunit.FieldStatus)
            {
                unit = hitunit;
            }
            else if (tiles.ContainsValue(hit.transform.gameObject))
            {
                tile = hit.transform.gameObject;
                // �^�C�����瑼���j�b�g��T��
                foreach (var item in tiles)
                {
                    if (item.Value == tile)
                    {
                        unit = units[item.Key.x, item.Key.y];
                    }
                }
                break;
            }
        }
        HandleTileSelection(tile, unit);
    }

    void HandleTileSelection(GameObject tile, UnitController unit) // �^�C���I���̐���
    {
        if (tile == null && unit == null) 
        {
            DeselectBoardPieces();
            return;
        }
        if (tile == null) return;
        if (movableTiles.TryGetValue(tile, out Vector2Int tileIndex))
        {
            if (tileIndex.y == -1)
            {
                capturedPieceManager.OnCapturedPieceClicked(capturedPieceManager.GetSelectedPieceType(), currentPlayer);
                absorptionPieceManager.OnAbsorptionPieceClicked(absorptionPieceManager.GetSelectedPieceType(), currentPlayer);
            }
            else if (capturedPieceManager.GetSelectedPieceType() != UnitType.None)
            {
                if (units[tileIndex.x, tileIndex.y] == null)
                {
                    PlacePieceFromCaptured(tileIndex);
                }
                else
                {
                    //Debug.Log("���̏ꏊ�ɂ͂��łɋ����܂��B");
                }
            }
            else if (absorptionPieceManager.GetSelectedPieceType() != UnitType.None)
            {
                if (units[tileIndex.x, tileIndex.y] == null)
                {
                    PlacePieceFromAbsorption(tileIndex);
                }
                else
                {
                    //Debug.Log("���̏ꏊ�ɂ͂��łɋ����܂��B");
                }
            }
        }
        else if (tile && absorptionHistory.Contains(unit))
        {
            // �I������������̋�ǂ����𔻕�
            if (unit.Player == currentPlayer)
            {
                // �����̋�̏ꍇ�ɂ̂ݐݒ�
                selectedReleaseUnit = unit;
                Debug.Log("���o������I�����܂���: " + selectedReleaseUnit.name);
            }
            else
            {
                Debug.Log("�����̋�ł͂���܂���B���o�����Ƃ��đI���ł��܂���B");
            }
        }
        int CurrentTurn = currentPlayer;

        // �����I������Ă��Ȃ���Ώ��������Ȃ�
        if (tile && selectUnit && movableTiles.ContainsKey(tile))
        {
            movableUnit(selectUnit, movableTiles[tile]);
            GetComponent<AudioSource>().Play();
            selectedReleaseUnit = null;
            EndTurn();
        }
        // �Ֆʂ̃^�C�����I�����ꂽ���A������I������Ă��邩�m�F
        if (tile != null && capturedPieceManager.GetSelectedPieceType() != UnitType.None)
        {
            Vector2Int selectedTileIndex = movableTiles[tile];
            PlacePieceFromCaptured(selectedTileIndex); // �������Ֆʂɔz�u
        }

        if (tile != null && absorptionPieceManager.GetSelectedPieceType() != UnitType.None)
        {
            Vector2Int selectedTileIndex = movableTiles[tile];
            PlacePieceFromAbsorption(selectedTileIndex); // �������Ֆʂɔz�u
        }

        // �z���J�[�\���̑I��
        else if (tile && selectUnit && absorbableTiles.ContainsKey(tile))
        {
            absorbableUnit(selectUnit, absorbableTiles[tile]);
        }

        // ���o�J�[�\���̑I��
        if (tile && selectedReleaseUnit != null && relesableTiles.ContainsKey(tile))
        {
            Debug.Log("���o�J�[�\�����N���b�N���܂����B����ړ����܂�: " + selectedReleaseUnit.name);

            // ��̈ړ�����
            selectedReleaseUnit.ReleseCheck = true;
            movableUnit(selectedReleaseUnit, relesableTiles[tile]);
            ReleaseEffect(selectedReleaseUnit);
            absorptionHistory.Remove(selectedReleaseUnit);
            selectedReleaseUnit.gameObject.GetComponent<MeshRenderer>().material = BaseMaterial;

            if (selectedReleaseUnit.AbsorUnitType != UnitType.None)
            {
                if (selectedReleaseUnit.AbsorUnitPlayer == currentPlayer)
                {
                    absorptionPieceManager.AddAbsorptionPiece(selectedReleaseUnit.AbsorUnitType, currentPlayer);
                }
                else
                {
                    capturedPieceManager.AddCapturedPiece(selectedReleaseUnit.AbsorUnitType, currentPlayer);
                }
            }
            selectedReleaseUnit.AbsorUnitType = UnitType.None;

            // �I����ԉ���
            selectedReleaseUnit = null;
            EndTurn();
        }
        // ���j�b�g��I��
        if (null != unit)
        {
            selectCursors(unit);
        }
    }
    //-------�^�[���������I���--------


    //-------���j�b�g���쏈���J�n--------
    void movableUnit(UnitController unit, Vector2Int tileindex)
    {
        if (unit == null || !tiles.ContainsKey(tileindex)) return;

        Vector2Int oldpos = unit.Pos;
        UnitController targetUnit = units[tileindex.x, tileindex.y];
        // ����̋����鏈��
        if (targetUnit != null && targetUnit.Player != unit.Player)
        {
            // �h����ʂ��m�F���A�L���Ȃ�U���𖳌���
            if (targetUnit.hasTemporaryDefense && targetUnit.fillingCheck)
            {
                targetUnit.hasTemporaryDefense = false;  // �h����ʂ�����
                Debug.Log($"{targetUnit.UnitType} �̖h����ʂ��������A�U��������������܂����I");
                if (targetUnit.AbsorUnitType != UnitType.None)
                {
                    if (targetUnit.AbsorUnitPlayer != currentPlayer)
                    {
                        absorptionPieceManager.AddAbsorptionPiece(targetUnit.AbsorUnitType, 1 - currentPlayer);
                    }
                    else
                    {
                        capturedPieceManager.AddCapturedPiece(targetUnit.AbsorUnitType, 1 - currentPlayer);
                    }
                }
                targetUnit.ReleseCheck = true;
                ReleaseEffect(targetUnit);
                unit.GetComponent<MeshRenderer>().material = BaseMaterial;
                targetUnit.GetComponent<MeshRenderer>().material = BaseMaterial;
                absorptionHistory.Remove(targetUnit);
                //EndTurn();
                return;  // ������X�L�b�v
            }
            AddCapturedUnit(targetUnit.UnitType);
            CaptureAndDemote(targetUnit);
            if (targetUnit.absorptionCheck && targetUnit.AbsorUnitType != UnitType.None)
            {
                if (targetUnit.AbsorUnitPlayer == currentPlayer) // �����̋���z�����Ă����ꍇ
                {
                    capturedPieceManager.AddCapturedPiece(targetUnit.AbsorUnitType, targetUnit.Player);
                }
                else // ����̋���z�����Ă����ꍇ
                {
                    absorptionPieceManager.AddAbsorptionPiece(targetUnit.AbsorUnitType, targetUnit.Player);
                }

                absorptionHistory.Remove(targetUnit);
            }
            //Destroy(targetUnit.gameObject);
        }
        // ���j�b�g�̈ړ�
        units[oldpos.x, oldpos.y] = null;
        unit.Move(tiles[tileindex], tileindex);
        units[tileindex.x, tileindex.y] = unit;

        ClearCursors();
        // ���蔻��
        if (ShouldPromote(unit, tileindex, oldpos))
        {
            ClearCursors();
            ShowPromoteOptions(unit, currentPlayer);// ����I�v�V������\��
            startOuteNextTurnDisabled = true; // �v�����[�g���肪���������ꍇ�A`StartOuteNextTurn`�𖳌���
            //oute(unit, tileindex);
            return;// ����{�^�����\������Ă���̂Ń^�[���͐؂�ւ��Ȃ�
        }
        //oute(unit, tileindex);
        //EndTurn();
        StartCoroutine(ClearCursorsWithDelay(0.01f));
    }

    void absorbableUnit(UnitController unit, Vector2Int tileindex)
    {
        gametime.isPaused = true;
        DisableInput();

        absorptionHistory.Add(selectUnit);

        if (unit == null || !tiles.ContainsKey(tileindex)) return;

        Vector2Int oldpos = unit.Pos;
        absorUnit = units[tileindex.x, tileindex.y];
        if (absorUnit != null)
        {
            if (absorUnit.hasTemporaryDefense && absorUnit.fillingCheck)
            {
                absorUnit.hasTemporaryDefense = false;  // �h����ʂ�����
                if (absorUnit.AbsorUnitType != UnitType.None)
                {
                    if (absorUnit.AbsorUnitPlayer != currentPlayer)
                    {
                        absorptionPieceManager.AddAbsorptionPiece(absorUnit.AbsorUnitType, 1 - currentPlayer);
                    }
                    else
                    {
                        capturedPieceManager.AddCapturedPiece(absorUnit.AbsorUnitType, 1 - currentPlayer);
                    }
                }
                unit.ReleseCheck = true;
                unit.absorptionCheck = true;
                ReleaseEffect(absorUnit);
                unit.GetComponent<MeshRenderer>().material = BaseMaterial;
                absorUnit.GetComponent<MeshRenderer>().material = BaseMaterial;
                absorptionHistory.Remove(absorUnit);
                EndTurn();
                return;
            }
            Destroy(absorUnit.gameObject, 0.7f);
            unit.Move(tiles[oldpos], oldpos);
            AbsorEffect(absorUnit);
            absorTurn = Turn + 1;
            unit.absorTurn = absorTurn;
            absorTurn = 0;
            unit.AbsorCheck();
            FillingEffect(unit);
            // absorbableUnit���\�b�h�̒��A�z�����m�肵����ɒǉ�
            if ((unit.UnitType == UnitType.hisya && (absorUnit.UnitType == UnitType.huhyou || absorUnit.UnitType == UnitType.kyousya)) ||
                    (unit.UnitType == UnitType.kyousya && (absorUnit.UnitType == UnitType.huhyou || absorUnit.UnitType == UnitType.kyousya))||
                    (unit.UnitType == UnitType.keima && absorUnit.UnitType == UnitType.keima))

            //if ((unit.UnitType == UnitType.hisya && (absorUnit.UnitType == UnitType.huhyou || absorUnit.UnitType == UnitType.kyousya)) ||
            //(unit.UnitType == UnitType.kyousya && absorUnit.UnitType == UnitType.huhyou))
            {
                // �z����Ɉ�x�����̖h����ʂ�t�^
                unit.hasTemporaryDefense = true;
                Debug.Log($"{unit.UnitType} �Ɉ�x�����̖h����ʂ��t�^����܂���");
            }
            unit.absorptionCheck = true;
            ClearCursors();
            unit.AbsorUnitPlayer = absorUnit.Player;
            // �z�����ꂽ��̌��v���C���[���r���ċ��ɑ���
            if (absorUnit.AbsorUnitType != UnitType.None)
            {
                if (absorUnit.Player == currentPlayer)
                {
                    // �z�����ꂽ������̋�̏ꍇ
                    if (absorUnit.AbsorUnitPlayer == currentPlayer)
                    {
                        absorptionPieceManager.AddAbsorptionPiece(absorUnit.AbsorUnitType, currentPlayer);
                    }
                    else
                    {
                        capturedPieceManager.AddCapturedPiece(absorUnit.AbsorUnitType, currentPlayer);
                    }
                }
                else
                {
                    // �z�����ꂽ�����̋�̏ꍇ
                    if (absorUnit.AbsorUnitPlayer == currentPlayer)
                    {
                        capturedPieceManager.AddCapturedPiece(absorUnit.AbsorUnitType, 1 - currentPlayer);
                    }
                    else
                    {
                        absorptionPieceManager.AddAbsorptionPiece(absorUnit.AbsorUnitType, 1 - currentPlayer);
                    }
                }
            }
        }
        absorptionHistory.Remove(absorUnit);
        Destroy(absorUnit.GetComponent<UnitController>());
        StartCoroutine(AbsorptionDelay(unit, tileindex));
    }
    //��̏�Ԃ��X�V���郁�\�b�h
    public void UpdateUnitPosition(UnitController unit)
    {
        if (unit == null)
        {
            //Debug.LogError("UpdateUnitPosition���\�b�h��unit��null�ł��B");
            return;
        }

        Vector2Int pos = unit.Pos;
        if (pos.x < 0 || pos.x >= boardWidth || pos.y < 0 || pos.y >= boardHeight)
        {
            //Debug.LogError("UpdateUnitPosition���\�b�h�Ŗ����ȃ|�W�V�������w�肳��܂����B");
            return;
        }

        // ���V�����ʒu�ɓo�^
        units[pos.x, pos.y] = unit;
        //Debug.Log($"{unit.UnitType} ���ʒu {pos} �ɍēo�^���܂����B");
    }

    private void CaptureAndDemote(UnitController unit)
    {
        UnitType originalType = unit.OldUnitType;  // ���̋�̎�ނ��擾
        unit.Demote(); // �����Ԃ��������Č��̋�ɖ߂�
        capturedPieceManager.AddCapturedPiece(originalType, currentPlayer);// ������Ƃ��Č��̋��ǉ�
        Destroy(unit.gameObject);// �ߊl���ꂽ��𖳌���
    }

    public bool ShouldPromote(UnitController unit, Vector2Int newPos, Vector2Int oldpos)
    {
        bool wasInOpponentTerritory = (unit.Player == 0 && oldpos.y <= 2) || (unit.Player == 1 && oldpos.y >= 6);
        bool isInOpponentTerritory = (unit.Player == 0 && newPos.y <= 2) || (unit.Player == 1 && newPos.y >= 6);
        return (unit.UnitType != UnitType.narigoma && unit.UnitType != UnitType.kinsyou && unit.UnitType != UnitType.ryuuou && unit.UnitType != UnitType.ryuuma) && (isInOpponentTerritory || wasInOpponentTerritory);
    }
    //-------���j�b�g���쏈���I���--------

    //-------�J�[�\�������J�n--------
    List<Vector2Int> getMovableTiles(UnitController unit)// �ړ��\�͈͂��擾
    {
        List<Vector2Int> ret = unit.GetMovableTiles(units); // �ʏ�͈͊O
        return ret;
    }
    List<Vector2Int> getAbsorbableTiles(UnitController unit)
    {
        List<Vector2Int> ret = unit.GetAbsorptionTiles(units);
        return ret;
    }
    List<Vector2Int> getRelesableTiles(UnitController unit)
    {
        List<Vector2Int> ret = unit.GetRelesableTiles(units);
        return ret;
    }
    public List<Vector2Int> getMovableTilesWithRelease(UnitController unit)
    {
        List<Vector2Int> movableTiles = unit.GetMovableTiles(units);
        List<Vector2Int> releaseTiles = unit.GetRelesableTiles(units);

        // �ʏ�̏[�U���������Ă���ꍇ�A���o�}�X��ǉ�
        if (unit.fillingCheck)
        {
            movableTiles.AddRange(releaseTiles);
        }
        return movableTiles;
    }

    void selectCursors(UnitController unit = null)
    {
        // �����̃J�[�\�����폜
        ClearCursors();

        // �I�𒆂̃��j�b�g���I����Ԃɖ߂�
        if (selectUnit != null)
        {
            selectUnit.Selected(false);
            selectUnit = null;
        }
        if (unit.Player != currentPlayer)
        {
            selectedReleaseUnit = null;
        }
        // ���j�b�g�����݂��Ȃ��ꍇ�͏I��
        if (unit == null) return;

        // �����̃^�[�����ǂ����̊m�F
        // �����̃^�[���łȂ��ꍇ�A�����̃��j�b�g��I���ł��Ȃ��悤�ɂ���
        if (turnChange && unit.Player != currentPlayer)
        {
            //Debug.Log("�����̃^�[���ł͂���܂���");
            return; // �����̋�łȂ��ꍇ�⎩���̃^�[���łȂ��ꍇ�͏������Ȃ�
        }
        // �I���������currentReleaseUnit�Ƃ��Đݒ�
        currentReleaseUnit = unit;
        // �ړ��\�͈͂��擾
        List<Vector2Int> movabletiles = getMovableTiles(unit);
        movableTiles.Clear();

        // �z���\�͈͂��擾
        List<Vector2Int> absorbabletiles = getAbsorbableTiles(unit);
        absorbableTiles.Clear();

        // ���o�\�͈͂��擾
        List<Vector2Int> relesabletiles = getRelesableTiles(unit);
        relesableTiles.Clear();

        foreach (var tile in movabletiles)
        {
            movableTiles.Add(tiles[tile], tile);

            // �J�[�\����\��
            Vector3 pos = tiles[tile].transform.position;
            pos.y -= 0.01f;
            GameObject cursor = Instantiate(prefabCursor, pos, Quaternion.identity);
            cursors.Add(cursor);

        }
        //foreach (var tile in relesabletiles)
        //{
        //    relesableTiles.Add(tiles[tile], tile);
        //    Vector3 pos = tiles[tile].transform.position;
        //    pos.y -= 0.01f;
        //    GameObject relesecursor = Instantiate(releseCursor, pos, Quaternion.identity);
        //    relesecursors.Add(relesecursor);
        //}

        //�z����cursor
        //if (!unit.absorptionCheck) // ��x���z�����Ă��Ȃ�������
        //if (!unit.absorptionCheck && (unit.PutTurn == 0 || Turn > unit.PutTurn + 2)) // �{��䂩��u����1�^�[���o������
        if (!unit.absorptionCheck && (( unit.PutUnitCheck && unit.Movecount >= 1) || !unit.PutUnitCheck)) // �{��䂩��u���Ēu�������������
        {
            foreach (var tile in absorbabletiles)
            {
                absorbableTiles.Add(tiles[tile], tile);

                // �J�[�\����\��
                Vector3 pos = tiles[tile].transform.position;
                pos.y -= 0.01f;
                GameObject absorcursor = Instantiate(absorption, pos, Quaternion.identity);
                absorptioncursors.Add(absorcursor);
            }
        }

        // ���o�̃J�[�\��
        //unit.FillingCheck(unit); // �[�U�`�F�b�N

        if (!unit.ReleseCheck && unit.fillingCheck)
        {
            foreach (var tile in relesabletiles)
            {
                // �L�[�̑��݂��m�F���Ă���ǉ�
                if (!relesableTiles.ContainsKey(tiles[tile]))
                {
                    relesableTiles.Add(tiles[tile], tile);

                    // �J�[�\����\��
                    Vector3 pos = tiles[tile].transform.position;
                    pos.y -= 0.01f;
                    GameObject relesecursor = Instantiate(releseCursor, pos, Quaternion.identity);
                    relesecursors.Add(relesecursor);
                }
            }
        }


        // �I����Ԃɂ���
        unit.Selected();
        selectUnit = unit;

        //����`�F�b�N
        outecheck(unit);
    }

    public void DisplayPlaceableCursors()
    {
        ClearCursors();  // �����̃J�[�\�����N���A

        foreach (var tile in tiles)
        {
            Vector2Int position = tile.Key;

            // ����`�F�b�N��ǉ�: �����̕������łɂ����ɂ̓J�[�\����\�����Ȃ�
            if (capturedPieceManager.GetSelectedPieceType() == UnitType.huhyou && IsNifu(UnitType.huhyou, position.x))
            {
                continue;  // �������������ꏊ�ɂ̓J�[�\����\�����Ȃ�
            }

            // ������u����ꏊ���`�F�b�N
            if (CanPlacePiece(capturedPieceManager.GetSelectedPieceType(), position))
            {
                Vector3 pos = tile.Value.transform.position;
                pos.y += 0.05f;  // �����������ĕ\��

                GameObject cursor = Instantiate(prefabCursor, pos, Quaternion.identity);
                cursors.Add(cursor);

                // �^�C���� movableTiles �ɒǉ�
                movableTiles[tile.Value] = position;
            }
        }
    }

    public void DisplayPlaceableCursorsAbsor()
    {
        ClearCursors();  // �����̃J�[�\�����N���A

        foreach (var tile in tiles)
        {
            Vector2Int position = tile.Key;

            // ����`�F�b�N��ǉ�: �����̕������łɂ����ɂ̓J�[�\����\�����Ȃ�
            if (absorptionPieceManager.GetSelectedPieceType() == UnitType.huhyou && IsNifu(UnitType.huhyou, position.x))
            {
                continue;
            }

            // ������u����ꏊ���`�F�b�N
            if (CanPlaceAbsorPiece(absorptionPieceManager.GetSelectedPieceType(), position))
            {
                Vector3 pos = tile.Value.transform.position;
                pos.y += 0.05f;  // �����������ĕ\��

                GameObject cursor = Instantiate(prefabCursor, pos, Quaternion.identity);
                cursors.Add(cursor);

                // �^�C���� movableTiles �ɒǉ�
                movableTiles[tile.Value] = position;
            }
        }
    }

    // �J�[�\�����N���A����R���[�`��
    private IEnumerator ClearCursorsWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);  // �w�肵���b���҂�
        ClearCursors();  // �J�[�\�����N���A
    }
    //�z���̃J�[�\�����N���A����R���[�`��
    IEnumerator AbsorptionDelay(UnitController unit, Vector2Int tileindex)
    {
        yield return new WaitForSeconds(0.01f);

        ClearCursors(); // �J�[�\���N���A

        // �z����̏���
        EndTurn();
    }

    public void ClearCursors()
    {
        Debug.Log("a");
        foreach (var cursor in cursors) Destroy(cursor);
        cursors.Clear();
        foreach (var absorcursor in absorptioncursors) Destroy(absorcursor);
        absorptioncursors.Clear();
        foreach (var relesecursor in relesecursors) Destroy(relesecursor);  
        relesecursors.Clear();
        DestroyObjectsByName("Mark");
        DestroyObjectsByName("AbsorptionMark");
        DestroyObjectsByName("ReleseMark");
    }

    // ���O�ŃI�u�W�F�N�g���폜���郆�[�e�B���e�B���\�b�h
    private void DestroyObjectsByName(string name)
    {
        GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();
        foreach (var obj in objects)
        {
            if (obj.name == name)
            {
                Destroy(obj);
            }
        }
    }
    //-------�J�[�\�������I���--------

    //-------�^�C�����Z�b�g�����J�n--------
    private void ResetTileStates()
    {
        movableTiles.Clear();
        absorbableTiles.Clear();
        relesableTiles.Clear();
    }
    //-------�^�C�����Z�b�g�����J�n--------

    //-------��������J�n--------
    public bool CanPlacePiece(UnitType type, Vector2Int position)
    {
        // �ʒu���󂢂Ă��邩���m�F
        if (units[position.x, position.y] != null)
        {
            //Debug.Log("���̏ꏊ�ɂ͊��ɋ����܂��B");
            return false;
        }
        // ����̋�̔z�u����
        int boardSizeY = units.GetLength(1);
        int enemyTerritoryY = (currentPlayer == 0) ? boardSizeY - 9 : 8;
        int enemyTerritoryYMinusOne = (currentPlayer == 0) ? boardSizeY - 8 : 7;
        // �����܂��͍��Ԃ�G�w��ԉ��ɒu���Ȃ�
        if ((type == UnitType.huhyou || type == UnitType.kyousya) && position.y == enemyTerritoryY)
        {
            //Debug.Log($"{type} �͓G�w�̈�ԉ��ɒu���܂���B");
            return false;
        }
        // �j�n��G�w��ԉ�����т��̎�O�ɒu���Ȃ�
        if (type == UnitType.keima && (position.y == enemyTerritoryY || position.y == enemyTerritoryYMinusOne))
        {
            //Debug.Log("�j�n�͓G�w�̈�ԉ��܂��͂��̎�O�ɒu���܂���B");
            return false;
        }

        return true;
    }

    public bool CanPlaceAbsorPiece(UnitType type, Vector2Int position)
    {
        // �ʒu���󂢂Ă��邩���m�F
        if (units[position.x, position.y] != null)
        {
            //Debug.Log("���̏ꏊ�ɂ͊��ɋ����܂��B");
            return false;
        }

        // ����̋�̔z�u����
        bool dontPutArea = ((currentPlayer == 0 && position.y <= 5) || (currentPlayer == 1 && position.y >= 3));
        // ���w�ɂ����u���Ȃ�
        if (dontPutArea)
        {
            //Debug.Log("���w�ɂ����u���܂���");
            return false;
        }
        return true;
    }

    //����̃`�F�b�N
    private bool IsNifu(UnitType pieceType, int column)
    {
        // ���ȊO�̋�̏ꍇ�͓���̃`�F�b�N���s��Ȃ�
        if (pieceType != UnitType.huhyou)
        {
            return false;
        }

        // �Ֆʏ�̋���m�F
        for (int y = 0; y < boardHeight; y++)
        {
            UnitController unit = units[column, y];
            // �����̕������ɂ���ꍇ�͓��
            if (unit != null && unit.Player == currentPlayer && unit.UnitType == UnitType.huhyou)
            {
                return true;  // ����ɂȂ�
            }
        }

        return false;  // ����ł͂Ȃ�
    }

    private void PlacePieceFromCaptured(Vector2Int position)
    {
        // ����`�F�b�N��ǉ�
        if (capturedPieceManager.GetSelectedPieceType() == UnitType.huhyou && IsNifu(UnitType.huhyou, position.x))
        {
            //Debug.Log("����͂ł��܂���B");
            return;  // ����Ȃ̂ŋ��u�����A�^�[�����I�����Ȃ�
        }

        // �ʏ�̋�z�u����
        if (!CanPlacePiece(capturedPieceManager.GetSelectedPieceType(), position))
        {
            //Debug.Log("���̏ꏊ�ɂ͋��u���܂���B");
            return;
        }

        // ��̃v���t�@�u���擾���Đ���
        GameObject piecePrefab = PrefabUnits[(int)capturedPieceManager.GetSelectedPieceType() - 1];
        if (piecePrefab == null)
        {
            //Debug.LogError("��̃v���t�@�u��������܂���B");
            return;
        }

        // ��𐶐����A��]��K�p
        Quaternion rotation = Quaternion.Euler(-90, currentPlayer * 180, 0);
        GameObject newPiece = Instantiate(piecePrefab, GetTile(position).transform.position, rotation);
        UnitController unitController = newPiece.GetComponent<UnitController>();
        unitController.Init(currentPlayer, (int)capturedPieceManager.GetSelectedPieceType(), GetTile(position), position);
        GetComponent<AudioSource>().Play();

        unitController.PutTurn = Turn;
        unitController.PutUnitCheck = true;

        // ���j�b�g��ՖʂɃZ�b�g
        units[position.x, position.y] = unitController;

        // ���̎���������炷����
        capturedPieceManager.ReduceCapturedPieceCount(capturedPieceManager.GetSelectedPieceType(), currentPlayer);

        // �I�������ƃJ�[�\���N���A
        capturedPieceManager.ClearSelectedPiece();
        ClearCursors();

        // �^�[���I������
        EndTurn();  // �^�[�����I���̂͂��̃^�C�~���O����
    }

    private void PlacePieceFromAbsorption(Vector2Int position)
    {
        // ����`�F�b�N��ǉ�
        if (absorptionPieceManager.GetSelectedPieceType() == UnitType.huhyou && IsNifu(UnitType.huhyou, position.x))
        {
            //Debug.Log("����͂ł��܂���B");
            return;  // ����Ȃ̂ŋ��u�����A�^�[�����I�����Ȃ�
        }

        if (!CanPlaceAbsorPiece(absorptionPieceManager.GetSelectedPieceType(), position))
        {
            //Debug.Log("���̏ꏊ�ɂ͋��u���܂���B");
            return;
        }

        GameObject piecePrefab = PrefabUnits[(int)absorptionPieceManager.GetSelectedPieceType() - 1];
        if (piecePrefab == null)
        {
            //Debug.LogError("��̃v���t�@�u��������܂���B");
            return;
        }

        // ��𐶐����A��]��K�p
        Quaternion rotation = Quaternion.Euler(-90, currentPlayer * 180, 0);
        GameObject newPiece = Instantiate(piecePrefab, GetTile(position).transform.position, rotation);
        UnitController unitController = newPiece.GetComponent<UnitController>();
        unitController.Init(currentPlayer, (int)absorptionPieceManager.GetSelectedPieceType(), GetTile(position), position);
        GetComponent<AudioSource>().Play();

        unitController.PutTurn = Turn;
        unitController.PutUnitCheck = true;


        // ���j�b�g��ՖʂɃZ�b�g
        units[position.x, position.y] = unitController;

        // ���̎���������炷����
        absorptionPieceManager.ReduceAbsorPieceCount(absorptionPieceManager.GetSelectedPieceType(), currentPlayer);

        // �I�������ƃJ�[�\���N���A
        absorptionPieceManager.ClearSelectedPiece();
        ClearCursors();

        // �^�[���I������
        EndTurn();  // �^�[�����I���̂͂��̃^�C�~���O����
    }
    //-------��������I���--------

    //-----���蔻��-----
    //���肳�ꂽ���̉��̍��W Player0KingPos Player1KingPos
    public void oute(UnitController unit, Vector2Int tileindex)
    {
        if (unit == null)
        {
            Debug.LogError("Unit is null in oute method.");
            return;
        }

        if (tileindex == null)
        {
            Debug.LogError("Tile index is null in oute method.");
            return;
        }
        // ��̈ړ��͈͂��擾�B�[�U���������Ă���ꍇ�̂ݕ��o�}�X���܂߂�
        List<Vector2Int> outeTiles = unit.fillingCheck ? getMovableTilesWithRelease(unit) : getMovableTiles(unit);

        Debug.Log($"�� {unit.UnitType} �̈ړ��}�X����ѕ��o�}�X: {string.Join(", ", outeTiles)}");

        // ����̔���
        foreach (var tilePos in outeTiles)
        {
            UnitController targetUnit = units[tilePos.x, tilePos.y];
            //���̈ʒu
            GetKingPosition(unit.Player);
            UpdateKingPosition(unit);
            Vector2Int kingPosition = GetCurrentKingPosition(unit.Player);
            if (targetUnit != null && targetUnit.Player != unit.Player &&
                (targetUnit.UnitType == UnitType.oushyou || targetUnit.UnitType == UnitType.gyokusyou))
            {
                Debug.Log($"���蔻��: {unit.UnitType} (�v���C���[{unit.Player}) �� {targetUnit.UnitType} (�v���C���[{targetUnit.Player}) �����肵�܂����I");
                isKingInCheck = true;
                isCheckTriggeredNextTurn = true; // ���̃^�[���ł̉��胍�O�o�̓t���O
                if (unit.Player == 0 && targetUnit.Player == 1)
                {
                    Player1KingPos = targetUnit.Pos;
                }
                else if (unit.Player == 1 && targetUnit.Player == 0)
                {
                    Player0KingPos = targetUnit.Pos;
                }
            }
        }

        // ��������`�F�b�N
        if (!DidBlockCheck(unit, tileindex))
        {
            isKingInCheck = IsKingStillInCheck(); // ���肪�܂������Ă��邩���m�F
            if (isKingInCheck)
            {
                isCheckTriggeredNextTurn = false; // ��������Ȃ�t���O�����Z�b�g
                //Debug.Log("�����������܂����B");
            }
        }
        else
        {
            isKingInCheck = false; // ����������ꂽ�ꍇ�̓t���O�����Z�b�g
        }

        // ���������Ȃ������ꍇ�̏���
        if (isKingInCheck)
        {
            UnitController kingUnit = GetKingUnit(currentPlayer);
            if (kingUnit != null && (kingUnit.Pos == Player0KingPos || kingUnit.Pos == Player1KingPos))
            {
                kingMoveCount++;
                if (kingMoveCount >= maxMoveCount)
                {
                    string winner = currentPlayer == 0 ? "Player2" : "Player1";
                    Debug.Log($"���������Ȃ��������ߔs�k���܂����I�v���C���[{currentPlayer + 1} �̏����ł��B");
                    StartCoroutine(EndGame(winner)); return;
                }
            }
            else
            {
                kingMoveCount = 0; // �����������̂ŃJ�E���g�����Z�b�g
            }
        }
    }

    //���肳�ꂽ���̃^�[���ɃA�j���[�V����
    public void StartOuteNextTurn()
    {
        //skipActivateText = true; // StartOuteNextTurn�����s���ꂽ�ꍇ�Ƀt���O�𗧂Ă�
        if (isCheckTriggeredNextTurn)
        {
            gametime.isPaused = true;
            DisableInput();

            Debug.Log("���肪�p�����Ă��܂��I");
            isCheckTriggeredNextTurn = false; // ���O�o�͌�Ƀ��Z�b�g
            skipActivateText = true; // StartOuteNextTurn�����s���ꂽ�ꍇ�Ƀt���O�𗧂Ă�
            GameObject[] Backimage = { OuteEffect[0].transform.GetChild(0).gameObject, OuteEffect[1].transform.GetChild(0).gameObject };
            GameObject[] image = { OuteEffect[0].transform.GetChild(1).gameObject, OuteEffect[1].transform.GetChild(1).gameObject };
            GameObject[] text = { OuteEffect[0].transform.GetChild(2).gameObject, OuteEffect[1].transform.GetChild(2).gameObject };
            if (currentPlayer == 0)
            {
                Backimage[0].GetComponent<Image>().color = new Color(100f / 255f, 30f / 255f, 30f / 255f, 0.5f);
                image[0].GetComponent<Image>().color = new Color(1f, 30f / 255f, 30f / 255f, 0.5f);
                text[0].GetComponent<TextMeshProUGUI>().color = new Color(1f, 30f / 255f, 30f / 255f, 0.5f);
                Backimage[1].GetComponent<Image>().color = new Color(30f / 255f, 100f / 255f, 175f / 255f, 0.5f);
                image[1].GetComponent<Image>().color = new Color(30f / 255f, 0.5f, 1f, 0.5f);
                text[1].GetComponent<TextMeshProUGUI>().color = new Color(30f / 255f, 0.5f, 1f, 0.5f);
            }
            else if (currentPlayer == 1)
            {
                Backimage[0].GetComponent<Image>().color = new Color(30f / 255f, 100f / 255f, 175f / 255f, 0.5f);
                image[0].GetComponent<Image>().color = new Color(30f / 255f, 0.5f, 1f, 0.5f);
                text[0].GetComponent<TextMeshProUGUI>().color = new Color(30f / 255f, 0.5f, 1f, 0.5f);
                Backimage[1].GetComponent<Image>().color = new Color(70f / 255f, 30f / 255f, 30f / 255f, 0.5f);
                image[1].GetComponent<Image>().color = new Color(1f, 30f / 255f, 30f / 255f, 0.5f);
                text[1].GetComponent<TextMeshProUGUI>().color = new Color(1f, 30f / 255f, 30f / 255f, 0.5f);
            }
            else
            {
                return;
            }
            ActivateHighlights((GameObject[])OuteEffect);
            Invoke("DeactivateTitleTimeHighlights", 2.0f); // 1�b��Ƀn�C���C�g���\��
            Invoke("ActivateText", 2.0f); // �n�C���C�g�I�����ActivateText�����s
        }
    }

    private void outecheck(UnitController unit)
    {
        // �������ړ�����O�Ɍ��݉��肩�ǂ������m�F����
        if (unit.UnitType == UnitType.oushyou || unit.UnitType == UnitType.gyokusyou)
        {
            Vector2Int kingPos = unit.Pos; // ���݂̉����̈ʒu���擾
            bool isInCheck = false;

            // �G�̋���݂̉����̈ʒu�ɑ΂��ĉ��肵�Ă��邩�m�F
            foreach (var myUnit in units)
            {
                if (myUnit != null && myUnit.Player != unit.Player)  // �G�̋���m�F
                {
                    List<Vector2Int> myUnitMovableTiles = myUnit.GetMovableTiles(units);

                    // �����̈ʒu���G�̋�̈ړ��\�͈͓��ɂ��邩�ǂ����m�F
                    if (myUnitMovableTiles.Contains(kingPos))
                    {
                        isInCheck = true;
                        break;
                    }
                }
            }

            // �ړ��O�ɉ���Ȃ烍�O���o��
            if (isInCheck)
            {
                Debug.Log("����̂܂܂ł��I");
            }
        }
    }
    void eat(UnitType type)
    {
        //�����v���C���[
        int WinningPlayerTwo = (currentPlayer == 0) ? 0 : 1;

        // ���������ꂽ���m�F
        if (type == UnitType.oushyou || type == UnitType.gyokusyou) // UnitType.King ��������\��
        {
            string winner = WinningPlayerTwo == 0 ? "Player1" : "Player2";

            // ���s�m�莞��EndGame���Ăяo��
            Debug.LogError("end�Q�[���Ăяo���Q");
            StartCoroutine(EndGame(winner));
            Debug.Log($"�����܂��͋ʏ�������܂����I{WinningPlayerTwo + 1} �̏����ł�"); //�������͂����Ă���
            //Debug.LogError("sceneName��Result�������Ă��܂���");
            return; // ���������ꂽ�炻��ȏ�̏������s��Ȃ�
        }
    }

    List<Vector2Int> GetOpponentCheckTiles()
    {
        List<Vector2Int> checkTiles = new List<Vector2Int>();

        // ���݂̃v���C���[�̓G�̋�����ׂă`�F�b�N
        foreach (var unit in units)
        {
            if (unit != null && unit.Player != currentPlayer)
            {
                // �G�̋�̈ړ��\�Ȕ͈͂��擾
                List<Vector2Int> movableTiles = unit.GetMovableTiles(units);
                checkTiles.AddRange(movableTiles);
            }
        }
        return checkTiles;
    }

    bool DidBlockCheck(UnitController unit, Vector2Int newPosition)
    {
        // �G�̉���͈̔͂��擾
        List<Vector2Int> checkTiles = GetOpponentCheckTiles();

        //����͈̔͂Ɉړ�������̐V�����ʒu���܂܂�Ă��邩�m�F
        if (checkTiles.Contains(newPosition))
        {
            // ���ȊO�̋����͈͂ɓ������ꍇ�A�����ƌ��Ȃ�
            //return unit.UnitType != UnitType.oushyou || unit.UnitType != UnitType.gyokusyou;
        }
        return false;
    }
    bool IsKingStillInCheck()
    {
        // ���݂̃v���C���[�̉����擾
        UnitController kingUnit = GetKingUnit(currentPlayer);
        if (kingUnit == null)
        {
            //Debug.LogError("����������܂���ł����B");
            return false;
        }

        // �G�̋�ړ�����ѕ��o�\�Ȕ͈͂��擾
        List<Vector2Int> opponentCheckTiles = GetOpponentCheckTilesWithRelease();

        // �����G�̍U���͈͂ɂ��邩�ǂ������m�F
        bool isInCheck = opponentCheckTiles.Contains(kingUnit.Pos);
        Debug.Log($"���̈ʒu: {kingUnit.Pos} | ������: {isInCheck}");

        return isInCheck;
    }

    // �G�̈ړ��}�X�ƕ��o�}�X���擾���郁�\�b�h
    List<Vector2Int> GetOpponentCheckTilesWithRelease()
    {
        List<Vector2Int> opponentCheckTiles = new List<Vector2Int>();

        foreach (var unit in units)
        {
            if (unit != null && unit.Player != currentPlayer) // �G�̋�̂݃`�F�b�N
            {
                List<Vector2Int> combinedTiles = getMovableTilesWithRelease(unit); // �ړ��ƕ��o�}�X�𓝍�
                opponentCheckTiles.AddRange(combinedTiles);
            }
        }

        return opponentCheckTiles;
    }
    UnitController GetKingUnit(int player)
    {
        foreach (var unit in units)
        {
            if (unit != null && unit.Player == player &&
                (unit.UnitType == UnitType.oushyou || unit.UnitType == UnitType.gyokusyou))
            {
                return unit;
            }
        }
        return null;
    }

    // �v���C���[���Ƃ̉��̈ʒu���擾����
    Vector2Int GetKingPosition(int player)
    {
        // �v���C���[0�Ȃ�u�����v�܂��́u�ʏ��v�̈ʒu�� OusyouPos ����擾
        // �v���C���[1�Ȃ�u�����v�܂��́u�ʏ��v�̈ʒu�� GyokusyouPos ����擾
        return (player == 0) ? Player0KingPos : Player1KingPos;
    }

    void UpdateKingPosition(UnitController unit)
    {
        if (unit.UnitType == UnitType.oushyou || unit.UnitType == UnitType.gyokusyou)
        {
            if (unit.Player == 0)
            {
                // �v���C���[0�̉��̈ʒu���X�V
                Player0KingPos = unit.Pos;
            }
            else if (unit.Player == 1)
            {
                // �v���C���[1�̉��̈ʒu���X�V
                Player1KingPos = unit.Pos;
            }
        }
    }

    // �v���C���[���Ƃ̌��݂̉��̈ʒu���擾����֐�
    Vector2Int GetCurrentKingPosition(int player)
    {
        return (player == 0) ? Player0KingPos : Player1KingPos;
    }

    void InitializeKingPositions()
    {
        // Player0�̉��̏����ʒu��ݒ�
        Player0KingPos = new Vector2Int(4, 8); //�v���C���[0�̉��̏����ʒu�o�^�@�������Ă��ꏊ�͕ς��Ȃ�
        Player1KingPos = new Vector2Int(4, 0); //�v���C���[1�̉��̏����ʒu�o�^�@�������Ă��ꏊ�͕ς��Ȃ�
    }

    public void CheckForCheckAfterMove()
    {
        // �G�v���C���[�̂��ׂĂ̋����������Ă��邩���m�F
        List<UnitController> enemyUnits = GetEnemyUnits(currentPlayer); // �G�v���C���[�̋�X�g�擾

        foreach (var enemyUnit in enemyUnits)
        {
            List<Vector2Int> enemyMovableTiles = enemyUnit.GetMovableTiles(units); // �G��̈ړ��\�}�X���擾
            Vector2Int kingPosition = GetCurrentKingPosition(currentPlayer); // ���v���C���[�̉��̈ʒu���擾

            // �G�̈ړ��͈͂Ɏ����̉��̈ʒu������ꍇ
            if (enemyMovableTiles.Contains(kingPosition))
            {
                Debug.Log($"{enemyUnit.UnitType} (�v���C���[{enemyUnit.Player}) �� {kingPosition} �ŉ���������Ă��܂��I");
                isCheckTriggeredNextTurn = true;
                isKingInCheck = true;
                break;
            }
        }

        // ���肪�m�F����Ȃ��ꍇ
        if (!isKingInCheck)
        {
            Debug.Log("����͂������Ă��܂���B");
        }
    }

    // �G�v���C���[�̋�X�g���擾����w���p�[���\�b�h
    private List<UnitController> GetEnemyUnits(int player)
    {
        List<UnitController> enemyUnits = new List<UnitController>();
        foreach (var unit in units)
        {
            if (unit != null && unit.Player != player)
            {
                enemyUnits.Add(unit);
            }
        }
        return enemyUnits;
    }

    //���肵�����̃J�b�g�C�����o
    public void ActivateHighlights(GameObject[] highlights)
    {
        foreach (GameObject highlight in highlights)
        {
            highlight.gameObject.SetActive(true);
        }
    }

   
    //-----���蔻��I��-----

    //-------��I����ԉ��������J�n--------
    public void DeselectBoardPieces()
    {
        if (selectUnit != null)
        {
            ClearCursors();
            selectUnit.transform.position = new Vector3(selectUnit.transform.position.x, 0.1f, selectUnit.transform.position.z); // ���������Z�b�g
            selectUnit.Selected(false); // �I������
            selectUnit = null; // �I�𒆂̃��j�b�g�����Z�b�g
        }
    }
    //-------��I����ԉ��������I���--------

    //-------�^�[���̏I�������J�n--------
    private bool IsKing(UnitController unit)
    {
        return unit.UnitType == UnitType.oushyou || unit.UnitType == UnitType.gyokusyou;
    }
    public void EndTurn()
    {
        Debug.Log("a");
        ClearCursors();
        foreach (var unit in units)
        {
            if (unit != null && unit.Player == currentPlayer && IsKing(unit)) 
            {
                Debug.Log("aa");

                oute(selectUnit, unit.Pos); // ���蔻����s��
            }
        }

        foreach (var unit in absorptionHistory)
        {
            if (!unit.fillingCheck && Turn >= unit.absorTurn + 1)//�[�U�����͂O�ɂ���
            {
                unit.fillingCheck = true; // �[�U�����t���O�𗧂Ă�
                oute(unit, unit.Pos); // �[�U�������ɑ����ɉ��蔻������s
            }
        }

        playerMoveCounts[currentPlayer]++;// ���݂̃v���C���[�̎萔���J�E���g
        PlayerPrefs.SetInt("Player1MoveCount", playerMoveCounts[0]);
        PlayerPrefs.SetInt("Player2MoveCount", playerMoveCounts[1]);
        FillingEffectDestroy();
        TotalTime++;
        PlayerPrefs.SetInt("TotalTime", Mathf.RoundToInt(TotalElapsedTime)); // �b�P�ʂɕϊ�
        if (IsKingStillInCheck())
        {
            string winner = currentPlayer == 0 ? "Player2" : "Player1";
            Debug.Log($"���ł��������܂����B�v���C���[{winner}�̏����ł��B");
            StartCoroutine(EndGame(winner));
            return;
        }
        currentPlayer = 1 - currentPlayer;
        ResetTileStates();
        if (promoteManager != null) { promoteManager.HidePromoteOptions(currentPlayer); }
        capturedPieceManager.ClearSelectedPiece();
        absorptionPieceManager.ClearSelectedPiece();
        DeselectBoardPieces();
        Debug.Log($"�v���C���[{currentPlayer + 1}�̃^�[���ł��B");
        gametime.ResetBattleTime();
        ClearCursors();
        // �v���C���[�̃^�[�����؂�ւ��ۂɃ^�C�}�[���؂�ւ�
        FindAnyObjectByType<GameTime>().StopBattleTime(currentPlayer);
        CheckForCheckAfterMove();
        if (!startOuteNextTurnDisabled) //���蔻�肪�s��ꂽ���A�g���Ȃ��悤�ɂ���
        {
            StartOuteNextTurn();
            if (!skipActivateText)
            {
                ActivateText();
            }
        }
        skipActivateText = false; // �t���O�����Z�b�g���Ď��^�[���ōĂѓ���\��
        startOuteNextTurnDisabled = false; // ���̃^�[���ł͍ēx�g�p�ł���悤�Ƀ��Z�b�g
        Turn += 1;
        
    }
    //-------�^�[���̏I�������I���--------

    //-------�Q�[���̏I�������J�n--------
    public IEnumerator EndGame(string winner)
    {
        ClearAbsorEffects();
        yield return new WaitForSeconds(0.1f);
        Debug.Log("end�Q�[���Ăяo���ł�");
        isGameActive = false;
        PlayerPrefs.SetInt("Player1MoveCount", playerMoveCounts[0]);// �v���C���[�萔��ۑ�
        PlayerPrefs.SetInt("Player2MoveCount", playerMoveCounts[1]);
        PlayerPrefs.SetString("Winner", winner);// ���ҏ���ۑ�
        PlayerPrefs.SetInt("TotalTime", Mathf.RoundToInt(TotalElapsedTime));// �������Ԃ�ۑ�
        CaptureFinalBoards();
        // ���U���g�V�[���Ɉڍs
        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log("���U���g�V�[���ɍs���܂�");
            SceneManager.LoadScene(sceneName); // ���U���g�V�[���̖��O���w��
        }
        else
        {
            Debug.LogError("���U���g�V�[�����ݒ肳��Ă��܂���B");
        }
    }
    //-------�Q�[���̏I�������I���--------

    void CaptureFinalBoards()
    {
        RenderTexture renderTexture = new RenderTexture(totalWidth, totalHeight, 24);
        player1Camera.targetTexture = renderTexture;
        player2Camera.targetTexture = renderTexture;

        Texture2D combinedScreenshot = new Texture2D(totalWidth, totalHeight, TextureFormat.RGB24, false);

        // player1Camera �̃L���v�`��
        player1Camera.Render();
        RenderTexture.active = renderTexture;
        combinedScreenshot.ReadPixels(new Rect(0, 0, playerWidth, playerHeight), 0, 0);

        // player2Camera �̃L���v�`��
        player2Camera.Render();
        RenderTexture.active = renderTexture;
        combinedScreenshot.ReadPixels(new Rect(playerWidth, 0, playerWidth, playerHeight), playerWidth, 0);

        combinedScreenshot.Apply();

        // �摜��ۑ�
        string path = Application.persistentDataPath + "/BothCameras.png";
        File.WriteAllBytes(path, combinedScreenshot.EncodeToPNG());
        Debug.Log("���J�����̃X�N���[���V���b�g��ۑ����܂���: " + path);

        player1Camera.targetTexture = null;
        player2Camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);
    }
    void ShowPromoteOptions(UnitController unit, int currentPlayer)
    {
        promoteManager.ShowPromoteOptions(unit, currentPlayer);
    }

    void AddCapturedUnit(UnitType type)
    {
        eat(type);
        capturedUnits[type]++;
        //Debug.Log($"{type} �����܂����B���݂̐�: {capturedUnits[type]}");
    }

    public void AbsorEffect(UnitController unit)
    {
        Vector3 pos = unit.transform.position;
        currentAbsorEffect = Instantiate(absorEffect, pos, Quaternion.identity);
        Invoke("AbsorEffectDestroy", 1.9f);
    }

    private void ClearAbsorEffects()
    {
        // �V�[�����̂��ׂĂ�absorEffect�I�u�W�F�N�g���������A�폜
        GameObject[] absorEffects = GameObject.FindGameObjectsWithTag("AbsorEffect");
        foreach (GameObject effect in absorEffects)
        {
            Destroy(effect);
        }
    }
    public void FillingEffect(UnitController unit)
    {
        switch (unit.AbsorUnitType)
        {
            case UnitType.huhyou:
                newColor = Color.white;
                break;
            case UnitType.kakugyou:
                newColor = new Color(1f, 155f / 255f, 0f, 1f);
                break;
            case UnitType.hisya:
                newColor = Color.blue;
                break;
            case UnitType.kyousya:
                newColor = Color.red;
                break;
            case UnitType.keima:
                newColor = Color.green;
                break;
            case UnitType.ginsyou:
                newColor = Color.magenta;
                break;
            case UnitType.kinsyou:
                newColor = Color.yellow;
                break;
        }

        Vector3 pos = unit.transform.position;
        if (unit.Player == 1)
        {
            GameObject fillingeffect = Instantiate(fillingEffect, pos, Quaternion.Euler(-90, 180, 0));
            fillingeffect.transform.parent = unit.transform;
            //for (int i = 0; i < 2; i++)
            //{
            //    GameObject child = fillingeffect.transform.GetChild(i).gameObject;
            //    child.GetCompo
            //    nent<ParticleSystem>().startColor = Color.white;
            //}
            GameObject child = fillingeffect.transform.GetChild(0).gameObject;
            //child.GetComponent<ParticleSystem>().startColor = newColor;
            var mainModule = child.GetComponent<ParticleSystem>().main;
            mainModule.startColor = newColor;
            GameObject child2 = fillingeffect.transform.GetChild(1).gameObject;
            //child2.GetComponent<ParticleSystem>().startColor = Color.white;
            var mainModule2 = child2.GetComponent<ParticleSystem>().main;
            mainModule2.startColor = new Color(150f / 255f, 1f, 1f, 1f);
        }
        else
        {
            GameObject fillingeffect = Instantiate(fillingEffect, pos, Quaternion.Euler(-90, 0, 0));
            fillingeffect.transform.parent = unit.transform;
            //for (int i = 0; i < 2; i++)
            //{
            //    GameObject child = fillingeffect.transform.GetChild(i).gameObject;
            //    child.GetComponent<ParticleSystem>().startColor = Color.white;
            //}
            GameObject child = fillingeffect.transform.GetChild(0).gameObject;
            //child.GetComponent<ParticleSystem>().startColor = newColor;
            var mainModule = child.GetComponent<ParticleSystem>().main;
            mainModule.startColor = newColor;
            GameObject child2 = fillingeffect.transform.GetChild(1).gameObject;
            //child2.GetComponent<ParticleSystem>().startColor = Color.white;
            var mainModule2 = child2.GetComponent<ParticleSystem>().main;
            mainModule2.startColor = new Color(150f / 255f, 1f, 1f, 1f);
        }
    }

    public void ReleaseEffect(UnitController unit)
    {
        Destroy(unit.transform.GetChild(1).gameObject);

        switch (unit.AbsorUnitType)
        {
            case UnitType.huhyou:
                newColor = new Color(191f / 255f, 189f / 255f, 182f / 255f, 1f);
                break;
            case UnitType.kakugyou:
                newColor = new Color(1f, 155f / 255f, 0f, 1f);
                break;
            case UnitType.hisya:
                newColor = new Color(0f, 6f / 255f, 191f / 255f, 1f);
                break;
            case UnitType.kyousya:
                newColor = new Color(191f / 255f, 0f, 4f / 255f, 1f);
                break;
            case UnitType.keima:
                newColor = new Color(0f / 255f, 191f / 255f, 2f / 255f, 1f);
                break;
            case UnitType.ginsyou:
                newColor = new Color(191f / 255f, 12f / 255f, 104f / 255f, 1f);
                break;
            case UnitType.kinsyou:
                newColor = new Color(191f / 255f, 145f / 255f, 0f, 1f);
                break;
        }

        Vector3 pos = unit.transform.position;
        GameObject releaseeffect = Instantiate(releaseEffect, pos, Quaternion.identity);
        GameObject child = releaseeffect.transform.GetChild(0).gameObject;
        var mainModule = child.GetComponent<ParticleSystem>().main;
        mainModule.startColor = newColor;
        Destroy(releaseeffect, 2.0f);
    }
    public void FillingEffectDestroy()
    {
        for (int i = 0; i < absorptionHistory.Count; i++)
        {
            UnitController unitController = absorptionHistory[i].gameObject.GetComponent<UnitController>();
            if (unitController.fillingCheck && !unitController.ReleseCheck)
            {
                if (Turn >= unitController.absorTurn + 2)
                {
                    unitController.fillingCheck = true;
                    if (absorptionHistory[i].gameObject.transform.childCount > 1)
                    {
                        GameObject FillingEffect = absorptionHistory[i].gameObject.transform.GetChild(1).gameObject;
                        UnitType unitType = unitController.AbsorUnitType;

                        // UnitType�ɉ������J���[�̑I��
                        switch (unitType)
                        {
                            case UnitType.huhyou:
                                newColor = new Color(191f / 255f, 189f / 255f, 182f / 255f, 1f);
                                newMaterial = newMaterials[0];
                                break;
                            case UnitType.kakugyou:
                                newColor = new Color(1f, 155f / 255f, 0f, 1f);
                                newMaterial = newMaterials[1];
                                break;
                            case UnitType.hisya:
                                newColor = new Color(0f, 6f / 255f, 191f / 255f, 1f);
                                newMaterial = newMaterials[2];
                                break;
                            case UnitType.kyousya:
                                newColor = new Color(191f / 255f, 0f, 4f / 255f, 1f);
                                newMaterial = newMaterials[3];
                                break;
                            case UnitType.keima:
                                newColor = new Color(0f / 255f, 191f / 255f, 2f / 255f, 1f);
                                newMaterial = newMaterials[4];
                                break;
                            case UnitType.ginsyou:
                                newColor = new Color(191f / 255f, 12f / 255f, 104f / 255f, 1f);
                                newMaterial = newMaterials[5];
                                break;
                            case UnitType.kinsyou:
                                newColor = new Color(191f / 255f, 145f / 255f, 0f, 1f);
                                newMaterial = newMaterials[6];
                                break;
                            default:
                                newColor = Color.clear; // �f�t�H���g�œ����i��O�I�ȃP�[�X���l���j
                                break;

                        }


                        Vector3 pos = FillingEffect.transform.position;
                        GameObject FilledEffect = Instantiate(filledEffect, pos, FillingEffect.transform.rotation);
                        FilledEffect.transform.parent = FillingEffect.transform.parent;

                        // �q�I�u�W�F�N�g�̃p�[�e�B�N���V�X�e���̐F��ݒ�
                        foreach (Transform child in FilledEffect.transform)
                        {
                            //child.GetComponent<ParticleSystem>().startColor = newColor;
                            var mainModule = child.GetComponent<ParticleSystem>().main;
                            mainModule.startColor = newColor;
                        }

                        FilledEffect.transform.parent.GetComponent<MeshRenderer>().material = newMaterial;
                        Destroy(FillingEffect);
                        //InstantLoss(unitController);
                    }

                }
                continue; // �X�L�b�v���Ď��̗v�f���m�F
            }
            //else           
            //    break; // ���[�v���I��
        }
    }

    public void InstantLoss(UnitController unit)
    {
        if (Turn >= unit.absorTurn + 2 && ( unit.UnitType == UnitType.hisya || unit.UnitType == UnitType.ryuuou) && (unit.AbsorUnitType == UnitType.huhyou || unit.AbsorUnitType == UnitType.kyousya))
        {
            Debug.Log("��Ԃ����������Ԃ��z�����ď[�U������B");
            if (unit.AbsorUnitType != UnitType.None)
            {
                if (unit.AbsorUnitPlayer != currentPlayer)
                {
                    absorptionPieceManager.AddAbsorptionPiece(unit.AbsorUnitType, 1 - currentPlayer);
                }
                else
                {
                    capturedPieceManager.AddCapturedPiece(unit.AbsorUnitType, 1 - currentPlayer);
                }
            }
            unit.ReleseCheck = true;
            unit.gameObject.GetComponent<MeshRenderer>().material = BaseMaterial;
            Destroy(unit.transform.GetChild(1).gameObject);
            Destroy(unit.transform.GetChild(2).gameObject);
        }
    }
    public void quitgame(int QuitPlayer)
    {
        string winner;
        if (QuitPlayer == 0)
        {
            winner = "Player2";
        }
        else if (QuitPlayer == 1)
        {
            winner = "Player1";
        }
        else
        {
            return;
        }
        StartCoroutine(EndGame(winner));
    }

    public void ActivateText()
    {
        gametime.isPaused = true;
        DisableInput();

        // �����v���C���[�̃^�[��
        if (currentPlayer == 0)
        {
            TurnText[0].SetActive(true); // Player1
            TurnText[0].GetComponentInChildren<TextMeshProUGUI>().text = "���Ȃ��̃^�[��";
            TurnText[1].SetActive(true); // Player2
            TurnText[1].GetComponentInChildren<TextMeshProUGUI>().text = "����̃^�[��";

            TurnText_always[0].GetComponentInChildren<TextMeshProUGUI>().text = "���Ȃ��̃^�[��"; // Player1
            TurnText_always[1].GetComponentInChildren<TextMeshProUGUI>().text = "����̃^�[��"; // Player2
        }
        // �E���v���C���[�̃^�[��
        if (currentPlayer == 1)
        {
            TurnText[0].SetActive(true); //Player1
            TurnText[0].GetComponentInChildren<TextMeshProUGUI>().text = "����̃^�[��";
            TurnText[1].SetActive(true); //Player2
            TurnText[1].GetComponentInChildren<TextMeshProUGUI>().text = "���Ȃ��̃^�[��";

            TurnText_always[0].GetComponentInChildren<TextMeshProUGUI>().text = "����̃^�[��"; //Player1
            TurnText_always[1].GetComponentInChildren<TextMeshProUGUI>().text = "���Ȃ��̃^�[��"; //Player2
        }
        Invoke("DeactivateText", 1.5f);
    }

    
    //-------�����Ă͍s���Ȃ�
    //����G�t�F�N�g
    private void DeactivateTitleTimeHighlights()
    {
        foreach (GameObject highlight in OuteEffect)
        {
            highlight.gameObject.SetActive(false);
        }
            EnableInput();
            gametime.isPaused = false;

    }

    //�z���G�t�F�N�g
    private void AbsorEffectDestroy()
    {
        if (currentAbsorEffect != null)
        {
            Destroy(currentAbsorEffect);
        }
        EnableInput();
        gametime.isPaused = false;
    }

    //�^�[���\��
    private void DeactivateText()
    {
        foreach (GameObject text in TurnText)
        {
            text.SetActive(false);
        }
        gametime.isPaused = false;
        EnableInput();
    }
    //-------�����Ă͍s���Ȃ�
}