using System.Collections;
using UnityEngine;

public class FatPush : MonoBehaviour
{
    [Header("Настройки толчка")]
    [SerializeField] private float pushSpeed = 15f;

    private Unit selfUnit;

    private void Awake()
    {
        selfUnit = GetComponent<Unit>();
    }

    public void PushUnitsInArea()
    {
        if (selfUnit == null || selfUnit.attackRange == null || PlateManager.Instance == null) return;

        Collider attackCol = selfUnit.attackRange.GetComponent<Collider>();
        if (attackCol == null) return;

        Unit[] allUnits = FindObjectsOfType<Unit>();

        foreach (var otherUnit in allUnits)
        {
            if (otherUnit == selfUnit || otherUnit.isMoving) continue;

            Vector3 otherPos = otherUnit.transform.position;
            if (attackCol.bounds.Contains(new Vector3(otherPos.x, attackCol.bounds.center.y, otherPos.z)))
            {
                PushSingleUnit(otherUnit);
            }
        }
    }

    private void PushSingleUnit(Unit otherUnit)
    {
        Vector3 otherPos = otherUnit.transform.position;

        // Маленький жук (unitSize == 1) -> Сдвигаем на ближайшую свободную Plate
        if (otherUnit.unitData != null && otherUnit.unitData.unitSize == 1)
        {
            Plate freePlate = PlateManager.Instance.GetNearestEmptyPlate(otherPos);
            if (freePlate != null)
            {
                if (otherUnit.currentPlate != null) otherUnit.currentPlate.currentUnit = null;

                otherUnit.currentPlate = freePlate;
                freePlate.currentUnit = otherUnit;

                StartCoroutine(AnimatePushedUnit(otherUnit, freePlate.transform.position));
            }
        }
        // Большой жук (unitSize == 2) -> Сдвигаем на ближайший свободный Dot
        else if (otherUnit.unitData != null && otherUnit.unitData.unitSize == 2)
        {
            Dot freeDot = PlateManager.Instance.GetNearestEmptyDot(otherPos);
            if (freeDot != null)
            {
                if (otherUnit.currentDot != null) otherUnit.currentDot.currentUnit = null;

                otherUnit.currentDot = freeDot;
                freeDot.currentUnit = otherUnit;

                StartCoroutine(AnimatePushedUnit(otherUnit, freeDot.transform.position));
            }
        }
    }

    private IEnumerator AnimatePushedUnit(Unit pushedUnit, Vector3 targetWorldPos)
    {
        pushedUnit.isMoving = true;
        Vector3 destination = new Vector3(targetWorldPos.x, pushedUnit.transform.position.y, targetWorldPos.z);

        // Ищем SpriteRenderer в том числе на дочерних объектах
        SpriteRenderer pushedSprite = pushedUnit.GetComponentInChildren<SpriteRenderer>();

        if (pushedSprite != null && Mathf.Abs(destination.x - pushedUnit.transform.position.x) > 0.01f)
        {
            // Если спрайт смотрит ВЛЕВО по умолчанию:
            pushedSprite.flipX = destination.x > pushedUnit.transform.position.x;

            // ЕСЛИ СПРАЙТ ПО УМОЛЧАНИЮ СМОТРИТ ВПРАВО, раскомментируйте строчку ниже вместо верхней:
            // pushedSprite.flipX = destination.x < pushedUnit.transform.position.x;
        }

        // Запуск триггера анимации
        if (pushedUnit.animator != null)
        {
            pushedUnit.animator.ResetTrigger("move");
            pushedUnit.animator.SetTrigger("pushed");
        }

        // Движение к целевой точке
        while (Vector3.Distance(pushedUnit.transform.position, destination) > 0.05f)
        {
            pushedUnit.transform.position = Vector3.MoveTowards(
                pushedUnit.transform.position,
                destination,
                pushSpeed * Time.deltaTime
            );
            yield return null;
        }

        pushedUnit.transform.position = destination;

        // Сброс триггера (flipX НЕ сбрасываем, чтобы жук остался развёрнутым в сторону движения)
        if (pushedUnit.animator != null)
        {
            pushedUnit.animator.ResetTrigger("pushed");
        }

        pushedUnit.isMoving = false;
    }
}