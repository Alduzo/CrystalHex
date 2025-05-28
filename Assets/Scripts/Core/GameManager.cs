using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Cámaras")]
    public Camera mainCamera;          // Cámara cercana (LOD 0)
    public Camera minimapCamera;       // Cámara media (LOD 1)
    public Camera globeCamera;         // Cámara lejana (LOD 2)

    [Header("Zoom Thresholds")]
    public float closeThreshold = 10f;
    public float mediumThreshold = 30f;
    public float globeThreshold = 50f;

    [Header("Referencias")]
    public ChunkManager chunkManager;
    public WorldMapManager worldMapManager;

    [SerializeField] public RawImage minimapImage;

    private bool minimapGenerated = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        mainCamera.gameObject.SetActive(true);  // Siempre activa
        minimapCamera.gameObject.SetActive(false);  // Inicialmente desactiva
    }

    private void Update()
    {
        float currentZoom = mainCamera.transform.position.y;

        if (currentZoom <= closeThreshold)
        {
            // Nivel cercano - solo mainCamera activa
            if (minimapCamera.enabled) minimapCamera.enabled = false;
            if (globeCamera != null && globeCamera.enabled) globeCamera.enabled = false;
            minimapGenerated = false;
        }
        else if (currentZoom > closeThreshold && currentZoom <= mediumThreshold)
        {
            if (!minimapCamera.enabled) minimapCamera.enabled = true;

            if (!minimapGenerated)
            {
                Debug.Log("🗺 Generando minimapa procedural...");
                worldMapManager.GenerateMinimapTextureOrSphere();
                minimapGenerated = true;
            }
        }
        else if (currentZoom > mediumThreshold)
        {
            if (minimapCamera.enabled) minimapCamera.enabled = false;  // Desactiva minimapCamera a este nivel
            // Si llegas a tener una globeCamera, aquí podrías activar otra, pero vamos a dejarla desactivada por ahora
        }
    }
}
