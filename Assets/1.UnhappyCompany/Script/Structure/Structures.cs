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
public enum GameState
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