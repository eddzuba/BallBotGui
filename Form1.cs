using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Telegram.Bot;


namespace BallBotGui
{

    public partial class Form1 : Form
    {
        private TelegramBotClient botClient;
        private TelegramConnector telConnector;
        private string? botKey;
        private StateManager stateManager = new();

        readonly BindingSource bsPoll = new(); // Poll
        readonly BindingSource bsPlayer = new(); // Player
        readonly BindingSource bsRating = new(); // Player

        public Form1() => InitializeComponent();

        public void refreshGrids()
        {
            if (this.dataGridViewPoll.InvokeRequired)
            {
                this.dataGridViewPoll.Invoke(new Action(() =>
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

            initDs();
        }
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Проверяем, что текущая ячейка принадлежит к столбцу с индексом 0 (первый столбец) и номер строки меньше 10
            if (e.RowIndex < 14)
            {
                // Устанавливаем цвет фона для текущей ячейки
                e.CellStyle.BackColor = Color.LightGreen;
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

            bsRating.DataSource = stateManager.players;
            dataGridViewRating.DataSource = bsRating;
            dataGridViewRating.AutoGenerateColumns = true;


        }

        private async void minuteTimer_Tick(object sender, EventArgs e)
        {
            DateTime curTime = DateTime.Now;

            int pollHour = Properties.Settings.Default.pollHour;

            int[] days = getPollDays();
            if (Array.Exists(days, el => el == ((int)curTime.DayOfWeek))
                && curTime.Hour == pollHour && curTime.Minute == 00)
            {
                createNewPoll();
            }

            if (curTime.Hour == 23 && curTime.Minute == 30)
            {
                telConnector.ArchPolls();

            }

            if (curTime.Hour == 11 && curTime.Minute == 00)
            {
                sendInvitation();
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

        private int[] getPollDays()
        {
            // get day number when we need to grate a poll
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
            createNewPoll();
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
            telConnector.ArchPolls();
            bsPoll.ResetBindings(false);
            bsPlayer.ResetBindings(false);
        }

        private void clickSendInvitation(object sender, EventArgs e)
        {
            sendInvitation();
        }

        private void sendInvitation()
        {
            Poll? todayApprovedGamePoll = stateManager.getTodayApprovedGamePoll();
            if (todayApprovedGamePoll != null)
            {
                telConnector.sendInvitation(todayApprovedGamePoll);
            }
        }

        private async void createNewPoll()
        {

            DateTime curTime = DateTime.Now;
            int pollBeforeGame = Properties.Settings.Default.pollBeforeGame;

            await telConnector.createOnePoll(curTime.AddDays(pollBeforeGame));
            bsPoll.ResetBindings(false);
            bsPlayer.ResetBindings(false);
        }

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
    }
}