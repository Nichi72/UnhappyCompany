﻿#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;

namespace GSpawn_Lite
{
    public class XYZMeasureUI : VisualElement
    {
        FloatField  _xField;
        FloatField  _yField;
        FloatField  _zField;
        Button      _actionButton;

        public FloatField   xField          { get { return _xField; } }
        public FloatField   yField          { get { return _yField; } }
        public FloatField   zField          { get { return _zField; } }
        public Button       actionButton    { get { return _actionButton; } }

        public void init(VisualElement parent, Vector3 value, string actionName, Func<Vector3> measureAction)
        {
            style.flexDirection = FlexDirection.Row;
            parent.Add(this);
            {
                _actionButton = new Button();
                _actionButton.text = actionName;
                _actionButton.style.marginRight = -2.0f;
                Add(_actionButton);
                _actionButton.clicked += () =>
                {
                    Vector3 size = measureAction();
                    _xField.value = size.x;
                    _yField.value = size.y;
                    _zField.value = size.z;
                };

                _xField = new FloatField();
                _yField = new FloatField();
                _zField = new FloatField();

                _xField.style.width = 40.0f;
                _yField.style.width = 40.0f;
                _zField.style.width = 40.0f;

                _xField.setTextColor(DefaultSystemValues.xAxisColor);
                _yField.setTextColor(DefaultSystemValues.yAxisColor);
                _zField.setTextColor(DefaultSystemValues.zAxisColor);

                _xField.value = value.x;
                _yField.value = value.y;
                _zField.value = value.z;

                Add(_xField);
                Add(_yField);
                Add(_zField);
            }
        }
    }

    public class ObjectSelectionUI : PluginUI
    {
        private enum ViewId
        {
            Settings = 0,
            TransformTools,
            Misc
        }

        private ToolbarButton   _moveGimzoBtn;
        private ToolbarButton   _rotationGizmoBtn;
        private ToolbarButton   _scaleGizmoBtn;
        private ToolbarButton   _universalGizmoBtn;
        private ToolbarButton   _extrudeGizmoBtn;

        private ToolbarButton   _gizmoPivotBtn;
        private ToolbarButton   _gizmoTransformSpaceBtn;

        private ToolbarButton   _selectionRectBtn;
        private ToolbarButton   _selectionSegmentsBtn;
        private ToolbarButton   _selectionBoxBtn;

        private ToolbarButton   _settingsBtn;
        private ToolbarButton   _transformToolsBtn;
        private ToolbarButton   _miscBtn;

        private Button          _projectOnGridBtn;
        private Button          _projectOnObjectBtn;

        private XYZMeasureUI    _boundsMeasureUI;
        private XYZMeasureUI    _2BOverlapMeasureUI;
        [SerializeField]
        private Vector3         _boundsMeasureResult = Vector3.zero;
        [SerializeField]
        private Vector3         _2BOverlapMeasureResult = Vector3.zero;

        [SerializeField]
        private ViewId              _activeViewId       = ViewId.Settings;
        [SerializeField]
        private UISection           _transformSection;
        [SerializeField]
        private ObjectTransformUI   _transformUI;

        [SerializeField]
        private UISection   _selectionSettingsSection;
        [SerializeField]
        private UISection   _extrudeGizmoSettingsSection;
        [SerializeField]
        private UISection   _modularSnapSettingsSection;
        [SerializeField]
        private UISection   _surfaceSnapSettingsSection;
        [SerializeField]
        private UISection   _projectionSettingsSection;
        [SerializeField]
        private UISection   _vertexSnapSettingsSection;
        [SerializeField]
        private UISection   _boxSnapSettingsSection;
        [SerializeField]
        private UISection   _selectionGrowSettingsSection;
        [SerializeField]
        private UISection   _measureToolsSection;

        private UISection           transformSection                        { get { if (_transformSection == null) _transformSection = ScriptableObject.CreateInstance<UISection>(); return _transformSection; } }
        private ObjectTransformUI   transformUI                             { get { if (_transformUI == null) _transformUI = ScriptableObject.CreateInstance<ObjectTransformUI>(); return _transformUI; } }
        private UISection           selectionSettingsSection                { get { if (_selectionSettingsSection == null) _selectionSettingsSection = ScriptableObject.CreateInstance<UISection>(); return _selectionSettingsSection; } }
        private UISection           extrudeGizmoSettingsSection             { get { if (_extrudeGizmoSettingsSection == null) _extrudeGizmoSettingsSection = ScriptableObject.CreateInstance<UISection>(); return _extrudeGizmoSettingsSection; } }
        private UISection           modularSnapSettingsSection              { get { if (_modularSnapSettingsSection == null) _modularSnapSettingsSection = ScriptableObject.CreateInstance<UISection>(); return _modularSnapSettingsSection; } }
        private UISection           surfaceSnapSettingsSection              { get { if (_surfaceSnapSettingsSection == null) _surfaceSnapSettingsSection = ScriptableObject.CreateInstance<UISection>(); return _surfaceSnapSettingsSection; } }
        private UISection           projectionSettingsSection               { get { if (_projectionSettingsSection == null) _projectionSettingsSection = ScriptableObject.CreateInstance<UISection>(); return _projectionSettingsSection; } }
        private UISection           vertexSnapSettingsSection               { get { if (_vertexSnapSettingsSection == null) _vertexSnapSettingsSection = ScriptableObject.CreateInstance<UISection>(); return _vertexSnapSettingsSection; } }
        private UISection           boxSnapSettingsSection                  { get { if (_boxSnapSettingsSection == null) _boxSnapSettingsSection = ScriptableObject.CreateInstance<UISection>(); return _boxSnapSettingsSection; } }
        private UISection           selectionGrowSettingsSection            { get { if (_selectionGrowSettingsSection == null) _selectionGrowSettingsSection = ScriptableObject.CreateInstance<UISection>(); return _selectionGrowSettingsSection; } }
        private UISection           measureToolsSection                     { get { if (_measureToolsSection == null) _measureToolsSection = ScriptableObject.CreateInstance<UISection>(); return _measureToolsSection; } }

        private static string       uiSectionRowSeparator_Transform         { get { return "A"; } }
        private static string       uiSectionRowSeparator_Settings          { get { return "B"; } }
        private static string       uiSectionRowSeparator_TransformTools    { get { return "C"; } }
        private static string       uiSectionRowSeparator_Misc              { get { return "D"; } }

        public ObjectTransformUI    objectTransformUI                       { get { return transformUI; } }

        public static ObjectSelectionUI instance                            { get { return GSpawn.active.objectSelectionUI; } }

        public void setObjectTransformUIEnabled(bool enabled)
        {
            if (ready)
            {
                transformSection.contentContainer.SetEnabled(enabled);
            }
        }

        public void refreshObjectTransformUI()
        {
            if (ready)
            {
                transformUI.refresh();
                transformSection.setTitle(getTransformUISectionTitle());
            }
        }

        protected override void onRefresh()
        {
            refreshGizmoSelectionButtons();
            refreshGizmoPivotButton();
            refreshGizmoTransformSpaceButton();
            refreshObjectSelectShapeButtons();
            refreshViewSelectionButtons();
            refreshTooltips();
            transformUI.refreshTooltips();
            updateVisibility();
        }

        protected override void onBuild()
        {
            var toolbarContainer                    = new VisualElement();
            toolbarContainer.style.flexDirection    = FlexDirection.Row;
            contentContainer.Add(toolbarContainer);

            Toolbar toolbar                         = UI.createStylizedToolbar(toolbarContainer);
            toolbar.style.height                    = UIValues.mediumToolbarButtonSize + 2.0f;
            toolbar.style.borderRightColor          = UIValues.toolbarBorderColor;
            toolbar.style.borderRightWidth          = 1.0f;
            toolbar.style.borderBottomWidth         = 1.0f;
            toolbar.style.borderBottomColor         = UIValues.toolbarBorderColor;

            _moveGimzoBtn                           = UI.createSmallToolSelectionToolbarButton(TexturePool.instance.moveGizmo, "", toolbar);
            _moveGimzoBtn.clicked                   += () =>
            {
                ObjectSelectionGizmos.instance.setGizmoEnabled(ObjectSelectionGizmoId.Move, true, true);
                SceneViewEx.focus();
            };

            _rotationGizmoBtn                       = UI.createSmallToolSelectionToolbarButton(TexturePool.instance.rotationGizmo, "", toolbar);
            _rotationGizmoBtn.clicked               += () =>
            {
                ObjectSelectionGizmos.instance.setGizmoEnabled(ObjectSelectionGizmoId.Rotate, true, true);
                SceneViewEx.focus();
            };

            _scaleGizmoBtn                          = UI.createSmallToolSelectionToolbarButton(TexturePool.instance.scaleGizmo, "", toolbar);
            _scaleGizmoBtn.clicked                  += () =>
            {
                ObjectSelectionGizmos.instance.setGizmoEnabled(ObjectSelectionGizmoId.Scale, true, true);
                SceneViewEx.focus();
            };

            _universalGizmoBtn                      = UI.createSmallToolSelectionToolbarButton(TexturePool.instance.universalGizmo, "", toolbar);
            _universalGizmoBtn.clicked              += () =>
            {
                ObjectSelectionGizmos.instance.setGizmoEnabled(ObjectSelectionGizmoId.Universal, true, true);
                SceneViewEx.focus();
            };

            _extrudeGizmoBtn                        = UI.createSmallToolSelectionToolbarButton(TexturePool.instance.extrudeGizmo, "", toolbar);
            _extrudeGizmoBtn.clicked                += () =>
            {
                ObjectSelectionGizmos.instance.setGizmoEnabled(ObjectSelectionGizmoId.Extrude, true, true);
                SceneViewEx.focus();
            };

            refreshGizmoSelectionButtons();

            toolbar                                 = UI.createStylizedToolbar(toolbarContainer);
            toolbar.style.height                    = UIValues.mediumToolbarButtonSize + 2.0f;
            toolbar.style.flexGrow                  = 1.0f;
            toolbar.style.borderBottomWidth         = 1.0f;
            toolbar.style.borderBottomColor         = UIValues.toolbarBorderColor;

            _gizmoPivotBtn                          = UI.createSmallToolSelectionToolbarButton(TexturePool.instance.gizmoCenterPivot, "", toolbar);
            _gizmoPivotBtn.clicked                  += () => 
            { 
                ObjectSelectionGizmos.instance.transformPivot = ObjectSelectionGizmos.instance.transformPivot == ObjectGizmoTransformPivot.Center ? ObjectGizmoTransformPivot.Mesh : ObjectGizmoTransformPivot.Center; 
                SceneViewEx.focus();
            };
            _gizmoTransformSpaceBtn                 = UI.createSmallToolSelectionToolbarButton(TexturePool.instance.gizmoLocalTransformSpace, "", toolbar);;
            _gizmoTransformSpaceBtn.clicked         += () => 
            { 
                ObjectSelectionGizmos.instance.transformSpace = ObjectSelectionGizmos.instance.transformSpace == ObjectGizmoTransformSpace.Global ? ObjectGizmoTransformSpace.Local : ObjectGizmoTransformSpace.Global; 
                SceneViewEx.focus();
            };

            toolbar                                 = UI.createToolSelectionToolbar(contentContainer);
            _selectionRectBtn                       = UI.createMediumToolSelectionToolbarButton(TexturePool.instance.selectionRect, "", toolbar);
            _selectionRectBtn.style.marginTop       = 1.0f;
            _selectionRectBtn.clicked               += () => 
            { 
                ObjectSelection.instance.selectionShapeType = ObjectSelectionShape.Type.Rect;
                SceneViewEx.focus();
            };

            _selectionSegmentsBtn                   = UI.createMediumToolSelectionToolbarButton(TexturePool.instance.selectionSegments, "", toolbar);
            _selectionSegmentsBtn.style.marginTop   = 1.0f;
            _selectionSegmentsBtn.clicked           += () => 
            { 
                ObjectSelection.instance.selectionShapeType = ObjectSelectionShape.Type.Segments;
                SceneViewEx.focus();
            };

            _selectionBoxBtn                        = UI.createMediumToolSelectionToolbarButton(TexturePool.instance.selectionBox, "", toolbar);
            _selectionBoxBtn.style.marginTop        = 1.0f;
            _selectionBoxBtn.clicked                += () => 
            { 
                ObjectSelection.instance.selectionShapeType = ObjectSelectionShape.Type.Box;
                SceneViewEx.focus();
            };

            refreshGizmoPivotButton();
            refreshGizmoTransformSpaceButton();
            refreshObjectSelectShapeButtons();

            toolbar                                 = UI.createToolSelectionToolbar(contentContainer);
            _settingsBtn                            = UI.createToolbarButton(TexturePool.instance.settings, UI.ButtonStyle.Push, UIValues.mediumToolbarButtonSize, toolbar);
            UI.useDefaultMargins(_settingsBtn);
            _settingsBtn.style.marginTop            = 1.0f;
            _settingsBtn.tooltip                    = "Settings";
            _settingsBtn.clicked                    += () =>
            {
                _activeViewId = ViewId.Settings;
                updateVisibility();
                refreshViewSelectionButtons();
                SceneViewEx.focus();
            };

            _transformToolsBtn                      = UI.createToolbarButton(TexturePool.instance.transform, UI.ButtonStyle.Push, UIValues.mediumToolbarButtonSize, toolbar);
            UI.useDefaultMargins(_transformToolsBtn);
            _transformToolsBtn.style.marginTop      = 1.0f;
            _transformToolsBtn.tooltip              = "Transform tools";
            _transformToolsBtn.clicked              += () =>
            {
                _activeViewId = ViewId.TransformTools;
                updateVisibility();
                refreshViewSelectionButtons();
                SceneViewEx.focus();
            };

            _miscBtn                                = UI.createToolbarButton(TexturePool.instance.misc, UI.ButtonStyle.Push, UIValues.mediumToolbarButtonSize, toolbar);
            UI.useDefaultMargins(_miscBtn);
            _miscBtn.tooltip                        = "Misc";
            _miscBtn.clicked                        += () =>
            {
                _activeViewId = ViewId.Misc;
                updateVisibility();
                refreshViewSelectionButtons();
                SceneViewEx.focus();
            };
       
            transformSection.build(getTransformUISectionTitle(), TexturePool.instance.transform, true, contentContainer);
            transformUI.build(ObjectSelection.instance.objectCollection, transformSection.contentContainer);
            transformUI.visibilityCondition = () => { return uiVisibleAndReady; };
      
            UI.createUISectionRowSeparator(contentContainer, uiSectionRowSeparator_Settings);
            selectionSettingsSection.build("Selection", TexturePool.instance.settings, true, contentContainer);
            ObjectSelection.instance.settings.buildUI(selectionSettingsSection.contentContainer);            

            UI.createUISectionRowSeparator(contentContainer, uiSectionRowSeparator_Settings);
            extrudeGizmoSettingsSection.build("Extrude Gizmo", TexturePool.instance.extrudeGizmo, true, contentContainer);
            ObjectSelectionGizmos.instance.extrudeGizmoSettings.buildUI(extrudeGizmoSettingsSection.contentContainer);

            UI.createUISectionRowSeparator(contentContainer, uiSectionRowSeparator_TransformTools);
            modularSnapSettingsSection.build("Modular Snap", TexturePool.instance.modularSnapSpawn, true, contentContainer);
            ObjectSelection.instance.modularSnapSettings.buildUI(modularSnapSettingsSection.contentContainer);

            UI.createUISectionRowSeparator(contentContainer, uiSectionRowSeparator_TransformTools);
            surfaceSnapSettingsSection.build("Surface Snap", TexturePool.instance.objectSurfaceSnap, true, contentContainer);
            ObjectSelection.instance.surfaceSnapSettings.buildUI(surfaceSnapSettingsSection.contentContainer);

            UI.createUISectionRowSeparator(contentContainer, uiSectionRowSeparator_TransformTools);
            projectionSettingsSection.build("Projection", TexturePool.instance.projection, true, contentContainer);
            ObjectSelection.instance.projectionSettings.buildUI(projectionSettingsSection.contentContainer);

            VisualElement buttonsContainer          = new VisualElement();
            buttonsContainer.style.flexDirection    = FlexDirection.Row;
            projectionSettingsSection.contentContainer.Add(buttonsContainer);

            _projectOnGridBtn                       = new Button();
            _projectOnGridBtn.text                  = "Project on grid";
            _projectOnGridBtn.style.width           = UIValues.useDefaultsButtonWidth;
            _projectOnGridBtn.clicked               += () => { ObjectSelection.instance.projectOnGrid(PluginScene.instance.grid); };
            buttonsContainer.Add(_projectOnGridBtn);

            _projectOnObjectBtn                     = new Button();
            _projectOnObjectBtn.text                = "Project on object";            
            _projectOnObjectBtn.style.width         = 110.0f;
            _projectOnObjectBtn.style.marginLeft    = UIValues.actionButtonLeftMargin;
            _projectOnObjectBtn.clicked             += () => { ObjectSelection.instance.beginTransformSession(ObjectTransformSessionType.Projection); };
            buttonsContainer.Add(_projectOnObjectBtn);

            UI.createUISectionRowSeparator(contentContainer, uiSectionRowSeparator_TransformTools);
            vertexSnapSettingsSection.build("Vertex Snap", TexturePool.instance.vertexSnap, true, contentContainer);
            ObjectSelection.instance.vertexSnapSettings.buildUI(vertexSnapSettingsSection.contentContainer);

            UI.createUISectionRowSeparator(contentContainer, uiSectionRowSeparator_TransformTools);
            boxSnapSettingsSection.build("Box Snap", TexturePool.instance.boxSnap, true, contentContainer);
            ObjectSelection.instance.boxSnapSettings.buildUI(boxSnapSettingsSection.contentContainer);

            UI.createUISectionRowSeparator(contentContainer, uiSectionRowSeparator_Misc);
            selectionGrowSettingsSection.build("Selection Grow", TexturePool.instance.selectionGrow, true, contentContainer);
            ObjectSelection.instance.growSettings.buildUI(selectionGrowSettingsSection.contentContainer);

            createMeasureSection();

            var growBtn                 = new Button();
            selectionGrowSettingsSection.contentContainer.Add(growBtn);
            growBtn.text                = "Grow";
            growBtn.style.width         = UIValues.useDefaultsButtonWidth;
            growBtn.tooltip             = "Grow selection" + ShortcutProfileDb.instance.activeProfile.getShortcutUITooltip(ObjectSelectionShortcutNames.grow);
            growBtn.clicked             += () => 
            {
                ObjectSelection.instance.grow();
            };

            updateVisibility();
        }

        private void createMeasureSection()
        {
            UI.createUISectionRowSeparator(contentContainer, uiSectionRowSeparator_Misc);
            measureToolsSection.build("Measure Tools", TexturePool.instance.ruler, true, contentContainer);

            const float btnWidth = 90.0f;

            _boundsMeasureUI = new XYZMeasureUI();
            _boundsMeasureUI.init(measureToolsSection.contentContainer, _boundsMeasureResult, "Bounds", () => 
            { return ObjectSelection.instance.calcAABB().size; });
            _boundsMeasureUI.actionButton.style.width = btnWidth;
            _boundsMeasureUI.actionButton.tooltip = "Measure the object selection bounds.";

            _boundsMeasureUI.xField.RegisterValueChangedCallback((e) => { _boundsMeasureResult.x = e.newValue; EditorUtility.SetDirty(this); });
            _boundsMeasureUI.yField.RegisterValueChangedCallback((e) => { _boundsMeasureResult.y = e.newValue; EditorUtility.SetDirty(this); });
            _boundsMeasureUI.zField.RegisterValueChangedCallback((e) => { _boundsMeasureResult.z = e.newValue; EditorUtility.SetDirty(this); });

            _2BOverlapMeasureUI = new XYZMeasureUI();
            _2BOverlapMeasureUI.init(measureToolsSection.contentContainer, _2BOverlapMeasureResult, "2B. Overlap", () => 
            { return ObjectSelection.instance.calc2BOverlapSize(); });
            _2BOverlapMeasureUI.actionButton.style.width = btnWidth;
            _2BOverlapMeasureUI.actionButton.tooltip = "Measure the bounds overlap of 2 selected objects. Only 2 objects have to be selected for this to work.";

            _2BOverlapMeasureUI.xField.RegisterValueChangedCallback((e) => { _2BOverlapMeasureResult.x = e.newValue; EditorUtility.SetDirty(this); });
            _2BOverlapMeasureUI.yField.RegisterValueChangedCallback((e) => { _2BOverlapMeasureResult.y = e.newValue; EditorUtility.SetDirty(this); });
            _2BOverlapMeasureUI.zField.RegisterValueChangedCallback((e) => { _2BOverlapMeasureResult.z = e.newValue; EditorUtility.SetDirty(this); });
        }

        private void refreshTooltips()
        {
            _projectOnGridBtn.tooltip       = ShortcutProfileDb.instance.activeProfile.getShortcutUITooltip(ObjectSelectionShortcutNames.projectOnGrid, "Project selected objects on the scene grid.");
            _projectOnObjectBtn.tooltip     = ShortcutProfileDb.instance.activeProfile.getShortcutUITooltip(ObjectSelectionShortcutNames.projectOnObject, "Project selected objects on an object that you pick in the scene.");
        }

        private string getTransformUISectionTitle()
        {
            if (ObjectSelection.instance.numSelectedObjects == 1)
                return "Transform (" + ObjectSelection.instance.numSelectedObjects + ") - " + ObjectSelection.instance.getSelectedObject(0).name;
            else 
                return ObjectSelection.instance.numSelectedObjects != 0 ? "Transform (" + ObjectSelection.instance.numSelectedObjects + ")" : "Transform";
        }

        private void refreshGizmoSelectionButtons()
        {
            _moveGimzoBtn.tooltip = "Move" + ShortcutProfileDb.instance.activeProfile.getShortcutUITooltip(ObjectSelectionShortcutNames.moveGizmo);
            _moveGimzoBtn.style.unityBackgroundImageTintColor = UIValues.inactiveButtonTintColor;

            _rotationGizmoBtn.tooltip = "Rotate" + ShortcutProfileDb.instance.activeProfile.getShortcutUITooltip(ObjectSelectionShortcutNames.rotationGizmo);
            _rotationGizmoBtn.style.unityBackgroundImageTintColor = UIValues.inactiveButtonTintColor;

            _scaleGizmoBtn.tooltip = "Scale" + ShortcutProfileDb.instance.activeProfile.getShortcutUITooltip(ObjectSelectionShortcutNames.scaleGizmo);
            _scaleGizmoBtn.style.unityBackgroundImageTintColor = UIValues.inactiveButtonTintColor;

            _universalGizmoBtn.tooltip = "Move/Rotate/Scale" + ShortcutProfileDb.instance.activeProfile.getShortcutUITooltip(ObjectSelectionShortcutNames.universalGizmo);
            _universalGizmoBtn.style.unityBackgroundImageTintColor = UIValues.inactiveButtonTintColor;

            _extrudeGizmoBtn.tooltip = "Extrude" + ShortcutProfileDb.instance.activeProfile.getShortcutUITooltip(ObjectSelectionShortcutNames.extrudeGizmo);
            _extrudeGizmoBtn.style.unityBackgroundImageTintColor = UIValues.inactiveButtonTintColor;

            var selectionGizmos = ObjectSelectionGizmos.instance;
            if (selectionGizmos.isGizmoEnabled(ObjectSelectionGizmoId.Move)) _moveGimzoBtn.style.unityBackgroundImageTintColor = UIValues.activeButtonColor;
            if (selectionGizmos.isGizmoEnabled(ObjectSelectionGizmoId.Rotate)) _rotationGizmoBtn.style.unityBackgroundImageTintColor = UIValues.activeButtonColor;
            if (selectionGizmos.isGizmoEnabled(ObjectSelectionGizmoId.Scale)) _scaleGizmoBtn.style.unityBackgroundImageTintColor = UIValues.activeButtonColor;
            if (selectionGizmos.isGizmoEnabled(ObjectSelectionGizmoId.Universal)) _universalGizmoBtn.style.unityBackgroundImageTintColor = UIValues.activeButtonColor;
            if (selectionGizmos.isGizmoEnabled(ObjectSelectionGizmoId.Extrude)) _extrudeGizmoBtn.style.unityBackgroundImageTintColor = UIValues.activeButtonColor;
        }

        private void refreshGizmoPivotButton()
        {
            if (ObjectSelectionGizmos.instance.transformPivot == ObjectGizmoTransformPivot.Center)
            {
                _gizmoPivotBtn.tooltip = "Center Pivot\r\n\r\nThe gizmo's position in placed in the center of the object selection.";
                _gizmoPivotBtn.style.backgroundImage = TexturePool.instance.gizmoCenterPivot;
            }
            else
            {
                _gizmoPivotBtn.tooltip = "Mesh Pivot\r\n\r\nThe gizmo's position is defined by the object's mesh pivot.";
                _gizmoPivotBtn.style.backgroundImage = TexturePool.instance.gizmoMeshPivot;
            }
        }

        private void refreshGizmoTransformSpaceButton()
        {
            if (ObjectSelectionGizmos.instance.transformSpace == ObjectGizmoTransformSpace.Global)
            {
                _gizmoTransformSpaceBtn.tooltip = "Global\r\n\r\nThe gizmo axes are aligned to the global coordinate system.";
                _gizmoTransformSpaceBtn.style.backgroundImage = TexturePool.instance.gizmoGlobalTransformSpace;
            }
            else
            {
                _gizmoTransformSpaceBtn.tooltip = "Local\r\n\r\nThe gizmo inherits the rotation of the object.";
                _gizmoTransformSpaceBtn.style.backgroundImage = TexturePool.instance.gizmoLocalTransformSpace;
            }
        }

        private void refreshObjectSelectShapeButtons()
        {
            _selectionRectBtn.tooltip = "Selection Rectangle" + ShortcutProfileDb.instance.activeProfile.getShortcutUITooltip(ObjectSelectionShortcutNames.selectionRect, "Multi-object selection is performed using a selection rectangle.");
            _selectionRectBtn.style.unityBackgroundImageTintColor = UIValues.inactiveButtonTintColor;

            _selectionSegmentsBtn.tooltip = "Selection Segments" + ShortcutProfileDb.instance.activeProfile.getShortcutUITooltip(ObjectSelectionShortcutNames.selectionSegments, "Multi-object selection is performed using a chain of segments.");
            _selectionSegmentsBtn.style.unityBackgroundImageTintColor = UIValues.inactiveButtonTintColor;

            _selectionBoxBtn.tooltip = "Selection Box" + ShortcutProfileDb.instance.activeProfile.getShortcutUITooltip(ObjectSelectionShortcutNames.selectionBox, "Multi-object selection is performed using a selection box.");
            _selectionBoxBtn.style.unityBackgroundImageTintColor = UIValues.inactiveButtonTintColor;

            if (ObjectSelection.instance.selectionShapeType == ObjectSelectionShape.Type.Rect) _selectionRectBtn.style.unityBackgroundImageTintColor = UIValues.activeButtonColor;
            else if (ObjectSelection.instance.selectionShapeType == ObjectSelectionShape.Type.Segments) _selectionSegmentsBtn.style.unityBackgroundImageTintColor = UIValues.activeButtonColor;
            else if (ObjectSelection.instance.selectionShapeType == ObjectSelectionShape.Type.Box) _selectionBoxBtn.style.unityBackgroundImageTintColor = UIValues.activeButtonColor;
        }

        private void refreshViewSelectionButtons()
        {
            if (_activeViewId == ViewId.Settings)
            {
                _settingsBtn.style.backgroundColor          = UIValues.activeButtonColor;
                _transformToolsBtn.style.backgroundColor    = UIValues.inactiveButtonColor;
                _miscBtn.style.backgroundColor              = UIValues.inactiveButtonColor;
            }
            else
            if (_activeViewId == ViewId.TransformTools)
            {
                _settingsBtn.style.backgroundColor          = UIValues.inactiveButtonColor;
                _transformToolsBtn.style.backgroundColor    = UIValues.activeButtonColor;
                _miscBtn.style.backgroundColor              = UIValues.inactiveButtonColor;
            }
            else
            if (_activeViewId == ViewId.Misc)
            {
                _settingsBtn.style.backgroundColor          = UIValues.inactiveButtonColor;
                _transformToolsBtn.style.backgroundColor    = UIValues.inactiveButtonColor;
                _miscBtn.style.backgroundColor              = UIValues.activeButtonColor;
            }
        }

        private void updateVisibility()
        {
            bool visible = _activeViewId == ViewId.Settings;
            contentContainer.setChildrenDisplayVisible(uiSectionRowSeparator_Settings, visible);
            selectionSettingsSection.setVisible(visible);
            extrudeGizmoSettingsSection.setVisible(visible);

            visible = _activeViewId == ViewId.TransformTools;
            contentContainer.setChildrenDisplayVisible(uiSectionRowSeparator_TransformTools, visible);
            modularSnapSettingsSection.setVisible(visible);
            surfaceSnapSettingsSection.setVisible(visible);
            projectionSettingsSection.setVisible(visible);
            vertexSnapSettingsSection.setVisible(visible);
            boxSnapSettingsSection.setVisible(visible);

            visible = _activeViewId == ViewId.Misc;
            contentContainer.setChildrenDisplayVisible(uiSectionRowSeparator_Misc, visible);
            selectionGrowSettingsSection.setVisible(visible);
            measureToolsSection.setVisible(visible);
        }

        protected override void onDestroy()
        {
            ScriptableObjectEx.destroyImmediate(_transformSection);
            ScriptableObjectEx.destroyImmediate(_transformUI);
            ScriptableObjectEx.destroyImmediate(_selectionSettingsSection);
            ScriptableObjectEx.destroyImmediate(_extrudeGizmoSettingsSection);
            ScriptableObjectEx.destroyImmediate(_modularSnapSettingsSection);
            ScriptableObjectEx.destroyImmediate(_surfaceSnapSettingsSection);
            ScriptableObjectEx.destroyImmediate(_projectionSettingsSection);
            ScriptableObjectEx.destroyImmediate(_vertexSnapSettingsSection);
            ScriptableObjectEx.destroyImmediate(_boxSnapSettingsSection);
            ScriptableObjectEx.destroyImmediate(_selectionGrowSettingsSection);
            ScriptableObjectEx.destroyImmediate(_measureToolsSection);
        }
    }
}
#endif