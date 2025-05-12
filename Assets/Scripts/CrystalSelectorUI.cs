using UnityEngine;

public class CrystalSelectorUI : MonoBehaviour
{
    public static CrystalSelectorUI Instance;

    public HexBehavior.CrystalType selectedCrystal = HexBehavior.CrystalType.Red;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SelectRedCrystal() => selectedCrystal = HexBehavior.CrystalType.Red;
    public void SelectBlueCrystal() => selectedCrystal = HexBehavior.CrystalType.Blue;
    public void SelectGreenCrystal() => selectedCrystal = HexBehavior.CrystalType.Green;
}
