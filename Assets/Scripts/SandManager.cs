using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 50;
    public int height = 30;
    public float updateTime = 0.1f;
    public GameObject cellPrefab;

    [Header("Camera Settings")]
    public float cameraSpeed = 5f;
    public float zoomSpeed = 2f;
    public float minZoom = 2f;
    public float maxZoom = 10f;

    private bool[,] grid;
    private bool[,] nextGrid;
    private GameObject[,] cellObjects;
    private float timer;
    private bool isPaused = false;

    private Camera mainCamera;
    private Vector2 cameraMovement;

    void Start()
    {
        mainCamera = Camera.main;
        grid = new bool[width, height];
        nextGrid = new bool[width, height];
        cellObjects = new GameObject[width, height];
        GenerateGrid();
        RandomizeGrid();
    }

    void Update()
    {
        HandleCameraMovement();
        HandleCameraZoom();
        HandleMouseInput();

        if (isPaused) return;

        timer += Time.deltaTime;
        if (timer >= updateTime)
        {
            Step();
            UpdateVisuals();
            timer = 0f;
        }
    }

    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cell = Instantiate(cellPrefab, new Vector3(x, y, 0), Quaternion.identity);
                cell.transform.parent = transform;
                cellObjects[x, y] = cell;
            }
        }
    }

    void RandomizeGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = Random.value > 0.7f;
            }
        }
        UpdateVisuals();
    }

    void ResetGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = false;
            }
        }
        RandomizeGrid();
    }

    void Step()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                nextGrid[x, y] = grid[x, y];
            }
        }

        for (int y = 1; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y])
                {
                    if (!grid[x, y - 1])
                    {
                        nextGrid[x, y] = false;
                        nextGrid[x, y - 1] = true;
                    }
                    else
                    {
                        bool moved = false;
                        bool goLeftFirst = (Random.value > 0.5f);

                        if (goLeftFirst)
                        {
                            if (x > 0 && !grid[x - 1, y - 1])
                            {
                                nextGrid[x, y] = false;
                                nextGrid[x - 1, y - 1] = true;
                                moved = true;
                            }
                            else if (x < width - 1 && !grid[x + 1, y - 1])
                            {
                                nextGrid[x, y] = false;
                                nextGrid[x + 1, y - 1] = true;
                                moved = true;
                            }
                        }
                        else
                        {
                            if (x < width - 1 && !grid[x + 1, y - 1])
                            {
                                nextGrid[x, y] = false;
                                nextGrid[x + 1, y - 1] = true;
                                moved = true;
                            }
                            else if (x > 0 && !grid[x - 1, y - 1])
                            {
                                nextGrid[x, y] = false;
                                nextGrid[x - 1, y - 1] = true;
                                moved = true;
                            }
                        }
                    }
                }
            }
        }

        var temp = grid;
        grid = nextGrid;
        nextGrid = temp;
    }

    void UpdateVisuals()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var rend = cellObjects[x, y].GetComponent<SpriteRenderer>();
                rend.color = grid[x, y] ? Color.yellow : Color.black;
            }
        }
    }

    void HandleCameraMovement()
    {
        Vector3 movement = Vector3.zero;

        if (Keyboard.current.wKey.isPressed)
            movement.y += 1;
        if (Keyboard.current.sKey.isPressed)
            movement.y -= 1;
        if (Keyboard.current.aKey.isPressed)
            movement.x -= 1;
        if (Keyboard.current.dKey.isPressed)
            movement.x += 1;

        if (movement.magnitude > 1)
            movement.Normalize();

        mainCamera.transform.position += movement * cameraSpeed * Time.deltaTime;
    }

    void HandleCameraZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y / 1000f;
        if (scroll != 0)
        {
            float newSize = mainCamera.orthographicSize - (scroll * zoomSpeed * 0.1f);
            mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }

    void HandleMouseInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            int gridX = Mathf.RoundToInt(worldPosition.x);
            int gridY = Mathf.RoundToInt(worldPosition.y);

            if (gridX >= 0 && gridX < width && gridY >= 0 && gridY < height)
            {
                grid[gridX, gridY] = true;
                UpdateVisuals();
            }
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            isPaused = !isPaused;
            Debug.Log(isPaused ? "Simulación pausada" : "Simulación reanudada");
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetGrid();
            Debug.Log("Simulación reiniciada");
        }
    }
}
