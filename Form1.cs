using Newtonsoft.Json;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Telegram.Bot;
using static System.Net.Mime.MediaTypeNames;

namespace BallBotGui
{

    public partial class Form1 : Form
    {
        private TelegramBotClient? botClient;
        private TelegramConnector? telConnector;
        private string? botKey;
        private StateManager stateManager = new();

        readonly BindingSource bsPoll = new(); // Poll
        readonly BindingSource bsPlayer = new(); // Player
        readonly BindingSource bsRating = new(); // Player

        private BindingSource bsCars = new();
        readonly BindingSource bsCarStops = new(); // CarStops
        private GameManager? gameManager;



        public Form1()
        {
            InitializeComponent();
        }

        public void refreshGrids()
        {
            if (dataGridViewPoll.InvokeRequired)
            {
                dataGridViewPoll.Invoke(new Action(() =>
                {
                    bsPoll.ResetBindings(false);
                    bsPlayer.ResetBindings(false);
                }));
            }
            else
            {
                bsPoll.ResetBindings(false);
                bsPlayer.ResetBindings(false);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Подписываемся на событие CellFormatting
            dataGridViewPlayers.CellFormatting += DataGridView1_CellFormatting;

            botKey = Properties.Settings.Default.curBotKey;
            botClient = new TelegramBotClient(botKey);
            telConnector = new TelegramConnector(botClient, stateManager, this);
            gameManager = new GameManager(telConnector);

            initDs();
        }
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Проверяем, что текущая ячейка принадлежит к столбцу с индексом 0 (первый столбец) и номер строки меньше 10
            if (e.RowIndex < 14)
            {
                // Устанавливаем цвет фона для текущей ячейки
                e.CellStyle.BackColor = System.Drawing.Color.LightGreen;
            }
        }

        private void initDs()
        {
            // Set up data binding for the parent Poll
            bsPoll.DataSource = stateManager.state.pollList;
            dataGridViewPoll.DataSource = bsPoll;
            dataGridViewPoll.AutoGenerateColumns = true;

            bsPlayer.DataSource = bsPoll; // chaining bsP to bsA
            bsPlayer.DataMember = "playrsList";
            dataGridViewPlayers.DataSource = bsPlayer;
            dataGridViewPlayers.AutoGenerateColumns = true;

            bsRating.DataSource = new BindingList<Player>(stateManager.players);
            dataGridViewRating.DataSource = bsRating;
            dataGridViewRating.AutoGenerateColumns = true;


            bsCars.DataSource = stateManager.state.carList;
            dgvCars.DataSource = bsCars;
            dgvCars.AutoGenerateColumns = true;



            /*bsCarStops.DataSource = bsCars;
            bsCarStops.DataMember = "carStops";

            dataGridViewCarStops.DataSource = bsCarStops;
            dataGridViewCarStops.AutoGenerateColumns = true;
            // Разрешаем редактирование данных в таблице
            dataGridViewCarStops.ReadOnly = false;
*/






        }

        private async void MinuteTimer_Tick(object sender, EventArgs e)
        {
            DateTime curTime = DateTime.Now;

            if (gameManager != null)
            {
                var pullCreated = await gameManager.CheckScheduleAndCreatePollAsync(curTime);
                if (pullCreated)
                {
                    bsPoll.ResetBindings(false);
                    bsPlayer.ResetBindings(false);
                }
            }

            if (curTime.Hour == 23 && curTime.Minute == 30)
            {
                telConnector.ArchPolls();

            }

            if (curTime.Hour == 11 && curTime.Minute == 00)
            {
                await sendInvitationAsync();
                /*  await sendCarsInfo(); */
                await askNewPlayesrsAsync();
            }


            var polls = stateManager.getTodayApprovedGamePoll();
            if (polls != null)
            {
                foreach (var poll in polls)
                {
                    if (telConnector != null && poll.isTimeToSendBeforeGameInvite(curTime))
                    {
                        await telConnector.sendBeforeGameInvite(poll);
                        break;
                    }
                }
            }



            if ((curTime.Hour == 10 && curTime.Minute == 00))
            {
                foreach (var curPoll in this.stateManager.state.pollList)
                {
                    stateManager.AddPlayersToRating(curPoll);
                }

                bsRating.ResetBindings(false);
            }

        }

        private async Task askNewPlayesrsAsync()
        {
            var todayApprovedGamePolls = stateManager.getTodayApprovedGamePoll();
            if (todayApprovedGamePolls != null && todayApprovedGamePolls.Any())
            {
                var newPlayes = telConnector.askNewPlayers(todayApprovedGamePolls);
                if (newPlayes != null)
                {
                    foreach (var player in newPlayes)
                    {
                        await telConnector.askAboutFirstGameAsync(player);
                    }
                }
            }
        }

        private int[] getPollDays()
        {
            // get day number when we need to create a poll
            var stringArray = Properties.Settings.Default.pollDayList.Split(',');
            // Convert string array to integer array
            int[] intArray = new int[stringArray.Length];
            for (int i = 0; i < stringArray.Length; i++)
            {
                // Use int.TryParse if strings may contain non-integer values
                if (int.TryParse(stringArray[i], out int result))
                {
                    intArray[i] = result;
                }
                else
                {
                    Console.WriteLine($"Unable to parse the string {stringArray[i]} to an integer.");
                }
            }

            return intArray;
        }

        private async void btnCreatePoll_press(object sender, EventArgs e)
        {
            //  createNewPoll();
            DateTime now = DateTime.Now; // Текущее время
            int targetDay = 0; // День недели (1 = понедельник, 2 = вторник, ..., 7 = воскресенье). Например, 4 = четверг.

            int currentDay = (int)now.DayOfWeek == 0 ? 7 : (int)now.DayOfWeek; // Преобразуем DayOfWeek (0 = воскресенье) в систему, где 1 = понедельник
            int daysUntilTarget = (targetDay - currentDay + 7) % 7; // Количество дней до цели

            if (daysUntilTarget == 0 && now.TimeOfDay > new TimeSpan(23, 0, 0))
            {
                // Если сегодня целевой день, но время уже больше 23:00, берём следующий такой день
                daysUntilTarget = 7;
            }

            DateTime nextTargetDayAt23 = now.Date.AddDays(daysUntilTarget).AddHours(22).AddMinutes(00);
            if (gameManager != null)
            {
                var pullCreated = await gameManager.CheckScheduleAndCreatePollAsync(nextTargetDayAt23);
            }
        }

        private void testSave(object sender, EventArgs e)
        {
            stateManager.SaveState();
        }

        private void RestoreState(object sender, EventArgs e)
        {
            stateManager.LoadState();

        }

        private async void ReadUpdates(object sender, EventArgs e)
        {
            await telConnector.ReadMessages(0);
        }

        private void onPlayerSelect(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            int selectedIndex = dataGridViewPlayers.SelectedRows[0].Index;
            var list = stateManager.state.pollList[dataGridViewPoll.SelectedRows[0].Index].playrsList;
            movePlayerUp(list, selectedIndex);

            if (selectedIndex > 0)
            {
                bsPoll.ResetBindings(false);
                bsPlayer.ResetBindings(false);
                dataGridViewPlayers.Rows[selectedIndex].Selected = false;
                dataGridViewPlayers.Rows[selectedIndex - 1].Selected = true;
                stateManager.SaveState();
            }


        }

        void movePlayerDown(List<PlayerVote> players, int index)
        {
            if (index < 0 || index >= players.Count - 1)
            {
                Console.WriteLine("Игрок уже находится на нижней позиции.");
                return;
            }

            PlayerVote temp = players[index + 1];
            players[index + 1] = players[index];
            players[index] = temp;


        }

        // Поднять игрока в списке
        void movePlayerUp(List<PlayerVote> players, int index)
        {
            if (index <= 0 || index >= players.Count)
            {
                Console.WriteLine("Игрок уже находится на верхней позиции.");
                return;
            }

            PlayerVote temp = players[index - 1];
            players[index - 1] = players[index];
            players[index] = temp;

        }

        private void button5_Click(object sender, EventArgs e) // DOWN
        {
            int selectedIndex = dataGridViewPlayers.SelectedRows[0].Index;
            var list = stateManager.state.pollList[dataGridViewPoll.SelectedRows[0].Index].playrsList;
            movePlayerDown(list, selectedIndex);

            if (selectedIndex < list.Count - 1)
            {

                bsPoll.ResetBindings(false);
                bsPlayer.ResetBindings(false);
                dataGridViewPlayers.Rows[selectedIndex].Selected = false;
                dataGridViewPlayers.Rows[selectedIndex + 1].Selected = true;
                stateManager.SaveState();
            }

        }

        private void ArchPools(object sender, EventArgs e)
        {
            telConnector?.ArchPolls();
            bsPoll.ResetBindings(false);
            bsPlayer.ResetBindings(false);
        }

        private void clickSendInvitation(object sender, EventArgs e)
        {
            // sendInvitation();
            // sendCarsInfo();
        }

        private async Task sendInvitationAsync()
        {
            var todayApprovedGamePolls = stateManager.getTodayApprovedGamePoll();
            if (todayApprovedGamePolls != null && todayApprovedGamePolls.Any())
            {
                foreach (var poll in todayApprovedGamePolls)
                {
                    await telConnector?.sendInvitation(poll);
                    await telConnector.sendCarsMessage(poll);
                }
            }
        }

        /*private async Task sendCarsInfo()
        {
            var todayApprovedGamePolls = stateManager.getTodayApprovedGamePoll();
            if (todayApprovedGamePolls != null && todayApprovedGamePolls.Any())
            {
                foreach (var poll in todayApprovedGamePolls)
                {
                    await telConnector.sendCarsMessage(poll);
                }
            }
        }*/



       /* private async void createNewPoll() //
        {

            DateTime curTime = DateTime.Now;
            int pollBeforeGame = Properties.Settings.Default.pollBeforeGame;

            await telConnector.createOnePoll(curTime.AddDays(pollBeforeGame));
            bsPoll.ResetBindings(false);
            bsPlayer.ResetBindings(false);
        }*/

        private void AddPlayers(object sender, EventArgs e)
        {
            if (bsPoll.Current is Poll curPoll)
            {
                stateManager.AddPlayersToRating(curPoll);
            }

        }

        private void dataGridViewRating_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            stateManager.SavePlayers();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            // stateManager.Take2Teams();

        }

        private void Form1_Activated(object sender, EventArgs e)
        {

        }

        /*  private void addCar_Click(object sender, EventArgs e)
          {

              var newCar = new Car(1, "ed", "edGzrd", 4);
              var stop1 = new CarStops("Парковка Gardens 19.35", "");
              var stop2 = new CarStops("Метро Gardens 19.40", "");

              newCar.carStops.Add(stop1);
              newCar.carStops.Add(stop2);

              this.stateManager.state.carList.Add(newCar);

          }*/

        private void getCars_Click(object sender, EventArgs e)
        {
            this.telConnector.startScoreBoard();
        }

        private void dgvCars_SelectionChanged(object sender, EventArgs e)
        {
            Console.WriteLine("changed");
            if (dgvCars.CurrentRow != null)
            {
                var selectedCar = dgvCars.CurrentRow.DataBoundItem as Car;
                if (selectedCar != null)
                {

                    /*   bsCars.DataSource = stateManager.state.carList;
                       dgvCars.DataSource = bsCars;
                       dgvCars.AutoGenerateColumns = true;

   */

                    bsCarStops.DataSource = selectedCar.carStops;
                    bsCarStops.ResetBindings(false);
                    dataGridViewCarStops.DataSource = bsCarStops;
                    dataGridViewCarStops.AutoGenerateColumns = true;
                    dataGridViewCarStops.ReadOnly = false;

                    /*bsCarStops.DataSource = bsCars;
                    bsCarStops.DataMember = "carStops";

                    dataGridViewCarStops.DataSource = bsCarStops;
                    dataGridViewCarStops.AutoGenerateColumns = true;
                    // Разрешаем редактирование данных в таблице
                    dataGridViewCarStops.ReadOnly = false;
        */
                    // Выбранный объект доступен в selectedCar
                    // Можно выполнить нужные действия с выбранным объектом
                }
            }
        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            this.telConnector.sendTestMessageAsync();
        }

        private void getStat(object sender, EventArgs e)
        {
            string archiveFolderName = "Arch";
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), archiveFolderName);
            HashSet<long> uniquePlayerIds = new HashSet<long>();

            foreach (string filePath in Directory.GetFiles(directoryPath, "Arch*.json"))
            {
                try
                {
                    string fileContent = File.ReadAllText(filePath);
                    var gameData = JsonConvert.DeserializeObject<Poll>(fileContent);

                    if (gameData?.playrsList != null)
                    {
                        var firstmaxPlayersCountPlayers = gameData.playrsList.Take(gameData.maxPlayersCount);
                        foreach (var player in firstmaxPlayersCountPlayers)
                        {

                            uniquePlayerIds.Add(player.id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
                }
            }

            var distinctCount = uniquePlayerIds.Count();
            var d = 0;
        }

        private void button11_ClickAsync(object sender, EventArgs e)
        {
            sendInvitationAsync();
            // sendCarsInfo();
            // askNewPlayesrsAsync();
        }

        private void filter_TextChanged(object sender, EventArgs e)
        {
            string filterText = (sender as TextBox)?.Text?.Trim().ToLower() ?? "";

            CurrencyManager cm = (CurrencyManager)BindingContext[dataGridViewRating.DataSource];
            cm.SuspendBinding();

            // Предварительно убираем текущую ячейку и выделение
            dataGridViewRating.ClearSelection();
            dataGridViewRating.CurrentCell = null;

            bool anyVisible = false;

            foreach (DataGridViewRow row in dataGridViewRating.Rows)
            {
                if (row.DataBoundItem is Player player)
                {
                    bool visible = string.IsNullOrEmpty(filterText) ||
                                   (player.name != null && player.name.ToLower().Contains(filterText))
                                   || player.id.ToString().Contains(filterText)
                    || (player.firstName != null && player.firstName.ToLower().Contains(filterText));
                    row.Visible = visible;

                    if (visible) anyVisible = true;
                }
            }

            cm.ResumeBinding();

            // Если остались видимые строки — выделим первую
            if (anyVisible)
            {
                foreach (DataGridViewRow row in dataGridViewRating.Rows)
                {
                    if (row.Visible)
                    {
                        row.Selected = true;
                        dataGridViewRating.CurrentCell = row.Cells[0];
                        break;
                    }
                }
            }
        }

    }
}