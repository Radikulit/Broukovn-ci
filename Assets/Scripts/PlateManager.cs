using System.Collections.Generic;
using UnityEngine;

public class PlateManager : MonoBehaviour
{
    public static PlateManager Instance { get; private set; }

    [Header("Все плитки арены")]
    public List<Plate> allPlates = new List<Plate>();

    // Плитка под курсором
    public Plate HoveredPlate { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Если список не заполнен вручную в инспекторе, находим все плитки
        if (allPlates.Count == 0)
        {
            allPlates.AddRange(Object.FindObjectsByType<Plate>(FindObjectsSortMode.None));
        }

        // Сортируем плитки по tileIndex
        allPlates.Sort((a, b) => a.tileIndex.CompareTo(b.tileIndex));
    }

    public void SetHoveredPlate(Plate plate)
    {
        HoveredPlate = plate;
    }

    // Получить плитку по индексу (с учетом кольца)
    public Plate GetPlateByIndex(int index)
    {
        if (allPlates.Count == 0) return null;
        int normalizedIndex = (index % allPlates.Count + allPlates.Count) % allPlates.Count;
        return allPlates.Find(p => p != null && p.tileIndex == normalizedIndex);
    }
}
