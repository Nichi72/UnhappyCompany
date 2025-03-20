#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace GSpawn_Lite
{
    public class ObjectSpawn : ScriptableObject
    {
        [SerializeField]
        private ObjectSpawnToolId           _activeToolId                       = ObjectSpawnToolId.ModularSnap;

        // Note: Maps to ObjectSpawnToolId
        [SerializeField]
        private ObjectSpawnTool[]           _tools                              = new ObjectSpawnTool[Enum.GetValues(typeof(ObjectSpawnToolId)).Length];

        public bool                         isSpawnGuideTransformSessionActive  { get { return activeTool.isSpawnGuidePresentInScene; } }
        public bool                         isSpawnGuidePresentInScene          { get { return activeTool.isSpawnGuidePresentInScene; } }

        public ModularSnapObjectSpawn       modularSnapObjectSpawn              { get { return _tools[(int)ObjectSpawnToolId.ModularSnap] as ModularSnapObjectSpawn; } }
        public SegmentsObjectSpawn          segmentsObjectSpawn                 { get { return _tools[(int)ObjectSpawnToolId.Segments] as SegmentsObjectSpawn; } }
        public BoxObjectSpawn               boxObjectSpawn                      { get { return _tools[(int)ObjectSpawnToolId.Box] as BoxObjectSpawn; } }
        public PropsObjectSpawn             propsObjectSpawn                    { get { return _tools[(int)ObjectSpawnToolId.Props] as PropsObjectSpawn; } }
        public ObjectSpawnGuide             spawnGuide                          { get { return activeTool.spawnGuide; } }
        public ObjectSpawnTool              activeTool                          { get { return _tools[(int)activeToolId]; } }
        public ObjectSpawnToolId            activeToolId 
        { 
            get { return _activeToolId; } 
            set 
            { 
                if (value != _activeToolId) 
                {
                    PluginPrefab spawnGuidePrefab = activeTool.spawnGuidePrefab;
                    activeTool.onNoLongerActive();
                    _activeToolId = value;

                    if (activeTool.requiresSpawnGuide && spawnGuidePrefab != null) 
                        activeTool.setSpawnGuidePrefab(spawnGuidePrefab);

                    PluginInspectorUI.instance.refresh(); 
                    SceneView.RepaintAll(); 
                } 
            } 
        }

        public static ObjectSpawn           instance { get { return GSpawn.active.objectSpawn; } }

        public void onSceneGUI()
        {
            activeTool.onSceneGUI();
        }

        public void usePrefab(PluginPrefab prefab)
        {
            activeTool.setSpawnGuidePrefab(prefab);
        }

        public void onLevelDesignToolChanged()
        {
            if (GSpawn.active.levelDesignToolId != LevelDesignToolId.ObjectSpawn)
                activeTool.onNoLongerActive();
        }

        public void executeModularSnapSessionCommand(ObjectModularSnapSessionCommand command)
        {
            if (_activeToolId == ObjectSpawnToolId.ModularSnap) modularSnapObjectSpawn.executeModularSnapSessionCommand(command);
            else if (_activeToolId == ObjectSpawnToolId.Box) boxObjectSpawn.executeModularSnapSessionCommand(command);
            else if (_activeToolId == ObjectSpawnToolId.Segments) segmentsObjectSpawn.executeModularSnapSessionCommand(command);
        }

        public void executeSurfaceSnapSessionCommand(ObjectSurfaceSnapSessionCommand command)
        {
            if (_activeToolId == ObjectSpawnToolId.Props)
                propsObjectSpawn.executeSurfaceSnapSessionCommand(command);
        }

        public void matchGridCellSizeToSpawnGuideSize()
        {
            if (!activeTool.requiresSpawnGuide) return;

            OBB spawnGuideOBB = activeTool.calcSpawnGuideWorldOBB();
            PluginScene.instance.grid.matchCellSizeToOBBSize(spawnGuideOBB);
        }

        public OBB calcSpawnGuideWorldOBB()
        {
            return activeTool.calcSpawnGuideWorldOBB();
        }

        public void resetSpawnGuideRotationToOriginal()
        {
            activeTool.resetSpawnGuideRotationToOriginal();
        }

        public void resetSpawnGuideScaleToOriginal()
        {
            activeTool.resetSpawnGuideScaleToOriginal();
        }

        public void rotateSpawnGuide(Vector3 axis, float degrees)
        {
            activeTool.rotateSpawnGuide(axis, degrees);
        }

        public void rotateSpawnGuide(Vector3 point, Vector3 axis, float degrees)
        {
            activeTool.rotateSpawnGuide(point, axis, degrees);
        }

        public void setSpawnGuideRotation(Quaternion rotation)
        {
            activeTool.setSpawnGuideRotation(rotation);
        }

        private void OnEnable()
        {
            int numSpawnTools = Enum.GetValues(typeof(ObjectSpawnToolId)).Length;
            if (_tools.Length != numSpawnTools)
            {
                for (int i = 0; i < _tools.Length; ++i)
                {
                    ScriptableObjectEx.destroyImmediate(_tools[i]);
                    _tools[i] = null;
                }

                _tools = new ObjectSpawnTool[numSpawnTools];
            }
            
            if (_tools[(int)ObjectSpawnToolId.ModularSnap] == null)
                _tools[(int)ObjectSpawnToolId.ModularSnap]          = ScriptableObject.CreateInstance<ModularSnapObjectSpawn>();

            if (_tools[(int)ObjectSpawnToolId.Segments] == null)
                _tools[(int)ObjectSpawnToolId.Segments]             = ScriptableObject.CreateInstance<SegmentsObjectSpawn>();

            if (_tools[(int)ObjectSpawnToolId.Box] == null)
                _tools[(int)ObjectSpawnToolId.Box]                  = ScriptableObject.CreateInstance<BoxObjectSpawn>();

            if (_tools[(int)ObjectSpawnToolId.Props] == null)
                _tools[(int)ObjectSpawnToolId.Props]                = ScriptableObject.CreateInstance<PropsObjectSpawn>();
        }

        private void OnDestroy()
        {
            int numTools = _tools.Length;
            for (int i = 0; i <  numTools; ++i)
            {
                ScriptableObjectEx.destroyImmediate(_tools[i]);
                _tools[i] = null;
            }
        }
    }
}
#endif