using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_NZ_Attacking : FSM_Base<FSM_NZ_Manager>
{
    private NormalZombie owner;

    private bool attack_flag;

    private float waitBeforeAttack_TIMER;
    private float attack_TIMER;

    public override void EnterState(FSM_NZ_Manager stateManager)
    {
        owner ??= stateManager.Owner;
        owner.d_EnteredTrigger += OnTrigger;

        waitBeforeAttack_TIMER = owner.DurationBeforeAttack;
        attack_TIMER = owner.Attack_DURATION;
        attack_flag = false;
    }

    public override void UpdateState(FSM_NZ_Manager stateManager)
    {
        if (waitBeforeAttack_TIMER > 0) waitBeforeAttack_TIMER -= Time.deltaTime;
        else if (waitBeforeAttack_TIMER <= 0 && !attack_flag)
        {
            owner.Attack.OnStart(owner);
            attack_flag = true;
        }

        if (attack_flag && attack_TIMER > 0) attack_TIMER -= Time.deltaTime;
    }

    public override void FixedUpdateState(FSM_NZ_Manager stateManager)
    {

    }

    public override void ExitState(FSM_NZ_Manager stateManager)
    {
        owner.Attack.OnExit(owner);
        owner.d_EnteredTrigger -= OnTrigger;
    }

    public override void Conditions(FSM_NZ_Manager stateManager)
    {
        if (attack_TIMER <= 0) stateManager.SwitchState(stateManager.chasingState);
    }

    private void OnTrigger(Collider2D collider)
    {
        PlayerCharacter p = collider.transform.root.GetComponent<PlayerCharacter>();
        if (p == null) return;

        p.OnTakeDamages(owner.GetStats.BaseDamages(owner.StatsModifiers), owner.RollCrit());
    }

    public override string ToString() => "Attacking";
}
