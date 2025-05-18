namespace TheGateKeeper.Server.ConnectionManager
{
    public interface IConnectionManager
    {
        void AddConnection(string connectionId);
        void RemoveConnection(string connectionId);
        int GetConnectionCount();
    }

    public class ConnectionManager : IConnectionManager
    {
        private static HashSet<string> _connections = [];

        public void AddConnection(string connectionId)
        {
            _connections.Add(connectionId);
        }

        public void RemoveConnection(string connectionId)
        {
            _connections.Remove(connectionId);
        }

        public int GetConnectionCount()
        {
            return _connections.Count;
        }
    }
}