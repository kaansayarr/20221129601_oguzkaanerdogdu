using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Internet_1.Hubs
{
    public class GeneralHub : Hub
    {
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }

        public async Task UpdateCounts(int lessonCount, int videoCount, int instructorCount, int userCount)
        {
            await Clients.All.SendAsync("UpdateCounts", lessonCount, videoCount, instructorCount, userCount);
        }
    }
}
