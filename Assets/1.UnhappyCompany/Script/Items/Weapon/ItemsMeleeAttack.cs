using System.Collections;
using UnityEngine;

public class ItemsMeleeAttack : Item ,IDamager
{
    public Animator armAnimator;
    public int _damage= 20;
    public int damage { get => _damage; set => _damage = value; }
    public LayerMask DamageLayer { get => damageLayer; set => damageLayer = value; }
    public float Distance { get => distance; set => distance = value; }

    [SerializeField] private LayerMask damageLayer;
    [SerializeField] private float distance;

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
        
        DamageSystem.RaycastDamage(damage, Distance, DamageLayer, DealDamage);
    }

    public void DealDamage(int damage, IDamageable target) 
    {
        target.TakeDamage(damage, DamageType.Physical);
        Debug.Log($"{target.ToString()} Damage! _ Left HP { target.hp}");
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

    public override void Mount(Player player, object state = null)
    {
        base.Mount(player, state);
        if(state != null)
        {
            DeserializeState(state);
        }
        armAnimator = player.armAnimator;

        Rigidbody rd =  GetComponent<Rigidbody>();
        rd.isKinematic = true;
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
