#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
#endif

namespace redd096.Attributes.AttributesEditorUtility
{
    public static class FindSerializedPropertyUtility
    {
#if UNITY_EDITOR

        #region find target object

        /// <summary>
        /// Gets the object that the property is a member of
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static object GetTargetObjectWithProperty(this SerializedProperty property)
        {
            string path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            string[] elements = path.Split('.');

            for (int i = 0; i < elements.Length - 1; i++)
            {
                string element = elements[i];
                if (element.Contains("["))
                {
                    string elementName = element.Substring(0, element.IndexOf("["));
                    int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            return obj;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
            {
                return null;
            }

            Type type = source.GetType();

            while (type != null)
            {
                FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    return field.GetValue(source);
                }

                PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    return property.GetValue(source, null);
                }

                type = type.BaseType;
            }

            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            IEnumerable enumerable = GetValue_Imp(source, name) as IEnumerable;
            if (enumerable == null)
            {
                return null;
            }

            IEnumerator enumerator = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++)
            {
                if (!enumerator.MoveNext())
                {
                    return null;
                }
            }

            return enumerator.Current;
        }

        #endregion

        #region find property

        /// <summary>
        /// Find property also if inside an array or in parent
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyToFind"></param>
        /// <returns></returns>
        public static SerializedProperty FindCorrectProperty(this SerializedProperty property, string propertyToFind)
        {
            //if parent is null, path is: propertyName
            //if property is array, path is: propertyName.Array.data[index] (because property drawer calculate every element of array as single property)
            //if property has a parent, path is: parentName.propertyName
            //if parent is array, path is: parentName.Array.data[index].propertyName

            //if propertyToFind is in same object of this property, call: property.serializedObject.FindProperty(propertyToFind)
            //if this property is an array and propertyToFind is an element inside of this array, call: property.FindPropertyRelative(propertyToFind)
            //if this property is a struct or class and propertyToFind is an element inside of it, call: property.FindPropertyRelative(propertyToFind)
            //if this property is inside a struct and propertyToFind is in same object of the struct, call: property.GetParent().serializedObject.FindProperty(propertyToFind)

            //try get property inside of this object
            SerializedProperty foundProperty = property.serializedObject.FindProperty(propertyToFind);
            if (foundProperty != null) 
                return foundProperty;

            //else try to find inside of this property (if it's a struct, a class or array and propertyToFind is inside of it)
            foundProperty = property.FindPropertyRelative(propertyToFind);
            if (foundProperty != null)
                return foundProperty;

            //else try to find inside every parent
            SerializedProperty parent = property.GetPropertyParent();
            while (foundProperty == null && parent != null)
            {
                foundProperty = parent.serializedObject.FindProperty(propertyToFind);
                parent = property.GetPropertyParent();
            }

            return foundProperty;
        }
        
        /// <summary>
        /// Return parent of this property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static SerializedProperty GetPropertyParent(this SerializedProperty property)
        {
            //if in array, path will be
            //parentName.Array.data[index].property
            //but if this property is an array too, path will be
            //parentName.Array.data[index].property.Array.data[index]

            //remove every .Array.data so we have parentName[index].property (or parentName[index].property[index])
            string path = property.propertyPath.Replace(".Array.data", "");
            int i = path.LastIndexOf('.');

            //if there isn't, there is not a parent
            if (i < 0)
                return null;

            //remove [index] if property is array
            if (path[path.Length -1] == ']')
            {
                int bracketIndex = path.LastIndexOf('[');
                path = path.Remove(bracketIndex, path.LastIndexOf(']') - bracketIndex + 1);
            }
            //if parent is array, remove [index] also from it, so path will be parentName.property
            if (path[i - 1] == ']')
            {
                int bracketIndex = path.LastIndexOf('[');
                path = path.Remove(bracketIndex, path.LastIndexOf(']') - bracketIndex + 1);
            }

            //reset arrays in parents
            path = path.Replace("[", ".Array.data[");
        
            //get the path to parent and return it
            path = path.Substring(0, path.LastIndexOf('.'));
            return property.serializedObject.FindProperty(path);
        }

        #endregion

        #region property array

        /// <summary>
        /// Return property as array instead of elements inside of it
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static SerializedProperty GetPropertyArray(this SerializedProperty property)
        {
            //if this property is array, path is
            //property.Array.data[index]

            string path = property.propertyPath;

            //remove Array.data[index] if property is array
            if (path[path.Length - 1] == ']')
            {
                path = path.Remove(path.LastIndexOf('.'), 1);
                path = path.Substring(0, path.LastIndexOf('.'));
            }

            //return it
            return property.serializedObject.FindProperty(path);
        }

        /// <summary>
        /// Return index of this element inside an array
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static int GetPropertyIndexInsideArray(this SerializedProperty property)
        {
            //if this property is array, path is
            //property.Array.data[index]

            string path = property.propertyPath;

            //if array, remove brackets and read index
            if (path[path.Length - 1] == ']')
            {
                int bracketIndex = path.LastIndexOf('[');
                path = path.Replace("[", "").Replace("]", "");

                if (int.TryParse(path.Substring(bracketIndex), out int result))
                    return result;
            }

            return -1;
        }

        #endregion

#endif
    }
}