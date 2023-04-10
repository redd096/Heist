using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace redd096
{
    /// <summary>
    /// Get a text mesh pro Dropdown and makes it show hours to select
    /// </summary>
    [AddComponentMenu("redd096/MonoBehaviours/Time Picker")]
    public class TimePicker : MonoBehaviour
    {
        [Header("Dropdown to use - else can instantiate objects in ScrollView")]
        [SerializeField] TMP_Dropdown dropdown = default;
        [SerializeField] bool instantiateInsteadOfUseDropdown = false;
        [SerializeField] GameObject prefab = default;
        [SerializeField] ScrollSnap scrollSnap = default;

        [Header("On Awake")]
        [Tooltip("On awake set DateTime.Now as selected date and default result, to generate default dropdown and have result to read without open dropdown")][SerializeField] bool setDefaultOnAwake = false;

        [Header("Hours to show in dropdown")]
        [SerializeField] string hoursFormat = "HH:mm";
        [Tooltip("Step between hours, for example: 08:00, 08:30, 09:00 and so on")][SerializeField] int minutesStep = 30;

        [Header("Selected Date, to know if disable hours")]
        [SerializeField] string selectedDate = "16/12/2022";

        [Header("Not Clickable Hours")]
        [Tooltip("This only if selected date is today or earlier")][SerializeField] bool disableHoursBeforeNow = true;
        [Tooltip("If enabled, can for example disable hours if there isn't at least one hour of difference from now")][SerializeField] int additiveHoursAfterNow = 1;
        [Tooltip("Disable every hours before of this time")][SerializeField] string startHours = "08:00";
        [Tooltip("Disable every hours after of this time")][SerializeField] string endHours = "17:00";

        [Header("Result")]
        [SerializeField] string resultFormat = "HH:mm";
        [SerializeField] string result = "12:30";
        [SerializeField] UnityEvent<string> onResult = default;

        public ScrollSnap Scroll => scrollSnap;
        public List<string> SavedHours => savedHours;

        DateTime date = DateTime.Today;                         //selected date (or date to use)
        DateTime now = DateTime.Now;                            //DateTime.Now + additiveHours
        DateTime start = DateTime.Now;                          //startHours
        DateTime end = DateTime.Now;                            //endHours
        List<DateTime> hoursInDropdown = new List<DateTime>();  //hours showed in dropdown (hours found with GetPossibleHoursForDropdown) as DateTime

        List<string> savedHours = new List<string>();           //hours showed in dropdown (hours found with GetPossibleHoursForDropdown). These are changed only when call SetHoursInDropdown

        private void Awake()
        {
            //checks
            if (instantiateInsteadOfUseDropdown == false && dropdown == null)
                Debug.LogError("Miss dropdown");
            else if (instantiateInsteadOfUseDropdown && (prefab == null || scrollSnap == null))
                Debug.LogError("Miss prefab to instantiate or scrollSnap");

            if (setDefaultOnAwake)
            {
                //set today as default date
                selectedDate = DateTime.Now.ToString("dd/MM/yyyy");

                //set today as default result
                result = DateTime.Now.ToString(resultFormat);
            }

            //register to dropdown event
            if (dropdown)
                dropdown.onValueChanged.AddListener(OnClick);

            //generate default hours
            SetHoursInDropdown();
        }

        private void OnDestroy()
        {
            //unregister from dropdown event
            if (dropdown)
                dropdown.onValueChanged.RemoveListener(OnClick);
        }

        #region public API

        /// <summary>
        /// Used to update selected date by script
        /// </summary>
        /// <param name="date"></param>
        public void SetSelectedDate(string date)
        {
            selectedDate = date;

            //update also hours in dropdown
            SetHoursInDropdown();
        }

        /// <summary>
        /// Check if with this date there are hours in dropdown
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool ThereAreHoursThisDate(DateTime date)
        {
            //update list and check number of elements in list
            return GetPossibleHoursForDropdown(date.ToString("dd/MM/yyyy")).Count > 0;
        }

        /// <summary>
        /// Check if with selected date, there is this hour in the list
        /// </summary>
        /// <param name="hour"></param>
        /// <returns></returns>
        public bool IsSelectableThisHour(DateTime hour)
        {
            //update list
            GetPossibleHoursForDropdown(selectedDate);

            //and check if contains this hour in the list
            return hoursInDropdown.Contains(date.Add(hour.TimeOfDay));
        }

        /// <summary>
        /// Check if with this date, there is this hour in the list
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public bool IsSelectableThisDateTime(DateTime dt)
        {
            //update list
            GetPossibleHoursForDropdown(dt.ToString("dd/MM/yyyy"));

            //and check if contains this hour in the list
            return hoursInDropdown.Contains(dt);
        }

        /// <summary>
        /// Used to force result. For example to set a default value before open dropdown (to get from a variable instead of set DateTime.Now in awake)
        /// </summary>
        public void SetResult(string result)
        {
            this.result = result;
        }

        /// <summary>
        /// Update. Maybe it's been an hour and now the result is different
        /// </summary>
        public void UpdateResult()
        {
            SetHoursInDropdown();
        }

        #endregion

        #region private API

        void SetHoursInDropdown()
        {
            savedHours = GetPossibleHoursForDropdown(selectedDate);

            //update dropdown using selected date
            if (dropdown)
            {
                dropdown.ClearOptions();
                dropdown.AddOptions(savedHours);

                //set value
                int index = GetIndexStartTime();
                if (dropdown.value != index)
                    dropdown.value = index;         //set value
                else
                    OnClick(index);                 //if value isn't changed, call anyway to update result
            }
            //else instantiate objects in scrollview
            else if (instantiateInsteadOfUseDropdown)
            {
                //remove previous childs
                for (int i = scrollSnap.Content.childCount - 1; i >= 0; i--)
                {
                    Destroy(scrollSnap.Content.GetChild(i).gameObject);
                }

                //instantiate new ones
                for (int i = 0; i < savedHours.Count; i++)
                {
                    GameObject go = Instantiate(prefab, scrollSnap.Content);
                    go.GetComponentInChildren<TextMeshProUGUI>().text = savedHours[i];
                }

                //set value
                int index = GetIndexStartTime();
                scrollSnap.ScrollToItem(index); //TODO non si aggiorna perché è spento
                OnClick(index);
            }

        }

        int GetIndexStartTime()
        {
            DateTime currentTime = DateTime.Parse(result);
            DateTime nearestTime = start;
            int nearestIndex = 0;

            for (int i = 0; i < hoursInDropdown.Count; i++)
            {
                //if greater than nearest one, but not greater than current time (to show from last selected item - default is DateTime.Now)
                if (hoursInDropdown[i].TimeOfDay > nearestTime.TimeOfDay && hoursInDropdown[i].TimeOfDay <= currentTime.TimeOfDay)
                {
                    nearestTime = hoursInDropdown[i];
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }

        List<string> GetPossibleHoursForDropdown(string dateToUse)
        {
            //parse inspector variables
            DateTime.TryParse(startHours, out start);
            DateTime.TryParse(endHours, out end);
            DateTime.TryParse(dateToUse, out date);
            date = date.Date;                                                       //be sure date has only date and not TimeOfDay
            now = DateTime.Now.Add(new TimeSpan(additiveHoursAfterNow, 0, 0));      //now + additive hours

            //generate hours in dropdown
            hoursInDropdown = new List<DateTime>();
            List<string> hoursToShow = new List<string>();
            for (DateTime dt = date.Add(start.TimeOfDay); dt.TimeOfDay <= end.TimeOfDay; dt = dt.Add(new TimeSpan(0, minutesStep, 0)))
            {
                //only if enabled
                if (CanBeEnabledNow(dt) && IsInsideHoursRange(dt))
                {
                    hoursInDropdown.Add(dt);
                    hoursToShow.Add(dt.ToString(hoursFormat));
                }
            }

            return hoursToShow;
        }

        #endregion

        #region check to disable hours

        bool CanBeEnabledNow(DateTime dt)
        {
            //bool is disabled
            //or DateTime is greater than now + additive hours
            return disableHoursBeforeNow == false || dt > now;
        }

        bool IsInsideHoursRange(DateTime dt)
        {
            //check DateTime is between Start and End
            return dt.TimeOfDay >= start.TimeOfDay && dt.TimeOfDay <= end.TimeOfDay;
        }

        #endregion

        void OnClick(int index)
        {
            if (savedHours.Count > index)
            {
                //parse selected value
                DateTime dt = DateTime.Parse(savedHours[index]);
                result = dt.ToString(resultFormat);

                //on select value, call event
                onResult?.Invoke(result);
            }
        }
    }
}