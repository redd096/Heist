using UnityEngine;
using redd096.Attributes;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Components/Camera Component")]
    public class CameraComponent : MonoBehaviour
    {
        enum EUpdateMode { Update, FixedUpdate, LateUpdate }

        [Header("Camera - by default is MainCamera")]
        [SerializeField] Camera cameraToControl = default;

        [Header("Update camera position to follow this object")]
        [SerializeField] bool updatePosition = true;
        [Tooltip("Use LateUpdate or Update to move the camera?")][EnableIf("updatePosition")][SerializeField] EUpdateMode updateMode = EUpdateMode.LateUpdate;
        [EnableIf("updatePosition")][SerializeField] Vector3 offsetPosition = new Vector3(0, 0, -10);

        [Header("Clamp camera position to pixelsPerUnit")]
        [SerializeField] bool usePixelClamp = false;
        [EnableIf("usePixelClamp")][SerializeField] float pixelsPerUnit = 16;

        [Header("Drop Camera On Death (necessary HealthComponent - default get from this gameObject)")]
        [SerializeField] bool dropCameraOnDeath = true;
        [EnableIf("dropCameraOnDeath")][SerializeField] HealthComponent healthComponent = default;

        [Header("Confine camera in a cube")]
        [SerializeField] bool isConfined = false;
        [EnableIf("isConfined")][SerializeField] Vector3 minConfiner = -Vector3.one * 10;
        [EnableIf("isConfined")][SerializeField] Vector3 maxConfiner = Vector3.one * 10;
        [SerializeField] ShowDebugRedd096 showConfineArea = Color.red;

        Transform cameraParent;
        Vector3 movement;
        Vector3 oldPosition;
        Vector3 newPosition;

        void OnDrawGizmos()
        {
            if (showConfineArea)
            {
                Gizmos.color = showConfineArea.ColorDebug;

                //front quad
                Gizmos.DrawLine(minConfiner, new Vector3(maxConfiner.x, minConfiner.y, minConfiner.z));
                Gizmos.DrawLine(new Vector3(maxConfiner.x, minConfiner.y, minConfiner.z), new Vector3(maxConfiner.x, maxConfiner.y, minConfiner.z));
                Gizmos.DrawLine(new Vector3(maxConfiner.x, maxConfiner.y, minConfiner.z), new Vector3(minConfiner.x, maxConfiner.y, minConfiner.z));
                Gizmos.DrawLine(new Vector3(minConfiner.x, maxConfiner.y, minConfiner.z), minConfiner);

                //back quad
                Gizmos.DrawLine(maxConfiner, new Vector3(minConfiner.x, maxConfiner.y, maxConfiner.z));
                Gizmos.DrawLine(new Vector3(minConfiner.x, maxConfiner.y, maxConfiner.z), new Vector3(minConfiner.x, minConfiner.y, maxConfiner.z));
                Gizmos.DrawLine(new Vector3(minConfiner.x, minConfiner.y, maxConfiner.z), new Vector3(maxConfiner.x, minConfiner.y, maxConfiner.z));
                Gizmos.DrawLine(new Vector3(maxConfiner.x, minConfiner.y, maxConfiner.z), maxConfiner);

                //depth
                Gizmos.DrawLine(minConfiner, new Vector3(minConfiner.x, minConfiner.y, maxConfiner.z));
                Gizmos.DrawLine(new Vector3(maxConfiner.x, minConfiner.y, minConfiner.z), new Vector3(maxConfiner.x, minConfiner.y, maxConfiner.z));
                Gizmos.DrawLine(new Vector3(maxConfiner.x, maxConfiner.y, minConfiner.z), maxConfiner);
                Gizmos.DrawLine(new Vector3(minConfiner.x, maxConfiner.y, minConfiner.z), new Vector3(minConfiner.x, maxConfiner.y, maxConfiner.z));

                Gizmos.color = Color.white;
            }
        }

        void OnEnable()
        {
            //get references
            if (cameraToControl == null) cameraToControl = Camera.main;
            if (healthComponent == null) healthComponent = GetComponent<HealthComponent>();

            //set cam parent
            if (cameraToControl && cameraParent == null)
            {
                cameraParent = new GameObject("Camera Parent (camera component)").transform;
                cameraParent.SetParent(cameraToControl.transform.parent);               //set same parent (if camera was child of something)
                cameraParent.localPosition = cameraToControl.transform.localPosition;   //set start local position
                cameraToControl.transform.SetParent(cameraParent);					    //set camera parent
            }

            //add events
            if (healthComponent)
            {
                healthComponent.onDie += OnDie;
            }
        }

        void OnDisable()
        {
            //remove events
            if (healthComponent)
            {
                healthComponent.onDie -= OnDie;
            }
        }

        void Update()
        {
            //update camera position if necessary
            if (updatePosition && updateMode == EUpdateMode.Update)
            {
                if (cameraParent)
                {
                    MoveCamera();
                }
            }
        }

        private void FixedUpdate()
        {
            //update camera position if necessary
            if (updatePosition && updateMode == EUpdateMode.FixedUpdate)
            {
                if (cameraParent)
                {
                    MoveCamera();
                }
            }
        }

        void LateUpdate()
        {
            //update camera position if necessary
            if (updatePosition && updateMode == EUpdateMode.LateUpdate)
            {
                if (cameraParent)
                {
                    MoveCamera();
                }
            }
        }

        void OnDie(HealthComponent whoDied, Character whoHit)
        {
            //drop camera on death, if setted
            if (dropCameraOnDeath && cameraToControl)
            {
                //only if child of this gameObject
                foreach (Camera childCam in GetComponentsInChildren<Camera>())
                {
                    if (childCam == cameraToControl)
                    {
                        cameraToControl.transform.SetParent(null);
                        break;
                    }
                }
            }
        }

        #region private API

        void MoveCamera()
        {
            //move camera clamped to pixelPerUnit
            if (usePixelClamp)
            {
                movement = (transform.position + offsetPosition) - cameraParent.position;   //calculate movement
                movement = PixelClamp(movement);                                            //clamp movement to pixelPerUnit
                oldPosition = PixelClamp(cameraParent.position);                            //clamp old position to pixelsPerUnit

                newPosition = oldPosition + movement;                                       //set new position using clamped values
            }
            //just move camera to position + offset
            else
            {
                newPosition = transform.position + offsetPosition;
            }

            //set position (confined or not)
            cameraParent.position = isConfined ? ConfinedPosition(newPosition) : newPosition;
        }

        Vector3 PixelClamp(Vector3 position)
        {
            //round position * pixelsPerUnit, then divide by pixelsPerUnit
            return new Vector3(Mathf.RoundToInt(position.x * pixelsPerUnit), Mathf.RoundToInt(position.y * pixelsPerUnit), Mathf.RoundToInt(position.z * pixelsPerUnit)) / pixelsPerUnit;
        }

        Vector3 ConfinedPosition(Vector3 position)
        {
            //confine x, y, z
            return new Vector3(
                Mathf.Clamp(position.x, minConfiner.x, maxConfiner.x),
                Mathf.Clamp(position.y, minConfiner.y, maxConfiner.y),
                Mathf.Clamp(position.z, minConfiner.z, maxConfiner.z));
        }

        #endregion

        #region public API

        /// <summary>
        /// Set confiners using box collider 2D
        /// </summary>
        /// <param name="boxCollider"></param>
        public void SetConfine(BoxCollider2D boxCollider)
        {
            if (boxCollider == null)
                return;

            //calculate box collider size
            Vector2 center = (Vector2)boxCollider.transform.position + boxCollider.offset * boxCollider.transform.lossyScale;
            Vector2 halfSize = boxCollider.size * boxCollider.transform.lossyScale * 0.5f;

            //set confiner (keep Z already setted)
            isConfined = true;
            minConfiner = new Vector3(center.x - halfSize.x, center.y - halfSize.y, minConfiner.z);
            maxConfiner = new Vector3(center.x + halfSize.x, center.y + halfSize.y, maxConfiner.z);
        }

        /// <summary>
        /// Set confiners using box collider
        /// </summary>
        /// <param name="boxCollider"></param>
        public void SetConfine(BoxCollider boxCollider)
        {
            if (boxCollider == null)
                return;

            //calculate box collider size
            Vector3 center = boxCollider.transform.position + Vector3.Scale(boxCollider.center, boxCollider.transform.lossyScale);
            Vector3 halfSize = Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale) * 0.5f;

            //set confiner
            isConfined = true;
            minConfiner = new Vector3(center.x - halfSize.x, center.y - halfSize.y, center.z - halfSize.z);
            maxConfiner = new Vector3(center.x + halfSize.x, center.y + halfSize.y, center.z + halfSize.z);
        }

        /// <summary>
        /// Set min confiner
        /// </summary>
        /// <param name="minConfine"></param>
        public void SetMinConfine(Vector3 minConfine)
        {
            isConfined = true;
            minConfiner = minConfine;
        }

        /// <summary>
        /// Set max confiner
        /// </summary>
        /// <param name="maxConfine"></param>
        public void SetMaxConfine(Vector3 maxConfine)
        {
            isConfined = true;
            maxConfiner = maxConfine;
        }

        /// <summary>
        /// Set confined inside already setted min and max confiner
        /// </summary>
        public void Confine()
        {
            isConfined = true;
        }

        /// <summary>
        /// Remove confiners
        /// </summary>
        public void UnConfine()
        {
            isConfined = false;
        }

        #endregion
    }
}