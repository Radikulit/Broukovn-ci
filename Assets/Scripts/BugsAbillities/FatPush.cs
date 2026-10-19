using System.Collections;
using System.Collections.Generic;
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

    public void PushUnitsInArea(Vector3 moveDirection)
    {
        if (selfUnit == null || selfUnit.attackRange == null || PlateManager.Instance == null) return;

        // Если вектор направления равен нулю, задаем дефолт
        if (moveDirection == Vector3.zero) moveDirection = Vector3.right;

        // 1. СТРОГО БРОНИРУЕМ 4 плитки под Толстяком прямо сейчас!
        if (selfUnit.currentDot != null)
        {
            selfUnit.currentDot.SetUnit(selfUnit);
        }

        Collider attackCol = selfUnit.attackRange.GetComponent<Collider>();
        if (attackCol == null) return;

        Unit[] allUnits = FindObjectsOfType<Unit>();
        List<Unit> unitsToPush = new List<Unit>();

        // 2. Находим всех жертв толчка
        foreach (var otherUnit in allUnits)
        {
            if (otherUnit == selfUnit || otherUnit.isMoving) continue;

            Vector3 otherPos = otherUnit.transform.position;
            if (attackCol.bounds.Contains(new Vector3(otherPos.x, attackCol.bounds.center.y, otherPos.z)))
            {
                unitsToPush.Add(otherUnit);
            }
        }

        // 3. Сбрасываем привязку плиток у расталкиваемых жуков
        foreach (var unit in unitsToPush)
        {
            if (unit.unitData != null && unit.unitData.unitSize == 1 && unit.currentPlate != null)
            {
                unit.currentPlate.currentUnit = null;
            }
            else if (unit.unitData != null && unit.unitData.unitSize == 2 && unit.currentDot != null)
            {
                unit.currentDot.ClearUnit();
            }
        }

        // 4. Распределяем жуков по свободным клеткам
        foreach (var unit in unitsToPush)
        {
            PushSingleUnit(unit, moveDirection);
        }
    }

    private void PushSingleUnit(Unit otherUnit, Vector3 moveDirection)
    {
        Vector3 currentPos = otherUnit.transform.position;

        // 1. Округляем вектор направления до чистого направления по осям (-1, 0, 1)
        Vector3 snapDirection = new Vector3(
            Mathf.Round(moveDirection.x),
            0f,
            Mathf.Round(moveDirection.z)
        ).normalized;

        // 2. Смещаем точку поиска СТРОГО на 0.7 от размера плитки (чтобы гарантированно попасть в СОСЕДНЮЮ клетку, а не через одну)
        float gridStep = 1.0f; // Укажите ваш точный размер плитки, если он отличается от 1.0
        Vector3 exactTargetPos = currentPos + snapDirection * (gridStep * 0.7f);

        // Маленький жук (unitSize == 1)
        if (otherUnit.unitData != null && otherUnit.unitData.unitSize == 1)
        {
            Plate targetPlate = GetClosestPlateToPoint(exactTargetPos);

            if (targetPlate != null)
            {
                otherUnit.currentPlate = targetPlate;
                targetPlate.currentUnit = otherUnit; // Сразу бронируем

                StartCoroutine(AnimatePushedUnit(otherUnit, targetPlate.transform.position));
            }
        }
        // Большой жук (unitSize == 2)
        else if (otherUnit.unitData != null && otherUnit.unitData.unitSize == 2)
        {
            Dot freeDot = PlateManager.Instance.GetNearestEmptyDot(exactTargetPos);

            if (freeDot != null)
            {
                otherUnit.currentDot = freeDot;
                freeDot.SetUnit(otherUnit);

                StartCoroutine(AnimatePushedUnit(otherUnit, freeDot.transform.position));
            }
        }
    }

    // Поиск САМОЙ близкой свободной плитки к точке смещения
    private Plate GetClosestPlateToPoint(Vector3 targetWorldPos)
    {
        Plate[] allPlates = FindObjectsOfType<Plate>();
        Plate closestPlate = null;
        float minDistance = float.MaxValue;

        foreach (var plate in allPlates)
        {
            // Пропускаем плитки, которые уже заняты
            if (plate.currentUnit != null) continue;

            // Исключаем плитки под самим Толстяком
            if (selfUnit.currentDot != null)
            {
                float distToPusher = Vector3.Distance(selfUnit.transform.position, plate.transform.position);
                if (distToPusher < 0.9f) continue;
            }

            float dist = Vector3.Distance(targetWorldPos, plate.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestPlate = plate;
            }
        }

        return closestPlate;
    }

    private IEnumerator AnimatePushedUnit(Unit pushedUnit, Vector3 targetWorldPos)
    {
        pushedUnit.isMoving = true;
        Vector3 destination = new Vector3(targetWorldPos.x, pushedUnit.transform.position.y, targetWorldPos.z);

        SpriteRenderer pushedSprite = pushedUnit.GetComponentInChildren<SpriteRenderer>();

        if (pushedSprite != null && Mathf.Abs(destination.x - pushedUnit.transform.position.x) > 0.01f)
        {
            pushedSprite.flipX = destination.x > pushedUnit.transform.position.x;
        }

        if (pushedUnit.animator != null)
        {
            pushedUnit.animator.ResetTrigger("move");
            pushedUnit.animator.SetTrigger("pushed");
        }

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

        if (pushedUnit.animator != null)
        {
            pushedUnit.animator.ResetTrigger("pushed");
        }

        pushedUnit.isMoving = false;
    }
}