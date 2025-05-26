using System.Collections.Generic;


public enum TerrainType
{
    Ocean,
    CoastalWater,
    SandyBeach,
    RockyBeach,
    Plains,
    Hills,
    Valley,
    Mountain,
    Plateau
}

public enum HexType { Natural, Rural, Urban }
public enum ResourceType { Minerals, Wood, Food, Water, Energy }

public class HexData
{
    public HexCoordinates coordinates;

    // Capa estática
    public float elevation;
    public float slope;
    public float moisture;
    public float temperature;
    public TerrainType terrainType;
    public bool neighborsAssigned = false;




    // Capa dinámica
    public HexType hexType = HexType.Natural;
    public bool isExplored = false;
    public Dictionary<ResourceType, float> extractedResources = new();

    // Vecinos (sólo coordenadas, útil para persistencia o reconstrucción rápida)
    public List<HexCoordinates> neighborCoords = new();

    // En runtime, puede poblarse dinámicamente con referencias (si es necesario)
    public List<HexData> neighborRefs = new();

        // Capa de agua (propiedades dinámicas)
    public float waterAmount = 0f;    // Cantidad de agua acumulada
    public bool isRiver = false;      // Indica si forma parte de un río
    public bool isLake = false;       // Indica si forma parte de un lago

    
}