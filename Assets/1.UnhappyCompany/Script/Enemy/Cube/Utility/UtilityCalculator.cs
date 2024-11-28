public class UtilityCalculator
{
    /// <summary>
    /// 순찰 상태의 유틸리티 값을 계산합니다.
    /// </summary>
    /// <returns>순찰 유틸리티 값</returns>
    public float CalculatePatrolUtility()
    {
        // 순찰 유틸리티 계산 로직
        return 0.5f;
    }

    /// <summary>
    /// 추적 상태의 유틸리티 값을 계산합니다.
    /// </summary>
    /// <returns>추적 유틸리티 값</returns>
    public float CalculateChaseUtility()
    {
        // 추적 유틸리티 계산 로직
        return 0.7f;
    }

    /// <summary>
    /// 공격 상태의 유틸리티 값을 계산합니다.
    /// </summary>
    /// <returns>공격 유틸리티 값</returns>
    public float CalculateAttackUtility()
    {
        // 공격 유틸리티 계산 로직
        return 0.9f;
    }
} 