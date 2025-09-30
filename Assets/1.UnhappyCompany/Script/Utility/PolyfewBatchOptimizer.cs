using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using BrainFailProductions.PolyFewRuntime;
using BrainFailProductions.PolyFew;
using System;
using System.IO;

namespace UnhappyCompany.Utility
{
    public class PolyfewBatchOptimizer
    {
        private int processedCount = 0;
        private int totalCount = 0;
        private int errorCount = 0;
        private List<string> errorMessages = new List<string>();
        
        // 전체 폴리곤 감축 통계
        private int totalTrianglesBeforeOptimization = 0;
        private int totalTrianglesAfterOptimization = 0;
        
        /// <summary>
        /// 여러 오브젝트를 비동기적으로 최적화합니다.
        /// </summary>
        /// <param name="targetObjects">최적화할 오브젝트 리스트</param>
        /// <param name="profile">최적화 프로필</param>
        /// <param name="saveAssetsPath">에셋 저장 경로</param>
        /// <param name="generateLODs">LOD 생성 여부</param>
        /// <param name="stopOnError">에러 발생 시 중단 여부</param>
        public async Task OptimizeObjectsAsync(List<GameObject> targetObjects, PolyfewOptimizationProfile profile, 
            string saveAssetsPath, bool generateLODs, bool stopOnError)
        {
            if (targetObjects == null || targetObjects.Count == 0)
            {
                throw new ArgumentException("최적화할 오브젝트가 없습니다.");
            }
            
            if (profile == null)
            {
                throw new ArgumentException("최적화 프로필이 지정되지 않았습니다.");
            }
            
            // 초기화
            processedCount = 0;
            totalCount = targetObjects.Count;
            errorCount = 0;
            errorMessages.Clear();
            totalTrianglesBeforeOptimization = 0;
            totalTrianglesAfterOptimization = 0;
            
            Debug.Log($"[Polyfew Batch Optimizer] 최적화 시작: {totalCount}개 오브젝트");
            Debug.Log($"[Polyfew Batch Optimizer] 프로필: {profile.name}");
            Debug.Log($"[Polyfew Batch Optimizer] 저장 경로: {saveAssetsPath}");
            Debug.Log($"[Polyfew Batch Optimizer] LOD 생성: {(generateLODs ? "활성화" : "비활성화")}");
            Debug.Log($"[Polyfew Batch Optimizer] 에러 처리: {(stopOnError ? "중단" : "건너뛰기")}");
            
            // 저장 경로 생성
            CreateSaveDirectory(saveAssetsPath);
            
            // 배치 크기에 따라 처리
            int batchSize = profile.batchSize;
            for (int i = 0; i < targetObjects.Count; i += batchSize)
            {
                int endIndex = Mathf.Min(i + batchSize, targetObjects.Count);
                var batch = targetObjects.GetRange(i, endIndex - i);
                
                Debug.Log($"[Polyfew Batch Optimizer] 배치 처리 중: {i + 1}-{endIndex}/{totalCount}");
                
                await ProcessBatch(batch, profile, saveAssetsPath, generateLODs, stopOnError);
                
                // UI 업데이트를 위한 짧은 대기
                await Task.Delay(100);
            }
            
            // 결과 리포트
            LogFinalResults();
        }
        
        private async Task ProcessBatch(List<GameObject> batch, PolyfewOptimizationProfile profile, 
            string saveAssetsPath, bool generateLODs, bool stopOnError)
        {
            foreach (GameObject targetObject in batch)
            {
                if (targetObject == null)
                {
                    Debug.LogWarning($"[Polyfew Batch Optimizer] null 오브젝트 건너뛰기");
                    processedCount++;
                    continue;
                }
                
                try
                {
                    Debug.Log($"[Polyfew Batch Optimizer] 처리 중: {targetObject.name} ({processedCount + 1}/{totalCount})");
                    
                    await OptimizeSingleObject(targetObject, profile, saveAssetsPath, generateLODs);
                    
                    Debug.Log($"[Polyfew Batch Optimizer] 완료: {targetObject.name}");
                    processedCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    string errorMessage = $"오브젝트 '{targetObject.name}' 처리 실패: {ex.Message}";
                    errorMessages.Add(errorMessage);
                    
                    Debug.LogError($"[Polyfew Batch Optimizer] {errorMessage}");
                    
                    if (stopOnError)
                    {
                        throw new Exception($"에러 발생으로 처리 중단: {errorMessage}", ex);
                    }
                    
                    processedCount++;
                }
                
                // 진행률 표시
                float progress = (float)processedCount / totalCount;
                EditorUtility.DisplayProgressBar("Polyfew Batch Optimization", 
                    $"처리 중: {targetObject.name} ({processedCount}/{totalCount})", progress);
            }
        }
        
        private async Task OptimizeSingleObject(GameObject targetObject, PolyfewOptimizationProfile profile, 
            string saveAssetsPath, bool generateLODs)
        {
            // null 체크
            if (targetObject == null)
            {
                throw new ArgumentNullException(nameof(targetObject), "최적화할 오브젝트가 null입니다.");
            }
            
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile), "최적화 프로필이 null입니다.");
            }
            
            // 메쉬 정보 추출
            var objectMeshPairs = PolyfewRuntime.GetObjectMeshPairs(targetObject, true);
            
            if (objectMeshPairs == null || objectMeshPairs.Count == 0)
            {
                throw new InvalidOperationException($"오브젝트 '{targetObject.name}'에서 최적화 가능한 메쉬를 찾을 수 없습니다.");
            }
            
            Debug.Log($"[Polyfew Batch Optimizer] 메쉬 발견: {objectMeshPairs.Count}개 ({targetObject.name})");
            
            // 최적화 옵션 생성
            var simplificationOptions = profile.CreateSimplificationOptions();
            if (simplificationOptions == null)
            {
                throw new InvalidOperationException($"최적화 옵션 생성 실패 ({targetObject.name})");
            }
            
            // 원본 메쉬 정보 백업 (LOD 0용) - 최적화 전에 먼저 백업!
            var originalMeshes = BackupOriginalMeshes(objectMeshPairs);
            
            // 기본 최적화 수행 (LOD 1용) - 원본은 건드리지 않고 복사본에서 작업
            int trianglesBefore = PolyfewRuntime.CountTriangles(true, targetObject);
            
            // 원본을 보존하기 위해 복사본 생성
            var tempObject = CreateTemporaryObjectForOptimization(targetObject);
            var tempObjectMeshPairs = PolyfewRuntime.GetObjectMeshPairs(tempObject, true);
            
            // 임시 오브젝트에서 최적화 수행
            Debug.Log($"[Polyfew Batch Optimizer] SimplifyObjectDeep 실행 중... ({targetObject.name})");
            var resultPairs = PolyfewRuntime.SimplifyObjectDeep(tempObjectMeshPairs, simplificationOptions, null);
            
            Debug.Log($"[Polyfew Batch Optimizer] SimplifyObjectDeep 실행 완료");
            
            // 최적화 결과 검증
            var updatedTempObjectMeshPairs = PolyfewRuntime.GetObjectMeshPairs(tempObject, true);
            if (updatedTempObjectMeshPairs != null && updatedTempObjectMeshPairs.Count > 0)
            {
                Debug.Log($"[Polyfew Batch Optimizer] 최적화 후 메쉬 확인: {updatedTempObjectMeshPairs.Count}개");
            }
            else
            {
                Debug.LogWarning($"[Polyfew Batch Optimizer] 최적화 후 메쉬를 찾을 수 없습니다.");
            }
            
            int trianglesAfter = PolyfewRuntime.CountTriangles(true, tempObject);
            float reductionPercentage = trianglesBefore > 0 ? ((float)(trianglesBefore - trianglesAfter) / trianglesBefore) * 100f : 0f;
            
            // 전체 통계에 추가
            totalTrianglesBeforeOptimization += trianglesBefore;
            totalTrianglesAfterOptimization += trianglesAfter;
            
            Debug.Log($"[Polyfew Batch Optimizer] 폴리곤 감소: {trianglesBefore} → {trianglesAfter} " +
                     $"({reductionPercentage:F1}% 감소) ({targetObject.name})");
            
            // 최적화 검증
            if (trianglesAfter >= trianglesBefore)
            {
                Debug.LogWarning($"[Polyfew Batch Optimizer] ⚠ 폴리곤이 감소하지 않았습니다! 최적화 설정을 확인하세요. ({targetObject.name})");
            }
            
            // 최적화된 메쉬를 원본 오브젝트와 매핑해서 추출 (업데이트된 메쉬 정보 사용)
            var optimizedMeshes = ExtractOptimizedMeshes(targetObject, tempObject, updatedTempObjectMeshPairs);
            
            // 최적화된 메쉬를 파일로 저장
            await SaveOptimizedMeshesToFiles(optimizedMeshes, saveAssetsPath, targetObject.name);
            
            // 임시 오브젝트 제거
            UnityEngine.Object.DestroyImmediate(tempObject);
            
            // LOD 생성 (원본은 그대로, LOD 1만 별도 생성)
            if (generateLODs && profile.lodLevels > 1)
            {
                await GenerateOptimizedLOD(targetObject, profile, optimizedMeshes, saveAssetsPath);
            }
            
            // 변경사항 저장
            EditorUtility.SetDirty(targetObject);
            
            // 메모리 정리를 위한 짧은 대기
            await Task.Delay(50);
        }
        
        
        private void CreateSaveDirectory(string saveAssetsPath)
        {
            if (string.IsNullOrEmpty(saveAssetsPath))
            {
                saveAssetsPath = "Assets/OptimizedMeshes";
            }
            
            // 절대 경로로 변환
            string fullPath = Path.GetFullPath(saveAssetsPath);
            
            try
            {
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                    AssetDatabase.Refresh();
                    Debug.Log($"[Polyfew Batch Optimizer] 저장 디렉토리 생성: {saveAssetsPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Polyfew Batch Optimizer] 저장 디렉토리 생성 실패: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 원본을 보존하기 위해 최적화용 임시 오브젝트를 생성합니다 (메쉬도 개별 복사)
        /// </summary>
        private GameObject CreateTemporaryObjectForOptimization(GameObject originalObject)
        {
            // 원본 오브젝트를 복사
            var tempObject = UnityEngine.Object.Instantiate(originalObject);
            tempObject.name = originalObject.name + "_TempForOptimization";
            tempObject.SetActive(false); // 화면에 보이지 않게
            
            // 모든 메쉬를 개별적으로 복사하여 독립적인 인스턴스 생성
            var meshFilters = tempObject.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                if (meshFilter.sharedMesh != null)
                {
                    // 메쉬를 개별 복사하여 독립적인 인스턴스 생성
                    var meshCopy = UnityEngine.Object.Instantiate(meshFilter.sharedMesh);
                    meshCopy.name = meshFilter.sharedMesh.name + "_TempCopy";
                    meshFilter.sharedMesh = meshCopy;
                    
                    Debug.Log($"[Polyfew Batch Optimizer] 메쉬 개별 복사: {meshCopy.name}");
                }
            }
            
            // 스킨드 메쉬 렌더러도 처리
            var skinnedMeshRenderers = tempObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var skinnedRenderer in skinnedMeshRenderers)
            {
                if (skinnedRenderer.sharedMesh != null)
                {
                    var meshCopy = UnityEngine.Object.Instantiate(skinnedRenderer.sharedMesh);
                    meshCopy.name = skinnedRenderer.sharedMesh.name + "_TempCopy";
                    skinnedRenderer.sharedMesh = meshCopy;
                    
                    Debug.Log($"[Polyfew Batch Optimizer] 스킨드 메쉬 개별 복사: {meshCopy.name}");
                }
            }
            
            Debug.Log($"[Polyfew Batch Optimizer] 최적화용 임시 오브젝트 생성 완료: {tempObject.name}");
            return tempObject;
        }
        
        /// <summary>
        /// 원본 메쉬 정보를 백업합니다 (LOD 0용)
        /// </summary>
        private Dictionary<GameObject, Mesh> BackupOriginalMeshes(PolyfewRuntime.ObjectMeshPairs objectMeshPairs)
        {
            var originalMeshes = new Dictionary<GameObject, Mesh>();
            
            foreach (var kvp in objectMeshPairs)
            {
                if (kvp.Key != null && kvp.Value.mesh != null)
                {
                    // 원본 메쉬를 그대로 사용 (복사하지 않음)
                    originalMeshes[kvp.Key] = kvp.Value.mesh;
                }
            }
            
            Debug.Log($"[Polyfew Batch Optimizer] 원본 메쉬 참조 보존: {originalMeshes.Count}개");
            return originalMeshes;
        }
        
        /// <summary>
        /// 임시 오브젝트에서 최적화된 메쉬를 추출하여 원본 오브젝트와 매핑합니다
        /// </summary>
        private Dictionary<GameObject, Mesh> ExtractOptimizedMeshes(GameObject originalObject, GameObject tempObject, 
            PolyfewRuntime.ObjectMeshPairs tempObjectMeshPairs)
        {
            var optimizedMeshes = new Dictionary<GameObject, Mesh>();
            
            Debug.Log($"[Polyfew Batch Optimizer] 최적화된 메쉬 추출 시작: {tempObjectMeshPairs.Count}개 ({originalObject.name})");
            
            // 원본 오브젝트의 렌더러들을 가져오기
            var originalRenderers = originalObject.GetComponentsInChildren<MeshRenderer>();
            var originalRenderersDict = new Dictionary<string, MeshRenderer>();
            
            foreach (var renderer in originalRenderers)
            {
                if (renderer != null && renderer.gameObject != null)
                {
                    originalRenderersDict[renderer.gameObject.name] = renderer;
                }
            }
            
            // 임시 오브젝트에서 최적화된 메쉬들을 추출
            foreach (var kvp in tempObjectMeshPairs)
            {
                if (kvp.Key != null && kvp.Value.mesh != null)
                {
                    // 임시 오브젝트의 이름에서 원본 이름 추출
                    string tempObjectName = kvp.Key.name;
                    string originalObjectName = tempObjectName;
                    
                    // 원본 렌더러 찾기
                    if (originalRenderersDict.TryGetValue(originalObjectName, out MeshRenderer originalRenderer))
                    {
                        // 최적화된 메쉬를 복사
                        var optimizedMesh = UnityEngine.Object.Instantiate(kvp.Value.mesh);
                        optimizedMesh.name = kvp.Value.mesh.name + "_Optimized";
                        
                        // 메쉬 최적화 검증
                        int originalTriangles = originalRenderer.GetComponent<MeshFilter>()?.sharedMesh?.triangles?.Length / 3 ?? 0;
                        int optimizedTriangles = optimizedMesh.triangles?.Length / 3 ?? 0;
                        
                        if (optimizedTriangles < originalTriangles)
                        {
                            float meshReduction = ((float)(originalTriangles - optimizedTriangles) / originalTriangles) * 100f;
                            Debug.Log($"[Polyfew Batch Optimizer] ✓ 메쉬 최적화 확인: {originalObjectName} " +
                                     $"({originalTriangles} → {optimizedTriangles}, {meshReduction:F1}% 감소)");
                        }
                        else
                        {
                            Debug.LogWarning($"[Polyfew Batch Optimizer] ⚠ 메쉬 최적화 실패: {originalObjectName} " +
                                           $"({originalTriangles} → {optimizedTriangles})");
                        }
                        
                        optimizedMeshes[originalRenderer.gameObject] = optimizedMesh;
                        Debug.Log($"[Polyfew Batch Optimizer] 메쉬 매핑 성공: {originalObjectName} -> {optimizedMesh.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"[Polyfew Batch Optimizer] 원본 렌더러를 찾을 수 없음: {originalObjectName}");
                    }
                }
            }
            
            Debug.Log($"[Polyfew Batch Optimizer] 최적화된 메쉬 추출 완료: {optimizedMeshes.Count}개");
            return optimizedMeshes;
        }
        
        /// <summary>
        /// 최적화된 메쉬들을 파일로 저장합니다
        /// </summary>
        private async Task SaveOptimizedMeshesToFiles(Dictionary<GameObject, Mesh> optimizedMeshes, string saveAssetsPath, string objectName)
        {
            try
            {
                // 저장할 폴더 경로 생성
                string meshSaveFolder = $"{saveAssetsPath}/{objectName}_OptimizedMeshes";
                CreateSaveDirectory(meshSaveFolder);
                
                Debug.Log($"[Polyfew Batch Optimizer] 최적화된 메쉬 파일 저장 시작: {optimizedMeshes.Count}개 ({objectName})");
                
                // Unity 기본 방식으로 메쉬 저장
                foreach (var kvp in optimizedMeshes)
                {
                    if (kvp.Key != null && kvp.Value != null)
                    {
                        try
                        {
                            string meshPath = $"{meshSaveFolder}/{kvp.Value.name}.asset";
                            AssetDatabase.CreateAsset(kvp.Value, meshPath);
                            Debug.Log($"[Polyfew Batch Optimizer] 메쉬 저장 성공: {meshPath}");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[Polyfew Batch Optimizer] 메쉬 저장 실패: {kvp.Value.name} - {ex.Message}");
                        }
                    }
                }
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                Debug.Log($"[Polyfew Batch Optimizer] 최적화된 메쉬 파일 저장 완료: {optimizedMeshes.Count}개 ({objectName})");
                
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Polyfew Batch Optimizer] 메쉬 저장 중 오류 ({objectName}): {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// LOD 0은 원본 그대로, LOD 1은 최적화된 메쉬로 깔끔하게 구조화된 LOD를 생성합니다
        /// </summary>
        private async Task GenerateOptimizedLOD(GameObject targetObject, PolyfewOptimizationProfile profile,
            Dictionary<GameObject, Mesh> optimizedMeshes, string saveAssetsPath)
        {
#if UNITY_EDITOR
            try
            {
                if (targetObject == null || profile == null)
                {
                    Debug.LogError("[Polyfew Batch Optimizer] LOD 생성 실패: 매개변수가 null입니다.");
                    return;
                }
                
                Debug.Log($"[Polyfew Batch Optimizer] 깔끔한 LOD 생성 시도: {targetObject.name}");
                
                // 기존 LOD 그룹 제거
                var existingLodGroup = targetObject.GetComponent<LODGroup>();
                if (existingLodGroup != null)
                {
                    UnityEngine.Object.DestroyImmediate(existingLodGroup);
                }
                
                // 새 LOD 그룹 생성
                var lodGroup = targetObject.AddComponent<LODGroup>();
                
                // LOD 0: 원본 렌더러들 (기존 그대로)
                var originalRenderers = targetObject.GetComponentsInChildren<Renderer>();
                var validOriginalRenderers = new List<Renderer>();
                foreach (var renderer in originalRenderers)
                {
                    if (renderer != null && renderer.gameObject != null)
                    {
                        validOriginalRenderers.Add(renderer);
                    }
                }
                
                // LOD 1: 최적화된 메쉬용 빈 오브젝트 생성
                var lod1Container = new GameObject("LOD1_Optimized");
                lod1Container.transform.SetParent(targetObject.transform);
                lod1Container.transform.localPosition = Vector3.zero;
                lod1Container.transform.localRotation = Quaternion.identity;
                lod1Container.transform.localScale = Vector3.one;
                
                // LOD 1용 렌더러들 생성
                var lod1Renderers = CreateOptimizedLODRenderers(lod1Container, optimizedMeshes);
                
                // LOD 배열 생성
                LOD[] lods = new LOD[2];
                lods[0] = new LOD(0.6f, validOriginalRenderers.ToArray()); // 60% 이상에서 원본 사용
                lods[1] = new LOD(0.3f, lod1Renderers.ToArray()); // 30% 이상에서 최적화된 메쉬 사용
                
                // LOD 설정 적용
                lodGroup.SetLODs(lods);
                lodGroup.RecalculateBounds();
                
                Debug.Log($"[Polyfew Batch Optimizer] 깔끔한 LOD 생성 완료: LOD0({validOriginalRenderers.Count}개 원본), LOD1({lod1Renderers.Count}개 최적화) ({targetObject.name})");
                
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Polyfew Batch Optimizer] LOD 생성 중 오류 ({targetObject?.name ?? "null"}): {ex.Message}");
                throw;
            }
#else
            Debug.LogWarning($"[Polyfew Batch Optimizer] LOD 생성은 에디터에서만 지원됩니다: {targetObject?.name ?? "null"}");
            await Task.Delay(100);
#endif
        }
        
        /// <summary>
        /// 최적화된 메쉬로 LOD1용 렌더러들을 깔끔하게 생성합니다
        /// </summary>
        private List<Renderer> CreateOptimizedLODRenderers(GameObject parentContainer, Dictionary<GameObject, Mesh> optimizedMeshes)
        {
            var renderers = new List<Renderer>();
            
            Debug.Log($"[Polyfew Batch Optimizer] LOD1 렌더러 생성 시작: {optimizedMeshes.Count}개 메쉬");
            
            foreach (var kvp in optimizedMeshes)
            {
                if (kvp.Key != null && kvp.Value != null)
                {
                    Debug.Log($"[Polyfew Batch Optimizer] LOD1 오브젝트 생성 중: {kvp.Key.name} -> {kvp.Value.name}");
                    
                    // LOD1용 자식 오브젝트 생성
                    var lodObject = new GameObject(kvp.Key.name + "_Optimized");
                    lodObject.transform.SetParent(parentContainer.transform);
                    
                    // Transform 정보 복사 (원본 오브젝트 기준)
                    lodObject.transform.position = kvp.Key.transform.position;
                    lodObject.transform.rotation = kvp.Key.transform.rotation;
                    lodObject.transform.localScale = kvp.Key.transform.localScale;
                    
                    // 메쉬 필터와 렌더러 추가
                    var meshFilter = lodObject.AddComponent<MeshFilter>();
                    var meshRenderer = lodObject.AddComponent<MeshRenderer>();
                    
                    meshFilter.sharedMesh = kvp.Value;
                    
                    // 원본 렌더러에서 머티리얼 복사
                    var originalRenderer = kvp.Key.GetComponent<MeshRenderer>();
                    if (originalRenderer != null)
                    {
                        meshRenderer.sharedMaterials = originalRenderer.sharedMaterials;
                        Debug.Log($"[Polyfew Batch Optimizer] 머티리얼 복사 완료: {originalRenderer.sharedMaterials.Length}개");
                    }
                    else
                    {
                        Debug.LogWarning($"[Polyfew Batch Optimizer] 원본 렌더러를 찾을 수 없음: {kvp.Key.name}");
                    }
                    
                    renderers.Add(meshRenderer);
                    Debug.Log($"[Polyfew Batch Optimizer] LOD1 오브젝트 생성 완료: {lodObject.name}");
                }
                else
                {
                    Debug.LogWarning($"[Polyfew Batch Optimizer] null 발견: Key={kvp.Key?.name}, Value={kvp.Value?.name}");
                }
            }
            
            Debug.Log($"[Polyfew Batch Optimizer] LOD1 렌더러 생성 완료: {renderers.Count}개 ({parentContainer.transform.parent?.name ?? "null"})");
            return renderers;
        }
        
        private void LogFinalResults()
        {
            EditorUtility.ClearProgressBar();
            
            Debug.Log("=== Polyfew Batch Optimizer 결과 ===");
            Debug.Log($"전체 오브젝트: {totalCount}개");
            Debug.Log($"성공적으로 처리: {processedCount - errorCount}개");
            Debug.Log($"오류 발생: {errorCount}개");
            
            // 전체 폴리곤 감축 통계
            if (totalTrianglesBeforeOptimization > 0)
            {
                int reducedTriangles = totalTrianglesBeforeOptimization - totalTrianglesAfterOptimization;
                float totalReductionPercentage = ((float)reducedTriangles / totalTrianglesBeforeOptimization) * 100f;
                
                Debug.Log("=== 전체 폴리곤 감축 통계 ===");
                Debug.Log($"최적화 전 총 삼각형: {totalTrianglesBeforeOptimization:N0}개");
                Debug.Log($"최적화 후 총 삼각형: {totalTrianglesAfterOptimization:N0}개");
                Debug.Log($"감축된 삼각형: {reducedTriangles:N0}개");
                Debug.Log($"전체 감축률: {totalReductionPercentage:F1}%");
                
                if (reducedTriangles > 0)
                {
                    Debug.Log($"<color=green>✓ 폴리곤 최적화 성공! {totalReductionPercentage:F1}% 감축으로 성능 향상 달성</color>");
                }
                else
                {
                    Debug.LogWarning("⚠ 폴리곤 감축이 이루어지지 않았습니다. 최적화 설정을 확인해보세요.");
                }
            }
            
            if (errorMessages.Count > 0)
            {
                Debug.LogWarning("=== 오류 목록 ===");
                foreach (string errorMessage in errorMessages)
                {
                    Debug.LogWarning($"- {errorMessage}");
                }
            }
            
            float successRate = ((float)(processedCount - errorCount) / totalCount) * 100f;
            Debug.Log($"성공률: {successRate:F1}%");
            Debug.Log("=== 최적화 완료 ===");
        }
    }
}
