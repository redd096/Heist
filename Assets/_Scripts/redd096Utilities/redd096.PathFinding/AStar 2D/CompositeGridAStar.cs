using System.Collections.Generic;
using UnityEngine;

namespace redd096.PathFinding.AStar2D
{
    /// <summary>
    /// Used to create a single grid using every grid in the scene
    /// </summary>
    [AddComponentMenu("redd096/.PathFinding/AStar 2D/Composite Grid A Star 2D")]
    public class CompositeGridAStar : GridAStar
    {
        [Header("Composite")]
        [Tooltip("If the node we want to reach makes the agent overlap with a node outside of every grid, we want our agent to move anyway? Then set true.\n" +
            "Or maybe we prefer to calculate it like a wall? Then set false")]
        [SerializeField] bool agentCanOverlapNodesOutsideGrids = true;

        List<GridAStar> gridsAStar = new List<GridAStar>();
        Vector2 gridWorldPosition;

        float leftCompositeGrid;
        float rightCompositeGrid;
        float upCompositeGrid;
        float downCompositeGrid;

        //return setted position
        public override Vector2 GridWorldPosition => gridWorldPosition;

        protected override void SetGrid()
        {
            //get every grid in scene and update composite grid
            GetGridsAndUpdateCompositeGrid();

            base.SetGrid();
        }

        protected override bool IsWalkable(Vector2 worldPosition, out bool agentCanMoveThrough)
        {
            //check is walkable, only if inside one of the grids in the array
            foreach (GridAStar gridAStar in gridsAStar)
            {
                if (gridAStar.IsInsideGrid(worldPosition))
                    return base.IsWalkable(worldPosition, out agentCanMoveThrough);
            }

            //else return false if outside of any grid (but if setted, agent can move through because is not a really wall, just is not walkable)
            agentCanMoveThrough = agentCanOverlapNodesOutsideGrids;
            return false;
        }

        #region private API

        void GetGridsAndUpdateCompositeGrid()
        {
            gridsAStar.Clear();
            gridWorldSize = Vector2.zero;

            //find every grid in scene
            foreach (GridAStar gridAStar in FindObjectsOfType<GridAStar>())
            {
                //remove self
                if (gridAStar == this)
                    continue;

                //and set or update extremes of the grid
                if (gridsAStar.Count <= 0)
                    SetGridExtremes(gridAStar);            //if this is the first grid to add, set its vars as default
                else
                    UpdateGridExtremes(gridAStar);

                //add to the list
                gridsAStar.Add(gridAStar);
            }

            //update grid world size
            UpdateGridWorldSize();
        }

        void SetGridExtremes(GridAStar gridAStar)
        {
            //set points same as on grid parameter
            leftCompositeGrid = gridAStar.GridWorldPosition.x - (gridAStar.GridWorldSize.x / 2);
            rightCompositeGrid = gridAStar.GridWorldPosition.x + (gridAStar.GridWorldSize.x / 2);
            upCompositeGrid = gridAStar.GridWorldPosition.y + (gridAStar.GridWorldSize.y / 2);
            downCompositeGrid = gridAStar.GridWorldPosition.y - (gridAStar.GridWorldSize.y / 2);
        }

        void UpdateGridExtremes(GridAStar gridAStar)
        {
            //calculate points on grid parameter
            float left = gridAStar.GridWorldPosition.x - (gridAStar.GridWorldSize.x / 2);
            float right = gridAStar.GridWorldPosition.x + (gridAStar.GridWorldSize.x / 2);
            float up = gridAStar.GridWorldPosition.y + (gridAStar.GridWorldSize.y / 2);
            float down = gridAStar.GridWorldPosition.y - (gridAStar.GridWorldSize.y / 2);

            //check if update current points
            if (left < leftCompositeGrid)
                leftCompositeGrid = left;
            if (right > rightCompositeGrid)
                rightCompositeGrid = right;
            if (up > upCompositeGrid)
                upCompositeGrid = up;
            if (down < downCompositeGrid)
                downCompositeGrid = down;
        }

        void UpdateGridWorldSize()
        {
            //set world center of the grid
            gridWorldPosition =
                Vector2.right * (rightCompositeGrid - (rightCompositeGrid - leftCompositeGrid) * 0.5f)  //x
                + Vector2.up * (upCompositeGrid - (upCompositeGrid - downCompositeGrid) * 0.5f);        //y

            //set grid world size
            gridWorldSize = new Vector2(rightCompositeGrid - leftCompositeGrid, upCompositeGrid - downCompositeGrid);
        }

        #endregion
    }
}