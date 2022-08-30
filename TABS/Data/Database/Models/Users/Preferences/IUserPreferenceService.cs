using System.Threading.Tasks;
using TABS.Data;

namespace TABS
{
    public interface IUserPreferenceService
    {
        string AdID { get; set; }

        Preferences GetPreferences();
        void Initialize(string newADId);
        Task<bool> SaveUserPreferences();
        void SetPreferences(Preferences preferences);
    }
}