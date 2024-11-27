using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
   駒移動
   駒選択
   駒成り
   駒成り解除
   駒吸収
   駒「移動」可能範囲取得
   駒「吸収」可能範囲取得
   充填チェック
   駒「放出」可能範囲取得
   プレハブ変更
   補助メソッド
*/


// 駒の種類
public enum UnitType
{
    None = -1,
    huhyou = 1, // 歩兵
    kakugyou, // 角
    hisya, // 飛車
    kyousya, // 香車
    keima, // 桂馬
    ginsyou, // 銀将
    kinsyou, // 金将
    oushyou, // 王将
    gyokusyou,//玉将
    narigoma,
    ryuuma, // 竜馬
    ryuuou, // 竜王
}

public enum FieldStatus
{
    None = -1,
    OnBard,
    Captured,
}

public class UnitController : MonoBehaviour
{
    public int Player;  // プレイヤー番号
    public UnitType UnitType, OldUnitType, AbsorUnitType;  // 現在の駒タイプ、成り前の駒タイプ、吸収された駒のタイプ
    public FieldStatus FieldStatus;  // 駒の状態 (盤上、捕獲)
    public Vector2Int Pos;  // 盤上の位置
    public bool absorptionCheck = false;  // 吸収確認フラグ
    public bool fillingCheck = false;     // 充填確認フラグ
    public bool ReleseCheck = false;      // 放出確認フラグ
    public int absorTurn = 0;             // 吸収ターン
    private GameSystem gameSystem;        // GameSystemの参照
    public UnitController SelectUnit;
    //private AudioSource audioSource;
    //[SerializeField] AudioClip SE;

    public int Movecount;
    public int PutTurn = 0;
    public bool PutUnitCheck = false;

    public GameObject originalPrefab;  // 元のプレハブ
    public GameObject promotedPrefab;  // 成り駒のプレハブ
    public int AbsorUnitPlayer { get; set; }
    public bool hasTemporaryDefense = false;
    List<UnitType> narigoma;

    //--------初期化処理開始-----------
    void Start()
    {
        gameSystem = FindAnyObjectByType<GameSystem>();
        SelectUnit = gameSystem.selectUnit;
    }

    public void Init(int player, int unittype, GameObject tile, Vector2Int pos)
    {
        Player = player;
        FieldStatus = FieldStatus.OnBard;
        if (tile != null)
        {
            Move(tile, pos);
        }
        else
        {
            //Debug.LogWarning("Initメソッドでtileがnullです。Moveは呼び出されませんでした。");
        }
        Pos = pos; // Posを設定

        Movecount = 0;
    }
    //--------初期化処理終わり----------

    //--------駒移動処理開始-----------
    public void Move(GameObject tile, Vector2Int tileindex)
    {
        if (tile == null)
        {
            //Debug.LogError("Moveメソッドでtileがnullです。");
            return;
        }
        Vector3 pos = tile.transform.position;
        pos.y = 0.1f;
        transform.position = pos;
        Pos = tileindex;
        //audioSource.PlayOneShot(SE)
        Movecount += 1;
;    }
    //--------駒移動処理終わり----------

    //--------駒選択処理開始-----------
    public void Selected(bool select = true)
    {
        Vector3 pos = transform.position;
        bool isKinematic = true;

        if (select)
        {
            pos.y = 0.3f;  // 選択時に高さを変更
        }
        else
        {
            pos.y = 0.1f;  // 非選択時に元の高さに戻す
        }

        GetComponent<Rigidbody>().isKinematic = isKinematic;
        transform.position = pos;
    }
    //--------駒選択処理終わり----------

    //--------駒成り処理開始-----------
    public void Promote()
    {
        //Debug.Log($"成り前の駒の種類: {UnitType}");
        StartCoroutine(PromoteWithDelay());
    }

    private IEnumerator PromoteWithDelay()
    {
        OldUnitType = UnitType;// 元の駒の種類を保存
        //Debug.Log($"OldUnitTypeに保存された駒: {OldUnitType}");

        switch (UnitType)// 成り駒に変換
        {
            case UnitType.huhyou: UnitType = UnitType.narigoma; break;
            case UnitType.kyousya: UnitType = UnitType.narigoma; break;
            case UnitType.keima: UnitType = UnitType.narigoma; break;
            case UnitType.ginsyou: UnitType = UnitType.narigoma; break;
            case UnitType.hisya: UnitType = UnitType.ryuuou; break;
            case UnitType.kakugyou: UnitType = UnitType.ryuuma; break;
        }
        //Debug.Log($"成り後の駒の種類: {UnitType}");


        // インスペクターの更新を強制
#if UNITY_EDITOR　
        UnityEditor.EditorUtility.SetDirty(this);
#endif

        yield return new WaitForSeconds(0.5f);// 少し待つ（見た目の調整）

        GameObject newUnit = Instantiate(promotedPrefab, transform.position, transform.rotation);// 非アクティブなプレファブを生成
        newUnit.SetActive(true);  // 生成後にアクティブ化
        UnitController newUnitController = newUnit.GetComponent<UnitController>();
        newUnitController.Init(Player, (int)UnitType, null, Pos); // 新しい駒にプレイヤー情報と位置情報を引き継ぐ

        if (this.absorptionCheck) // 吸収処理の引継ぎ
        {
            newUnitController.hasTemporaryDefense = this.hasTemporaryDefense;
            newUnitController.absorptionCheck = true;
            newUnitController.AbsorUnitType = this.AbsorUnitType;
            gameSystem.absorptionHistory.Remove(this);
            gameSystem.absorptionHistory.Add(newUnitController);
            if (!this.ReleseCheck)
            {
                this.gameObject.transform.GetChild(1).parent = newUnit.transform;
            }
        }
        if (this.fillingCheck) // 充填処理の引継ぎ
        {
            newUnitController.fillingCheck = true;
            newUnit.GetComponent<MeshRenderer>().material = this.gameObject.GetComponent<MeshRenderer>().material;
        }
        if (this.ReleseCheck) // 放出処理の引継ぎ
        {
            newUnitController.ReleseCheck = true;
            gameSystem.absorptionHistory.Remove(newUnitController);
            newUnit.GetComponent<MeshRenderer>().material = this.gameObject.GetComponent<MeshRenderer>().material;
        }

        gameSystem.UpdateUnitPosition(newUnitController);// GameSystemに再登録

        Debug.Log(this.gameObject + "　111");
        Destroy(this.gameObject);// 元の駒を削除
    }
    //--------駒成り処理終わり----------

    //--------駒成り解除処理開始-----------
    public void Demote()
    {
        //Debug.Log($"成り状態を元に戻す: {UnitType} -> {OldUnitType}");
        UnitType = OldUnitType;
        FieldStatus = FieldStatus.Captured;
        transform.position = Vector3.zero;// 位置を初期化して持ち駒として準備
        transform.rotation = Quaternion.identity;
        promotedPrefab.SetActive(false);// プレハブを非表示にするだけで削除しない
        //Debug.Log($"{UnitType} を持ち駒にしました。");
    }
    //--------駒成り解除処理終わり----------

    //--------駒吸収処理開始-----------
    public void AbsorCheck()
    {
        if (gameSystem.absorUnit.UnitType == UnitType.narigoma|| gameSystem.absorUnit.UnitType == UnitType.ryuuma || gameSystem.absorUnit.AbsorUnitType == UnitType.ryuuou)// 吸収した駒に吸収された駒のユニットタイプを参照
        {
            AbsorUnitType = gameSystem.absorUnit.OldUnitType;
            Debug.Log(gameSystem.absorUnit.UnitType + "を吸収したよ");
            Debug.Log("OldUnitは" + AbsorUnitType + "だよ");
        }
        else
        {
            AbsorUnitType = gameSystem.absorUnit.OldUnitType;
            Debug.Log("普通の駒吸収したよ");
            Debug.Log("OldUnitは" + AbsorUnitType + "だよ");
        }
    }
    //--------駒吸収処理終わり----------

    //--------駒「移動」可能範囲取得処理開始-----------
    public List<Vector2Int> GetMovableTiles(UnitController[,] units)
    {
        List<Vector2Int> ret = new List<Vector2Int>();

        switch (UnitType)
        {
            //歩兵
            case UnitType.huhyou:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int forward = new Vector2Int(0, dir);
                    Vector2Int checkpos = Pos + forward;
                    if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                    {
                        ret.Add(checkpos);
                    }
                }
                break;

            //香車
            case UnitType.kyousya:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    for (int i = 1; i < 9; i++)
                    {
                        Vector2Int forward = new Vector2Int(0, dir * i);
                        Vector2Int checkpos = Pos + forward;
                        if (!isCheckable(units, checkpos)) break;
                        if (units[checkpos.x, checkpos.y] != null)
                        {
                            if (units[checkpos.x, checkpos.y].Player == Player) break;
                            ret.Add(checkpos);
                            break;
                        }
                        ret.Add(checkpos);
                    }
                }
                break;

            //桂馬
            case UnitType.keima:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] knightMoves = { new Vector2Int(1, 2 * dir), new Vector2Int(-1, 2 * dir) };
                    foreach (var move in knightMoves)
                    {
                        Vector2Int checkpos = Pos + move;
                        if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                        {
                            ret.Add(checkpos);
                        }
                    }
                }
                break;

            //銀将
            case UnitType.ginsyou:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions =
                    {
                    new Vector2Int(0, dir), // 前進
　　　　　          new Vector2Int(1, dir), // 右前
　　　　　          new Vector2Int(-1, dir), // 左前
　　　　　          new Vector2Int(1, -dir), // 右後ろ
　　　　　          new Vector2Int(-1, -dir) // 左後ろ
　　　　　  　　    };
                    foreach (var move in directions)
                    {
                        Vector2Int checkpos = Pos + move;
                        if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                        {
                            ret.Add(checkpos);
                        }
                    }
                }
                break;

            //金将
            case UnitType.kinsyou:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions = {
                    new Vector2Int(0, dir),   // 前進
　　　　　          new Vector2Int(1, dir),   // 右前
　　　　　          new Vector2Int(-1, dir),  // 左前
　　　　　          new Vector2Int(0, -dir),  // 後退
　　　　　          new Vector2Int(1, 0),     // 右
　　　　　          new Vector2Int(-1, 0)     // 左
　　　　　  　　    };
                    foreach (var move in directions)
                    {
                        Vector2Int checkpos = Pos + move;
                        if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                        {
                            ret.Add(checkpos);
                        }
                    }
                }
                break;
            //王将
            case UnitType.oushyou:
                {
                    Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1) };
                    foreach (var move in directions)
                    {
                        Vector2Int checkpos = Pos + move;
                        if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                        {
                            ret.Add(checkpos);
                        }
                    }
                }
                break;

            //玉将
            case UnitType.gyokusyou:
                {
                    Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1) };
                    foreach (var move in directions)
                    {
                        Vector2Int checkpos = Pos + move;
                        if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                        {
                            ret.Add(checkpos);
                        }
                    }
                }
                break;

            //飛車
            case UnitType.hisya:
                {
                    Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
                    foreach (var dir in directions)
                    {
                        for (int i = 1; i < 9; i++)
                        {
                            Vector2Int checkpos = Pos + dir * i;
                            if (!isCheckable(units, checkpos)) break;
                            if (units[checkpos.x, checkpos.y] != null)
                            {
                                if (units[checkpos.x, checkpos.y].Player == Player) break;
                                ret.Add(checkpos);
                                break;
                            }
                            ret.Add(checkpos);
                        }
                    }
                }
                break;

            //角行
            case UnitType.kakugyou:
                {
                    Vector2Int[] directions = { new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1) };
                    foreach (var dir in directions)
                    {
                        for (int i = 1; i < 9; i++)
                        {
                            Vector2Int checkpos = Pos + dir * i;
                            if (!isCheckable(units, checkpos)) break;
                            if (units[checkpos.x, checkpos.y] != null)
                            {
                                if (units[checkpos.x, checkpos.y].Player == Player) break;
                                ret.Add(checkpos);
                                break;
                            }
                            ret.Add(checkpos);
                        }
                    }
                }
                break;

            //成り駒
            case UnitType.narigoma:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions = {
        new Vector2Int(0, dir),   // 前進
　　　　　        new Vector2Int(1, dir),   // 右前
　　　　　        new Vector2Int(-1, dir),  // 左前
　　　　　        new Vector2Int(0, -dir),  // 後退
　　　　　        new Vector2Int(1, 0),     // 右
　　　　　        new Vector2Int(-1, 0)     // 左
　　　　　  　　  };
                    foreach (var move in directions)
                    {
                        Vector2Int checkpos = Pos + move;
                        if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                        {
                            ret.Add(checkpos);
                        }
                    }
                }
                break;

            case UnitType.ryuuou:
                {
                    // 飛車と同じ縦横の無制限移動
                    Vector2Int[] straightDirections = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
                    foreach (var dir in straightDirections)
                    {
                        for (int i = 1; i < 9; i++)
                        {
                            Vector2Int checkpos = Pos + dir * i;
                            if (!isCheckable(units, checkpos)) break;
                            if (units[checkpos.x, checkpos.y] != null)
                            {
                                if (units[checkpos.x, checkpos.y].Player == Player) break;
                                ret.Add(checkpos);
                                break;
                            }
                            ret.Add(checkpos);
                        }
                    }

                    // 1マスの斜め移動
                    Vector2Int[] diagonalDirections = { new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1) };
                    foreach (var move in diagonalDirections)
                    {
                        Vector2Int checkpos = Pos + move;
                        if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                        {
                            ret.Add(checkpos);
                        }
                    }
                }
                break;

            case UnitType.ryuuma:
                {
                    // 角行と同じ斜めの無制限移動
                    Vector2Int[] diagonalDirections = { new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1) };
                    foreach (var dir in diagonalDirections)
                    {
                        for (int i = 1; i < 9; i++)
                        {
                            Vector2Int checkpos = Pos + dir * i;
                            if (!isCheckable(units, checkpos)) break;
                            if (units[checkpos.x, checkpos.y] != null)
                            {
                                if (units[checkpos.x, checkpos.y].Player == Player) break;
                                ret.Add(checkpos);
                                break;
                            }
                            ret.Add(checkpos);
                        }
                    }

                    // 1マスの縦横移動
                    Vector2Int[] straightDirections = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
                    foreach (var move in straightDirections)
                    {
                        Vector2Int checkpos = Pos + move;
                        if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                        {
                            ret.Add(checkpos);
                        }
                    }
                }
                break;

        }
        return ret;
    }
    //--------駒「移動」可能範囲取得処理終わり---------

    //--------駒「吸収」可能範囲取得処理開始-----------
    public List<Vector2Int> GetAbsorptionTiles(UnitController[,] units)
    {
        List<Vector2Int> ret = new List<Vector2Int>();
        //UnitContlroller unitctrl = GetComponent<UnitContlroller>();

        //switch (unitctrl.UnitType)
        switch (UnitType)
        {
            // 歩兵
            case UnitType.huhyou:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions =
                    {
                        new Vector2Int(1, dir),   // 右前
                        new Vector2Int(-1, dir),  // 左前
                        new Vector2Int(1, 0),     // 右
                        new Vector2Int(-1, 0),    // 左
                        new Vector2Int(1, -dir),  // 右後ろ
                        new Vector2Int(-1, -dir), // 左後ろ
                        new Vector2Int(0, -dir)  // 後退
                    };
                    //すべての方向を確認
                    foreach (var absor in directions)
                    {
                        Vector2Int checkpos = Pos + absor;
                        if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] != null && UnitType.oushyou != units[checkpos.x, checkpos.y].UnitType)
                        {
                            ret.Add(checkpos);
                        }
                    }
                }
                break;

            // 香車
            case UnitType.kyousya:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions =
                    {
                        new Vector2Int(1, dir),   // 右前
                        new Vector2Int(-1, dir),  // 左前
                        new Vector2Int(1, 0),     // 右
                        new Vector2Int(-1, 0),    // 左
                        new Vector2Int(1, -dir),  // 右後ろ
                        new Vector2Int(-1, -dir), // 左後ろ
                        new Vector2Int(0, -dir)  // 後退
                    };
                    //すべての方向を確認
                    foreach (var absor in directions)
                    {
                        Vector2Int checkpos = Pos + absor;
                        if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] != null && UnitType.oushyou != units[checkpos.x, checkpos.y].UnitType)
                        {
                            ret.Add(checkpos);
                        }
                    }
                }
                break;

            // 桂馬
            case UnitType.keima:
                {
                    Vector2Int[] directions =
                    {
                        new Vector2Int(1, 0),
                        new Vector2Int(-1, 0),
                        new Vector2Int(0, 1),
                        new Vector2Int(0, -1),
                        new Vector2Int(1, 1),
                        new Vector2Int(-1, 1),
                        new Vector2Int(1, -1),
                        new Vector2Int(-1, -1)
                    };
                    //すべての方向を確認
                    foreach (var absor in directions)
                    {
                        Vector2Int checkpos = Pos + absor;
                        if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] != null && UnitType.oushyou != units[checkpos.x, checkpos.y].UnitType)
                        {
                            ret.Add(checkpos);
                        }
                    }
                }
                break;

            // 銀将
            case UnitType.ginsyou:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions =
                    {
                        new Vector2Int(1, 0),     // 右
                        new Vector2Int(-1, 0),    // 左
                        new Vector2Int(0, -dir)  // 後退
                    };
                    //すべての方向を確認
                    foreach (var absor in directions)
                    {
                        Vector2Int checkpos = Pos + absor;
                        if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] != null && UnitType.oushyou != units[checkpos.x, checkpos.y].UnitType)
                        {
                            ret.Add(checkpos);
                        }
                    }
                }
                break;

            // 金将
            case UnitType.kinsyou:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions =
                    {
                        new Vector2Int(1, -dir),  // 右後ろ
                        new Vector2Int(-1, -dir), // 左後ろ
                    };
                    //すべての方向を確認
                    foreach (var absor in directions)
                    {
                        Vector2Int checkpos = Pos + absor;
                        if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] != null && UnitType.oushyou != units[checkpos.x, checkpos.y].UnitType)
                        {
                            ret.Add(checkpos);
                        }
                    }
                }
                break;

            // 飛車
            case UnitType.hisya:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions =
                    {
                        new Vector2Int(1, dir),   // 右前
                        new Vector2Int(-1, dir),  // 左前
                        new Vector2Int(1, -dir),  // 右後ろ
                        new Vector2Int(-1, -dir), // 左後ろ
                    };
                    //すべての方向を確認
                    foreach (var absor in directions)
                    {
                        Vector2Int checkpos = Pos + absor;
                        if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] != null && UnitType.oushyou != units[checkpos.x, checkpos.y].UnitType)
                        {
                            ret.Add(checkpos);
                        }
                    }
                }
                break;

            // 角行
            case UnitType.kakugyou:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions =
                    {
                        new Vector2Int(1, 0),     // 右
                        new Vector2Int(-1, 0),    // 左
                        new Vector2Int(0, dir) , // 後退
                        new Vector2Int(0, -dir)  // 前
                    };
                    //すべての方向を確認
                    foreach (var absor in directions)
                    {
                        Vector2Int checkpos = Pos + absor;
                        if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] != null && UnitType.oushyou != units[checkpos.x, checkpos.y].UnitType)
                        {
                            ret.Add(checkpos);
                        }
                    }
                }
                break;

            // 成り駒
            case UnitType.narigoma:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions =
                    {
                        new Vector2Int(1, -dir),  // 右後ろ
                        new Vector2Int(-1, -dir), // 左後ろ
                    };
                    //すべての方向を確認
                    foreach (var absor in directions)
                    {
                        Vector2Int checkpos = Pos + absor;
                        if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] != null && UnitType.oushyou != units[checkpos.x, checkpos.y].UnitType)
                        {
                            ret.Add(checkpos);
                        }
                    }
                }
                break;
        }
        return ret;
    }
    //--------駒「吸収」可能範囲取得処理終わり----------

    //--------充填チェック処理開始-----------
    public void FillingCheck(UnitController unit)
    {
        // 吸収された駒の情報を確認し、充填処理を行う
        if (unit.absorptionCheck && gameSystem.Turn >= unit.absorTurn +2)//充填無しは０にする
        {
            unit.fillingCheck = true;       // 通常の充填完了状態
        }
        else if (unit.absorptionCheck && gameSystem.Turn >= unit.absorTurn)
        {
            unit.fillingCheck = false;       // 通常の充填はまだ完了していない
        }
        else
        {
            unit.fillingCheck = false;       // 充填がまだ完了していない
        }
    }
    //--------充填チェック処理終わり----------

    //--------駒「放出」可能範囲取得処理開始-----------
    public List<Vector2Int> GetRelesableTiles(UnitController[,] units)
    {
        List<Vector2Int> ret = new List<Vector2Int>();

        // switch() の UnitType は 吸収された駒のタイプ、case 内の UnitType は吸収した駒のタイプ
        switch (AbsorUnitType)
        {
            case UnitType.huhyou: // 吸収された駒が歩兵
                {
                    // プレイヤーによって向きを変える
                    int dir = (Player == 0) ? -1 : 1;
                    // スタート位置
                    int X = 0;
                    int Y = 0;
                    // ＋歩兵の動き（親：歩兵、銀将、金将、成り駒、竜馬）　2マス前に増加
                    if (UnitType == UnitType.huhyou || UnitType == UnitType.ginsyou || UnitType == UnitType.kinsyou || UnitType == UnitType.narigoma || UnitType == UnitType.ryuuma)
                    {
                        X = 0;
                        Y = 2;
                    }

                    //香車歩兵
                    ////＋歩兵の動き（親：香車）　1マス後ろに増加
                    //if (UnitType == UnitType.kyousya)
                    //{
                    //    X = 0;
                    //    Y = -1;
                    //}

                    // ＋歩兵の動き（親：角行、桂馬）　1マス前に増加
                    if (UnitType == UnitType.kakugyou || UnitType == UnitType.keima)
                    {
                        X = 0;
                        Y = 1;
                    }

                    Vector2Int Foward = new Vector2Int(X, dir);
                    Vector2Int Checkpos = Pos + Foward;
                    Vector2Int foward = new Vector2Int(X, dir * Y);
                    Vector2Int checkpos = Pos + foward;
                    if (isCheckable(units, checkpos))
                    {
                        if (units[Checkpos.x, Checkpos.y] == null)
                        {
                            if (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player)
                            {
                                ret.Add(checkpos);
                            }
                        }
                        else if (units[Checkpos.x, Checkpos.y].Player != Player)
                        {
                            ret.Add(Checkpos);
                        }
                    }
                }
                break;

            case UnitType.kyousya: // 吸収された駒が香車
                {
                    // プレイヤーによって向きを変える
                    int dir = (Player == 0) ? -1 : 1;
                    // 縦方向のスタート位置
                    int Fowards = 0;

                    Vector2Int Foward = new Vector2Int(0, dir);
                    Vector2Int Checkpos = Pos + Foward;

                    //香車香車
                    //// ＋香車の動き（親：香車）　後ろ方向に増加
                    //if (UnitType == UnitType.kyousya)
                    //{
                    //    Fowards = 1;
                    //    dir *= -1;

                    //    // 太く
                    //    Vector2Int offset1 = new Vector2Int(-1, -1); // 右
                    //    Vector2Int offset2 = new Vector2Int(1, -1); // 左

                    //    // 右前
                    //    Vector2Int direction1 = new Vector2Int(-1, -1); 
                    //    Vector2Int checkpos1 = Pos + direction1;
                    //    if (isCheckable(units, checkpos1) && units[checkpos1.x, checkpos1.y] == null)
                    //    {
                    //        Vector2Int newCenterPos1 = Pos + offset1;
                    //        Vector2Int directions1 = new Vector2Int(0, -1); // 右上
                    //        for (int i = 1; i < 9; i++)
                    //        {
                    //            Vector2Int Checkpos1 = newCenterPos1 + directions1 * i;
                    //            if (!isCheckable(units, Checkpos1)) break;
                    //            if (units[Checkpos1.x, Checkpos1.y] != null)
                    //            {
                    //                if (units[Checkpos1.x, Checkpos1.y].Player == Player) break;
                    //                ret.Add(Checkpos1);
                    //                break;
                    //            }
                    //            ret.Add(Checkpos1);
                    //        }
                    //        ret.Add(checkpos1);
                    //    }
                    //    else if (units[checkpos1.x, checkpos1.y].Player != Player)
                    //    {
                    //        ret.Add(checkpos1);
                    //    }

                    //    // 右前
                    //    Vector2Int direction2 = new Vector2Int(1, -1);
                    //    Vector2Int checkpos2 = Pos + direction2;
                    //    if (isCheckable(units, checkpos2) && units[checkpos2.x, checkpos2.y] == null)
                    //    {
                    //        Vector2Int newCenterPos2 = Pos + offset2;
                    //        Vector2Int directions2 = new Vector2Int(0, -1); // 右上
                    //        for (int i = 1; i < 9; i++)
                    //        {
                    //            Vector2Int Checkpos2 = newCenterPos2 + directions2 * i;
                    //            if (!isCheckable(units, Checkpos2)) break;
                    //            if (units[Checkpos2.x, Checkpos2.y] != null)
                    //            {
                    //                if (units[Checkpos2.x, Checkpos2.y].Player == Player) break;
                    //                ret.Add(Checkpos2);
                    //                break;
                    //            }
                    //            ret.Add(Checkpos2);
                    //        }
                    //        ret.Add(checkpos2);
                    //    }
                    //    else if (units[checkpos2.x, checkpos2.y].Player != Player)
                    //    {
                    //        ret.Add(checkpos2);
                    //    }
                    //}

                    // ＋香車の動き（親：角行、桂馬）　前方向に増加
                    if (UnitType == UnitType.kakugyou || UnitType == UnitType.keima)
                    {
                        Fowards = 1;
                    }
                    // ＋香車の動き（親：歩兵、銀将、金将、成り駒、竜馬）　前方向2マス目から増加
                    if (UnitType == UnitType.huhyou || UnitType == UnitType.ginsyou || UnitType == UnitType.kinsyou || UnitType == UnitType.narigoma || UnitType == UnitType.ryuuma)
                    {
                        Fowards = 2;
                    }

                    // 増加分（前方向）
                    for (int i = Fowards; i < 9; i++)
                    {
                        Vector2Int forward = new Vector2Int(0, dir * i);
                        Vector2Int checkpos = Pos + forward;
                        if (!isCheckable(units, checkpos)) break;
                        if (units[Checkpos.x, Checkpos.y] != null)
                        {
                            if (units[Checkpos.x, Checkpos.y].Player != Player)
                            {
                                ret.Add(Checkpos);
                                break;
                            }
                            else break;
                        }
                        if (units[checkpos.x, checkpos.y] != null)
                        {
                            if (units[checkpos.x, checkpos.y].Player == Player) break;
                            ret.Add(checkpos);
                            break;
                        }
                        ret.Add(checkpos);
                    }
                }
                break;

            case UnitType.keima:  // 吸収された駒が桂馬
                {
                    // プレイヤーによって向きを変える
                    int dir = (Player == 0) ? -1 : 1;
                    //桂馬桂馬
                    //// ＋桂馬の動き（親：桂馬）　後ろ方向に増加
                    //if (UnitType == UnitType.keima)
                    //{
                    //    //dir *= -1;

                    //    // 太く
                    //    Vector2Int[] knightMoves1 = { new Vector2Int(2, 2 * dir), new Vector2Int(-2, 2 * dir), new Vector2Int(0, 2 * dir) };
                    //    foreach (var move in knightMoves1)
                    //    {
                    //        Vector2Int checkpos = Pos + move;
                    //        if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                    //        {
                    //            ret.Add(checkpos);
                    //        }
                    //    }

                    //}
                    //// ＋桂馬の動き（親：桂馬以外）　前方向に増加
                    //else
                    {
                        dir *= 1;
                    }

                    // 増加分
                    Vector2Int[] knightMoves = { new Vector2Int(1, 2 * dir), new Vector2Int(-1, 2 * dir) };
                    foreach (var move in knightMoves)
                    {
                        Vector2Int checkpos = Pos + move;
                        if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                        {
                            ret.Add(checkpos);
                        }
                    }
                }
                break;

            case UnitType.ginsyou: // 吸収された駒が銀将
                {
                    // プレイヤーによって向きを変える
                    int dir = (Player == 0) ? -1 : 1;
                    // 各方向のスタート位置
                    int Fowards = 0;
                    int Foward_Rights = 0;
                    int Foward_Lefts = 0;
                    int Back_Rights = 0;
                    int Back_Lefts = 0;

                    // ＋銀将の動き（親：歩兵）
                    if (UnitType == UnitType.huhyou)
                    {
                        Fowards = 2;
                        Foward_Rights = 1;
                        Foward_Lefts = 1;
                        Back_Rights = 1;
                        Back_Lefts = 1;
                    }
                    // ＋銀将の動き（親：角行）
                    if (UnitType == UnitType.kakugyou)
                    {
                        Fowards = 1;
                    }
                    // ＋銀将の動き（親：飛車、香車）
                    if (UnitType == UnitType.hisya || UnitType == UnitType.kyousya)
                    {
                        Foward_Rights = 1;
                        Foward_Lefts = 1;
                        Back_Rights = 1;
                        Back_Lefts = 1;
                    }
                    // ＋銀将の動き（親：桂馬）
                    if (UnitType == UnitType.keima)
                    {
                        Fowards = 1;
                        Foward_Rights = 1;
                        Foward_Lefts = 1;
                        Back_Rights = 1;
                        Back_Lefts = 1;
                    }
                    // ＋銀将の動き（親：銀将）
                    if (UnitType == UnitType.ginsyou)
                    {
                        Fowards = 2;
                        Foward_Rights = 2;
                        Foward_Lefts = 2;
                        Back_Rights = 2;
                        Back_Lefts = 2;
                    }
                    // ＋銀将の動き（親：金将、成り駒）
                    if (UnitType == UnitType.kinsyou || UnitType == UnitType.narigoma)
                    {
                        Fowards = 2;
                        Foward_Rights = 2;
                        Foward_Lefts = 2;
                        Back_Rights = 1;
                        Back_Lefts = 1;
                    }
                    // ＋銀将の動き（親：竜馬）
                    if (UnitType == UnitType.ryuuma)
                    {
                        Fowards = 2;
                    }
                    // ＋銀将の動き（親：龍王）
                    if (UnitType == UnitType.ryuuou)
                    {
                        Foward_Rights = 2;
                        Foward_Lefts = 2;
                        Back_Rights = 2;
                        Back_Lefts = 2;
                    }

                    // 前
                    Vector2Int Foward = new Vector2Int(0, dir);
                    Vector2Int Checkpos1 = Pos + Foward;
                    Vector2Int foward = new Vector2Int(0, dir * Fowards);
                    Vector2Int checkpos1 = Pos + foward;
                    if (isCheckable(units, checkpos1))
                    {
                        if (units[Checkpos1.x, Checkpos1.y] == null)
                        {
                            if (units[checkpos1.x, checkpos1.y] == null || units[checkpos1.x, checkpos1.y].Player != Player)
                            {
                                ret.Add(checkpos1);
                            }
                        }
                        else if (units[Checkpos1.x, Checkpos1.y].Player != Player)
                        {
                            ret.Add(Checkpos1);
                        }
                    }
                    // 右前
                    Vector2Int Foward_Right = new Vector2Int(-1, dir);
                    Vector2Int Checkpos2 = Pos + Foward_Right;
                    Vector2Int foward_right = new Vector2Int(-1 * Foward_Rights, dir * Foward_Rights);
                    Vector2Int checkpos2 = Pos + foward_right;
                    if (isCheckable(units, checkpos2))
                    {
                        if (units[Checkpos2.x, Checkpos2.y] == null)
                        {
                            if (units[checkpos2.x, checkpos2.y] == null || units[checkpos2.x, checkpos2.y].Player != Player)
                            {
                                ret.Add(checkpos2);
                            }
                        }
                        else if (units[Checkpos2.x, Checkpos2.y].Player != Player)
                        {
                            ret.Add(Checkpos2);
                        }
                    }
                    // 左前
                    Vector2Int Foward_Left = new Vector2Int(1, dir);
                    Vector2Int Checkpos3 = Pos + Foward_Left;
                    Vector2Int foward_left = new Vector2Int(1 * Foward_Lefts, dir * Foward_Lefts);
                    Vector2Int checkpos3 = Pos + foward_left;
                    if (isCheckable(units, checkpos3))
                    {
                        if (units[Checkpos3.x, Checkpos3.y] == null)
                        {
                            if (units[checkpos3.x, checkpos3.y] == null || units[checkpos3.x, checkpos3.y].Player != Player)
                            {
                                ret.Add(checkpos3);
                            }
                        }
                        else if (units[Checkpos3.x, Checkpos3.y].Player != Player)
                        {
                            ret.Add(Checkpos3);
                        }
                    }
                    // 右後ろ
                    Vector2Int Back_Right = new Vector2Int(-1, -dir);
                    Vector2Int Checkpos4 = Pos + Back_Right;
                    Vector2Int back_right = new Vector2Int(-1 * Back_Rights, -dir * Back_Rights);
                    Vector2Int checkpos4 = Pos + back_right;
                    if (isCheckable(units, checkpos4))
                    {
                        if (units[Checkpos4.x, Checkpos4.y] == null)
                        {
                            if (units[checkpos4.x, checkpos4.y] == null || units[checkpos4.x, checkpos4.y].Player != Player)
                            {
                                ret.Add(checkpos4);
                            }
                        }
                        else if (units[Checkpos4.x, Checkpos4.y].Player != Player)
                        {
                            ret.Add(Checkpos4);
                        }
                    }
                    // 左後ろ
                    Vector2Int Back_Left = new Vector2Int(1, -dir);
                    Vector2Int Checkpos5 = Pos + Back_Left;
                    Vector2Int back_left = new Vector2Int(1 * Back_Lefts, -dir * Back_Lefts);
                    Vector2Int checkpos5 = Pos + back_left;
                    if (isCheckable(units, checkpos5))
                    {
                        if (units[Checkpos5.x, Checkpos5.y] == null)
                        {
                            if (units[checkpos5.x, checkpos5.y] == null || units[checkpos5.x, checkpos5.y].Player != Player)
                            {
                                ret.Add(checkpos5);
                            }
                        }
                        else if (units[Checkpos5.x, Checkpos5.y].Player != Player)
                        {
                            ret.Add(Checkpos5);
                        }
                    }
                }
                break;

            case UnitType.kinsyou: // 吸収された駒が金将
                {
                    // プレイヤーによって向きを変える
                    int dir = (Player == 0) ? -1 : 1;
                    // 各方向のスタート位置
                    int Fowards = 0;
                    int Foward_Rights = 0;
                    int Foward_Lefts = 0;
                    int Rights = 0;
                    int Lefts = 0;
                    int Backs = 0;

                    // ＋金将の動き（親：歩兵）　前方向2マス目から増加。その他、普通に増加
                    if (UnitType == UnitType.huhyou)
                    {
                        Fowards = 2;
                        Foward_Rights = 1;
                        Foward_Lefts = 1;
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // ＋金将の動き（親：角行）　縦横1マス目に増加
                    if (UnitType == UnitType.kakugyou)
                    {
                        Fowards = 1;
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // ＋金将の動き（親：飛車）　斜め前方向に増加
                    if (UnitType == UnitType.hisya)
                    {
                        Foward_Rights = 1;
                        Foward_Lefts = 1;
                    }
                    // ＋金将の動き（親：香車）　前方向以外に普通に増加
                    if (UnitType == UnitType.kyousya)
                    {
                        Foward_Rights = 1;
                        Foward_Lefts = 1;
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // ＋金将の動き（親：桂馬）　普通に増加
                    if (UnitType == UnitType.keima)
                    {
                        Fowards = 1;
                        Foward_Rights = 1;
                        Foward_Lefts = 1;
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // ＋金将の動き（親：銀将）　前、斜め前方向2マス目に増加。その他、普通に増加。
                    if (UnitType == UnitType.ginsyou)
                    {
                        Fowards = 2;
                        Foward_Rights = 2;
                        Foward_Lefts = 2;
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // ＋金将の動き（親：金将、成り駒）　各方向2マス目に増加
                    if (UnitType == UnitType.kinsyou || UnitType == UnitType.narigoma)
                    {
                        Fowards = 2;
                        Foward_Rights = 2;
                        Foward_Lefts = 2;
                        Rights = 2;
                        Lefts = 2;
                        Backs = 2;
                    }
                    // ＋金将の動き（親：竜馬）　縦横2マス目に増加
                    if (UnitType == UnitType.ryuuma)
                    {
                        Fowards = 2;
                        Rights = 2;
                        Lefts = 2;
                        Backs = 2;
                    }
                    // ＋金将の動き（親：龍王）　斜め前方向2マス目に増加
                    if (UnitType == UnitType.ryuuou)
                    {
                        Foward_Rights = 2;
                        Foward_Lefts = 2;
                    }

                    // 前
                    Vector2Int Foward = new Vector2Int(0, dir);
                    Vector2Int Checkpos1 = Pos + Foward;
                    Vector2Int foward = new Vector2Int(0, dir * Fowards);
                    Vector2Int checkpos1 = Pos + foward;
                    if (isCheckable(units, checkpos1))
                    {
                        if (units[Checkpos1.x, Checkpos1.y] == null)
                        {
                            if (units[checkpos1.x, checkpos1.y] == null || units[checkpos1.x, checkpos1.y].Player != Player)
                            {
                                ret.Add(checkpos1);
                            }
                        }
                        else if (units[Checkpos1.x, Checkpos1.y].Player != Player)
                        {
                            ret.Add(Checkpos1);
                        }
                    }
                    // 右前
                    Vector2Int Foward_Right = new Vector2Int(-1, dir);
                    Vector2Int Checkpos2 = Pos + Foward_Right;
                    Vector2Int foward_right = new Vector2Int(-1 * Foward_Rights, dir * Foward_Rights);
                    Vector2Int checkpos2 = Pos + foward_right;
                    if (isCheckable(units, checkpos2))
                    {
                        if (units[Checkpos2.x, Checkpos2.y] == null)
                        {
                            if (units[checkpos2.x, checkpos2.y] == null || units[checkpos2.x, checkpos2.y].Player != Player)
                            {
                                ret.Add(checkpos2);
                            }
                        }
                        else if (units[Checkpos2.x, Checkpos2.y].Player != Player)
                        {
                            ret.Add(Checkpos2);
                        }
                    }
                    // 左前
                    Vector2Int Foward_Left = new Vector2Int(1, dir);
                    Vector2Int Checkpos3 = Pos + Foward_Left;
                    Vector2Int foward_left = new Vector2Int(1 * Foward_Lefts, dir * Foward_Lefts);
                    Vector2Int checkpos3 = Pos + foward_left;
                    if (isCheckable(units, checkpos3))
                    {
                        if (units[Checkpos3.x, Checkpos3.y] == null)
                        {
                            if (units[checkpos3.x, checkpos3.y] == null || units[checkpos3.x, checkpos3.y].Player != Player)
                            {
                                ret.Add(checkpos3);
                            }
                        }
                        else if (units[Checkpos3.x, Checkpos3.y].Player != Player)
                        {
                            ret.Add(Checkpos3);
                        }
                    }
                    // 右
                    Vector2Int Right = new Vector2Int(-1, 0);
                    Vector2Int Checkpos4 = Pos + Right;
                    Vector2Int right = new Vector2Int(-1 * Rights, 0);
                    Vector2Int checkpos4 = Pos + right;
                    if (isCheckable(units, checkpos4))
                    {
                        if (units[Checkpos4.x, Checkpos4.y] == null)
                        {
                            if (units[checkpos4.x, checkpos4.y] == null || units[checkpos4.x, checkpos4.y].Player != Player)
                            {
                                ret.Add(checkpos4);
                            }
                        }
                        else if (units[Checkpos4.x, Checkpos4.y].Player != Player)
                        {
                            ret.Add(Checkpos4);
                        }
                    }
                    // 左
                    Vector2Int Left = new Vector2Int(1, 0);
                    Vector2Int Checkpos5 = Pos + Left;
                    Vector2Int left = new Vector2Int(1 * Lefts, 0);
                    Vector2Int checkpos5 = Pos + left;
                    if (isCheckable(units, checkpos3))
                    {
                        if (units[Checkpos5.x, Checkpos5.y] == null)
                        {
                            if (units[checkpos5.x, checkpos5.y] == null || units[checkpos5.x, checkpos5.y].Player != Player)
                            {
                                ret.Add(checkpos5);
                            }
                        }
                        else if (units[Checkpos5.x, Checkpos5.y].Player != Player)
                        {
                            ret.Add(Checkpos5);
                        }
                    }
                    // 前
                    Vector2Int Back = new Vector2Int(0, -dir);
                    Vector2Int Checkpos6 = Pos + Back;
                    Vector2Int back = new Vector2Int(0, -dir * Backs);
                    Vector2Int checkpos6 = Pos + back;
                    if (isCheckable(units, checkpos6))
                    {
                        if (units[Checkpos6.x, Checkpos6.y] == null)
                        {
                            if (units[checkpos6.x, checkpos6.y] == null || units[checkpos6.x, checkpos6.y].Player != Player)
                            {
                                ret.Add(checkpos6);
                            }
                        }
                        else if (units[Checkpos6.x, Checkpos6.y].Player != Player)
                        {
                            ret.Add(Checkpos6);
                        }
                    }
                }
                break;

            case UnitType.hisya: // 吸収された駒が飛車
                {
                    // プレイヤーによって向きを変える
                    int dir = (Player == 0) ? -1 : 1;
                    // 縦横4方向のスタート位置
                    int Fowards = 0;
                    int Rights = 0;
                    int Lefts = 0;
                    int Backs = 0;

                    // ＋飛車の動き（親：歩兵、銀将）　前方向のみ2マス目から増加。その他、普通に増加。
                    if (UnitType == UnitType.huhyou || UnitType == UnitType.ginsyou)
                    {
                        Fowards = 2;
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // ＋飛車の動き（親：角行、桂馬）　各方向普通に増加。
                    if (UnitType == UnitType.kakugyou || UnitType == UnitType.keima)
                    {
                        Fowards = 1;
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // ＋飛車の動き（親：香車）　前方向以外の3方向増加。
                    if (UnitType == UnitType.kyousya)
                    {
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // ＋飛車の動き（親：金将、成り駒、竜馬）　各方向2マス目から増加。
                    if (UnitType == UnitType.kinsyou || UnitType == UnitType.narigoma || UnitType == UnitType.ryuuma)
                    {
                        Fowards = 2;
                        Rights = 2;
                        Lefts = 2;
                        Backs = 2;
                    }

                    // ＋飛車の動き（親：飛車）※例外につき注意
                    if (UnitType == UnitType.hisya)
                    {
                        Vector2Int offset1 = new Vector2Int(1, 1); // 左下
                        Vector2Int offset2 = new Vector2Int(-1, 1); // 右下
                        Vector2Int offset3 = new Vector2Int(-1, -1); // 右上
                        Vector2Int offset4 = new Vector2Int(1, -1); // 左上

                        Vector2Int[] direction1 = { new Vector2Int(1, 1) }; // 左下
                        foreach (var dir1 in direction1)
                        {
                            Vector2Int checkpos = Pos + dir1;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
                            {
                                Vector2Int newCenterPos1 = Pos + offset1;
                                Vector2Int[] directions1 = { new Vector2Int(0, 1), new Vector2Int(1, 0) }; // 左下
                                foreach (var dire in directions1)
                                {
                                    for (int i = 1; i < 9; i++)
                                    {
                                        Vector2Int Checkpos = newCenterPos1 + dire * i;
                                        if (!isCheckable(units, Checkpos)) break;
                                        if (units[Checkpos.x, Checkpos.y] != null)
                                        {
                                            if (units[Checkpos.x, Checkpos.y].Player == Player) break;
                                            ret.Add(Checkpos);
                                            break;
                                        }
                                        ret.Add(Checkpos);
                                    }
                                }
                                ret.Add(checkpos);
                            }
                            else
                            {
                                ret.Add(checkpos);
                            }
                        }

                        Vector2Int[] direction2 = { new Vector2Int(-1, 1) }; // 右下
                        foreach (var dir2 in direction2)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
                            {
                                Vector2Int newCenterPos2 = Pos + offset2;
                                Vector2Int[] directions2 = { new Vector2Int(0, 1), new Vector2Int(-1, 0) }; // 右下
                                foreach (var dire in directions2)
                                {
                                    for (int i = 1; i < 9; i++)
                                    {
                                        Vector2Int Checkpos = newCenterPos2 + dire * i;
                                        if (!isCheckable(units, Checkpos)) break;
                                        if (units[Checkpos.x, Checkpos.y] != null)
                                        {
                                            if (units[Checkpos.x, Checkpos.y].Player == Player) break;
                                            ret.Add(Checkpos);
                                            break;
                                        }
                                        ret.Add(Checkpos);
                                    }
                                }
                                ret.Add(checkpos);
                            }
                            else
                            {
                                ret.Add(checkpos);
                            }
                        }

                        Vector2Int[] direction3 = { new Vector2Int(-1, -1) }; // 右上
                        foreach (var dir2 in direction3)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
                            {
                                Vector2Int newCenterPos3 = Pos + offset3;
                                Vector2Int[] directions3 = { new Vector2Int(0, -1), new Vector2Int(-1, 0) }; // 右上
                                foreach (var dire in directions3)
                                {
                                    for (int i = 1; i < 9; i++)
                                    {
                                        Vector2Int Checkpos = newCenterPos3 + dire * i;
                                        if (!isCheckable(units, Checkpos)) break;
                                        if (units[Checkpos.x, Checkpos.y] != null)
                                        {
                                            if (units[Checkpos.x, Checkpos.y].Player == Player) break;
                                            ret.Add(Checkpos);
                                            break;
                                        }
                                        ret.Add(Checkpos);
                                    }
                                }
                                ret.Add(checkpos);
                            }
                            else
                            {
                                ret.Add(checkpos);
                            }
                        }

                        Vector2Int[] direction4 = { new Vector2Int(1, -1) }; // 左上
                        foreach (var dir2 in direction4)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
                            {
                                Vector2Int newCenterPos4 = Pos + offset4;
                                Vector2Int[] directions4 = { new Vector2Int(0, -1), new Vector2Int(1, 0) }; // 左上
                                foreach (var dire in directions4)
                                {
                                    for (int i = 1; i < 9; i++)
                                    {
                                        Vector2Int Checkpos = newCenterPos4 + dire * i;
                                        if (!isCheckable(units, Checkpos)) break;
                                        if (units[Checkpos.x, Checkpos.y] != null)
                                        {
                                            if (units[Checkpos.x, Checkpos.y].Player == Player) break;
                                            ret.Add(Checkpos);
                                            break;
                                        }
                                        ret.Add(Checkpos);
                                    }
                                }
                                ret.Add(checkpos);
                            }
                            else
                            {
                                ret.Add(checkpos);
                            }
                        }
                    }

                    // ＋飛車の動き（親：龍王） ※例外につき注意
                    if (UnitType == UnitType.ryuuou)
                    {
                        Vector2Int offset1 = new Vector2Int(1, 1); // 左下
                        Vector2Int offset2 = new Vector2Int(-1, 1); // 右下
                        Vector2Int offset3 = new Vector2Int(-1, -1); // 右上
                        Vector2Int offset4 = new Vector2Int(1, -1); // 左上

                        Vector2Int[] direction1 = { new Vector2Int(1, 1) }; // 左下
                        foreach (var dir1 in direction1)
                        {
                            Vector2Int checkpos = Pos + dir1;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
                            {
                                Vector2Int newCenterPos1 = Pos + offset1;
                                Vector2Int[] directions1 = { new Vector2Int(0, 1), new Vector2Int(1, 0) }; // 左下
                                foreach (var dire in directions1)
                                {
                                    for (int i = 1; i < 9; i++)
                                    {
                                        Vector2Int Checkpos = newCenterPos1 + dire * i;
                                        if (!isCheckable(units, Checkpos)) break;
                                        if (units[Checkpos.x, Checkpos.y] != null)
                                        {
                                            if (units[Checkpos.x, Checkpos.y].Player == Player) break;
                                            ret.Add(Checkpos);
                                            break;
                                        }
                                        ret.Add(Checkpos);
                                    }
                                }
                            }
                        }

                        Vector2Int[] direction2 = { new Vector2Int(-1, 1) }; // 右下
                        foreach (var dir2 in direction2)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
                            {
                                Vector2Int newCenterPos2 = Pos + offset2;
                                Vector2Int[] directions2 = { new Vector2Int(0, 1), new Vector2Int(-1, 0) }; // 右下
                                foreach (var dire in directions2)
                                {
                                    for (int i = 1; i < 9; i++)
                                    {
                                        Vector2Int Checkpos = newCenterPos2 + dire * i;
                                        if (!isCheckable(units, Checkpos)) break;
                                        if (units[Checkpos.x, Checkpos.y] != null)
                                        {
                                            if (units[Checkpos.x, Checkpos.y].Player == Player) break;
                                            ret.Add(Checkpos);
                                            break;
                                        }
                                        ret.Add(Checkpos);
                                    }
                                }
                            }
                        }

                        Vector2Int[] direction3 = { new Vector2Int(-1, -1) }; // 右上
                        foreach (var dir2 in direction3)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
                            {
                                Vector2Int newCenterPos3 = Pos + offset3;
                                Vector2Int[] directions3 = { new Vector2Int(0, -1), new Vector2Int(-1, 0) }; // 右上
                                foreach (var dire in directions3)
                                {
                                    for (int i = 1; i < 9; i++)
                                    {
                                        Vector2Int Checkpos = newCenterPos3 + dire * i;
                                        if (!isCheckable(units, Checkpos)) break;
                                        if (units[Checkpos.x, Checkpos.y] != null)
                                        {
                                            if (units[Checkpos.x, Checkpos.y].Player == Player) break;
                                            ret.Add(Checkpos);
                                            break;
                                        }
                                        ret.Add(Checkpos);
                                    }
                                }
                            }
                        }

                        Vector2Int[] direction4 = { new Vector2Int(1, -1) }; // 左上
                        foreach (var dir2 in direction4)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
                            {
                                Vector2Int newCenterPos4 = Pos + offset4;
                                Vector2Int[] directions4 = { new Vector2Int(0, -1), new Vector2Int(1, 0) }; // 左上
                                foreach (var dire in directions4)
                                {
                                    for (int i = 1; i < 9; i++)
                                    {
                                        Vector2Int Checkpos = newCenterPos4 + dire * i;
                                        if (!isCheckable(units, Checkpos)) break;
                                        if (units[Checkpos.x, Checkpos.y] != null)
                                        {
                                            if (units[Checkpos.x, Checkpos.y].Player == Player) break;
                                            ret.Add(Checkpos);
                                            break;
                                        }
                                        ret.Add(Checkpos);
                                    }
                                }
                            }
                        }
                    }

                    // 前
                    Vector2Int Foward = new Vector2Int(0, dir);
                    Vector2Int Checkpos1 = Pos + Foward;
                    for (int i = Fowards; i < 9; i++)
                    {
                        Vector2Int forward = new Vector2Int(0, dir * i);
                        Vector2Int checkpos = Pos + forward;
                        if (!isCheckable(units, checkpos)) break;
                        if (units[Checkpos1.x, Checkpos1.y] != null)
                        {
                            if (units[Checkpos1.x, Checkpos1.y].Player != Player)
                            {
                                ret.Add(Checkpos1);
                                break;
                            }
                            else break;
                        }
                        if (units[checkpos.x, checkpos.y] != null)
                        {
                            if (units[checkpos.x, checkpos.y].Player == Player) break;
                            ret.Add(checkpos);
                            break;
                        }
                        ret.Add(checkpos);
                    }
                    // 後ろ
                    Vector2Int Back = new Vector2Int(0, -dir);
                    Vector2Int Checkpos2 = Pos + Back;
                    for (int i = Backs; i < 9; i++)
                    {
                        Vector2Int back = new Vector2Int(0, -dir * i);
                        Vector2Int checkpos2 = Pos + back;
                        if (!isCheckable(units, checkpos2)) break;
                        if (units[Checkpos2.x, Checkpos2.y] != null)
                        {
                            if (units[Checkpos2.x, Checkpos2.y].Player != Player)
                            {
                                ret.Add(Checkpos2);
                                break;
                            }
                            else break;
                        }
                        if (units[checkpos2.x, checkpos2.y] != null)
                        {
                            if (units[checkpos2.x, checkpos2.y].Player == Player) break;
                            ret.Add(checkpos2);
                            break;
                        }
                        ret.Add(checkpos2);
                    }
                    // 右
                    Vector2Int Right = new Vector2Int(-1, 0);
                    Vector2Int Checkpos3 = Pos + Right;
                    for (int i = Rights; i < 9; i++)
                    {
                        Vector2Int right = new Vector2Int(-i, 0);
                        Vector2Int checkpos3 = Pos + right;
                        if (!isCheckable(units, checkpos3)) break;
                        if (units[Checkpos3.x, Checkpos3.y] != null)
                        {
                            if (units[Checkpos3.x, Checkpos3.y].Player != Player)
                            {
                                ret.Add(Checkpos3);
                                break;
                            }
                            else break;
                        }
                        if (units[checkpos3.x, checkpos3.y] != null)
                        {
                            if (units[checkpos3.x, checkpos3.y].Player == Player) break;
                            ret.Add(checkpos3);
                            break;
                        }
                        ret.Add(checkpos3);
                    }
                    // 左
                    Vector2Int Left = new Vector2Int(1, 0);
                    Vector2Int Checkpos4 = Pos + Left;
                    for (int i = Lefts; i < 9; i++)
                    {
                        Vector2Int left = new Vector2Int(i, 0);
                        Vector2Int checkpos4 = Pos + left;
                        if (!isCheckable(units, checkpos4)) break;
                        if (units[Checkpos4.x, Checkpos4.y] != null)
                        {
                            if (units[Checkpos4.x, Checkpos4.y].Player != Player)
                            {
                                ret.Add(Checkpos4);
                                break;
                            }
                            else break;
                        }
                            if (units[checkpos4.x, checkpos4.y] != null)
                        {
                            if (units[checkpos4.x, checkpos4.y].Player == Player) break;
                            ret.Add(checkpos4);
                            break;
                        }
                        ret.Add(checkpos4);
                    }
                }
                break;

            case UnitType.kakugyou: // 吸収された駒が角行
                {
                    // プレイヤーによって向きを変える
                    int dir = (Player == 0) ? -1 : 1;
                    // 斜め4方向のスタート位置
                    int Foward_Left = 0;   // 左前
                    int Foward_Right = 0;  // 右前
                    int Back_Right = 0;    // 右下
                    int Back_Left = 0;     // 左下

                    // ＋角行の動き（親：歩兵、飛車、香車、桂馬）　各方向普通に増加。
                    if (UnitType == UnitType.huhyou || UnitType == UnitType.hisya || UnitType == UnitType.kyousya || UnitType == UnitType.keima)
                    {
                        Foward_Left = 1;
                        Foward_Right = 1;
                        Back_Right = 1;
                        Back_Left = 1;

                    }
                    // ＋角行の動き（親：銀将、龍王）　各方向2マス目から増加。
                    if (UnitType == UnitType.ginsyou || UnitType == UnitType.ryuuou)
                    {
                        Foward_Left = 2;
                        Foward_Right = 2;
                        Back_Right = 2;
                        Back_Left = 2;
                    }
                    // ＋角行の動き（親：金将、成り駒）　前方向のみ2マス目から増加。その他、普通に増加。
                    if (UnitType == UnitType.kinsyou || UnitType == UnitType.narigoma)
                    {
                        Foward_Left = 2;
                        Foward_Right = 2;
                        Back_Right = 1;
                        Back_Left = 1;
                    }

                    // ＋角行の動き（親：角行）※例外につき注意
                    if (UnitType == UnitType.kakugyou)
                    {
                        Vector2Int offset1 = new Vector2Int(0, -1); // 上
                        Vector2Int offset2 = new Vector2Int(0, 1); // 下
                        Vector2Int offset3 = new Vector2Int(-1, 0); // 右
                        Vector2Int offset4 = new Vector2Int(1, 0); // 左

                        Vector2Int[] direction1 = { new Vector2Int(0, -1) };
                        foreach (var dir1 in direction1)
                        {
                            Vector2Int checkpos = Pos + dir1;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
                            {
                                Vector2Int newCenterPos1 = Pos + offset1;
                                Vector2Int[] directions1 = { new Vector2Int(1, -1), new Vector2Int(-1, -1) };
                                foreach (var dire in directions1)
                                {
                                    for (int i = 1; i < 9; i++)
                                    {
                                        Vector2Int Checkpos = newCenterPos1 + dire * i;
                                        if (!isCheckable(units, Checkpos)) break;
                                        if (units[Checkpos.x, Checkpos.y] != null)
                                        {
                                            if (units[Checkpos.x, Checkpos.y].Player == Player) break;
                                            ret.Add(Checkpos);
                                            break;
                                        }
                                        ret.Add(Checkpos);
                                    }
                                }
                                ret.Add(checkpos);
                            }
                            else
                            {
                                ret.Add(checkpos);
                            }
                        }

                        Vector2Int[] direction2 = { new Vector2Int(0, 1) };
                        foreach (var dir2 in direction2)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
                            {
                                Vector2Int newCenterPos2 = Pos + offset2;
                                Vector2Int[] directions2 = { new Vector2Int(1, 1), new Vector2Int(-1, 1) };
                                foreach (var dire in directions2)
                                {
                                    for (int i = 1; i < 9; i++)
                                    {
                                        Vector2Int Checkpos = newCenterPos2 + dire * i;
                                        if (!isCheckable(units, Checkpos)) break;
                                        if (units[Checkpos.x, Checkpos.y] != null)
                                        {
                                            if (units[Checkpos.x, Checkpos.y].Player == Player) break;
                                            ret.Add(Checkpos);
                                            break;
                                        }
                                        ret.Add(Checkpos);
                                    }
                                }
                                ret.Add(checkpos);
                            }
                            else
                            {
                                ret.Add(checkpos);
                            }
                        }

                        Vector2Int[] direction3 = { new Vector2Int(-1, 0) };
                        foreach (var dir3 in direction3)
                        {
                            Vector2Int checkpos = Pos + dir3;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
                            {
                                Vector2Int newCenterPos3 = Pos + offset3;
                                Vector2Int[] directions3 = { new Vector2Int(-1, -1), new Vector2Int(-1, 1) };
                                foreach (var dire in directions3)
                                {
                                    for (int i = 1; i < 9; i++)
                                    {
                                        Vector2Int Checkpos = newCenterPos3 + dire * i;
                                        if (!isCheckable(units, Checkpos)) break;
                                        if (units[Checkpos.x, Checkpos.y] != null)
                                        {
                                            if (units[Checkpos.x, Checkpos.y].Player == Player) break;
                                            ret.Add(Checkpos);
                                            break;
                                        }
                                        ret.Add(Checkpos);
                                    }
                                }
                                ret.Add(checkpos);
                            }
                            else
                            {
                                ret.Add(checkpos);
                            }
                        }

                        Vector2Int[] direction4 = { new Vector2Int(1, 0) };
                        foreach (var dir4 in direction4)
                        {
                            Vector2Int checkpos = Pos + dir4;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
                            {
                                Vector2Int newCenterPos4 = Pos + offset4;
                                Vector2Int[] directions4 = { new Vector2Int(1, -1), new Vector2Int(1, 1) };
                                foreach (var dire in directions4)
                                {
                                    for (int i = 1; i < 9; i++)
                                    {
                                        Vector2Int Checkpos = newCenterPos4 + dire * i;
                                        if (!isCheckable(units, Checkpos)) break;
                                        if (units[Checkpos.x, Checkpos.y] != null)
                                        {
                                            if (units[Checkpos.x, Checkpos.y].Player == Player) break;
                                            ret.Add(Checkpos);
                                            break;
                                        }
                                        ret.Add(Checkpos);
                                    }
                                }
                                ret.Add(checkpos);
                            }
                            else
                            {
                                ret.Add(checkpos);
                            }
                        }
                    }

                    // ＋角行の動き（親：竜馬） ※例外につき注意
                    if (UnitType == UnitType.ryuuma)
                    {
                        Vector2Int offset1 = new Vector2Int(0, -1); // 上
                        Vector2Int offset2 = new Vector2Int(0, 1); // 下
                        Vector2Int offset3 = new Vector2Int(-1, 0); // 右
                        Vector2Int offset4 = new Vector2Int(1, 0); // 左

                        Vector2Int[] direction1 = { new Vector2Int(0, -1) };
                        foreach (var dir1 in direction1)
                        {
                            Vector2Int checkpos = Pos + dir1;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
                            {
                                Vector2Int newCenterPos1 = Pos + offset1;
                                Vector2Int[] directions1 = { new Vector2Int(1, -1), new Vector2Int(-1, -1) };
                                foreach (var dire in directions1)
                                {
                                    for (int i = 1; i < 9; i++)
                                    {
                                        Vector2Int Checkpos = newCenterPos1 + dire * i;
                                        if (!isCheckable(units, Checkpos)) break;
                                        if (units[Checkpos.x, Checkpos.y] != null)
                                        {
                                            if (units[Checkpos.x, Checkpos.y].Player == Player) break;
                                            ret.Add(Checkpos);
                                            break;
                                        }
                                        ret.Add(Checkpos);
                                    }
                                }
                            }
                        }

                        Vector2Int[] direction2 = { new Vector2Int(0, 1) };
                        foreach (var dir2 in direction2)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
                            {
                                Vector2Int newCenterPos2 = Pos + offset2;
                                Vector2Int[] directions2 = { new Vector2Int(1, 1), new Vector2Int(-1, 1) };
                                foreach (var dire in directions2)
                                {
                                    for (int i = 1; i < 9; i++)
                                    {
                                        Vector2Int Checkpos = newCenterPos2 + dire * i;
                                        if (!isCheckable(units, Checkpos)) break;
                                        if (units[Checkpos.x, Checkpos.y] != null)
                                        {
                                            if (units[Checkpos.x, Checkpos.y].Player == Player) break;
                                            ret.Add(Checkpos);
                                            break;
                                        }
                                        ret.Add(Checkpos);
                                    }
                                }
                            }
                        }

                        Vector2Int[] direction3 = { new Vector2Int(-1, 0) };
                        foreach (var dir3 in direction3)
                        {
                            Vector2Int checkpos = Pos + dir3;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
                            {
                                Vector2Int newCenterPos3 = Pos + offset3;
                                Vector2Int[] directions3 = { new Vector2Int(-1, -1), new Vector2Int(-1, 1) };
                                foreach (var dire in directions3)
                                {
                                    for (int i = 1; i < 9; i++)
                                    {
                                        Vector2Int Checkpos = newCenterPos3 + dire * i;
                                        if (!isCheckable(units, Checkpos)) break;
                                        if (units[Checkpos.x, Checkpos.y] != null)
                                        {
                                            if (units[Checkpos.x, Checkpos.y].Player == Player) break;
                                            ret.Add(Checkpos);
                                            break;
                                        }
                                        ret.Add(Checkpos);
                                    }
                                }
                            }
                        }

                        Vector2Int[] direction4 = { new Vector2Int(1, 0) };
                        foreach (var dir4 in direction4)
                        {
                            Vector2Int checkpos = Pos + dir4;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
                            {
                                Vector2Int newCenterPos4 = Pos + offset4;
                                Vector2Int[] directions4 = { new Vector2Int(1, -1), new Vector2Int(1, 1) };
                                foreach (var dire in directions4)
                                {
                                    for (int i = 1; i < 9; i++)
                                    {
                                        Vector2Int Checkpos = newCenterPos4 + dire * i;
                                        if (!isCheckable(units, Checkpos)) break;
                                        if (units[Checkpos.x, Checkpos.y] != null)
                                        {
                                            if (units[Checkpos.x, Checkpos.y].Player == Player) break;
                                            ret.Add(Checkpos);
                                            break;
                                        }
                                        ret.Add(Checkpos);
                                    }
                                }
                            }
                        }
                    }

                    // 左上
                    Vector2Int Foward_left = new Vector2Int(1, dir);
                    Vector2Int Checkpos1 = Pos + Foward_left;
                    for (int i = Foward_Left; i < 9; i++)
                    {
                        Vector2Int foward_left = new Vector2Int(i, dir * i);
                        Vector2Int checkpos1 = Pos + foward_left;
                        if (!isCheckable(units, checkpos1)) break;
                        if (units[Checkpos1.x, Checkpos1.y] != null)
                        {
                            if (units[Checkpos1.x, Checkpos1.y].Player != Player)
                            {
                                ret.Add(Checkpos1);
                                break;
                            }
                            else break;
                        }
                        if (units[checkpos1.x, checkpos1.y] != null)
                        {
                            if (units[checkpos1.x, checkpos1.y].Player == Player) break;
                            ret.Add(checkpos1);
                            break;
                        }
                        ret.Add(checkpos1);
                    }
                    // 右上
                    Vector2Int Foward_right = new Vector2Int(-1, dir);
                    Vector2Int Checkpos2 = Pos + Foward_right;
                    for (int i = Foward_Right; i < 9; i++)
                    {
                        Vector2Int foward_right = new Vector2Int(-i, dir * i);
                        Vector2Int checkpos2 = Pos + foward_right;
                        if (!isCheckable(units, checkpos2)) break;
                        if (units[Checkpos2.x, Checkpos2.y] != null)
                        {
                            if (units[Checkpos2.x, Checkpos2.y].Player != Player)
                            {
                                ret.Add(Checkpos2);
                                break;
                            }
                            else break;
                        }
                        if (units[checkpos2.x, checkpos2.y] != null)
                        {
                            if (units[checkpos2.x, checkpos2.y].Player == Player) break;
                            ret.Add(checkpos2);
                            break;
                        }
                        ret.Add(checkpos2);
                    }
                    // 左下
                    Vector2Int Back_left = new Vector2Int(1, -dir);
                    Vector2Int Checkpos3 = Pos + Back_left;
                    for (int i = Back_Left; i < 9; i++)
                    {
                        Vector2Int back_left = new Vector2Int(i, -dir * i);
                        Vector2Int checkpos3 = Pos + back_left;
                        if (!isCheckable(units, checkpos3)) break;
                        if (units[Checkpos3.x, Checkpos3.y] != null)
                        {
                            if (units[Checkpos3.x, Checkpos3.y].Player != Player)
                            {
                                ret.Add(Checkpos3);
                                break;
                            }
                            else break;
                        }
                        if (units[checkpos3.x, checkpos3.y] != null)
                        {
                            if (units[checkpos3.x, checkpos3.y].Player == Player) break;
                            ret.Add(checkpos3);
                            break;
                        }
                        ret.Add(checkpos3);
                    }
                    // 右下
                    Vector2Int Back_right = new Vector2Int(-1, -dir);
                    Vector2Int Checkpos4 = Pos + Back_right;
                    for (int i = Back_Right; i < 9; i++)
                    {
                        Vector2Int back_right = new Vector2Int(-i, -dir * i);
                        Vector2Int checkpos4 = Pos + back_right;
                        if (!isCheckable(units, checkpos4)) break;
                        if (units[Checkpos4.x, Checkpos4.y] != null)
                        {
                            if (units[Checkpos4.x, Checkpos4.y].Player != Player)
                            {
                                ret.Add(Checkpos4);
                                break;
                            }
                            else break;
                        }
                        if (units[checkpos4.x, checkpos4.y] != null)
                        {
                            if (units[checkpos4.x, checkpos4.y].Player == Player) break;
                            ret.Add(checkpos4);
                            break;
                        }
                        ret.Add(checkpos4);
                    }
                }
                break;
        }
        return ret;
    }
    //--------駒「放出」可能範囲取得処理終わり----------

    //--------プレハブ変更処理開始-----------
    public void ChangePrefab(GameObject newPrefab)
    {
        if (newPrefab == null)
        {
            //Debug.LogError("ChangePrefabメソッドでnewPrefabがnullです。");
            return;
        }

        // 現在の位置情報と回転情報を保存
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;
        Vector2Int currentPosIndex = Pos;

        // 新しいプレハブを生成
        GameObject newUnit = Instantiate(newPrefab, currentPosition, currentRotation);
        UnitController newUnitController = newUnit.GetComponent<UnitController>();

        // Colliderがアタッチされているか確認
        if (newUnitController.GetComponent<Collider>() == null)
        {
            //Debug.LogWarning("新しい駒にはColliderがアタッチされていません。");
        }

        //// 元の駒のRigidbody状態（物理的な挙動）を引き継ぐ
        //Rigidbody oldRigidbody = GetComponent<Rigidbody>();
        //if (oldRigidbody != null)
        //{
        //    Rigidbody newRigidbody = newUnit.GetComponent<Rigidbody>();
        //    if (newRigidbody != null)
        //    {
        //        newRigidbody.isKinematic = oldRigidbody.isKinematic;
        //    }
        //    else
        //    {
        //        Debug.LogWarning("新しい駒にRigidbodyが見つかりません。");
        //    }
        //}

        // ゲームシステムを取得
        GameSystem gameSystem = UnityEngine.Object.FindAnyObjectByType<GameSystem>();
        if (gameSystem == null)
        {
            //Debug.LogError("GameSystemが見つかりませんでした。");
            return;
        }

        // 現在のタイルを取得
        GameObject currentTile = gameSystem.GetTile(currentPosIndex);
        if (currentTile == null)
        {
            //Debug.LogError("指定された位置のタイルが見つかりませんでした。");
            return;
        }

        // 新しい駒にプレイヤーと位置情報を引き継ぐ
        newUnitController.Init(Player, (int)UnitType, currentTile, currentPosIndex);

        // 元のオブジェクトを削除
        Destroy(this.gameObject);
    }

    //--------プレハブ変更処理終わり-----------


    //--------補助メソッド開始-----------

    bool isCheckable(UnitController[,] ary, Vector2Int idx)
    {
        return idx.x >= 0 && idx.x < ary.GetLength(0) && idx.y >= 0 && idx.y < ary.GetLength(1);
    }
    //--------補助メソッド終わり----------
    //---------充填確認--------------------
}