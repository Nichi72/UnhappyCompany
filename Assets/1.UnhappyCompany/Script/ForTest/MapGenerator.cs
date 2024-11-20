using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public GameObject[] terrainPrefabs; // 지형 프리팹 배열
    public int mapWidth = 10;
    public int mapHeight = 10;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapHeight; z++)
            {
                // 랜덤한 지형 프리팹을 선택하여 인스턴스화
                GameObject terrainTile = Instantiate(
                    terrainPrefabs[Random.Range(0, terrainPrefabs.Length)],
                    new Vector3(x * 1.0f, 0, z * 1.0f),
                    Quaternion.identity
                );
                terrainTile.transform.parent = transform; // 계층 구조를 유지하기 위해 부모 설정
            }
        }
    }
}
