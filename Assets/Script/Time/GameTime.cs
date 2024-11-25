using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTime : MonoBehaviour
{
    [SerializeField, Header("�������Ԃ̕\��")]
    public TMP_Text[] playerTimes;
    public TMP_Text[] SettingTime;

    private GameSystem gameSystem;
    private int battleTime;
    private int[] battleTimes = new int[2]; // �e�v���C���[�̐������Ԃ��i�[
    private float timer;
    private int initialBattleTime;
    private int initialBattleTimes;
    private bool canResetTime;
    private bool canStopTime;
    private bool[] isTimeRunning = new bool[2]; // �e�v���C���[�̎��Ԃ��i�s�����ǂ���
    private bool[] extraTimeAdded = new bool[2]; // �������Ԃ�ǉ����������m�F���邽�߂̃t���O�z���ǉ�
    public bool isPaused = false;

    void Start()
    {
        TimeSystem();
        // �Q�[���V�X�e�����猻�݂̃v���C���[���擾���Ď������Ԃ�ݒ�
        if (gameSystem != null && canStopTime)
        {
            int currentPlayer = gameSystem.GetCurrentPlayer(); // GameSystem�̌��݂̃v���C���[���擾
            StopBattleTime(currentPlayer); // �擾�����v���C���[�Ŏ������Ԃ��X�^�[�g
        }
    }

    void Update()
    {
        if (isPaused) return; // isPaused��true�Ȃ玞�Ԃ�i�߂Ȃ�
        //���ł�
        One();
        //��������
        HandTime();
    }

    private void TimeSystem()
    {
        gameSystem = FindAnyObjectByType<GameSystem>();  // GameSystem���擾
        string selectedTimeType = PlayerPrefs.GetString("SelectedTimeType", "");
        if (selectedTimeType == "TitleTime1" && PlayerPrefs.HasKey("SelectedBattleTime1"))
        {
            initialBattleTime = PlayerPrefs.GetInt("SelectedBattleTime1");
            canResetTime = true;
            canStopTime = false;
            battleTime = initialBattleTime;  // �����̐������Ԃ�ۑ�
            DisplayBattleTime(battleTime);
        }
        else if (selectedTimeType == "TitleTime2" && PlayerPrefs.HasKey("SelectedBattleTime2"))
        {
            initialBattleTimes = PlayerPrefs.GetInt("SelectedBattleTime2");
            canResetTime = false;
            canStopTime = true;
            battleTimes[0] = initialBattleTimes;
            battleTimes[1] = initialBattleTimes;
            DisplayBattleTimes(0);
            DisplayBattleTimes(1);
            StopBattleTime(0);  // �Q�[���J�n���Ƀv���C���[1�̎��Ԃ�i�s������
        }
        else
        {
            initialBattleTime = 60;
            canResetTime = true;
            Debug.LogWarning("�ݒ肪������Ȃ����߁A�f�t�H���g�̐������Ԃ��g�p���܂��B");
            battleTime = initialBattleTime;  // �����̐������Ԃ�ۑ�
            DisplayBattleTime(battleTime);
        }
    }

    //���ł�
    private void One()
    {
        if (canResetTime == true)
        {
            timer += UnityEngine.Time.deltaTime;
            if (timer >= 1)
            {
                timer = 0;
                battleTime--;
                DisplayBattleTime(battleTime);
            }

            if (battleTime <= 0)
            {
                if (gameSystem != null)
                {
                    string winner = (gameSystem.GetCurrentPlayer() == 0) ? "Player2" : "Player1";
                    StartCoroutine(gameSystem.EndGame(winner));
                }
                else
                {
                    Debug.LogError("GameSystem�ւ̎Q�Ƃ�������܂���");
                }
            }
        }
    }

    //��������
    private void HandTime()
    {
        if (canStopTime == true)
        {
            for (int i = 0; i < isTimeRunning.Length; i++)
            {
                if (isTimeRunning[i])
                {
                    timer += UnityEngine.Time.deltaTime;
                    if (timer >= 1)
                    {
                        timer = 0;
                        battleTimes[i]--;
                        DisplayBattleTimes(i);
                    }

                    if (battleTimes[i] <= 0)
                    {
                        // �������Ԃ�0�ȉ��ɂȂ����ہA�t���O��false�̎�����1����ǉ����A���̌�͈ێ�
                        if (!extraTimeAdded[i])
                        {
                            battleTimes[i] = 60;  // �������Ԃ�1���Ƀ��Z�b�g
                            extraTimeAdded[i] = true;  // �t���O��true�̂܂܈ێ�
                        }
                        else if (extraTimeAdded[i])
                        {
                            // �t���O��true�̂܂܂ŁA0�ɂȂ����ꍇ�ɂ̂݃V�[���J�ڂ��s��
                            if (gameSystem != null)
                            {
                                string winner = (gameSystem.GetCurrentPlayer() == 0) ? "Player2" : "Player1";
                                StartCoroutine(gameSystem.EndGame(winner));
                            }
                            else
                            {
                                Debug.LogError("GameSystem�ւ̎Q�Ƃ�������܂���");
                            }
                        }
                    }
                }
            }
        }
    }

    //���
    private void DisplayBattleTime(int limitTime)
    {
        string timeString = ((int)(limitTime / 60)).ToString("00") + ":" + ((int)(limitTime % 60)).ToString("00");
        foreach (TMP_Text playerTime in playerTimes)
        {
            playerTime.text = timeString;
        }
        foreach (TMP_Text playerTime in SettingTime)
        {
            playerTime.text = timeString;
        }
    }

    //��������
    private void DisplayBattleTimes(int playerIndex)
    {
        string timeString = ((int)(battleTimes[playerIndex] / 60)).ToString("00") + ":" + ((int)(battleTimes[playerIndex] % 60)).ToString("00");
        playerTimes[playerIndex].text = timeString;
        SettingTime[playerIndex].text = timeString;
    }

    //���
    public void ResetBattleTime()
    {
        if (canResetTime)
        {
            battleTime = initialBattleTime;
            Debug.Log("���Ԃ����Z�b�g����܂���: " + battleTime);
            DisplayBattleTime(battleTime);
        }
        else
        {
            Debug.LogWarning("���̃o�g���^�C���ݒ�ł͎��Ԃ̃��Z�b�g�͎g�p�ł��܂���B");
        }
    }

    //��������
    public void StopBattleTime(int currentPlayer)
    {
        if (canStopTime)
        {
            isTimeRunning[0] = currentPlayer == 0;
            isTimeRunning[1] = currentPlayer == 1;
            //Debug.Log($"�v���C���[{currentPlayer + 1}�̎��Ԃ��i�s���Ă��܂��B");
            if (extraTimeAdded[0] == true)
            {
                battleTimes[0] = 60;
                DisplayBattleTimes(0);
            }
            if (extraTimeAdded[1] == true)
            {
                battleTimes[1] = 60;
                DisplayBattleTimes(1);
            }
        }
        else
        {
            //Debug.LogWarning("���̃o�g���^�C���ݒ�ł͎��Ԃ̃L�[�v�͎g�p�o���܂���B");
        }
    }
}
