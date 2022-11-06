using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBaseWeapon : PlayerWeapon
{
    [SerializeField] private float maxRange = 2f;
    [SerializeField] private float lastAttackDamagesMultiplier = 1.5f;

    [SerializeField] private GameObject effectObject;

    protected override void Awake()
    {
        base.Awake();
        effectObject ??= this.transform.GetComponentInChildren<GameObject>();
    }

    public override void StartWeaponAttack(bool isLastAttack)
    {
        base.StartWeaponAttack(isLastAttack);
        hitEntities = Physics2D.OverlapCircleAll(effectObject.transform.position, owner.GetStats.AttackRange(owner.StatsModifiers), damageablesLayer);
        foreach (var item in hitEntities)
        {
            var damageable = item.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                float damages = owner.GetStats.BaseDamages(owner.StatsModifiers);

                if (isLastAttack) damages *= lastAttackDamagesMultiplier;

                if (damageable.OnTakeDamages(damages, owner.GetStats.Team, owner.RollCrit()) == false)
                    continue;

                SuccessfulHit(item.transform.position);
            }
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!debugMode || owner == null) return;

        Gizmos.DrawWireSphere(this.transform.position, owner.GetStats.AttackRange(owner.StatsModifiers));
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(owner.transform.position, maxRange);
#endif
    }
}
