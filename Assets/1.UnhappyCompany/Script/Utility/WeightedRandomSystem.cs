using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
namespace UnhappyCompany.Utility
{
    
/// <summary>
/// 가중치 기반 랜덤 선택 엔트리
/// </summary>
[System.Serializable]
    public class WeightedEntry<T>
    {
        [Tooltip("선택될 항목")]
        public T item;
        
        [Tooltip("가중치 (0~1, 총합이 1.0 권장)")]
        [Range(0f, 1f)]
        public float weight = 0.1f;

        public WeightedEntry() { }
        
        public WeightedEntry(T item, float weight)
        {
            this.item = item;
            this.weight = weight;
        }
    }

    /// <summary>
    /// 가중치 기반 랜덤 선택 시스템
    /// 다양한 용도로 사용 가능한 범용 확률 유틸리티
    /// </summary>
    [System.Serializable]
    public class WeightedRandomSystem<T>
    {
        [Tooltip("가중치 기반 항목 리스트")]
        public List<WeightedEntry<T>> entries = new List<WeightedEntry<T>>();

        public WeightedRandomSystem() { }

        public WeightedRandomSystem(List<WeightedEntry<T>> entries)
        {
            this.entries = entries;
        }

        /// <summary>
        /// 가중치 기반으로 하나의 항목 선택
        /// </summary>
        /// <param name="validateItem">항목 유효성 검증 함수 (null이면 모든 항목 허용)</param>
        /// <returns>선택된 항목 (없으면 default(T))</returns>
        public T SelectOne(Func<T, bool> validateItem = null)
        {
            if (entries == null || entries.Count == 0)
            {
                Debug.LogWarning("[WeightedRandomSystem] 항목 리스트가 비어있습니다!");
                return default(T);
            }

            // 유효한 항목만 필터링
            var validEntries = entries.Where(e => validateItem == null || validateItem(e.item)).ToList();
            
            if (validEntries.Count == 0)
            {
                Debug.LogWarning("[WeightedRandomSystem] 유효한 항목이 없습니다!");
                return default(T);
            }

            // 총 가중치 계산
            float totalWeight = validEntries.Sum(e => e.weight);
            
            if (totalWeight <= 0f)
            {
                Debug.LogWarning("[WeightedRandomSystem] 총 가중치가 0입니다!");
                return default(T);
            }

            // 랜덤 값 굴리기
            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            
            // 누적 가중치로 선택
            float cumulativeWeight = 0f;
            foreach (var entry in validEntries)
            {
                cumulativeWeight += entry.weight;
                
                if (randomValue <= cumulativeWeight)
                {
                    return entry.item;
                }
            }

            // 폴백: 마지막 유효 항목 반환
            return validEntries[validEntries.Count - 1].item;
        }

        /// <summary>
        /// 가중치 기반으로 여러 항목 선택 (중복 가능)
        /// </summary>
        /// <param name="count">선택할 개수</param>
        /// <param name="validateItem">항목 유효성 검증 함수</param>
        /// <returns>선택된 항목 리스트</returns>
        public List<T> SelectMultiple(int count, Func<T, bool> validateItem = null)
        {
            List<T> selectedItems = new List<T>();
            
            for (int i = 0; i < count; i++)
            {
                T selected = SelectOne(validateItem);
                if (!EqualityComparer<T>.Default.Equals(selected, default(T)))
                {
                    selectedItems.Add(selected);
                }
            }
            
            return selectedItems;
        }

        /// <summary>
        /// 가중치 기반으로 여러 항목 선택 (중복 없음)
        /// </summary>
        /// <param name="count">선택할 개수</param>
        /// <param name="validateItem">항목 유효성 검증 함수</param>
        /// <returns>선택된 항목 리스트</returns>
        public List<T> SelectMultipleUnique(int count, Func<T, bool> validateItem = null)
        {
            List<T> selectedItems = new List<T>();
            List<WeightedEntry<T>> remainingEntries = new List<WeightedEntry<T>>(entries);
            
            for (int i = 0; i < count && remainingEntries.Count > 0; i++)
            {
                // 임시 선택기로 하나 선택
                WeightedRandomSystem<T> tempSelector = new WeightedRandomSystem<T>(remainingEntries);
                T selected = tempSelector.SelectOne(validateItem);
                
                if (!EqualityComparer<T>.Default.Equals(selected, default(T)))
                {
                    selectedItems.Add(selected);
                    
                    // 선택된 항목을 리스트에서 제거
                    remainingEntries.RemoveAll(e => EqualityComparer<T>.Default.Equals(e.item, selected));
                }
                else
                {
                    break; // 더 이상 선택할 수 없음
                }
            }
            
            return selectedItems;
        }

        /// <summary>
        /// 총 가중치 계산
        /// </summary>
        /// <param name="validateItem">항목 유효성 검증 함수</param>
        /// <returns>총 가중치</returns>
        public float CalculateTotalWeight(Func<T, bool> validateItem = null)
        {
            if (entries == null || entries.Count == 0)
                return 0f;

            return entries
                .Where(e => validateItem == null || validateItem(e.item))
                .Sum(e => e.weight);
        }

        /// <summary>
        /// 각 항목의 실제 드랍 확률 계산 (백분율)
        /// </summary>
        /// <param name="validateItem">항목 유효성 검증 함수</param>
        /// <returns>항목별 확률 딕셔너리</returns>
        public Dictionary<T, float> CalculateDropChances(Func<T, bool> validateItem = null)
        {
            Dictionary<T, float> chances = new Dictionary<T, float>();
            
            float totalWeight = CalculateTotalWeight(validateItem);
            if (totalWeight <= 0f)
                return chances;

            foreach (var entry in entries)
            {
                if (validateItem == null || validateItem(entry.item))
                {
                    float percentage = (entry.weight / totalWeight) * 100f;
                    chances[entry.item] = percentage;
                }
            }

            return chances;
        }

        /// <summary>
        /// 가중치 상태 정보 가져오기
        /// </summary>
        /// <param name="validateItem">항목 유효성 검증 함수</param>
        /// <returns>상태 메시지</returns>
        public string GetWeightStatusMessage(Func<T, bool> validateItem = null)
        {
            float totalWeight = CalculateTotalWeight(validateItem);
            
            if (totalWeight <= 0f)
            {
                return "⚠️ 경고: 가중치가 설정되지 않았습니다!";
            }
            else if (totalWeight > 1f)
            {
                return $"⚠️ 경고: 총 가중치가 1.0을 초과합니다! ({totalWeight:F3})\n" +
                    $"정규화되어 처리됩니다.";
            }
            else if (totalWeight < 1f)
            {
                float nothingChance = (1f - totalWeight) * 100f;
                return $"✅ 정상 (총 가중치: {totalWeight:F3})\n" +
                    $"아무것도 선택 안 될 확률: {nothingChance:F1}%";
            }
            else
            {
                return $"✅ 완벽! (총 가중치: {totalWeight:F3})\n" +
                    "항상 항목이 선택됩니다.";
            }
        }
    }

    /// <summary>
    /// 가중치 기반 랜덤 선택 결과 정보
    /// </summary>
    public class WeightedSelectionResult<T>
    {
        public T selectedItem;
        public float weight;
        public float probability; // 실제 선택 확률 (백분율)
        public float rolledValue; // 굴린 랜덤 값

        public WeightedSelectionResult(T item, float weight, float probability, float rolled)
        {
            this.selectedItem = item;
            this.weight = weight;
            this.probability = probability;
            this.rolledValue = rolled;
        }
    }
}


