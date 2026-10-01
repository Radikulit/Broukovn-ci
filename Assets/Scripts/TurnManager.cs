using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("UI Настройки")]
    public Transform queueContainer;
    public GameObject unitIconPrefab;
    public int maxVisibleIcons = 6;

    [Header("События конца хода")]
    [Tooltip("Событие, вызываемое при завершении или пропуске хода")]
    public UnityEvent onTurnEnded;

    private List<Unit> masterTurnOrder = new List<Unit>();
    private Queue<Unit> currentRoundQueue = new Queue<Unit>();

    public Unit CurrentUnit { get; private set; }

    private void Awake() => Instance = this;

    private void Start() => Invoke(nameof(InitializeBattle), 0.1f);

    public void InitializeBattle()
    {
        masterTurnOrder = FindObjectsOfType<Unit>()
            .Where(u => u != null)
            .OrderByDescending(u => Random.Range(1, 7) + (u.unitData ? u.unitData.speed : 0))
            .ToList();

        StartNewRound();
    }

    private void StartNewRound()
    {
        currentRoundQueue.Clear();
        foreach (var unit in masterTurnOrder)
        {
            if (unit != null) currentRoundQueue.Enqueue(unit);
        }

        NextTurn();
    }

    public void NextTurn()
    {
        // Вызываем событие конца хода (если кто-то уже ходил)
        if (CurrentUnit != null)
        {
            onTurnEnded?.Invoke();
        }

        CurrentUnit?.Deselect();

        if (currentRoundQueue.Count == 0)
        {
            StartNewRound();
            return;
        }

        CurrentUnit = currentRoundQueue.Dequeue();
        CurrentUnit.SelectUnit();

        UpdateTurnUI();
    }

    public void PassTurn()
    {
        if (CurrentUnit != null && CurrentUnit.isMoving) return;

        Debug.Log($"Юнит {CurrentUnit?.name} пропустил ход.");

        // Переход к следующему ходу автоматически заставит сработать событие onTurnEnded!
        NextTurn();
    }

    private void UpdateTurnUI()
    {
        if (!queueContainer || !unitIconPrefab || masterTurnOrder.Count == 0) return;

        foreach (Transform child in queueContainer)
        {
            Destroy(child.gameObject);
        }

        List<Unit> futureOrder = new List<Unit> { CurrentUnit };
        futureOrder.AddRange(currentRoundQueue);

        while (futureOrder.Count < maxVisibleIcons)
        {
            foreach (var unit in masterTurnOrder)
            {
                if (unit != null)
                {
                    futureOrder.Add(unit);
                    if (futureOrder.Count >= maxVisibleIcons) break;
                }
            }
        }

        for (int i = 0; i < futureOrder.Count; i++)
        {
            CreateIcon(futureOrder[i], i == 0);
        }
    }

    private void CreateIcon(Unit unit, bool isCurrent)
    {
        if (unit == null || unit.unitData == null || unit.unitData.unitIcon == null) return;

        GameObject iconObj = Instantiate(unitIconPrefab, queueContainer);
        Image img = iconObj.GetComponent<Image>();

        if (img != null)
        {
            img.sprite = unit.unitData.unitIcon;
            img.preserveAspect = true;
            img.color = isCurrent ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.75f);
        }

        if (isCurrent)
        {
            iconObj.transform.localScale = Vector3.one * 1.15f;
        }
    }
}