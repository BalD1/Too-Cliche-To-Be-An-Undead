using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using BalDUtilities.MouseUtils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.Users;
using System.Linq;
using UnityEngine.TextCore.Text;
using System.Text;

public class PlayerCharacter : Entity, IInteractable
{
    #region Animator args

    public const string ANIMATOR_ARGS_ATTACKING = "Attacking";
    public const string ANIMATOR_ARGS_ATTACKINDEX = "AttackIndex";
    public const string ANIMATOR_ARGS_INSKILL = "InSkill";
    public const string ANIMATOR_ARGS_DASHING = "Dashing";

    #endregion

    #region vars

    [SerializeField] public LayerMask feetsBaseLayer;
    [SerializeField] public LayerMask ignoreHoleLayer;

    [SerializeField] private int playerIndex;

    [SerializeField] private FSM_Player_Manager stateManager;

    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private GameObject minimapMarker;
    [SerializeField] private PlayerInteractor selfInteractor;
    [SerializeField] private PlayerWeapon weapon;
    [SerializeField] private SkillHolder skillHolder;
    [SerializeField] private SCRPT_Dash playerDash;
    [SerializeField] private TextMeshPro selfReviveText;
    [SerializeField] private GameObject pivotOffset;

    [SerializeField] private Animator skillTutorialAnimator;

    [SerializeField] private PlayersManager.GamepadShakeData onTakeDamagesGamepadShake;

    [SerializeField] private List<EnemyBase> attackers = new List<EnemyBase>();

    [SerializeField] private Material outlineMaterial;
    [SerializeField] private Material defaultMaterial;

    [SerializeField] private Image portrait;
    [SerializeField] private Image hpBar;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Image skillIcon;
    [SerializeField] private Image dashIcon;
    [SerializeField] private Image skillDurationIcon;

    [SerializeField] private UIManager.CharacterPortrait characterPortrait;

    private int currentPortraitIdx;

    [SerializeField] private Vector3 iconsMaxScale = new Vector3(1.3f, 1.3f, 1.3f);
    [SerializeField] private float maxScaleTime = .7f;
    [SerializeField] private LeanTweenType inType = LeanTweenType.easeInSine;
    [SerializeField] private LeanTweenType outType = LeanTweenType.easeOutSine;

    [SerializeField] private static int money;
    [SerializeField] private static int level;

    [SerializeField] private float dyingState_DURATION = 20f;
    [SerializeField][Range(0, 1)] private float reviveHealPercentage = 0.25f;

    public int selfReviveCount;

    public float MaxSkillCD_M { get; private set; }
    public float MaxDashCD_M { get; private set; }

    private float dash_CD_TIMER;
    public bool isDashing;

    private bool stayStatic;

    private Vector2 velocity;

    private Vector2 lastDirection;

    [SerializeField] private PlayerInput inputs;

    public delegate void D_AttackInput();
    public D_AttackInput D_attackInput;

    public delegate void D_StartHoldAttackInput();
    public D_StartHoldAttackInput D_startHoldAttackInput;

    public delegate void D_EndHoldAttackInput();
    public D_EndHoldAttackInput D_endHoldAttackInput;

    public delegate void D_SkillInput();
    public D_SkillInput D_skillInput;

    public delegate void D_DashInput();
    public D_DashInput D_dashInput;

    public delegate void D_AimInput(Vector2 val);
    public D_AimInput D_aimInput;

    private InputAction movementsAction;

    public GameObject PivotOffset { get => pivotOffset; }
    public PlayerAnimationController AnimationController { get => animationController; }
    public FSM_Player_Manager StateManager { get => stateManager; }
    public PlayerInteractor GetInteractor { get => selfInteractor; }
    public PlayerWeapon Weapon { get => weapon; }
    public SkillHolder GetSkillHolder { get => skillHolder; }
    public SCRPT_Skill GetSkill { get => skillHolder.Skill; }
    public Animator SkillTutoAnimator { get => skillTutorialAnimator; }
    public Image SkillDurationIcon { get => skillDurationIcon; }
    public Image GetSkillIcon { get => skillIcon; }
    public TextMeshPro SelfReviveText { get => selfReviveText; }
    public Vector2 Velocity { get => velocity; }
    public PlayerInput Inputs { get => inputs; }
    public int Money { get => money; }
    public int PlayerIndex { get => playerIndex; }
    public int Level { get => level; }
    public float DyingState_DURATION { get => dyingState_DURATION; }
    public SCRPT_Dash PlayerDash { get => playerDash; }
    public List<EnemyBase> Attackers { get => attackers; }
    public Vector2 LastDirection { get => lastDirection; set => lastDirection = value; }

    public const string SCHEME_KEYBOARD = "Keyboard&Mouse";
    public const string SCHEME_GAMEPAD = "Gamepad";

    private float gamepadShake_TIMER;

#if UNITY_EDITOR
    public bool debugPush;
    private Vector2 gizmosMouseDir;
    private Ray gizmosPushRay;
    private float gizmosPushEnd;
    private float gizmosPushDrag;
#endif

    #endregion

    #region A/S/U/F

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (DataKeeper.Instance.playersDataKeep.Count <= 0) return;
        selfInteractor.RemoveEverythingFromList();

        stateManager.SwitchState(stateManager.idleState);
        CancelInvoke(nameof(ClearAttackers));

        if (scene.name.Equals("MainMenu"))
        {
            if (this.playerIndex > 0)
            {
                DataKeeper.Instance.RemoveData(this.playerIndex);
                Destroy(this.gameObject);
                return;
            }
            SwitchControlMapToUI();
        }
        else
        {
            this.transform.position = GameManager.Instance.GetSpawnPoint(this.playerIndex).position;
            if (this.playerIndex == 0) CameraManager.Instance.TeleportCamera(this.transform.position);

            SwitchControlMapToDialogue();

            currentPortraitIdx = 0;

            SetHUD();

            SetCharacter();

            ResetStats();

            UpdateHPonUI();
        }

        this.minimapMarker.SetActive(true);

        if (this.playerIndex == 0) GameManager.Instance.SetPlayer1(this);

        this.attackers.Clear();

        this.stateManager.SwitchState(stateManager.idleState);
    }

    private void SetHUD()
    {
        UIManager.PlayerHUD pHUD = UIManager.Instance.PlayerHUDs[this.playerIndex];
        if (pHUD.container != null)
        {
            pHUD.container.SetActive(true);

            this.portrait = pHUD.portrait;
            this.hpBar = pHUD.hpBar;
            this.hpText = pHUD.hpText;
            this.skillIcon = pHUD.skillThumbnail;
            this.dashIcon = pHUD.dashThumbnail;
        }

        UIManager.Instance.KeycardContainer.SetActive(false);
    }

    private void SetCharacter()
    {
        GameManager.E_CharactersNames character = DataKeeper.Instance.playersDataKeep[this.PlayerIndex].character;

        foreach (var item in UIManager.Instance.CharacterPortraits)
        {
            if (item.characterName.Equals(character))
            {
                characterPortrait = item;
            }
        }

        PlayersManager.PlayerCharacterComponents pcc = PlayersManager.Instance.GetCharacterComponents(character);

        SwitchCharacter(pcc.dash, pcc.skill, pcc.stats, pcc.sprite, pcc.character);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    protected override void Awake()
    {
        base.Awake();

        this.MaxSkillCD_M = skillHolder.Skill.Cooldown;
        this.MaxDashCD_M = playerDash.Dash_COOLDOWN;

        if (SceneManager.GetActiveScene().name.Equals("MainMenu"))
            SwitchControlMapToUI();
        else
            SwitchControlMapToInGame();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected override void Start()
    {
        DontDestroyOnLoad(this);
        base.Start();

        movementsAction = inputs.actions.FindAction("Movements");

        GameManager.Instance._onSceneReload += OnSceneReload;

        SetKeepedData();

        inputs.neverAutoSwitchControlSchemes = this.playerIndex != 0;

        UIManager.Instance.D_exitPause += SwitchControlMapToInGame;

        if (this.hpBar == null && !GameManager.CompareCurrentScene(GameManager.E_ScenesNames.MainMenu))
        {
            UIManager.PlayerHUD pHUD = UIManager.Instance.PlayerHUDs[0];
            characterPortrait = UIManager.Instance.CharacterPortraits[this.playerIndex];
            currentPortraitIdx = 0;

            GameManager.E_CharactersNames character = GameManager.E_CharactersNames.Shirley;
            if (DataKeeper.Instance.IsPlayerDataKeepSet())
            {
                foreach (var item in UIManager.Instance.CharacterPortraits)
                {
                    if (item.characterName.Equals(character)) characterPortrait = item;
                }
            }
            else
            {
                GameManager.Instance.SetPlayer1(this);
                GameManager.Instance.playersByName = new List<GameManager.PlayersByName>();
                GameManager.Instance.playersByName.Add(new GameManager.PlayersByName("soloP1", this));
            }

            if (pHUD.container != null)
            {
                pHUD.container.SetActive(true);
                this.portrait = pHUD.portrait;
                this.hpBar = pHUD.hpBar;
                this.hpText = pHUD.hpText;
                this.skillIcon = pHUD.skillThumbnail;

                pHUD.portrait.sprite = UIManager.Instance.GetBasePortrait(character);
            }

            PlayersManager.PlayerCharacterComponents pcc = PlayersManager.Instance.GetCharacterComponents(GameManager.E_CharactersNames.Shirley);

            SwitchCharacter(pcc.dash, pcc.skill, pcc.stats, pcc.sprite, pcc.character);

            this.currentHP = maxHP_M;

            SwitchControlMapToDialogue();
        }

        if (GameManager.Instance.playersByName.Count <= 0) GameManager.Instance.SetPlayersByNameList();

        this.minimapMarker.SetActive(true);

        SetDashThumbnail(playerDash.Thumbnail);
    }

    protected override void Update()
    {
        if (GameManager.Instance.GameState != GameManager.E_GameState.InGame) return;
        base.Update();

        if (dash_CD_TIMER > 0)
        {
            dash_CD_TIMER -= Time.deltaTime;
            if (dashIcon != null)
            {
                dashIcon.fillAmount = -((dash_CD_TIMER / MaxDashCD_M) - 1);

                if (dash_CD_TIMER <= 0) ScaleTweenObject(dashIcon.gameObject);
            }
        }

        if (gamepadShake_TIMER > 0)
        {
            gamepadShake_TIMER -= Time.deltaTime;
            if (gamepadShake_TIMER <= 0) StopGamepadShake();
        }

    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        Vector3 minScreenBounds = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 maxScreenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        //transform.position = new Vector3(Mathf.Clamp(transform.position.x, minScreenBounds.x + 1, maxScreenBounds.x - 1), Mathf.Clamp(transform.position.y, minScreenBounds.y + 1, maxScreenBounds.y - 1), transform.position.z);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    #endregion

    #region Controls / Movements

    public void SetSelfVelocity(Vector2 _velocity) => velocity = _velocity;
    public void SetAllVelocity(Vector2 _velocity)
    {
        this.velocity = _velocity;
        if (this.rb != null)
            this.rb.velocity = _velocity;
    }

    public void SwitchControlMap(string map) => inputs.SwitchCurrentActionMap(map);
    public void SwitchControlMapToInGame() => inputs.SwitchCurrentActionMap("InGame");
    public void SwitchControlMapToUI() => inputs.SwitchCurrentActionMap("UI");
    public void SwitchControlMapToDialogue() => inputs.SwitchCurrentActionMap("Dialogue");

    public void ReadMovementsInputs(InputAction.CallbackContext context)
    {
        if (inputs.currentActionMap?.name.Equals("InGame") == false) return;
        velocity = context.ReadValue<Vector2>();
        if (velocity != Vector2.zero) lastDirection = velocity;
    }

    public void ForceUpdateMovementsInput()
    {
        movementsAction.Disable();
        movementsAction.Enable();
    }

    public void StartAction(InputAction.CallbackContext context)
    {
        Debug.Log(context);
        if (!context.performed) return;

        InputDevice d = context.control.device;

        Debug.Log(d);
        if (!InputUser.all[0].pairedDevices.Contains(d)) return;

        GameManager.ChangeScene(GameManager.E_ScenesNames.MainScene);
    }

    public void Movements()
    {
        if (stayStatic) return;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed_M);

        animationController.FlipSkeleton(velocity.x > 0);

        this.rb.MovePosition(this.rb.position + velocity * maxSpeed_M * Time.fixedDeltaTime);
    }

    public void StartGamepadShake(PlayersManager.GamepadShakeData shakeData) 
        =>  StartGamepadShake(shakeData.lowFrequency, shakeData.highFrequency, shakeData.duration);
    public void StartGamepadShake(float lowFrequency, float highFrequency, float time)
    {
        if (DataKeeper.Instance.allowGamepadShake == false) return;

        foreach (var item in inputs.devices)
        {
            (item as Gamepad)?.SetMotorSpeeds(lowFrequency, highFrequency);
        }

        gamepadShake_TIMER = time;
    }

    public void StopGamepadShake()
    {
        foreach (var item in inputs.devices)
        {
            (item as Gamepad)?.SetMotorSpeeds(0, 0);
        }
    }

    #endregion

    #region Attack / Damages / Heal

    public void StartAttack()
    {
        if (!weapon.prepareNextAttack) weapon.prepareNextAttack = true;
        else weapon.inputStored = true;
    }

    public void CancelAttackAnimation()
    {
        this.animator.Play("AN_Wh_Idle");
        weapon.EffectAnimator.Play("Main State");
        weapon.ResetAttack();
    }

    public override bool OnTakeDamages(float amount, bool isCrit = false, bool fakeDamages = false)
    {
        if (!IsAlive()) return false;

        bool res;

        if (GameManager.Instance.IsInTutorial) fakeDamages = true;

        res = base.OnTakeDamages(amount, isCrit, fakeDamages);
        if (res == false) return res;

        if (portrait != null)
        {
            LeanTween.color(portrait.rectTransform, Color.red, .2f).setEase(inType)
            .setOnComplete(() =>
            {
                LeanTween.color(portrait.rectTransform, Color.white, .2f);
            });

            if (SetPortrait())
            {
                GameObject target = UIManager.Instance.PlayerHUDs[this.playerIndex].container;

                LeanTween.scale(target, iconsMaxScale, maxScaleTime / 2).setEase(inType)
                .setOnComplete(() =>
                {
                    LeanTween.scale(target, Vector3.one, maxScaleTime / 2).setEase(outType);
                });
            }
        }

        StartGamepadShake(onTakeDamagesGamepadShake);

        UpdateHPonUI();

        return res;
    }

    private bool SetPortrait(float currentMaxHP = -1)
    {
        if (currentMaxHP == -1)
            currentMaxHP = maxHP_M;


        if (currentPortraitIdx + 1 <= characterPortrait.characterPortraitsByHP.Length)
        {
            if (currentHP * (currentMaxHP / 100) <= currentMaxHP * CurrentCharacterPortrait().percentage)
            {
                SetCharacterPortrait(currentPortraitIdx + 1);
                SetPortrait(currentMaxHP);
                return true;
            }
        }   

        if (currentPortraitIdx > 0)
        {
            if (currentHP * (currentMaxHP / 100) > currentMaxHP * LastCharacterPortrait().percentage)
            {
                SetCharacterPortrait(currentPortraitIdx - 1);
                SetPortrait(currentMaxHP);
                return true;
            }
        }

        return false;
    }

    private UIManager.CharacterPortraitByHP CurrentCharacterPortrait() => characterPortrait.characterPortraitsByHP[currentPortraitIdx];
    private UIManager.CharacterPortraitByHP NextCharacterPortrait() => characterPortrait.characterPortraitsByHP[currentPortraitIdx + 1];
    private UIManager.CharacterPortraitByHP LastCharacterPortrait() => characterPortrait.characterPortraitsByHP[currentPortraitIdx - 1];
    private void SetCharacterPortrait(int idx)
    {
        if (idx < characterPortrait.characterPortraitsByHP.Length)
            this.portrait.sprite = characterPortrait.characterPortraitsByHP[idx].portrait;

        currentPortraitIdx = idx;
    }

    protected override void ApplyModifier(StatsModifier m)
    {
        switch (m.StatType)
        {
            case StatsModifier.E_StatType.MaxHP:
                maxHP_M += m.Modifier;
                break;

            case StatsModifier.E_StatType.Damages:
                maxDamages_M += m.Modifier;
                break;

            case StatsModifier.E_StatType.AttackRange:
                maxAttRange_M += m.Modifier;
                break;

            case StatsModifier.E_StatType.Attack_CD:
                maxAttCD_M += m.Modifier;
                break;

            case StatsModifier.E_StatType.Speed:
                maxSpeed_M += m.Modifier;
                break;

            case StatsModifier.E_StatType.CritChances:
                maxCritChances_M += (int)m.Modifier;
                break;

            case StatsModifier.E_StatType.DASH_CD:
                MaxDashCD_M += m.Modifier;
                break;

            case StatsModifier.E_StatType.SKILL_CD:
                MaxSkillCD_M += m.Modifier;
                break;
        }
    }

    public override void OnHeal(float amount, bool isCrit = false, bool canExceedMaxHP = false)
    {
        base.OnHeal(amount, isCrit, canExceedMaxHP);

        UpdateHPonUI();

        SetPortrait();
    }

    private void UpdateHPonUI()
    {
        if (hpBar != null)
            hpBar.fillAmount = (currentHP / maxHP_M);
        if (hpText != null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(currentHP);
            sb.Append(" / ");
            sb.Append(maxHP_M);
            hpText.text = sb.ToString();
        }
    }

    public override void OnDeath(bool forceDeath = false)
    {
        if (this.stateManager.ToString().Equals("Dying")) return;
        
        base.OnDeath(forceDeath);
        this.selfInteractor.ResetCollider();
        this.stateManager.SwitchState(stateManager.dyingState);
    }

    public void Revive()
    {
        this.OnHeal(this.maxHP_M * reviveHealPercentage);
        stateManager.SwitchState(stateManager.idleState);
    }

    public void DefinitiveDeath()
    {
        if (selfReviveCount > 0)
        {
            selfReviveCount -= 1;
            Revive();
            return;
        }

        stateManager.SwitchState(stateManager.deadState);
        NormalZombie zombifiedSelf = Instantiate(GameAssets.Instance.NormalZombiePF, this.transform.position, Quaternion.identity).GetComponent<NormalZombie>();
        zombifiedSelf.SetAsZombifiedPlayer(this.sprite.sprite, this.maxHP_M, this.maxDamages_M, this.maxSpeed_M, this.maxCritChances_M);

        this.minimapMarker.SetActive(false);

        PlayersManager.Instance.DefinitiveDeath(this);
    }

    public bool AddAttacker(EnemyBase attacker)
    {
        if (attackers.Count >= GameManager.MaxAttackers) return false;
        if (attackers.Contains(attacker)) return false;

        attackers.Add(attacker);
        return true;
    }

    public void RemoveAttacker(EnemyBase attacker)
    {
        attackers.Remove(attacker);
    }

    public void ClearAttackers()
    {
        attackers.Clear();
    }

    #endregion

    #region Money

    public static void AddMoney(int amount) => money += amount;
    public static void RemoveMoney(int amount, bool canGoInDebt)
    {
        if (!canGoInDebt && money < 0) return;

        money -= amount;

        if (!canGoInDebt && money < 0) money = 0;
    }
    public static void SetMoney(int newMoney) => money = newMoney;
    public static int GetMoney() => money;
    public static bool HasEnoughMoney(int price) => money >= price ? true : false;

    #endregion

    #region Inputs

        #region InGame

    public void AttackInput(InputAction.CallbackContext context)
    {
        if (context.performed) D_attackInput?.Invoke();
    }

    public void MaintainedAttackIpunt(InputAction.CallbackContext context)
    {
        if (context.started) D_startHoldAttackInput?.Invoke();
        else if (context.canceled) D_endHoldAttackInput?.Invoke();
    }

    public void SkillInput(InputAction.CallbackContext context)
    {
        if (context.performed) D_skillInput?.Invoke();
    }

    public void DashInput(InputAction.CallbackContext context)
    {
        if (context.performed) D_dashInput?.Invoke();
    }

    public void SelectInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //PlayersManager.Instance.JoinAction(context);
        }
    }

    public void StayStaticInput(InputAction.CallbackContext context)
    {
        if (context.started) stayStatic = true;
        else if (context.canceled) stayStatic = false;
    }

    public void AimInput(InputAction.CallbackContext context)
    {
        if (context.performed) D_aimInput?.Invoke(context.ReadValue<Vector2>());
    }

    public void SelfRevive(InputAction.CallbackContext context)
    {
        if (context.performed && stateManager.ToString().Equals("Dying") && selfReviveCount > 0)
        {
            selfReviveCount -= 1;
            Revive();
        }
    }

    #endregion

        #region Menus

    public void LeftArrowInput(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.GameState != GameManager.E_GameState.MainMenu) return;
        if (context.performed) UIManager.Instance.PlayerLeftArrowOnPanel(playerIndex);
    }

    public void RightArrowInput(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.GameState != GameManager.E_GameState.MainMenu) return;
        if (context.performed) UIManager.Instance.PlayerRightArrowOnPanel(playerIndex);
    }

    public void QuitLobby(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.GameState != GameManager.E_GameState.MainMenu) return;
        if (context.performed) GameManager.Instance.QuitLobby(PlayerIndex);
    }

    public void PauseInputRelay(InputAction.CallbackContext context)
    {
        if (context.started) GameManager.Instance.HandlePause();
    }

    public void CancelMenu(InputAction.CallbackContext context)
    {
        if (context.performed == false) return;

        if (UIManager.Instance.OpenMenusQueues.Count > 0)
        {
            UIManager.Instance.CloseYoungerMenu();
            return;
        }
    }

    public void ScrollCurrentVerticalBarDown(InputAction.CallbackContext context)
    {
        UIManager.Instance.ScrollCurrentVerticalBarDown(context);
    }
    public void ScrollCurrentVerticalBarUp(InputAction.CallbackContext context)
    {
        UIManager.Instance.ScrollCurrentVerticalBarUp(context);
    }

    public void ScrollCurrentHorizontalBarLeft(InputAction.CallbackContext context)
    {
        UIManager.Instance.ScrollCurrentHorizontalBarLeft(context);
    }
    public void ScrollCurrentHorizontalBarRight(InputAction.CallbackContext context)
    {
        UIManager.Instance.ScrollCurrentHorizontalBarRight(context);
    }

    #endregion

        #region Dialogues

    public void ContinueDialogue(InputAction.CallbackContext context)
    {
        if (context.performed) DialogueManager.Instance.TryNextLine();
    }

        #endregion

    #endregion

    #region Skill

    public void SetSkillThumbnail(Sprite image)
    {
        if (skillIcon == null) return;

        skillIcon.sprite = image;
    }
    public void UpdateSkillThumbnailFill(float fill)
    {
        if (skillIcon == null) return;

        skillIcon.fillAmount = fill;
    }

    public void ResetSkillAnimator()
    {
        this.GetSkillHolder.GetAnimator.ResetTrigger("EndSkill");
        this.GetSkillHolder.GetAnimator.Play("MainState");
    }

    public void OffsetSkillHolder(float offset)
    {
        skillHolder.transform.localPosition = (Vector3)weapon.GetGeneralDirectionOfMouseOrGamepad() * offset;
    }

    public void RotateSkillHolder()
    {
        switch (weapon.GetGeneralDirectionOfMouseOrGamepad())
        {
            case Vector2 v when v.Equals(Vector2.left):
                skillHolder.transform.eulerAngles = new Vector3(0, 0, -90);
                break;

            case Vector2 v when v.Equals(Vector2.right):
                skillHolder.transform.eulerAngles = new Vector3(0, 0, 90);
                break;

            case Vector2 v when v.Equals(Vector2.up):
                skillHolder.transform.eulerAngles = new Vector3(0, 0, -180);
                break;

            case Vector2 v when v.Equals(Vector2.down):
                skillHolder.transform.eulerAngles = new Vector3(0, 0, 0);
                break;
        }
    }

    #endregion

    #region Dash / Push

    public void SetDashThumbnail(Sprite image)
    {
        if (dashIcon == null) return;

        dashIcon.sprite = image;
    }

    public void StartDash()
    {
        if (dash_CD_TIMER > 0) return;

        isDashing = true;
    }

    public void StartDashTimer() => dash_CD_TIMER = MaxDashCD_M;

    public override Vector2 Push(Vector2 pusherPosition, float pusherForce, Entity originalPusher)
    {
        if (!canBePushed) return Vector2.zero;

        Vector2 v = base.Push(pusherPosition, pusherForce, originalPusher);

        if (v.magnitude <= Vector2.zero.magnitude) return Vector2.zero;

        stateManager.SwitchState(stateManager.pushedState.SetForce(v, originalPusher));

        return v;
    }

    private float CalculateAllKeys()
    {
        float res = 0;
        for (int i = 0; i < playerDash.DashSpeedCurve.length; i++)
        {
            if (i + 1 < playerDash.DashSpeedCurve.length)
                res += CalculateSingleKey(i, i + 1);
        }

        return res;
    }
    private float CalculateSingleKey(int index1, int index2)
    {
        Keyframe startKey = playerDash.DashSpeedCurve[index1];
        Keyframe endKey = playerDash.DashSpeedCurve[index2];

        float res = ((startKey.value + endKey.value) / 2) * (endKey.time - startKey.time);

        return res;
    }

    #endregion

    #region Keep Data / Scene Reload

    private void SetKeepedData()
    {
        this.playerIndex = DataKeeper.Instance.CreateData(this);
        PlayerCharacter.money = DataKeeper.Instance.money;

        PlayersManager.Instance.SetupPanels(playerIndex);
    }

    public void ForceSetIndex(int idx)
    {
        this.playerIndex = idx;
        this.gameObject.name = "Player " + idx;
    }

    private void OnSceneReload()
    {
        this.StatsModifiers.Clear();
        this.attackers.Clear();

        this.currentHP = this.maxHP_M;
        if (this.hpBar != null)
            this.hpBar.fillAmount = 1;

        this.stateManager.ResetAll();

        DataKeeper.Instance.money = PlayerCharacter.money;
        DataKeeper.Instance.maxLevel = this.Level;
    }

    #endregion

    #region Tweening

    public void ScaleTweenObject(GameObject target)
    {
        if (target == null) return;

        LeanTween.scale(target, iconsMaxScale, maxScaleTime).setEase(inType)
        .setOnComplete(() =>
        {
            LeanTween.scale(target, Vector3.one, maxScaleTime).setEase(outType);
        });
    }
    public void ScaleTweenObject(GameObject target, LeanTweenType _inType, LeanTweenType _outType)
    {
        if (target == null) return;

        LeanTween.scale(target, iconsMaxScale, maxScaleTime).setEase(_inType)
        .setOnComplete(() =>
        {
            LeanTween.scale(target, Vector3.one, maxScaleTime).setEase(_outType);
        });
    }
    public void ScaleTweenObject(RectTransform target)
    {
        if (target == null) return;

        LeanTween.scale(target, iconsMaxScale, maxScaleTime).setEase(inType)
        .setOnComplete(() =>
        {
            LeanTween.scale(target, Vector3.one, maxScaleTime).setEase(outType);
        });
    }

    #endregion

    #region Interactions

    public void EnteredInRange(GameObject interactor)
    {
        sprite.material = outlineMaterial;
    }

    public void ExitedRange(GameObject interactor)
    {
        sprite.material = defaultMaterial;
    }

    public void Interact(GameObject interactor)
    {
        Revive();

        sprite.material = defaultMaterial;
    }

    public bool CanBeInteractedWith() => this.stateManager.ToString().Equals("Dying");

    public float GetDistanceFrom(Transform target) => Vector2.Distance(this.transform.position, target.position);
 
    #endregion

    #region Level

    public static void LevelUp() => level++;
    public static void SetLevel(int newLevel) => level = newLevel;
    public static int GetLevel => PlayerCharacter.level;

    #endregion

    #region Switch & Set

    public void SwitchCharacter(SCRPT_Dash newDash, SCRPT_Skill newSkill, SCRPT_EntityStats newStats, Sprite newSprite, GameManager.E_CharactersNames character)
    {
        this.playerDash = newDash;
        if (dashIcon != null)
            this.dashIcon.sprite = newDash.Thumbnail;

        this.skillHolder.ChangeSkill(newSkill);
        this.stats = newStats;
        this.sprite.sprite = newSprite;

        if (this.portrait != null)
            this.portrait.sprite = UIManager.Instance.GetBasePortrait(character);

        this.minimapMarker.GetComponent<SpriteRenderer>().sprite = this.portrait.sprite;

        foreach (var item in UIManager.Instance.CharacterPortraits)
        {
            if (item.characterName.Equals(character)) characterPortrait = item;
        }

        currentPortraitIdx = 0;
        SetPortrait();

        this.MaxDashCD_M = newDash.Dash_COOLDOWN;
        this.MaxSkillCD_M = newSkill.Cooldown;

        this.maxHP_M = newStats.MaxHP;
        this.maxDamages_M = newStats.BaseDamages;
        this.maxAttRange_M = newStats.AttackRange;
        this.maxAttCD_M = newStats.Attack_COOLDOWN;
        this.maxSpeed_M = newStats.Speed;
        this.maxCritChances_M = newStats.CritChances;

        this.currentHP = this.maxHP_M;

        foreach (var item in statsModifiers)
        {
            this.ApplyModifier(item);
        }

        UpdateHPonUI();
    }

    public void SetAttack(GameObject newWeapon)
    {
        weapon.ResetAttack();
        Destroy(weapon.gameObject);
        GameObject gO = Instantiate(newWeapon, this.transform);
        weapon = gO.GetComponent<PlayerWeapon>();
        stateManager.OwnerWeapon = weapon;
    }

    #endregion

    public override void Stun(float duration, bool resetAttackTimer = false)
    {
        stateManager.SwitchState(stateManager.stunnedState.SetDuration(duration, resetAttackTimer));
    }

    protected override void ResetStats()
    {
        base.ResetStats();

        this.MaxSkillCD_M = skillHolder.Skill.Cooldown;
        this.MaxDashCD_M = playerDash.Dash_COOLDOWN;
    }

    public GameManager.E_CharactersNames GetCharacterName()
    {
        return DataKeeper.Instance.playersDataKeep[this.playerIndex].character;
    }

    public GameManager.E_CharactersNames GetCharacterName(int idx)
    {
        if (idx > DataKeeper.Instance.playersDataKeep.Count) return GameManager.E_CharactersNames.Shirley;
        
        return DataKeeper.Instance.playersDataKeep[idx].character;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        d_EnteredCollider?.Invoke(collision);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") || collision.CompareTag("Player"))
            d_EnteredTrigger?.Invoke(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") || collision.CompareTag("Player"))
            d_ExitedTrigger?.Invoke(collision);
    }

    #region Gizmos

    protected override void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!debugMode) return;
        base.OnDrawGizmos();
        GizmosDrawDashPush();
#endif
    }

    private void GizmosDrawDashPush()
    {
#if UNITY_EDITOR

        if (stateManager.ToString().Equals("Dashing") == false)
        {
            gizmosMouseDir = Vector2.zero;
            Vector2 mousPos = MousePosition.GetMouseWorldPosition();
            gizmosMouseDir = (mousPos - (Vector2)this.transform.position).normalized;

            gizmosPushEnd = 1;

            gizmosPushEnd = CalculateAllKeys();
            gizmosPushRay = new Ray(this.transform.position,gizmosMouseDir * gizmosPushEnd);
        }

        Gizmos.DrawRay(this.transform.position, gizmosMouseDir * gizmosPushEnd);

        RaycastHit2D[] rh = Physics2D.RaycastAll(this.transform.position, gizmosMouseDir * gizmosPushEnd);
        foreach (var item in rh)
        {
            if (item.collider.CompareTag("Enemy") == false) continue;

            Vector2 origin = ((Vector2)item.collider.transform.position - item.point).normalized;

            // we take the time needed to travel to the Point with the Dash Velocity,
            // then multiply it by the max time of the Dash Speed Curve.
            // this roughly simulates how much Dash Time would remain if we actually dashed. (current time / max time)
            float currentDistanceByMax = (gizmosPushEnd - Vector2.Distance(this.transform.position, item.point)) * 2;
            float maxTime = playerDash.DashSpeedCurve[playerDash.DashSpeedCurve.length - 1].time;
            float dashVel = playerDash.DashSpeedCurve.Evaluate(0);

            float remainingPushForce = playerDash.PushForce * (currentDistanceByMax / dashVel * maxTime);

            float finalForce = remainingPushForce - item.collider.GetComponentInParent<Entity>().GetStats.Weight;
            Gizmos.DrawRay(item.point, (origin * finalForce));
        }
#endif
    }

    #endregion
}
