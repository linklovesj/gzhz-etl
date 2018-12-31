namespace RiverImport.DataCompilation
{
    partial class ProvinceDataFrm
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
            this.rtbInfo = new System.Windows.Forms.RichTextBox();
            this.btnBase = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.btnGetToken = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rtbInfo
            // 
            this.rtbInfo.Location = new System.Drawing.Point(3, 465);
            this.rtbInfo.Name = "rtbInfo";
            this.rtbInfo.Size = new System.Drawing.Size(1185, 217);
            this.rtbInfo.TabIndex = 43;
            this.rtbInfo.Text = "";
            // 
            // btnBase
            // 
            this.btnBase.Location = new System.Drawing.Point(12, 95);
            this.btnBase.Name = "btnBase";
            this.btnBase.Size = new System.Drawing.Size(379, 51);
            this.btnBase.TabIndex = 48;
            this.btnBase.Text = "获取巡河记录";
            this.btnBase.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 166);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(379, 51);
            this.button1.TabIndex = 49;
            this.button1.Text = "上报巡河记录";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 246);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(379, 51);
            this.button2.TabIndex = 50;
            this.button2.Text = "获取公众投诉";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // btnGetToken
            // 
            this.btnGetToken.Location = new System.Drawing.Point(12, 29);
            this.btnGetToken.Name = "btnGetToken";
            this.btnGetToken.Size = new System.Drawing.Size(379, 51);
            this.btnGetToken.TabIndex = 51;
            this.btnGetToken.Text = "获取Token";
            this.btnGetToken.UseVisualStyleBackColor = true;
            this.btnGetToken.Click += new System.EventHandler(this.btnGetToken_Click);
            // 
            // ProvinceDataFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 694);
            this.Controls.Add(this.btnGetToken);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnBase);
            this.Controls.Add(this.rtbInfo);
            this.Name = "ProvinceDataFrm";
            this.Text = "省平台数据对接";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbInfo;
        private System.Windows.Forms.Button btnBase;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnGetToken;
    }
}