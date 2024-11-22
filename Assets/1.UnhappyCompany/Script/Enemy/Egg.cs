using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Egg : MonoBehaviour , IDamageable
{
    [Tooltip("���� �μ����� �� ������ ������ ������")] public GameObject eggShatterItemPrefab; // ���� �μ����� �� ������ ������ ������
    public GameObject enemyPrefab;
    public int hp { get; set; } = 100;

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log($"{gameObject.name} Take Damage {damage} _ Left HP :{hp}");
        if(hp<=0)
        {
            DestroyEgg();
        }
    }

    void DestroyEgg()
    {
        // �� �ı� �� ������ ����
        Instantiate(eggShatterItemPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    void SetEnemy()
    {
        var enemy = Instantiate(enemyPrefab);
        enemy.transform.position = transform.position;
        
    }
}