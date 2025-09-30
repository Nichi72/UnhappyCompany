using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveSchedule", menuName = "Scriptable Objects/WaveSchedule")]
public class WaveSchedule : ScriptableObject
{
    public List<WaveDB> waveDBs = new List<WaveDB>();
}

[System.Serializable]
public class WaveDB
{
    
    // 
    public TimeSpan time;
    public List<EnemyAIController<BaseEnemyAIData>> enemies;
    
}
