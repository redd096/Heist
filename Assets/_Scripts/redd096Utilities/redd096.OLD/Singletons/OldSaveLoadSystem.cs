using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace redd096.OLD
{
    #region custom editor

#if UNITY_EDITOR

    using UnityEditor;

    [CustomEditor(typeof(OldSaveLoadSystem))]
    public class OldSaveLoadSystemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            OldSaveLoadSystem saveLoad = target as OldSaveLoadSystem;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(5);

            //header
            EditorGUILayout.LabelField("Path Directory:");

            EditorGUILayout.Space(-15);

            //show path directory
            EditorGUILayout.SelectableLabel(saveLoad.PathDirectory);

            EditorGUILayout.Space(10);

            //button
            if (GUILayout.Button("Delete Saves"))
            {
                DeleteAll();
            }

            EditorGUILayout.EndVertical();
        }

        void DeleteAll()
        {
            //use this SaveLoadSystem, cause instance is not setted
            OldSaveLoadSystem saveLoadSystem = target as OldSaveLoadSystem;
            if (saveLoadSystem == null)
                return;

            //check there is a directory
            if (Directory.Exists(saveLoadSystem.PathDirectory) == false)
            {
                if (saveLoadSystem.ShowDebugLogs)
                    Debug.Log("Directory not found: " + saveLoadSystem.PathDirectory);

                return;
            }

            //delete directory
            Directory.Delete(saveLoadSystem.PathDirectory, true);
        }
    }

#endif

    #endregion

    #region save class

    [System.Serializable]
    public class OldExampleClassToSave
    {
        public int test;
        public string[] testArray;

        public OldExampleClassToSave(int test, string[] testArray)
        {
            this.test = test;
            this.testArray = testArray;
        }
    }

    #endregion

    public enum SaveFolder
    {
        persistentDataPath, gameFolder, nothing
    }

    [AddComponentMenu("redd096/.OLD/Singletons/Old Save and Load System")]
    [DefaultExecutionOrder(-200)]
    public class OldSaveLoadSystem : Singleton<OldSaveLoadSystem>
    {
        [Header("Data Directory")]
        [SerializeField] SaveFolder saveFolder = SaveFolder.persistentDataPath;
        [SerializeField] string directoryName = "Saves";

        [Header("Debug Mode")]
        public bool ShowDebugLogs = false;

        public string PathDirectory
        {
            get
            {
                if (saveFolder == SaveFolder.persistentDataPath)
                    return Path.Combine(Application.persistentDataPath, directoryName);     //return persistent data path + directory path
                else if (saveFolder == SaveFolder.gameFolder)
                    return Path.Combine(Application.dataPath, directoryName);               //return game folder path + directory path
                else
                    return directoryName;                                                   //return only directory path
            }
        }
    }

    public static class OldSaveLoadJSON
    {
        /// <summary>
        /// Get path to the file (directory path + name of the file (key) + format (.json))
        /// </summary>
        /// <param name="key">Name of the file</param>
        /// <returns></returns>
        public static string GetPathFile(string key)
        {
            //directory path + name of the file (key) + format (.json)
            return Path.Combine(OldSaveLoadSystem.instance.PathDirectory, key + ".json");
        }

        /// <summary>
        /// Save class in directory/key.json
        /// </summary>
        /// <param name="key">Name of the file</param>
        /// <param name="value">Value to save</param>
        public static void Save<T>(string key, T value)
        {
            //if there is no directory, create it
            if (Directory.Exists(OldSaveLoadSystem.instance.PathDirectory) == false)
            {
                Directory.CreateDirectory(OldSaveLoadSystem.instance.PathDirectory);
            }

            //value to json, then save file
            string jsonValue = JsonUtility.ToJson(value);
            File.WriteAllText(GetPathFile(key), jsonValue);
        }

        /// <summary>
        /// Load class from directory/key.json
        /// </summary>
        /// <param name="key">name of the file</param>
        public static T Load<T>(string key) where T : class
        {
            //if there is no file, return null
            if (File.Exists(GetPathFile(key)) == false)
            {
                if (OldSaveLoadSystem.instance.ShowDebugLogs)
                    Debug.Log("Save file not found: " + GetPathFile(key));

                return null;
            }

            //load file, then json to value
            string jsonValue = File.ReadAllText(GetPathFile(key));
            return JsonUtility.FromJson<T>(jsonValue);
        }

        /// <summary>
        /// Save class in directory/key.json
        /// </summary>
        /// <param name="key">Name of the file</param>
        /// <param name="value">Value to save</param>
        public static void Save(string key, object value)
        {
            //if there is no directory, create it
            if (Directory.Exists(OldSaveLoadSystem.instance.PathDirectory) == false)
            {
                Directory.CreateDirectory(OldSaveLoadSystem.instance.PathDirectory);
            }

            //value to json, then save file
            string jsonValue = JsonUtility.ToJson(value);
            File.WriteAllText(GetPathFile(key), jsonValue);
        }

        /// <summary>
        /// Load class from directory/key.json
        /// </summary>
        /// <param name="key">name of the file</param>
        /// <param name="type">type of the value to load</param>
        /// <returns></returns>
        public static object Load(string key, System.Type type)
        {
            //if there is no file, return null
            if (File.Exists(GetPathFile(key)) == false)
            {
                if (OldSaveLoadSystem.instance.ShowDebugLogs)
                    Debug.Log("Save file not found: " + GetPathFile(key));

                return null;
            }

            //load file, then json to value
            string jsonValue = File.ReadAllText(GetPathFile(key));
            return JsonUtility.FromJson(jsonValue, type);
        }

        /// <summary>
        /// Delete a file
        /// </summary>
        /// <param name="key">Name of the file</param>
        public static void DeleteData(string key)
        {
            //check there is a file
            if (File.Exists(GetPathFile(key)) == false)
            {
                if (OldSaveLoadSystem.instance.ShowDebugLogs)
                    Debug.Log("Save file not found: " + GetPathFile(key));

                return;
            }

            //delete file
            File.Delete(GetPathFile(key));
        }

        /// <summary>
        /// Delete directory with every file
        /// </summary>
        public static void DeleteAll()
        {
            //check there is a directory
            if (Directory.Exists(OldSaveLoadSystem.instance.PathDirectory) == false)
            {
                if (OldSaveLoadSystem.instance.ShowDebugLogs)
                    Debug.Log("Directory not found: " + OldSaveLoadSystem.instance.PathDirectory);

                return;
            }

            //delete directory
            Directory.Delete(OldSaveLoadSystem.instance.PathDirectory, true);
        }
    }

    public static class OldSaveLoadBinary
    {
        /// <summary>
        /// Get path to the file (directory path + name of the file (key) + format (.bin))
        /// </summary>
        /// <param name="key">Name of the file</param>
        /// <returns></returns>
        public static string GetPathFile(string key)
        {
            //directory path + name of the file (key) + format (.bin)
            return Path.Combine(OldSaveLoadSystem.instance.PathDirectory, key + ".bin");
        }

        /// <summary>
        /// Save class in directory/key.bin
        /// </summary>
        /// <param name="key">Name of the file</param>
        /// <param name="value">Value to save</param>
        public static void Save<T>(string key, T value)
        {
            //if there is no directory, create it
            if (Directory.Exists(OldSaveLoadSystem.instance.PathDirectory) == false)
            {
                Directory.CreateDirectory(OldSaveLoadSystem.instance.PathDirectory);
            }

            //create stream at file position
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(GetPathFile(key), FileMode.Create);

            //then save value to file position, and close stream
            formatter.Serialize(stream, value);
            stream.Close();
        }

        /// <summary>
        /// Load class from directory/key.bin
        /// </summary>
        /// <param name="key">name of the file</param>
        public static T Load<T>(string key) where T : class
        {
            //if there is no file, return null
            if (File.Exists(GetPathFile(key)) == false)
            {
                if (OldSaveLoadSystem.instance.ShowDebugLogs)
                    Debug.Log("Save file not found: " + GetPathFile(key));

                return null;
            }

            //create stream at file position
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(GetPathFile(key), FileMode.Open);

            //then load from file position as value, and close stream
            T value = formatter.Deserialize(stream) as T;
            stream.Close();

            return value;
        }

        /// <summary>
        /// Save class in directory/key.bin
        /// </summary>
        /// <param name="key">Name of the file</param>
        /// <param name="value">Value to save</param>
        public static void Save(string key, object value)
        {
            //if there is no directory, create it
            if (Directory.Exists(OldSaveLoadSystem.instance.PathDirectory) == false)
            {
                Directory.CreateDirectory(OldSaveLoadSystem.instance.PathDirectory);
            }

            //create stream at file position
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(GetPathFile(key), FileMode.Create);

            //then save value to file position, and close stream
            formatter.Serialize(stream, value);
            stream.Close();
        }

        /// <summary>
        /// Load class from directory/key.bin
        /// </summary>
        /// <param name="key">name of the file</param>
        public static object Load(string key)
        {
            //if there is no file, return null
            if (File.Exists(GetPathFile(key)) == false)
            {
                if (OldSaveLoadSystem.instance.ShowDebugLogs)
                    Debug.Log("Save file not found: " + GetPathFile(key));

                return null;
            }

            //create stream at file position
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(GetPathFile(key), FileMode.Open);

            //then load from file position as value, and close stream
            object value = formatter.Deserialize(stream);
            stream.Close();

            return value;
        }

        /// <summary>
        /// Delete a file
        /// </summary>
        /// <param name="key">Name of the file</param>
        public static void DeleteData(string key)
        {
            //check there is a file
            if (File.Exists(GetPathFile(key)) == false)
            {
                if (OldSaveLoadSystem.instance.ShowDebugLogs)
                    Debug.Log("Save file not found: " + GetPathFile(key));

                return;
            }

            //delete file
            File.Delete(GetPathFile(key));
        }

        /// <summary>
        /// Delete directory with every file
        /// </summary>
        public static void DeleteAll()
        {
            //check there is a directory
            if (Directory.Exists(OldSaveLoadSystem.instance.PathDirectory) == false)
            {
                if (OldSaveLoadSystem.instance.ShowDebugLogs)
                    Debug.Log("Directory not found: " + OldSaveLoadSystem.instance.PathDirectory);

                return;
            }

            //delete directory
            Directory.Delete(OldSaveLoadSystem.instance.PathDirectory, true);
        }
    }
}