using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSpriteHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image buttonImage;

    [Header("Спрайты")]
    [Tooltip("Обычный спрайт кнопки")]
    public Sprite normalSprite;
    [Tooltip("Спрайт при наведении мыши")]
    public Sprite hoverSprite;

    private void Awake()
    {
        buttonImage = GetComponent<Image>();

        // Если спрайт по умолчанию не задан вручную, берем текущий спрайт из Image
        if (normalSprite == null && buttonImage != null)
        {
            normalSprite = buttonImage.sprite;
        }
    }

    // Срабатывает, когда курсор мыши заходит на кнопку
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonImage != null && hoverSprite != null)
        {
            buttonImage.sprite = hoverSprite;
        }
    }

    // Срабатывает, когда курсор мыши уходит с кнопки
    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonImage != null && normalSprite != null)
        {
            buttonImage.sprite = normalSprite;
        }
    }
}