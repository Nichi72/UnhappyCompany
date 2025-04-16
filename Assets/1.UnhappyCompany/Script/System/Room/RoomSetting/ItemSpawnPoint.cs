using UnityEngine;

public class ItemSpawnPoint : MonoBehaviour
{
    public static bool isDrawGizmos = false;
    private void OnDrawGizmos()
    {
        if (isDrawGizmos)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 1f);
                
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position, gameObject.name);
            #endif
        }
    }
}
