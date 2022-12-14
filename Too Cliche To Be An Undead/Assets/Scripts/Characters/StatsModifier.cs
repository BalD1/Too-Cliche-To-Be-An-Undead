using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class StatsModifier
{
    private string idName;

    private float maxDuration;
    private float timer;
    private float modifier;

#if UNITY_EDITOR
    public bool showInEditor;
#endif

    public enum E_StatType
    {
        MaxHP,
        Damages,
        AttackRange,
        Attack_CD,
        DASH_CD,
        SKILL_CD,
        Speed,
        CritChances,
    }
    private E_StatType statType;

    public string IDName { get => idName; }

    public float MaxDuration { get => maxDuration; }
    public float Timer { get => timer; }
    public float Modifier { get => modifier; }
    public E_StatType StatType { get => statType; }

    public StatsModifier(string _idName, float _modifier, float _maxDuration, E_StatType _statType)
    {
        this.idName = _idName;
        this.maxDuration = _maxDuration;
        this.timer = _maxDuration;
        modifier = _modifier;
        this.statType = _statType;
    }
    public StatsModifier(string _idName, float _modifier, E_StatType _statType)
    {
        idName = _idName;
        this.maxDuration = -1;
        modifier = _modifier;
        this.statType = _statType;
    }

    public bool Update(float time)
    {
        if (maxDuration < 0) return false;
        
        timer -= time;

        if (timer <= 0) return true;

        return false;
    }

    public void ResetTimer() => timer = maxDuration;

    public void InverseModifier() => modifier *= -1;

    public string TypeToString()
    {
        return TypeToString(StatType);
    }

    public static string TypeToString(E_StatType type)
    {
        string s = "";

        switch (type)
        {
            case E_StatType.MaxHP:
                s = "HEALTH";
                break;

            case E_StatType.Damages:
                s = "DAMAGES";
                break;

            case E_StatType.AttackRange:
                s = "ATTRANGE";
                break;

            case E_StatType.Attack_CD:
                s = "ATTCD";
                break;

            case E_StatType.DASH_CD:
                s = "DASHCD";
                break;

            case E_StatType.SKILL_CD:
                s = "SKILLCD";
                break;

            case E_StatType.Speed:
                s = "SPEED";
                break;

            case E_StatType.CritChances:
                s = "CRIT";
                break;
        }

        return s;
    }

    public static string TypeToString_UI(E_StatType type)
    {
        {
            string s = "";

            switch (type)
            {
                case E_StatType.MaxHP:
                    s = "Max Health";
                    break;

                case E_StatType.Damages:
                    s = "Damages";
                    break;

                case E_StatType.AttackRange:
                    s = "Attack Range";
                    break;

                case E_StatType.Attack_CD:
                    s = "Attack Cooldown";
                    break;

                case E_StatType.DASH_CD:
                    s = "Dash Cooldown";
                    break;

                case E_StatType.SKILL_CD:
                    s = "Skill Cooldown";
                    break;

                case E_StatType.Speed:
                    s = "Movement Speed";
                    break;

                case E_StatType.CritChances:
                    s = "Critical Hit Chances";
                    break;
            }

            return s;
        }
    }
}
