using UnityEngine;

namespace redd096
{
    public abstract class TileBase : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] protected Vector2Int positionInGrid;

        public Vector2Int PositionInGrid => positionInGrid;

        public void Init(Vector2Int positionInGrid)
        {
            this.positionInGrid = positionInGrid;
        }
    }
}