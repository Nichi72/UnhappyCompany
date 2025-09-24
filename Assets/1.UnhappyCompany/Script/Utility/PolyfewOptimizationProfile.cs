using System.Collections.Generic;
using UnityEngine;
using BrainFailProductions.PolyFewRuntime;
#if UNITY_EDITOR
using BrainFailProductions.PolyFew;
#endif

namespace UnhappyCompany.Utility
{
    [CreateAssetMenu(fileName = "New Polyfew Optimization Profile", menuName = "UnhappyCompany/Polyfew Optimization Profile")]
    public class PolyfewOptimizationProfile : ScriptableObject
    {
        [Header("기본 최적화 설정")]
        [Tooltip("폴리곤 감소 강도 (0-100)")]
        [Range(0f, 100f)]
        public float simplificationStrength = 50f;
        
        [Tooltip("UV Foldover 영역 보존")]
        public bool preserveUVFoldover = false;
        
        [Tooltip("경계 엣지 보존")]
        public bool preserveBorders = true;
        
        [Tooltip("UV 심 엣지 보존")]
        public bool preserveUVSeams = true;
        
        [Tooltip("엣지 정렬 사용")]
        public bool useEdgeSort = false;
        
        [Tooltip("노말 재계산")]
        public bool recalculateNormals = true;
        
        [Tooltip("블렌드셰이프 제거")]
        public bool clearBlendshapes = true;
        
        [Tooltip("UV2 생성")]
        public bool generateUV2 = true;
        
        [Tooltip("스마트 링킹 활성화")]
        public bool enableSmartlinking = true;
        
        [Tooltip("곡률 고려")]
        public bool regardCurvature = false;
        
        [Header("고급 설정")]
        [Tooltip("최대 반복 횟수")]
        [Range(100, 1000)]
        public int maxIterations = 100;
        
        [Tooltip("공격성 (높을수록 품질 향상, 느려짐)")]
        [Range(7f, 20f)]
        public float aggressiveness = 7f;
        
        [Header("LOD 설정")]
        [Tooltip("LOD 레벨 수")]
        [Range(1, 4)]
        public int lodLevels = 2;
        
        [Tooltip("각 LOD 레벨별 설정")]
        public List<LODLevelData> lodLevelSettings = new List<LODLevelData>();
        
        [Header("배치 처리 설정")]
        [Tooltip("한 번에 처리할 오브젝트 수")]
        [Range(1, 20)]
        public int batchSize = 5;
        
        [System.Serializable]
        public class LODLevelData
        {
            [Tooltip("이 LOD 레벨의 폴리곤 감소 강도")]
            [Range(0f, 100f)]
            public float reductionStrength = 50f;
            
            [Tooltip("화면 상대 전환 높이")]
            [Range(0f, 1f)]
            public float transitionHeight = 0.5f;
            
            [Tooltip("메쉬 결합 여부")]
            public bool combineMeshes = false;
        }
        
        private void OnValidate()
        {
            // LOD 레벨 수가 변경되면 설정 리스트도 맞춰서 조정
            while (lodLevelSettings.Count < lodLevels)
            {
                float defaultReduction = (lodLevelSettings.Count + 1) * 25f; // 25%, 50%, 75%, 100%
                float defaultTransition = 1f - (float)(lodLevelSettings.Count + 1) / (lodLevels + 1);
                
                lodLevelSettings.Add(new LODLevelData
                {
                    reductionStrength = Mathf.Min(defaultReduction, 100f),
                    transitionHeight = defaultTransition,
                    combineMeshes = false
                });
            }
            
            while (lodLevelSettings.Count > lodLevels)
            {
                lodLevelSettings.RemoveAt(lodLevelSettings.Count - 1);
            }
        }
        
        /// <summary>
        /// 이 프로필에서 SimplificationOptions 객체를 생성합니다.
        /// </summary>
        public PolyfewRuntime.SimplificationOptions CreateSimplificationOptions()
        {
            var options = new PolyfewRuntime.SimplificationOptions();
            
            options.simplificationStrength = simplificationStrength;
            options.preserveUVFoldoverEdges = preserveUVFoldover;
            options.preserveBorderEdges = preserveBorders;
            options.preserveUVSeamEdges = preserveUVSeams;
            options.useEdgeSort = useEdgeSort;
            options.recalculateNormals = recalculateNormals;
            options.enableSmartlinking = enableSmartlinking;
            options.regardCurvature = regardCurvature;
            options.maxIterations = maxIterations;
            options.aggressiveness = aggressiveness;
            
            return options;
        }
        
        /// <summary>
        /// 이 프로필에서 LOD 레벨 데이터를 가져옵니다.
        /// </summary>
        public List<LODLevelData> GetLODLevelData()
        {
            return lodLevelSettings;
        }
    }
}
