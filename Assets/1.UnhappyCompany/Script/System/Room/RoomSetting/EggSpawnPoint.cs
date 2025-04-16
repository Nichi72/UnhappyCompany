using UnityEngine;

public class EggSpawnPoint : MonoBehaviour
{
    public static bool isDrawGizmos = false;
    private void OnDrawGizmos()
    {
        if (isDrawGizmos)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1f);
        
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position, gameObject.name);
            #endif
        }
    }
}
