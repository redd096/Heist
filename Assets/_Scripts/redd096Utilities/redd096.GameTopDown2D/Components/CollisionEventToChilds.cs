using UnityEngine;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Components/Collision Event To Childs")]
    public class CollisionEventToChilds : MonoBehaviour
    {
        public System.Action<Collision2D> onCollisionEnter2D { get; set; }
        public System.Action<Collision2D> onCollisionStay2D { get; set; }
        public System.Action<Collision2D> onCollisionExit2D { get; set; }
        public System.Action<Collider2D> onTriggerEnter2D { get; set; }
        public System.Action<Collider2D> onTriggerStay2D { get; set; }
        public System.Action<Collider2D> onTriggerExit2D { get; set; }

        void OnCollisionEnter2D(Collision2D collision)
        {
            onCollisionEnter2D?.Invoke(collision);
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            onCollisionStay2D?.Invoke(collision);
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            onCollisionExit2D?.Invoke(collision);
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            onTriggerEnter2D?.Invoke(collision);
        }

        void OnTriggerStay2D(Collider2D collision)
        {
            onTriggerStay2D?.Invoke(collision);
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            onTriggerExit2D?.Invoke(collision);
        }
    }
}