using System.Collections;
using UnityEngine;

namespace UnhappyCompany
{
    /// <summary>
    /// 가위바위보 슬롯머신의 심볼을 관리하는 컴포넌트
    /// 기존 SlotMachineSymbol을 확장하여 가위바위보 게임에 특화된 효과를 추가합니다.
    /// </summary>
    public class RSPSlotMachineSymbol : SlotMachineSymbol
    {
        [Header("가위바위보 심볼 설정")]
        [Tooltip("가위바위보 심볼 타입")]
        public RSPSymbolType symbolType = RSPSymbolType.Rock;
        
        [Tooltip("승리 시 재생할 효과")]
        public GameObject winEffect;
        
        [Tooltip("패배 시 재생할 효과")]
        public GameObject loseEffect;
        
        // 가위바위보 심볼 타입 열거형
        public enum RSPSymbolType
        {
            Rock,
            Scissors,
            Paper
        }
        
        /// <summary>
        /// 승리 효과 재생
        /// </summary>
        public void PlayWinEffect()
        {
            // 하이라이트 효과 활성화
            enableHighlight = true;
            highlightColor = Color.green;
            highlightIntensity = 2.0f;
            
            // 특수 효과 활성화 (있는 경우)
            if (winEffect != null)
            {
                winEffect.SetActive(true);
                StartCoroutine(DeactivateEffectAfterDelay(winEffect, 2.0f));
            }
            
            // 심볼 애니메이션 (예: 확대 효과)
            StartCoroutine(ScalePulseEffect(1.2f, 0.5f, 3));
        }
        
        /// <summary>
        /// 패배 효과 재생
        /// </summary>
        public void PlayLoseEffect()
        {
            // 하이라이트 효과 활성화
            enableHighlight = true;
            highlightColor = Color.red;
            highlightIntensity = 1.5f;
            
            // 특수 효과 활성화 (있는 경우)
            if (loseEffect != null)
            {
                loseEffect.SetActive(true);
                StartCoroutine(DeactivateEffectAfterDelay(loseEffect, 1.5f));
            }
            
            // 심볼 애니메이션 (예: 흔들림 효과)
            StartCoroutine(ShakeEffect(0.1f, 0.5f));
        }
        
        /// <summary>
        /// 무승부 효과 재생
        /// </summary>
        public void PlayDrawEffect()
        {
            // 하이라이트 효과 활성화
            enableHighlight = true;
            highlightColor = Color.yellow;
            highlightIntensity = 1.2f;
            
            // 심볼 애니메이션 (예: 회전 효과)
            StartCoroutine(RotationEffect(360f, 1.0f));
        }
        
        /// <summary>
        /// 일정 시간 후 효과 비활성화
        /// </summary>
        private IEnumerator DeactivateEffectAfterDelay(GameObject effect, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (effect != null)
            {
                effect.SetActive(false);
            }
        }
        
        /// <summary>
        /// 펄스 스케일 효과 (확대/축소 반복)
        /// </summary>
        private IEnumerator ScalePulseEffect(float maxScale, float duration, int pulseCount)
        {
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * maxScale;
            
            for (int i = 0; i < pulseCount; i++)
            {
                // 확대
                float elapsedTime = 0f;
                while (elapsedTime < duration / 2)
                {
                    transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / (duration / 2));
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                
                // 축소
                elapsedTime = 0f;
                while (elapsedTime < duration / 2)
                {
                    transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / (duration / 2));
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
            
            // 원래 크기로 복원
            transform.localScale = originalScale;
        }
        
        /// <summary>
        /// 흔들림 효과
        /// </summary>
        private IEnumerator ShakeEffect(float intensity, float duration)
        {
            Vector3 originalPosition = transform.localPosition;
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                Vector3 randomOffset = Random.insideUnitSphere * intensity;
                transform.localPosition = originalPosition + randomOffset;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // 원래 위치로 복원
            transform.localPosition = originalPosition;
        }
        
        /// <summary>
        /// 회전 효과
        /// </summary>
        private IEnumerator RotationEffect(float angle, float duration)
        {
            Quaternion originalRotation = transform.localRotation;
            Quaternion targetRotation = originalRotation * Quaternion.Euler(0, angle, 0);
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                transform.localRotation = Quaternion.Slerp(
                    originalRotation, 
                    targetRotation, 
                    elapsed / duration
                );
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // 회전 완료 후 원래 회전으로 복원 (선택적)
            transform.localRotation = originalRotation;
        }
    }
} 