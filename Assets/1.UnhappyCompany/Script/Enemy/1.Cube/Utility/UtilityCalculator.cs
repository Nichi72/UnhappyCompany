public class UtilityCalculator
{
    /// <summary>
    /// 순찰 상태의 유틸리티 값을 계산합니다.
    /// </summary>
    /// <param name="timeOfDay">현재 시간대</param>
    /// <returns>순찰 유틸리티 값</returns>
    public float CalculatePatrolUtility(TimeOfDay timeOfDay)
    {
        if (timeOfDay == TimeOfDay.Morning)
        {
            return 0.6f; // 오전 유틸리티 값
        }
        else
        {
            return 0.4f; // 오후 유틸리티 값
        }
    }

    /// <summary>
    /// 추적 상태의 유틸리티 값을 계산합니다.
    /// </summary>
    /// <param name="timeOfDay">현재 시간대</param>
    /// <returns>추적 유틸리티 값</returns>
    public float CalculateChaseUtility(TimeOfDay timeOfDay)
    {
        if (timeOfDay == TimeOfDay.Morning)
        {
            return 0.8f; // 오전 유틸리티 값
        }
        else
        {
            return 0.5f; // 오후 유틸리티 값
        }
    }

    /// <summary>
    /// 공격 상태의 유틸리티 값을 계산합니다.
    /// </summary>
    /// <returns>공격 유틸리티 값</returns>
    public float CalculateAttackUtility()
    {
        return 0.9f; // 공격 유틸리티 값
    }
} 