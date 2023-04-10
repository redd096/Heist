using UnityEngine;
using System.IO;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace redd096
{
    enum SaveFolder
    {
        persistentDataPath, gameFolder, downloadFolder, nothing
    }

    [AddComponentMenu("redd096/Singletons/Save Manager")]
    [DefaultExecutionOrder(-200)]
    public class SaveManager : Singleton<SaveManager>
    {
        [Header("Data Directory")]
        [SerializeField] SaveFolder saveFolder = SaveFolder.persistentDataPath;
        [SerializeField] string directoryName = "Saves";

        [Header("Optimization")]
        [SerializeField] bool saveAsync = true;
        [Tooltip("Load everything at start and put in a dictionary, so in game just read dictionary")][SerializeField] bool usePreload = true;

        [Header("Debug Mode")]
        public bool ShowDebugLogs = false;

        public string DirectoryName => directoryName;
        public string PathDirectory
        {
            get
            {
                if (saveFolder == SaveFolder.persistentDataPath)
                    return Path.Combine(Application.persistentDataPath, directoryName);     //return persistent data path + directory path
                else if (saveFolder == SaveFolder.gameFolder)
                    return Path.Combine(Application.dataPath, directoryName);               //return game folder path + directory path
                else if (saveFolder == SaveFolder.downloadFolder)
                    return Path.Combine(GetKnownFolders.GetDownloadPath(), directoryName);  //return download folder + directory path
                else
                    return directoryName;                                                   //return only directory path
            }
        }

        //used when "Use Preload" is enabled -> key: file name without extension, value: json
        private Dictionary<string, string> savesJson = new Dictionary<string, string>();
        //used when create one single file with more variables -> key: file name without extension, value: another dictionary with key: variableName, value: json
        private Dictionary<string, Dictionary<string, string>> filesWithMoreVariables = new Dictionary<string, Dictionary<string, string>>();
        //system to use for save and load
        private ISaveLoadSystem saveLoadSystem;
        //string to identify files with more variables
        private const string FWMV_STRING = "redd096FWMV";

        protected override void Awake()
        {
            base.Awake();

            //if this is the instance
            if (instance == this)
            {
                //set save load system based on platform
#if UNITY_STEAM
                saveLoadSystem = new SaveLoadSystem_Steam();
#elif UNITY_STANDALONE
                saveLoadSystem = new SaveLoadSystem_PC();
#elif UNITY_WEBGL
                saveLoadSystem = new SaveLoadSystem_WebGL();
#elif UNITY_IOS || UNITY_ANDROID
                saveLoadSystem = new SaveLoadSystem_Mobile();
#elif UNITY_GAMECORE
                saveLoadSystem = new SaveLoadSystem_GameCore();
#elif UNITY_PS4
                saveLoadSystem = new SaveLoadSystem_PS4();
#elif UNITY_PS5
                saveLoadSystem = new SaveLoadSystem_PS5();
#elif UNITY_SWITCH
                saveLoadSystem = new SaveLoadSystem_Switch();
#else
                Debug.LogError("SaveManager doesn't have a SaveLoadSystem for this platform");
#endif

                //initialize SaveLoadSystem (e.g. call Preload)
                StartCoroutine(saveLoadSystem.Initialize(usePreload));
            }
        }

        /// <summary>
        /// Called from Preload, to set dictionary (used when "Use Preload" is enabled)
        /// </summary>
        public void FinishPreload(Dictionary<string, string> jsons)
        {
            savesJson = new Dictionary<string, string>(jsons);

            //add also file with more variables to their dictionary
            foreach (string fileName in savesJson.Keys) ReadFileWithMoreVariables(fileName, savesJson[fileName]);
        }

        private void ReadFileWithMoreVariables(string fileName, string fileString)
        {
            if (fileString == null || fileString.StartsWith(FWMV_STRING) == false)  //redd096 File With More Variables
            {
                if (ShowDebugLogs)
                    Debug.Log("Incorrect file: " + saveLoadSystem.GetPathFile(fileName));

                return;
            }

            //add file to dictionary
            if (filesWithMoreVariables.ContainsKey(fileName) == false)
                filesWithMoreVariables.Add(fileName, new Dictionary<string, string>());

            string[] lines = fileString.Split('\n');
            for (int i = 2; i < lines.Length; i += 2)   //skip 0 because is our custom string, then move by 2 and read variable name and value
            {
                //first line is variable name
                if (filesWithMoreVariables[fileName].ContainsKey(lines[i - 1]) == false)
                    filesWithMoreVariables[fileName].Add(lines[i - 1], "");

                //and second line is json
                if (i < lines.Length)
                    filesWithMoreVariables[fileName][lines[i - 1]] = lines[i];
            }
        }

        #region classes for save and load

        /// <summary>
        /// Save/Load files
        /// </summary>
        public class Generic
        {
            /// <summary>
            /// Save in directory/key.json
            /// </summary>
            /// <param name="key">file name</param>
            /// <param name="json">file value</param>
            public static void Save(string key, string json)
            {
                //add to dictionary
                if (instance.savesJson.ContainsKey(key))
                    instance.savesJson[key] = json;
                else
                    instance.savesJson.Add(key, json);

                //save async or normal
                if (instance.saveAsync)
                {
                    _ = instance.saveLoadSystem.SaveAsync(key, json);
                }
                else
                {
                    instance.saveLoadSystem.Save(key, json);
                }
            }

            /// <summary>
            /// Load from directory/key.json
            /// </summary>
            /// <param name="key">file name</param>
            /// <returns></returns>
            public static string Load(string key)
            {
                //load from dictionary if use preload
                if (instance.usePreload)
                {
                    if (instance.savesJson.ContainsKey(key))
                        return instance.savesJson[key];

                    if (instance.ShowDebugLogs)
                        Debug.Log("Save file not found: " + instance.saveLoadSystem.GetPathFile(key));
                }
                //else load normally from file
                else
                {
                    return instance.saveLoadSystem.Load(key);
                }

                return default;
            }

            /// <summary>
            /// Check if there is a file with this name
            /// </summary>
            /// <param name="key">file name</param>
            /// <returns></returns>
            public static bool HasKey(string key)
            {
                //if use preload, check if there is inside dictionary
                if (instance.usePreload)
                {
                    return instance.savesJson.ContainsKey(key);
                }
                //else, check if exists this file
                else
                {
                    return instance.saveLoadSystem.FileExists(key);
                }
            }

            /// <summary>
            /// Delete a file
            /// </summary>
            /// <param name="key">file name</param>
            public static void Delete(string key)
            {
                instance.saveLoadSystem.DeleteData(key);

                //delete also from dictionaries
                if (instance.savesJson.ContainsKey(key))
                    instance.savesJson.Remove(key);
                if (instance.filesWithMoreVariables.ContainsKey(key))   //also from filesWithMoreVariables to be sure
                    instance.filesWithMoreVariables.Remove(key);
            }

            /// <summary>
            /// Delete every file
            /// </summary>
            public static void DeleteAll()
            {
                instance.saveLoadSystem.DeleteAll();

                //clear also dictionaries
                instance.savesJson.Clear();
                instance.filesWithMoreVariables.Clear();                //also filesWithMoreVariables to be sure
            }
        }

        /// <summary>
        /// Save/Load files with more variables inside. These will be saved in a dictionary and loaded from it
        /// </summary>
        public class GenericFWMV
        {
            /// <summary>
            /// Save value in a dictionary. Use SaveOnDisk to save on disk
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="key">variable name</param>
            /// <param name="json">variable value</param>
            public static void Save(string fileName, string key, string json)
            {
                //if there is already a file, set or add variable
                if (instance.filesWithMoreVariables.ContainsKey(fileName))
                {
                    if (instance.filesWithMoreVariables[fileName].ContainsKey(key))
                        instance.filesWithMoreVariables[fileName][key] = json;
                    else
                        instance.filesWithMoreVariables[fileName].Add(key, json);
                }
                //else add file name and variable
                else
                {
                    instance.filesWithMoreVariables.Add(fileName, new Dictionary<string, string>());
                    instance.filesWithMoreVariables[fileName].Add(key, json);
                }
            }

            /// <summary>
            /// Save on disk every file setted in dictionary
            /// </summary>
            public static void SaveOnDisk()
            {
                //save every file to disk
                foreach (string fileName in instance.filesWithMoreVariables.Keys)
                {
                    SaveOnDisk(fileName);
                }
            }

            /// <summary>
            /// Save file on disk with every variable setted in dictionary
            /// </summary>
            public static void SaveOnDisk(string fileName)
            {
                //add this string as first line, to identify the files
                System.Text.StringBuilder fileString = new System.Text.StringBuilder($"{FWMV_STRING}\n");   //redd096 File With More Variables
                if (instance.filesWithMoreVariables.ContainsKey(fileName))
                {
                    //create a string "1stVarName\n1stJson\n2ndVarName\n2ndJson\n..."
                    foreach (string variableName in instance.filesWithMoreVariables[fileName].Keys)
                    {
                        fileString.Append(variableName).Append("\n").Append(instance.filesWithMoreVariables[fileName][variableName]).Append("\n");
                    }
                }

                //save
                Generic.Save(fileName, fileString.ToString());
            }

            /// <summary>
            /// Load variable from dictionary. Use LoadFromDisk or enable "Use Preload" to fill dictionary
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="key">variable name</param>
            /// <returns></returns>
            public static string Load(string fileName, string key)
            {
                if (instance.filesWithMoreVariables.ContainsKey(fileName))
                {
                    //get variable from dictionary
                    if (instance.filesWithMoreVariables[fileName].ContainsKey(key))
                        return instance.filesWithMoreVariables[fileName][key];
                    else if (instance.ShowDebugLogs)
                        Debug.Log("Variable " + key + " not found in: " + instance.saveLoadSystem.GetPathFile(fileName));
                }
                else if (instance.ShowDebugLogs)
                    Debug.Log("File not found: " + instance.saveLoadSystem.GetPathFile(fileName));

                return default;
            }

            /// <summary>
            /// Load from disk and set in dictionary
            /// </summary>
            /// <param name="fileName"></param>
            public static void LoadFromDisk(string fileName)
            {
                //load from disk
                string fileString = Generic.Load(fileName);

                //add to dictionary
                instance.ReadFileWithMoreVariables(fileName, fileString);
            }

            /// <summary>
            /// Check if there is a file with this name
            /// </summary>
            /// <param name="fileName"></param>
            /// <returns></returns>
            public static bool HasFile(string fileName)
            {
                //load and add to dictionary, if don't use preload
                if (instance.usePreload == false)
                    LoadFromDisk(fileName);

                //return if there is a file with this name
                return instance.filesWithMoreVariables.ContainsKey(fileName);
            }

            /// <summary>
            /// Check if there is a variable with this name in this file
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="key">variable name</param>
            /// <returns></returns>
            public static bool HasVariable(string fileName, string key)
            {
                //load and add to dictionary, if don't use preload
                if (instance.usePreload == false)
                    LoadFromDisk(fileName);

                //check there is this file and has this variable
                if (instance.filesWithMoreVariables.ContainsKey(fileName) && instance.filesWithMoreVariables[fileName].ContainsKey(key))
                    return true;

                return false;
            }

            /// <summary>
            /// Delete a file
            /// </summary>
            /// <param name="fileName"></param>
            public static void DeleteFile(string fileName)
            {
                Generic.Delete(fileName);
            }

            /// <summary>
            /// Delete a variable from dictionary
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="key">variable name</param>
            public static void DeleteVariable(string fileName, string key)
            {
                if (instance.filesWithMoreVariables.ContainsKey(fileName))
                {
                    //remove variable from dictionary
                    if (instance.filesWithMoreVariables[fileName].ContainsKey(key))
                        instance.filesWithMoreVariables[fileName].Remove(key);
                    else if (instance.ShowDebugLogs)
                        Debug.Log("Variable " + key + " not found in: " + instance.saveLoadSystem.GetPathFile(fileName));
                }
                else if (instance.ShowDebugLogs)
                    Debug.Log("File not found: " + instance.saveLoadSystem.GetPathFile(fileName));
            }

            /// <summary>
            /// Return a list of every variable name saved in this file
            /// </summary>
            /// <param name="fileName"></param>
            /// <returns></returns>
            public static List<string> GetEveryVariableInThisFile(string fileName)
            {
                List<string> vars = new List<string>();

                //add every variable name in this file
                if (instance.filesWithMoreVariables.ContainsKey(fileName))
                {
                    foreach (string variableName in instance.filesWithMoreVariables[fileName].Keys)
                        vars.Add(variableName);
                }

                return vars;
            }
        }

        /// <summary>
        /// Save/Load files like with PlayerPrefs. Use this to save in a single file with more variables inside. These will be saved in a dictionary and loaded from it
        /// </summary>
        public class PlayerPrefsFWMV
        {
            /// <summary>
            /// Save value in a dictionary. Use SaveOnDisk to save on disk
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="key">variable name</param>
            /// <param name="value"></param>
            /// <param name="automaticSaveOnDisk">call automatically SaveOnDisk</param>
            public static void SetInt(string fileName, string key, int value, bool automaticSaveOnDisk = true)
            {
                GenericFWMV.Save(fileName, key, value.ToString());
                if (automaticSaveOnDisk) GenericFWMV.SaveOnDisk(fileName);
            }

            /// <summary>
            /// Load value from dictionary. If there is no variable, return defaultValue
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="key">variable name</param>
            /// <param name="defaultValue"></param>
            /// <returns></returns>
            public static int GetInt(string fileName, string key, int defaultValue)
            {
                //load and add to dictionary, if don't use preload
                if (instance.usePreload == false)
                    GenericFWMV.LoadFromDisk(fileName);

                //check if there is a file saved to return
                if (HasKey(fileName, key))
                    return int.Parse(GenericFWMV.Load(fileName, key));

                //else return default value
                return defaultValue;
            }

            /// <summary>
            /// Load value from dictionary
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="key">variable name</param>
            /// <returns></returns>
            public static int GetInt(string fileName, string key)
            {
                //load and add to dictionary, if don't use preload
                if (instance.usePreload == false)
                    GenericFWMV.LoadFromDisk(fileName);

                return int.Parse(GenericFWMV.Load(fileName, key));
            }

            /// <summary>
            /// Save value in a dictionary. Use SaveOnDisk to save on disk
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="key">variable name</param>
            /// <param name="value"></param>
            /// <param name="automaticSaveOnDisk">call automatically SaveOnDisk</param>
            public static void SetFloat(string fileName, string key, float value, bool automaticSaveOnDisk = true)
            {
                GenericFWMV.Save(fileName, key, value.ToString());
                if (automaticSaveOnDisk) GenericFWMV.SaveOnDisk(fileName);
            }

            /// <summary>
            /// Load value from dictionary. If there is no variable, return defaultValue
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="key">variable name</param>
            /// <param name="defaultValue"></param>
            /// <returns></returns>
            public static float GetFloat(string fileName, string key, float defaultValue)
            {
                //load and add to dictionary, if don't use preload
                if (instance.usePreload == false)
                    GenericFWMV.LoadFromDisk(fileName);

                //check if there is a file saved to return
                if (HasKey(fileName, key))
                    return float.Parse(GenericFWMV.Load(fileName, key));

                //else return default value
                return defaultValue;
            }

            /// <summary>
            /// Load value from dictionary
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="key">variable name</param>
            /// <returns></returns>
            public static float GetFloat(string fileName, string key)
            {
                //load and add to dictionary, if don't use preload
                if (instance.usePreload == false)
                    GenericFWMV.LoadFromDisk(fileName);

                return float.Parse(GenericFWMV.Load(fileName, key));
            }

            /// <summary>
            /// Save value in a dictionary. Use SaveOnDisk to save on disk
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="key">variable name</param>
            /// <param name="value"></param>
            /// <param name="automaticSaveOnDisk">call automatically SaveOnDisk</param>
            public static void SetString(string fileName, string key, string value, bool automaticSaveOnDisk = true)
            {
                GenericFWMV.Save(fileName, key, value);
                if (automaticSaveOnDisk) GenericFWMV.SaveOnDisk(fileName);
            }

            /// <summary>
            /// Load value from dictionary. If there is no variable, return defaultValue
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="key">variable name</param>
            /// <param name="defaultValue"></param>
            /// <returns></returns>
            public static string GetString(string fileName, string key, string defaultValue)
            {
                //load and add to dictionary, if don't use preload
                if (instance.usePreload == false)
                    GenericFWMV.LoadFromDisk(fileName);

                //check if there is a file saved to return
                if (HasKey(fileName, key))
                    return GenericFWMV.Load(fileName, key);

                //else return default value
                return defaultValue;
            }

            /// <summary>
            /// Load value from dictionary
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="key">variable name</param>
            /// <returns></returns>
            public static string GetString(string fileName, string key)
            {
                //load and add to dictionary, if don't use preload
                if (instance.usePreload == false)
                    GenericFWMV.LoadFromDisk(fileName);

                return GenericFWMV.Load(fileName, key);
            }

            /// <summary>
            /// Save value in a dictionary. Use SaveOnDisk to save on disk
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="key">variable name</param>
            /// <param name="value"></param>
            /// <param name="automaticSaveOnDisk">call automatically SaveOnDisk</param>
            public static void SetBool(string fileName, string key, bool value, bool automaticSaveOnDisk = true)
            {
                GenericFWMV.Save(fileName, key, value.ToString());
                if (automaticSaveOnDisk) GenericFWMV.SaveOnDisk(fileName);
            }

            /// <summary>
            /// Load value from dictionary. If there is no variable, return defaultValue
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="key">variable name</param>
            /// <param name="defaultValue"></param>
            /// <returns></returns>
            public static bool GetBool(string fileName, string key, bool defaultValue)
            {
                //load and add to dictionary, if don't use preload
                if (instance.usePreload == false)
                    GenericFWMV.LoadFromDisk(fileName);

                //check if there is a file saved to return
                if (HasKey(fileName, key))
                    return bool.Parse(GenericFWMV.Load(fileName, key));

                //else return default value
                return defaultValue;
            }

            /// <summary>
            /// Load value from dictionary
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="key">variable name</param>
            /// <returns></returns>
            public static bool GetBool(string fileName, string key)
            {
                //load and add to dictionary, if don't use preload
                if (instance.usePreload == false)
                    GenericFWMV.LoadFromDisk(fileName);

                return bool.Parse(GenericFWMV.Load(fileName, key));
            }

            /// <summary>
            /// Check if there is a file with this name
            /// </summary>
            /// <param name="fileName"></param>
            /// <returns></returns>
            public static bool HasFile(string fileName)
            {
                return GenericFWMV.HasFile(fileName);
            }

            /// <summary>
            /// Check if there is a variable with this name in this file
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="key">variable name</param>
            /// <returns></returns>
            public static bool HasKey(string fileName, string key)
            {
                return GenericFWMV.HasVariable(fileName, key);
            }

            /// <summary>
            /// Delete a variable from dictionary
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="key">variable name</param>
            public static void DeleteKey(string fileName, string key)
            {
                GenericFWMV.DeleteVariable(fileName, key);
            }

            /// <summary>
            /// Save on disk every file setted in dictionary
            /// </summary>
            public static void Save()
            {
                GenericFWMV.SaveOnDisk();
            }

            /// <summary>
            /// Save file on disk with every variable setted in dictionary
            /// </summary>
            public static void Save(string fileName)
            {
                GenericFWMV.SaveOnDisk(fileName);
            }
        }

        /// <summary>
        /// Save/Load files like with PlayerPrefs
        /// </summary>
        public class PlayerPrefs
        {
            /// <summary>
            /// Save value
            /// </summary>
            /// <param name="key">file name</param>
            /// <param name="value"></param>
            public static void SetInt(string key, int value)
            {
                Generic.Save(key, value.ToString());
            }

            /// <summary>
            /// Load value. If there is no file, return defaultValue
            /// </summary>
            /// <param name="key">file name</param>
            /// <param name="defaultValue"></param>
            /// <returns></returns>
            public static int GetInt(string key, int defaultValue)
            {
                //check if there is a file saved to return
                if (HasKey(key))
                    return int.Parse(Generic.Load(key));

                //else return default value
                return defaultValue;
            }

            /// <summary>
            /// Load value
            /// </summary>
            /// <param name="key">file name</param>
            /// <returns></returns>
            public static int GetInt(string key)
            {
                return int.Parse(Generic.Load(key));
            }

            /// <summary>
            /// Save value
            /// </summary>
            /// <param name="key">file name</param>
            /// <param name="value"></param>
            public static void SetFloat(string key, float value)
            {
                Generic.Save(key, value.ToString());
            }

            /// <summary>
            /// Load value. If there is no file, return defaultValue
            /// </summary>
            /// <param name="key">file name</param>
            /// <param name="defaultValue"></param>
            /// <returns></returns>
            public static float GetFloat(string key, float defaultValue)
            {
                //check if there is a file saved to return
                if (HasKey(key))
                    return float.Parse(Generic.Load(key));

                //else return default value
                return defaultValue;
            }

            /// <summary>
            /// Load value
            /// </summary>
            /// <param name="key">file name</param>
            /// <returns></returns>
            public static float GetFloat(string key)
            {
                return float.Parse(Generic.Load(key));
            }

            /// <summary>
            /// Save value
            /// </summary>
            /// <param name="key">file name</param>
            /// <param name="value"></param>
            public static void SetString(string key, string value)
            {
                Generic.Save(key, value);
            }

            /// <summary>
            /// Load value. If there is no file, return defaultValue
            /// </summary>
            /// <param name="key">file name</param>
            /// <param name="defaultValue"></param>
            /// <returns></returns>
            public static string GetString(string key, string defaultValue)
            {
                //check if there is a file saved to return
                if (HasKey(key))
                    return Generic.Load(key);

                //else return default value
                return defaultValue;
            }

            /// <summary>
            /// Load value
            /// </summary>
            /// <param name="key">file name</param>
            /// <returns></returns>
            public static string GetString(string key)
            {
                return Generic.Load(key);
            }

            /// <summary>
            /// Save value
            /// </summary>
            /// <param name="key">file name</param>
            /// <param name="value"></param>
            public static void SetBool(string key, bool value)
            {
                Generic.Save(key, value.ToString());
            }

            /// <summary>
            /// Load value. If there is no file, return defaultValue
            /// </summary>
            /// <param name="key">file name</param>
            /// <param name="defaultValue"></param>
            /// <returns></returns>
            public static bool GetBool(string key, bool defaultValue)
            {
                //check if there is a file saved to return
                if (HasKey(key))
                    return bool.Parse(Generic.Load(key));

                //else return default value
                return defaultValue;
            }

            /// <summary>
            /// Load value
            /// </summary>
            /// <param name="key">file name</param>
            /// <returns></returns>
            public static bool GetBool(string key)
            {
                return bool.Parse(Generic.Load(key));
            }

            /// <summary>
            /// Check if there is a file with this name
            /// </summary>
            /// <param name="key">file name</param>
            /// <returns></returns>
            public static bool HasKey(string key)
            {
                return Generic.HasKey(key);
            }

            /// <summary>
            /// If there is a file with this name, delete it
            /// </summary>
            /// <param name="key">file name</param>
            public static void DeleteKey(string key)
            {
                Generic.Delete(key);
            }

            /// <summary>
            /// Delete every file
            /// </summary>
            public static void DeleteAll()
            {
                Generic.DeleteAll();
            }
        }

        #endregion
    }

    #region example save class

    [System.Serializable]
    public class ExampleClassToSave
    {
        public int test;
        public string[] testArray;

        public ExampleClassToSave(int test, string[] testArray)
        {
            this.test = test;
            this.testArray = testArray;
        }

        public void Serialize() => SaveManager.Generic.Save("Example", JsonUtility.ToJson(this));
        public ExampleClassToSave Deserialize() => JsonUtility.FromJson<ExampleClassToSave>(SaveManager.Generic.Load("Example"));
    }

    #endregion

    #region custom editor

#if UNITY_EDITOR

    [CustomEditor(typeof(SaveManager))]
    public class SaveManagerEditor : Editor
    {
        SaveManager saveManager;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            //get ref
            saveManager = target as SaveManager;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(5);

            //header
            EditorGUILayout.LabelField("Path Directory:");

            EditorGUILayout.Space(-15);

            //show path directory
            EditorGUILayout.SelectableLabel(saveManager.PathDirectory);

            EditorGUILayout.Space(10);

            //button open folder
            if (GUILayout.Button("Open Saves Folder"))
            {
                OpenFolder();
            }

            //button delete folder
            if (GUILayout.Button("Delete Saves Folder"))
            {
                DeleteAll();
            }

            EditorGUILayout.EndVertical();
        }

        void OpenFolder()
        {
            //use this SaveLoadSystem, cause instance is not setted
            if (saveManager == null)
                return;

            //if directory doesn't exists, create it
            if (Directory.Exists(saveManager.PathDirectory) == false)
                Directory.CreateDirectory(saveManager.PathDirectory);

            //open directory
            EditorUtility.RevealInFinder(saveManager.PathDirectory);
        }

        void DeleteAll()
        {
            //use this SaveLoadSystem, cause instance is not setted
            if (saveManager == null)
                return;

            //check there is a directory - delete directory, else debug log
            if (Directory.Exists(saveManager.PathDirectory))
                Directory.Delete(saveManager.PathDirectory, true);
            else
                Debug.Log("Directory not found: " + saveManager.PathDirectory);
        }
    }

#endif

    #endregion
}