using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relese : MonoBehaviour
{
    private UnitController unit;         // UnitControllerの参照
    private GameSystem gameSystem;        // GameSystemの参照

    public int Player;  // プレイヤー番号
    public UnitType UnitType, OldUnitType, AbsorUnitType;  // 現在の駒タイプ、成り前の駒タイプ、吸収された駒のタイプ
    public FieldStatus FieldStatus;  // 駒の状態 (盤上、捕獲)
    public Vector2Int Pos;  // 盤上の位置


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
    }

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
    }

    public void AbsorCheck()
    {
        if (gameSystem.absorUnit.UnitType == UnitType.narigoma)// 吸収した駒に吸収された駒のユニットタイプを参照
        {
            //Debug.Log(gameSystem.absorUnit.UnitType + "を吸収したよ");
            //Debug.Log("OldUnitは" + OldUnitType + "だよ");
            AbsorUnitType = OldUnitType;
        }
        else
        {
            //Debug.Log("普通の駒吸収したよ");
            AbsorUnitType = gameSystem.absorUnit.UnitType;
        }
    }

    public List<Vector2Int> GetRelesableTiles(UnitController[,] units)
    {
        List<Vector2Int> ret = new List<Vector2Int>();
        //Debug.Log(Pos);

        // switch() の UnitType は 吸収された駒のタイプ、case 内の UnitType は吸収した駒のタイプ
        switch (AbsorUnitType)
        {
            case UnitType.huhyou: // 吸収された駒が歩兵
                {
                    // プレイヤーによって向きを変える
                    int dir = (Player == 0) ? -1 : 1;
                    // ＋歩兵の動き（親：歩兵、銀将、金将）
                    if (UnitType == UnitType.huhyou || UnitType == UnitType.ginsyou || UnitType == UnitType.kinsyou)
                    {
                        Vector2Int Foward = new Vector2Int(0, dir);
                        Vector2Int Checkpos = Pos + Foward;
                        Vector2Int foward = new Vector2Int(0, dir * 2);
                        Vector2Int checkpos = Pos + foward;
                        if (isCheckable(units, checkpos) && units[Checkpos.x, Checkpos.y] == null && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                        {
                            ret.Add(checkpos);
                        }
                    }
                    // ＋歩兵の動き（親：香車）
                    if (UnitType == UnitType.kyousya)
                    {
                        Vector2Int foward = new Vector2Int(0, -dir);
                        Vector2Int checkpos = Pos + foward;
                        if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                        {
                            ret.Add(checkpos);
                        }
                    }
                    // ＋歩兵の動き（親：角行、桂馬）
                    else if (UnitType == UnitType.kakugyou || UnitType == UnitType.keima)
                    {
                        Vector2Int forward = new Vector2Int(0, dir);
                        Vector2Int checkpos = Pos + forward;
                        if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                        {
                            ret.Add(checkpos);
                        }
                    }
                }
                break;

            case UnitType.kyousya: // 吸収された駒が香車
                {
                    // プレイヤーによって向きを変える
                    int dir = (Player == 0) ? -1 : 1;
                    // ＋香車の動き（親：歩兵、銀将、金将）
                    if (UnitType == UnitType.huhyou || UnitType == UnitType.ginsyou || UnitType == UnitType.kinsyou)
                    {
                        Vector2Int Foward = new Vector2Int(0, dir);
                        Vector2Int Checkpos = Pos + Foward;

                        for (int i = 2; i < 9; i++)
                        {
                            Vector2Int forward = new Vector2Int(0, dir * i);
                            Vector2Int checkpos = Pos + forward;
                            if (!isCheckable(units, checkpos)) break;
                            if (units[Checkpos.x, Checkpos.y] != null) break;
                            if (units[checkpos.x, checkpos.y] != null)
                            {
                                if (units[checkpos.x, checkpos.y].Player == Player) break;
                                ret.Add(checkpos);
                                break;
                            }
                            ret.Add(checkpos);
                        }
                    }
                    // ＋香車の動き（親：香車）
                    if (UnitType == UnitType.kyousya)
                    {
                        Vector2Int Foward = new Vector2Int(0, dir);
                        Vector2Int Checkpos = Pos + Foward;

                        for (int i = 1; i < 9; i++)
                        {
                            Vector2Int forward = new Vector2Int(0, -dir * i);
                            Vector2Int checkpos = Pos + forward;
                            if (!isCheckable(units, checkpos)) break;
                            if (units[Checkpos.x, Checkpos.y] != null) break;
                            if (units[checkpos.x, checkpos.y] != null)
                            {
                                if (units[checkpos.x, checkpos.y].Player == Player) break;
                                ret.Add(checkpos);
                                break;
                            }
                            ret.Add(checkpos);
                        }

                    }
                    // ＋香車の動き（親：角行、桂馬）
                    else if (UnitType == UnitType.kakugyou || UnitType == UnitType.keima)
                    {
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
                }
                break;

            case UnitType.keima:  // 吸収された駒が桂馬
                {
                    // プレイヤーによって向きを変える
                    int dir = (Player == 0) ? -1 : 1;
                    // ＋桂馬の動き（親：桂馬）
                    if (UnitType == UnitType.keima)
                    {
                        Vector2Int[] knightMoves = { new Vector2Int(1, 2 * -dir), new Vector2Int(-1, 2 * -dir) };
                        foreach (var move in knightMoves)
                        {
                            Vector2Int checkpos = Pos + move;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                            {
                                ret.Add(checkpos);
                            }
                        }
                    }
                    // ＋桂馬の動き（親：桂馬以外）
                    else
                    {
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
                }
                break;

            case UnitType.ginsyou: // 吸収された駒が銀将
                {
                    // プレイヤーによって向きを変える
                    int dir = (Player == 0) ? -1 : 1;
                    // ＋銀将の動き（親：歩兵）
                    if (UnitType == UnitType.huhyou)
                    {
                        Vector2Int[] directions =
                        {
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
                        Vector2Int Foward = new Vector2Int(0, dir);
                        Vector2Int Checkpos = Pos + Foward;
                        Vector2Int foward = new Vector2Int(0, dir * 2);
                        Vector2Int checkpos1 = Pos + foward;
                        if (isCheckable(units, checkpos1) && units[Checkpos.x, Checkpos.y] == null && (units[checkpos1.x, checkpos1.y] == null || units[checkpos1.x, checkpos1.y].Player != Player))
                        {
                            ret.Add(checkpos1);
                        }
                    }
                    // ＋銀将の動き（親：角行）
                    if (UnitType == UnitType.kakugyou)
                    {
                        Vector2Int forward = new Vector2Int(0, dir);
                        Vector2Int checkpos = Pos + forward;
                        if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                        {
                            ret.Add(checkpos);
                        }
                    }
                    // ＋銀将の動き（親：飛車、香車）
                    if (UnitType == UnitType.hisya || UnitType == UnitType.kyousya)
                    {
                        Vector2Int[] directions =
                        {
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
                    // ＋銀将の動き（親：桂馬）
                    if (UnitType == UnitType.keima)
                    {
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
                    // ＋銀将の動き（親：銀将）
                    if (UnitType == UnitType.ginsyou)
                    {
                        // 前
                        Vector2Int Foward_up = new Vector2Int(0, dir);
                        Vector2Int Checkpos1 = Pos + Foward_up;
                        Vector2Int foward_up = new Vector2Int(0, dir * 2);
                        Vector2Int checkpos1 = Pos + foward_up;
                        if (isCheckable(units, checkpos1) && units[Checkpos1.x, Checkpos1.y] == null && (units[checkpos1.x, checkpos1.y] == null || units[checkpos1.x, checkpos1.y].Player != Player))
                        {
                            ret.Add(checkpos1);
                        }
                        // 左前
                        Vector2Int Foward_rightup = new Vector2Int(1, dir);
                        Vector2Int Checkpos2 = Pos + Foward_rightup;
                        Vector2Int foward_rightup = new Vector2Int(2, dir * 2);
                        Vector2Int checkpos2 = Pos + foward_rightup;
                        if (isCheckable(units, checkpos2) && units[Checkpos2.x, Checkpos2.y] == null && (units[checkpos2.x, checkpos2.y] == null || units[checkpos2.x, checkpos2.y].Player != Player))
                        {
                            ret.Add(checkpos2);
                        }
                        // 右前
                        Vector2Int Foward_leftup = new Vector2Int(-1, dir);
                        Vector2Int Checkpos3 = Pos + Foward_leftup;
                        Vector2Int foward_leftup = new Vector2Int(-2, dir * 2);
                        Vector2Int checkpos3 = Pos + foward_leftup;
                        if (isCheckable(units, checkpos3) && units[Checkpos3.x, Checkpos3.y] == null && (units[checkpos3.x, checkpos3.y] == null || units[checkpos3.x, checkpos3.y].Player != Player))
                        {
                            ret.Add(checkpos3);
                        }
                        // 左後ろ
                        Vector2Int Foward_rightdown = new Vector2Int(1, -dir);
                        Vector2Int Checkpos4 = Pos + Foward_rightdown;
                        Vector2Int foward_rightdown = new Vector2Int(2, -dir * 2);
                        Vector2Int checkpos4 = Pos + foward_rightdown;
                        if (isCheckable(units, checkpos4) && units[Checkpos4.x, Checkpos4.y] == null && (units[checkpos4.x, checkpos4.y] == null || units[checkpos4.x, checkpos4.y].Player != Player))
                        {
                            ret.Add(checkpos4);
                        }
                        // 右後ろ
                        Vector2Int Foward_leftdown = new Vector2Int(-1, -dir);
                        Vector2Int Checkpos5 = Pos + Foward_leftdown;
                        Vector2Int foward_leftdown = new Vector2Int(-2, -dir * 2);
                        Vector2Int checkpos5 = Pos + foward_leftdown;
                        if (isCheckable(units, checkpos5) && units[Checkpos5.x, Checkpos5.y] == null && (units[checkpos5.x, checkpos5.y] == null || units[checkpos5.x, checkpos5.y].Player != Player))
                        {
                            ret.Add(checkpos5);
                        }
                    }
                    // ＋銀将の動き（親：金将）
                    if (UnitType == UnitType.kinsyou)
                    {
                        Vector2Int[] directions =
                        {
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
                        // 前
                        Vector2Int Foward_up = new Vector2Int(0, dir);
                        Vector2Int Checkpos1 = Pos + Foward_up;
                        Vector2Int foward_up = new Vector2Int(0, dir * 2);
                        Vector2Int checkpos1 = Pos + foward_up;
                        if (isCheckable(units, checkpos1) && units[Checkpos1.x, Checkpos1.y] == null && (units[checkpos1.x, checkpos1.y] == null || units[checkpos1.x, checkpos1.y].Player != Player))
                        {
                            ret.Add(checkpos1);
                        }
                        // 左前
                        Vector2Int Foward_rightup = new Vector2Int(1, dir);
                        Vector2Int Checkpos2 = Pos + Foward_rightup;
                        Vector2Int foward_rightup = new Vector2Int(2, dir * 2);
                        Vector2Int checkpos2 = Pos + foward_rightup;
                        if (isCheckable(units, checkpos2) && units[Checkpos2.x, Checkpos2.y] == null && (units[checkpos2.x, checkpos2.y] == null || units[checkpos2.x, checkpos2.y].Player != Player))
                        {
                            ret.Add(checkpos2);
                        }
                        // 右前
                        Vector2Int Foward_leftup = new Vector2Int(-1, dir);
                        Vector2Int Checkpos3 = Pos + Foward_leftup;
                        Vector2Int foward_leftup = new Vector2Int(-2, dir * 2);
                        Vector2Int checkpos3 = Pos + foward_leftup;
                        if (isCheckable(units, checkpos3) && units[Checkpos3.x, Checkpos3.y] == null && (units[checkpos3.x, checkpos3.y] == null || units[checkpos3.x, checkpos3.y].Player != Player))
                        {
                            ret.Add(checkpos3);
                        }
                    }
                }
                break;

            case UnitType.kinsyou: // 吸収された駒が金将
                {
                    // プレイヤーによって向きを変える
                    int dir = (Player == 0) ? -1 : 1;
                    // ＋金将の動き（親：歩兵）
                    if (UnitType == UnitType.huhyou)
                    {
                        Vector2Int[] directions =
                        {
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
                        Vector2Int Foward = new Vector2Int(0, dir);
                        Vector2Int Checkpos = Pos + Foward;
                        Vector2Int foward = new Vector2Int(0, dir * 2);
                        Vector2Int checkpos1 = Pos + foward;
                        if (isCheckable(units, checkpos1) && units[Checkpos.x, Checkpos.y] == null && (units[checkpos1.x, checkpos1.y] == null || units[checkpos1.x, checkpos1.y].Player != Player))
                        {
                            ret.Add(checkpos1);
                        }

                    }
                    // ＋金将の動き（親：角行）
                    if (UnitType == UnitType.kakugyou)
                    {
                        Vector2Int[] directions =
                        {
                            new Vector2Int(0, dir),   // 前進
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
                    // ＋金将の動き（親：飛車）
                    if (UnitType == UnitType.hisya)
                    {
                        Vector2Int[] directions =
                        {
                  new Vector2Int(1, dir),   // 右前
　　　　　                  new Vector2Int(-1, dir),  // 左前
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
                    // ＋金将の動き（親：香車）
                    if (UnitType == UnitType.kyousya)
                    {
                        Vector2Int[] directions =
{
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
                    // ＋金将の動き（親：桂馬）
                    if (UnitType == UnitType.keima)
                    {
                        Vector2Int[] directions =
                        {
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
                    // ＋金将の動き（親：銀将）
                    if (UnitType == UnitType.ginsyou)
                    {
                        Vector2Int[] directions =
{
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
                        // 前
                        Vector2Int Foward_up = new Vector2Int(0, dir);
                        Vector2Int Checkpos1 = Pos + Foward_up;
                        Vector2Int foward_up = new Vector2Int(0, dir * 2);
                        Vector2Int checkpos1 = Pos + foward_up;
                        if (isCheckable(units, checkpos1) && units[Checkpos1.x, Checkpos1.y] == null && (units[checkpos1.x, checkpos1.y] == null || units[checkpos1.x, checkpos1.y].Player != Player))
                        {
                            ret.Add(checkpos1);
                        }
                        // 左前
                        Vector2Int Foward_rightup = new Vector2Int(1, dir);
                        Vector2Int Checkpos2 = Pos + Foward_rightup;
                        Vector2Int foward_rightup = new Vector2Int(2, dir * 2);
                        Vector2Int checkpos2 = Pos + foward_rightup;
                        if (isCheckable(units, checkpos2) && units[Checkpos2.x, Checkpos2.y] == null && (units[checkpos2.x, checkpos2.y] == null || units[checkpos2.x, checkpos2.y].Player != Player))
                        {
                            ret.Add(checkpos2);
                        }
                        // 右前
                        Vector2Int Foward_leftup = new Vector2Int(-1, dir);
                        Vector2Int Checkpos3 = Pos + Foward_leftup;
                        Vector2Int foward_leftup = new Vector2Int(-2, dir * 2);
                        Vector2Int checkpos3 = Pos + foward_leftup;
                        if (isCheckable(units, checkpos3) && units[Checkpos3.x, Checkpos3.y] == null && (units[checkpos3.x, checkpos3.y] == null || units[checkpos3.x, checkpos3.y].Player != Player))
                        {
                            ret.Add(checkpos3);
                        }
                    }
                    // ＋金将の動き（親：金将）
                    if (UnitType == UnitType.kinsyou)
                    {
                        // 前
                        Vector2Int Foward_up = new Vector2Int(0, dir);
                        Vector2Int Checkpos1 = Pos + Foward_up;
                        Vector2Int foward_up = new Vector2Int(0, dir * 2);
                        Vector2Int checkpos1 = Pos + foward_up;
                        if (isCheckable(units, checkpos1) && units[Checkpos1.x, Checkpos1.y] == null && (units[checkpos1.x, checkpos1.y] == null || units[checkpos1.x, checkpos1.y].Player != Player))
                        {
                            ret.Add(checkpos1);
                        }
                        // 左前
                        Vector2Int Foward_rightup = new Vector2Int(1, dir);
                        Vector2Int Checkpos2 = Pos + Foward_rightup;
                        Vector2Int foward_rightup = new Vector2Int(2, dir * 2);
                        Vector2Int checkpos2 = Pos + foward_rightup;
                        if (isCheckable(units, checkpos2) && units[Checkpos2.x, Checkpos2.y] == null && (units[checkpos2.x, checkpos2.y] == null || units[checkpos2.x, checkpos2.y].Player != Player))
                        {
                            ret.Add(checkpos2);
                        }
                        // 右前
                        Vector2Int Foward_leftup = new Vector2Int(-1, dir);
                        Vector2Int Checkpos3 = Pos + Foward_leftup;
                        Vector2Int foward_leftup = new Vector2Int(-2, dir * 2);
                        Vector2Int checkpos3 = Pos + foward_leftup;
                        if (isCheckable(units, checkpos3) && units[Checkpos3.x, Checkpos3.y] == null && (units[checkpos3.x, checkpos3.y] == null || units[checkpos3.x, checkpos3.y].Player != Player))
                        {
                            ret.Add(checkpos3);
                        }
                        // 右
                        Vector2Int Right = new Vector2Int(1, 0);
                        Vector2Int Checkpos4 = Pos + Right;
                        Vector2Int right = new Vector2Int(2, 0);
                        Vector2Int checkpos4 = Pos + right;
                        if (isCheckable(units, checkpos4) && units[Checkpos4.x, Checkpos4.y] == null && (units[checkpos4.x, checkpos4.y] == null || units[checkpos4.x, checkpos4.y].Player != Player))
                        {
                            ret.Add(checkpos4);
                        }
                        // 左
                        Vector2Int Left = new Vector2Int(-1, 0);
                        Vector2Int Checkpos5 = Pos + Left;
                        Vector2Int left = new Vector2Int(-2, 0);
                        Vector2Int checkpos5 = Pos + left;
                        if (isCheckable(units, checkpos5) && units[Checkpos5.x, Checkpos5.y] == null && (units[checkpos5.x, checkpos5.y] == null || units[checkpos5.x, checkpos5.y].Player != Player))
                        {
                            ret.Add(checkpos5);
                        }
                        // 後ろ
                        Vector2Int Back = new Vector2Int(0, -dir);
                        Vector2Int Checkpos6 = Pos + Back;
                        Vector2Int back = new Vector2Int(0, -dir * 2);
                        Vector2Int checkpos6 = Pos + back;
                        if (isCheckable(units, checkpos6) && units[Checkpos6.x, Checkpos6.y] == null && (units[checkpos6.x, checkpos6.y] == null || units[checkpos6.x, checkpos6.y].Player != Player))
                        {
                            ret.Add(checkpos6);
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

                    // ＋飛車の動き（親：歩兵、銀将）
                    if (UnitType == UnitType.huhyou || UnitType == UnitType.ginsyou)
                    {
                        Fowards = 2;
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // ＋飛車の動き（親：角行、桂馬）
                    if (UnitType == UnitType.kakugyou || UnitType == UnitType.keima)
                    {
                        Fowards = 1;
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // ＋飛車の動き（親：香車）
                    if (UnitType == UnitType.kyousya)
                    {
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // ＋飛車の動き（親：金将）
                    if (UnitType == UnitType.kinsyou)
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
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
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
                        }

                        Vector2Int[] direction2 = { new Vector2Int(-1, 1) }; // 右下
                        foreach (var dir2 in direction2)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
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
                        }

                        Vector2Int[] direction3 = { new Vector2Int(-1, -1) }; // 右上
                        foreach (var dir2 in direction3)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
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
                        }

                        Vector2Int[] direction4 = { new Vector2Int(1, -1) }; // 左上
                        foreach (var dir2 in direction4)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
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
                        if (units[Checkpos1.x, Checkpos1.y] != null) break;
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
                        if (units[Checkpos2.x, Checkpos2.y] != null) break;
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
                        if (units[Checkpos3.x, Checkpos3.y] != null) break;
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
                        if (units[Checkpos4.x, Checkpos4.y] != null) break;
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

                    // ＋角行の動き（親：歩兵、飛車、香車、桂馬）
                    if (UnitType == UnitType.huhyou || UnitType == UnitType.hisya || UnitType == UnitType.kyousya || UnitType == UnitType.keima)
                    {
                        Foward_Left = 1;
                        Foward_Right = 1;
                        Back_Right = 1;
                        Back_Left = 1;

                    }
                    // ＋角行の動き（親：銀将）
                    if (UnitType == UnitType.ginsyou)
                    {
                        Foward_Left = 2;
                        Foward_Right = 2;
                        Back_Right = 2;
                        Back_Left = 2;
                    }
                    // ＋角行の動き（親：金将）
                    if (UnitType == UnitType.kinsyou)
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
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
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
                        }

                        Vector2Int[] direction2 = { new Vector2Int(0, 1) };
                        foreach (var dir2 in direction2)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
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
                        }

                        Vector2Int[] direction3 = { new Vector2Int(-1, 0) };
                        foreach (var dir3 in direction3)
                        {
                            Vector2Int checkpos = Pos + dir3;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
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
                        }

                        Vector2Int[] direction4 = { new Vector2Int(1, 0) };
                        foreach (var dir4 in direction4)
                        {
                            Vector2Int checkpos = Pos + dir4;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
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
                        if (units[Checkpos1.x, Checkpos1.y] != null) break;
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
                        if (units[Checkpos2.x, Checkpos2.y] != null) break;
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
                        if (units[Checkpos3.x, Checkpos3.y] != null) break;
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
                        if (units[Checkpos4.x, Checkpos4.y] != null) break;
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

    bool isCheckable(UnitController[,] ary, Vector2Int idx)
    {
        return idx.x >= 0 && idx.x < ary.GetLength(0) && idx.y >= 0 && idx.y < ary.GetLength(1);
    }
}

