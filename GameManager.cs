using System.Text.Json;
using BallBotGui;

internal class GameManager
{
    public  List<VolleybollGame> Games = new();
    private readonly TelegramConnector telConnector;

    public GameManager(TelegramConnector telegramConnector) {
        telConnector = telegramConnector;
        LoadGames();
    }   

    private void LoadGames()
    {
        var json = BallBotGui.Properties.Settings.Default.GamesJson;
        if (string.IsNullOrWhiteSpace(json))
        {
            Games = new();
        }

        try
        {
            Games =  JsonSerializer.Deserialize<List<VolleybollGame>>(json) ?? new List<VolleybollGame>();
        }
        catch
        {
            // Если формат JSON неверный, вернуть пустой список
            Games = new();
        }
    }

    public static void SaveGames(List<VolleybollGame> games)
    {
        var json = JsonSerializer.Serialize(games, options: new JsonSerializerOptions { WriteIndented = true });
        BallBotGui.Properties.Settings.Default.GamesJson = json;
        BallBotGui.Properties.Settings.Default.Save();
    }
    public  async Task<bool> CheckScheduleAndCreatePollAsync(DateTime currentTime)
    {

        // Поиск подходящей игры
        foreach (var game in Games)
        {
            if (game.IsTimeToCreatePoll(currentTime))
            {
                // CreatePullDay(game);
                // await telConnector.createOnePoll(currentTime.AddDays(pollBeforeGame));
                await telConnector.createOnePoll(currentTime, game);
                return true; // Создаём голосование только для одной подходящей игры
            }
        }
        return false;
    }
}
