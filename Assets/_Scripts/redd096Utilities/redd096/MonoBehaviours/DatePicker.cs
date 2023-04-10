using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using redd096.Attributes;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace redd096
{
    /// <summary>
    /// Calendar must have a GameObject (our calendar to open and close) with child:
    /// - (facoltative) header, with TMPro month text and buttons for NextMonth and PreviousMonth
    /// - a GridLayoutGroup with 7 columns.
    /// Firsts 7 child of GridLayoutGroup are 7 GameObjects with TMPro in self or child (unity button without button component). These are days of week (M,T,W,T,F,S,S)
    /// Buttons for every day, with TMPro in self or child (unity button). Text to show day of month and button to call OnResult event
    /// - (facoltative) button to come back to CurrentMonth
    /// 
    /// You can use AutoGenerate button to generate DaysOfWeek and every day Button
    /// </summary>
	[AddComponentMenu("redd096/MonoBehaviours/Date Picker")]
    public class DatePicker : MonoBehaviour
    {
        [Header("Calendar to open and close")]
        [SerializeField] GameObject calendar = default;
        [SerializeField] bool closeOnAwake = true;
        [Tooltip("On awake set DateTime.Now as default result, to have result to read without open calendar")][SerializeField] bool setDefaultOnAwake = true;
        [Tooltip("If set default on awake, start from first day selectable")][SerializeField] bool findFirstDaySelectable = true;

        [Header("Close when not selected")]
        [Tooltip("If click outside of calendar or change selection, close it")][SerializeField] bool closeWhenCalendarNotSelected = true;
        [Tooltip("If click calendar gameObject setted above or its childs, keep calendar open?")][SerializeField] bool calendarObjectIsInTheList = true;
        [Tooltip("If click one of these objects or childs of them, keep calendar open")][SerializeField] GameObject[] objectsCanBeSelectedWithoutCloseCalendar = default;

        [Header("Month Header")]
        [SerializeField] TextMeshProUGUI monthText = default;
        [SerializeField] string monthFormat = "MMMM yyyy";

        [Header("Days of the week")]
        [SerializeField] DayOfWeek firstDayOfWeek = DayOfWeek.Monday;
        [SerializeField] GameObject[] daysOfWeek = default;

        [Header("Calendar")]
        [SerializeField] Button[] daysButtons = default;

        [Header("Not Clickable Days")]
        [Tooltip("Disable yesterday and before")][SerializeField] bool disableDaysBeforeThanToday = true;
        [Tooltip("If bool enabled and TimePicker setted, disable also today if there are not selectable hours")][SerializeField] TimePicker timePicker = default;
        [Tooltip("Disable these days of the week")][SerializeField] DayOfWeek[] disabledDaysOfTheWeek = new DayOfWeek[2] { DayOfWeek.Saturday, DayOfWeek.Sunday };

        [Header("Result")]
        [SerializeField] string resultFormat = "dd/MM/yyyy";
        [SerializeField] string result = "19/12/2022";
        [SerializeField] UnityEvent<string> onResult = default;

        [Space(10)]
        [Header("DEBUG - AutoGenerate DaysOfWeek and Calendar")]
        [Tooltip("Parent where spawn")][SerializeField] Transform contentGridLayout = default;
        [Tooltip("Prefab for DaysOfWeek and CalendarButtons")][SerializeField] Button buttonPrefab = default;

        DateTime monthToGenerate = DateTime.Today;
        DateTime selectedDay = DateTime.Today;

        //check where click
        //this is used, cause if click calendar but miss day buttons, the background image isn't a Selectable, so EventSystem set selectedGameObject to null and close calendar (if setted in inspector to close when not selected)
        GameObject lastSelectedGameObject;

        private void Awake()
        {
            //set today as default result
            if (setDefaultOnAwake)
            {
                result = DateTime.Now.ToString(resultFormat);

                //or find first day selectable
                if (findFirstDaySelectable)
                {
                    DateTime day = DateTime.Now;
                    while (true)
                    {
                        if (timePicker) timePicker.SetSelectedDate(day.ToString(resultFormat));     //update time picker to check also if there are hours this day
                        if (IsEnabledThisDay(day.Date))                                             //check is enabled (using variables setted for not clicable days)
                            break;
                        day = day.AddDays(1);                                                       //else check next day
                    }
                    result = day.ToString(resultFormat);
                }
            }

            //set current date (to show from previous selected month - default is DateTime.Now) as start month
            monthToGenerate = DateTime.Parse(result).Date;
            selectedDay = DateTime.Parse(result).Date;          //set also selected day
            GenerateCalendar();

            //close on awake
            if (closeOnAwake)
                CloseCalendar();
        }

        void Update()
        {
            //if calendar is open, check to close it
            if (closeWhenCalendarNotSelected && IsCalendarOpen())
            {
                //check only if changed selected gameObject
                if (EventSystem.current.currentSelectedGameObject == lastSelectedGameObject)
                    return;

                lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;

                //if some object in calendar is selected or cliked, don't close
                if (calendarObjectIsInTheList)
                {
                    if (CheckOneTransformIsSelected(calendar.GetComponentsInChildren<Transform>()) || CheckClickedInsideOneRectTransform(calendar.GetComponentsInChildren<RectTransform>()))
                        return;
                }

                //if some of these objects is selected or cliked, don't close
                if (objectsCanBeSelectedWithoutCloseCalendar != null)
                {
                    foreach (GameObject go in objectsCanBeSelectedWithoutCloseCalendar)
                    {
                        if (CheckOneTransformIsSelected(go.GetComponentsInChildren<Transform>()) || CheckClickedInsideOneRectTransform(go.GetComponentsInChildren<RectTransform>()))
                            return;
                    }
                }

                //else close it
                CloseCalendar();
            }
        }

        #region public API

        public void OpenCalendar()
        {
            //set current date (to show from previous selected month - default is DateTime.Now) as start month
            monthToGenerate = DateTime.Parse(result).Date;
            selectedDay = DateTime.Parse(result).Date;          //set also selected day
            GenerateCalendar();

            //and open calendar
            ActiveCalendar(true);
        }

        public void CurrentMonth()
        {
            //set today as start month
            monthToGenerate = DateTime.Now.Date;
            GenerateCalendar();
        }

        public void NextMonth()
        {
            //next month
            monthToGenerate = monthToGenerate.AddMonths(1);
            GenerateCalendar();
        }

        public void PreviousMonth()
        {
            //previous month
            monthToGenerate = monthToGenerate.AddMonths(-1);
            GenerateCalendar();
        }

        public void CloseCalendar()
        {
            ActiveCalendar(false);
        }

        public bool IsCalendarOpen()
        {
            if (calendar)
                return calendar.activeInHierarchy;

            return false;
        }

        /// <summary>
        /// Used to force result. For example to set a default value before open calendar (to get from a variable instead of set DateTime.Now in awake)
        /// </summary>
        public void SetResult(string result)
        {
            this.result = result;
        }

        public void UpdateResult()
        {
            //update selected day
            selectedDay = DateTime.Parse(result).Date;
            GenerateCalendar();
        }

        public bool IsEnabledThisDay(DateTime day)
        {
            return CanBeEnabledToday(day.Date) && IsDayEnabled(day.Date);
        }

        #endregion

        #region checks to close calendar

        bool CheckOneTransformIsSelected(Transform[] transforms)
        {
            for (int i = 0; i < transforms.Length; i++)
            {
                //if event system has this selected
                if (lastSelectedGameObject == transforms[i].gameObject)
                    return true;
            }
            return false;
        }

        bool CheckClickedInsideOneRectTransform(RectTransform[] rectTransforms)
        {
#if ENABLE_INPUT_SYSTEM
            //only if clicked this frame
            if (Mouse.current.leftButton.wasPressedThisFrame == false)
                return false;

            Camera cam = Camera.main;
            Vector3 mousePosition = Mouse.current.position.ReadValue();
#else
            //only if clicked this frame
            if (Input.GetMouseButtonDown(0) == false)
                return false;

            Camera cam = Camera.main;
            Vector3 mousePosition = Input.mousePosition;
#endif

            for (int i = 0; i < rectTransforms.Length; i++)
            {
                //if mouse position is inside rect transform
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransforms[i], mousePosition, cam))
                    return true;
            }
            return false;
        }

        #endregion

        #region generate calendar

        void ActiveCalendar(bool active)
        {
            if (calendar)
                calendar.SetActive(active);
        }

        void GenerateCalendar()
        {
            //set month text
            SetHeader();

            //show name of days of week
            UpdateDaysOfWeek();

            //create calendar from Monday to Sunday * number of rows
            UpdateDaysButtons();
        }

        void SetHeader()
        {
            string s = monthToGenerate.ToString(monthFormat);

            //set month text
            if (monthText)
                monthText.text = s[0].ToString().ToUpper() + s.Substring(1);
        }

        void UpdateDaysOfWeek()
        {
            if (daysOfWeek == null)
                return;

            //start from monday (or other day setted in inspector)
            DateTime firstDayOfCalendar = GetFirstDayOfCalendar();

            //update every gameObject with day of week name
            for (int i = 0; i < daysOfWeek.Length; i++)
            {
                DateTime dt = firstDayOfCalendar.AddDays(i);

                //only first char of day of week
                daysOfWeek[i].GetComponentInChildren<TextMeshProUGUI>().text = dt.ToString("dddd").Substring(0, 1).ToUpper();

                //set order in grid
                daysOfWeek[i].transform.SetSiblingIndex(i);
            }
        }

        void UpdateDaysButtons()
        {
            if (daysButtons == null)
                return;

            //start from monday (or other day setted in inspector)
            DateTime firstDayOfCalendar = GetFirstDayOfCalendar();

            //create calendar from Monday to Sunday * number of rows
            for (int i = 0; i < daysButtons.Length; i++)
            {
                DateTime dt = firstDayOfCalendar.AddDays(i);

                //set text and add event on click
                Button b = daysButtons[i];
                b.GetComponentInChildren<TextMeshProUGUI>().text = dt.Day.ToString();
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(() => OnClick(dt));

                //set order in grid
                b.transform.SetSiblingIndex(daysOfWeek != null ? daysOfWeek.Length + i : i);

                //check if button is enabled
                b.interactable = CanBeEnabledToday(dt) && IsDayEnabled(dt);

                //select current day
                if (dt.Date == selectedDay.Date)
                    b.Select();
            }
        }

        DateTime GetFirstDayOfCalendar()
        {
            //move to first day of the month
            DateTime dt = new DateTime(monthToGenerate.Year, monthToGenerate.Month, 1);

            //if 1st day isn't the one setted in inspector, use previous month
            for (int i = 0; i < 7; i++)
            {
                dt = dt.AddDays(-1);
                if (dt.DayOfWeek == firstDayOfWeek)
                    break;
            }

            return dt;
        }
        #endregion

        #region checks to disable buttons

        bool CanBeEnabledToday(DateTime dt)
        {
            //if time picker is null, don't check hours
            bool thereAreHoursToday = timePicker == null || timePicker.ThereAreHoursThisDate(dt);

            //if bool is disabled
            //or check if DateTime is greater or equal than today (DateTime.Now without check the hours) AND if there is a time picker, be sure it isn't after last possible hour
            return disableDaysBeforeThanToday == false || (dt.Date >= DateTime.Now.Date && thereAreHoursToday);
        }

        bool IsDayEnabled(DateTime dt)
        {
            //check every disabled day
            foreach (DayOfWeek dayOfWeek in disabledDaysOfTheWeek)
            {
                //if DateTime is one of these, then is disabled
                if (dt.DayOfWeek == dayOfWeek)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        void OnClick(DateTime dt)
        {
            //on button click, call event
            result = dt.ToString(resultFormat);
            onResult?.Invoke(result);
        }

        #region editor only

        /// <summary>
        /// editor only
        /// </summary>
        [Button]
        public void AutoGenerateCalendar()
        {
            //remove old buttons
            for (int i = contentGridLayout.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                    Destroy(contentGridLayout.GetChild(i).gameObject);
                else
                    DestroyImmediate(contentGridLayout.GetChild(0).gameObject);
            }

            //generate days of week
            daysOfWeek = new GameObject[7];
            for (int i = 0; i < daysOfWeek.Length; i++)
            {
#if UNITY_EDITOR
                daysOfWeek[i] = (UnityEditor.PrefabUtility.InstantiatePrefab(buttonPrefab, contentGridLayout) as Button).gameObject;
#else
                daysOfWeek[i] = Instantiate(buttonPrefab, contentGridLayout).gameObject;
#endif

                //remove button component
                if (Application.isPlaying)
                    Destroy(daysOfWeek[i].GetComponent<Button>());
                else
                    DestroyImmediate(daysOfWeek[i].GetComponent<Button>());
            }

            //create buttons for calendar from Monday to Sunday * number of rows (6 to be sure to show 31 days also when first day of the month is last day of week)
            daysButtons = new Button[7 * 6];
            for (int i = 0; i < daysButtons.Length; i++)
            {
#if UNITY_EDITOR
                daysButtons[i] = UnityEditor.PrefabUtility.InstantiatePrefab(buttonPrefab, contentGridLayout) as Button;
#else
                daysButtons[i] = Instantiate(buttonPrefab, contentGridLayout);
#endif
            }

            //set today as start month and update buttons texts
            monthToGenerate = DateTime.Now.Date;
            GenerateCalendar();
        }

        #endregion
    }
}