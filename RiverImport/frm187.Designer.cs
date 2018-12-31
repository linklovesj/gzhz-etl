namespace RiverImport
{
    partial class frm187
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
            this.btnStartVillage = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dtpEnd = new System.Windows.Forms.DateTimePicker();
            this.dtpStart = new System.Windows.Forms.DateTimePicker();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.bgWorkArea = new System.ComponentModel.BackgroundWorker();
            this.btnStartTown = new System.Windows.Forms.Button();
            this.btnStartArea = new System.Windows.Forms.Button();
            this.lblInfo = new System.Windows.Forms.Label();
            this.bgWorkTown = new System.ComponentModel.BackgroundWorker();
            this.bgWorkVillage = new System.ComponentModel.BackgroundWorker();
            this.lblChiefID = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnRunAll = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnStartVillage
            // 
            this.btnStartVillage.Location = new System.Drawing.Point(14, 55);
            this.btnStartVillage.Name = "btnStartVillage";
            this.btnStartVillage.Size = new System.Drawing.Size(556, 60);
            this.btnStartVillage.TabIndex = 0;
            this.btnStartVillage.Text = "开始187履职数据转换--->村级";
            this.btnStartVillage.UseVisualStyleBackColor = true;
            this.btnStartVillage.Click += new System.EventHandler(this.btnStartVillage_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(312, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 15);
            this.label2.TabIndex = 11;
            this.label2.Text = "结束：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 15);
            this.label1.TabIndex = 10;
            this.label1.Text = "开始：";
            // 
            // dtpEnd
            // 
            this.dtpEnd.CustomFormat = "yyyy-MM-dd 23:59:59";
            this.dtpEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEnd.Location = new System.Drawing.Point(370, 12);
            this.dtpEnd.Name = "dtpEnd";
            this.dtpEnd.Size = new System.Drawing.Size(200, 25);
            this.dtpEnd.TabIndex = 9;
            this.dtpEnd.ValueChanged += new System.EventHandler(this.dtpEnd_ValueChanged);
            // 
            // dtpStart
            // 
            this.dtpStart.CustomFormat = "yyyy-MM-dd 00:00:00";
            this.dtpStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStart.Location = new System.Drawing.Point(69, 12);
            this.dtpStart.Name = "dtpStart";
            this.dtpStart.Size = new System.Drawing.Size(200, 25);
            this.dtpStart.TabIndex = 8;
            this.dtpStart.ValueChanged += new System.EventHandler(this.dateTimePicker1_ValueChanged);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(14, 309);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(556, 23);
            this.progressBar1.TabIndex = 12;
            // 
            // bgWorkArea
            // 
            this.bgWorkArea.WorkerReportsProgress = true;
            // 
            // btnStartTown
            // 
            this.btnStartTown.Location = new System.Drawing.Point(14, 131);
            this.btnStartTown.Name = "btnStartTown";
            this.btnStartTown.Size = new System.Drawing.Size(556, 60);
            this.btnStartTown.TabIndex = 13;
            this.btnStartTown.Text = "开始187履职数据转换--->镇级";
            this.btnStartTown.UseVisualStyleBackColor = true;
            this.btnStartTown.Click += new System.EventHandler(this.btnStartTown_Click);
            // 
            // btnStartArea
            // 
            this.btnStartArea.Location = new System.Drawing.Point(14, 212);
            this.btnStartArea.Name = "btnStartArea";
            this.btnStartArea.Size = new System.Drawing.Size(556, 60);
            this.btnStartArea.TabIndex = 14;
            this.btnStartArea.Text = "开始187履职数据转换--->区级";
            this.btnStartArea.UseVisualStyleBackColor = true;
            this.btnStartArea.Click += new System.EventHandler(this.btnStartArea_Click);
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(14, 356);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(121, 15);
            this.lblInfo.TabIndex = 15;
            this.lblInfo.Text = "系统提示信息...";
            // 
            // bgWorkTown
            // 
            this.bgWorkTown.WorkerReportsProgress = true;
            // 
            // bgWorkVillage
            // 
            this.bgWorkVillage.WorkerReportsProgress = true;
            // 
            // lblChiefID
            // 
            this.lblChiefID.Location = new System.Drawing.Point(681, 9);
            this.lblChiefID.Name = "lblChiefID";
            this.lblChiefID.Size = new System.Drawing.Size(218, 25);
            this.lblChiefID.TabIndex = 16;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(604, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 15);
            this.label3.TabIndex = 17;
            this.label3.Text = "河长ID：";
            // 
            // btnRunAll
            // 
            this.btnRunAll.Location = new System.Drawing.Point(607, 55);
            this.btnRunAll.Name = "btnRunAll";
            this.btnRunAll.Size = new System.Drawing.Size(509, 60);
            this.btnRunAll.TabIndex = 18;
            this.btnRunAll.Text = "一键全启动";
            this.btnRunAll.UseVisualStyleBackColor = true;
            // 
            // frm187
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1145, 497);
            this.Controls.Add(this.btnRunAll);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblChiefID);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.btnStartArea);
            this.Controls.Add(this.btnStartTown);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dtpEnd);
            this.Controls.Add(this.dtpStart);
            this.Controls.Add(this.btnStartVillage);
            this.Name = "frm187";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "187河长履职评价";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.frm187_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStartVillage;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpEnd;
        private System.Windows.Forms.DateTimePicker dtpStart;
        public System.Windows.Forms.ProgressBar progressBar1;
        private System.ComponentModel.BackgroundWorker bgWorkArea;
        private System.Windows.Forms.Button btnStartTown;
        private System.Windows.Forms.Button btnStartArea;
        private System.Windows.Forms.Label lblInfo;
        private System.ComponentModel.BackgroundWorker bgWorkTown;
        public System.ComponentModel.BackgroundWorker bgWorkVillage;
        private System.Windows.Forms.TextBox lblChiefID;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnRunAll;
    }
}