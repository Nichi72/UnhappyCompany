using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnhappyCompany
{
    /// <summary>
    /// 슬롯머신 심볼의 시각적 효과와 상태를 관리하는 컴포넌트
    /// 각 심볼에 추가하여 사용합니다.
    /// </summary>
    public class SlotMachineSymbol : MonoBehaviour
    {
        [Header("시각 효과 설정")]
        [Tooltip("심볼이 중앙에 있을 때 하이라이트 효과")]
        public bool enableHighlight = true;
        
        [Tooltip("하이라이트 효과의 색상")]
        public Color highlightColor = Color.yellow;
        
        [Tooltip("기본 색상")]
        public Color normalColor = Color.white;
        
        [Tooltip("하이라이트 효과의 강도")]
        [Range(0.0f, 2.0f)]
        public float highlightIntensity = 1.5f;
        
        [Header("회전 효과")]
        [Tooltip("심볼 자체 회전 활성화")]
        public bool enableSelfRotation = false;
        
        [Tooltip("심볼 회전 속도 (도/초)")]
        public float rotationSpeed = 45.0f;
        
        // 렌더러 컴포넌트
        private Renderer symbolRenderer;
        
        // 머티리얼의 원래 색상
        private Color originalColor;
        
        // 중앙에 있는지 여부
        private bool isAtCenter = false;
        
        // 중앙 각도
        private float centerAngle = 0.0f;
        
        // 중앙 허용 오차
        private float centerTolerance = 5.0f;
        
        private void Awake()
        {
            // 렌더러 컴포넌트 가져오기
            symbolRenderer = GetComponent<Renderer>();
            if (symbolRenderer == null)
            {
                symbolRenderer = GetComponentInChildren<Renderer>();
            }
            
            // 원래 색상 저장
            if (symbolRenderer != null && symbolRenderer.material != null)
            {
                originalColor = symbolRenderer.material.color;
            }
            else
            {
                originalColor = normalColor;
            }
        }
        
        private void Update()
        {
            // 자체 회전 효과 적용
            if (enableSelfRotation)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }
        }
        
        /// <summary>
        /// 심볼의 각도 설정
        /// </summary>
        public void SetAngle(float angle, float centerAngleValue)
        {
            // 중앙 각도 저장
            centerAngle = centerAngleValue;
            
            // 중앙에 있는지 확인
            bool wasAtCenter = isAtCenter;
            isAtCenter = Mathf.Abs(Mathf.DeltaAngle(angle, centerAngle)) < centerTolerance;
            
            // 상태가 변경되었을 때만 효과 적용
            if (isAtCenter != wasAtCenter)
            {
                if (isAtCenter)
                {
                    ApplyCenterEffect();
                }
                else
                {
                    RemoveCenterEffect();
                }
            }
        }
        
        /// <summary>
        /// 중앙 효과 적용
        /// </summary>
        private void ApplyCenterEffect()
        {
            // 색상 효과
            if (enableHighlight && symbolRenderer != null && symbolRenderer.material != null)
            {
                symbolRenderer.material.color = highlightColor * highlightIntensity;
                
                // 발광 효과 추가
                symbolRenderer.material.EnableKeyword("_EMISSION");
                symbolRenderer.material.SetColor("_EmissionColor", highlightColor * highlightIntensity);
            }
        }
        
        /// <summary>
        /// 중앙 효과 제거
        /// </summary>
        private void RemoveCenterEffect()
        {
            // 색상 복원
            if (symbolRenderer != null && symbolRenderer.material != null)
            {
                symbolRenderer.material.color = normalColor;
                
                // 발광 효과 제거
                symbolRenderer.material.DisableKeyword("_EMISSION");
            }
        }
        
        /// <summary>
        /// 심볼이 등장할 때 페이드인 효과
        /// </summary>
        public void PlayEnterEffect()
        {
            // 게임 오브젝트가 비활성화되어 있으면 코루틴을 시작하지 않음
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            
            StartCoroutine(FadeInEffect());
        }
        
        /// <summary>
        /// 심볼이 사라질 때 페이드아웃 효과
        /// </summary>
        public void PlayExitEffect()
        {
            // 게임 오브젝트가 비활성화되어 있으면 코루틴을 시작하지 않음
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            
            StartCoroutine(FadeOutEffect());
        }
        
        /// <summary>
        /// 페이드인 효과
        /// </summary>
        private IEnumerator FadeInEffect()
        {
            if (symbolRenderer == null || symbolRenderer.material == null)
            {
                yield break;
            }
            
            // 알파값 조정을 위한 설정
            symbolRenderer.material.EnableKeyword("_ALPHABLEND_ON");
            
            // 시작 색상 (투명)
            Color startColor = normalColor;
            startColor.a = 0;
            symbolRenderer.material.color = startColor;
            
            // 페이드인 효과
            float duration = 0.3f;
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                // 경과 시간 기반 알파값 계산
                float t = elapsed / duration;
                
                // 색상 보간
                Color currentColor = Color.Lerp(startColor, normalColor, t);
                symbolRenderer.material.color = currentColor;
                
                // 시간 업데이트
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // 최종 색상으로 설정
            symbolRenderer.material.color = normalColor;
        }
        
        /// <summary>
        /// 페이드아웃 효과
        /// </summary>
        private IEnumerator FadeOutEffect()
        {
            if (symbolRenderer == null || symbolRenderer.material == null)
            {
                yield break;
            }
            
            // 알파값 조정을 위한 설정
            symbolRenderer.material.EnableKeyword("_ALPHABLEND_ON");
            
            // 시작 색상 (원래 색상)
            Color startColor = symbolRenderer.material.color;
            
            // 종료 색상 (투명)
            Color endColor = startColor;
            endColor.a = 0;
            
            // 페이드아웃 효과
            float duration = 0.3f;
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                // 경과 시간 기반 알파값 계산
                float t = elapsed / duration;
                
                // 색상 보간
                Color currentColor = Color.Lerp(startColor, endColor, t);
                symbolRenderer.material.color = currentColor;
                
                // 시간 업데이트
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // 완전히 투명하게 설정
            symbolRenderer.material.color = endColor;
        }
    }
} 