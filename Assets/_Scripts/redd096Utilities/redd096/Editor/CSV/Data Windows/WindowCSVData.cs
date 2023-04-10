using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace redd096
{
    public class WindowCSVData : ScriptableObject
    {
        const string DATANAME = "Window CSV Data.asset";

        public int IndexStruct = 0;
        public List<WindowCSVStruct> StructCSV = new List<WindowCSVStruct>();

        #region static functions

        /// <summary>
        /// Get Path to this scriptable object saved in project folder
        /// </summary>
        /// <returns></returns>
        public static string GetDataPath()
        {
            //find script folder and create scriptable object there
            string[] guids = AssetDatabase.FindAssets(typeof(WindowCSVData).ToString());
            foreach (string guid in guids)
            {
                string pathToScript = AssetDatabase.GUIDToAssetPath(guid);                                                                          //get path to the script
                string pathToFolder = pathToScript.Remove(pathToScript.LastIndexOf('/'), pathToScript.Length - pathToScript.LastIndexOf('/'));      //remove everything from last slash (script name) to get only directory path

                return Path.Combine(pathToFolder, DATANAME);
            }

            return null;
        }

        /// <summary>
        /// Load or Create Scriptable Object
        /// </summary>
        /// <returns></returns>
        public static WindowCSVData LoadData()
        {
            //try get scriptable object (data)
            WindowCSVData data = AssetDatabase.LoadAssetAtPath<WindowCSVData>(GetDataPath());

            //if there is no data, create it
            if (data == null)
            {
                data = CreateInstance<WindowCSVData>();
                data.StructCSV.Add(new WindowCSVStruct());          //create first element in the list
                AssetDatabase.CreateAsset(data, GetDataPath());
            }

            //load data
            return data;
        }

        #endregion
    }

    [System.Serializable]
    public class WindowCSVStruct
    {
        [Header("Name")]
        public string StructName = "Songs";

        [Header("Download")]
        [TextArea(2, 5)]
        public string LinkCSV = "https://docs.google.com/spreadsheets/d/e/2PACX-1vQUwVhCiPn1nvfdwMfOwU1RDmPAmllJ4tQYx7w-30kFyiSomK_GR7ebaFNnBYF6MlJEWuHbgK03kMdb/pub?gid=0&single=true&output=csv";

        [Header("Window")]
        public int IndexPath = 0;
        public string[] OptionsPath = new string[] { "Data Path", "Persistent Data Path" };

        [Header("CSV")]
        [TextArea(2, 5)]
        public string PathDownload = "";
        public string FolderName = "Commons/CSV";
        public string FileName = "Songs.csv";

        public string PathFolder => Path.Combine(PathDownload, FolderName);
        public string PathFile => Path.Combine(PathDownload, FolderName, FileName);

        public WindowCSVStruct()
        {

        }

        //create clone
        public WindowCSVStruct(WindowCSVStruct itemToClone)
        {
            StructName = itemToClone.StructName;

            LinkCSV = itemToClone.LinkCSV;

            IndexPath = itemToClone.IndexPath;
            OptionsPath = itemToClone.OptionsPath;

            PathDownload = itemToClone.PathDownload;
            FolderName = itemToClone.FolderName;
            FileName = itemToClone.FileName;
        }
    }
}