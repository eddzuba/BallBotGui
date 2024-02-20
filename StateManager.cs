﻿using Newtonsoft.Json;
using System;
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
                                players.Add(new Player(player.id, player.name, player.firstName));
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
            bool playerExistsInTopPlayers = poll.playrsList.Take(14).Any(p => p.id == update?.Message?.From?.Id || update.Message.From.Id == 245566701);

            Random random = new();
            Teams teams = new();   
            if (poll != null && poll.playrsList.Count >= 12 && playerExistsInTopPlayers)
            {
                // Ограничение списка проголосовавших игроков до максимум 14
                var votedPlayersLimited = poll.playrsList.Take(14).ToList();

                // Получаем список игроков, которые проголосовали и переводим в список игроков
                // Получение списка игроков с их рейтингами
                var playersWithRatings = votedPlayersLimited
                    .Join(this.players, p => p.id, pr => pr.id, (p, pr) => new { Player = p, Rating = pr.group });

                // Группировка игроков по уровню рейтинга
                var groupedPlayersWithRatings = playersWithRatings.GroupBy(p => p.Rating);

                // Создание словаря для хранения игроков каждой группы
                Dictionary<int, List<(Player player, int Rating)>> groupedPlayersDictionary = new();
                foreach (var group in groupedPlayersWithRatings.OrderBy(g => g.Key))
                {
                    // Случайное перемешивание порядка игроков в группе и конвертация в List<(Player, int)>
                    var shuffledGroup = group.OrderBy(x => random.Next())
                                        .Select(x => (new Player(x.Player.id, x.Player.name, x.Player.firstName), x.Rating))
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

                // Перемешивание игроков в команде 1
                teams.Team1 = teams.Team1.OrderBy(x => random.Next()).ToList();

                // Перемешивание игроков в команде 2
                teams.Team2 = teams.Team2.OrderBy(x => random.Next()).ToList();




            }
            return teams;
        }

        public void AddVote(string IdPoll, long idPlayer, string name, string lastName, long idVoute)
        {
            var curPoll = state.pollList.FirstOrDefault(x => x.idPoll == IdPoll);
            if(curPoll != null)
            {
                curPoll.AddPlayerToList(idPlayer, name, lastName, idVoute);
            }
        }

        internal void RemoteVote(string idPoll, long idPlayer)
        {
            var curPoll = state.pollList.FirstOrDefault(x => x.idPoll == idPoll);
            if (curPoll != null)
            {
                curPoll.DeletePlayerFromList(idPlayer);
            }
        }

        internal void ArchPolls(Telegram.Bot.TelegramBotClient botClient)
        {
            // Архивировать опросы с датой раньше текущей даты
            foreach (var poll in state.pollList.Where(p => p.GetGameDate() < DateTime.Now.AddDays(-1)).ToList())
            {
                ArchivePoll(poll);
                state.pollList.Remove(poll);
                if(poll.idMessage > 0)
                {
                    botClient.UnpinChatMessageAsync(Properties.Settings.Default.curChatId, poll.idMessage);
                }
                
            }

            SaveState();
            
        }

        private void ArchivePoll(Poll poll)
        {
            var a = poll;
            var polDate = poll.GetGameDate().ToString("ddMMyyyy");

            string json = JsonConvert.SerializeObject(poll);
            var fileName = "Arch" + polDate + ".json";
            File.WriteAllText(fileName, json);

        }

        internal Poll? getTodayApprovedGamePoll()
        {
            var now = DateTime.Now.ToString("dd.MM");

            Poll ?poll = state.pollList.FirstOrDefault(x => x.date == now && x.approved);

            return poll;
        }
    }
}