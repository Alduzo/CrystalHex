using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 15f;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
{
    // Movimiento horizontal/vertical en X-Z (WASD / flechas)
    float moveX = Input.GetAxis("Horizontal");
    float moveZ = Input.GetAxis("Vertical");

    // Movimiento vertical en Y (Q/E)
    float moveY = 0f;
    if (Input.GetKey(KeyCode.E)) moveY = 1f;
    if (Input.GetKey(KeyCode.Q)) moveY = -1f;

    Vector3 move = new Vector3(moveX, moveY, moveZ) * moveSpeed * Time.deltaTime;
    transform.position += move;

        // Zoom with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }
}
