using UnityEngine;

public class SaveLoadUI : MonoBehaviour
{
    // 定义面板的两种模式
    public enum PanelMode { Save, Load }
    private PanelMode currentMode;

    [SerializeField] private GameObject saveSlotPrefab;
    [SerializeField] private Transform slotsParent; // 现在应该指向ScrollView的Content对象
    [SerializeField] private int numberOfSlots = 6;

    private void Start()
    {
        gameObject.SetActive(false);

        for (int i = 0; i < numberOfSlots; i++)
        {
            GameObject slotGO = Instantiate(saveSlotPrefab, slotsParent);
            SaveSlot slot = slotGO.GetComponent<SaveSlot>();
            slot.slotIndex = i;
            int index = i;
            slot.button.onClick.AddListener(() => OnSlotClicked(index));
        }
    }

    /// <summary>
    /// 打开面板，并设置其模式
    /// </summary>
    public void OpenPanel(PanelMode mode)
    {
        currentMode = mode; // 记录当前模式
        gameObject.SetActive(true);
        RefreshUI();
    }

    // 【新增】为了方便在按钮的 Inspector 中调用，我们创建两个独立的公共方法
    public void OpenInSaveMode()
    {
        OpenPanel(PanelMode.Save);
    }

    public void OpenInLoadMode()
    {
        OpenPanel(PanelMode.Load);
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    private void RefreshUI()
    {
        foreach (Transform child in slotsParent)
        {
            SaveSlot slot = child.GetComponent<SaveSlot>();
            GameData data = SaveManager.Instance.GetSaveInfo(slot.slotIndex);

            if (data == null) // 存档为空
            {
                slot.slotInfoText.text = "空存档";
                slot.slotStatusText.text = (currentMode == PanelMode.Save) ? "点击以保存" : "";
            }
            else // 存档存在
            {
                slot.slotInfoText.text = $"第 {data.currentDay} 天 | 金币: {data.gold}";
                slot.slotStatusText.text = (currentMode == PanelMode.Save) ? $"<color=orange>点击以覆盖</color>\n{data.saveTime}" : data.saveTime;
            }
        }
    }

    private void OnSlotClicked(int slotIndex)
    {
        if (currentMode == PanelMode.Save)
        {
            // 存档模式下，无论槽位是否为空，都执行保存/覆盖
            SaveManager.Instance.SaveGame(slotIndex);
            RefreshUI(); // 保存后刷新UI
            // 可以在这里加一个提示或者自动关闭面板
            // ClosePanel();
        }
        else // Load模式
        {
            GameData data = SaveManager.Instance.GetSaveInfo(slotIndex);
            if (data != null)
            {
                // 只有在槽位有数据时才执行读取
                SaveManager.Instance.LoadGame(slotIndex);
            }
            else
            {
                // 空槽位，不执行任何操作
                Debug.Log($"槽位 {slotIndex} 为空，无法读取。");
            }
        }
    }

    public void OnDeleteAllClicked()
    {
        SaveManager.Instance.DeleteAllSaves();
        RefreshUI();
    }
}