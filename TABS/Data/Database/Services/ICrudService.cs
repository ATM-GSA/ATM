using System.Collections.Generic;
using System.Threading.Tasks;

namespace TABS.Data
{
    public interface ICrudService
    {
        Task<bool> DeleteAsync<T>(T row);
        Task<List<T>> GetDataAsync<T>() where T : class;
        Task<List<T>> GetMultipleRecordsAsync<T>(ICollection<int> ids) where T : class;
        Task<T> GetRecordAsync<T>(int id) where T : class;
        Task<T> GetRecordAsync<T>(params int[] ids) where T : class;
        Task<bool> InsertAsync<T>(T row);
        Task<bool> UpdateAsync<T>(T row);
    }
}