using UnityEngine;

[CreateAssetMenu(fileName = "PlacementToolProfile", menuName = "LabWatcher/PlacementToolProfile", order = 0)]
public class PlacementToolProfile : ScriptableObject
{
    [Header("Grid Snap")]
    public bool enableGridSnap = true;
    public float gridSize = 1.0f;

    [Header("Array")]
    public GameObject arrayPrefab;
    public Vector3 arrayStartPos = Vector3.zero;
    public Vector3 arraySpacing = new Vector3(1, 0, 1);
    public int countX = 5;
    public int countY = 1;
    public int countZ = 5;

    [Header("Random Placement")]
    public GameObject randomPrefab;
    public int randomCount = 10;
    public float positionRange = 5f;
    public float rotationMaxAngle = 30f;
    public float scaleMin = 1f;
    public float scaleMax = 1f;

    [Header("Object Painter")]
    public GameObject paintPrefab;
    public float brushSize = 1f;
    public float brushDensity = 1f;     // 마우스 이동당 몇 개를 찍을지
    public float paintRandomRotationMax = 0f;
    public bool paintUseRandomScale = false;
    public float paintScaleMin = 1f;
    public float paintScaleMax = 1f;

    [Header("Batch Edit")]
    // 예: 일괄 편집용 파라미터(회전, 스케일 등)
    public Vector3 batchPositionOffset = Vector3.zero;
    public Vector3 batchRotationOffset = Vector3.zero;
    public Vector3 batchScaleMultiplier = Vector3.one;

    [Header("Align & Distribute")]
    // 예: 어떤 축으로 정렬할지
    public bool alignX = true;
    public bool alignY = false;
    public bool alignZ = false;

    // [Header("Etc.")]
    // // 그 외 필요한 옵션들
}
