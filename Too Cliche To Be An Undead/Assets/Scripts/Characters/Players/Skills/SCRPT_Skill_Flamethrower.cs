using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable/Entity/Skills/Flamethrower")]
public class SCRPT_Skill_Flamethrower : SCRPT_Skill
{
    private const string inTriggerTickDamages_ID = "SKILL_FT";

    private List<Entity> entitiesInTrigger = new List<Entity>();

    [SerializeField] private float tickDamagesMultiplier = .5f;
    private float tickDamages;

    public override void StartSkill(PlayerCharacter owner)
    {
        isInUse = true; 
        owner.GetSkillHolder.D_enteredTrigger += EnteredTrigger;
        owner.GetSkillHolder.D_exitedTrigger += ExitedTrigger;

        owner.GetSkillHolder.GetComponent<SpriteRenderer>().sortingLayerName = layerName.ToString();
        owner.GetSkillHolder.GetAnimator.Play(animationToPlay);

        owner.SkillTutoAnimator.SetTrigger(skillTutoAnimatorName);

        finalDamages = owner.maxDamages_M * damagesPercentageModifier;
        tickDamages = finalDamages * tickDamagesMultiplier;
    }

    public override void UpdateSkill(PlayerCharacter owner)
    {
        owner.OffsetSkillHolder(offset);
        owner.RotateSkillHolder();
    }

    public override void StopSkill(PlayerCharacter owner)
    {
        owner.GetSkillHolder.GetAnimator.SetTrigger("EndSkill");
        owner.GetSkillHolder.AnimationEnded();
        owner.GetSkillHolder.StartTimer();
        isInUse = false;

        owner.SkillTutoAnimator.SetTrigger("finish");
    }

    public void EnteredTrigger(Entity entity)
    {
        if (GameManager.IsInLayerMask(entity.gameObject, entitiesToAffect) == false) return;

        if (entity as EnemyBase != null)
            entitiesInTrigger.Add(entity);

        TickDamages appliedTickDamages = entity.GetAppliedTickDamages(inTriggerTickDamages_ID);

        if (appliedTickDamages == null)
            entity.AddTickDamages(inTriggerTickDamages_ID, finalDamages, .5f, 2f, true);
        else
        {
            appliedTickDamages.ResetTimer();
            appliedTickDamages.ModifyDamages(finalDamages);
        }
    }

    public void ExitedTrigger(Entity entity)
    {
        if (GameManager.IsInLayerMask(entity.gameObject, entitiesToAffect) == false) return;

        if (entity as EnemyBase != null)
            entitiesInTrigger.Remove(entity);

        TickDamages appliedTickDamages = entity.GetAppliedTickDamages(inTriggerTickDamages_ID);

        if (appliedTickDamages != null)
        {
            appliedTickDamages.ResetTimer();
            appliedTickDamages.ModifyDamages(tickDamages);
        }
    }
}
