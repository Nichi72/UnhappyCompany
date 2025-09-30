using UnityEngine;

public class FootIK : MonoBehaviour
{
    private Animator animator;
    public LayerMask layerMask;

    // �⺻ ������ (�ִϸ��̼� Ŭ���� ��Ͽ� ���� �� ���)
    public float defaultDistanceGround = 0.1f;
    public float defaultRayStartHeight = 0.5f;

    // �ִϸ��̼Ǻ� Ŀ���� ������ ���� (�ν��z�Ϳ��� ����)
    [System.Serializable]
    public class AnimationSettings
    {
        public string animationName; // �ִϸ��̼� Ŭ�� �̸�
        public float distanceGround; // �ش� �ִϸ��̼ǿ� �Ÿ�
        public float rayStartHeight; // �ش� �ִϸ��̼ǿ� Ray ����
    }

    public AnimationSettings[] animationSettings;

    // ���� ���� ���� ����
    private float currentDistanceGround;
    private float currentRayStartHeight;

    void Start()
    {
        animator = GetComponent<Animator>();
        ApplySettings("Default"); // �⺻ ���� ����
    }

    void Update()
    {
        // ���� ��� ���� �ִϸ��̼� Ŭ�� Ȯ��
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length > 0)
        {
            string currentClipName = clipInfo[0].clip.name;
            ApplySettings(currentClipName); // �ִϸ��̼Ǻ� ���� ����
        }
    }

    void ApplySettings(string animationName)
    {
        // �⺻������ ����
        currentDistanceGround = defaultDistanceGround;
        currentRayStartHeight = defaultRayStartHeight;

        // �ش� �ִϸ��̼ǿ� �´� ���� ã��
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