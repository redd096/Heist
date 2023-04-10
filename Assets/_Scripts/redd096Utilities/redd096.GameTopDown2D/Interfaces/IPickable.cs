using UnityEngine;

namespace redd096.GameTopDown2D
{
    public interface IPickable
    {
        /// <summary>
        /// When someone pick up this object
        /// </summary>
        void PickUp();
    }
}