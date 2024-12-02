using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BallBotGui
{
    internal class StatisticsManager
    {
        internal string getPlayerStat(Update update)
        {
            string message = "";
            var (currentMonthFiles, previousMonthFiles) = GetArchivesForCurrentAndPreviousMonth();
            var currentMonthPolls = ReadPollsFromFiles(currentMonthFiles);
            var previousMonthPolls = ReadPollsFromFiles(previousMonthFiles);

            var (curTotalGames, curEncountersBelow15th, curEncountersAbove15th) = this.GetPlayerEncountersInfo(currentMonthPolls, update.Message.From.Id);
            var (previousTotalGames, previousEncountersBelow15th, previousEncountersAbove15th) = this.GetPlayerEncountersInfo(previousMonthPolls, update.Message.From.Id);

            message += $"Статистика по игроку @{update.Message.From.Username} {update.Message.From.FirstName}\n\n";
            message += "ПРОШЛЫЙ МЕСЯЦ\n\n";
            message += CreateTelegramMessage(previousTotalGames, previousEncountersBelow15th, previousEncountersAbove15th);
            message += "\n\nТЕКУЩИЙ МЕСЯЦ\n\n";
            message += CreateTelegramMessage(curTotalGames, curEncountersBelow15th, curEncountersAbove15th);



            return message;
        }
        string CreateTelegramMessage(int X, int Y, int Z)
        {
            int kcalPerHour = 350;
            string volleyballEmoji = "\U0001F3D0"; // Unicode для волейбольного мяча

            int totalKcalBurned = Y * 2 * kcalPerHour;
            double fatNotAccumulated = Math.Round(totalKcalBurned / 7.7); // 1 г жира = 7.7 ккалF

            string message = $"{volleyballEmoji} Всего игр: {X}\n" +
                             $"Играли: {Y} \n" +
                             $"Не получилось: {Z} \n" +
                             $"Сожгли {totalKcalBurned} ккал\n" +
                             $"Сожгли {fatNotAccumulated} гр. жира";

            return message;
        }
        private (List<string> currentMonthFiles, List<string> previousMonthFiles) GetArchivesForCurrentAndPreviousMonth()
        {
            var currentMonth = DateTime.Today.Month;
            var previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var currentYear = DateTime.Today.Year;

            var currentMonthFiles = new List<string>();
            var previousMonthFiles = new List<string>();

            var archiveFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Arch");

            if (Directory.Exists(archiveFolderPath))
            {
                var allFiles = Directory.GetFiles(archiveFolderPath);

                foreach (var file in allFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var fileMonth = int.Parse(fileName.Substring(6, 2));
                    var fileYear = int.Parse(fileName.Substring(8, 4));

                    if (fileYear == currentYear && fileMonth == currentMonth)
                    {
                        currentMonthFiles.Add(file);
                    }
                    else if (fileYear == (currentMonth == 1 ? currentYear - 1 : currentYear) && fileMonth == previousMonth)
                    {
                        previousMonthFiles.Add(file);
                    }
                }
            }

            return (currentMonthFiles, previousMonthFiles);
        }

        private List<Poll> ReadPollsFromFiles(List<string> files)
        {
            var polls = new List<Poll>();

            foreach (var file in files)
            {
                var json = System.IO.File.ReadAllText(file);
                var poll = JsonConvert.DeserializeObject<Poll>(json);
                polls.Add(poll);
            }

            return polls;
        }

        private (int totalGames, int encountersBelow15th, int encountersAbove15th) GetPlayerEncountersInfo(List<Poll> polls, long playerId)
        {
            int totalGames = 0;
            int encountersBelow15th = 0;
            int encountersAbove15th = 0;

            foreach (var poll in polls)
            {
                if (!poll.approved)
                {
                    continue; // Пропускаем неодобренные опросы
                }
                
                totalGames++;
                var player = poll.playrsList.FirstOrDefault(p => p.id == playerId);
                if (player != null)
                {
                    
                    var playerIndex = poll.playrsList.IndexOf(player);
                    if (playerIndex < 15)
                    {
                        encountersBelow15th++;
                    }
                    else
                    {
                        encountersAbove15th++;
                    }
                }
            }

            return (totalGames, encountersBelow15th, encountersAbove15th);
        }

    }
}
