
using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Данные и ссылки")]
    public UnitData unitData;
    public float moveSpeed = 5f;
    public Plate currentPlate;
    public Transform AttackRange;
    public Transform movementRange; 

    public static Unit SelectedUnit { get; private set; }

    private bool isMoving;
    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (movementRange != null && unitData != null)
        {
            float size = (unitData.speed * 2f) + 1f;
            movementRange.localScale = new Vector3(size, 1f, size);
        }

        if (AttackRange != null && unitData != null)
        {
            float size = (unitData.range * 2f) + 1f;
            AttackRange.localScale = new Vector3(size, 1f, size);
        }

        if (currentPlate != null)
        {
            currentPlate.currentUnit = this;
            transform.position = new Vector3(currentPlate.transform.position.x, transform.position.y, currentPlate.transform.position.z);
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

        if (SelectedUnit != null) SelectedUnit.Deselect();

        SelectedUnit = this;
        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    public void MoveToPlate(Plate targetPlate)
    {
        if (isMoving || targetPlate == null) return;

        // Проверка: находится ли клетка внутри MovementRange
        if (movementRange != null)
        {
            Collider rangeCollider = movementRange.GetComponent<Collider>();
            Vector3 pos = targetPlate.transform.position;

            if (!rangeCollider.bounds.Contains(new Vector3(pos.x, rangeCollider.bounds.center.y, pos.z)))
            {
                Debug.Log("Плитка вне радиуса хода!");
                return;
            }
        }

        Deselect();
        StartCoroutine(MoveRoutine(targetPlate));
    }

    private IEnumerator MoveRoutine(Plate targetPlate)
    {
        isMoving = true;
        if (currentPlate != null) currentPlate.currentUnit = null;

        Vector3 targetPos = new Vector3(targetPlate.transform.position.x, transform.position.y, targetPlate.transform.position.z);

        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
        currentPlate = targetPlate;
        currentPlate.currentUnit = this;
        isMoving = false;
    }

    public void Deselect()
    {
        if (SelectedUnit == this) SelectedUnit = null;
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }
    }

    private IEnumerator BlinkRoutine()
    {
        if (spriteRenderer == null) yield break;

        while (true)
        {
            for (float a = 1f; a >= 0.75f; a -= Time.deltaTime * 1f)
            {
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, a);
                yield return null;
            }

            for (float a = 0.65f; a <= 1f; a += Time.deltaTime * 1f)
            {
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, a);
                yield return null;
            }
        }
    }
}
