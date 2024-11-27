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
    //-------ゲーム設定開始--------
    //シーン遷移
    public string sceneName;
    //シーン遷移終了
    private float TotalElapsedTime; // 試合の経過時間
    private bool isGameActive = false; // ゲームが進行中かどうかのフラグ

    const int PlayerMax = 2; // プレイヤー数
    int currentPlayer;// 現在のプレイヤー (0が味方、1が敵)
    [SerializeField, Tooltip("最初の手番をランダムにするかどうか")]
    bool randomizeStartingPlayer= false;

    [SerializeField] private bool turnChange = true; // ターン交代のフラグ
    public int[] playerMoveCounts = new int[PlayerMax];// 2人のプレイヤー分の手のカウント
    public int TotalTime = 0;

    int kingMoveCount = 0;// 王が動かなかった回数
    public bool isKingInCheck = false;// 王手かどうかのフラグ
    [SerializeField] Vector2Int Player0KingPos; // 王手がかかったときの王将の位置
    [SerializeField] Vector2Int Player1KingPos;　// 王手がかかったときの王将の位置
    int maxMoveCount = 1;// 王が動かなかった場合の最大カウント
    [SerializeField, Header("王手Image")]
    public GameObject[] OuteEffect; // 光るエフェクト用のUIオブジェクトの配列
    private bool isCheckTriggeredNextTurn = false; // 王手状態フラグを追加
    private bool startOuteNextTurnDisabled = false; // 無効化フラグ

    int boardWidth;
    int boardHeight;

    public Material BaseMaterial;  // 元の枠の色
    public Material HoheiMaterial;  // 歩兵の文字マテリアル

    private bool isInputDisabled = false;// 入力無効フラグ

    private AudioClip SE;

    public void DisableInput()
    { isInputDisabled = true; }// 入力を無効にする

    public void EnableInput()
    { isInputDisabled = false; }// 入力を有効に戻す

    [SerializeField] private GameTime gametime;  // ゲームのプレイ時間
    [SerializeField] GameObject PrefabTile; // タイルのプレハブ化
    [SerializeField] List<GameObject> PrefabUnits; // 駒のプレハブ化
    private Player2Handicap handicap2; //駒落ち
    private Player1Handicap handicap1;

    int[,] boardsetting =  // 駒の初期配置
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

        // デバッグ作業用
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

    //駒落ち
    private void ApplyHandicap()
    {
        int handicapIndex2 = PlayerPrefs.GetInt("HandicapSetting2", 0); // デフォルトは「なし」
        int handicapIndex1 = PlayerPrefs.GetInt("HandicapSetting1", 0); // デフォルトは「なし」
        string handicapPositions2 = PlayerPrefs.GetString("HandicapPositions2", ""); // デフォルトは空文字列
        string handicapPositions1 = PlayerPrefs.GetString("HandicapPositions1", ""); // デフォルトは空文字列

        // 駒落ち設定がある場合、位置情報を取得して反映
        if (!string.IsNullOrEmpty(handicapPositions2) || !string.IsNullOrEmpty(handicapPositions1))
        {
            var positions2 = ParsePositions(handicapPositions2);
            var positions1 = ParsePositions(handicapPositions1);
            foreach (var (row, col) in positions2)
            {
                boardsetting[row, col] = 0; // データとしての駒を削除

                // 駒のGameObjectを削除
                if (units[row, col] != null)
                {
                    Destroy(units[row, col].gameObject);
                    units[row, col] = null;
                }
            }
            foreach (var (row, col) in positions1)
            {
                boardsetting[row, col] = 0; // データとしての駒を削除

                // 駒のGameObjectを削除
                if (units[row, col] != null)
                {
                    Destroy(units[row, col].gameObject);
                    units[row, col] = null;
                }
            }
            Debug.Log("駒落ち設定が反映されました2" + handicapIndex2 + ", 設定内容 " + handicapPositions2);
            Debug.Log("駒落ち設定が反映されました1" + handicapIndex1 + ", 設定内容 " + handicapPositions1);
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
    //-------ゲーム設定終わり--------


    //-------駒の管理開始--------
    private Dictionary<UnitType, int> capturedUnits;// 取った駒の種類と数を管理するための辞書

    [SerializeField] private PromoteManager promoteManager; // 成りの管理
    [SerializeField] private CapturedPieceManager capturedPieceManager; // 取る処理の管理
    [SerializeField] private AbsorptionPieceManager absorptionPieceManager;

    public UnitController selectUnit; // 選択中の駒
    public UnitController GetSelectUnit()//GetSelectUnitメソッドを追加
    {
        return selectUnit;  // 現在選択されているユニットを返す
    }
    public void ClearSelectUnit()//ClearSelectUnitメソッドを追加
    {
        selectUnit = null;  // 選択を解除
    }
    public List<UnitController> absorptionHistory = new List<UnitController>();
    private UnitController currentReleaseUnit;
    private UnitController selectedReleaseUnit; // 選択中の放出駒

    Dictionary<GameObject, Vector2Int> movableTiles;// 移動可能タイル
    Dictionary<GameObject, Vector2Int> absorbableTiles;// 吸収可能タイル
    Dictionary<GameObject, Vector2Int> relesableTiles;// 放出可能タイル

    [SerializeField] GameObject prefabCursor;// 移動範囲のプレハブ
    [SerializeField] GameObject absorption;// 吸収範囲のプレハブ
    [SerializeField] GameObject releseCursor;// 放出可能のプレハブ

    List<GameObject> cursors;// 移動範囲のオブジェクト
    List<GameObject> absorptioncursors;// 吸収範囲オブジェクト
    List<GameObject> relesecursors;// 放出範囲オブジェクト

    [SerializeField] GameObject absorEffect; // エフェクトのプレハブ
    [SerializeField] GameObject fillingEffect;
    [SerializeField] GameObject filledEffect;
    private Color newColor;
    private Material newMaterial;
    [SerializeField] List<Material> newMaterials;
    [SerializeField] GameObject releaseEffect;
    private GameObject currentAbsorEffect; // インスタンス化したエフェクトの参照
    public bool skipActivateText = false;

    public UnitController absorUnit;
    public int Turn;// 経過ターン
    public int absorTurn;// 吸収したターン
    //-------駒の管理終わり--------


    //-------カメラ設定開始--------
    public Camera player1Camera;
    public Camera player2Camera;
    public int totalWidth = 1920;  // 全画面幅
    public int totalHeight = 540;  // 高さ
    public int playerWidth = 960;  // 各プレイヤーの幅
    public int playerHeight = 540;  // 高さ
    //-------カメラ設定終わり--------


    //-------ゲームの初期化処理開始--------
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
        isGameActive = true; // ゲーム進行中フラグを立てる
        ApplyHandicap(); //駒落ち
        // ゲーム開始時に OuteEffect の各オブジェクトを非アクティブにする
        foreach (GameObject highlight in OuteEffect)
        {
            highlight.SetActive(false);
        }

        foreach (GameObject text in TurnText)
        {
            text.SetActive(false);
        }

        // ゲーム開始時の表示
        if (currentPlayer == 0)
        {
            TurnText_always[0].GetComponentInChildren<TextMeshProUGUI>().text = "あなたのターン"; // Player1
            TurnText_always[1].GetComponentInChildren<TextMeshProUGUI>().text = "相手のターン"; // Player2
        }
        if (currentPlayer == 1)
        {
            TurnText_always[0].GetComponentInChildren<TextMeshProUGUI>().text = "相手のターン"; //Player1
            TurnText_always[1].GetComponentInChildren<TextMeshProUGUI>().text = "あなたのターン"; //Player2
        }
        ActivateText();
        Invoke("DeactivateText", 1.5f);
    }

    void InitializeCameras() // カメラ
    {
        player1Camera = GameObject.Find("Player1").GetComponent<Camera>();
        player2Camera = GameObject.Find("Player2").GetComponent<Camera>();
        player1Camera.rect = new Rect(0, 0, 0.5f, 1);
        player2Camera.rect = new Rect(0.5f, 0, 0.5f, 1);
    }

    void InitializeBoard() // ボード
    {
        if (randomizeStartingPlayer) { currentPlayer = UnityEngine.Random.Range(0, 2);}//ランダムターン
        else { currentPlayer = 0; }
        boardWidth = boardsetting.GetLength(0);// ボードのサイズ
        boardHeight = boardsetting.GetLength(1);
        tiles = new Dictionary<Vector2Int, GameObject>();// フィールド初期化
        units = new UnitController[boardWidth, boardHeight];
        movableTiles = new Dictionary<GameObject, Vector2Int>();// 移動可能距離
        cursors = new List<GameObject>();
        absorbableTiles = new Dictionary<GameObject, Vector2Int>(); // 吸収可能距離
        absorptioncursors = new List<GameObject>();
        relesableTiles = new Dictionary<GameObject, Vector2Int>();// 放出可能距離
        relesecursors = new List<GameObject>();
    }

    void InitializeUnits() //駒
    {
        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardHeight; j++)
            {
                CreateTileAndUnit(i, j);
            }
        }
        capturedUnits = new Dictionary<UnitType, int>();
        // すべての種類の駒を0で初期化
        foreach (UnitType type in Enum.GetValues(typeof(UnitType)))
        {
            if (type != UnitType.None)
                capturedUnits[type] = 0;
        }
    }

    void CreateTileAndUnit(int i, int j) // 駒と盤の生成
    {
        float tileSpacingX = 0.5f, tileSpacingZ = 0.6f;// タイルとユニットのポジション
        float x = (i - boardWidth / 2) * tileSpacingX;
        float y = (j - boardHeight / 2) * tileSpacingZ;
        Vector3 pos = new Vector3(x, 0, y);
        GameObject tile = Instantiate(PrefabTile, pos, Quaternion.identity); // タイル作成
        Vector2Int idx = new Vector2Int(i, j);// タイルのインデックス
        tiles.Add(idx, tile);
        movableTiles.Add(tile, idx);// 移動可能距離を仮で決める

        int type = boardsetting[i, j] % 10; // 駒作成
        int player = boardsetting[i, j] / 10;
        if (type == 0) return;

        pos.y = 0.7f;
        GameObject prefab = PrefabUnits[type - 1];
        GameObject unit = Instantiate(prefab, pos, Quaternion.Euler(-90, player * 180, 0));
        UnitController unitctrl = unit.GetComponent<UnitController>();
        unitctrl.Init(player, type, tile, idx);
        units[i, j] = unitctrl;
    }
    //-------ゲームの初期化処理終わり--------


    //-------ターン制処理開始--------
    void Update()
    {
        if (isInputDisabled) return;
        HandlePlayerInput();
        if (Input.GetMouseButtonUp(1)){ DeselectBoardPieces();capturedPieceManager.DeselectPiece(); absorptionPieceManager.AbsorDeselectPiece(); }
        if (isGameActive){ TotalElapsedTime += Time.deltaTime;}
    }

    void HandlePlayerInput()　// プレイヤー制御
    {
        if (Input.GetMouseButtonUp(0))
        {
            Vector3 mousePos = Input.mousePosition;
            Ray ray = (currentPlayer == 0 ? player1Camera : player2Camera).ScreenPointToRay(mousePos);
            HandleRaycast(ray);
        }
    }

    void HandleRaycast(Ray ray) // レイの制御
    {
        GameObject tile = null;
        UnitController unit = null;

        foreach (RaycastHit hit in Physics.RaycastAll(ray))
        {
            UnitController hitunit = hit.transform.GetComponent<UnitController>();

            if (hitunit != null && hitunit.Player != currentPlayer)
            {
                //Debug.Log("相手のターンのため、駒を選択できません");
                return;
            }
            if (hitunit != null && FieldStatus.Captured == hitunit.FieldStatus)
            {
                unit = hitunit;
            }
            else if (tiles.ContainsValue(hit.transform.gameObject))
            {
                tile = hit.transform.gameObject;
                // タイルから他ユニットを探す
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

    void HandleTileSelection(GameObject tile, UnitController unit) // タイル選択の制御
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
                    //Debug.Log("その場所にはすでに駒があります。");
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
                    //Debug.Log("その場所にはすでに駒があります。");
                }
            }
        }
        else if (tile && absorptionHistory.Contains(unit))
        {
            // 選択した駒が自分の駒かどうかを判別
            if (unit.Player == currentPlayer)
            {
                // 自分の駒の場合にのみ設定
                selectedReleaseUnit = unit;
                Debug.Log("放出する駒を選択しました: " + selectedReleaseUnit.name);
            }
            else
            {
                Debug.Log("自分の駒ではありません。放出する駒として選択できません。");
            }
        }
        int CurrentTurn = currentPlayer;

        // 何も選択されていなければ処理をしない
        if (tile && selectUnit && movableTiles.ContainsKey(tile))
        {
            movableUnit(selectUnit, movableTiles[tile]);
            GetComponent<AudioSource>().Play();
            selectedReleaseUnit = null;
            EndTurn();
        }
        // 盤面のタイルが選択されたか、持ち駒が選択されているか確認
        if (tile != null && capturedPieceManager.GetSelectedPieceType() != UnitType.None)
        {
            Vector2Int selectedTileIndex = movableTiles[tile];
            PlacePieceFromCaptured(selectedTileIndex); // 持ち駒を盤面に配置
        }

        if (tile != null && absorptionPieceManager.GetSelectedPieceType() != UnitType.None)
        {
            Vector2Int selectedTileIndex = movableTiles[tile];
            PlacePieceFromAbsorption(selectedTileIndex); // 持ち駒を盤面に配置
        }

        // 吸収カーソルの選択
        else if (tile && selectUnit && absorbableTiles.ContainsKey(tile))
        {
            absorbableUnit(selectUnit, absorbableTiles[tile]);
        }

        // 放出カーソルの選択
        if (tile && selectedReleaseUnit != null && relesableTiles.ContainsKey(tile))
        {
            Debug.Log("放出カーソルをクリックしました。駒を移動します: " + selectedReleaseUnit.name);

            // 駒の移動処理
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

            // 選択状態解除
            selectedReleaseUnit = null;
            EndTurn();
        }
        // ユニットを選択
        if (null != unit)
        {
            selectCursors(unit);
        }
    }
    //-------ターン制処理終わり--------


    //-------ユニット操作処理開始--------
    void movableUnit(UnitController unit, Vector2Int tileindex)
    {
        if (unit == null || !tiles.ContainsKey(tileindex)) return;

        Vector2Int oldpos = unit.Pos;
        UnitController targetUnit = units[tileindex.x, tileindex.y];
        // 相手の駒を取る処理
        if (targetUnit != null && targetUnit.Player != unit.Player)
        {
            // 防御効果を確認し、有効なら攻撃を無効化
            if (targetUnit.hasTemporaryDefense && targetUnit.fillingCheck)
            {
                targetUnit.hasTemporaryDefense = false;  // 防御効果を消費
                Debug.Log($"{targetUnit.UnitType} の防御効果が発動し、攻撃が無効化されました！");
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
                return;  // 駒取りをスキップ
            }
            AddCapturedUnit(targetUnit.UnitType);
            CaptureAndDemote(targetUnit);
            if (targetUnit.absorptionCheck && targetUnit.AbsorUnitType != UnitType.None)
            {
                if (targetUnit.AbsorUnitPlayer == currentPlayer) // 自分の駒を吸収していた場合
                {
                    capturedPieceManager.AddCapturedPiece(targetUnit.AbsorUnitType, targetUnit.Player);
                }
                else // 相手の駒を吸収していた場合
                {
                    absorptionPieceManager.AddAbsorptionPiece(targetUnit.AbsorUnitType, targetUnit.Player);
                }

                absorptionHistory.Remove(targetUnit);
            }
            //Destroy(targetUnit.gameObject);
        }
        // ユニットの移動
        units[oldpos.x, oldpos.y] = null;
        unit.Move(tiles[tileindex], tileindex);
        units[tileindex.x, tileindex.y] = unit;

        ClearCursors();
        // 成り判定
        if (ShouldPromote(unit, tileindex, oldpos))
        {
            ClearCursors();
            ShowPromoteOptions(unit, currentPlayer);// 成りオプションを表示
            startOuteNextTurnDisabled = true; // プロモート判定が発生した場合、`StartOuteNextTurn`を無効化
            //oute(unit, tileindex);
            return;// 成りボタンが表示されているのでターンは切り替えない
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
                absorUnit.hasTemporaryDefense = false;  // 防御効果を消費
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
            // absorbableUnitメソッドの中、吸収が確定した後に追加
            if ((unit.UnitType == UnitType.hisya && (absorUnit.UnitType == UnitType.huhyou || absorUnit.UnitType == UnitType.kyousya)) ||
                    (unit.UnitType == UnitType.kyousya && (absorUnit.UnitType == UnitType.huhyou || absorUnit.UnitType == UnitType.kyousya))||
                    (unit.UnitType == UnitType.keima && absorUnit.UnitType == UnitType.keima))

            //if ((unit.UnitType == UnitType.hisya && (absorUnit.UnitType == UnitType.huhyou || absorUnit.UnitType == UnitType.kyousya)) ||
            //(unit.UnitType == UnitType.kyousya && absorUnit.UnitType == UnitType.huhyou))
            {
                // 吸収駒に一度だけの防御効果を付与
                unit.hasTemporaryDefense = true;
                Debug.Log($"{unit.UnitType} に一度だけの防御効果が付与されました");
            }
            unit.absorptionCheck = true;
            ClearCursors();
            unit.AbsorUnitPlayer = absorUnit.Player;
            // 吸収された駒の元プレイヤーを比較して駒台に送る
            if (absorUnit.AbsorUnitType != UnitType.None)
            {
                if (absorUnit.Player == currentPlayer)
                {
                    // 吸収された駒が自分の駒の場合
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
                    // 吸収された駒が相手の駒の場合
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
    //駒の状態を更新するメソッド
    public void UpdateUnitPosition(UnitController unit)
    {
        if (unit == null)
        {
            //Debug.LogError("UpdateUnitPositionメソッドでunitがnullです。");
            return;
        }

        Vector2Int pos = unit.Pos;
        if (pos.x < 0 || pos.x >= boardWidth || pos.y < 0 || pos.y >= boardHeight)
        {
            //Debug.LogError("UpdateUnitPositionメソッドで無効なポジションが指定されました。");
            return;
        }

        // 駒を新しい位置に登録
        units[pos.x, pos.y] = unit;
        //Debug.Log($"{unit.UnitType} を位置 {pos} に再登録しました。");
    }

    private void CaptureAndDemote(UnitController unit)
    {
        UnitType originalType = unit.OldUnitType;  // 元の駒の種類を取得
        unit.Demote(); // 成り状態を解除して元の駒に戻す
        capturedPieceManager.AddCapturedPiece(originalType, currentPlayer);// 持ち駒として元の駒を追加
        Destroy(unit.gameObject);// 捕獲された駒を無効化
    }

    public bool ShouldPromote(UnitController unit, Vector2Int newPos, Vector2Int oldpos)
    {
        bool wasInOpponentTerritory = (unit.Player == 0 && oldpos.y <= 2) || (unit.Player == 1 && oldpos.y >= 6);
        bool isInOpponentTerritory = (unit.Player == 0 && newPos.y <= 2) || (unit.Player == 1 && newPos.y >= 6);
        return (unit.UnitType != UnitType.narigoma && unit.UnitType != UnitType.kinsyou && unit.UnitType != UnitType.ryuuou && unit.UnitType != UnitType.ryuuma) && (isInOpponentTerritory || wasInOpponentTerritory);
    }
    //-------ユニット操作処理終わり--------

    //-------カーソル処理開始--------
    List<Vector2Int> getMovableTiles(UnitController unit)// 移動可能範囲を取得
    {
        List<Vector2Int> ret = unit.GetMovableTiles(units); // 通常範囲外
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

        // 通常の充填が完了している場合、放出マスを追加
        if (unit.fillingCheck)
        {
            movableTiles.AddRange(releaseTiles);
        }
        return movableTiles;
    }

    void selectCursors(UnitController unit = null)
    {
        // 既存のカーソルを削除
        ClearCursors();

        // 選択中のユニットを非選択状態に戻す
        if (selectUnit != null)
        {
            selectUnit.Selected(false);
            selectUnit = null;
        }
        if (unit.Player != currentPlayer)
        {
            selectedReleaseUnit = null;
        }
        // ユニットが存在しない場合は終了
        if (unit == null) return;

        // 自分のターンかどうかの確認
        // 自分のターンでない場合、自分のユニットを選択できないようにする
        if (turnChange && unit.Player != currentPlayer)
        {
            //Debug.Log("自分のターンではありません");
            return; // 自分の駒でない場合や自分のターンでない場合は処理しない
        }
        // 選択した駒をcurrentReleaseUnitとして設定
        currentReleaseUnit = unit;
        // 移動可能範囲を取得
        List<Vector2Int> movabletiles = getMovableTiles(unit);
        movableTiles.Clear();

        // 吸収可能範囲を取得
        List<Vector2Int> absorbabletiles = getAbsorbableTiles(unit);
        absorbableTiles.Clear();

        // 放出可能範囲を取得
        List<Vector2Int> relesabletiles = getRelesableTiles(unit);
        relesableTiles.Clear();

        foreach (var tile in movabletiles)
        {
            movableTiles.Add(tiles[tile], tile);

            // カーソルを表示
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

        //吸収のcursor
        //if (!unit.absorptionCheck) // 一度も吸収していなかったら
        //if (!unit.absorptionCheck && (unit.PutTurn == 0 || Turn > unit.PutTurn + 2)) // ＋駒台から置いて1ターン経ったら
        if (!unit.absorptionCheck && (( unit.PutUnitCheck && unit.Movecount >= 1) || !unit.PutUnitCheck)) // ＋駒台から置いて置いた駒が動いたら
        {
            foreach (var tile in absorbabletiles)
            {
                absorbableTiles.Add(tiles[tile], tile);

                // カーソルを表示
                Vector3 pos = tiles[tile].transform.position;
                pos.y -= 0.01f;
                GameObject absorcursor = Instantiate(absorption, pos, Quaternion.identity);
                absorptioncursors.Add(absorcursor);
            }
        }

        // 放出のカーソル
        //unit.FillingCheck(unit); // 充填チェック

        if (!unit.ReleseCheck && unit.fillingCheck)
        {
            foreach (var tile in relesabletiles)
            {
                // キーの存在を確認してから追加
                if (!relesableTiles.ContainsKey(tiles[tile]))
                {
                    relesableTiles.Add(tiles[tile], tile);

                    // カーソルを表示
                    Vector3 pos = tiles[tile].transform.position;
                    pos.y -= 0.01f;
                    GameObject relesecursor = Instantiate(releseCursor, pos, Quaternion.identity);
                    relesecursors.Add(relesecursor);
                }
            }
        }


        // 選択状態にする
        unit.Selected();
        selectUnit = unit;

        //王手チェック
        outecheck(unit);
    }

    public void DisplayPlaceableCursors()
    {
        ClearCursors();  // 既存のカーソルをクリア

        foreach (var tile in tiles)
        {
            Vector2Int position = tile.Key;

            // 二歩チェックを追加: 自分の歩がすでにある列にはカーソルを表示しない
            if (capturedPieceManager.GetSelectedPieceType() == UnitType.huhyou && IsNifu(UnitType.huhyou, position.x))
            {
                continue;  // 二歩が発生する場所にはカーソルを表示しない
            }

            // 持ち駒が置ける場所かチェック
            if (CanPlacePiece(capturedPieceManager.GetSelectedPieceType(), position))
            {
                Vector3 pos = tile.Value.transform.position;
                pos.y += 0.05f;  // 少し浮かせて表示

                GameObject cursor = Instantiate(prefabCursor, pos, Quaternion.identity);
                cursors.Add(cursor);

                // タイルを movableTiles に追加
                movableTiles[tile.Value] = position;
            }
        }
    }

    public void DisplayPlaceableCursorsAbsor()
    {
        ClearCursors();  // 既存のカーソルをクリア

        foreach (var tile in tiles)
        {
            Vector2Int position = tile.Key;

            // 二歩チェックを追加: 自分の歩がすでにある列にはカーソルを表示しない
            if (absorptionPieceManager.GetSelectedPieceType() == UnitType.huhyou && IsNifu(UnitType.huhyou, position.x))
            {
                continue;
            }

            // 持ち駒が置ける場所かチェック
            if (CanPlaceAbsorPiece(absorptionPieceManager.GetSelectedPieceType(), position))
            {
                Vector3 pos = tile.Value.transform.position;
                pos.y += 0.05f;  // 少し浮かせて表示

                GameObject cursor = Instantiate(prefabCursor, pos, Quaternion.identity);
                cursors.Add(cursor);

                // タイルを movableTiles に追加
                movableTiles[tile.Value] = position;
            }
        }
    }

    // カーソルをクリアするコルーチン
    private IEnumerator ClearCursorsWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);  // 指定した秒数待つ
        ClearCursors();  // カーソルをクリア
    }
    //吸収のカーソルをクリアするコルーチン
    IEnumerator AbsorptionDelay(UnitController unit, Vector2Int tileindex)
    {
        yield return new WaitForSeconds(0.01f);

        ClearCursors(); // カーソルクリア

        // 吸収後の処理
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

    // 名前でオブジェクトを削除するユーティリティメソッド
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
    //-------カーソル処理終わり--------

    //-------タイルリセット処理開始--------
    private void ResetTileStates()
    {
        movableTiles.Clear();
        absorbableTiles.Clear();
        relesableTiles.Clear();
    }
    //-------タイルリセット処理開始--------

    //-------持ち駒処理開始--------
    public bool CanPlacePiece(UnitType type, Vector2Int position)
    {
        // 位置が空いているかを確認
        if (units[position.x, position.y] != null)
        {
            //Debug.Log("その場所には既に駒があります。");
            return false;
        }
        // 特定の駒の配置制限
        int boardSizeY = units.GetLength(1);
        int enemyTerritoryY = (currentPlayer == 0) ? boardSizeY - 9 : 8;
        int enemyTerritoryYMinusOne = (currentPlayer == 0) ? boardSizeY - 8 : 7;
        // 歩兵または香車を敵陣一番奥に置けない
        if ((type == UnitType.huhyou || type == UnitType.kyousya) && position.y == enemyTerritoryY)
        {
            //Debug.Log($"{type} は敵陣の一番奥に置けません。");
            return false;
        }
        // 桂馬を敵陣一番奥およびその手前に置けない
        if (type == UnitType.keima && (position.y == enemyTerritoryY || position.y == enemyTerritoryYMinusOne))
        {
            //Debug.Log("桂馬は敵陣の一番奥またはその手前に置けません。");
            return false;
        }

        return true;
    }

    public bool CanPlaceAbsorPiece(UnitType type, Vector2Int position)
    {
        // 位置が空いているかを確認
        if (units[position.x, position.y] != null)
        {
            //Debug.Log("その場所には既に駒があります。");
            return false;
        }

        // 特定の駒の配置制限
        bool dontPutArea = ((currentPlayer == 0 && position.y <= 5) || (currentPlayer == 1 && position.y >= 3));
        // 自陣にしか置けない
        if (dontPutArea)
        {
            //Debug.Log("自陣にしか置けません");
            return false;
        }
        return true;
    }

    //二歩のチェック
    private bool IsNifu(UnitType pieceType, int column)
    {
        // 歩以外の駒の場合は二歩のチェックを行わない
        if (pieceType != UnitType.huhyou)
        {
            return false;
        }

        // 盤面上の駒を確認
        for (int y = 0; y < boardHeight; y++)
        {
            UnitController unit = units[column, y];
            // 自分の歩が既にある場合は二歩
            if (unit != null && unit.Player == currentPlayer && unit.UnitType == UnitType.huhyou)
            {
                return true;  // 二歩になる
            }
        }

        return false;  // 二歩ではない
    }

    private void PlacePieceFromCaptured(Vector2Int position)
    {
        // 二歩チェックを追加
        if (capturedPieceManager.GetSelectedPieceType() == UnitType.huhyou && IsNifu(UnitType.huhyou, position.x))
        {
            //Debug.Log("二歩はできません。");
            return;  // 二歩なので駒を置けず、ターンも終了しない
        }

        // 通常の駒配置処理
        if (!CanPlacePiece(capturedPieceManager.GetSelectedPieceType(), position))
        {
            //Debug.Log("その場所には駒を置けません。");
            return;
        }

        // 駒のプレファブを取得して生成
        GameObject piecePrefab = PrefabUnits[(int)capturedPieceManager.GetSelectedPieceType() - 1];
        if (piecePrefab == null)
        {
            //Debug.LogError("駒のプレファブが見つかりません。");
            return;
        }

        // 駒を生成し、回転を適用
        Quaternion rotation = Quaternion.Euler(-90, currentPlayer * 180, 0);
        GameObject newPiece = Instantiate(piecePrefab, GetTile(position).transform.position, rotation);
        UnitController unitController = newPiece.GetComponent<UnitController>();
        unitController.Init(currentPlayer, (int)capturedPieceManager.GetSelectedPieceType(), GetTile(position), position);
        GetComponent<AudioSource>().Play();

        unitController.PutTurn = Turn;
        unitController.PutUnitCheck = true;

        // ユニットを盤面にセット
        units[position.x, position.y] = unitController;

        // 駒台の持ち駒数を減らす処理
        capturedPieceManager.ReduceCapturedPieceCount(capturedPieceManager.GetSelectedPieceType(), currentPlayer);

        // 選択解除とカーソルクリア
        capturedPieceManager.ClearSelectedPiece();
        ClearCursors();

        // ターン終了処理
        EndTurn();  // ターンが終わるのはこのタイミングだけ
    }

    private void PlacePieceFromAbsorption(Vector2Int position)
    {
        // 二歩チェックを追加
        if (absorptionPieceManager.GetSelectedPieceType() == UnitType.huhyou && IsNifu(UnitType.huhyou, position.x))
        {
            //Debug.Log("二歩はできません。");
            return;  // 二歩なので駒を置けず、ターンも終了しない
        }

        if (!CanPlaceAbsorPiece(absorptionPieceManager.GetSelectedPieceType(), position))
        {
            //Debug.Log("その場所には駒を置けません。");
            return;
        }

        GameObject piecePrefab = PrefabUnits[(int)absorptionPieceManager.GetSelectedPieceType() - 1];
        if (piecePrefab == null)
        {
            //Debug.LogError("駒のプレファブが見つかりません。");
            return;
        }

        // 駒を生成し、回転を適用
        Quaternion rotation = Quaternion.Euler(-90, currentPlayer * 180, 0);
        GameObject newPiece = Instantiate(piecePrefab, GetTile(position).transform.position, rotation);
        UnitController unitController = newPiece.GetComponent<UnitController>();
        unitController.Init(currentPlayer, (int)absorptionPieceManager.GetSelectedPieceType(), GetTile(position), position);
        GetComponent<AudioSource>().Play();

        unitController.PutTurn = Turn;
        unitController.PutUnitCheck = true;


        // ユニットを盤面にセット
        units[position.x, position.y] = unitController;

        // 駒台の持ち駒数を減らす処理
        absorptionPieceManager.ReduceAbsorPieceCount(absorptionPieceManager.GetSelectedPieceType(), currentPlayer);

        // 選択解除とカーソルクリア
        absorptionPieceManager.ClearSelectedPiece();
        ClearCursors();

        // ターン終了処理
        EndTurn();  // ターンが終わるのはこのタイミングだけ
    }
    //-------持ち駒処理終わり--------

    //-----王手判定-----
    //王手された時の王の座標 Player0KingPos Player1KingPos
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
        // 駒の移動範囲を取得。充填が完了している場合のみ放出マスを含める
        List<Vector2Int> outeTiles = unit.fillingCheck ? getMovableTilesWithRelease(unit) : getMovableTiles(unit);

        Debug.Log($"駒 {unit.UnitType} の移動マスおよび放出マス: {string.Join(", ", outeTiles)}");

        // 王手の判定
        foreach (var tilePos in outeTiles)
        {
            UnitController targetUnit = units[tilePos.x, tilePos.y];
            //王の位置
            GetKingPosition(unit.Player);
            UpdateKingPosition(unit);
            Vector2Int kingPosition = GetCurrentKingPosition(unit.Player);
            if (targetUnit != null && targetUnit.Player != unit.Player &&
                (targetUnit.UnitType == UnitType.oushyou || targetUnit.UnitType == UnitType.gyokusyou))
            {
                Debug.Log($"王手判定: {unit.UnitType} (プレイヤー{unit.Player}) が {targetUnit.UnitType} (プレイヤー{targetUnit.Player}) を王手しました！");
                isKingInCheck = true;
                isCheckTriggeredNextTurn = true; // 次のターンでの王手ログ出力フラグ
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

        // 王手解除チェック
        if (!DidBlockCheck(unit, tileindex))
        {
            isKingInCheck = IsKingStillInCheck(); // 王手がまだ続いているかを確認
            if (isKingInCheck)
            {
                isCheckTriggeredNextTurn = false; // 王手解除ならフラグをリセット
                //Debug.Log("王手解除されました。");
            }
        }
        else
        {
            isKingInCheck = false; // 王手解除された場合はフラグをリセット
        }

        // 王が動かなかった場合の処理
        if (isKingInCheck)
        {
            UnitController kingUnit = GetKingUnit(currentPlayer);
            if (kingUnit != null && (kingUnit.Pos == Player0KingPos || kingUnit.Pos == Player1KingPos))
            {
                kingMoveCount++;
                if (kingMoveCount >= maxMoveCount)
                {
                    string winner = currentPlayer == 0 ? "Player2" : "Player1";
                    Debug.Log($"王が動かなかったため敗北しました！プレイヤー{currentPlayer + 1} の勝利です。");
                    StartCoroutine(EndGame(winner)); return;
                }
            }
            else
            {
                kingMoveCount = 0; // 王が動いたのでカウントをリセット
            }
        }
    }

    //王手された次のターンにアニメーション
    public void StartOuteNextTurn()
    {
        //skipActivateText = true; // StartOuteNextTurnが実行された場合にフラグを立てる
        if (isCheckTriggeredNextTurn)
        {
            gametime.isPaused = true;
            DisableInput();

            Debug.Log("王手が継続しています！");
            isCheckTriggeredNextTurn = false; // ログ出力後にリセット
            skipActivateText = true; // StartOuteNextTurnが実行された場合にフラグを立てる
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
            Invoke("DeactivateTitleTimeHighlights", 2.0f); // 1秒後にハイライトを非表示
            Invoke("ActivateText", 2.0f); // ハイライト終了後にActivateTextを実行
        }
    }

    private void outecheck(UnitController unit)
    {
        // 王将が移動する前に現在王手かどうかを確認する
        if (unit.UnitType == UnitType.oushyou || unit.UnitType == UnitType.gyokusyou)
        {
            Vector2Int kingPos = unit.Pos; // 現在の王将の位置を取得
            bool isInCheck = false;

            // 敵の駒が現在の王将の位置に対して王手しているか確認
            foreach (var myUnit in units)
            {
                if (myUnit != null && myUnit.Player != unit.Player)  // 敵の駒を確認
                {
                    List<Vector2Int> myUnitMovableTiles = myUnit.GetMovableTiles(units);

                    // 王将の位置が敵の駒の移動可能範囲内にあるかどうか確認
                    if (myUnitMovableTiles.Contains(kingPos))
                    {
                        isInCheck = true;
                        break;
                    }
                }
            }

            // 移動前に王手ならログを出力
            if (isInCheck)
            {
                Debug.Log("王手のままです！");
            }
        }
    }
    void eat(UnitType type)
    {
        //勝利プレイヤー
        int WinningPlayerTwo = (currentPlayer == 0) ? 0 : 1;

        // 王将が取られたか確認
        if (type == UnitType.oushyou || type == UnitType.gyokusyou) // UnitType.King が王将を表す
        {
            string winner = WinningPlayerTwo == 0 ? "Player1" : "Player2";

            // 勝敗確定時にEndGameを呼び出し
            Debug.LogError("endゲーム呼び出し２");
            StartCoroutine(EndGame(winner));
            Debug.Log($"王将または玉将が取られました！{WinningPlayerTwo + 1} の勝利です"); //こっちはあっている
            //Debug.LogError("sceneNameにResultが入っていません");
            return; // 王将が取られたらそれ以上の処理を行わない
        }
    }

    List<Vector2Int> GetOpponentCheckTiles()
    {
        List<Vector2Int> checkTiles = new List<Vector2Int>();

        // 現在のプレイヤーの敵の駒をすべてチェック
        foreach (var unit in units)
        {
            if (unit != null && unit.Player != currentPlayer)
            {
                // 敵の駒の移動可能な範囲を取得
                List<Vector2Int> movableTiles = unit.GetMovableTiles(units);
                checkTiles.AddRange(movableTiles);
            }
        }
        return checkTiles;
    }

    bool DidBlockCheck(UnitController unit, Vector2Int newPosition)
    {
        // 敵の王手の範囲を取得
        List<Vector2Int> checkTiles = GetOpponentCheckTiles();

        //王手の範囲に移動した駒の新しい位置が含まれているか確認
        if (checkTiles.Contains(newPosition))
        {
            // 王以外の駒が王手範囲に入った場合、解除と見なす
            //return unit.UnitType != UnitType.oushyou || unit.UnitType != UnitType.gyokusyou;
        }
        return false;
    }
    bool IsKingStillInCheck()
    {
        // 現在のプレイヤーの王を取得
        UnitController kingUnit = GetKingUnit(currentPlayer);
        if (kingUnit == null)
        {
            //Debug.LogError("王が見つかりませんでした。");
            return false;
        }

        // 敵の駒が移動および放出可能な範囲を取得
        List<Vector2Int> opponentCheckTiles = GetOpponentCheckTilesWithRelease();

        // 王が敵の攻撃範囲にいるかどうかを確認
        bool isInCheck = opponentCheckTiles.Contains(kingUnit.Pos);
        Debug.Log($"王の位置: {kingUnit.Pos} | 王手状態: {isInCheck}");

        return isInCheck;
    }

    // 敵の移動マスと放出マスを取得するメソッド
    List<Vector2Int> GetOpponentCheckTilesWithRelease()
    {
        List<Vector2Int> opponentCheckTiles = new List<Vector2Int>();

        foreach (var unit in units)
        {
            if (unit != null && unit.Player != currentPlayer) // 敵の駒のみチェック
            {
                List<Vector2Int> combinedTiles = getMovableTilesWithRelease(unit); // 移動と放出マスを統合
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

    // プレイヤーごとの王の位置を取得する
    Vector2Int GetKingPosition(int player)
    {
        // プレイヤー0なら「王将」または「玉将」の位置を OusyouPos から取得
        // プレイヤー1なら「王将」または「玉将」の位置を GyokusyouPos から取得
        return (player == 0) ? Player0KingPos : Player1KingPos;
    }

    void UpdateKingPosition(UnitController unit)
    {
        if (unit.UnitType == UnitType.oushyou || unit.UnitType == UnitType.gyokusyou)
        {
            if (unit.Player == 0)
            {
                // プレイヤー0の王の位置を更新
                Player0KingPos = unit.Pos;
            }
            else if (unit.Player == 1)
            {
                // プレイヤー1の王の位置を更新
                Player1KingPos = unit.Pos;
            }
        }
    }

    // プレイヤーごとの現在の王の位置を取得する関数
    Vector2Int GetCurrentKingPosition(int player)
    {
        return (player == 0) ? Player0KingPos : Player1KingPos;
    }

    void InitializeKingPositions()
    {
        // Player0の王の初期位置を設定
        Player0KingPos = new Vector2Int(4, 8); //プレイヤー0の王の初期位置登録　いじっても場所は変わらない
        Player1KingPos = new Vector2Int(4, 0); //プレイヤー1の王の初期位置登録　いじっても場所は変わらない
    }

    public void CheckForCheckAfterMove()
    {
        // 敵プレイヤーのすべての駒が王手をかけているかを確認
        List<UnitController> enemyUnits = GetEnemyUnits(currentPlayer); // 敵プレイヤーの駒リスト取得

        foreach (var enemyUnit in enemyUnits)
        {
            List<Vector2Int> enemyMovableTiles = enemyUnit.GetMovableTiles(units); // 敵駒の移動可能マスを取得
            Vector2Int kingPosition = GetCurrentKingPosition(currentPlayer); // 自プレイヤーの王の位置を取得

            // 敵の移動範囲に自分の王の位置がある場合
            if (enemyMovableTiles.Contains(kingPosition))
            {
                Debug.Log($"{enemyUnit.UnitType} (プレイヤー{enemyUnit.Player}) が {kingPosition} で王手をかけています！");
                isCheckTriggeredNextTurn = true;
                isKingInCheck = true;
                break;
            }
        }

        // 王手が確認されない場合
        if (!isKingInCheck)
        {
            Debug.Log("王手はかかっていません。");
        }
    }

    // 敵プレイヤーの駒リストを取得するヘルパーメソッド
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

    //王手した時のカットイン演出
    public void ActivateHighlights(GameObject[] highlights)
    {
        foreach (GameObject highlight in highlights)
        {
            highlight.gameObject.SetActive(true);
        }
    }

   
    //-----王手判定終了-----

    //-------駒選択状態解除処理開始--------
    public void DeselectBoardPieces()
    {
        if (selectUnit != null)
        {
            ClearCursors();
            selectUnit.transform.position = new Vector3(selectUnit.transform.position.x, 0.1f, selectUnit.transform.position.z); // 高さをリセット
            selectUnit.Selected(false); // 選択解除
            selectUnit = null; // 選択中のユニットをリセット
        }
    }
    //-------駒選択状態解除処理終わり--------

    //-------ターンの終了処理開始--------
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

                oute(selectUnit, unit.Pos); // 王手判定を行う
            }
        }

        foreach (var unit in absorptionHistory)
        {
            if (!unit.fillingCheck && Turn >= unit.absorTurn + 1)//充填無しは０にする
            {
                unit.fillingCheck = true; // 充填完了フラグを立てる
                oute(unit, unit.Pos); // 充填完了時に即座に王手判定を実行
            }
        }

        playerMoveCounts[currentPlayer]++;// 現在のプレイヤーの手数をカウント
        PlayerPrefs.SetInt("Player1MoveCount", playerMoveCounts[0]);
        PlayerPrefs.SetInt("Player2MoveCount", playerMoveCounts[1]);
        FillingEffectDestroy();
        TotalTime++;
        PlayerPrefs.SetInt("TotalTime", Mathf.RoundToInt(TotalElapsedTime)); // 秒単位に変換
        if (IsKingStillInCheck())
        {
            string winner = currentPlayer == 0 ? "Player2" : "Player1";
            Debug.Log($"自滅が発生しました。プレイヤー{winner}の勝利です。");
            StartCoroutine(EndGame(winner));
            return;
        }
        currentPlayer = 1 - currentPlayer;
        ResetTileStates();
        if (promoteManager != null) { promoteManager.HidePromoteOptions(currentPlayer); }
        capturedPieceManager.ClearSelectedPiece();
        absorptionPieceManager.ClearSelectedPiece();
        DeselectBoardPieces();
        Debug.Log($"プレイヤー{currentPlayer + 1}のターンです。");
        gametime.ResetBattleTime();
        ClearCursors();
        // プレイヤーのターンが切り替わる際にタイマーも切り替え
        FindAnyObjectByType<GameTime>().StopBattleTime(currentPlayer);
        CheckForCheckAfterMove();
        if (!startOuteNextTurnDisabled) //成り判定が行われた時、使えないようにする
        {
            StartOuteNextTurn();
            if (!skipActivateText)
            {
                ActivateText();
            }
        }
        skipActivateText = false; // フラグをリセットして次ターンで再び動作可能に
        startOuteNextTurnDisabled = false; // 次のターンでは再度使用できるようにリセット
        Turn += 1;
        
    }
    //-------ターンの終了処理終わり--------

    //-------ゲームの終了処理開始--------
    public IEnumerator EndGame(string winner)
    {
        ClearAbsorEffects();
        yield return new WaitForSeconds(0.1f);
        Debug.Log("endゲーム呼び出しです");
        isGameActive = false;
        PlayerPrefs.SetInt("Player1MoveCount", playerMoveCounts[0]);// プレイヤー手数を保存
        PlayerPrefs.SetInt("Player2MoveCount", playerMoveCounts[1]);
        PlayerPrefs.SetString("Winner", winner);// 勝者情報を保存
        PlayerPrefs.SetInt("TotalTime", Mathf.RoundToInt(TotalElapsedTime));// 試合時間を保存
        CaptureFinalBoards();
        // リザルトシーンに移行
        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log("リザルトシーンに行きます");
            SceneManager.LoadScene(sceneName); // リザルトシーンの名前を指定
        }
        else
        {
            Debug.LogError("リザルトシーンが設定されていません。");
        }
    }
    //-------ゲームの終了処理終わり--------

    void CaptureFinalBoards()
    {
        RenderTexture renderTexture = new RenderTexture(totalWidth, totalHeight, 24);
        player1Camera.targetTexture = renderTexture;
        player2Camera.targetTexture = renderTexture;

        Texture2D combinedScreenshot = new Texture2D(totalWidth, totalHeight, TextureFormat.RGB24, false);

        // player1Camera のキャプチャ
        player1Camera.Render();
        RenderTexture.active = renderTexture;
        combinedScreenshot.ReadPixels(new Rect(0, 0, playerWidth, playerHeight), 0, 0);

        // player2Camera のキャプチャ
        player2Camera.Render();
        RenderTexture.active = renderTexture;
        combinedScreenshot.ReadPixels(new Rect(playerWidth, 0, playerWidth, playerHeight), playerWidth, 0);

        combinedScreenshot.Apply();

        // 画像を保存
        string path = Application.persistentDataPath + "/BothCameras.png";
        File.WriteAllBytes(path, combinedScreenshot.EncodeToPNG());
        Debug.Log("両カメラのスクリーンショットを保存しました: " + path);

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
        //Debug.Log($"{type} を取りました。現在の数: {capturedUnits[type]}");
    }

    public void AbsorEffect(UnitController unit)
    {
        Vector3 pos = unit.transform.position;
        currentAbsorEffect = Instantiate(absorEffect, pos, Quaternion.identity);
        Invoke("AbsorEffectDestroy", 1.9f);
    }

    private void ClearAbsorEffects()
    {
        // シーン内のすべてのabsorEffectオブジェクトを検索し、削除
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

                        // UnitTypeに応じたカラーの選択
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
                                newColor = Color.clear; // デフォルトで透明（例外的なケースを考慮）
                                break;

                        }


                        Vector3 pos = FillingEffect.transform.position;
                        GameObject FilledEffect = Instantiate(filledEffect, pos, FillingEffect.transform.rotation);
                        FilledEffect.transform.parent = FillingEffect.transform.parent;

                        // 子オブジェクトのパーティクルシステムの色を設定
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
                continue; // スキップして次の要素を確認
            }
            //else           
            //    break; // ループを終了
        }
    }

    public void InstantLoss(UnitController unit)
    {
        if (Turn >= unit.absorTurn + 2 && ( unit.UnitType == UnitType.hisya || unit.UnitType == UnitType.ryuuou) && (unit.AbsorUnitType == UnitType.huhyou || unit.AbsorUnitType == UnitType.kyousya))
        {
            Debug.Log("飛車が歩兵か香車を吸収して充填したよ。");
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

        // 左側プレイヤーのターン
        if (currentPlayer == 0)
        {
            TurnText[0].SetActive(true); // Player1
            TurnText[0].GetComponentInChildren<TextMeshProUGUI>().text = "あなたのターン";
            TurnText[1].SetActive(true); // Player2
            TurnText[1].GetComponentInChildren<TextMeshProUGUI>().text = "相手のターン";

            TurnText_always[0].GetComponentInChildren<TextMeshProUGUI>().text = "あなたのターン"; // Player1
            TurnText_always[1].GetComponentInChildren<TextMeshProUGUI>().text = "相手のターン"; // Player2
        }
        // 右側プレイヤーのターン
        if (currentPlayer == 1)
        {
            TurnText[0].SetActive(true); //Player1
            TurnText[0].GetComponentInChildren<TextMeshProUGUI>().text = "相手のターン";
            TurnText[1].SetActive(true); //Player2
            TurnText[1].GetComponentInChildren<TextMeshProUGUI>().text = "あなたのターン";

            TurnText_always[0].GetComponentInChildren<TextMeshProUGUI>().text = "相手のターン"; //Player1
            TurnText_always[1].GetComponentInChildren<TextMeshProUGUI>().text = "あなたのターン"; //Player2
        }
        Invoke("DeactivateText", 1.5f);
    }

    
    //-------消しては行けない
    //王手エフェクト
    private void DeactivateTitleTimeHighlights()
    {
        foreach (GameObject highlight in OuteEffect)
        {
            highlight.gameObject.SetActive(false);
        }
            EnableInput();
            gametime.isPaused = false;

    }

    //吸収エフェクト
    private void AbsorEffectDestroy()
    {
        if (currentAbsorEffect != null)
        {
            Destroy(currentAbsorEffect);
        }
        EnableInput();
        gametime.isPaused = false;
    }

    //ターン表示
    private void DeactivateText()
    {
        foreach (GameObject text in TurnText)
        {
            text.SetActive(false);
        }
        gametime.isPaused = false;
        EnableInput();
    }
    //-------消しては行けない
}