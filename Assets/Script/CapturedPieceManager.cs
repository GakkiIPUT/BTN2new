using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

/*
   持ち駒追加・表示更新
   駒クリック
   持ち駒選択解除
   補助メソッド
   持ち駒クリック
*/

public class CapturedPieceManager : MonoBehaviour
{
    [SerializeField] private AbsorptionPieceManager absorptionPieceManager; // 吸収駒台の参照を追加

    [SerializeField] private Transform player1CapturedArea; // Player1の駒台の位置
    [SerializeField] private Transform player2CapturedArea; // Player2の駒台の位置

    [SerializeField] private GameObject[] kanjiPrefabs; // 駒の漢字プレファブ（7個）
    [SerializeField] private GameObject[] numberPrefabs; // 数字のプレファブ（10個）

    [SerializeField] private GameObject selectionHighlightPrefab;// 駒選択用のハイライトプレファブ
    // 各プレイヤーごとに、駒の種類とその数を管理する辞書
    private Dictionary<int, Dictionary<UnitType, int>> capturedPieces = new Dictionary<int, Dictionary<UnitType, int>>();

    private GameSystem gameSystem;// GameSystem を参照

    private GameObject currentHighlight; // 現在の選択ハイライト
    public UnitType selectedPieceType = UnitType.None;// 現在選択中の駒の種類

    //--------初期化処理開始-----------
    void Start()
    {
        // GameSystem を参照
        gameSystem = FindAnyObjectByType<GameSystem>();
        absorptionPieceManager = FindAnyObjectByType<AbsorptionPieceManager>();
        if (gameSystem == null)
        {
            UnityEngine.Debug.LogError("GameSystem が見つかりませんでした。");
        }
        // プレイヤーごとの駒台を初期化
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
    //--------初期化処理終わり----------

    //--------持ち駒追加・表示更新処理開始-----------
    // 駒を駒台に追加するメソッド
    public void AddCapturedPiece(UnitType type, int player)
    {
        // カウントを1だけ増やす
        if (capturedPieces[player][type] >= 0)
        {
            capturedPieces[player][type]++;
            //Debug.Log($"{type} が捕獲されました。現在の数: {capturedPieces[player][type]}");
        }
        // 駒台に表示するための処理
        UpdateCapturedPieceDisplay(player);
    }

    // 駒台の表示を更新するメソッド
    private void UpdateCapturedPieceDisplay(int player)
    {
        // 駒台の位置を取得
        Transform capturedArea = player == 0 ? player1CapturedArea : player2CapturedArea;

        // 既存の駒表示を削除
        foreach (Transform child in capturedArea)
        {
            Destroy(child.gameObject);
        }

        // 駒の種類とその数をリスト化し、指定された順で表示
        List<(UnitType, int)> pieceList = GetSortedPieceList(player);

        // 駒の表示位置
        Vector3 currentPos = Vector3.zero;
        int pieceCount = 0; // 駒の数をカウント

        foreach (var (type, count) in pieceList)
        {
            if (count > 0)
            {
                // 駒の漢字プレファブを生成
                GameObject kanjiPrefab = GetKanjiPrefab(type);
                if (kanjiPrefab == null) continue;

                GameObject newPiece = Instantiate(kanjiPrefab, capturedArea);
                newPiece.name = type.ToString();
                newPiece.transform.localPosition = currentPos;

                // 駒にColliderがなければ追加する
                if (newPiece.GetComponent<Collider>() == null)
                {
                    newPiece.AddComponent<BoxCollider>();
                }
                // 駒がクリックされたときのイベントを追加
                newPiece.AddComponent<CapturedPieceClickHandler>().Init(this, type, player);

                // 駒数が1より大きい場合は漢数字のプレファブを追加
                if (count > 1)
                {
                    GameObject numberPrefab = Instantiate(GetNumberPrefab(count), newPiece.transform);
                    numberPrefab.name = "Count";
                    numberPrefab.transform.localPosition = new Vector3(-0.002f, -0.005f, 0.002f); // 駒の右下に配置
                }

                // 次の駒の表示位置を更新
                currentPos.x += -0.62f; // 横方向に詰めて配置
                pieceCount++;

                // 改行処理（4個ごとに改行）
                if (pieceCount % 4 == 0)
                {
                    currentPos.x = 0;
                    currentPos.z += 0.8f; // 縦方向に少し下げる
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

    // 指定された順で駒を並べ替えるメソッド
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
    //--------持ち駒追加・表示更新処理終わり----------

    //--------駒クリック処理開始-----------
    // 駒がクリックされた時の処理
    public void OnCapturedPieceClicked(UnitType type, int player)
    {
        absorptionPieceManager.AbsorDeselectPiece();
        DeselectPiece();
        // 現在のプレイヤーのターンかどうか確認
        if (gameSystem.GetCurrentPlayer() != player)
        {
            return;
        }
        gameSystem.DeselectBoardPieces();

        // 駒の選択
        selectedPieceType = type;

        // 駒の位置にハイライトを表示
        GameObject clickedPiece = GameObject.Find(type.ToString()); // 駒の名前でオブジェクトを探す
        if (clickedPiece != null)
        {
            ShowHighlight(clickedPiece.transform.position); // 駒の位置にハイライトを表示
        }

        // 駒を置ける場所にカーソルを表示
        gameSystem.DisplayPlaceableCursors();
    }
    // ハイライトを駒の位置に表示するメソッド
    public void ShowHighlight(Vector3 position)
    {
        ClearHighlight(); // 既存のハイライトをクリア
        AbsorptionPieceManager absorptionPieceManager = GetComponent<AbsorptionPieceManager>();
        absorptionPieceManager.ClearHighlight();

        // 座標を調整する（指定のオフセットを加える）
        Vector3 adjustedPosition = new Vector3(
            position.x - 0.22f,  // x を -0.22 調整
            position.y - 2.7f,   // y を -2.7 調整
            position.z - 0.86f   // z を +0.86 調整
        );

        // ハイライトプレファブを生成
        currentHighlight = Instantiate(selectionHighlightPrefab, adjustedPosition, Quaternion.identity);
        currentHighlight.transform.localPosition = adjustedPosition;
    }
    // 選択状態を解除
    public void ClearHighlight()
    {
        if (currentHighlight != null)
        {
            Destroy(currentHighlight);
        }
    }
    //--------駒クリック処理終わり----------

    //--------持ち駒選択解除処理開始-----------
    public void DeselectPiece()
    {
        // 持ち駒の選択を解除
        ClearSelectedPiece();
        //Debug.Log("駒の選択を解除しました。");

        // GameSystem の ClearCursors メソッドを呼び出す
        if (gameSystem != null)
        {
            gameSystem.ClearCursors();  // カーソルをクリア
            if (gameSystem.GetSelectUnit() != null)// 将棋盤上の駒の選択も解除
            {
                gameSystem.GetSelectUnit().Selected(false);  // 選択を解除
                gameSystem.ClearSelectUnit();  // GameSystem の選択状態を解除
            }
        }
    }
    // 選択中の駒を解除するメソッド
    public void ClearSelectedPiece()
    {
        selectedPieceType = UnitType.None;

        //Debug.Log("選択中の駒が解除されました。");
        ClearHighlight();
    }

    //--------駒選択解除処理終わり----------

    //--------補助メソッド開始-----------
    public GameObject GetKanjiPrefab(UnitType type)// 駒の種類に応じて漢字プレファブを取得するメソッド
    {
        switch (type)
        {
            case UnitType.huhyou: return kanjiPrefabs[0]; // 歩
            case UnitType.kyousya: return kanjiPrefabs[1]; // 香
            case UnitType.keima: return kanjiPrefabs[2]; // 桂
            case UnitType.ginsyou: return kanjiPrefabs[3]; // 銀
            case UnitType.kinsyou: return kanjiPrefabs[4]; // 金
            case UnitType.hisya: return kanjiPrefabs[5]; // 飛
            case UnitType.kakugyou: return kanjiPrefabs[6]; // 角
            default: return null;
        }
    }
    // 数字プレファブを取得するメソッド
    private GameObject GetNumberPrefab(int count)
    {
        if (count < 1 || count > 10) return null;
        return numberPrefabs[count - 1];
    }
    //--------補助メソッド終わり----------

    //--------持ち駒クリック処理開始-----------
    public class CapturedPieceClickHandler : MonoBehaviour
    {
        private CapturedPieceManager capturedPieceManager;
        private UnitType pieceType;
        private int player;

        // 初期化メソッド
        public void Init(CapturedPieceManager manager, UnitType type, int playerIndex)
        {
            capturedPieceManager = manager;
            pieceType = type;
            player = playerIndex;
        }

        // 駒がクリックされたときに呼ばれるメソッド
        public void OnMouseUpAsButton()
        {
            if (capturedPieceManager != null)
            {
                // 駒がクリックされたときの処理
                capturedPieceManager.OnCapturedPieceClicked(pieceType, player);

                // ハイライトを表示（調整された駒の位置に）
                Vector3 highlightPosition = transform.position; // 駒の位置を取得
                capturedPieceManager.ShowHighlight(highlightPosition);
            }
        }

    }
    // 現在選択されている駒の種類を取得するメソッド
    public UnitType GetSelectedPieceType()
    {
        return selectedPieceType;
    }
    //--------持ち駒クリック処理終わり----------
}
