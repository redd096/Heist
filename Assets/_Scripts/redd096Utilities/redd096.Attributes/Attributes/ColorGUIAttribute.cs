using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace redd096.Attributes
{
    /// <summary>
    /// Attribute to show this variable in inspector as read only
    /// </summary>
    public class ColorGUIAttribute : PropertyAttribute
    {
        public enum EColorType { GUI, Background, Content }

        public readonly AttributesUtility.EColor color;
        public readonly EColorType colorType;

        public ColorGUIAttribute(AttributesUtility.EColor color, EColorType colorType = EColorType.Content)
        {
            this.color = color;
            this.colorType = colorType;
        }
    }

    #region editor

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(ColorGUIAttribute))]
    public class ColorDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //update height when open a list/array/struct in inspector
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ColorGUIAttribute at = attribute as ColorGUIAttribute;

            //color GUI for this property
            Color color;
            SetColorGUI(at, out color);

            EditorGUI.PropertyField(position, property, label, true);

            //reset GUI color
            ResetColorGUI(at, color);
        }

        void SetColorGUI(ColorGUIAttribute at, out Color color)
        {
            //set GUI color
            if (at.colorType == ColorGUIAttribute.EColorType.GUI)
            {
                color = GUI.color;
                GUI.color = AttributesUtility.GetColor(at.color);
            }
            //or content color
            else if (at.colorType == ColorGUIAttribute.EColorType.Content)
            {
                color = GUI.contentColor;
                GUI.contentColor = AttributesUtility.GetColor(at.color);
            }
            //or background color
            else
            {
                color = GUI.backgroundColor;
                GUI.backgroundColor = AttributesUtility.GetColor(at.color);
            }
        }

        void ResetColorGUI(ColorGUIAttribute at, Color color)
        {
            //reset GUI, content or background color
            if (at.colorType == ColorGUIAttribute.EColorType.GUI)
                GUI.color = color;
            else if (at.colorType == ColorGUIAttribute.EColorType.Content)
                GUI.contentColor = color;
            else
                GUI.backgroundColor = color;
        }
    }

#endif

    #endregion
}