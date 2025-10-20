using UnityEngine;
using System.Collections;

namespace UnhappyCompany.Utility
{
    /// <summary>
    /// 파티클 및 이펙트를 독립적으로 재생하고 자동으로 정리하는 범용 유틸리티 시스템
    /// </summary>
    public static class EffectSpawnSystem
    {
        /// <summary>
        /// 파티클을 지정된 위치에 생성하고 재생 후 자동 삭제
        /// </summary>
        /// <param name="particlePrefab">생성할 파티클 프리팹</param>
        /// <param name="position">생성 위치</param>
        /// <param name="rotation">생성 회전 (기본값: Quaternion.identity)</param>
        /// <param name="parent">부모 Transform (기본값: null - 월드 루트)</param>
        /// <param name="autoScale">부모의 스케일 영향 받을지 여부 (기본값: true)</param>
        /// <returns>생성된 파티클 GameObject</returns>
        public static GameObject SpawnParticle(
            ParticleSystem particlePrefab, 
            Vector3 position, 
            Quaternion rotation = default,
            Transform parent = null,
            bool autoScale = true)
        {
            if (particlePrefab == null)
            {
                Debug.LogWarning("[EffectSpawnSystem] 파티클 프리팹이 null입니다!");
                return null;
            }

            if (rotation == default)
                rotation = Quaternion.identity;

            // 파티클 인스턴스 생성
            GameObject particleInstance = Object.Instantiate(particlePrefab.gameObject, position, rotation, parent);
            ParticleSystem ps = particleInstance.GetComponent<ParticleSystem>();

            if (ps == null)
            {
                Debug.LogWarning("[EffectSpawnSystem] ParticleSystem 컴포넌트를 찾을 수 없습니다!");
                Object.Destroy(particleInstance);
                return null;
            }

            // 자동 삭제 컴포넌트 추가
            AutoDestroyParticle autoDestroy = particleInstance.AddComponent<AutoDestroyParticle>();
            autoDestroy.Initialize(ps);

            // 파티클 재생
            ps.Play();

            Debug.Log($"[EffectSpawnSystem] 파티클 생성: {particlePrefab.name} at {position}");
            return particleInstance;
        }

        /// <summary>
        /// 파티클을 지정된 Transform 위치에 생성
        /// </summary>
        public static GameObject SpawnParticleAtTransform(
            ParticleSystem particlePrefab,
            Transform targetTransform,
            Vector3 offset = default,
            bool attachToParent = false)
        {
            if (targetTransform == null)
            {
                Debug.LogWarning("[EffectSpawnSystem] targetTransform이 null입니다!");
                return null;
            }

            Vector3 spawnPosition = targetTransform.position + offset;
            Quaternion spawnRotation = targetTransform.rotation;
            Transform parent = attachToParent ? targetTransform : null;

            return SpawnParticle(particlePrefab, spawnPosition, spawnRotation, parent);
        }

        /// <summary>
        /// 일회성 이펙트 재생 (프리팹에서 직접)
        /// </summary>
        /// <param name="effectPrefab">이펙트 GameObject 프리팹</param>
        /// <param name="position">생성 위치</param>
        /// <param name="rotation">생성 회전</param>
        /// <param name="duration">자동 삭제 시간 (0이면 파티클 duration 사용)</param>
        /// <returns>생성된 이펙트 GameObject</returns>
        public static GameObject SpawnEffect(
            GameObject effectPrefab,
            Vector3 position,
            Quaternion rotation = default,
            float duration = 0f)
        {
            if (effectPrefab == null)
            {
                Debug.LogWarning("[EffectSpawnSystem] 이펙트 프리팹이 null입니다!");
                return null;
            }

            if (rotation == default)
                rotation = Quaternion.identity;

            GameObject effectInstance = Object.Instantiate(effectPrefab, position, rotation);

            // ParticleSystem이 있으면 자동 삭제 설정
            ParticleSystem ps = effectInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                AutoDestroyParticle autoDestroy = effectInstance.AddComponent<AutoDestroyParticle>();
                autoDestroy.Initialize(ps);
                ps.Play();
            }
            else if (duration > 0f)
            {
                // ParticleSystem이 없으면 지정된 시간 후 삭제
                Object.Destroy(effectInstance, duration);
            }
            else
            {
                Debug.LogWarning("[EffectSpawnSystem] ParticleSystem이 없고 duration도 0입니다. 수동으로 삭제해야 합니다!");
            }

            return effectInstance;
        }

        /// <summary>
        /// 여러 파티클을 동시에 재생
        /// </summary>
        public static GameObject[] SpawnMultipleParticles(
            ParticleSystem[] particlePrefabs,
            Vector3 position,
            Quaternion rotation = default,
            Transform parent = null)
        {
            if (particlePrefabs == null || particlePrefabs.Length == 0)
            {
                Debug.LogWarning("[EffectSpawnSystem] 파티클 배열이 비어있습니다!");
                return null;
            }

            GameObject[] instances = new GameObject[particlePrefabs.Length];

            for (int i = 0; i < particlePrefabs.Length; i++)
            {
                if (particlePrefabs[i] != null)
                {
                    instances[i] = SpawnParticle(particlePrefabs[i], position, rotation, parent);
                }
            }

            return instances;
        }
    }

    /// <summary>
    /// 파티클 재생 완료 후 자동으로 GameObject를 삭제하는 컴포넌트
    /// </summary>
    public class AutoDestroyParticle : MonoBehaviour
    {
        private ParticleSystem[] particleSystems;
        private float maxDuration = 0f;
        private float spawnTime;
        private bool isInitialized = false;

        /// <summary>
        /// 파티클 시스템 초기화
        /// </summary>
        public void Initialize(ParticleSystem mainParticleSystem)
        {
            // 자신과 모든 자식의 ParticleSystem 가져오기
            particleSystems = GetComponentsInChildren<ParticleSystem>();

            if (particleSystems == null || particleSystems.Length == 0)
            {
                Debug.LogWarning("[AutoDestroyParticle] ParticleSystem을 찾을 수 없습니다!");
                Destroy(gameObject);
                return;
            }

            // 가장 긴 파티클 duration 찾기
            foreach (var ps in particleSystems)
            {
                float duration = ps.main.duration + ps.main.startLifetime.constantMax;
                if (ps.main.loop)
                {
                    Debug.LogWarning($"[AutoDestroyParticle] {ps.name}이(가) Loop 상태입니다. 자동 삭제되지 않습니다!");
                    duration = 10f; // 루프 파티클은 기본 10초 후 삭제
                }
                maxDuration = Mathf.Max(maxDuration, duration);
            }

            spawnTime = Time.time;
            isInitialized = true;

            Debug.Log($"[AutoDestroyParticle] {gameObject.name} - {maxDuration:F2}초 후 자동 삭제 예정");
        }

        private void Update()
        {
            if (!isInitialized)
                return;

            // 파티클이 모두 끝났는지 체크
            bool allFinished = true;
            foreach (var ps in particleSystems)
            {
                if (ps != null && ps.IsAlive())
                {
                    allFinished = false;
                    break;
                }
            }

            // 모든 파티클이 끝났거나 최대 시간이 지나면 삭제
            if (allFinished || Time.time - spawnTime > maxDuration + 1f)
            {
                Debug.Log($"[AutoDestroyParticle] {gameObject.name} 파티클 재생 완료 - 삭제");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 파티클을 즉시 중지하고 삭제
        /// </summary>
        public void StopAndDestroy()
        {
            if (particleSystems != null)
            {
                foreach (var ps in particleSystems)
                {
                    if (ps != null)
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
            Destroy(gameObject);
        }
    }
}


