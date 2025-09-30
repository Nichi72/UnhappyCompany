using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Destroyer : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 3f; // 삭제될 시간(초)

    void Start()
    {
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

}
