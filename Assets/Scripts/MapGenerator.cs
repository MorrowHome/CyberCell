using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance;

    [Header("��ͼ����")]
    [SerializeField] private int COLS = 10;
    [SerializeField] private int ROWS = 10;
    [SerializeField] private int LAYS = 10;
    [SerializeField] private float sizeScale = 5f;

    [Header("����Prefab")]
    [SerializeField] private GameObject cubeGridPrefab;                // ��ͨ��
    [SerializeField] private GameObject cubeGridWithResourcesPrefab;   // ��Դ��

    [Header("��Դ�ֲ�����")]
    [SerializeField, Range(0f, 1f)] private float resourceSpawnChance = 0.1f; // ��������
    [SerializeField, Range(0f, 1f)] private float clusterBonus = 0.25f;       // �ھӼӳɸ���

    [Header("������ʾ")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Transform lineContainer;
    [SerializeField] private GameObject heartCellPrefab;
    [SerializeField] private Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f);




    [SerializeField] private Transform cubeGridContainer;




    public Dictionary<Vector3, Transform> Vector3_Transform_Dictionary = new Dictionary<Vector3, Transform>();
    public Dictionary<Transform, Vector3> Transform_Vector3_Dictionary = new Dictionary<Transform, Vector3>();
    public List<Transform> allGrids = new List<Transform>();

    private int ID = 0;
    private bool[,,] resourceMap;  // ��¼�����Ƿ�����Դ��

    public Transform heartCellTransform;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        resourceMap = new bool[COLS, LAYS, ROWS];

        GenerateMap();
        DrawGridLines();
        GenerateHeartCell();
    }

    private void GenerateMap()
    {
        for (int x = 0; x < COLS; x++)
        {
            for (int y = 0; y < LAYS; y++)
            {
                for (int z = 0; z < ROWS; z++)
                {
                    float chance = resourceSpawnChance;

                    // ����ھӣ��������Դ�������Ӹ���
                    if (HasResourceNeighbor(x, y, z))
                    {
                        chance += clusterBonus;
                    }

                    bool isResource = Random.value < chance;
                    resourceMap[x, y, z] = isResource;

                    Vector3 position = new Vector3(x * sizeScale, y * sizeScale, z * sizeScale);
                    GameObject prefabToUse = isResource ? cubeGridWithResourcesPrefab : cubeGridPrefab;

                    GameObject ins = Instantiate(prefabToUse, position, Quaternion.identity, cubeGridContainer);
                    allGrids.Add(ins.transform);
                    Vector3_Transform_Dictionary.Add(new Vector3(x,y,z), ins.transform);
                    Transform_Vector3_Dictionary.Add(ins.transform, new Vector3(x, y, z));
                    ins.name = isResource ? $"ResourceCube_{ID++}" : $"Cube_{ID++}";
                }
            }
        }
    }

    // �ж�ĳ���Ƿ�����Դ�ھ�
    private bool HasResourceNeighbor(int x, int y, int z)
    {
        // ֻ���֮ǰ���ɵĸ��ӣ�����δ��δ���ɵĸ���Ӱ�죩
        int[] dx = { -1, 0, 1 };
        int[] dy = { -1, 0, 1 };
        int[] dz = { -1, 0, 1 };

        foreach (int i in dx)
        {
            foreach (int j in dy)
            {
                foreach (int k in dz)
                {
                    if (i == 0 && j == 0 && k == 0) continue; // ��������

                    int nx = x + i, ny = y + j, nz = z + k;
                    if (nx >= 0 && nx < COLS &&
                        ny >= 0 && ny < LAYS &&
                        nz >= 0 && nz < ROWS)
                    {
                        if (resourceMap[nx, ny, nz]) return true;
                    }
                }
            }
        }
        return false;
    }

    private void GenerateHeartCell()
    {
        Vector3 centerPoint = new Vector3(
            (int)(COLS / 2) * sizeScale,
            (int)(LAYS / 2) * sizeScale,
            (int)(ROWS / 2) * sizeScale
        );
        Vector3 centerVector3 = new Vector3(
            (int)(COLS / 2) ,
            (int)(LAYS / 2) ,
            (int)(ROWS / 2) 
        );
        Vector3_Transform_Dictionary.TryGetValue(centerVector3, out Transform cubeGrid);
        GameObject ins = Instantiate(heartCellPrefab, centerPoint, Quaternion.identity, cubeGrid);
        heartCellTransform = ins.transform;
        cubeGrid.GetComponent<CubeGrid>().whatIsOnMe = ins.transform;
        
    }

    private void DrawGridLines()
    {
        GameObject lineParent = new GameObject("GridLines");
        lineParent.transform.parent = lineContainer;

        // X����
        for (int y = 0; y <= LAYS; y++)
        {
            for (int z = 0; z <= ROWS; z++)
            {
                DrawLine(new Vector3(0, y * sizeScale, z * sizeScale) + offset,
                         new Vector3(COLS * sizeScale, y * sizeScale, z * sizeScale) + offset,
                         lineParent.transform);
            }
        }

        // Y����
        for (int x = 0; x <= COLS; x++)
        {
            for (int z = 0; z <= ROWS; z++)
            {
                DrawLine(new Vector3(x * sizeScale, 0, z * sizeScale) + offset,
                         new Vector3(x * sizeScale, LAYS * sizeScale, z * sizeScale) + offset,
                         lineParent.transform);
            }
        }

        // Z����
        for (int x = 0; x <= COLS; x++)
        {
            for (int y = 0; y <= LAYS; y++)
            {
                DrawLine(new Vector3(x * sizeScale, y * sizeScale, 0) + offset,
                         new Vector3(x * sizeScale, y * sizeScale, ROWS * sizeScale) + offset,
                         lineParent.transform);
            }
        }
    }

    private void DrawLine(Vector3 start, Vector3 end, Transform parent)
    {
        GameObject lineObj = new GameObject("Line");
        lineObj.transform.parent = parent;
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.useWorldSpace = true;
    }
}
