using UnityEngine;

// 목표의 타입을 정의하는 열거형
public enum TargetType
{
    Transform, // 특정 Transform을 따라감
    Position,  // 특정 위치로 이동
    Tag        // 특정 태그를 가진 오브젝트를 찾아 이동
}

// BaseAIController를 상속받는 클래스
public class FlexibleAIController : BaseAIController
{
    // 목표 타입을 지정 (기본값: Transform)
    public TargetType targetType = TargetType.Transform;
    // TargetType이 Tag일 때 사용할 태그 이름
    public string targetTag;
    // TargetType이 Position일 때 사용할 위치
    public Vector3 targetPosition;

    // AI가 목표를 감지할 수 있는 범위
    public float detectionRange = 15f;

    // TargetType이 Tag일 때, 찾은 오브젝트를 캐싱할 변수
    private GameObject cachedTargetObject;

    // BaseAIController의 추상 메서드를 구현
    protected override void CustomUpdate()
    {
        switch (targetType)
        {
            case TargetType.Transform:
                // 목표가 존재하고, 목표와의 거리가 감지 범위 이내일 때
                if (target != null && Vector3.Distance(transform.position, target.position) <= detectionRange)
                {
                    agent.SetDestination(target.position); // 목표 위치로 이동
                }
                break;
            case TargetType.Position:
                // 지정된 위치와의 거리가 감지 범위 이내일 때
                if (Vector3.Distance(transform.position, targetPosition) <= detectionRange)
                {
                    agent.SetDestination(targetPosition); // 지정된 위치로 이동
                }
                break;
            case TargetType.Tag:
                // 캐싱된 오브젝트가 없을 때 태그로 검색
                if (cachedTargetObject == null)
                {
                    cachedTargetObject = GameObject.FindWithTag(targetTag);
                }
                // 오브젝트가 존재하고, 거리 조건을 만족할 때
                if (cachedTargetObject != null && Vector3.Distance(transform.position, cachedTargetObject.transform.position) <= detectionRange)
                {
                    agent.SetDestination(cachedTargetObject.transform.position); // 오브젝트 위치로 이동
                }
                break;
        }
    }
}
