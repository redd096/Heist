using UnityEngine;

namespace redd096
{
    [AddComponentMenu("redd096/MonoBehaviours/Look Camera")]
    public class LookCamera : MonoBehaviour
    {
        [Header("Important")]
        [Tooltip("Don't follow y axis of the camera (up or down)")]
        [SerializeField] bool ignoreYAxis = false;

        [Header("Override, if you don't want to use defaults")]
        [Tooltip("Default is main camera")]
        [SerializeField] Camera cam;
        [Tooltip("Default is this transform")]
        [SerializeField] Transform transformToRotate;

        void Start()
        {
            //get main camera
            if (cam == null)
                cam = Camera.main;

            //get this gameObject
            if (transformToRotate == null)
                transformToRotate = transform;
        }

        void Update()
        {
            if (cam && transformToRotate)
            {
                //look at camera
                Vector3 cameraPosition = cam.transform.position;

                //ignore y axis
                if (ignoreYAxis)
                    cameraPosition.y = transformToRotate.position.y;

                //get look rotation
                Vector3 direction = cameraPosition - transformToRotate.position;
                Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

                //set rotation
                transformToRotate.rotation = rotation;
            }
        }
    }
}