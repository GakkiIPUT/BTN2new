using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTime : MonoBehaviour
{
    [SerializeField, Header("制限時間の表示")]
    public TMP_Text[] playerTimes;
    public TMP_Text[] SettingTime;

    private GameSystem gameSystem;
    private int battleTime;
    private int[] battleTimes = new int[2]; // 各プレイヤーの制限時間を格納
    private float timer;
    private int initialBattleTime;
    private int initialBattleTimes;
    private bool canResetTime;
    private bool canStopTime;
    private bool[] isTimeRunning = new bool[2]; // 各プレイヤーの時間が進行中かどうか
    private bool[] extraTimeAdded = new bool[2]; // 持ち時間を追加したかを確認するためのフラグ配列を追加
    public bool isPaused = false;

    void Start()
    {
        TimeSystem();
        // ゲームシステムから現在のプレイヤーを取得して持ち時間を設定
        if (gameSystem != null && canStopTime)
        {
            int currentPlayer = gameSystem.GetCurrentPlayer(); // GameSystemの現在のプレイヤーを取得
            StopBattleTime(currentPlayer); // 取得したプレイヤーで持ち時間をスタート
        }
    }

    void Update()
    {
        if (isPaused) return; // isPausedがtrueなら時間を進めない
        //一手打ち
        One();
        //持ち時間
        HandTime();
    }

    private void TimeSystem()
    {
        gameSystem = FindAnyObjectByType<GameSystem>();  // GameSystemを取得
        string selectedTimeType = PlayerPrefs.GetString("SelectedTimeType", "");
        if (selectedTimeType == "TitleTime1" && PlayerPrefs.HasKey("SelectedBattleTime1"))
        {
            initialBattleTime = PlayerPrefs.GetInt("SelectedBattleTime1");
            canResetTime = true;
            canStopTime = false;
            battleTime = initialBattleTime;  // 初期の制限時間を保存
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
            StopBattleTime(0);  // ゲーム開始時にプレイヤー1の時間を進行させる
        }
        else
        {
            initialBattleTime = 60;
            canResetTime = true;
            Debug.LogWarning("設定が見つからないため、デフォルトの制限時間を使用します。");
            battleTime = initialBattleTime;  // 初期の制限時間を保存
            DisplayBattleTime(battleTime);
        }
    }

    //一手打ち
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
                    Debug.LogError("GameSystemへの参照が見つかりません");
                }
            }
        }
    }

    //持ち時間
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
                        // 持ち時間が0以下になった際、フラグがfalseの時だけ1分を追加し、その後は維持
                        if (!extraTimeAdded[i])
                        {
                            battleTimes[i] = 60;  // 持ち時間を1分にリセット
                            extraTimeAdded[i] = true;  // フラグをtrueのまま維持
                        }
                        else if (extraTimeAdded[i])
                        {
                            // フラグがtrueのままで、0になった場合にのみシーン遷移を行う
                            if (gameSystem != null)
                            {
                                string winner = (gameSystem.GetCurrentPlayer() == 0) ? "Player2" : "Player1";
                                StartCoroutine(gameSystem.EndGame(winner));
                            }
                            else
                            {
                                Debug.LogError("GameSystemへの参照が見つかりません");
                            }
                        }
                    }
                }
            }
        }
    }

    //一手
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

    //持ち時間
    private void DisplayBattleTimes(int playerIndex)
    {
        string timeString = ((int)(battleTimes[playerIndex] / 60)).ToString("00") + ":" + ((int)(battleTimes[playerIndex] % 60)).ToString("00");
        playerTimes[playerIndex].text = timeString;
        SettingTime[playerIndex].text = timeString;
    }

    //一手
    public void ResetBattleTime()
    {
        if (canResetTime)
        {
            battleTime = initialBattleTime;
            Debug.Log("時間がリセットされました: " + battleTime);
            DisplayBattleTime(battleTime);
        }
        else
        {
            Debug.LogWarning("このバトルタイム設定では時間のリセットは使用できません。");
        }
    }

    //持ち時間
    public void StopBattleTime(int currentPlayer)
    {
        if (canStopTime)
        {
            isTimeRunning[0] = currentPlayer == 0;
            isTimeRunning[1] = currentPlayer == 1;
            //Debug.Log($"プレイヤー{currentPlayer + 1}の時間が進行しています。");
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
            //Debug.LogWarning("このバトルタイム設定では時間のキープは使用出来ません。");
        }
    }
}
