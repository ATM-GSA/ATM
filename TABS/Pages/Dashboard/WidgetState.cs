using System;
using System.Collections.Generic;
using System.Linq;

namespace TABS.Pages.Dashboard
{
    public class WidgetState
    {
        public event Action OnChange;
        public bool editMode = false;
        public List<Widget> activeWidgets = new();
        public List<Widget> hiddenWidgets = new();
        public List<Widget> widgetsToAdd = new();

        /// <summary>
        /// Used to increment the order of selected widget, and decrement the order of the next widget.
        /// </summary>
        /// <param name="name". The name of the widget to increment. </param>
        /// <returns> void </returns>
        public void IncrementOrder(string name)
        {
            int curWidgetIndex = activeWidgets.FindIndex(x => x.widgetName == name);
            int curWidgetOrder = activeWidgets.Find(x => x.widgetName == name).order;
            if (curWidgetIndex != activeWidgets.Count - 1)
            {
                activeWidgets[curWidgetIndex].order = curWidgetOrder + 1;
                activeWidgets[curWidgetIndex + 1].order = curWidgetOrder;
                activeWidgets = activeWidgets.OrderBy(o => o.order).ToList();
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Used to decrement the order of selected widget, and increment the order of the previous widget.
        /// </summary>
        /// <param name="name". The name of the widget to decrement. </param>
        /// <returns> void </returns>
        public void DecrementOrder(string name)
        {
            int curWidgetIndex = activeWidgets.FindIndex(x => x.widgetName == name);
            int curWidgetOrder = activeWidgets.Find(x => x.widgetName == name).order;
            if (curWidgetIndex != 0)
            {
                activeWidgets[curWidgetIndex].order = curWidgetOrder - 1;
                activeWidgets[curWidgetIndex - 1].order = curWidgetOrder;
                activeWidgets = activeWidgets.OrderBy(o => o.order).ToList();
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Moves the widget row from the Active widgets table to the hidden widgets table
        /// </summary>
        /// <param name="name". The name of the widget to move. </param>
        /// <returns> void </returns>
        public void Hide(string name)
        {
            Widget widget = activeWidgets.Find(x => x.widgetName == name);
            if (activeWidgets.Where(w => w.order > widget.order) != null)
            {
                // decrement the order by 1 of all the widgets that come after the widget to hide
                foreach (var activeWidgetEntry in activeWidgets.Where(w => w.order > widget.order))
                {
                    activeWidgetEntry.order = activeWidgetEntry.order - 1;
                }
            }
            // hidden widgets have their order set to 0
            widget.order = 0;
            widget.fullWidth = false;
            hiddenWidgets.Insert(hiddenWidgets.Count, widget);
            hiddenWidgets = hiddenWidgets.OrderBy(o => o.widgetName).ToList();
            activeWidgets = activeWidgets.Where(x => x.widgetName != name).ToList();
            NotifyStateChanged();
        }

        public int GetOrderOfWidget(string name)
        {
            Widget widget = activeWidgets.Find(x => x.widgetName == name);
            if (widget == null) return 0;
            return widget.order;
        }

        public bool GetWidthOfActiveWidget(string name)
        {
            Widget widget = activeWidgets.Find(x => x.widgetName == name);
            if (widget == null) return false;
            return widget.fullWidth;
        }

        public void SetWidthOfActiveWidget(string name, bool fullWidth)
        {
            Widget widget = activeWidgets.Find(x => x.widgetName == name);
            widget.fullWidth = fullWidth;
        }

        public void SetWidthOfHiddenWidget(string name, bool fullWidth)
        {
            Widget widget = hiddenWidgets.Find(x => x.widgetName == name);
            widget.fullWidth = fullWidth;
        }

        public bool GetWidthOfHiddenWidget(string name)
        {
            Widget widget = hiddenWidgets.Find(x => x.widgetName == name);
            return widget.fullWidth;
        }

        public void SwitchToEditMode()
        {
            editMode = true;
            NotifyStateChanged();
        }

        public void SwitchToViewMode()
        {
            editMode = false;
            NotifyStateChanged();
        }

        public void SelectWidgetToAdd(string widgetName)
        {
            Widget widget = hiddenWidgets.Find(x => x.widgetName == widgetName);
            widgetsToAdd.Add(widget);
            NotifyStateChanged();

        }

        public void UnselectWidgetToAdd(string widgetName)
        {
            Widget widget = hiddenWidgets.Find(x => x.widgetName == widgetName);
            widgetsToAdd.Remove(widget);
            NotifyStateChanged();
        }

        ///// <summary>
        ///// Moves the widget row from the hidden widgets table to the active widgets table
        ///// </summary>
        ///// <param name="name". The name of the widget to move. </param>
        ///// <returns> void </returns>
        //private void activateWidget(string name)
        //{
        //    Widget widget = hiddenWidgets.Find(x => x.widgetName == name);
        //    // place the widget at the end of the active widgets table
        //    widget.order = activeWidgets.Count + 1;
        //    activeWidgets.Add(widget);
        //}

        public void AddWidgetsToDashboard()
        {
            foreach (Widget widget in widgetsToAdd)
            {
                widget.order = activeWidgets.Count + 1;
                activeWidgets.Add(widget);
            }
            activeWidgets.OrderBy(o => o.order).ToList();
            hiddenWidgets = hiddenWidgets.Except(widgetsToAdd).ToList();
            widgetsToAdd.Clear();
            NotifyStateChanged();
        }

        public void ClearWidgetsToAdd()
        {
            widgetsToAdd.Clear();
            NotifyStateChanged();
        }

        /// <summary>
        /// A publically accessible method that invokes the OnChange event
        /// </summary>
        public void RefreshWidgets()
        {
            NotifyStateChanged();
        }

        /// <summary>
        /// Invokes the OnChange event
        /// </summary>
        private void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }
    }
}
