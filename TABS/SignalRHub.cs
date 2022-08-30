using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TABS.Data;

namespace TABS
{
    [Authorize()]
    public class SignalRHub : Hub
    {
        public const string HubUrl = "/chat";

        public EmailService _emailService { get; set; }

        /// <summary>
        /// Inject state-based services for initialization
        /// </summary>
        public SignalRHub(EmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task BroadcastAnnouncement(User sender, List<User> recievingUsers, Announcement announcement)
        {
            await Clients.All.SendAsync("ReceiveNotification",
                                        announcement);

            foreach (User user in recievingUsers)
            {
                if (user.UserID != sender.UserID)
                {
                    // TODO: send announcement email
                }
            }
        }

        public async Task SendNotification(string recieverAdID, string notif)
        {
            await Clients.User(recieverAdID).SendAsync("ReceiveNotification", notif);
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"{Context.ConnectionId}, {Context.UserIdentifier} connected");
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}");
            await base.OnDisconnectedAsync(e);
        }
    }
}