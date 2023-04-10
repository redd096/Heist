using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using redd096.Attributes;

namespace redd096.OLD
{
	[AddComponentMenu("redd096/.OLD/Components/Old Collision Component")]
	public class OldCollisionComponent : MonoBehaviour
	{
		public enum EUpdateModes { Update, FixedUpdate, Coroutine, None }
		public enum EDirectionEnum { up, right, left, down }
		public enum ECollisionResponse { Collision, Trigger, Ignore }

		[Header("Check Raycasts")]
		[Tooltip("Check collisions on Update or FixedUpdate? If setted to None don't check collisions, but can use CalculateReachablePosition to check if hit something when moving, used for example for bullets")]
		[SerializeField] EUpdateModes updateMode = EUpdateModes.Coroutine;
		[Tooltip("Delay between updates using Coroutine method")] [EnableIf("updateMode", EUpdateModes.Coroutine)] [SerializeField] float timeCoroutine = 0.1f;
		[Tooltip("Number of rays cast for every side horizontally")] [SerializeField] int numberOfHorizontalRays = 4;
		[Tooltip("Number of rays cast for every side vertically")] [SerializeField] int numberOfVerticalRays = 4;
		[Tooltip("A small value to accomodate for edge cases")] [SerializeField] float offsetRays = 0.01f;
		[Tooltip("Layers that raycasts ignore")] [SerializeField] LayerMask layersToIgnore = default;

		[Header("ONLY BOX AND CIRCLE - Necessary Components (by default get in children)")]
		[SerializeField] Collider2D selfCollider = default;

		[Header("DEBUG")]
		[SerializeField] bool drawDebugInPlay = false;
		//[ShowNativeProperty] bool IsHittingRight => rightHits.Count > 0;
		//[ShowNativeProperty] bool IsHittingLeft => leftHits.Count > 0;
		//[ShowNativeProperty] bool IsHittingUp => upHits.Count > 0;
		//[ShowNativeProperty] bool IsHittingDown => downHits.Count > 0;

		public EUpdateModes UpdateMode { get => updateMode; set => updateMode = value; }

		//events
		public System.Action<RaycastHit2D> onCollisionEnter { get; set; }
		public System.Action<RaycastHit2D> onCollisionStay { get; set; }
		public System.Action<Collider2D> onCollisionExit { get; set; }
		public System.Action<RaycastHit2D> onTriggerEnter { get; set; }
		public System.Action<RaycastHit2D> onTriggerStay { get; set; }
		public System.Action<Collider2D> onTriggerExit { get; set; }

		//colliders to ignore (like Physics2D.IgnoreCollision(col, otherCol))
		List<Collider2D> collidersToIgnore = new List<Collider2D>();

		//hits
		List<RaycastHit2D> rightHits = new List<RaycastHit2D>();
		List<RaycastHit2D> leftHits = new List<RaycastHit2D>();
		List<RaycastHit2D> upHits = new List<RaycastHit2D>();
		List<RaycastHit2D> downHits = new List<RaycastHit2D>();

		//bounds
		BoxCollider2D boxCollider = default;
		CircleCollider2D circleCollider = default;
		Vector2 centerBounds;
		float horizontalExtents;
		float verticalExtents;
		float radiusSelfCollider;

		//bounds limits
		float rightBounds;
		float leftBounds;
		float upBounds;
		float downBounds;

		//used for events
		Dictionary<Collider2D, RaycastHit2D> currentCollisions = new Dictionary<Collider2D, RaycastHit2D>();
		List<Collider2D> previousCollisions = new List<Collider2D>();

		//for debug
		float drawDebugDuration = -1;

		//update mode
		Coroutine updateCoroutine;

		//vars for checks - to not fill garbage collector
		Vector2 raycastOriginFirst;
		Vector2 raycastOriginSecond;
		float raycastLength;
		Vector2 raycastOriginPoint;
		RaycastHit2D firstHit;
		RaycastHit2D secondHit;
		float bounds;
		int numberOfRays;
		Vector2 raycastDirection;
		List<Collider2D> hitsForCollisionEvent = new List<Collider2D>();

		void OnEnable()
		{
			//reset vars, for pooling
			ResetVars();

			//start coroutine
			if (updateMode == EUpdateModes.Coroutine)
				updateCoroutine = StartCoroutine(UpdateCoroutine());
		}

		void OnDisable()
		{
			//be sure to stop coroutine
			if (updateCoroutine != null)
			{
				StopCoroutine(updateCoroutine);
				updateCoroutine = null;
			}
		}

		void Update()
		{
			//do only if update mode is Update
			if (updateMode == EUpdateModes.Update)
				UpdateCollisions();
		}

		void FixedUpdate()
		{
			//do only if update mode is FixedUpdate
			if (updateMode == EUpdateModes.FixedUpdate)
				UpdateCollisions();
		}

		IEnumerator UpdateCoroutine()
		{
			//do only if update mode is Coroutine
			while (updateMode == EUpdateModes.Coroutine)
			{
				yield return new WaitForSeconds(timeCoroutine); //wait then update, otherwise update is called OnEnable and other scripts can't register to events for trigger enter
				UpdateCollisions();
			}
		}

		[Button]
		void DrawCollisions()
		{
			//be sure drawDebug is true
			bool previousDraw = drawDebugInPlay;
			drawDebugInPlay = true;

			//set time
			drawDebugDuration = 2;

			//check collisions
			UpdateCollisions();

			//restore debug values
			drawDebugInPlay = previousDraw;
			drawDebugDuration = -1;
		}

		#region private API

		bool CheckComponents()
		{
			//get references
			if (selfCollider == null)
				selfCollider = GetComponentInChildren<Collider2D>();

			//warning
			if (selfCollider == null)
			{
				Debug.LogWarning("Miss Collider on " + name);
				return false;
			}

			//save if box 
			if (selfCollider is BoxCollider2D)
			{
				boxCollider = selfCollider as BoxCollider2D;
			}
			//or circle collider
			else if (selfCollider is CircleCollider2D)
			{
				circleCollider = selfCollider as CircleCollider2D;
			}
			//else warning
			else
			{
				Debug.LogWarning("Collider on " + name + " can be only BoxCollider2D or CircleCollider2D");
				return false;
			}

			return true;
		}

		void CheckCollisionsHorizontal()
		{
			//horizontal raycast vars
			raycastOriginFirst = new Vector2(centerBounds.x, downBounds + offsetRays);
			raycastOriginSecond = new Vector2(centerBounds.x, upBounds - offsetRays);
			raycastLength = (rightBounds - leftBounds) * 0.5f;
			rightHits.Clear();
			leftHits.Clear();

			for (int i = 0; i < numberOfHorizontalRays; i++)
			{
				//from bottom to top
				raycastOriginPoint = Vector2.Lerp(raycastOriginFirst, raycastOriginSecond, (float)i / (numberOfHorizontalRays - 1));

				//raycast right and left
				RayCastHitSomething(raycastOriginPoint, Vector2.right, raycastLength, out firstHit, hitsForCollisionEvent);
				RayCastHitSomething(raycastOriginPoint, Vector2.left, raycastLength, out secondHit, hitsForCollisionEvent);

				//save hits
				if (firstHit) rightHits.Add(firstHit);
				if (secondHit) leftHits.Add(secondHit);

				//debug raycasts
				if (drawDebugInPlay)
				{
					DebugRaycast(raycastOriginPoint, Vector2.right, raycastLength, firstHit ? Color.red : Color.cyan);
					DebugRaycast(raycastOriginPoint, Vector2.left, raycastLength, secondHit ? Color.red : Color.cyan);
				}
			}
		}

		void CheckCollisionsVertical()
		{
			//vertical raycast vars
			raycastOriginFirst = new Vector2(leftBounds + offsetRays, centerBounds.y);
			raycastOriginSecond = new Vector2(rightBounds - offsetRays, centerBounds.y);
			raycastLength = (upBounds - downBounds) * 0.5f;
			upHits.Clear();
			downHits.Clear();

			for (int i = 0; i < numberOfVerticalRays; i++)
			{
				//from left to right
				raycastOriginPoint = Vector2.Lerp(raycastOriginFirst, raycastOriginSecond, (float)i / (numberOfVerticalRays - 1));

				//raycasts up and down
				RayCastHitSomething(raycastOriginPoint, Vector2.up, raycastLength, out firstHit, hitsForCollisionEvent);
				RayCastHitSomething(raycastOriginPoint, Vector2.down, raycastLength, out secondHit, hitsForCollisionEvent);

				//save hits
				if (firstHit) upHits.Add(firstHit);
				if (secondHit) downHits.Add(secondHit);

				//debug raycasts
				if (drawDebugInPlay)
				{
					DebugRaycast(raycastOriginPoint, Vector2.up, raycastLength, firstHit ? Color.red : Color.blue);
					DebugRaycast(raycastOriginPoint, Vector2.down, raycastLength, secondHit ? Color.red : Color.blue);
				}
			}
		}

		void RayCastHitSomething(Vector2 originPoint, Vector2 direction, float distance, out RaycastHit2D nearestHit, List<Collider2D> savedHitsForEvent, bool saveCollisionForEvents = true)
		{
			nearestHit = default;
			savedHitsForEvent.Clear();

			//foreach (RaycastHit2D hit in Physics2D.RaycastAll(originPoint, direction, distance, ~layersToIgnore))
			foreach (RaycastHit2D hit in Physics2D.LinecastAll(originPoint, originPoint + direction * distance, ~layersToIgnore))
			{
				//for every hit, be sure to not hit self and be sure to not hit colliders to ignore
				if (hit && hit.collider != selfCollider && collidersToIgnore.Contains(hit.collider) == false)
				{
					//if using circle collider, be sure is inside radius
					if (circleCollider == null || Vector2.Distance(hit.point, centerBounds) < radiusSelfCollider)
					{
						//save for events
						if (saveCollisionForEvents && currentCollisions.ContainsKey(hit.collider) == false)
						{
							currentCollisions.Add(hit.collider, hit);
							savedHitsForEvent.Add(hit.collider);    //save in a list, so we know which colliders hits with this function
						}

						//calculate nearest hit, only if self collider is not trigger and doesn't hit trigger collider
						if (selfCollider.isTrigger == false && hit.collider.isTrigger == false)
						{
							if (nearestHit == false || hit.distance < nearestHit.distance)
							{
								nearestHit = hit;
							}
						}
					}
				}
			}
		}

		void DebugRaycast(Vector2 originPoint, Vector2 direction, float distance, Color color)
		{
			////debug
			//if (drawDebugDuration > 0)
			//	Debug.DrawRay(originPoint, direction * distance, color, drawDebugDuration);					//when called by press the button, visualize for few seconds
			//else
			//	Debug.DrawRay(originPoint, direction * distance, color);									//else show at every update

			//debug
			if (drawDebugDuration > 0)
				Debug.DrawLine(originPoint, originPoint + direction * distance, color, drawDebugDuration);  //when called by press the button, visualize for few seconds
			else
				Debug.DrawLine(originPoint, originPoint + direction * distance, color);                     //else show at every update
		}

		void CheckCollisionEvents()
		{
			//call Enter or Stay
			foreach (Collider2D col in currentCollisions.Keys)
			{
				if (previousCollisions.Contains(col) == false)
				{
					if (col.isTrigger || selfCollider.isTrigger)
						onTriggerEnter?.Invoke(currentCollisions[col]);     //trigger enter
					else
						onCollisionEnter?.Invoke(currentCollisions[col]);   //collision enter
				}
				else
				{
					if (col.isTrigger || selfCollider.isTrigger)
						onTriggerStay?.Invoke(currentCollisions[col]);      //trigger stay
					else
						onCollisionStay?.Invoke(currentCollisions[col]);    //collision stay
				}
			}

			//call Exit
			foreach (Collider2D col in previousCollisions)
			{
				if (col && currentCollisions.ContainsKey(col) == false)
				{
					if (col.isTrigger || selfCollider.isTrigger)
						onTriggerExit?.Invoke(col);                         //trigger exit
					else
						onCollisionExit?.Invoke(col);                       //collision exit
				}
			}
		}

		void CheckCollisionEvents(Collider2D col)
		{
			if (col == null)
				return;

			//call Enter or Stay
			if (currentCollisions.ContainsKey(col))
			{
				if (previousCollisions.Contains(col) == false)
				{
					if (col.isTrigger || selfCollider.isTrigger)
						onTriggerEnter?.Invoke(currentCollisions[col]);     //trigger enter
					else
						onCollisionEnter?.Invoke(currentCollisions[col]);   //collision enter
				}
				else
				{
					if (col.isTrigger || selfCollider.isTrigger)
						onTriggerStay?.Invoke(currentCollisions[col]);      //trigger stay
					else
						onCollisionStay?.Invoke(currentCollisions[col]);    //collision stay
				}
			}
			//call Exit
			else if (previousCollisions.Contains(col))
			{
				if (col.isTrigger || selfCollider.isTrigger)
					onTriggerExit?.Invoke(col);                         //trigger exit
				else
					onCollisionExit?.Invoke(col);                       //collision exit
			}
		}

		void ResetVars()
		{
			//reset vars
			rightHits.Clear();
			leftHits.Clear();
			upHits.Clear();
			downHits.Clear();

			currentCollisions.Clear();
			previousCollisions.Clear();

			collidersToIgnore.Clear();
		}

		#endregion

		#region public API

		/// <summary>
		/// Update collisions
		/// </summary>
		public void UpdateCollisions()
		{
			//start only if there are all necessary components
			if (CheckComponents() == false)
				return;

			//reset current collisions (to recreate this frame)
			currentCollisions.Clear();

			//check collisions
			UpdateBounds();
			CheckCollisionsHorizontal();
			CheckCollisionsVertical();

			//check events (collision enter, stay, exit)
			CheckCollisionEvents();

			//copy collisions in another list for next frame checks
			previousCollisions.Clear();
			previousCollisions.AddRange(currentCollisions.Keys);
		}

		/// <summary>
		/// Update bounds informations
		/// </summary>
		public void UpdateBounds()
		{
			//check only if there are all necessary components
			if (CheckComponents() == false)
				return;

			//update bounds
			centerBounds = selfCollider.bounds.center;
			verticalExtents = selfCollider.bounds.extents.y;
			horizontalExtents = selfCollider.bounds.extents.x;
			radiusSelfCollider = circleCollider ? circleCollider.radius : 0;
			//centerBounds = (Vector2)transform.position + (selfCollider.offset * transform.lossyScale);
			//horizontalExtents = circleCollider ? circleCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y) : boxCollider.size.x * transform.lossyScale.x * 0.5f;
			//verticalExtents = circleCollider ? circleCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y) : boxCollider.size.y * transform.lossyScale.y * 0.5f;
			//radiusSelfCollider = horizontalExtents;	//is used only with circle collider, and horizontal and vertical is the same for circle collider

			//bounds limits
			upBounds = centerBounds.y + verticalExtents;
			downBounds = centerBounds.y - verticalExtents;
			rightBounds = centerBounds.x + horizontalExtents;
			leftBounds = centerBounds.x - horizontalExtents;
		}

		/// <summary>
		/// Return reachable position in direction (cause collisions), nearest to desired one
		/// </summary>
		/// <param name="direction"></param>
		/// <param name="desiredPosition"></param>
		/// <param name="addCollisionIfHitSomething">if hit something, add to hits and call collision event</param>
		/// <returns></returns>
		public Vector2 CalculateReachablePosition(EDirectionEnum direction, Vector2 desiredPosition, bool addCollisionIfHitSomething = true)
		{
			//raycast vars
			UpdateBounds();
			bounds = GetBounds(direction);
			numberOfRays = direction == EDirectionEnum.right || direction == EDirectionEnum.left ? numberOfHorizontalRays : numberOfVerticalRays;
			raycastOriginFirst = direction == EDirectionEnum.right || direction == EDirectionEnum.left ? new Vector2(centerBounds.x, downBounds + offsetRays) : new Vector2(leftBounds + offsetRays, centerBounds.y);
			raycastOriginSecond = direction == EDirectionEnum.right || direction == EDirectionEnum.left ? new Vector2(centerBounds.x, upBounds - offsetRays) : new Vector2(rightBounds - offsetRays, centerBounds.y);
			raycastLength = direction == EDirectionEnum.right || direction == EDirectionEnum.left ? horizontalExtents + Mathf.Abs(desiredPosition.x - transform.position.x) : verticalExtents + Mathf.Abs(desiredPosition.y - transform.position.y);

			//direction
			if (direction == EDirectionEnum.right || direction == EDirectionEnum.left)
				raycastDirection = direction == EDirectionEnum.right ? Vector2.right : Vector2.left;
			else
				raycastDirection = direction == EDirectionEnum.up ? Vector2.up : Vector2.down;

			//use raycasts
			for (int i = 0; i < numberOfRays; i++)
			{
				//from bottom to top, raycast right or left
				raycastOriginPoint = Vector2.Lerp(raycastOriginFirst, raycastOriginSecond, (float)i / (numberOfRays - 1));
				RayCastHitSomething(raycastOriginPoint, raycastDirection, raycastLength, out firstHit, hitsForCollisionEvent, addCollisionIfHitSomething);

				//adjust position
				if (firstHit)
				{
					if (direction == EDirectionEnum.right || direction == EDirectionEnum.left)
					{
						desiredPosition.x = firstHit.point.x - (bounds - transform.position.x);
					}
					else if (direction == EDirectionEnum.up || direction == EDirectionEnum.down)
					{
						desiredPosition.y = firstHit.point.y - (bounds - transform.position.y);
					}

					//add to hits, to save hitting this way (only if update mode is not None, otherwise will be never reset)
					if (addCollisionIfHitSomething && updateMode != EUpdateModes.None)
					{
						if (direction == EDirectionEnum.right) rightHits.Add(firstHit);
						else if (direction == EDirectionEnum.left) leftHits.Add(firstHit);
						else if (direction == EDirectionEnum.up) upHits.Add(firstHit);
						else if (direction == EDirectionEnum.down) downHits.Add(firstHit);
					}
				}

				//call collision enter event (to not wait until update collisions) - NB that if update mode is setted to None, will be never reset, so neither collision stay or collision exit will be called)
				if (addCollisionIfHitSomething)
				{
					foreach (Collider2D col in hitsForCollisionEvent)
					{
						//check collision enter (no collision stay because will be called on update collisions)
						if (currentCollisions.ContainsKey(col) && previousCollisions.Contains(col) == false)
						{
							if (col.isTrigger || selfCollider.isTrigger)
								onTriggerEnter?.Invoke(currentCollisions[col]);     //trigger enter
							else
								onCollisionEnter?.Invoke(currentCollisions[col]);   //collision enter

							//add to previous collisions, to not call again collision enter when update collisions
							previousCollisions.Add(col);
						}
					}
				}
			}

			return desiredPosition;
		}

		/// <summary>
		/// Return last hitting information in direction (call UpdateCollisions to have updated informations)
		/// </summary>
		/// <param name="direction"></param>
		/// <returns></returns>
		public bool IsHitting(EDirectionEnum direction)
		{
			switch (direction)
			{
				case EDirectionEnum.up:
					return upHits.Count > 0;
				case EDirectionEnum.right:
					return rightHits.Count > 0;
				case EDirectionEnum.left:
					return leftHits.Count > 0;
				case EDirectionEnum.down:
					return downHits.Count > 0;
				default:
					return false;
			}
		}

		/// <summary>
		/// Return last hits in direction (call UpdateCollisions to have updated informations)
		/// </summary>
		/// <param name="direction"></param>
		/// <returns></returns>
		public RaycastHit2D[] GetHits(EDirectionEnum direction)
		{
			switch (direction)
			{
				case EDirectionEnum.up:
					return upHits.ToArray();
				case EDirectionEnum.right:
					return rightHits.ToArray();
				case EDirectionEnum.left:
					return leftHits.ToArray();
				case EDirectionEnum.down:
					return downHits.ToArray();
				default:
					return null;
			}
		}

		/// <summary>
		/// Clear every hit
		/// </summary>
		public void ClearHits()
		{
			rightHits.Clear();
			leftHits.Clear();
			upHits.Clear();
			downHits.Clear();
		}

		/// <summary>
		/// Return last center of the bounds (call UpdateBounds to have updated informations)
		/// </summary>
		/// <returns></returns>
		public Vector2 GetCenterBound()
		{
			return centerBounds;
		}

		/// <summary>
		/// Return last length from center bounds to limit in direction (call UpdateBounds to have updated informations)
		/// </summary>
		/// <param name="direction"></param>
		/// <returns></returns>
		public float GetBoundsLength(EDirectionEnum direction)
		{
			switch (direction)
			{
				case EDirectionEnum.up:
					return verticalExtents;
				case EDirectionEnum.right:
					return horizontalExtents;
				case EDirectionEnum.left:
					return horizontalExtents;
				case EDirectionEnum.down:
					return verticalExtents;
				default:
					return 0;
			}
		}

		/// <summary>
		/// Return last bounds informations (call UpdateBounds to have updated informations)
		/// </summary>
		/// <param name="direction"></param>
		/// <returns></returns>
		public float GetBounds(EDirectionEnum direction)
		{
			switch (direction)
			{
				case EDirectionEnum.up:
					return upBounds;
				case EDirectionEnum.right:
					return rightBounds;
				case EDirectionEnum.left:
					return leftBounds;
				case EDirectionEnum.down:
					return downBounds;
				default:
					return 0;
			}
		}

		/// <summary>
		/// Return every collision event received in this frame. This return both collision and trigger events
		/// </summary>
		/// <returns></returns>
		public RaycastHit2D[] GetCurrentCollisionEvents()
		{
			return new List<RaycastHit2D>(currentCollisions.Values).ToArray();
		}

		/// <summary>
		/// Clear every collision event (this can cause to call again collision enter instead of collision stay)
		/// </summary>
		public void ClearCollisionEvents()
		{
			currentCollisions.Clear();
			previousCollisions.Clear();
		}

		/// <summary>
		/// Get if can hit this collider. Return will be "Ignore" if can't hit, "Trigger" if can call a trigger event, "Collision" if can call a collision event
		/// </summary>
		/// <param name="col"></param>
		/// <returns></returns>
		public ECollisionResponse CanHit(Collider2D col)
		{
			//ignore if collider is null or is self collider, or is in list of colliders to ignore
			if (col == null || selfCollider == null || col == selfCollider || collidersToIgnore.Contains(col))
				return ECollisionResponse.Ignore;

			//ignore if in layer to ignore
			if (layersToIgnore == (layersToIgnore | (1 << col.gameObject.layer)))
				return ECollisionResponse.Ignore;

			//if collider or self is trigger, return trigger
			if (col.isTrigger || selfCollider.isTrigger)
				return ECollisionResponse.Trigger;

			//else return collision
			return ECollisionResponse.Collision;
		}

		/// <summary>
		/// Ignore collisions with this collider. Will be reset when this object is disabled and re-enabled
		/// </summary>
		/// <param name="col"></param>
		/// <param name="ignore">Ignore collision with this collider or not</param>
		public void IgnoreCollision(Collider2D col, bool ignore = true)
		{
			//add to ignore list
			if (ignore)
			{
				if (collidersToIgnore.Contains(col) == false)
					collidersToIgnore.Add(col);
			}
			//remove from ignore list
			else
			{
				if (collidersToIgnore.Contains(col))
					collidersToIgnore.Remove(col);
			}
		}

		/// <summary>
		/// Clear every IgnoreCollision
		/// </summary>
		public void ClearIgnoreCollisions()
		{
			collidersToIgnore.Clear();
		}

		#endregion
	}
}