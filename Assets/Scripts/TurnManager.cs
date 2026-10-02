using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events; // Обязательно добавляем эту директиву!
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("UI Настройки")]
    public Transform queueContainer;
    public GameObject unitIconPrefab;
    public int maxVisibleIcons = 6;
    public TongueController tongueController;

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
        if (CurrentUnit != null)
        {
            onTurnEnded?.Invoke();
        }

        CurrentUnit?.Deselect();

        // Если все сходили — начинаем новый раунд и сбрасываем очередь
        if (currentRoundQueue.Count == 0)
        {
            StartNewRound();
            return;
        }

        CurrentUnit = currentRoundQueue.Dequeue();
        CurrentUnit.SelectUnit();

        // Перерисовываем UI (количество листиков уменьшится)
        UpdateTurnUI();
    }

    private void UpdateTurnUI()
    {
        if (!queueContainer || !unitIconPrefab) return;

        // 1. Очищаем старые листки
        foreach (Transform child in queueContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Собираем только тех, кто еще ходит в этом раунде
        List<Unit> remainingInThisRound = new List<Unit>();
        if (CurrentUnit != null) remainingInThisRound.Add(CurrentUnit);
        remainingInThisRound.AddRange(currentRoundQueue);

        // 3. Спавним листки
        foreach (var unit in remainingInThisRound)
        {
            CreateIcon(unit, unit == CurrentUnit);
        }

        // 4. Обновляем размер языка по оси Y
        if (tongueController != null)
        {
            tongueController.UpdateTongueSize(remainingInThisRound.Count);
        }
    }

    public void PassTurn()
    {
        if (CurrentUnit != null && CurrentUnit.isMoving) return;
        GameObject currentButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        Animator btnAnimator = currentButton.GetComponent<Animator>();
        btnAnimator.SetTrigger("flip");
        NextTurn();
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