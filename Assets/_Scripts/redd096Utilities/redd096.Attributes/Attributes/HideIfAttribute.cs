using UnityEngine;
#if UNITY_EDITOR
using redd096.Attributes.AttributesEditorUtility;
using UnityEditor;
#endif

namespace redd096.Attributes
{
    /// <summary>
    /// Attribute to show variable only if condition is false
    /// </summary>
    public class HideIfAttribute : PropertyAttribute
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

        public HideIfAttribute(params string[] values)
        {
            comparing = false;

            //by default condition is AND
            conditionOperator = EConditionOperator.AND;
            this.values = values;
        }

        public HideIfAttribute(EConditionOperator conditionOperator, params string[] values)
        {
            comparing = false;

            this.conditionOperator = conditionOperator;
            this.values = values;
        }

        public HideIfAttribute(string propertyA, object valueToCompare, EComparisonType comparisonType = EComparisonType.isEqual)
        {
            comparing = true;

            this.propertyA = propertyA;
            this.valueToCompare = valueToCompare;
            this.comparisonType = comparisonType;
        }
    }

    #region editor

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(HideIfAttribute))]
    public class HideIfDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //update height only if can hide is false (to hide space where there are hidden variables) - necessary also to update height when open a list/array/struct in inspector
            if (CanHide(property) == false)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            //else hide space
            return 0;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //show only if CanHide return false
            if (CanHide(property) == false)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

    #region private API

        bool CanHide(SerializedProperty property)
        {
            HideIfAttribute at = attribute as HideIfAttribute;

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

        bool CheckValues(HideIfAttribute at, SerializedProperty property)
        {
            //foreach value to check
            foreach (string value in at.values)
            {
                bool check = (bool)property.GetValue(value, typeof(bool));

                //if check AND, when find one false, return false
                if (at.conditionOperator == HideIfAttribute.EConditionOperator.AND && check == false)
                {
                    return false;
                }
                //if check OR, when find one true, return true
                else if (at.conditionOperator == HideIfAttribute.EConditionOperator.OR && check)
                {
                    return true;
                }
            }

            //else return if check is AND, cause it mean every value is true if check AND, or every value is false if check OR
            return at.conditionOperator == HideIfAttribute.EConditionOperator.AND;
        }

        bool CompareEqual<T>(HideIfAttribute at, T comparedProperty)
        {
            //equal
            if (at.comparisonType == HideIfAttribute.EComparisonType.isEqual)
                return comparedProperty.Equals((T)at.valueToCompare);
            //not equal
            else if (at.comparisonType == HideIfAttribute.EComparisonType.isNotEqual)
                return comparedProperty.Equals((T)at.valueToCompare) == false;
            else
                return false;
        }

        bool CompareInt(HideIfAttribute at, int comparedProperty)
        {
            //compare property A and value
            switch (at.comparisonType)
            {
                case HideIfAttribute.EComparisonType.isEqual:
                    return comparedProperty == (int)at.valueToCompare;
                case HideIfAttribute.EComparisonType.isNotEqual:
                    return comparedProperty != (int)at.valueToCompare;
                case HideIfAttribute.EComparisonType.isGreater:
                    return comparedProperty > (int)at.valueToCompare;
                case HideIfAttribute.EComparisonType.isGreaterEqual:
                    return comparedProperty >= (int)at.valueToCompare;
                case HideIfAttribute.EComparisonType.isLower:
                    return comparedProperty < (int)at.valueToCompare;
                case HideIfAttribute.EComparisonType.isLowerEqual:
                    return comparedProperty <= (int)at.valueToCompare;
                default:
                    return comparedProperty == (int)at.valueToCompare;
            }
        }

        bool CompareFloat(HideIfAttribute at, float comparedProperty)
        {
            //compare property A and value
            switch (at.comparisonType)
            {
                case HideIfAttribute.EComparisonType.isEqual:
                    return Mathf.Approximately(comparedProperty, (float)at.valueToCompare);
                case HideIfAttribute.EComparisonType.isNotEqual:
                    return Mathf.Approximately(comparedProperty, (float)at.valueToCompare) == false;
                case HideIfAttribute.EComparisonType.isGreater:
                    return comparedProperty > (float)at.valueToCompare;
                case HideIfAttribute.EComparisonType.isGreaterEqual:
                    return comparedProperty >= (float)at.valueToCompare;
                case HideIfAttribute.EComparisonType.isLower:
                    return comparedProperty < (float)at.valueToCompare;
                case HideIfAttribute.EComparisonType.isLowerEqual:
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