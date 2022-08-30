using System.Threading.Tasks;

namespace TABS.Shared.ModuleTables
{
    interface IModuleTable
    {
        public Task<bool> saveChanges();
        public bool hasErrors();

        public void reset();
    }


}
