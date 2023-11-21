using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardInteractions : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    private RectTransform rectTransform;
    private Vector3 CardInitialPos;
    private Vector3 _cardLocalPosition;

    private Vector3 _dragOffset;
    private Camera _cam;
    private ClientDataSO _client;

    private GameObject _playCardField;

    private List<RaycastResult> _raycastResults = new List<RaycastResult>();

    private bool _isDragging;
    private bool _placeMode;

    private Color _defaultColor = new Color(0, 0, 0, 0);
    private Color _selectedColor = new Color32(112, 224, 81, 150);

    private int _index;

    private void OnEnable()
    {
        BattleManager.OnDragStateChanged += UpdateDragState;
        BattleManager.OnDragStateChanged += UpdatePlaceModeState;
        ClientManager.OnClientDataChanged += UpdateClientState;
    }
    public override void OnDestroy()
    {
        BattleManager.OnDragStateChanged -= UpdateDragState;
        BattleManager.OnDragStateChanged -= UpdatePlaceModeState;
        ClientManager.OnClientDataChanged -= UpdateClientState;
    }

    private void UpdateClientState()
    {
        _client = ClientManager.Instance.GetClientData();
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

        if (IsClient)
        {
            _client = ClientManager.Instance.GetClientData();
            Debug.Log("Client Can Act " + _client.ClientCanAct);
        }


    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(1) && _isDragging)
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
            ChangeCardSizeServerRpc();

        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeCardSizeServerRpc()
    {
        transform.localScale = transform.localScale * 1.2f;
        ChangeSiblingIndexClientRpc();
    }

    [ClientRpc]
    private void ChangeSiblingIndexClientRpc()
    {
        _index = transform.GetSiblingIndex();
        transform.SetSiblingIndex(transform.parent.childCount - 1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ChangeCardSizeBackServerRpc();

    }


    [ServerRpc(RequireOwnership = false)]
    private void ChangeCardSizeBackServerRpc()
    {
        transform.localScale = Vector3.one;
        ChangeBackSiblingIndexClientRpc();
    }

    [ClientRpc]
    private void ChangeBackSiblingIndexClientRpc()
    {
        transform.SetSiblingIndex(_index);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        int manaValue = 0;

        if (_client.ClientCanAct)
        {
            if (ClientManager.Instance.State == GameState.FirstPlayerTurn)
            {
                manaValue = BattleManager.Instance.CurrentManaPlayer1.Value;

            }
            else if (ClientManager.Instance.State == GameState.SecondPlayerTurn)
            {
                manaValue = BattleManager.Instance.CurrentManaPlayer2.Value;
            }

            if (_client != null && transform.GetComponent<Card>().cardStats.ManaCost <= manaValue)
            {
                ChangeCardOwnershipServerRpc(_client.ClientId);
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
        }

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isDragging)
        {
            rectTransform.position = Input.mousePosition + _dragOffset;

            EventSystem.current.RaycastAll(eventData, _raycastResults);
            if (!_raycastResults.Exists(x => x.gameObject.name == "HandFieldPlayer" + _client.ClientId))
            {
                _playCardField.GetComponent<RawImage>().color = _selectedColor;
            }
            else { _playCardField.GetComponent<RawImage>().color = _defaultColor; }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeCardOwnershipServerRpc(int clientId)
    {
        gameObject.GetComponent<NetworkObject>().ChangeOwnership((ulong)clientId);
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        CardManager.Instance.CardSelected = false;
        _playCardField.GetComponent<RawImage>().color = _defaultColor;
        if (!_raycastResults.Exists(x => x.gameObject.name == "HandFieldPlayer" + _client.ClientId) && _isDragging)
        {
            CardManager.Instance.CardDestroyed = gameObject;
            gameObject.SetActive(false);

            ClientDataSO clientData = ClientManager.Instance.GetClientData();
            clientData.ClientHand.Remove(gameObject);
            SpawnManager.Instance.SpawnUnit(BattleManager.Instance.PlayerId, transform.GetComponent<Card>().cardStats.UnitId, _cam.ScreenToWorldPoint(Input.mousePosition));
            BattleManager.Instance.UseManaServerRpc(-transform.GetComponent<Card>().cardStats.ManaCost);
            SpawnManager.Instance.DespawnCard(clientData.ClientId, gameObject.GetComponent<Card>().CardId.Value);

        }
        ChangeDragState(false);
        rectTransform.localPosition = CardInitialPos;
        ChangeCardOwnershipServerRpc(0);

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
