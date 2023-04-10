using UnityEngine;

namespace redd096
{
    public static class CreateLayer
    {
        // layers are binary
        // this is to get 8 in binary
        // LayerMask layer = 1 << 8;

        // this is to add the binary value of a layer named "Ignore Raycast"
        // layer |= 1 << LayerMask.NameToLayer("Ignore Raycast");

        // a raycast with this layer, now hits only things with layer 8 or Ignore Raycast, 
        // but we can use tilde (~) to reverse and hits everything except those layers
        // Layer = ~Layer;

        //or with only one line
        //LayerMask Layer = ~( (1 << 8) | (1 << LayerMask.NameToLayer("Ignore Raycast")) );


        /// <summary>
        /// A raycast with this LayerMask, will hit every layer except the one passed
        /// </summary>
        public static LayerMask LayerAllExcept(int layerIndex)
        {
            LayerMask Layer = new LayerMask();

            Layer |= 1 << layerIndex;

            Layer = ~Layer;

            return Layer;
        }

        /// <summary>
        /// A raycast with this LayerMask, will hit every layer except the one passed
        /// </summary>
        public static LayerMask LayerAllExcept(string layerName)
        {
            int layer = LayerUtility.NameToLayer(layerName);

            return LayerAllExcept(layer);
        }

        /// <summary>
        /// A raycast with this LayerMask, will hit only the layer passed
        /// </summary>
        public static LayerMask LayerOnly(int layerIndex)
        {
            LayerMask Layer = new LayerMask();

            Layer |= 1 << layerIndex;

            return Layer;
        }

        /// <summary>
        /// A raycast with this LayerMask, will hit only the layer passed
        /// </summary>
        public static LayerMask LayerOnly(string layerName)
        {
            int layer = LayerUtility.NameToLayer(layerName);

            return LayerOnly(layer);
        }



        /// <summary>
        /// A raycast with this LayerMask, will hit every layer except those in the array
        /// </summary>
        public static LayerMask LayerAllExcept(int[] layersIndex)
        {
            LayerMask Layer = new LayerMask();

            for (int i = 0; i < layersIndex.Length; i++)
            {
                Layer |= 1 << layersIndex[i];
            }

            Layer = ~Layer;

            return Layer;
        }

        /// <summary>
        /// A raycast with this LayerMask, will hit every layer except those in the array
        /// </summary>
        public static LayerMask LayerAllExcept(string[] layersName)
        {
            int[] layers = LayerUtility.NameToLayers(layersName);

            return LayerAllExcept(layers);
        }

        /// <summary>
        /// A raycast with this LayerMask, will hit only the layers in the array
        /// </summary>
        public static LayerMask LayerOnly(int[] layersIndex)
        {
            LayerMask Layer = new LayerMask();

            for (int i = 0; i < layersIndex.Length; i++)
            {
                Layer |= 1 << layersIndex[i];
            }

            return Layer;
        }

        /// <summary>
        /// A raycast with this LayerMask, will hit only the layers in the array
        /// </summary>
        public static LayerMask LayerOnly(string[] layersName)
        {
            int[] layers = LayerUtility.NameToLayers(layersName);

            return LayerOnly(layers);
        }
    }

    public static class LayerUtility
    {
        /// <summary>
        /// Return int layer from name - check if layer exists
        /// </summary>
        public static int NameToLayer(string layerName)
        {
            IsLayerCreated(layerName);

            int layer = LayerMask.NameToLayer(layerName);

            return layer;
        }

        /// <summary>
        /// Return array of index layers from names - check if every layer exists
        /// </summary>
        public static int[] NameToLayers(string[] layersName)
        {
            int[] layersIndex = new int[layersName.Length];

            for (int i = 0; i < layersIndex.Length; i++)
            {
                IsLayerCreated(layersName[i]);

                int layer = LayerMask.NameToLayer(layersName[i]);

                layersIndex[i] = layer;
            }

            return layersIndex;
        }

        /// <summary>
        /// Check if layer exists
        /// </summary>
        public static bool IsLayerCreated(string layerName)
        {
            if (LayerMask.NameToLayer(layerName) == -1)
            {
                Debug.LogWarning("Nobody created: " + layerName);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if this layer is inside LayerMask
        /// </summary>
        public static bool ContainsLayer(this LayerMask layerMask, int layerToCompare)
        {
            //if add layer to this layermask, and layermask remain equals, then layermask contains this layer
            return layerMask == (layerMask | (1 << layerToCompare));
        }

        /// <summary>
        /// Add layer to this LayerMask
        /// </summary>
        public static LayerMask AddLayer(LayerMask layerMask, int layer)
        {
            return layerMask | (1 << layer);
        }

        /// <summary>
        /// Remove layer from this LayerMask
        /// </summary>
        public static LayerMask RemoveLayer(LayerMask layerMask, int layer)
        {
            return layerMask & ~(1 << layer);
        }
    }
}