using UnityEngine;
#if UNITY_EDITOR
using redd096.Attributes.AttributesEditorUtility;
using UnityEditor;
#endif

namespace redd096.Attributes
{
    /// <summary>
    /// Show a progress bar (property/max value)
    /// </summary>
    public class ProgressBarAttribute : PropertyAttribute
    {
        public readonly string name;
        public readonly float maxValue;
        public readonly string maxValueName;
        public readonly AttributesUtility.EColor color;
        public readonly bool canInteract;

        public ProgressBarAttribute(string name, float maxValue, AttributesUtility.EColor color = AttributesUtility.EColor.Red, bool canInteract = true)
        {
            this.name = name;
            this.maxValue = maxValue;
            this.color = color;
            this.canInteract = canInteract;
        }

        public ProgressBarAttribute(string name, string maxValueName, AttributesUtility.EColor color = AttributesUtility.EColor.Red, bool canInteract = true)
        {
            this.name = name;
            this.maxValueName = maxValueName;
            this.color = color;
            this.canInteract = canInteract;
        }

        public ProgressBarAttribute(float maxValue, AttributesUtility.EColor color = AttributesUtility.EColor.Red, bool canInteract = true)
        {
            this.name = string.Empty;
            this.maxValue = maxValue;
            this.color = color;
            this.canInteract = canInteract;
        }

        public ProgressBarAttribute(string maxValueName, AttributesUtility.EColor color = AttributesUtility.EColor.Red, bool canInteract = true)
        {
            this.name = string.Empty;
            this.maxValueName = maxValueName;
            this.color = color;
            this.canInteract = canInteract;
        }
    }

    #region editor

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(ProgressBarAttribute))]
	public class ProgressBarDrawer : PropertyDrawer
	{
		float value;
		float maxValue;
		string barLabel;
		Color barColor;
		bool canInteract;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			//get attribute and max value
			ProgressBarAttribute at = attribute as ProgressBarAttribute;
			object maxValueObject = property.GetValue(at.maxValueName, typeof(int), typeof(float));

			//be sure property and maxValue are numbers
			if (IsNumber(property) && IsNumber(maxValueObject))
			{
				//set values
				value = property.propertyType == SerializedPropertyType.Integer ? property.intValue : property.floatValue;
				string valueFormatted = property.propertyType == SerializedPropertyType.Integer ? value.ToString() : string.Format("{0:0.00}", value);
				maxValue = maxValueObject is int ? (int)maxValueObject : (float)maxValueObject;
				string maxValueFormatted = maxValueObject is int ? maxValue.ToString() : string.Format("{0:0.00}", maxValue);
				barLabel = (!string.IsNullOrEmpty(at.name) ? "[" + at.name + "] " : "") + valueFormatted + "/" + maxValueFormatted;
				barColor = AttributesUtility.GetColor(at.color);
				canInteract = at.canInteract;

				//draw bar
				DrawBar(property, position);
			}
			else
			{
				Debug.LogWarning(property.serializedObject.targetObject + " - " + at.GetType().Name + " can be used only on int or float properties", (property.serializedObject.targetObject as Component).gameObject);
			}
		}

		bool IsNumber(SerializedProperty property)
		{
			//be sure is int or float
			return property != null && (property.propertyType == SerializedPropertyType.Integer || property.propertyType == SerializedPropertyType.Float);
		}

		bool IsNumber(object maxValueObject)
        {
			return maxValueObject != null && (maxValueObject is int || maxValueObject is float);
        }

		void DrawBar(SerializedProperty property, Rect position)
		{
			Rect fillRect = new Rect(position.x, position.y, position.width * (value / maxValue), position.height);

			//draw bar
			EditorGUI.DrawRect(position, new Color(0.13f, 0.13f, 0.13f));
			EditorGUI.DrawRect(fillRect, barColor);

			//save GUI default colors and alignment
			Color contentColor = GUI.contentColor;
			Color color = GUI.color;
			TextAnchor align = GUI.skin.label.alignment;

			//set alignment and color
			GUI.skin.label.alignment = TextAnchor.UpperCenter;
			GUI.contentColor = Color.white;

			//draw label
			Rect labelRect = new Rect(position.x, position.y - 2, position.width, position.height);
			EditorGUI.DropShadowLabel(labelRect, barLabel);

			//draw invisible slider just to edit bar value
			if (canInteract)
			{
				GUI.color = Color.clear;
				if (property.propertyType == SerializedPropertyType.Integer)
					property.intValue = Mathf.FloorToInt(GUI.HorizontalSlider(position, property.intValue, 0, maxValue));
				else
					property.floatValue = GUI.HorizontalSlider(position, property.floatValue, 0, maxValue);
			}

			//reset color and alignment
			GUI.contentColor = contentColor;
			GUI.color = color;
			GUI.skin.label.alignment = align;
		}
	}

#endif

    #endregion
}