using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RegionManager : MonoBehaviour
{
    public static RegionManager instance;

    // 공격 할 확률
    public float attackProbability = 0.5f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }


    void Update()
    {
        
    }
    //  시간이 되면 Budget을 차감해서 그만큼 적이 행동하게 됨.
    public void TryScheduleEnemy()
    {
        
    }

    // 기본 스케줄 초기화
    public void InitDefaultSchedule()
    {
        Debug.Log(" EnemyManager Schedule 초기화 시작");
        // 기본 스케줄 초기화
        TimeSpan startTime = TimeManager.instance.SunriseTime;
        TimeSpan endTime = TimeManager.instance.SunsetTime;

        // 랜덤 간격의 최소 및 최대값 설정
        int minMinutes = 60; // 최소 1시간
        int maxMinutes = 120; // 최대 2시간

        List<TimeSpan> times = new List<TimeSpan>();
        TimeSpan currentTime = startTime;
        // 최소 1시간 최대 2시간 랜덤 간격으로 스케줄 추가
        while (currentTime < endTime)
        {
            times.Add(currentTime);
            Debug.Log(" 스케줄 추가 : " + currentTime);
            TimeSpan randomInterval = new TimeSpan(0, UnityEngine.Random.Range(minMinutes, maxMinutes), 0);
            currentTime = currentTime.Add(randomInterval);
        }

        foreach (TimeSpan time in times)
        {
            AddScheduleEnemy(time);
        }
    }
    public void AddScheduleEnemy(TimeSpan time)
    {
        TimeManager.instance.AddEnemyScheduleEvent(time, () => 
        {
            Debug.Log($"{time}에 적이 행동하게 됨.");
            Wave();
        });
    }

    public void Wave(List<EnemyAIController<BaseEnemyAIData>> specialCreatures = null)
    {
        Debug.Log(" Wave 시작");
        int budget = 0;
        ///
        List<GameObject> spawnedCreatures = EnemyManager.instance.activeEnemies;
        List<EnemyAIController> selectedCreatures = new List<EnemyAIController>();

        // CalculateCurrentBudget를 계산하면서 버짓에 영향을 준 크리처에 Flag를 설정한다.
        // 생성된 크리처 리스트를 순회하며 예산을 계산한다.
        foreach (GameObject enemy in spawnedCreatures)
        {
            EnemyAIController enemyController = enemy.GetComponent<EnemyAIController>();
            // 크리처가 새로 생성된 경우
            if(enemyController.budgetFlag == EnemyBudgetFlag.Created)
            {
                // 예산에 추가된 상태로 플래그를 변경하고 예산에 크리처의 비용을 더한다.
                selectedCreatures.Add(enemyController);
                budget += enemyController.EnemyData.Cost;
                Debug.Log($" EnemyBudgetFlag.Created :{enemy.name} 이 Budget&selectedCreatures에 추가되었습니다.");

                enemyController.budgetFlag = EnemyBudgetFlag.AddedToBudget;

            }
            // 크리처가 이미 예산에 추가된 경우
            else if(enemyController.budgetFlag == EnemyBudgetFlag.AddedToBudget)
            {
                // 예산에 크리처의 비용을 더한다.
                selectedCreatures.Add(enemyController);
                budget += enemyController.EnemyData.Cost;
                Debug.Log($" EnemyBudgetFlag.AddedToBudget :{enemy.name} 이 Budget&selectedCreatures에 추가되었습니다.");

            }
            else if (enemyController.budgetFlag == EnemyBudgetFlag.SubtractedFromBudget)
            {
                Debug.Log($" EnemyBudgetFlag.SubtractedFromBudget :{enemy.name} 예산에서 빠진 크리처 입니다.");
            }
        }

        int enemyScheduleEventCount = TimeManager.instance.GetEnemyScheduleEventCount();
        

        int remainingCost = budget / enemyScheduleEventCount;;
        Debug.Log($"remainingCost : {remainingCost} = waveCost currentBudget ({budget}) / enemyScheduleEventCount ({enemyScheduleEventCount})");
        // 특별 크리처가 있는 경우
        if (specialCreatures != null)
        {
            Debug.Log($" 특별 크리처를 할당 하여 센터 공격 활동이 등록 됩니다.");
            foreach (EnemyAIController enemy in specialCreatures)
            {
                if (remainingCost > 0)
                {
                    if (enemy.EnemyData.Cost <= remainingCost)
                    {
                        remainingCost -= enemy.EnemyData.Cost;
                        enemy.budgetFlag = EnemyBudgetFlag.SubtractedFromBudget;
                        if(Random.Range(0, 1f) < attackProbability)
                        {
                            enemy.AttackCenter();
                            Debug.Log($" 특별 크리처 : {enemy.name} / 남은 비용 : {remainingCost}");
                        }
                        else
                        {
                            Debug.Log($"{enemy.name}은 공격 확률에 의해 공격하지 않습니다.");
                        }
                    }
                }
            }
        }
        else
        {
            Debug.Log($"특별 지정 크리쳐가 없습니다.");
        }

        Debug.Log($" 랜덤 크리쳐가 센터를 공격합니다. 남은 비용 : {remainingCost}");
        
        while (remainingCost > 0)
        {
            // 현재 활동중인 적중
            // flag가 AddedToBudget이 아닌 경우에
            // 랜덤으로 생성된 적을 선택합니다.
            GameObject randomEnemy = spawnedCreatures[Random.Range(0, spawnedCreatures.Count)];
            EnemyAIController randomEnemyController = randomEnemy.GetComponent<EnemyAIController>();
            // 선택된 적이 예산에서 빠지지 않은 경우
            if(randomEnemyController.budgetFlag != EnemyBudgetFlag.SubtractedFromBudget)
            {
                // 예산에서 해당 적을 뺍니다.
                remainingCost -= randomEnemyController.EnemyData.Cost;
                randomEnemyController.budgetFlag = EnemyBudgetFlag.SubtractedFromBudget;
                randomEnemyController.AttackCenter();
                // 디버그 로그를 출력합니다.
                Debug.Log($" 적 생성 : {randomEnemy.name} / 남은 비용 : {remainingCost}");
            }
        }
    }
}
