using System.Collections;
using UnityEngine;

public static class Structures
{
    public const string LAB_WATCHER = "Lab Watcher";
    public const string LAB_WATCHER_TOOL = "Lab Watcher/Placement Tool";
}

public enum ETag
{
    Untagged,
    RaycastHit,
    Item,
    Wall,
    Player,
    Pushable,
    Enemy,
}
public enum DoorTrapBtnType
{
        Fire,
        Water,
        Hammer,
        Electric
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

public enum EnemyBudgetFlag
{
    // 생성이 되었음.
    Created,
    // Budget에 추가되었음.
    AddedToBudget,
    // Budget에서 차감되었음.
    SubtractedFromBudget,
}

    public enum DoorDirection
    {
        North,  // 북쪽(+Z)
        South,  // 남쪽(-Z)
        East,   // 동쪽(+X)
        West,   // 서쪽(-X)
        Up,     // 위쪽(+Y)
        Down    // 아래쪽(-Y)
    }
    

// public enum RSPChoice { Rock, Scissors, Paper }
// public enum RSPResult { Win, Lose, Draw }
public enum RSPChoice
{
    Rock,
    Scissors,
    Paper
}

public enum RSPResult
{
    Win,
    Lose,
    Draw
}