using DG.Tweening;
using UnityEngine;

public class TankFireEffect : MonoBehaviour
{
    [Header("后坐力设置")]
    [SerializeField] private float recoilDistance = 0.3f; // 后退距离
    [SerializeField] private float recoilDuration = 0.05f; // 后退耗时
    [SerializeField] private float restoreDuration = 0.2f; // 回复耗时

    private Vector3 originalLocalPos;

    void Start()
    {
        // 记录初始局部坐标
        originalLocalPos = transform.localPosition;
    }

    // 暴露给开火脚本调用的接口
    public void PlayRecoil()
    {
        // 停止当前正在运行的位移动画，防止连发时位置错乱
        transform.DOKill();

        // 确保回到初始位置开始
        transform.localPosition = originalLocalPos;

        // 先向后移动 (局部坐标 Z 轴负方向)
        // 然后紧接着移动回初始位置
        transform.DOLocalMoveZ(originalLocalPos.z - recoilDistance, recoilDuration)
            .OnComplete(() =>
            {
                transform.DOLocalMoveZ(originalLocalPos.z, restoreDuration).SetEase(Ease.OutQuad);
            });
    }
}