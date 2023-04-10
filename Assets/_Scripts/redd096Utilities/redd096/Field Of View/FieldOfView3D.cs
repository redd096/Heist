using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using redd096.Attributes;

namespace redd096
{
    [AddComponentMenu("redd096/Field Of View/Field Of View 3D")]
    public class FieldOfView3D : MonoBehaviour
    {
        [Header("Field of View")]
        public float viewRadius = 10;
        [Range(0, 360)] public float viewAngle = 30;
        [SerializeField] Transform pointDirection = default;

        [Header("Layers")]
        [SerializeField] LayerMask targetMask = -1;
        [SerializeField] LayerMask obstacleMask = default;

        [Header("DEBUG")]
        [SerializeField] bool drawDebugFieldOfView = false;
        [SerializeField] Color colorDebugFieldOfView = Color.cyan;
        [SerializeField] bool drawDebugLineToTargets = false;
        [SerializeField] Color colorDebugLineToTargets = Color.red;

#if UNITY_EDITOR
        [Button("Update Targets")] void ButtonUpdateTargets() { FindVisibleTargets(); UnityEditor.SceneView.RepaintAll(); }
#endif

        List<Transform> visibleTargets = new List<Transform>();

        public List<Transform> VisibleTargets => visibleTargets;
        public Vector3 StartDirection => pointDirection ? (pointDirection.position - transform.position).normalized : transform.forward;   //direction from our to pointDirection - else use forward

        private void OnDrawGizmos()
        {
            if (drawDebugFieldOfView)
            {
#if UNITY_EDITOR
                //draw circle
                UnityEditor.Handles.color = colorDebugFieldOfView;
                UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, viewRadius);
                UnityEditor.Handles.color = Color.white;
#endif

                //draw 2 line to see cone vision
                Gizmos.color = colorDebugFieldOfView;
                Vector3 ViewAngleA = DirFromAngle(-viewAngle / 2, false);
                Vector3 ViewAngleB = DirFromAngle(viewAngle / 2, false);
                Gizmos.DrawLine(transform.position, transform.position + ViewAngleA * viewRadius);
                Gizmos.DrawLine(transform.position, transform.position + ViewAngleB * viewRadius);
            }
            if (drawDebugLineToTargets)
            {
                //show targets
                Gizmos.color = colorDebugLineToTargets;
                foreach (Transform visibleTarget in visibleTargets)
                {
                    Gizmos.DrawLine(transform.position, visibleTarget.position);
                }
            }

            Gizmos.color = Color.white;
        }

        void OnEnable()
        {
            //start look every tot seconds
            StartCoroutine("FindTargetsWithDelay", 0.2f);
        }

        IEnumerator FindTargetsWithDelay(float delay)
        {
            while (true)
            {
                //wait then find targets
                yield return new WaitForSeconds(delay);
                FindVisibleTargets();
            }
        }

        #region public API

        public void FindVisibleTargets()
        {
            visibleTargets.Clear();

            //find targets
            Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

            //foreach target
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                //check is in angle
                Transform target = targetsInViewRadius[i].transform;
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                if (Vector3.Angle(StartDirection, dirToTarget) < viewAngle / 2)
                {
                    //throw raycast to see is not hide by an obstacle
                    float distToTarget = Vector3.Distance(transform.position, target.position);
                    if (Physics.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask) == false)
                    {
                        //add to list
                        visibleTargets.Add(target);
                    }
                }
            }
        }

        public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
        {
            //direction cone vision
            if (angleIsGlobal == false)
            {
                //rotate to point direction
                if (pointDirection)
                {
                    float angle = Vector3.SignedAngle(transform.forward, StartDirection, Vector3.up);
                    Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);

                    angleInDegrees += rotation.eulerAngles.y;
                }
                //rotate to forward
                else
                {
                    angleInDegrees += transform.eulerAngles.y;
                }
            }

            //direction from start direction (used to create one of two line of cone vision)
            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        }

        #endregion
    }
}