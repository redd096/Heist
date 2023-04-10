using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using redd096.Attributes;

namespace redd096
{
	[AddComponentMenu("redd096/MonoBehaviours/Canvas Shake")]
	public class CanvasShake : MonoBehaviour
	{
		public static CanvasShake instance;

		[Header("Objects to Shake (inside canvas)")]
		[SerializeField] RectTransform[] objectsToShake = default;

		[Header("Shake")]
		[Tooltip("How long should shake")] [SerializeField] float shakeDuration = 0.3f;
		[Tooltip("Amplitude of the shake. A larger value shakes the camera harder")] [SerializeField] float shakeAmount = 15f;

		[Header("Overwrite - start shake also if another is running")]
		[Tooltip("If another shake is already running, stop it and start new one")] [SerializeField] bool overwriteShake = true;

		Coroutine shakeCoroutine;
		Dictionary<RectTransform, Vector2> startPositions = new Dictionary<RectTransform, Vector2>();
		CustomCanvasShake customCanvasShake;    //used to shake others objects instead of objectsToShake setted in inspector

		void Start()
		{
			//set instance to call it without use FindObjectOfType
			instance = this;

			//save start positions
			foreach (RectTransform obj in objectsToShake)
				if (obj)
					startPositions.Add(obj, obj.localPosition);

			//create custom canvas shake
			customCanvasShake = new CustomCanvasShake(this);
		}

		IEnumerator ShakeCoroutine(float duration, float amount)
		{
			//reset position
			foreach (RectTransform obj in startPositions.Keys)
				if (obj)
					obj.localPosition = startPositions[obj];

			//set shake duration
			float shake = Time.time + duration;

			//shake
			while (shake > Time.time && Time.timeScale > 0)
			{
				foreach (RectTransform obj in startPositions.Keys)
					if (obj)
						obj.localPosition = startPositions[obj] + (Vector2)Random.insideUnitSphere * amount;

				yield return null;
			}

			//then reset to original position
			foreach (RectTransform obj in startPositions.Keys)
				if (obj)
					obj.localPosition = startPositions[obj];

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

		/// <summary>
		/// Start shake, using other objects instead of the ones setted on CanvasShake in scene. 
		/// Use still duration and amount setted in inspector
		/// </summary>
		/// <param name="customObjectsToShake"></param>
		public void StartShake(RectTransform[] customObjectsToShake)
		{
			customCanvasShake.StartShake(customObjectsToShake, shakeDuration, shakeAmount, overwriteShake);
		}

		/// <summary>
		/// Start shake, using other objects instead of the ones setted on CanvasShake in scene. 
		/// Use specific duration and amount
		/// </summary>
		/// <param name="customObjectsToShake"></param>
		/// <param name="duration"></param>
		/// <param name="amount"></param>
		public void StartShake(RectTransform[] customObjectsToShake, float duration, float amount)
		{
			customCanvasShake.StartShake(customObjectsToShake, duration, amount, overwriteShake);
		}

		#endregion
	}

	public class CustomCanvasShake
	{
		//this saves every coroutine that is running, and will be removed when the coroutines finish
		//If more coroutines use same RectTransform, pray ('^o^)/\  八(＾o＾')   
		Dictionary<RectTransform[], Coroutine> currentShakes = new Dictionary<RectTransform[], Coroutine>();

		//this saves start position for every RectTransform. This will be saved for the first time a shake is called on a RectTransform, and never changed.
		//If is called again shake to a RectTransform, will use start position already saved
		Dictionary<RectTransform, Vector2> startPositions = new Dictionary<RectTransform, Vector2>();

		//used to start coroutines
		CanvasShake owner;

		public CustomCanvasShake(CanvasShake owner)
		{
			this.owner = owner;
		}

		/// <summary>
		/// Start shake, using specific duration and amount
		/// </summary>
		public void StartShake(RectTransform[] customObjectsToShake, float duration, float amount, bool overwriteShake)
		{
			//save start positions
			foreach (RectTransform obj in customObjectsToShake)
			{
				if (startPositions.ContainsKey(obj) == false)
					startPositions.Add(obj, obj.localPosition);
			}

			if (owner == null)
			{
				Debug.LogWarning("Miss owner in Custom Canvas Shake");
				return;
			}

			//do only if there is not another shake on these objects, or can overwrite it
			if (currentShakes.ContainsKey(customObjectsToShake) == false || overwriteShake)
			{
				//start or restart shake coroutine
				if (currentShakes.ContainsKey(customObjectsToShake) && currentShakes[customObjectsToShake] != null)
				{
					owner.StopCoroutine(currentShakes[customObjectsToShake]);
				}

				currentShakes[customObjectsToShake] = owner.StartCoroutine(ShakeCoroutine(customObjectsToShake, duration, amount));
			}
		}

		IEnumerator ShakeCoroutine(RectTransform[] customObjectsToShake, float duration, float amount)
		{
			//reset position
			foreach (RectTransform obj in customObjectsToShake)
				if (obj && startPositions.ContainsKey(obj))
					obj.localPosition = startPositions[obj];

			//set shake duration
			float shake = Time.time + duration;

			//shake
			while (shake > Time.time && Time.timeScale > 0)
			{
				foreach (RectTransform obj in customObjectsToShake)
					if (obj && startPositions.ContainsKey(obj))
						obj.localPosition = startPositions[obj] + (Vector2)Random.insideUnitSphere * amount;

				yield return null;
			}

			//then reset to original position
			foreach (RectTransform obj in customObjectsToShake)
				if (obj && startPositions.ContainsKey(obj))
					obj.localPosition = startPositions[obj];

			//remove from coroutines
			if (currentShakes.ContainsKey(customObjectsToShake))
				currentShakes.Remove(customObjectsToShake);
		}
	}

	[System.Serializable]
	public struct CanvasShakeStruct
	{
		public bool DoShake;
		[Tooltip("Use custom objects? If null, use objects setted on CanvasShake in scene")] public RectTransform[] CustomObjectsToShake;
		[EnableIf("DoShake")] public bool CustomShake;
		[EnableIf("DoShake", "CustomShake")] public float ShakeDuration;
		[EnableIf("DoShake", "CustomShake")] public float ShakeAmount;

		public void TryShake()
		{
			//camera shake
			if (DoShake && CanvasShake.instance)
			{
				//custom or default
				if (CustomObjectsToShake == null || CustomObjectsToShake.Length <= 0)
				{
					if (CustomShake)
						CanvasShake.instance.StartShake(ShakeDuration, ShakeAmount);
					else
						CanvasShake.instance.StartShake();
				}
				//custom objects to shake
				else
				{
					if (CustomShake)
						CanvasShake.instance.StartShake(CustomObjectsToShake, ShakeDuration, ShakeAmount);
					else
						CanvasShake.instance.StartShake(CustomObjectsToShake);
				}
			}
		}
	}
}