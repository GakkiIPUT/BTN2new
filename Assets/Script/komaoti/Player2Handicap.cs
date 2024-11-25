using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Player2Handicap : MonoBehaviour
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
            { 1, new List<(int, int)> { (0, 0) } }, //����(�p�̂��鑤)�̍�
            { 2, new List<(int, int)> { (1, 1) } }, // �p����
            { 3, new List<(int, int)> { (7, 1) } }, //��ԗ���
            { 4, new List<(int, int)> { (7, 1), (0, 0) } }, //��Ԃƍ���(�p�̂��鑤)�̍�
            { 5, new List<(int, int)> { (1, 1), (7, 1) } }, //��ԂƊp
            { 6, new List<(int, int)> { (1, 1), (7, 1), (0, 0), (8, 0) }}, //��ԂƊp�A�����̍�
            { 7, new List<(int, int)> { (1, 1), (7, 1), (0, 0), (8 ,0 ), (1, 0), (7, 0) }}, //��ԂƊp�A�����̌j�ƍ�
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
            PlayerPrefs.SetInt("HandicapSetting2", index); // �C���f�b�N�X��ۑ�

            // ����̈ʒu���𕶎���ŕۑ�
            string positions = string.Join(";", CurrentHandicapSetting.Select(pos => $"{pos.row},{pos.col}"));
            PlayerPrefs.SetString("HandicapPositions2", positions);

            //Debug.Log("Handicap2 - ����ݒ肪�ύX����܂���: �C���f�b�N�X " + index + ", �ݒ���e " + positions);
            //displayText.text = ("����ݒ肪�ύX����܂���2" + index + ", �ݒ���e " + positions);
        }
    }
}
