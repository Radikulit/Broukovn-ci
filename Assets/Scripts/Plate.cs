using UnityEngine;

public class Plate : MonoBehaviour
{
    [Header("Настройки материалов")]
    public Material defaultMaterial;   
    public Material hoverMaterial;     

    [Header("Данные клетки")]
    public int tileIndex;              
    public bool isOccupied = false;   

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
    }

    private void OnMouseExit()
    {
        if (defaultMaterial != null && tileRenderer != null)
        {
            tileRenderer.material = defaultMaterial;
        }
    }
}
