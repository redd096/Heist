using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace redd096.Attributes
{
    /// <summary>
    /// Show help box above property
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class HelpBoxAttribute : PropertyAttribute
    {
        public enum EMessageType { None, Info, Warning, Error }

        public readonly EMessageType messageType;
        public readonly string message;
        public readonly bool wide;

        /// <summary>
        /// Show help box above property
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageType"></param>
        /// <param name="wide">If true, the box will cover the whole width of the window; otherwise it will cover the controls part only</param>
        public HelpBoxAttribute(string message, EMessageType messageType = EMessageType.Info, bool wide = true)
        {
            this.message = message;
            this.messageType = messageType;
            this.wide = wide;
        }
    }

    #region editor

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxDrawer : DecoratorDrawer
    {
        HelpBoxAttribute at;

        public override float GetHeight()
        {
            return base.GetHeight() + EditorStyles.helpBox.CalcSize(new GUIContent(at != null ? at.message : "")).y;
        }

        public override void OnGUI(Rect position)
        {
            //base.OnGUI(position);
            at = attribute as HelpBoxAttribute;

            EditorGUI.HelpBox(position, at.message, GetMessageType(at.messageType));
        }

        MessageType GetMessageType(HelpBoxAttribute.EMessageType messageType)
        {
            switch (messageType)
            {
                case HelpBoxAttribute.EMessageType.None:
                    return MessageType.None;
                case HelpBoxAttribute.EMessageType.Info:
                    return MessageType.Info;
                case HelpBoxAttribute.EMessageType.Warning:
                    return MessageType.Warning;
                case HelpBoxAttribute.EMessageType.Error:
                    return MessageType.Error;
                default:
                    return MessageType.Info;
            }
        }
    }

#endif

    #endregion
}