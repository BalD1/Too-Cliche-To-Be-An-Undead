using BalDUtilities.EditorUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpawnersManager))]
public class ED_SpawnersManager : Editor
{
    private SpawnersManager targetScript;
    private Vector2 spawnerSpawnPos;

    private bool showSpawnerArgs;

    private ElementSpawner.E_ElementToSpawn elementToSpawn = ElementSpawner.E_ElementToSpawn.Coins;
    private bool destroyAfterSpawn;
    private bool spawnAtStart;

    private void OnEnable()
    {
        targetScript = (SpawnersManager)target;

    }

    public override void OnInspectorGUI()
    {
        ReadOnlyDraws.EditorScriptDraw(typeof(ED_GameManager), this);
        DrawDefaultInspector();

        GUILayout.Space(5);

        ElementSpawner sp = null;

        if (GUILayout.Button("Create Spawner"))
        {
            GameObject parent = GameObject.FindGameObjectWithTag("SpawnersContainer");
            sp = (PrefabUtility.InstantiatePrefab(targetScript.Spawner_PF, parent.transform) as GameObject).GetComponent<ElementSpawner>();
            Undo.RegisterCreatedObjectUndo(sp.gameObject, "Create my GameObject");
            sp?.Setup(elementToSpawn, destroyAfterSpawn, spawnAtStart);
        }

        EditorGUI.indentLevel++;

        spawnerSpawnPos = EditorGUILayout.Vector2Field("Spawner Position", spawnerSpawnPos);
        elementToSpawn = (ElementSpawner.E_ElementToSpawn)EditorGUILayout.EnumPopup("Element to spawn", (ElementSpawner.E_ElementToSpawn)elementToSpawn);
        GUILayout.BeginHorizontal();
        destroyAfterSpawn = EditorGUILayout.Toggle("Destroy after spawn", destroyAfterSpawn);
        spawnAtStart = EditorGUILayout.Toggle("Spawn at start", spawnAtStart);
        GUILayout.EndHorizontal();

        EditorGUI.indentLevel--;

        GUILayout.Space(5);
        SimpleDraws.HorizontalLine();
        GUILayout.Space(5);

        if (GUILayout.Button("Setup spawners array"))
        {
            GameObject[] res = GameObject.FindGameObjectsWithTag("ElementSpawner");

            targetScript.SetupArray(res);
        }

        GUILayout.Space(5);
        SimpleDraws.HorizontalLine();
        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Spawn Keycards only"))
            targetScript.ManageKeycardSpawn();
        if (GUILayout.Button("Spawn all"))
            targetScript.ForceSpawnAll();
        GUILayout.EndHorizontal();

        EditorUtility.SetDirty(targetScript);
        EditorUtility.SetDirty(this);
        serializedObject.ApplyModifiedProperties();
    }
}
