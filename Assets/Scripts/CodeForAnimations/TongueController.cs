using UnityEngine;
using System.Collections;

public class TongueController : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private RectTransform tongueRect;

    [Header("Настройки Размера")]
    [Tooltip("Базовая длина языка от рта до низа первой карточки")]
    [SerializeField] private float baseHeight = 220f;

    [Tooltip("Сколько длины добавляет КАЖДЫЙ ДОПОЛНИТЕЛЬНЫЙ листок")]
    [SerializeField] private float heightPerExtraLeaf = 130f;

    [Tooltip("Отступ под кончик языка")]
    [SerializeField] private float bottomPadding = 60f;

    [Header("Плавность")]
    [SerializeField] private float lerpSpeed = 8f;

    private float targetHeight;

    private void Reset()
    {
        tongueRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (tongueRect == null) return;

        // Плавно сжимаем/растягиваем язык к целевой высоте
        if (Mathf.Abs(tongueRect.sizeDelta.y - targetHeight) > 0.5f)
        {
            Vector2 size = tongueRect.sizeDelta;
            size.y = Mathf.Lerp(size.y, targetHeight, Time.deltaTime * lerpSpeed);
            tongueRect.sizeDelta = size;
        }
    }

    /// <summary>
    /// Вызывается напрямую из TurnManager с передачей точного количества активных листков
    /// </summary>
    public void UpdateTongueSize(int activeLeavesCount)
    {
        if (activeLeavesCount <= 0)
        {
            targetHeight = 0f;
            return;
        }

        // Вычисляем целевую высоту без задержек и поиска дочерних объектов
        targetHeight = baseHeight + ((activeLeavesCount - 1) * heightPerExtraLeaf) + bottomPadding;
    }
}
