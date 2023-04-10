using UnityEngine;

/// <summary>
/// Can use this for a Shop, where can sell Weapons and other things. Just add this interface to WeaponBASE
/// </summary>
public interface ISellable
{
    string SellName { get; }
    int SellPrice { get; }
    Sprite SellSprite { get; }
}
