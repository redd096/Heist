using redd096.Attributes;
using redd096.GameTopDown2D;

public class PlayerPawn : Character
{
    [ReadOnly] public PlayerController CurrentController = default;
}
