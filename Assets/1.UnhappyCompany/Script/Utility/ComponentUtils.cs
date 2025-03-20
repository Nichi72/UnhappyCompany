using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
namespace MyUtility
{
    public static class ComponentUtils
    {
        /// <summary>
        /// 특정 게임 오브젝트의 부모들에서 특정 타입의 모든 컴포넌트를 찾는 메서드.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <param name="includeInactive">includeInactive가 true로 설정되면 비활성화된 객체도 검색에 포함됩니다.</param>
        /// <returns></returns>
        public static T[] GetAllComponentsInParents<T>(GameObject gameObject, bool includeInactive = false) where T : Component
        {
            return gameObject.GetComponentsInParent<T>(includeInactive);
        }

        /// <summary>
        /// 주어진 GameObject로부터 모든 부모들의 모든 컴포넌트를 찾는 메서드.
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

    public static class LocalizationUtils
    {
        public static string GetLocalizedString(string tableEntryReference ,string tableReference =null,Locale locale = null)
        {
            if(locale == null)
            {
                locale = LocalizationSettings.SelectedLocale;
            }
            if(tableReference == null)
            {
                tableReference = "TESTs";
            }
            return LocalizationSettings.StringDatabase.GetLocalizedString(tableReference: tableReference, tableEntryReference: tableEntryReference, locale: locale);
        }
    }
}


