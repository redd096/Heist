using UnityEngine;

namespace redd096
{
    [AddComponentMenu("redd096/TileGrid/Tile Color Map")]
    [SelectionBase]
    public class TileColorMap : TileBase
    {
        [Header("Color Map")]
        public Color tileColor = Color.white;
    }
}