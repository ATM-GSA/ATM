using shortid;
using shortid.Configuration;
using System;

namespace TABS.Data
{
    public abstract class Notification: IEquatable<Notification>
    {
        public string id { get; set; } = ShortId.Generate( // Automatically assign an id
            new GenerationOptions
            {
                UseNumbers = true,
                UseSpecialCharacters = false,
                Length = 8
            }
        );

        public DateTime timestamp { get; set; }

        public int type { get; set; } // refer to NotificationTypeEnum

        public bool unread { get; set; } = true;

        public bool displayed { get; set; } = false;

        public bool Equals(Notification other)
        {
            if (other is null)
                return false;

            return this.id == other.id && this.timestamp == other.timestamp && this.type == other.type && this.unread == other.unread && this.displayed == other.displayed;
        }

        public override bool Equals(object obj) => Equals(obj as Notification);
        public override int GetHashCode() => (id, timestamp, type, unread, displayed).GetHashCode();
    }
}
