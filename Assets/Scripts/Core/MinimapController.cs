using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("Minimap Settings")]
    public RawImage minimapImage;
    public RectTransform playerMarker;
    public int minimapResolution = 512;
    public float zoomSpeed = 0.1f;
    public float globeThreshold = 40f;

    private float minimapZoom = 1f;
    private WorldMapManager worldMap;
    private Transform playerTransform;

    private void Start()
    {
        worldMap = WorldMapManager.Instance;
        playerTransform = Camera.main.transform;
            minimapImage.texture = worldMap.GenerateMinimapTexture(minimapResolution);

    }

    private void Update()
    {
        HandleZoom();
        UpdatePlayerMarker();
        HandleExpansion();
    }

    private void HandleZoom()
    {
        float zoomDelta = 0f;
        if (Input.GetKey(KeyCode.L)) zoomDelta = 1f;
        if (Input.GetKey(KeyCode.O)) zoomDelta = -1f;

        if (zoomDelta != 0f)
        {
            minimapZoom = Mathf.Clamp(minimapZoom - zoomDelta * zoomSpeed * Time.deltaTime, 0.2f, 5f);
            minimapImage.rectTransform.localScale = Vector3.one * minimapZoom;
        }
    }

    private void UpdatePlayerMarker()
    {
        int resolution = minimapResolution;
        int minQ = int.MaxValue, maxQ = int.MinValue;
        int minR = int.MaxValue, maxR = int.MinValue;
        foreach (var hex in worldMap.GetAllHexes())
        {
            minQ = Mathf.Min(minQ, hex.coordinates.Q);
            maxQ = Mathf.Max(maxQ, hex.coordinates.Q);
            minR = Mathf.Min(minR, hex.coordinates.R);
            maxR = Mathf.Max(maxR, hex.coordinates.R);
        }

        float scaleQ = (float)resolution / (maxQ - minQ + 1);
        float scaleR = (float)resolution / (maxR - minR + 1);

        HexCoordinates playerCoord = HexCoordinates.FromWorldPosition(playerTransform.position, HexRenderer.SharedOuterRadius);
        int pixelX = Mathf.RoundToInt((playerCoord.Q - minQ) * scaleQ);
        int pixelY = Mathf.RoundToInt((playerCoord.R - minR) * scaleR);

        RectTransform rt = minimapImage.rectTransform;
        Vector2 markerPos = new Vector2(pixelX * rt.rect.width / resolution, pixelY * rt.rect.height / resolution);
        playerMarker.anchoredPosition = markerPos;
    }

    private void HandleExpansion()
    {
        if (playerTransform.position.y > globeThreshold)
        {
            minimapImage.rectTransform.anchorMin = Vector2.zero;
            minimapImage.rectTransform.anchorMax = Vector2.one;
            minimapImage.rectTransform.offsetMin = Vector2.zero;
            minimapImage.rectTransform.offsetMax = Vector2.zero;
        }
        else
        {
            minimapImage.rectTransform.anchorMin = new Vector2(0.7f, 0.7f);
            minimapImage.rectTransform.anchorMax = new Vector2(1f, 1f);
            minimapImage.rectTransform.offsetMin = Vector2.zero;
            minimapImage.rectTransform.offsetMax = Vector2.zero;
        }
    }
}
