using UnityEngine;
using System.Collections.Generic;
using System.Collections;
#if UNITY_EDITOR
using System;
using System.Reflection;
using redd096.Attributes.AttributesEditorUtility;
using UnityEditor;
#endif

namespace redd096.Attributes
{
    /// <summary>
    /// Show dropdown of values
    /// </summary>
    public class DropdownAttribute : PropertyAttribute
    {
        public readonly string valuesName;

        public DropdownAttribute(string valuesName)
        {
            this.valuesName = valuesName;
        }
    }

    #region DropdownList

    public interface IDropdownList : IEnumerable<KeyValuePair<string, object>>
    {
    }

    public class DropdownList<T> : IDropdownList
    {
        private List<KeyValuePair<string, object>> _values;

        public DropdownList()
        {
            _values = new List<KeyValuePair<string, object>>();
        }

        public void Add(string displayName, T value)
        {
            _values.Add(new KeyValuePair<string, object>(displayName, value));
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static explicit operator DropdownList<object>(DropdownList<T> target)
        {
            DropdownList<object> result = new DropdownList<object>();
            foreach (var kvp in target)
            {
                result.Add(kvp.Key, kvp.Value);
            }

            return result;
        }
    }

    #endregion

    #region editor

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(DropdownAttribute))]
	public class DropdownDrawer : PropertyDrawer
	{
		object targetObject;
		FieldInfo dropdownField;
		object valuesObject;

		object[] values;
		string[] displayNames;
		int selectedIndex;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			//get target and property field
			targetObject = property.GetTargetObjectWithProperty();
			dropdownField = property.GetTargetObjectWithProperty().GetField(property.name);

			//get values as an object
			DropdownAttribute at = attribute as DropdownAttribute;
			valuesObject = GetValues(property, at);

			//check field and values are same type
			if (dropdownField.FieldType == ReflectionUtility.GetListElementType(valuesObject.GetType()))
			{
				//set arrays dropdown
				if (valuesObject is IList)
				{
					SetArraysNormally();
				}
				else if (valuesObject is IDropdownList)
				{
					SetArraysDropdownList();
				}

				//show dropdown to select
				selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, displayNames);
				object newValue = values[selectedIndex];

				//if dropdown has changed value, set it
				object dropdownValue = dropdownField.GetValue(targetObject);
				if (dropdownValue == null || dropdownValue.Equals(newValue) == false)
				{
					Undo.RecordObject(property.serializedObject.targetObject, "Dropdown");

					// TODO: Problem with structs, because they are value type.
					// The solution is to make boxing/unboxing but unfortunately I don't know the compile time type of the target object
					dropdownField.SetValue(targetObject, newValue);
				}
			}
			else
			{
				Debug.LogWarning((targetObject as Component).gameObject + " - error between property type and method's return type", (targetObject as Component).gameObject);
			}

			EditorGUI.EndProperty();
		}

		object GetValues(SerializedProperty property, DropdownAttribute at)
		{
			//try get values from field
			FieldInfo fieldValues = targetObject.GetField(at.valuesName);
			if (fieldValues != null)
			{
				return fieldValues.GetValue(targetObject);
			}

			//else try get from a property
			PropertyInfo propertyValues = targetObject.GetProperty(at.valuesName);
			if (propertyValues != null)
			{
				return propertyValues.GetValue(targetObject);
			}

			//else try from a method
			MethodInfo methodValues = targetObject.GetMethod(at.valuesName);
			if (methodValues != null)
			{
				//can have only zero or optional parameters and must return something different from void
				if (methodValues.HasZeroParameterOrOnlyOptional() && methodValues.ReturnType != typeof(void))
				{
					return methodValues.Invoke(targetObject, methodValues.GetDefaultParameters());      //pass default values, if there are optional parameters
				}
				else
				{
					Debug.LogWarning(at.GetType().Name + " can't invoke '" + methodValues.Name + "'. It can invoke only methods with 0 or optional parameters and return type different from void", property.serializedObject.targetObject);
				}
			}

			return null;
		}

		void SetArraysNormally()
		{
			//arrays of values and names
			IList valuesList = (IList)valuesObject;
			values = new object[valuesList.Count];
			displayNames = new string[valuesList.Count];

			//set them
			object value;
			for (int i = 0; i < values.Length; i++)
			{
				value = valuesList[i];
				values[i] = value;                                                  //set value
				displayNames[i] = value == null ? "<null>" : value.ToString();      //set name
			}

			//find selected value index
			selectedIndex = Array.IndexOf(values, dropdownField.GetValue(targetObject));
			if (selectedIndex < 0)
			{
				selectedIndex = 0;
			}
		}

		void SetArraysDropdownList()
		{
			//current value
			object selectedValue = dropdownField.GetValue(targetObject);

			//list of values and names
			IDropdownList dropdown = (IDropdownList)valuesObject;
			List<object> valuesList = new List<object>();
			List<string> displayNamesList = new List<string>();

			//set them
			int i = -1;
			selectedIndex = -1;
			using (IEnumerator<KeyValuePair<string, object>> dropdownEnumerator = dropdown.GetEnumerator())
			{
				while (dropdownEnumerator.MoveNext())
				{
					i++;

					//find selected value index
					KeyValuePair<string, object> current = dropdownEnumerator.Current;
					if (current.Value?.Equals(selectedValue) == true)
					{
						selectedIndex = i;
					}

					valuesList.Add(current.Value);                                  //set value

					if (current.Key == null)                                        //set name
					{
						displayNamesList.Add("<null>");
					}
					else if (string.IsNullOrWhiteSpace(current.Key))
					{
						displayNamesList.Add("<empty>");
					}
					else
					{
						displayNamesList.Add(current.Key);
					}
				}
			}

			//be sure selected index is at least 0
			if (selectedIndex < 0)
			{
				selectedIndex = 0;
			}

			//set arrays
			values = valuesList.ToArray();
			displayNames = displayNamesList.ToArray();
		}
	}

#endif

    #endregion
}