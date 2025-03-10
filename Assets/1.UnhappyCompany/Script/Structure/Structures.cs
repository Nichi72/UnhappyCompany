using System.Collections;
using UnityEngine;

public static class Structures
{
    public const string LAB_WATCHER = "Lab Watcher";
    public const string LAB_WATCHER_TOOL = "Lab Watcher/Placement Tool";
}

public enum ETag
{
    RaycastHit,
    Item
}

public enum EItem
{
    CCTV
}

public enum EAIState
{
    Idle,
    Patrol,
    Chase,
    Attack
}
public enum EGameState
{
    None,
    Ready,
    Playing,
    End
}

public enum EObjectTrackerUIType
{
    Enemy,
    Egg,
    CollectibleItem
}

public enum ENotificationCategory
{
    Normal,
    Warning,
    Error,
    Danger
}
public enum EnemyType
{
    Machine,    // 기계형 (물 속성)
    Human,      // 인간형 (불 속성)
    Animal      // 동물형 (물리 속성)
}

public enum RSPChoice { Rock, Scissors, Paper }
public enum RSPResult { Win, Lose, Draw }