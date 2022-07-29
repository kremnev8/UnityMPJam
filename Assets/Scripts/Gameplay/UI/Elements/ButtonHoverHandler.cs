using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Gameplay.UI
{
    public class ButtonHoverHandler : MonoBehaviour, IPointerEnterHandler
    {
        public UnityEvent onHover;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            onHover?.Invoke();
        }
    }
}