using UnityEngine;

public class PlayerController : TankBase
{
    private Vector3 movementInput;
    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
    private BuffSystem buffSystem;

    protected override void Awake()
    {
        base.Awake();
        buffSystem = GetComponent<BuffSystem>();
    }

    void Update()
    {
        // 获取移动输入
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        movementInput = new Vector3(moveX, 0f, moveZ).normalized;

        // 获取瞄准与开火输入
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (groundPlane.Raycast(ray, out float rayDistance))
            {
                Vector3 aimPoint = ray.GetPoint(rayDistance);

                // 调用基类的统合方法：瞄准，如果按了左键且瞄准完成则自动开火
                AimAndFire(aimPoint, true);
            }
        }
    }

    void FixedUpdate()
    {
        // 调用基类的物理移动
        MoveTank(movementInput);
    }

    // 武器切换
    public override void SwitchWeapon(GameObject newWeaponPrefab)
    {
        // 销毁旧武器模型（因为是独有配置，直接Destroy即可，不需要对象池）
        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject);
        }

        // 生成新武器作为炮塔子物体
        GameObject weaponObj = Instantiate(newWeaponPrefab, turretTransform);
        weaponObj.transform.localPosition = Vector3.zero;
        weaponObj.transform.localRotation = Quaternion.identity;

        currentWeapon = weaponObj.GetComponent<WeaponControllerBase>();

        // 通知 Buff 系统重新同步反弹次数
        if (buffSystem != null) buffSystem.NotifyWeaponChanged(currentWeapon);
    }

    // 重写受伤方法，加入护盾免疫伤害逻辑
    public override void TakeDamage(float damageAmount)
    {
        if (buffSystem != null && buffSystem.isShielded)
        {
            Debug.Log("护盾抵挡了伤害！");
            return;
        }
        base.TakeDamage(damageAmount); // 没护盾就调用基类扣血
    }

    // 移速修改接口
    public void AddSpeed(float amount)
    {
        moveSpeed += amount;
    }

    protected override void Die()
    {
        Debug.Log("玩家阵亡！");
        // 玩家死亡不需要回收，直接隐藏，留给GameManager处理游戏失败
        gameObject.SetActive(false);
    }
}