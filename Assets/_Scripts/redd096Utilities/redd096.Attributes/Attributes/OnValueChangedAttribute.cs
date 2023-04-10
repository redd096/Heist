using UnityEngine;
#if UNITY_EDITOR
using System.Reflection;
using redd096.Attributes.AttributesEditorUtility;
using UnityEditor;
#endif

namespace redd096.Attributes
{
    /// <summary>
    /// Attribute to call a method when value is changed. Method must have zero parameters, or optional. 
    /// Otherwise must have 2 variable: previous value and new value
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class OnValueChangedAttribute : PropertyAttribute
    {
        public readonly string methodName;
        public readonly bool callMethodInParent;

        /// <summary>
        /// Attribute to call a method when value is changed. Method must have zero parameters, or optional. 
        /// Otherwise must have 2 variable: previous value and new value
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="callMethodInParent">If this field is nested for example in a struct, call method in the struct or in the parent class</param>
        public OnValueChangedAttribute(string methodName, bool callMethodInParent = false)
        {
            this.methodName = methodName;
            this.callMethodInParent = callMethodInParent;
        }
    }

    #region editor

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(OnValueChangedAttribute))]
    public class OnValueChangedDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //save previous value
            object previousValue = property.GetField().GetValue(property.GetTargetObjectWithProperty());

            //draw property
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property);

            //if changed something
            if (EditorGUI.EndChangeCheck())
            {
                //apply modifications, so new value is updated in serialized object
                property.serializedObject.ApplyModifiedProperties();

                OnValueChangedAttribute at = attribute as OnValueChangedAttribute;

                //in target object, find method with same name and invoke
                object targetObject = at.callMethodInParent ? property.serializedObject.targetObject : property.GetTargetObjectWithProperty();
                MethodInfo method = targetObject.GetMethod(at.methodName);

                //only if with 0 or only optional parameters
                if (method != null && method.HasZeroParameterOrOnlyOptional())
                {
                    method.Invoke(targetObject, method.GetDefaultParameters());
                }
                //or with 2 parameters of correct type (previous and new value of this property)
                else if (method != null && HasCorrectParameters(method, previousValue.GetType()))
                {
                    object newValue = property.GetField().GetValue(property.GetTargetObjectWithProperty());
                    method.Invoke(targetObject, new object[2] { previousValue, newValue });
                }
                else
                {
                    Debug.LogWarning($"{targetObject} can invoke only methods with 0 parameters or with 2 parameters of correct type: previous and new value ({at.methodName})", property.serializedObject.targetObject);
                }
            }
        }

        bool HasCorrectParameters(MethodInfo method, System.Type type)
        {
            //must have 2 parameters: previous and new value
            if (method.GetParameters().Length == 2)
            {
                //both parameters must be of correct type (this property type)
                foreach (ParameterInfo parameter in method.GetParameters())
                {
                    if (parameter.ParameterType != type)
                        return false;
                }

                return true;
            }

            return false;
        }
    }

#endif

    #endregion
}