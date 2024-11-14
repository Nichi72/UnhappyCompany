using System.Collections.Generic;
using UnityEngine;
namespace MyUtility
{
    public static class ComponentUtils
    {
        /// <summary>
        ///  /// <summary>
        /// ���� �������� Ư�� Ÿ���� ��� ������Ʈ�� ã�� �޼���.
        /// </summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <param name="includeInactive">includeInactive�� true�� �����ϸ� ��Ȱ��ȭ�� �θ� �˻��� �����մϴ�.</param>
        /// <returns></returns>
        public static T[] GetAllComponentsInParents<T>(GameObject gameObject, bool includeInactive = false) where T : Component
        {
            return gameObject.GetComponentsInParent<T>(includeInactive);
        }

        /// <summary>
        /// ���� GameObject���� ���� �������� ��� �θ��� ��� ������Ʈ�� ã�� �޼���.
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
