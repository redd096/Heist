using System.IO;
using UnityEngine;
using redd096.Attributes;

namespace redd096
{
    [AddComponentMenu("redd096/MonoBehaviours/GenerateAtlas")]
    public class GenerateAtlas : MonoBehaviour
    {
        [Header("New Texture")]
        [SerializeField] string textureName = "Atlas";
        [SerializeField] TextureFormat textureFormat = TextureFormat.RGBA32;
        [SerializeField] bool mipChain = false;

        [Header("Where to Save")]
        [SerializeField] SaveFolder saveFolder = SaveFolder.gameFolder;
        [SerializeField] string directoryName = "PORTING/Resources/Sprite Assets";

        [Header("Sprites to use (enable Read/Write in inspector)")]
        [SerializeField] int widthSprites = 100;
        [SerializeField] int heightSprites = 100;
        [SerializeField] Color colorEmptySpaces = new Color(0, 0, 0, 0);
        [SerializeField] Sprite[] spritesToUse = default;

        Texture2D resultTexture;

        [Button]
        void GenerateTexture()
        {
            //calculate number of rows (and columns)
            int rows = GetNumberOfRows();

            //create texture
            resultTexture = new Texture2D(rows * widthSprites, rows * heightSprites, textureFormat, mipChain);

            //draw each sprite from left to right
            for (int i = 0; i < rows * rows; i++)
            {
                //when reach rows, move down
                //Y doesn't start from 0 and move up, but start from up and move to 0. We are drawing starting from up left
                DrawSprite(i < spritesToUse.Length ? spritesToUse[i] : null, (i % rows) * widthSprites, ((rows - 1) - (i / rows)) * heightSprites);

                //example with an atlas of 5 rows (5x5 sprites)
                // (x = i % rows) so x follow i when is 0, 1, 2, 3, 4. Then restart from 0 for next row
                // (y = i / rows) so y is 0 when i is 0, 1, 2, 3, 4. Then increase by one for next row
                // (y = (rows - 1) - y) so y doesn't start from 0, but start from 4 and move to 0 following the result described above
            }

            //apply all SetPixel to texture
            resultTexture.Apply();

            //save texture on disk
            SaveTexture();
        }

        #region private API

        /// <summary>
        /// Row * row will return a square that can contains all the sprites in the array
        /// </summary>
        /// <returns></returns>
        int GetNumberOfRows()
        {
            ////find number of rows and columns
            //int rows = 0;
            //while (true)
            //{
            //    //we need a square, so rows * rows must be equal to our number (or greater, and last square will be empty)
            //    rows++;
            //    if (rows * rows >= spritesToUse.Length)
            //    {
            //        break;
            //    }
            //}
            //
            //return rows;

            //square root to find the number of rows and columns (we want a square).
            //If the result is not an integer, take the number greater, and the square in excess will be empty
            return Mathf.CeilToInt(Mathf.Sqrt(spritesToUse.Length));
        }

        /// <summary>
        /// Draw this sprite in resultTexture
        /// </summary>
        /// <param name="sprite">Sprite to draw in our resultTexture</param>
        /// <param name="startX">From which pixel of resultTexture start to draw</param>
        /// <param name="startY">From which pixel of resultTexture start to draw</param>
        void DrawSprite(Sprite sprite, int startX, int startY)
        {
            //get texture (or null)
            Texture2D texture = (sprite == null || sprite.texture == null) ? null : sprite.texture;

            //and draw every pixel
            for (int x = 0; x < widthSprites; x++)
            {
                for (int y = 0; y < heightSprites; y++)
                {
                    if (texture != null && texture.width > x && texture.height > y)
                        resultTexture.SetPixel(startX + x, startY + y, texture.GetPixel(x, y));     //get from sprite
                    else
                        resultTexture.SetPixel(startX + x, startY + y, colorEmptySpaces);           //or empty color if sprite is smaller (or null)
                }
            }
        }

        /// <summary>
        /// Create directory and png file from resultTexture
        /// </summary>
        void SaveTexture()
        {
            //if there is no directory, create it
            if (Directory.Exists(GetFolderPath()) == false)
            {
                Directory.CreateDirectory(GetFolderPath());
            }

            //create png
            byte[] data = resultTexture.EncodeToPNG();

            //save on disk
            File.WriteAllBytes(Path.Combine(GetFolderPath(), textureName + ".png"), data);

#if UNITY_EDITOR
            //refresh project to show created folder and png
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        #endregion

        #region get folder path

        enum SaveFolder
        {
            persistentDataPath, gameFolder, nothing
        }

        string GetFolderPath()
        {
            if (saveFolder == SaveFolder.persistentDataPath)
                return Path.Combine(Application.persistentDataPath, directoryName);     //return persistent data path + directory path
            else if (saveFolder == SaveFolder.gameFolder)
                return Path.Combine(Application.dataPath, directoryName);               //return game folder path + directory path
            else
                return directoryName;                                                   //return only directory path
        }

        #endregion
    }
}