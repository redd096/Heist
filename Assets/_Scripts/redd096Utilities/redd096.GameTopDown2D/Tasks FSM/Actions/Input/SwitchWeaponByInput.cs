using UnityEngine;
using redd096.Attributes;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Actions/Input/Switch Weapon By Input")]
    public class SwitchWeaponByInput : ActionTask
    {
#if ENABLE_INPUT_SYSTEM
        enum EDelayMode { Never, OnlyMouseScroll, AlwaysButNumbers, Always }

        [Header("Necessary Components - default get in parent")]
        [SerializeField] WeaponComponent weaponComponent = default;
        [SerializeField] PlayerInput playerInput = default;

        [Header("Switch Weapon")]
        [SerializeField] string inputName = "Switch Weapon";
        [SerializeField] string inputSwitchToWeapon1 = "Switch To Weapon 1";
        [SerializeField] string inputSwitchToWeapon2 = "Switch To Weapon 2";
        [SerializeField] bool canSwitchWithMouseScroll = true;

        [Header("Delay Between Switches")]
        [SerializeField] EDelayMode delayMode = EDelayMode.OnlyMouseScroll;
        [DisableIf("delayMode", EDelayMode.Never)][SerializeField] float delayBetweenSwitches = 0.5f;

        float timeDelay;

        protected override void OnInitTask()
        {
            base.OnInitTask();

            //set references
            if (weaponComponent == null) weaponComponent = GetStateMachineComponent<WeaponComponent>();
            if (playerInput == null) playerInput = GetStateMachineComponent<PlayerInput>();

            //show warnings if not found
            if (playerInput && playerInput.actions == null)
                Debug.LogWarning("Miss Actions on PlayerInput on " + StateMachine);
        }

        public override void OnUpdateTask()
        {
            base.OnUpdateTask();

            if (weaponComponent == null || playerInput == null || playerInput.actions == null)
                return;

            //if press, switch to exact weapon
            if (SwitchWithNumbers())
            {
                return;
            }
            //else, try switch with button
            else if (SwitchWithButton())
            {
                return;
            }

            //else, switch with mouse scroll
            SwitchWithMouseScroll();
        }

        #region private API

        bool SwitchWithNumbers()
        {
            //delay works on numbers
            bool delayAffectsSwitch = delayMode == EDelayMode.Always;

            //block switch 'cause delay
            if (delayAffectsSwitch && timeDelay > Time.time)
                return false;

            //if press, switch to number 1
            if (playerInput.actions.FindAction(inputSwitchToWeapon1).WasPressedThisFrame())
            {
                //be sure has no equipped this weapon
                if (weaponComponent.IndexEquippedWeapon != 0)
                {
                    weaponComponent.SwitchWeaponTo(0);

                    //set delay if necessary
                    if (delayAffectsSwitch)
                        timeDelay = Time.time + delayBetweenSwitches;
                }
                return true;
            }
            //or switch to number 2
            else if (playerInput.actions.FindAction(inputSwitchToWeapon2).WasPressedThisFrame())
            {
                //be sure has no equipped this weapon
                if (weaponComponent.IndexEquippedWeapon != 1)
                {
                    weaponComponent.SwitchWeaponTo(1);

                    //set delay if necessary
                    if (delayAffectsSwitch)
                        timeDelay = Time.time + delayBetweenSwitches;
                }
                return true;
            }

            return false;
        }

        bool SwitchWithButton()
        {
            //delay works on button
            bool delayAffectsSwitch = delayMode == EDelayMode.Always || delayMode == EDelayMode.AlwaysButNumbers;

            //block switch 'cause delay
            if (delayAffectsSwitch && timeDelay > Time.time)
                return false;

            //on input down
            if (playerInput.actions.FindAction(inputName).WasPressedThisFrame())
            {
                //switch weapon
                weaponComponent.SwitchWeapon();

                //set delay if necessary
                if (delayAffectsSwitch)
                    timeDelay = Time.time + delayBetweenSwitches;

                return true;
            }

            return false;
        }

        bool SwitchWithMouseScroll()
        {
            //delay works on mouse scroll
            bool delayAffectsSwitch = delayMode != EDelayMode.Never;

            //block switch 'cause delay
            if (delayAffectsSwitch && timeDelay > Time.time)
                return false;

            //scroll with mouse
            else if (canSwitchWithMouseScroll && Mouse.current != null && Mouse.current.scroll.IsActuated())
            {
                //switch weapon
                weaponComponent.SwitchWeapon(Mouse.current.scroll.ReadValue().y > 0);

                //set delay if necessary
                if (delayAffectsSwitch)
                    timeDelay = Time.time + delayBetweenSwitches;

                return true;
            }

            return false;
        }

        #endregion
#else
        [HelpBox("This works only with new unity input system", HelpBoxAttribute.EMessageType.Error)]
        public string Error = "It works only with new unity input system";
#endif
    }
}