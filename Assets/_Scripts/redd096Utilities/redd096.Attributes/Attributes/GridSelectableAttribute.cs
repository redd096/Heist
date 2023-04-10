using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using redd096.Attributes.AttributesEditorUtility;
using UnityEditor;
#endif

namespace redd096.Attributes
{
    /// <summary>
    /// Draw a grid. Return Vector2Int for every selected square. If this attribute is on an Integer or Vector2Int, can use it as size
    /// </summary>
    public class GridSelectableAttribute : PropertyAttribute
    {
        public readonly string vector2IntArrayProperty;
        public int sizeX { get; private set; }
        public int sizeY { get; private set; }

        /// <summary>
        /// Draw a grid. Return Vector2Int for every selected square. If this attribute is on an Integer or Vector2Int, can use it as size
        /// </summary>
        /// <param name="sizeX"></param>
        /// <param name="sizeY"></param>
        public GridSelectableAttribute(string vector2IntArrayProperty, int sizeX = 3, int sizeY = 3)
        {
            this.vector2IntArrayProperty = vector2IntArrayProperty;

            SetSize(sizeX, sizeY);
        }

        public void SetSize(int sizeX, int sizeY)
        {
            //can't be negative
            this.sizeX = Mathf.Max(1, sizeX);
            this.sizeY = Mathf.Max(1, sizeY);
        }
    }

    #region editor
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(GridSelectableAttribute))]
    public class GridSelectableDrawer : PropertyDrawer
    {
        GridSelectableAttribute at;
        int propertyHeight = 18;
        int littleSpace = 2;
        int sizeSquare = 40;
        float spaceBetweenSquares = 2;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //size to draw first property (if int or vector2 int)
            int propertySize = property.propertyType == SerializedPropertyType.Integer || property.propertyType == SerializedPropertyType.Vector2Int ?
                propertyHeight : 0;

            //property height + little space + every row * (size square + space between)
            int y = (attribute as GridSelectableAttribute).sizeY;
            return littleSpace + propertySize + littleSpace + y * (sizeSquare + spaceBetweenSquares);

            //return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //get vector2IntArrayProperty value
            at = attribute as GridSelectableAttribute;
            Vector2Int[] arrayValues = property.GetValue(at.vector2IntArrayProperty) as Vector2Int[];

            bool someValueIsChanged = false;    //check if update property
            Vector2 startPosition = new Vector2(position.x, position.y + littleSpace);

            //set size
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, propertyHeight), property, new GUIContent(property.name));
                property.intValue = Mathf.Max(1, property.intValue);
                property.serializedObject.ApplyModifiedProperties();

                at.SetSize(property.intValue, property.intValue);

                startPosition.y += propertyHeight + littleSpace;
            }
            else if (property.propertyType == SerializedPropertyType.Vector2Int)
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, propertyHeight), property, new GUIContent(property.name));
                property.vector2IntValue = new Vector2Int(Mathf.Max(1, property.vector2IntValue.x), Mathf.Max(1, property.vector2IntValue.y));
                property.serializedObject.ApplyModifiedProperties();

                at.SetSize(property.vector2IntValue.x, property.vector2IntValue.y);

                startPosition.y += propertyHeight + littleSpace;
            }

            ValueStruct[,] values = new ValueStruct[at.sizeX, at.sizeY];
            bool xIsEven = at.sizeX % 2 == 0;       //if even skip coordinate 0
            bool yIsEven = at.sizeY % 2 == 0;       //if even skip coordinate 0
            int coordinatesX = -at.sizeX / 2;       //example odd with size 5 => -2, -1, 0, 1, 2
            int coordinatesY = -at.sizeY / 2;       //example even with size 4 => -2, -1, 1, 2

            for (int x = 0; x < at.sizeX; x++)
            {
                //if even, skip 0
                if (xIsEven && coordinatesX == 0)
                    coordinatesX++;

                for (int y = at.sizeY - 1; y >= 0; y--)     //inverse, because EditorGUI has 0,0 on top left instead of down left
                {
                    //if even, skip 0
                    if (yIsEven && coordinatesY == 0)
                        coordinatesY++;

                    Vector2Int coordinates = new Vector2Int(coordinatesX, coordinatesY);
                    values[x,y] = new ValueStruct { coordinates = coordinates, isUsed = ContainsValue(arrayValues, coordinates) };

                    //change button gui color
                    Color previousGUIColor = GUI.backgroundColor;

                    //set button
                    string buttonName = coordinates.x + "," + coordinates.y;
                    GUI.backgroundColor = values[x,y].isUsed ? Color.red : Color.grey;

                    //when click, change value
                    if (GUI.Button(new Rect(startPosition.x + x * (sizeSquare + spaceBetweenSquares), startPosition.y + y * (sizeSquare + spaceBetweenSquares), 
                        sizeSquare, sizeSquare), buttonName))
                    {
                        someValueIsChanged = true;
                        values[x,y].isUsed = !values[x,y].isUsed;

                        //if keep pressed Shift, reset value
                        if (Event.current.shift)
                            values[x,y].isUsed = false;
                    }

                    //reset color
                    GUI.backgroundColor = previousGUIColor;

                    coordinatesY++;
                }

                coordinatesY = -at.sizeY / 2;
                coordinatesX++;
            }

            //update property only if some value is changed
            if (someValueIsChanged == false)
                return;

            //set values in property
            List<Vector2Int> v = new List<Vector2Int>();
            for (int x = 0; x < at.sizeX; x++)
                for (int y = at.sizeY - 1; y >= 0; y--)     //inverse, because EditorGUI has 0,0 on top left instead of down left
                    if (values[x,y].isUsed)
                        v.Add(values[x,y].coordinates);

            //get vector2IntArrayProperty serialized property
            SerializedProperty arrayProperty = property.FindCorrectProperty(at.vector2IntArrayProperty);

            arrayProperty.arraySize = v.Count;
            for (int i = 0; i < v.Count; i++)
                arrayProperty.GetArrayElementAtIndex(i).vector2IntValue = v[i];

            arrayProperty.serializedObject.ApplyModifiedProperties();
        }

        bool ContainsValue(Vector2Int[] array, Vector2Int value)
        {
            if (array == null)
                return false;

            //check if value is inside array
            foreach (Vector2Int v in array)
                if (v == value)
                    return true;

            return false;
        }

        public struct ValueStruct
        {
            public bool isUsed;
            public Vector2Int coordinates;
        }
    }

#endif
    #endregion
}