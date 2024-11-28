using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public GameObject[] terrainPrefabs; // ���� ������ �迭
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
                // ������ ���� �������� �����Ͽ� �ν��Ͻ�ȭ
                GameObject terrainTile = Instantiate(
                    terrainPrefabs[Random.Range(0, terrainPrefabs.Length)],
                    new Vector3(x * 1.0f, 0, z * 1.0f),
                    Quaternion.identity
                );
                terrainTile.transform.parent = transform; // ���� ������ �����ϱ� ���� �θ� ����
            }
        }
    }
}
