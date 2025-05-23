using UnityEngine;

/// <summary>
/// Component that controls the singleton camera that navigates the hex map.
/// </summary>
public class HexMapCamera : MonoBehaviour
{
    // Existing fields for runtime control (These will now operate relative to the new initial setup)
    [SerializeField]
    float stickMinZoom, stickMaxZoom;

    [SerializeField]
    float swivelMinZoom, swivelMaxZoom;

    [SerializeField]
    float moveSpeedMinZoom, moveSpeedMaxZoom;

    [SerializeField]
    float rotationSpeed;

    [Header("Vertical Zoom Settings")]
    [SerializeField]
    float yZoomSpeed = 10f;
    [SerializeField]
    float yMinHeight = 5f;
    [SerializeField]
    float yMaxHeight = 50f;

    [Header("Initial Camera Placement (Relative to Player)")]
    [Tooltip("Offset from player's position for camera's starting X, Y, Z. (0.81, 5, -21.57 for your request)")]
    [SerializeField]
    private Vector3 initialOffsetFromPlayer = new Vector3(0.81f, 5f, -21.57f); // Custom offset for 3rd person view
    [Tooltip("Height above player to look at. Adjust for a better view target (e.g., player's head).")]
    [SerializeField]
    private float lookAtHeightOffset = 1.5f; // Adjust this to look slightly above the player's base

    // References to child transforms for camera mechanics
    Transform swivel, stick;

    // Internal state variables
    float zoom = 1f; // Will be set to a default that works with the new perspective
    float rotationAngle; // Tracks the camera's Y-rotation for manual adjustments
    static HexMapCamera instance; // Singleton instance

    public static bool Locked
    {
        set => instance.enabled = !value;
    }

    // ValidatePosition is still available if you need to call it from other scripts,
    // but its internal implementation might need adjustment depending on how it's used.
    public static void ValidatePosition()
    {
        // This method's behavior might need to be adjusted if it was used to reposition
        // the camera based on an old system. For now, it remains a placeholder.
    }

    void Awake()
    {
        // Get references to child transforms (swivel and stick)
        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);

        // Reset swivel and stick to neutral local positions and rotations.
        // Their main purpose will now be for runtime adjustments (like mouse scroll zoom affecting distance/pitch)
        // relative to the new initial camera world position and rotation set in Start().
        swivel.localPosition = Vector3.zero;
        swivel.localRotation = Quaternion.identity;
        stick.localPosition = Vector3.zero;

        // Set 'zoom' to a default value that works well with the new 3rd person perspective.
        // This might require experimentation in the editor. 0.5f is a common midpoint.
        zoom = 0.5f;
        // Apply the initial zoom state to stick/swivel immediately in Awake.
        // This ensures the stick and swivel are at a consistent state when the game starts.
        AdjustZoom(0); // Calling with 0 delta just applies the current 'zoom' value.
    }

    void OnEnable()
    {
        // Set the singleton instance when the component is enabled
        instance = this;
    }

    void Start()
    {
        // Position and orient the camera based on the player's position and desired offset.
        // This is called in Start() to ensure the player object is fully initialized.
        SetInitialCameraPositionAndLook();
    }

    void Update()
    {
        // --- ZOOM (Mouse Scroll Wheel) ---
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        if (zoomDelta != 0f)
        {
            AdjustZoom(zoomDelta);
        }

        // --- ROTATION (Q Key for Left Rotation) ---
        // 'E' is now dedicated to vertical zoom.
        float rotationDelta = 0f;
        if (Input.GetKey(KeyCode.Q)) rotationDelta = -1f; // Q rotates left

        if (rotationDelta != 0f)
        {
            AdjustRotation(rotationDelta);
        }

        // --- VERTICAL ZOOM (Y-level) (E/R Keys) ---
        float yMoveDelta = 0f;
        if (Input.GetKey(KeyCode.O)) yMoveDelta = -1f; // E to zoom in (move camera down)
        if (Input.GetKey(KeyCode.L)) yMoveDelta = 1f;  // R to zoom out (move camera up)

        if (yMoveDelta != 0f)
        {
            AdjustYHeight(yMoveDelta);
        }

        // --- CAMERA MOVEMENT (Arrow Keys for Horizontal Panning) ---
        float xDelta = 0f;
        if (Input.GetKey(KeyCode.RightArrow)) xDelta = 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) xDelta = -1f;

        float zDelta = 0f;
        if (Input.GetKey(KeyCode.UpArrow)) zDelta = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) zDelta = -1f;

        if (xDelta != 0f || zDelta != 0f)
        {
            AdjustPosition(xDelta, zDelta);
        }

        // --- Manual Focus On Player (C Key) ---
        // When 'C' is pressed, re-apply the initial camera position and look-at.
        // This effectively snaps the camera back to the defined 3rd person view.
        if (Input.GetKeyDown(KeyCode.C))
        {
            SetInitialCameraPositionAndLook();
        }
    }

    /// <summary>
    /// Adjusts the camera's distance and pitch (angle of view) based on the 'zoom' value.
    /// This uses the swivel and stick child transforms.
    /// </summary>
    /// <param name="delta">Amount to change the zoom by. Set to 0 to just apply current 'zoom' value.</param>
    void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);

        // Interpolate distance for the camera's stick (zoom in/out distance)
        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        // Interpolate angle for the camera's swivel (pitch/tilt)
        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }

    /// <summary>
    /// Adjusts the camera's Y-axis rotation (yaw).
    /// </summary>
    /// <param name="delta">Positive for clockwise, negative for counter-clockwise.</param>
    void AdjustRotation(float delta)
    {
        rotationAngle += delta * rotationSpeed * Time.deltaTime;
        if (rotationAngle < 0f)
        {
            rotationAngle += 360f;
        }
        else if (rotationAngle >= 360f)
        {
            rotationAngle -= 360f;
        }
        transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
    }

    /// <summary>
    /// Moves the camera horizontally in world space.
    /// </summary>
    void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * damping * Time.deltaTime;

        Vector3 position = transform.localPosition;
        position += direction * distance;
        transform.localPosition = position;
    }

    /// <summary>
    /// Adjusts the camera's Y height (vertical position) within defined limits.
    /// </summary>
    /// <param name="delta">Positive to move up, negative to move down.</param>
    private void AdjustYHeight(float delta)
    {
        Vector3 position = transform.position;
        position.y = Mathf.Clamp(position.y + delta * yZoomSpeed * Time.deltaTime, yMinHeight, yMaxHeight);
        transform.position = position;
    }

    /// <summary>
    /// Sets the camera's initial world position relative to the player and makes it look at the player.
    /// This method is called once in Start() and can be triggered manually via the 'C' key.
    /// </summary>
    private void SetInitialCameraPositionAndLook()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            // Set camera's world position based on player's position and the specified offset
            transform.position = playerObject.transform.position + initialOffsetFromPlayer;

            // Make the camera look at the player's position, with an optional height offset
            // (e.g., to aim at the player's head instead of their feet).
            transform.LookAt(playerObject.transform.position + Vector3.up * lookAtHeightOffset);

            // After directly setting the camera's rotation using LookAt,
            // update the 'rotationAngle' variable to match the camera's current Y-rotation.
            // This ensures that subsequent manual rotations (using 'Q' key) continue correctly
            // from the new orientation.
            rotationAngle = transform.localRotation.eulerAngles.y;
        }
        else
        {
            Debug.LogWarning("HexMapCamera: Player GameObject not found! Make sure your player has the tag 'Player' and is active in the scene.");
        }
    }
}