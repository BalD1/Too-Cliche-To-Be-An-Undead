using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public abstract bool OnTakeDamages(float amount, bool isCrit = false, bool fakeDamages = false);
    public abstract bool OnTakeDamages(float amount, SCRPT_EntityStats.E_Team damagerTeam, bool isCrit = false, bool fakeDamages = false);

    public abstract void OnHeal(float amount, bool isCrit = false, bool canExceedMaxHP = false);

    public abstract void OnDeath(bool forceDeath = false);

    public bool IsAlive();
}
