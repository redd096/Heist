using UnityEngine;

namespace redd096
{
    #region editor

#if UNITY_EDITOR

    using UnityEditor;

    [CustomEditor(typeof(GridColorMap))]
    public class GridBaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);

            if (GUILayout.Button("Regen Grid"))
            {
                ((GridColorMap)target).RegenGrid();

                //set undo
                Undo.RegisterFullObjectHierarchyUndo(target, "Regen Grid");
            }

            if(GUILayout.Button("Destroy Grid"))
            {
                ((GridColorMap)target).DestroyGrid();

                //set undo
                Undo.RegisterFullObjectHierarchyUndo(target, "Destroy Grid");
            }
        }
    }

#endif

    #endregion

    [AddComponentMenu("redd096/TileGrid/Grid Color Map")]
    public class GridColorMap : GridBase
    {
        [Header("Color Map")]
        [Tooltip("When import texture, set Non-Power of 2 to None, and enable Read/Write")] [SerializeField] Texture2D gridImage = default;
        [SerializeField] TileColorMap[] tiles = default;

        protected override TileBase GetTilePrefab(int x, int y)
        {
            //get color in texture2D
            Color color = gridImage.GetPixel(x, y);

            //foreach tile in list, find tile with this color
            foreach(TileColorMap tile in tiles)
            {
                if (tile.tileColor == color)
                    return tile;
            }

            return null;
        }

        public void RegenGrid()
        {
            //remove old grid and generate new one
            RemoveOldGrid();
            GenerateGrid(gridImage.width, gridImage.height);
        }

        public void DestroyGrid()
        {
            //remove old grid
            RemoveOldGrid();
        }
    }
}