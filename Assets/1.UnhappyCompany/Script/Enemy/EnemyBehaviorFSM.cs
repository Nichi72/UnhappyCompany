using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class EnemyBehaviorFSM : MonoBehaviour
{
    [Tooltip("���� ������ �ݰ�")] public float patrolRadius = 5.0f; // ���� �ݰ�
    [Tooltip("���� �̵� �ӵ�")] public float moveSpeed = 2.0f; // �̵� �ӵ�
    [Tooltip("�÷��̾ �����ϴ� �ݰ�")] public float detectionRadius = 10.0f; // �÷��̾� ���� �ݰ�

    private Vector3 patrolPoint;
    private GameObject player;
    private EnemyState currentState;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        ChangeState(EnemyState.Idle);
    }

    void Update()
    {
        // ���� ���¿� ���� �ൿ ����
        switch (currentState)
        {
            case EnemyState.Idle:
                // ��� ���� ����
                ChangeState(EnemyState.Patrolling);
                break;
            case EnemyState.Patrolling:
                Patrol();
                if (player != null && Vector3.Distance(transform.position, player.transform.position) <= detectionRadius)
                {
                    ChangeState(EnemyState.Charging);
                }
                break;
            case EnemyState.Charging:
                ChargePlayer();
                break;
            case EnemyState.Dead:
                // ���� ���� ����
                break;
        }
    }

    public void ChangeState(EnemyState newState)
    {
        currentState = newState;

        // ���� ��ȯ �� �ʱ�ȭ �۾� ����
        switch (currentState)
        {
            case EnemyState.Idle:
                // ��� ���� �ʱ�ȭ
                break;
            case EnemyState.Patrolling:
                SetRandomPatrolPoint();
                break;
            case EnemyState.Charging:
                // ���� ���� �ʱ�ȭ
                break;
            case EnemyState.Dead:
                // ���� ó�� ����
                Destroy(gameObject);
                break;
        }
    }

    void SetRandomPatrolPoint()
    {
        // ������ ������ ���� ����
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y; // ������ ���� ����
        patrolPoint = randomDirection;
    }

    void Patrol()
    {
        // ���� ���� ����
        transform.position = Vector3.MoveTowards(transform.position, patrolPoint, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, patrolPoint) < 0.1f)
        {
            SetRandomPatrolPoint();
        }
    }

    void ChargePlayer()
    {
        // �÷��̾�� ����
        if (player != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, moveSpeed * 2 * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerWeapon"))
        {
            // �÷��̾� ���⿡ ������ ���� ���·� ��ȯ
            ChangeState(EnemyState.Dead);
        }
    }
}