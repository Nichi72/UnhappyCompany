using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Structures", menuName = "Scriptable Objects/Structures")]
public class Structures
{
    
}

public enum Tag
{
    RaycastHit,
    Item
}

public enum EItem
{
    CCTV
}

public enum AIState
{
    Idle,
    Patrol,
    Chase,
    Attack
}

public static class CoroutineHelper
{
    public static IEnumerator WaitForSecondsInFixedUpdate(float waitTime)
    {
        float elapsedTime = 0f;
        while (elapsedTime < waitTime)
        {
            yield return new WaitForFixedUpdate(); // FixedUpdate 주기 대기
            elapsedTime += Time.fixedDeltaTime;   // 고정 시간 간격 누적
        }
    }
}