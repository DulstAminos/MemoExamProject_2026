using UnityEngine;

/// <summary>
/// 玩家输入处理脚本（仅负责采集输入，传递给PlayerController）
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    private PlayerController _playerController;
    private bool _isInitialized;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        if (_playerController == null)
        {
            Debug.LogError($"[{nameof(PlayerInputHandler)}] 未找到PlayerController组件！");
            enabled = false;
        }
        _isInitialized = true;
    }

    private void Update()
    {
        if (!_isInitialized) return;

        // 1. 采集移动输入（WASD）
        CollectMovementInput();

        // 2. 采集炮台目标输入（鼠标左键）
        CollectTurretTargetInput();
    }

    // 采集移动输入并传递给PlayerController
    private void CollectMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 moveDir = new Vector3(horizontal, 0, vertical).normalized;

        // 传递移动方向给总控
        _playerController.OnReceiveMoveInput(moveDir);
    }

    // 采集鼠标点击的炮台目标点并传递
    private void CollectTurretTargetInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, LayerMask.GetMask("Ground")))
            {
                // 传递目标点给总控
                _playerController.OnReceiveTurretTargetInput(hit.point);
            }
        }
    }
}
