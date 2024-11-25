using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relese : MonoBehaviour
{
    private UnitController unit;         // UnitController�̎Q��
    private GameSystem gameSystem;        // GameSystem�̎Q��

    public int Player;  // �v���C���[�ԍ�
    public UnitType UnitType, OldUnitType, AbsorUnitType;  // ���݂̋�^�C�v�A����O�̋�^�C�v�A�z�����ꂽ��̃^�C�v
    public FieldStatus FieldStatus;  // ��̏�� (�Տ�A�ߊl)
    public Vector2Int Pos;  // �Տ�̈ʒu


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
            //Debug.LogWarning("Init���\�b�h��tile��null�ł��BMove�͌Ăяo����܂���ł����B");
        }
        Pos = pos; // Pos��ݒ�
    }

    public void Move(GameObject tile, Vector2Int tileindex)
    {
        if (tile == null)
        {
            //Debug.LogError("Move���\�b�h��tile��null�ł��B");
            return;
        }
        Vector3 pos = tile.transform.position;
        pos.y = 0.1f;
        transform.position = pos;
        Pos = tileindex;
    }

    public void AbsorCheck()
    {
        if (gameSystem.absorUnit.UnitType == UnitType.narigoma)// �z��������ɋz�����ꂽ��̃��j�b�g�^�C�v���Q��
        {
            //Debug.Log(gameSystem.absorUnit.UnitType + "���z��������");
            //Debug.Log("OldUnit��" + OldUnitType + "����");
            AbsorUnitType = OldUnitType;
        }
        else
        {
            //Debug.Log("���ʂ̋�z��������");
            AbsorUnitType = gameSystem.absorUnit.UnitType;
        }
    }

    public List<Vector2Int> GetRelesableTiles(UnitController[,] units)
    {
        List<Vector2Int> ret = new List<Vector2Int>();
        //Debug.Log(Pos);

        // switch() �� UnitType �� �z�����ꂽ��̃^�C�v�Acase ���� UnitType �͋z��������̃^�C�v
        switch (AbsorUnitType)
        {
            case UnitType.huhyou: // �z�����ꂽ�����
                {
                    // �v���C���[�ɂ���Č�����ς���
                    int dir = (Player == 0) ? -1 : 1;
                    // �{�����̓����i�e�F�����A�⏫�A�����j
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
                    // �{�����̓����i�e�F���ԁj
                    if (UnitType == UnitType.kyousya)
                    {
                        Vector2Int foward = new Vector2Int(0, -dir);
                        Vector2Int checkpos = Pos + foward;
                        if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                        {
                            ret.Add(checkpos);
                        }
                    }
                    // �{�����̓����i�e�F�p�s�A�j�n�j
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

            case UnitType.kyousya: // �z�����ꂽ�����
                {
                    // �v���C���[�ɂ���Č�����ς���
                    int dir = (Player == 0) ? -1 : 1;
                    // �{���Ԃ̓����i�e�F�����A�⏫�A�����j
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
                    // �{���Ԃ̓����i�e�F���ԁj
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
                    // �{���Ԃ̓����i�e�F�p�s�A�j�n�j
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

            case UnitType.keima:  // �z�����ꂽ��j�n
                {
                    // �v���C���[�ɂ���Č�����ς���
                    int dir = (Player == 0) ? -1 : 1;
                    // �{�j�n�̓����i�e�F�j�n�j
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
                    // �{�j�n�̓����i�e�F�j�n�ȊO�j
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

            case UnitType.ginsyou: // �z�����ꂽ��⏫
                {
                    // �v���C���[�ɂ���Č�����ς���
                    int dir = (Player == 0) ? -1 : 1;
                    // �{�⏫�̓����i�e�F�����j
                    if (UnitType == UnitType.huhyou)
                    {
                        Vector2Int[] directions =
                        {
                  new Vector2Int(1, dir), // �E�O
�@�@�@�@�@                  new Vector2Int(-1, dir), // ���O
�@�@�@�@�@                  new Vector2Int(1, -dir), // �E���
�@�@�@�@�@                  new Vector2Int(-1, -dir) // �����
�@�@�@�@�@  �@�@        };
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
                    // �{�⏫�̓����i�e�F�p�s�j
                    if (UnitType == UnitType.kakugyou)
                    {
                        Vector2Int forward = new Vector2Int(0, dir);
                        Vector2Int checkpos = Pos + forward;
                        if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                        {
                            ret.Add(checkpos);
                        }
                    }
                    // �{�⏫�̓����i�e�F��ԁA���ԁj
                    if (UnitType == UnitType.hisya || UnitType == UnitType.kyousya)
                    {
                        Vector2Int[] directions =
                        {
                  new Vector2Int(1, dir), // �E�O
�@�@�@�@�@                  new Vector2Int(-1, dir), // ���O
�@�@�@�@�@                  new Vector2Int(1, -dir), // �E���
�@�@�@�@�@                  new Vector2Int(-1, -dir) // �����
�@�@�@�@�@  �@�@        };
                        foreach (var move in directions)
                        {
                            Vector2Int checkpos = Pos + move;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                            {
                                ret.Add(checkpos);
                            }
                        }
                    }
                    // �{�⏫�̓����i�e�F�j�n�j
                    if (UnitType == UnitType.keima)
                    {
                        Vector2Int[] directions =
                        {
                            new Vector2Int(0, dir), // �O�i
�@�@�@�@�@                  new Vector2Int(1, dir), // �E�O
�@�@�@�@�@                  new Vector2Int(-1, dir), // ���O
�@�@�@�@�@                  new Vector2Int(1, -dir), // �E���
�@�@�@�@�@                  new Vector2Int(-1, -dir) // �����
�@�@�@�@�@  �@�@        };
                        foreach (var move in directions)
                        {
                            Vector2Int checkpos = Pos + move;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                            {
                                ret.Add(checkpos);
                            }
                        }
                    }
                    // �{�⏫�̓����i�e�F�⏫�j
                    if (UnitType == UnitType.ginsyou)
                    {
                        // �O
                        Vector2Int Foward_up = new Vector2Int(0, dir);
                        Vector2Int Checkpos1 = Pos + Foward_up;
                        Vector2Int foward_up = new Vector2Int(0, dir * 2);
                        Vector2Int checkpos1 = Pos + foward_up;
                        if (isCheckable(units, checkpos1) && units[Checkpos1.x, Checkpos1.y] == null && (units[checkpos1.x, checkpos1.y] == null || units[checkpos1.x, checkpos1.y].Player != Player))
                        {
                            ret.Add(checkpos1);
                        }
                        // ���O
                        Vector2Int Foward_rightup = new Vector2Int(1, dir);
                        Vector2Int Checkpos2 = Pos + Foward_rightup;
                        Vector2Int foward_rightup = new Vector2Int(2, dir * 2);
                        Vector2Int checkpos2 = Pos + foward_rightup;
                        if (isCheckable(units, checkpos2) && units[Checkpos2.x, Checkpos2.y] == null && (units[checkpos2.x, checkpos2.y] == null || units[checkpos2.x, checkpos2.y].Player != Player))
                        {
                            ret.Add(checkpos2);
                        }
                        // �E�O
                        Vector2Int Foward_leftup = new Vector2Int(-1, dir);
                        Vector2Int Checkpos3 = Pos + Foward_leftup;
                        Vector2Int foward_leftup = new Vector2Int(-2, dir * 2);
                        Vector2Int checkpos3 = Pos + foward_leftup;
                        if (isCheckable(units, checkpos3) && units[Checkpos3.x, Checkpos3.y] == null && (units[checkpos3.x, checkpos3.y] == null || units[checkpos3.x, checkpos3.y].Player != Player))
                        {
                            ret.Add(checkpos3);
                        }
                        // �����
                        Vector2Int Foward_rightdown = new Vector2Int(1, -dir);
                        Vector2Int Checkpos4 = Pos + Foward_rightdown;
                        Vector2Int foward_rightdown = new Vector2Int(2, -dir * 2);
                        Vector2Int checkpos4 = Pos + foward_rightdown;
                        if (isCheckable(units, checkpos4) && units[Checkpos4.x, Checkpos4.y] == null && (units[checkpos4.x, checkpos4.y] == null || units[checkpos4.x, checkpos4.y].Player != Player))
                        {
                            ret.Add(checkpos4);
                        }
                        // �E���
                        Vector2Int Foward_leftdown = new Vector2Int(-1, -dir);
                        Vector2Int Checkpos5 = Pos + Foward_leftdown;
                        Vector2Int foward_leftdown = new Vector2Int(-2, -dir * 2);
                        Vector2Int checkpos5 = Pos + foward_leftdown;
                        if (isCheckable(units, checkpos5) && units[Checkpos5.x, Checkpos5.y] == null && (units[checkpos5.x, checkpos5.y] == null || units[checkpos5.x, checkpos5.y].Player != Player))
                        {
                            ret.Add(checkpos5);
                        }
                    }
                    // �{�⏫�̓����i�e�F�����j
                    if (UnitType == UnitType.kinsyou)
                    {
                        Vector2Int[] directions =
                        {
                              new Vector2Int(1, -dir), // �E���
�@�@�@�@�@                  new Vector2Int(-1, -dir) // �����
�@�@�@�@�@  �@�@        };
                        foreach (var move in directions)
                        {
                            Vector2Int checkpos = Pos + move;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                            {
                                ret.Add(checkpos);
                            }
                        }
                        // �O
                        Vector2Int Foward_up = new Vector2Int(0, dir);
                        Vector2Int Checkpos1 = Pos + Foward_up;
                        Vector2Int foward_up = new Vector2Int(0, dir * 2);
                        Vector2Int checkpos1 = Pos + foward_up;
                        if (isCheckable(units, checkpos1) && units[Checkpos1.x, Checkpos1.y] == null && (units[checkpos1.x, checkpos1.y] == null || units[checkpos1.x, checkpos1.y].Player != Player))
                        {
                            ret.Add(checkpos1);
                        }
                        // ���O
                        Vector2Int Foward_rightup = new Vector2Int(1, dir);
                        Vector2Int Checkpos2 = Pos + Foward_rightup;
                        Vector2Int foward_rightup = new Vector2Int(2, dir * 2);
                        Vector2Int checkpos2 = Pos + foward_rightup;
                        if (isCheckable(units, checkpos2) && units[Checkpos2.x, Checkpos2.y] == null && (units[checkpos2.x, checkpos2.y] == null || units[checkpos2.x, checkpos2.y].Player != Player))
                        {
                            ret.Add(checkpos2);
                        }
                        // �E�O
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

            case UnitType.kinsyou: // �z�����ꂽ�����
                {
                    // �v���C���[�ɂ���Č�����ς���
                    int dir = (Player == 0) ? -1 : 1;
                    // �{�����̓����i�e�F�����j
                    if (UnitType == UnitType.huhyou)
                    {
                        Vector2Int[] directions =
                        {
                  new Vector2Int(1, dir),   // �E�O
�@�@�@�@�@                  new Vector2Int(-1, dir),  // ���O
�@�@�@�@�@                  new Vector2Int(0, -dir),  // ���
�@�@�@�@�@                  new Vector2Int(1, 0),     // �E
�@�@�@�@�@                  new Vector2Int(-1, 0)     // ��
�@�@�@�@�@  �@�@        };
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
                    // �{�����̓����i�e�F�p�s�j
                    if (UnitType == UnitType.kakugyou)
                    {
                        Vector2Int[] directions =
                        {
                            new Vector2Int(0, dir),   // �O�i
�@�@�@�@�@                  new Vector2Int(0, -dir),  // ���
�@�@�@�@�@                  new Vector2Int(1, 0),     // �E
�@�@�@�@�@                  new Vector2Int(-1, 0)     // ��
�@�@�@�@�@  �@�@        };
                        foreach (var move in directions)
                        {
                            Vector2Int checkpos = Pos + move;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                            {
                                ret.Add(checkpos);
                            }
                        }
                    }
                    // �{�����̓����i�e�F��ԁj
                    if (UnitType == UnitType.hisya)
                    {
                        Vector2Int[] directions =
                        {
                  new Vector2Int(1, dir),   // �E�O
�@�@�@�@�@                  new Vector2Int(-1, dir),  // ���O
�@�@�@�@�@  �@�@        };
                        foreach (var move in directions)
                        {
                            Vector2Int checkpos = Pos + move;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                            {
                                ret.Add(checkpos);
                            }
                        }
                    }
                    // �{�����̓����i�e�F���ԁj
                    if (UnitType == UnitType.kyousya)
                    {
                        Vector2Int[] directions =
{
                  new Vector2Int(1, dir),   // �E�O
�@�@�@�@�@                  new Vector2Int(-1, dir),  // ���O
�@�@�@�@�@                  new Vector2Int(0, -dir),  // ���
�@�@�@�@�@                  new Vector2Int(1, 0),     // �E
�@�@�@�@�@                  new Vector2Int(-1, 0)     // ��
�@�@�@�@�@  �@�@        };
                        foreach (var move in directions)
                        {
                            Vector2Int checkpos = Pos + move;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                            {
                                ret.Add(checkpos);
                            }
                        }
                    }
                    // �{�����̓����i�e�F�j�n�j
                    if (UnitType == UnitType.keima)
                    {
                        Vector2Int[] directions =
                        {
                            new Vector2Int(0, dir),   // �O�i
�@�@�@�@�@                  new Vector2Int(1, dir),   // �E�O
�@�@�@�@�@                  new Vector2Int(-1, dir),  // ���O
�@�@�@�@�@                  new Vector2Int(0, -dir),  // ���
�@�@�@�@�@                  new Vector2Int(1, 0),     // �E
�@�@�@�@�@                  new Vector2Int(-1, 0)     // ��
�@�@�@�@�@  �@�@        };
                        foreach (var move in directions)
                        {
                            Vector2Int checkpos = Pos + move;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                            {
                                ret.Add(checkpos);
                            }
                        }
                    }
                    // �{�����̓����i�e�F�⏫�j
                    if (UnitType == UnitType.ginsyou)
                    {
                        Vector2Int[] directions =
{
                  new Vector2Int(0, -dir),  // ���
�@�@�@�@�@                  new Vector2Int(1, 0),     // �E
�@�@�@�@�@                  new Vector2Int(-1, 0)     // ��
�@�@�@�@�@  �@�@        };
                        foreach (var move in directions)
                        {
                            Vector2Int checkpos = Pos + move;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                            {
                                ret.Add(checkpos);
                            }
                        }
                        // �O
                        Vector2Int Foward_up = new Vector2Int(0, dir);
                        Vector2Int Checkpos1 = Pos + Foward_up;
                        Vector2Int foward_up = new Vector2Int(0, dir * 2);
                        Vector2Int checkpos1 = Pos + foward_up;
                        if (isCheckable(units, checkpos1) && units[Checkpos1.x, Checkpos1.y] == null && (units[checkpos1.x, checkpos1.y] == null || units[checkpos1.x, checkpos1.y].Player != Player))
                        {
                            ret.Add(checkpos1);
                        }
                        // ���O
                        Vector2Int Foward_rightup = new Vector2Int(1, dir);
                        Vector2Int Checkpos2 = Pos + Foward_rightup;
                        Vector2Int foward_rightup = new Vector2Int(2, dir * 2);
                        Vector2Int checkpos2 = Pos + foward_rightup;
                        if (isCheckable(units, checkpos2) && units[Checkpos2.x, Checkpos2.y] == null && (units[checkpos2.x, checkpos2.y] == null || units[checkpos2.x, checkpos2.y].Player != Player))
                        {
                            ret.Add(checkpos2);
                        }
                        // �E�O
                        Vector2Int Foward_leftup = new Vector2Int(-1, dir);
                        Vector2Int Checkpos3 = Pos + Foward_leftup;
                        Vector2Int foward_leftup = new Vector2Int(-2, dir * 2);
                        Vector2Int checkpos3 = Pos + foward_leftup;
                        if (isCheckable(units, checkpos3) && units[Checkpos3.x, Checkpos3.y] == null && (units[checkpos3.x, checkpos3.y] == null || units[checkpos3.x, checkpos3.y].Player != Player))
                        {
                            ret.Add(checkpos3);
                        }
                    }
                    // �{�����̓����i�e�F�����j
                    if (UnitType == UnitType.kinsyou)
                    {
                        // �O
                        Vector2Int Foward_up = new Vector2Int(0, dir);
                        Vector2Int Checkpos1 = Pos + Foward_up;
                        Vector2Int foward_up = new Vector2Int(0, dir * 2);
                        Vector2Int checkpos1 = Pos + foward_up;
                        if (isCheckable(units, checkpos1) && units[Checkpos1.x, Checkpos1.y] == null && (units[checkpos1.x, checkpos1.y] == null || units[checkpos1.x, checkpos1.y].Player != Player))
                        {
                            ret.Add(checkpos1);
                        }
                        // ���O
                        Vector2Int Foward_rightup = new Vector2Int(1, dir);
                        Vector2Int Checkpos2 = Pos + Foward_rightup;
                        Vector2Int foward_rightup = new Vector2Int(2, dir * 2);
                        Vector2Int checkpos2 = Pos + foward_rightup;
                        if (isCheckable(units, checkpos2) && units[Checkpos2.x, Checkpos2.y] == null && (units[checkpos2.x, checkpos2.y] == null || units[checkpos2.x, checkpos2.y].Player != Player))
                        {
                            ret.Add(checkpos2);
                        }
                        // �E�O
                        Vector2Int Foward_leftup = new Vector2Int(-1, dir);
                        Vector2Int Checkpos3 = Pos + Foward_leftup;
                        Vector2Int foward_leftup = new Vector2Int(-2, dir * 2);
                        Vector2Int checkpos3 = Pos + foward_leftup;
                        if (isCheckable(units, checkpos3) && units[Checkpos3.x, Checkpos3.y] == null && (units[checkpos3.x, checkpos3.y] == null || units[checkpos3.x, checkpos3.y].Player != Player))
                        {
                            ret.Add(checkpos3);
                        }
                        // �E
                        Vector2Int Right = new Vector2Int(1, 0);
                        Vector2Int Checkpos4 = Pos + Right;
                        Vector2Int right = new Vector2Int(2, 0);
                        Vector2Int checkpos4 = Pos + right;
                        if (isCheckable(units, checkpos4) && units[Checkpos4.x, Checkpos4.y] == null && (units[checkpos4.x, checkpos4.y] == null || units[checkpos4.x, checkpos4.y].Player != Player))
                        {
                            ret.Add(checkpos4);
                        }
                        // ��
                        Vector2Int Left = new Vector2Int(-1, 0);
                        Vector2Int Checkpos5 = Pos + Left;
                        Vector2Int left = new Vector2Int(-2, 0);
                        Vector2Int checkpos5 = Pos + left;
                        if (isCheckable(units, checkpos5) && units[Checkpos5.x, Checkpos5.y] == null && (units[checkpos5.x, checkpos5.y] == null || units[checkpos5.x, checkpos5.y].Player != Player))
                        {
                            ret.Add(checkpos5);
                        }
                        // ���
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

            case UnitType.hisya: // �z�����ꂽ����
                {
                    // �v���C���[�ɂ���Č�����ς���
                    int dir = (Player == 0) ? -1 : 1;
                    // �c��4�����̃X�^�[�g�ʒu
                    int Fowards = 0;
                    int Rights = 0;
                    int Lefts = 0;
                    int Backs = 0;

                    // �{��Ԃ̓����i�e�F�����A�⏫�j
                    if (UnitType == UnitType.huhyou || UnitType == UnitType.ginsyou)
                    {
                        Fowards = 2;
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // �{��Ԃ̓����i�e�F�p�s�A�j�n�j
                    if (UnitType == UnitType.kakugyou || UnitType == UnitType.keima)
                    {
                        Fowards = 1;
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // �{��Ԃ̓����i�e�F���ԁj
                    if (UnitType == UnitType.kyousya)
                    {
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // �{��Ԃ̓����i�e�F�����j
                    if (UnitType == UnitType.kinsyou)
                    {
                        Fowards = 2;
                        Rights = 2;
                        Lefts = 2;
                        Backs = 2;
                    }

                    // �{��Ԃ̓����i�e�F��ԁj����O�ɂ�����
                    if (UnitType == UnitType.hisya)
                    {
                        Vector2Int offset1 = new Vector2Int(1, 1); // ����
                        Vector2Int offset2 = new Vector2Int(-1, 1); // �E��
                        Vector2Int offset3 = new Vector2Int(-1, -1); // �E��
                        Vector2Int offset4 = new Vector2Int(1, -1); // ����

                        Vector2Int[] direction1 = { new Vector2Int(1, 1) }; // ����
                        foreach (var dir1 in direction1)
                        {
                            Vector2Int checkpos = Pos + dir1;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                            {
                                Vector2Int newCenterPos1 = Pos + offset1;
                                Vector2Int[] directions1 = { new Vector2Int(0, 1), new Vector2Int(1, 0) }; // ����
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

                        Vector2Int[] direction2 = { new Vector2Int(-1, 1) }; // �E��
                        foreach (var dir2 in direction2)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                            {
                                Vector2Int newCenterPos2 = Pos + offset2;
                                Vector2Int[] directions2 = { new Vector2Int(0, 1), new Vector2Int(-1, 0) }; // �E��
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

                        Vector2Int[] direction3 = { new Vector2Int(-1, -1) }; // �E��
                        foreach (var dir2 in direction3)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                            {
                                Vector2Int newCenterPos3 = Pos + offset3;
                                Vector2Int[] directions3 = { new Vector2Int(0, -1), new Vector2Int(-1, 0) }; // �E��
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

                        Vector2Int[] direction4 = { new Vector2Int(1, -1) }; // ����
                        foreach (var dir2 in direction4)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && (units[checkpos.x, checkpos.y] == null || units[checkpos.x, checkpos.y].Player != Player))
                            {
                                Vector2Int newCenterPos4 = Pos + offset4;
                                Vector2Int[] directions4 = { new Vector2Int(0, -1), new Vector2Int(1, 0) }; // ����
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

                    // �O
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
                    // ���
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
                    // �E
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
                    // ��
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

            case UnitType.kakugyou: // �z�����ꂽ��p�s
                {
                    // �v���C���[�ɂ���Č�����ς���
                    int dir = (Player == 0) ? -1 : 1;
                    // �΂�4�����̃X�^�[�g�ʒu
                    int Foward_Left = 0;   // ���O
                    int Foward_Right = 0;  // �E�O
                    int Back_Right = 0;    // �E��
                    int Back_Left = 0;     // ����

                    // �{�p�s�̓����i�e�F�����A��ԁA���ԁA�j�n�j
                    if (UnitType == UnitType.huhyou || UnitType == UnitType.hisya || UnitType == UnitType.kyousya || UnitType == UnitType.keima)
                    {
                        Foward_Left = 1;
                        Foward_Right = 1;
                        Back_Right = 1;
                        Back_Left = 1;

                    }
                    // �{�p�s�̓����i�e�F�⏫�j
                    if (UnitType == UnitType.ginsyou)
                    {
                        Foward_Left = 2;
                        Foward_Right = 2;
                        Back_Right = 2;
                        Back_Left = 2;
                    }
                    // �{�p�s�̓����i�e�F�����j
                    if (UnitType == UnitType.kinsyou)
                    {
                        Foward_Left = 2;
                        Foward_Right = 2;
                        Back_Right = 1;
                        Back_Left = 1;
                    }

                    // �{�p�s�̓����i�e�F�p�s�j����O�ɂ�����
                    if (UnitType == UnitType.kakugyou)
                    {
                        Vector2Int offset1 = new Vector2Int(0, -1); // ��
                        Vector2Int offset2 = new Vector2Int(0, 1); // ��
                        Vector2Int offset3 = new Vector2Int(-1, 0); // �E
                        Vector2Int offset4 = new Vector2Int(1, 0); // ��

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

                    // ����
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

                    // �E��
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

                    // ����
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

                    // �E��
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

