#if UNITY_EDITOR
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GSpawn_Lite
{
    public class ObjectSpawnUI : PluginUI
    {
        private ToolbarButton   _modularSnapBtn;
        private ToolbarButton   _segmentsBtn;
        private ToolbarButton   _boxBtn;
        private ToolbarButton   _propsBtn;

        [SerializeField]
        private UISection       _modularSnapSpawnSettingsSection;
        [SerializeField]
        private UISection       _modularSnapSpawnGuideSettingsSection;
        [SerializeField]
        private UISection       _segmentsSpawnModularSnapSettingsSection;
        [SerializeField]
        private UISection       _segmentsSpawnSettingsProfileSection;
        [SerializeField]
        private UISection       _boxSpawnModularSnapSettingsSection;
        [SerializeField]
        private UISection       _boxSpawnSettingsProfileSection;
        [SerializeField]
        private UISection       _propsSpawnSurfaceSnapSettingsSection;
        [SerializeField]
        private UISection       _propsSpawnDragSpawnSettingsSection;
        [SerializeField]
        private UISection       _propsSpawnTerrainFlattenSettingsSection;
        [SerializeField]
        private UISection       _propsSpawnGuideSettingsSection;

        private UISection       modularSnapSpawnSettingsSection                 { get { if (_modularSnapSpawnSettingsSection == null) _modularSnapSpawnSettingsSection = ScriptableObject.CreateInstance<UISection>(); return _modularSnapSpawnSettingsSection; } }
        private UISection       modularSnapSpawnGuideSettingsSection            { get { if (_modularSnapSpawnGuideSettingsSection == null) _modularSnapSpawnGuideSettingsSection = ScriptableObject.CreateInstance<UISection>(); return _modularSnapSpawnGuideSettingsSection; } }     
        private UISection       segmentsSpawnModularSnapSettingsSection         { get { if (_segmentsSpawnModularSnapSettingsSection == null) _segmentsSpawnModularSnapSettingsSection = ScriptableObject.CreateInstance<UISection>(); return _segmentsSpawnModularSnapSettingsSection; } }
        private UISection       segmentsSpawnSettingsProfileSection             { get { if (_segmentsSpawnSettingsProfileSection == null) _segmentsSpawnSettingsProfileSection = ScriptableObject.CreateInstance<UISection>(); return _segmentsSpawnSettingsProfileSection; } }
        private UISection       boxSpawnModularSnapSettingsSection              { get { if (_boxSpawnModularSnapSettingsSection == null) _boxSpawnModularSnapSettingsSection = ScriptableObject.CreateInstance<UISection>(); return _boxSpawnModularSnapSettingsSection; } }
        private UISection       boxSpawnSettingsProfileSection                  { get { if (_boxSpawnSettingsProfileSection == null) _boxSpawnSettingsProfileSection = ScriptableObject.CreateInstance<UISection>(); return _boxSpawnSettingsProfileSection; } }       
        private UISection       propsSpawnSurfaceSnapSettingsSection            { get { if (_propsSpawnSurfaceSnapSettingsSection == null) _propsSpawnSurfaceSnapSettingsSection = ScriptableObject.CreateInstance<UISection>(); return _propsSpawnSurfaceSnapSettingsSection; } }
        private UISection       propsSpawnDragSpawnSettingsSection              { get { if (_propsSpawnDragSpawnSettingsSection == null) _propsSpawnDragSpawnSettingsSection = ScriptableObject.CreateInstance<UISection>(); return _propsSpawnDragSpawnSettingsSection; } }
        private UISection       propsSpawnTerrainFlattenSettingsSection         { get { if (_propsSpawnTerrainFlattenSettingsSection == null) _propsSpawnTerrainFlattenSettingsSection = ScriptableObject.CreateInstance<UISection>(); return _propsSpawnTerrainFlattenSettingsSection; } }
        private UISection       propsSpawnGuideSettingsSection                  { get { if (_propsSpawnGuideSettingsSection == null) _propsSpawnGuideSettingsSection = ScriptableObject.CreateInstance<UISection>(); return _propsSpawnGuideSettingsSection; } }

        private static string   uiSectionRowSeparator_ModularSnapSpawnName      { get { return "A"; } }
        private static string   uiSectionRowSeparator_SegmentsSpawnName         { get { return "B"; } }
        private static string   uiSectionRowSeparator_BoxSpawnName              { get { return "C"; } }
        private static string   uiSectionRowSeparator_PropsSpawnName            { get { return "D"; } }

        public static ObjectSpawnUI instance                                    { get { return GSpawn.active.objectSpawnUI; } }

        protected override void onRefresh()
        {
            refreshSpawnModeButtons();
            refreshToolButtons();
            refreshToolTips();
            updateVisibility();
        }

        protected override void onBuild()
        {
            Toolbar spawnToolsToolbar            = UI.createToolSelectionToolbar(contentContainer);
            spawnToolsToolbar.style.height       = UIValues.mediumToolbarButtonSize + 3.0f;

            _modularSnapBtn             = UI.createToolbarButton(TexturePool.instance.modularSnapSpawn, UI.ButtonStyle.Push, UIValues.mediumToolbarButtonSize, spawnToolsToolbar);
            UI.useDefaultMargins(_modularSnapBtn);
            _modularSnapBtn.clicked     += () => { ObjectSpawn.instance.activeToolId = ObjectSpawnToolId.ModularSnap; SceneViewEx.focus(); };

            _segmentsBtn                = UI.createToolbarButton(TexturePool.instance.segmentsSpawn, UI.ButtonStyle.Push, UIValues.mediumToolbarButtonSize, spawnToolsToolbar);
            UI.useDefaultMargins(_segmentsBtn);
            _segmentsBtn.clicked        += () => { ObjectSpawn.instance.activeToolId = ObjectSpawnToolId.Segments; SceneViewEx.focus(); };

            _boxBtn                     = UI.createToolbarButton(TexturePool.instance.boxSpawn, UI.ButtonStyle.Push, UIValues.mediumToolbarButtonSize, spawnToolsToolbar);
            UI.useDefaultMargins(_boxBtn);
            _boxBtn.clicked             += () => { ObjectSpawn.instance.activeToolId = ObjectSpawnToolId.Box; SceneViewEx.focus(); };

            _propsBtn                   = UI.createToolbarButton(TexturePool.instance.propsSpawn, UI.ButtonStyle.Push, UIValues.mediumToolbarButtonSize, spawnToolsToolbar);
            UI.useDefaultMargins(_propsBtn);
            _propsBtn.clicked           += () => { ObjectSpawn.instance.activeToolId = ObjectSpawnToolId.Props; SceneViewEx.focus(); };

            // Modular snap spawn
            modularSnapSpawnSettingsSection.build("Modular Snap", TexturePool.instance.modularSnapSpawn, true, contentContainer);
            ObjectSpawn.instance.modularSnapObjectSpawn.modularSnapSettings.buildUI(modularSnapSpawnSettingsSection.contentContainer);

            UI.createUISectionRowSeparator(contentContainer, uiSectionRowSeparator_ModularSnapSpawnName);
            modularSnapSpawnGuideSettingsSection.build("Spawn Guide", TexturePool.instance.location, true, contentContainer);
            ObjectSpawn.instance.modularSnapObjectSpawn.spawnGuideSettings.buildUI(modularSnapSpawnGuideSettingsSection.contentContainer, ObjectSpawnGuideSettingsUsage.Other);

            // Segments spawn
            segmentsSpawnModularSnapSettingsSection.build("Modular Snap", TexturePool.instance.modularSnapSpawn, true, contentContainer);
            ObjectSpawn.instance.segmentsObjectSpawn.modularSnapSettings.buildUI(segmentsSpawnModularSnapSettingsSection.contentContainer);

            UI.createUISectionRowSeparator(contentContainer, uiSectionRowSeparator_SegmentsSpawnName);
            segmentsSpawnSettingsProfileSection.build("Segments", TexturePool.instance.segmentsSpawn, true, contentContainer);
            SegmentsObjectSpawnSettingsProfileDbUI.instance.build(segmentsSpawnSettingsProfileSection.contentContainer, GSpawn.active.inspectorUI.targetEditor);

            // Box spawn
            boxSpawnModularSnapSettingsSection.build("Modular Snap", TexturePool.instance.modularSnapSpawn, true, contentContainer);
            ObjectSpawn.instance.boxObjectSpawn.modularSnapSettings.buildUI(boxSpawnModularSnapSettingsSection.contentContainer);

            UI.createUISectionRowSeparator(contentContainer, uiSectionRowSeparator_BoxSpawnName);
            boxSpawnSettingsProfileSection.build("Box", TexturePool.instance.boxSpawn, true, contentContainer);
            BoxObjectSpawnSettingsProfileDbUI.instance.build(boxSpawnSettingsProfileSection.contentContainer, GSpawn.active.inspectorUI.targetEditor);

            // Props spawn
            propsSpawnSurfaceSnapSettingsSection.build("Surface Snap", TexturePool.instance.objectSurfaceSnap, true, contentContainer);
            ObjectSpawn.instance.propsObjectSpawn.surfaceSnapSettings.buildUI(propsSpawnSurfaceSnapSettingsSection.contentContainer);

            UI.createUISectionRowSeparator(contentContainer, uiSectionRowSeparator_PropsSpawnName);
            propsSpawnDragSpawnSettingsSection.build("Drag Spawn", TexturePool.instance.dragArrow, true, contentContainer);
            ObjectSpawn.instance.propsObjectSpawn.dragSpawnSettings.buildUI(propsSpawnDragSpawnSettingsSection.contentContainer);

            UI.createUISectionRowSeparator(contentContainer, uiSectionRowSeparator_PropsSpawnName);
            propsSpawnTerrainFlattenSettingsSection.build("Terrain Flatten", TexturePool.instance.terrainFlatten, true, contentContainer);
            ObjectSpawn.instance.propsObjectSpawn.terrainFlattenSettings.buildUI(propsSpawnTerrainFlattenSettingsSection.contentContainer);

            UI.createUISectionRowSeparator(contentContainer, uiSectionRowSeparator_PropsSpawnName);
            propsSpawnGuideSettingsSection.build("Spawn Guide", TexturePool.instance.location, true, contentContainer);
            ObjectSpawn.instance.propsObjectSpawn.spawnGuideSettings.buildUI(propsSpawnGuideSettingsSection.contentContainer, ObjectSpawnGuideSettingsUsage.PropsSpawn);

            refreshSpawnModeButtons();
            refreshToolButtons();
            refreshToolTips();
            updateVisibility();
        }

        private void refreshSpawnModeButtons()
        {
            _modularSnapBtn.tooltip                 = "Modular Snap";
            _modularSnapBtn.style.backgroundColor   = UIValues.inactiveButtonColor;

            _segmentsBtn.tooltip                    = "Segments";
            _segmentsBtn.style.backgroundColor      = UIValues.inactiveButtonColor;

            _boxBtn.tooltip                         = "Box";
            _boxBtn.style.backgroundColor           = UIValues.inactiveButtonColor;

            _propsBtn.tooltip                       = "Props";
            _propsBtn.style.backgroundColor         = UIValues.inactiveButtonColor;

            var objectSpawnToolId = ObjectSpawn.instance.activeToolId;
            if (objectSpawnToolId == ObjectSpawnToolId.ModularSnap)         _modularSnapBtn.style.backgroundColor       = UIValues.activeButtonColor;
            else if (objectSpawnToolId == ObjectSpawnToolId.Segments)       _segmentsBtn.style.backgroundColor          = UIValues.activeButtonColor;
            else if (objectSpawnToolId == ObjectSpawnToolId.Box)            _boxBtn.style.backgroundColor               = UIValues.activeButtonColor;
            else if (objectSpawnToolId == ObjectSpawnToolId.Props)          _propsBtn.style.backgroundColor             = UIValues.activeButtonColor;
        }

        private void refreshToolTips()
        {
        }

        private void refreshToolButtons()
        {
        }

        private void updateVisibility()
        {
            var objectSpawnToolId = ObjectSpawn.instance.activeToolId;
            bool visible = (objectSpawnToolId == ObjectSpawnToolId.ModularSnap);
            contentContainer.setChildrenDisplayVisible(uiSectionRowSeparator_ModularSnapSpawnName, visible);
            modularSnapSpawnSettingsSection.setVisible(visible);
            modularSnapSpawnGuideSettingsSection.setVisible(visible);

            visible = (objectSpawnToolId == ObjectSpawnToolId.Segments);
            contentContainer.setChildrenDisplayVisible(uiSectionRowSeparator_SegmentsSpawnName, visible);
            segmentsSpawnModularSnapSettingsSection.setVisible(visible);
            segmentsSpawnSettingsProfileSection.setVisible(visible);

            visible = (objectSpawnToolId == ObjectSpawnToolId.Box);
            contentContainer.setChildrenDisplayVisible(uiSectionRowSeparator_BoxSpawnName, visible);
            boxSpawnModularSnapSettingsSection.setVisible(visible);
            boxSpawnSettingsProfileSection.setVisible(visible);

            visible = (objectSpawnToolId == ObjectSpawnToolId.Props);
            contentContainer.setChildrenDisplayVisible(uiSectionRowSeparator_PropsSpawnName, visible);
            propsSpawnSurfaceSnapSettingsSection.setVisible(visible);
            propsSpawnDragSpawnSettingsSection.setVisible(visible);
            propsSpawnTerrainFlattenSettingsSection.setVisible(visible);
            propsSpawnGuideSettingsSection.setVisible(visible);
        }

        protected override void onDestroy()
        {
            ScriptableObjectEx.destroyImmediate(_modularSnapSpawnSettingsSection);
            ScriptableObjectEx.destroyImmediate(_modularSnapSpawnGuideSettingsSection);
            ScriptableObjectEx.destroyImmediate(_segmentsSpawnModularSnapSettingsSection);
            ScriptableObjectEx.destroyImmediate(_segmentsSpawnSettingsProfileSection);
            ScriptableObjectEx.destroyImmediate(_boxSpawnModularSnapSettingsSection);
            ScriptableObjectEx.destroyImmediate(_boxSpawnSettingsProfileSection);
            ScriptableObjectEx.destroyImmediate(_propsSpawnSurfaceSnapSettingsSection);
            ScriptableObjectEx.destroyImmediate(_propsSpawnDragSpawnSettingsSection);
            ScriptableObjectEx.destroyImmediate(_propsSpawnTerrainFlattenSettingsSection);
            ScriptableObjectEx.destroyImmediate(_propsSpawnGuideSettingsSection);
        }
    }
}
#endif