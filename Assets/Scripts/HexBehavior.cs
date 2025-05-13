using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexBehavior : MonoBehaviour
{
    public enum HexState { Empty, Influenced, Seeded, Growing, Full }
    public enum PlayerType { Red, Blue, Green, Yellow, Purple, Orange }

    public enum CrystalType { Red, Blue, Green, Yellow, Purple, Orange }

    public enum CrystalSubtype {
        Red1, Red2, Red3,
        Blue1, Blue2, Blue3,
        Green1, Green2, Green3,
        Yellow1, Yellow2, Yellow3,
        Purple1, Purple2, Purple3,
        Orange1, Orange2, Orange3
    }

    public HexState state = HexState.Empty;
    public CrystalType? crystalType = null;
    public CrystalSubtype? crystalSubtype = null;
    public CrystalType? influencedByType = null;

    public int gridX, gridY;
    public List<HexBehavior> neighbors = new();

    private HexRenderer hexRenderer;

    private int ticksRemaining = -1;
    private bool isProgressing = false;

    public Dictionary<CrystalType, int> influenceMap = new();
    public int influenceAmount = 0;

    public int influenceThreshold = 2;

    [Header("Crystal Growth")]
    public float growthMultiplier = 1f; // Default, adjusted by terrain

    public static Dictionary<CrystalType, PlayerType> crystalToPlayer = new()
    {
        { CrystalType.Red, PlayerType.Red },
        { CrystalType.Blue, PlayerType.Blue },
        { CrystalType.Green, PlayerType.Green },
        { CrystalType.Yellow, PlayerType.Yellow },
        { CrystalType.Purple, PlayerType.Purple },
        { CrystalType.Orange, PlayerType.Orange }
    };

    private void Awake()
    {
        hexRenderer = GetComponent<HexRenderer>();
    }

    private void Start()
    {
        var tickManager = Object.FindFirstObjectByType<TickManager>();
        tickManager?.Register(this);
    }

    private void OnMouseDown()
    {
        if (state == HexState.Empty || state == HexState.Influenced)
        {
            crystalType = CrystalSelectorUI.Instance?.selectedCrystal ?? CrystalType.Red;
            state = HexState.Seeded;
            hexRenderer.SetColor(GetColorForCrystal(crystalType.Value));

            var terrainGen = GameObject.FindFirstObjectByType<TerrainGenerator>();
            terrainGen?.TryExpandFrom(new Vector2Int(gridX, gridY));
        }
    }

    public void OnTick()
    {
        EvaluateInfluence();

        if (isProgressing)
        {
            ticksRemaining--;
            if (ticksRemaining <= 0)
                AdvanceState();
            return;
        }

        if ((state == HexState.Empty || state == HexState.Influenced) && influencedByType != null)
        {
            if (influenceAmount >= influenceThreshold)
            {
                if (state == HexState.Empty)
                {
                    state = HexState.Influenced;
                    hexRenderer.SetColor(Color.Lerp(Color.gray, Color.white, 0.3f));
                    isProgressing = true;
                    ticksRemaining = Mathf.Clamp(Mathf.RoundToInt((10f - influenceAmount * 2f) / growthMultiplier), 1, 6);
                    return;
                }

                if (state == HexState.Influenced)
                {
                    state = HexState.Growing;
                    crystalType = influencedByType;
                    hexRenderer.SetColor(GetColorForCrystal(crystalType.Value));
                    isProgressing = true;
                    ticksRemaining = Mathf.Clamp(Mathf.RoundToInt((10f - influenceAmount * 2f) / growthMultiplier), 1, 5);
                    return;
                }
            }
        }

        if (state == HexState.Seeded)
        {
            isProgressing = true;
            ticksRemaining = Mathf.Clamp(Mathf.RoundToInt((8f - influenceAmount * 1f) / growthMultiplier), 2, 6);
        }
        else if (state == HexState.Growing)
        {
            isProgressing = true;
            ticksRemaining = Mathf.RoundToInt(3f / growthMultiplier);
        }

        if (state == HexState.Full && influencedByType != null && influencedByType != crystalType)
        {
            if (influenceMap[influencedByType.Value] >= influenceThreshold + 1)
            {
                state = HexState.Growing;
                crystalType = influencedByType;
                hexRenderer.SetColor(GetColorForCrystal(crystalType.Value));
                isProgressing = true;
                ticksRemaining = Mathf.Clamp(Mathf.RoundToInt((8f - influenceAmount * 1f) / growthMultiplier), 1, 5);
            }
        }

        if (state == HexState.Full)
        {
            var terrainGen = GameObject.FindFirstObjectByType<TerrainGenerator>();
            terrainGen?.TryExpandFrom(new Vector2Int(gridX, gridY));
        }
    }

    public void AdvanceState()
    {
        isProgressing = false;

        switch (state)
        {
            case HexState.Seeded:
                state = HexState.Growing;
                hexRenderer.SetColor(GetColorForCrystal(crystalType ?? CrystalType.Red));
                break;

            case HexState.Growing:
                state = HexState.Full;
                hexRenderer.SetColor(GetColorForCrystal(crystalType ?? CrystalType.Red));
                break;
        }
    }

    public void EvaluateInfluence()
    {
        influenceMap.Clear();

        foreach (var neighbor in neighbors)
        {
            if (neighbor.crystalType != null && neighbor.state == HexState.Full)
            {
                var type = neighbor.crystalType.Value;
                if (!influenceMap.ContainsKey(type))
                    influenceMap[type] = 0;

                influenceMap[type]++;
            }
        }

       if (influenceMap.Count == 0)
        {
            influencedByType = null;
            influenceAmount = 0;
            return;
        }

        var sorted = influenceMap.OrderByDescending(kvp => kvp.Value).ToList();
        var top = sorted[0];

        if (sorted.Count > 1 && top.Value == sorted[1].Value)
        {
            influencedByType = null;
            influenceAmount = 0;
        }
        else
        {
            influencedByType = top.Key;
            influenceAmount = top.Value;

            // Mostrar color tenue si aún no llega al umbral
            if (state == HexState.Empty && influenceAmount > 0 && influenceAmount < influenceThreshold)
            {
                var baseColor = GetColorForCrystal(influencedByType.Value);
                hexRenderer.SetColor(Color.Lerp(baseColor, Color.white, 0.7f)); // Color desaturado
            }
        }


    }

    public static Color GetColorForCrystal(CrystalType type)
    {
        return type switch
        {
            CrystalType.Red => Color.red,
            CrystalType.Blue => Color.cyan,
            CrystalType.Green => Color.green,
            CrystalType.Yellow => new Color(1f, 1f, 0.2f),
            CrystalType.Purple => new Color(0.6f, 0f, 0.6f),
            CrystalType.Orange => new Color(1f, 0.5f, 0f),
            _ => Color.white
        };
    }
}
