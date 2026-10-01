using UnityEngine;

public class SwallowAnimationHandler : MonoBehaviour
{
    [Header("Настройки Анимации")]
    [SerializeField] private Animator animator;
    [SerializeField] private string triggerName = "swallow"; // Имя триггера в Animator

    private void Reset()
    {
        // Автоматически подтягиваем Animator, если он висит на этом же объекте
        animator = GetComponent<Animator>();
    }

    // Метод, который мы будем вызывать при событии
    public void PlaySwallowAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(triggerName))
        {
            animator.SetTrigger(triggerName);
        }
    }
}
