using Microsoft.AspNetCore.SignalR;
using TheGateKeeper.Server.ConnectionManager;

namespace TheGateKeeper.Server
{
    public class EventHub : Hub
    {
        private readonly IConnectionManager _connectionManager;
        public EventHub(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task SendMessage(Task<IEnumerable<FrontEndInfo>> frontEndInfos)
        {
            await Clients.All.SendAsync("ReceiveFrontEndInfo", frontEndInfos);
        }

        public override async Task OnConnectedAsync()
        {
            _connectionManager.AddConnection(Context.ConnectionId);
            await Clients.All.SendAsync("UsersOnline", new GateKeeperAppInfoDtoV1() { UsersOnline = _connectionManager.GetConnectionCount() });
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _connectionManager.RemoveConnection(Context.ConnectionId);
            await Clients.All.SendAsync("UsersOnline", new GateKeeperAppInfoDtoV1() { UsersOnline = _connectionManager.GetConnectionCount() });
            await base.OnDisconnectedAsync(exception);
        }
    }
}
