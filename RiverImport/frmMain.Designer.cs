namespace RiverImport
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.履职ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.周报ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btn187 = new System.Windows.Forms.ToolStripButton();
            this.btnWeekReport = new System.Windows.Forms.ToolStripButton();
            this.tsbWeekAuto = new System.Windows.Forms.ToolStripButton();
            this.tsbDataDeal = new System.Windows.Forms.ToolStripButton();
            this.tsbDataCompilation = new System.Windows.Forms.ToolStripButton();
            this.tsbDateEtl = new System.Windows.Forms.ToolStripButton();
            this.tsbRiverPlotting = new System.Windows.Forms.ToolStripButton();
            this.tsbImportSection = new System.Windows.Forms.ToolStripButton();
            this.tsbImportOnePlatform = new System.Windows.Forms.ToolStripButton();
            this.tsbProvinceData = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.履职ToolStripMenuItem,
            this.周报ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1263, 25);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 履职ToolStripMenuItem
            // 
            this.履职ToolStripMenuItem.Name = "履职ToolStripMenuItem";
            this.履职ToolStripMenuItem.Size = new System.Drawing.Size(65, 21);
            this.履职ToolStripMenuItem.Text = "187履职";
            // 
            // 周报ToolStripMenuItem
            // 
            this.周报ToolStripMenuItem.Name = "周报ToolStripMenuItem";
            this.周报ToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            this.周报ToolStripMenuItem.Text = "周报";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 624);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 10, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1263, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btn187,
            this.btnWeekReport,
            this.tsbWeekAuto,
            this.tsbDataDeal,
            this.tsbDataCompilation,
            this.tsbDateEtl,
            this.tsbRiverPlotting,
            this.tsbImportSection,
            this.tsbImportOnePlatform,
            this.tsbProvinceData});
            this.toolStrip1.Location = new System.Drawing.Point(0, 25);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1263, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btn187
            // 
            this.btn187.Image = global::RiverImport.Properties.Resources._4开采强度分区图;
            this.btn187.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btn187.Name = "btn187";
            this.btn187.Size = new System.Drawing.Size(97, 22);
            this.btn187.Text = "187履职评价";
            this.btn187.Click += new System.EventHandler(this.btn187_Click);
            // 
            // btnWeekReport
            // 
            this.btnWeekReport.Image = ((System.Drawing.Image)(resources.GetObject("btnWeekReport.Image")));
            this.btnWeekReport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnWeekReport.Name = "btnWeekReport";
            this.btnWeekReport.Size = new System.Drawing.Size(100, 22);
            this.btnWeekReport.Text = "周报数据处理";
            this.btnWeekReport.Click += new System.EventHandler(this.btnWeekReport_Click);
            // 
            // tsbWeekAuto
            // 
            this.tsbWeekAuto.Image = global::RiverImport.Properties.Resources._4开采强度分区图;
            this.tsbWeekAuto.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbWeekAuto.Name = "tsbWeekAuto";
            this.tsbWeekAuto.Size = new System.Drawing.Size(124, 22);
            this.tsbWeekAuto.Text = "定时周报数据处理";
            this.tsbWeekAuto.Click += new System.EventHandler(this.tsbWeekAuto_Click);
            // 
            // tsbDataDeal
            // 
            this.tsbDataDeal.Image = global::RiverImport.Properties.Resources._4开采强度分区图;
            this.tsbDataDeal.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbDataDeal.Name = "tsbDataDeal";
            this.tsbDataDeal.Size = new System.Drawing.Size(100, 22);
            this.tsbDataDeal.Text = "数据处理工具";
            this.tsbDataDeal.Click += new System.EventHandler(this.tsbDataDeal_Click);
            // 
            // tsbDataCompilation
            // 
            this.tsbDataCompilation.Image = ((System.Drawing.Image)(resources.GetObject("tsbDataCompilation.Image")));
            this.tsbDataCompilation.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbDataCompilation.Name = "tsbDataCompilation";
            this.tsbDataCompilation.Size = new System.Drawing.Size(76, 22);
            this.tsbDataCompilation.Text = "数据整编";
            this.tsbDataCompilation.Click += new System.EventHandler(this.tsbDataCompilation_Click);
            // 
            // tsbDateEtl
            // 
            this.tsbDateEtl.Image = ((System.Drawing.Image)(resources.GetObject("tsbDateEtl.Image")));
            this.tsbDateEtl.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbDateEtl.Name = "tsbDateEtl";
            this.tsbDateEtl.Size = new System.Drawing.Size(76, 22);
            this.tsbDateEtl.Text = "数据处理";
            this.tsbDateEtl.Click += new System.EventHandler(this.tsbDateEtl_Click);
            // 
            // tsbRiverPlotting
            // 
            this.tsbRiverPlotting.Image = ((System.Drawing.Image)(resources.GetObject("tsbRiverPlotting.Image")));
            this.tsbRiverPlotting.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRiverPlotting.Name = "tsbRiverPlotting";
            this.tsbRiverPlotting.Size = new System.Drawing.Size(136, 22);
            this.tsbRiverPlotting.Text = "导入河段标绘点信息";
            this.tsbRiverPlotting.Click += new System.EventHandler(this.tsbRiverPlotting_Click);
            // 
            // tsbImportSection
            // 
            this.tsbImportSection.Image = ((System.Drawing.Image)(resources.GetObject("tsbImportSection.Image")));
            this.tsbImportSection.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbImportSection.Name = "tsbImportSection";
            this.tsbImportSection.Size = new System.Drawing.Size(124, 22);
            this.tsbImportSection.Text = "导入河涌分段信息";
            this.tsbImportSection.Click += new System.EventHandler(this.tsbImportSection_Click);
            // 
            // tsbImportOnePlatform
            // 
            this.tsbImportOnePlatform.Image = ((System.Drawing.Image)(resources.GetObject("tsbImportOnePlatform.Image")));
            this.tsbImportOnePlatform.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbImportOnePlatform.Name = "tsbImportOnePlatform";
            this.tsbImportOnePlatform.Size = new System.Drawing.Size(136, 22);
            this.tsbImportOnePlatform.Text = "导入一体化平台数据";
            this.tsbImportOnePlatform.Click += new System.EventHandler(this.tsbImportOnePlatform_Click);
            // 
            // tsbProvinceData
            // 
            this.tsbProvinceData.Image = ((System.Drawing.Image)(resources.GetObject("tsbProvinceData.Image")));
            this.tsbProvinceData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbProvinceData.Name = "tsbProvinceData";
            this.tsbProvinceData.Size = new System.Drawing.Size(112, 22);
            this.tsbProvinceData.Text = "省平台数据对接";
            this.tsbProvinceData.Click += new System.EventHandler(this.tsbProvinceData_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1263, 646);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "frmMain";
            this.Text = "广州河长管理信息系统辅助程序";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 履职ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 周报ToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btn187;
        private System.Windows.Forms.ToolStripButton btnWeekReport;
        private System.Windows.Forms.ToolStripButton tsbWeekAuto;
        private System.Windows.Forms.ToolStripButton tsbDataDeal;
        private System.Windows.Forms.ToolStripButton tsbDataCompilation;
        private System.Windows.Forms.ToolStripButton tsbDateEtl;
        private System.Windows.Forms.ToolStripButton tsbRiverPlotting;
        private System.Windows.Forms.ToolStripButton tsbImportSection;
        private System.Windows.Forms.ToolStripButton tsbImportOnePlatform;
        private System.Windows.Forms.ToolStripButton tsbProvinceData;
    }
}