using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CafePOS.API.Hubs;

public class OrderHub : Hub
{
    // Extensible helper methods allowing clients to join/leave specific station groups
    public async Task JoinStationGroup(string stationName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, stationName);
    }

    public async Task LeaveStationGroup(string stationName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, stationName);
    }
}
