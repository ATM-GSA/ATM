using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using shortid;
using shortid.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TABS.Data
{
    public static class DBInitializer
    {

        private static readonly Random rng = new(); // Used to make ids

        /// <summary>
        /// Intiailize the databse using the given context.
        /// </summary>
        /// <param name="context">DB context to the database that needs to be initialized</param>
        /// <param name="deleteExisting">Delete existing data in the database</param>
        /// <param name="checkSeeded">Check if database is already seeded before initializing</param>
        public static void Initialize(TABSDBContext context, bool deleteExisting, bool checkSeeded)
        {
            context.Database.EnsureCreated();

            // migrate db
            context.Database.Migrate();
            context.SaveChanges();

            if (deleteExisting)
            {
                context.ApplicationSubscriptions.RemoveRange(context.ApplicationSubscriptions);
                // Delete all data from all tables
                context.Dependencies.RemoveRange(context.Dependencies);
                context.Architectures.RemoveRange(context.Architectures);

                context.Servers.RemoveRange(context.Servers);
                // context.ServerEnvironments.RemoveRange(context.ServerEnvironments);

                context.Databases.RemoveRange(context.Databases);
                //context.DatabaseEnvironments.RemoveRange(context.DatabaseEnvironments);

                context.Reports.RemoveRange(context.Reports);
                context.BASMOnboardings.RemoveRange(context.BASMOnboardings);

                context.FortifyScans.RemoveRange(context.FortifyScans);
                context.Securities.RemoveRange(context.Securities);

                context.Contacts.RemoveRange(context.Contacts);
                context.Applications.RemoveRange(context.Applications);

                /*context.Users.RemoveRange(context.Users);
                context.Roles.RemoveRange(context.Roles);*/

                // reseed db so PK values stay the same each time
                context.Database.ExecuteSqlRaw("DBCC CHECKIDENT('Applications', RESEED, 0)");

                context.SaveChanges();
            }

            // Look for any exisitng data.
            if (checkSeeded && context.Applications.Any())
            {
                return;   // DB has been seeded
            }

            IntializeApplicationData(context);
        }

        private static void IntializeApplicationData(TABSDBContext context)
        {
            List<User> user = context.Set<User>().ToList();
            if (user.Count == 0)
            {
                throw new Exception("Cannot initialize data if there are users in the db");
            }

            #region Create Application

            Console.WriteLine("Adding apps...");

            // Create applications
            var apps = new List<Application>();
            for (int i = 0; i < 200; i++)
            {
                ApplicationProperties ApplicationProperties = new()
                {
                    availableModules = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                    featuredModules = new List<int> { rng.Next(1, 8) }
                };

                Application app = new()
                {
                    Properties = JsonConvert.SerializeObject(ApplicationProperties),
                    // use one hard-coded short-id for testing
                    ShortID = i == 0 ? "abcdefghijkl" : ShortId.Generate(
                                new GenerationOptions
                                {
                                    UseNumbers = true,
                                    UseSpecialCharacters = false,
                                    Length = 12
                                }),
                    IsArchived = false,
                    IsComplete = true,
                    CreateByUserID = user[0].UserID
                };

                // We need to instantiate sub-modules here since it's a one-to-one relationship
                // I.e. each app will get exactly one sub-module

                // Create Identification
                ApplicationIdentification identification = new()
                {
                    Application = app,
                    APMID = i,
                    Name = string.Format("Project - {0}", i),
                    Title = string.Format("BeSD Project 202{0}", i),
                    Status = "Active",
                    Visibility = "Internal",
                    SWIM = false,
                    CMPPortfolio = i % 2 == 0,
                    WebURL = string.Format("https://ec.gc.ca/project-{0}", i),
                    ClientBranch = "RAD"
                };
                app.Identification = identification;

                // Create architecture
                Architecture architecture = new()
                {
                    Application = app,
                    AppPlatform = i == 1 ? "Browsér" : "Browser", // use diacritic for testing
                    SMARTFramework = "SMARTUI",
                    SMARTUpgradePlanning = "underway",
                    XmlBlob = false,
                    CDTSVersion = "1.5.3",
                    NETVersion = "5.0.3",
                    SEASVersion = "11.0.1"
                };
                app.Architecture = architecture;

                // Create security
                Security security = new()
                {
                    Application = app,
                    SQLSSLEncryption = true,
                    AttachmentDropZone = false,
                    AuditLogs = true,
                    WebconfigEncrpted = true,
                    WAFTest = DateTime.Now,
                    WAFStatus = DateTime.Now,
                    Exempt = false,
                    ExemptionReasoning = "",
                    SecurityLevel = "Protected A",
                    SatoExpiry = DateTime.Now,
                    SAD = false,
                    ICAP = false,
                    HSTS = true,
                    StaticRemediationCompleted = true,
                    DynamicRemediationCompleted = true
                };
                var fortifyScans = new List<FortifyScan>();
                // Create fortify scans
                for (int j = 0; j < 4; j++)
                {
                    FortifyScan.ScanTypeLookUp scanType = j % 2 == 0 ? FortifyScan.ScanTypeLookUp.StaticScan : FortifyScan.ScanTypeLookUp.DynamicScan;
                    fortifyScans.Add(new FortifyScan
                    {
                        Security = security,
                        Name = string.Format("Project-{0} : Fortify Scan {1}", i, j),
                        ScanDate = DateTime.Now,
                        Notes = string.Format("Fortify scan for project-{0} (type: {1})", i, scanType.ToString()),
                        ReportLink = string.Format("https://ec.gc.ca/project-{0}/fortify/report-{1}-{2}.pdf", i, j, scanType.ToString()),
                        ScanType = scanType
                    });
                }
                security.FortifyScans = fortifyScans;
                app.Security = security;

                // Create BASMOnboarding
                BASMOnboarding onboarding = new()
                {
                    Application = app,
                    AssystSetup = true,
                    IntroEmail = false,
                    PowerUserGuide = true,
                    BASMGuide = false,
                    DemoToBASM = true,
                    ClientIntro = false,
                    AppMonitoring = false
                };
                app.BASMOnboarding = onboarding;

                // Create reports
                Report report = new()
                {
                    Application = app,
                    Performance = string.Format("https://ec.gc.ca/reports/project-{0}/reports/performance/report.pdf", i),
                    Accessibility = string.Format("https://ec.gc.ca/reports/project-{0}/reports/accessibility/report.pdf", i)
                };
                app.Report = report;

                // Create contacts (only if there are any users in the test DB)
                Contact contact = new();
                app.Contact = contact;

                // Create servers
                ServerEnvironment serverEnvironment = new()
                {
                    Application = app
                };
                foreach (Server.ServerType t in Enum.GetValues(typeof(Server.ServerType)))
                {
                    serverEnvironment.Servers.Add(new Server
                    {
                        ServerEnvironment = serverEnvironment,
                        Name = string.Format("Project-{0}-{1}-Server", i, t.ToString()),
                        URL = string.Format("https://ec.gc.ca/project-{0}/server-{1}", i, t.ToString()),
                        Type = t,
                        Version = string.Format("1.0.{0}", i)
                    });
                }
                app.ServerEnvironment = serverEnvironment;

                // Create databases
                DatabaseEnvironment databaseEnvironment = new()
                {
                    Application = app
                };
                foreach (Database.DatabaseType t in Enum.GetValues(typeof(Database.DatabaseType)))
                {
                    databaseEnvironment.Databases.Add(new Database
                    {
                        DatabaseEnvironment = databaseEnvironment,
                        Name = string.Format("Project-{0}-{1}-Database", i, t.ToString()),
                        URL = string.Format("https://ec.gc.ca/project-{0}/database-{1}", i, t.ToString()),
                        Type = t,
                        Version = string.Format("3.0.{0}", i),
                        Platform = "MicrosoftSQL"
                    });
                }
                app.DatabaseEnvironment = databaseEnvironment;

                apps.Add(app);
            }
            context.Applications.AddRange(apps);
            context.SaveChanges();

            Console.WriteLine($"Added {apps.Count} apps to the database.");
            #endregion

            #region Create Dependencies

            /*
            // Create dependencies (hard-coded): 
            // app[0] -> app[1]
            // app[1] -> app[4]
            // app[2] -> app[4]
            // app[3] -> app[2]
            Dependency dependency1 = new() { Dependee = apps[0], Dependent = apps[1] };
            apps[0].Dependents.Add(dependency1);
            apps[1].Dependees.Add(dependency1);

            Dependency dependency2 = new() { Dependee = apps[1], Dependent = apps[4] };
            apps[1].Dependents.Add(dependency2);
            apps[4].Dependees.Add(dependency2);

            Dependency dependency3 = new() { Dependee = apps[2], Dependent = apps[4] };
            apps[2].Dependents.Add(dependency3);
            apps[4].Dependees.Add(dependency3);

            Dependency dependency4 = new() { Dependee = apps[3], Dependent = apps[2] };
            apps[3].Dependents.Add(dependency4);
            apps[2].Dependees.Add(dependency4);
            context.Dependencies.AddRange(dependency1, dependency2, dependency3, dependency4);
            context.SaveChanges();
            */

            #endregion
        }
    }
}