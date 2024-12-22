using UnityEngine;

public class CreateCollider : MonoBehaviour
{
    private BoxCollider boxCollider;

    void Start()
    {
        InitializeBoxCollider();
        FitToChildren();
        DestroyImmediate(this);
    }

    // Called in the editor when the script is loaded or a value is changed in the inspector
    void OnValidate()
    {
        InitializeBoxCollider();
        FitToChildren();
    }
    private void InitializeBoxCollider()
    {
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
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
}
