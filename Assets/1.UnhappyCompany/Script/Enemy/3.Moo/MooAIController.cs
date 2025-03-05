using UnityEngine;
using UnityEngine.AI;

public class MooAIController : EnemyAIController<MooAIData>
{
    public float slimeEmitInterval = 10f;
    private float lastSlimeEmitTime;
    public Animator animator;

    protected override void Start()
    {
        base.Start();
        ChangeState(new MooIdleState(this));
        lastSlimeEmitTime = Time.time;
    }

    protected override void Update()
    {
        base.Update();
        if (Time.time - lastSlimeEmitTime > slimeEmitInterval)
        {
            ChangeState(new MooSlimeEmitState(this));
            lastSlimeEmitTime = Time.time;
        }
    }

    public override void TakeDamage(int damage, DamageType damageType)
    {
        base.TakeDamage(damage, damageType);
        PlayAnimation("HitReaction");
        ChangeState(new MooFleeState(this));
    }

    public void PlayAnimation(string animationName)
    {
        animator.CrossFade(animationName, 0.2f);
    }

    public float SlimeDuration => enemyData.slimeDuration;

    public GameObject SlimePrefab => enemyData.slimePrefab;
} 