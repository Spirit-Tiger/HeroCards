using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private GameObject HandBox;

    private Vector3 _upPosition = new Vector3(0, 200f, 0);
    public void OnPointerEnter(PointerEventData eventData)
    {
        HandBox.transform.position = HandBox.transform.position + _upPosition;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HandBox.transform.position = HandBox.transform.position - _upPosition;
    }

}
