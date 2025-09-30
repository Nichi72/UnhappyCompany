#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace GSpawn_Lite
{
    public class ObjectSpawnPrefs : Prefs<ObjectSpawnPrefs>
    {
        [SerializeField][UIFieldConfig("Extension plane fill color", "The fill color used when drawing the extension plane.", "Segments Spawn", true)]
        private Color       _segmentsSpawnExtensionPlaneFillColor               = defaultSegmentsSpawnExtensionPlaneFillColor;
        [SerializeField][UIFieldConfig("Extension plane border color", "The border color used when drawing the extension plane.")]
        private Color       _segmentsSpawnExtensionPlaneBorderColor             = defaultSegmentsSpawnExtensionPlaneBorderColor;
        [SerializeField][Min(0.0f)][UIFieldConfig("Extension plane inflate amount", "Specifies how much the size of the extension plane is inflated.")]
        private float       _segmentsSpawnExtensionPlaneInflateAmount           = defaultSegmentsSpawnExtensionPlaneInflateAmount;
        [SerializeField][UIFieldConfig("Cell wire color", "The color which is used to draw the object spawn cells.")]
        private Color       _segmentsSpawnCellWireColor                         = defaultSegmentsSpawnCellWireColor;
        [SerializeField][UIFieldConfig("X axis color", "The color used to draw the box X axis.", "Box Spawn", true)]
        private Color       _boxSpawnXAxisColor                                 = defaultBoxSpawnXAxisColor;
        [SerializeField][UIFieldConfig("Y axis color", "The color used to draw the box Y axis. This is the height axis along which the box can be raised or lowered.")]
        private Color       _boxSpawnYAxisColor                                 = defaultBoxSpawnYAxisColor;
        [SerializeField][UIFieldConfig("Z axis color", "The color used to draw the box Z axis.")]
        private Color       _boxSpawnZAxisColor                                 = defaultBoxSpawnZAxisColor;
        [SerializeField][Min(0.0f)][UIFieldConfig("X axis length", "The length of the box X axis.")]
        private float       _boxSpawnXAxisLength                                = defaultBoxSpawnXAxisLength;
        [SerializeField][Min(0.0f)][UIFieldConfig("Y axis length", "The length of the box Y axis.")]
        private float       _boxSpawnYAxisLength                                = defaultBoxSpawnYAxisLength;
        [SerializeField][Min(0.0f)][UIFieldConfig("Z axis length", "The length of the box Z axis.")]
        private float       _boxSpawnZAxisLength                                = defaultBoxSpawnZAxisLength;
        [SerializeField][UIFieldConfig("Extension plane fill color", "The fill color used when drawing the extension plane.")]
        private Color       _boxSpawnExtensionPlaneFillColor                    = defaultBoxSpawnExtensionPlaneFillColor;
        [SerializeField][UIFieldConfig("Extension plane border color", "The border color used when drawing the extension plane.")]
        private Color       _boxSpawnExtensionPlaneBorderColor                  = defaultBoxSpawnExtensionPlaneBorderColor;
        [SerializeField][Min(0.0f)][UIFieldConfig("Extension plane inflate amount", "Specifies how much the size of the extension plane is inflated.")]
        private float       _boxSpawnExtensionPlaneInflateAmount                = defaultBoxSpawnExtensionPlaneInflateAmount;
        [SerializeField][UIFieldConfig("Cell wire color", "The color which is used to draw the object spawn cells.")]
        private Color       _boxSpawnCellWireColor                              = defaultBoxSpawnCellWireColor;
        [SerializeField][UIFieldConfig("Show info text", "If checked, the plugin will offer textual information during while using the box spawn tool.")]
        private bool        _boxSpawnShowInfoText                               = defaultBoxSpawnShowInfoText;
        [SerializeField][UIFieldConfig("Terrain flatten area color", "The color used when drawing the terrain flatten area indicator.", "Props Spawn", true)]
        private Color       _propsSpawnTerrainFlattenAreaColor                  = defaultPropsSpawnTerrainFlattenAreaColor;

        public Color        segmentsSpawnExtensionPlaneFillColor                { get { return _segmentsSpawnExtensionPlaneFillColor; } set { UndoEx.record(this); _segmentsSpawnExtensionPlaneFillColor = value; EditorUtility.SetDirty(this); } }
        public Color        segmentsSpawnExtensionPlaneBorderColor              { get { return _segmentsSpawnExtensionPlaneBorderColor; } set { UndoEx.record(this); _segmentsSpawnExtensionPlaneBorderColor = value; EditorUtility.SetDirty(this); } }
        public float        segmentsSpawnExtensionPlaneInflateAmount            { get { return _segmentsSpawnExtensionPlaneInflateAmount; } set { UndoEx.record(this); _segmentsSpawnExtensionPlaneInflateAmount = Mathf.Max(0.0f, value); EditorUtility.SetDirty(this); } }
        public Color        segmentsSpawnCellWireColor                          { get { return _segmentsSpawnCellWireColor; } set { UndoEx.record(this); _segmentsSpawnCellWireColor = value; EditorUtility.SetDirty(this); } }
        public Color        boxSpawnXAxisColor                                  { get { return _boxSpawnXAxisColor; } set { UndoEx.record(this); _boxSpawnXAxisColor = value; EditorUtility.SetDirty(this); } }
        public Color        boxSpawnYAxisColor                                  { get { return _boxSpawnYAxisColor; } set { UndoEx.record(this); _boxSpawnYAxisColor = value; EditorUtility.SetDirty(this); } }
        public Color        boxSpawnZAxisColor                                  { get { return _boxSpawnZAxisColor; } set { UndoEx.record(this); _boxSpawnZAxisColor = value; EditorUtility.SetDirty(this); } }
        public float        boxSpawnXAxisLength                                 { get { return _boxSpawnXAxisLength; } set { UndoEx.record(this); _boxSpawnXAxisLength = Mathf.Max(0.0f, value); EditorUtility.SetDirty(this); } }
        public float        boxSpawnYAxisLength                                 { get { return _boxSpawnYAxisLength; } set { UndoEx.record(this); _boxSpawnYAxisLength = Mathf.Max(0.0f, value); EditorUtility.SetDirty(this); } }
        public float        boxSpawnZAxisLength                                 { get { return _boxSpawnZAxisLength; } set { UndoEx.record(this); _boxSpawnZAxisLength = Mathf.Max(0.0f, value); EditorUtility.SetDirty(this); } }
        public Color        boxSpawnExtensionPlaneFillColor                     { get { return _boxSpawnExtensionPlaneFillColor; } set { UndoEx.record(this); _boxSpawnExtensionPlaneFillColor = value; EditorUtility.SetDirty(this); } }
        public Color        boxSpawnExtensionPlaneBorderColor                   { get { return _boxSpawnExtensionPlaneBorderColor; } set { UndoEx.record(this); _boxSpawnExtensionPlaneBorderColor = value; EditorUtility.SetDirty(this); } }
        public float        boxSpawnExtensionPlaneInflateAmount                 { get { return _boxSpawnExtensionPlaneInflateAmount; } set { UndoEx.record(this); _boxSpawnExtensionPlaneInflateAmount = Mathf.Max(0.0f, value); EditorUtility.SetDirty(this); } }
        public Color        boxSpawnCellWireColor                               { get { return _boxSpawnCellWireColor; } set { UndoEx.record(this); _boxSpawnCellWireColor = value; EditorUtility.SetDirty(this); } }
        public bool         boxSpawnShowInfoText                                { get { return _boxSpawnShowInfoText; } set { UndoEx.record(this); _boxSpawnShowInfoText = value; EditorUtility.SetDirty(this); } }
        public Color        propsSpawnTerrainFlattenAreaColor                   { get { return _propsSpawnTerrainFlattenAreaColor; } set { UndoEx.record(this); _propsSpawnTerrainFlattenAreaColor = value; EditorUtility.SetDirty(this); } }
        
        public static Color defaultSegmentsSpawnExtensionPlaneFillColor         { get { return Color.green.createNewAlpha(0.05f); } }
        public static Color defaultSegmentsSpawnExtensionPlaneBorderColor       { get { return Color.black; } }
        public static float defaultSegmentsSpawnExtensionPlaneInflateAmount     { get { return 2.0f; } }
        public static Color defaultSegmentsSpawnCellWireColor                   { get { return Color.white; } }
        public static Color defaultBoxSpawnXAxisColor                           { get { return DefaultSystemValues.xAxisColor; } }
        public static Color defaultBoxSpawnYAxisColor                           { get { return DefaultSystemValues.yAxisColor; } }
        public static Color defaultBoxSpawnZAxisColor                           { get { return DefaultSystemValues.zAxisColor; } }
        public static float defaultBoxSpawnXAxisLength                          { get { return 10.0f; } }
        public static float defaultBoxSpawnYAxisLength                          { get { return 10.0f; } }
        public static float defaultBoxSpawnZAxisLength                          { get { return 10.0f; } }
        public static Color defaultBoxSpawnExtensionPlaneFillColor              { get { return Color.green.createNewAlpha(0.05f); } }
        public static Color defaultBoxSpawnExtensionPlaneBorderColor            { get { return Color.black; } }
        public static float defaultBoxSpawnExtensionPlaneInflateAmount          { get { return 2.0f; } }
        public static Color defaultBoxSpawnCellWireColor                        { get { return Color.white; } }
        public static bool  defaultBoxSpawnShowInfoText                         { get { return true; } }
        public static Color defaultPropsSpawnTerrainFlattenAreaColor            { get { return Color.green; } }

        public override void useDefaults()
        {
            segmentsSpawnExtensionPlaneFillColor        = defaultSegmentsSpawnExtensionPlaneFillColor;
            segmentsSpawnExtensionPlaneBorderColor      = defaultSegmentsSpawnExtensionPlaneBorderColor;
            segmentsSpawnExtensionPlaneInflateAmount    = defaultSegmentsSpawnExtensionPlaneInflateAmount;
            segmentsSpawnCellWireColor                  = defaultSegmentsSpawnCellWireColor;
            boxSpawnXAxisColor                          = defaultBoxSpawnXAxisColor;
            boxSpawnYAxisColor                          = defaultBoxSpawnYAxisColor;
            boxSpawnZAxisColor                          = defaultBoxSpawnZAxisColor;
            boxSpawnXAxisLength                         = defaultBoxSpawnXAxisLength;
            boxSpawnYAxisLength                         = defaultBoxSpawnYAxisLength;
            boxSpawnZAxisLength                         = defaultBoxSpawnZAxisLength;
            boxSpawnExtensionPlaneFillColor             = defaultBoxSpawnExtensionPlaneFillColor;
            boxSpawnExtensionPlaneBorderColor           = defaultBoxSpawnExtensionPlaneBorderColor;
            boxSpawnExtensionPlaneInflateAmount         = defaultBoxSpawnExtensionPlaneInflateAmount;
            boxSpawnCellWireColor                       = defaultBoxSpawnCellWireColor;
            boxSpawnShowInfoText                        = defaultBoxSpawnShowInfoText;
            propsSpawnTerrainFlattenAreaColor           = defaultPropsSpawnTerrainFlattenAreaColor;

            EditorUtility.SetDirty(this);
        }
    }

    class ObjectSpawnPrefsProvider : SettingsProvider
    {
        public ObjectSpawnPrefsProvider(string path, SettingsScope scope)
            : base(path, scope) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            UI.createPrefsTitleLabel("Object Spawn", rootElement);
            ObjectSpawnPrefs.instance.buildDefaultUI(rootElement, PluginSettingsUIBuildConfig.defaultConfig);

            const float labelWidth = 265.0f;
            rootElement.Query<Label>().ForEach(item => item.setChildLabelWidth(labelWidth));
        }

        [SettingsProvider]
        public static SettingsProvider create()
        {
            if (GSpawn.active == null) return null;
            return new ObjectSpawnPrefsProvider("Preferences/" + GSpawn.pluginName + "/Object Spawn", SettingsScope.User);
        }
    }
}
#endif