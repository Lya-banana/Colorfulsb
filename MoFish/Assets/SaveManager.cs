using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // �����ļ���д�����ռ�

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    // ����Ϸ����ʱ��isLoading �ᱻ��Ϊ true
    // GameManager ��������״̬�������ǿ�ʼ��һ�컹��Ӧ�ö�ȡ������
    public bool isLoading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ��ȡ�浵�ļ�������·��
    /// </summary>
    /// <param name="slotIndex">�浵��λ����</param>
    /// <returns>�ļ�·��</returns>
    private string GetSavePath(int slotIndex)
    {
        // Application.persistentDataPath ��Unity�Ƽ��ġ���ƽ̨��ȫ�Ķ�д·��
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slotIndex}.json");
    }

    /// <summary>
    /// ������Ϸ
    /// </summary>
    /// <param name="slotIndex">Ҫ���浽�Ĳ�λ</param>
    public void SaveGame(int slotIndex)
    {
        // 1. ��GameManager��ȡ��ǰ����Ϸ����
        GameData dataToSave = GameManager.Instance.GetGameDataForSave();

        // 2. ������ת����JSON�ַ���
        string jsonData = JsonUtility.ToJson(dataToSave, true); // true ��ʾ��ʽ��������������

        // 3. ��JSON�ַ���д���ļ�
        File.WriteAllText(GetSavePath(slotIndex), jsonData);

        Debug.Log($"��Ϸ�ѱ��浽��λ {slotIndex}");
    }

    /// <summary>
    /// ��ȡ��Ϸ
    /// </summary>
    /// <param name="slotIndex">Ҫ��ȡ�Ĳ�λ</param>
    public void LoadGame(int slotIndex)
    {
        string path = GetSavePath(slotIndex);
        if (!File.Exists(path))
        {
            Debug.LogError($"δ�ҵ��浵�ļ�: {path}");
            return;
        }

        // 1. ���ļ���ȡJSON�ַ���
        string jsonData = File.ReadAllText(path);

        // 2. ��JSON�ַ���ת����GameData����
        GameData loadedData = JsonUtility.FromJson<GameData>(jsonData);

        // 3. ����ȡ��������Ӧ�õ�GameManager
        GameManager.Instance.ApplyGameDataFromLoad(loadedData);

        // 4. ���ü��ر�־λ�������¼���������
        isLoading = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");

        Debug.Log($"�Ӳ�λ {slotIndex} ��ȡ��Ϸ");
    }

    /// <summary>
    /// ��ȡָ����λ�Ĵ浵��Ϣ������UI��ʾ������ʵ�ʼ�����Ϸ
    /// </summary>
    /// <returns>����浵���ڣ�����GameData�����򷵻�null</returns>
    public GameData GetSaveInfo(int slotIndex)
    {
        string path = GetSavePath(slotIndex);
        if (!File.Exists(path))
        {
            return null;
        }

        string jsonData = File.ReadAllText(path);
        return JsonUtility.FromJson<GameData>(jsonData);
    }

    // �� SaveManager.cs ���������·���

    public void DeleteAllSaves()
    {
        // ע�⣺���ֵ��Ҫ��SaveLoadUI�е�numberOfSlots����һ��
        int numberOfSlots = 6;
        for (int i = 0; i < numberOfSlots; i++)
        {
            string path = GetSavePath(i);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        Debug.Log("���д浵�ѱ�ɾ����");
    }
}