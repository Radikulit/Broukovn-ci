using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("UI Настройки")]
    public Transform queueContainer;
    public GameObject unitIconPrefab;
    [Tooltip("Сколько иконок отображать в шкале одновременно")]
    public int maxVisibleIcons = 6;

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

    private void UpdateTurnUI()
    {
        if (!queueContainer || !unitIconPrefab || masterTurnOrder.Count == 0) return;

        // Очищаем старые элементы UI
        foreach (Transform child in queueContainer)
        {
            Destroy(child.gameObject);
        }

        // Формируем полный список будущего порядка ходов
        List<Unit> futureOrder = new List<Unit>();

        // 1. Текущий ходящий юнит
        futureOrder.Add(CurrentUnit);

        // 2. Оставшиеся юниты в текущем раунде
        futureOrder.AddRange(currentRoundQueue);

        // 3. Дозаполняем список юнитами из следующих раундов до достижения maxVisibleIcons
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

        // Отрисовываем иконки
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
            // Активный юнит яркий, остальные чуть затемнены
            img.color = isCurrent ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.75f);
        }

        if (isCurrent)
        {
            iconObj.transform.localScale = Vector3.one * 1.15f;
        }
    }
}