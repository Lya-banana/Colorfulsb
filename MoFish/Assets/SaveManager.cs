using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // 引入文件读写命名空间

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    // 当游戏加载时，isLoading 会被设为 true
    // GameManager 会根据这个状态来决定是开始新一天还是应用读取的数据
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
    /// 获取存档文件的完整路径
    /// </summary>
    /// <param name="slotIndex">存档槽位索引</param>
    /// <returns>文件路径</returns>
    private string GetSavePath(int slotIndex)
    {
        // Application.persistentDataPath 是Unity推荐的、跨平台安全的读写路径
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slotIndex}.json");
    }

    /// <summary>
    /// 保存游戏
    /// </summary>
    /// <param name="slotIndex">要保存到的槽位</param>
    public void SaveGame(int slotIndex)
    {
        // 1. 从GameManager获取当前的游戏数据
        GameData dataToSave = GameManager.Instance.GetGameDataForSave();

        // 2. 将数据转换成JSON字符串
        string jsonData = JsonUtility.ToJson(dataToSave, true); // true 表示格式化输出，方便调试

        // 3. 将JSON字符串写入文件
        File.WriteAllText(GetSavePath(slotIndex), jsonData);

        Debug.Log($"游戏已保存到槽位 {slotIndex}");
    }

    /// <summary>
    /// 读取游戏
    /// </summary>
    /// <param name="slotIndex">要读取的槽位</param>
    public void LoadGame(int slotIndex)
    {
        string path = GetSavePath(slotIndex);
        if (!File.Exists(path))
        {
            Debug.LogError($"未找到存档文件: {path}");
            return;
        }

        // 1. 从文件读取JSON字符串
        string jsonData = File.ReadAllText(path);

        // 2. 将JSON字符串转换回GameData对象
        GameData loadedData = JsonUtility.FromJson<GameData>(jsonData);

        // 3. 将读取到的数据应用到GameManager
        GameManager.Instance.ApplyGameDataFromLoad(loadedData);

        // 4. 设置加载标志位，并重新加载主场景
        isLoading = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");

        Debug.Log($"从槽位 {slotIndex} 读取游戏");
    }

    /// <summary>
    /// 获取指定槽位的存档信息（用于UI显示），不实际加载游戏
    /// </summary>
    /// <returns>如果存档存在，返回GameData；否则返回null</returns>
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

    // 在 SaveManager.cs 中添加这个新方法

    public void DeleteAllSaves()
    {
        // 注意：这个值需要和SaveLoadUI中的numberOfSlots保持一致
        int numberOfSlots = 6;
        for (int i = 0; i < numberOfSlots; i++)
        {
            string path = GetSavePath(i);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        Debug.Log("所有存档已被删除！");
    }
}