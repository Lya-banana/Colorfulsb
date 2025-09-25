using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间
using TMPro; // 引入TextMeshPro命名空间
using UnityEngine.SceneManagement; // 引入场景管理命名空间
using System.Collections.Generic; // 引入集合命名空间
using System.IO; // 引入文件读写命名空间

public class GameManager : MonoBehaviour
{
    // ========= 单例模式 =========
    // 确保整个游戏中只有一个GameManager实例
    public static GameManager Instance;

    // ========= 游戏状态变量 =========
    public int currentDay = 1; // 当前天数
    public int totalDays = 30; // 总天数
    public int actionPoints; // 当前行动点
    private const int MAX_ACTION_POINTS = 5; // 每轮最大行动点

    // ========= UI引用 =========
    // 在Unity编辑器中，需要把场景里的UI拖到这里
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI apText;
    public TextMeshProUGUI messageText;

    // ========= 数据存储 =========
    // 使用一个字典来存储每天的信息，Key是天数(int)，Value是信息(string)
    private Dictionary<int, string> dailyMessages = new Dictionary<int, string>();

    void Awake()
    {
        // 单例模式的实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切换场景时不销毁此对象
            LoadDailyData(); // 在游戏开始时就加载数据
        }
        else
        {
            Destroy(gameObject); // 如果已存在实例，则销毁新的这个
        }
    }

    void Start()
    {
        // 游戏第一次启动时调用
        // 如果是从转场场景回来，OnSceneLoaded会处理初始化
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            StartNewDay();
        }
    }

    // 当一个场景被加载时，这个方法会被调用
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 场景加载完成后的回调函数
    // 场景加载完成后的回调函数
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 检查是否回到了主场景
        if (scene.name == "MainScene")
        {
            Debug.Log("已返回主场景，正在重新初始化UI...");

            // 1. 重新获取UI文本元素的引用
            dayText = GameObject.Find("DayText").GetComponent<TextMeshProUGUI>();
            apText = GameObject.Find("APText").GetComponent<TextMeshProUGUI>();
            messageText = GameObject.Find("MessageText").GetComponent<TextMeshProUGUI>();

            // 2. 【新增代码】重新查找并绑定所有行动按钮的点击事件
            // 首先找到所有带 "ActionButton" 标签的游戏对象
            GameObject[] actionButtons = GameObject.FindGameObjectsWithTag("ActionButton");

            Debug.Log($"找到了 {actionButtons.Length} 个行动按钮。");

            // 遍历所有找到的按钮
            foreach (GameObject buttonObj in actionButtons)
            {
                // 获取该对象上的Button组件
                Button button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    // 先移除所有旧的监听，防止重复添加
                    button.onClick.RemoveAllListeners();
                    // 再添加新的监听，让它调用我们的函数
                    button.onClick.AddListener(OnActionButtonClicked);
                }
            }

            // 3. 开始新的一天
            StartNewDay();
        }
    }


    // 读取CSV数据的方法
    void LoadDailyData()
    {
        // 构造文件的完整路径
        // Application.dataPath 在编辑器中指向 Assets 文件夹
        string filePath = Path.Combine(Application.dataPath, "Data", "DailyData.csv");

        // 检查文件是否存在
        if (!File.Exists(filePath))
        {
            Debug.LogError("在 Assets/Data/ 文件夹中找不到 DailyData.csv 文件! 路径: " + filePath);
            return;
        }

        try
        {
            // 读取文件的所有行
            string[] lines = File.ReadAllLines(filePath);

            // 从第二行开始读取 (跳过表头)
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] values = line.Split(',');

                // 确保数据格式正确，至少有两列
                if (values.Length < 2)
                {
                    Debug.LogWarning($"第 {i + 1} 行数据格式不正确，已跳过: {line}");
                    continue;
                }

                int day;
                // 尝试解析天数，如果失败则跳过此行
                if (!int.TryParse(values[0], out day))
                {
                    Debug.LogWarning($"无法解析第 {i + 1} 行的天数，已跳过: {line}");
                    continue;
                }

                string message = values[1];

                // 如果信息中包含双引号（通常是为了包含逗号），需要处理一下
                if (message.StartsWith("\"") && message.EndsWith("\""))
                {
                    message = message.Substring(1, message.Length - 2);
                }

                // 存入字典
                if (!dailyMessages.ContainsKey(day))
                {
                    dailyMessages.Add(day, message);
                }
                else
                {
                    Debug.LogWarning($"CSV文件中存在重复的天数: {day}，后出现的数据将覆盖前者。");
                    dailyMessages[day] = message;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("读取或解析CSV文件时发生错误: " + e.Message);
        }
    }

    // 开始新的一天
    void StartNewDay()
    {
        // 检查天数是否超过总天数
        if (currentDay > totalDays)
        {
            // 游戏结束逻辑：直接加载结束场景
            Debug.Log("游戏结束，跳转到结束场景。");
            SceneManager.LoadScene("EndScene"); // <-- 修改点在这里
            return; // 确保后续代码不执行
        }

        // 如果游戏未结束，则正常开始新的一天
        actionPoints = MAX_ACTION_POINTS; // 重置行动点
        UpdateUI(); // 更新界面显示
    }

    // 更新所有UI文本
    void UpdateUI()
    {
        if (dayText == null || apText == null || messageText == null)
        {
            Debug.LogError("UI引用未设置或未找到！");
            return;
        }

        dayText.text = $"第 {currentDay} 天";
        apText.text = $"行动点: {actionPoints} / {MAX_ACTION_POINTS}";

        // 从字典中获取当天的信息
        if (dailyMessages.ContainsKey(currentDay))
        {
            messageText.text = dailyMessages[currentDay];
        }
        else
        {
            messageText.text = "今天没有特殊信息。";
        }
    }

    // 按钮被点击时调用的公共方法
    public void OnActionButtonClicked()
    {
        if (actionPoints > 0)
        {
            actionPoints--; // 消耗一个行动点
            UpdateUI(); // 更新行动点显示

            // 可以在这里添加不同按钮的独特逻辑，例如增加属性等
            Debug.Log("一个行动被执行了，剩余行动点: " + actionPoints);

            if (actionPoints <= 0)
            {
                // 行动点耗尽，结束这一天
                EndDay();
            }
        }
    }

    // 结束一天，准备转场
    void EndDay()
    {
        Debug.Log($"第 {currentDay} 天结束。");
        currentDay++; // 天数增加
        // 加载转场场景
        SceneManager.LoadScene("TransitionScene");
    }

    // 辅助方法：设置所有按钮是否可交互
    void SetAllButtonsInteractable(bool isInteractable)
    {
        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button btn in buttons)
        {
            btn.interactable = isInteractable;
        }
    }
}