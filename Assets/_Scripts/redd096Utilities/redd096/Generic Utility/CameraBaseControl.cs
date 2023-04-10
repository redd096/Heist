using System.Collections;
using UnityEngine;

namespace redd096
{
    [System.Serializable]
    public class CameraBaseControl
    {
        [Header("Important")]
        [SerializeField] Vector3 cameraOffset = Vector3.zero;
        [SerializeField] bool firstPerson = false;

        [Header("Smooth")]
        [SerializeField] float smoothPosition = 50;
        [SerializeField] float smoothRotation = 15;

        [Header("Sensitivity")]
        [SerializeField] float sensitivityX = 200;
        [SerializeField] float sensitivityY = 100;

        [Header("Clamp X")]
        [SerializeField] float minX = -360f;
        [SerializeField] float maxX = 360f;

        [Header("Clamp Y")]
        [SerializeField] float minY = -60f;
        [SerializeField] float maxY = 60f;

        float rotX, rotY;
        Transform cam;
        Transform player;

        #region IMPORTANT

        /// <summary>
        /// Call in DrawGizmos. Show the cam position with this offset
        /// </summary>
        public void OnDrawGizmos(Transform cam, Transform player)
        {
            Gizmos.color = Color.green;

            //get cam position
            Quaternion rotation = firstPerson ? player.rotation : cam.rotation;
            Vector3 camPosition = player.position + WorldToLocalDirection(cameraOffset, rotation);

            //draw sphere at position and line to forward
            Gizmos.DrawWireSphere(camPosition, 0.2f);
            Gizmos.DrawRay(camPosition, WorldToLocalDirection(Vector3.forward, rotation));
        }

        /// <summary>
        /// Call in Start or Awake. Set references and default rotation
        /// </summary>
        public void StartDefault(Transform cam, Transform player, bool setDefault = true)
        {
            //set references
            this.cam = cam;
            this.player = player;

            if (setDefault)
            {
                //set rotX and rotY
                SetDefaultRotation();

                //set position and rotation
                SetPositionImmediatly();
                SetRotationImmediatly();
            }
        }

        /// <summary>
        /// Call in Update. Make the camera follow the player
        /// </summary>
        public void UpdateCameraPosition()
        {
            //use player for first person rotation, because camera rotate also on X axis (Mouse Y)
            //you can use camera rotation if you want the camera to move on top and bottom of the player, like 3rd person
            Quaternion rotation = firstPerson ? player.rotation : cam.rotation;

            cam.position = Vector3.Slerp(cam.position, player.position + WorldToLocalDirection(cameraOffset, rotation), Time.deltaTime * smoothPosition);
        }

        /// <summary>
        /// Call in Update. Rotate the camera by input
        /// </summary>
        public void UpdateRotation(float inputX, float inputY)
        {
            //get the rotation we want
            Vector3 camEuler;
            Vector3 playerEuler;
            GetRotations(inputX, inputY, out camEuler, out playerEuler);

            //from vector3 to quaternion
            Quaternion playerRotation;
            Quaternion camRotation;
            FromEulerToRotation(camEuler, playerEuler, out camRotation, out playerRotation);

            //do rotation
            DoRotation(camRotation, playerRotation);
            //DoRotation(camEuler, playerEuler);   //euler -> no smooth
        }

        #endregion

        #region private API

        #region utility

        /// <summary>
        /// Return the local direction
        /// </summary>
        Vector3 WorldToLocalDirection(Vector3 worldDirection, Quaternion rotation)
        {
            return rotation * worldDirection;
        }

        /// <summary>
        /// Clamp from min to max
        /// </summary>
        float ClampAngle(float angle, float min, float max)
        {
            //can't go under -360
            if (angle < -360)
                angle += 360;

            //can't go over 360
            if (angle > 360)
                angle -= 360;

            return Mathf.Clamp(angle, min, max);
        }

        /// <summary>
        /// When we need negative value, like -90 instead of 270, for example with clamp from -90 to 90
        /// </summary>
        float NegativeAngle(float angle, float min, float max)
        {
            //if greater than 180, subtract 360 to get negative value
            if (angle > 180)
                angle -= 360;

            return Mathf.Clamp(angle, min, max);
        }

        #endregion

        #region set immediatly

        void SetPositionImmediatly()
        {
            Quaternion rotation = firstPerson ? player.rotation : cam.rotation;

            //set camera position
            cam.position = player.position + WorldToLocalDirection(cameraOffset, rotation);
        }

        void SetRotationImmediatly()
        {
            //set camera and player rotation
            cam.rotation = Quaternion.Euler(-rotY, rotX, 0);
            player.rotation = Quaternion.Euler(0, rotX, 0);
        }

        #endregion

        #region rotation

        void GetRotations(float inputX, float inputY, out Vector3 camEuler, out Vector3 playerEuler)
        {
            //we use float, so we can clamp easy
            rotX += inputX * sensitivityX * Time.deltaTime;
            rotY += inputY * sensitivityY * Time.deltaTime;

            rotX = ClampAngle(rotX, minX, maxX);
            rotY = ClampAngle(rotY, minY, maxY);

            //the rotation we want for cam and player, on world space
            camEuler = new Vector3(-rotY, rotX, 0);
            playerEuler = new Vector3(0, rotX, 0);
        }

        void FromEulerToRotation(Vector3 camEuler, Vector3 playerEuler, out Quaternion camRotation, out Quaternion playerRotation)
        {
            //from vector3 to quaternion
            camRotation = Quaternion.Euler(camEuler);
            playerRotation = Quaternion.Euler(playerEuler);
        }

        void DoRotation(Quaternion camRotation, Quaternion playerRotation)
        {
            //set rotations
            cam.rotation = Quaternion.Slerp(cam.rotation, camRotation, Time.deltaTime * smoothRotation);
            player.rotation = Quaternion.Slerp(player.rotation, playerRotation, Time.deltaTime * smoothRotation);
        }

        void DoRotation(Vector3 camEuler, Vector3 playerEuler)
        {
            //set rotations - no smooth, cause there is a problem with gimbal lock
            cam.eulerAngles = camEuler;
            player.eulerAngles = playerEuler;
        }

        #endregion

        #endregion

        #region public API

        #region set rotation

        /// <summary>
        /// Set default rotation based on player and camera current rotation
        /// </summary>
        public void SetDefaultRotation()
        {
            //maybe we need negative values, like -90 instead of 270, for example with clamp from -90 to 90
            float rotationX = NegativeAngle(player.eulerAngles.y, minX, maxX);
            float rotationY = NegativeAngle(cam.eulerAngles.x, minY, maxY);

            //final set
            rotX = rotationX;
            rotY = -rotationY;
        }

        /// <summary>
        /// Set player and camera rotation
        /// </summary>
        public void SetRotation(Vector3 euler)
        {
            //maybe we need negative values, like -90 instead of 270, for example with clamp from -90 to 90
            float rotationX = NegativeAngle(euler.y, minX, maxX);
            float rotationY = NegativeAngle(euler.x, minY, maxY);

            //final set
            rotX = rotationX;
            rotY = -rotationY;
        }

        /// <summary>
        /// Set player and camera rotation
        /// </summary>
        public void SetRotation(Quaternion rotation)
        {
            Vector3 euler = rotation.eulerAngles;

            //maybe we need negative values, like -90 instead of 270, for example with clamp from -90 to 90
            float rotationX = NegativeAngle(euler.y, minX, maxX);
            float rotationY = NegativeAngle(euler.x, minY, maxY);

            //final set
            rotX = rotationX;
            rotY = -rotationY;
        }

        #endregion

        #region add rotation

        /// <summary>
        /// Add rotation to camera and player, like you moved the mouse
        /// </summary>
        public void AddRotation(float addX, float addY)
        {
            //we add the values
            float rotationX = rotX + addX;
            float rotationY = rotY + addY;

            //maybe we need negative values, like -90 instead of 270, for example with clamp from -90 to 90
            rotationX = NegativeAngle(rotationX, minX, maxX);
            rotationY = NegativeAngle(rotationY, minY, maxY);

            //final set
            rotX = rotationX;
            rotY = rotationY;
        }

        /// <summary>
        /// Add smooth rotation to camera and player, like you moved the mouse
        /// </summary>
        public IEnumerator Smooth_AddRotation(float addX, float addY, float durationAnimation)
        {
            //we use local variable, so can't be modified from player during animation
            float startX = rotX;
            float startY = rotY;
            float rotationX = rotX + addX;
            float rotationY = rotY + addY;

            //maybe we need negative values, like -90 instead of 270, for example with clamp from -90 to 90
            rotationX = NegativeAngle(rotationX, minX, maxX);
            rotationY = NegativeAngle(rotationY, minY, maxY);

            //animation
            float delta = 0;
            while (delta < 1)
            {
                delta += Time.deltaTime / durationAnimation;

                rotX = Mathf.Lerp(startX, rotationX, delta);
                rotY = Mathf.Lerp(startY, rotationY, delta);

                yield return null;
            }

            //final set
            rotX = rotationX;
            rotY = rotationY;
        }

        #endregion

        #endregion
    }
}