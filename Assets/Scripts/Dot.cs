using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Dot : MonoBehaviour
{
    public Unit currentUnit;
    public Material hoverMaterial;

    private Collider dotCollider;
    private MeshRenderer meshRenderer;
    private Material defaultMaterial;

    // Список 4-х плиток, перекрываемых этим Дотом
    private List<Plate> occupiedPlates = new List<Plate>();

    private void Awake()
    {
        dotCollider = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) defaultMaterial = meshRenderer.material;
    }

    /// <summary>
    /// Гарантированно находит 4 самые близкие плитки к центру Дота
    /// </summary>
    public void CacheAdjacentPlates()
    {
        if (occupiedPlates.Count == 4) return; // Ужe закэшированы

        Plate[] allPlates = FindObjectsOfType<Plate>();

        // Сортируем все плитки по расстоянию до центра Dot и берём ровно 4 первых
        occupiedPlates = allPlates
            .OrderBy(p => Vector3.Distance(transform.position, p.transform.position))
            .Take(4)
            .ToList();
    }

    /// <summary>
    /// Назначает юнита на Дот и блокирует 4 смежные плитки
    /// </summary>
    public void SetUnit(Unit newUnit)
    {
        CacheAdjacentPlates(); // Проверка кэша на случай, если Start ещё не отработал

        // Если кто-то уже стоял, сначала освобождаем плитки
        if (currentUnit != null && newUnit != currentUnit)
        {
            ClearUnit();
        }

        currentUnit = newUnit;

        if (currentUnit != null)
        {
            foreach (var plate in occupiedPlates)
            {
                if (plate != null)
                {
                    plate.currentUnit = currentUnit;
                }
            }
        }
    }

    /// <summary>
    /// Освобождает Дот и 4 смежные плитки
    /// </summary>
    public void ClearUnit()
    {
        CacheAdjacentPlates();

        if (currentUnit != null)
        {
            foreach (var plate in occupiedPlates)
            {
                if (plate != null && plate.currentUnit == currentUnit)
                {
                    plate.currentUnit = null;
                }
            }
        }
        currentUnit = null;
    }

    private void OnMouseDown()
    {
        if (Unit.SelectedUnit != null && Unit.SelectedUnit.unitData.unitSize == 2)
        {
            Unit.SelectedUnit.MoveToDot(this);
        }
    }

    private void OnMouseEnter()
    {
        if (dotCollider != null && !dotCollider.enabled) return;
        if (hoverMaterial != null && meshRenderer != null)
        {
            meshRenderer.material = hoverMaterial;
        }
    }

    private void OnMouseExit()
    {
        if (defaultMaterial != null && meshRenderer != null)
        {
            meshRenderer.material = defaultMaterial;
            SetActiveState(dotCollider != null && dotCollider.enabled);
        }
    }

    public void SetActiveState(bool active)
    {
        if (dotCollider != null) dotCollider.enabled = active;

        if (meshRenderer != null && meshRenderer.material != null)
        {
            Color c = meshRenderer.material.color;
            float targetAlpha = active ? (145f / 255f) : (38f / 255f);
            meshRenderer.material.color = new Color(c.r, c.g, c.b, targetAlpha);
        }
    }
}