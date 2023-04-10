using UnityEngine;

namespace redd096.Attributes
{
    public static class AttributesUtility
    {
        public enum EColor
        {
            Clear,
            Cyan,
            Grey,
            Gray,
            Magenta,
            Red,
            Yellow,
            Black,
            White,
            Green,
            Blue,

            SmoothRed,
            Pink,
            Orange,
            SmoothYellow,
            SmoothGreen,
            SmoothBlue,
            Indigo,
            Violet
        }

        public static Color GetColor(EColor color)
        {
            switch (color)
            {
                case EColor.Clear:
                    return Color.clear;
                case EColor.Cyan:
                    return Color.cyan;
                case EColor.Grey:
                    return Color.grey;
                case EColor.Gray:
                    return Color.gray;
                case EColor.Magenta:
                    return Color.magenta;
                case EColor.Red:
                    return Color.red;
                case EColor.Yellow:
                    return Color.yellow;
                case EColor.Black:
                    return Color.black;
                case EColor.White:
                    return Color.white;
                case EColor.Green:
                    return Color.green;
                case EColor.Blue:
                    return Color.blue;

                case EColor.SmoothRed:
                    return new Color32(255, 0, 63, 255);
                case EColor.Pink:
                    return new Color32(255, 152, 203, 255);
                case EColor.Orange:
                    return new Color32(255, 128, 0, 255);
                case EColor.SmoothYellow:
                    return new Color32(255, 211, 0, 255);
                case EColor.SmoothGreen:
                    return new Color32(98, 200, 79, 255);
                case EColor.SmoothBlue:
                    return new Color32(0, 135, 189, 255);
                case EColor.Indigo:
                    return new Color32(75, 0, 130, 255);
                case EColor.Violet:
                    return new Color32(128, 0, 255, 255);
                default:
                    return Color.black;
            }
        }
    }
}