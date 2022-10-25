using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro.EditorUtilities;
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

}