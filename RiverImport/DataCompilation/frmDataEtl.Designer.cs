namespace RiverImport.DataCompilation
{
    partial class frmDataEtl
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
            this.components = new System.ComponentModel.Container();
            this.btnPatrol = new System.Windows.Forms.Button();
            this.chkContiuneJob = new System.Windows.Forms.CheckBox();
            this.chkDelAndInDB = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblChiefID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dtpEnd = new System.Windows.Forms.DateTimePicker();
            this.dtpStart = new System.Windows.Forms.DateTimePicker();
            this.bgWorker_Patrol = new System.ComponentModel.BackgroundWorker();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.rtbInfo = new System.Windows.Forms.RichTextBox();
            this.lblInfo = new System.Windows.Forms.Label();
            this.btnTest = new System.Windows.Forms.Button();
            this.pBarProblem = new System.Windows.Forms.ProgressBar();
            this.btnProblem = new System.Windows.Forms.Button();
            this.bgWorker_Problem = new System.ComponentModel.BackgroundWorker();
            this.pBar_Base = new System.Windows.Forms.ProgressBar();
            this.btnBase = new System.Windows.Forms.Button();
            this.bgWork_Base = new System.ComponentModel.BackgroundWorker();
            this.pBarFootPatrol = new System.Windows.Forms.ProgressBar();
            this.btn_footPatrol = new System.Windows.Forms.Button();
            this.bg_footPatrol = new System.ComponentModel.BackgroundWorker();
            this.pBarProblemType = new System.Windows.Forms.ProgressBar();
            this.btnProblemType = new System.Windows.Forms.Button();
            this.bg_problemType = new System.ComponentModel.BackgroundWorker();
            this.pBarFourCheck = new System.Windows.Forms.ProgressBar();
            this.btnFourCheck = new System.Windows.Forms.Button();
            this.bg_fourCheck = new System.ComponentModel.BackgroundWorker();
            this.pBarProblemAssign = new System.Windows.Forms.ProgressBar();
            this.btnProblemAssign = new System.Windows.Forms.Button();
            this.bg_problemAssign = new System.ComponentModel.BackgroundWorker();
            this.pBarWaterQuality = new System.Windows.Forms.ProgressBar();
            this.btnWaterQuality = new System.Windows.Forms.Button();
            this.bg_waterQuality = new System.ComponentModel.BackgroundWorker();
            this.pBarAppUse = new System.Windows.Forms.ProgressBar();
            this.btnAppUse = new System.Windows.Forms.Button();
            this.bg_appUse = new System.ComponentModel.BackgroundWorker();
            this.pBarScore = new System.Windows.Forms.ProgressBar();
            this.btnScore = new System.Windows.Forms.Button();
            this.bg_score = new System.ComponentModel.BackgroundWorker();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkVillageSwitch = new System.Windows.Forms.CheckBox();
            this.chkTownSwitch = new System.Windows.Forms.CheckBox();
            this.chkAreaSwitch = new System.Windows.Forms.CheckBox();
            this.chkCitySwitch = new System.Windows.Forms.CheckBox();
            this.timer_start = new System.Windows.Forms.Timer(this.components);
            this.btn_startAuto = new System.Windows.Forms.Button();
            this.rtbBase = new System.Windows.Forms.RichTextBox();
            this.rtbPatrol = new System.Windows.Forms.RichTextBox();
            this.rtbProblem = new System.Windows.Forms.RichTextBox();
            this.rtbFootPatrol = new System.Windows.Forms.RichTextBox();
            this.rtbProblemType = new System.Windows.Forms.RichTextBox();
            this.rtbFourCheck = new System.Windows.Forms.RichTextBox();
            this.rtbProblemAssign = new System.Windows.Forms.RichTextBox();
            this.rtbWaterQuality = new System.Windows.Forms.RichTextBox();
            this.rtbAppUse = new System.Windows.Forms.RichTextBox();
            this.rtbScore = new System.Windows.Forms.RichTextBox();
            this.chkFixedPeriod = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnPatrol
            // 
            this.btnPatrol.Location = new System.Drawing.Point(10, 198);
            this.btnPatrol.Name = "btnPatrol";
            this.btnPatrol.Size = new System.Drawing.Size(379, 51);
            this.btnPatrol.TabIndex = 0;
            this.btnPatrol.Text = "巡河数据";
            this.btnPatrol.UseVisualStyleBackColor = true;
            this.btnPatrol.Click += new System.EventHandler(this.btnPatrol_Click);
            // 
            // chkContiuneJob
            // 
            this.chkContiuneJob.AutoSize = true;
            this.chkContiuneJob.Location = new System.Drawing.Point(1049, 11);
            this.chkContiuneJob.Name = "chkContiuneJob";
            this.chkContiuneJob.Size = new System.Drawing.Size(144, 16);
            this.chkContiuneJob.TabIndex = 40;
            this.chkContiuneJob.Text = "继续跑完剩余按钮数据";
            this.chkContiuneJob.UseVisualStyleBackColor = true;
            this.chkContiuneJob.CheckedChanged += new System.EventHandler(this.chkContiuneJob_CheckedChanged);
            // 
            // chkDelAndInDB
            // 
            this.chkDelAndInDB.AutoSize = true;
            this.chkDelAndInDB.Location = new System.Drawing.Point(692, 11);
            this.chkDelAndInDB.Margin = new System.Windows.Forms.Padding(2);
            this.chkDelAndInDB.Name = "chkDelAndInDB";
            this.chkDelAndInDB.Size = new System.Drawing.Size(84, 16);
            this.chkDelAndInDB.TabIndex = 39;
            this.chkDelAndInDB.Text = "删除及入库";
            this.chkDelAndInDB.UseVisualStyleBackColor = true;
            this.chkDelAndInDB.CheckedChanged += new System.EventHandler(this.chkDelAndInDB_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(444, 16);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 38;
            this.label3.Text = "河长ID：";
            // 
            // lblChiefID
            // 
            this.lblChiefID.Location = new System.Drawing.Point(502, 11);
            this.lblChiefID.Margin = new System.Windows.Forms.Padding(2);
            this.lblChiefID.Name = "lblChiefID";
            this.lblChiefID.Size = new System.Drawing.Size(164, 21);
            this.lblChiefID.TabIndex = 37;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(234, 17);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 36;
            this.label2.Text = "结束：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 17);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 35;
            this.label1.Text = "开始：";
            // 
            // dtpEnd
            // 
            this.dtpEnd.CustomFormat = "yyyy-MM-dd 23:59:59";
            this.dtpEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEnd.Location = new System.Drawing.Point(278, 11);
            this.dtpEnd.Margin = new System.Windows.Forms.Padding(2);
            this.dtpEnd.Name = "dtpEnd";
            this.dtpEnd.Size = new System.Drawing.Size(151, 21);
            this.dtpEnd.TabIndex = 34;
            this.dtpEnd.ValueChanged += new System.EventHandler(this.dtpEnd_ValueChanged);
            // 
            // dtpStart
            // 
            this.dtpStart.CustomFormat = "yyyy-MM-dd 00:00:00";
            this.dtpStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStart.Location = new System.Drawing.Point(52, 11);
            this.dtpStart.Margin = new System.Windows.Forms.Padding(2);
            this.dtpStart.Name = "dtpStart";
            this.dtpStart.Size = new System.Drawing.Size(151, 21);
            this.dtpStart.TabIndex = 33;
            this.dtpStart.ValueChanged += new System.EventHandler(this.dtpStart_ValueChanged);
            // 
            // bgWorker_Patrol
            // 
            this.bgWorker_Patrol.WorkerReportsProgress = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(10, 249);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(379, 10);
            this.progressBar1.TabIndex = 41;
            // 
            // rtbInfo
            // 
            this.rtbInfo.Location = new System.Drawing.Point(10, 518);
            this.rtbInfo.Name = "rtbInfo";
            this.rtbInfo.Size = new System.Drawing.Size(772, 113);
            this.rtbInfo.TabIndex = 42;
            this.rtbInfo.Text = "";
            this.rtbInfo.TextChanged += new System.EventHandler(this.rtbInfo_TextChanged);
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.ForeColor = System.Drawing.Color.Red;
            this.lblInfo.Location = new System.Drawing.Point(8, 498);
            this.lblInfo.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(53, 12);
            this.lblInfo.TabIndex = 43;
            this.lblInfo.Text = "提示信息";
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(707, 492);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 44;
            this.btnTest.Text = "测试";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // pBarProblem
            // 
            this.pBarProblem.Location = new System.Drawing.Point(10, 394);
            this.pBarProblem.Name = "pBarProblem";
            this.pBarProblem.Size = new System.Drawing.Size(379, 10);
            this.pBarProblem.TabIndex = 46;
            // 
            // btnProblem
            // 
            this.btnProblem.Location = new System.Drawing.Point(10, 343);
            this.btnProblem.Name = "btnProblem";
            this.btnProblem.Size = new System.Drawing.Size(379, 51);
            this.btnProblem.TabIndex = 45;
            this.btnProblem.Text = "问题数据";
            this.btnProblem.UseVisualStyleBackColor = true;
            this.btnProblem.Click += new System.EventHandler(this.btnProblem_Click);
            // 
            // bgWorker_Problem
            // 
            this.bgWorker_Problem.WorkerReportsProgress = true;
            // 
            // pBar_Base
            // 
            this.pBar_Base.Location = new System.Drawing.Point(13, 100);
            this.pBar_Base.Name = "pBar_Base";
            this.pBar_Base.Size = new System.Drawing.Size(379, 10);
            this.pBar_Base.TabIndex = 48;
            // 
            // btnBase
            // 
            this.btnBase.Location = new System.Drawing.Point(13, 49);
            this.btnBase.Name = "btnBase";
            this.btnBase.Size = new System.Drawing.Size(379, 51);
            this.btnBase.TabIndex = 47;
            this.btnBase.Text = "基础数据";
            this.btnBase.UseVisualStyleBackColor = true;
            this.btnBase.Click += new System.EventHandler(this.btnBase_Click);
            // 
            // bgWork_Base
            // 
            this.bgWork_Base.WorkerReportsProgress = true;
            // 
            // pBarFootPatrol
            // 
            this.pBarFootPatrol.Location = new System.Drawing.Point(407, 103);
            this.pBarFootPatrol.Name = "pBarFootPatrol";
            this.pBarFootPatrol.Size = new System.Drawing.Size(379, 10);
            this.pBarFootPatrol.TabIndex = 50;
            // 
            // btn_footPatrol
            // 
            this.btn_footPatrol.Location = new System.Drawing.Point(407, 52);
            this.btn_footPatrol.Name = "btn_footPatrol";
            this.btn_footPatrol.Size = new System.Drawing.Size(379, 51);
            this.btn_footPatrol.TabIndex = 49;
            this.btn_footPatrol.Text = "徒步巡河";
            this.btn_footPatrol.UseVisualStyleBackColor = true;
            this.btn_footPatrol.Click += new System.EventHandler(this.btn_footPatrol_Click);
            // 
            // bg_footPatrol
            // 
            this.bg_footPatrol.WorkerReportsProgress = true;
            // 
            // pBarProblemType
            // 
            this.pBarProblemType.Location = new System.Drawing.Point(407, 249);
            this.pBarProblemType.Name = "pBarProblemType";
            this.pBarProblemType.Size = new System.Drawing.Size(379, 10);
            this.pBarProblemType.TabIndex = 52;
            // 
            // btnProblemType
            // 
            this.btnProblemType.Location = new System.Drawing.Point(407, 198);
            this.btnProblemType.Name = "btnProblemType";
            this.btnProblemType.Size = new System.Drawing.Size(379, 51);
            this.btnProblemType.TabIndex = 51;
            this.btnProblemType.Text = "问题上报数据-问题类型";
            this.btnProblemType.UseVisualStyleBackColor = true;
            this.btnProblemType.Click += new System.EventHandler(this.btnProblemType_Click);
            // 
            // bg_problemType
            // 
            this.bg_problemType.WorkerReportsProgress = true;
            // 
            // pBarFourCheck
            // 
            this.pBarFourCheck.Location = new System.Drawing.Point(407, 394);
            this.pBarFourCheck.Name = "pBarFourCheck";
            this.pBarFourCheck.Size = new System.Drawing.Size(379, 10);
            this.pBarFourCheck.TabIndex = 54;
            // 
            // btnFourCheck
            // 
            this.btnFourCheck.Location = new System.Drawing.Point(407, 343);
            this.btnFourCheck.Name = "btnFourCheck";
            this.btnFourCheck.Size = new System.Drawing.Size(379, 51);
            this.btnFourCheck.TabIndex = 53;
            this.btnFourCheck.Text = "四个查清(type=2)";
            this.btnFourCheck.UseVisualStyleBackColor = true;
            this.btnFourCheck.Click += new System.EventHandler(this.btnFourCheck_Click);
            // 
            // bg_fourCheck
            // 
            this.bg_fourCheck.WorkerReportsProgress = true;
            // 
            // pBarProblemAssign
            // 
            this.pBarProblemAssign.Location = new System.Drawing.Point(800, 103);
            this.pBarProblemAssign.Name = "pBarProblemAssign";
            this.pBarProblemAssign.Size = new System.Drawing.Size(379, 10);
            this.pBarProblemAssign.TabIndex = 56;
            // 
            // btnProblemAssign
            // 
            this.btnProblemAssign.Location = new System.Drawing.Point(800, 52);
            this.btnProblemAssign.Name = "btnProblemAssign";
            this.btnProblemAssign.Size = new System.Drawing.Size(379, 51);
            this.btnProblemAssign.TabIndex = 55;
            this.btnProblemAssign.Text = "问题交办、重大问题上报、整改";
            this.btnProblemAssign.UseVisualStyleBackColor = true;
            this.btnProblemAssign.Click += new System.EventHandler(this.btnProblemAssign_Click);
            // 
            // bg_problemAssign
            // 
            this.bg_problemAssign.WorkerReportsProgress = true;
            // 
            // pBarWaterQuality
            // 
            this.pBarWaterQuality.Location = new System.Drawing.Point(800, 249);
            this.pBarWaterQuality.Name = "pBarWaterQuality";
            this.pBarWaterQuality.Size = new System.Drawing.Size(379, 10);
            this.pBarWaterQuality.TabIndex = 58;
            // 
            // btnWaterQuality
            // 
            this.btnWaterQuality.Location = new System.Drawing.Point(800, 198);
            this.btnWaterQuality.Name = "btnWaterQuality";
            this.btnWaterQuality.Size = new System.Drawing.Size(379, 51);
            this.btnWaterQuality.TabIndex = 57;
            this.btnWaterQuality.Text = "水质";
            this.btnWaterQuality.UseVisualStyleBackColor = true;
            this.btnWaterQuality.Click += new System.EventHandler(this.btnWaterQuality_Click);
            // 
            // bg_waterQuality
            // 
            this.bg_waterQuality.WorkerReportsProgress = true;
            // 
            // pBarAppUse
            // 
            this.pBarAppUse.Location = new System.Drawing.Point(800, 394);
            this.pBarAppUse.Name = "pBarAppUse";
            this.pBarAppUse.Size = new System.Drawing.Size(379, 10);
            this.pBarAppUse.TabIndex = 60;
            // 
            // btnAppUse
            // 
            this.btnAppUse.Location = new System.Drawing.Point(800, 343);
            this.btnAppUse.Name = "btnAppUse";
            this.btnAppUse.Size = new System.Drawing.Size(379, 51);
            this.btnAppUse.TabIndex = 59;
            this.btnAppUse.Text = "APP使用";
            this.btnAppUse.UseVisualStyleBackColor = true;
            this.btnAppUse.Click += new System.EventHandler(this.btnAppUse_Click);
            // 
            // bg_appUse
            // 
            this.bg_appUse.WorkerReportsProgress = true;
            // 
            // pBarScore
            // 
            this.pBarScore.Location = new System.Drawing.Point(800, 543);
            this.pBarScore.Name = "pBarScore";
            this.pBarScore.Size = new System.Drawing.Size(379, 10);
            this.pBarScore.TabIndex = 62;
            // 
            // btnScore
            // 
            this.btnScore.Location = new System.Drawing.Point(800, 492);
            this.btnScore.Name = "btnScore";
            this.btnScore.Size = new System.Drawing.Size(379, 51);
            this.btnScore.TabIndex = 61;
            this.btnScore.Text = "河长积分(type=3)";
            this.btnScore.UseVisualStyleBackColor = true;
            this.btnScore.Click += new System.EventHandler(this.btnScore_Click);
            // 
            // bg_score
            // 
            this.bg_score.WorkerReportsProgress = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkVillageSwitch);
            this.groupBox1.Controls.Add(this.chkTownSwitch);
            this.groupBox1.Controls.Add(this.chkAreaSwitch);
            this.groupBox1.Controls.Add(this.chkCitySwitch);
            this.groupBox1.Location = new System.Drawing.Point(1200, 49);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 121);
            this.groupBox1.TabIndex = 63;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "市、区、镇、村数据开关";
            // 
            // chkVillageSwitch
            // 
            this.chkVillageSwitch.AutoSize = true;
            this.chkVillageSwitch.Location = new System.Drawing.Point(7, 96);
            this.chkVillageSwitch.Name = "chkVillageSwitch";
            this.chkVillageSwitch.Size = new System.Drawing.Size(72, 16);
            this.chkVillageSwitch.TabIndex = 3;
            this.chkVillageSwitch.Text = "村级河段";
            this.chkVillageSwitch.UseVisualStyleBackColor = true;
            this.chkVillageSwitch.CheckedChanged += new System.EventHandler(this.chkVillageSwitch_CheckedChanged);
            // 
            // chkTownSwitch
            // 
            this.chkTownSwitch.AutoSize = true;
            this.chkTownSwitch.Location = new System.Drawing.Point(7, 70);
            this.chkTownSwitch.Name = "chkTownSwitch";
            this.chkTownSwitch.Size = new System.Drawing.Size(72, 16);
            this.chkTownSwitch.TabIndex = 2;
            this.chkTownSwitch.Text = "镇级河段";
            this.chkTownSwitch.UseVisualStyleBackColor = true;
            this.chkTownSwitch.CheckedChanged += new System.EventHandler(this.chkTownSwitch_CheckedChanged);
            // 
            // chkAreaSwitch
            // 
            this.chkAreaSwitch.AutoSize = true;
            this.chkAreaSwitch.Location = new System.Drawing.Point(7, 45);
            this.chkAreaSwitch.Name = "chkAreaSwitch";
            this.chkAreaSwitch.Size = new System.Drawing.Size(72, 16);
            this.chkAreaSwitch.TabIndex = 1;
            this.chkAreaSwitch.Text = "区级河段";
            this.chkAreaSwitch.UseVisualStyleBackColor = true;
            this.chkAreaSwitch.CheckedChanged += new System.EventHandler(this.chkAreaSwitch_CheckedChanged);
            // 
            // chkCitySwitch
            // 
            this.chkCitySwitch.AutoSize = true;
            this.chkCitySwitch.Location = new System.Drawing.Point(7, 21);
            this.chkCitySwitch.Name = "chkCitySwitch";
            this.chkCitySwitch.Size = new System.Drawing.Size(72, 16);
            this.chkCitySwitch.TabIndex = 0;
            this.chkCitySwitch.Text = "市级河段";
            this.chkCitySwitch.UseVisualStyleBackColor = true;
            this.chkCitySwitch.CheckedChanged += new System.EventHandler(this.chkCitySwitch_CheckedChanged);
            // 
            // timer_start
            // 
            this.timer_start.Interval = 1000;
            this.timer_start.Tick += new System.EventHandler(this.timer_start_Tick);
            // 
            // btn_startAuto
            // 
            this.btn_startAuto.Location = new System.Drawing.Point(1200, 188);
            this.btn_startAuto.Name = "btn_startAuto";
            this.btn_startAuto.Size = new System.Drawing.Size(200, 131);
            this.btn_startAuto.TabIndex = 64;
            this.btn_startAuto.Text = "开启定时跑数据";
            this.btn_startAuto.UseVisualStyleBackColor = true;
            this.btn_startAuto.Click += new System.EventHandler(this.btn_startAuto_Click);
            // 
            // rtbBase
            // 
            this.rtbBase.Location = new System.Drawing.Point(13, 110);
            this.rtbBase.Name = "rtbBase";
            this.rtbBase.Size = new System.Drawing.Size(379, 79);
            this.rtbBase.TabIndex = 65;
            this.rtbBase.Text = "";
            this.rtbBase.TextChanged += new System.EventHandler(this.rtbBase_TextChanged);
            // 
            // rtbPatrol
            // 
            this.rtbPatrol.Location = new System.Drawing.Point(10, 258);
            this.rtbPatrol.Name = "rtbPatrol";
            this.rtbPatrol.Size = new System.Drawing.Size(379, 79);
            this.rtbPatrol.TabIndex = 66;
            this.rtbPatrol.Text = "";
            this.rtbPatrol.TextChanged += new System.EventHandler(this.rtbBase_TextChanged);
            // 
            // rtbProblem
            // 
            this.rtbProblem.Location = new System.Drawing.Point(10, 404);
            this.rtbProblem.Name = "rtbProblem";
            this.rtbProblem.Size = new System.Drawing.Size(379, 79);
            this.rtbProblem.TabIndex = 67;
            this.rtbProblem.Text = "";
            this.rtbProblem.TextChanged += new System.EventHandler(this.rtbBase_TextChanged);
            // 
            // rtbFootPatrol
            // 
            this.rtbFootPatrol.Location = new System.Drawing.Point(407, 113);
            this.rtbFootPatrol.Name = "rtbFootPatrol";
            this.rtbFootPatrol.Size = new System.Drawing.Size(379, 79);
            this.rtbFootPatrol.TabIndex = 68;
            this.rtbFootPatrol.Text = "";
            this.rtbFootPatrol.TextChanged += new System.EventHandler(this.rtbBase_TextChanged);
            // 
            // rtbProblemType
            // 
            this.rtbProblemType.Location = new System.Drawing.Point(407, 258);
            this.rtbProblemType.Name = "rtbProblemType";
            this.rtbProblemType.Size = new System.Drawing.Size(379, 79);
            this.rtbProblemType.TabIndex = 69;
            this.rtbProblemType.Text = "";
            this.rtbProblemType.TextChanged += new System.EventHandler(this.rtbBase_TextChanged);
            // 
            // rtbFourCheck
            // 
            this.rtbFourCheck.Location = new System.Drawing.Point(407, 404);
            this.rtbFourCheck.Name = "rtbFourCheck";
            this.rtbFourCheck.Size = new System.Drawing.Size(379, 79);
            this.rtbFourCheck.TabIndex = 70;
            this.rtbFourCheck.Text = "";
            this.rtbFourCheck.TextChanged += new System.EventHandler(this.rtbBase_TextChanged);
            // 
            // rtbProblemAssign
            // 
            this.rtbProblemAssign.Location = new System.Drawing.Point(800, 113);
            this.rtbProblemAssign.Name = "rtbProblemAssign";
            this.rtbProblemAssign.Size = new System.Drawing.Size(379, 79);
            this.rtbProblemAssign.TabIndex = 71;
            this.rtbProblemAssign.Text = "";
            this.rtbProblemAssign.TextChanged += new System.EventHandler(this.rtbBase_TextChanged);
            // 
            // rtbWaterQuality
            // 
            this.rtbWaterQuality.Location = new System.Drawing.Point(800, 258);
            this.rtbWaterQuality.Name = "rtbWaterQuality";
            this.rtbWaterQuality.Size = new System.Drawing.Size(379, 79);
            this.rtbWaterQuality.TabIndex = 72;
            this.rtbWaterQuality.Text = "";
            this.rtbWaterQuality.TextChanged += new System.EventHandler(this.rtbBase_TextChanged);
            // 
            // rtbAppUse
            // 
            this.rtbAppUse.Location = new System.Drawing.Point(800, 404);
            this.rtbAppUse.Name = "rtbAppUse";
            this.rtbAppUse.Size = new System.Drawing.Size(379, 79);
            this.rtbAppUse.TabIndex = 73;
            this.rtbAppUse.Text = "";
            this.rtbAppUse.TextChanged += new System.EventHandler(this.rtbBase_TextChanged);
            // 
            // rtbScore
            // 
            this.rtbScore.Location = new System.Drawing.Point(800, 553);
            this.rtbScore.Name = "rtbScore";
            this.rtbScore.Size = new System.Drawing.Size(379, 79);
            this.rtbScore.TabIndex = 74;
            this.rtbScore.Text = "";
            this.rtbScore.TextChanged += new System.EventHandler(this.rtbBase_TextChanged);
            // 
            // chkFixedPeriod
            // 
            this.chkFixedPeriod.AutoSize = true;
            this.chkFixedPeriod.Location = new System.Drawing.Point(791, 11);
            this.chkFixedPeriod.Name = "chkFixedPeriod";
            this.chkFixedPeriod.Size = new System.Drawing.Size(240, 16);
            this.chkFixedPeriod.TabIndex = 75;
            this.chkFixedPeriod.Text = "跑固定周期数据（如本周、本月等）数据";
            this.chkFixedPeriod.UseVisualStyleBackColor = true;
            this.chkFixedPeriod.CheckedChanged += new System.EventHandler(this.chkFixedPeriod_CheckedChanged);
            // 
            // frmDataEtl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1418, 647);
            this.Controls.Add(this.chkFixedPeriod);
            this.Controls.Add(this.rtbScore);
            this.Controls.Add(this.rtbAppUse);
            this.Controls.Add(this.rtbWaterQuality);
            this.Controls.Add(this.rtbProblemAssign);
            this.Controls.Add(this.rtbFourCheck);
            this.Controls.Add(this.rtbProblemType);
            this.Controls.Add(this.rtbFootPatrol);
            this.Controls.Add(this.rtbProblem);
            this.Controls.Add(this.rtbPatrol);
            this.Controls.Add(this.rtbBase);
            this.Controls.Add(this.btn_startAuto);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pBarScore);
            this.Controls.Add(this.btnScore);
            this.Controls.Add(this.pBarAppUse);
            this.Controls.Add(this.btnAppUse);
            this.Controls.Add(this.pBarWaterQuality);
            this.Controls.Add(this.btnWaterQuality);
            this.Controls.Add(this.pBarProblemAssign);
            this.Controls.Add(this.btnProblemAssign);
            this.Controls.Add(this.pBarFourCheck);
            this.Controls.Add(this.btnFourCheck);
            this.Controls.Add(this.pBarProblemType);
            this.Controls.Add(this.btnProblemType);
            this.Controls.Add(this.pBarFootPatrol);
            this.Controls.Add(this.btn_footPatrol);
            this.Controls.Add(this.pBar_Base);
            this.Controls.Add(this.btnBase);
            this.Controls.Add(this.pBarProblem);
            this.Controls.Add(this.btnProblem);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.rtbInfo);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.chkContiuneJob);
            this.Controls.Add(this.chkDelAndInDB);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblChiefID);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dtpEnd);
            this.Controls.Add(this.dtpStart);
            this.Controls.Add(this.btnPatrol);
            this.Name = "frmDataEtl";
            this.Text = "frmDataEtl";
            this.Load += new System.EventHandler(this.frmDataEtl_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPatrol;
        private System.Windows.Forms.CheckBox chkContiuneJob;
        private System.Windows.Forms.CheckBox chkDelAndInDB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox lblChiefID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpEnd;
        private System.Windows.Forms.DateTimePicker dtpStart;
        private System.ComponentModel.BackgroundWorker bgWorker_Patrol;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.RichTextBox rtbInfo;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.ProgressBar pBarProblem;
        private System.Windows.Forms.Button btnProblem;
        private System.ComponentModel.BackgroundWorker bgWorker_Problem;
        private System.Windows.Forms.ProgressBar pBar_Base;
        private System.Windows.Forms.Button btnBase;
        private System.ComponentModel.BackgroundWorker bgWork_Base;
        private System.Windows.Forms.ProgressBar pBarFootPatrol;
        private System.Windows.Forms.Button btn_footPatrol;
        private System.ComponentModel.BackgroundWorker bg_footPatrol;
        private System.Windows.Forms.ProgressBar pBarProblemType;
        private System.Windows.Forms.Button btnProblemType;
        private System.ComponentModel.BackgroundWorker bg_problemType;
        private System.Windows.Forms.ProgressBar pBarFourCheck;
        private System.Windows.Forms.Button btnFourCheck;
        private System.ComponentModel.BackgroundWorker bg_fourCheck;
        private System.Windows.Forms.ProgressBar pBarProblemAssign;
        private System.Windows.Forms.Button btnProblemAssign;
        private System.ComponentModel.BackgroundWorker bg_problemAssign;
        private System.Windows.Forms.ProgressBar pBarWaterQuality;
        private System.Windows.Forms.Button btnWaterQuality;
        private System.ComponentModel.BackgroundWorker bg_waterQuality;
        private System.Windows.Forms.ProgressBar pBarAppUse;
        private System.Windows.Forms.Button btnAppUse;
        private System.ComponentModel.BackgroundWorker bg_appUse;
        private System.Windows.Forms.ProgressBar pBarScore;
        private System.Windows.Forms.Button btnScore;
        private System.ComponentModel.BackgroundWorker bg_score;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkVillageSwitch;
        private System.Windows.Forms.CheckBox chkTownSwitch;
        private System.Windows.Forms.CheckBox chkAreaSwitch;
        private System.Windows.Forms.CheckBox chkCitySwitch;
        private System.Windows.Forms.Timer timer_start;
        private System.Windows.Forms.Button btn_startAuto;
        private System.Windows.Forms.RichTextBox rtbBase;
        private System.Windows.Forms.RichTextBox rtbPatrol;
        private System.Windows.Forms.RichTextBox rtbProblem;
        private System.Windows.Forms.RichTextBox rtbFootPatrol;
        private System.Windows.Forms.RichTextBox rtbProblemType;
        private System.Windows.Forms.RichTextBox rtbFourCheck;
        private System.Windows.Forms.RichTextBox rtbProblemAssign;
        private System.Windows.Forms.RichTextBox rtbWaterQuality;
        private System.Windows.Forms.RichTextBox rtbAppUse;
        private System.Windows.Forms.RichTextBox rtbScore;
        private System.Windows.Forms.CheckBox chkFixedPeriod;
    }
}