using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [System.Serializable] 是一个重要的特性，它告诉Unity这个类可以被序列化（转换成JSON）
[System.Serializable]
public class GameData
{
    // 需要保存的所有数据
    public int currentDay;
    public int hp;
    public int sanity;
    public int gold;
    public int actionPoints;

    // 我们还可以保存一些额外信息，用于在存档槽位上显示
    public string saveTime; // 例如："2025/09/27 08:30"
}