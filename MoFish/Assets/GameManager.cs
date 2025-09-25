using UnityEngine;
using UnityEngine.UI; // ����UI�����ռ�
using TMPro; // ����TextMeshPro�����ռ�
using UnityEngine.SceneManagement; // ���볡�����������ռ�
using System.Collections.Generic; // ���뼯�������ռ�
using System.IO; // �����ļ���д�����ռ�

public class GameManager : MonoBehaviour
{
    // ========= ����ģʽ =========
    // ȷ��������Ϸ��ֻ��һ��GameManagerʵ��
    public static GameManager Instance;

    // ========= ��Ϸ״̬���� =========
    public int currentDay = 1; // ��ǰ����
    public int totalDays = 30; // ������
    public int actionPoints; // ��ǰ�ж���
    private const int MAX_ACTION_POINTS = 5; // ÿ������ж���

    // ========= UI���� =========
    // ��Unity�༭���У���Ҫ�ѳ������UI�ϵ�����
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI apText;
    public TextMeshProUGUI messageText;

    // ========= ���ݴ洢 =========
    // ʹ��һ���ֵ����洢ÿ�����Ϣ��Key������(int)��Value����Ϣ(string)
    private Dictionary<int, string> dailyMessages = new Dictionary<int, string>();

    void Awake()
    {
        // ����ģʽ��ʵ��
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �л�����ʱ�����ٴ˶���
            LoadDailyData(); // ����Ϸ��ʼʱ�ͼ�������
        }
        else
        {
            Destroy(gameObject); // ����Ѵ���ʵ�����������µ����
        }
    }

    void Start()
    {
        // ��Ϸ��һ������ʱ����
        // ����Ǵ�ת������������OnSceneLoaded�ᴦ���ʼ��
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            StartNewDay();
        }
    }

    // ��һ������������ʱ����������ᱻ����
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ����������ɺ�Ļص�����
    // ����������ɺ�Ļص�����
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ����Ƿ�ص���������
        if (scene.name == "MainScene")
        {
            Debug.Log("�ѷ������������������³�ʼ��UI...");

            // 1. ���»�ȡUI�ı�Ԫ�ص�����
            dayText = GameObject.Find("DayText").GetComponent<TextMeshProUGUI>();
            apText = GameObject.Find("APText").GetComponent<TextMeshProUGUI>();
            messageText = GameObject.Find("MessageText").GetComponent<TextMeshProUGUI>();

            // 2. ���������롿���²��Ҳ��������ж���ť�ĵ���¼�
            // �����ҵ����д� "ActionButton" ��ǩ����Ϸ����
            GameObject[] actionButtons = GameObject.FindGameObjectsWithTag("ActionButton");

            Debug.Log($"�ҵ��� {actionButtons.Length} ���ж���ť��");

            // ���������ҵ��İ�ť
            foreach (GameObject buttonObj in actionButtons)
            {
                // ��ȡ�ö����ϵ�Button���
                Button button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    // ���Ƴ����оɵļ�������ֹ�ظ����
                    button.onClick.RemoveAllListeners();
                    // ������µļ����������������ǵĺ���
                    button.onClick.AddListener(OnActionButtonClicked);
                }
            }

            // 3. ��ʼ�µ�һ��
            StartNewDay();
        }
    }


    // ��ȡCSV���ݵķ���
    void LoadDailyData()
    {
        // �����ļ�������·��
        // Application.dataPath �ڱ༭����ָ�� Assets �ļ���
        string filePath = Path.Combine(Application.dataPath, "Data", "DailyData.csv");

        // ����ļ��Ƿ����
        if (!File.Exists(filePath))
        {
            Debug.LogError("�� Assets/Data/ �ļ������Ҳ��� DailyData.csv �ļ�! ·��: " + filePath);
            return;
        }

        try
        {
            // ��ȡ�ļ���������
            string[] lines = File.ReadAllLines(filePath);

            // �ӵڶ��п�ʼ��ȡ (������ͷ)
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] values = line.Split(',');

                // ȷ�����ݸ�ʽ��ȷ������������
                if (values.Length < 2)
                {
                    Debug.LogWarning($"�� {i + 1} �����ݸ�ʽ����ȷ��������: {line}");
                    continue;
                }

                int day;
                // ���Խ������������ʧ������������
                if (!int.TryParse(values[0], out day))
                {
                    Debug.LogWarning($"�޷������� {i + 1} �е�������������: {line}");
                    continue;
                }

                string message = values[1];

                // �����Ϣ�а���˫���ţ�ͨ����Ϊ�˰������ţ�����Ҫ����һ��
                if (message.StartsWith("\"") && message.EndsWith("\""))
                {
                    message = message.Substring(1, message.Length - 2);
                }

                // �����ֵ�
                if (!dailyMessages.ContainsKey(day))
                {
                    dailyMessages.Add(day, message);
                }
                else
                {
                    Debug.LogWarning($"CSV�ļ��д����ظ�������: {day}������ֵ����ݽ�����ǰ�ߡ�");
                    dailyMessages[day] = message;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("��ȡ�����CSV�ļ�ʱ��������: " + e.Message);
        }
    }

    // ��ʼ�µ�һ��
    void StartNewDay()
    {
        // ��������Ƿ񳬹�������
        if (currentDay > totalDays)
        {
            // ��Ϸ�����߼���ֱ�Ӽ��ؽ�������
            Debug.Log("��Ϸ��������ת������������");
            SceneManager.LoadScene("EndScene"); // <-- �޸ĵ�������
            return; // ȷ���������벻ִ��
        }

        // �����Ϸδ��������������ʼ�µ�һ��
        actionPoints = MAX_ACTION_POINTS; // �����ж���
        UpdateUI(); // ���½�����ʾ
    }

    // ��������UI�ı�
    void UpdateUI()
    {
        if (dayText == null || apText == null || messageText == null)
        {
            Debug.LogError("UI����δ���û�δ�ҵ���");
            return;
        }

        dayText.text = $"�� {currentDay} ��";
        apText.text = $"�ж���: {actionPoints} / {MAX_ACTION_POINTS}";

        // ���ֵ��л�ȡ�������Ϣ
        if (dailyMessages.ContainsKey(currentDay))
        {
            messageText.text = dailyMessages[currentDay];
        }
        else
        {
            messageText.text = "����û��������Ϣ��";
        }
    }

    // ��ť�����ʱ���õĹ�������
    public void OnActionButtonClicked()
    {
        if (actionPoints > 0)
        {
            actionPoints--; // ����һ���ж���
            UpdateUI(); // �����ж�����ʾ

            // ������������Ӳ�ͬ��ť�Ķ����߼��������������Ե�
            Debug.Log("һ���ж���ִ���ˣ�ʣ���ж���: " + actionPoints);

            if (actionPoints <= 0)
            {
                // �ж���ľ���������һ��
                EndDay();
            }
        }
    }

    // ����һ�죬׼��ת��
    void EndDay()
    {
        Debug.Log($"�� {currentDay} �������");
        currentDay++; // ��������
        // ����ת������
        SceneManager.LoadScene("TransitionScene");
    }

    // �����������������а�ť�Ƿ�ɽ���
    void SetAllButtonsInteractable(bool isInteractable)
    {
        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button btn in buttons)
        {
            btn.interactable = isInteractable;
        }
    }
}