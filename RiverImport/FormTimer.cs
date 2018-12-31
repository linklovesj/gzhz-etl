using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RiverImport
{
    public partial class FormTimer : Form
    {
        public FormTimer()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;
        }

        public String startTm = "";
        public String endTm = "";
        public Boolean if_timerStart = false;
        private void FormTimer_Load(object sender, EventArgs e)
        {
            dtpStart.Value = DateTime.Now.AddDays(1 - Convert.ToInt16(DateTime.Now.DayOfWeek) - 7);
            dtpEnd.Value = DateTime.Now.AddDays(7 - Convert.ToInt16(DateTime.Now.DayOfWeek) - 7);

            startTm = dtpStart.Text;
            endTm = dtpEnd.Text;

            timer1.Start();

            lblInfo.Text = "任务等待中...";
        }

        private Boolean is_running = false;
        private void timer1_Tick(object sender, EventArgs e)
        {
            dtpStart.Value = DateTime.Now.AddDays(1 - Convert.ToInt16(DateTime.Now.DayOfWeek) - 7);
            dtpEnd.Value = DateTime.Now.AddDays(7 - Convert.ToInt16(DateTime.Now.DayOfWeek) - 7);

            startTm = dtpStart.Text;
            endTm = dtpEnd.Text;

            if (!if_timerStart)
            {
                writeInfo("定时任务启动成功...");

                if_timerStart = true;
            }

            //周一三点开始跑数据
            if (DateTime.Now.DayOfWeek.ToString().Equals("Monday") && DateTime.Now.Hour == 0 && DateTime.Now.Minute == 30 && !is_running)//(!is_running)Monday,Thursday,Friday,Saturday
            {
                is_running = true;
                lblInfo.Text = "任务进行中...";

                frmWeek taskWeekForm = new frmWeek();
                taskWeekForm.startTm = startTm;
                taskWeekForm.endTm = endTm;
                taskWeekForm.progressBar1 = progressBar1;
                taskWeekForm.rtxInfo = rtxInfo;

                taskWeekForm.auto_mode = true;
                taskWeekForm.if_delAndInDB = true;
                taskWeekForm.btnStartVillage_Click(null, null);
                //taskWeekForm.button3_Click(null, null);
            }
            else if (DateTime.Now.DayOfWeek.ToString().Equals("Tuesday") && DateTime.Now.Hour == 1 && DateTime.Now.Minute == 0)//周二凌晨1点恢复
            {
                is_running = false;

                lblInfo.Text = "任务等待中...";
            }
        }

        private void writeInfo(String msg)
        {
            rtxInfo.AppendText("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + msg + "\r\n");
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
