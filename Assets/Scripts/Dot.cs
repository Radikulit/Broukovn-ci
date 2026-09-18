using UnityEngine;

public class Dot : MonoBehaviour
{
    public Unit currentUnit;
    public Material hoverMaterial; // Перетащите сюда материал подсветки в Инспекторе

    private Collider dotCollider;
    private MeshRenderer meshRenderer;
    private Material defaultMaterial;

    private void Awake()
    {
        dotCollider = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) defaultMaterial = meshRenderer.material;
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
        // Подсвечиваем только если точка активна
        if (dotCollider != null && !dotCollider.enabled) return;
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