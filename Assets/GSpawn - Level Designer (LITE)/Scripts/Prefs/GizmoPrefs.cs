#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace GSpawn_Lite
{
    public class GizmoPrefs : Prefs<GizmoPrefs>
    {
        [SerializeField][UIFieldConfig("Wire color", "The wire color used to draw the extrude gizmo box.", "Extrude Gizmo", false)]
        private Color       _extrudeWireColor               = defaultExtrudeWireColor;
        [SerializeField][UIFieldConfig("X axis color", "The color of the X axis handles.")]
        private Color       _extrudeXAxisColor              = defaultExtrudeXAxisColor;
        [SerializeField][UIFieldConfig("Y axis color", "The color of the Y axis handles.")]
        private Color       _extrudeYAxisColor              = defaultExtrudeYAxisColor;
        [SerializeField][UIFieldConfig("Z axis color", "The color of the Z axis handles.")]
        private Color       _extrudeZAxisColor              = defaultExtrudeZAxisColor;
        [SerializeField][UIFieldConfig("Sgl-axis handle size", "The size of the single-axis handles.")][Min(1e-4f)]
        private float       _extrudeSglAxisSize             = defaultExtrudeSglAxisSize;
        [SerializeField][UIFieldConfig("Extrude cell wire color", "The wire color used to draw the extrude cells.")]
        private Color       _extrudeCellWireColor           = defaultExtrudeCellWireColor;
        [SerializeField][UIFieldConfig("Extrude cell fill color", "The fill color used to draw the extrude cells.")]
        private Color       _extrudeCellFillColor           = defaultExtrudeCellFillColor;
        [SerializeField][UIFieldConfig("Dbl-axis tick size", "The size of the double-axis ticks.")][Min(1e-4f)]
        private float       _extrudeDblAxisSize             = defaultExtrudeDblAxisSize;
        [SerializeField][UIFieldConfig("Show info text", "If checked, the plugin will offer textual information during while using the extrude gizmo.")]
        private bool        _extrudeShowInfoText            = defaultExtrudeShowInfoText;
        [SerializeField][UIFieldConfig("Select spawned", "If this is checked, all objects which were spawned as a result of the extrusion, will be selected.")]
        private bool        _extrudeSelectSpawned           = defaultExtrudeSelectSpawned;

        public Color        extrudeWireColor                            { get { return _extrudeWireColor; } set { UndoEx.record(this); _extrudeWireColor = value; EditorUtility.SetDirty(this); } }
        public Color        extrudeXAxisColor                           { get { return _extrudeXAxisColor; } set { UndoEx.record(this); _extrudeXAxisColor = value; EditorUtility.SetDirty(this); } }
        public Color        extrudeYAxisColor                           { get { return _extrudeYAxisColor; } set { UndoEx.record(this); _extrudeYAxisColor = value; EditorUtility.SetDirty(this); } }
        public Color        extrudeZAxisColor                           { get { return _extrudeZAxisColor; } set { UndoEx.record(this); _extrudeZAxisColor = value; EditorUtility.SetDirty(this); } }
        public float        extrudeSglHandleSize                        { get { return _extrudeSglAxisSize; } set { UndoEx.record(this); _extrudeSglAxisSize = Mathf.Max(1e-4f, value); EditorUtility.SetDirty(this); } }
        public Color        extrudeCellWireColor                        { get { return _extrudeCellWireColor; } set { UndoEx.record(this); _extrudeCellWireColor = value; EditorUtility.SetDirty(this); } }
        public Color        extrudeCellFillColor                        { get { return _extrudeCellFillColor; } set { UndoEx.record(this); _extrudeCellFillColor = value; EditorUtility.SetDirty(this); } }
        public float        extrudeDblAxisSize                          { get { return _extrudeDblAxisSize; } set { UndoEx.record(this); _extrudeDblAxisSize = Mathf.Max(1e-4f, value); EditorUtility.SetDirty(this); } }
        public bool         extrudeShowInfoText                         { get { return _extrudeShowInfoText; } set { UndoEx.record(this); _extrudeShowInfoText = value; EditorUtility.SetDirty(this); } }
        public bool         extrudeSelectSpawned                        { get { return _extrudeSelectSpawned; } set { UndoEx.record(this); _extrudeSelectSpawned = value; EditorUtility.SetDirty(this); } }

        public static Color defaultExtrudeWireColor                     { get { return Color.white; } }
        public static Color defaultExtrudeXAxisColor                    { get { return DefaultSystemValues.xAxisColor; } }
        public static Color defaultExtrudeYAxisColor                    { get { return DefaultSystemValues.yAxisColor; } }
        public static Color defaultExtrudeZAxisColor                    { get { return DefaultSystemValues.zAxisColor; } }
        public static float defaultExtrudeSglAxisSize                   { get { return 0.18f; } }
        public static Color defaultExtrudeCellWireColor                 { get { return Color.white; } }
        public static Color defaultExtrudeCellFillColor                 { get { return Color.gray.createNewAlpha(0.0f); } }
        public static float defaultExtrudeDblAxisSize                   { get { return DefaultSystemValues.tickSize; } }
        public static bool  defaultExtrudeShowInfoText                  { get { return true; } }
        public static bool  defaultExtrudeSelectSpawned                 { get { return true; } }

        public override void useDefaults()
        {
            extrudeWireColor                    = defaultExtrudeWireColor;
            extrudeXAxisColor                   = defaultExtrudeXAxisColor;
            extrudeYAxisColor                   = defaultExtrudeYAxisColor;
            extrudeZAxisColor                   = defaultExtrudeZAxisColor;
            extrudeSglHandleSize                = defaultExtrudeSglAxisSize;
            extrudeCellWireColor                = defaultExtrudeCellWireColor;
            extrudeCellFillColor                = defaultExtrudeCellFillColor;
            extrudeDblAxisSize                  = defaultExtrudeDblAxisSize;
            extrudeShowInfoText                 = defaultExtrudeShowInfoText;
            extrudeSelectSpawned                = defaultExtrudeSelectSpawned;

            EditorUtility.SetDirty(this);
        }
    }

    class GizmoPrefsProvider : SettingsProvider
    {
        public GizmoPrefsProvider(string path, SettingsScope scope)
            : base(path, scope) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            UI.createPrefsTitleLabel("Gizmos", rootElement);
            var uiBuildConfig = PluginSettingsUIBuildConfig.defaultConfig;
            uiBuildConfig.onUseDefaults += () => { PluginInspectorUI.instance.refresh(); };

            GizmoPrefs.instance.buildDefaultUI(rootElement, uiBuildConfig);

            const float labelWidth = 200.0f;
            rootElement.Query<Label>().ForEach(item => item.setChildLabelWidth(labelWidth));

            // Register additional callbacks to repaint inspector
            rootElement.Query<ColorField>("_mirrorXYPlaneColor").ForEach(item => item.RegisterValueChangedCallback(p => { PluginInspectorUI.instance.refresh(); }));
            rootElement.Query<ColorField>("_mirrorYZPlaneColor").ForEach(item => item.RegisterValueChangedCallback(p => { PluginInspectorUI.instance.refresh(); }));
            rootElement.Query<ColorField>("_mirrorZXPlaneColor").ForEach(item => item.RegisterValueChangedCallback(p => { PluginInspectorUI.instance.refresh(); }));
        }

        [SettingsProvider]
        public static SettingsProvider create()
        {
            if (GSpawn.active == null) return null;
            return new GizmoPrefsProvider("Preferences/" + GSpawn.pluginName + "/Gizmos", SettingsScope.User);
        }
    }
}
#endif