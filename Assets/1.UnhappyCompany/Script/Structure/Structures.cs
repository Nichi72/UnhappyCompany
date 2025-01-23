using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Structures", menuName = "Scriptable Objects/Structures")]
public class Structures
{
    
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
public static class CoroutineHelper
{
    public static IEnumerator WaitForSecondsInFixedUpdate(float waitTime)
    {
        float elapsedTime = 0f;
        while (elapsedTime < waitTime)
        {
            yield return new WaitForFixedUpdate(); // FixedUpdate �ֱ� ���
            elapsedTime += Time.fixedDeltaTime;   // ���� �ð� ���� ����
        }
    }
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