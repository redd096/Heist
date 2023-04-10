using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

namespace redd096
{
    public class SaveLoadSystem_PC : ISaveLoadSystem
    {
        /// <summary>
        /// Some platforms need to be initialized before access saves. So use this to wait before call preload or other functions
        /// </summary>
        /// <param name="usePreload"></param>
        /// <returns></returns>
        public IEnumerator Initialize(bool usePreload)
        {
            //call preload if necessary
            if (usePreload)
                Preload();

            yield return null;
        }

        /// <summary>
        /// Get path to the file (directory path + name of the file (key) + format (.json))
        /// </summary>
        /// <param name="key">file name</param>
        /// <returns></returns>
        public string GetPathFile(string key)
        {
            //directory path + name of the file (key) + format (.json)
            return Path.Combine(SaveManager.instance.PathDirectory, key + ".json");
        }

        /// <summary>
        /// Load every file and call SaveManager to update dictionary
        /// </summary>
        /// <returns></returns>
        public void Preload()
        {
            Dictionary<string, string> jsons = new Dictionary<string, string>();

            //get all files in directory
            if (Directory.Exists(SaveManager.instance.PathDirectory))
            {
                foreach (string filePath in Directory.GetFiles(SaveManager.instance.PathDirectory, "*", SearchOption.AllDirectories))
                {
                    //and save in dictionary (key, json), where key is file name without extension
                    string key = Path.GetFileNameWithoutExtension(filePath);
                    jsons.Add(key, Load(key));
                }
            }

            //update dictionary in SaveManager, used when use preload is true
            SaveManager.instance.FinishPreload(jsons);
        }

        /// <summary>
        /// Save in directory/key.json
        /// </summary>
        /// <param name="key">file name</param>
        /// <param name="json">file value</param>
        /// <returns></returns>
        public bool Save(string key, string json)
        {
            try
            {
                //if there is no directory, create it
                if (Directory.Exists(SaveManager.instance.PathDirectory) == false)
                {
                    Directory.CreateDirectory(SaveManager.instance.PathDirectory);
                }

                //save file
                File.WriteAllText(GetPathFile(key), json);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Load from directory/key.json
        /// </summary>
        /// <param name="key">file name</param>
        /// <returns></returns>
        public string Load(string key)
        {
            try
            {
                //if there is no file, return
                if (File.Exists(GetPathFile(key)) == false)
                {
                    if (SaveManager.instance.ShowDebugLogs)
                        Debug.Log("Save file not found: " + GetPathFile(key));

                    return string.Empty;
                }

                //load file
                return File.ReadAllText(GetPathFile(key));
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// Save async in directory/key.json
        /// </summary>
        /// <param name="key">file name</param>
        /// <param name="json">file value</param>
        /// <returns></returns>
        public async Task<bool> SaveAsync(string key, string json)
        {
            try
            {
                //if there is no directory, create it
                if (Directory.Exists(SaveManager.instance.PathDirectory) == false)
                {
                    Directory.CreateDirectory(SaveManager.instance.PathDirectory);
                }

                //save file async
                await File.WriteAllTextAsync(GetPathFile(key), json);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Load async from directory/key.json
        /// </summary>
        /// <param name="key">file name</param>
        /// <returns></returns>
        public async Task<string> LoadAsync(string key)
        {
            try
            {
                //if there is no file, return
                if (File.Exists(GetPathFile(key)) == false)
                {
                    if (SaveManager.instance.ShowDebugLogs)
                        Debug.Log("Save file not found: " + GetPathFile(key));

                    return string.Empty;
                }

                //load file async
                return await File.ReadAllTextAsync(GetPathFile(key));
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// Check if file is saved on disk
        /// </summary>
        /// <param name="key">file name</param>
        /// <returns></returns>
        public bool FileExists(string key)
        {
            return File.Exists(GetPathFile(key));
        }

        /// <summary>
        /// Delete a file
        /// </summary>
        /// <param name="key">file name</param>
        /// <returns></returns>
        public bool DeleteData(string key)
        {
            try
            {
                //check there is a file
                if (File.Exists(GetPathFile(key)) == false)
                {
                    if (SaveManager.instance.ShowDebugLogs)
                        Debug.Log("Save file not found: " + GetPathFile(key));

                    return false;
                }

                //delete file
                File.Delete(GetPathFile(key));
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Delete directory with every file
        /// </summary>
        /// <returns></returns>
        public bool DeleteAll()
        {
            try
            {
                //check there is a directory
                if (Directory.Exists(SaveManager.instance.PathDirectory) == false)
                {
                    if (SaveManager.instance.ShowDebugLogs)
                        Debug.Log("Directory not found: " + SaveManager.instance.PathDirectory);

                    return false;
                }

                //delete directory
                Directory.Delete(SaveManager.instance.PathDirectory, true);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }
    }
}