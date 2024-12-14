using System.Collections;
using UnityEngine;

public class ItemsMeleeAttack : Item ,IDamager
{
    public Animator animator;
    
    public int damage { get; set; } = 20;

    public override void Use()
    {
        base.Use();
        StartCoroutine(PlayAttackAnimation());
    }
    IEnumerator PlayAttackAnimation()
    {
        yield return new WaitForEndOfFrame(); // 
        animator.Play("AttackAnimation");
    }

    private void Update()
    {
        //if(Input.GetKeyDown(KeyCode.Mouse1))
        //{
        //    animator.Play("AttackAnimation");
        //}
    }

    public void AniEvt_Attack()
    {
        Attack();
    }

    public void Attack()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        var interactionSystemTemp = MyUtility.ComponentUtils.GetAllComponentsInParents<InteractionSystem>(gameObject,true)[0];

        // ���̾ ���� Raycast ����
        if (Physics.Raycast(ray, out hit, interactionSystemTemp.raycastMaxDistance, interactionSystemTemp.interactionLayer))
        {
            var damageAbleTemp = hit.transform.GetComponent<IDamageable>();
            if (damageAbleTemp == null)
            {
                return;
            }
            DealDamage(damageAbleTemp);
        }
    }

    public void DealDamage(IDamageable target)
    {
        target.TakeDamage(damage, DamageType.Physical);
        Debug.Log($"{target.ToString()} Damage! _ Left HP { target.hp}");
    }

    public override void PickUp()
    {
        base.PickUp();
    }
}
