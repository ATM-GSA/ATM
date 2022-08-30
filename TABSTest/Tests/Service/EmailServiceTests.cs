using TABS.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class EmailServiceTests
    {
        static EmailService _emailService;

        [ClassInitialize]
        public static void ClassInitialize(TestContext _context)
        {
            _emailService = new EmailService();
        }

        [TestMethod]
        public async Task Send_Approval_Email_Test()
        {
            await _emailService.SendTemplatedEmail("", "Approval", new { Name = "Sharven", Role = 1, Env = "" });
        }

        [TestMethod]
        public async Task Send_Denied_Email_Test()
        {
            await _emailService.SendTemplatedEmail("", "Denied", new { Name = "Sharven" });
        }

        [TestMethod]
        public async Task Send_Announcement_Email_Test()
        {
            await _emailService.SendTemplatedEmail("", "Announcement", new { Name = "Sharven", SenderName = "Matthew", Title = "Hello", Message = "Update the security modules please. Thanks.", RecipientCount = 42, Env = "" });
        }

        [TestMethod]
        public async Task Send_Alert_Email_Test()
        {
            await _emailService.SendTemplatedEmail("", "Alert",
                new
                {
                    Name = "Sharven",
                    UpdateDate = "March 15, 2022",
                    Reminders = new Dictionary<string, List<object>> {
                        { 
                            "en-US", 
                            new List<object> {
                                new { Link = "", AppName = "", Module = "Security" },
                                new { Link = "", AppName = "", Module = "Application Identification" },
                                new { Link = "", AppName = "", Module = "Database Environment" },
                            }
                        },
                        {
                            "fr-CA",
                            new List<object> {
                                new { Link = "", AppName = "", Module = "[fr] Security" },
                                new { Link = "", AppName = "", Module = "[fr] Application Identification" },
                                new { Link = "", AppName = "", Module = "[fr] Database Environment" },
                            }
                        }
                    },
                    Env = ""
                });
        }

        [TestMethod]
        public async Task Send_Digest_Email_Daily()
        {
            await _emailService.SendTemplatedEmail("", "DigestDaily", new
            {
                Name = "Sharven",
                StartDate = "March 100, 2022",
                Reminders = new Dictionary<string, List<object>> 
                {
                    {
                        "en-US",
                        new List<object> {
                            new { Link = "https://www.google.ca", AppName = "", Module = "Security", UpdateDate = "March 14, 2022" },
                            new { Link = "https://www.google.ca", AppName = "", Module = "Architecture", UpdateDate = "March 14, 2022" },
                            new { Link = "https://www.google.ca", AppName = "", Module = "Application Identification", UpdateDate = "March 11, 2022" }
                        }
                    },
                    {
                        "fr-CA",
                        new List<object> {
                            new { Link = "https://www.google.ca", AppName = "", Module = "[fr] Security", UpdateDate = "March 14, 2022" },
                            new { Link = "https://www.google.ca", AppName = "", Module = "[fr] Architecture", UpdateDate = "March 14, 2022" },
                            new { Link = "https://www.google.ca", AppName = "", Module = "[fr] Application Identification", UpdateDate = "March 11, 2022" }
                        }
                    }
                },
                Updates = new Dictionary<string, List<object>>
                {
                    {
                        "en-US",
                        new List<object> {
                            new { Link = "https://www.google.ca", AppName = "", Module = "Security", NumUpdates = 5 },
                            new { Link = "https://www.google.ca", AppName = "", Module = "BASM Onboarding", NumUpdates = 1 },
                            new { Link = "https://www.google.ca", AppName = "", Module = "Application Identification", NumUpdates = 1 }
                        }
                    },
                    {
                        "fr-CA",
                        new List<object> {
                            new { Link = "https://www.google.ca", AppName = "", Module = "[fr] Security", NumUpdates = 5 },
                            new { Link = "https://www.google.ca", AppName = "", Module = "[fr] BASM Onboarding", NumUpdates = 1 },
                            new { Link = "https://www.google.ca", AppName = "", Module = "[fr] Application Identification", NumUpdates = 1 }
                        }
                    }
                },
                Env = ""
            });
        }

        [TestMethod]
        public async Task Send_Digest_Email_Weekly()
        {
            await _emailService.SendTemplatedEmail("", "DigestWeekly", new
            {
                Name = "Sharven",
                StartDate = "March 100",
                EndDate = "February 30, 3000",
                Reminders = new Dictionary<string, List<object>>
                {
                    {
                        "en-US",
                        new List<object> {
                            new { Link = "https://www.google.ca", AppName = "", Module = "Security", UpdateDate = "March 14, 2022" },
                            new { Link = "https://www.google.ca", AppName = "", Module = "Architecture", UpdateDate = "March 14, 2022" },
                            new { Link = "https://www.google.ca", AppName = "", Module = "Application Identification", UpdateDate = "March 11, 2022" }
                        }
                    },
                    {
                        "fr-CA",
                        new List<object> {
                            new { Link = "https://www.google.ca", AppName = "", Module = "[fr] Security", UpdateDate = "March 14, 2022" },
                            new { Link = "https://www.google.ca", AppName = "", Module = "[fr] Architecture", UpdateDate = "March 14, 2022" },
                            new { Link = "https://www.google.ca", AppName = "", Module = "[fr] Application Identification", UpdateDate = "March 11, 2022" }
                        }
                    }
                },
                Updates = new Dictionary<string, List<object>>
                {
                    {
                        "en-US",
                        new List<object> {
                            new { Link = "https://www.google.ca", AppName = "", Module = "Security", NumUpdates = 5 },
                            new { Link = "https://www.google.ca", AppName = "", Module = "BASM Onboarding", NumUpdates = 1 },
                            new { Link = "https://www.google.ca", AppName = "", Module = "Application Identification", NumUpdates = 1 }
                        }
                    },
                    {
                        "fr-CA",
                        new List<object> {
                            new { Link = "https://www.google.ca", AppName = "", Module = "[fr] Security", NumUpdates = 5 },
                            new { Link = "https://www.google.ca", AppName = "", Module = "[fr] BASM Onboarding", NumUpdates = 1 },
                            new { Link = "https://www.google.ca", AppName = "", Module = "[fr] Application Identification", NumUpdates = 1 }
                        }
                    }
                },
                Env = ""
            });
        }
    }
}
