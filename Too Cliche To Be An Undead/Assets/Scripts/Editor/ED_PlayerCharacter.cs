using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BalDUtilities.EditorUtils;
using BalDUtilities.Misc;

[CustomEditor(typeof(PlayerCharacter))]
public class ED_PlayerCharacter : Editor
{
    private PlayerCharacter targetScript;

    private bool showDefaultInspector;
    private bool showComponents = true;
    private bool showState = true;
    private bool showStats = true;
    private bool showMisc = true;

    private bool showSrptStats;

    private float damagesAmount = 50;
    private bool critDamages;
    private float healAmount = 50;
    private bool critHeal;

    private enum E_PlayerStates
    {
        Idle,
        Moving,
    }
    private E_PlayerStates stateToForce;

    private void OnEnable()
    {
        targetScript = (PlayerCharacter)target;
    }

    public override void OnInspectorGUI()
    {
        showDefaultInspector = EditorGUILayout.Toggle("Show Default Inspector", showDefaultInspector);

        if (showDefaultInspector)
        {
            ReadOnlyDraws.EditorScriptDraw(typeof(ED_PlayerCharacter), this);
            base.OnInspectorGUI();
            return;
        }

        ReadOnlyDraws.EditorScriptDraw(typeof(ED_PlayerCharacter), this);
        ReadOnlyDraws.ScriptDraw(typeof(PlayerCharacter), targetScript, true);

        DrawComponents();
        DrawState();
        DrawStats();
        DrawMisc();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawComponents()
    {
        EditorGUILayout.Space(5);
        if (GUILayout.Button("Components", ButtonToLabelStyle())) showComponents = !showComponents;
        if (!showComponents) return;

        EditorGUILayout.BeginVertical("GroupBox");

        SerializedProperty rb = serializedObject.FindProperty("rb");
        EditorGUILayout.PropertyField(rb);

        SerializedProperty sprite = serializedObject.FindProperty("sprite");
        EditorGUILayout.PropertyField(sprite);

        SerializedProperty hitMaterial = serializedObject.FindProperty("hitMaterial");
        EditorGUILayout.PropertyField(hitMaterial);

        SerializedProperty animator = serializedObject.FindProperty("animator");
        EditorGUILayout.PropertyField(animator);

        EditorGUILayout.EndVertical();
    }

    private void DrawState()
    {
        EditorGUILayout.Space(5);

        if (GUILayout.Button("State", ButtonToLabelStyle())) showState = !showState;
        if (!showState) return;

        EditorGUILayout.BeginVertical("GroupBox");

        SerializedProperty stateManager = serializedObject.FindProperty("stateManager");
        EditorGUILayout.PropertyField(stateManager);

        EditorGUILayout.LabelField("Current State", targetScript.StateManager.ToString());


        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Force new state", GUILayout.MaxWidth(150)))
        {
            FSM_Player_Manager playerManager = targetScript.StateManager;
            switch (stateToForce)
            {
                case E_PlayerStates.Idle:
                    playerManager.SwitchState(playerManager.idleState);
                    break;

                case E_PlayerStates.Moving:
                    playerManager.SwitchState(playerManager.movingState);
                    break;
            }
        }
        stateToForce = (E_PlayerStates)EditorGUILayout.EnumPopup(stateToForce, GUILayout.MaxWidth(100));

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void DrawStats()
    {
        EditorGUILayout.Space(5);

        if (GUILayout.Button("Stats", ButtonToLabelStyle())) showStats = !showStats;
        if (!showStats) return;

        EditorGUILayout.BeginVertical("GroupBox");

        SerializedProperty stats = serializedObject.FindProperty("stats");
        EditorGUILayout.PropertyField(stats);

        GUI.enabled = false;

        EditorGUILayout.LabelField("Current HP : " + targetScript.CurrentHP + " / " + targetScript.GetStats.MaxHP + "(" + targetScript.CurrentHP / targetScript.GetStats.MaxHP * 100 + "%)");

        GUIStyle style = new GUIStyle(EditorStyles.foldout);
        style.fixedWidth = 0;
        EditorGUI.indentLevel++;
        showSrptStats = EditorGUILayout.Foldout(showSrptStats, "", style);
        EditorGUI.indentLevel--;

        if (showSrptStats)
        {
            SCRPT_EntityStats playerStats = targetScript.GetStats;

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical("GroupBox");

            EditorGUILayout.TextField("Entity Type", playerStats.EntityType);
            EditorGUILayout.FloatField("Max HP", playerStats.MaxHP);
            EditorGUILayout.FloatField("Base Damages", playerStats.BaseDamages);
            EditorGUILayout.FloatField("Attack Range", playerStats.AttackRange);
            EditorGUILayout.FloatField("Attack Cooldown", playerStats.Attack_COOLDOWN);
            EditorGUILayout.FloatField("Invincibility Cooldown", playerStats.Invincibility_COOLDOWN);
            EditorGUILayout.FloatField("Speed", playerStats.Speed);
            EditorGUILayout.IntField("Crit Chances", playerStats.CritChances);
            EditorGUILayout.TextField("Team", EnumsExtension.EnumToString(playerStats.Team));

            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        GUI.enabled = true;

        EditorGUILayout.EndVertical();
    }

    private void DrawMisc()
    {
        EditorGUILayout.Space(5);

        if (GUILayout.Button("Misc", ButtonToLabelStyle())) showMisc = !showMisc;
        if (!showMisc) return;

        EditorGUILayout.BeginVertical("GroupBox");

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Damage", GUILayout.MaxWidth(70)))
            targetScript.OnTakeDamages(damagesAmount, critDamages);
        damagesAmount = EditorGUILayout.FloatField(damagesAmount, GUILayout.MaxWidth(200));
        critDamages = EditorGUILayout.Toggle(critDamages);

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Heal", GUILayout.MaxWidth(70)))
            targetScript.OnHeal(healAmount, critHeal);
        healAmount = EditorGUILayout.FloatField(healAmount, GUILayout.MaxWidth(200));
        critHeal = EditorGUILayout.Toggle(critHeal);

        EditorGUILayout.EndHorizontal();

        GUI.enabled = false;
        EditorGUILayout.Space(5);
        EditorGUILayout.Vector2Field("Input Velocity", targetScript.Velocity);
        EditorGUILayout.Vector2Field("RB Velocity", targetScript.GetRb.velocity);
        GUI.enabled = true;

        EditorGUILayout.EndVertical();
    }

    private GUIStyle ButtonToLabelStyle()
    {
        var s = new GUIStyle();
        var b = s.border;
        b.left = 0;
        b.top = 0;
        b.right = 0;
        b.bottom = 0;
        s.fontStyle = FontStyle.Bold;
        s.normal.textColor = Color.white;
        return s;
    }
}
