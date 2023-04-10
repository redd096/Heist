using UnityEngine;
#if UNITY_EDITOR
using redd096.Attributes.AttributesEditorUtility;
using UnityEditor;
#endif

namespace redd096.Attributes
{
    /// <summary>
    /// Attribute to show variable writable only if condition is false
    /// </summary>
    public class DisableIfAttribute : PropertyAttribute
    {
        public enum EComparisonType { isEqual, isNotEqual, isGreater, isGreaterEqual, isLower, isLowerEqual }
        public enum EConditionOperator { AND, OR }

        public readonly bool comparing;

        //check every value
        public readonly string[] values;
        public readonly EConditionOperator conditionOperator;

        //compare value
        public readonly string propertyA;
        public readonly object valueToCompare;
        public readonly EComparisonType comparisonType;

        public DisableIfAttribute(params string[] values)
        {
            comparing = false;

            //by default condition is AND
            conditionOperator = EConditionOperator.AND;
            this.values = values;
        }

        public DisableIfAttribute(EConditionOperator conditionOperator, params string[] values)
        {
            comparing = false;

            this.conditionOperator = conditionOperator;
            this.values = values;
        }

        public DisableIfAttribute(string propertyA, object valueToCompare, EComparisonType comparisonType = EComparisonType.isEqual)
        {
            comparing = true;

            this.propertyA = propertyA;
            this.valueToCompare = valueToCompare;
            this.comparisonType = comparisonType;
        }
    }

    #region editor

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(DisableIfAttribute))]
    public class DisableIfDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //update height when open a list/array/struct in inspector
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //disable GUI for this property if CanDisable is true
            GUI.enabled = CanDisable(property) == false;

            EditorGUI.PropertyField(position, property, label, true);

            //re-enable GUI
            GUI.enabled = true;
        }

    #region private API

        bool CanDisable(SerializedProperty property)
        {
            DisableIfAttribute at = attribute as DisableIfAttribute;

            //compare property
            if (at.comparing)
            {
                object comparedObject = property.GetValue(at.propertyA);

                //compare based on type
                if (comparedObject is bool)
                    return CompareEqual(at, (bool)comparedObject);
                else if (comparedObject is string)
                    return CompareEqual(at, (string)comparedObject);
                else if (comparedObject is System.Enum)
                    return CompareInt(at, (int)comparedObject);
                else if (comparedObject is int)
                    return CompareInt(at, (int)comparedObject);
                else if (comparedObject is float)
                    return CompareFloat(at, (float)comparedObject);
                else
                    return CompareEqual(at, comparedObject);

                //return false;
            }
            //or normally check every value
            else
            {
                return CheckValues(at, property);
            }
        }

        bool CheckValues(DisableIfAttribute at, SerializedProperty property)
        {
            //foreach value to check
            foreach (string value in at.values)
            {
                bool check = (bool)property.GetValue(value, typeof(bool));

                //if check AND, when find one false, return false
                if (at.conditionOperator == DisableIfAttribute.EConditionOperator.AND && check == false)
                {
                    return false;
                }
                //if check OR, when find one true, return true
                else if (at.conditionOperator == DisableIfAttribute.EConditionOperator.OR && check)
                {
                    return true;
                }
            }

            //else return if check is AND, cause it mean every value is true if check AND, or every value is false if check OR
            return at.conditionOperator == DisableIfAttribute.EConditionOperator.AND;
        }

        bool CompareEqual<T>(DisableIfAttribute at, T comparedProperty)
        {
            //equal
            if (at.comparisonType == DisableIfAttribute.EComparisonType.isEqual)
                return comparedProperty.Equals((T)at.valueToCompare);
            //not equal
            else if (at.comparisonType == DisableIfAttribute.EComparisonType.isNotEqual)
                return comparedProperty.Equals((T)at.valueToCompare) == false;
            else
                return false;
        }

        bool CompareInt(DisableIfAttribute at, int comparedProperty)
        {
            //compare property A and value
            switch (at.comparisonType)
            {
                case DisableIfAttribute.EComparisonType.isEqual:
                    return comparedProperty == (int)at.valueToCompare;
                case DisableIfAttribute.EComparisonType.isNotEqual:
                    return comparedProperty != (int)at.valueToCompare;
                case DisableIfAttribute.EComparisonType.isGreater:
                    return comparedProperty > (int)at.valueToCompare;
                case DisableIfAttribute.EComparisonType.isGreaterEqual:
                    return comparedProperty >= (int)at.valueToCompare;
                case DisableIfAttribute.EComparisonType.isLower:
                    return comparedProperty < (int)at.valueToCompare;
                case DisableIfAttribute.EComparisonType.isLowerEqual:
                    return comparedProperty <= (int)at.valueToCompare;
                default:
                    return comparedProperty == (int)at.valueToCompare;
            }
        }

        bool CompareFloat(DisableIfAttribute at, float comparedProperty)
        {
            //compare property A and value
            switch (at.comparisonType)
            {
                case DisableIfAttribute.EComparisonType.isEqual:
                    return Mathf.Approximately(comparedProperty, (float)at.valueToCompare);
                case DisableIfAttribute.EComparisonType.isNotEqual:
                    return Mathf.Approximately(comparedProperty, (float)at.valueToCompare) == false;
                case DisableIfAttribute.EComparisonType.isGreater:
                    return comparedProperty > (float)at.valueToCompare;
                case DisableIfAttribute.EComparisonType.isGreaterEqual:
                    return comparedProperty >= (float)at.valueToCompare;
                case DisableIfAttribute.EComparisonType.isLower:
                    return comparedProperty < (float)at.valueToCompare;
                case DisableIfAttribute.EComparisonType.isLowerEqual:
                    return comparedProperty <= (float)at.valueToCompare;
                default:
                    return Mathf.Approximately(comparedProperty, (float)at.valueToCompare);
            }
        }

    #endregion
    }

#endif

    #endregion
}