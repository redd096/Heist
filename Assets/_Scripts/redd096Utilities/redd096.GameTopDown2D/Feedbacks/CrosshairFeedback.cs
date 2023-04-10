using UnityEngine;
using redd096.Attributes;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Feedbacks/Crosshair Feedback")]
    public class CrosshairFeedback : FeedbackRedd096
    {
#if ENABLE_INPUT_SYSTEM
        [Header("Necessary Components - default get in parent")]
        [SerializeField] AimComponent aimComponent = default;
        [SerializeField] PlayerInput playerInput = default;

        [Header("Aim Sprite (already in scene/prefab)")]
        [SerializeField] GameObject aimSprite = default;
        [SerializeField] float minDistance = 1.5f;
        [SerializeField] float maxDistance = 2.5f;

        [Header("Mouse free or clamped by min and max?")]
        [SerializeField] bool mouseFree = true;
        [SerializeField] string mouseSchemeName = "KeyboardAndMouse";

        [Header("If using canvas Screen Space - default is main camera")]
        [Tooltip("Set if moving something in screen space (canvas) or world space")][SerializeField] bool isScreenSpace = true;
        [EnableIf("isScreenSpace")][SerializeField] Camera cam = default;

        protected override void OnEnable()
        {
            //get references
            if (aimComponent == null) aimComponent = GetComponentInParent<AimComponent>();
            if (playerInput == null) playerInput = GetComponentInParent<PlayerInput>();

            if (cam == null) cam = Camera.main;

            base.OnEnable();
        }

        void Update()
        {
            //update sprite position
            UpdateAimSpritePosition();
        }

        void UpdateAimSpritePosition()
        {
            if (aimSprite == null || aimComponent == null)
                return;

            //if using mouse
            if (playerInput && playerInput.currentControlScheme == mouseSchemeName)
            {
                //if mouse free
                if (mouseFree)
                {
                    aimSprite.transform.position = GetPosition(aimComponent.AimPositionNotNormalized);                                              //set current position
                }
                //else check distance
                else
                {
                    float distance = Vector2.Distance(transform.position, aimComponent.AimPositionNotNormalized);

                    if (distance > maxDistance)
                        aimSprite.transform.position = GetPosition((Vector2)transform.position + aimComponent.AimDirectionInput * maxDistance);     //max distance
                    else if (distance < minDistance)
                        aimSprite.transform.position = GetPosition((Vector2)transform.position + aimComponent.AimDirectionInput * minDistance);     //min distance
                    else
                        aimSprite.transform.position = GetPosition(aimComponent.AimPositionNotNormalized);                                          //current distance
                }
            }
            //if NOT using mouse
            else
            {
                //from 0 to 1, from min to max (gamepad input is from 0 to 1, so removing transform.position we will get that value)
                float value = Mathf.Lerp(minDistance, maxDistance, (aimComponent.AimPositionNotNormalized - (Vector2)transform.position).magnitude);

                //set position
                aimSprite.transform.position = GetPosition((Vector2)transform.position + aimComponent.AimDirectionInput * value);
            }
        }

        Vector2 GetPosition(Vector2 worldPosition)
        {
            if (isScreenSpace)
                return cam.WorldToScreenPoint(worldPosition);
            else
                return worldPosition;
        }
#else
        [HelpBox("This works only with new unity input system", HelpBoxAttribute.EMessageType.Error)]
        public string Error = "It works only with new unity input system";
#endif
    }
}