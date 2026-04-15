using UnityEngine;

/// <summary>
/// 玩家坦克核心控制器（总控）
/// 职责：1. 初始化所有子模块 
///     2. 协调输入与功能模块 
///     3. 处理玩家专属逻辑（血条、受伤、道具）
///     4. 触发扩展逻辑
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("核心模块引用")]
    public BaseMovement movementModule; // 移动模块
    public BaseChassisRotator chassisRotatorModule; // 底盘旋转模块
    public BaseTurretController turretModule; // 炮台模块

    //[Header("玩家属性（扩展：生命值）")]
    //public int maxHp = 100; // 最大生命值
    //private int _currentHp; // 当前生命值

    //[Header("扩展模块（预留）")]
    //public AudioSource audioSource; // 音效源
    //public Animator tankAnimator; // 动画控制器
    //public ItemManager itemManager; // 道具管理器

    private bool _isInitialized;

    #region 初始化

    private void Awake() => Init();

    // 初始化
    private void Init()
    {
        InitModules();
        //InitPlayerProps();
        _isInitialized = true;
    }

    // 初始化所有功能模块
    private void InitModules()
    {
        // 自动获取未手动绑定的模块
        if (movementModule == null)
            movementModule = GetComponent<BaseMovement>();
        if (chassisRotatorModule == null)
            chassisRotatorModule = GetComponent<BaseChassisRotator>();
        if (turretModule == null)
            turretModule = GetComponentInChildren<BaseTurretController>();

        // 校验模块
        if (movementModule == null)
            Debug.LogError($"[{nameof(PlayerController)}] 移动模块未找到！");
        if (chassisRotatorModule == null)
            Debug.LogError($"[{nameof(PlayerController)}] 底盘旋转模块未找到！");
        if (turretModule == null)
            Debug.LogError($"[{nameof(PlayerController)}] 炮台模块未找到！");
    }

    // 初始化玩家属性
    //private void InitPlayerProps()
    //{
    //    _currentHp = maxHp;
    //    // 初始化音效/动画/道具模块（预留）
    //    if (audioSource == null)
    //        audioSource = GetComponent<AudioSource>();
    //    if (tankAnimator == null)
    //        tankAnimator = GetComponent<Animator>();
    //}
    #endregion

    #region 输入回调（由PlayerInputHandler调用）
    /// <summary>
    /// 接收移动输入
    /// </summary>
    /// <param name="moveDir">归一化的世界移动方向</param>
    public void OnReceiveMoveInput(Vector3 moveDir)
    {
        if (!_isInitialized) return;
        movementModule.SetMoveDirection(moveDir);
    }

    /// <summary>
    /// 接收炮台目标输入
    /// </summary>
    /// <param name="targetPos">世界目标点</param>
    public void OnReceiveTurretTargetInput(Vector3 targetPos)
    {
        if (!_isInitialized) return;
        turretModule.SetTargetPos(targetPos);
    }

    /// <summary>
    /// 接收武器切换输入
    /// </summary>
    /// <param name="shellTypeIndex">炮弹类型索引</param>
    //public void OnSwitchShellType(int shellTypeIndex)
    //{
    //    if (!_isInitialized || itemManager == null) return;
    //    // 扩展：从道具管理器获取对应炮弹预制体
    //    GameObject newShell = itemManager.GetShellByIndex(shellTypeIndex);
    //    turretModule.SwitchShellType(newShell);

    //    // 扩展：播放武器切换音效
    //    //PlayAudio("SwitchWeapon");
    //}
    #endregion

    #region 扩展接口实现（生命值/受伤/道具）
    /// <summary>
    /// 实现IDamageable接口：受伤扣血
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="attacker">攻击者</param>
    //public void TakeDamage(int damage, GameObject attacker = null)
    //{
    //    _currentHp = Mathf.Max(0, _currentHp - damage);

    //    // 扩展：触发受伤动画/音效
    //    tankAnimator?.SetTrigger("TakeDamage");
    //    PlayAudio("Hurt");

    //    // 扩展：血条UI更新
    //    UIManager.Instance?.UpdatePlayerHp(_currentHp, maxHp);

    //    // 扩展：血量为0时死亡
    //    if (_currentHp <= 0)
    //        OnPlayerDead();
    //}

    /// <summary>
    /// 实现IItemCollectible接口：拾取道具
    /// </summary>
    /// <param name="item">拾取的道具</param>
    //public void CollectItem(ItemBase item)
    //{
    //    if (item == null) return;

    //    // 根据道具类型处理（加血、加弹药、切换武器等）
    //    switch (item.ItemType)
    //    {
    //        case ItemType.Health:
    //            _currentHp = Mathf.Min(maxHp, _currentHp + item.Value);
    //            UIManager.Instance?.UpdatePlayerHp(_currentHp, maxHp);
    //            PlayAudio("CollectHealth");
    //            break;
    //        case ItemType.Shell:
    //            itemManager.AddShell(item.ShellPrefab, item.Count);
    //            PlayAudio("CollectShell");
    //            break;
    //        case ItemType.Buff:
    //            // 扩展：添加增益buff（如移动加速、开火冷却减少）
    //            ApplyBuff(item.BuffType, item.Duration);
    //            PlayAudio("CollectBuff");
    //            break;
    //    }
    //}

    // 玩家死亡逻辑（扩展）
    //private void OnPlayerDead()
    //{
    //    // 停止所有模块
    //    movementModule.ResetMovement();
    //    turretModule.PauseFire(float.MaxValue); // 永久暂停开火

    //    // 触发死亡动画/音效
    //    tankAnimator?.SetTrigger("Dead");
    //    PlayAudio("Dead");

    //    // 扩展：游戏结束逻辑
    //    GameManager.Instance?.OnPlayerDead();
    //}
    #endregion

    #region 扩展方法（音效/动画/Buff）
    /// <summary>
    /// 播放音效（统一管理）
    /// </summary>
    /// <param name="audioName">音效名称（需提前配置AudioClip）</param>
    //private void PlayAudio(string audioName)
    //{
    //    if (audioSource == null) return;
    //    // 扩展：从音效管理器获取对应音频剪辑
    //    AudioClip clip = AudioManager.Instance?.GetAudioClip(audioName);
    //    if (clip != null)
    //        audioSource.PlayOneShot(clip);
    //}

    /// <summary>
    /// 应用增益Buff（扩展）
    /// </summary>
    /// <param name="buffType">Buff类型</param>
    /// <param name="duration">持续时间</param>
    //private void ApplyBuff(BuffType buffType, float duration)
    //{
    //    switch (buffType)
    //    {
    //        case BuffType.MoveSpeedUp:
    //            movementModule.moveSpeed *= 1.5f;
    //            StartCoroutine(ResetBuffAfterTime(buffType, duration));
    //            break;
    //        case BuffType.FireSpeedUp:
    //            turretModule.fireCooldown *= 0.7f;
    //            StartCoroutine(ResetBuffAfterTime(buffType, duration));
    //            break;
    //    }
    //}

    // Buff到期重置（扩展）
    //private IEnumerator ResetBuffAfterTime(BuffType buffType, float duration)
    //{
    //    yield return new WaitForSeconds(duration);
    //    switch (buffType)
    //    {
    //        case BuffType.MoveSpeedUp:
    //            movementModule.moveSpeed /= 1.5f;
    //            break;
    //        case BuffType.FireSpeedUp:
    //            turretModule.fireCooldown /= 0.7f;
    //            break;
    //    }
    //}
    #endregion
}
