using System.Collections.Generic;
using System.Threading.Tasks;

namespace TABS.Data
{
    public interface IUserService
    {
        Task<bool> ActivateUser(User user, User _admin);
        Task<bool> ApproveUser(User user, User _admin);
        Task<bool> DeactivateUser(User user, User _admin);
        Task<bool> DeleteUser(User user, User _admin);
        Task<List<User>> GetApprovedUsers();
        Task<List<User>> GetDeactivatedUsers();
        Task<List<User>> GetNeedsApproval();
        Task<User> GetUserByAdID(string AdID);
        Task<User> GetUserByUserID(int UserID);
        Task<List<User>> SearchUsersByName(string Name);
    }
}