using UnityEngine;
using UnityEditor;
namespace redd096
{
    public class WindowParseCSV : EditorWindow
    {
        Vector2 scrollPosition = Vector2.zero;
        Vector2 scrollCSV = Vector2.zero;
        Vector2 scrollParsedCSV = Vector2.zero;
        Vector2 scrollArraysParsedCSV = Vector2.zero;

        WindowParseCSVData data;
        WindowCSVData csvData;

        /// <summary>
        /// Open Window from Editor
        /// </summary>
        [MenuItem("redd096/Open Window Parse CSV")]
        static void OpenWindowParseCSV()
        {
            //open window (and set title)
            GetWindow<WindowParseCSV>("Window Parse CSV");
        }

        void OnEnable()
        {
            //load data
            data = WindowParseCSVData.LoadData();
            csvData = WindowCSVData.LoadData();
        }

        void OnDisable()
        {
            //set dirty to save
            EditorUtility.SetDirty(data);
            EditorUtility.SetDirty(csvData);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.Space(50);

            //select item to load
            SelectItemToLoad();

            EditorGUILayout.Space(30);

            //parse options
            GUIParse();

            EditorGUILayout.Space(30);

            //show CSVs
            GUICSV();

            EditorGUILayout.Space(15);
            EditorGUILayout.EndScrollView();
        }

        #region window GUI API

        void SelectItemToLoad()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();

            //create options with name of every item in the list
            string[] optionsList = new string[csvData.StructCSV.Count];
            for (int i = 0; i < csvData.StructCSV.Count; i++)
            {
                //check every next element in the list
                for (int j = i + 1; j < csvData.StructCSV.Count; j++)
                {
                    //if same name already in the list, change it to not have duplicates (because EditorGUILayout.Popup doesn't show duplicates - and now name is used also in LoadFile(string name))
                    while (csvData.StructCSV[i].StructName.Equals(csvData.StructCSV[j].StructName))
                        csvData.StructCSV[i].StructName += "#";
                }

                optionsList[i] = csvData.StructCSV[i].StructName;
            }

            //show every item
            data.IndexStruct = EditorGUILayout.Popup(data.IndexStruct, optionsList);

            EditorGUILayout.Space();

            //button load songs
            if (GUILayout.Button("Load CSV"))
            {
                data.ParseClass.DefaultCSV = ManageCSV.LoadFile(data.IndexStruct);
                data.ParseClass.ParsedCSV = null;
                data.ParseClass.ArraysParsedCSV = null;
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        void GUIParse()
        {
            //label Parse
            EditorGUILayout.LabelField("Parse Options", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            //dictionary
            DictionaryParse();

            EditorGUILayout.Space();

            //first parse
            FirstParse();

            EditorGUILayout.Space();

            //split arrays
            SplitArraysParse();

            EditorGUILayout.Space();

            //button for parse
            ButtonForParse();
        }

        void DictionaryParse()
        {
            EditorGUILayout.LabelField("Dictionary", EditorStyles.boldLabel);

            //set vars for dictionary
            data.ParseClass.AllCapsDictionaryValues = EditorGUILayout.Toggle("All Caps", data.ParseClass.AllCapsDictionaryValues);
            data.ParseClass.NoSpacesDictionaryValues = EditorGUILayout.Toggle("No Spaces", data.ParseClass.NoSpacesDictionaryValues);
        }

        void FirstParse()
        {
            EditorGUILayout.LabelField("First Parse", EditorStyles.boldLabel);

            //set var to trim elements
            data.ParseClass.TrimParsedElements = EditorGUILayout.Toggle("Trim Parsed Elements", data.ParseClass.TrimParsedElements);
        }

        void SplitArraysParse()
        {
            EditorGUILayout.LabelField("Split Arrays", EditorStyles.boldLabel);

            //check if split arrays
            data.ParseClass.SplitArrays = EditorGUILayout.Toggle("Split Arrays", data.ParseClass.SplitArrays);
            if (data.ParseClass.SplitArrays)
            {
                //set vars to split arrays
                data.ParseClass.StringToSplitArrayElements = EditorGUILayout.TextField("String to Split:", data.ParseClass.StringToSplitArrayElements);
                data.ParseClass.TrimArrayElements = EditorGUILayout.Toggle("Trim Elements", data.ParseClass.TrimArrayElements);
                data.ParseClass.RemoveEmptyArrayElements = EditorGUILayout.Toggle("Remove Empty", data.ParseClass.RemoveEmptyArrayElements);
                data.ParseClass.RemoveDuplicates = EditorGUILayout.Toggle("Remove Duplicates", data.ParseClass.RemoveDuplicates);
            }
        }

        void ButtonForParse()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();

            //button for Parse
            if (GUILayout.Button("Parse CSV"))
            {
                data.ParseClass.Parse();
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
        }

        void GUICSV()
        {
            //show CSV
            ShowCSV();

            EditorGUILayout.Space();

            //show parsed CSV
            ShowParsedCSV();

            EditorGUILayout.Space();

            //show array parsed CSV
            ShowArrayParsedCSV();
        }

        void ShowCSV()
        {
            //label CSV
            EditorGUILayout.LabelField("Loaded CSV", EditorStyles.boldLabel);

            scrollCSV = EditorGUILayout.BeginScrollView(scrollCSV);
            Vector2 textSize = EditorStyles.whiteLabel.CalcSize(new GUIContent(data.ParseClass.DefaultCSV));

            //show CSV
            EditorGUILayout.LabelField(data.ParseClass.DefaultCSV, EditorStyles.whiteLabel, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true), GUILayout.MinHeight(textSize.y), GUILayout.MinWidth(textSize.x));

            EditorGUILayout.EndScrollView();
        }

        void ShowParsedCSV()
        {
            //label CSV
            EditorGUILayout.LabelField("Parsed CSV", EditorStyles.boldLabel);

            scrollParsedCSV = EditorGUILayout.BeginScrollView(scrollParsedCSV);

            string parsed = "";

            //split columns with || and TAB
            //split rows with NEW LINE
            if (data.ParseClass.ParsedCSV != null)
            {
                for (int row = 0; row < data.ParseClass.ParsedCSV.Length; row++)
                {
                    for (int column = 0; column < data.ParseClass.ParsedCSV[row].Length; column++)
                    {
                        parsed += $"|{data.ParseClass.ParsedCSV[row][column]}|\t";
                    }

                    parsed += "\n";
                }
            }

            Vector2 textSize = EditorStyles.whiteLabel.CalcSize(new GUIContent(parsed));

            //show parsed
            EditorGUILayout.LabelField(parsed, EditorStyles.whiteLabel, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true), GUILayout.MinHeight(textSize.y), GUILayout.MinWidth(textSize.x));

            EditorGUILayout.EndScrollView();
        }

        void ShowArrayParsedCSV()
        {
            //label CSV
            EditorGUILayout.LabelField("Arrays Parsed CSV", EditorStyles.boldLabel);

            scrollArraysParsedCSV = EditorGUILayout.BeginScrollView(scrollArraysParsedCSV);

            string parsed = "";

            //split items of array with /NUMBER/
            //split columns with || and TAB
            //split rows with NEW LINE
            if (data.ParseClass.ArraysParsedCSV != null)
            {
                for (int row = 0; row < data.ParseClass.ArraysParsedCSV.Length; row++)
                {
                    for (int column = 0; column < data.ParseClass.ArraysParsedCSV[row].Length; column++)
                    {
                        parsed += "|";
                        for (int itemArray = 0; itemArray < data.ParseClass.ArraysParsedCSV[row][column].Length; itemArray++)
                        {
                            parsed += $"/{data.ParseClass.ArraysParsedCSV[row][column][itemArray]}{itemArray}/";

                            //if not last array element, split from others with another TAB
                            if (itemArray < data.ParseClass.ArraysParsedCSV[row][column].Length - 1)
                                parsed += "\t";
                        }

                        parsed += "|\t";
                    }

                    parsed += "\n";
                }
            }

            Vector2 textSize = EditorStyles.whiteLabel.CalcSize(new GUIContent(parsed));

            //show parsed
            EditorGUILayout.LabelField(parsed, EditorStyles.whiteLabel, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true), GUILayout.MinHeight(textSize.y), GUILayout.MinWidth(textSize.x));

            EditorGUILayout.EndScrollView();
        }

        #endregion
    }
}