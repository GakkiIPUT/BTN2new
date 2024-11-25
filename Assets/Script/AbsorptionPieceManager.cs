using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AbsorptionPieceManager : MonoBehaviour
{
    [SerializeField] private CapturedPieceManager capturedPieceManager; // 通常駒台の参照を追加

    [SerializeField] private Transform player1AbsorArea; // Player1の吸収駒台の位置 
    [SerializeField] private Transform player2AbsorArea; // Player2の吸収駒台の位置 

    [SerializeField] private GameObject[] kanjiPrefabs; // 駒の漢字プレファブ（7個）
    [SerializeField] private GameObject[] numberPrefabs; // 数字のプレファブ（10個）

    // 駒選択用のハイライトプレファブ
    [SerializeField] private GameObject selectionHighlightPrefab;
    private GameObject currentHighlight; // 現在の選択ハイライト

    // 各プレイヤーごとに、駒の種類とその数を管理する辞書
    private Dictionary<int, Dictionary<UnitType, int>> absorptionPices = new Dictionary<int, Dictionary<UnitType, int>>();

    // 現在選択中の駒の種類
    public UnitType selectedPieceType = UnitType.None;

    // GameSystem を参照
    private GameSystem gameSystem;

    // 駒台表示時の漢字の色
    public Material absorPiece;

    // ボタンから呼び出すための選択解除メソッド

    //--------初期化処理開始-----------
    void Start()
    {
        // GameSystem を参照
        gameSystem = FindAnyObjectByType<GameSystem>();
        capturedPieceManager = FindAnyObjectByType<CapturedPieceManager>();

        // プレイヤーごとの駒台を初期化
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
    //--------初期化処理終わり----------

    //--------持ち駒追加・表示更新処理開始-----------
    // 駒を駒台に追加するメソッド
    public void AddAbsorptionPiece(UnitType type, int player2)
    {
        // カウントを1だけ増やす
        if (absorptionPices[player2][type] >= 0)
        {
            absorptionPices[player2][type]++;
            //Debug.Log($"{type} が捕獲されました。現在の数: {capturedPieces[player][type]}");
        }
        // 駒台に表示するための処理
        UpdateAbsorPieceDisplay(player2);
    }

    private void UpdateAbsorPieceDisplay(int player)
    {
        // 駒台の位置を取得
        Transform absorArea = player == 0 ? player1AbsorArea : player2AbsorArea;

        // 既存の駒表示を削除
        foreach (Transform child2 in absorArea)
        {
            Destroy(child2.gameObject);
        }

        // 駒の種類とその数をリスト化し、指定された順で表示
        List<(UnitType, int)> pieceList2 = GetSortedPieceList(player);

        // 駒の表示位置
        Vector3 currentPos2 = Vector3.zero;
        int pieceCount2 = 0;

        foreach (var (type, count) in pieceList2)
        {
            if (count > 0)
            {
                // 駒の漢字プレファブを生成
                GameObject kanjiPrefab = GetKanjiPrefab(type);
                if (kanjiPrefab == null) continue;

                GameObject newPiece2 = Instantiate(kanjiPrefab, absorArea);
                newPiece2.GetComponent<MeshRenderer>().material = absorPiece;
                newPiece2.name = type.ToString();
                newPiece2.transform.localPosition = currentPos2;

                // 駒にColliderがなければ追加する
                if (newPiece2.GetComponent<Collider>() == null)
                {
                    newPiece2.AddComponent<BoxCollider>();
                }
                // 駒がクリックされたときのイベントを追加
                newPiece2.AddComponent<CapturedPieceClickHandler>().Init(this, type, player);

                // 駒数が1より大きい場合は漢数字のプレファブを追加
                if (count > 1)
                {
                    GameObject numberPrefab2 = Instantiate(GetNumberPrefab(count), newPiece2.transform);
                    numberPrefab2.name = "Count";
                    numberPrefab2.transform.localPosition = new Vector3(-0.002f, -0.004f, 0.002f); // 駒の右下に配置
                }

                // 次の駒の表示位置を更新
                currentPos2.x += -0.62f;
                pieceCount2++;

                // 改行処理（4個ごとに改行）
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

    // 指定された順で駒を並べ替えるメソッド
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
    //--------持ち駒追加・表示更新処理終わり----------


    // 駒の種類に応じて漢字プレファブを取得するメソッド
    public GameObject GetKanjiPrefab(UnitType type)
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


    //--------駒クリック処理開始-----------
    // 駒がクリックされた時の処理
    public void OnAbsorptionPieceClicked(UnitType type, int player)
    {
        capturedPieceManager.DeselectPiece();
        AbsorDeselectPiece();
        // 現在のプレイヤーのターンかどうか確認
        if (gameSystem.GetCurrentPlayer() != player)
        {
            return;
        }

        // 駒の選択
        selectedPieceType = type;

        // 駒の位置にハイライトを表示
        GameObject clickedPiece = GameObject.Find(type.ToString()); // 駒の名前でオブジェクトを探す
        if (clickedPiece != null)
        {
            ShowHighlight(clickedPiece.transform.position); // 駒の位置にハイライトを表示
        }

        // 駒を置ける場所にカーソルを表示
        gameSystem.DisplayPlaceableCursorsAbsor();
    }

    // ハイライトを駒の位置に表示するメソッド
    public void ShowHighlight(Vector3 position)
    {
        ClearHighlight(); // 既存のハイライトをクリア
        CapturedPieceManager capturedPieceManager = GetComponent<CapturedPieceManager>();
        capturedPieceManager.ClearHighlight();

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
    public void AbsorDeselectPiece()
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
    // 数字プレファブを取得するメソッド
    private GameObject GetNumberPrefab(int count)
    {
        if (count < 1 || count > 10) return null;
        return numberPrefabs[count - 1];
    }
    //--------補助メソッド終わり----------

    //--------CapturedPieceClickHandlerクラス開始-----------
    public class CapturedPieceClickHandler : MonoBehaviour
    {
        private AbsorptionPieceManager absorptionPieceManager;
        private UnitType pieceType;
        private int player;

        // 初期化メソッド
        public void Init( AbsorptionPieceManager manager, UnitType type, int playerIndex)
        {
            absorptionPieceManager = manager;
            pieceType = type;
            player = playerIndex;
        }

        // 駒がクリックされたときに呼ばれるメソッド
        public void OnMouseUpAsButton()
        {
            if (absorptionPieceManager != null)
            {
                // 駒がクリックされたときの処理
                absorptionPieceManager.OnAbsorptionPieceClicked(pieceType, player);

                // ハイライトを表示（調整された駒の位置に）
                Vector3 highlightPosition = transform.position; // 駒の位置を取得
                absorptionPieceManager.ShowHighlight(highlightPosition);
            }
        }

    }
    // 現在選択されている駒の種類を取得するメソッド
    public UnitType GetSelectedPieceType()
    {
        return selectedPieceType;
    }
    //--------CapturedPieceClickHandlerクラス終わり----------

}
