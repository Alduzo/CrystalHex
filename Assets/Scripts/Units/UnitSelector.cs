// 📁 UnitSelector.cs
using UnityEngine;
using System;

public class UnitSelector : MonoBehaviour
{
    public LayerMask terrainLayer; // Layer for terrain detection (e.g., "Terrain")
    public LayerMask unitLayer;    // Layer for unit detection (e.g., "Player" or a specific "Unit" layer)
    [SerializeField] private float hoverRaycastDistance = 100f;

    private GameObject selectedUnit;
    private HexBehavior lastHoveredHex; // To track the currently hovered hex

    // Public static events that other scripts can subscribe to
    public static event Action<UnitMover, bool> OnUnitSelected; // unit and isSelected (true/false)
    public static event Action<HexBehavior> OnUnitHovered;       // hex that is currently hovered (can be null)

    void Update()
    {
        HandleSelectionInput();
        HandleHoverDetection(); // Continuously check for hovered hex
    }

    private void HandleSelectionInput()
{
    if (Input.GetMouseButtonDown(0))
    {
        Debug.Log("🖱️ Clic izquierdo detectado.");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * hoverRaycastDistance, Color.yellow, 2f);

        if (Physics.Raycast(ray, out hit, hoverRaycastDistance, unitLayer))
        {
            Debug.Log($"🎯 Raycast impactó: {hit.collider.name} (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");

            // Intenta encontrar UnitMover en varias partes de la jerarquía
            UnitMover clickedUnit = hit.collider.GetComponent<UnitMover>()
                                   ?? hit.collider.GetComponentInChildren<UnitMover>()
                                   ?? hit.collider.GetComponentInParent<UnitMover>();

            if (clickedUnit != null)
            {
                Debug.Log($"✅ Unidad con UnitMover encontrada: {clickedUnit.gameObject.name}");
                SelectUnit(clickedUnit);
            }
            else
            {
                Debug.LogWarning("⚠️ Raycast impactó algo en unitLayer, pero no encontró UnitMover en la jerarquía.");
            }
        }
        else
        {
            Debug.Log("👀 Raycast no impactó ningún objeto en la capa de unidades.");
            DeselectUnit();
        }
    }

    if (Input.GetMouseButtonDown(1) && selectedUnit != null)
    {
        Debug.Log("🖱️ Clic derecho detectado. Intentando mover unidad seleccionada...");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, hoverRaycastDistance, terrainLayer))
        {
            HexBehavior hex = hit.collider.GetComponentInParent<HexBehavior>();
            if (hex != null)
            {
                UnitMover mover = selectedUnit.GetComponent<UnitMover>();
                if (mover != null)
                {
                    Debug.Log($"🏃 Moviendo unidad a Hex ({hex.coordinates.Q}, {hex.coordinates.R})");
                    mover.MoveTo(hex);
                }
                else
                {
                    Debug.LogWarning("⚠️ Unidad seleccionada no tiene UnitMover.");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ El objeto clickeado no tiene HexBehavior en su jerarquía.");
            }
        }
        else
        {
            Debug.Log("❌ Clic derecho no impactó el terreno.");
        }
    }
}


    /// <summary>
    /// Detects which hex the mouse is currently hovering over and invokes the OnUnitHovered event.
    /// </summary>
    private void HandleHoverDetection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        HexBehavior currentHovered = null;

        if (Camera.main != null && Physics.Raycast(ray, out hit, hoverRaycastDistance, terrainLayer))
        {
            currentHovered = hit.collider.GetComponentInParent<HexBehavior>();
        }

        // Only invoke the event if the hovered hex has changed
        if (currentHovered != lastHoveredHex)
        {
            lastHoveredHex = currentHovered;
            OnUnitHovered?.Invoke(lastHoveredHex); // Invoke the event, passing null if no hex is hovered
        }
    }

    private void SelectUnit(UnitMover unit)
    {
        if (selectedUnit != null && selectedUnit != unit.gameObject)
        {
            // Deselect previous unit if a different one is selected
            OnUnitSelected?.Invoke(selectedUnit.GetComponent<UnitMover>(), false);
        }

        selectedUnit = unit.gameObject;
        OnUnitSelected?.Invoke(unit, true); // Select new unit
        Debug.Log("✅ Unidad seleccionada: " + selectedUnit.name);
    }

    private void DeselectUnit()
    {
        if (selectedUnit != null)
        {
            OnUnitSelected?.Invoke(selectedUnit.GetComponent<UnitMover>(), false);
            selectedUnit = null;
            Debug.Log("❌ Unidad deseleccionada.");
        }
    }
}