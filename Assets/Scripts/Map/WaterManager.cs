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

    public void SimulateWaterFlow(Dictionary<HexCoordinates, HexData> worldMap)
    {
        if (worldMap == null)
        {
            Debug.LogWarning("🌊 WaterManager: worldMap es nulo. Simulación cancelada.");
            return;
        }

        Debug.Log("🌊 Iniciando simulación de flujo de agua...");

        // Inicializa agua
        foreach (var hex in worldMap.Values)
        {
            float rainfall = PerlinUtility.Perlin(hex.coordinates, WorldMapManager.Instance.perlinSettings.moistureFreq, WorldMapManager.Instance.perlinSettings.seed);
            hex.waterAmount = rainfall * initialRainAmount;
        }

        // Simulación dinámica
        for (int tick = 0; tick < simulationTicks; tick++)
        {
            Dictionary<HexData, float> waterTransfer = new();

            foreach (var hex in worldMap.Values)
            {
                if (hex.waterAmount <= 0f) continue;

                HexData lowestNeighbor = null;
                float lowestElevation = hex.elevation;

                foreach (var neighbor in hex.neighborRefs)
                {
                    if (neighbor.elevation < lowestElevation)
                    {
                        lowestElevation = neighbor.elevation;
                        lowestNeighbor = neighbor;
                    }
                }

                if (lowestNeighbor != null && lowestNeighbor != hex)
                {
                    float flowAmount = Mathf.Min(hex.waterAmount, waterFlowSpeed);
                    hex.waterAmount -= flowAmount;

                    if (!waterTransfer.ContainsKey(lowestNeighbor))
                        waterTransfer[lowestNeighbor] = 0f;

                    waterTransfer[lowestNeighbor] += flowAmount;
                }
            }

            foreach (var kvp in waterTransfer)
                kvp.Key.waterAmount += kvp.Value;
        }

        // Marcar ríos y lagos
        foreach (var hex in worldMap.Values)
        {
            hex.isRiver = hex.waterAmount >= riverThreshold;
            hex.isLake = hex.waterAmount >= lakeThreshold && !hex.isRiver;
        }

        Debug.Log("🌊 Simulación de agua completada.");
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
