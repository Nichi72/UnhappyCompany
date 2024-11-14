using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Spawning")]
    [Tooltip("�� ���� ��ġ ����Ʈ")] public List<Transform> spawnPoints; // �� ���� ��ġ ����Ʈ
    [Tooltip("�� ������ �� ������")] public GameObject eggPrefab; // �� ������ �� ������
    [Tooltip("��ü ������ �� ������")] public GameObject adultEnemyPrefab; // ��ü ������ �� ������
    [Tooltip("���� ������ Ȯ�� (0���� 1 ������ ��)")] public float spawnChance = 0.5f; // ���� ������ Ȯ�� (0���� 1 ������ ��)
    [Tooltip("���� ��ü�� ��ȭ�ϴ� �� �ɸ��� �ð�")] public float eggHatchTime = 5.0f; // ���� ��ü�� ��ȭ�ϴ� �� �ɸ��� �ð�

    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        // �� ���� ��ġ�� ���� ���� �������� ����
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (Random.value <= spawnChance)
            {
                GameObject egg = Instantiate(eggPrefab, spawnPoint.position, Quaternion.identity);
                activeEnemies.Add(egg);
                StartCoroutine(HatchEgg(egg));
            }
        }
    }

    IEnumerator HatchEgg(GameObject egg)
    {
        // ���� �ð��� ���� �� ���� ��ȭ��Ŵ
        yield return new WaitForSeconds(eggHatchTime);

        if (egg != null) // ���� ���� �����ϴ��� Ȯ��
        {
            Vector3 eggPosition = egg.transform.position;
            Destroy(egg); // ���� �ı�
            GameObject adult = Instantiate(adultEnemyPrefab, eggPosition, Quaternion.identity); // ��ü�� ��ȭ
            activeEnemies.Add(adult);
            EnemyBehaviorFSM enemyBehavior = adult.GetComponent<EnemyBehaviorFSM>();
            if (enemyBehavior != null)
            {
                enemyBehavior.ChangeState(EnemyState.Patrolling);
            }
        }
    }
}

public enum EnemyState
{
    Idle, // ��� ����
    Patrolling, // ���� ����
    Charging, // ���� ����
    Dead // ���� ����
}