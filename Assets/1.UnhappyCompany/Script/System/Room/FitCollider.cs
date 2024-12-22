using UnityEngine;

public class FitCollider : MonoBehaviour
{
    private BoxCollider boxCollider;
    private OtherGoundChecker otherGroundChecker;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeBoxCollider();
        InitializeOtherGroundChecker();
        FitToChildren();
    }

    // Called in the editor when the script is loaded or a value is changed in the inspector
    void OnValidate()
    {
        InitializeBoxCollider();
        InitializeOtherGroundChecker();
        FitToChildren();
    }

    // Initializes the BoxCollider component
    private void InitializeBoxCollider()
    {
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }
    }
    private void InitializeOtherGroundChecker()
    {
        otherGroundChecker = GetComponent<OtherGoundChecker>();
        if (otherGroundChecker == null)
        {
            otherGroundChecker = gameObject.AddComponent<OtherGoundChecker>();
            otherGroundChecker.AddGround();
        }
    }

    // Adjusts the BoxCollider to fit all child objects
    [ContextMenu("Fit Collider to Children")]
    void FitToChildren()
    {
        Bounds bounds = new Bounds(transform.position, Vector3.zero);

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            if (renderer.GetComponent<DoorEdge>() == null)
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        boxCollider.center = bounds.center - transform.position;
        boxCollider.size = bounds.size;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
