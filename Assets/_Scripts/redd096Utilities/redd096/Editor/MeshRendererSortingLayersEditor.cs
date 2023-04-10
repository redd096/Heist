using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
/*
using UnityEngine.Rendering;
*/

namespace redd096
{
    //show sorting layer and order in layer on 3D mesh (to show above or bottom sprite renderers)
    [CanEditMultipleObjects()]
    [CustomEditor(typeof(MeshRenderer))]
    public class MeshRendererSortingLayersEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            //get sorting layer and order in layer properties
            SerializedProperty sortingLayerID = serializedObject.FindProperty(propertyPath: "m_SortingLayerID");
            SerializedProperty sortingOrder = serializedObject.FindProperty("m_SortingOrder");

            //draw red label
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.richText = true;
            EditorGUILayout.LabelField("<b><color=#EE4035FF>Sorting Layers Options:</color></b>", style);

            //sorting layer
            Rect rectSortingLayer = EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginProperty(rectSortingLayer, GUIContent.none, sortingLayerID); //used only to show changes to save
            string[] layerNames = GetSortingLayerNames();                               //get layer names to show
            int[] layerIDs = GetSortingLayerUniqueIDs();                                //get layer ids
            int selected = GetSelectedSortingLayer(sortingLayerID, layerIDs);
            selected = EditorGUILayout.Popup("Sorting Layer", selected, layerNames);    //show dropdown
            sortingLayerID.intValue = layerIDs[selected];                               //set property value
            EditorGUI.EndProperty();
            EditorGUILayout.EndHorizontal();

            //order in layer
            EditorGUILayout.PropertyField(sortingOrder, new GUIContent("Order in Layer"));

            //apply modified properties
            serializedObject.ApplyModifiedProperties();
        }

        string[] GetSortingLayerNames()
        {
            //get sorting layer names
            Type internalEditorUtilityType = typeof(InternalEditorUtility);
            PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
            return (string[])sortingLayersProperty.GetValue(null, new object[0]);
        }

        int[] GetSortingLayerUniqueIDs()
        {
            //get sorting layer IDs
            Type internalEditorUtilityType = typeof(InternalEditorUtility);
            PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
            return (int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);
        }

        int GetSelectedSortingLayer(SerializedProperty sortingLayerID, int[] layerIDs)
        {
            int selected = -1;
            int sID = sortingLayerID.intValue;

            //find sorting layer ID
            for (int i = 0; i < layerIDs.Length; i++)
                if (sID == layerIDs[i])
                    selected = i;

            //if not found, get layer ID 0
            if (selected == -1)
                for (int i = 0; i < layerIDs.Length; i++)
                    if (layerIDs[i] == 0)
                        selected = i;

            return selected;
        }

        /*
        public override void OnInspectorGUI()
        {

            #region Get Serialized Property
            SerializedProperty sortingLayerID = serializedObject.FindProperty(propertyPath: "m_SortingLayerID");
            SerializedProperty sortingOrder = serializedObject.FindProperty("m_SortingOrder");

            SerializedProperty castShadows = serializedObject.FindProperty("m_CastShadows");
            SerializedProperty receiveShadows = serializedObject.FindProperty("m_ReceiveShadows");
            SerializedProperty motionVectors = serializedObject.FindProperty("m_MotionVectors");
            SerializedProperty materials = serializedObject.FindProperty("m_Materials");
            SerializedProperty lightProbes = serializedObject.FindProperty("m_LightProbeUsage");
            SerializedProperty reflectionProbes = serializedObject.FindProperty("m_ReflectionProbeUsage");
            SerializedProperty anchorProbes = serializedObject.FindProperty("m_ProbeAnchor");
            #endregion

            #region Draw Properties
            AddPropertyField(castShadows);
            AddPropertyField(receiveShadows);
            AddPropertyField(motionVectors);
            AddPropertyField(materials);
            AddPopup(ref lightProbes, "Light Probes", typeof(LightProbeUsage));
            AddPopup(ref reflectionProbes, "Reflection Probes", typeof(ReflectionProbeUsage));
            AddPropertyField(anchorProbes, "Anchor Override");
            #endregion


            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.richText = true;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<b><color=#EE4035FF>SortingLayers Options:</color></b>", style);
            #region SortingLayer
            Rect firstHoriz = EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            //    EditorGUI.PropertyField (mat, new GUIContent ("Materials"));
            EditorGUI.BeginProperty(firstHoriz, GUIContent.none, sortingLayerID);
            string[] layerNames = GetSortingLayerNames();
            int[] layerID = GetSortingLayerUniqueIDs();
            int selected = -1;
            int sID = sortingLayerID.intValue;
            for (int i = 0; i < layerID.Length; i++)
                if (sID == layerID[i])
                    selected = i;
            if (selected == -1)
                for (int i = 0; i < layerID.Length; i++)
                    if (layerID[i] == 0)
                        selected = i;
            selected = EditorGUILayout.Popup("Sorting Layer", selected, layerNames);

            sortingLayerID.intValue = layerID[selected];
            EditorGUI.EndProperty();
            EditorGUILayout.EndHorizontal();
            #endregion

            #region OrderInLayer
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(sortingOrder, new GUIContent("Order in Layer"));
            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
            #endregion


        }
        */

        /*
        void AddPropertyField(SerializedProperty ourSerializedProperty)
        {
            Rect ourRect = EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginProperty(ourRect, GUIContent.none, ourSerializedProperty);
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(property: ourSerializedProperty, includeChildren: true); //I set includeChildren:true to display material children

            EditorGUI.EndProperty();
            EditorGUILayout.EndHorizontal();
        }

        void AddPropertyField(SerializedProperty ourSerializedProperty, string name)
        {
            Rect ourRect = EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginProperty(ourRect, GUIContent.none, ourSerializedProperty);
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(ourSerializedProperty, new GUIContent(name), true);

            EditorGUI.EndProperty();
            EditorGUILayout.EndHorizontal();
        }

        void AddPopup(ref SerializedProperty ourSerializedProperty, string nameOfLabel, Type typeOfEnum)
        {
            Rect ourRect = EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginProperty(ourRect, GUIContent.none, ourSerializedProperty);
            EditorGUI.BeginChangeCheck();

            int actualSelected = 1;
            int selectionFromInspector = ourSerializedProperty.intValue;
            string[] enumNamesList = System.Enum.GetNames(typeOfEnum);
            actualSelected = EditorGUILayout.Popup(nameOfLabel, selectionFromInspector, enumNamesList);
            ourSerializedProperty.intValue = actualSelected;

            EditorGUI.EndProperty();
            EditorGUILayout.EndHorizontal();
        }
        */
    }
}