using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Types;
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
        private ScoreListener? _scoreListener;

        
        readonly BindingSource bsGames = new(); // Games
        private BindingSource bsGyms = new BindingSource();


        public Form1()
        {
            InitializeComponent();
        }

        public void SafeInvoke(Action action)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        public void refreshGrids()
        {
            SafeInvoke(() =>
            {
                bsPoll.ResetBindings(false);
                bsPlayer.ResetBindings(false);
                bsRating.ResetBindings(false);
                bsCars.ResetBindings(false);
            });
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            stateManager.LoadState();
            stateManager.LoadPlayers();

            // Подписываемся на событие CellFormatting
            dataGridViewPlayers.CellFormatting += DataGridView1_CellFormatting;

            botKey = Properties.Settings.Default.curBotKey;
            botClient = new TelegramBotClient(botKey);
            telConnector = new TelegramConnector(botClient, stateManager, this);
            gameManager = new GameManager(telConnector);

            initDs();
            
            // Handle DataError to prevent the default error dialog
            dataGridViewPoll.DataError += dataGridView_DataError;
            dataGridViewPlayers.DataError += dataGridView_DataError;
            dataGridViewRating.DataError += dataGridView_DataError;
            dgvCars.DataError += dataGridView_DataError;
            dgvGames.DataError += dataGridView_DataError;
            dgvGames.CellFormatting += dgvGames_CellFormatting;
            
            // Первоначальный расчет размеров
            Form1_Resize(this, EventArgs.Empty);

            // Запускаем HTTP-слушатель счёта
            var portStr = AppConfigHelper.LoadSetting("ScoreListenerPort", "5050");
            if (int.TryParse(portStr, out int scorePort))
            {
                _scoreListener = new ScoreListener(stateManager, scorePort);
                _scoreListener.OnScoreUpdated = () => SafeInvoke(async () =>
                {
                    RefreshScoreTab();
                    if (telConnector != null)
                    {
                        var msg = await telConnector.UpdateLastTeamsMessageWithScore();
                        // Убираем HTML теги для отображения в обычном TextBox
                        txtLastTelegramMessage.Text = msg.Replace("<b>", "").Replace("</b>", "").Replace("<i>", "").Replace("</i>", "");
                    }
                });
                _scoreListener.OnRawMessageReceived = (json) => SafeInvoke(() => {
                    string display = json.Length > 50 ? json.Substring(0, 47) + "..." : json;
                    lblLastRawScore.Text = $"Last Received: {display}";
                });
                _scoreListener.Start();
            }

            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _scoreListener?.Stop();
        }

        private void btnRefreshScore_Click(object? sender, EventArgs e)
        {
            RefreshScoreTab();
        }

        private void RefreshScoreTab()
        {
            pnlScoreCards.Controls.Clear();

            var todayPolls = stateManager.state.pollList
                .Where(p => p.date == DateTime.Now.ToString("dd.MM") && p.approved)
                .OrderBy(p => p.curGame?.GameStartHour)
                .ToList();

            if (!todayPolls.Any())
            {
                // Если игр нет, но есть автономный счет - показываем его
                if (stateManager.state.StandaloneScores.Any())
                {
                    // Создаем фиктивный объект Poll для отображения
                    var dummyPoll = new Poll("standalone", DateTime.Now.ToString("dd.MM"), "Автономный счёт (без опроса)", 0, null, 0);
                    dummyPoll.GameScores = stateManager.state.StandaloneScores.ToList();
                    pnlScoreCards.Controls.Add(BuildScoreCard(dummyPoll));
                    return;
                }

                var lbl = new Label
                {
                    Text = "Сегодня нет активных игр.",
                    AutoSize = true,
                    Font = new System.Drawing.Font("Segoe UI", 14f),
                    ForeColor = System.Drawing.Color.Gray,
                    Margin = new Padding(10)
                };
                pnlScoreCards.Controls.Add(lbl);
                return;
            }

            foreach (var poll in todayPolls)
            {
                pnlScoreCards.Controls.Add(BuildScoreCard(poll));
            }
        }

        private Panel BuildScoreCard(Poll poll)
        {
            var card = new Panel
            {
                Width = pnlScoreCards.Width - 30,
                AutoSize = true,
                Margin = new Padding(4, 4, 4, 10),
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = System.Drawing.Color.FromArgb(245, 245, 255)
            };

            int y = 10;

            // Заголовок — время игры и название
            var gameTitle = poll.question ?? poll.date;
            var startTime = poll.curGame != null
                ? $"{poll.curGame.GameStartHour:D2}:{poll.curGame.GameStartMinute:D2}"
                : "--:--";

            var lblTitle = new Label
            {
                Text = $"⚽ {startTime}  |  {gameTitle}",
                Font = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, y),
                ForeColor = System.Drawing.Color.FromArgb(40, 40, 120)
            };
            card.Controls.Add(lblTitle);
            y += 40;

            var lastScore = poll.GameScores.LastOrDefault();

            // Текущий счёт
            if (lastScore != null)
            {
                var isFinished = lastScore.IsFinished;
                var statusText = isFinished ? "✅ Игра завершена" : "🟢 Игра идёт";
                var statusColor = isFinished
                    ? System.Drawing.Color.FromArgb(0, 120, 0)
                    : System.Drawing.Color.FromArgb(0, 90, 180);

                var lblStatus = new Label
                {
                    Text = statusText,
                    Font = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Italic),
                    AutoSize = true,
                    Location = new Point(10, y),
                    ForeColor = statusColor
                };
                card.Controls.Add(lblStatus);
                y += 30;

                // Делаем снимок для потокобезопасности
                var scoresSnapshot = poll.GameScores.ToList();

                // Показываем результаты завершенных партий
                var finishedResults = new List<string>();
                bool prevWasFinished = false;
                foreach (var gs in scoresSnapshot)
                {
                    if (gs.IsFinished)
                    {
                        string scoreLine = $"🟢 {gs.Team1Score} : 🟡 {gs.Team2Score}";
                        if (!prevWasFinished) finishedResults.Add(scoreLine);
                        else if (finishedResults.Count > 0) finishedResults[finishedResults.Count - 1] = scoreLine;
                        prevWasFinished = true;
                    }
                    else prevWasFinished = false;
                }

                if (finishedResults.Any())
                {
                    var lblSets = new Label
                    {
                        Text = "Партии: " + string.Join(", ", finishedResults),
                        Font = new System.Drawing.Font("Segoe UI", 13f, System.Drawing.FontStyle.Bold),
                        AutoSize = true,
                        Location = new Point(10, y),
                        ForeColor = System.Drawing.Color.FromArgb(80, 80, 80)
                    };
                    card.Controls.Add(lblSets);
                    y += 35;
                }

                var lblScore = new Label
                {
                    Text = $"🟢 {lastScore.Team1Score}  :  🟡 {lastScore.Team2Score}",
                    Font = new System.Drawing.Font("Segoe UI", 32f, System.Drawing.FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(10, y),
                    ForeColor = System.Drawing.Color.FromArgb(30, 30, 100)
                };
                card.Controls.Add(lblScore);
                y += 70;

                var lblTime = new Label
                {
                    Text = $"Обновлено: {lastScore.Timestamp:HH:mm:ss}",
                    Font = new System.Drawing.Font("Segoe UI", 9f),
                    AutoSize = true,
                    Location = new Point(10, y),
                    ForeColor = System.Drawing.Color.Gray
                };
                card.Controls.Add(lblTime);
                y += 30;

                // История изменений счета (последние 10)
                if (scoresSnapshot.Count > 1)
                {
                    var lblHistHeader = new Label
                    {
                        Text = "История:",
                        Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
                        AutoSize = true,
                        Location = new Point(10, y),
                        ForeColor = System.Drawing.Color.DimGray
                    };
                    card.Controls.Add(lblHistHeader);
                    y += 25;

                    var histEntries = scoresSnapshot
                        .Take(scoresSnapshot.Count - 1)
                        .TakeLast(10)
                        .Reverse()
                        .ToList();

                    foreach (var hs in histEntries)
                    {
                        var lblHist = new Label
                        {
                            Text = $"  {hs.Timestamp:HH:mm:ss}  -  🟢 {hs.Team1Score} : 🟡 {hs.Team2Score}",
                            Font = new System.Drawing.Font("Segoe UI", 9f),
                            AutoSize = true,
                            Location = new Point(10, y),
                            ForeColor = System.Drawing.Color.Gray
                        };
                        card.Controls.Add(lblHist);
                        y += 22;
                    }
                }
            }
            else
            {
                var lblNoScore = new Label
                {
                    Text = "Счёт ещё не получен",
                    Font = new System.Drawing.Font("Segoe UI", 12f),
                    AutoSize = true,
                    Location = new Point(10, y),
                    ForeColor = System.Drawing.Color.FromArgb(160, 100, 0)
                };
                card.Controls.Add(lblNoScore);
                y += 40;
            }

            card.Height = y + 10;
            return card;
        }

        private void dataGridView_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
            // Log the error but don't show the dialog
            string gridName = (sender as Control)?.Name ?? "UnknownGrid";
            Logger.Log($"UI DataError in {gridName}: {e.Exception?.Message}");
            e.ThrowException = false;
        }
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Проверяем, что текущая ячейка принадлежит к столбцу с индексом 0 (первый столбец) и номер строки меньше 14
            if (e.RowIndex >= 0 && e.RowIndex < 14)
            {
                // Устанавливаем цвет фона для первых 14 игроков
                e.CellStyle.BackColor = System.Drawing.Color.LightGreen;
            }

            // Индикация наличия машины
            if (e.RowIndex >= 0 && dataGridViewPlayers.Columns[e.ColumnIndex].Name == "CarIcon")
            {
                if (dataGridViewPlayers.Rows[e.RowIndex].DataBoundItem is PlayerVote pv)
                {
                    bool hasCar = stateManager.state.carList.Any(c => c.idPlayer == pv.id);
                    e.Value = hasCar ? "🚗" : "";
                }
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
            bsPlayer.CurrentChanged += BsPlayer_CurrentChanged;

            // Добавляем колонку-индикатор машины, если её еще нет
            if (dataGridViewPlayers.Columns["CarIcon"] == null)
            {
                var col = new DataGridViewTextBoxColumn();
                col.Name = "CarIcon";
                col.HeaderText = "🚗";
                col.Width = 40;
                col.ReadOnly = true;
                col.DisplayIndex = 0;
                dataGridViewPlayers.Columns.Insert(0, col);
            }

            bsRating.DataSource = new BindingList<Player>(stateManager.players);
            dataGridViewRating.DataSource = bsRating;
            dataGridViewRating.AutoGenerateColumns = true;

            // Bindings for editing controls
            txtEditId.DataBindings.Add("Text", bsRating, "id", true, DataSourceUpdateMode.OnPropertyChanged);
            txtEditName.DataBindings.Add("Text", bsRating, "name", true, DataSourceUpdateMode.OnPropertyChanged);
            txtEditFirstName.DataBindings.Add("Text", bsRating, "firstName", true, DataSourceUpdateMode.OnPropertyChanged);
            txtEditNormalName.DataBindings.Add("Text", bsRating, "normalName", true, DataSourceUpdateMode.OnPropertyChanged);
            numEditRating.DataBindings.Add("Value", bsRating, "rating", true, DataSourceUpdateMode.OnPropertyChanged);
            numEditGroup.DataBindings.Add("Value", bsRating, "group", true, DataSourceUpdateMode.OnPropertyChanged);
            chkEditIsFemale.DataBindings.Add("Checked", bsRating, "isFemale", true, DataSourceUpdateMode.OnPropertyChanged);
            chkEditLevelChecked.DataBindings.Add("Checked", bsRating, "IsLevelChecked", true, DataSourceUpdateMode.OnPropertyChanged);

            // Save player data on any change in the binding list
            bsRating.ListChanged += (s, e) => {
                if (e.ListChangedType == ListChangedType.ItemChanged)
                {
                    stateManager.SavePlayers();
                }
            };

            bsCars.DataSource = stateManager.state.carList;
            dgvCars.DataSource = bsCars;
            dgvCars.AutoGenerateColumns = true;

            // Настройка комбобокса мест
            cmbCarPlaceCount.Items.Clear();
            for (int i = 1; i <= 5; i++) cmbCarPlaceCount.Items.Add(i);

            // Настройка биндинга для выбора игрока в машине
            RefreshCarPlayerPicker();

            // Биндинги для редактирования машин
            txtCarName.DataBindings.Add("Text", bsCars, "name", true, DataSourceUpdateMode.Never);
            txtCarFirstName.DataBindings.Add("Text", bsCars, "firstName", true, DataSourceUpdateMode.Never);
            cmbCarPlaceCount.DataBindings.Add("SelectedItem", bsCars, "placeCount", true, DataSourceUpdateMode.OnPropertyChanged);

            bsCars.CurrentChanged += BsCars_CurrentChanged;
            
            // Первичная подтяжка имен для всех машин
            UpdateCarNamesFromPlayers();

            // Инициализируем схему для остановок, чтобы биндинги не падали при отсутствии данных
            bsCarStops.DataSource = typeof(CarStops);
            dataGridViewCarStops.DataSource = bsCarStops;
            dataGridViewCarStops.AutoGenerateColumns = true;

            // Биндинги для редактирования остановок
            txtStopName.DataBindings.Add("Text", bsCarStops, "name", true, DataSourceUpdateMode.OnPropertyChanged);
            txtStopLink.DataBindings.Add("Text", bsCarStops, "link", true, DataSourceUpdateMode.OnPropertyChanged);
            numStopMinBefore.DataBindings.Add("Value", bsCarStops, "minBefore", true, DataSourceUpdateMode.OnPropertyChanged);

            // Настройка биндинга для редактирования игр
            var gamesJson = AppConfigHelper.LoadSetting("GamesJson");
            var gameList = string.IsNullOrEmpty(gamesJson) ? new List<VolleybollGame>() : System.Text.Json.JsonSerializer.Deserialize<List<VolleybollGame>>(gamesJson) ?? new List<VolleybollGame>();
            bsGames.DataSource = new BindingList<VolleybollGame>(gameList);
            dgvGames.DataSource = bsGames;
            dgvGames.AutoGenerateColumns = true;
            dgvGames.ReadOnly = true;
            dgvGames.AllowUserToAddRows = false;
            dgvGames.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            
            // Ждем завершения генерации колонок и перемещаем GameDay на первое место
            void OnColumnsAdded(object? sender, EventArgs e)
            {
                // Список нужных колонок и их параметров
                var columnsToSet = new[] {
                    new { Name = "GameDay", Order = 0, Header = "День", Visible = true },
                    new { Name = "GymId", Order = 1, Header = "Зал", Visible = true },
                    new { Name = "GameStartHour", Order = 2, Header = "Время", Visible = true },
                    new { Name = "ActiveGame", Order = 3, Header = "Активна", Visible = true },
                    new { Name = "RatingGame", Order = 4, Header = "Рейтинговая", Visible = true },
                    new { Name = "TrainingGame", Order = 5, Header = "Тренировка", Visible = true }
                };

                // Сначала скрываем всё
                foreach (DataGridViewColumn col in dgvGames.Columns) {
                    col.Visible = false;
                }

                // Затем показываем и настраиваем нужные
                foreach (var info in columnsToSet) {
                    if (dgvGames.Columns.Contains(info.Name)) {
                        var col = dgvGames.Columns[info.Name];
                        col.Visible = info.Visible;
                        col.DisplayIndex = info.Order;
                        col.HeaderText = info.Header;
                    }
                }
                
                dgvGames.DataBindingComplete -= OnColumnsAdded;
            }
            dgvGames.DataBindingComplete += OnColumnsAdded;

            // Привязка контролов
            txtTitle.DataBindings.Add("Text", bsGames, "Title", true, DataSourceUpdateMode.OnPropertyChanged);

            var days = new List<DayInfo> {
                new DayInfo { Value = 1, Name = "Понедельник" },
                new DayInfo { Value = 2, Name = "Вторник" },
                new DayInfo { Value = 3, Name = "Среда" },
                new DayInfo { Value = 4, Name = "Четверг" },
                new DayInfo { Value = 5, Name = "Пятница" },
                new DayInfo { Value = 6, Name = "Суббота" },
                new DayInfo { Value = 7, Name = "Воскресенье" }
            };
            cmbGameDay.DataSource = days;
            cmbGameDay.DisplayMember = "Name";
            cmbGameDay.ValueMember = "Value";
            cmbGameDay.DataBindings.Add("SelectedValue", bsGames, "GameDay", true, DataSourceUpdateMode.OnPropertyChanged);
            cmbGameDay.SelectionChangeCommitted += (s, e) => {
                try {
                    if (cmbGameDay.SelectedValue is int dayId && bsGames.Current is VolleybollGame current) {
                        current.GameDay = dayId; // Принудительно синхронизируем значение
                        string dayName = GetDayName(dayId);
                        if (!string.IsNullOrEmpty(current.Title)) {
                            string ratingPrefix = "Рейтинговая игра! ";
                            string trainingPrefix = "Тренировка! ";
                            
                            string cleanTitle = current.Title;
                            if (cleanTitle.StartsWith(ratingPrefix)) cleanTitle = cleanTitle.Substring(ratingPrefix.Length);
                            else if (cleanTitle.StartsWith(trainingPrefix)) cleanTitle = cleanTitle.Substring(trainingPrefix.Length);

                            // Ищем название дня перед первой запятой или просто в начале строки
                            var dayRegex = new System.Text.RegularExpressions.Regex(@"^[^,]*", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            if (dayRegex.IsMatch(cleanTitle)) {
                                cleanTitle = dayRegex.Replace(cleanTitle, dayName);
                            } else {
                                cleanTitle = dayName + ", " + cleanTitle;
                            }
                            current.Title = cleanTitle; // Сначала ставим чистый заголовок
                            UpdateTitlePrefixes(current); // А потом вешаем нужные префиксы
                        } else {
                            current.Title = dayName + ", @GameDayName Волейбол";
                        }
                        txtTitle.Text = current.Title;
                        dgvGames.Invalidate(); 
                    }
                } catch (Exception ex) {
                    Logger.Log("Error in cmbGameDay_SelectionChangeCommitted", ex);
                }
            };

            var hours = Enumerable.Range(0, 24).ToList();
            cmbGameStartHour.DataSource = hours;
            cmbGameStartHour.DataBindings.Add("SelectedItem", bsGames, "GameStartHour", true, DataSourceUpdateMode.OnPropertyChanged);
            cmbGameStartHour.SelectionChangeCommitted += (s, e) => UpdateGameTitleTime();
            var minutes = Enumerable.Range(0, 60).ToList();
            cmbGameStartMinute.DataSource = minutes;
            cmbGameStartMinute.DataBindings.Add("SelectedItem", bsGames, "GameStartMinute", true, DataSourceUpdateMode.OnPropertyChanged);
            cmbGameStartMinute.SelectionChangeCommitted += (s, e) => UpdateGameTitleTime();
            numPullBeforeDay.DataBindings.Add("Value", bsGames, "PullBeforeDay", true, DataSourceUpdateMode.OnPropertyChanged);

            var pullHours = Enumerable.Range(0, 24).ToList();
            cmbPullHour.DataSource = pullHours;
            cmbPullHour.DataBindings.Add("SelectedItem", bsGames, "PullHour", true, DataSourceUpdateMode.OnPropertyChanged);

            var pullMinutes = Enumerable.Range(0, 60).ToList();
            cmbPullMinute.DataSource = pullMinutes;
            cmbPullMinute.DataBindings.Add("SelectedItem", bsGames, "PullMinute", true, DataSourceUpdateMode.OnPropertyChanged);
            chkActiveGame.DataBindings.Add("Checked", bsGames, "ActiveGame", true, DataSourceUpdateMode.OnPropertyChanged);
            chkActiveGame.CheckedChanged += (s, e) => dgvGames.Invalidate();
            chkRatingGame.DataBindings.Add("Checked", bsGames, "RatingGame", true, DataSourceUpdateMode.OnPropertyChanged);
            chkRatingGame.Click += (s, e) => {
                try {
                    if (bsGames.Current is VolleybollGame current) {
                        current.RatingGame = chkRatingGame.Checked;
                        if (current.RatingGame) {
                            current.TrainingGame = false;
                            chkTrainingGame.Checked = false;
                        }
                        UpdateTitlePrefixes(current);
                        txtTitle.Text = current.Title;
                        dgvGames.Invalidate();
                    }
                } catch (Exception ex) {
                    Logger.Log("Error in chkRatingGame_Click", ex);
                }
            };

            chkTrainingGame.DataBindings.Add("Checked", bsGames, "TrainingGame", true, DataSourceUpdateMode.OnPropertyChanged);
            chkTrainingGame.Click += (s, e) => {
                try {
                    if (bsGames.Current is VolleybollGame current) {
                        current.TrainingGame = chkTrainingGame.Checked;
                        if (current.TrainingGame) {
                            current.RatingGame = false;
                            chkRatingGame.Checked = false;
                        }
                        UpdateTitlePrefixes(current);
                        txtTitle.Text = current.Title;
                        dgvGames.Invalidate();
                    }
                } catch (Exception ex) {
                    Logger.Log("Error in chkTrainingGame_Click", ex);
                }
            };

            // Настройка для залов
            var gymsJson = AppConfigHelper.LoadSetting("GymsJson");
            var gyms = string.IsNullOrEmpty(gymsJson) ? new List<Gym>() : System.Text.Json.JsonSerializer.Deserialize<List<Gym>>(gymsJson) ?? new List<Gym>();
            bsGyms.DataSource = new BindingList<Gym>(gyms);
            dgvGyms.DataSource = bsGyms;
            dgvGyms.AutoGenerateColumns = true;
            dgvGyms.ReadOnly = true;
            dgvGyms.AllowUserToAddRows = false;

            // Настройка выбора зала (делаем после того как в bsGyms появились данные)
            cmbGym.DisplayMember = "Name";
            cmbGym.ValueMember = "Id";
            cmbGym.DataSource = bsGyms;
            cmbGym.DataBindings.Add("SelectedValue", bsGames, "GymId", true, DataSourceUpdateMode.OnPropertyChanged);
            cmbGym.SelectionChangeCommitted += (s, e) => {
                try {
                    if (cmbGym.SelectedValue is int gymId && bsGames.Current is VolleybollGame current) {
                        current.GymId = gymId; // Принудительно обновляем
                        
                        var gyms = bsGyms.DataSource as BindingList<Gym>;
                        var gym = gyms?.FirstOrDefault(g => g.Id == gymId);
                        if (gym != null) {
                            UpdateGameTitleWithGymName(current, gym.Name ?? "");
                            txtTitle.Text = current.Title;
                        }
                        dgvGames.Invalidate(); // Обновляем грид
                    }
                } catch (Exception ex) {
                    Logger.Log("Error in cmbGym_SelectionChangeCommitted", ex);
                }
            };

            // Автоматическое обновление всех игр при изменении названия зала в справочнике
            bsGyms.ListChanged += (s, e) => {
                if (e.ListChangedType == ListChangedType.ItemChanged) {
                    if (bsGyms[e.NewIndex] is Gym gym && !string.IsNullOrEmpty(gym.Name)) {
                        var gamesList = bsGames.DataSource as BindingList<VolleybollGame>;
                        if (gamesList != null) {
                            for (int i = 0; i < gamesList.Count; i++) {
                                if (gamesList[i].GymId == gym.Id) {
                                    UpdateGameTitleWithGymName(gamesList[i], gym.Name);
                                    if (bsGames.Current == gamesList[i]) {
                                        txtTitle.Text = gamesList[i].Title;
                                    }
                                }
                            }
                            dgvGames.Invalidate(); // Перерисовываем сетку чтобы увидеть изменения
                        }
                    }
                }
            };

            numGymId.DataBindings.Add("Value", bsGyms, "Id", true, DataSourceUpdateMode.OnPropertyChanged);
            txtGymName.DataBindings.Add("Text", bsGyms, "Name", true, DataSourceUpdateMode.OnPropertyChanged);
            txtGymLocation.DataBindings.Add("Text", bsGyms, "Location", true, DataSourceUpdateMode.OnPropertyChanged);
        }

        private async void MinuteTimer_Tick(object sender, EventArgs e)
        {
            try
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

                if (curTime.Hour == 22 && curTime.Minute == 55)
                {
                    if (telConnector != null) await telConnector.DeleteUnansweredSurveys();
                    safeArchPolls();
                }

                if (curTime.Hour == 23 && curTime.Minute == 55)
                {
                    safeArchPolls();
                }

                if (curTime.Hour == 11 && curTime.Minute == 00)
                {
                    await sendInvitationAsync();
                    bsPoll.ResetBindings(false);
                    bsPlayer.ResetBindings(false);
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
                        }

                        if (telConnector != null && poll.isTimeToSendAfterGameSurvey(curTime))
                        {
                            // опрос после игры
                            await telConnector.sendAfterGameSurvey(poll);
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
            catch (Exception ex)
            {
                Logger.Log("Error in MinuteTimer_Tick", ex);
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
                    Logger.Log($"Unable to parse the string {stringArray[i]} to an integer.");
                }
            }

            return intArray;
        }

        private async void btnCreatePoll_press(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Logger.Log("Error in btnCreatePoll_press", ex);
                Logger.Log(ex.Message, ex);
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
            try
            {
                await telConnector.ReadMessages(0);
            }
            catch (Exception ex)
            {
                Logger.Log("Error in ReadUpdates", ex);
            }
        }

        private void onPlayerSelect(object sender, EventArgs e)
        {
            if (bsPlayer.Current is PlayerVote selectedVote)
            {
                // Clear filter to ensure the player is visible in the right list
                if (!string.IsNullOrEmpty(filter.Text))
                {
                    filter.Text = "";
                }

                // Find player in bsRating by ID
                for (int i = 0; i < bsRating.Count; i++)
                {
                    if (bsRating[i] is Player p && p.id == selectedVote.id)
                    {
                        bsRating.Position = i;
                        break;
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridViewPlayers.SelectedRows.Count == 0 || dataGridViewPoll.SelectedRows.Count == 0)
                return;

            int selectedIndex = dataGridViewPlayers.SelectedRows[0].Index;
            int pollIndex = dataGridViewPoll.SelectedRows[0].Index;

            if (pollIndex < 0 || pollIndex >= stateManager.state.pollList.Count)
                return;

            var list = stateManager.state.pollList[pollIndex].playrsList;
            movePlayerUp(list, selectedIndex);

            if (selectedIndex > 0)
            {
                bsPoll.ResetBindings(false);
                bsPlayer.ResetBindings(false);
                if (dataGridViewPlayers.Rows.Count > selectedIndex)
                {
                    dataGridViewPlayers.Rows[selectedIndex].Selected = false;
                    dataGridViewPlayers.Rows[selectedIndex - 1].Selected = true;
                }
                stateManager.SaveState();
            }
        }

        void movePlayerDown(List<PlayerVote> players, int index)
        {
            if (index < 0 || index >= players.Count - 1)
            {
                Logger.Log("Игрок уже находится на нижней позиции.");
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
                Logger.Log("Игрок уже находится на верхней позиции.");
                return;
            }

            PlayerVote temp = players[index - 1];
            players[index - 1] = players[index];
            players[index] = temp;

        }

        private void button5_Click(object sender, EventArgs e) // DOWN
        {
            if (dataGridViewPlayers.SelectedRows.Count == 0 || dataGridViewPoll.SelectedRows.Count == 0)
                return;

            int selectedIndex = dataGridViewPlayers.SelectedRows[0].Index;
            int pollIndex = dataGridViewPoll.SelectedRows[0].Index;

            if (pollIndex < 0 || pollIndex >= stateManager.state.pollList.Count)
                return;

            var list = stateManager.state.pollList[pollIndex].playrsList;
            movePlayerDown(list, selectedIndex);

            if (selectedIndex < list.Count - 1)
            {
                bsPoll.ResetBindings(false);
                bsPlayer.ResetBindings(false);
                if (dataGridViewPlayers.Rows.Count > selectedIndex + 1)
                {
                    dataGridViewPlayers.Rows[selectedIndex].Selected = false;
                    dataGridViewPlayers.Rows[selectedIndex + 1].Selected = true;
                }
                stateManager.SaveState();
            }
        }

        private void ArchPools(object sender, EventArgs e)
        {
            safeArchPolls();
        }

        private void safeArchPolls()
        {
            SafeInvoke(() =>
            {
                try
                {
                    // Reset position to safe row before modification
                    if (bsPoll.Count > 0) bsPoll.Position = 0;
                    
                    telConnector?.ArchPolls();
                    
                    bsPoll.ResetBindings(false);
                    bsPlayer.ResetBindings(false);
                    
                    Logger.Log("Архивация опросов выполнена успешно.");
                }
                catch (Exception ex)
                {
                    Logger.Log("Ошибка при архивации опросов", ex);
                }
            });
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

              var stop1 = new CarStops("Парковка Gardens 19.35", "");
              var stop2 = new CarStops("Метро Gardens 19.40", "");

              newCar.carStops.Add(stop1);
              newCar.carStops.Add(stop2);

              this.stateManager.state.carList.Add(newCar);

          }*/

        private void AddPlayers(object sender, EventArgs e)
        {
            if (bsPoll.Current is Poll curPoll)
            {
                stateManager.AddPlayersToRating(curPoll);
            }

        }

        private void btnSavePlayer_Click(object sender, EventArgs e)
        {
            try
            {
                bsRating.EndEdit();
                stateManager.SavePlayers();
                MessageBox.Show("Изменения сохранены", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Log("Error in btnSavePlayer_Click", ex);
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }

        private void btnDeletePlayer_Click(object sender, EventArgs e)
        {
            try
            {
                if (bsRating.Current is Player player)
                {
                    var res = MessageBox.Show($"Удалить игрока {player.name}?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (res == DialogResult.Yes)
                    {
                        stateManager.players.Remove(player);
                        stateManager.SavePlayers();
                        bsRating.ResetBindings(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error in btnDeletePlayer_Click", ex);
                MessageBox.Show("Ошибка при удалении: " + ex.Message);
            }
        }

        private void dataGridViewRating_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            stateManager.SavePlayers();
        }

        private async void button9_Click(object sender, EventArgs e)
        {
            // Отправка опроса после игры
            if (stateManager.state.pollList != null && stateManager.state.pollList.Any())
            {
                // Пытаемся найти опрос на сегодня
                var today = DateTime.Now.Date;
                var targetPoll = stateManager.state.pollList.FirstOrDefault(p =>
                {
                    if (DateTime.TryParse(p.date, out DateTime d)) return d.Date == today;
                    return false;
                });

                // Если на сегодня нет, берем первый в списке
                if (targetPoll == null)
                {
                    targetPoll = stateManager.state.pollList.First();
                }

                if (targetPoll != null && telConnector != null)
                {
                    await telConnector.sendAfterGameSurvey(targetPoll);
                    MessageBox.Show($"Опрос после игры отправлен для игры: {targetPoll.date}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Нет доступных игр для отправки опроса", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Form1_Activated(object sender, EventArgs e)
        {

        }


        private void filter_TextChanged(object sender, EventArgs e)
        {
            try
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
                        
                        // Безопасная установка видимости
                        if (row.Visible != visible)
                        {
                            row.Visible = visible;
                        }

                        if (visible) anyVisible = true;
                    }
                }

                cm.ResumeBinding();
                
                // Force a refresh to sync UI indices
                dataGridViewRating.Refresh();

                // Если остались видимые строки — выделим первую
                if (anyVisible)
                {
                    foreach (DataGridViewRow row in dataGridViewRating.Rows)
                    {
                        if (row.Visible && row.Cells.Count > 0)
                        {
                            dataGridViewRating.CurrentCell = row.Cells[0];
                            row.Selected = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error in filter_TextChanged", ex);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (dataGridViewPoll == null || dataGridViewPlayers == null || tabControl1 == null) return;

            int margin = 10;
            int bottomPanelHeight = 105; // Компактная панель кнопок
            int formWidth = this.ClientSize.Width;
            int formHeight = this.ClientSize.Height;

            // 1. Верхняя таблица (Опросы) - 20% высоты, но не более 220 пикселей
            dataGridViewPoll.Left = margin;
            dataGridViewPoll.Top = margin;
            dataGridViewPoll.Width = formWidth - 2 * margin;
            dataGridViewPoll.Height = Math.Min((int)(formHeight * 0.2), 220);

            // 2. Расчет области для нижних гридов
            int gridsTop = dataGridViewPoll.Bottom + 5;
            int remainingHeight = formHeight - gridsTop - bottomPanelHeight - 5;
            if (remainingHeight < 150) remainingHeight = 150; 

            // 3. Таблица игроков
            int leftPanelWidth = (int)(formWidth * 0.45);
            dataGridViewPlayers.Left = margin;
            dataGridViewPlayers.Top = gridsTop;
            dataGridViewPlayers.Width = leftPanelWidth;
            dataGridViewPlayers.Height = remainingHeight;

            // 4. Кнопки UP/DOWN
            if (button4 != null && button5 != null)
            {
                int buttonWidth = 50;
                int spacerLeft = dataGridViewPlayers.Right + 5;
                button4.Left = button5.Left = spacerLeft;
                button4.Width = button5.Width = buttonWidth;
                
                int halfHeight = remainingHeight / 2;
                button4.Top = gridsTop;
                button4.Height = halfHeight;
                button5.Top = gridsTop + halfHeight;
                button5.Height = remainingHeight - halfHeight;
            }

            // 5. Вкладки
            int tabsLeft = (button4?.Right ?? dataGridViewPlayers.Right) + 5;
            tabControl1.Left = tabsLeft;
            tabControl1.Top = gridsTop;
            tabControl1.Width = formWidth - tabsLeft - margin;
            tabControl1.Height = remainingHeight;

            // 6. Кнопки в самом низу (два ряда)
            int btnY1 = gridsTop + remainingHeight + 5;
            int btnY2 = btnY1 + 48;
            int btnWidth = (formWidth - 5 * margin) / 4;

            // Первый ряд
            if (btnCreatePoll != null) btnCreatePoll.SetBounds(margin, btnY1, btnWidth, 40);
            if (button1 != null) button1.SetBounds(margin + btnWidth + 5, btnY1, btnWidth, 40);
            if (button2 != null) button2.SetBounds(margin + 2 * (btnWidth + 5), btnY1, btnWidth, 40);
            if (button3 != null) button3.SetBounds(margin + 3 * (btnWidth + 5), btnY1, btnWidth, 40);

            // Второй ряд
            int smallBtnWidth = (formWidth - 7 * margin) / 6;
            if (button11 != null) button11.SetBounds(margin, btnY2, smallBtnWidth, 40);
            if (button9 != null) button9.SetBounds(margin + smallBtnWidth + 5, btnY2, smallBtnWidth, 40);
            if (getCars != null) getCars.SetBounds(margin + 2 * (smallBtnWidth + 5), btnY2, smallBtnWidth, 40);
            if (button10 != null) button10.SetBounds(margin + 3 * (smallBtnWidth + 5), btnY2, smallBtnWidth, 40);
            if (del_afterGameSurvey != null) del_afterGameSurvey.SetBounds(margin + 4 * (smallBtnWidth + 5), btnY2, smallBtnWidth, 40);
            if (btnUpdateSummary != null) btnUpdateSummary.SetBounds(margin + 5 * (smallBtnWidth + 5), btnY2, smallBtnWidth, 40);
        }

        private void txtCarFilter_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string filterText = (sender as TextBox)?.Text?.Trim().ToLower() ?? "";

                CurrencyManager cm = (CurrencyManager)BindingContext[dgvCars.DataSource];
                cm.SuspendBinding();

                dgvCars.ClearSelection();
                dgvCars.CurrentCell = null;

                bool anyVisible = false;

                foreach (DataGridViewRow row in dgvCars.Rows)
                {
                    if (row.DataBoundItem is Car car)
                    {
                        bool visible = string.IsNullOrEmpty(filterText) ||
                                       (car.name != null && car.name.ToLower().Contains(filterText))
                                       || car.idPlayer.ToString().Contains(filterText)
                                       || (car.firstName != null && car.firstName.ToLower().Contains(filterText));

                        if (row.Visible != visible)
                        {
                            row.Visible = visible;
                        }

                        if (visible) anyVisible = true;
                    }
                }

                cm.ResumeBinding();
                dgvCars.Refresh();

                if (anyVisible)
                {
                    foreach (DataGridViewRow row in dgvCars.Rows)
                    {
                        if (row.Visible && row.Cells.Count > 0)
                        {
                            dgvCars.CurrentCell = row.Cells[0];
                            row.Selected = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error in txtCarFilter_TextChanged", ex);
            }
        }

        private void getStat(object sender, EventArgs e)
        {
            // Заглушка для статистики
            MessageBox.Show("Функция статистики в разработке", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void button11_ClickAsync(object sender, EventArgs e)
        {
            // Приглашение игроков
            await sendInvitationAsync();
        }

        private void getCars_Click(object sender, EventArgs e)
        {
            // Заглушка для табло
            MessageBox.Show("Функция табло в разработке", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BsPlayer_CurrentChanged(object? sender, EventArgs e)
        {
            // Если мы на вкладке "Машины", пытаемся найти машину выбранного игрока
            if (tabControl1.SelectedTab == Cars && bsPlayer.Current is PlayerVote pv)
            {
                for (int i = 0; i < bsCars.Count; i++)
                {
                    if (bsCars[i] is Car car && car.idPlayer == pv.id)
                    {
                        bsCars.Position = i;
                        break;
                    }
                }
            }
        }

        private void dgvCars_SelectionChanged(object sender, EventArgs e)
        {
            if (bsCars.Current is Car selectedCar)
            {
                // Запрещаем редактирование ID у существующих машин, если нужно, 
                // или просто обновляем селект в комбобоксе
                UpdatePlayerPickerFromCurrentCar();
                bsCarStops.DataSource = selectedCar.carStops;
            }
            else
            {
                // Вместо null используем тип, чтобы сохранить метаданные для биндингов
                bsCarStops.DataSource = typeof(CarStops);
            }
        }

        private void BsCars_CurrentChanged(object? sender, EventArgs e)
        {
            UpdatePlayerPickerFromCurrentCar();
        }

        private void UpdatePlayerPickerFromCurrentCar()
        {
            if (bsCars.Current is Car car)
            {
                cmbCarIdPlayer.SelectedIndexChanged -= cmbCarIdPlayer_SelectedIndexChanged;
                // Ищем игрока в комбобоксе
                int foundIndex = -1;
                for (int i = 0; i < cmbCarIdPlayer.Items.Count; i++)
                {
                    if (cmbCarIdPlayer.Items[i] is Player p && p.id == car.idPlayer)
                    {
                        foundIndex = i;
                        break;
                    }
                }
                cmbCarIdPlayer.SelectedIndex = foundIndex;
                cmbCarIdPlayer.SelectedIndexChanged += cmbCarIdPlayer_SelectedIndexChanged;
            }
        }

        private void UpdateCarNamesFromPlayers()
        {
            foreach (var car in stateManager.state.carList)
            {
                var player = stateManager.players.FirstOrDefault(p => p.id == car.idPlayer);
                if (player != null) car.UpdateFromPlayer(player);
            }
        }

        private void RefreshCarPlayerPicker()
        {
            cmbCarIdPlayer.Items.Clear();
            var sortedPlayers = stateManager.players.OrderBy(p => p.name).ToList();
            foreach (var p in sortedPlayers)
            {
                cmbCarIdPlayer.Items.Add(p);
            }
            cmbCarIdPlayer.DisplayMember = "NameWithId";
            cmbCarIdPlayer.DropDownWidth = 500;
        }

        private void cmbCarIdPlayer_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (bsCars.Current is Car car && cmbCarIdPlayer.SelectedItem is Player p)
            {
                car.idPlayer = p.id;
                car.UpdateFromPlayer(p);
                bsCars.ResetCurrentItem();
            }
        }

        private void btnAddCar_Click(object sender, EventArgs e)
        {
            var newCar = new Car(0, "Ник", "Имя", 4);
            stateManager.state.carList.Add(newCar);
            bsCars.MoveLast();
        }

        private void btnDeleteCar_Click(object sender, EventArgs e)
        {
            if (bsCars.Current is Car car)
            {
                if (MessageBox.Show($"Удалить машину игрока {car.name}?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    bsCars.RemoveCurrent();
                }
            }
        }

        private void btnSaveCars_Click(object sender, EventArgs e)
        {
            try
            {
                this.Validate();
                bsCars.EndEdit();
                bsCarStops.EndEdit();
                stateManager.SaveState();
                MessageBox.Show("Данные о машинах и остановках сохранены.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private void btnAddStop_Click(object sender, EventArgs e)
        {
            if (bsCars.Current is Car selectedCar)
            {
                var newStop = new CarStops("Новая остановка", "http://", 0);
                selectedCar.carStops.Add(newStop);
                bsCarStops.ResetBindings(false);
                bsCarStops.MoveLast();
            }
        }

        private void btnOpenStopLink_Click(object sender, EventArgs e)
        {
            try
            {
                string url = txtStopLink.Text;
                if (!string.IsNullOrWhiteSpace(url))
                {
                    // Для Windows нужно указывать UseShellExecute = true, чтобы открыть URL в браузере по умолчанию
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть ссылку: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteStop_Click(object sender, EventArgs e)
        {
            if (bsCarStops.Current is CarStops stop)
            {
                if (MessageBox.Show($"Удалить остановку {stop.name}?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    bsCarStops.RemoveCurrent();
                }
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            telConnector?.DeleteUnansweredSurveys();
        }

        private async void btnUpdateSummary_Click(object sender, EventArgs e)
        {
            if (stateManager.state.pollList != null && stateManager.state.pollList.Any())
            {
                // Пытаемся найти опрос на сегодня
                var today = DateTime.Now.ToString("dd.MM");
                var targetPoll = stateManager.state.pollList.FirstOrDefault(p => p.date == today && p.approved);


                if (targetPoll != null && telConnector != null)
                {
                    await telConnector.updatePostGameSummaryMessage(targetPoll);
                    MessageBox.Show($"Сообщение с итогами обновлено для игры: {targetPoll.date}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
            else
            {
                MessageBox.Show("Нет доступных игр для обновления итогов", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSaveGamesJson_Click(object sender, EventArgs e)
        {
            try
            {
                // Принудительно завершаем редактирование ячейки и формы
                this.Validate();
                bsGames.EndEdit();

                var gamesBindingList = (BindingList<VolleybollGame>)bsGames.DataSource;
                var games = gamesBindingList.ToList();

                var formattedJson = System.Text.Json.JsonSerializer.Serialize(games, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
                });
                
                AppConfigHelper.SaveSetting("GamesJson", formattedJson);
                BallBotGui.Properties.Settings.Default.GamesJson = formattedJson;
                
                if (gameManager != null)
                {
                    gameManager.LoadGames();
                }
                
                MessageBox.Show("Настройки успешно сохранены и применены.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddGame_Click(object sender, EventArgs e)
        {
            var newGame = new VolleybollGame
            {
                Title = "Новая игра",
                ActiveGame = true,
                GameDay = 1,
                GameStartHour = 20,
                GameStartMinute = 0,
                PullBeforeDay = 1,
                PullHour = 10,
                PullMinute = 0
            };
            
            bsGames.Add(newGame);
        }

        private void btnDeleteGame_Click(object sender, EventArgs e)
        {
            if (bsGames.Current != null)
            {
                if (MessageBox.Show("Уверены, что хотите удалить эту игру?", "Удаление", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    bsGames.RemoveCurrent();
                }
            }
        }

        private void btnAddGym_Click(object sender, EventArgs e)
        {
            var gyms = bsGyms.DataSource as BindingList<Gym>;
            if (gyms != null)
            {
                int nextId = gyms.Count > 0 ? gyms.Max(g => g.Id) + 1 : 1;
                gyms.Add(new Gym { Id = nextId, Name = "Новый зал" });
                bsGyms.MoveLast();
            }
        }

        private void btnDeleteGym_Click(object sender, EventArgs e)
        {
            if (bsGyms.Current is Gym gym)
            {
                if (MessageBox.Show($"Удалить зал '{gym.Name}'?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    bsGyms.RemoveCurrent();
                }
            }
        }

        private void btnSaveGyms_Click(object sender, EventArgs e)
        {
            try
            {
                this.Validate();
                bsGyms.EndEdit();

                var gyms = bsGyms.DataSource as BindingList<Gym>;
                if (gyms != null)
                {
                    string json = System.Text.Json.JsonSerializer.Serialize(gyms, new JsonSerializerOptions 
                    { 
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });
                    AppConfigHelper.SaveSetting("GymsJson", json);
                    Properties.Settings.Default.GymsJson = json;
                    MessageBox.Show("Справочник залов сохранен успешно!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении залов: {ex.Message}");
            }
        }

        private void dgvGames_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            try {
                if (e.RowIndex < 0 || e.RowIndex >= dgvGames.Rows.Count) return;

                var game = dgvGames.Rows[e.RowIndex].DataBoundItem as VolleybollGame;
                if (game == null) return;

                // Подсветка активных игр
                if (game.ActiveGame)
                {
                    e.CellStyle.BackColor = System.Drawing.Color.LightGreen;
                }

                // Проверяем наличие колонок перед обращением
                if (dgvGames.Columns.Count <= e.ColumnIndex) return;
                var colName = dgvGames.Columns[e.ColumnIndex].Name;

                // Замена номера дня на название
                if (colName == "GameDay")
                {
                    if (e.Value is int dayId)
                    {
                        e.Value = GetDayName(dayId);
                        e.FormattingApplied = true;
                    }
                }

                if (colName == "GymId")
                {
                    if (e.Value is int gymId)
                    {
                        var gyms = bsGyms.DataSource as BindingList<Gym>;
                        var gym = gyms?.FirstOrDefault(g => g.Id == gymId);
                        if (gym != null)
                        {
                            e.Value = gym.Name;
                            e.FormattingApplied = true;
                        }
                    }
                }

                // Форматирование времени начала
                if (colName == "GameStartHour")
                {
                    e.Value = $"{game.GameStartHour:D2}:{game.GameStartMinute:D2}";
                    e.FormattingApplied = true;
                }
            } catch {
                // Formatting errors are usually transient or during shutdown/init, silent ignore
            }
        }

        private void UpdateGameTitleTime()
        {
            try {
                if (bsGames.Current is VolleybollGame current) {
                    // Синхронизируем значения из контролов в объект вручную, 
                    // так как биндинг может срабатывать с задержкой
                    if (cmbGameStartHour.SelectedItem is int h) current.GameStartHour = h;
                    if (cmbGameStartMinute.SelectedItem is int m) current.GameStartMinute = m;

                    string startTime = $"{current.GameStartHour:D2}:{current.GameStartMinute:D2}";
                    string endTime = $"{(current.GameStartHour + 2) % 24:D2}:{current.GameStartMinute:D2}";
                    string timeRange = $"{startTime} - {endTime}";

                    if (string.IsNullOrEmpty(current.Title)) {
                        current.Title = $"@GameDayName Волейбол {timeRange}";
                    } else {
                        // Ищем паттерн времени HH.mm - HH.mm или HH:mm - HH:mm
                        var regex = new System.Text.RegularExpressions.Regex(@"\d{2}[\.:]\d{2}\s*-\s*\d{2}[\.:]\d{2}");
                        if (regex.IsMatch(current.Title)) {
                            current.Title = regex.Replace(current.Title, timeRange);
                        } else {
                            current.Title = current.Title.Trim() + " " + timeRange;
                        }
                    }
                    
                    // Обновляем текст напрямую, чтобы не мешать вводу в NumericUpDown (не вызываем ResetCurrentItem)
                    if (txtTitle.Text != current.Title) {
                        txtTitle.Text = current.Title;
                    }
                    dgvGames.Invalidate(); // Обновляем грид при смене времени
                }
            } catch (Exception ex) {
                Logger.Log("Error in UpdateGameTitleTime", ex);
            }
        }

        private void UpdateGameTitleWithGymName(VolleybollGame game, string gymName)
        {
            if (string.IsNullOrEmpty(game.Title) || string.IsNullOrEmpty(gymName)) return;
            // Пытаемся найти формат ЗАЛЕ(Название) или ЗАЛ(Название) и заменить Название
            var regex = new System.Text.RegularExpressions.Regex(@"ЗАЛЕ?\s*\((.*?)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var match = regex.Match(game.Title);
            if (match.Success)
            {
                game.Title = game.Title.Substring(0, match.Groups[1].Index) + gymName + game.Title.Substring(match.Groups[1].Index + match.Groups[1].Length);
            }
        }

        private void UpdateTitlePrefixes(VolleybollGame game)
        {
            if (string.IsNullOrEmpty(game.Title)) return;

            string ratingPrefix = "Рейтинговая игра! ";
            string trainingPrefix = "Тренировка! ";

            // Сначала очищаем от обоих префиксов
            if (game.Title.StartsWith(ratingPrefix)) game.Title = game.Title.Substring(ratingPrefix.Length);
            else if (game.Title.StartsWith(trainingPrefix)) game.Title = game.Title.Substring(trainingPrefix.Length);

            // Добавляем нужный
            if (game.RatingGame) game.Title = ratingPrefix + game.Title;
            else if (game.TrainingGame) game.Title = trainingPrefix + game.Title;
        }

        private string GetDayName(int dayId)
        {
            return dayId switch
            {
                1 => "Понедельник",
                2 => "Вторник",
                3 => "Среда",
                4 => "Четверг",
                5 => "Пятница",
                6 => "Суббота",
                7 => "Воскресенье",
                _ => dayId.ToString()
            };
        }

        public class DayInfo
        {
            public int Value { get; set; }
            public string Name { get; set; } = "";
        }
    }
}