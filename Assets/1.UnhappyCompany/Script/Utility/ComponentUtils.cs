using System.Collections.Generic;
using UnityEngine;
namespace MyUtility
{
    public static class ComponentUtils
    {
        /// <summary>
        ///  /// <summary>
        /// 상위 계층에서 특정 타입의 모든 컴포넌트를 찾는 메서드.
        /// </summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <param name="includeInactive">includeInactive를 true로 설정하면 비활성화된 부모도 검색에 포함합니다.</param>
        /// <returns></returns>
        public static T[] GetAllComponentsInParents<T>(GameObject gameObject, bool includeInactive = false) where T : Component
        {
            return gameObject.GetComponentsInParent<T>(includeInactive);
        }

        /// <summary>
        /// 현재 GameObject에서 상위 계층까지 모든 부모의 모든 컴포넌트를 찾는 메서드.
        /// </summary>
        public static List<Component> GetAllComponentsInParents(GameObject gameObject)
        {
            List<Component> allComponents = new List<Component>();
            Transform current = gameObject.transform.parent;

            while (current != null)
            {
                Component[] components = current.GetComponents<Component>();
                allComponents.AddRange(components);
                current = current.parent;
            }

            return allComponents;
        }
    }
}
