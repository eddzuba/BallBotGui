namespace BallBotGui
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            minuteTimer = new System.Windows.Forms.Timer(components);
            btnCreatePoll = new Button();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            dataGridViewPoll = new DataGridView();
            dataGridViewPlayers = new DataGridView();
            button4 = new Button();
            button5 = new Button();
            button6 = new Button();
            button7 = new Button();
            button8 = new Button();
            dataGridViewRating = new DataGridView();
            tabControl1 = new TabControl();
            Players = new TabPage();
            label1 = new Label();
            filter = new TextBox();
            Cars = new TabPage();
            dataGridViewCarStops = new DataGridView();
            dgvCars = new DataGridView();
            carStopsBindingSource = new BindingSource(components);
            getCars = new Button();
            button9 = new Button();
            button10 = new Button();
            button11 = new Button();
            del_afterGameSurvey = new Button();
            btnUpdateSummary = new Button();
            lblCarFilter = new Label();
            txtCarFilter = new TextBox();
            tabGamesConfig = new TabPage();
            dgvGames = new DataGridView();
            gbEditGame = new GroupBox();
            lblTitle = new Label();
            txtTitle = new TextBox();
            lblGameDay = new Label();
            cmbGameDay = new ComboBox();
            lblGameStartHour = new Label();
            cmbGameStartHour = new ComboBox();
            lblGameStartMinute = new Label();
            cmbGameStartMinute = new ComboBox();
            lblPullBeforeDay = new Label();
            numPullBeforeDay = new NumericUpDown();
            lblPullHour = new Label();
            cmbPullHour = new ComboBox();
            lblPullMinute = new Label();
            cmbPullMinute = new ComboBox();
            chkActiveGame = new CheckBox();
            chkRatingGame = new CheckBox();
            chkTrainingGame = new CheckBox();
            btnAddGame = new Button();
            btnDeleteGame = new Button();
            btnSaveGamesJson = new Button();
            tabGyms = new TabPage();
            dgvGyms = new DataGridView();
            gbEditGym = new GroupBox();
            lblGymId = new Label();
            numGymId = new NumericUpDown();
            lblGymName = new Label();
            txtGymName = new TextBox();
            lblGymLocation = new Label();
            txtGymLocation = new TextBox();
            btnAddGym = new Button();
            btnDeleteGym = new Button();
            btnSaveGyms = new Button();
            lblGymChoice = new Label();
            cmbGym = new ComboBox();
            gbEditPlayer = new GroupBox();
            gbEditCar = new GroupBox();
            lblCarIdPlayer = new Label();
            cmbCarIdPlayer = new ComboBox();
            lblCarName = new Label();
            txtCarName = new TextBox();
            lblCarFirstName = new Label();
            txtCarFirstName = new TextBox();
            lblCarPlaceCount = new Label();
            cmbCarPlaceCount = new ComboBox();
            btnAddCar = new Button();
            btnDeleteCar = new Button();
            btnSaveCars = new Button();

            gbEditStop = new GroupBox();
            btnOpenStopLink = new Button();
            lblStopName = new Label();
            txtStopName = new TextBox();
            lblStopLink = new Label();
            txtStopLink = new TextBox();
            lblStopMinBefore = new Label();
            numStopMinBefore = new NumericUpDown();
            btnAddStop = new Button();
            btnDeleteStop = new Button();

            lblCarsHeader = new Label();
            lblStopsHeader = new Label();

            lblEditName = new Label();
            txtEditName = new TextBox();
            lblEditFirstName = new Label();
            txtEditFirstName = new TextBox();
            lblEditNormalName = new Label();
            txtEditNormalName = new TextBox();
            lblEditRating = new Label();
            numEditRating = new NumericUpDown();
            lblEditGroup = new Label();
            numEditGroup = new NumericUpDown();
            chkEditIsFemale = new CheckBox();
            chkEditLevelChecked = new CheckBox();
            lblEditId = new Label();
            txtEditId = new TextBox();
            ((System.ComponentModel.ISupportInitialize)dataGridViewPoll).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewPlayers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewRating).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numEditRating).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numEditGroup).BeginInit();
            tabControl1.SuspendLayout();
            Players.SuspendLayout();
            Cars.SuspendLayout();
            gbEditCar.SuspendLayout();
            tabGamesConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvGames).BeginInit();
            gbEditGame.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPullBeforeDay).BeginInit();
            tabGyms.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvGyms).BeginInit();
            gbEditGym.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numGymId).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewCarStops).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvCars).BeginInit();
            ((System.ComponentModel.ISupportInitialize)carStopsBindingSource).BeginInit();
            SuspendLayout();
            // 
            // minuteTimer
            // 
            minuteTimer.Enabled = true;
            minuteTimer.Interval = 60000;
            minuteTimer.Tick += MinuteTimer_Tick;
            // 
            // btnCreatePoll
            // 
            btnCreatePoll.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCreatePoll.Location = new Point(1790, 1850);
            btnCreatePoll.Name = "btnCreatePoll";
            btnCreatePoll.Size = new Size(350, 60);
            btnCreatePoll.TabIndex = 0;
            btnCreatePoll.Text = "Запуск голосовалки";
            btnCreatePoll.UseVisualStyleBackColor = true;
            btnCreatePoll.Click += btnCreatePoll_press;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button1.Location = new Point(1140, 1850);
            button1.Name = "button1";
            button1.Size = new Size(320, 60);
            button1.TabIndex = 1;
            button1.Text = "Сохранение состояния";
            button1.UseVisualStyleBackColor = true;
            button1.Click += testSave;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button2.Location = new Point(1790, 1920);
            button2.Name = "button2";
            button2.Size = new Size(350, 60);
            button2.TabIndex = 2;
            button2.Text = "Загрузить состояния";
            button2.UseVisualStyleBackColor = true;
            button2.Click += RestoreState;
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button3.Location = new Point(1465, 1920);
            button3.Name = "button3";
            button3.Size = new Size(320, 60);
            button3.TabIndex = 3;
            button3.Text = "Прочитать обновления";
            button3.UseVisualStyleBackColor = true;
            button3.Click += ReadUpdates;
            // 
            // dataGridViewPoll
            // 
            dataGridViewPoll.AllowUserToAddRows = false;
            dataGridViewPoll.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewPoll.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewPoll.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewPoll.Location = new Point(12, 12);
            dataGridViewPoll.MultiSelect = false;
            dataGridViewPoll.Name = "dataGridViewPoll";
            dataGridViewPoll.RowHeadersWidth = 82;
            dataGridViewPoll.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewPoll.Size = new Size(2140, 300);
            dataGridViewPoll.TabIndex = 4;
            // 
            // dataGridViewPlayers
            // 
            dataGridViewPlayers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            dataGridViewPlayers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewPlayers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewPlayers.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewPlayers.Location = new Point(12, 320);
            dataGridViewPlayers.MultiSelect = false;
            dataGridViewPlayers.Name = "dataGridViewPlayers";
            dataGridViewPlayers.RowHeadersWidth = 82;
            dataGridViewPlayers.RowTemplate.Height = 30;
            dataGridViewPlayers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewPlayers.Size = new Size(1001, 1600);
            dataGridViewPlayers.TabIndex = 5;
            dataGridViewPlayers.SelectionChanged += onPlayerSelect;
            // 
            // button4
            // 
            button4.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            button4.Location = new Point(1015, 320);
            button4.Name = "button4";
            button4.Size = new Size(115, 600);
            button4.TabIndex = 6;
            button4.Text = "UP";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // button5
            // 
            button5.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            button5.Location = new Point(1015, 930);
            button5.Name = "button5";
            button5.Size = new Size(115, 990);
            button5.TabIndex = 7;
            button5.Text = "DOWN";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // button6
            // 
            button6.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button6.Location = new Point(1465, 1850);
            button6.Name = "button6";
            button6.Size = new Size(320, 60);
            button6.TabIndex = 8;
            button6.Text = "Архивировать опросы";
            button6.UseVisualStyleBackColor = true;
            button6.Click += ArchPools;
            // 
            // button7
            // 
            button7.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button7.Location = new Point(1469, 1126);
            button7.Name = "button7";
            button7.Size = new Size(303, 46);
            button7.TabIndex = 9;
            button7.Text = "Машины показать";
            button7.UseVisualStyleBackColor = true;
            button7.Click += clickSendInvitation;
            // 
            // button8
            // 
            button8.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button8.Location = new Point(1778, 1178);
            button8.Name = "button8";
            button8.Size = new Size(303, 46);
            button8.TabIndex = 10;
            button8.Text = "Добавить игроков";
            button8.UseVisualStyleBackColor = true;
            button8.Click += AddPlayers;
            // 
            // dataGridViewRating
            // 
            dataGridViewRating.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewRating.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewRating.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewRating.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewRating.Location = new Point(3, 70);
            dataGridViewRating.MultiSelect = false;
            dataGridViewRating.Name = "dataGridViewRating";
            dataGridViewRating.RowHeadersWidth = 82;
            dataGridViewRating.RowTemplate.Height = 30;
            dataGridViewRating.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewRating.Size = new Size(990, 440);
            dataGridViewRating.TabIndex = 11;
            dataGridViewRating.CellEndEdit += dataGridViewRating_CellEndEdit;
            // 
            // gbEditPlayer
            // 
            gbEditPlayer.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gbEditPlayer.Controls.Add(chkEditLevelChecked);
            gbEditPlayer.Controls.Add(chkEditIsFemale);
            gbEditPlayer.Controls.Add(numEditGroup);
            gbEditPlayer.Controls.Add(lblEditGroup);
            gbEditPlayer.Controls.Add(numEditRating);
            gbEditPlayer.Controls.Add(lblEditRating);
            gbEditPlayer.Controls.Add(txtEditNormalName);
            gbEditPlayer.Controls.Add(lblEditNormalName);
            gbEditPlayer.Controls.Add(txtEditFirstName);
            gbEditPlayer.Controls.Add(lblEditFirstName);
            gbEditPlayer.Controls.Add(txtEditName);
            gbEditPlayer.Controls.Add(lblEditName);
            gbEditPlayer.Controls.Add(txtEditId);
            gbEditPlayer.Controls.Add(lblEditId);
            gbEditPlayer.Location = new Point(3, 520);
            gbEditPlayer.Name = "gbEditPlayer";
            gbEditPlayer.Size = new Size(990, 440);
            gbEditPlayer.TabIndex = 14;
            gbEditPlayer.TabStop = false;
            gbEditPlayer.Text = "Редактирование игрока";
            // 
            // lblEditId
            // 
            lblEditId.AutoSize = true;
            lblEditId.Location = new Point(15, 45);
            lblEditId.Name = "lblEditId";
            lblEditId.Size = new Size(41, 32);
            lblEditId.TabIndex = 12;
            lblEditId.Text = "ID:";
            // 
            // txtEditId
            // 
            txtEditId.Location = new Point(204, 38);
            txtEditId.Name = "txtEditId";
            txtEditId.ReadOnly = true;
            txtEditId.Size = new Size(488, 39);
            txtEditId.TabIndex = 13;
            // 
            // lblEditName
            // 
            lblEditName.AutoSize = true;
            lblEditName.Location = new Point(15, 95);
            lblEditName.Name = "lblEditName";
            lblEditName.Size = new Size(125, 32);
            lblEditName.TabIndex = 0;
            lblEditName.Text = "Имя (ник):";
            // 
            // txtEditName
            // 
            txtEditName.Location = new Point(204, 78);
            txtEditName.Name = "txtEditName";
            txtEditName.Size = new Size(488, 39);
            txtEditName.TabIndex = 1;
            // 
            // lblEditFirstName
            // 
            lblEditFirstName.AutoSize = true;
            lblEditFirstName.Location = new Point(15, 145);
            lblEditFirstName.Name = "lblEditFirstName";
            lblEditFirstName.Size = new Size(66, 32);
            lblEditFirstName.TabIndex = 2;
            lblEditFirstName.Text = "Имя:";
            // 
            // txtEditFirstName
            // 
            txtEditFirstName.Location = new Point(204, 118);
            txtEditFirstName.Name = "txtEditFirstName";
            txtEditFirstName.Size = new Size(488, 39);
            txtEditFirstName.TabIndex = 3;
            // 
            // lblEditNormalName
            // 
            lblEditNormalName.AutoSize = true;
            lblEditNormalName.Location = new Point(15, 195);
            lblEditNormalName.Name = "lblEditNormalName";
            lblEditNormalName.Size = new Size(149, 32);
            lblEditNormalName.TabIndex = 4;
            lblEditNormalName.Text = "Норм. имя:";
            // 
            // txtEditNormalName
            // 
            txtEditNormalName.Location = new Point(204, 158);
            txtEditNormalName.Name = "txtEditNormalName";
            txtEditNormalName.Size = new Size(488, 39);
            txtEditNormalName.TabIndex = 5;
            // 
            // lblEditRating
            // 
            lblEditRating.AutoSize = true;
            lblEditRating.Location = new Point(15, 245);
            lblEditRating.Name = "lblEditRating";
            lblEditRating.Size = new Size(107, 32);
            lblEditRating.TabIndex = 6;
            lblEditRating.Text = "Рейтинг:";
            // 
            // numEditRating
            // 
            numEditRating.Location = new Point(204, 198);
            numEditRating.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
            numEditRating.Name = "numEditRating";
            numEditRating.Size = new Size(240, 39);
            numEditRating.TabIndex = 7;
            // 
            // lblEditGroup
            // 
            lblEditGroup.AutoSize = true;
            lblEditGroup.Location = new Point(15, 295);
            lblEditGroup.Name = "lblEditGroup";
            lblEditGroup.Size = new Size(95, 32);
            lblEditGroup.TabIndex = 8;
            lblEditGroup.Text = "Группа:";
            // 
            // numEditGroup
            // 
            numEditGroup.Location = new Point(204, 238);
            numEditGroup.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numEditGroup.Name = "numEditGroup";
            numEditGroup.Size = new Size(240, 39);
            numEditGroup.TabIndex = 9;
            // 
            // chkEditIsFemale
            // 
            chkEditIsFemale.AutoSize = true;
            chkEditIsFemale.Location = new Point(204, 285);
            chkEditIsFemale.Name = "chkEditIsFemale";
            chkEditIsFemale.Size = new Size(146, 36);
            chkEditIsFemale.TabIndex = 10;
            chkEditIsFemale.Text = "Женщина";
            chkEditIsFemale.UseVisualStyleBackColor = true;
            // 
            // chkEditLevelChecked
            // 
            chkEditLevelChecked.AutoSize = true;
            chkEditLevelChecked.Location = new Point(204, 325);
            chkEditLevelChecked.Name = "chkEditLevelChecked";
            chkEditLevelChecked.Size = new Size(211, 36);
            chkEditLevelChecked.TabIndex = 11;
            chkEditLevelChecked.Text = "Уровень пров.";
            chkEditLevelChecked.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(Players);
            tabControl1.Controls.Add(Cars);
            tabControl1.Controls.Add(tabGamesConfig);
            tabControl1.Controls.Add(tabGyms);
            tabControl1.Location = new Point(1140, 320);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1012, 1520);
            tabControl1.TabIndex = 13;
            // 
            // Players
            // 
            Players.Controls.Add(label1);
            Players.Controls.Add(filter);
            Players.Controls.Add(dataGridViewRating);
            Players.Controls.Add(gbEditPlayer);
            Players.Location = new Point(8, 46);
            Players.Name = "Players";
            Players.Padding = new Padding(3);
            Players.Size = new Size(996, 1037);
            Players.TabIndex = 0;
            Players.Text = "Игроки";
            Players.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(18, 18);
            label1.Name = "label1";
            label1.Size = new Size(94, 32);
            label1.TabIndex = 13;
            label1.Text = "Фильтр";
            // 
            // filter
            // 
            filter.Location = new Point(207, 15);
            filter.Name = "filter";
            filter.Size = new Size(488, 39);
            filter.TabIndex = 12;
            filter.TextChanged += filter_TextChanged;
            // 
            // Cars
            // 
            Cars.Controls.Add(btnSaveCars);
            Cars.Controls.Add(btnAddCar);
            Cars.Controls.Add(btnDeleteCar);
            Cars.Controls.Add(gbEditCar);
            Cars.Controls.Add(lblCarFilter);
            Cars.Controls.Add(txtCarFilter);
            Cars.Controls.Add(dgvCars);
            Cars.Controls.Add(lblCarsHeader);
            Cars.Location = new Point(8, 46);
            Cars.Name = "Cars";
            Cars.Padding = new Padding(3);
            Cars.Size = new Size(996, 1037);
            Cars.TabIndex = 1;
            Cars.Text = "Машины";
            Cars.UseVisualStyleBackColor = true;
            // 
            // tabGamesConfig
            // 
            tabGamesConfig.Controls.Add(btnSaveGamesJson);
            tabGamesConfig.Controls.Add(btnAddGame);
            tabGamesConfig.Controls.Add(btnDeleteGame);
            tabGamesConfig.Controls.Add(gbEditGame);
            tabGamesConfig.Controls.Add(dgvGames);
            tabGamesConfig.Location = new Point(8, 46);
            tabGamesConfig.Name = "tabGamesConfig";
            tabGamesConfig.Padding = new Padding(3);
            tabGamesConfig.Size = new Size(996, 1037);
            tabGamesConfig.TabIndex = 2;
            tabGamesConfig.Text = "Настройки игр";
            tabGamesConfig.UseVisualStyleBackColor = true;
            // 
            // dgvGames
            // 
            dgvGames.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvGames.Location = new Point(3, 3);
            dgvGames.MultiSelect = false;
            dgvGames.Name = "dgvGames";
            dgvGames.RowHeadersWidth = 82;
            dgvGames.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGames.Size = new Size(990, 250);
            dgvGames.TabIndex = 0;
            // 
            // gbEditGame
            // 
            gbEditGame.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gbEditGame.Controls.Add(lblTitle);
            gbEditGame.Controls.Add(txtTitle);
            gbEditGame.Controls.Add(lblGameDay);
            gbEditGame.Controls.Add(cmbGameDay);
            gbEditGame.Controls.Add(lblGameStartHour);
            gbEditGame.Controls.Add(cmbGameStartHour);
            gbEditGame.Controls.Add(lblGameStartMinute);
            gbEditGame.Controls.Add(cmbGameStartMinute);
            gbEditGame.Controls.Add(lblPullBeforeDay);
            gbEditGame.Controls.Add(numPullBeforeDay);
            gbEditGame.Controls.Add(lblPullHour);
            gbEditGame.Controls.Add(cmbPullHour);
            gbEditGame.Controls.Add(lblPullMinute);
            gbEditGame.Controls.Add(cmbPullMinute);
            gbEditGame.Controls.Add(chkActiveGame);
            gbEditGame.Controls.Add(chkRatingGame);
            gbEditGame.Controls.Add(chkTrainingGame);
            gbEditGame.Controls.Add(lblGymChoice);
            gbEditGame.Controls.Add(cmbGym);
            gbEditGame.Location = new Point(3, 260);
            gbEditGame.Name = "gbEditGame";
            gbEditGame.Size = new Size(990, 600);
            gbEditGame.TabIndex = 1;
            gbEditGame.TabStop = false;
            gbEditGame.Text = "Редактирование игры";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(15, 45);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(130, 32);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Заголовок:";
            // 
            // txtTitle
            // 
            txtTitle.Location = new Point(240, 42);
            txtTitle.Name = "txtTitle";
            txtTitle.Size = new Size(700, 39);
            txtTitle.TabIndex = 1;
            // 
            // lblGymChoice
            // 
            lblGymChoice.AutoSize = true;
            lblGymChoice.Location = new Point(15, 95);
            lblGymChoice.Name = "lblGymChoice";
            lblGymChoice.Size = new Size(134, 32);
            lblGymChoice.TabIndex = 4;
            lblGymChoice.Text = "Выбор зала:";
            // 
            // cmbGym
            // 
            cmbGym.Location = new Point(240, 92);
            cmbGym.Name = "cmbGym";
            cmbGym.Size = new Size(700, 40);
            cmbGym.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGym.TabIndex = 5;
            // 
            // cmbGameDay
            // 
            cmbGameDay.Location = new Point(240, 142);
            cmbGameDay.Name = "cmbGameDay";
            cmbGameDay.Size = new Size(200, 40);
            cmbGameDay.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGameDay.TabIndex = 7;
            // 
            // lblGameDay
            // 
            lblGameDay.AutoSize = true;
            lblGameDay.Location = new Point(15, 145);
            lblGameDay.Name = "lblGameDay";
            lblGameDay.Size = new Size(160, 32);
            lblGameDay.TabIndex = 6;
            lblGameDay.Text = "День игры:";
            // 
            // lblGameStartHour
            // 
            lblGameStartHour.AutoSize = true;
            lblGameStartHour.Location = new Point(15, 195);
            lblGameStartHour.Name = "lblGameStartHour";
            lblGameStartHour.Size = new Size(120, 32);
            lblGameStartHour.TabIndex = 8;
            lblGameStartHour.Text = "Час игры:";
            // 
            // cmbGameStartHour
            // 
            cmbGameStartHour.Location = new Point(240, 192);
            cmbGameStartHour.Name = "cmbGameStartHour";
            cmbGameStartHour.Size = new Size(120, 40);
            cmbGameStartHour.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGameStartHour.TabIndex = 9;
            // 
            // lblGameStartMinute
            // 
            lblGameStartMinute.AutoSize = true;
            lblGameStartMinute.Location = new Point(15, 245);
            lblGameStartMinute.Name = "lblGameStartMinute";
            lblGameStartMinute.Size = new Size(130, 32);
            lblGameStartMinute.TabIndex = 10;
            lblGameStartMinute.Text = "Мин. игры:";
            // 
            // cmbGameStartMinute
            // 
            cmbGameStartMinute.Location = new Point(240, 242);
            cmbGameStartMinute.Name = "cmbGameStartMinute";
            cmbGameStartMinute.Size = new Size(120, 40);
            cmbGameStartMinute.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGameStartMinute.TabIndex = 11;
            // 
            // lblPullBeforeDay
            // 
            lblPullBeforeDay.AutoSize = true;
            lblPullBeforeDay.Location = new Point(500, 145);
            lblPullBeforeDay.Name = "lblPullBeforeDay";
            lblPullBeforeDay.Size = new Size(190, 32);
            lblPullBeforeDay.TabIndex = 12;
            lblPullBeforeDay.Text = "За ск. дней опрос:";
            // 
            // numPullBeforeDay
            // 
            numPullBeforeDay.Location = new Point(750, 142);
            numPullBeforeDay.Name = "numPullBeforeDay";
            numPullBeforeDay.Size = new Size(120, 39);
            numPullBeforeDay.TabIndex = 13;
            // 
            // lblPullHour
            // 
            lblPullHour.AutoSize = true;
            lblPullHour.Location = new Point(500, 195);
            lblPullHour.Name = "lblPullHour";
            lblPullHour.Size = new Size(140, 32);
            lblPullHour.TabIndex = 14;
            lblPullHour.Text = "Час опроса:";
            // 
            // cmbPullHour
            // 
            cmbPullHour.Location = new Point(750, 192);
            cmbPullHour.Name = "cmbPullHour";
            cmbPullHour.Size = new Size(120, 40);
            cmbPullHour.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPullHour.TabIndex = 15;
            // 
            // lblPullMinute
            // 
            lblPullMinute.AutoSize = true;
            lblPullMinute.Location = new Point(500, 245);
            lblPullMinute.Name = "lblPullMinute";
            lblPullMinute.Size = new Size(150, 32);
            lblPullMinute.TabIndex = 16;
            lblPullMinute.Text = "Мин. опроса:";
            // 
            // cmbPullMinute
            // 
            cmbPullMinute.Location = new Point(750, 242);
            cmbPullMinute.Name = "cmbPullMinute";
            cmbPullMinute.Size = new Size(120, 40);
            cmbPullMinute.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPullMinute.TabIndex = 17;
            // 
            // chkActiveGame
            // 
            chkActiveGame.AutoSize = true;
            chkActiveGame.Location = new Point(15, 295);
            chkActiveGame.Name = "chkActiveGame";
            chkActiveGame.Size = new Size(200, 36);
            chkActiveGame.TabIndex = 18;
            chkActiveGame.Text = "Активная игра";
            chkActiveGame.UseVisualStyleBackColor = true;
            // 
            // chkRatingGame
            // 
            chkRatingGame.AutoSize = true;
            chkRatingGame.Location = new Point(240, 295);
            chkRatingGame.Name = "chkRatingGame";
            chkRatingGame.Size = new Size(200, 36);
            chkRatingGame.TabIndex = 19;
            chkRatingGame.Text = "Рейтинговая";
            chkRatingGame.UseVisualStyleBackColor = true;
            // 
            // chkTrainingGame
            // 
            chkTrainingGame.AutoSize = true;
            chkTrainingGame.Location = new Point(460, 295);
            chkTrainingGame.Name = "chkTrainingGame";
            chkTrainingGame.Size = new Size(200, 36);
            chkTrainingGame.TabIndex = 22;
            chkTrainingGame.Text = "Тренировка";
            chkTrainingGame.UseVisualStyleBackColor = true;
            btnAddGame.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAddGame.Location = new Point(15, 900);
            btnAddGame.Name = "btnAddGame";
            btnAddGame.Size = new Size(300, 46);
            btnAddGame.TabIndex = 3;
            btnAddGame.Text = "Добавить игру";
            btnAddGame.UseVisualStyleBackColor = true;
            btnAddGame.Click += btnAddGame_Click;
            // 
            // btnDeleteGame
            // 
            btnDeleteGame.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeleteGame.Location = new Point(345, 900);
            btnDeleteGame.Name = "btnDeleteGame";
            btnDeleteGame.Size = new Size(300, 46);
            btnDeleteGame.TabIndex = 4;
            btnDeleteGame.Text = "Удалить игру";
            btnDeleteGame.UseVisualStyleBackColor = true;
            btnDeleteGame.Click += btnDeleteGame_Click;
            // 
            // btnSaveGamesJson
            // 
            btnSaveGamesJson.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSaveGamesJson.Location = new Point(675, 900);
            btnSaveGamesJson.Name = "btnSaveGamesJson";
            btnSaveGamesJson.Size = new Size(300, 46);
            btnSaveGamesJson.TabIndex = 1;
            btnSaveGamesJson.Text = "Сохранить и применить";
            btnSaveGamesJson.UseVisualStyleBackColor = true;
            btnSaveGamesJson.Click += btnSaveGamesJson_Click;
            // 
            // tabGyms
            // 
            tabGyms.Controls.Add(btnSaveGyms);
            tabGyms.Controls.Add(btnAddGym);
            tabGyms.Controls.Add(btnDeleteGym);
            tabGyms.Controls.Add(gbEditGym);
            tabGyms.Controls.Add(dgvGyms);
            tabGyms.Location = new Point(8, 46);
            tabGyms.Name = "tabGyms";
            tabGyms.Padding = new Padding(3);
            tabGyms.Size = new Size(996, 1037);
            tabGyms.TabIndex = 3;
            tabGyms.Text = "Справочник залов";
            tabGyms.UseVisualStyleBackColor = true;
            // 
            // dgvGyms
            // 
            dgvGyms.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvGyms.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvGyms.Location = new Point(3, 3);
            dgvGyms.MultiSelect = false;
            dgvGyms.Name = "dgvGyms";
            dgvGyms.ReadOnly = true;
            dgvGyms.RowHeadersWidth = 82;
            dgvGyms.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGyms.Size = new Size(990, 250);
            dgvGyms.TabIndex = 0;
            // 
            // gbEditGym
            // 
            gbEditGym.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gbEditGym.Controls.Add(lblGymId);
            gbEditGym.Controls.Add(numGymId);
            gbEditGym.Controls.Add(lblGymName);
            gbEditGym.Controls.Add(txtGymName);
            gbEditGym.Controls.Add(lblGymLocation);
            gbEditGym.Controls.Add(txtGymLocation);
            gbEditGym.Location = new Point(3, 260);
            gbEditGym.Name = "gbEditGym";
            gbEditGym.Size = new Size(990, 600);
            gbEditGym.TabIndex = 1;
            gbEditGym.TabStop = false;
            gbEditGym.Text = "Редактирование зала";
            // 
            // lblGymId
            // 
            lblGymId.AutoSize = true;
            lblGymId.Location = new Point(15, 45);
            lblGymId.Name = "lblGymId";
            lblGymId.Size = new Size(50, 32);
            lblGymId.TabIndex = 0;
            lblGymId.Text = "ID:";
            // 
            // numGymId
            // 
            numGymId.Location = new Point(240, 42);
            numGymId.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numGymId.Name = "numGymId";
            numGymId.Size = new Size(120, 39);
            numGymId.TabIndex = 1;
            // 
            // lblGymName
            // 
            lblGymName.AutoSize = true;
            lblGymName.Location = new Point(15, 95);
            lblGymName.Name = "lblGymName";
            lblGymName.Size = new Size(130, 32);
            lblGymName.TabIndex = 2;
            lblGymName.Text = "Название:";
            // 
            // txtGymName
            // 
            txtGymName.Location = new Point(240, 92);
            txtGymName.Name = "txtGymName";
            txtGymName.Size = new Size(700, 39);
            txtGymName.TabIndex = 3;
            // 
            // lblGymLocation
            // 
            lblGymLocation.AutoSize = true;
            lblGymLocation.Location = new Point(15, 145);
            lblGymLocation.Name = "lblGymLocation";
            lblGymLocation.Size = new Size(110, 32);
            lblGymLocation.TabIndex = 4;
            lblGymLocation.Text = "Локация (URL):";
            // 
            // txtGymLocation
            // 
            txtGymLocation.Location = new Point(240, 142);
            txtGymLocation.Name = "txtGymLocation";
            txtGymLocation.Size = new Size(700, 39);
            txtGymLocation.TabIndex = 5;
            // 
            // btnAddGym
            // 
            btnAddGym.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAddGym.Location = new Point(15, 900);
            btnAddGym.Name = "btnAddGym";
            btnAddGym.Size = new Size(300, 46);
            btnAddGym.TabIndex = 3;
            btnAddGym.Text = "Добавить зал";
            btnAddGym.UseVisualStyleBackColor = true;
            btnAddGym.Click += btnAddGym_Click;
            // 
            // btnDeleteGym
            // 
            btnDeleteGym.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnDeleteGym.Location = new Point(345, 900);
            btnDeleteGym.Name = "btnDeleteGym";
            btnDeleteGym.Size = new Size(300, 46);
            btnDeleteGym.TabIndex = 4;
            btnDeleteGym.Text = "Удалить зал";
            btnDeleteGym.UseVisualStyleBackColor = true;
            btnDeleteGym.Click += btnDeleteGym_Click;
            // 
            // btnSaveGyms
            // 
            btnSaveGyms.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSaveGyms.Location = new Point(675, 900);
            btnSaveGyms.Name = "btnSaveGyms";
            btnSaveGyms.Size = new Size(300, 46);
            btnSaveGyms.TabIndex = 1;
            btnSaveGyms.Text = "Сохранить";
            btnSaveGyms.UseVisualStyleBackColor = true;
            btnSaveGyms.Click += btnSaveGyms_Click;
            // 
            // dataGridViewCarStops
            // 
            dataGridViewCarStops.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewCarStops.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCarStops.Dock = DockStyle.None;
            dataGridViewCarStops.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewCarStops.Location = new Point(15, 175);
            dataGridViewCarStops.MultiSelect = false;
            dataGridViewCarStops.Name = "dataGridViewCarStops";
            dataGridViewCarStops.RowHeadersWidth = 82;
            dataGridViewCarStops.RowTemplate.Height = 30;
            dataGridViewCarStops.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewCarStops.Size = new Size(960, 325);
            dataGridViewCarStops.TabIndex = 15;
            // 
            // lblStopsHeader
            // 
            lblStopsHeader.AutoSize = true;
            lblStopsHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStopsHeader.Location = new Point(15, 140);
            lblStopsHeader.Name = "lblStopsHeader";
            lblStopsHeader.Size = new Size(141, 32);
            lblStopsHeader.TabIndex = 22;
            lblStopsHeader.Text = "Остановки:";
            // 
            // dgvCars
            // 
            dgvCars.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvCars.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCars.Dock = DockStyle.None;
            dgvCars.EditMode = DataGridViewEditMode.EditProgrammatically;
            dgvCars.Location = new Point(3, 90);
            dgvCars.MultiSelect = false;
            dgvCars.Name = "dgvCars";
            dgvCars.ReadOnly = true;
            dgvCars.RowHeadersWidth = 82;
            dgvCars.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCars.Size = new Size(990, 160);
            dgvCars.TabIndex = 14;
            dgvCars.SelectionChanged += dgvCars_SelectionChanged;
            // 
            // lblCarFilter
            // 
            lblCarFilter.AutoSize = true;
            lblCarFilter.Location = new Point(3, 45);
            lblCarFilter.Name = "lblCarFilter";
            lblCarFilter.Size = new Size(100, 32);
            lblCarFilter.TabIndex = 23;
            lblCarFilter.Text = "Фильтр:";
            // 
            // txtCarFilter
            // 
            txtCarFilter.Location = new Point(140, 42);
            txtCarFilter.Name = "txtCarFilter";
            txtCarFilter.Size = new Size(400, 39);
            txtCarFilter.TabIndex = 24;
            txtCarFilter.TextChanged += txtCarFilter_TextChanged;
            // 
            // lblCarsHeader
            // 
            lblCarsHeader.AutoSize = true;
            lblCarsHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCarsHeader.Location = new Point(3, 5);
            lblCarsHeader.Name = "lblCarsHeader";
            lblCarsHeader.Size = new Size(122, 32);
            lblCarsHeader.TabIndex = 21;
            lblCarsHeader.Text = "Машины:";
            // 
            // gbEditCar
            // 
            gbEditCar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gbEditCar.Controls.Add(lblStopsHeader);
            gbEditCar.Controls.Add(gbEditStop);
            gbEditCar.Controls.Add(dataGridViewCarStops);
            gbEditCar.Controls.Add(cmbCarPlaceCount);
            gbEditCar.Controls.Add(lblCarPlaceCount);
            gbEditCar.Controls.Add(txtCarFirstName);
            gbEditCar.Controls.Add(lblCarFirstName);
            gbEditCar.Controls.Add(txtCarName);
            gbEditCar.Controls.Add(lblCarName);
            gbEditCar.Controls.Add(cmbCarIdPlayer);
            gbEditCar.Controls.Add(lblCarIdPlayer);
            gbEditCar.Location = new Point(3, 260);
            gbEditCar.Name = "gbEditCar";
            gbEditCar.Size = new Size(990, 680);
            gbEditCar.TabIndex = 16;
            gbEditCar.TabStop = false;
            gbEditCar.Text = "Редактирование машины";
            // 
            // lblCarIdPlayer
            // 
            lblCarIdPlayer.AutoSize = true;
            lblCarIdPlayer.Location = new Point(15, 45);
            lblCarIdPlayer.Name = "lblCarIdPlayer";
            lblCarIdPlayer.Size = new Size(110, 32);
            lblCarIdPlayer.TabIndex = 0;
            lblCarIdPlayer.Text = "Игрок:";
            // 
            // cmbCarIdPlayer
            // 
            cmbCarIdPlayer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbCarIdPlayer.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbCarIdPlayer.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbCarIdPlayer.FormattingEnabled = true;
            cmbCarIdPlayer.Location = new Point(130, 42);
            cmbCarIdPlayer.Name = "cmbCarIdPlayer";
            cmbCarIdPlayer.Size = new Size(820, 40);
            cmbCarIdPlayer.TabIndex = 1;
            cmbCarIdPlayer.SelectedIndexChanged += cmbCarIdPlayer_SelectedIndexChanged;
            // 
            // lblCarName
            // 
            lblCarName.AutoSize = true;
            lblCarName.Location = new Point(320, 45);
            lblCarName.Name = "lblCarName";
            lblCarName.Size = new Size(60, 32);
            lblCarName.TabIndex = 2;
            lblCarName.Text = "Ник:";
            lblCarName.Visible = false;
            // 
            // txtCarName
            // 
            txtCarName.Location = new Point(385, 42);
            txtCarName.Name = "txtCarName";
            txtCarName.ReadOnly = true;
            txtCarName.Size = new Size(200, 39);
            txtCarName.TabIndex = 3;
            txtCarName.Visible = false;
            // 
            // lblCarFirstName
            // 
            lblCarFirstName.AutoSize = true;
            lblCarFirstName.Location = new Point(600, 45);
            lblCarFirstName.Name = "lblCarFirstName";
            lblCarFirstName.Size = new Size(66, 32);
            lblCarFirstName.TabIndex = 4;
            lblCarFirstName.Text = "Имя:";
            lblCarFirstName.Visible = false;
            // 
            // txtCarFirstName
            // 
            txtCarFirstName.Location = new Point(670, 42);
            txtCarFirstName.Name = "txtCarFirstName";
            txtCarFirstName.ReadOnly = true;
            txtCarFirstName.Size = new Size(200, 39);
            txtCarFirstName.TabIndex = 5;
            txtCarFirstName.Visible = false;
            // 
            // lblCarPlaceCount
            // 
            lblCarPlaceCount.AutoSize = true;
            lblCarPlaceCount.Location = new Point(15, 95);
            lblCarPlaceCount.Name = "lblCarPlaceCount";
            lblCarPlaceCount.Size = new Size(76, 32);
            lblCarPlaceCount.TabIndex = 6;
            lblCarPlaceCount.Text = "Мест:";
            // 
            // cmbCarPlaceCount
            // 
            cmbCarPlaceCount.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCarPlaceCount.Location = new Point(130, 92);
            cmbCarPlaceCount.Name = "cmbCarPlaceCount";
            cmbCarPlaceCount.Size = new Size(120, 40);
            cmbCarPlaceCount.TabIndex = 7;
            // 
            // btnAddCar
            // 
            btnAddCar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAddCar.Location = new Point(15, 985);
            btnAddCar.Name = "btnAddCar";
            btnAddCar.Size = new Size(300, 46);
            btnAddCar.TabIndex = 17;
            btnAddCar.Text = "Добавить машину";
            btnAddCar.UseVisualStyleBackColor = true;
            btnAddCar.Click += btnAddCar_Click;
            // 
            // btnDeleteCar
            // 
            btnDeleteCar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeleteCar.Location = new Point(345, 985);
            btnDeleteCar.Name = "btnDeleteCar";
            btnDeleteCar.Size = new Size(300, 46);
            btnDeleteCar.TabIndex = 18;
            btnDeleteCar.Text = "Удалить машину";
            btnDeleteCar.UseVisualStyleBackColor = true;
            btnDeleteCar.Click += btnDeleteCar_Click;
            // 
            // btnSaveCars
            // 
            btnSaveCars.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSaveCars.Location = new Point(675, 985);
            btnSaveCars.Name = "btnSaveCars";
            btnSaveCars.Size = new Size(300, 46);
            btnSaveCars.TabIndex = 19;
            btnSaveCars.Text = "Сохранить";
            btnSaveCars.UseVisualStyleBackColor = true;
            btnSaveCars.Click += btnSaveCars_Click;
            // 
            // gbEditStop
            // 
            gbEditStop.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gbEditStop.Controls.Add(btnOpenStopLink);
            gbEditStop.Controls.Add(btnAddStop);
            gbEditStop.Controls.Add(btnDeleteStop);
            gbEditStop.Controls.Add(numStopMinBefore);
            gbEditStop.Controls.Add(lblStopMinBefore);
            gbEditStop.Controls.Add(txtStopLink);
            gbEditStop.Controls.Add(lblStopLink);
            gbEditStop.Controls.Add(txtStopName);
            gbEditStop.Controls.Add(lblStopName);
            gbEditStop.Location = new Point(10, 470);
            gbEditStop.Name = "gbEditStop";
            gbEditStop.Size = new Size(970, 200);
            gbEditStop.TabIndex = 20;
            gbEditStop.TabStop = false;
            gbEditStop.Text = "Редактирование остановки";
            // 
            // lblStopName
            // 
            lblStopName.AutoSize = true;
            lblStopName.Location = new Point(15, 45);
            lblStopName.Name = "lblStopName";
            lblStopName.Size = new Size(125, 32);
            lblStopName.TabIndex = 0;
            lblStopName.Text = "Название:";
            // 
            // txtStopName
            // 
            txtStopName.Location = new Point(150, 42);
            txtStopName.Name = "txtStopName";
            txtStopName.Size = new Size(300, 39);
            txtStopName.TabIndex = 1;
            // 
            // lblStopMinBefore
            // 
            lblStopMinBefore.AutoSize = true;
            lblStopMinBefore.Location = new Point(470, 45);
            lblStopMinBefore.Name = "lblStopMinBefore";
            lblStopMinBefore.Size = new Size(200, 32);
            lblStopMinBefore.TabIndex = 4;
            lblStopMinBefore.Text = "Мин. до:";
            // 
            // numStopMinBefore
            // 
            numStopMinBefore.Location = new Point(590, 42);
            numStopMinBefore.Name = "numStopMinBefore";
            numStopMinBefore.Size = new Size(100, 39);
            numStopMinBefore.TabIndex = 5;
            // 
            // lblStopLink
            // 
            lblStopLink.AutoSize = true;
            lblStopLink.Location = new Point(15, 95);
            lblStopLink.Name = "lblStopLink";
            lblStopLink.Size = new Size(110, 32);
            lblStopLink.TabIndex = 2;
            lblStopLink.Text = "Ссылка:";
            // 
            // txtStopLink
            // 
            txtStopLink.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtStopLink.Location = new Point(140, 92);
            txtStopLink.Name = "txtStopLink";
            txtStopLink.Size = new Size(740, 39);
            txtStopLink.TabIndex = 3;
            // 
            // btnOpenStopLink
            // 
            btnOpenStopLink.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOpenStopLink.Location = new Point(890, 90);
            btnOpenStopLink.Name = "btnOpenStopLink";
            btnOpenStopLink.Size = new Size(60, 42);
            btnOpenStopLink.TabIndex = 4;
            btnOpenStopLink.Text = "🌐";
            btnOpenStopLink.UseVisualStyleBackColor = true;
            btnOpenStopLink.Click += btnOpenStopLink_Click;
            // 
            // btnAddStop
            // 
            btnAddStop.Location = new Point(15, 145);
            btnAddStop.Name = "btnAddStop";
            btnAddStop.Size = new Size(300, 42);
            btnAddStop.TabIndex = 6;
            btnAddStop.Text = "Добавить остановку";
            btnAddStop.UseVisualStyleBackColor = true;
            btnAddStop.Click += btnAddStop_Click;
            // 
            // btnDeleteStop
            // 
            btnDeleteStop.Location = new Point(345, 145);
            btnDeleteStop.Name = "btnDeleteStop";
            btnDeleteStop.Size = new Size(300, 42);
            btnDeleteStop.TabIndex = 7;
            btnDeleteStop.Text = "Удалить остановку";
            btnDeleteStop.UseVisualStyleBackColor = true;
            btnDeleteStop.Click += btnDeleteStop_Click;
            // 
            // getCars
            // 
            getCars.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            getCars.Location = new Point(12, 1920);
            getCars.Name = "getCars";
            getCars.Size = new Size(320, 60);
            getCars.TabIndex = 15;
            getCars.Text = "Табло запус";
            getCars.UseVisualStyleBackColor = true;
            getCars.Click += getCars_Click;
            // 
            // button9
            // 
            button9.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button9.Location = new Point(12, 1850);
            button9.Name = "button9";
            button9.Size = new Size(320, 60);
            button9.TabIndex = 16;
            button9.Text = "Опрос после игры";
            button9.UseVisualStyleBackColor = true;
            button9.Click += button9_Click;
            // 
            // button10
            // 
            button10.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button10.Location = new Point(340, 1850);
            button10.Name = "button10";
            button10.Size = new Size(320, 60);
            button10.TabIndex = 17;
            button10.Text = "Статистика";
            button10.UseVisualStyleBackColor = true;
            button10.Click += getStat;
            // 
            // button11
            // 
            button11.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button11.Location = new Point(340, 1920);
            button11.Name = "button11";
            button11.Size = new Size(320, 60);
            button11.TabIndex = 18;
            button11.Text = "Приглашение игроков";
            button11.UseVisualStyleBackColor = true;
            button11.Click += button11_ClickAsync;
            // 
            // del_afterGameSurvey
            // 
            del_afterGameSurvey.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            del_afterGameSurvey.Location = new Point(670, 1920);
            del_afterGameSurvey.Name = "del_afterGameSurvey";
            del_afterGameSurvey.Size = new Size(320, 60);
            del_afterGameSurvey.TabIndex = 19;
            del_afterGameSurvey.Text = "Del Опрос после игры";
            del_afterGameSurvey.UseVisualStyleBackColor = true;
            del_afterGameSurvey.Click += button12_Click;
            // 
            // btnUpdateSummary
            // 
            btnUpdateSummary.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnUpdateSummary.Location = new Point(670, 1850);
            btnUpdateSummary.Name = "btnUpdateSummary";
            btnUpdateSummary.Size = new Size(320, 60);
            btnUpdateSummary.TabIndex = 20;
            btnUpdateSummary.Text = "Обновить итоги (звезды)";
            btnUpdateSummary.UseVisualStyleBackColor = true;
            btnUpdateSummary.Click += btnUpdateSummary_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.None;
            components = new System.ComponentModel.Container();
            ClientSize = new System.Drawing.Size(1600, 900);
            Controls.Add(btnUpdateSummary);
            Controls.Add(del_afterGameSurvey);
            Controls.Add(button11);

            Controls.Add(button10);
            Controls.Add(button9);
            Controls.Add(getCars);
            Controls.Add(tabControl1);
            Controls.Add(button8);
            Controls.Add(button7);
            Controls.Add(button6);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(dataGridViewPlayers);
            Controls.Add(dataGridViewPoll);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(btnCreatePoll);
            Margin = new Padding(5);
            Text = "Волейбольный бот";
            this.WindowState = FormWindowState.Maximized;
            Activated += Form1_Activated;
            Load += Form1_Load;
            Resize += Form1_Resize;
            ((System.ComponentModel.ISupportInitialize)dataGridViewPoll).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewPlayers).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewRating).EndInit();
            ((System.ComponentModel.ISupportInitialize)numEditRating).EndInit();
            ((System.ComponentModel.ISupportInitialize)numEditGroup).EndInit();
            tabControl1.ResumeLayout(false);
            Players.ResumeLayout(false);
            Players.PerformLayout();
            Cars.ResumeLayout(false);
            gbEditCar.ResumeLayout(false);
            gbEditCar.PerformLayout();
            gbEditStop.ResumeLayout(false);
            gbEditStop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numStopMinBefore).EndInit();
            tabGamesConfig.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvGames).EndInit();
            gbEditGame.ResumeLayout(false);
            gbEditGame.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numPullBeforeDay).EndInit();
            tabGyms.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvGyms).EndInit();
            gbEditGym.ResumeLayout(false);
            gbEditGym.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numGymId).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewCarStops).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvCars).EndInit();
            ((System.ComponentModel.ISupportInitialize)carStopsBindingSource).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Timer minuteTimer;
        private Button btnCreatePoll;
        private Button button1;
        private Button button2;
        private Button button3;
        private DataGridView dataGridViewPoll;
        private DataGridView dataGridViewPlayers;
        private Button button4;
        private Button button5;
        private Button button6;
        private Button button7;
        private Button button8;
        private DataGridView dataGridViewRating;
        private TabControl tabControl1;
        private TabPage Players;
        private TabPage Cars;
        private TabPage tabGamesConfig;
        private DataGridView dgvGames;
        private GroupBox gbEditGame;
        private Label lblTitle;
        private TextBox txtTitle;
        private Label lblGameDay;
        private ComboBox cmbGameDay;
        private Label lblGameStartHour;
        private ComboBox cmbGameStartHour;
        private Label lblGameStartMinute;
        private ComboBox cmbGameStartMinute;
        private Label lblPullBeforeDay;
        private NumericUpDown numPullBeforeDay;
        private Label lblPullHour;
        private ComboBox cmbPullHour;
        private Label lblPullMinute;
        private ComboBox cmbPullMinute;
        private CheckBox chkActiveGame;
        private CheckBox chkRatingGame;
        private Button btnAddGame;
        private Button btnDeleteGame;
        private Button btnSaveGamesJson;
        private CheckBox chkTrainingGame;
        private DataGridView dgvCars;
        private DataGridView dataGridViewCarStops;
        private Button getCars;
        private BindingSource carStopsBindingSource;
        private Button button9;
        private Button button10;
        private Button button11;
        private Label label1;
        private TextBox filter;
        private Button del_afterGameSurvey;
        private Button btnUpdateSummary;
        private TabPage tabGyms;
        private Label lblGymChoice;
        private ComboBox cmbGym;
        private DataGridView dgvGyms;
        private GroupBox gbEditGym;
        private Label lblGymId;
        private NumericUpDown numGymId;
        private Label lblGymName;
        private TextBox txtGymName;
        private Label lblGymLocation;
        private TextBox txtGymLocation;
        private Button btnAddGym;
        private Button btnDeleteGym;
        private Button btnSaveGyms;
        private GroupBox gbEditPlayer;

        private Label lblEditName;
        private TextBox txtEditName;
        private Label lblEditFirstName;
        private TextBox txtEditFirstName;
        private Label lblEditNormalName;
        private TextBox txtEditNormalName;
        private Label lblEditRating;
        private NumericUpDown numEditRating;
        private Label lblEditGroup;
        private NumericUpDown numEditGroup;
        private CheckBox chkEditIsFemale;
        private CheckBox chkEditLevelChecked;
        private Label lblEditId;
        private TextBox txtEditId;
        private GroupBox gbEditCar;
        private Label lblCarIdPlayer;
        private ComboBox cmbCarIdPlayer;
        private Label lblCarName;
        private TextBox txtCarName;
        private Label lblCarFirstName;
        private TextBox txtCarFirstName;
        private Label lblCarPlaceCount;
        private ComboBox cmbCarPlaceCount;
        private Button btnAddCar;
        private Button btnDeleteCar;
        private Button btnSaveCars;

        private GroupBox gbEditStop;
        private Button btnOpenStopLink;
        private Label lblStopName;
        private TextBox txtStopName;
        private Label lblStopLink;
        private TextBox txtStopLink;
        private Label lblStopMinBefore;
        private NumericUpDown numStopMinBefore;
        private Button btnAddStop;
        private Button btnDeleteStop;
        private Label lblCarsHeader;
        private Label lblStopsHeader;
        private Label lblCarFilter;
        private TextBox txtCarFilter;
    }
}