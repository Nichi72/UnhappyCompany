using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Egg : MonoBehaviour , IDamageable
{
    [Tooltip("알이 부서졌을 때 생성될 아이템 프리팹")] public GameObject eggShatterItemPrefab; // 알이 부서졌을 때 생성될 아이템 프리팹
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
        // 알 파괴 시 아이템 생성
        Instantiate(eggShatterItemPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    void SetEnemy()
    {
        var enemy = Instantiate(enemyPrefab);
        enemy.transform.position = transform.position;
        
    }
}