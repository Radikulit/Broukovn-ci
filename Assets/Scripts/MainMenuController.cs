using System.Collections;
using UnityEngine;
public class MainMenuController : MonoBehaviour
{
    [Header("Ссылки на Canvas")]
    [Tooltip("Канвас главного меню (который показывается в начале)")]
    public GameObject startMenuCanvas;

    [Tooltip("Канвас игрового интерфейса (который изначально скрыт)")]
    public GameObject gameCanvas;

    [Header("Анимация")]
    [Tooltip("Animator камеры, на которой настроен отъезд назад")]
    public Animator cameraAnimator;

    private void Start()
    {
        // На старте игры гарантируем правильное состояние UI
        if (startMenuCanvas != null) startMenuCanvas.SetActive(true);
        if (gameCanvas != null) gameCanvas.SetActive(false);
    }

    // Этот метод вешаем на кнопку "Играть"
    public void OnPlayButtonClicked()
    {
        // 1. Запускаем триггер анимации отъезда камеры
        if (cameraAnimator != null)
        {
            cameraAnimator.SetTrigger("startGame");
        }

        // 2. Сразу скрываем начальное меню (или можете запустить корутину, если хотите скрыть его позже)
        if (startMenuCanvas != null)
        {
            startMenuCanvas.SetActive(false);
        }

        // 3. Запускаем таймер на 1.5 секунды, чтобы показать игровой канвас
        StartCoroutine(ShowGameCanvasWithDelay(1.5f));
    }

    private IEnumerator ShowGameCanvasWithDelay(float delay)
    {
        // Ждем ровно полторы секунды
        yield return new WaitForSeconds(delay);

        // Включаем второй канвас
        if (gameCanvas != null)
        {
            gameCanvas.SetActive(true);
        }
    }

    // Метод для кнопки выхода (оставляем на всякий случай)
    public void OnQuitButtonClicked()
    {
        Debug.Log("Выход из игры...");
        Application.Quit();
    }
}
