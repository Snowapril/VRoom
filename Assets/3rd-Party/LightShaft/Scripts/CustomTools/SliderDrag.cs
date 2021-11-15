using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LightShaft.Scripts.CustomTools
{
    [RequireComponent(typeof(Slider))]
    public class SliderDrag : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        public UnityEvent onSliderStartDrag;
        public SliderDragEvent onSliderEndDrag;
        
        private float SliderValue
        {
            get { return gameObject.GetComponent<Slider>().value; }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            onSliderStartDrag.Invoke();
        }

        public void OnPointerUp(PointerEventData data)
        {
            onSliderEndDrag.Invoke(SliderValue);
        }
    }

    [System.Serializable]
    public class SliderDragEvent : UnityEvent<float>
    {
    }
}