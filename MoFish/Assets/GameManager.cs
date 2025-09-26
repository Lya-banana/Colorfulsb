using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class GameManager : MonoBehaviour
{
    // ========= 单例模式 =========
    public static GameManager Instance;

    // ========= 游戏状态变量 =========
    public int currentDay = 1;
    public int totalDays = 30;
    public int actionPoints;
    private const int MAX_ACTION_POINTS = 5;

    // ========= 玩家属性 (全局变量) =========
    public int hp = 100;
    public int sanity = 100; // 精神值
    public int gold = 50;

    // ========= UI引用 =========
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI apText;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI sanityText;
    public TextMeshProUGUI goldText;

    // ========= 数据存储 =========
    private Dictionary<int, string> dailyMessages = new Dictionary<int, string>();
    // --- 修改点 1: 将 endOfDaySummary 设为 public ---
    public string endOfDaySummary = ""; // 用于存储每日总结

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDailyData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            // 首次启动时，找到所有UI并开始第一天
            FindAllUIElements();
            StartNewDay();
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 场景加载完成后的回调函数
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainScene")
        {
            // 每次返回主场景，都需要重新查找UI元素和重新绑定按钮
            FindAllUIElements();
            BindButtons();
            StartNewDay();
        }
    }

    // 集中查找所有UI元素
    void FindAllUIElements()
    {
        dayText = GameObject.Find("DayText")?.GetComponent<TextMeshProUGUI>();
        apText = GameObject.Find("APText")?.GetComponent<TextMeshProUGUI>();
        messageText = GameObject.Find("MessageText")?.GetComponent<TextMeshProUGUI>();
        hpText = GameObject.Find("HPText")?.GetComponent<TextMeshProUGUI>();
        sanityText = GameObject.Find("SanityText")?.GetComponent<TextMeshProUGUI>();
        goldText = GameObject.Find("GoldText")?.GetComponent<TextMeshProUGUI>();
    }

    // 集中绑定按钮事件
    void BindButtons()
    {
        // 通过按钮名字查找并绑定对应的方法
        Button trainButton = GameObject.Find("TrainButton")?.GetComponent<Button>();
        if (trainButton != null)
        {
            trainButton.onClick.RemoveAllListeners();
            trainButton.onClick.AddListener(OnTrainButtonClicked);
        }

        Button restButton = GameObject.Find("RestButton")?.GetComponent<Button>();
        if (restButton != null)
        {
            restButton.onClick.RemoveAllListeners();
            restButton.onClick.AddListener(OnRestButtonClicked);
        }

        Button studyButton = GameObject.Find("StudyButton")?.GetComponent<Button>();
        if (studyButton != null)
        {
            studyButton.onClick.RemoveAllListeners();
            studyButton.onClick.AddListener(OnStudyButtonClicked);
        }
    }


    void LoadDailyData()
    {
        string filePath = Path.Combine(Application.dataPath, "Data", "DailyData.csv");
        if (!File.Exists(filePath))
        {
            Debug.LogError("在 Assets/Data/ 文件夹中找不到 DailyData.csv 文件! 路径: " + filePath);
            return;
        }
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;
                string[] values = line.Split(',');
                if (values.Length < 2) continue;
                if (!int.TryParse(values[0], out int day)) continue;
                string message = values[1];
                if (message.StartsWith("\"") && message.EndsWith("\"")) message = message.Substring(1, message.Length - 2);
                if (!dailyMessages.ContainsKey(day)) dailyMessages.Add(day, message);
            }
        }
        catch (System.Exception e) { Debug.LogError("读取或解析CSV文件时发生错误: " + e.Message); }
    }

    // --- 修改点 2: 简化 StartNewDay 函数 ---
    void StartNewDay()
    {
        if (currentDay > totalDays)
        {
            SceneManager.LoadScene("EndScene");
            return;
        }

        actionPoints = MAX_ACTION_POINTS;
        UpdateUI();

        // 获取当天的信息并直接显示
        string dailyMessage = dailyMessages.ContainsKey(currentDay) ? dailyMessages[currentDay] : "今天又是平常的一天。";
        if (messageText != null)
        {
            messageText.text = dailyMessage;
        }

        // 清空上一天的总结，因为它已经在过渡场景中显示过了
        endOfDaySummary = "";
    }


    // 更新所有UI文本
    void UpdateUI()
    {
        if (dayText == null || apText == null || messageText == null || hpText == null || sanityText == null || goldText == null)
        {
            Debug.LogError("一个或多个UI引用未设置或未找到！");
            return;
        }
        dayText.text = $"第 {currentDay} 天";
        apText.text = $"行动点: {actionPoints} / {MAX_ACTION_POINTS}";
        hpText.text = $"HP: {hp}";
        sanityText.text = $"精神: {sanity}";
        goldText.text = $"金币: {gold}";
    }

    // 统一处理行动点消耗
    private void ConsumeActionPoint()
    {
        if (actionPoints > 0)
        {
            actionPoints--;
            UpdateUI(); // 每次消耗都更新UI
            if (actionPoints <= 0)
            {
                EndDay();
            }
        }
    }

    // ========= 按钮对应的具体方法 =========
    public void OnTrainButtonClicked()
    {
        if (actionPoints <= 0) return;
        hp -= 5;
        sanity -= 7;
        Debug.Log("执行训练：HP-5, 精神-7");
        ConsumeActionPoint();
    }

    public void OnRestButtonClicked()
    {
        if (actionPoints <= 0) return;
        hp += 3;
        sanity += 2;
        Debug.Log("执行休息：HP+3, 精神+2");
        ConsumeActionPoint();
    }

    public void OnStudyButtonClicked()
    {
        if (actionPoints <= 0) return;
        hp -= 9;
        sanity -= 4;
        Debug.Log("执行学习：HP-9, 精神-4");
        ConsumeActionPoint();
    }

    // 结束一天，进行结算
    void EndDay()
    {
        Debug.Log($"第 {currentDay} 天结束。");

        // 1. 记录变化前数值
        int oldHp = hp;
        int oldSanity = sanity;
        int oldGold = gold;

        // 2. 结算每日收益和随机恢复
        gold += 20;
        int hpRecovery = Random.Range(0, 6); // 随机恢复0到5点
        int sanityRecovery = Random.Range(0, 6); // 随机恢复0到5点
        hp += hpRecovery;
        sanity += sanityRecovery;

        // 3. 生成总结文本，为下一天做准备
        endOfDaySummary = $"<color=yellow>本日结束结算：</color>\n" +
                          $"HP: {oldHp} -> {hp} (恢复了{hpRecovery})\n" +
                          $"精神: {oldSanity} -> {sanity} (恢复了{sanityRecovery})\n" +
                          $"金币: {oldGold} -> {gold} (获得了20)";

        // 4. 天数增加并加载转场场景
        currentDay++;
        SceneManager.LoadScene("TransitionScene");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }
    }
}

