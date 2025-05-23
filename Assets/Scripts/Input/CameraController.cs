
/*
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 15f;

    public float rotationSpeed = 50f; // velocidad de rotación en grados por segundo
    public Transform rotationPivot;   // el punto alrededor del cual girará la cámara (puede ser el jugador)


    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // Movimiento horizontal/vertical / flechas)
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.RightArrow)) moveX = 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) moveX = -1f;
        if (Input.GetKey(KeyCode.UpArrow)) moveZ = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) moveZ = -1f;

        // Movimiento vertical en Y (H/N)
        float moveY = 0f;
        if (Input.GetKey(KeyCode.H)) moveY = 1f;
        if (Input.GetKey(KeyCode.N)) moveY = -1f;



        Vector3 move = new Vector3(moveX, moveY, moveZ) * moveSpeed * Time.deltaTime;
        transform.position += move;

        // Zoom with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
        
        // Rotación horizontal con teclas J y K
        float rotationInput = 0f;
        if (Input.GetKey(KeyCode.K)) rotationInput = 1f;
        if (Input.GetKey(KeyCode.J)) rotationInput = -1f;

        if (Mathf.Abs(rotationInput) > 0f)
        {
            Vector3 pivot = rotationPivot != null ? rotationPivot.position : transform.position;
            // Rotar alrededor del pivot sin cambiar tilt
            transform.RotateAround(pivot, Vector3.up, rotationInput * rotationSpeed * Time.deltaTime);

            // Corrige la inclinación (mantener vista en horizontal)
            Vector3 angles = transform.eulerAngles;
            angles.x = 45f; // O el ángulo que desees mantener
            angles.z = 0f;
            transform.eulerAngles = angles;

        }

    }
}
*/