using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Player1Handicap : MonoBehaviour
{
    [SerializeField, Header("Handicap �h���b�v�_�E��")]
    private TMP_Dropdown HandicapDropdown;

    //public TMP_Text displayText;  // �I����e��\�����邽�߂̃e�L�X�g

    private Dictionary<int, List<(int row, int col)>> handicapSettings;
    public List<(int row, int col)> CurrentHandicapSetting { get; private set; } = new List<(int row, int col)>();

    private void Awake()
    {
        InitializeHandicapSettings();
        SetupDropdown();
        HandicapDropdown.onValueChanged.AddListener(OnHandicapSelected);
    }

    private void Start()
    {
        // �����I�����ꂽ�l�Ɋ�Â��A����ݒ�𔽉f
        OnHandicapSelected(HandicapDropdown.value);
    }

    private void InitializeHandicapSettings()
    {
        handicapSettings = new Dictionary<int, List<(int, int)>>()
        {
            { 0, new List<(int, int)> { } }, // �Ȃ�
            { 1, new List<(int, int)> { (8, 8) } }, //����(�p�̂��鑤)�̍�
            { 2, new List<(int, int)> { (7, 7) } }, // �p����
            { 3, new List<(int, int)> { (1, 7) } }, //��ԗ���
            { 4, new List<(int, int)> { (1, 7), (8, 8) } }, //��Ԃƍ���(�p�̂��鑤)�̍�
            { 5, new List<(int, int)> { (7, 7), (1, 7) } }, //��ԂƊp
            { 6, new List<(int, int)> { (7, 7), (1, 7), (8, 8), (0, 8) }}, //��ԂƊp�A�����̍�
            { 7, new List<(int, int)> { (7, 7), (1, 7), (8, 8), (0, 8), (7, 8), (1, 8) }}, //��ԂƊp�A�����̌j�ƍ�
        };
        //Debug.Log("Handicap - ����ݒ��������");
    }

    private void SetupDropdown()
    {
        HandicapDropdown.ClearOptions();
        HandicapDropdown.AddOptions(new List<string> { "����", "������(�����̍�)", "�p����", "��ԗ���", "��ԍ�����(��Ԃƍ����̍�)", "�񖇗���(��ԂƊp)", "�l������(��ԂƊp�A�����̍�)", "�Z������(��ԂƊp�A�����̌j�ƍ�)" });
        //Debug.Log("Handicap - �h���b�v�_�E���ɃI�v�V������ݒ�");
    }

    private void OnHandicapSelected(int index)
    {
        if (handicapSettings.ContainsKey(index))
        {
            CurrentHandicapSetting = handicapSettings[index];
            PlayerPrefs.SetInt("HandicapSetting1", index); // �C���f�b�N�X��ۑ�

            // ����̈ʒu���𕶎���ŕۑ�
            string positions = string.Join(";", CurrentHandicapSetting.Select(pos => $"{pos.row},{pos.col}"));
            PlayerPrefs.SetString("HandicapPositions1", positions);

            //Debug.Log("����ݒ肪�ύX����܂���" + index + ", �ݒ���e " + positions);
            //displayText.text = ("����ݒ肪�ύX����܂���1" + index + ", �ݒ���e " + positions);
        }
    }
}
