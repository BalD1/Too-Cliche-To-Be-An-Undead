using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_Player_InSkill : FSM_Base<FSM_Player_Manager>
{
    private PlayerCharacter owner;
    private float maxTimer;
    private float timer;

    private const float cooldownForCancel = 1.5f;
    private float timerForCancel;

    public override void EnterState(FSM_Player_Manager stateManager)
    {
        owner ??= stateManager.Owner;

        timerForCancel = cooldownForCancel;

        Vector2 mouseDir = stateManager.Owner.Weapon.GetGeneralDirectionOfMouseOrGamepad();

        owner.SetAnimatorArgs(PlayerCharacter.ANIMATOR_ARGS_HORIZONTAL, mouseDir.x);
        owner.SetAnimatorArgs(PlayerCharacter.ANIMATOR_ARGS_VERTICAL, mouseDir.y);
        owner.SetAnimatorArgs(PlayerCharacter.ANIMATOR_ARGS_INSKILL, true);

        owner.GetSkillHolder.Trigger.enabled = true;
        owner.GetSkill.StartSkill(owner);

        owner.D_skillInput += StopSkill;

        owner.canBePushed = true;
    }

    public override void UpdateState(FSM_Player_Manager stateManager)
    {
        timer -= Time.deltaTime;

        if (timerForCancel > 0) timerForCancel -= Time.deltaTime;

        stateManager.OwnerWeapon.SetRotation(owner.GetSkill.AimAtMovements);

        owner.SkillDurationIcon.fillAmount = timer / maxTimer;

        owner.GetSkill.UpdateSkill(owner);
    }

    public override void FixedUpdateState(FSM_Player_Manager stateManager)
    {
        if (owner.GetSkill.CanMove) owner.Movements();
    }

    public override void ExitState(FSM_Player_Manager stateManager)
    {
        owner.GetSkill.StopSkill(owner);
        owner.GetSkillHolder.Trigger.enabled = false;

        owner.ForceUpdateMovementsInput();

        owner.D_skillInput -= StopSkill;

        owner.SetAnimatorArgs(PlayerCharacter.ANIMATOR_ARGS_INSKILL, false);
    }

    private void StopSkill()
    {
        if (timerForCancel > 0) return;

        this.timer = 0;
    }

    public override void Conditions(FSM_Player_Manager stateManager)
    {
        if (timer <= 0) stateManager.SwitchState(stateManager.idleState);
    }

    public FSM_Player_InSkill SetTimer(float _timer)
    {
        timer = _timer;
        maxTimer = _timer;
        return this;
    }
    public override string ToString() => "InSkill";
}
