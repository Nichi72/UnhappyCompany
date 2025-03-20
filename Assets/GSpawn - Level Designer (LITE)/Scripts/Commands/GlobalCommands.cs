#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

namespace GSpawn_Lite
{
    public class Global_EnableObjectSpawnTool : PluginCommand
    {
        protected override void onEnter()
        {
            GSpawn.active.levelDesignToolId = LevelDesignToolId.ObjectSpawn;
        }
    }

    public class Global_EnableObjectSelectionTool : PluginCommand
    {
        protected override void onEnter()
        {
            GSpawn.active.levelDesignToolId = LevelDesignToolId.ObjectSelection;
        }
    }

    public class Global_EnableObjectEraseTool : PluginCommand
    {
        protected override void onEnter()
        {
            GSpawn.active.levelDesignToolId = LevelDesignToolId.ObjectErase;
        }
    }

    public class Global_Grid_VerticalStepDown : PluginCommand
    {
        protected override void onEnter()
        {
            var grid = PluginScene.instance.grid;
            grid.activeSettings.localOriginYOffset -= grid.activeSettings.cellSizeY;
        }
    }

    public class Global_Grid_VerticalStepUp : PluginCommand
    {
        protected override void onEnter()
        {
            var grid = PluginScene.instance.grid;
            grid.activeSettings.localOriginYOffset += grid.activeSettings.cellSizeY;
        }
    }

    public class Global_Grid_EnableSnapToPickedObject : PluginCommand
    {
        protected override void onEnter()
        {
            ObjectSelection.instance.clickSelectEnabled         = false;
            ObjectSelection.instance.gizmosEnabled              = false;
            ObjectSelection.instance.multiSelectEnabled         = false;
            PluginScene.instance.snapGridToPickedObjectEnabled  = true;
        }

        protected override void onExit()
        {
            ObjectSelection.instance.clickSelectEnabled         = true;
            ObjectSelection.instance.gizmosEnabled              = true;
            ObjectSelection.instance.multiSelectEnabled         = true;
            PluginScene.instance.snapGridToPickedObjectEnabled  = false;
        }
    }

    public class Global_Transform_RotateAroundX : PluginCommand
    {
        protected override void onEnter()
        {
            var toolId = GSpawn.active.levelDesignToolId;
            Vector3 rotationAxis = InputPrefs.instance.getRotationAxis(0);

            if (toolId == LevelDesignToolId.ObjectSpawn) ObjectSpawn.instance.rotateSpawnGuide(rotationAxis, InputPrefs.instance.keyboardXRotationStep);
            else if (toolId == LevelDesignToolId.ObjectSelection) ObjectSelection.instance.rotate(rotationAxis, InputPrefs.instance.keyboardXRotationStep);
        }
    }

    public class Global_Transform_RotateAroundXAroundCenter : PluginCommand
    {
        protected override void onEnter()
        {
            var toolId = GSpawn.active.levelDesignToolId;
            Vector3 rotationAxis = InputPrefs.instance.getRotationAxis(0);

            if (toolId == LevelDesignToolId.ObjectSpawn) ObjectSpawn.instance.rotateSpawnGuide(ObjectSpawn.instance.calcSpawnGuideWorldOBB().center, rotationAxis, InputPrefs.instance.keyboardXRotationStep);
            else if (toolId == LevelDesignToolId.ObjectSelection) ObjectSelection.instance.rotate(ObjectSelection.instance.calcSelectionCenter(), rotationAxis, InputPrefs.instance.keyboardXRotationStep);
        }
    }

    public class Global_Transform_RotateAroundY : PluginCommand
    {
        protected override void onEnter()
        {
            var toolId = GSpawn.active.levelDesignToolId;
            Vector3 rotationAxis = InputPrefs.instance.getRotationAxis(1);

            if (toolId == LevelDesignToolId.ObjectSpawn)
            {
                ObjectSpawn.instance.rotateSpawnGuide(rotationAxis, InputPrefs.instance.keyboardYRotationStep);
            }
            else if (toolId == LevelDesignToolId.ObjectSelection) ObjectSelection.instance.rotate(rotationAxis, InputPrefs.instance.keyboardYRotationStep);
        }
    }

    public class Global_Transform_RotateAroundYAroundCenter : PluginCommand
    {
        protected override void onEnter()
        {
            var toolId = GSpawn.active.levelDesignToolId;
            Vector3 rotationAxis = InputPrefs.instance.getRotationAxis(1);

            if (toolId == LevelDesignToolId.ObjectSpawn) ObjectSpawn.instance.rotateSpawnGuide(ObjectSpawn.instance.calcSpawnGuideWorldOBB().center, rotationAxis, InputPrefs.instance.keyboardYRotationStep);
            else if (toolId == LevelDesignToolId.ObjectSelection) ObjectSelection.instance.rotate(ObjectSelection.instance.calcSelectionCenter(), rotationAxis, InputPrefs.instance.keyboardYRotationStep);
        }
    }

    public class Global_Transform_RotateAroundZ : PluginCommand
    {
        protected override void onEnter()
        {
            var toolId = GSpawn.active.levelDesignToolId;
            Vector3 rotationAxis = InputPrefs.instance.getRotationAxis(2);

            if (toolId == LevelDesignToolId.ObjectSpawn) ObjectSpawn.instance.rotateSpawnGuide(rotationAxis, InputPrefs.instance.keyboardZRotationStep);
            else if (toolId == LevelDesignToolId.ObjectSelection) ObjectSelection.instance.rotate(rotationAxis, InputPrefs.instance.keyboardZRotationStep);
        }
    }

    public class Global_Transform_RotateAroundZAroundCenter : PluginCommand
    {
        protected override void onEnter()
        {
            var toolId = GSpawn.active.levelDesignToolId;
            Vector3 rotationAxis = InputPrefs.instance.getRotationAxis(2);

            if (toolId == LevelDesignToolId.ObjectSpawn) ObjectSpawn.instance.rotateSpawnGuide(ObjectSpawn.instance.calcSpawnGuideWorldOBB().center, rotationAxis, InputPrefs.instance.keyboardZRotationStep);
            else if (toolId == LevelDesignToolId.ObjectSelection) ObjectSelection.instance.rotate(ObjectSelection.instance.calcSelectionCenter(), rotationAxis, InputPrefs.instance.keyboardZRotationStep);
        }
    }

    public class Global_Transform_ResetRotationToOriginal : PluginCommand
    {
        protected override void onEnter()
        {
            var toolId = GSpawn.active.levelDesignToolId;
            if (toolId == LevelDesignToolId.ObjectSpawn) ObjectSpawn.instance.resetSpawnGuideRotationToOriginal();
            else if (toolId == LevelDesignToolId.ObjectSelection) ObjectSelection.instance.resetRotationToOriginal();
        }
    }

    public class Global_Transform_ResetScaleToOriginal : PluginCommand
    {
        protected override void onEnter()
        {
            var toolId = GSpawn.active.levelDesignToolId;
            if (toolId == LevelDesignToolId.ObjectSpawn) ObjectSpawn.instance.resetSpawnGuideScaleToOriginal();
            else if (toolId == LevelDesignToolId.ObjectSelection) ObjectSelection.instance.resetScaleToOriginal();
        }
    }

    public class Global_Selection_FrameSelected : PluginCommand
    {
        protected override void onEnter()
        {
            if (GSpawn.active.levelDesignToolId == LevelDesignToolId.ObjectSelection)
                ObjectSelection.instance.frameSelected();
        }
    }

    public class Global_Selection_DeleteSelected : PluginCommand
    {
        protected override void onEnter()
        {
            if (GSpawn.active.levelDesignToolId == LevelDesignToolId.ObjectSelection)
                ObjectSelection.instance.deleteSelected();
        }
    }

    public class Global_Selection_DuplicateSelected : PluginCommand
    {
        protected override void onEnter()
        {
            if (GSpawn.active.levelDesignToolId == LevelDesignToolId.ObjectSelection)
                ObjectSelection.instance.duplicateSelected();
        }
    }
}
#endif