using UnityEngine;
using redd096.Attributes;   //help box when input system isn't enabled
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace redd096
{
    [AddComponentMenu("redd096/Singletons/Input Manager redd096")]
    public class InputManagerRedd096 : Singleton<InputManagerRedd096>
    {
#if ENABLE_INPUT_SYSTEM
        [Header("Input Action Asset")]
        public InputActionAsset inputActionAsset;

        void OnEnable()
        {
            inputActionAsset.Enable();
        }

        void OnDisable()
        {
            inputActionAsset.Disable();
        }
#else
        [HelpBox("This works only with new unity input system", HelpBoxAttribute.EMessageType.Error)]
        public string Error = "It works only with new unity input system";
#endif
    }

    public static class InputRedd096
    {
#if ENABLE_INPUT_SYSTEM
        #region using input actions asset

        /// <summary>
        /// Returns true during the frame the user pressed down the virtual button identified by inputName
        /// </summary>
        public static bool GetButtonDown(string inputName, InputActionAsset inputActionAsset = null)
        {
            //use singleton if no input action asset
            if (inputActionAsset == null)
                inputActionAsset = InputManagerRedd096.instance.inputActionAsset;

            return inputActionAsset.FindAction(inputName).WasPressedThisFrame();
        }

        /// <summary>
        /// Returns true during the frame the user released the virtual button identified by inputName
        /// </summary>
        public static bool GetButtonUp(string inputName, InputActionAsset inputActionAsset = null)
        {
            //use singleton if no input action asset
            if (inputActionAsset == null)
                inputActionAsset = InputManagerRedd096.instance.inputActionAsset;

            return inputActionAsset.FindAction(inputName).WasReleasedThisFrame();
        }

        /// <summary>
        /// Returns true while the virtual button identified by inputName is held down
        /// </summary>
        public static bool GetButton(string inputName, InputActionAsset inputActionAsset = null)
        {
            //use singleton if no input action asset
            if (inputActionAsset == null)
                inputActionAsset = InputManagerRedd096.instance.inputActionAsset;

            return inputActionAsset.FindAction(inputName).IsPressed();
        }

        /// <summary>
        /// Returns the value of the virtual T identified by inputtName (GetAxis returns only float, this one returns every type of value)
        /// </summary>
        public static T GetValue<T>(string inputName, InputActionAsset inputActionAsset = null) where T : struct
        {
            //use singleton if no input action asset
            if (inputActionAsset == null)
                inputActionAsset = InputManagerRedd096.instance.inputActionAsset;

            return inputActionAsset.FindAction(inputName).ReadValue<T>();
        }

        #endregion

        #region utility

        /// <summary>
        /// Returns true if current activeControl is the same on inputName1 and inputName2 
        /// </summary>
        public static bool IsSameInput(string inputName1, string inputName2, InputActionAsset inputActionAsset = null)
        {
            //use singleton if no input action asset
            if (inputActionAsset == null)
                inputActionAsset = InputManagerRedd096.instance.inputActionAsset;

            //if both with active control != null, else obviously is not same input
            if (inputActionAsset.FindAction(inputName1).activeControl != null && inputActionAsset.FindAction(inputName2).activeControl != null)
                return inputActionAsset.FindAction(inputName1).activeControl.name == inputActionAsset.FindAction(inputName2).activeControl.name;

            return false;
        }

        /// <summary>
        /// Returns true if current activeControl in inputName1 is "escape"
        /// </summary>
        public static bool IsEscape(string inputName, InputActionAsset inputActionAsset = null)
        {
            //use singleton if no input action asset
            if (inputActionAsset == null)
                inputActionAsset = InputManagerRedd096.instance.inputActionAsset;

            return inputActionAsset.FindAction(inputName).activeControl.name == "escape";
        }

        /// <summary>
        /// Returns true if controlSchemeName is current control scheme
        /// </summary>
        public static bool IsCurrentControlScheme(PlayerInput playerInput, string controlSchemeName)
        {
            //return InputManagerRedd096.instance.playerInput.currentControlScheme == InputManagerRedd096.instance.inputActionAsset.FindControlScheme(controlSchemeName).Value.name
            return playerInput.currentControlScheme == controlSchemeName;
        }

        /// <summary>
        /// Returns name of active control with this action
        /// </summary>
        public static string GetActiveControlName(string inputName, InputActionAsset inputActionAsset = null)
        {
            //use singleton if no input action asset
            if (inputActionAsset == null)
                inputActionAsset = InputManagerRedd096.instance.inputActionAsset;

            if (inputActionAsset.FindAction(inputName).activeControl != null)
                return inputActionAsset.FindAction(inputName).activeControl.name;

            //if no active control, return empty string
            return string.Empty;
        }

        /// <summary>
        /// Returns name of selected control
        /// </summary>
        public static string GetControlName(string inputName, int index, InputActionAsset inputActionAsset = null)
        {
            //use singleton if no input action asset
            if (inputActionAsset == null)
                inputActionAsset = InputManagerRedd096.instance.inputActionAsset;

            if (inputActionAsset.FindAction(inputName) != null)
                if (inputActionAsset.FindAction(inputName).controls.Count > index)
                    return inputActionAsset.FindAction(inputName).controls[index].name;

            //if no control, return empty string
            return string.Empty;
        }

        /// <summary>
        /// Returns display name of selected control
        /// </summary>
        public static string GetControlDisplayName(string inputName, int index, InputActionAsset inputActionAsset = null)
        {
            //use singleton if no input action asset
            if (inputActionAsset == null)
                inputActionAsset = InputManagerRedd096.instance.inputActionAsset;

            if (inputActionAsset.FindAction(inputName) != null)
                if (inputActionAsset.FindAction(inputName).controls.Count > index)
                    return inputActionAsset.FindAction(inputName).controls[index].displayName;

            //if no control, return empty string
            return string.Empty;
        }

        /// <summary>
        /// Returns binding display name for selected control
        /// </summary>
        public static string GetBindingDisplayName(string inputName, int index, InputActionAsset inputActionAsset = null)
        {
            //use singleton if no input action asset
            if (inputActionAsset == null)
                inputActionAsset = InputManagerRedd096.instance.inputActionAsset;

            InputAction inputAction = inputActionAsset.FindAction(inputName);

            if (inputAction != null)
                if (inputAction.controls.Count > index)
                    return inputAction.GetBindingDisplayString(inputAction.GetBindingIndexForControl(inputAction.controls[index]));

            //if no control, return empty string
            return string.Empty;
        }

        #endregion

        #region replace old input system

        /// <summary>
        /// Returns the value of the virtual axis identified by inputName
        /// </summary>
        public static float GetAxis(string inputName, InputActionAsset inputActionAsset = null)
        {
            //use singleton if no input action asset
            if (inputActionAsset == null)
                inputActionAsset = InputManagerRedd096.instance.inputActionAsset;

            return inputActionAsset.FindAction(inputName).ReadValue<float>();
        }

        /// <summary>
        /// Return TouchState struct (with position, phase and much more)
        /// </summary>
        public static UnityEngine.InputSystem.LowLevel.TouchState GetTouch(int index)
        {
            return Touchscreen.current.touches[index].ReadValue();
        }

        #region get key

        /// <summary>
        /// Returns true during the frame the user pressed down the key identified by the Key enum parameter
        /// </summary>
        public static bool GetKeyDown(Key key)
        {
            return Keyboard.current[key].wasPressedThisFrame;
        }

        /// <summary>
        /// Returns true while the user holds down the key identified by the Key enum parameter
        /// </summary>
        public static bool GetKey(Key key)
        {
            return Keyboard.current[key].isPressed;
        }

        /// <summary>
        /// Returns true during the frame the user releases the key identified by the Key enum parameter
        /// </summary>
        public static bool GetKeyUp(Key key)
        {
            return Keyboard.current[key].wasReleasedThisFrame;
        }

        /// <summary>
        /// Returns true during the frame the user pressed down the key identified by the GamepadButton enum parameter
        /// </summary>
        public static bool GetKeyDown(UnityEngine.InputSystem.LowLevel.GamepadButton key)
        {
            return Gamepad.current[key].wasPressedThisFrame;
        }

        /// <summary>
        /// Returns true while the user holds down the key identified by the GamepadButton enum parameter
        /// </summary>
        public static bool GetKey(UnityEngine.InputSystem.LowLevel.GamepadButton key)
        {
            return Gamepad.current[key].isPressed;
        }

        /// <summary>
        /// Returns true during the frame the user releases the key identified by the GamepadButton enum parameter
        /// </summary>
        public static bool GetKeyUp(UnityEngine.InputSystem.LowLevel.GamepadButton key)
        {
            return Gamepad.current[key].wasReleasedThisFrame;
        }

        #endregion

        #region get mouse button

        /// <summary>
        /// Returns true during the frame the user pressed the given mouse button (0 left, 1 right, 2 middle)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool GetMouseButtonDown(int index)
        {
            if (index == 0)
                return GetLeftMouseButtonDown();
            else if (index == 1)
                return GetRightMouseButtonDown();
            else if (index == 2)
                return GetMiddleMouseButtonDown();

            return false;
        }

        /// <summary>
        /// Returns whether the given mouse button is held down (0 left, 1 right, 2 middle)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool GetMouseButton(int index)
        {
            if (index == 0)
                return GetLeftMouseButton();
            else if (index == 1)
                return GetRightMouseButton();
            else if (index == 2)
                return GetMiddleMouseButton();

            return false;
        }

        /// <summary>
        /// Returns true during the frame the user releases the given mouse button (0 left, 1 right, 2 middle)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool GetMouseButtonUp(int index)
        {
            if (index == 0)
                return GetLeftMouseButtonUp();
            else if (index == 1)
                return GetRightMouseButtonUp();
            else if (index == 2)
                return GetMiddleMouseButtonUp();

            return false;
        }

        #endregion

        #region general

        /// <summary>
        /// Last measured linear acceleration of a device in three-dimensional space
        /// </summary>
        public static Vector3 acceleration => Accelerometer.current.acceleration.ReadValue();
        /// <summary>
        /// Is any key or mouse button currenttly held down?
        /// </summary>
        public static bool anyKey => Keyboard.current.anyKey.isPressed || Mouse.current.leftButton.isPressed || Mouse.current.rightButton.isPressed || Mouse.current.middleButton.isPressed;
        /// <summary>
        /// Returns true the first frame the user hits any key or mouse button
        /// </summary>
        public static bool anyKeyDown => Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame || Mouse.current.middleButton.wasPressedThisFrame;
        /// <summary>
        /// Returns the attitude (ie, orientation in space) of the device
        /// </summary>
        public static Quaternion gyroAttitude => AttitudeSensor.current.attitude.ReadValue();
        /// <summary>
        /// The current mouse position in pixel coordinates
        /// </summary>
        public static Vector2 mousePosition => Mouse.current.position.ReadValue();
        /// <summary>
        /// Number of touches
        /// </summary>
        public static int touchCount => Touchscreen.current.touches.Count;

        #endregion

        #endregion

        #region mouse utility

        /// <summary>
        /// Returns true during the frame the user pressed down the left mouse button
        /// </summary>
        public static bool GetLeftMouseButtonDown()
        {
            return Mouse.current.leftButton.wasPressedThisFrame;
        }

        /// <summary>
        /// Returns true while the user holds down the left mouse button
        /// </summary>
        public static bool GetLeftMouseButton()
        {
            return Mouse.current.leftButton.isPressed;
        }

        /// <summary>
        /// Returns true during the frame the user releases the left mouse button
        /// </summary>
        public static bool GetLeftMouseButtonUp()
        {
            return Mouse.current.leftButton.wasReleasedThisFrame;
        }

        /// <summary>
        /// Returns true during the frame the user pressed down the right mouse button
        /// </summary>
        public static bool GetRightMouseButtonDown()
        {
            return Mouse.current.rightButton.wasPressedThisFrame;
        }

        /// <summary>
        /// Returns true while the user holds down the right mouse button
        /// </summary>
        public static bool GetRightMouseButton()
        {
            return Mouse.current.rightButton.isPressed;
        }

        /// <summary>
        /// Returns true during the frame the user releases the right mouse button
        /// </summary>
        public static bool GetRightMouseButtonUp()
        {
            return Mouse.current.rightButton.wasReleasedThisFrame;
        }

        /// <summary>
        /// Returns true during the frame the user pressed down the middle mouse button
        /// </summary>
        public static bool GetMiddleMouseButtonDown()
        {
            return Mouse.current.middleButton.wasPressedThisFrame;
        }

        /// <summary>
        /// Returns true while the user holds down the middle mouse button
        /// </summary>
        public static bool GetMiddleMouseButton()
        {
            return Mouse.current.middleButton.isPressed;
        }

        /// <summary>
        /// Returns true during the frame the user releases the middle mouse button
        /// </summary>
        public static bool GetMiddleMouseButtonUp()
        {
            return Mouse.current.middleButton.wasReleasedThisFrame;
        }

        #endregion
#endif
    }
}