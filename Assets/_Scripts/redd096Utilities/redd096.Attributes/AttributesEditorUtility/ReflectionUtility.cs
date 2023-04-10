using System.Reflection;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace redd096.Attributes.AttributesEditorUtility
{
	public static class ReflectionUtility
	{
        #region everything from SerializedProperty
#if UNITY_EDITOR

        /// <summary>
        /// Return field of this property, used for example to get or set a value without know the type
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static FieldInfo GetField(this SerializedProperty property)
		{
			//find field using property.name
			return property.GetTargetObjectWithProperty().GetField(property.name);
        }

        /// <summary>
        /// Return a value from a field, or property, or method. Use this property targetObject
        /// </summary>
        /// <param name="property"></param>
        /// <param name="valueName"></param>
        /// <param name="methodReturnTypes">Call methods only with this return type. If null call every method</param>
        /// <returns></returns>
        public static object GetValue(this SerializedProperty property, string valueName, params System.Type[] methodReturnTypes)
		{
			return property.GetTargetObjectWithProperty().GetValue(valueName, methodReturnTypes);
		}

#endif
        #endregion

        #region return first

        /// <summary>
        /// Get Field by name
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static FieldInfo GetField(this object targetObject, string fieldName)
		{
			foreach (FieldInfo field in targetObject.GetFields())
				if (field.Name.Equals(fieldName, System.StringComparison.Ordinal))
					return field;

			return null;
		}

		/// <summary>
		/// Get Property by name
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public static PropertyInfo GetProperty(this object targetObject, string propertyName)
		{
			foreach (PropertyInfo property in targetObject.GetProperties())
				if (property.Name.Equals(propertyName, System.StringComparison.Ordinal))
					return property;

			return null;
		}

		/// <summary>
		/// Get Method by name
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public static MethodInfo GetMethod(this object targetObject, string methodName)
		{
			foreach (MethodInfo method in targetObject.GetMethods())
				if (method.Name.Equals(methodName, System.StringComparison.Ordinal))
					return method;

			return null;
		}

		#endregion

		#region return array

		/// <summary>
		/// Return every field in this object
		/// </summary>
		/// <param name="targetObject"></param>
		/// <returns></returns>
		public static FieldInfo[] GetFields(this object targetObject)
		{
			return targetObject.GetType().GetFieldsRecursively();
		}

		/// <summary>
		/// Return every property in this object
		/// </summary>
		/// <param name="targetObject"></param>
		/// <returns></returns>
		public static PropertyInfo[] GetProperties(this object targetObject)
		{
			return targetObject.GetType().GetPropertiesRecursively();
		}

		/// <summary>
		/// Return every method in this object
		/// </summary>
		/// <param name="targetObject"></param>
		/// <returns></returns>
		public static MethodInfo[] GetMethods(this object targetObject)
		{
			return targetObject.GetType().GetMethodsRecursively();
		}

        #endregion

        #region return array recursively

        /// <summary>
        /// Return every field in this type and parents' type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static FieldInfo[] GetFieldsRecursively(this System.Type type)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;

            //call this function until there is no base type
            if (type.BaseType == null || type.BaseType == type)
                return type.GetFields(flags);
            else
                return type.GetFields(flags).Concat(GetFieldsRecursively(type.BaseType)).ToArray();
        }

        /// <summary>
        /// Return every property in this type and parents' type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetPropertiesRecursively(this System.Type type)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;

            //call this function until there is no base type
            if (type.BaseType == null || type.BaseType == type)
                return type.GetProperties(flags);
            else
                return type.GetProperties(flags).Concat(GetPropertiesRecursively(type.BaseType)).ToArray();
        }

        /// <summary>
        /// Return every property in this type and parents' type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MethodInfo[] GetMethodsRecursively(this System.Type type)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;

            //call this function until there is no base type
            if (type.BaseType == null || type.BaseType == type)
                return type.GetMethods(flags);
            else
                return type.GetMethods(flags).Concat(GetMethodsRecursively(type.BaseType)).ToArray();
        }

        #endregion

        #region generic

        /// <summary>
        /// Return type of a list
        /// </summary>
        /// <param name="listType"></param>
        /// <returns></returns>
        public static System.Type GetListElementType(System.Type listType)
		{
			//if generic, get first argument type
			if (listType.IsGenericType)
			{
				return listType.GetGenericArguments()[0];
			}
			//else get list type
			else
			{
				return listType.GetElementType();
			}
		}

		/// <summary>
		/// Return default value for every parameter inside a method
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		public static object[] GetDefaultParameters(this MethodInfo method)
		{
			//foreach parameter, return default value
			return method.GetParameters().Select(p => p.DefaultValue).ToArray();
		}

		/// <summary>
		/// Return if this method has 0 parameters, or if there are parameters but all optionals
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		public static bool HasZeroParameterOrOnlyOptional(this MethodInfo method)
		{
			return method.GetParameters().All(p => p.IsOptional);
		}

		/// <summary>
		/// Return true if this method's ReturnType is void
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		public static bool IsReturnTypeVoid(this MethodInfo method)
        {
			return method.ReturnType == typeof(void);
        }

		/// <summary>
		/// Return true if this method's ReturnType is one of these in the parameters
		/// </summary>
		/// <param name="method"></param>
		/// <param name="methodReturnTypes"></param>
		/// <returns></returns>
		public static bool IsReturnTypeOneOfThese(this MethodInfo method, params System.Type[] methodReturnTypes)
        {
			foreach (System.Type t in methodReturnTypes)
            {
				if (method.ReturnType == t)
					return true;
            }

			return false;
        }

		#endregion

		#region return value

		/// <summary>
		/// Return a value from a field, or property, or method.
		/// </summary>
		/// <param name="targetObject"></param>
		/// <param name="valueName"></param>
		/// <param name="methodReturnTypes">Call methods only with this return type. If null call every method</param>
		/// <returns></returns>
		public static object GetValue(this object targetObject, string valueName, params System.Type[] methodReturnTypes)
		{
			//if string is empty, return null
			if (string.IsNullOrEmpty(valueName))
			{
				return null;
			}
			else
			{
				//else try get values from field
				FieldInfo fieldValues = targetObject.GetField(valueName);
				if (fieldValues != null)
				{
					return fieldValues.GetValue(targetObject);
				}

				//else try get from a property
				PropertyInfo propertyValues = targetObject.GetProperty(valueName);
				if (propertyValues != null)
				{
					return propertyValues.GetValue(targetObject);
				}

				//else try from a method
				MethodInfo methodValues = targetObject.GetMethod(valueName);
				if (methodValues != null)
				{
					//can have only zero or optional parameters
					if (methodValues.HasZeroParameterOrOnlyOptional())
					{
						//if there aren't return types, call every type of method
						if (methodReturnTypes == null || methodReturnTypes.Length <= 0)
						{
							//pass default values, if there are optional parameters
							return methodValues.Invoke(targetObject, methodValues.GetDefaultParameters());
						}
						//else check if return type is inside the array
						else
						{
							foreach (System.Type t in methodReturnTypes)
							{
								//pass default values, if there are optional parameters
								if (methodValues.ReturnType == t)
									return methodValues.Invoke(targetObject, methodValues.GetDefaultParameters());
							}
						}
					}

					UnityEngine.Object targetUnityObject = targetObject as UnityEngine.Object;
					UnityEngine.Debug.LogWarning(targetUnityObject.name + " can't invoke '" + methodValues.Name + "'. It can invoke only methods with 0 or optional parameters and return type: " + SystemTypesToString(false, methodReturnTypes), targetUnityObject);
				}

				return null;
			}
		}

		#endregion

		#region debug

		/// <summary>
		/// Return a string with every System.Type separated by a comma
		/// </summary>
		/// <param name="defaultIsVoid">If types are null or length is 0, return void as type</param>
		/// <param name="types"></param>
		/// <returns></returns>
		public static string SystemTypesToString(bool defaultIsVoid = true, params System.Type[] types)
        {
			//if no types, return void as type or empty
			if (types == null || types.Length <= 0)
            {
				return defaultIsVoid ? "void" : "";
            }

			//else return every type separated by a comma
			string s = "";
			foreach (System.Type t in types)
            {
				s += (t.Name + ", ");
            }

			return s;
        }

        #endregion
    }
}