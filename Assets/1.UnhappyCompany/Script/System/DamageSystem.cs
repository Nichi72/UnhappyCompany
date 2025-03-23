using System;
using UnityEngine;

public static class DamageSystem 
{
    public static void RaycastDamage(int damage, float distance, LayerMask damageLayer , Action<int, IDamageable> damageAction)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance, damageLayer))
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageAction?.Invoke(damage, damageable);
            }
        }
    }
    
}
