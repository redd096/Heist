using System.Collections;
using UnityEngine;
using redd096.Attributes;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace redd096
{
	[AddComponentMenu("redd096/MonoBehaviours/Gamepad Vibration")]
	public class GamepadVibration : MonoBehaviour
	{
		public static GamepadVibration instance;

		[Header("Vibration")]
		[Tooltip("How long should vibrate")][SerializeField] float vibrationDuration = 0.1f;
		[Tooltip("Amplitude of the vibration")][SerializeField] float lowFrequency = 0.5f;
		[Tooltip("Amplitude of the vibration")][SerializeField] float highFrequency = 0.8f;

		[Header("Overwrite - start vibration also if another is running")]
		[Tooltip("If another vibration is already running, stop it and start new one")][SerializeField] bool overwriteVibration = true;

		Coroutine vibrationCoroutine;

		void Awake()
		{
			//set instance to call it without use FindObjectOfType
			instance = this;
		}

		IEnumerator VibrationCoroutine(float duration)
		{
			yield return new WaitForSeconds(duration);

#if ENABLE_INPUT_SYSTEM
			//stop vibration
			if (Gamepad.current != null)
				Gamepad.current.SetMotorSpeeds(0, 0);
#endif

			vibrationCoroutine = null;
		}

		#region public API

		/// <summary>
		/// Start vibration, using variables in inspector
		/// </summary>
		public void StartVibration()
		{
			StartVibration(vibrationDuration, lowFrequency, highFrequency);
		}

		/// <summary>
		/// Start vibration, using specific duration and amount
		/// </summary>
		public void StartVibration(float duration, float low, float high)
		{
			//do only if there is not another vibration, or can overwrite it
			if (vibrationCoroutine == null || overwriteVibration)
			{
				//stop coroutine if already running
				if (vibrationCoroutine != null)
					StopCoroutine(vibrationCoroutine);

#if ENABLE_INPUT_SYSTEM
				//set new vibration
				if (Gamepad.current != null)
					Gamepad.current.SetMotorSpeeds(low, high);
#endif

				//start or restart vibration coroutine to stop after few seconds
				vibrationCoroutine = StartCoroutine(VibrationCoroutine(duration));
			}
		}

		#endregion
	}

	[System.Serializable]
	public struct GamepadVibrationStruct
	{
		public bool DoVibration;
		[EnableIf("DoVibration")] public bool CustomVibration;
		[EnableIf("DoVibration", "CustomVibration")] public float VibrationDuration;
		[EnableIf("DoVibration", "CustomVibration")] public float LowFrequency;
		[EnableIf("DoVibration", "CustomVibration")] public float HighFrequency;

		public void TryVibration()
		{
			//gamepad vibration
			if (DoVibration && GamepadVibration.instance)
			{
				//custom or default
				if (CustomVibration)
					GamepadVibration.instance.StartVibration(VibrationDuration, LowFrequency, HighFrequency);
				else
					GamepadVibration.instance.StartVibration();
			}
		}
	}
}