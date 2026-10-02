using UnityEngine;

public class Plate : MonoBehaviour
{
    public Unit currentUnit;
    public Material hoverMaterial; // Перетащите сюда материал подсветки в Инспекторе

    private Collider plateCollider;
    private MeshRenderer meshRenderer;
    private Material defaultMaterial;

    private void Awake()
    {
        plateCollider = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) defaultMaterial = meshRenderer.material;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Пытаемся найти Unit на зашедшем объекте или его родителях
        Unit enteringUnit = other.GetComponentInParent<Unit>();
        if (enteringUnit != null)
        {
            // Назначаем текущим юнитом этой плитки
            currentUnit = enteringUnit;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Unit exitingUnit = other.GetComponentInParent<Unit>();
        if (exitingUnit != null && currentUnit == exitingUnit)
        {
            // Освобождаем плитку при уходе юнита
            currentUnit = null;
        }
    }

    private void OnMouseDown()
    {
        if (Unit.SelectedUnit != null && Unit.SelectedUnit.unitData.unitSize == 1)
        {
            Unit.SelectedUnit.MoveToPlate(this);
        }
    }

    private void OnMouseEnter()
    {
        // Подсвечиваем только если плитка активна и не занята
        if (plateCollider != null && !plateCollider.enabled) return;
        if (hoverMaterial != null && meshRenderer != null)
        {
            meshRenderer.material = hoverMaterial;
        }
    }

    private void OnMouseExit()
    {
        // Возвращаем исходный материал при уходе курсора
        if (defaultMaterial != null && meshRenderer != null)
        {
            meshRenderer.material = defaultMaterial;
            // Восстанавливаем прозрачность для текущего состояния
            SetActiveState(plateCollider != null && plateCollider.enabled);
        }
    }

    public void SetActiveState(bool active)
    {
        if (plateCollider != null) plateCollider.enabled = active;

        if (meshRenderer != null && meshRenderer.material != null)
        {
            Color c = meshRenderer.material.color;
            float targetAlpha = active ? (145f / 255f) : (38f / 255f);
            meshRenderer.material.color = new Color(c.r, c.g, c.b, targetAlpha);
        }
    }
}