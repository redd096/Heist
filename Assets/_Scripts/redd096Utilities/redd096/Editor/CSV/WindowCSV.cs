using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.IO;

namespace redd096
{
    public class WindowCSV : EditorWindow
    {
        Vector2 scrollPosition = Vector2.zero;
        WindowCSVData data;

        /// <summary>
        /// Open Window from Editor
        /// </summary>
        [MenuItem("redd096/Open Window CSV")]
        static void OpenWindowCSV()
        {
            //open window (and set title)
            GetWindow<WindowCSV>("Window CSV");
        }

        void OnEnable()
        {
            //load data
            data = WindowCSVData.LoadData();
        }

        void OnDisable()
        {
            //set dirty to save
            EditorUtility.SetDirty(data);
        }

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.Space(15);

            //select item from list
            SelectItemFromList();

            //set link to CSV
            SetLinkCSV();

            EditorGUILayout.Space(50);

            //GUI download path
            GUIDownloadPath();

            EditorGUILayout.Space(30);

            //button abort and percentage download CSV (only if there is a download)
            if (ManageDownloadCSV.www != null)
            {
                ButtonAbortAndPercentageDownloadCSV();
            }

            EditorGUILayout.Space(60);

            //buttons delete file and delete directory
            ButtonDeleteFileAndDirectory();

            EditorGUILayout.Space(15);
            EditorGUILayout.EndScrollView();
        }

        #region window GUI API

        void SelectItemFromList()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();

            //create options with name of every item in the list
            string[] optionsList = new string[data.StructCSV.Count];
            for (int i = 0; i < data.StructCSV.Count; i++)
            {
                //check every next element in the list
                for (int j = i + 1; j < data.StructCSV.Count; j++)
                {
                    //if same name already in the list, change it to not have duplicates (because EditorGUILayout.Popup doesn't show duplicates - and now name is used also in LoadFile(string name))
                    while (data.StructCSV[i].StructName.Equals(data.StructCSV[j].StructName))
                        data.StructCSV[i].StructName += "#";
                }

                optionsList[i] = data.StructCSV[i].StructName;
            }

            //show every item
            data.IndexStruct = EditorGUILayout.Popup(data.IndexStruct, optionsList);

            EditorGUILayout.Space();

            //set name link
            data.StructCSV[data.IndexStruct].StructName = EditorGUILayout.TextField(data.StructCSV[data.IndexStruct].StructName, EditorStyles.textField);

            EditorGUILayout.Space();

            //create new item at index
            if (GUILayout.Button("Add Item"))
            {
                //clone item
                WindowCSVStruct newItem = new WindowCSVStruct(data.StructCSV[data.IndexStruct]);

                //while there is an item with same name, change it
                while (ArrayUtility.Contains(optionsList, newItem.StructName))
                    newItem.StructName += "#";

                //add item to the list and move index to the item created
                data.StructCSV.Insert(data.IndexStruct + 1, newItem);
                data.IndexStruct += 1;

                return;
            }

            //remove item at index
            if (GUILayout.Button("Remove Item"))
            {
                //only if there are others (always keep 1 in the list)
                if (data.StructCSV.Count > 1)
                {
                    //remove from the list
                    data.StructCSV.RemoveAt(data.IndexStruct);

                    //if index is not at 0, move to previous item in the list
                    if (data.IndexStruct > 0)
                        data.IndexStruct -= 1;

                    return;
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        void SetLinkCSV()
        {
            //set link to CSV
            EditorGUILayout.LabelField("Links to CSV", EditorStyles.boldLabel);
            data.StructCSV[data.IndexStruct].LinkCSV = EditorGUILayout.TextArea(data.StructCSV[data.IndexStruct].LinkCSV, EditorStyles.textArea);
        }

        void GUIDownloadPath()
        {
            //set path download
            SetPathDownload();

            EditorGUILayout.Space();

            //set folder and file name
            SetFolderAndFileName();

            EditorGUILayout.Space();

            //button to download CSV
            ButtonDownloadCSV();
        }

        void SetPathDownload()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();

            //set directory path
            data.StructCSV[data.IndexStruct].IndexPath = EditorGUILayout.Popup(data.StructCSV[data.IndexStruct].IndexPath, data.StructCSV[data.IndexStruct].OptionsPath);
            if (GUILayout.Button("Set Path"))
            {
                if (data.StructCSV[data.IndexStruct].IndexPath <= 0)
                    data.StructCSV[data.IndexStruct].PathDownload = Application.dataPath;
                else
                    data.StructCSV[data.IndexStruct].PathDownload = Application.persistentDataPath;
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            //set path download
            EditorGUILayout.LabelField("Path Download", EditorStyles.boldLabel);
            data.StructCSV[data.IndexStruct].PathDownload = EditorGUILayout.TextArea(data.StructCSV[data.IndexStruct].PathDownload, EditorStyles.textArea);
        }

        void SetFolderAndFileName()
        {
            //set folder name
            data.StructCSV[data.IndexStruct].FolderName = EditorGUILayout.TextField("Folder:", data.StructCSV[data.IndexStruct].FolderName, EditorStyles.textField);

            //set file name
            data.StructCSV[data.IndexStruct].FileName = EditorGUILayout.TextField("File:", data.StructCSV[data.IndexStruct].FileName, EditorStyles.textField);
        }

        void ButtonDownloadCSV()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();

            //button to download CSV
            if (GUILayout.Button("Download CSV"))
            {
                ManageDownloadCSV.DownloadCSV();
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
        }

        void ButtonAbortAndPercentageDownloadCSV()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();

            //button to abort
            if (GUILayout.Button("Abort Download"))
            {
                ManageDownloadCSV.AbortDownloadCSV();
                return;     //return to avoid error, because there will be no download to show percentage
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            //show percentage
            EditorGUILayout.Slider("Percentage:", ManageDownloadCSV.www.downloadProgress, 0f, 1f);

            //when complete, show error or Download Completed!
            if (ManageDownloadCSV.www.isDone)
            {
                //if (ManageDownloadCSV.www.isHttpError || ManageDownloadCSV.www.isNetworkError)
                if (ManageDownloadCSV.www.result == UnityWebRequest.Result.ProtocolError || ManageDownloadCSV.www.result == UnityWebRequest.Result.ConnectionError)
                    EditorGUILayout.LabelField("Error:", ManageDownloadCSV.www.error, EditorStyles.boldLabel);
                else
                    EditorGUILayout.LabelField("Download Completed!", EditorStyles.boldLabel);
            }

            //repaint to update editor window
            Repaint();
        }

        void ButtonDeleteFileAndDirectory()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();

            //button to delete file
            if (GUILayout.Button("Delete File"))
            {
                ManageCSV.DeleteFile();
            }

            EditorGUILayout.Space();

            //button to delete directory
            if (GUILayout.Button("Delete Directory"))
            {
                ManageCSV.DeleteDirectory();
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
        }

        #endregion
    }

    public static class ManageDownloadCSV
    {
        public static UnityWebRequest www { get; set; }

        /// <summary>
        /// Start Download from link
        /// </summary>
        public static void DownloadCSV()
        {
            //load data
            WindowCSVData data = WindowCSVData.LoadData();

            //UnityWebRequest replace old WWW
            www = UnityWebRequest.Get(data.StructCSV[data.IndexStruct].LinkCSV);
            UnityWebRequestAsyncOperation request = www.SendWebRequest();

            //wait download
            request.completed += OnCompleteDownload;

            //while (!request.isDone) { }
        }

        /// <summary>
        /// If downloading, Abort it
        /// </summary>
        public static void AbortDownloadCSV()
        {
            //if there is a download, abort it
            if (www != null)
            {
                www.Abort();
                www = null;
            }
        }

        /// <summary>
        /// On complete download, Save CSV
        /// </summary>
        /// <param name="asyncOperation"></param>
        static void OnCompleteDownload(AsyncOperation asyncOperation)
        {
            //do only if there are not errors
            //if (www.isHttpError || www.isNetworkError)
            if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError)
                return;

            //save CSV
            ManageCSV.SaveFile(www.downloadHandler.text);
        }
    }

    public static class ManageCSV
    {
        /// <summary>
        /// Load File from folder
        /// </summary>
        public static string LoadFile(WindowCSVStruct structCSV)
        {
            //if there is no file, return null
            if (File.Exists(structCSV.PathFile) == false)
            {
                Debug.Log("File not found: " + structCSV.PathFile);
                return null;
            }

            //create stream at file position
            StreamReader reader = new StreamReader(structCSV.PathFile);

            //then load from file position as value, and close stream
            string value = reader.ReadToEnd();
            reader.Close();

            return value;
        }

        /// <summary>
        /// Load File from folder using selected one in the window
        /// </summary>
        public static string LoadFile()
        {
            //load data
            WindowCSVData data = WindowCSVData.LoadData();

            //load file using index in data
            return LoadFile(data.StructCSV[data.IndexStruct]);
        }

        /// <summary>
        /// Load File from folder using index
        /// </summary>
        public static string LoadFile(int structIndex)
        {
            //load data
            WindowCSVData data = WindowCSVData.LoadData();

            //load file using index
            return LoadFile(data.StructCSV[structIndex]);
        }

        /// <summary>
        /// Load File from folder looking in the list using StructName
        /// </summary>
        public static string LoadFile(string structName)
        {
            //load data
            WindowCSVData data = WindowCSVData.LoadData();

            foreach (WindowCSVStruct structCSV in data.StructCSV)
            {
                //if found name
                if (structCSV.StructName.Equals(structName))
                    return LoadFile(structCSV);
            }

            return null;
        }

        /// <summary>
        /// Save downloaded File in folder
        /// </summary>
        /// <param name="value"></param>
        public static void SaveFile(string value)
        {
            //load data
            WindowCSVData data = WindowCSVData.LoadData();

            //if there is no directory, create it
            if (Directory.Exists(data.StructCSV[data.IndexStruct].PathFolder) == false)
            {
                Directory.CreateDirectory(data.StructCSV[data.IndexStruct].PathFolder);
            }

            //create stream to file position
            StreamWriter writer = new StreamWriter(data.StructCSV[data.IndexStruct].PathFile);

            //then save value to file position, and close stream
            writer.Write(value);
            writer.Close();
        }

        /// <summary>
        /// Delete file
        /// </summary>
        public static void DeleteFile()
        {
            //load data
            WindowCSVData data = WindowCSVData.LoadData();

            //check there is a file
            if (File.Exists(data.StructCSV[data.IndexStruct].PathFile) == false)
            {
                Debug.Log("File not found: " + data.StructCSV[data.IndexStruct].PathFile);
                return;
            }

            //delete file
            File.Delete(data.StructCSV[data.IndexStruct].PathFile);
            Debug.Log("File deleted successfully: " + data.StructCSV[data.IndexStruct].PathFile);
        }

        /// <summary>
        /// Delete directory with every file
        /// </summary>
        public static void DeleteDirectory()
        {
            //load data
            WindowCSVData data = WindowCSVData.LoadData();

            //check there is a directory
            if (Directory.Exists(data.StructCSV[data.IndexStruct].PathFolder) == false)
            {
                Debug.Log("Directory not found: " + data.StructCSV[data.IndexStruct].PathFolder);
                return;
            }

            //delete directory
            Directory.Delete(data.StructCSV[data.IndexStruct].PathFolder, true);
            Debug.Log("Directory deleted successfully: " + data.StructCSV[data.IndexStruct].PathFolder);
        }
    }
}