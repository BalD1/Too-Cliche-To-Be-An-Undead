using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BalDUtilities.Misc;
using static GameManager;

public class GameManager : MonoBehaviour
{

    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null) Debug.LogError("GameManager Instance not found.");

            return instance;
        }
    }

    [SerializeField] private PlayerCharacter playerRef;
    public static PlayerCharacter PlayerRef { get => Instance.playerRef; }

    [SerializeField] private Shop shop;
    public Shop GetShop { get => shop; }

    [System.Serializable]
    public class PlayersByName
    {
        public string playerName;
        public PlayerCharacter playerScript;
    }

    public List<PlayersByName> playersByName;

    public delegate void D_OnSceneReload();
    public D_OnSceneReload _onSceneReload;

    public bool hasKey;

    public static int MaxAttackers = 5;

    #region GameStates

    public enum E_GameState
    {
        MainMenu,
        InGame,
        Pause,
        Win,
        GameOver
    }

    private E_GameState gameState;
    public E_GameState GameState
    {
        get => gameState;
        set
        {
            gameState = value;

            ProcessStateChange(value);

            if (UIManager.Instance)
                UIManager.Instance.WindowsManager(value);
        }
    }

    private void ProcessStateChange(E_GameState newState)
    {
        switch (newState)
        {
            case E_GameState.MainMenu:
                break;

            case E_GameState.InGame:
                Time.timeScale = 1;
                break;

            case E_GameState.Pause:
                Time.timeScale = 0;
                break;

            case E_GameState.Win:
                Time.timeScale = 0;
                break;

            case E_GameState.GameOver:
                Time.timeScale = 0;
                break;

            default:
                Debug.LogError(newState + "not found in switch statement.");
                break;
        }
    }

    #endregion

    public enum E_ScenesNames
    {
        MainMenu,
        MainScene,
        T_flo,
        Test_Kankan,
    }

    public static float gameTimeSpeed = 1f;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (DataKeeper.Instance.IsPlayerDataKeepSet())
        {
            foreach (var item in DataKeeper.Instance.playersDataKeep)
            {
                var p = new PlayersByName();
                p.playerName = item.playerName;
                playersByName.Add(p);
            }
        }    
        InitState();
    }

    private void InitState()
    {
        if (CompareCurrentScene(E_ScenesNames.MainMenu)) GameState = E_GameState.MainMenu;
        else if (CompareCurrentScene(E_ScenesNames.MainScene)) GameState = E_GameState.InGame;
#if UNITY_EDITOR
        else if (CompareCurrentScene(E_ScenesNames.T_flo)) GameState = E_GameState.InGame;
        else if (CompareCurrentScene(E_ScenesNames.Test_Kankan)) GameState = E_GameState.InGame;
#endif
    }

    public void HandlePause()
    {
        if (UIManager.Instance.OpenMenusQueues.Count > 0)
        {
            UIManager.Instance.CloseYoungerMenu();
            return;
        }

        if (GameState.Equals(E_GameState.InGame))
        {
            GameState = E_GameState.Pause;
        }
    }
    
    public int SetPlayerIndex(PlayerCharacter newPlayer)
    {
        foreach (var item in playersByName)
        {
            if (item.playerName.Equals(newPlayer.name))
            {
                item.playerScript = newPlayer;
                return playersByName.IndexOf(item);
            }
        }

        var p = new PlayersByName();
        p.playerName = newPlayer.name;
        p.playerScript = newPlayer;

        playersByName.Add(p);
        DataKeeper.Instance.playersDataKeep.Add(new DataKeeper.PlayerDataKeep(newPlayer.name));
        return playersByName.Count - 1;
    }

    #region Scenes

    /// <summary> <para>
    /// Returns true if the current scene is <paramref name="sceneName"/>. </para>
    /// <para> Uses <seealso cref="BalDUtilities.Misc.EnumsExtension"/> from <seealso cref="BalDUtilities.Misc"/>
    /// </para> </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    public static bool CompareCurrentScene(E_ScenesNames sceneName)
    {
        return EnumsExtension.EnumToString(sceneName).Equals(SceneManager.GetActiveScene().name);
    }
    public static bool CompareCurrentScene(string sceneName)
    {
        return sceneName.Equals(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Changes the scene to <paramref name="newScene"/>.
    /// </summary>
    /// <param name="newScene"></param>
    public static void ChangeScene(E_ScenesNames newScene, bool allowReload = false, bool async = false)
    {
        string sceneName = EnumsExtension.EnumToString(newScene);

        if (!allowReload)
        {
            if (CompareCurrentScene(sceneName))
            {
                Debug.LogError(sceneName + " is already the current scene.");
                return;
            }
        }

        if (async) SceneManager.LoadSceneAsync(sceneName);
        else SceneManager.LoadScene(sceneName);
    }

    public void ReloadScene()
    {
        _onSceneReload?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    #endregion
}
