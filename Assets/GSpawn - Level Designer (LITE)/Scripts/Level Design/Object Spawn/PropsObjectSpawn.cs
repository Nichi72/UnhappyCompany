#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace GSpawn_Lite
{
    public class PropsObjectSpawn : ObjectSpawnTool
    {
        [NonSerialized]
        private ObjectSpawnGuideSettings    _spawnGuideSettings;
        [NonSerialized]
        private ObjectSurfaceSnapSettings   _surfaceSnapSettings;
        [NonSerialized]
        private ObjectDragSpawnSettings     _dragSpawnSettings;
        [NonSerialized]
        private TerrainFlattenSettings      _terrainFlattenSettings;
        [SerializeField]
        private ObjectSurfaceSnapSession    _surfaceSnapSession;
        [NonSerialized]
        private Vector3                     _lastSurfacePickPoint;
        [NonSerialized]
        private SceneRaycastFilter          _pickPrefabRaycastFilter    = new SceneRaycastFilter();
        [NonSerialized]
        private List<OBB>                   _obbBuffer                  = new List<OBB>();
        [NonSerialized]
        private List<Vector3>               _vector3Buffer              = new List<Vector3>();
        [NonSerialized]
        private Vector3[]                   _terrainPatchCorners        = new Vector3[4];

        private ObjectSurfaceSnapSession    surfaceSnapSession
        {
            get
            {
                if (_surfaceSnapSession == null)
                {
                    _surfaceSnapSession = CreateInstance<ObjectSurfaceSnapSession>();
                    _surfaceSnapSession.sharedSettings = surfaceSnapSettings;
                }
                return _surfaceSnapSession;
            }
        }

        public ObjectSurfaceSnapSettings    surfaceSnapSettings
        {
            get
            {
                if (_surfaceSnapSettings == null) _surfaceSnapSettings = AssetDbEx.loadScriptableObject<ObjectSurfaceSnapSettings>(PluginFolders.settings, typeof(PropsObjectSpawn).Name + "_" + typeof(ObjectSurfaceSnapSettings).Name);
                return _surfaceSnapSettings;
            }
        }
        public ObjectDragSpawnSettings      dragSpawnSettings
        {
            get
            {
                if (_dragSpawnSettings == null) _dragSpawnSettings = AssetDbEx.loadScriptableObject<ObjectDragSpawnSettings>(PluginFolders.settings, typeof(PropsObjectSpawn).Name + "_" + typeof(ObjectDragSpawnSettings).Name);
                return _dragSpawnSettings;
            }
        }
        public TerrainFlattenSettings       terrainFlattenSettings
        {
            get
            {
                if (_terrainFlattenSettings == null) _terrainFlattenSettings = AssetDbEx.loadScriptableObject<TerrainFlattenSettings>(PluginFolders.settings, typeof(PropsObjectSpawn).Name + "_" + typeof(TerrainFlattenSettings).Name);
                return _terrainFlattenSettings;
            }
        }
        public override ObjectSpawnGuideSettings spawnGuideSettings
        {
            get
            {
                if (_spawnGuideSettings == null) _spawnGuideSettings = AssetDbEx.loadScriptableObject<ObjectSpawnGuideSettings>(PluginFolders.settings, typeof(PropsObjectSpawn).Name + "_" + typeof(ObjectSpawnGuideSettings).Name);
                return _spawnGuideSettings;
            }
        }
        public override ObjectSpawnToolId   spawnToolId             { get { return ObjectSpawnToolId.Props; } }
        public override bool                requiresSpawnGuide      { get { return true; } }

        public PropsObjectSpawn()
        {
        }

        public override void setSpawnGuidePrefab(PluginPrefab prefab)
        {
            spawnGuide.usePrefab(prefab, surfaceSnapSession);
        }

        public override void onNoLongerActive()
        {
            spawnGuide.destroyGuide();
            enableSpawnGuidePrefabScroll = false;
        }

        public void executeSurfaceSnapSessionCommand(ObjectSurfaceSnapSessionCommand command)
        {
            surfaceSnapSession.executeCommand(command);
        }

        protected override void doOnSceneGUI()
        {
            spawnGuide.onSceneGUI();

            Event e = Event.current;
            if (FixedShortcuts.enablePickSpawnGuidePrefabFromScene(e))
            {
                if (e.isLeftMouseButtonDownEvent())
                {
                    var prefabPickResult = PluginScene.instance.pickPrefab(PluginCamera.camera.getCursorRay(), _pickPrefabRaycastFilter, ObjectRaycastConfig.defaultConfig);
                    if (prefabPickResult != null)
                    {
                        setSpawnGuidePrefab(prefabPickResult.pickedPluginPrefab);
                        spawnGuide.setRotationAndScale(prefabPickResult.pickedObject.transform.rotation, prefabPickResult.pickedObject.transform.lossyScale);
                    }
                }
            }
            else
            {
                if (enableSpawnGuidePrefabScroll && e.isScrollWheel)
                {
                    PluginPrefab newPrefab = PluginPrefabManagerUI.instance.scrollVisiblePrefabSelection((int)e.getMouseScrollSign());
                    if (newPrefab != null)
                    {
                        setSpawnGuidePrefab(newPrefab);
                        e.disable();
                    }
                }
            }

            if (surfaceSnapSession.isActive)
            {
                if (surfaceSnapSession.isSurfaceValid)
                {
                    if (e.isLeftMouseButtonDownEvent() && !e.alt)
                    {
                        surfaceSnapSession.addObjectHierarchyIgnoredAsSurface(spawn());
                        _lastSurfacePickPoint = surfaceSnapSession.surfacePickPoint;
                        if (surfaceSnapSession.isSurfaceTerrain) surfaceSnapSession.setSurfaceLocked(true);
                    }
                    else
                    if (e.isLeftMouseButtonDragEvent() && !e.alt)
                    {
                        float minDragDistance = dragSpawnSettings.minDragDistance;
                        if (dragSpawnSettings.useSafeDragDistance) minDragDistance = Mathf.Max(minDragDistance, spawnGuide.volumeRadius * 2.0f);
                        if ((_lastSurfacePickPoint - surfaceSnapSession.surfacePickPoint).magnitude >= minDragDistance)
                        {
                            surfaceSnapSession.addObjectHierarchyIgnoredAsSurface(spawn());
                             _lastSurfacePickPoint = surfaceSnapSession.surfacePickPoint;
                            if (surfaceSnapSession.isSurfaceTerrain) surfaceSnapSession.setSurfaceLocked(true);
                        }
                    }
                    else
                    if (FixedShortcuts.changeRadiusByScrollWheel(e))
                    {
                        if (terrainFlattenSettings.flattenTerrain)
                        {
                            e.disable();

                            int amount = (int)(e.getMouseScrollSign() * 0.5f);
                            if (amount == 0) amount = (int)(e.getMouseScrollSign());

                            terrainFlattenSettings.terrainQuadRadius -= amount;
                            EditorUtility.SetDirty(terrainFlattenSettings);
                        }
                    }
                }
            }

            if (e.type == EventType.MouseUp || e.type == EventType.MouseLeaveWindow)
            {
                surfaceSnapSession.clearObjectsIgnoredAsSurface();
                surfaceSnapSession.setSurfaceLocked(false);
            }
        }

        protected override void draw()
        {
            if (surfaceSnapSession.isActive)
            {
                if (surfaceSnapSession.isSurfaceUnityTerrain && terrainFlattenSettings.flattenTerrain)
                {
                    Terrain terrain     = surfaceSnapSession.surfaceObject.getTerrain();
                    float terrainYPos   = terrain.transform.position.y;

                    spawnGuide.calcWorldOBB().calcCorners(_vector3Buffer, false);
                    AABB aabb           = new AABB(_vector3Buffer);
                    terrain.calcTerrainPatchCorners(terrainFlattenSettings.terrainQuadRadius, aabb, _terrainPatchCorners);

                    HandlesEx.saveColor();
                    Handles.color = ObjectSpawnPrefs.instance.propsSpawnTerrainFlattenAreaColor;

                    const int numSamplePoints   = 30;
                    float stepSize              = 1.0f / (float)numSamplePoints;
                    for (int lineIndex = 0; lineIndex < 4; ++lineIndex)
                    {
                        Vector3 p0      = _terrainPatchCorners[lineIndex];
                        Vector3 p1      = _terrainPatchCorners[(lineIndex + 1) % 4];
                        Vector3 dir     = (p1 - p0);
                        Vector3 prevPt  = terrain.projectPoint(terrainYPos, p0);
                        for (int i = 0; i < numSamplePoints; ++i)
                        {
                            Vector3 currentPt = terrain.projectPoint(terrainYPos, prevPt + dir * stepSize);
                            Handles.DrawLine(prevPt, currentPt);
                            prevPt = currentPt;
                        }
                    }

                    HandlesEx.restoreColor();
                }
            }
        }

        private GameObject spawn()
        {
            Vector3 surfaceNormal = surfaceSnapSession.surfacePickNormal;
            if (!surfaceSnapSettings.alignAxis) surfaceNormal = surfaceSnapSession.getNoAlignmentRotationAxis();

            GameObject spawnedObject;
            if (surfaceSnapSession.isSurfaceUnityTerrain && terrainFlattenSettings.flattenTerrain)
            {
                Terrain terrain                     = surfaceSnapSession.surfaceObject.getTerrain();

                TerrainFlattenConfig flattenConfig  = new TerrainFlattenConfig();
                flattenConfig.terrainQuadRadius     = terrainFlattenSettings.terrainQuadRadius;
                flattenConfig.mode                  = terrainFlattenSettings.mode;
                flattenConfig.applyFalloff          = terrainFlattenSettings.applyFalloff;
                terrain.flattenAroundOBB(spawnGuide.calcWorldOBB(), flattenConfig);

                surfaceSnapSession.projectTargetsOnSurface();
                spawnedObject = spawnGuide.spawn();
            }
            else spawnedObject = spawnGuide.spawn();

            if (spawnGuideSettings.randomizePrefab)
            {
                RandomPrefab randomPrefab = spawnGuideSettings.randomPrefabProfile.pickPrefab();
                if (randomPrefab != null) setSpawnGuidePrefab(randomPrefab.pluginPrefab);
            }

            spawnGuide.randomizeTransformIfNecessary(spawnGuideSettings.transformRandomizationSettings, surfaceNormal);

            return spawnedObject;
        }

        protected override void onEnabled()
        {
            _pickPrefabRaycastFilter            = createDefaultPrefabPickRaycastFilter();
            surfaceSnapSession.sharedSettings   = surfaceSnapSettings;
        }

        protected override void onDestroy()
        {
            ScriptableObjectEx.destroyImmediate(_surfaceSnapSession);
        }
    }
}
#endif