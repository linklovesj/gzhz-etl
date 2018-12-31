namespace RiverImport.DataCompilation
{
    partial class frmImportOnePlatform
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
            this.rtbInfo = new System.Windows.Forms.RichTextBox();
            this.pBar_Base = new System.Windows.Forms.ProgressBar();
            this.btnHuanBaoData = new System.Windows.Forms.Button();
            this.bgHuanBao = new System.ComponentModel.BackgroundWorker();
            this.timer_start = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // rtbInfo
            // 
            this.rtbInfo.Location = new System.Drawing.Point(12, 362);
            this.rtbInfo.Name = "rtbInfo";
            this.rtbInfo.Size = new System.Drawing.Size(1024, 204);
            this.rtbInfo.TabIndex = 43;
            this.rtbInfo.Text = "";
            // 
            // pBar_Base
            // 
            this.pBar_Base.Location = new System.Drawing.Point(12, 79);
            this.pBar_Base.Name = "pBar_Base";
            this.pBar_Base.Size = new System.Drawing.Size(379, 10);
            this.pBar_Base.TabIndex = 50;
            // 
            // btnHuanBaoData
            // 
            this.btnHuanBaoData.Location = new System.Drawing.Point(12, 28);
            this.btnHuanBaoData.Name = "btnHuanBaoData";
            this.btnHuanBaoData.Size = new System.Drawing.Size(379, 51);
            this.btnHuanBaoData.TabIndex = 49;
            this.btnHuanBaoData.Text = "环保水质数据";
            this.btnHuanBaoData.UseVisualStyleBackColor = true;
            this.btnHuanBaoData.Click += new System.EventHandler(this.btnHuanBaoData_Click);
            // 
            // bgHuanBao
            // 
            this.bgHuanBao.WorkerReportsProgress = true;
            // 
            // timer_start
            // 
            this.timer_start.Interval = 1000;
            // 
            // frmImportOnePlatform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1091, 578);
            this.Controls.Add(this.pBar_Base);
            this.Controls.Add(this.btnHuanBaoData);
            this.Controls.Add(this.rtbInfo);
            this.Name = "frmImportOnePlatform";
            this.Text = "一体化平台数据导入";
            this.Load += new System.EventHandler(this.frmImportOnePlatform_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbInfo;
        private System.Windows.Forms.ProgressBar pBar_Base;
        private System.Windows.Forms.Button btnHuanBaoData;
        private System.ComponentModel.BackgroundWorker bgHuanBao;
        private System.Windows.Forms.Timer timer_start;
    }
}