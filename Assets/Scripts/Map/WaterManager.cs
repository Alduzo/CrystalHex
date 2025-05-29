using System.Collections.Generic;
using UnityEngine;

public class WaterManager : MonoBehaviour
{
    public static WaterManager Instance;

    [Header("Water Flow Settings")]
    [SerializeField] private float initialRainAmount = 10f;
    [SerializeField] private float waterFlowSpeed = 1f;
    [SerializeField] private float riverThreshold = 5f;
    [SerializeField] private float lakeThreshold = 10f;
    [SerializeField] private int simulationTicks = 10;

    [Header("Visual Settings")]
    [SerializeField] private Material riverMaterial;
    [SerializeField] private Material lakeMaterial;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

   public void SimulateWaterFlow()
{
    foreach (var kvp in WorldMapManager.Instance.GetAllHexes())
    {
        HexData hex = kvp;

        // 🌊 Flujo inicial basado en humedad y pendiente
        float moistureFactor = hex.moisture;
        float slopeFactor = Mathf.Clamp01(hex.slope * 10f);  // Normaliza pendiente (0-1)

        // 🌊 Capacidad de absorción según tipo de terreno
        float absorption = 1f;
        switch (hex.terrainType)
        {
            case TerrainType.Plains:
            case TerrainType.Valley:
            case TerrainType.Plateau:
                absorption = 0.7f;  // Más capacidad de absorción
                break;
            case TerrainType.Hills:
                absorption = 0.4f;  // Menos absorción
                break;
            case TerrainType.Mountain:
                absorption = 0.2f;  // Muy baja absorción
                break;
        }

        // 💧 Calcular flujo inicial (retención y pendiente)
        float baseWater = moistureFactor * initialRainAmount * absorption;
        float flowBoost = slopeFactor * initialRainAmount * (1f - absorption);  // Complementa absorción

        hex.waterAmount = baseWater + flowBoost;

        // 🏞 Visualización opcional: marcar ríos o lagos si excede umbral
        if (hex.waterAmount > 1.5f)  // Umbral ajustable
        {
            hex.isRiver = true;
            hex.isLake = false;
        }
        else if (hex.waterAmount > 0.8f)  // Menor acumulación
        {
            hex.isRiver = false;
            hex.isLake = true;
        }
        else
        {
            hex.isRiver = false;
            hex.isLake = false;
        }
    }

    Debug.Log("🌊 Simulación de flujo de agua completada con lógica mejorada.");
}


    // Proporcionar materiales para que ChunkGenerator los aplique
    public Material GetRiverMaterial() => riverMaterial;
    public Material GetLakeMaterial() => lakeMaterial;

    // Configuración para UI
    public void SetRainAmount(float value) => initialRainAmount = value;
    public void SetFlowSpeed(float value) => waterFlowSpeed = value;
    public void SetRiverThreshold(float value) => riverThreshold = value;
    public void SetLakeThreshold(float value) => lakeThreshold = value;
    public void SetSimulationTicks(int value) => simulationTicks = value;
}
