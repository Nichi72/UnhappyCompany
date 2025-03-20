#if UNITY_EDITOR

namespace GSpawn_Lite
{
    public class ObjectSpawn_EnableModularSnapSpawn : PluginCommand
    {
        protected override void onEnter()
        {
            if (GSpawn.active.levelDesignToolId == LevelDesignToolId.ObjectSpawn)
                ObjectSpawn.instance.activeToolId = ObjectSpawnToolId.ModularSnap;
        }
    }

    public class ObjectSpawn_EnableSegmentsSpawn : PluginCommand
    {
        protected override void onEnter()
        {
            if (GSpawn.active.levelDesignToolId == LevelDesignToolId.ObjectSpawn)
                ObjectSpawn.instance.activeToolId = ObjectSpawnToolId.Segments;
        }
    }

    public class ObjectSpawn_EnableBoxSpawn : PluginCommand
    {
        protected override void onEnter()
        {
            if (GSpawn.active.levelDesignToolId == LevelDesignToolId.ObjectSpawn)
                ObjectSpawn.instance.activeToolId = ObjectSpawnToolId.Box;
        }
    }

    public class ObjectSpawn_EnablePropsSpawn : PluginCommand
    {
        protected override void onEnter()
        {
            if (GSpawn.active.levelDesignToolId == LevelDesignToolId.ObjectSpawn)
                ObjectSpawn.instance.activeToolId = ObjectSpawnToolId.Props;
        }
    }

    public class ObjectSpawn_SpawnGuide_SyncGridCellSize : PluginCommand
    {
        protected override void onEnter()
        {
            if (GSpawn.active.levelDesignToolId == LevelDesignToolId.ObjectSpawn)
            {
                var objectSpawn = ObjectSpawn.instance;
                var toolId      = ObjectSpawn.instance.activeToolId;
                if (toolId == ObjectSpawnToolId.ModularSnap || 
                    (toolId == ObjectSpawnToolId.Segments && !objectSpawn.segmentsObjectSpawn.isBuildingSegments) ||
                    (toolId == ObjectSpawnToolId.Box && !objectSpawn.boxObjectSpawn.isBuildingBox))
                {
                    var spawnGuide = objectSpawn.activeTool.spawnGuide;
                    if (spawnGuide != null) spawnGuide.syncGridCellSizeToPrefabSize();
                }
            }
        }
    }

    public class ObjectSpawn_SpawnGuide_ScrollPrefab : PluginCommand
    {
        protected override void onEnter()
        {
            var toolId = GSpawn.active.levelDesignToolId;
            if (toolId == LevelDesignToolId.ObjectSpawn)
            {
                if ( ObjectSpawn.instance.activeToolId == ObjectSpawnToolId.ModularSnap ||
                    (ObjectSpawn.instance.activeToolId == ObjectSpawnToolId.Segments && !ObjectSpawn.instance.segmentsObjectSpawn.isBuildingSegments) ||
                    (ObjectSpawn.instance.activeToolId == ObjectSpawnToolId.Box && !ObjectSpawn.instance.boxObjectSpawn.isBuildingBox) ||
                     ObjectSpawn.instance.activeToolId == ObjectSpawnToolId.Props)
                {
                    ObjectSpawn.instance.activeTool.enableSpawnGuidePrefabScroll = true;
                }
            }
        }

        protected override void onExit()
        {
            var toolId = GSpawn.active.levelDesignToolId;
            if (toolId == LevelDesignToolId.ObjectSpawn)
            {
                if (ObjectSpawn.instance.activeToolId == ObjectSpawnToolId.ModularSnap ||
                    (ObjectSpawn.instance.activeToolId == ObjectSpawnToolId.Segments && !ObjectSpawn.instance.segmentsObjectSpawn.isBuildingSegments) ||
                    (ObjectSpawn.instance.activeToolId == ObjectSpawnToolId.Box && !ObjectSpawn.instance.boxObjectSpawn.isBuildingBox) ||
                     ObjectSpawn.instance.activeToolId == ObjectSpawnToolId.Props)
                {
                    ObjectSpawn.instance.activeTool.enableSpawnGuidePrefabScroll = false;
                }
            }
        }
    }

    public class ObjectSpawn_SegmentsSpawn_Raise : PluginCommand
    {
        protected override void onEnter()
        {
            if (GSpawn.active.levelDesignToolId == LevelDesignToolId.ObjectSpawn &&
                ObjectSpawn.instance.activeToolId == ObjectSpawnToolId.Segments &&
                ObjectSpawn.instance.segmentsObjectSpawn.isBuildingSegments)
            {
                ObjectSpawn.instance.segmentsObjectSpawn.raiseCurrentHeight();
            }
        }
    }

    public class ObjectSpawn_SegmentsSpawn_Lower : PluginCommand
    {
        protected override void onEnter()
        {
            if (GSpawn.active.levelDesignToolId == LevelDesignToolId.ObjectSpawn &&
                ObjectSpawn.instance.activeToolId == ObjectSpawnToolId.Segments &&
                ObjectSpawn.instance.segmentsObjectSpawn.isBuildingSegments)
            {
                ObjectSpawn.instance.segmentsObjectSpawn.lowerCurrentHeight();
            }
        }
    }

    public class ObjectSpawn_BoxSpawn_Raise : PluginCommand
    {
        protected override void onEnter()
        {
            if (GSpawn.active.levelDesignToolId == LevelDesignToolId.ObjectSpawn &&
                ObjectSpawn.instance.activeToolId == ObjectSpawnToolId.Box &&
                ObjectSpawn.instance.boxObjectSpawn.isBuildingBox)
            {
                ObjectSpawn.instance.boxObjectSpawn.raiseCurrentHeight();
            }
        }
    }

    public class ObjectSpawn_BoxSpawn_Lower : PluginCommand
    {
        protected override void onEnter()
        {
            if (GSpawn.active.levelDesignToolId == LevelDesignToolId.ObjectSpawn &&
                ObjectSpawn.instance.activeToolId == ObjectSpawnToolId.Box &&
                ObjectSpawn.instance.boxObjectSpawn.isBuildingBox)
            {
                ObjectSpawn.instance.boxObjectSpawn.lowerCurrentHeight();
            }
        }
    }
}
#endif