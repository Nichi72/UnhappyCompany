using UnityEngine;

// ��ǥ�� Ÿ���� �����ϴ� ������
public enum TargetType
{
    Transform, // Ư�� Transform�� ����
    Position,  // Ư�� ��ġ�� �̵�
    Tag        // Ư�� �±׸� ���� ������Ʈ�� ã�� �̵�
}

// BaseAIController�� ��ӹ޴� Ŭ����
public class FlexibleAIController : BaseAIController
{
    // ��ǥ Ÿ���� ���� (�⺻��: Transform)
    public TargetType targetType = TargetType.Transform;
    // TargetType�� Tag�� �� ����� �±� �̸�
    public string targetTag;
    // TargetType�� Position�� �� ����� ��ġ
    public Vector3 targetPosition;

    // AI�� ��ǥ�� ������ �� �ִ� ����
    public float detectionRange = 15f;

    // TargetType�� Tag�� ��, ã�� ������Ʈ�� ĳ���� ����
    private GameObject cachedTargetObject;

    // BaseAIController�� �߻� �޼��带 ����
    protected override void CustomUpdate()
    {
        switch (targetType)
        {
            case TargetType.Transform:
                // ��ǥ�� �����ϰ�, ��ǥ���� �Ÿ��� ���� ���� �̳��� ��
                if (target != null && Vector3.Distance(transform.position, target.position) <= detectionRange)
                {
                    agent.SetDestination(target.position); // ��ǥ ��ġ�� �̵�
                }
                break;
            case TargetType.Position:
                // ������ ��ġ���� �Ÿ��� ���� ���� �̳��� ��
                if (Vector3.Distance(transform.position, targetPosition) <= detectionRange)
                {
                    agent.SetDestination(targetPosition); // ������ ��ġ�� �̵�
                }
                break;
            case TargetType.Tag:
                // ĳ�̵� ������Ʈ�� ���� �� �±׷� �˻�
                if (cachedTargetObject == null)
                {
                    cachedTargetObject = GameObject.FindWithTag(targetTag);
                }
                // ������Ʈ�� �����ϰ�, �Ÿ� ������ ������ ��
                if (cachedTargetObject != null && Vector3.Distance(transform.position, cachedTargetObject.transform.position) <= detectionRange)
                {
                    agent.SetDestination(cachedTargetObject.transform.position); // ������Ʈ ��ġ�� �̵�
                }
                break;
        }
    }
}
