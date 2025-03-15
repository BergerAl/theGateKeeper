using Microsoft.AspNetCore.SignalR;

namespace TheGateKeeper.Server
{
    public class EventHub : Hub
    {
        public async Task SendMessage(Task<IEnumerable<FrontEndInfo>> frontEndInfos)
        {
            await Clients.All.SendAsync("ReceiveFrontEndInfo", frontEndInfos);
        }
    }
}
