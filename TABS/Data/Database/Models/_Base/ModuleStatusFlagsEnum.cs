
namespace TABS.Data
{
    // Values need to be powers of 2
    // If all flags are 0, then the module is up to date
    public enum ModuleStatusFlagEnum
    {
        NeedsAttention = 1,
        //SecurityUpdateRequired = 2,
        //InvalidValues = 4
    }
}
