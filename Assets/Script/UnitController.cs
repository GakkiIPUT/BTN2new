using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
   ��ړ�
   ��I��
   ���
   ������
   ��z��
   ��u�ړ��v�\�͈͎擾
   ��u�z���v�\�͈͎擾
   �[�U�`�F�b�N
   ��u���o�v�\�͈͎擾
   �v���n�u�ύX
   �⏕���\�b�h
*/


// ��̎��
public enum UnitType
{
    None = -1,
    huhyou = 1, // ����
    kakugyou, // �p
    hisya, // ���
    kyousya, // ����
    keima, // �j�n
    ginsyou, // �⏫
    kinsyou, // ����
    oushyou, // ����
    gyokusyou,//�ʏ�
    narigoma,
    ryuuma, // ���n
    ryuuou, // ����
}

public enum FieldStatus
{
    None = -1,
    OnBard,
    Captured,
}

public class UnitController : MonoBehaviour
{
    public int Player;  // �v���C���[�ԍ�
    public UnitType UnitType, OldUnitType, AbsorUnitType;  // ���݂̋�^�C�v�A����O�̋�^�C�v�A�z�����ꂽ��̃^�C�v
    public FieldStatus FieldStatus;  // ��̏�� (�Տ�A�ߊl)
    public Vector2Int Pos;  // �Տ�̈ʒu
    public bool absorptionCheck = false;  // �z���m�F�t���O
    public bool fillingCheck = false;     // �[�U�m�F�t���O
    public bool ReleseCheck = false;      // ���o�m�F�t���O
    public int absorTurn = 0;             // �z���^�[��
    private GameSystem gameSystem;        // GameSystem�̎Q��
    public UnitController SelectUnit;
    //private AudioSource audioSource;
    //[SerializeField] AudioClip SE;

    public int Movecount;
    public int PutTurn = 0;
    public bool PutUnitCheck = false;

    public GameObject originalPrefab;  // ���̃v���n�u
    public GameObject promotedPrefab;  // �����̃v���n�u
    public int AbsorUnitPlayer { get; set; }
    public bool hasTemporaryDefense = false;
    List<UnitType> narigoma;

    //--------�����������J�n-----------
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
            //Debug.LogWarning("Init���\�b�h��tile��null�ł��BMove�͌Ăяo����܂���ł����B");
        }
        Pos = pos; // Pos��ݒ�

        Movecount = 0;
    }
    //--------�����������I���----------

    //--------��ړ������J�n-----------
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
        //audioSource.PlayOneShot(SE)
        Movecount += 1;
;    }
    //--------��ړ������I���----------

    //--------��I�������J�n-----------
    public void Selected(bool select = true)
    {
        Vector3 pos = transform.position;
        bool isKinematic = true;

        if (select)
        {
            pos.y = 0.3f;  // �I�����ɍ�����ύX
        }
        else
        {
            pos.y = 0.1f;  // ��I�����Ɍ��̍����ɖ߂�
        }

        GetComponent<Rigidbody>().isKinematic = isKinematic;
        transform.position = pos;
    }
    //--------��I�������I���----------

    //--------��菈���J�n-----------
    public void Promote()
    {
        //Debug.Log($"����O�̋�̎��: {UnitType}");
        StartCoroutine(PromoteWithDelay());
    }

    private IEnumerator PromoteWithDelay()
    {
        OldUnitType = UnitType;// ���̋�̎�ނ�ۑ�
        //Debug.Log($"OldUnitType�ɕۑ����ꂽ��: {OldUnitType}");

        switch (UnitType)// �����ɕϊ�
        {
            case UnitType.huhyou: UnitType = UnitType.narigoma; break;
            case UnitType.kyousya: UnitType = UnitType.narigoma; break;
            case UnitType.keima: UnitType = UnitType.narigoma; break;
            case UnitType.ginsyou: UnitType = UnitType.narigoma; break;
            case UnitType.hisya: UnitType = UnitType.ryuuou; break;
            case UnitType.kakugyou: UnitType = UnitType.ryuuma; break;
        }
        //Debug.Log($"�����̋�̎��: {UnitType}");


        // �C���X�y�N�^�[�̍X�V������
#if UNITY_EDITOR�@
        UnityEditor.EditorUtility.SetDirty(this);
#endif

        yield return new WaitForSeconds(0.5f);// �����҂i�����ڂ̒����j

        GameObject newUnit = Instantiate(promotedPrefab, transform.position, transform.rotation);// ��A�N�e�B�u�ȃv���t�@�u�𐶐�
        newUnit.SetActive(true);  // ������ɃA�N�e�B�u��
        UnitController newUnitController = newUnit.GetComponent<UnitController>();
        newUnitController.Init(Player, (int)UnitType, null, Pos); // �V������Ƀv���C���[���ƈʒu���������p��

        if (this.absorptionCheck) // �z�������̈��p��
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
        if (this.fillingCheck) // �[�U�����̈��p��
        {
            newUnitController.fillingCheck = true;
            newUnit.GetComponent<MeshRenderer>().material = this.gameObject.GetComponent<MeshRenderer>().material;
        }
        if (this.ReleseCheck) // ���o�����̈��p��
        {
            newUnitController.ReleseCheck = true;
            gameSystem.absorptionHistory.Remove(newUnitController);
            newUnit.GetComponent<MeshRenderer>().material = this.gameObject.GetComponent<MeshRenderer>().material;
        }

        gameSystem.UpdateUnitPosition(newUnitController);// GameSystem�ɍēo�^

        Debug.Log(this.gameObject + "�@111");
        Destroy(this.gameObject);// ���̋���폜
    }
    //--------��菈���I���----------

    //--------�����������J�n-----------
    public void Demote()
    {
        //Debug.Log($"�����Ԃ����ɖ߂�: {UnitType} -> {OldUnitType}");
        UnitType = OldUnitType;
        FieldStatus = FieldStatus.Captured;
        transform.position = Vector3.zero;// �ʒu�����������Ď�����Ƃ��ď���
        transform.rotation = Quaternion.identity;
        promotedPrefab.SetActive(false);// �v���n�u���\���ɂ��邾���ō폜���Ȃ�
        //Debug.Log($"{UnitType} ��������ɂ��܂����B");
    }
    //--------�����������I���----------

    //--------��z�������J�n-----------
    public void AbsorCheck()
    {
        if (gameSystem.absorUnit.UnitType == UnitType.narigoma|| gameSystem.absorUnit.UnitType == UnitType.ryuuma || gameSystem.absorUnit.AbsorUnitType == UnitType.ryuuou)// �z��������ɋz�����ꂽ��̃��j�b�g�^�C�v���Q��
        {
            AbsorUnitType = gameSystem.absorUnit.OldUnitType;
            Debug.Log(gameSystem.absorUnit.UnitType + "���z��������");
            Debug.Log("OldUnit��" + AbsorUnitType + "����");
        }
        else
        {
            AbsorUnitType = gameSystem.absorUnit.OldUnitType;
            Debug.Log("���ʂ̋�z��������");
            Debug.Log("OldUnit��" + AbsorUnitType + "����");
        }
    }
    //--------��z�������I���----------

    //--------��u�ړ��v�\�͈͎擾�����J�n-----------
    public List<Vector2Int> GetMovableTiles(UnitController[,] units)
    {
        List<Vector2Int> ret = new List<Vector2Int>();

        switch (UnitType)
        {
            //����
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

            //����
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

            //�j�n
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

            //�⏫
            case UnitType.ginsyou:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions =
                    {
                    new Vector2Int(0, dir), // �O�i
�@�@�@�@�@          new Vector2Int(1, dir), // �E�O
�@�@�@�@�@          new Vector2Int(-1, dir), // ���O
�@�@�@�@�@          new Vector2Int(1, -dir), // �E���
�@�@�@�@�@          new Vector2Int(-1, -dir) // �����
�@�@�@�@�@  �@�@    };
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

            //����
            case UnitType.kinsyou:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions = {
                    new Vector2Int(0, dir),   // �O�i
�@�@�@�@�@          new Vector2Int(1, dir),   // �E�O
�@�@�@�@�@          new Vector2Int(-1, dir),  // ���O
�@�@�@�@�@          new Vector2Int(0, -dir),  // ���
�@�@�@�@�@          new Vector2Int(1, 0),     // �E
�@�@�@�@�@          new Vector2Int(-1, 0)     // ��
�@�@�@�@�@  �@�@    };
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
            //����
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

            //�ʏ�
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

            //���
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

            //�p�s
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

            //�����
            case UnitType.narigoma:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions = {
        new Vector2Int(0, dir),   // �O�i
�@�@�@�@�@        new Vector2Int(1, dir),   // �E�O
�@�@�@�@�@        new Vector2Int(-1, dir),  // ���O
�@�@�@�@�@        new Vector2Int(0, -dir),  // ���
�@�@�@�@�@        new Vector2Int(1, 0),     // �E
�@�@�@�@�@        new Vector2Int(-1, 0)     // ��
�@�@�@�@�@  �@�@  };
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
                    // ��ԂƓ����c���̖������ړ�
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

                    // 1�}�X�̎΂߈ړ�
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
                    // �p�s�Ɠ����΂߂̖������ړ�
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

                    // 1�}�X�̏c���ړ�
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
    //--------��u�ړ��v�\�͈͎擾�����I���---------

    //--------��u�z���v�\�͈͎擾�����J�n-----------
    public List<Vector2Int> GetAbsorptionTiles(UnitController[,] units)
    {
        List<Vector2Int> ret = new List<Vector2Int>();
        //UnitContlroller unitctrl = GetComponent<UnitContlroller>();

        //switch (unitctrl.UnitType)
        switch (UnitType)
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
                    //���ׂĂ̕������m�F
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
                    //���ׂĂ̕������m�F
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
                    //���ׂĂ̕������m�F
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
                    //���ׂĂ̕������m�F
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

            // ����
            case UnitType.kinsyou:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions =
                    {
                        new Vector2Int(1, -dir),  // �E���
                        new Vector2Int(-1, -dir), // �����
                    };
                    //���ׂĂ̕������m�F
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
                    //���ׂĂ̕������m�F
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
                    //���ׂĂ̕������m�F
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

            // �����
            case UnitType.narigoma:
                {
                    int dir = (Player == 0) ? -1 : 1;
                    Vector2Int[] directions =
                    {
                        new Vector2Int(1, -dir),  // �E���
                        new Vector2Int(-1, -dir), // �����
                    };
                    //���ׂĂ̕������m�F
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
    //--------��u�z���v�\�͈͎擾�����I���----------

    //--------�[�U�`�F�b�N�����J�n-----------
    public void FillingCheck(UnitController unit)
    {
        // �z�����ꂽ��̏����m�F���A�[�U�������s��
        if (unit.absorptionCheck && gameSystem.Turn >= unit.absorTurn +2)//�[�U�����͂O�ɂ���
        {
            unit.fillingCheck = true;       // �ʏ�̏[�U�������
        }
        else if (unit.absorptionCheck && gameSystem.Turn >= unit.absorTurn)
        {
            unit.fillingCheck = false;       // �ʏ�̏[�U�͂܂��������Ă��Ȃ�
        }
        else
        {
            unit.fillingCheck = false;       // �[�U���܂��������Ă��Ȃ�
        }
    }
    //--------�[�U�`�F�b�N�����I���----------

    //--------��u���o�v�\�͈͎擾�����J�n-----------
    public List<Vector2Int> GetRelesableTiles(UnitController[,] units)
    {
        List<Vector2Int> ret = new List<Vector2Int>();

        // switch() �� UnitType �� �z�����ꂽ��̃^�C�v�Acase ���� UnitType �͋z��������̃^�C�v
        switch (AbsorUnitType)
        {
            case UnitType.huhyou: // �z�����ꂽ�����
                {
                    // �v���C���[�ɂ���Č�����ς���
                    int dir = (Player == 0) ? -1 : 1;
                    // �X�^�[�g�ʒu
                    int X = 0;
                    int Y = 0;
                    // �{�����̓����i�e�F�����A�⏫�A�����A�����A���n�j�@2�}�X�O�ɑ���
                    if (UnitType == UnitType.huhyou || UnitType == UnitType.ginsyou || UnitType == UnitType.kinsyou || UnitType == UnitType.narigoma || UnitType == UnitType.ryuuma)
                    {
                        X = 0;
                        Y = 2;
                    }

                    //���ԕ���
                    ////�{�����̓����i�e�F���ԁj�@1�}�X���ɑ���
                    //if (UnitType == UnitType.kyousya)
                    //{
                    //    X = 0;
                    //    Y = -1;
                    //}

                    // �{�����̓����i�e�F�p�s�A�j�n�j�@1�}�X�O�ɑ���
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

            case UnitType.kyousya: // �z�����ꂽ�����
                {
                    // �v���C���[�ɂ���Č�����ς���
                    int dir = (Player == 0) ? -1 : 1;
                    // �c�����̃X�^�[�g�ʒu
                    int Fowards = 0;

                    Vector2Int Foward = new Vector2Int(0, dir);
                    Vector2Int Checkpos = Pos + Foward;

                    //���ԍ���
                    //// �{���Ԃ̓����i�e�F���ԁj�@�������ɑ���
                    //if (UnitType == UnitType.kyousya)
                    //{
                    //    Fowards = 1;
                    //    dir *= -1;

                    //    // ����
                    //    Vector2Int offset1 = new Vector2Int(-1, -1); // �E
                    //    Vector2Int offset2 = new Vector2Int(1, -1); // ��

                    //    // �E�O
                    //    Vector2Int direction1 = new Vector2Int(-1, -1); 
                    //    Vector2Int checkpos1 = Pos + direction1;
                    //    if (isCheckable(units, checkpos1) && units[checkpos1.x, checkpos1.y] == null)
                    //    {
                    //        Vector2Int newCenterPos1 = Pos + offset1;
                    //        Vector2Int directions1 = new Vector2Int(0, -1); // �E��
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

                    //    // �E�O
                    //    Vector2Int direction2 = new Vector2Int(1, -1);
                    //    Vector2Int checkpos2 = Pos + direction2;
                    //    if (isCheckable(units, checkpos2) && units[checkpos2.x, checkpos2.y] == null)
                    //    {
                    //        Vector2Int newCenterPos2 = Pos + offset2;
                    //        Vector2Int directions2 = new Vector2Int(0, -1); // �E��
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

                    // �{���Ԃ̓����i�e�F�p�s�A�j�n�j�@�O�����ɑ���
                    if (UnitType == UnitType.kakugyou || UnitType == UnitType.keima)
                    {
                        Fowards = 1;
                    }
                    // �{���Ԃ̓����i�e�F�����A�⏫�A�����A�����A���n�j�@�O����2�}�X�ڂ��瑝��
                    if (UnitType == UnitType.huhyou || UnitType == UnitType.ginsyou || UnitType == UnitType.kinsyou || UnitType == UnitType.narigoma || UnitType == UnitType.ryuuma)
                    {
                        Fowards = 2;
                    }

                    // �������i�O�����j
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

            case UnitType.keima:  // �z�����ꂽ��j�n
                {
                    // �v���C���[�ɂ���Č�����ς���
                    int dir = (Player == 0) ? -1 : 1;
                    //�j�n�j�n
                    //// �{�j�n�̓����i�e�F�j�n�j�@�������ɑ���
                    //if (UnitType == UnitType.keima)
                    //{
                    //    //dir *= -1;

                    //    // ����
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
                    //// �{�j�n�̓����i�e�F�j�n�ȊO�j�@�O�����ɑ���
                    //else
                    {
                        dir *= 1;
                    }

                    // ������
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

            case UnitType.ginsyou: // �z�����ꂽ��⏫
                {
                    // �v���C���[�ɂ���Č�����ς���
                    int dir = (Player == 0) ? -1 : 1;
                    // �e�����̃X�^�[�g�ʒu
                    int Fowards = 0;
                    int Foward_Rights = 0;
                    int Foward_Lefts = 0;
                    int Back_Rights = 0;
                    int Back_Lefts = 0;

                    // �{�⏫�̓����i�e�F�����j
                    if (UnitType == UnitType.huhyou)
                    {
                        Fowards = 2;
                        Foward_Rights = 1;
                        Foward_Lefts = 1;
                        Back_Rights = 1;
                        Back_Lefts = 1;
                    }
                    // �{�⏫�̓����i�e�F�p�s�j
                    if (UnitType == UnitType.kakugyou)
                    {
                        Fowards = 1;
                    }
                    // �{�⏫�̓����i�e�F��ԁA���ԁj
                    if (UnitType == UnitType.hisya || UnitType == UnitType.kyousya)
                    {
                        Foward_Rights = 1;
                        Foward_Lefts = 1;
                        Back_Rights = 1;
                        Back_Lefts = 1;
                    }
                    // �{�⏫�̓����i�e�F�j�n�j
                    if (UnitType == UnitType.keima)
                    {
                        Fowards = 1;
                        Foward_Rights = 1;
                        Foward_Lefts = 1;
                        Back_Rights = 1;
                        Back_Lefts = 1;
                    }
                    // �{�⏫�̓����i�e�F�⏫�j
                    if (UnitType == UnitType.ginsyou)
                    {
                        Fowards = 2;
                        Foward_Rights = 2;
                        Foward_Lefts = 2;
                        Back_Rights = 2;
                        Back_Lefts = 2;
                    }
                    // �{�⏫�̓����i�e�F�����A�����j
                    if (UnitType == UnitType.kinsyou || UnitType == UnitType.narigoma)
                    {
                        Fowards = 2;
                        Foward_Rights = 2;
                        Foward_Lefts = 2;
                        Back_Rights = 1;
                        Back_Lefts = 1;
                    }
                    // �{�⏫�̓����i�e�F���n�j
                    if (UnitType == UnitType.ryuuma)
                    {
                        Fowards = 2;
                    }
                    // �{�⏫�̓����i�e�F�����j
                    if (UnitType == UnitType.ryuuou)
                    {
                        Foward_Rights = 2;
                        Foward_Lefts = 2;
                        Back_Rights = 2;
                        Back_Lefts = 2;
                    }

                    // �O
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
                    // �E�O
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
                    // ���O
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
                    // �E���
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
                    // �����
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

            case UnitType.kinsyou: // �z�����ꂽ�����
                {
                    // �v���C���[�ɂ���Č�����ς���
                    int dir = (Player == 0) ? -1 : 1;
                    // �e�����̃X�^�[�g�ʒu
                    int Fowards = 0;
                    int Foward_Rights = 0;
                    int Foward_Lefts = 0;
                    int Rights = 0;
                    int Lefts = 0;
                    int Backs = 0;

                    // �{�����̓����i�e�F�����j�@�O����2�}�X�ڂ��瑝���B���̑��A���ʂɑ���
                    if (UnitType == UnitType.huhyou)
                    {
                        Fowards = 2;
                        Foward_Rights = 1;
                        Foward_Lefts = 1;
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // �{�����̓����i�e�F�p�s�j�@�c��1�}�X�ڂɑ���
                    if (UnitType == UnitType.kakugyou)
                    {
                        Fowards = 1;
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // �{�����̓����i�e�F��ԁj�@�΂ߑO�����ɑ���
                    if (UnitType == UnitType.hisya)
                    {
                        Foward_Rights = 1;
                        Foward_Lefts = 1;
                    }
                    // �{�����̓����i�e�F���ԁj�@�O�����ȊO�ɕ��ʂɑ���
                    if (UnitType == UnitType.kyousya)
                    {
                        Foward_Rights = 1;
                        Foward_Lefts = 1;
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // �{�����̓����i�e�F�j�n�j�@���ʂɑ���
                    if (UnitType == UnitType.keima)
                    {
                        Fowards = 1;
                        Foward_Rights = 1;
                        Foward_Lefts = 1;
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // �{�����̓����i�e�F�⏫�j�@�O�A�΂ߑO����2�}�X�ڂɑ����B���̑��A���ʂɑ����B
                    if (UnitType == UnitType.ginsyou)
                    {
                        Fowards = 2;
                        Foward_Rights = 2;
                        Foward_Lefts = 2;
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // �{�����̓����i�e�F�����A�����j�@�e����2�}�X�ڂɑ���
                    if (UnitType == UnitType.kinsyou || UnitType == UnitType.narigoma)
                    {
                        Fowards = 2;
                        Foward_Rights = 2;
                        Foward_Lefts = 2;
                        Rights = 2;
                        Lefts = 2;
                        Backs = 2;
                    }
                    // �{�����̓����i�e�F���n�j�@�c��2�}�X�ڂɑ���
                    if (UnitType == UnitType.ryuuma)
                    {
                        Fowards = 2;
                        Rights = 2;
                        Lefts = 2;
                        Backs = 2;
                    }
                    // �{�����̓����i�e�F�����j�@�΂ߑO����2�}�X�ڂɑ���
                    if (UnitType == UnitType.ryuuou)
                    {
                        Foward_Rights = 2;
                        Foward_Lefts = 2;
                    }

                    // �O
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
                    // �E�O
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
                    // ���O
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
                    // �E
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
                    // ��
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
                    // �O
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

            case UnitType.hisya: // �z�����ꂽ����
                {
                    // �v���C���[�ɂ���Č�����ς���
                    int dir = (Player == 0) ? -1 : 1;
                    // �c��4�����̃X�^�[�g�ʒu
                    int Fowards = 0;
                    int Rights = 0;
                    int Lefts = 0;
                    int Backs = 0;

                    // �{��Ԃ̓����i�e�F�����A�⏫�j�@�O�����̂�2�}�X�ڂ��瑝���B���̑��A���ʂɑ����B
                    if (UnitType == UnitType.huhyou || UnitType == UnitType.ginsyou)
                    {
                        Fowards = 2;
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // �{��Ԃ̓����i�e�F�p�s�A�j�n�j�@�e�������ʂɑ����B
                    if (UnitType == UnitType.kakugyou || UnitType == UnitType.keima)
                    {
                        Fowards = 1;
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // �{��Ԃ̓����i�e�F���ԁj�@�O�����ȊO��3���������B
                    if (UnitType == UnitType.kyousya)
                    {
                        Rights = 1;
                        Lefts = 1;
                        Backs = 1;
                    }
                    // �{��Ԃ̓����i�e�F�����A�����A���n�j�@�e����2�}�X�ڂ��瑝���B
                    if (UnitType == UnitType.kinsyou || UnitType == UnitType.narigoma || UnitType == UnitType.ryuuma)
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
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
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
                            else
                            {
                                ret.Add(checkpos);
                            }
                        }

                        Vector2Int[] direction2 = { new Vector2Int(-1, 1) }; // �E��
                        foreach (var dir2 in direction2)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
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
                            else
                            {
                                ret.Add(checkpos);
                            }
                        }

                        Vector2Int[] direction3 = { new Vector2Int(-1, -1) }; // �E��
                        foreach (var dir2 in direction3)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
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
                            else
                            {
                                ret.Add(checkpos);
                            }
                        }

                        Vector2Int[] direction4 = { new Vector2Int(1, -1) }; // ����
                        foreach (var dir2 in direction4)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
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
                            else
                            {
                                ret.Add(checkpos);
                            }
                        }
                    }

                    // �{��Ԃ̓����i�e�F�����j ����O�ɂ�����
                    if (UnitType == UnitType.ryuuou)
                    {
                        Vector2Int offset1 = new Vector2Int(1, 1); // ����
                        Vector2Int offset2 = new Vector2Int(-1, 1); // �E��
                        Vector2Int offset3 = new Vector2Int(-1, -1); // �E��
                        Vector2Int offset4 = new Vector2Int(1, -1); // ����

                        Vector2Int[] direction1 = { new Vector2Int(1, 1) }; // ����
                        foreach (var dir1 in direction1)
                        {
                            Vector2Int checkpos = Pos + dir1;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
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
                            }
                        }

                        Vector2Int[] direction2 = { new Vector2Int(-1, 1) }; // �E��
                        foreach (var dir2 in direction2)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
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
                            }
                        }

                        Vector2Int[] direction3 = { new Vector2Int(-1, -1) }; // �E��
                        foreach (var dir2 in direction3)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
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
                            }
                        }

                        Vector2Int[] direction4 = { new Vector2Int(1, -1) }; // ����
                        foreach (var dir2 in direction4)
                        {
                            Vector2Int checkpos = Pos + dir2;
                            if (isCheckable(units, checkpos) && units[checkpos.x, checkpos.y] == null)
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
                    // ���
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
                    // �E
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
                    // ��
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

            case UnitType.kakugyou: // �z�����ꂽ��p�s
                {
                    // �v���C���[�ɂ���Č�����ς���
                    int dir = (Player == 0) ? -1 : 1;
                    // �΂�4�����̃X�^�[�g�ʒu
                    int Foward_Left = 0;   // ���O
                    int Foward_Right = 0;  // �E�O
                    int Back_Right = 0;    // �E��
                    int Back_Left = 0;     // ����

                    // �{�p�s�̓����i�e�F�����A��ԁA���ԁA�j�n�j�@�e�������ʂɑ����B
                    if (UnitType == UnitType.huhyou || UnitType == UnitType.hisya || UnitType == UnitType.kyousya || UnitType == UnitType.keima)
                    {
                        Foward_Left = 1;
                        Foward_Right = 1;
                        Back_Right = 1;
                        Back_Left = 1;

                    }
                    // �{�p�s�̓����i�e�F�⏫�A�����j�@�e����2�}�X�ڂ��瑝���B
                    if (UnitType == UnitType.ginsyou || UnitType == UnitType.ryuuou)
                    {
                        Foward_Left = 2;
                        Foward_Right = 2;
                        Back_Right = 2;
                        Back_Left = 2;
                    }
                    // �{�p�s�̓����i�e�F�����A�����j�@�O�����̂�2�}�X�ڂ��瑝���B���̑��A���ʂɑ����B
                    if (UnitType == UnitType.kinsyou || UnitType == UnitType.narigoma)
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

                    // �{�p�s�̓����i�e�F���n�j ����O�ɂ�����
                    if (UnitType == UnitType.ryuuma)
                    {
                        Vector2Int offset1 = new Vector2Int(0, -1); // ��
                        Vector2Int offset2 = new Vector2Int(0, 1); // ��
                        Vector2Int offset3 = new Vector2Int(-1, 0); // �E
                        Vector2Int offset4 = new Vector2Int(1, 0); // ��

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

                    // ����
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
                    // �E��
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
                    // ����
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
                    // �E��
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
    //--------��u���o�v�\�͈͎擾�����I���----------

    //--------�v���n�u�ύX�����J�n-----------
    public void ChangePrefab(GameObject newPrefab)
    {
        if (newPrefab == null)
        {
            //Debug.LogError("ChangePrefab���\�b�h��newPrefab��null�ł��B");
            return;
        }

        // ���݂̈ʒu���Ɖ�]����ۑ�
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;
        Vector2Int currentPosIndex = Pos;

        // �V�����v���n�u�𐶐�
        GameObject newUnit = Instantiate(newPrefab, currentPosition, currentRotation);
        UnitController newUnitController = newUnit.GetComponent<UnitController>();

        // Collider���A�^�b�`����Ă��邩�m�F
        if (newUnitController.GetComponent<Collider>() == null)
        {
            //Debug.LogWarning("�V������ɂ�Collider���A�^�b�`����Ă��܂���B");
        }

        //// ���̋��Rigidbody��ԁi�����I�ȋ����j�������p��
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
        //        Debug.LogWarning("�V�������Rigidbody��������܂���B");
        //    }
        //}

        // �Q�[���V�X�e�����擾
        GameSystem gameSystem = UnityEngine.Object.FindAnyObjectByType<GameSystem>();
        if (gameSystem == null)
        {
            //Debug.LogError("GameSystem��������܂���ł����B");
            return;
        }

        // ���݂̃^�C�����擾
        GameObject currentTile = gameSystem.GetTile(currentPosIndex);
        if (currentTile == null)
        {
            //Debug.LogError("�w�肳�ꂽ�ʒu�̃^�C����������܂���ł����B");
            return;
        }

        // �V������Ƀv���C���[�ƈʒu���������p��
        newUnitController.Init(Player, (int)UnitType, currentTile, currentPosIndex);

        // ���̃I�u�W�F�N�g���폜
        Destroy(this.gameObject);
    }

    //--------�v���n�u�ύX�����I���-----------


    //--------�⏕���\�b�h�J�n-----------

    bool isCheckable(UnitController[,] ary, Vector2Int idx)
    {
        return idx.x >= 0 && idx.x < ary.GetLength(0) && idx.y >= 0 && idx.y < ary.GetLength(1);
    }
    //--------�⏕���\�b�h�I���----------
    //---------�[�U�m�F--------------------
}