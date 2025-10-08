using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MiniMap : MonoBehaviour
{
    [Header("RawImage to display the mini map")]
    public RawImage miniMapImage;

    [Header("Colors")]
    public Color emptyColor = Color.black;
    public Color bloodVesselColor = Color.red;
    public Color collectorColor = Color.green;
    public Color resourceColor = Color.purple;
    public Color enemyColor = Color.yellow;
    public Color towerColor = Color.blue;
    public Color playerColor = Color.cyan;
    public Color viewRectColor = new Color(1f, 1f, 1f, 0.3f);

    [Header("Player & View")]
    public Vector2Int playerPosition;
    public Vector2Int viewSize = new Vector2Int(5, 5);

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.1f;
    public float minZoom = 0.5f;
    public float maxZoom = 2f;

    [Header("Drag Settings")]
    public float dragSpeed = 1f;

    private Texture2D miniMapTexture;
    private int width, height;
    private RectTransform miniMapRect;

    private bool isDragging = false;
    private Vector2 lastMousePosition;

    private void Start()
    {
        if (MapGenerator.Instance == null)
        {
            Debug.LogError("MapGenerator not found!");
            return;
        }

        width = MapGenerator.Instance.COLS;
        height = MapGenerator.Instance.ROWS;

        miniMapTexture = new Texture2D(width, height);
        miniMapTexture.filterMode = FilterMode.Point;
        miniMapTexture.wrapMode = TextureWrapMode.Clamp;

        if (miniMapImage != null)
        {
            miniMapImage.texture = miniMapTexture;
            miniMapRect = miniMapImage.GetComponent<RectTransform>();
        }

        DrawMiniMap();
    }

    private void Update()
    {
        UpdatePlayerPosition();
        DrawMiniMap();
        HandleZoom();
        HandleDrag();
    }

    private void UpdatePlayerPosition()
    {

    }

    private void DrawMiniMap()
    {
        if (MapGenerator.Instance == null || miniMapTexture == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = new Vector3(x, 0, y);
                MapGenerator.Instance.Vector3_Transform_Dictionary.TryGetValue(worldPos, out var cellT);

                Color color = emptyColor;

                if (cellT != null)
                {
                    CubeGrid cell = cellT.GetComponent<CubeGrid>();
                    if (cell != null && cell.whatIsOnMe != null)
                    {
                        if (cell.whatIsOnMe.TryGetComponent<BloodVessel>(out _))
                            color = bloodVesselColor;
                        else if (cell.whatIsOnMe.TryGetComponent<GlucoseCollectorCell>(out _))
                            color = collectorColor;
                        else if (cell.whatIsOnMe.TryGetComponent<GlucoseCubeGrid>(out _))
                            color = resourceColor;
                        else if (cell.whatIsOnMe.TryGetComponent<Virus>(out _))
                            color = enemyColor;
                        else if (cell.whatIsOnMe.TryGetComponent<ImmuneBCell>(out _))
                            color = enemyColor;
                    }
                    else if (cellT.GetComponent<GlucoseCubeGrid>())
                    {
                        color = resourceColor;
                    }
                }

                miniMapTexture.SetPixel(x, y, color);
            }
        }

        // 玩家
        if (playerPosition.x >= 0 && playerPosition.x < width && playerPosition.y >= 0 && playerPosition.y < height)
            miniMapTexture.SetPixel(playerPosition.x, playerPosition.y, playerColor);

        // 玩家视野矩形
        for (int dx = -viewSize.x; dx <= viewSize.x; dx++)
        {
            for (int dy = -viewSize.y; dy <= viewSize.y; dy++)
            {
                int px = playerPosition.x + dx;
                int py = playerPosition.y + dy;
                if (px >= 0 && px < width && py >= 0 && py < height)
                    miniMapTexture.SetPixel(px, py, viewRectColor);
            }
        }

        miniMapTexture.Apply();
    }

    private void HandleZoom()
    {
        if (miniMapRect == null || Mouse.current == null) return;

        Vector2 scrollDelta = Mouse.current.scroll.ReadValue();
        if (Mathf.Abs(scrollDelta.y) > 0.01f)
        {
            Vector3 scale = miniMapRect.localScale;
            scale += Vector3.one * scrollDelta.y * zoomSpeed;
            scale.x = Mathf.Clamp(scale.x, minZoom, maxZoom);
            scale.y = Mathf.Clamp(scale.y, minZoom, maxZoom);
            miniMapRect.localScale = scale;
        }
    }

    private void HandleDrag()
    {
        if (miniMapRect == null || Mouse.current == null) return;

        // 鼠标左键按下开始拖拽
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isDragging = true;
            lastMousePosition = Mouse.current.position.ReadValue();
        }

        // 鼠标左键释放结束拖拽
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 currentMousePos = Mouse.current.position.ReadValue();
            Vector2 delta = currentMousePos - lastMousePosition;
            miniMapRect.anchoredPosition += delta * dragSpeed;
            lastMousePosition = currentMousePos;
        }
    }
}
