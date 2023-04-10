using redd096.Attributes;
using UnityEngine;
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Actions/Attack/Attack With Weapon Component")]
    public class AttackWithWeaponComponent : ActionTask
    {
        [Header("Necessary Components - default get in parent")]
        [SerializeField] WeaponComponent weaponComponent = default;

        [Header("Attack")]
        [SerializeField] float timeBeforeFirstAttack = 0.5f;
        [SerializeField] float durationAttack = 1;
        [SerializeField] float timeBetweenAttacks = 0.5f;

        [Header("Call OnCompleteTask after few attacks (0 = no call)")]
        [ColorGUI(AttributesUtility.EColor.Orange)][SerializeField] int numberOfAttacks = 0;

        float timerBeforeAttack;    //time between attacks
        float timerAttack;          //duration attack
        bool isAttacking;
        int numberAttacksDone;

        protected override void OnInitTask()
        {
            base.OnInitTask();

            //get references
            if (weaponComponent == null) weaponComponent = GetStateMachineComponent<WeaponComponent>();
        }

        protected override void OnEnterTask()
        {
            base.OnEnterTask();

            //be sure is not attacking and set timer for next attack
            StopAttack();
            timerBeforeAttack = Time.time + timeBeforeFirstAttack;
            numberAttacksDone = 0;
        }

        protected override void OnExitTask()
        {
            base.OnExitTask();

            //be sure to stop attack
            StopAttack();
        }

        public override void OnUpdateTask()
        {
            base.OnUpdateTask();

            //wait before attack
            if (timerBeforeAttack > Time.time)
                return;

            //if not attacking, start attack
            if (isAttacking == false)
            {
                StartAttack();

                //set duration attack
                timerAttack = Time.time + durationAttack;
            }
            //if attacking, wait timer then stop attack
            else if (Time.time > timerAttack)
            {
                StopAttack();
                CompleteAttack();

                //set time before next attack
                timerBeforeAttack = Time.time + timeBetweenAttacks;
            }
        }

        #region private API

        void StartAttack()
        {
            isAttacking = true;

            //if there is a weapon equipped, start attack
            if (weaponComponent && weaponComponent.CurrentWeapon)
                weaponComponent.CurrentWeapon.PressAttack();
        }

        void StopAttack()
        {
            isAttacking = false;

            //if there is a weapon equipped, stop attack
            if (weaponComponent && weaponComponent.CurrentWeapon)
                weaponComponent.CurrentWeapon.ReleaseAttack();
        }

        void CompleteAttack()
        {
            //call event if reach number of attack
            numberAttacksDone++;
            if (numberOfAttacks > 0 && numberAttacksDone >= numberOfAttacks)
                CompleteTask();
        }

        #endregion
    }
}