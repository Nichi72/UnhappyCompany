using UnityEngine;

public class FootIK : MonoBehaviour
{
    private Animator animator;
    public LayerMask layerMask;

    // 기본 보정값 (애니메이션 클립이 목록에 없을 때 사용)
    public float defaultDistanceGround = 0.1f;
    public float defaultRayStartHeight = 0.5f;

    // 애니메이션별 커스텀 보정값 설정 (인스펮터에서 편집)
    [System.Serializable]
    public class AnimationSettings
    {
        public string animationName; // 애니메이션 클립 이름
        public float distanceGround; // 해당 애니메이션용 거리
        public float rayStartHeight; // 해당 애니메이션용 Ray 높이
    }

    public AnimationSettings[] animationSettings;

    // 현재 적용 중인 설정
    private float currentDistanceGround;
    private float currentRayStartHeight;

    void Start()
    {
        animator = GetComponent<Animator>();
        ApplySettings("Default"); // 기본 설정 적용
    }

    void Update()
    {
        // 현재 재생 중인 애니메이션 클립 확인
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length > 0)
        {
            string currentClipName = clipInfo[0].clip.name;
            ApplySettings(currentClipName); // 애니메이션별 설정 적용
        }
    }

    void ApplySettings(string animationName)
    {
        // 기본값으로 시작
        currentDistanceGround = defaultDistanceGround;
        currentRayStartHeight = defaultRayStartHeight;

        // 해당 애니메이션에 맞는 설정 찾기
        foreach (AnimationSettings setting in animationSettings)
        {
            if (setting.animationName == animationName)
            {
                currentDistanceGround = setting.distanceGround;
                currentRayStartHeight = setting.rayStartHeight;
                break;
            }
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator)
        {
            // Left Foot
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);

            Vector3 leftStartPoint = animator.GetIKPosition(AvatarIKGoal.LeftFoot) + (Vector3.up * currentRayStartHeight);
            Ray leftRay = new Ray(leftStartPoint, Vector3.down);

            if (Physics.Raycast(leftRay, out RaycastHit leftHit, currentRayStartHeight + currentDistanceGround + 0.1f, layerMask))
            {
                if (leftHit.transform.CompareTag("WalkableGround"))
                {
                    Vector3 footPosition = leftHit.point;
                    footPosition.y += currentDistanceGround;

                    animator.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);
                    animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, leftHit.normal));
                }
            }

            // Right Foot
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);

            Vector3 rightStartPoint = animator.GetIKPosition(AvatarIKGoal.RightFoot) + (Vector3.up * currentRayStartHeight);
            Ray rightRay = new Ray(rightStartPoint, Vector3.down);

            if (Physics.Raycast(rightRay, out RaycastHit rightHit, currentRayStartHeight + currentDistanceGround + 0.1f, layerMask))
            {
                if (rightHit.transform.CompareTag("WalkableGround"))
                {
                    Vector3 footPosition = rightHit.point;
                    footPosition.y += currentDistanceGround;

                    animator.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);
                    animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, rightHit.normal));
                }
            }
        }
    }
}