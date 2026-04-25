using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MadnShared.Logger;
using MadnShared.Messages.Base;
using MadnShared.Messages.ClientToServer;
using MadnShared.Messages.ServerToClient;
using MadnShared.Utils;
using System.IO;
using System.Text.Json;
using MadnShared.Stats;
using System.Linq;
namespace MadnClient;

public class ConsoleClient
{
    private Guid _playerId = Guid.Empty;
    private TaskCompletionSource<ListGamesResponseMessage> _listGamesTcs;
    private TaskCompletionSource<GameCreatedMessage> _createGameTcs;
    private TaskCompletionSource<GameJoinedMessage> _joinGameTcs;
    private readonly IWebSocketClient _wsClient;
    private GameFrontend _frontend;
    private TaskCompletionSource<Guid> _welcomeTcs;

    public ConsoleClient(IWebSocketClient wsClient)
    {
        _wsClient = wsClient;
        _wsClient.MessageReceived += OnWsMessageReceived;
        _welcomeTcs = new TaskCompletionSource<Guid>();
    }

    public async Task RunAsync(string serverUri)
    {
        await _wsClient.ConnectAsync(serverUri);
        ShowWelcome();
        Console.ReadKey(true);

        while (true)
        {
            var choice = ShowMenu();
            if (choice == "1")
            {
                Console.Clear();
                Console.WriteLine("Erstelle Spiel ...");
                Logger.LogInfo("Requested to create game");
                var response = await SendCreateGameAsync();
                if (response == null)
                {
                    Console.WriteLine("Spiel konnte nicht erstellt werden.");
                    Logger.LogError("Failed to create game");
                }
                else
                {
                    _frontend = new GameFrontend(_wsClient, _playerId, response.Color, response.Gameboard);
                    await _frontend.EnterGameAsync(response.GameId);
                }

            }
            else if (choice == "2")
            {
                var response = await ListGamesAsync();
                if (response.Games == null)
                {
                    Logger.LogError("Failed to get game list from server.");
                    continue;
                }

                var game = ShowGameList(response);
                if (game == "b" || game == "B")
                {
                    continue;
                }
                else if (game != null && int.TryParse(game, out int id))
                {
                    if (id < 1 || id > response.Games.Count)
                    {
                        Console.WriteLine("Invalid game number");
                        continue;
                    }

                    var gameId = response.Games.Keys.ElementAtOrDefault(id - 1);

                    var res = await SendJoinGameAsync(gameId);

                    if (res == null)
                    {
                        Console.WriteLine("Beitritt nicht möglich");
                        Logger.LogInfo("Failed to join game");
                    }
                    else
                    {
                        _frontend = new GameFrontend(_wsClient, _playerId, res.Color, res.Gameboard);
                        await _frontend.EnterGameAsync(res.GameId);
                    }
                }
                else
                {
                    Console.WriteLine("Invalid choice");
                }
            }
            else if (choice == "3")
            {
                // Matchhistory
                await ShowMatchHistoryAsync();
            }
            else if (choice == "q" || choice == "Q")
            {
                await CloseAsync();
                Logger.LogInfo($"Client closed");
                break;
            }
            else
            {
                Console.WriteLine("Invalid choice");
            }
        }
    }

    private void ShowWelcome()
    {
        Console.Clear();
        Console.WriteLine("Willkommen zu Mensch ärgere dich nicht");
        Console.WriteLine();
        Console.WriteLine("Beliebige Taste drücken...");
    }

    private string ShowMenu()
    {
        Console.Clear();
        Console.WriteLine("Menü:");
        Console.WriteLine("1) Spiel erstellen");
        Console.WriteLine("2) Spiel beitreten");
        Console.WriteLine("3) Matchhistory");
        Console.WriteLine("Q) Beenden");
        Console.Write("Auswahl: ");
        var key = Console.ReadKey(true);
        Console.WriteLine(key.KeyChar);
        return key.KeyChar.ToString();
    }

    private async Task ShowMatchHistoryAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Matchhistory\n");
            var files = FindMatchFiles();
            if (files == null || files.Count == 0)
            {
                Console.WriteLine("Keine Matches gefunden.");
                Console.WriteLine("Drücke eine Taste, um zurückzugehen...");
                Console.ReadKey(true);
                return;
            }

            Console.WriteLine("Num | GameId                                 | StartTime               | Winner");
            Console.WriteLine(new string('-', 80));
            for (int i = 0; i < files.Count; i++)
            {
                var path = files[i];
                try
                {
                    var json = File.ReadAllText(path);
                    var ms = JsonSerializer.Deserialize<MatchStats>(json);
                    var winner = ms?.WinnerColor.HasValue == true ? ms.WinnerColor.Value.ToString() : "-";
                    var start = ms?.StartTime.ToString("s") ?? "-";
                    Console.WriteLine($"{i + 1,3} | {ms?.GameId,-36} | {start,-22} | {winner}");
                }
                catch
                {
                    Console.WriteLine($"{i + 1,3} | {Path.GetFileName(path),-36} | (invalid) ");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Gib die Nummer eines Spiels ein, um Details anzusehen, oder 'b' um zurückzugehen:");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) continue;
            if (input.Equals("b", StringComparison.OrdinalIgnoreCase)) return;
            if (int.TryParse(input, out int sel))
            {
                if (sel < 1 || sel > files.Count)
                {
                    Console.WriteLine("Ungültige Auswahl, drücke eine Taste...");
                    Console.ReadKey(true);
                    continue;
                }

                var selPath = files[sel - 1];
                ShowMatchDetails(selPath);
            }
        }
    }

    private List<string> FindMatchFiles()
    {
        var candidates = new List<string>();
        // try several reasonable locations relative to the client
        var baseDir = AppContext.BaseDirectory;
        // 1) repo root MadnServer/logs/matches (upwards search for .sln)
        var dir = new DirectoryInfo(baseDir);
        DirectoryInfo? root = dir;
        while (root != null && root.Parent != null)
        {
            if (root.GetFiles("*.sln").Any()) break;
            root = root.Parent;
        }

        if (root != null && root.GetFiles("*.sln").Any())
        {
            // prefer repoRoot/MadnServer/logs/matches
            var p1 = Path.Combine(root.FullName, "MadnServer", "logs", "matches");
            var p2 = Path.Combine(root.FullName, "logs", "matches");
            candidates.Add(p1);
            candidates.Add(p2);
        }

        // 2) relative to current working directory
        candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), "MadnServer", "logs", "matches"));
        candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), "logs", "matches"));

        // 3) relative to AppContext.BaseDirectory
        candidates.Add(Path.Combine(AppContext.BaseDirectory, "..", "..", "MadnServer", "logs", "matches"));
        candidates.Add(Path.Combine(AppContext.BaseDirectory, "..", "..", "logs", "matches"));

        foreach (var c in candidates)
        {
            try
            {
                var full = Path.GetFullPath(c);
                if (Directory.Exists(full))
                {
                    var files = Directory.GetFiles(full, "*.json").OrderByDescending(File.GetLastWriteTime).ToList();
                    if (files.Count > 0) return files;
                }
            }
            catch
            {
                /* ignore invalid paths */
            }
        }

        return new List<string>();
    }

    private void ShowMatchDetails(string filePath)
    {
        Console.Clear();
        Console.WriteLine($"Match: {Path.GetFileName(filePath)}\n");
        try
        {
            var json = File.ReadAllText(filePath);
            var ms = JsonSerializer.Deserialize<MatchStats>(json);
            if (ms == null)
            {
                Console.WriteLine("Could not parse match file.");
                Console.WriteLine("Drücke eine Taste um zurückzugehen...");
                Console.ReadKey(true);
                return;
            }

            Console.WriteLine($"GameId: {ms.GameId}");
            Console.WriteLine($"Start: {ms.StartTime}");
            Console.WriteLine($"End: {ms.EndTime}");
            Console.WriteLine($"TotalTurns: {ms.TotalTurns}");
            Console.WriteLine(
                $"Winner: {(ms.WinnerColor.HasValue ? ms.WinnerColor.Value.ToString() : "-")} ({(ms.WinnerPlayerId.HasValue ? ms.WinnerPlayerId.ToString() : "-")})\n");

            Console.WriteLine("Players:");
            Console.WriteLine("Color    MovementCount    Captures");
            Console.WriteLine(new string('-', 40));
            foreach (var p in ms.Players)
            {
                Console.WriteLine($"{p.Color,-8} {p.MovementCount,14} {p.Captures,12}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fehler beim Lesen der Match-Datei: " + ex.Message);
        }

        Console.WriteLine();
        Console.WriteLine("Drücke eine Taste um zurückzugehen...");
        Console.ReadKey(true);
    }

    private async Task SendMessageAsync(IMessage message)
    {
        try
        {
            await _wsClient.SendAsync(message);
        }
        catch (Exception ex)
        {
            Logger.LogError("Cannot send message: " + ex.Message);
        }
    }

    private async Task<GameCreatedMessage> SendCreateGameAsync()
    {
        try
        {
            _createGameTcs = new TaskCompletionSource<GameCreatedMessage>();
            var createMsg = new CreateGameMessage();

            await SendMessageAsync(createMsg);
            Logger.LogInfo($"Sent CreateGame message");
            return await _createGameTcs.Task;
        }
        catch (Exception ex)
        {
            Logger.LogError("Exception when sending CreateGame message: " + ex.Message);
            return null;
        }
    }

    private async Task<GameJoinedMessage> SendJoinGameAsync(Guid gameId)
    {
        try
        {
            _joinGameTcs = new TaskCompletionSource<GameJoinedMessage>();
            var createMsg = new JoinGameMessage
            {
                GameId = gameId,
                PlayerId = _playerId
            };

            await SendMessageAsync(createMsg);
            Logger.LogInfo($"Sent JoinGame message with GameId {createMsg.GameId}");
            return await _joinGameTcs.Task;
        }
        catch (Exception ex)
        {
            Logger.LogError("Exception when sending JoinGame message: " + ex.Message);
            return null;
        }
    }

    private async Task<ListGamesResponseMessage> ListGamesAsync()
    {
        try
        {
            _listGamesTcs = new TaskCompletionSource<ListGamesResponseMessage>();
            var listMsg = new ListGamesMessage();

            await SendMessageAsync(listMsg);
            Logger.LogInfo($"Sent ListGames message");
            return await _listGamesTcs.Task;
        }
        catch (Exception ex)
        {
            Logger.LogError("Exception when sending JoinGame message: " + ex.Message);
            return null;
        }
    }

    private void OnWsMessageReceived(IMessage message)
    {
        Logger.LogInfo($"Received message: {message}");
        switch (message)
        {
            case WelcomeMessage welcome:
                _playerId = welcome.ClientId;
                Logger.LogInfo($"Received WelcomeMessage. Assigned client id {_playerId}");
                _welcomeTcs?.TrySetResult(_playerId);
                break;
            case GameCreatedMessage createdMsg:
                _createGameTcs?.TrySetResult(createdMsg);
                break;
            case ListGamesResponseMessage listResponse:
                _listGamesTcs?.TrySetResult(listResponse);
                break;
            case GameJoinedMessage joinResponse:
                _joinGameTcs?.TrySetResult(joinResponse);
                break;
        }
    }

    private async Task CloseAsync()
    {
        try
        {
            await _wsClient.CloseAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError("Error closing WebSocket: " + ex.Message);
        }
    }

    private string ShowGameList(ListGamesResponseMessage response)
    {
        Console.Clear();
        Console.WriteLine("Verfügbare Spiele:");
        const string headerFormat = "| {0,-3} | {1,-36} | {2,7} |";
        const string divider = "+-----+--------------------------------------+---------+";

        Console.WriteLine(divider);
        Console.WriteLine(headerFormat, "Nr.", "GameId", "Spieler");
        Console.WriteLine(divider);

        int i = 1;
        foreach (var game in response.Games)
        {
            Console.WriteLine(headerFormat, i, game.Key, $"{game.Value}/4");
            i++;
        }

        Console.WriteLine(divider);
        Console.WriteLine(
            "Geben Sie eine Nummer ein, um einem Spiel beizutreten, oder 'b' um zurück zum Menü zu gehen:");
        var input = Console.ReadLine();
        return input ?? string.Empty;
    }

}


