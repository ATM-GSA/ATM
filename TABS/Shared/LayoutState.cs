using System;
using System.Collections.Generic;
using System.Linq;
using TABS.Data;

namespace TABS.Shared
{
    public class LayoutState
    {
        public event Action OnChange;
        public List<Application> FavApps { get; set; } = new List<Application>();

        public int RegistrationCount { get; set; }

        public bool collapsed { get; set; }

        public string UserName { get; set; }

        public void SetUserName(string newUserName)
        {
            UserName = newUserName;
            NotifyStateChanged();
        }

        public void OnFavAppsChange(List<Application> currentFavApps)
        {
            if (currentFavApps.Except(FavApps).ToList().Any() || FavApps.Except(currentFavApps).ToList().Any() || (currentFavApps.Count() != FavApps.Count()))
            {
                FavApps = currentFavApps;
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Decreases RegistrationCount. Called by AdminTable.
        /// </summary>
        public void DecreaseRegistrationCount()
        {
            RegistrationCount = RegistrationCount - 1;
            NotifyStateChanged();
        }

        public void ToggleCollapse()
        {
            collapsed = !collapsed;
            NotifyStateChanged();
        }

        /// <summary>
        /// Adds/Removes apps from the favourited apps in the sidebar. Called by AppDetails.
        /// </summary>
        /// <param name="app"></param>
        public void OnAppFav(Application app)
        {
            Application toFav = FavApps.Find(a => a.ShortID == app.ShortID);
            if (toFav != null)
            {
                FavApps.Remove(toFav);
            }
            else
            {
                FavApps.Add(app);
            }
            NotifyStateChanged();
        }

        /// <summary>
        /// Renames a favourited app in the sidebar. Called by AppSettings.
        /// </summary>
        /// <param name="app"></param>
        public void OnAppRename(Application app)
        {
            Application toRename = FavApps.Find(a => a.ShortID == app.ShortID);
            if (toRename != null)
            {
                toRename.Identification.Name = app.Identification.Name;
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Removes a favourited app from the sidebar. Called by AppSettings.
        /// </summary>
        /// <param name="app"></param>
        public void OnAppDelete(Application app)
        {
            Application toDelete = FavApps.Find(a => a.ShortID == app.ShortID);
            if (toDelete != null)
            {
                FavApps.Remove(toDelete);
                NotifyStateChanged();
            }
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
