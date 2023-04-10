using UnityEngine;
using redd096.Attributes;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Actions/Input/Attack By Input")]
    public class AttackByInput : ActionTask
    {
#if ENABLE_INPUT_SYSTEM
        [Header("Necessary Components - default get in parent")]
        [SerializeField] WeaponComponent weaponComponent = default;
        [SerializeField] PlayerInput playerInput = default;

        [Header("Attack")]
        [SerializeField] string inputName = "Attack";

        bool isAttacking;
        bool inputValue;

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

            //get input
            inputValue = playerInput.actions.FindAction(inputName).IsPressed();

            //when press, attack
            if (inputValue && isAttacking == false)
            {
                isAttacking = true;

                if (weaponComponent.CurrentWeapon)
                    weaponComponent.CurrentWeapon.PressAttack();
            }
            //on release, stop it if automatic shoot
            else if (inputValue == false && isAttacking)
            {
                isAttacking = false;

                if (weaponComponent.CurrentWeapon)
                    weaponComponent.CurrentWeapon.ReleaseAttack();
            }
        }

        protected override void OnExitTask()
        {
            base.OnExitTask();

            //be sure to stop attack
            if (isAttacking)
            {
                isAttacking = false;

                if (weaponComponent && weaponComponent.CurrentWeapon)
                    weaponComponent.CurrentWeapon.ReleaseAttack();
            }
        }
#else
        [HelpBox("This works only with new unity input system", HelpBoxAttribute.EMessageType.Error)]
        public string Error = "It works only with new unity input system";
#endif
    }
}