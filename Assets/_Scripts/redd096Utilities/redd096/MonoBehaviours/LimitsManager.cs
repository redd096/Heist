using System.Collections.Generic;
using UnityEngine;

namespace redd096
{
    #region editor
#if UNITY_EDITOR

    using UnityEditor;

    [CustomEditor(typeof(LimitsManager))]
    public class LimitsManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);

            //button regen
            if (GUILayout.Button("Regen Limits"))
            {
                ((LimitsManager)target).RegenLimits();

                //repaint scene and set undo
                SceneView.RepaintAll();
                Undo.RegisterFullObjectHierarchyUndo(target, "Regen Limits");
            }

            //button destroy
            if (GUILayout.Button("Destroy Limits"))
            {
                ((LimitsManager)target).DestroyLimits();

                //repaint scene and set undo
                SceneView.RepaintAll();
                Undo.RegisterFullObjectHierarchyUndo(target, "Destroy Limits");
            }
        }
    }

#endif
    #endregion

    [AddComponentMenu("redd096/MonoBehaviours/Limits Manager")]
    public class LimitsManager : MonoBehaviour
    {
        [Header("If there aren't 4 walls, regen on play")]
        [SerializeField] bool canRegenOnPlay = true;
        [SerializeField] List<Transform> walls = new List<Transform>();

        Camera cam;

        float width;
        float height;

        void Start()
        {
            //do only if can regen on play
            if (canRegenOnPlay == false)
                return;

            cam = Camera.main;

            //if there aren't 4 walls, destroy everything and recreate
            if (walls == null || walls.Count < 4)
            {
                DestroyLimits();
                CreateLimits();
            }
        }

        void Update()
        {
            //if there aren't 4 walls, don't resize
            if (walls == null || walls.Count < 4)
                return;

            //if different size, reset limits
            if (Screen.width != width || Screen.height != height)
            {
                width = Screen.width;
                height = Screen.height;

                SetLimits();
            }
        }

        #region public API

        public void RegenLimits()
        {
            cam = Camera.main;

            //recreate from zero
            DestroyLimits();
            CreateLimits();
            SetLimits();
        }

        public void DestroyLimits()
        {
            //remove every child
            foreach (Transform child in transform)
            {
#if UNITY_EDITOR
                EditorApplication.delayCall += () => DestroyImmediate(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }

            //reset list
            walls = new List<Transform>();
        }

        #endregion

        void CreateLimits()
        {
            //then create walls
            for (int i = 0; i < 4; i++)
            {
                GameObject wall = new GameObject("Wall", typeof(BoxCollider2D), typeof(SpriteRenderer));
                wall.transform.SetParent(transform);

                walls.Add(wall.transform);
            }
        }

        void SetLimits()
        {
            //get size of the walls
            float depthScreen = cam.WorldToViewportPoint(transform.position).z;
            Vector3 size = GetScale(depthScreen);

            float movementX = size.x / 2;
            float movementY = size.y / 2;

            CreateWall(walls[0], new Vector3(1, 0.5f, depthScreen), size, new Vector3(movementX, 0, 0));        //right
            CreateWall(walls[1], new Vector3(0, 0.5f, depthScreen), size, new Vector3(-movementX, 0, 0));       //left
            CreateWall(walls[2], new Vector3(0.5f, 1, depthScreen), size, new Vector3(0, movementY, 0));        //up
            CreateWall(walls[3], new Vector3(0.5f, 0, depthScreen), size, new Vector3(0, -movementY, 0));       //down
        }

        #region set limits

        Vector3 GetScale(float depth)
        {
            //get size for the wall from the screen width and height
            Vector3 left = cam.ViewportToWorldPoint(new Vector3(0, 0, depth));
            Vector3 right = cam.ViewportToWorldPoint(new Vector3(1.5f, 1.5f, depth));

            Vector3 size = right - left;

            return new Vector3(size.x, size.y, 1);
        }

        void CreateWall(Transform wall, Vector3 viewportPoint, Vector3 size, Vector3 movement)
        {
            //move and set size
            wall.position = cam.ViewportToWorldPoint(viewportPoint);
            wall.localScale = size;
            wall.position += movement;
        }

        #endregion
    }
}