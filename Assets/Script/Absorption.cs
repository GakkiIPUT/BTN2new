using UnityEngine;
using System.Collections.Generic;

/*
   �z���\�^�C���擾
   ��z���E���o����
   ���̑�����
*/
public class Absorption : MonoBehaviour
{
    public int Player;
    public UnitType UnitType, OldUnitType;
    public FieldStatus FieldStatus;
    public Vector2Int Pos;

    //--------�����������J�n-----------
    public void Init(int player, int unittype, GameObject tile, Vector2Int pos)
    {
        Player = player;
        UnitType = (UnitType)unittype;
        OldUnitType = (UnitType)unittype;
        FieldStatus = FieldStatus.OnBard;
        Pos = pos;
    }
    //--------�����������I���----------

    //--------�z���\�^�C���擾�����J�n-----------
    public List<Vector2Int> GetAbsorptionTiles(UnitController[,] units)
    {
        List<Vector2Int> ret = new List<Vector2Int>();
        UnitController unitctrl = GetComponent<UnitController>();

        switch (unitctrl.UnitType)
        {
            // ����
            case UnitType.huhyou:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions =
                    {
                        new Vector2Int(1, dir),   // �E�O
                        new Vector2Int(-1, dir),  // ���O
                        new Vector2Int(1, 0),     // �E
                        new Vector2Int(-1, 0),    // ��
                        new Vector2Int(1, -dir),  // �E���
                        new Vector2Int(-1, -dir), // �����
                        new Vector2Int(0, -dir)  // ���
                    };
                    // ���ׂĂ̕������m�F
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

            // ����
            case UnitType.kyousya:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions =
                    {
                        new Vector2Int(1, dir),   // �E�O
                        new Vector2Int(-1, dir),  // ���O
                        new Vector2Int(1, 0),     // �E
                        new Vector2Int(-1, 0),    // ��
                        new Vector2Int(1, -dir),  // �E���
                        new Vector2Int(-1, -dir), // �����
                        new Vector2Int(0, -dir)  // ���
                    };
                }
                break;

            // �j�n
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

            // �⏫
            case UnitType.ginsyou:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions =
                    {
                        new Vector2Int(1, 0),     // �E
                        new Vector2Int(-1, 0),    // ��
                        new Vector2Int(0, -dir)  // ���
                    };
                }
                break;

            // ����
            case UnitType.kinsyou:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions =
                    {
                        new Vector2Int(1, -dir),  // �E���
                        new Vector2Int(-1, -dir), // �����
                    };
                }
                break;

            // ���
            case UnitType.hisya:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions =
                    {
                        new Vector2Int(1, dir),   // �E�O
                        new Vector2Int(-1, dir),  // ���O
                        new Vector2Int(1, -dir),  // �E���
                        new Vector2Int(-1, -dir), // �����
                    };
                }
                break;

            // �p�s
            case UnitType.kakugyou:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions =
                    {
                        new Vector2Int(1, 0),     // �E
                        new Vector2Int(-1, 0),    // ��
                        new Vector2Int(0, dir) , // ���
                        new Vector2Int(0, -dir)  // �O
                    };
                }
                break;

            // �����
            case UnitType.narigoma:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions =
                    {
                        new Vector2Int(1, -dir),  // �E���
                        new Vector2Int(-1, -dir), // �����
                    };
                }
                break;
        }
        return ret;
    }
    //--------�z���\�^�C���擾�����I���----------

    //--------��z���E���o�����J�n-----------
    public List<GameObject> AbsorptionObjcts = new List<GameObject>();
    //public GameObject AbsorptionObj;
    public Vector2Int Absorptionpos;


    //--------���̑������J�n-----------
    bool isCheckable(UnitController[,] ary, Vector2Int idx)
    {
        return idx.x >= 0 && idx.x < ary.GetLength(0) && idx.y >= 0 && idx.y < ary.GetLength(1);
    }
    //--------���̑������I���----------
}
