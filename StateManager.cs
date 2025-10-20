using Newtonsoft.Json;
using System;
using System.Linq;
using System.Numerics;
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
                    if (newState != null)
                    {
                        // Преобразуем все стоп-слова в нижний регистр (инвариантно)
                        newState.spamStopWords = newState.spamStopWords
                            .Select(w => w.ToLowerInvariant())
                            .Distinct()
                            .ToList();

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
            try
            {
                if (poll != null)
                {
                    if (poll.playrsList.Count > 0)
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
        public int getPlayerRating(Update update)
        {
            if (update?.Message?.From?.Id == null)
                return 0;

            long playerId = update.Message.From.Id;
            var player = players.FirstOrDefault(p => p.id == playerId);
            return player?.rating ?? 0;
        }

        public string getPlayerRatingText(Update update)
        {
            var rating = getPlayerRating(update);
            return rating switch
            {
                1 => "A",
                2 => "B",
                3 => "C",
                4 => "D",
                _ => "Не задан."
            };
        }

        public Teams Take2Teams(Update update)
        {
            // получает ближайщий опрос на сегодняшний день
            Poll? poll = GetClosestApprovedPollForToday();
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

                foreach (var (player, rating) in playersWithRatings.ToList()) // копия списка для безопасного перебора
                {
                    var isInTeam1 = teams.Team1.Contains(player);
                    var isInTeam2 = teams.Team2.Contains(player);

                    if (!isInTeam1 && !isInTeam2)
                        continue; // игрок уже не в командах

                    var currentTeam = isInTeam1 ? teams.Team1 : teams.Team2;
                    var oppositeTeam = isInTeam1 ? teams.Team2 : teams.Team1;

                    // Проверяем конфликт игрока с текущей командой
                    if (!HasConflictWithTeam(player, currentTeam.Where(p => p.id != player.id), dislikedTeammates))
                        continue;

                    // Ищем кандидата на обмен с таким же рейтингом
                    var swapCandidate = oppositeTeam.FirstOrDefault(p =>
                        playersWithRatings.Any(tp => tp.player.id == p.id && tp.rating == rating) &&
                        !HasConflictWithTeam(player, oppositeTeam.Where(x => x.id != p.id).Append(p), dislikedTeammates) &&
                        !HasConflictWithTeam(p, currentTeam.Where(x => x.id != player.id).Append(player), dislikedTeammates)
                    );

                    if (swapCandidate != null)
                    {
                        // Обмен
                        currentTeam.Remove(player);
                        oppositeTeam.Remove(swapCandidate);

                        currentTeam.Add(swapCandidate);
                        oppositeTeam.Add(player);

                        changesMade = true;
                    }
                }
            } while (changesMade);
        }

        private void DistributeFemalesEvenly(Teams teams, List<(Player player, int rating)> playersWithRatings)
        {
            var dislikedTeammates = state.dislikedTeammates;

            while (true)
            {
                var femalesInTeam1 = teams.Team1.Where(p => p.isFemale).ToList();
                var femalesInTeam2 = teams.Team2.Where(p => p.isFemale).ToList();

                int diff = femalesInTeam1.Count - femalesInTeam2.Count;

                if (Math.Abs(diff) <= 1)
                    return;

                var fromTeam = diff > 0 ? teams.Team1 : teams.Team2;
                var toTeam = diff > 0 ? teams.Team2 : teams.Team1;

                var femaleCandidates = fromTeam
                    .Where(p => p.isFemale)
                    .OrderBy(_ => Guid.NewGuid())
                    .ToList();

                bool swapped = false;

                foreach (var female in femaleCandidates)
                {
                    var rating = playersWithRatings.FirstOrDefault(pr => pr.player.id == female.id).rating;

                    var swapCandidate = toTeam.FirstOrDefault(p =>
                        !p.isFemale &&
                        playersWithRatings.Any(tp => tp.player.id == p.id && tp.rating == rating) &&
                        !dislikedTeammates.Any(dt =>
                            (dt.idPlayer == p.id && dt.dislikedPlayers.Contains(female.id)) ||
                            (dt.idPlayer == female.id && dt.dislikedPlayers.Contains(p.id))
                        ));

                    if (swapCandidate == null)
                        continue;

                    // Исключаем игроков, которых заменяют
                    var futureToTeam = toTeam.Where(p => p.id != swapCandidate.id).Append(female);
                    var futureFromTeam = fromTeam.Where(p => p.id != female.id).Append(swapCandidate);

                    // Проверяем отсутствие конфликтов в обеих новых командах
                    if (HasConflictWithTeam(female, futureToTeam, dislikedTeammates))
                        continue;

                    if (HasConflictWithTeam(swapCandidate, futureFromTeam, dislikedTeammates))
                        continue;

                    // Выполняем обмен
                    fromTeam.Remove(female);
                    toTeam.Remove(swapCandidate);

                    fromTeam.Add(swapCandidate);
                    toTeam.Add(female);

                    swapped = true;
                    break;
                }

                if (!swapped)
                    break;
            }
        }

        private bool HasConflictWithTeam(Player player, IEnumerable<Player> team, List<DislikedTeammates> dislikedTeammates)
        {
            return team.Any(other =>
                other.id != player.id &&
                dislikedTeammates.Any(dt =>
                    (dt.idPlayer == player.id && dt.dislikedPlayers.Contains(other.id)) ||
                    (dt.idPlayer == other.id && dt.dislikedPlayers.Contains(player.id))
                )
            );
        }


        public Teams Take4Teams(Update update)
        {

            /* // получает опрос на сегодняшний день
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

             return teams;*/
            return null;
        }
        public void AddVote(string IdPoll, long idPlayer, string userName, string firstName, long idVoute)
        {
            var curPoll = state.pollList.FirstOrDefault(x => x.idPoll == IdPoll);
            if (curPoll != null)
            {
                var curPlayer  = players.FirstOrDefault( x => x.id == idPlayer);
                if (curPlayer == null) {
                    curPlayer = new Player(idPlayer, firstName, userName, userName, false);
                    players.Add(curPlayer);
                }

                if (curPlayer != null)
                {
                    curPoll.AddPlayerToList(idPlayer, userName, firstName, idVoute, curPlayer.rating);
                    
                    if(curPlayer.name !=  userName )
                    {
                        curPlayer.name = userName;
                    }
                   
                }
                AddPlayersToRating(curPoll);
                SaveState();
            
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
                if (poll.idMessage > 0)
                {
                    botClient.UnpinChatMessage(Properties.Settings.Default.chatId, poll.idMessage);
                }

            }

            SaveState();

        }

        private void ArchivePoll(Poll poll)
        {
            var archiveFolderName = "Arch";
            var pollDate = poll.GetGameDate().ToString("ddMMyyyy");
            var archiveFolderPath = Path.Combine(Directory.GetCurrentDirectory(), archiveFolderName);

            if (!Directory.Exists(archiveFolderPath))
            {
                Directory.CreateDirectory(archiveFolderPath);
            }

            string json = JsonConvert.SerializeObject(poll);
            var fileName = $"Arch{pollDate}.json";
            var filePath = Path.Combine(archiveFolderPath, fileName);

            File.WriteAllText(filePath, json);

        }

        internal List<Poll> getTodayApprovedGamePoll()
        {
            var now = DateTime.Now.ToString("dd.MM");
            // Возвращаем все опросы, которые соответствуют сегодняшней дате и одобрены
            var polls = state.pollList.Where(x => x.date == now && x.approved).ToList();
            return polls;
        }

        internal Poll? GetClosestApprovedPollForToday()
        {
            var now = DateTime.Now;

            // Получаем все опросы, которые соответствуют сегодняшней дате и одобрены
            var todayPolls = state.pollList
                .Where(x => x.date == now.ToString("dd.MM") && x.approved)
                .OrderBy(p => new DateTime(now.Year, now.Month, now.Day, p.curGame.GameStartHour, p.curGame.GameStartMinute, 0)) // Сортируем по времени начала игры
                .ToList();

            if (!todayPolls.Any())
                return null; // Если опросов нет, возвращаем null

            // Проверяем, есть ли игры, которые уже начались
            // TODO: нужно не отнимать один час, это должна быть настройка, для разных часовых поясов
            var startedPolls = todayPolls.Where(p =>
                  new DateTime(now.Year, now.Month, now.Day, p.curGame.GameStartHour - 1, p.curGame.GameStartMinute, 0) <= now).ToList();

            if (startedPolls.Any())
            {
                // Если есть начавшиеся игры, возвращаем последнюю из них
                return startedPolls.Last();
            }

            // Если ни одна игра ещё не началась, возвращаем самую раннюю из всех
            return todayPolls.First();
        }

        internal (string message, string type, Player? player) getRatingRequestStatusText(Update update)
        {
            if (update?.Message?.From?.Id == null)
                return ("Ошибка: не удалось определить пользователя.", "error", null);

            long playerId = update.Message.From.Id;
            var player = players.FirstOrDefault(p => p.id == playerId);

            if (player == null)
                return ("Ошибка: игрок не найден.", "error", null);

            DateTime lastRequestDate;
            bool hasDate = DateTime.TryParse(player.ratingRequestDate, out lastRequestDate);

            if (hasDate && (DateTime.Now - lastRequestDate).TotalDays < 60)
            {
                return ($"Нельзя запрашивать чаще одного раза в два месяца. Последний запрос был: {lastRequestDate:dd.MM.yyyy}", "limit", player);
            }

            // Сохраняем новую дату запроса
            player.ratingRequestDate = DateTime.Now.ToString("yyyy-MM-dd");
            SavePlayers();

            return ("Запрос на рейтинг отправлен", "success", player);
        }
    }
       
}
