using UnityEngine;
using UnityEngine.AI;
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

        Vector2 mousePos = actions.Player.Point.ReadValue<Vector2>();
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Villager"))
        {
            selectedVillager = hit.collider.gameObject;
            heldVillager = selectedVillager.GetComponent<Villager>();
            selectedAgent = selectedVillager.GetComponent<NavMeshAgent>();




            offset = selectedVillager.transform.position - (Vector3)worldPos;
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
                selectedAgent.ResetPath();

                selectedVillager.GetComponent<VillagerAI>().fsm.OnDropped();
            }

            if (!isDragging && heldVillager != null)
            {
                ShowVillagerUI(heldVillager);
            }

            selectedVillager = null;
            selectedAgent = null;
            heldVillager = null;
            isDragging = false;
            clickTimer = 0;
        }
    }

    private void Update()
    {
        
        if (isDragging)
        {
            Vector2 mousePos = actions.Player.Point.ReadValue<Vector2>();
            Vector2 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

            selectedVillager.transform.position = (Vector3)worldPos + offset;
        }

        if (selectedVillager != null && !isDragging)
        {
            clickTimer += Time.deltaTime;
            if (clickTimer >= dragDelay)
            {
                isDragging = true; // Start dragging after threshold

                if (selectedAgent != null)
                    selectedAgent.enabled = false;
            }
        }
    }

    private void ShowVillagerUI(Villager villager)
    {
        villagerUI.SetActive(true);
        SetVillagerUI.Instance.setSkillText(villager);
        //Debug.Log($"Show villager info for {villager.name}");
        // VillagerUIManager.Instance.OpenUIFor(villager);
    }
}
