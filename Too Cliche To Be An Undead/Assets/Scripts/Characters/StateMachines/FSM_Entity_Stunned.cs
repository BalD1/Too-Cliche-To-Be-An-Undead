using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_Entity_Stunned<T> : FSM_Base<T>
{
    protected Entity owner;
    protected float stun_TIMER;
    protected bool baseConditionChecked;

    public override void EnterState(T stateManager)
    {
        owner.GetRb.velocity = Vector3.zero;
    }

    public override void UpdateState(T stateManager)
    {
        if (stun_TIMER > 0) stun_TIMER -= Time.deltaTime;
    }

    public override void FixedUpdateState(T stateManager)
    {
    }

    public override void ExitState(T stateManager)
    {
        baseConditionChecked = false;
    }

    public override void Conditions(T stateManager)
    {
        if (stun_TIMER <= 0) baseConditionChecked = true;   
    }

    public void SetOwner(Entity _owner) => owner = _owner;

    public FSM_Entity_Stunned<T> SetDuration(float duration, bool resetAttackTimer = false)
    {
        stun_TIMER = duration;
        if (owner != null && resetAttackTimer) owner.ResetAttackTimer();
        return this;
    }

    public override string ToString() => "Stunned";
}
