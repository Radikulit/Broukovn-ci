using System;
using System.Collections;
using UnityEngine;

public class Plate : MonoBehaviour
{
    [Header("Настройки материалов")]
    public Material defaultMaterial;   // Обычный материал
    public Material hoverMaterial;     // Материал при наведении

    [Header("Данные клетки")]
    public int tileIndex;              // Порядковый номер клетки по кругу (от 0 до 51)
    public bool isOccupied = false;    // Занята ли клетка персонажем
    public Unit currentUnit;           // Юнит, который сейчас стоит на этой плитке

    private Renderer tileRenderer;

    void Awake()
    {
        tileRenderer = GetComponent<Renderer>();
        if (defaultMaterial == null && tileRenderer != null)
        {
            defaultMaterial = tileRenderer.material;
        }
    }

    private void OnMouseEnter()
    {
        if (hoverMaterial != null && tileRenderer != null)
        {
            tileRenderer.material = hoverMaterial;
        }
        PlateManager.Instance?.SetHoveredPlate(this);
    }

    private void OnMouseExit()
    {
        if (defaultMaterial != null && tileRenderer != null)
        {
            tileRenderer.material = defaultMaterial;
        }
        if (PlateManager.Instance?.HoveredPlate == this)
        {
            PlateManager.Instance.SetHoveredPlate(null);
        }
    }

    private void OnMouseDown()
    {
        if (Unit.SelectedUnit != null)
        {
            Unit.SelectedUnit.MoveToPlate(this);
        }
    }
}
