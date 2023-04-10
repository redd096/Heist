using UnityEngine;
#if UNITY_EDITOR
using redd096.Attributes.AttributesEditorUtility;
using UnityEditor;
#endif

namespace redd096.Attributes
{
    /// <summary>
    /// Show sprite or texture preview
    /// </summary>
    public class ShowAssetPreviewAttribute : PropertyAttribute
    {
        public readonly int width;
        public readonly int height;

        public ShowAssetPreviewAttribute(int width = 60, int height = 60)
        {
            this.width = width;
            this.height = height;
        }
    }

    #region editor

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(ShowAssetPreviewAttribute))]
    public class ShowAssetPreviewDrawer : PropertyDrawer
    {
        ShowAssetPreviewAttribute at;
        Texture2D assetPreview;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //if there is texture, add height and 10 (a bit of space between vars)
            return EditorGUI.GetPropertyHeight(property, label, true) + (at != null && assetPreview ? at.height + 10 : 0);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //draw property
            EditorGUI.PropertyField(position, property, label, true);

            //get attribute
            at = attribute as ShowAssetPreviewAttribute;

            //get property value (texture or sprite) and get AssetPreview from it
            object obj = property.GetValue(property.name, typeof(Texture), typeof(Sprite));
            assetPreview = AssetPreview.GetAssetPreview(obj as Object);

            //show texture preview
            if (at != null && assetPreview)
            {
                GUI.DrawTexture(new Rect(position.x, position.y + position.height - at.height - 5, at.width, at.height), assetPreview);
            }


            //Texture textureToShow = null;
            //if (obj is Texture)
            //{
            //    textureToShow = obj as Texture;
            //}
            //else if (obj is Sprite)
            //{
            //    Sprite sprite = obj as Sprite;
            //    if (sprite)
            //        textureToShow = sprite.texture;
            //}
            //
            //if (at != null && textureToShow)
            //{
            //    EditorGUI.DrawPreviewTexture(new Rect(position.x, position.y + position.height - at.height - 5, at.width, at.height), textureToShow);
            //}
        }
    }

#endif

    #endregion
}