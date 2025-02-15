using System.Collections;
using UnityEngine;

public class ItemsMeleeAttack : Item ,IDamager
{
    public Animator armAnimator;
    public int damage { get; set; } = 20;


    public override void Use(Player player)
    {
        base.Use(player);
        StartCoroutine(PlayAttackAnimation());
        armAnimator.Play("Attack");
    }
    IEnumerator PlayAttackAnimation()
    {
        yield return new WaitForEndOfFrame(); 
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
    public override void OnDrop()
    {
        base.OnDrop();
        UnMount();
    }

    public override void Mount(Player player)
    {
        base.Mount(player);
        armAnimator = player.armAnimator;

        Rigidbody rd =  GetComponent<Rigidbody>();
        rd.isKinematic = true;

        // Vector3(0.00400000019,0.0949999988,0.108999997)
        // Vector3(356.488678,264.935211,265.85611)
        // Vector3(0.135276109,0.223078802,0.24817732)

        
        transform.localPosition = itemData.ItemPosition;
        transform.localRotation = Quaternion.Euler(itemData.ItemRotation);
        transform.localScale = itemData.ItemScale;
        
        int layerIndex = armAnimator.GetLayerIndex("MeleeAttack");
        armAnimator.SetLayerWeight(layerIndex, 1);
    }
    public override void UnMount()
    {
        base.UnMount();
        Debug.Log("UnMount");
        int layerIndex = armAnimator.GetLayerIndex("MeleeAttack");
        armAnimator.SetLayerWeight(layerIndex, 0);
    }
}
