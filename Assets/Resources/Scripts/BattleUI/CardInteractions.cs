using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardInteractions : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
   
    private RectTransform rectTransform;
    private Vector3 CardInitialPos;

    private Vector3 _dragOffset;
    private Camera _cam;

    [SerializeField]
    private GameObject _playCardField;

    private List<RaycastResult> _raycastResults = new List<RaycastResult>();

    private bool _isDragging;
    private bool _placeMode;

    private Color _defaultColor = new Color(0, 0, 0, 0);
    private Color _selectedColor = new Color32(112, 224, 81, 150);

    private void OnEnable()
    {
        BattleManager.OnDragStateChanged += UpdateDragState;
        BattleManager.OnDragStateChanged += UpdatePlaceModeState;
    }
    private void OnDisable()
    {
        BattleManager.OnDragStateChanged -= UpdateDragState;
        BattleManager.OnDragStateChanged -= UpdatePlaceModeState;
    }

    private void UpdateDragState(bool dragState)
    {
        _isDragging = dragState;
    }

    private void UpdatePlaceModeState(bool placeModeState)
    {
        _placeMode = placeModeState;
    }
    private void Awake()
    {
        _cam = Camera.main;

        _playCardField = GameObject.Find("PlayCardField");
        rectTransform = GetComponent<RectTransform>();

    }

    private void Start()
    {
        _isDragging = BattleManager.Instance.IsDragging;
        _placeMode = BattleManager.Instance.PlaceMode;
        CardInitialPos = rectTransform.localPosition;

    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            rectTransform.localPosition = CardInitialPos;
            _playCardField.GetComponent<RawImage>().color = _defaultColor;
            ChangeDragState(false);

        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CardManager.Instance.CardSelected)
        {
            transform.localScale = transform.localScale * 1.2f;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ChangeDragState(true);

        Transform parentTransform = transform.parent;
        int childCount = 0;
        if (parentTransform != null)
        {
            childCount = parentTransform.childCount;
        }
        transform.SetSiblingIndex(childCount - 1);
        CardManager.Instance.CardSelected = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isDragging)
        {
            rectTransform.position = Input.mousePosition + _dragOffset;

            EventSystem.current.RaycastAll(eventData, _raycastResults);
            if (!_raycastResults.Exists(x => x.gameObject.name == "HandField"))
            {
                _playCardField.GetComponent<RawImage>().color = _selectedColor;
            }
            else { _playCardField.GetComponent<RawImage>().color = _defaultColor; }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        CardManager.Instance.CardSelected = false;
        _playCardField.GetComponent<RawImage>().color = _defaultColor;
        if (!_raycastResults.Exists(x => x.gameObject.name == "HandField") && _isDragging)
        {
            CardManager.Instance.CardDestroyed = gameObject;
            gameObject.SetActive(false);

            ClientDataSO clientData = ClientManager.Instance.GetClientData();
            SpawnManager.Instance.SpawnUnit(BattleManager.Instance.PlayerId,0, _cam.ScreenToWorldPoint(Input.mousePosition));
            SpawnManager.Instance.DespawnCard(clientData.ClientId,gameObject.GetComponent<Card>().CardId.Value);
        }
        ChangeDragState(false);
        rectTransform.localPosition = CardInitialPos;

    }

    private void ChangeDragState(bool dragValue)
    {
        BattleManager.Instance.ChangeDragState(dragValue);
    }

    private void ChangePlaceModeState(bool placeModeValue)
    {
        BattleManager.Instance.ChangePlaceModeState(placeModeValue);
    }
}
