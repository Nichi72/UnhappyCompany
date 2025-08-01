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
    // 현재 몇일인지?
    // 현재 시간이 몇인지?
    // 맵이 얼마나 많이 있는지?
    // 알이 얼마나 있는지?
    // 현재 Enemy가 어떻게 존재하는지?
    // 1. 어떤 종류의 Enemy가 있는가?
    // 2. 어떤 SpawnPoint에서 존재하는가?
    // 3. SapwnPoint를 어떻게 수립할것인가? 골고루 분포되게 할것인가? 아님 모두 한곳에 모여있게 할것인가?
    // 4. 거리를 어떻게 조정할것인가?
    // 3. 어떤 
    // 
    public TimeSpan time;
    public List<EnemyAIController<BaseEnemyAIData>> enemies;
    
}
