using System.Collections;
using UnityEngine;

public class ItemsMeleeAttack : Item ,IDamager
{
    public Animator animator;
    
    public int damage { get; set; } = 20;

    public override void Use(Player player)
    {
        base.Use(player);
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
        // Vector3 attackDirection = Camera.main.transform.forward;
        // transform.forward = attackDirection;
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
        Debug.Log($"{target.ToString()} Damage! _ Left HP { target.Hp}");
    }

    public override void PickUp(Player player)
    {
        base.PickUp(player);    
    }
}
