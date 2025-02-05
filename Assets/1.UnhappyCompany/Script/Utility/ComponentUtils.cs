using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
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


