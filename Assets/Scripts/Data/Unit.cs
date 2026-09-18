using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Данные и ссылки")]
    public Animator animator;
    public UnitData unitData;
    public float moveSpeed = 5f;
    public Plate currentPlate;
    public Dot currentDot; // Ссылка на текущую точку для толстяка (unitSize == 2)
    public Transform attackRange;
    public Transform movementRange;

    public static Unit SelectedUnit { get; private set; }

    private bool isMoving;
    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;

    private void Awake() => spriteRenderer = GetComponentInChildren<SpriteRenderer>();

    private void Start()
    {
        if (unitData != null)
        {
            SetRangeScale(movementRange, unitData.speed);
            SetRangeScale(attackRange, unitData.range);
        }

        // Спавн и привязка на старте в зависимости от размера
        if (unitData != null && unitData.unitSize == 2 && currentDot != null)
        {
            currentDot.currentUnit = this;
            transform.position = GetFlatPosition(currentDot.transform.position);
        }
        else if (currentPlate != null)
        {
            currentPlate.currentUnit = this;
            transform.position = GetFlatPosition(currentPlate.transform.position);
        }
    }

    private void OnMouseDown()
    {
        if (isMoving) return;

        if (SelectedUnit == this)
        {
            Deselect();
            return;
        }

        SelectedUnit?.Deselect();
        SelectedUnit = this;

        // Переключаем сетку под размер текущего юнита
        if (PlateManager.Instance != null && unitData != null)
        {
            PlateManager.Instance.SwitchGridMode(unitData.unitSize);
        }

        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    // Движение маленького юнита по Плиткам (unitSize = 1)
    public void MoveToPlate(Plate targetPlate)
    {
        if (isMoving || targetPlate == null || (targetPlate.currentUnit != null && targetPlate.currentUnit != this)) return;
        if (!IsInRange(targetPlate.transform.position)) return;

        Deselect();
        StartCoroutine(MoveRoutine(targetPlate.transform.position, () => {
            if (currentPlate != null) currentPlate.currentUnit = null;
            currentPlate = targetPlate;
            currentPlate.currentUnit = this;
        }));
    }

    // Движение большой туши по Точкам (unitSize = 2)
    public void MoveToDot(Dot targetDot)
    {
        if (isMoving || targetDot == null || (targetDot.currentUnit != null && targetDot.currentUnit != this)) return;
        if (!IsInRange(targetDot.transform.position)) return;

        Deselect();
        StartCoroutine(MoveRoutine(targetDot.transform.position, () => {
            if (currentDot != null) currentDot.currentUnit = null;
            currentDot = targetDot;
            currentDot.currentUnit = this;
        }));
    }

    private bool IsInRange(Vector3 targetPos)
    {
        if (movementRange == null) return true;
        Collider col = movementRange.GetComponent<Collider>();
        return col.bounds.Contains(new Vector3(targetPos.x, col.bounds.center.y, targetPos.z));
    }

    private IEnumerator MoveRoutine(Vector3 targetPos, System.Action onComplete)
    {
        isMoving = true;
        Vector3 destination = GetFlatPosition(targetPos);

        if (animator != null)
        {
            animator.SetTrigger("move");
        }

        while (Vector3.Distance(transform.position, destination) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = destination;

        // Сбрасываем триггер и принудительно возвращаем в IDLE
        if (animator != null)
        {
            animator.ResetTrigger("move");
            animator.Play("IDLE", 0, 0f); // Мгновенно переключает в IDLE с начала клипа
        }

        onComplete?.Invoke();
        isMoving = false;
    }

    private Vector3 GetFlatPosition(Vector3 targetPos) => new Vector3(targetPos.x, transform.position.y, targetPos.z);

    private void SetRangeScale(Transform rangeTransform, float statValue)
    {
        if (rangeTransform == null) return;
        float size = (statValue * 2f) + (unitData != null ? unitData.unitSize : 1);
        rangeTransform.localScale = new Vector3(size, 1f, size);
    }

    public void Deselect()
    {
        if (SelectedUnit == this) SelectedUnit = null;
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        SetAlpha(1f);

        // Возвращаем сетку в стандартный режим (для 1x1 плиток) при снятии выделения
        if (PlateManager.Instance != null)
        {
            PlateManager.Instance.SwitchGridMode(1);
        }
    }

    private IEnumerator BlinkRoutine()
    {
        if (spriteRenderer == null) yield break;
        while (true)
        {
            for (float a = 1f; a >= 0.75f; a -= Time.deltaTime) { SetAlpha(a); yield return null; }
            for (float a = 0.75f; a <= 1f; a += Time.deltaTime) { SetAlpha(a); yield return null; }
        }
    }

    private void SetAlpha(float a)
    {
        if (spriteRenderer == null) return;
        Color c = spriteRenderer.color;
        spriteRenderer.color = new Color(c.r, c.g, c.b, a);
    }
}