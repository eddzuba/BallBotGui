using Newtonsoft.Json;
using System;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace BallBotGui
{
    internal class StateManager
    {
        public State state = new();
        public List<Player> players = new();

        public string fileName = "pollState.json";
        public string ratingFileName = "playersRating.json";

        public void SaveState()
        {
            string json = JsonConvert.SerializeObject(state);
            File.WriteAllText(fileName, json);
        }

        // Функция для загрузки опроса с диска из формата JSON
        public void LoadState()
        {
            try
            {
                string json = File.ReadAllText(fileName);
                if (json != null)
                {
                    var newState = JsonConvert.DeserializeObject<State>(json);
                    if(newState != null)
                    {
                        state = newState;

                    }
                }
            }
            catch (Exception)
            {

            }
            

        }
        public void LoadPlayers()
        {
            try
            {
                string json = File.ReadAllText(ratingFileName);
                if (json != null)
                {
                    var newRating = JsonConvert.DeserializeObject<List<Player>>(json);
                    if (newRating != null)
                    {
                        players = newRating;

                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public void SavePlayers()
        {
            string json = JsonConvert.SerializeObject(players);
            File.WriteAllText(ratingFileName, json);
        }

        public void AddPlayersToRating(Poll poll)
        {
            try { 
                if(poll != null)
                {
                    if(poll.playrsList.Count > 0)
                    {
                        foreach (var player in poll.playrsList)
                        {
                            if (!players.Any(p => p.id == player.id))
                            {
                                players.Add(new Player(player.id, player.name, player.firstName, player.firstName, false));
                            }
                        }
                    }

                    this.SavePlayers();
                }
            } 
            catch { }   
        }

        public Teams Take2Teams(Update update)
        {
            // получает опрос на сегодняшний день
            Poll? poll = getTodayApprovedGamePoll();
            Teams teams = new();

            if (poll != null)
            {
                bool playerExistsInTopPlayers = poll.playrsList.Take(14).Any(p => p.id == update?.Message?.From?.Id || update.Message.From.Id == 245566701);

                Random random = new();

                if (poll != null && poll.playrsList.Count >= 12 && playerExistsInTopPlayers)
                {
                    // Ограничение списка проголосовавших игроков до максимум 14
                    var votedPlayersLimited = poll.playrsList.Take(14).ToList();

                    // Получаем список игроков, которые проголосовали и переводим в список игроков
                    // Получение списка игроков с их рейтингами
                    var playersWithRatings = votedPlayersLimited
                        .Join(this.players, p => p.id, pr => pr.id, (p, pr) =>
                        new { Player = p, Rating = pr.group, NormalName = pr.normalName, pr.isFemale }).ToList();



                    // Группировка игроков по уровню рейтинга
                    var groupedPlayersWithRatings = playersWithRatings.GroupBy(p => p.Rating);

                    // Создание словаря для хранения игроков каждой группы
                    Dictionary<int, List<(Player player, int Rating)>> groupedPlayersDictionary = new();
                    foreach (var group in groupedPlayersWithRatings.OrderBy(g => g.Key))
                    {
                        // Случайное перемешивание порядка игроков в группе и конвертация в List<(Player, int)>
                        var shuffledGroup = group.OrderBy(x => random.Next())
                                            .Select(x => (new Player(x.Player.id, x.Player.name, x.Player.firstName, x.NormalName, x.isFemale), x.Rating))
                                            .ToList();
                        groupedPlayersDictionary[group.Key] = shuffledGroup;
                    }

                    // Создание списка первых 14 игроков с учетом рейтинга и порядка в группе
                    List<(Player player, int rating)> topPlayers = new();
                    foreach (var group in groupedPlayersDictionary.Values)
                    {
                        int countToAdd = Math.Min(group.Count, 14 - topPlayers.Count);
                        topPlayers.AddRange(group.Take(countToAdd));
                        if (topPlayers.Count >= 14)
                        {
                            break;
                        }
                    }

                    for (int i = 0; i < topPlayers.Count; i++) 
                    {
                        if (i % 2 == 0)
                        {
                            teams.Team1.Add(topPlayers[i].player);
                        }
                        else
                        {
                            teams.Team2.Add(topPlayers[i].player);
                        }
                    }
                    var pList = playersWithRatings.Select(p =>
                    (new Player(p.Player.id, p.Player.name, p.Player.firstName, p.NormalName, p.isFemale), p.Rating)).ToList();
                    FixConflicts(teams, pList);
                    DistributeFemalesEvenly(teams, pList);

                    // Перемешивание игроков в команде 1
                    teams.Team1 = teams.Team1.OrderBy(x => random.Next()).ToList();

                    // Перемешивание игроков в команде 2
                    teams.Team2 = teams.Team2.OrderBy(x => random.Next()).ToList();




                }
            }

            return teams;
        }

        private void FixConflicts(Teams teams, List<(Player player, int rating)> playersWithRatings)
        {
            bool changesMade;
            List<DislikedTeammates> dislikedTeammates = state.dislikedTeammates;
            do
            {
                changesMade = false;

                // Create a copy of the list to iterate without issues when modifying
                var playersToCheck = playersWithRatings.ToList();

                foreach (var (player, rating) in playersToCheck)
                {
                    var team1Contains = teams.Team1.Contains(player);
                    var team2Contains = teams.Team2.Contains(player);

                    if (!team1Contains && !team2Contains)
                        continue; // Player is no longer in teams (possibly replaced earlier)

                    var currentTeam = team1Contains ? teams.Team1 : teams.Team2;
                    var oppositeTeam = team1Contains ? teams.Team2 : teams.Team1;

                    var dislikedPlayer = dislikedTeammates.FirstOrDefault(dt => dt.idPlayer == player.id);
                    var dislikedIds = dislikedPlayer != null ? dislikedPlayer.dislikedPlayers : new List<long>();

                    // Check if the player has conflicts in the current team
                    bool hasConflict = currentTeam.Any(p => dislikedIds.Contains(p.id));

                    if (!hasConflict)
                        continue; // If no conflicts, move to the next player

                    // Find a player to swap with the same rating who doesn't conflict with the new team
                    var swapCandidate = oppositeTeam.FirstOrDefault(p =>
                             playersWithRatings.Any(tp => tp.player.id == p.id && tp.rating == rating) && // тот же рейтинг
                              !dislikedTeammates.Any(dt =>
                                  (dt.idPlayer == p.id && dt.dislikedPlayers.Intersect(currentTeam.Select(tp => tp.id)).Any()) || // кандидат конфликтует с текущей командой
                                  (dt.idPlayer == player.id && dt.dislikedPlayers.Intersect(oppositeTeam.Select(tp => tp.id).Where(id => id != p.id)).Any()) // исходный игрок конфликтует с новой командой, исключая игрока, которого убираем
                              )
                    );

                    if (swapCandidate != null)
                    {
                        // Swap players
                        currentTeam.Remove(player);
                        oppositeTeam.Remove(swapCandidate);

                        currentTeam.Add(swapCandidate);
                        oppositeTeam.Add(player);

                        changesMade = true; // Mark that a swap was made
                    }
                }
            } 
            while (changesMade); // Repeat until no more improvements
        }
        private void DistributeFemalesEvenly(Teams teams, List<(Player player, int rating)> playersWithRatings)
        {
            var dislikedTeammates = state.dislikedTeammates;

            var femalesInTeam1 = teams.Team1.Where(p => p.isFemale).ToList();
            var femalesInTeam2 = teams.Team2.Where(p => p.isFemale).ToList();

            int diff = femalesInTeam1.Count - femalesInTeam2.Count;

            if (Math.Abs(diff) <= 1)
                return; // already balanced

            var fromTeam = diff > 0 ? teams.Team1 : teams.Team2;
            var toTeam = diff > 0 ? teams.Team2 : teams.Team1;

            var femaleCandidates = fromTeam
                .Where(p => p.isFemale)
                .OrderBy(p => Guid.NewGuid()) // randomize
                .ToList();

            foreach (var female in femaleCandidates)
            {
                var rating = playersWithRatings.FirstOrDefault(pr => pr.player.id == female.id).rating;

                var swapCandidate = toTeam.FirstOrDefault(p =>
                    p.isFemale == false &&
                    playersWithRatings.Any(tp => tp.player.id == p.id && tp.rating == rating) &&
                    !dislikedTeammates.Any(dt =>
                        (dt.idPlayer == p.id && dt.dislikedPlayers.Contains(female.id)) ||
                        (dt.idPlayer == female.id && dt.dislikedPlayers.Contains(p.id))
                    ));

                if (swapCandidate != null)
                {
                    fromTeam.Remove(female);
                    toTeam.Remove(swapCandidate);

                    fromTeam.Add(swapCandidate);
                    toTeam.Add(female);

                    break; // Try just one swap for now
                }
            }
        }


        public Teams Take4Teams(Update update)
        {
            // получает опрос на сегодняшний день
            Poll? poll = getTodayApprovedGamePoll();
            bool playerExistsInTopPlayers = poll.playrsList.Take(poll.maxPlayersCount).Any(p => p.id == update?.Message?.From?.Id || update.Message.From.Id == 245566701);

            Random random = new();
            Teams teams = new();

            if (poll != null && poll.playrsList.Count >= 20 && playerExistsInTopPlayers)
            {
                // Ограничение списка проголосовавших игроков до максимум maxPlayersCount
                var votedPlayersLimited = poll.playrsList.Take(poll.maxPlayersCount).ToList();

                // Получаем список игроков, которые проголосовали и переводим в список игроков
                var playersWithRatings = votedPlayersLimited
                    .Join(this.players, p => p.id, pr => pr.id, (p, pr) => new { Player = p, Rating = pr.group, NormalName = pr.normalName, pr.isFemale });

                // Группировка игроков по уровню рейтинга
                var groupedPlayersWithRatings = playersWithRatings.GroupBy(p => p.Rating);

                // Создание словаря для хранения игроков каждой группы
                Dictionary<int, List<(Player player, int Rating)>> groupedPlayersDictionary = new();
                foreach (var group in groupedPlayersWithRatings.OrderBy(g => g.Key))
                {
                    // Случайное перемешивание порядка игроков в группе
                    var shuffledGroup = group.OrderBy(x => random.Next())
                                             .Select(x => (new Player(x.Player.id, x.Player.name, x.Player.firstName, x.NormalName, x.isFemale), x.Rating))
                                             .ToList();
                    groupedPlayersDictionary[group.Key] = shuffledGroup;
                }

                // Создание списка первых poll.maxPlayersCount игроков с учетом рейтинга и порядка в группе
                List<(Player player, int rating)> topPlayers = new();
                foreach (var group in groupedPlayersDictionary.Values)
                {
                    int countToAdd = Math.Min(group.Count, poll.maxPlayersCount - topPlayers.Count);
                    topPlayers.AddRange(group.Take(countToAdd));
                    if (topPlayers.Count >= poll.maxPlayersCount)
                    {
                        break;
                    }
                }

                // Распределение игроков по четырем командам
                for (int i = 0; i < topPlayers.Count; i++)
                {
                    switch (i % 4)
                    {
                        case 0:
                            teams.Team1.Add(topPlayers[i].player);
                            break;
                        case 1:
                            teams.Team2.Add(topPlayers[i].player);
                            break;
                        case 2:
                            teams.Team3.Add(topPlayers[i].player);
                            break;
                        case 3:
                            teams.Team4.Add(topPlayers[i].player);
                            break;
                    }
                }

                // Перемешивание игроков в каждой команде
                teams.Team1 = teams.Team1.OrderBy(x => random.Next()).ToList();
                teams.Team2 = teams.Team2.OrderBy(x => random.Next()).ToList();
                teams.Team3 = teams.Team3.OrderBy(x => random.Next()).ToList();
                teams.Team4 = teams.Team4.OrderBy(x => random.Next()).ToList();
            }

            return teams;
        }
        public void AddVote(string IdPoll, long idPlayer, string name, string lastName, long idVoute)
        {
            var curPoll = state.pollList.FirstOrDefault(x => x.idPoll == IdPoll);
            if(curPoll != null)
            {
                curPoll.AddPlayerToList(idPlayer, name, lastName, idVoute);
                AddPlayersToRating(curPoll);
            }
        }

        internal bool RemoteVote(string idPoll, long idPlayer)
        {
            var curPoll = state.pollList.FirstOrDefault(x => x.idPoll == idPoll);
            if (curPoll != null)
            {
                return curPoll.DeletePlayerFromList(idPlayer);
            
            }
            return false;
        }

        internal void ArchPolls(TelegramBotClient botClient)
        {
            var curTime = DateTime.Now; ;
            // Архивировать опросы с датой раньше текущей даты
            foreach (var poll in state.pollList.Where(p => p.GetGameDate() < curTime.AddDays(0)).ToList())
            {
                ArchivePoll(poll);
                state.pollList.Remove(poll);
                if(poll.idMessage > 0)
                {
                    botClient.UnpinChatMessage(Properties.Settings.Default.chatId, poll.idMessage);
                }
                
            }

            SaveState();
            
        }

        private void ArchivePoll(Poll poll)
        {
            var archiveFolderName = "Arch";
            var polDate = poll.GetGameDate().ToString("ddMMyyyy");
            var archiveFolderPath = Path.Combine(Directory.GetCurrentDirectory(), archiveFolderName);

            if (!Directory.Exists(archiveFolderPath))
            {
                Directory.CreateDirectory(archiveFolderPath);
            }

            string json = JsonConvert.SerializeObject(poll);
            var fileName = $"Arch{polDate}.json";
            var filePath = Path.Combine(archiveFolderPath, fileName);

            File.WriteAllText(filePath, json);

        }

        internal Poll? getTodayApprovedGamePoll()
        {
            var now = DateTime.Now.ToString("dd.MM");
            // TODO Uncomment!!!!
            Poll ?poll = state.pollList.FirstOrDefault(x =>  x.date == now &&  x.approved);

            return poll;
        }
    }
}
