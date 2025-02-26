using UnityEngine;
using UnityEngine.AI;

public class PathCalculator : MonoBehaviour
{
    public Transform target;
    public NavMeshPath path;
    private int currentPathIndex = 0;
    public float moveSpeed = 3.0f;
    public float minDistanceToPoint = 0.5f;

    void Start()
    {
        path = new NavMeshPath();
        CalculateNewPath();
    }

    public void CalculateNewPath()
    {
        if (target != null)
        {
            NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);
            currentPathIndex = 0;
        }
    }
} 