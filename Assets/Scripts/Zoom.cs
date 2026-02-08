using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ZoomAndPan : MonoBehaviour
{
    [Header("Zoom settings")]
    public float minSize = 2f;           // most zoomed in
    public float maxSize = 10f;          // most zoomed out
    public float zoomSpeed = 4f;
    public float zoomSmoothness = 0.12f;

    [Header("Pan settings")]
    public float panSpeed = 1f;          // how fast the camera moves (multiplier)
    public bool invertPanX = false;      // sometimes people prefer inverted drag
    public bool invertPanY = false;

    private Camera mainCamera;
    private float targetSize;

    // Panning
    private Vector3 lastMouseWorldPos;
    private bool isPanning;

    [Header("Input")]
    [SerializeField] private InputAction zoomAction;       // scroll wheel
    [SerializeField] private InputAction panAction;        // usually left mouse button

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
            enabled = false;
            return;
        }

        targetSize = mainCamera.orthographicSize;
    }

    private void OnEnable()
    {
        zoomAction?.Enable();
        panAction?.Enable();

        zoomAction.performed += OnZoomPerformed;

        // We only use performed to detect press/release reliably
        panAction.performed += ctx => TryStartPanning();
        panAction.canceled += ctx => StopPanning();
    }

    private void OnDisable()
    {
        if (zoomAction != null)
        {
            zoomAction.performed -= OnZoomPerformed;
        }

        if (panAction != null)
        {
            panAction.performed -= ctx => TryStartPanning();
            panAction.canceled -= ctx => StopPanning();
        }

        zoomAction?.Disable();
        panAction?.Disable();
    }

    private void OnZoomPerformed(InputAction.CallbackContext ctx)
    {
        float scroll = ctx.ReadValue<float>();
        if (Mathf.Abs(scroll) < 0.01f) return;

        float zoomDirection = -Mathf.Sign(scroll);
        float zoomAmount = zoomSpeed * zoomDirection;

        targetSize += zoomAmount;
        targetSize = Mathf.Clamp(targetSize, minSize, maxSize);
    }

    // Called when left mouse button is pressed
    private void TryStartPanning()
    {
        // Only start panning if NOT over UI
        if (IsPointerOverUI())
            return;

        isPanning = true;
        lastMouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
    }

    private void StopPanning()
    {
        isPanning = false;
    }

    // Safe helper — now called from Update/LateUpdate where UI state is up-to-date
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition   // or use pointer position for new input system
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            // Only count "real" UI layer (5)
            if (result.gameObject.layer == 5)
            {
                return true;
            }
        }

        return false;
    }

    private void LateUpdate()
    {
        if (!mainCamera.orthographic) return;

        // ─── Zoom (keeps point under mouse fixed) ──────────────────
        float currentSize = mainCamera.orthographicSize;
        if (Mathf.Abs(currentSize - targetSize) > 0.001f)
        {
            float newSize = Mathf.Lerp(currentSize, targetSize, zoomSmoothness);

            Vector3 mouseBefore = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mainCamera.orthographicSize = newSize;
            Vector3 mouseAfter = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            mainCamera.transform.position += mouseBefore - mouseAfter;
        }

        if (isPanning)
        {
            if (IsPointerOverUI())
            {
                isPanning = false;
                return;
            }

            Vector3 currentMouseScreen = Input.mousePosition;           // ← screen space is stable
            Vector3 currentMouseWorld = mainCamera.ScreenToWorldPoint(currentMouseScreen);

            // Option A: only apply movement if mouse actually moved a bit
            Vector3 deltaWorld = currentMouseWorld - lastMouseWorldPos;

            // Small threshold helps remove micro-jitter from float precision / input noise
            if (deltaWorld.sqrMagnitude > 0.0005f)   // ≈ 0.02–0.03 world units
            {
                float dirX = invertPanX ? -1f : 1f;
                float dirY = invertPanY ? -1f : 1f;

                Vector3 move = new Vector3(deltaWorld.x * dirX, deltaWorld.y * dirY, 0f) * panSpeed;

                mainCamera.transform.position -= move;

                // Only update reference point **after** we moved
                lastMouseWorldPos = mainCamera.ScreenToWorldPoint(currentMouseScreen);
            }
            // else: mouse didn't move enough → don't update reference position
        }
    }

    public void SetZoomLevel(float normalized)
    {
        targetSize = Mathf.Lerp(minSize, maxSize, Mathf.Clamp01(normalized));
    }
}