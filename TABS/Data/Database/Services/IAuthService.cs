using System.Threading.Tasks;

namespace TABS.Data
{
    public interface IAuthService
    {
        Task<string> GetUserSID();
        Task<string> GetUsersPrimaryName();
    }
}