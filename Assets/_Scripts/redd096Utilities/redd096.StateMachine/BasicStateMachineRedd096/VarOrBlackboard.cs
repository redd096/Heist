using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace redd096.StateMachine.BasicStateMachineRedd096
{
    /// <summary>
    /// Display variable with a toggle to select Blackboard Variable or Normal Variable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class VarOrBlackboard<T>
    {
        [SerializeField] bool useBlackboard;
        [SerializeField] string blackboardName;
        [SerializeField] T normalVariable;

        public VarOrBlackboard()
        {
        }

        /// <summary>
        /// Use this constructor to create default blackboard variable, e.g. VarOrBlackboard<int> varName = new VarOrBlackboard<int>("Number");
        /// </summary>
        /// <param name="blackboardName"></param>
        public VarOrBlackboard(string blackboardName)
        {
            useBlackboard = true;
            this.blackboardName = blackboardName;
        }

        /// <summary>
        /// Get value from blackboard or normal variable
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <returns></returns>
        public T GetValue(StateMachineRedd096 stateMachine)
        {
            return useBlackboard ? (stateMachine ? stateMachine.GetBlackboardElement<T>(blackboardName) : default) : normalVariable;
        }

        /// <summary>
        /// Return blackboard name
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return blackboardName;
        }

        /// <summary>
        /// Use this convertor to make for example VarOrBlackboard<int> varName = 5;
        /// </summary>
        /// <param name="v"></param>
        public static implicit operator VarOrBlackboard<T>(T value)
        {
            //set only normal variable
            VarOrBlackboard<T> v = new VarOrBlackboard<T>();
            v.normalVariable = value;
            return v;
        }
    }

    #region editor
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(VarOrBlackboard<>), true)]
    public class VarOrBlackboardEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //return base.GetPropertyHeight(property, label);
            return 0;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);

            //draw in horizontal
            EditorGUI.BeginProperty(position, label, property);
            EditorGUILayout.BeginHorizontal();

            //get property
            SerializedProperty useBlackboard = property.FindPropertyRelative("useBlackboard");

            //set button color
            Color color = GUI.color;
            GUI.color = useBlackboard.boolValue ? Color.yellow : new Color32(255, 152, 203, 255);

            //button with B (blackboard) or N (normal)
            if (GUILayout.Button(useBlackboard.boolValue ? "B" : "N"))
                useBlackboard.boolValue = !useBlackboard.boolValue;

            GUI.color = color;

            //show variable name
            EditorGUILayout.PrefixLabel(label);

            //show blackboard name or normal variable
            EditorGUILayout.PropertyField(useBlackboard.boolValue ?
                property.FindPropertyRelative("blackboardName") :
                property.FindPropertyRelative("normalVariable"),
                GUIContent.none);

            EditorGUILayout.EndHorizontal();
            EditorGUI.EndProperty();
        }
    }

#endif
    #endregion
}