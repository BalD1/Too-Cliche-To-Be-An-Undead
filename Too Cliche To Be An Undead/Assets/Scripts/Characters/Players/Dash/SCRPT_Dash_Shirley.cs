using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Shirley", menuName = "Scriptable/Entity/Dash/Shirley")]
public class SCRPT_Dash_Shirley : SCRPT_Dash
{
    public const string MODIF_ID = "SH_SKILL_CRIT_20";

    public override void OnDashStart(PlayerCharacter owner)
    {
    }

    public override void OnDashUpdate(PlayerCharacter owner)
    {
    }

    public override void OnDashStop(PlayerCharacter owner)
    {
        if (owner.SearchModifier(MODIF_ID) == null)
            owner.AddModifier(MODIF_ID, value: 20, time: 1, type: StatsModifier.E_StatType.CritChances);
    }
}