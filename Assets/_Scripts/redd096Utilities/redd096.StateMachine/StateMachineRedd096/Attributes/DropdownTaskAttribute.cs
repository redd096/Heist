using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace redd096.StateMachine.StateMachineRedd096
{
    #region editor property drawer

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(DropdownTaskAttribute))]
    public class DropdownTaskDrawer : PropertyDrawer
    {
        BaseTask[] possibleTasks;
        string[] tasksNames;

        const float heightRef = 20;             //height for readonly reference
        const float spaceBetweemElements = 5;   //space between elements in array

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //add height to show readonly ref + space for next element in array
            return base.GetPropertyHeight(property, label) + heightRef + spaceBetweemElements;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            //get tasks
            GetTasks(property);

            //show dropdown to select and set property
            if (possibleTasks != null && possibleTasks.Length > 0)
            {
                //get current index
                int selectedIndex = GetSelectedIndex(property);

                selectedIndex = EditorGUI.Popup(new Rect(position.x, position.y, position.width, position.height - heightRef - spaceBetweemElements), selectedIndex, tasksNames);   //remove height, because will be used for readonly ref
                property.objectReferenceValue = possibleTasks[selectedIndex];
            }
            //else show NONE
            else
            {
                EditorGUI.Popup(new Rect(position.x, position.y, position.width, position.height - heightRef - spaceBetweemElements), 0, new string[1] { "NONE" });                 //remove height, because will be used for readonly ref
                property.objectReferenceValue = null;
            }

            //show readonly reference
            GUI.enabled = false;
            EditorGUI.PropertyField(new Rect(position.x, position.y + position.height - heightRef - spaceBetweemElements, position.width, heightRef), property);                    //move bottom (-height) and set height for readonly ref
            GUI.enabled = true;

            EditorGUI.EndProperty();

        }

        void GetTasks(SerializedProperty property)
        {
            //get owner of the property
            Component owner = property.serializedObject.targetObject as Component;
            DropdownTaskAttribute at = attribute as DropdownTaskAttribute;
            if (owner)
            {
                //get tasks in children
                Component[] components = owner.GetComponentsInChildren(at.TaskType);

                //set arrays for dropdown
                if (components != null && components.Length > 0)
                {
                    possibleTasks = new BaseTask[components.Length + 1];
                    tasksNames = new string[components.Length + 1];

                    //set NONE as first task
                    possibleTasks[0] = null;
                    tasksNames[0] = "NONE";

                    for (int i = 0; i < components.Length; i++)
                    {
                        possibleTasks[i + 1] = components[i] as BaseTask;
                        tasksNames[i + 1] = possibleTasks[i + 1].TaskName;
                    }

                    return;
                }
            }

            //if there are no tasks, set NONE as unique name
            possibleTasks = new BaseTask[1] { null };
            tasksNames = new string[1] { "NONE" };
        }

        int GetSelectedIndex(SerializedProperty property)
        {
            //return current index
            for (int i = 0; i < possibleTasks.Length; i++)
            {
                if (possibleTasks[i] == property.objectReferenceValue)
                    return i;
            }

            return 0;
        }
    }

#endif

    #endregion

    //attribute
    public class DropdownTaskAttribute : PropertyAttribute
    {
        public readonly System.Type TaskType;

        /// <summary>
        /// Attribute to draw a dropdown for a BaseTask
        /// </summary>
        /// <param name="TaskType">Type of task (ActionTask or ConditionTask)</param>
        public DropdownTaskAttribute(System.Type TaskType)
        {
            this.TaskType = TaskType;
        }
    }
}