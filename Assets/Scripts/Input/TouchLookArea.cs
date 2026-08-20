using UnityEngine;
using UnityEngine.EventSystems;

namespace FokusTour.Input
{
    /// <summary>
    /// Transparent UI area for first-person look. Place on the right side of the screen.
    /// </summary>
    public class TouchLookArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private Vector2 _delta;
        private bool _isDragging;

        /// <summary>
        /// Look delta since last consume. Call from the controller once per frame.
        /// </summary>
        public Vector2 ConsumeDelta()
        {
            Vector2 value = _delta;
            _delta = Vector2.zero;
            return value;
        }

        public bool IsDragging => _isDragging;

        public void OnPointerDown(PointerEventData eventData)
        {
            _isDragging = true;
            _delta = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _delta += eventData.delta;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isDragging = false;
            _delta = Vector2.zero;
        }
    }
}
