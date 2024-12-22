using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(EnemyManager.instance != null)
        {
            EnemyManager.instance.spawnPoints.Add(transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
