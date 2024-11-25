using UnityEngine;
using System.Collections.Generic;

/*
   吸収可能タイル取得
   駒吸収・放出処理
   その他処理
*/
public class Absorption : MonoBehaviour
{
    public int Player;
    public UnitType UnitType, OldUnitType;
    public FieldStatus FieldStatus;
    public Vector2Int Pos;

    //--------初期化処理開始-----------
    public void Init(int player, int unittype, GameObject tile, Vector2Int pos)
    {
        Player = player;
        UnitType = (UnitType)unittype;
        OldUnitType = (UnitType)unittype;
        FieldStatus = FieldStatus.OnBard;
        Pos = pos;
    }
    //--------初期化処理終わり----------

    //--------吸収可能タイル取得処理開始-----------
    public List<Vector2Int> GetAbsorptionTiles(UnitController[,] units)
    {
        List<Vector2Int> ret = new List<Vector2Int>();
        UnitController unitctrl = GetComponent<UnitController>();

        switch (unitctrl.UnitType)
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
                    // すべての方向を確認
                    foreach (var absor in directions)
                    {
                        Vector2Int checkpos = Pos + absor;
                        if (isCheckable(units, checkpos) && (units[checkpos.x,checkpos.y] == null))
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
                }
                break;
        }
        return ret;
    }
    //--------吸収可能タイル取得処理終わり----------

    //--------駒吸収・放出処理開始-----------
    public List<GameObject> AbsorptionObjcts = new List<GameObject>();
    //public GameObject AbsorptionObj;
    public Vector2Int Absorptionpos;


    //--------その他処理開始-----------
    bool isCheckable(UnitController[,] ary, Vector2Int idx)
    {
        return idx.x >= 0 && idx.x < ary.GetLength(0) && idx.y >= 0 && idx.y < ary.GetLength(1);
    }
    //--------その他処理終わり----------
}
