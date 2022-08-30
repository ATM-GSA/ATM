using Eccc.Sali;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TABS.Audit;

namespace TABS.Data
{
    public class CrudService : ICrudService
    {
        private readonly AuditLogService _auditLogService;

        public CrudService(AuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        #region Get List of records
        /// <summary>
        /// Retrieve all records of the provided entity
        /// </summary>
        /// <typeparam name="T">Desired entity to retrieve</typeparam>
        /// <returns>List of specified entity records</returns>
        public async Task<List<T>> GetDataAsync<T>() where T : class
        {
            using var dbContext = DBContextFactory.CreateInstance();
            return await dbContext.Set<T>().ToListAsync();
        }
        #endregion

        #region Get specific record
        /// <summary>
        /// Retrieve a specific entity record
        /// </summary>
        /// <typeparam name="T">Desired entity to retrieve</typeparam>
        /// <param name="id">Id of desired record</param>
        /// <returns>Desired entity object</returns>
        public async Task<T> GetRecordAsync<T>(int id) where T : class
        {
            using var dbContext = DBContextFactory.CreateInstance();
            return await dbContext.FindAsync<T>(id);
        }
        #endregion

        #region Get specific record with composite key
        /// <summary>
        /// Retrieve a specific entity record with a composite key
        /// </summary>
        /// <typeparam name="T">Desired entity to retrieve</typeparam>
        /// <param name="id">Id of desired record</param>
        /// <returns>Desired entity object</returns>
        public async Task<T> GetRecordAsync<T>(params int[] ids) where T : class
        {
            using var dbContext = DBContextFactory.CreateInstance();
            return await dbContext.FindAsync<T>(Array.ConvertAll(ids, id => (object)id));
        }
        #endregion

        #region Get list of multiple records
        /// <summary>
        /// Retrieve all records with the given list of ids.
        /// Only found ids will be returned in the list
        /// </summary>
        /// <typeparam name="T">Desired entity to retrieve</typeparam>
        /// <param name="id">Id of desired record</param>
        /// <returns>List of specified entity records</returns>
        public async Task<List<T>> GetMultipleRecordsAsync<T>(ICollection<int> ids) where T : class
        {
            List<T> records = new();

            foreach (int id in ids)
            {
                T record = await GetRecordAsync<T>(id);
                if (record != null)
                {
                    records.Add(record);
                }
            }

            return records;
        }
        #endregion

        #region Insert record
        /// <summary>
        /// Insert new row for supplied entity
        /// </summary>
        /// <typeparam name="T">Desired entity to insert</typeparam>
        /// <param name="row">Object of entity to be inserted</param>
        /// <returns>bool</returns>
        public async Task<bool> InsertAsync<T>(T row)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            await dbContext.AddAsync(row);
            await dbContext.SaveChangesAsync();
            await (_auditLogService?.LogAsync(row, AuditLog.ActionCategory.DataWriteCreate, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);
            return true;
        }
        #endregion

        #region Update record
        /// <summary>
        /// Update row for supplied entity
        /// </summary>
        /// <typeparam name="T">Desired entity to updated</typeparam>
        /// <param name="row">Object of entity to be updated</param>
        /// <returns>bool</returns>
        public async Task<bool> UpdateAsync<T>(T row)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            dbContext.Update(row);
            await dbContext.SaveChangesAsync();
            await (_auditLogService?.LogAsync(row, AuditLog.ActionCategory.DataWriteUpdate, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);
            return true;
        }
        #endregion

        #region Delete record
        /// <summary>
        /// Delete row for supplied entity
        /// </summary>
        /// <typeparam name="T">Desired entity to deleted</typeparam>
        /// <param name="row">Object of entity to be deleted</param>
        /// <returns>bool</returns>
        public async Task<bool> DeleteAsync<T>(T row)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            dbContext.Remove(row);
            await dbContext.SaveChangesAsync();
            await (_auditLogService?.LogAsync(row, AuditLog.ActionCategory.DataDelete, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);
            return true;
        }
        #endregion
    }
}