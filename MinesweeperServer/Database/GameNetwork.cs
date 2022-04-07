using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;

namespace MinesweeperServer.Database
{
    public class GameNetwork : IGameNetwork
    {
        private readonly ConcurrentDictionary<string, User> _users = new();
        
        public bool Join(string name, IServerStreamWriter<ServerMessage> channel) => _users.TryAdd(name, new User { Channel = channel, State = "lobby" });
        public bool Leave(string name) => _users.TryRemove(name, out _);
        public bool IsConnected(string name) => _users.ContainsKey(name);
        public bool AllStates(string state)
        {
            foreach (var user in _users.Values)
            {
                if (user.State != state)
                    return false;
            }
            return true;
        }
        public void DeclareWin(string name)
        {
            foreach (var user in _users)
            {
                if (user.Key == name)
                    user.Value.State = "win";
                else
                    user.Value.State = "lose";
            }
        }
        public void SetPlayerState(string name, string state) => _users[name].State = state;
        public string GetPlayerState(string name) => _users[name].State;
        public async Task SendPlayers(string name)
        {
            foreach (var player in _users.Where(x => x.Key != name))
            {
                await _users[name].Channel.WriteAsync(new ServerMessage { Text = player.Key, State = _users[player.Key].State });
            }
        }
        public async Task Broadcast(ServerMessage message, string name = "")
        {
            foreach (var player in _users.Where(x => x.Key != name))
            {
                await _users[player.Key].Channel.WriteAsync(message);
            }
        }
    }
}