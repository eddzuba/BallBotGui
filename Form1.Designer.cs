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
            button9 = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewPoll).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewPlayers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewRating).BeginInit();
            SuspendLayout();
            // 
            // minuteTimer
            // 
            minuteTimer.Enabled = true;
            minuteTimer.Interval = 60000;
            minuteTimer.Tick += minuteTimer_Tick;
            // 
            // btnCreatePoll
            // 
            btnCreatePoll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCreatePoll.Location = new Point(1780, 910);
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
            button1.Location = new Point(1780, 962);
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
            button2.Location = new Point(1780, 1014);
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
            button3.Location = new Point(1471, 962);
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
            dataGridViewPoll.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewPoll.Location = new Point(12, 12);
            dataGridViewPoll.MultiSelect = false;
            dataGridViewPoll.Name = "dataGridViewPoll";
            dataGridViewPoll.RowHeadersWidth = 82;
            dataGridViewPoll.RowTemplate.Height = 41;
            dataGridViewPoll.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewPoll.Size = new Size(983, 269);
            dataGridViewPoll.TabIndex = 4;
            // 
            // dataGridViewPlayers
            // 
            dataGridViewPlayers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            dataGridViewPlayers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewPlayers.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewPlayers.Location = new Point(12, 287);
            dataGridViewPlayers.MultiSelect = false;
            dataGridViewPlayers.Name = "dataGridViewPlayers";
            dataGridViewPlayers.RowHeadersWidth = 82;
            dataGridViewPlayers.RowTemplate.Height = 41;
            dataGridViewPlayers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewPlayers.Size = new Size(983, 780);
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
            button6.Location = new Point(1470, 1014);
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
            button7.Location = new Point(1471, 910);
            button7.Name = "button7";
            button7.Size = new Size(303, 46);
            button7.TabIndex = 9;
            button7.Text = "Отправить приглашения";
            button7.UseVisualStyleBackColor = true;
            button7.Click += clickSendInvitation;
            // 
            // button8
            // 
            button8.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button8.Location = new Point(1153, 910);
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
            dataGridViewRating.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewRating.Location = new Point(1153, 12);
            dataGridViewRating.MultiSelect = false;
            dataGridViewRating.Name = "dataGridViewRating";
            dataGridViewRating.RowHeadersWidth = 82;
            dataGridViewRating.RowTemplate.Height = 41;
            dataGridViewRating.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewRating.Size = new Size(921, 833);
            dataGridViewRating.TabIndex = 11;
            dataGridViewRating.CellEndEdit += dataGridViewRating_CellEndEdit;
            // 
            // button9
            // 
            button9.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button9.Location = new Point(1153, 962);
            button9.Name = "button9";
            button9.Size = new Size(303, 46);
            button9.TabIndex = 12;
            button9.Text = "Дать команды";
            button9.UseVisualStyleBackColor = true;
            button9.Click += button9_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2164, 1079);
            Controls.Add(button9);
            Controls.Add(dataGridViewRating);
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
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridViewPoll).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewPlayers).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewRating).EndInit();
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
        private Button button9;
    }
}