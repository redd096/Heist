using redd096.Attributes;
using redd096.GameTopDown2D;
using redd096.StateMachine.StateMachineRedd096;

public class PlayerPawn : Character
{
    [ReadOnly] public PlayerController CurrentController = default;

    private void Awake()
    {
        //deactive statemachine if we are online but we are not the server
        if (NetworkManager.instance && NetworkManager.instance.Runner.IsServer == false)
        {
            GetComponentInChildren<StateMachineRedd096>().enabled = false;
            GetComponentInChildren<RotateUsingAim>().enabled = false;
        }
    }
}
