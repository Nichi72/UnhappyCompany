#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace GSpawn_Lite
{
    public enum ObjectSpawnGuideSettingsUsage
    {
        PropsSpawn = 0,
        Other
    }

    public class ObjectSpawnGuideSettings : PluginSettings<ObjectSpawnGuideSettings>
    {
        [SerializeField]
        private TransformRandomizationSettings  _transformRandomizationSettings;
        [SerializeField]
        private bool                            _randomizePrefab                = defaultRandomizePrefab;
        [SerializeField]
        private string                          _randomPrefabProfileName        = defaultRandomPrefabProfileName;

        public TransformRandomizationSettings   transformRandomizationSettings  { get { if (_transformRandomizationSettings == null) _transformRandomizationSettings = ScriptableObject.CreateInstance<TransformRandomizationSettings>(); return _transformRandomizationSettings; } }
        public bool                             randomizePrefab                 { get { return _randomizePrefab; } set { UndoEx.record(this); _randomizePrefab = value; EditorUtility.SetDirty(this); } }
        public RandomPrefabProfile              randomPrefabProfile
        {
            get
            {
                var profile = RandomPrefabProfileDb.instance.findProfile(_randomPrefabProfileName);
                if (profile == null) profile = RandomPrefabProfileDb.instance.defaultProfile;
                return profile;
            }
        }

        public static bool                      defaultRandomizePrefab          { get { return false; } }
        public static string                    defaultRandomPrefabProfileName  { get { return RandomPrefabProfileDb.defaultProfileName; } }
        public static string                    randomizePrefabPropertyName     { get { return "_randomizePrefab"; } }

        public void buildUI(VisualElement parent, ObjectSpawnGuideSettingsUsage usage)
        {
            float labelWidth = 130.0f;
   
            transformRandomizationSettings.buildUI(parent, labelWidth);
            var randomizePrefabToggle = UI.createToggle(randomizePrefabPropertyName, serializedObject, "Randomize prefab", "If checked, the spawn guide's prefab will be picked randomly from the random prefab profile every time a new object is spawned.", parent);
            randomizePrefabToggle.setChildLabelWidth(labelWidth);

            IMGUIContainer imGUIContainer = UI.createIMGUIContainer(parent);
            imGUIContainer.onGUIHandler = () =>
            {
                if (randomizePrefab)
                {
                    string newName = EditorUIEx.profileNameSelectionField<RandomPrefabProfileDb, RandomPrefabProfile>
                    (RandomPrefabProfileDb.instance, "Random prefab profile", labelWidth, _randomPrefabProfileName);
                    if (newName != _randomPrefabProfileName)
                    {
                        UndoEx.record(this);
                        _randomPrefabProfileName = newName;
                        EditorUtility.SetDirty(this);
                    }
                }
            };

            UI.createUseDefaultsButton(() => useDefaults(), parent);
        }

        public override void useDefaults()
        {
            transformRandomizationSettings.useDefaults();
            randomizePrefab             = defaultRandomizePrefab;
            _randomPrefabProfileName    = defaultRandomPrefabProfileName;

            EditorUtility.SetDirty(this);
        }

        protected override void onDestroy()
        {
            ScriptableObjectEx.destroyImmediate(_transformRandomizationSettings);
        }
    }
}
#endif