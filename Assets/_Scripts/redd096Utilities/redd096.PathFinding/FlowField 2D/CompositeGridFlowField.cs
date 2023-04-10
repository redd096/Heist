using System.Collections.Generic;
using UnityEngine;

namespace redd096.PathFinding.FlowField2D
{
    /// <summary>
    /// Used to create a single grid using every grid in the scene
    /// </summary>
    [AddComponentMenu("redd096/.PathFinding/FlowField 2D/Composite Grid FlowField 2D")]
    public class CompositeGridFlowField : GridFlowField
    {
        [Header("Composite")]
        [Tooltip("If the node we want to reach makes the agent overlaps with a node outside of every grid, we want our agent to move anyway? Then set true.\n" +
            "Or maybe we prefer to calculate it like a wall? Then set false")]
        [SerializeField] bool agentCanOverlapNodesOutsideGrids = true;

        List<GridFlowField> grids = new List<GridFlowField>();
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

        protected override bool IsWalkable(Vector2 worldPosition, out bool agentCanOverlap)
        {
            //check is walkable, only if inside one of the grids in the array
            foreach (GridFlowField grid in grids)
            {
                if (grid.IsInsideGrid(worldPosition))
                    return base.IsWalkable(worldPosition, out agentCanOverlap);
            }

            //else return false if outside of any grid (but if setted, agent can overlap because is not a real wall, just is not walkable)
            agentCanOverlap = agentCanOverlapNodesOutsideGrids;
            return false;
        }

        #region private API

        void GetGridsAndUpdateCompositeGrid()
        {
            grids.Clear();
            gridWorldSize = Vector2.zero;

            //find every grid in scene
            foreach (GridFlowField grid in FindObjectsOfType<GridFlowField>())
            {
                //remove self and grids not used by composite
                if (grid == this || grid.UsedByComposite == false)
                    continue;

                //and set or update extremes of the grid
                if (grids.Count <= 0)
                    SetGridExtremes(grid);            //if this is the first grid to add, set its vars as default
                else
                    UpdateGridExtremes(grid);

                //add to the list
                grids.Add(grid);
            }

            //update grid world size
            UpdateGridWorldSize();
        }

        void SetGridExtremes(GridFlowField grid)
        {
            //set points same as on grid parameter
            leftCompositeGrid = grid.GridWorldPosition.x - (grid.GridWorldSize.x * 0.5f);
            rightCompositeGrid = grid.GridWorldPosition.x + (grid.GridWorldSize.x * 0.5f);
            upCompositeGrid = grid.GridWorldPosition.y + (grid.GridWorldSize.y * 0.5f);
            downCompositeGrid = grid.GridWorldPosition.y - (grid.GridWorldSize.y * 0.5f);
        }

        void UpdateGridExtremes(GridFlowField grid)
        {
            //calculate points on grid parameter
            float left = grid.GridWorldPosition.x - (grid.GridWorldSize.x * 0.5f);
            float right = grid.GridWorldPosition.x + (grid.GridWorldSize.x * 0.5f);
            float up = grid.GridWorldPosition.y + (grid.GridWorldSize.y * 0.5f);
            float down = grid.GridWorldPosition.y - (grid.GridWorldSize.y * 0.5f);

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
                Vector2.right * (rightCompositeGrid - (rightCompositeGrid - leftCompositeGrid) * 0.5f)              //x
                + Vector2.up * (upCompositeGrid - (upCompositeGrid - downCompositeGrid) * 0.5f);                    //y

            //set grid world size
            gridWorldSize = new Vector2(rightCompositeGrid - leftCompositeGrid, upCompositeGrid - downCompositeGrid);
        }

        #endregion
    }
}