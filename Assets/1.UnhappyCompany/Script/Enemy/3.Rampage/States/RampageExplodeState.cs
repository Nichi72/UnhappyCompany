using UnityEngine;

public class RampageExplodeState : IState
{
    private RampageAIController controller;
    private bool exploded = false;

    public RampageExplodeState(RampageAIController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        Debug.Log("Rampage: Explode(자폭) 상태 시작");
        DoExplode();
    }

    public void ExecuteMorning()
    {
    }

    public void ExecuteAfternoon()
    {
    }

    public void Exit()
    {
        Debug.Log("Rampage: Explode 상태 종료");
    }

    public void ExecuteFixedMorning() { }
    public void ExecuteFixedAfternoon() { }

    private void DoExplode()
    {
        if (exploded) return;
        exploded = true;

        // 폭발 범위 내 대상에게 대미지
        Collider[] hitColliders = Physics.OverlapSphere(controller.transform.position, controller.ExplodeRadius);
        foreach (var hit in hitColliders)
        {
            // TODO: 플레이어, 오브젝트 등에 폭발 대미지
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(controller.ExplodeDamage, DamageType.Nomal);
            }
        }

        // 폭발 이펙트, 사운드 등
        // TODO: Instantiate(폭발 이펙트), 사운드 재생

        // 자폭 후 곧바로 제거
        GameObject.Destroy(controller.gameObject);
    }
} 