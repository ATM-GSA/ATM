using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace TABS.Data
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [StringLength(200)]
        public string AdID { get; set; }

        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(200)]
        public string PrimaryName { get; set; }

        [StringLength(200)]
        public string Email { get; set; }

        public int ITLevel { get; set; }

        public bool Approved { get; set; }

        public bool IsDeactivated { get; set; }

        public DateTime RegistrationDate { get; set; }

        public Role Role { get; set; } // Inverse navigation property

        [Required]
        public string Preferences { get; set; }

        [Required]
        public string Notifications { get; set; }

        /// <summary>
        /// Deserialize the user's preferences string into a Preferences object
        /// </summary>
        /// <returns></returns>
        public Preferences GetPreferences()
        {
            return JsonConvert.DeserializeObject<Preferences>(Preferences);
        }

        /// <summary>
        /// Deserialize the user's notifications string into a NotificationsQueue object
        /// </summary>
        /// <returns></returns>
        public NotificationsQueue GetNotifications()
        {
            return JsonConvert.DeserializeObject<NotificationsQueue>(Notifications, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto // need this to serialize inheritance
            });
        }

        /// <summary>
        /// Returns this user as a ContactInfo object
        /// </summary>
        /// <returns>ContactInfo object encapsulating this user</returns>
        public ContactInfo GetContactInfo()
        {
            return new ContactInfo()
            {
                userID = UserID,
                userAdId = AdID,
                name = Name,
                email = Email,
                itLevel = ITLevel
            };
        }
    }
}