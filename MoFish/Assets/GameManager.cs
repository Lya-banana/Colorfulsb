using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class GameManager : MonoBehaviour
{
    // ========= ����ģʽ =========
    public static GameManager Instance;

    // ========= ��Ϸ״̬���� =========
    public int currentDay = 1;
    public int totalDays = 30;
    public int actionPoints;
    private const int MAX_ACTION_POINTS = 5;

    // ========= ������� (ȫ�ֱ���) =========
    public int hp = 100;
    public int sanity = 100; // ����ֵ
    public int gold = 50;

    // ========= UI���� =========
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI apText;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI sanityText;
    public TextMeshProUGUI goldText;

    // ========= ���ݴ洢 =========
    private Dictionary<int, string> dailyMessages = new Dictionary<int, string>();
    // --- �޸ĵ� 1: �� endOfDaySummary ��Ϊ public ---
    public string endOfDaySummary = ""; // ���ڴ洢ÿ���ܽ�

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
            // �״�����ʱ���ҵ�����UI����ʼ��һ��
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

    // ����������ɺ�Ļص�����
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainScene")
        {
            // ÿ�η���������������Ҫ���²���UIԪ�غ����°󶨰�ť
            FindAllUIElements();
            BindButtons();
            StartNewDay();
        }
    }

    // ���в�������UIԪ��
    void FindAllUIElements()
    {
        dayText = GameObject.Find("DayText")?.GetComponent<TextMeshProUGUI>();
        apText = GameObject.Find("APText")?.GetComponent<TextMeshProUGUI>();
        messageText = GameObject.Find("MessageText")?.GetComponent<TextMeshProUGUI>();
        hpText = GameObject.Find("HPText")?.GetComponent<TextMeshProUGUI>();
        sanityText = GameObject.Find("SanityText")?.GetComponent<TextMeshProUGUI>();
        goldText = GameObject.Find("GoldText")?.GetComponent<TextMeshProUGUI>();
    }

    // ���а󶨰�ť�¼�
    void BindButtons()
    {
        // ͨ����ť���ֲ��Ҳ��󶨶�Ӧ�ķ���
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
            Debug.LogError("�� Assets/Data/ �ļ������Ҳ��� DailyData.csv �ļ�! ·��: " + filePath);
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
        catch (System.Exception e) { Debug.LogError("��ȡ�����CSV�ļ�ʱ��������: " + e.Message); }
    }

    // --- �޸ĵ� 2: �� StartNewDay ���� ---
    void StartNewDay()
    {
        if (currentDay > totalDays)
        {
            SceneManager.LoadScene("EndScene");
            return;
        }

        actionPoints = MAX_ACTION_POINTS;
        UpdateUI();

        // ��ȡ�������Ϣ��ֱ����ʾ
        string dailyMessage = dailyMessages.ContainsKey(currentDay) ? dailyMessages[currentDay] : "��������ƽ����һ�졣";
        if (messageText != null)
        {
            messageText.text = dailyMessage;
        }

        // �����һ����ܽᣬ��Ϊ���Ѿ��ڹ��ɳ�������ʾ����
        endOfDaySummary = "";
    }


    // ��������UI�ı�
    void UpdateUI()
    {
        if (dayText == null || apText == null || messageText == null || hpText == null || sanityText == null || goldText == null)
        {
            Debug.LogError("һ������UI����δ���û�δ�ҵ���");
            return;
        }
        dayText.text = $"�� {currentDay} ��";
        apText.text = $"�ж���: {actionPoints} / {MAX_ACTION_POINTS}";
        hpText.text = $"HP: {hp}";
        sanityText.text = $"����: {sanity}";
        goldText.text = $"���: {gold}";
    }

    // ͳһ�����ж�������
    private void ConsumeActionPoint()
    {
        if (actionPoints > 0)
        {
            actionPoints--;
            UpdateUI(); // ÿ�����Ķ�����UI
            if (actionPoints <= 0)
            {
                EndDay();
            }
        }
    }

    // ========= ��ť��Ӧ�ľ��巽�� =========
    public void OnTrainButtonClicked()
    {
        if (actionPoints <= 0) return;
        hp -= 5;
        sanity -= 7;
        Debug.Log("ִ��ѵ����HP-5, ����-7");
        ConsumeActionPoint();
    }

    public void OnRestButtonClicked()
    {
        if (actionPoints <= 0) return;
        hp += 3;
        sanity += 2;
        Debug.Log("ִ����Ϣ��HP+3, ����+2");
        ConsumeActionPoint();
    }

    public void OnStudyButtonClicked()
    {
        if (actionPoints <= 0) return;
        hp -= 9;
        sanity -= 4;
        Debug.Log("ִ��ѧϰ��HP-9, ����-4");
        ConsumeActionPoint();
    }

    // ����һ�죬���н���
    void EndDay()
    {
        Debug.Log($"�� {currentDay} �������");

        // 1. ��¼�仯ǰ��ֵ
        int oldHp = hp;
        int oldSanity = sanity;
        int oldGold = gold;

        // 2. ����ÿ�����������ָ�
        gold += 20;
        int hpRecovery = Random.Range(0, 6); // ����ָ�0��5��
        int sanityRecovery = Random.Range(0, 6); // ����ָ�0��5��
        hp += hpRecovery;
        sanity += sanityRecovery;

        // 3. �����ܽ��ı���Ϊ��һ����׼��
        endOfDaySummary = $"<color=yellow>���ս������㣺</color>\n" +
                          $"HP: {oldHp} -> {hp} (�ָ���{hpRecovery})\n" +
                          $"����: {oldSanity} -> {sanity} (�ָ���{sanityRecovery})\n" +
                          $"���: {oldGold} -> {gold} (�����20)";

        // 4. �������Ӳ�����ת������
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

