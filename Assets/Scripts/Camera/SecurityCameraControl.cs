using Cinemachine;
using UnityEngine;

public class SecurityCameraControl : MonoBehaviour
{
    [Header("绑定对象")]
    public Transform player;          // 玩家
    public CinemachineVirtualCamera vCam; // 虚拟相机引用

    [Header("跟随开关")]
    public bool isFollowing = true;   // 镜头跟随开关

    [Header("视野控制 (FOV)")]
    public float minFOV = 30f;        // 玩家在中心时的 FOV
    public float maxFOV = 60f;        // 玩家在边缘时的 FOV
    public float maxDistance = 20f;   // 玩家偏离中心多远时 FOV 达到最大值

    [Header("相机位置控制")]
    public Vector3 baseCamPos = new Vector3(0, 15, -15); // 相机初始位置
    public float verticalMoveThreshold = 5f; // 玩家纵向移动超过多少开始移动相机
    public float verticalMoveSpeed = 2f;    // 相机随玩家纵向移动的灵敏度

    private Vector3 centerPoint = Vector3.zero; // 地图中心

    void Update()
    {
        if (isFollowing && player != null)
        {
            UpdateCameraDynamic();
        }
        else
        {
            ResetCameraToCenter();
        }
    }

    void UpdateCameraDynamic()
    {
        // 1. 计算看向的点 (LookAt)
        // 取玩家和中心点之间的一半位置，使相机不会死死盯着玩家
        Vector3 lookAtPoint = Vector3.Lerp(centerPoint, player.position, 0.5f);
        transform.position = lookAtPoint;

        // 2. 动态 FOV (镜头拉远/拉近)
        float distFromCenter = Vector3.Distance(new Vector3(player.position.x, 0, player.position.z), centerPoint);
        float t = Mathf.Clamp01(distFromCenter / maxDistance);
        vCam.m_Lens.FieldOfView = Mathf.Lerp(minFOV, maxFOV, t);

        // 3. 相机高度/位置调整 (纵向移动超过范围)
        // 假设玩家在 X-Z 平面移动，纵向是 Z
        float playerZOffset = player.position.z;
        Vector3 targetCamPos = baseCamPos;

        if (Mathf.Abs(playerZOffset) > verticalMoveThreshold)
        {
            // 如果玩家往北走(Z+)，相机也适当往北移(Z+)，或者往高移(Y+)
            float extraOffset = (Mathf.Abs(playerZOffset) - verticalMoveThreshold) * Mathf.Sign(playerZOffset);
            targetCamPos.z += extraOffset * 0.5f; // 这里的 0.5 是衰减系数，可以按需调整
            targetCamPos.y += Mathf.Abs(extraOffset) * 0.2f; // 越高视野越广
        }

        // 平滑移动
        vCam.transform.position = Vector3.Lerp(vCam.transform.position, targetCamPos, Time.deltaTime * verticalMoveSpeed);
    }

    void ResetCameraToCenter()
    {
        // 回到中心效果
        transform.position = Vector3.Lerp(transform.position, centerPoint, Time.deltaTime * 5f);
        vCam.m_Lens.FieldOfView = Mathf.Lerp(vCam.m_Lens.FieldOfView, minFOV, Time.deltaTime * 5f);
        vCam.transform.position = Vector3.Lerp(vCam.transform.position, baseCamPos, Time.deltaTime * 5f);
    }

    // 提供给 UI 调用的公共方法
    public void SetFollowEnabled(bool state)
    {
        isFollowing = state;
    }
}