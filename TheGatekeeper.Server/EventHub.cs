using Microsoft.AspNetCore.SignalR;

namespace ProductionSheet.Server
{
    public class EventHub : Hub
    {
        //public async Task SendMessage(Task<IEnumerable<WorkpieceListStructure>> workpieceListStructures)
        //{
        //    await Clients.All.SendAsync("ReceiveMessage", workpieceListStructures);
        //}
    }
}
