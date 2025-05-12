using UnityEngine;

namespace UnhappyCompany
{
    /// <summary>
    /// 슬롯머신 애니메이션에 사용되는 이징 함수들을 제공하는 유틸리티 클래스
    /// </summary>
    public static class SlotMachineEasingUtils
    {
        /// <summary>
        /// 선형 보간 (일정한 속도)
        /// </summary>
        /// <param name="t">시간 값 (0~1)</param>
        /// <returns>보간된 값 (0~1)</returns>
        public static float Linear(float t)
        {
            return t;
        }

        /// <summary>
        /// 2차 함수 방식의 끝 부분 감속 (EaseOut)
        /// </summary>
        /// <param name="t">시간 값 (0~1)</param>
        /// <returns>보간된 값 (0~1)</returns>
        public static float EaseOutQuad(float t)
        {
            return 1 - (1 - t) * (1 - t);
        }

        /// <summary>
        /// 3차 함수 방식의 끝 부분 감속 (EaseOut)
        /// </summary>
        /// <param name="t">시간 값 (0~1)</param>
        /// <returns>보간된 값 (0~1)</returns>
        public static float EaseOutCubic(float t)
        {
            return 1 - Mathf.Pow(1 - t, 3);
        }

        /// <summary>
        /// 4차 함수 방식의 끝 부분 감속 (EaseOut)
        /// </summary>
        /// <param name="t">시간 값 (0~1)</param>
        /// <returns>보간된 값 (0~1)</returns>
        public static float EaseOutQuart(float t)
        {
            return 1 - Mathf.Pow(1 - t, 4);
        }

        /// <summary>
        /// 지수 함수 방식의 끝 부분 감속 (EaseOut)
        /// </summary>
        /// <param name="t">시간 값 (0~1)</param>
        /// <returns>보간된 값 (0~1)</returns>
        public static float EaseOutExpo(float t)
        {
            return t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
        }

        /// <summary>
        /// 사인 곡선 방식의 끝 부분 감속 (EaseOut)
        /// </summary>
        /// <param name="t">시간 값 (0~1)</param>
        /// <returns>보간된 값 (0~1)</returns>
        public static float EaseOutSine(float t)
        {
            return Mathf.Sin(t * Mathf.PI / 2);
        }

        /// <summary>
        /// 탄성 곡선 방식의 끝 부분 감속 (EaseOut)
        /// </summary>
        /// <param name="t">시간 값 (0~1)</param>
        /// <returns>보간된 값 (0~1)</returns>
        public static float EaseOutElastic(float t)
        {
            float c4 = (2 * Mathf.PI) / 3;
            
            if (t == 0) return 0;
            if (t == 1) return 1;
            
            return Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * c4) + 1;
        }

        /// <summary>
        /// 바운스 방식의 끝 부분 감속 (EaseOut)
        /// </summary>
        /// <param name="t">시간 값 (0~1)</param>
        /// <returns>보간된 값 (0~1)</returns>
        public static float EaseOutBounce(float t)
        {
            float n1 = 7.5625f;
            float d1 = 2.75f;
            
            if (t < 1 / d1)
            {
                return n1 * t * t;
            }
            else if (t < 2 / d1)
            {
                t -= 1.5f / d1;
                return n1 * t * t + 0.75f;
            }
            else if (t < 2.5 / d1)
            {
                t -= 2.25f / d1;
                return n1 * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / d1;
                return n1 * t * t + 0.984375f;
            }
        }

        /// <summary>
        /// 슬롯머신에 최적화된 감속 (초반에 빠르게 감속 후 마지막에 천천히 멈춤)
        /// </summary>
        /// <param name="t">시간 값 (0~1)</param>
        /// <returns>보간된 값 (0~1)</returns>
        public static float SlotMachineEaseOut(float t)
        {
            // 초반에는 빠르게 감속하다가 마지막에 천천히 멈추는 복합 효과
            float elastic = EaseOutElastic(t);
            float expo = EaseOutExpo(t);
            float cubic = EaseOutCubic(t);
            
            // 시간에 따라 다른 감속 효과 혼합
            if (t < 0.3f)
            {
                // 초반 30%는 지수 감속 (빠르게 감속)
                return expo;
            }
            else if (t < 0.85f)
            {
                // 중간 55%는 지수 감속과 3차 감속 혼합
                float blend = (t - 0.3f) / 0.55f; // 0에서 1로 재정규화
                return Mathf.Lerp(expo, cubic, blend);
            }
            else
            {
                // 마지막 15%는 3차 감속과 탄성 감속 혼합 (부드럽게 멈춤)
                float blend = (t - 0.85f) / 0.15f; // 0에서 1로 재정규화
                return Mathf.Lerp(cubic, elastic * 0.3f + cubic * 0.7f, blend);
            }
        }
    }
} 