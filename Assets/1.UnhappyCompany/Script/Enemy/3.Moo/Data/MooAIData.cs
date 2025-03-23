using UnityEngine;

[CreateAssetMenu(fileName = "MooAIData", menuName = "UnhappyCompany/AI/Moo AI Data")]
public class MooAIData : BaseEnemyAIData
{
    [Header("Moo Settings")]
    public float slimeDuration = 5f;
    public GameObject slimePrefab;

    public float checkDistance = 10f;
} 