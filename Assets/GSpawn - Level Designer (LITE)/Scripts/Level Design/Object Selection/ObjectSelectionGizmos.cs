﻿#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace GSpawn_Lite
{
    public enum ObjectSelectionGizmoId
    {
        Move = 0,
        Rotate,
        Scale,
        Universal,
        Extrude,
    }

    public class ObjectSelectionGizmos : ScriptableObject
    {
        [SerializeField]
        private ObjectMoveGizmo             _moveGizmo;
        [SerializeField]
        private ObjectRotationGizmo         _rotationGizmo;
        [SerializeField]
        private ObjectScaleGizmo            _scaleGizmo;
        [SerializeField]
        private ObjectUniversalGizmo        _universalGizmo;
        [SerializeField]
        private ObjectExtrudeGizmo          _extrudeGizmo;
        [SerializeField]
        private PluginGizmo[]               _gizmos;

        [NonSerialized]
        private ObjectExtrudeGizmoSettings  _extrudeGizmoSettings;

        [SerializeField]
        private GameObject                  _pivotObject;

        [SerializeField]
        private ObjectGizmoTransformPivot   _transformPivot                 = ObjectGizmoTransformPivot.Center;
        [SerializeField]
        private ObjectGizmoTransformSpace   _transformSpace                 = ObjectGizmoTransformSpace.Global;

        [NonSerialized]
        private List<OBB>                   _mirroredOBBs                   = new List<OBB>();
        [NonSerialized]
        private List<OBB>                   _extrudeCellOBBs                = new List<OBB>();

        public ObjectExtrudeGizmoSettings   extrudeGizmoSettings
        {
            get
            {
                if (_extrudeGizmoSettings == null) _extrudeGizmoSettings = AssetDbEx.loadScriptableObject<ObjectExtrudeGizmoSettings>(PluginFolders.settings, typeof(ObjectSelectionGizmos).Name + "_" + typeof(ObjectExtrudeGizmoSettings).Name);
                return _extrudeGizmoSettings;
            }
        }
        public ObjectGizmoTransformPivot    transformPivot                  { get { return _transformPivot; } set { _transformPivot = value; onTransformPivotChanged(); } }
        public ObjectGizmoTransformSpace    transformSpace                  { get { return _transformSpace; } set { _transformSpace = value; onTransformSpaceChanged(); } }
        
        public static ObjectSelectionGizmos instance                        { get { return ObjectSelection.instance.gizmos; } }

        public void onSceneGUI()
        {
            foreach (var gizmo in _gizmos)
                gizmo.onSceneGUI();
        }

        public void bindTargetObjects(List<GameObject> targetObjects)
        {
            // Note: Just make sure all gizmos have their settings in place.
            _extrudeGizmo.sharedSettings    = extrudeGizmoSettings;

            _moveGizmo.bindTargetObjects(targetObjects);
            _rotationGizmo.bindTargetObjects(targetObjects);
            _scaleGizmo.bindTargetObjects(targetObjects);
            _universalGizmo.bindTargetObjects(targetObjects);
            _extrudeGizmo.bindTargetObjects(targetObjects);
        }

        public void onTargetObjectsUpdated(GameObject pivotObject)
        {
            _pivotObject = pivotObject;

            if (_moveGizmo.enabled)         _moveGizmo.onTargetObjectsUpdated(pivotObject);
            if (_rotationGizmo.enabled)     _rotationGizmo.onTargetObjectsUpdated(pivotObject);
            if (_scaleGizmo.enabled)        _scaleGizmo.onTargetObjectsUpdated(pivotObject);
            if (_universalGizmo.enabled)    _universalGizmo.onTargetObjectsUpdated(pivotObject);
            if (_extrudeGizmo.enabled)      _extrudeGizmo.onTargetObjectsUpdated();
        }

        public void onTargetObjectTransformsChanged()
        {
            if (_moveGizmo.enabled)         _moveGizmo.onTargetObjectTransformsChanged();
            if (_rotationGizmo.enabled)     _rotationGizmo.onTargetObjectTransformsChanged();
            if (_scaleGizmo.enabled)        _scaleGizmo.onTargetObjectTransformsChanged();
            if (_universalGizmo.enabled)    _universalGizmo.onTargetObjectTransformsChanged();
            if (_extrudeGizmo.enabled)      _extrudeGizmo.onTargetObjectTransformsChanged();
        }

        public bool isGizmoEnabled(ObjectSelectionGizmoId id)
        {
            return getGizmo(id).enabled;
        }

        public void setAllGizmosEnabled(bool enabled)
        {
            foreach (var gizmo in _gizmos)
                gizmo.enabled = enabled;
        }

        public void setAllGizmosEnabled(bool enabled, List<ObjectSelectionGizmoId> mask)
        {
            for (int index = 0; index < _gizmos.Length; ++index)
            {
                ObjectSelectionGizmoId id = (ObjectSelectionGizmoId)index;
                if (mask.Contains(id)) continue;

                _gizmos[index].enabled = enabled;
            }
        }

        public void setAllGizmosEnabled(bool enabled, ObjectSelectionGizmoId mask)
        {
            for (int index = 0; index < _gizmos.Length; ++index)
            {
                ObjectSelectionGizmoId id = (ObjectSelectionGizmoId)index;
                if (mask == id) continue;

                _gizmos[index].enabled = enabled;
            }
        }

        public void setGizmoEnabled(ObjectSelectionGizmoId id, bool enabled, bool disableTheRest)
        {
            if (disableTheRest)
                setAllGizmosEnabled(false);

            var pluginGizmo         = getGizmo(id);
            pluginGizmo.enabled     = enabled;
            if (pluginGizmo == _extrudeGizmo) _extrudeGizmo.onTargetObjectsUpdated();
            else if (pluginGizmo is ObjectTransformGizmo) ((ObjectTransformGizmo)pluginGizmo).onTargetObjectsUpdated(_pivotObject);

            PluginInspectorUI.instance.refresh();
            SceneView.RepaintAll();
        }

        public void setGizmoEnabled(ObjectSelectionGizmoId id, bool enabled, bool disableTheRest, List<ObjectSelectionGizmoId> disableMask)
        {
            if (disableTheRest)
                setAllGizmosEnabled(false, disableMask);

            var pluginGizmo     = getGizmo(id);
            pluginGizmo.enabled = enabled;

            if (pluginGizmo == _extrudeGizmo) _extrudeGizmo.onTargetObjectsUpdated();
            else if (pluginGizmo is ObjectTransformGizmo) ((ObjectTransformGizmo)pluginGizmo).refreshPositionAndRotation();

            PluginInspectorUI.instance.refresh();
            SceneView.RepaintAll();
        }

        public void setGizmoEnabled(ObjectSelectionGizmoId id, bool enabled, bool disableTheRest, ObjectSelectionGizmoId disableMask)
        {
            if (disableTheRest)
                setAllGizmosEnabled(false, disableMask);

            var pluginGizmo     = getGizmo(id);
            pluginGizmo.enabled = enabled;

            if (pluginGizmo == _extrudeGizmo) _extrudeGizmo.onTargetObjectsUpdated();
            else if (pluginGizmo is ObjectTransformGizmo) ((ObjectTransformGizmo)pluginGizmo).refreshPositionAndRotation();

            PluginInspectorUI.instance.refresh();
            SceneView.RepaintAll();
        }

        private void onTransformPivotChanged()
        {
            _moveGizmo.transformPivot       = transformPivot;
            _rotationGizmo.transformPivot   = transformPivot;
            _scaleGizmo.transformPivot      = transformPivot;
            _universalGizmo.transformPivot  = transformPivot;

            PluginInspectorUI.instance.refresh();
            SceneView.RepaintAll();
        }

        private void onTransformSpaceChanged()
        {
            _moveGizmo.transformSpace       = transformSpace;
            _rotationGizmo.transformSpace   = transformSpace;
            _scaleGizmo.transformSpace      = transformSpace;
            _universalGizmo.transformSpace  = transformSpace;

            PluginInspectorUI.instance.refresh();
            SceneView.RepaintAll();
        } 

        private PluginGizmo getGizmo(ObjectSelectionGizmoId id)
        {
            return _gizmos[(int)id];
        }

        private void onVerticalAxisExtrudeSpawn(ObjectExtrudeGizmo extrudeGizmo, List<GameObject> spawnedParents)
        {
        }

        private void onExtrudeSpawn(ObjectExtrudeGizmo extrudeGizmo, List<GameObject> spawnedParents)
        {
            if (GizmoPrefs.instance.extrudeSelectSpawned)
                ObjectSelection.instance.appendObjects(spawnedParents);
        }

        private void OnEnable()
        {
            // Note: Might have deleted Data folder and plugin objects might not have had the chance to get delted.
            if (!FileSystem.folderExists(PluginFolders.settings))
                return;

            if (_moveGizmo == null)
            {
                _moveGizmo                      = ScriptableObject.CreateInstance<ObjectMoveGizmo>();
                _moveGizmo.transformPivot       = _transformPivot;
                _moveGizmo.transformSpace       = _transformSpace;
                _moveGizmo.enabled              = true;

                _rotationGizmo                  = ScriptableObject.CreateInstance<ObjectRotationGizmo>();
                _rotationGizmo.transformPivot   = _transformPivot;
                _rotationGizmo.transformSpace   = _transformSpace;
                _rotationGizmo.enabled          = false;

                _scaleGizmo                     = ScriptableObject.CreateInstance<ObjectScaleGizmo>();
                _scaleGizmo.transformPivot      = _transformPivot;
                _scaleGizmo.transformSpace      = _transformSpace;
                _scaleGizmo.enabled             = false;

                _universalGizmo                 = ScriptableObject.CreateInstance<ObjectUniversalGizmo>();
                _universalGizmo.transformPivot  = _transformPivot;
                _universalGizmo.transformSpace  = _transformSpace;
                _universalGizmo.enabled         = false;

                _extrudeGizmo                   = ScriptableObject.CreateInstance<ObjectExtrudeGizmo>();
                _extrudeGizmo.enabled           = false;
        
                _gizmos                         = new PluginGizmo[5];
                _gizmos[(int)ObjectSelectionGizmoId.Move]       = _moveGizmo;
                _gizmos[(int)ObjectSelectionGizmoId.Rotate]     = _rotationGizmo;
                _gizmos[(int)ObjectSelectionGizmoId.Scale]      = _scaleGizmo;
                _gizmos[(int)ObjectSelectionGizmoId.Universal]  = _universalGizmo;
                _gizmos[(int)ObjectSelectionGizmoId.Extrude]    = _extrudeGizmo;
            }

            _extrudeGizmo.verticalAxisExtrudeSpawn  += onVerticalAxisExtrudeSpawn;
            _extrudeGizmo.extrudeSpawn              += onExtrudeSpawn;
            _extrudeGizmo.sharedSettings        = extrudeGizmoSettings;
        }

        private void OnDisable()
        {
            if (_extrudeGizmo != null)
            {
                _extrudeGizmo.verticalAxisExtrudeSpawn  -= onVerticalAxisExtrudeSpawn;
                _extrudeGizmo.extrudeSpawn              -= onExtrudeSpawn;
            }
        }

        private void OnDestroy()
        {
            ScriptableObjectEx.destroyImmediate(_moveGizmo);
            ScriptableObjectEx.destroyImmediate(_rotationGizmo);
            ScriptableObjectEx.destroyImmediate(_scaleGizmo);
            ScriptableObjectEx.destroyImmediate(_universalGizmo);
            ScriptableObjectEx.destroyImmediate(_extrudeGizmo);
        }
    }
}
#endif