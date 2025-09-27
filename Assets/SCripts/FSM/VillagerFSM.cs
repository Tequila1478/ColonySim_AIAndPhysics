using UnityEngine;

public class VillagerFSM : MonoBehaviour
{
    public VillagerStateBase currentState;
    public VillagerAI villager;

    public void ChangeState(VillagerStateBase newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;

        if (currentState != null)
            currentState.Enter();
    }

    void Update()
    {
        if (currentState != null)
            currentState.Execute();
    }

    public void OnDropped()
    {
        if (currentState != null)
            currentState.Dropped();
    }

    public void OnPickup()
    {
        if (currentState != null)
            currentState.PickUp();
    }
}
