using UnityEngine;

public class VillagerFSM : MonoBehaviour
{
    public IVillagerState currentState;

    public void ChangeState(IVillagerState newState)
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
            currentState.OnDropped();
    }
}
