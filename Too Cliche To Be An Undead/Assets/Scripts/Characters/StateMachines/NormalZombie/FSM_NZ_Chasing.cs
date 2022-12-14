using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_NZ_Chasing : FSM_Base<FSM_NZ_Manager>
{
    private NormalZombie owner;
    private Vector2 goalPosition;

    public override void EnterState(FSM_NZ_Manager stateManager)
    {
        owner ??= stateManager.Owner;
        owner.ResetVelocity();

        owner.canBePushed = true;
    }

    public override void UpdateState(FSM_NZ_Manager stateManager)
    {
        if (owner.Pathfinding != null)
            goalPosition = owner.Pathfinding.CheckWayPoint();
    }

    public override void FixedUpdateState(FSM_NZ_Manager stateManager)
    {
        //owner.GetRb.AddForce(owner.Pathfinding.CheckWayPoint() * owner.GetStats.Speed * owner.SpeedMultiplier * Time.fixedDeltaTime);
        stateManager.Movements(goalPosition);
    }

    public override void ExitState(FSM_NZ_Manager stateManager)
    {
        owner.GetRb.velocity = Vector2.zero;
    }

    public override void Conditions(FSM_NZ_Manager stateManager)
    {
        if (WanderingConditions())
            stateManager.SwitchState(stateManager.wanderingState);

        if (owner.CurrentPlayerTarget == null) return;

        if (stateManager.AttackConditions())
            stateManager.SwitchState(stateManager.attackingState);

        if (owner.CurrentPlayerTarget.Attackers.Count >= GameManager.MaxAttackers)
        {
            foreach (var item in owner.DetectedPlayers)
            {
                if (item.Attackers.Count < GameManager.MaxAttackers) owner.SetTarget(item);
            }
        }
    }

    private bool WanderingConditions() => owner.DetectedPlayers.Count == 0 && 
                                          Vector2.Distance(owner.transform.position, owner.CurrentPositionTarget) <= owner.DistanceBeforeStop;

    public override string ToString() => "Chasing";
}
