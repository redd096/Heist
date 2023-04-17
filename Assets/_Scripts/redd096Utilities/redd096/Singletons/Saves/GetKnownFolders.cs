using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;

namespace redd096
{
    public static class GetKnownFolders
    {
        public static string GetDownloadPath()
        {
#if UNITY_STANDALONE
            return GetKnownFolders_Windows.GetDownloadPath();
#elif UNITY_ANDROID
            return GetKnownFolders_Android.GetDownloadPath();
#endif
            return string.Empty;
        }
    }

    static class GetKnownFolders_Windows
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        public enum KnownFolder
        {
            Contacts,
            Downloads,
            Favorites,
            Links,
            SavedGames,
            SavedSearches
        }

        //list of guid for every folder
        //https://learn.microsoft.com/en-us/dotnet/desktop/winforms/controls/known-folder-guids-for-file-dialog-custom-places?view=netframeworkdesktop-4.8
        private static readonly Dictionary<KnownFolder, Guid> _guids = new Dictionary<KnownFolder, Guid>()
        {
            [KnownFolder.Contacts] = new Guid("56784854-C6CB-462B-8169-88E350ACB882"),
            [KnownFolder.Downloads] = new Guid("374DE290-123F-4565-9164-39C4925E467B"),
            [KnownFolder.Favorites] = new Guid("1777F761-68AD-4D8A-87BD-30B759FA33DD"),
            [KnownFolder.Links] = new Guid("BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968"),
            [KnownFolder.SavedGames] = new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"),
            [KnownFolder.SavedSearches] = new Guid("7D1D3A04-DEBB-4115-95CF-2F29DA2920DA")
        };

        //microsoft function to get folder path
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pPath);

        /// <summary>
        /// Get path to folder
        /// </summary>
        /// <param name="knownFolder"></param>
        /// <returns></returns>
        public static string GetPath(KnownFolder knownFolder)
        {
            //get path from windows
            IntPtr pPath;
            SHGetKnownFolderPath(_guids[knownFolder], 0, IntPtr.Zero, out pPath);

            //translate to string
            string path = Marshal.PtrToStringUni(pPath);
            Marshal.FreeCoTaskMem(pPath);

            return path;
        }

        /// <summary>
        /// Get path to Download folder
        /// </summary>
        /// <returns></returns>
        public static string GetDownloadPath() => GetPath(KnownFolder.Downloads);
#endif
    }

    static class GetKnownFolders_Android
    {
#if UNITY_ANDROID
        private static string GetPath()
        {
            //to access SD Card
            //http://anja-haumann.de/unity-how-to-save-on-sd-card/


            ////get android activity
            //AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            //AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            //get environment
            AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment");

            //get download directory
            AndroidJavaObject rootStorage = environment.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", environment.GetStatic<string>("DIRECTORY_DOWNLOADS"));
            return rootStorage.Call<string>("getPath");
        }

        /// <summary>
        /// Get path to Download folder
        /// </summary>
        /// <returns></returns>
        public static string GetDownloadPath() => GetPath();
#endif
    }
}