using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable/Entity/Skills/Smokescreen")]
public class SCRPT_Skill_Smokescreen : SCRPT_Skill
{
    [SerializeField] private float stunDuration;

    public override void Use(PlayerCharacter owner)
    {
        owner.OffsetSkillHolder(offset);
        owner.GetSkillHolder.GetAnimator.Play(animationToPlay);
        owner.GetSkillHolder.StartTimer(cooldown);

        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(owner.GetSkillHolder.transform.position, range, entitiesToAffect);

        Entity currentEntity;

        foreach (var item in hitTargets)
        {
            currentEntity = item.GetComponentInParent<Entity>();

            bool isFromSameTeam = currentEntity.GetStats.Team.Equals(owner.GetStats.Team);
            if (isFromSameTeam) continue;

            currentEntity.Stun(stunDuration);
        }
    }
}