using UnityEngine;
using System.Collections;

namespace UnhappyCompany.Utility
{
    /// <summary>
    /// Rigidbody Constraints를 일정 시간 후 복구하는 일회성 컴포넌트
    /// 폭발 등으로 임시로 Constraints를 해제한 후 자동으로 복구하기 위해 사용
    /// 플레이어인 경우 FirstPersonController와 CharacterController도 함께 비활성화/복구
    /// </summary>
    public class RigidbodyConstraintRestore : MonoBehaviour
    {
        private RigidbodyConstraints targetConstraints;
        private float restoreDelay;
        private Rigidbody targetRigidbody;
        private MonoBehaviour playerController;
        private CharacterController characterController;
        
        /// <summary>
        /// 복구 설정 초기화 (플레이어 컨트롤러 포함)
        /// </summary>
        public void Initialize(Rigidbody rb, RigidbodyConstraints originalConstraints, float delay, MonoBehaviour controller = null)
        {
            targetRigidbody = rb;
            restoreDelay = delay;
            playerController = controller;
            
            // CharacterController 찾기 (있으면)
            characterController = rb.GetComponent<CharacterController>();
            
            // 플레이어 컨트롤러 비활성화 (입력 차단)
            if (playerController != null)
            {
                playerController.enabled = false;
                Debug.Log($"[RigidbodyConstraintRestore] {playerController.GetType().Name} 일시 비활성화");
            }
            
            // CharacterController 비활성화 (물리 효과가 작동하도록)
            if (characterController != null)
            {
                characterController.enabled = false;
                Debug.Log($"[RigidbodyConstraintRestore] CharacterController 일시 비활성화 (물리 효과 적용)");
            }
            
            StartCoroutine(RestoreCoroutine());
        }
        
        private IEnumerator RestoreCoroutine()
        {
            yield return new WaitForSeconds(restoreDelay);
            
            // 1. CharacterController 재활성화 (먼저!)
            if (characterController != null)
            {
                characterController.enabled = true;
                Debug.Log($"[RigidbodyConstraintRestore] CharacterController 재활성화 완료");
            }
            
            // 2. 플레이어 컨트롤러 재활성화
            if (playerController != null)
            {
                playerController.enabled = true;
                Debug.Log($"[RigidbodyConstraintRestore] {playerController.GetType().Name} 재활성화 완료");
            }
            
            // 3. Constraints 복구 (마지막에!)
            // CharacterController가 켜진 후에 Constraints를 설정해야 덮어씌워지지 않음
            if (targetRigidbody != null)
            {
                targetRigidbody.constraints = 
                RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
                Debug.Log($"[RigidbodyConstraintRestore] Constraints 최종 복구: {targetConstraints}");
            }
            else
            {
                Debug.LogWarning("[RigidbodyConstraintRestore] Rigidbody가 null입니다. 복구 실패.");
            }
            
            // 복구 완료 후 컴포넌트 제거
            Destroy(this);
        }
    }
}

