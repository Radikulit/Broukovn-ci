using System.Collections.Generic;
using UnityEngine;

public class PlateManager : MonoBehaviour
{
    public static PlateManager Instance { get; private set; }

    public List<Plate> allPlates = new List<Plate>();
    public List<Dot> allDots = new List<Dot>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Небольшая задержка гарантирует, что все Awake у объектов прошли
        Invoke(nameof(SetDefaultGrid), 0.02f);
    }

    public void SetDefaultGrid()
    {
        SwitchGridMode(1);
    }

    public void SwitchGridMode(int unitSize)
    {
        bool isLarge = (unitSize == 2);

        foreach (var plate in allPlates)
        {
            if (plate != null) plate.SetActiveState(!isLarge);
        }

        foreach (var dot in allDots)
        {
            if (dot != null) dot.SetActiveState(isLarge);
        }
    }
    public Plate GetNearestEmptyPlate(Vector3 position)
    {
        Plate nearest = null;
        float minDistance = float.MaxValue;

        foreach (var plate in allPlates)
        {
            if (plate != null && plate.currentUnit == null)
            {
                float dist = Vector3.Distance(position, plate.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = plate;
                }
            }
        }
        return nearest;
    }
    public Dot GetNearestEmptyDot(Vector3 position)
    {
        Dot nearest = null;
        float minDistance = float.MaxValue;

        foreach (var dot in allDots)
        {
            if (dot != null && dot.currentUnit == null)
            {
                float dist = Vector3.Distance(position, dot.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = dot;
                }
            }
        }
        return nearest;
    }
}