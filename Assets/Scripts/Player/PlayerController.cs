using UnityEngine;

/// <summary>
/// 玩家坦克核心控制器（总控）
/// 职责：1. 初始化所有子模块 
///     2. 协调输入与功能模块 
///     3. 处理玩家专属逻辑（血条、受伤、道具）
///     4. 触发扩展逻辑
/// </summary>
public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("核心模块引用")]
    public BaseMovement movementModule; // 移动模块
    public BaseChassisRotator chassisRotatorModule; // 底盘旋转模块
    public BaseTurretController turretModule; // 炮台模块

    [Header("生命值系统配置")]
    [SerializeField] private float _maxHealth = 100f; // 最大生命值
    private float _currentHealth; // 当前生命值
    private bool _isDead = false; // 死亡状态标记

    private bool _isInitialized;

    #region 初始化

    private void Awake() => Init();

    // 初始化
    private void Init()
    {
        InitModules();
        InitHealthSystem();
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

    // 初始化生命值系统
    private void InitHealthSystem()
    {
        _currentHealth = _maxHealth; // 初始生命值=最大生命值
        _isDead = false; // 初始为存活状态
    }
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

    #region 生命值系统
    /// <summary>
    /// 实现IDamageable接口：受伤扣血
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(float damage)
    {
        // 防护逻辑：已死亡/未初始化 不处理掉血
        if (_isDead || !_isInitialized) return;

        // 确保伤害值非负
        damage = Mathf.Max(0, damage);
        // 扣血后确保生命值≥0
        _currentHealth = Mathf.Max(0, _currentHealth - damage);

        // 广播生命值变化事件（供UI/其他系统监听）
        EventManager.Instance.TriggerParamEvent("OnPlayerHealthChanged", _currentHealth);

        // 检查是否死亡
        if (_currentHealth <= 0)
        {
            OnPlayerDead();
        }
    }

    /// <summary>
    /// 当前生命值（只读，符合IDamageable接口）
    /// </summary>
    public float CurrentHealth => _currentHealth;

    /// <summary>
    /// 最大生命值（只读，符合IDamageable接口）
    /// </summary>
    public float MaxHealth => _maxHealth;

    /// <summary>
    /// 是否死亡（只读，符合IDamageable接口）
    /// </summary>
    public bool IsDead => _isDead;
    #endregion

    #region 死亡逻辑
    /// <summary>
    /// 玩家死亡处理
    /// </summary>
    private void OnPlayerDead()
    {
        _isDead = true;

        // 广播玩家死亡全局事件
        EventManager.Instance.TriggerEvent("OnPlayerDead");

        // 死亡后禁用核心功能
        movementModule?.ResetMovement();
        turretModule?.BanFire();
        GetComponent<PlayerInputHandler>().enabled = false;

        Debug.Log("[PlayerController] 玩家已死亡！");
    }
    #endregion

    #region 扩展方法（音效/动画/Buff）（未实现）
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
