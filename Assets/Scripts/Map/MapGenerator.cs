using UnityEngine;

// 一键生成平整地面，Unity编辑器内直接用，无需运行游戏
[ExecuteInEditMode]
public class MapGenerator : MonoBehaviour
{
    [Header("地面瓦片预制体")]
    public GameObject groundTilePrefab;
    [Header("地面尺寸（X×Z）")]
    public int mapWidth = 10;
    public int mapDepth = 10;
    [Header("方向增量")]
    public float increment_x = -1f;
    public float increment_y = -1f;
    public float increment_z = -1f;
    [Header("地面父物体")]
    public Transform mapParent;

    [ContextMenu("生成地面")]
    public void GenerateGround()
    {
        // 清空父物体下的旧瓦片
        for (int i = mapParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(mapParent.GetChild(i).gameObject);
        }

        // 循环生成瓦片，自动对齐1m网格
        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapDepth; z++)
            {
                GameObject tile = Instantiate(groundTilePrefab, mapParent);
                tile.transform.localPosition = new Vector3(x * increment_x, increment_y, z * increment_z);
                tile.name = $"Ground_Tile_{x}_{z}";
            }
        }
    }
}
