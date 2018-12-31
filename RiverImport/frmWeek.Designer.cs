namespace RiverImport
{
    partial class frmWeek
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.dtpStart = new System.Windows.Forms.DateTimePicker();
            this.dtpEnd = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.worker = new System.ComponentModel.BackgroundWorker();
            this.btnRed_BlakList = new System.Windows.Forms.Button();
            this.btnMyWorkScore = new System.Windows.Forms.Button();
            this.btnMyWaterQuality = new System.Windows.Forms.Button();
            this.bgWorkPatrol = new System.ComponentModel.BackgroundWorker();
            this.bgWorkMyWater = new System.ComponentModel.BackgroundWorker();
            this.bgWorkDownChief = new System.ComponentModel.BackgroundWorker();
            this.bgWorkRedBlack = new System.ComponentModel.BackgroundWorker();
            this.bgWorkFourCheck = new System.ComponentModel.BackgroundWorker();
            this.bgWorkMyScore = new System.ComponentModel.BackgroundWorker();
            this.bgWorkMyAppUse = new System.ComponentModel.BackgroundWorker();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnCheckSign = new System.Windows.Forms.Button();
            this.bgWorkCheckSign = new System.ComponentModel.BackgroundWorker();
            this.label3 = new System.Windows.Forms.Label();
            this.lblChiefID = new System.Windows.Forms.TextBox();
            this.button6 = new System.Windows.Forms.Button();
            this.bgAreaChiefDownChief = new System.ComponentModel.BackgroundWorker();
            this.btnStartArea = new System.Windows.Forms.Button();
            this.btnStartTown = new System.Windows.Forms.Button();
            this.btnStartVillage = new System.Windows.Forms.Button();
            this.bgWorkArea = new System.ComponentModel.BackgroundWorker();
            this.bgWorkTown = new System.ComponentModel.BackgroundWorker();
            this.bgWorkVillage = new System.ComponentModel.BackgroundWorker();
            this.chkDelAndInDB = new System.Windows.Forms.CheckBox();
            this.btn_single_rm_info = new System.Windows.Forms.Button();
            this.btn_single_my_score = new System.Windows.Forms.Button();
            this.btn_single_water_quality = new System.Windows.Forms.Button();
            this.btn_single_river_problem = new System.Windows.Forms.Button();
            this.btn_single_lower_level = new System.Windows.Forms.Button();
            this.btn_single_lower_my_score = new System.Windows.Forms.Button();
            this.btn_single_popup = new System.Windows.Forms.Button();
            this.bgSingleRmInfo = new System.ComponentModel.BackgroundWorker();
            this.bgSingleMyScore = new System.ComponentModel.BackgroundWorker();
            this.bgSingleWaterQuality = new System.ComponentModel.BackgroundWorker();
            this.bgSingleProblem = new System.ComponentModel.BackgroundWorker();
            this.bgSingleLowerLevel = new System.ComponentModel.BackgroundWorker();
            this.bgSingleLowerMyScore = new System.ComponentModel.BackgroundWorker();
            this.bgSinglePopup = new System.ComponentModel.BackgroundWorker();
            this.chkContiuneJob = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(11, 420);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(421, 53);
            this.button1.TabIndex = 5;
            this.button1.Text = "我的下级村河长履职情况";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(11, 244);
            this.button2.Margin = new System.Windows.Forms.Padding(2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(420, 51);
            this.button2.TabIndex = 2;
            this.button2.Text = "河长详情";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(11, 300);
            this.button3.Margin = new System.Windows.Forms.Padding(2);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(420, 54);
            this.button3.TabIndex = 2;
            this.button3.Text = "我的巡河";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(11, 359);
            this.button4.Margin = new System.Windows.Forms.Padding(2);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(420, 56);
            this.button4.TabIndex = 3;
            this.button4.Text = "我的APP使用情况";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // dtpStart
            // 
            this.dtpStart.CustomFormat = "yyyy-MM-dd 00:00:00";
            this.dtpStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStart.Location = new System.Drawing.Point(54, 23);
            this.dtpStart.Margin = new System.Windows.Forms.Padding(2);
            this.dtpStart.Name = "dtpStart";
            this.dtpStart.Size = new System.Drawing.Size(151, 21);
            this.dtpStart.TabIndex = 0;
            this.dtpStart.ValueChanged += new System.EventHandler(this.dateTimePicker1_ValueChanged);
            // 
            // dtpEnd
            // 
            this.dtpEnd.CustomFormat = "yyyy-MM-dd 23:59:59";
            this.dtpEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEnd.Location = new System.Drawing.Point(280, 23);
            this.dtpEnd.Margin = new System.Windows.Forms.Padding(2);
            this.dtpEnd.Name = "dtpEnd";
            this.dtpEnd.Size = new System.Drawing.Size(151, 21);
            this.dtpEnd.TabIndex = 5;
            this.dtpEnd.ValueChanged += new System.EventHandler(this.dtpEnd_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 29);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "开始：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(236, 29);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "结束：";
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.ForeColor = System.Drawing.Color.Red;
            this.lblInfo.Location = new System.Drawing.Point(14, 608);
            this.lblInfo.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(53, 12);
            this.lblInfo.TabIndex = 8;
            this.lblInfo.Text = "提示信息";
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(11, 478);
            this.button5.Margin = new System.Windows.Forms.Padding(2);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(419, 59);
            this.button5.TabIndex = 9;
            this.button5.Text = "四个查清";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(11, 560);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(2);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(420, 18);
            this.progressBar1.TabIndex = 10;
            // 
            // worker
            // 
            this.worker.WorkerReportsProgress = true;
            // 
            // btnRed_BlakList
            // 
            this.btnRed_BlakList.Location = new System.Drawing.Point(445, 66);
            this.btnRed_BlakList.Margin = new System.Windows.Forms.Padding(2);
            this.btnRed_BlakList.Name = "btnRed_BlakList";
            this.btnRed_BlakList.Size = new System.Drawing.Size(418, 51);
            this.btnRed_BlakList.TabIndex = 11;
            this.btnRed_BlakList.Text = "红黑榜";
            this.btnRed_BlakList.UseVisualStyleBackColor = true;
            this.btnRed_BlakList.Click += new System.EventHandler(this.btnRed_BlakList_Click);
            // 
            // btnMyWorkScore
            // 
            this.btnMyWorkScore.Location = new System.Drawing.Point(445, 122);
            this.btnMyWorkScore.Margin = new System.Windows.Forms.Padding(2);
            this.btnMyWorkScore.Name = "btnMyWorkScore";
            this.btnMyWorkScore.Size = new System.Drawing.Size(418, 54);
            this.btnMyWorkScore.TabIndex = 12;
            this.btnMyWorkScore.Text = "我的履职评价";
            this.btnMyWorkScore.UseVisualStyleBackColor = true;
            this.btnMyWorkScore.Click += new System.EventHandler(this.btnMyWorkScore_Click);
            // 
            // btnMyWaterQuality
            // 
            this.btnMyWaterQuality.Location = new System.Drawing.Point(445, 182);
            this.btnMyWaterQuality.Margin = new System.Windows.Forms.Padding(2);
            this.btnMyWaterQuality.Name = "btnMyWaterQuality";
            this.btnMyWaterQuality.Size = new System.Drawing.Size(418, 55);
            this.btnMyWaterQuality.TabIndex = 13;
            this.btnMyWaterQuality.Text = "我的水质变化";
            this.btnMyWaterQuality.UseVisualStyleBackColor = true;
            this.btnMyWaterQuality.Click += new System.EventHandler(this.btnMyWaterQuality_Click);
            // 
            // bgWorkPatrol
            // 
            this.bgWorkPatrol.WorkerReportsProgress = true;
            // 
            // bgWorkMyWater
            // 
            this.bgWorkMyWater.WorkerReportsProgress = true;
            // 
            // bgWorkDownChief
            // 
            this.bgWorkDownChief.WorkerReportsProgress = true;
            // 
            // bgWorkRedBlack
            // 
            this.bgWorkRedBlack.WorkerReportsProgress = true;
            // 
            // bgWorkFourCheck
            // 
            this.bgWorkFourCheck.WorkerReportsProgress = true;
            // 
            // bgWorkMyScore
            // 
            this.bgWorkMyScore.WorkerReportsProgress = true;
            // 
            // bgWorkMyAppUse
            // 
            this.bgWorkMyAppUse.WorkerReportsProgress = true;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(445, 300);
            this.btnTest.Margin = new System.Windows.Forms.Padding(2);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(418, 59);
            this.btnTest.TabIndex = 14;
            this.btnTest.Text = "测试";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // btnCheckSign
            // 
            this.btnCheckSign.Location = new System.Drawing.Point(445, 242);
            this.btnCheckSign.Margin = new System.Windows.Forms.Padding(2);
            this.btnCheckSign.Name = "btnCheckSign";
            this.btnCheckSign.Size = new System.Drawing.Size(418, 53);
            this.btnCheckSign.TabIndex = 15;
            this.btnCheckSign.Text = "补齐签到数据";
            this.btnCheckSign.UseVisualStyleBackColor = true;
            this.btnCheckSign.Click += new System.EventHandler(this.btnCheckSign_Click);
            // 
            // bgWorkCheckSign
            // 
            this.bgWorkCheckSign.WorkerReportsProgress = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(446, 28);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 19;
            this.label3.Text = "河长ID：";
            // 
            // lblChiefID
            // 
            this.lblChiefID.Location = new System.Drawing.Point(504, 23);
            this.lblChiefID.Margin = new System.Windows.Forms.Padding(2);
            this.lblChiefID.Name = "lblChiefID";
            this.lblChiefID.Size = new System.Drawing.Size(164, 21);
            this.lblChiefID.TabIndex = 18;
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(445, 364);
            this.button6.Margin = new System.Windows.Forms.Padding(2);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(418, 53);
            this.button6.TabIndex = 20;
            this.button6.Text = "我的下级镇河长履职情况";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // bgAreaChiefDownChief
            // 
            this.bgAreaChiefDownChief.WorkerReportsProgress = true;
            // 
            // btnStartArea
            // 
            this.btnStartArea.Location = new System.Drawing.Point(14, 182);
            this.btnStartArea.Margin = new System.Windows.Forms.Padding(2);
            this.btnStartArea.Name = "btnStartArea";
            this.btnStartArea.Size = new System.Drawing.Size(417, 55);
            this.btnStartArea.TabIndex = 23;
            this.btnStartArea.Text = "开始187履职数据转换--->区级";
            this.btnStartArea.UseVisualStyleBackColor = true;
            this.btnStartArea.Click += new System.EventHandler(this.btnStartArea_Click);
            // 
            // btnStartTown
            // 
            this.btnStartTown.Location = new System.Drawing.Point(14, 122);
            this.btnStartTown.Margin = new System.Windows.Forms.Padding(2);
            this.btnStartTown.Name = "btnStartTown";
            this.btnStartTown.Size = new System.Drawing.Size(417, 54);
            this.btnStartTown.TabIndex = 22;
            this.btnStartTown.Text = "开始187履职数据转换--->镇级";
            this.btnStartTown.UseVisualStyleBackColor = true;
            this.btnStartTown.Click += new System.EventHandler(this.btnStartTown_Click);
            // 
            // btnStartVillage
            // 
            this.btnStartVillage.Location = new System.Drawing.Point(14, 66);
            this.btnStartVillage.Margin = new System.Windows.Forms.Padding(2);
            this.btnStartVillage.Name = "btnStartVillage";
            this.btnStartVillage.Size = new System.Drawing.Size(417, 51);
            this.btnStartVillage.TabIndex = 21;
            this.btnStartVillage.Text = "开始187履职数据转换--->村级";
            this.btnStartVillage.UseVisualStyleBackColor = true;
            this.btnStartVillage.Click += new System.EventHandler(this.btnStartVillage_Click);
            // 
            // bgWorkArea
            // 
            this.bgWorkArea.WorkerReportsProgress = true;
            // 
            // bgWorkTown
            // 
            this.bgWorkTown.WorkerReportsProgress = true;
            // 
            // bgWorkVillage
            // 
            this.bgWorkVillage.WorkerReportsProgress = true;
            // 
            // chkDelAndInDB
            // 
            this.chkDelAndInDB.AutoSize = true;
            this.chkDelAndInDB.Location = new System.Drawing.Point(694, 23);
            this.chkDelAndInDB.Margin = new System.Windows.Forms.Padding(2);
            this.chkDelAndInDB.Name = "chkDelAndInDB";
            this.chkDelAndInDB.Size = new System.Drawing.Size(84, 16);
            this.chkDelAndInDB.TabIndex = 24;
            this.chkDelAndInDB.Text = "删除及入库";
            this.chkDelAndInDB.UseVisualStyleBackColor = true;
            this.chkDelAndInDB.CheckedChanged += new System.EventHandler(this.chkDelAndInDB_CheckedChanged);
            // 
            // btn_single_rm_info
            // 
            this.btn_single_rm_info.Location = new System.Drawing.Point(880, 66);
            this.btn_single_rm_info.Name = "btn_single_rm_info";
            this.btn_single_rm_info.Size = new System.Drawing.Size(301, 51);
            this.btn_single_rm_info.TabIndex = 25;
            this.btn_single_rm_info.Text = "（区级单条河涌详情）APPWEEK_RIVER_RM_INFO";
            this.btn_single_rm_info.UseVisualStyleBackColor = true;
            this.btn_single_rm_info.Click += new System.EventHandler(this.btn_single_rm_info_Click);
            // 
            // btn_single_my_score
            // 
            this.btn_single_my_score.Location = new System.Drawing.Point(880, 122);
            this.btn_single_my_score.Name = "btn_single_my_score";
            this.btn_single_my_score.Size = new System.Drawing.Size(301, 54);
            this.btn_single_my_score.TabIndex = 26;
            this.btn_single_my_score.Text = "我的履职评价（187河）APPWEEK_RIVER_MY_SCORE";
            this.btn_single_my_score.UseVisualStyleBackColor = true;
            this.btn_single_my_score.Click += new System.EventHandler(this.btn_single_my_score_Click);
            // 
            // btn_single_water_quality
            // 
            this.btn_single_water_quality.Location = new System.Drawing.Point(880, 182);
            this.btn_single_water_quality.Name = "btn_single_water_quality";
            this.btn_single_water_quality.Size = new System.Drawing.Size(301, 55);
            this.btn_single_water_quality.TabIndex = 27;
            this.btn_single_water_quality.Text = "我的水质变化APPWEEK_RIVER_MY_WATER_QUALITY";
            this.btn_single_water_quality.UseVisualStyleBackColor = true;
            this.btn_single_water_quality.Click += new System.EventHandler(this.btn_single_water_quality_Click);
            // 
            // btn_single_river_problem
            // 
            this.btn_single_river_problem.Location = new System.Drawing.Point(880, 242);
            this.btn_single_river_problem.Name = "btn_single_river_problem";
            this.btn_single_river_problem.Size = new System.Drawing.Size(301, 53);
            this.btn_single_river_problem.TabIndex = 28;
            this.btn_single_river_problem.Text = "管辖河段问题发现与处理APPWEEK_RIVER_JURISDICTION_PRO";
            this.btn_single_river_problem.UseVisualStyleBackColor = true;
            this.btn_single_river_problem.Click += new System.EventHandler(this.btn_single_river_problem_Click);
            // 
            // btn_single_lower_level
            // 
            this.btn_single_lower_level.Location = new System.Drawing.Point(880, 300);
            this.btn_single_lower_level.Name = "btn_single_lower_level";
            this.btn_single_lower_level.Size = new System.Drawing.Size(301, 59);
            this.btn_single_lower_level.TabIndex = 29;
            this.btn_single_lower_level.Text = "我的下级镇街河长履职APPWEEK_RIVER_LOWER_LEVEL_SITUATION";
            this.btn_single_lower_level.UseVisualStyleBackColor = true;
            this.btn_single_lower_level.Click += new System.EventHandler(this.btn_single_lower_level_Click);
            // 
            // btn_single_lower_my_score
            // 
            this.btn_single_lower_my_score.Location = new System.Drawing.Point(880, 365);
            this.btn_single_lower_my_score.Name = "btn_single_lower_my_score";
            this.btn_single_lower_my_score.Size = new System.Drawing.Size(301, 59);
            this.btn_single_lower_my_score.TabIndex = 30;
            this.btn_single_lower_my_score.Text = "我的下级镇街河长履职（187履职评价）APPWEEK_RIVER_LOWER_MY_SCORE";
            this.btn_single_lower_my_score.UseVisualStyleBackColor = true;
            this.btn_single_lower_my_score.Click += new System.EventHandler(this.btn_single_lower_my_score_Click);
            // 
            // btn_single_popup
            // 
            this.btn_single_popup.Location = new System.Drawing.Point(880, 430);
            this.btn_single_popup.Name = "btn_single_popup";
            this.btn_single_popup.Size = new System.Drawing.Size(301, 59);
            this.btn_single_popup.TabIndex = 31;
            this.btn_single_popup.Text = "我的下级镇街河长履职弹窗APPWEEK_RIVER_POPUP_LOWER_LEVEL";
            this.btn_single_popup.UseVisualStyleBackColor = true;
            this.btn_single_popup.Click += new System.EventHandler(this.btn_single_popup_Click);
            // 
            // bgSingleRmInfo
            // 
            this.bgSingleRmInfo.WorkerReportsProgress = true;
            // 
            // bgSingleMyScore
            // 
            this.bgSingleMyScore.WorkerReportsProgress = true;
            // 
            // bgSingleWaterQuality
            // 
            this.bgSingleWaterQuality.WorkerReportsProgress = true;
            // 
            // bgSingleProblem
            // 
            this.bgSingleProblem.WorkerReportsProgress = true;
            // 
            // bgSingleLowerLevel
            // 
            this.bgSingleLowerLevel.WorkerReportsProgress = true;
            // 
            // bgSingleLowerMyScore
            // 
            this.bgSingleLowerMyScore.WorkerReportsProgress = true;
            // 
            // bgSinglePopup
            // 
            this.bgSinglePopup.WorkerReportsProgress = true;
            // 
            // chkContiuneJob
            // 
            this.chkContiuneJob.AutoSize = true;
            this.chkContiuneJob.Location = new System.Drawing.Point(802, 22);
            this.chkContiuneJob.Name = "chkContiuneJob";
            this.chkContiuneJob.Size = new System.Drawing.Size(144, 16);
            this.chkContiuneJob.TabIndex = 32;
            this.chkContiuneJob.Text = "继续跑完剩余按钮数据";
            this.chkContiuneJob.UseVisualStyleBackColor = true;
            this.chkContiuneJob.CheckedChanged += new System.EventHandler(this.chkContiuneJob_CheckedChanged);
            // 
            // frmWeek
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1193, 645);
            this.Controls.Add(this.chkContiuneJob);
            this.Controls.Add(this.btn_single_popup);
            this.Controls.Add(this.btn_single_lower_my_score);
            this.Controls.Add(this.btn_single_lower_level);
            this.Controls.Add(this.btn_single_river_problem);
            this.Controls.Add(this.btn_single_water_quality);
            this.Controls.Add(this.btn_single_my_score);
            this.Controls.Add(this.btn_single_rm_info);
            this.Controls.Add(this.chkDelAndInDB);
            this.Controls.Add(this.btnStartArea);
            this.Controls.Add(this.btnStartTown);
            this.Controls.Add(this.btnStartVillage);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblChiefID);
            this.Controls.Add(this.btnCheckSign);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.btnMyWaterQuality);
            this.Controls.Add(this.btnMyWorkScore);
            this.Controls.Add(this.btnRed_BlakList);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dtpEnd);
            this.Controls.Add(this.dtpStart);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "frmWeek";
            this.Text = "周报数据处理";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.DateTimePicker dtpStart;
        private System.Windows.Forms.DateTimePicker dtpEnd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Button button5;
        public System.Windows.Forms.ProgressBar progressBar1;
        private System.ComponentModel.BackgroundWorker worker;
        private System.Windows.Forms.Button btnRed_BlakList;
        private System.Windows.Forms.Button btnMyWorkScore;
        private System.Windows.Forms.Button btnMyWaterQuality;
        private System.ComponentModel.BackgroundWorker bgWorkPatrol;
        private System.ComponentModel.BackgroundWorker bgWorkMyWater;
        private System.ComponentModel.BackgroundWorker bgWorkDownChief;
        private System.ComponentModel.BackgroundWorker bgWorkRedBlack;
        private System.ComponentModel.BackgroundWorker bgWorkFourCheck;
        private System.ComponentModel.BackgroundWorker bgWorkMyScore;
        private System.ComponentModel.BackgroundWorker bgWorkMyAppUse;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button btnCheckSign;
        private System.ComponentModel.BackgroundWorker bgWorkCheckSign;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox lblChiefID;
        private System.Windows.Forms.Button button6;
        private System.ComponentModel.BackgroundWorker bgAreaChiefDownChief;
        private System.Windows.Forms.Button btnStartArea;
        private System.Windows.Forms.Button btnStartTown;
        private System.Windows.Forms.Button btnStartVillage;
        private System.ComponentModel.BackgroundWorker bgWorkArea;
        private System.ComponentModel.BackgroundWorker bgWorkTown;
        public System.ComponentModel.BackgroundWorker bgWorkVillage;
        private System.Windows.Forms.CheckBox chkDelAndInDB;
        private System.Windows.Forms.Button btn_single_rm_info;
        private System.Windows.Forms.Button btn_single_my_score;
        private System.Windows.Forms.Button btn_single_water_quality;
        private System.Windows.Forms.Button btn_single_river_problem;
        private System.Windows.Forms.Button btn_single_lower_level;
        private System.Windows.Forms.Button btn_single_lower_my_score;
        private System.Windows.Forms.Button btn_single_popup;
        private System.ComponentModel.BackgroundWorker bgSingleRmInfo;
        private System.ComponentModel.BackgroundWorker bgSingleMyScore;
        private System.ComponentModel.BackgroundWorker bgSingleWaterQuality;
        private System.ComponentModel.BackgroundWorker bgSingleProblem;
        private System.ComponentModel.BackgroundWorker bgSingleLowerLevel;
        private System.ComponentModel.BackgroundWorker bgSingleLowerMyScore;
        private System.ComponentModel.BackgroundWorker bgSinglePopup;
        private System.Windows.Forms.CheckBox chkContiuneJob;
    }
}