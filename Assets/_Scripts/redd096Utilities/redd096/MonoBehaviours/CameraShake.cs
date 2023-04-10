using System.Collections;
using UnityEngine;
using redd096.Attributes;

namespace redd096
{
	[AddComponentMenu("redd096/MonoBehaviours/Camera Shake")]
	public class CameraShake : MonoBehaviour
	{
		public static CameraShake instance;

		[Header("Camera - by default is main camera")]
		[Tooltip("If null, use main camera")][SerializeField] Transform camTransform;

		[Header("Shake")]
		[Tooltip("How long should shake")][SerializeField] float shakeDuration = 1f;
		[Tooltip("Amplitude of the shake. A larger value shakes the camera harder")][SerializeField] float shakeAmount = 0.7f;

		[Header("Overwrite - start shake also if another is running")]
		[Tooltip("If another shake is already running, stop it and start new one")][SerializeField] bool overwriteShake = true;

		Coroutine shakeCoroutine;
		Transform cameraParent;

		void Awake()
		{
			//set instance to call it without use FindObjectOfType
			instance = this;

			//get reference if null
			if (camTransform == null)
			{
				camTransform = Camera.main.transform;
			}

			//set cam parent
			if (camTransform)
			{
				cameraParent = new GameObject("Camera Parent (camera shake)").transform;
				cameraParent.SetParent(camTransform.parent);                //set same parent (if camera was child of something)
				cameraParent.localPosition = Vector3.zero;                  //set start local position
				camTransform.SetParent(cameraParent);                       //set camera parent
			}
			else
			{
				Debug.LogWarning("There is no camera for camera shake");
			}
		}

		IEnumerator ShakeCoroutine(float duration, float amount)
		{
			//reset position
			if (cameraParent)
				cameraParent.localPosition = Vector3.zero;

			//set shake duration
			float shake = Time.time + duration;

			//shake
			while (shake > Time.time && Time.timeScale > 0)
			{
				//stop if there is no camera parent
				if (cameraParent == null)
					break;

				cameraParent.localPosition = Random.insideUnitSphere * amount;

				yield return null;
			}

			//then reset to original position
			if (cameraParent)
				cameraParent.localPosition = Vector3.zero;

			shakeCoroutine = null;
		}

		#region public API

		/// <summary>
		/// Start shake, using variables in inspector
		/// </summary>
		public void StartShake()
		{
			StartShake(shakeDuration, shakeAmount);
		}

		/// <summary>
		/// Start shake, using specific duration and amount
		/// </summary>
		public void StartShake(float duration, float amount)
		{
			//do only if there is not another shake, or can overwrite it
			if (shakeCoroutine == null || overwriteShake)
			{
				//start or restart shake coroutine
				if (shakeCoroutine != null)
					StopCoroutine(shakeCoroutine);

				shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, amount));
			}
		}

		#endregion
	}

	[System.Serializable]
	public struct CameraShakeStruct
	{
		public bool DoShake;
		[EnableIf("DoShake")] public bool CustomShake;
		[EnableIf("DoShake", "CustomShake")] public float ShakeDuration;
		[EnableIf("DoShake", "CustomShake")] public float ShakeAmount;

		public void TryShake()
		{
			//camera shake
			if (DoShake && CameraShake.instance)
			{
				//custom or default
				if (CustomShake)
					CameraShake.instance.StartShake(ShakeDuration, ShakeAmount);
				else
					CameraShake.instance.StartShake();
			}
		}
	}
}