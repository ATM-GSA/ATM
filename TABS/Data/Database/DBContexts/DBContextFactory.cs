using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace TABS.Data
{
    public class DBContextFactory
    {
        private static DbContextOptions dbContextOptions = null;

        /// <summary>
        /// Set the connection string for the DB Context Factory. 
        /// Created contexts will use this connection string. 
        /// </summary>
        /// <param name="connectionStringID">The connexting string ID from app settings</param>
        public static void SetConnectionString(string connectionStringID)
        {
            // Load connection string
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

            // Create the db context options
            dbContextOptions = new DbContextOptionsBuilder<TABSDBContext>()
                .UseSqlServer(configuration.GetConnectionString(connectionStringID))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options;
        }

        /// <summary>
        /// Create a new instance of a DB context.
        /// Returns null if no connection string has been set.
        /// </summary>
        /// <returns>TABSDBContext</returns>
        public static TABSDBContext CreateInstance()
        {
            if (dbContextOptions == null)
            {
                return null;
            }

            return new TABSDBContext(dbContextOptions);
        }
    }
}
