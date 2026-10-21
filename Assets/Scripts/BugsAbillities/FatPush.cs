using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FatPush : MonoBehaviour
{
    [Header("Настройки толчка")]
    [SerializeField] private float pushSpeed = 15f;
    [SerializeField] private float tileSize = 1.0f; // Укажите реальный размер вашей плитки (расстояние между центрами)

    private Unit selfUnit;

    private void Awake()
    {
        selfUnit = GetComponent<Unit>();
    }

    public void PushUnitsInArea(Vector3 moveDirection)
    {
        if (selfUnit == null || PlateManager.Instance == null) return;

        // 1. Очищаем погрешности вектора направления (оставляем чистые -1, 0, 1)
        Vector3 cleanDir = NormalizeToGridDirection(moveDirection);
        if (cleanDir == Vector3.zero) cleanDir = Vector3.right;

        // 2. Гарантируем блокировку 4-х плиток под Толстяком
        if (selfUnit.currentDot != null)
        {
            selfUnit.currentDot.SetUnit(selfUnit);
        }

        // 3. Ищем коллайдер атак-рейнджа по тегу "pusher"
        Collider attackCol = GetPusherCollider();
        if (attackCol == null) return;

        Unit[] allUnits = FindObjectsOfType<Unit>();
        List<Unit> unitsToPush = new List<Unit>();

        // 4. Фильтруем юнитов, попавших под удар
        foreach (var otherUnit in allUnits)
        {
            if (otherUnit == selfUnit || otherUnit.isMoving) continue;

            Vector3 otherPos = otherUnit.transform.position;
            if (attackCol.bounds.Contains(new Vector3(otherPos.x, attackCol.bounds.center.y, otherPos.z)))
            {
                unitsToPush.Add(otherUnit);
            }
        }

        // 5. Освобождаем текущие плитки расталкиваемых жуков
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

        // 6. Толкаем каждого жука ровно на 1 плитку вперед от ЕГО ТЕКУЩЕЙ ПЛИТКИ
        foreach (var unit in unitsToPush)
        {
            PushSingleUnit(unit, cleanDir);
        }
    }

    private void PushSingleUnit(Unit otherUnit, Vector3 cleanDir)
    {
        // Маленький жук (unitSize == 1)
        if (otherUnit.unitData != null && otherUnit.unitData.unitSize == 1)
        {
            // Берем за основу позицию ТЕКУЩЕЙ ПЛИТКИ жука (а не координаты самого жука)
            Vector3 originTilePos = otherUnit.currentPlate != null
                ? otherUnit.currentPlate.transform.position
                : otherUnit.transform.position;

            // Вычисляем точку ровно на 1 шаг плитки
            Vector3 targetTilePos = originTilePos + (cleanDir * tileSize);

            // Ищем плитку ровно в этой точке
            Plate freePlate = FindExactPlateAt(targetTilePos);

            // Если целевая плитка почему-то занята или отсутствует — ищем ближайшую свободную
            if (freePlate == null || freePlate.currentUnit != null)
            {
                freePlate = PlateManager.Instance.GetNearestEmptyPlate(targetTilePos);
            }

            if (freePlate != null)
            {
                otherUnit.currentPlate = freePlate;
                freePlate.currentUnit = otherUnit; // Бронируем плитку

                StartCoroutine(AnimatePushedUnit(otherUnit, freePlate.transform.position));
            }
        }
        // Большой жук (unitSize == 2)
        else if (otherUnit.unitData != null && otherUnit.unitData.unitSize == 2)
        {
            Vector3 originDotPos = otherUnit.currentDot != null
                ? otherUnit.currentDot.transform.position
                : otherUnit.transform.position;

            Vector3 targetDotPos = originDotPos + (cleanDir * tileSize);
            Dot freeDot = PlateManager.Instance.GetNearestEmptyDot(targetDotPos);

            if (freeDot != null)
            {
                otherUnit.currentDot = freeDot;
                freeDot.SetUnit(otherUnit);

                StartCoroutine(AnimatePushedUnit(otherUnit, freeDot.transform.position));
            }
        }
    }

    // Ищем коллайдер объекта AttackRange с тегом "pusher"
    private Collider GetPusherCollider()
    {
        if (selfUnit.attackRange != null && selfUnit.attackRange.CompareTag("pusher"))
        {
            return selfUnit.attackRange.GetComponent<Collider>();
        }

        // Если компонент висит на дочернем объекте
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            if (col.CompareTag("pusher")) return col;
        }

        return null;
    }

    // Находим плитку строго под конкретной точкой
    private Plate FindExactPlateAt(Vector3 targetPos)
    {
        Plate[] allPlates = FindObjectsOfType<Plate>();
        Plate bestPlate = null;
        float minDistance = float.MaxValue;

        foreach (var plate in allPlates)
        {
            if (plate.currentUnit != null) continue;

            // Исключаем плитки под брюхом Толстяка
            if (selfUnit.currentDot != null && Vector3.Distance(selfUnit.transform.position, plate.transform.position) < (tileSize * 0.8f))
            {
                continue;
            }

            float dist = Vector3.Distance(targetPos, plate.transform.position);
            // Плитка должна быть совсем близко к расчетной точке (в пределах пол-плитки)
            if (dist < (tileSize * 0.5f) && dist < minDistance)
            {
                minDistance = dist;
                bestPlate = plate;
            }
        }

        return bestPlate;
    }

    private Vector3 NormalizeToGridDirection(Vector3 dir)
    {
        float x = Mathf.Abs(dir.x) > 0.3f ? Mathf.Sign(dir.x) : 0f;
        float z = Mathf.Abs(dir.z) > 0.3f ? Mathf.Sign(dir.z) : 0f;
        return new Vector3(x, 0f, z).normalized;
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