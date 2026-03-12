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
            try
            {
                string json = JsonConvert.SerializeObject(state);
                File.WriteAllText(fileName, json);
            }
            catch (Exception ex)
            {
                Logger.Log("Ошибка при сохранении состояния", ex);
            }
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
            catch (Exception ex)
            {
                Logger.Log("Ошибка при загрузке состояния", ex);
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
            catch (Exception ex)
            {
                Logger.Log("Ошибка при загрузке игроков", ex);
            }
        }

        public void SavePlayers()
        {
            try
            {
                string json = JsonConvert.SerializeObject(players);
                File.WriteAllText(ratingFileName, json);
            }
            catch (Exception ex)
            {
                Logger.Log("Ошибка при сохранении игроков", ex);
            }
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

        public Teams Take2Teams(string idPoll, Update update)
        {
            // Находим конкретную игру по idPoll
            Poll? poll = state.pollList.FirstOrDefault(p => p.idPoll == idPoll);
            Teams teams = new();

            if (poll != null)
            {
                long? requesterId = update?.Message?.From?.Id;
                bool playerExistsInTopPlayers = poll.playrsList.Take(poll.maxPlayersCount).Any(p => p.id == requesterId || (requesterId.HasValue && requesterId.Value == 245566701));

                Random random = new();

                if (poll.playrsList.Count >= 12 && playerExistsInTopPlayers)
                {
                    // Ограничение списка проголосовавших игроков до poll.maxPlayersCount
                    var votedPlayersLimited = poll.playrsList.Take(poll.maxPlayersCount).ToList();

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

                    // Распределение игроков по двум командам
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

                    var allTeams = new List<List<Player>> { teams.Team1, teams.Team2 };
                    FixConflicts(allTeams, pList);
                    DistributeFemalesEvenly(allTeams, pList);

                    // Перемешивание игроков в каждой команде
                    teams.Team1 = teams.Team1.OrderBy(x => random.Next()).ToList();
                    teams.Team2 = teams.Team2.OrderBy(x => random.Next()).ToList();




                }
            }

            return teams;
        }

        private void FixConflicts(List<List<Player>> allTeams, List<(Player player, int rating)> playersWithRatings)
        {
            bool changesMade;
            List<DislikedTeammates> dislikedTeammates = state.dislikedTeammates;

            do
            {
                changesMade = false;

                foreach (var team in allTeams)
                {
                    var teamList = team.ToList(); // copy for iteration
                    foreach (var player in teamList)
                    {
                        var ratingObj = playersWithRatings.FirstOrDefault(pr => pr.player.id == player.id);
                        if (ratingObj.player == null) continue;
                        int rating = ratingObj.rating;

                        // Check conflict of player with current team
                        if (!HasConflictWithTeam(player, team.Where(p => p.id != player.id), dislikedTeammates))
                            continue;

                        // Try to find a swap candidate in other teams with the same rating
                        foreach (var otherTeam in allTeams.Where(t => t != team))
                        {
                            var swapCandidate = otherTeam.FirstOrDefault(p =>
                                playersWithRatings.Any(tp => tp.player.id == p.id && tp.rating == rating) &&
                                !HasConflictWithTeam(player, otherTeam.Where(x => x.id != p.id).Append(p), dislikedTeammates) &&
                                !HasConflictWithTeam(p, team.Where(x => x.id != player.id).Append(player), dislikedTeammates)
                            );

                            if (swapCandidate != null)
                            {
                                // Swap
                                team.Remove(player);
                                otherTeam.Remove(swapCandidate);

                                team.Add(swapCandidate);
                                otherTeam.Add(player);

                                changesMade = true;
                                break;
                            }
                        }
                        if (changesMade) break;
                    }
                    if (changesMade) break;
                }
            } while (changesMade);
        }

        private void DistributeFemalesEvenly(List<List<Player>> allTeams, List<(Player player, int rating)> playersWithRatings)
        {
            var dislikedTeammates = state.dislikedTeammates;
            bool changed = true;

            // Будем пытаться выравнивать, пока происходят успешные обмены
            while (changed)
            {
                changed = false;
                
                // Сортируем команды по количеству девушек (от большего к меньшему)
                var teamInfos = allTeams
                    .Select((t, i) => new { Index = i, Count = t.Count(p => p.isFemale) })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                var maxCount = teamInfos.First().Count;
                var minCount = teamInfos.Last().Count;

                // Если разница между самой "женской" и самой "мужской" командой <= 1, то распределение идеальное
                if (maxCount - minCount <= 1)
                    break;

                // Сначала пытаемся найти идеальный обмен (с тем же рейтингом) между любыми подходящими парами команд
                if (TryImproveBalance(allTeams, teamInfos, playersWithRatings, 0, dislikedTeammates))
                {
                    changed = true;
                    continue;
                }

                // Если идеальных обменов нет, пробуем обмен с разницей в рейтинге +-1
                if (TryImproveBalance(allTeams, teamInfos, playersWithRatings, 1, dislikedTeammates))
                {
                    changed = true;
                    continue;
                }
                
                // Если даже с допуском ничего не вышло - выходим
                break;
            }
        }

        private bool TryImproveBalance(List<List<Player>> allTeams, dynamic teamInfos, List<(Player player, int rating)> playersWithRatings, int tolerance, List<DislikedTeammates> dislikedTeammates)
        {
            foreach (var fromInfo in teamInfos)
            {
                // Пробуем команды с количеством девушек выше среднего
                if (fromInfo.Count <= 1) continue; 

                foreach (var toInfo in teamInfos)
                {
                    // Пробуем перекинуть в команду, где девушек заметно меньше
                    if (fromInfo.Count - toInfo.Count <= 1) continue;

                    var teamA = allTeams[fromInfo.Index];
                    var teamB = allTeams[toInfo.Index];

                    if (TrySwapBetweenTeams(teamA, teamB, playersWithRatings, tolerance, dislikedTeammates))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TrySwapBetweenTeams(List<Player> fromTeam, List<Player> toTeam, List<(Player player, int rating)> playersWithRatings, int tolerance, List<DislikedTeammates> dislikedTeammates)
        {
            var females = fromTeam.Where(p => p.isFemale).OrderBy(_ => Guid.NewGuid()).ToList();
            var potentials = toTeam.Where(p => !p.isFemale).OrderBy(_ => Guid.NewGuid()).ToList();

            foreach (var female in females)
            {
                int fRating = playersWithRatings.FirstOrDefault(pr => pr.player.id == female.id).rating;

                foreach (var male in potentials)
                {
                    int mRating = playersWithRatings.FirstOrDefault(pr => pr.player.id == male.id).rating;

                    // Проверяем допуск по рейтингу
                    if (Math.Abs(fRating - mRating) <= tolerance)
                    {
                        // Проверяем конфликты (disliked)
                        if (HasConflictWithTeam(female, toTeam.Where(p => p.id != male.id), dislikedTeammates)) continue;
                        if (HasConflictWithTeam(male, fromTeam.Where(p => p.id != female.id), dislikedTeammates)) continue;

                        // Выполняем обмен
                        fromTeam.Remove(female);
                        toTeam.Remove(male);
                        fromTeam.Add(male);
                        toTeam.Add(female);
                        return true;
                    }
                }
            }
            return false;
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
            // получает опрос на сегодняшний день
            Poll? poll = GetClosestApprovedPollForToday(update?.Message?.From?.Id);
            Teams teams = new();

            if (poll != null)
            {
                bool playerExistsInTopPlayers = poll.playrsList.Take(poll.maxPlayersCount).Any(p => p.id == update?.Message?.From?.Id || update.Message.From.Id == 245566701);

                Random random = new();

                if (poll.playrsList.Count >= 20 && playerExistsInTopPlayers)
                {
                    // Ограничение списка проголосовавших игроков до максимум maxPlayersCount
                    var votedPlayersLimited = poll.playrsList.Take(poll.maxPlayersCount).ToList();

                    // Получаем список игроков с их рейтингами
                    var playersWithRatings = votedPlayersLimited
                        .Join(this.players, p => p.id, pr => pr.id, (p, pr) =>
                        new { Player = p, Rating = pr.group, NormalName = pr.normalName, pr.isFemale }).ToList();

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
                            case 0: teams.Team1.Add(topPlayers[i].player); break;
                            case 1: teams.Team2.Add(topPlayers[i].player); break;
                            case 2: teams.Team3.Add(topPlayers[i].player); break;
                            case 3: teams.Team4.Add(topPlayers[i].player); break;
                        }
                    }

                    var pList = playersWithRatings.Select(p =>
                        (new Player(p.Player.id, p.Player.name, p.Player.firstName, p.NormalName, p.isFemale), p.Rating)).ToList();

                    var allTeams = new List<List<Player>> { teams.Team1, teams.Team2, teams.Team3, teams.Team4 };
                    FixConflicts(allTeams, pList);
                    DistributeFemalesEvenly(allTeams, pList);

                    // Перемешивание игроков в каждой команде
                    teams.Team1 = teams.Team1.OrderBy(x => random.Next()).ToList();
                    teams.Team2 = teams.Team2.OrderBy(x => random.Next()).ToList();
                    teams.Team3 = teams.Team3.OrderBy(x => random.Next()).ToList();
                    teams.Team4 = teams.Team4.OrderBy(x => random.Next()).ToList();
                }
            }

            return teams;
        }
        public void AddVote(string IdPoll, long idPlayer, string userName, string firstName, long idVoute)
        {
            var curPoll = state.pollList.FirstOrDefault(x => x.idPoll == IdPoll);
            if (curPoll != null)
            {
                var curPlayer = players.FirstOrDefault(x => x.id == idPlayer);
                if (curPlayer == null)
                {
                    curPlayer = new Player(idPlayer, firstName, userName, userName, false);
                    players.Add(curPlayer);
                }

                if (curPlayer != null)
                {
                    curPoll.AddPlayerToList(idPlayer, userName, firstName, idVoute, curPlayer.rating);

                    if (curPlayer.name != userName)
                    {
                        curPlayer.name = userName;
                    }
                    if (curPlayer.firstName != firstName)
                    {
                        curPlayer.firstName = firstName;
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
            try
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
                    if (poll.ratingMessageId > 0)
                    {
                        botClient.DeleteMessage(Properties.Settings.Default.chatId, poll.ratingMessageId);
                    }

                }

                SaveState();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, ex);
            }

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

            // Сначала пробуем стандартное имя файла
            var fileName = $"Arch{pollDate}.json";
            var filePath = Path.Combine(archiveFolderPath, fileName);

            // Если файл уже существует (вторая игра в тот же день), добавляем idPoll
            if (File.Exists(filePath))
            {
                fileName = $"Arch{pollDate}_{poll.idPoll}.json";
                filePath = Path.Combine(archiveFolderPath, fileName);
            }

            File.WriteAllText(filePath, json);

        }

        internal List<Poll> getTodayApprovedGamePoll()
        {
            var now = DateTime.Now.ToString("dd.MM");
            // Возвращаем все опросы, которые соответствуют сегодняшней дате и одобрены
            var polls = state.pollList.Where(x => x.date == now && x.approved).ToList();
            return polls;
        }

        internal Poll? GetClosestApprovedPollForToday(long? playerId = null)
        {
            var now = DateTime.Now;

            // Получаем все опросы, которые соответствуют сегодняшней дате и одобрены
            var todayPolls = state.pollList
                .Where(x => x.date == now.ToString("dd.MM") && x.approved);

            // Фильтруем по участию игрока, если playerId указан
            if (playerId.HasValue && playerId.Value > 0)
            {
                todayPolls = todayPolls.Where(p =>
                    p.playrsList.Take(p.maxPlayersCount)
                        .Any(player => player.id == playerId.Value));
            }

            // Сортируем по времени начала игры
            var sortedPolls = todayPolls
                .OrderBy(p => new DateTime(now.Year, now.Month, now.Day, p.curGame.GameStartHour, p.curGame.GameStartMinute, 0))
                .ToList();

            if (!sortedPolls.Any())
                return null; // Если опросов нет, возвращаем null

            // Проверяем, есть ли игры, которые уже начались
            // TODO: нужно не отнимать один час, это должна быть настройка, для разных часовых поясов
            var startedPolls = sortedPolls.Where(p =>
                  new DateTime(now.Year, now.Month, now.Day, p.curGame.GameStartHour - 1, p.curGame.GameStartMinute, 0) <= now).ToList();

            if (startedPolls.Any())
            {
                // Если есть начавшиеся игры, возвращаем последнюю из них
                return startedPolls.Last();
            }

            // Если ни одна игра ещё не началась, возвращаем самую раннюю из всех
            return sortedPolls.First();
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
