using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class VillagerDragHandler : MonoBehaviour
{
    private Camera mainCamera; //Uses main camera to get mouse location
    private GameObject selectedVillager; //Villager being picked up
    private InputSystem_Actions actions; //Defines our input actions
    private NavMeshAgent selectedAgent; //The navmeshagent of the villager needs to be disabled on pickup otherwise it stops working
    private Vector3 offset; //difference between mouse pos and click pos
    private bool isDragging; //Sets whether its currently dragging/ Might change to just use heldVillager instead

    public float dragDelay = 0.1f;
    private float clickTimer = 0f;

    public Villager heldVillager;

    public GameObject villagerUI;

    [Header("Camera controls")]
    public float cameraSpeed = 5f;
    public float zoomSpeed = 5f;

    public Vector2 minCameraPos = new Vector2(-10, -10);
    public Vector2 maxCameraPos = new Vector2(10, 10);
    public float minZoom = 3f;
    public float maxZoom = 20f;

    private bool isCameraDragging;
    private Vector3 cameraDragStart;

    [Header("Camera Follow")]
    private bool isFollowingVillager = false;
    private Transform followTarget;

    void Start()
    {
        mainCamera = Camera.main;
    }
    private void Awake()
    {
        mainCamera = Camera.main;

        // Create actions instance
        actions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        // Enable action map
        actions.Player.Enable();

        // Subscribe to events
        actions.Player.Click.started += OnClickStarted;
        actions.Player.Click.canceled += OnClickCanceled;
    }

    private void OnDisable()
    {
        // Unsubscribe (good practice to avoid leaks)
        actions.Player.Click.started -= OnClickStarted;
        actions.Player.Click.canceled -= OnClickCanceled;
        actions.Player.Disable();
    }

    private void OnClickStarted(InputAction.CallbackContext ctx)
    {
        clickTimer = 0f;
        isDragging = false;

        Vector2 mouseScreen = actions.Player.Point.ReadValue<Vector2>();

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreen);
        worldPos.z = 0;

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Villager"))
        {
            // Drag villager
            selectedVillager = hit.collider.gameObject;
            heldVillager = selectedVillager.GetComponent<Villager>();
            selectedAgent = selectedVillager.GetComponent<NavMeshAgent>();
            offset = selectedVillager.transform.position - worldPos;

            // Focus camera on this villager
            isFollowingVillager = true;
            followTarget = selectedVillager.transform;
        }
        else
        {
            // Drag camera
            isCameraDragging = true;
            cameraDragStart = mainCamera.ScreenToWorldPoint(mouseScreen);
            cameraDragStart.z = mainCamera.transform.position.z;
            isFollowingVillager = false;
        }
    }

    private void OnClickCanceled(InputAction.CallbackContext ctx)
    {
        if (selectedVillager != null)
        {
            if (selectedAgent != null && isDragging)
            {
                selectedAgent.enabled = true;
                selectedVillager.GetComponent<VillagerAI>().ApplyRole(selectedVillager.GetComponent<VillagerAI>().role);
                selectedAgent.Warp(selectedVillager.transform.position);
                selectedVillager.GetComponent<VillagerAI>().fsm.OnDropped();
            }

            if (!isDragging && heldVillager != null)
                ShowVillagerUI(heldVillager);

            selectedVillager = null;
            selectedAgent = null;
            heldVillager = null;
            isDragging = false;
            clickTimer = 0;
        }

        isCameraDragging = false;
    }

    private void Update()
    {
        HandleDragging();
        HandleCamera();
    }

    private void ShowVillagerUI(Villager villager)
    {
        villagerUI.SetActive(true);
        SetVillagerUI.Instance.setSkillText(villager);
        //Debug.Log($"Show villager info for {villager.name}");
        // VillagerUIManager.Instance.OpenUIFor(villager);
    }

    #region Camera Controls

    private void HandleDragging()
    {
        if (isDragging && selectedVillager != null)
        {
            Vector2 mouseScreen = actions.Player.Point.ReadValue<Vector2>();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreen);
            worldPos.z = 0;
            selectedVillager.transform.position = worldPos + offset;
        }

        if (selectedVillager != null && !isDragging)
        {
            clickTimer += Time.unscaledDeltaTime;
            if (clickTimer >= dragDelay)
            {
                isDragging = true;
                if (selectedAgent != null)
                    selectedAgent.enabled = false;


                selectedVillager.GetComponent<VillagerAI>().fsm.OnPickup();
            }

        }
    }

    private void HandleCamera()
    {
        // Zoom input cancels follow
        float zoom = actions.Player.Zoom.ReadValue<float>();
        if (Mathf.Abs(zoom) > 0.01f)
            isFollowingVillager = false;

        if (isFollowingVillager && followTarget != null)
        {
            // Smoothly follow villager
            Vector3 targetPos = followTarget.position;
            targetPos.z = mainCamera.transform.position.z;
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, cameraSpeed * Time.unscaledDeltaTime);
        }
        else
        {
            // Free camera movement
            if (isCameraDragging)
            {
                Vector2 mouseScreen = actions.Player.Point.ReadValue<Vector2>();
                Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);
                mouseWorld.z = mainCamera.transform.position.z;

                Vector3 diff = cameraDragStart - mouseWorld;
                mainCamera.transform.position += diff;
            }

            // Zoom
            mainCamera.orthographicSize -= zoom * zoomSpeed * Time.unscaledDeltaTime;
        }

        // Clamp camera position and zoom
        mainCamera.transform.position = new Vector3(
            Mathf.Clamp(mainCamera.transform.position.x, minCameraPos.x, maxCameraPos.x),
            Mathf.Clamp(mainCamera.transform.position.y, minCameraPos.y, maxCameraPos.y),
            mainCamera.transform.position.z
        );

        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);
    }

    #endregion
}
