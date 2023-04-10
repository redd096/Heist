using System.Collections;
using System.Threading.Tasks;

namespace redd096
{
    public interface ISaveLoadSystem
    {
        /// <summary>
        /// Some platforms need to be initialized before access saves. So use this to wait before call preload or other functions
        /// </summary>
        /// <param name="usePreload"></param>
        /// <returns></returns>
        IEnumerator Initialize(bool usePreload);

        /// <summary>
        /// Get path to the file (directory path + name of the file (key) + format (.json))
        /// </summary>
        /// <param name="key">file name</param>
        /// <returns></returns>
        string GetPathFile(string key);

        /// <summary>
        /// Load every file and call SaveManager to update dictionary
        /// </summary>
        /// <returns></returns>
        void Preload();

        /// <summary>
        /// Save in directory/key.json
        /// </summary>
        /// <param name="key">file name</param>
        /// <param name="json">file value</param>
        /// <returns></returns>
        bool Save(string key, string json);

        /// <summary>
        /// Load from directory/key.json
        /// </summary>
        /// <param name="key">file name</param>
        /// <returns></returns>
        string Load(string key);

        /// <summary>
        /// Save async in directory/key.json
        /// </summary>
        /// <param name="key">file name</param>
        /// <param name="json">file value</param>
        /// <returns></returns>
        Task<bool> SaveAsync(string key, string json);

        /// <summary>
        /// Load async from directory/key.json
        /// </summary>
        /// <param name="key">file name</param>
        /// <returns></returns>
        Task<string> LoadAsync(string key);

        /// <summary>
        /// Check if file is saved on disk
        /// </summary>
        /// <param name="key">file name</param>
        /// <returns></returns>
        bool FileExists(string key);

        /// <summary>
        /// Delete a file
        /// </summary>
        /// <param name="key">file name</param>
        /// <returns></returns>
        bool DeleteData(string key);

        /// <summary>
        /// Delete every file
        /// </summary>
        /// <returns></returns>
        bool DeleteAll();
    }
}