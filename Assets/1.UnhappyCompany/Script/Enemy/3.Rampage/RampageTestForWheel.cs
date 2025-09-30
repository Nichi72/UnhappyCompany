using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class RampageTestForWheel : MonoBehaviour
{
    public Transform target; // 목적지
    public WheelCollider frontLeft, frontRight, rearLeft, rearRight;
    public Transform frontLeftTransform, frontRightTransform, rearLeftTransform, rearRightTransform;
    
    public float maxMotorTorque = 1500f;   // 최대 엔진 토크
    public float maxSteeringAngle = 30f;   // 최대 조향 각도
    public float stoppingDistance = 2f;    // 도착 시 멈추는 거리
    public float waypointThreshold = 2f;   // 경유지 도착 판정 거리

    private Rigidbody rb;
    private NavMeshPath navPath;
    private List<Vector3> waypoints = new List<Vector3>();
    private int currentWaypointIndex = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        navPath = new NavMeshPath();
        UpdatePath();
    }

    void Update()
    {
        if (waypoints.Count == 0) return;

        // 현재 경유지를 가져옴
        Vector3 targetPosition = waypoints[currentWaypointIndex];
        Vector3 direction = targetPosition - transform.position;

        // 목표 지점까지의 거리 확인
        float distanceToWaypoint = direction.magnitude;

        // 현재 waypoint에 도착하면 다음 waypoint로 이동
        if (distanceToWaypoint < waypointThreshold)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Count)
            {
                StopCar();
                return;
            }
        }

        // 차량 방향 조정
        float steering = CalculateSteering(targetPosition);
        float motor = maxMotorTorque;

        // 목표 지점이 가까우면 감속
        if (Vector3.Distance(transform.position, target.position) < stoppingDistance)
        {
            motor = 0;
        }

        ApplyMovement(motor, steering);
    }

    void UpdatePath()
    {
        if (target == null) return;

        NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, navPath);
        waypoints.Clear();

        foreach (var corner in navPath.corners)
        {
            waypoints.Add(corner);
        }

        currentWaypointIndex = 0;
    }

    float CalculateSteering(Vector3 targetPos)
    {
        Vector3 relativeVector = transform.InverseTransformPoint(targetPos);
        float steeringAngle = (relativeVector.x / relativeVector.magnitude) * maxSteeringAngle;
        return steeringAngle;
    }

    void ApplyMovement(float motor, float steering)
    {
        // 앞바퀴 조향
        frontLeft.steerAngle = steering;
        frontRight.steerAngle = steering;

        // 뒷바퀴 구동력 적용
        rearLeft.motorTorque = motor;
        rearRight.motorTorque = motor;

        // 바퀴 모델 회전 반영
        UpdateWheel(frontLeft, frontLeftTransform);
        UpdateWheel(frontRight, frontRightTransform);
        UpdateWheel(rearLeft, rearLeftTransform);
        UpdateWheel(rearRight, rearRightTransform);
    }

    void UpdateWheel(WheelCollider collider, Transform trans)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        trans.position = pos;
        trans.rotation = rot;
    }

    void StopCar()
    {
        rearLeft.motorTorque = 0;
        rearRight.motorTorque = 0;
        frontLeft.steerAngle = 0;
        frontRight.steerAngle = 0;
    }

    public void SetDestination(Transform newTarget)
    {
        target = newTarget;
        UpdatePath();
    }
}
