using UnityEngine;
using System;
#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using redd096.Attributes.AttributesEditorUtility;
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

namespace redd096.Attributes
{
    /// <summary>
    /// Attribute to show button in inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : Attribute
    {
        public enum EEnableType { Always, Editor, PlayMode }

        public readonly string buttonName;
        public readonly EEnableType enableType = EEnableType.Always;

        public ButtonAttribute(EEnableType enableType = EEnableType.Always)
        {
            this.enableType = enableType;
        }

        public ButtonAttribute(string buttonName, EEnableType enableType = EEnableType.Always)
        {
            this.buttonName = buttonName;
            this.enableType = enableType;
        }
    }

    #region editor

#if UNITY_EDITOR

    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ButtonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            //get every method inside monobehaviour
            ButtonAttribute buttonAttribute;
            foreach (MethodInfo method in target.GetMethods())
            {
                //make sure it is decorated by our custom attribute
                buttonAttribute = method.GetCustomAttribute<ButtonAttribute>(true);
                if (buttonAttribute != null)
                {
                    //can have only zero or optional parameters
                    if (method.HasZeroParameterOrOnlyOptional())
                    {
                        //set if button is enabled or disabled
                        EditorGUI.BeginDisabledGroup(
                            buttonAttribute.enableType == ButtonAttribute.EEnableType.Editor && Application.isPlaying                   //if Editor button, disable when in play mode
                            || buttonAttribute.enableType == ButtonAttribute.EEnableType.PlayMode && Application.isPlaying == false);   //if PlayMode button, disable when in editor

                        //if the user clicks the button, invoke the method (show button name or method name)
                        if (GUILayout.Button(string.IsNullOrEmpty(buttonAttribute.buttonName) ? method.Name : buttonAttribute.buttonName))
                        {
                            IEnumerator methodResult = method.Invoke(target, method.GetDefaultParameters()) as IEnumerator;             //pass default values, if there are optional parameters

                            //in editor mode set target object and scene dirty to serialize changes to disk
                            if (Application.isPlaying == false)
                            {
                                EditorUtility.SetDirty(target);

                                PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
                                if (stage != null)
                                    EditorSceneManager.MarkSceneDirty(stage.scene);                             //prefab mode
                                else
                                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());     //normal scene

                                ////repaint scene and inspector
                                //SceneView.RepaintAll();
                                //Repaint();
                            }
                            //in play mode can call also coroutines
                            else if (methodResult != null && target is MonoBehaviour behaviour)
                            {
                                behaviour.StartCoroutine(methodResult);
                            }
                        }

                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        Debug.LogWarning(target.name + " can't invoke '" + method.Name + "'. It can invoke only methods with 0 or optional parameters", target);
                    }
                }
            }
        }
    }

#endif

    #endregion
}