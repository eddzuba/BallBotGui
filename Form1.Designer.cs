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
            ((System.ComponentModel.ISupportInitialize)dataGridViewPoll).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewPlayers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewRating).BeginInit();
            tabControl1.SuspendLayout();
            Players.SuspendLayout();
            Cars.SuspendLayout();
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
            btnCreatePoll.Location = new Point(1778, 1126);
            btnCreatePoll.Name = "btnCreatePoll";
            btnCreatePoll.Size = new Size(309, 46);
            btnCreatePoll.TabIndex = 0;
            btnCreatePoll.Text = "Запуск голосовалки";
            btnCreatePoll.UseVisualStyleBackColor = true;
            btnCreatePoll.Click += btnCreatePoll_press;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button1.Location = new Point(1148, 1126);
            button1.Name = "button1";
            button1.Size = new Size(309, 46);
            button1.TabIndex = 1;
            button1.Text = "Сохранение состояния";
            button1.UseVisualStyleBackColor = true;
            button1.Click += testSave;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button2.Location = new Point(1778, 1230);
            button2.Name = "button2";
            button2.Size = new Size(309, 46);
            button2.TabIndex = 2;
            button2.Text = "Загрузить состояния";
            button2.UseVisualStyleBackColor = true;
            button2.Click += RestoreState;
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button3.Location = new Point(1469, 1178);
            button3.Name = "button3";
            button3.Size = new Size(302, 46);
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
            dataGridViewPoll.Location = new Point(12, 12);
            dataGridViewPoll.MultiSelect = false;
            dataGridViewPoll.Name = "dataGridViewPoll";
            dataGridViewPoll.RowHeadersWidth = 82;
            dataGridViewPoll.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewPoll.Size = new Size(1113, 269);
            dataGridViewPoll.TabIndex = 4;
            // 
            // dataGridViewPlayers
            // 
            dataGridViewPlayers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            dataGridViewPlayers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewPlayers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewPlayers.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewPlayers.Location = new Point(12, 287);
            dataGridViewPlayers.MultiSelect = false;
            dataGridViewPlayers.Name = "dataGridViewPlayers";
            dataGridViewPlayers.RowHeadersWidth = 82;
            dataGridViewPlayers.RowTemplate.Height = 30;
            dataGridViewPlayers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewPlayers.Size = new Size(983, 930);
            dataGridViewPlayers.TabIndex = 5;
            dataGridViewPlayers.SelectionChanged += onPlayerSelect;
            // 
            // button4
            // 
            button4.Location = new Point(1001, 287);
            button4.Name = "button4";
            button4.Size = new Size(124, 206);
            button4.TabIndex = 6;
            button4.Text = "UP";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // button5
            // 
            button5.Location = new Point(1001, 499);
            button5.Name = "button5";
            button5.Size = new Size(124, 291);
            button5.TabIndex = 7;
            button5.Text = "DOWN";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // button6
            // 
            button6.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button6.Location = new Point(1468, 1230);
            button6.Name = "button6";
            button6.Size = new Size(304, 46);
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
            dataGridViewRating.AllowUserToAddRows = false;
            dataGridViewRating.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewRating.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewRating.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewRating.Location = new Point(3, 70);
            dataGridViewRating.MultiSelect = false;
            dataGridViewRating.Name = "dataGridViewRating";
            dataGridViewRating.RowHeadersWidth = 82;
            dataGridViewRating.RowTemplate.Height = 30;
            dataGridViewRating.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewRating.Size = new Size(990, 961);
            dataGridViewRating.TabIndex = 11;
            dataGridViewRating.CellEndEdit += dataGridViewRating_CellEndEdit;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(Players);
            tabControl1.Controls.Add(Cars);
            tabControl1.Location = new Point(1140, 12);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1012, 1091);
            tabControl1.TabIndex = 13;
            // 
            // Players
            // 
            Players.Controls.Add(label1);
            Players.Controls.Add(filter);
            Players.Controls.Add(dataGridViewRating);
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
            Cars.Controls.Add(dataGridViewCarStops);
            Cars.Controls.Add(dgvCars);
            Cars.Location = new Point(8, 46);
            Cars.Name = "Cars";
            Cars.Padding = new Padding(3);
            Cars.Size = new Size(996, 1037);
            Cars.TabIndex = 1;
            Cars.Text = "Машины";
            Cars.UseVisualStyleBackColor = true;
            // 
            // dataGridViewCarStops
            // 
            dataGridViewCarStops.AllowUserToOrderColumns = true;
            dataGridViewCarStops.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCarStops.Dock = DockStyle.Bottom;
            dataGridViewCarStops.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewCarStops.Location = new Point(3, 575);
            dataGridViewCarStops.MultiSelect = false;
            dataGridViewCarStops.Name = "dataGridViewCarStops";
            dataGridViewCarStops.RowHeadersWidth = 82;
            dataGridViewCarStops.RowTemplate.Height = 30;
            dataGridViewCarStops.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewCarStops.Size = new Size(990, 459);
            dataGridViewCarStops.TabIndex = 15;
            // 
            // dgvCars
            // 
            dgvCars.AllowUserToOrderColumns = true;
            dgvCars.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCars.Dock = DockStyle.Top;
            dgvCars.Location = new Point(3, 3);
            dgvCars.MultiSelect = false;
            dgvCars.Name = "dgvCars";
            dgvCars.RowHeadersWidth = 82;
            dgvCars.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCars.Size = new Size(990, 489);
            dgvCars.TabIndex = 14;
            dgvCars.SelectionChanged += dgvCars_SelectionChanged;
            // 
            // getCars
            // 
            getCars.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            getCars.Location = new Point(1151, 1230);
            getCars.Name = "getCars";
            getCars.Size = new Size(303, 46);
            getCars.TabIndex = 15;
            getCars.Text = "Табло запус";
            getCars.UseVisualStyleBackColor = true;
            getCars.Click += getCars_Click;
            // 
            // button9
            // 
            button9.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button9.Location = new Point(1151, 1178);
            button9.Name = "button9";
            button9.Size = new Size(303, 46);
            button9.TabIndex = 16;
            button9.Text = "Опрос после игры";
            button9.UseVisualStyleBackColor = true;
            button9.Click += button9_Click;
            // 
            // button10
            // 
            button10.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button10.Location = new Point(1145, 1291);
            button10.Name = "button10";
            button10.Size = new Size(309, 46);
            button10.TabIndex = 17;
            button10.Text = "Статистика";
            button10.UseVisualStyleBackColor = true;
            button10.Click += getStat;
            // 
            // button11
            // 
            button11.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button11.Location = new Point(1460, 1291);
            button11.Name = "button11";
            button11.Size = new Size(309, 46);
            button11.TabIndex = 18;
            button11.Text = "Приглашение игроков";
            button11.UseVisualStyleBackColor = true;
            button11.Click += button11_ClickAsync;
            // 
            // del_afterGameSurvey
            // 
            del_afterGameSurvey.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            del_afterGameSurvey.Location = new Point(1784, 1291);
            del_afterGameSurvey.Name = "del_afterGameSurvey";
            del_afterGameSurvey.Size = new Size(303, 46);
            del_afterGameSurvey.TabIndex = 19;
            del_afterGameSurvey.Text = "Del Опрос после игры";
            del_afterGameSurvey.UseVisualStyleBackColor = true;
            del_afterGameSurvey.Click += button12_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2164, 1374);
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
            Name = "Form1";
            Text = "Волейбольный бот";
            Activated += Form1_Activated;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridViewPoll).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewPlayers).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewRating).EndInit();
            tabControl1.ResumeLayout(false);
            Players.ResumeLayout(false);
            Players.PerformLayout();
            Cars.ResumeLayout(false);
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
    }
}