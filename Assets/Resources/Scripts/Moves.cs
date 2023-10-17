using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Moves : NetworkBehaviour
{
    private Camera _mainCamera;
    public LayerMask groundLayer;
    public LayerMask cellLayer;
    public GameObject planeMesh;
    public Plane plane;

    private bool _placeMode;
    private bool _placed = false;

    private int _playerId;

    private void Awake()
    {
        _mainCamera = Camera.main;
        planeMesh = GameObject.Find("BoardPlane");
        _placeMode = BattleManager.Instance.PlaceMode;
        plane = new Plane(Vector3.up, new Vector3(planeMesh.transform.position.x, planeMesh.transform.position.y, planeMesh.transform.position.z));
    }

    private void OnEnable()
    {
        BattleManager.OnPlaceModeStateChanged += UpdatePlaceModeState;
    }
    private void OnDisable()
    {
        BattleManager.OnPlaceModeStateChanged -= UpdatePlaceModeState;
    }

    private void UpdatePlaceModeState(bool placeModeState)
    {
        _placeMode = placeModeState;
    }


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _playerId = (int)OwnerClientId;
        }
    }

    private void Start()
    {
        ChangePlaceModeState(true);
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (_placeMode && !_placed)
            {

                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                float hitDistance;

                if (Input.GetMouseButtonDown(1))
                {
                    Debug.Log(CardManager.Instance.CardDestroyed.name);
                    ChangePlaceModeState(false);
                    CardManager.Instance.CardDestroyed.SetActive(true);
                    transform.gameObject.SetActive(false);
                }

                if (Physics.Raycast(ray, out hit, 100f, cellLayer))
                {
                    Cell cell;
                    GameObject cellObject;

                    cellObject = hit.collider.gameObject;
                    cell = hit.transform.GetComponent<Cell>();
                    if (cell != null && cell.IsEmpty && cellObject.CompareTag("Player"+BattleManager.Instance.PlayerId))
                    {
                        Vector3 center = new Vector3(hit.transform.position.x, hit.point.y, hit.transform.position.z);
                        transform.position = center + Vector3.up * 0.5f;
                        if (Input.GetMouseButtonDown(0))
                        {
                            ChangePlaceModeState(false);
                            cell.IsEmpty = false;
                            _placed = true;
                        }

                    }
                    else if (plane.Raycast(ray, out hitDistance))
                    {
                        MoveOnPlane(ray, hitDistance);
                    }
                }
                else if (plane.Raycast(ray, out hitDistance))
                {
                    MoveOnPlane(ray, hitDistance);
                }

            }
        }
    }

    private void MoveOnPlane(Ray ray, float distance)
    {
        transform.position = Vector3.Lerp(transform.position, ray.GetPoint(distance), 50f * Time.deltaTime);
    }

    private void ChangePlaceModeState(bool placeModeValue)
    {
        BattleManager.Instance.ChangePlaceModeState(placeModeValue);
    }

}
