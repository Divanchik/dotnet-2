using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MinesweeperServer.Database
{
    public class GameRepository : IGameRepository
    {
        private ConcurrentDictionary<string, Player> _players = new();
        private readonly IConfiguration _config;
        public Player this[string name] => _players[name];

        public GameRepository(IConfiguration config)
        {
            _config = config;
        }
        public bool TryAddPlayer(string name) => _players.TryAdd(name, new Player());
        public void Load()
        {
            if (File.Exists(_config["PathPlayers"]))
            {
                string jsonString = File.ReadAllText(_config["PathPlayers"]);
                _players = JsonSerializer.Deserialize<ConcurrentDictionary<string, Player>>(jsonString);
            }
        }
        public async Task DumpAsync()
        {
            await using FileStream stream = File.Create(_config["pathPlayers"]);
            await JsonSerializer.SerializeAsync<ConcurrentDictionary<string, Player>>(stream, _players, new JsonSerializerOptions { WriteIndented = true });
        }
        public bool CalcScore(string name, string state)
        {
            switch (state)
            {
                case "win":
                    _players[name].TotalScore += _config.GetValue<int>("WinPoints");
                    _players[name].WinCount++;
                    _players[name].WinStreak++;
                    if (_players[name].WinStreak % _config.GetValue<int>("StreakCount") == 0)
                        _players[name].TotalScore += _config.GetValue<int>("StreakBonus");
                    break;
                case "lose":
                    _players[name].TotalScore -= _config.GetValue<int>("LosePoints");
                    if (_players[name].TotalScore < 0)
                        _players[name].TotalScore = 0;
                    _players[name].LoseCount++;
                    _players[name].WinStreak = 0;
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}