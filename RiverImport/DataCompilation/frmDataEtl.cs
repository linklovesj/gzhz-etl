using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sunny.Misc;
using System.Globalization;
using System.Diagnostics;

namespace RiverImport.DataCompilation
{
    public partial class frmDataEtl : Form
    {
        public frmDataEtl()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;
        }

        String start_time = "";
        String end_time = "";
        public bool if_delAndInDB = false;
        public bool auto_mode = false;
        public delegate void WriteTxtInvoke(string msg);
        WriteTxtInvoke wt;

        public bool citySwitch = false;
        public bool areaSwitch = false;
        public bool townSwitch = false;
        public bool villageSwitch = false;

        public bool fixedPeriodWitch = false;

        public void UpdateForm(string msg)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new WriteTxtInvoke(UpdateForm), msg);
            }
            else
            {
                rtbInfo.AppendText("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + msg + "\r\n");
            }
        }

        private void frmDataEtl_Load(object sender, EventArgs e)
        {
            dtpStart.Value = DateTime.Now.AddDays(-1);

            dtpEnd.Value = DateTime.Now.AddDays(-1);

            start_time = dtpStart.Text;
            end_time = dtpEnd.Text;

            wt = new WriteTxtInvoke(UpdateForm);

            //timer_start.Start();
            //lblInfo.Text = "任务等待中...";
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (progressBar1.Value < progressBar1.Maximum)
                progressBar1.Value += 1;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = progressBar1.Maximum;
        }

        public void writeInfo(String msg)
        {
            try
            {

                if (this.rtbInfo.InvokeRequired)
                {
                    //this.BeginInvoke(wt, new Object[] { msg });
                }
                else
                {
                    rtbInfo.AppendText("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + msg + "\r\n");
                }
            }
            catch(Exception)
            {
            }
        }

        public void writeProgressInfo(String msg,RichTextBox rtb)
        {
            rtb.AppendText("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + msg + "\r\n");
        }

        private void btnPatrol_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgWorker_Patrol.DoWork += new DoWorkEventHandler(worker_DoWorkPatrol);
            bgWorker_Patrol.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgWorker_Patrol.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgWorker_Patrol.RunWorkerAsync();
        }

        void worker_DoWorkPatrol(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(巡河数据:RTD_PATROL_INFO)...";
            //writeInfo(lblInfo.Text);
            this.BeginInvoke(wt, new Object[] { lblInfo.Text });
            writeProgressInfo(lblInfo.Text, rtbPatrol);

            runPatrol(progressBar1, bgWorker_Patrol);

            lblInfo.Text = "数据处理完成(巡河数据:RTD_PATROL_INFO)";
            this.BeginInvoke(wt, new Object[] { lblInfo.Text });
            writeProgressInfo(lblInfo.Text, rtbPatrol);
        }

        public void runPatrol(ProgressBar pBar, BackgroundWorker bgWork)
        {
            //DateTime dt_start = Convert.ToDateTime(start_time);
            DateTime dt_end = Convert.ToDateTime(end_time);

            String sql = "";

            sql = "select id,type,addvcd_area,addvcd_town,addvcd_village,substr(parent_ids, 0, instr(parent_ids, ',') - 1) river_id,river_side,parent_ids from rm_river_lake";

            String riverType = "";
            if (citySwitch)
                riverType += "'1','2','9','13',";
            if (areaSwitch)
                riverType += "'2','3','6','9','10','13',";
            if (townSwitch)
                riverType += "'2','4','7','9','11','13',";
            if (villageSwitch)
                riverType += "'2','5','8','9','12','13'";

            riverType = riverType.TrimEnd(',');

            sql += " where type in(" + riverType + ")";

            DataTable dtRiver = SqlHelper2.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dtRiver.Rows.Count;
            for (var i = 0; i < dtRiver.Rows.Count; i++)
            {
                String type = dtRiver.Rows[i]["type"].ToString();
                String river_id = dtRiver.Rows[i]["river_id"].ToString();
                String section_id = dtRiver.Rows[i]["id"].ToString();
                String side = dtRiver.Rows[i]["river_side"].ToString();

                String parent_ids = dtRiver.Rows[i]["parent_ids"].ToString();
                String[] ids=parent_ids.Split(',');

                String area_section = null;
                String town_section = null;

                if (ids.Length >= 2)
                {
                    area_section = ids[1];
                }

                if (ids.Length >= 3)
                {
                    town_section = ids[2];
                }

                String addvcd_city = "440100000000";
                String addvcd_area = dtRiver.Rows[i]["addvcd_area"].ToString();
                String addvcd_town = dtRiver.Rows[i]["addvcd_town"].ToString();
                String addvcd_village = dtRiver.Rows[i]["addvcd_village"].ToString();

                //writeInfo((i + 1) + ":" + section_id);
                //this.BeginInvoke(wt, new Object[] { (i + 1) + ":" + section_id });
                writeProgressInfo((i + 1) + "/" + dtRiver.Rows.Count + ":" + section_id, rtbPatrol);

                //市级河段 每季度不少于1次
                DateTime dt_start = Convert.ToDateTime(start_time);

                while (dt_start <= dt_end)
                {
                    if ((type.Equals("1") || type.Equals("2") || type.Equals("9") || type.Equals("13")) && citySwitch)
                    {
                        if (dt_start.Day != 1 || (dt_start.Month == 2 || dt_start.Month == 3 || dt_start.Month == 5 || dt_start.Month == 6 || dt_start.Month == 8 || dt_start.Month == 9 || dt_start.Month == 11 || dt_start.Month == 12))
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddMonths(-3).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getCityChiefPeriod(dt_start.AddMonths(-3));
                        String cycle = dt_start.AddMonths(-3).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('1','27','3','2','39','4','28','29','30','40','41','42') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper2.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            String river_type = dtUserInfo.Rows[j]["river_type"].ToString();

                            //====================巡河数据=======================
                            patrolCount(userid, cycle, section_id, startTm, endTm, true, river_type);
                        }
                    }
                    //区级河段 一般河湖：每月不少于1次；黑臭河湖：每月巡查不少于一次，每次巡查完成不低于本辖区黑臭水体总条数的15%，每半年完成一轮全覆盖巡查；
                    if ((type.Equals("3") || type.Equals("2") || type.Equals("6") || type.Equals("9") || type.Equals("10") || type.Equals("13")) && areaSwitch)
                    {
                        if (dt_start.Day != 1 || (dt_start.Month == 2 || dt_start.Month == 4 || dt_start.Month == 6 || dt_start.Month == 8 || dt_start.Month == 10 || dt_start.Month == 12))
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddMonths(-2).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getAreaChiefPeriod(dt_start.AddMonths(-2));
                        String cycle = dt_start.AddMonths(-2).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('7','8','9','31','32','33','43','44','45') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper2.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            String river_type = dtUserInfo.Rows[j]["river_type"].ToString();

                            //====================巡河数据=======================
                            patrolCount(userid, cycle, section_id, startTm, endTm, true, river_type);
                        }
                    }
                    //镇级河段 一般河湖：每旬巡查不少于一次；黑臭河湖：每周巡查不少于一次，每次完成不低于本辖区黑臭水体总数的30%，每月完成一轮全覆盖巡查；
                    if ((type.Equals("4") || type.Equals("2") || type.Equals("7") || type.Equals("9") || type.Equals("11") || type.Equals("13")) && townSwitch)
                    {
                        if (Convert.ToInt32(dt_start.DayOfWeek) != 1)
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getWeek(dt_start.AddDays(-7));
                        String cycle = dt_start.AddDays(-7).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('12','50','13','34','35','36','46','47','48') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper2.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            String river_type = dtUserInfo.Rows[j]["river_type"].ToString();

                            //====================巡河数据=======================
                            patrolCount(userid, cycle, section_id, startTm, endTm, true, river_type);
                        }
                    }
                    //村级河段 一般河湖：每周巡查不少于一次；黑臭河湖：每个工作日一巡，每周完成黑臭水体全覆盖巡查；
                    if ((type.Equals("5") || type.Equals("2") || type.Equals("8") || type.Equals("9") || type.Equals("12") || type.Equals("13")) && villageSwitch)
                    {
                        String startTm = dt_start.ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.ToString("yyyy-MM-dd 23:59:59");

                        int periods = dt_start.DayOfYear;
                        String cycle = dt_start.Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('16','37','49') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper2.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            String river_type = dtUserInfo.Rows[j]["river_type"].ToString();

                            //====================巡河数据=======================
                            patrolCount(userid, cycle, section_id, startTm, endTm, true,river_type);
                        }
                    }

                    dt_start = dt_start.AddDays(1);
                }

                //lblInfo.Text = dtRiver.Rows[i][0].ToString();
                bgWork.ReportProgress(i + 1);
            }
        }

        private void patrolCount(String userid,String cycle,String section_id,String startTm,String endTm,Boolean ifDelOld,String river_type)
        {
            //获取base_id
            String sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

            DataTable dtBaseInfo = SqlHelper2.GetDataTable(sql, null);
            if (dtBaseInfo.Rows.Count <= 0)
                return;
            String base_id = dtBaseInfo.Rows[0][0].ToString();

            //====================巡河数据=======================
            //巡河天数
            sql = "select count(distinct to_char(strtime,'yyyy-MM-dd')) nums from ptl_patrol_r where userid='" + userid + "' and strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            DataTable dtXunHeTianShu = SqlHelper2.GetDataTable(sql, null);
            String partol_day = dtXunHeTianShu.Rows[0][0].ToString();
            //巡河总时间	巡河总里程	巡河次数
            sql = "select nvl(sum(duration),0) total_time,nvl(sum(distance),0) total_distance,count(patrolid) total_times from ptl_patrol_r where userid='" + userid + "' and strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and endtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            DataTable dtXunHeAll = SqlHelper2.GetDataTable(sql, null);
            String total_time = dtXunHeAll.Rows[0]["total_time"].ToString();
            String total_distance = dtXunHeAll.Rows[0]["total_distance"].ToString();
            String total_times = dtXunHeAll.Rows[0]["total_times"].ToString();
            //有效巡河次数 有效巡河时长 有效巡河里程
            sql = "select nvl(sum(duration),0) total_time,nvl(sum(distance),0) total_distance,count(patrolid) total_times from ptl_patrol_r where duration>=10 and userid='" + userid + "' and strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and endtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            DataTable dtXunHeValidAll = SqlHelper2.GetDataTable(sql, null);
            String valid_total_time = dtXunHeValidAll.Rows[0]["total_time"].ToString();
            String valid_total_distance = dtXunHeValidAll.Rows[0]["total_distance"].ToString();
            String valid_total_times = dtXunHeValidAll.Rows[0]["total_times"].ToString();
            //有效巡河天数
            sql = "select count(distinct to_char(strtime,'yyyy-MM-dd')) nums from ptl_patrol_r where duration>=10 and userid='" + userid + "' and strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and endtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            DataTable dtValidXunHeTianShu = SqlHelper2.GetDataTable(sql, null);
            String valid_partol_day = dtValidXunHeTianShu.Rows[0][0].ToString();
            //writeInfo(valid_partol_day);
            //无效巡河次数
            sql = "select count(patrolid) total_times from ptl_patrol_r where duration<10 and userid='" + userid + "' and strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and endtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            DataTable dtInvalidXunHeCiShu = SqlHelper2.GetDataTable(sql, null);
            String invalid_partol_times = dtInvalidXunHeCiShu.Rows[0][0].ToString();
            //多样化巡河次数
            sql = "select count(id) from ptl_patrol_other where user_id='" + userid + "' and tm>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and tm<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            DataTable dtOtherXunHeCiShu = SqlHelper2.GetDataTable(sql, null);
            String others_partol_times = dtOtherXunHeCiShu.Rows[0][0].ToString();

            //总的有效巡河天数，含多样化巡河天数
            sql = "select count(tm) nums from"
                + "("
                + "    select distinct userid,to_char(strtime,'yyyy-MM-dd') tm from ptl_patrol_r where userid='" + userid + "' and strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                + "    union all"
                + "    select to_char(user_id) as userid,to_char(tm,'yyyy-MM-dd') tm from ptl_patrol_other where user_id='" + userid + "' and tm>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and tm<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                + ")";
            String ALL_PATROL_DAYS = SqlHelper2.ExecuteScalar(sql, CommandType.Text, null).ToString();
            //上报问题数
            sql = "select count(problemid) nums from ptl_reportproblem where userid='"+userid+"' and state<>'-1'"
               + " and (sectionid='" + section_id + "' or sectionid in(select id from rm_river_lake where parent_ids like '%" + section_id + "%'))"
               + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') ";

            String REPORT_PROBLEMS = SqlHelper2.ExecuteScalar(sql, CommandType.Text, null).ToString();
            //插入RTD_PATROL_INFO 表
            String patrolId = Guid.NewGuid().ToString().Replace("-", "");

            if (if_delAndInDB)
            {
                if (ifDelOld)
                {
                    sql = "delete from rtd_patrol_info where base_id='" + base_id + "'";

                    SqlHelper2.ExecuteNonQuery(sql, CommandType.Text, null);
                }
                sql = "Insert into RTD_PATROL_INFO (ID,BASE_ID,TOTAL_DAYS,TOTAL_TIME,TOTAL_DISTANCE,TOTAL_TIMES,INVALID_TOTAL_TIMES,VALID_TOTAL_TIMES,VALID_TOTAL_DAYS,VALID_PATROL_TIME,VALID_TOTAL_DISTANCE,OTHER_TOTAL_DAYS,"
                    + "START_TIME,END_TIME,user_id,ALL_PATROL_DAYS,REPORT_PROBLEMS,section_id,cycle,river_type)"
                    + " values ('" + patrolId + "','" + base_id + "','" + partol_day + "','" + total_time + "','" + total_distance + "','" + total_times + "','" + invalid_partol_times + "','" + valid_total_times + "','" + valid_partol_day + "','" + valid_total_time + "','" + valid_total_distance + "','" + others_partol_times + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')" + ",to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'),'"
                    + userid + "','" + ALL_PATROL_DAYS + "','" + REPORT_PROBLEMS + "','" + section_id + "','" + cycle + "','" + river_type + "')";
                SqlHelper2.ExecuteNonQuery(sql, CommandType.Text, null);
            }
        }

        private int getWeek(DateTime dt)
        {
            GregorianCalendar gc = new GregorianCalendar();

            int week = gc.GetWeekOfYear(dt, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

            return week;
        }

        private int getCityChiefPeriod(DateTime dt)
        {
            if (dt.Month == 1)
                return 1;
            else if (dt.Month == 4)
                return 2;
            else if (dt.Month == 7)
                return 3;
            else if (dt.Month == 10)
                return 4;
            else
                return -1;

        }

        private int getAreaChiefPeriod(DateTime dt)
        {
            if (dt.Month == 1)
                return 1;
            else if (dt.Month == 3)
                return 2;
            else if (dt.Month == 5)
                return 3;
            else if (dt.Month == 7)
                return 4;
            else if (dt.Month == 9)
                return 5;
            else if (dt.Month == 11)
                return 6;
            else
                return -1;

        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            DateTime start_time = Convert.ToDateTime("2019-07-16 00:00:00");
            DateTime end_time = Convert.ToDateTime("2025-02-25 00:00:00");

            int x=100;
            MessageBox.Show(DateTime.Now.Year+x.ToString("D3"));

            //村河长
            /*int period = 17;
            while (start_time <= end_time)
            {
                String startTm = start_time.ToString("yyyy-MM-dd HH:mm:ss");
                String endTm = start_time.AddDays(14).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss");

                writeInfo(startTm + "---" + endTm);

                if (if_delAndInDB)
                {
                    String id = Guid.NewGuid().ToString().Replace("-", "");
                    String sql = "insert into ptl_fourcheck_phase(id,type,phase,start_date,end_date) values('"
                        + id + "','1','" + period + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss'),to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

                    SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                }
                start_time = start_time.AddDays(14);
                period++;
            }*/

            //镇河长
            /*int period = 5;
            while (start_time <= end_time)
            {
                String startTm = start_time.ToString("yyyy-MM-dd HH:mm:ss");
                String endTm = start_time.AddMonths(2).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss");

                writeInfo(startTm + "---" + endTm);

                if (if_delAndInDB)
                {
                    String id = Guid.NewGuid().ToString().Replace("-", "");
                    String sql = "insert into ptl_fourcheck_phase(id,type,phase,start_date,end_date) values('"
                        + id + "','2','" + period + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss'),to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

                    SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                }
                start_time = start_time.AddMonths(2);
                period++;
            }*/

            /*int period = 3;
            while (start_time <= end_time)
            {
                String startTm = start_time.ToString("yyyy-MM-dd HH:mm:ss");
                String endTm = start_time.AddMonths(6).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss");

                writeInfo(startTm + "---" + endTm);

                if (if_delAndInDB)
                {
                    String id = Guid.NewGuid().ToString().Replace("-", "");
                    String sql = "insert into ptl_fourcheck_phase(id,type,phase,start_date,end_date) values('"
                        + id + "','3','" + period + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss'),to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

                    SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                }
                start_time = start_time.AddMonths(6);
                period++;
            }*/
        }

        private void chkDelAndInDB_CheckedChanged(object sender, EventArgs e)
        {
            if_delAndInDB = chkDelAndInDB.Checked;
        }

        private void dtpStart_ValueChanged(object sender, EventArgs e)
        {
            start_time = dtpStart.Text;
        }

        private void dtpEnd_ValueChanged(object sender, EventArgs e)
        {
            end_time = dtpEnd.Text;
        }

        private void btnProblem_Click(object sender, EventArgs e)
        {
            pBarProblem.Value = 0;
            pBarProblem.Minimum = 0;

            bgWorker_Problem.DoWork += new DoWorkEventHandler(worker_DoWorkProblem);
            bgWorker_Problem.ProgressChanged += new ProgressChangedEventHandler(workerProblem_ProgressChanged);
            bgWorker_Problem.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerProblem_RunWorkerCompleted);

            bgWorker_Problem.RunWorkerAsync();
        }

        void worker_DoWorkProblem(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(问题数据:RTD_PROBLEM_SOURCE)...";
            //writeInfo(lblInfo.Text);
            this.BeginInvoke(wt, new Object[] { lblInfo.Text });
            writeProgressInfo(lblInfo.Text, rtbProblem);

            runProblem(pBarProblem, bgWorker_Problem);

            lblInfo.Text = "数据处理完成(问题数据:RTD_PROBLEM_SOURCE)";
            this.BeginInvoke(wt, new Object[] { lblInfo.Text });
            writeProgressInfo(lblInfo.Text, rtbProblem);
        }

        public void runProblem(ProgressBar pBar, BackgroundWorker bgWork)
        {
            //DateTime dt_start = Convert.ToDateTime(start_time);
            DateTime dt_end = Convert.ToDateTime(end_time);

            String sql = "";

            sql = "select id,type,addvcd_area,addvcd_town,addvcd_village,substr(parent_ids, 0, instr(parent_ids, ',') - 1) river_id,river_side,parent_ids from rm_river_lake";

            String riverType="";
            if (citySwitch)
                riverType += "'1','2','9','13',";
            if (areaSwitch)
                riverType += "'2','3','6','9','10','13',";
            if (townSwitch)
                riverType += "'2','4','7','9','11','13',";
            if (villageSwitch)
                riverType += "'2','5','8','9','12','13'";

            riverType = riverType.TrimEnd(',');

            sql += " where type in(" + riverType + ")";

            DataTable dtRiver = SqlHelper3.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dtRiver.Rows.Count;
            for (var i = 0; i < dtRiver.Rows.Count; i++)
            {
                String type = dtRiver.Rows[i]["type"].ToString();
                String river_id = dtRiver.Rows[i]["river_id"].ToString();
                String section_id = dtRiver.Rows[i]["id"].ToString();
                String side = dtRiver.Rows[i]["river_side"].ToString();

                String parent_ids = dtRiver.Rows[i]["parent_ids"].ToString();
                String[] ids = parent_ids.Split(',');

                String area_section = null;
                String town_section = null;

                if (ids.Length >= 2)
                {
                    area_section = ids[1];
                }

                if (ids.Length >= 3)
                {
                    town_section = ids[2];
                }

                String addvcd_city = "440100000000";
                String addvcd_area = dtRiver.Rows[i]["addvcd_area"].ToString();
                String addvcd_town = dtRiver.Rows[i]["addvcd_town"].ToString();
                String addvcd_village = dtRiver.Rows[i]["addvcd_village"].ToString();

                //writeInfo((i + 1) + ":" + section_id);
                //this.BeginInvoke(wt, new Object[] { (i + 1) + ":" + section_id });
                writeProgressInfo((i + 1) + "/" + dtRiver.Rows.Count + ":" + section_id, rtbProblem);

                //市级河段 每季度不少于1次
                DateTime dt_start = Convert.ToDateTime(start_time);

                while (dt_start <= dt_end)
                {
                    if ((type.Equals("1") || type.Equals("2") || type.Equals("9") || type.Equals("13")) && citySwitch)
                    {
                        if (dt_start.Day != 1 || (dt_start.Month == 2 || dt_start.Month == 3 || dt_start.Month == 5 || dt_start.Month == 6 || dt_start.Month == 8 || dt_start.Month == 9 || dt_start.Month == 11 || dt_start.Month == 12))
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddMonths(-3).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getCityChiefPeriod(dt_start.AddMonths(-3));
                        String cycle = dt_start.AddMonths(-3).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('1','27','3','2','39','4','28','29','30','40','41','42') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper3.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper3.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();

                            //================问题=====================
                            problemCount(userid, cycle, section_id, base_id, startTm, endTm, true, type);
                        }
                    }
                    //区级河段 一般河湖：每月不少于1次；黑臭河湖：每月巡查不少于一次，每次巡查完成不低于本辖区黑臭水体总条数的15%，每半年完成一轮全覆盖巡查；
                    if ((type.Equals("3") || type.Equals("2") || type.Equals("6") || type.Equals("9") || type.Equals("10") || type.Equals("13")) && areaSwitch)
                    {
                       if (dt_start.Day != 1 || (dt_start.Month == 2 || dt_start.Month == 4 || dt_start.Month == 6 || dt_start.Month == 8 || dt_start.Month == 10 || dt_start.Month == 12))
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddMonths(-2).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getAreaChiefPeriod(dt_start.AddMonths(-2));
                        String cycle = dt_start.AddMonths(-2).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('7','8','9','31','32','33','43','44','45') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper3.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper3.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();

                            //================问题=====================
                            problemCount(userid, cycle, section_id, base_id, startTm, endTm, true, type);
                        }
                    }
                    //镇级河段 一般河湖：每旬巡查不少于一次；黑臭河湖：每周巡查不少于一次，每次完成不低于本辖区黑臭水体总数的30%，每月完成一轮全覆盖巡查；
                    if ((type.Equals("4") || type.Equals("2") || type.Equals("7") || type.Equals("9") || type.Equals("11") || type.Equals("13")) && townSwitch)
                    {
                        if (Convert.ToInt32(dt_start.DayOfWeek) != 1)
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getWeek(dt_start.AddDays(-7));
                        String cycle = dt_start.AddDays(-7).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('12','50','13','34','35','36','46','47','48') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper3.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper3.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();

                            //================问题=====================
                            problemCount(userid, cycle, section_id, base_id, startTm, endTm, true, type);
                        }
                    }
                    //村级河段 一般河湖：每周巡查不少于一次；黑臭河湖：每个工作日一巡，每周完成黑臭水体全覆盖巡查；
                    if ((type.Equals("5") || type.Equals("2") || type.Equals("8") || type.Equals("9") || type.Equals("12") || type.Equals("13")) && villageSwitch)
                    {
                        String startTm = dt_start.ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.ToString("yyyy-MM-dd 23:59:59");

                        int periods = dt_start.DayOfYear;
                        String cycle = dt_start.Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('16','37','49') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper3.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();

                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper3.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();
                            //================问题=====================
                            problemCount(userid, cycle, section_id, base_id, startTm, endTm, true, type);
                        }
                    }

                    dt_start = dt_start.AddDays(1);
                }

                //==============================跑固定周期的数据====================================
                if (fixedPeriodWitch)//跑固定周期数据开关是否勾上
                {
                    List<Dictionary<String, Object>> cycleList = getCycleList();
                    for (int index = 0; index < cycleList.Count; index++)
                    {

                        String startTm = cycleList[index]["startTm"].ToString();
                        String endTm = cycleList[index]["endTm"].ToString();
                        String cycle = cycleList[index]["cycle"].ToString();

                        if ((type.Equals("1") || type.Equals("2") || type.Equals("9") || type.Equals("13")) && citySwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('1','27','3','2','39','4','28','29','30','40','41','42') and del_flag='0' and "
                                    + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper3.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper3.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题=====================
                                problemCount(userid, cycle, section_id, base_id, startTm, endTm, true, type);
                            }
                        }
                        if ((type.Equals("3") || type.Equals("2") || type.Equals("6") || type.Equals("9") || type.Equals("10") || type.Equals("13")) && areaSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('7','8','9','31','32','33','43','44','45') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper3.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();

                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper3.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题=====================
                                problemCount(userid, cycle, section_id, base_id, startTm, endTm, true, type);
                            }
                        }
                        if ((type.Equals("4") || type.Equals("2") || type.Equals("7") || type.Equals("9") || type.Equals("11") || type.Equals("13")) && townSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('12','50','13','34','35','36','46','47','48') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper3.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper3.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题=====================
                                problemCount(userid, cycle, section_id, base_id, startTm, endTm, true, type);
                            }
                        }
                        if ((type.Equals("5") || type.Equals("2") || type.Equals("8") || type.Equals("9") || type.Equals("12") || type.Equals("13")) && villageSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('16','37','49') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper3.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper3.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题=====================
                                problemCount(userid, cycle, section_id, base_id, startTm, endTm, true, type);
                            }
                        }
                    }
                }

                //lblInfo.Text = dtRiver.Rows[i][0].ToString();
                bgWork.ReportProgress(i + 1);
            }
        }

        //问题统计
        private void problemCount(String userId, String cycle, String section_id, String base_id, String startTm, String endTm, Boolean ifDelOldData, String section_type)
        {
            String allSql = "";
            //================问题=====================
            //河长上报问题数									
            String sql = "select count(problemid),'河长上报问题数' type,1 orders from ptl_reportproblem where state<>'-1' and sectionid in"
                + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
                + " and userid in(select id from sys_user_b where is_riverchief='Y')"
                + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //民间河长上报问题数
            sql = "select count(problemid) nums,'民间河长上报问题数' type,2 orders from ptl_reportproblem where state<>'-1' and sectionid in"
                + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
                + " and userid in(select id from sys_user_b where river_type='25')"
                + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //市民投诉问题数
            sql = "select count(problemid) nums,'市民投诉问题数' type,3 orders from ptl_reportproblem where state<>'-1' and sectionid in"
                + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
                + " and IS_CITIZEN='Y'"
                + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //市巡查投诉问题数
            sql = "select count(problemid) nums,'市巡查投诉问题数' type,4 orders from ptl_reportproblem where state<>'-1' and sectionid in"
                + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
                + " and (userid in(select id from sys_user_b where river_type='21') or IS_CITIZEN='N')"
                + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //媒体曝光问题数
            sql = "select count(problemid) nums,'媒体曝光问题数' type,5 orders from ptl_reportproblem where state<>'-1' and sectionid in"
                + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
                + " and pro_resource='5'"
                + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //舆情问题数
            sql = "select count(problemid) nums,'舆情问题数' type,6 orders from ptl_reportproblem where state<>'-1' and sectionid in"
                + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
                + " and pro_resource='3'"
                + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //暗访发现问题问题数
            sql = "select count(problemid) nums,'暗访发现问题问题数' type,7 orders from ptl_reportproblem where state<>'-1' and sectionid in"
                + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
                + " and pro_resource='6'"
               + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //微信投诉问题数
            sql = "select count(problemid) nums,'微信投诉问题数' type,8 orders from ptl_reportproblem where state<>'-1' and sectionid in"
                + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
                + " and pro_resource='2'"
               + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //电话投诉问题数
            sql = "select count(problemid) nums,'电话投诉问题数' type,9 orders from ptl_reportproblem where state<>'-1' and sectionid in"
                + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
                + " and pro_resource='1'"
                + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //自行处理问题数
            sql = "select count(problemid) nums,'自行处理问题数' type,10 orders from ptl_reportproblem where state<>'-1' and sectionid in"
                + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
                + " and NATIVEPROCESSING='1'"
                + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //===========================问题整改-按问题来源=============================
            //办结问题数
            sql = "select count(problemid) nums,'办结问题数' type,11 orders from ptl_reportproblem where state='3' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //超期未办结问题数	
            sql = "select count(problemid) nums,'超期未办结问题数' type,12 orders from ptl_reportproblem where round(to_number(sysdate-time))"
            + ">(select value from sys_dict where type='problem_deadline' and label=type) and sectionid in"
            + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
            + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //挂账问题数	
            sql = "select count(problemid) nums,'挂账问题数' type,13 orders from ptl_reportproblem where state<>'-1' and sectionid in"
               + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
               + " and problemid in(select problem_id from ptl_problem_account where "
               + "apply_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and apply_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            allSql += sql + " union ";
            //公众上报问题办结数	
            sql = "select count(problemid) nums,'公众上报问题办结数' type,14 orders from ptl_reportproblem where state='3' and  sectionid in"
              + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
              + " and IS_CITIZEN='Y'and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //公众上报问题超期数	
            sql = "select count(problemid) nums,'公众上报问题超期数' type,15 orders from ptl_reportproblem where state<>'-1' and round(to_number(sysdate-time))"
              + ">(select value from sys_dict where type='problem_deadline' and label=type) and sectionid in"
              + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
              + " and IS_CITIZEN='Y' and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //公众上报问题挂账数	
            sql = "select count(problemid) nums,'公众上报问题挂账数' type,16 orders from ptl_reportproblem where state<>'-1' and  sectionid in"
              + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
              + " and IS_CITIZEN='Y'and problemid in(select problem_id from ptl_problem_account where "
               + "apply_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and apply_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            allSql += sql + " union ";
            //市巡查投诉问题办结数	
            sql = "select count(problemid) nums,'市巡查投诉问题办结数' type,17 orders from ptl_reportproblem where state='3' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and (userid in(select id from sys_user_b where river_type='21') or IS_CITIZEN='N')"
              + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //市巡查投诉问题超期未办结数	
            sql = "select count(problemid) nums,'市巡查投诉问题超期未办结数' type,18 orders from ptl_reportproblem where state<>'-1'and round(to_number(sysdate-time))"
             + ">(select value from sys_dict where type='problem_deadline' and label=type) and sectionid in"
             + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
             + " and (userid in(select id from sys_user_b where river_type='21') or IS_CITIZEN='N')"
             + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //市巡查投诉问题挂账数	
            sql = "select count(problemid) nums,'市巡查投诉问题挂账数' type,19 orders from ptl_reportproblem where state<>'-1' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and (userid in(select id from sys_user_b where river_type='21') or IS_CITIZEN='N')"
              + " and problemid in(select problem_id from ptl_problem_account where "
              + "apply_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and apply_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            allSql += sql + " union ";
            //河长上报问题挂账数	
            sql = "select nvl(count(problemid),0) nums,'河长上报问题挂账数' type,20 orders from ptl_reportproblem where state<>'-1'"
               + " and userid in(select id from sys_user_b where is_riverchief='Y') and sectionid in"
               + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
               + " and problemid in(select problem_id from ptl_problem_account where "
               + "apply_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and apply_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            allSql += sql + " union ";
            //河长上报问题办结数	
            sql = "select nvl(count(problemid),0) nums,'河长上报问题办结数' type,21 orders from ptl_reportproblem where state='3' and "
               + "time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
               + " and userid in(select id from sys_user_b where is_riverchief='Y') and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')";

            //河长上报问题复核数

            allSql += sql + " union ";
            //电话上报问题挂账数	
            sql = "select count(problemid) nums,'电话上报问题挂账数' type,22 orders from ptl_reportproblem where state<>'-1' and pro_resource='1' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and problemid in(select problem_id from ptl_problem_account where "
              + "apply_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and apply_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            allSql += sql + " union ";
            //电话上报问题办结数	
            sql = "select count(problemid) nums,'电话上报问题办结数' type,23 orders from ptl_reportproblem where state='3' and pro_resource='1' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //河长上报问题复核数
            sql = "select count(problemid) nums,'河长上报问题复核数' type,24 orders from ptl_reportproblem where state='4' and sectionid in"
            + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
            + " and userid in(select id from sys_user_b where is_riverchief='Y')"
            + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //河长上报问题超期数
            sql = "select count(problemid) nums,'河长上报问题超期数' type,25 orders from ptl_reportproblem where state<>'-1' and round(to_number(sysdate-time))"
             + ">(select value from sys_dict where type='problem_deadline' and label=type) and sectionid in"
            + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
            + " and userid in(select id from sys_user_b where is_riverchief='Y')"
            + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //河长上报问题处理中数 
            sql = "select count(problemid) nums,'河长上报问题处理中数' type,26 orders from ptl_reportproblem where state not in ('-1','3','4','11') and sectionid in"
            + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
            + " and userid in(select id from sys_user_b where is_riverchief='Y')"
            + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //公众上报问题处理中数
            sql = "select count(problemid) nums,'公众上报问题处理中数' type,27 orders from ptl_reportproblem where state not in ('-1','3','4','11') and  sectionid in"
              + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
              + " and IS_CITIZEN='Y' and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //公众上报问题复核数
            sql = "select count(problemid) nums,'公众上报问题复核数' type,28 orders from ptl_reportproblem where state ='4' and  sectionid in"
              + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
              + " and IS_CITIZEN='Y' and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //市巡查(app)问题处理中数
            sql = "select count(problemid) nums,'市巡查(app)问题处理中数' type,29 orders from ptl_reportproblem where state not in ('-1','3','4','11')  and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and (userid in(select id from sys_user_b where river_type='21'))"
              + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //市巡查(app)问题复核数
            sql = "select count(problemid) nums,'市巡查(app)问题复核数' type,30 orders from ptl_reportproblem where state='4' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and (userid in(select id from sys_user_b where river_type='21'))"
              + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //市巡查(微信)问题处理中数
            sql = "select count(problemid) nums,'市巡查(微信)问题处理中数' type,31 orders from ptl_reportproblem where state not in ('-1','3','4','11') and pro_resource='2' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and IS_CITIZEN='N'"
              + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //市巡查(微信)问题已办结数
            sql = "select count(problemid) nums,'市巡查(微信)问题已办结数' type,32 orders from ptl_reportproblem where state='3' and pro_resource='2' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and IS_CITIZEN='N'"
              + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //市巡查(微信)问题已复核数
            sql = "select count(problemid) nums,'市巡查(微信)问题已复核数' type,33 orders from ptl_reportproblem where state='4' and pro_resource='2' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and IS_CITIZEN='N'"
              + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //市巡查(微信)问题挂账数
            sql = "select count(problemid) nums,'市巡查(微信)问题挂账数' type,34 orders from ptl_reportproblem where state<>'-1' and pro_resource='2' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and IS_CITIZEN='N'"
              + " and problemid in(select problem_id from ptl_problem_account where "
              + "apply_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and apply_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            allSql += sql + " union ";
            //市巡查(微信)问题超期数
            sql = "select count(problemid) nums,'市巡查(微信)问题超期数' type,35 orders from ptl_reportproblem where state<>'-1' and pro_resource='2' and round(to_number(sysdate-time))"
              + ">(select value from sys_dict where type='problem_deadline' and label=type)and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and IS_CITIZEN='N'"
              + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //电话上报处理中数
            sql = "select count(problemid) nums,'电话上报处理中数' type,36 orders from ptl_reportproblem where state not in ('-1','3','4','11') and pro_resource='1' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //电话上报复核数
            sql = "select count(problemid) nums,'电话上报复核数' type,37 orders from ptl_reportproblem where state='4' and pro_resource='1' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //电话上报超期数
            sql = "select count(problemid) nums,'电话上报超期数' type,38 orders from ptl_reportproblem where  state<>'-1' and pro_resource='1'and round(to_number(sysdate-time))"
              + ">(select value from sys_dict where type='problem_deadline' and label=type) and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " order by orders";
            DataTable dtData = SqlHelper3.GetDataTable(allSql, null);

            String chief_report_nums = dtData.Rows[0][0].ToString();
            String folkchief_report_nums = dtData.Rows[1][0].ToString();
            String citizen_report_nums = dtData.Rows[0][2].ToString();
            String citizen_patrol_report_nums = dtData.Rows[3][0].ToString();
            String media_report_nums = dtData.Rows[4][0].ToString();
            String public_report_nums = dtData.Rows[5][0].ToString();//舆情问题数
            String secret_report_nums = dtData.Rows[6][0].ToString();
            String wechat_problems = dtData.Rows[7][0].ToString();
            String phone_report_nums = dtData.Rows[8][0].ToString();
            String native_process_nums = dtData.Rows[9][0].ToString();
            String problem_finish_nums = dtData.Rows[10][0].ToString();
            String problem_overDue_nums = dtData.Rows[11][0].ToString();
            String hangUpNums = dtData.Rows[12][0].ToString();
            String citizen_report_finish_nums = dtData.Rows[13][0].ToString();
            String citizen_report_overDue_nums = dtData.Rows[14][0].ToString();
            String citizen_report_hangUp_nums = dtData.Rows[15][0].ToString();
            String citizen_patrol_finish_nums = dtData.Rows[16][0].ToString();
            String citizen_patrol_overDue_nums = dtData.Rows[17][0].ToString();
            String citizen_patrol_hangUp_nums = dtData.Rows[18][0].ToString();
            String chiefProblemHangUpNums = dtData.Rows[19][0].ToString();
            String chiefProblemFinishNums = dtData.Rows[20][0].ToString();
            String phoneHangUpProblemNums = dtData.Rows[21][0].ToString();
            String phoneFinishProblemNums = dtData.Rows[22][0].ToString();
            String chief_review = dtData.Rows[23][0].ToString();
            String chief_overDue = dtData.Rows[24][0].ToString();
            String chief_processing = dtData.Rows[25][0].ToString();
            String citizen_report_processing_nums = dtData.Rows[26][0].ToString();
            String citizen_report_review = dtData.Rows[27][0].ToString();
            String citizen_patrol_processing_nums = dtData.Rows[28][0].ToString();
            String citizen_patrol_review_nums = dtData.Rows[29][0].ToString();
            String city_wechatpatrol_processing_nums = dtData.Rows[30][0].ToString();
            String city_wechatpatrol_finish_nums = dtData.Rows[31][0].ToString();
            String city_wechatpatrol_review_nums = dtData.Rows[32][0].ToString();
            String city_wechatpatorl_account_Nums = dtData.Rows[33][0].ToString();
            String city_wechatpatrol_overdue_nums = dtData.Rows[34][0].ToString();
            String telephoneprocessingNums = dtData.Rows[35][0].ToString();
            String telephonereviewNums = dtData.Rows[36][0].ToString();
            String telephoneoverdueNums = dtData.Rows[37][0].ToString();

            //插入RTD_PROBLEM_SOURCE 表
            String problemSourceId = Guid.NewGuid().ToString().Replace("-", "");
            if (if_delAndInDB)
            {
                //是否删除老数据
                if (ifDelOldData)
                {
                    sql = "delete RTD_PROBLEM_SOURCE where base_id='" + base_id + "'";
                    SqlHelper3.ExecuteNonQuery(sql, CommandType.Text, null);
                }

                sql = "Insert into RTD_PROBLEM_SOURCE (ID,BASE_ID,CHIEF_REPORT,FOLK_CHIEF_REPORT,CITIZEN_REPORT,CITY_PATROL_REPORT,MEDIA_REPORT,"
                    + "OPINION_REPORT,SPOT_CHECKS_REPORT,TELEPHONE_REPORT,DEAL_ONESELF,FINISH_PROBLEM,OVERDUE_NO_FINISH,ACCOUNT_PROBLEM,"
                    + "CITIZEN_REPORT_FINISH,CITIZEN_OVERDUE_NO_FINISH,CITIZEN_ACCOUT,CITY_PATROL_FINISH,"
                    + "CITY_PATROL_OVERDUE_NO_FINISH,CITY_PATROL_ACCOUT,CHIEF_ACCOUNT,CHIEF_FINISH,TELEPHONE_ACCOUT,"
                    + "TELEPHONE_FINISH,START_TIME,END_TIME,CHIEF_REVIEW,CHIEF_OVERDUE,CHIEF_PROCESSING,"
                    + "CITIZEN_REPORT_PROCESSING,CITIZEN_REPORT_REVIEW,CITY_PATROL_PROCESSING,CITY_PATROL_REVIEW,CITY_WECHATPATROL_PROCESSING,CITY_WECHATPATROL_FINISH,CITY_WECHATPATROL_REVIEW,CITY_WECHATPATROL_ACCOUNT,CITY_WECHATPATROL_OVERDUE,"
                    + "TELEPHONE_PROCESSING,TELEPHONE_REVIEW,TELEPHONE_OVERDUE,WECHAT_PROBLEMS,user_id,section_id,cycle,section_type)"
                    
                    + " values ('" + problemSourceId + "','" + base_id + "','" + chief_report_nums + "','" + folkchief_report_nums + "','" + citizen_report_nums + "','" + citizen_patrol_report_nums + "','" + media_report_nums 
                    + "','" + public_report_nums + "','" + secret_report_nums + "','" + phone_report_nums + "','" + native_process_nums + "','" + problem_finish_nums + "','" + problem_overDue_nums + "','" + hangUpNums 
                    + "','" + citizen_report_finish_nums + "','" + citizen_report_overDue_nums + "','" + citizen_report_hangUp_nums + "','" + citizen_patrol_finish_nums 
                    + "','" + citizen_patrol_overDue_nums + "','" + citizen_patrol_hangUp_nums + "','" + chiefProblemHangUpNums + "','" + chiefProblemFinishNums + "','" + phoneHangUpProblemNums 
                    + "','" + phoneFinishProblemNums + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')" + ",to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')" + ",'" + chief_review + "','" + chief_overDue + "','" + chief_processing 
                    + "','" + citizen_report_processing_nums + "','" + citizen_report_review + "','" + citizen_patrol_processing_nums + "','" + citizen_patrol_review_nums + "','" + city_wechatpatrol_processing_nums + "','" + city_wechatpatrol_finish_nums
                    + "','" + city_wechatpatrol_review_nums + "','" + city_wechatpatorl_account_Nums + "','" + city_wechatpatrol_overdue_nums + "','" + telephoneprocessingNums + "','" + telephonereviewNums + "','" + telephoneoverdueNums + "','" + wechat_problems
                    + "','" + userId + "','" + section_id + "','" + cycle + "','" + section_type + "')";

                SqlHelper3.ExecuteNonQuery(sql, CommandType.Text, null);
            }
        }

        private void workerProblem_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (pBarProblem.Value < pBarProblem.Maximum)
                pBarProblem.Value += 1;
        }

        void workerProblem_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pBarProblem.Value = pBarProblem.Maximum;
        }

        private void rtbInfo_TextChanged(object sender, EventArgs e)
        {
            rtbInfo.SelectionStart = rtbInfo.TextLength;

            // Scrolls the contents of the control to the current caret position.
            rtbInfo.ScrollToCaret(); //Caret意思：脱字符号；插入符号; (^)
        }

        private void btnBase_Click(object sender, EventArgs e)
        {
            pBar_Base.Value = 0;
            pBar_Base.Minimum = 0;

            bgWork_Base.DoWork += new DoWorkEventHandler(worker_DoWorkBase);
            bgWork_Base.ProgressChanged += new ProgressChangedEventHandler(workerBase_ProgressChanged);
            bgWork_Base.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerBase_RunWorkerCompleted);

            bgWork_Base.RunWorkerAsync();
        }

        void worker_DoWorkBase(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(基础数据表:RTD_BASE)...";
            //writeInfo(lblInfo.Text);
            this.BeginInvoke(wt, new Object[] { lblInfo.Text });
            writeProgressInfo(lblInfo.Text, rtbBase);

            runBase(pBar_Base, bgWork_Base);

            lblInfo.Text = "数据处理完成(基础数据表:RTD_BASE)";
            writeProgressInfo(lblInfo.Text, rtbBase);
            this.BeginInvoke(wt, new Object[] { lblInfo.Text });

            if (auto_mode)
            {
                //继续跑巡河
                btnPatrol_Click(null, null);
                //继续跑问题
                btnProblem_Click(null, null);
                //继续跑徒步巡河
                btn_footPatrol_Click(null, null);
                //继续跑问题上报数据-问题类型
                btnProblemType_Click(null, null);
                //继续跑四个查清
                btnFourCheck_Click(null, null);
                //继续跑问题交办、重大问题上报、整改
                btnProblemAssign_Click(null, null);
                //继续跑水质
                btnWaterQuality_Click(null, null);
                //继续跑APP使用
                btnAppUse_Click(null, null);
                //继续跑河长积分
                btnScore_Click(null, null);
            }
        }

        public void runBase(ProgressBar pBar, BackgroundWorker bgWork)
        {
            //DateTime dt_start = Convert.ToDateTime(start_time);
            DateTime dt_end = Convert.ToDateTime(end_time);

            String sql = "";

            sql = "select id,type,addvcd_area,addvcd_town,addvcd_village,substr(parent_ids, 0, instr(parent_ids, ',') - 1) river_id,river_side,parent_ids from rm_river_lake";

            String riverType = "";
            if (citySwitch)
                riverType += "'1','2','9','13',";
            if (areaSwitch)
                riverType += "'2','3','6','9','10','13',";
            if (townSwitch)
                riverType += "'2','4','7','9','11','13',";
            if (villageSwitch)
                riverType += "'2','5','8','9','12','13'";

            riverType = riverType.TrimEnd(',');

            sql += " where type in(" + riverType + ")";

            DataTable dtRiver = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dtRiver.Rows.Count;
            List<String> sqlList = new List<string>();
            for (var i = 0; i < dtRiver.Rows.Count; i++)
            {
                String type = dtRiver.Rows[i]["type"].ToString();
                String river_id = dtRiver.Rows[i]["river_id"].ToString();
                String section_id = dtRiver.Rows[i]["id"].ToString();
                String side = dtRiver.Rows[i]["river_side"].ToString();

                String parent_ids = dtRiver.Rows[i]["parent_ids"].ToString();
                String[] ids = parent_ids.Split(',');

                String area_section = null;
                String town_section = null;

                if (ids.Length >= 2)
                {
                    area_section = ids[1];
                }

                if (ids.Length >= 3)
                {
                    town_section = ids[2];
                }

                String addvcd_city = "440100000000";
                String addvcd_area = dtRiver.Rows[i]["addvcd_area"].ToString();
                String addvcd_town = dtRiver.Rows[i]["addvcd_town"].ToString();
                String addvcd_village = dtRiver.Rows[i]["addvcd_village"].ToString();

                //writeInfo((i + 1) + ":" + section_id);
                writeProgressInfo((i + 1) + "/" + dtRiver.Rows.Count + ":" + section_id, rtbBase);
                //this.BeginInvoke(wt, new Object[] { (i + 1) + ":" + section_id });

                //市级河段 每季度不少于1次
                DateTime dt_start = Convert.ToDateTime(start_time);

                while (dt_start <= dt_end)
                {
                    if ((type.Equals("1") || type.Equals("2") || type.Equals("9") || type.Equals("13")) && citySwitch)
                    {
                        if (dt_start.Day != 1 || (dt_start.Month == 2 || dt_start.Month == 3 || dt_start.Month == 5 || dt_start.Month == 6 || dt_start.Month == 8 || dt_start.Month == 9 || dt_start.Month == 11 || dt_start.Month == 12))
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddMonths(-3).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getCityChiefPeriod(dt_start.AddMonths(-3));
                        String cycle = dt_start.AddMonths(-3).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('1','27','3','2','39','4','28','29','30','40','41','42') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            String id = Guid.NewGuid().ToString().Replace("-", "");
                            String river_type = dtUserInfo.Rows[j]["river_type"].ToString();

                            if (if_delAndInDB)
                            {
                                //获取base_id
                                sql = "select count(id) nums from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "'";

                                int baseIdCount = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                                if (baseIdCount <= 0)
                                {
                                    sql = "Insert into RTD_BASE (ID,ADDVCD_CITY,ADDVCD_AREA,ADDVCD_TOWN,ADDVCD_VILLAGE,RIVER_ID,SECTION_ID,SECTION_TYPE,BANK,USER_ID,CYCLE,start_time,end_time,area_section,town_section,river_type,type)"
                                        + " values ('" + id + "','" + addvcd_city + "','" + addvcd_area + "','" + addvcd_town + "','" + addvcd_village + "','" + river_id + "','" + section_id + "','" + type + "','" + side + "','"
                                        + userid + "','" + cycle + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')" + ",to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'),'" + area_section + "','" + town_section + "','" + river_type + "','1')";

                                    sqlList.Add(sql);
                                    SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                                }
                            }
                        }
                    }
                    //区级河段 一般河湖：每月不少于1次；黑臭河湖：每月巡查不少于一次，每次巡查完成不低于本辖区黑臭水体总条数的15%，每半年完成一轮全覆盖巡查；
                    if ((type.Equals("3") || type.Equals("2") || type.Equals("6") || type.Equals("9") || type.Equals("10") || type.Equals("13")) && areaSwitch)
                    {
                        if (dt_start.Day != 1 || (dt_start.Month == 2 || dt_start.Month == 4 || dt_start.Month == 6 || dt_start.Month == 8 || dt_start.Month == 10 || dt_start.Month == 12))
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddMonths(-2).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getAreaChiefPeriod(dt_start.AddMonths(-2));
                        String cycle = dt_start.AddMonths(-2).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('7','8','9','31','32','33','43','44','45') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            String id = Guid.NewGuid().ToString().Replace("-", "");
                            String river_type = dtUserInfo.Rows[j]["river_type"].ToString();

                            if (if_delAndInDB)
                            {
                                //获取base_id
                                sql = "select count(id) nums from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "'";

                                int baseIdCount = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                                if (baseIdCount <= 0)
                                {
                                    sql = "Insert into RTD_BASE (ID,ADDVCD_CITY,ADDVCD_AREA,ADDVCD_TOWN,ADDVCD_VILLAGE,RIVER_ID,SECTION_ID,SECTION_TYPE,BANK,USER_ID,CYCLE,start_time,end_time,area_section,town_section,river_type,type)"
                                        + " values ('" + id + "','" + addvcd_city + "','" + addvcd_area + "','" + addvcd_town + "','" + addvcd_village + "','" + river_id + "','" + section_id + "','" + type + "','" + side + "','"
                                        + userid + "','" + cycle + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')" + ",to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'),'" + area_section + "','" + town_section + "','" + river_type + "','1')";

                                    sqlList.Add(sql);
                                    SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                                }
                            }
                        }
                    }
                    //镇级河段 一般河湖：每旬巡查不少于一次；黑臭河湖：每周巡查不少于一次，每次完成不低于本辖区黑臭水体总数的30%，每月完成一轮全覆盖巡查；
                    if ((type.Equals("4") || type.Equals("2") || type.Equals("7") || type.Equals("9") || type.Equals("11") || type.Equals("13")) && townSwitch)
                    {
                        if (Convert.ToInt32(dt_start.DayOfWeek) != 1)
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getWeek(dt_start.AddDays(-7));
                        String cycle = dt_start.AddDays(-7).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('12','50','13','34','35','36','46','47','48') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            String id = Guid.NewGuid().ToString().Replace("-", "");
                            String river_type = dtUserInfo.Rows[j]["river_type"].ToString();

                            if (if_delAndInDB)
                            {
                                //获取base_id
                                sql = "select count(id) nums from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "'";

                                int baseIdCount = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                                if (baseIdCount <= 0)
                                {
                                    sql = "Insert into RTD_BASE (ID,ADDVCD_CITY,ADDVCD_AREA,ADDVCD_TOWN,ADDVCD_VILLAGE,RIVER_ID,SECTION_ID,SECTION_TYPE,BANK,USER_ID,CYCLE,start_time,end_time,area_section,town_section,river_type,type)"
                                        + " values ('" + id + "','" + addvcd_city + "','" + addvcd_area + "','" + addvcd_town + "','" + addvcd_village + "','" + river_id + "','" + section_id + "','" + type + "','" + side + "','"
                                        + userid + "','" + cycle + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')" + ",to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'),'" + area_section + "','" + town_section + "','" + river_type + "','1')";

                                    sqlList.Add(sql);
                                    SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                                }
                            }
                        }
                    }
                    //村级河段 一般河湖：每周巡查不少于一次；黑臭河湖：每个工作日一巡，每周完成黑臭水体全覆盖巡查；
                    if ((type.Equals("5") || type.Equals("2") || type.Equals("8") || type.Equals("9") || type.Equals("12") || type.Equals("13")) && villageSwitch)
                    {
                        String startTm = dt_start.ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.ToString("yyyy-MM-dd 23:59:59");

                        int periods = dt_start.DayOfYear;
                        String cycle = dt_start.Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('16','37','49') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            String id = Guid.NewGuid().ToString().Replace("-", "");
                            String river_type = dtUserInfo.Rows[j]["river_type"].ToString();

                            if (if_delAndInDB)
                            {
                                //获取base_id
                                sql = "select count(id) nums from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "'";

                                int baseIdCount = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                                if (baseIdCount <= 0)
                                {
                                    sql = "Insert into RTD_BASE (ID,ADDVCD_CITY,ADDVCD_AREA,ADDVCD_TOWN,ADDVCD_VILLAGE,RIVER_ID,SECTION_ID,SECTION_TYPE,BANK,USER_ID,CYCLE,start_time,end_time,area_section,town_section,river_type,type)"
                                        + " values ('" + id + "','" + addvcd_city + "','" + addvcd_area + "','" + addvcd_town + "','" + addvcd_village + "','" + river_id + "','" + section_id + "','" + type + "','" + side + "','"
                                        + userid + "','" + cycle + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')" + ",to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'),'" + area_section + "','" + town_section + "','" + river_type + "','1')";

                                    sqlList.Add(sql);
                                    SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                                }
                            }

                           
                        }
                    }

                    dt_start = dt_start.AddDays(1);
                }

                if (fixedPeriodWitch)//跑固定周期数据开关是否勾上
                {
                    List<Dictionary<String, Object>> cycleList = getCycleList();
                    for (int index = 0; index < cycleList.Count; index++)
                    {

                        String startTm = cycleList[index]["startTm"].ToString();
                        String endTm = cycleList[index]["endTm"].ToString();
                        String cycle = cycleList[index]["cycle"].ToString();

                        if ((type.Equals("1") || type.Equals("2") || type.Equals("9") || type.Equals("13")) && citySwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('1','27','3','2','39','4','28','29','30','40','41','42') and del_flag='0' and "
                                    + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                String id = Guid.NewGuid().ToString().Replace("-", "");
                                String river_type = dtUserInfo.Rows[j]["river_type"].ToString();

                                if (if_delAndInDB)
                                {
                                    //获取base_id
                                    sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "'";

                                    DataTable dtOldBaseInfo = SqlHelper.GetDataTable(sql, null);
                                    if (dtOldBaseInfo.Rows.Count <= 0)
                                    {
                                        sql = "Insert into RTD_BASE (ID,ADDVCD_CITY,ADDVCD_AREA,ADDVCD_TOWN,ADDVCD_VILLAGE,RIVER_ID,SECTION_ID,SECTION_TYPE,BANK,USER_ID,CYCLE,start_time,end_time,area_section,town_section,river_type,type)"
                                        + " values ('" + id + "','" + addvcd_city + "','" + addvcd_area + "','" + addvcd_town + "','" + addvcd_village + "','" + river_id + "','" + section_id + "','" + type + "','" + side + "','"
                                        + userid + "','" + cycle + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')" + ",to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'),'" + area_section + "','" + town_section + "','" + river_type + "','4')";

                                        sqlList.Add(sql);
                                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                                    }
                                    else
                                    {
                                        String base_id = dtOldBaseInfo.Rows[0][0].ToString();
                                        sql = "update RTD_BASE set CREATE_DATE=sysdate,start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss'),end_time=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') where id='" + base_id + "'";

                                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                                    }
                                }
                            }
                        }
                        if ((type.Equals("3") || type.Equals("2") || type.Equals("6") || type.Equals("9") || type.Equals("10") || type.Equals("13")) && areaSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('7','8','9','31','32','33','43','44','45') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                String river_type = dtUserInfo.Rows[j]["river_type"].ToString();

                                String id = Guid.NewGuid().ToString().Replace("-", "");

                                if (if_delAndInDB)
                                {
                                    //获取base_id
                                    sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "'";

                                    DataTable dtOldBaseInfo = SqlHelper.GetDataTable(sql, null);
                                    if (dtOldBaseInfo.Rows.Count <= 0)
                                    {
                                        sql = "Insert into RTD_BASE (ID,ADDVCD_CITY,ADDVCD_AREA,ADDVCD_TOWN,ADDVCD_VILLAGE,RIVER_ID,SECTION_ID,SECTION_TYPE,BANK,USER_ID,CYCLE,start_time,end_time,area_section,town_section,river_type,type)"
                                            + " values ('" + id + "','" + addvcd_city + "','" + addvcd_area + "','" + addvcd_town + "','" + addvcd_village + "','" + river_id + "','" + section_id + "','" + type + "','" + side + "','"
                                            + userid + "','" + cycle + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')" + ",to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'),'" + area_section + "','" + town_section + "','" + river_type + "','4')";

                                        sqlList.Add(sql);
                                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                                    }
                                    else
                                    {
                                        String base_id = dtOldBaseInfo.Rows[0][0].ToString();
                                        sql = "update RTD_BASE set CREATE_DATE=sysdate,start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss'),end_time=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') where id='" + base_id + "'";

                                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                                    }
                                }
                            }
                        }
                        if ((type.Equals("4") || type.Equals("2") || type.Equals("7") || type.Equals("9") || type.Equals("11") || type.Equals("13")) && townSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('12','50','13','34','35','36','46','47','48') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                String id = Guid.NewGuid().ToString().Replace("-", "");
                                String river_type = dtUserInfo.Rows[j]["river_type"].ToString();

                                if (if_delAndInDB)
                                {
                                    //获取base_id
                                    sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "'";

                                    DataTable dtOldBaseInfo = SqlHelper.GetDataTable(sql, null);
                                    if (dtOldBaseInfo.Rows.Count <= 0)
                                    {
                                        sql = "Insert into RTD_BASE (ID,ADDVCD_CITY,ADDVCD_AREA,ADDVCD_TOWN,ADDVCD_VILLAGE,RIVER_ID,SECTION_ID,SECTION_TYPE,BANK,USER_ID,CYCLE,start_time,end_time,area_section,town_section,river_type,type)"
                                        + " values ('" + id + "','" + addvcd_city + "','" + addvcd_area + "','" + addvcd_town + "','" + addvcd_village + "','" + river_id + "','" + section_id + "','" + type + "','" + side + "','"
                                        + userid + "','" + cycle + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')" + ",to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'),'" + area_section + "','" + town_section + "','" + river_type + "','4')";

                                        sqlList.Add(sql);
                                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                                    }
                                    else
                                    {
                                        String base_id = dtOldBaseInfo.Rows[0][0].ToString();
                                        sql = "update RTD_BASE set CREATE_DATE=sysdate,start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss'),end_time=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') where id='" + base_id + "'";

                                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                                    }
                                }
                            }
                        }
                        if ((type.Equals("5") || type.Equals("2") || type.Equals("8") || type.Equals("9") || type.Equals("12") || type.Equals("13")) && villageSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('16','37','49') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                String id = Guid.NewGuid().ToString().Replace("-", "");
                                String river_type = dtUserInfo.Rows[j]["river_type"].ToString();

                                if (if_delAndInDB)
                                {
                                    //获取base_id
                                    sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "'";

                                    DataTable dtOldBaseInfo = SqlHelper.GetDataTable(sql, null);
                                    if (dtOldBaseInfo.Rows.Count <= 0)
                                    {
                                        sql = "Insert into RTD_BASE (ID,ADDVCD_CITY,ADDVCD_AREA,ADDVCD_TOWN,ADDVCD_VILLAGE,RIVER_ID,SECTION_ID,SECTION_TYPE,BANK,USER_ID,CYCLE,start_time,end_time,area_section,town_section,river_type,type)"
                                        + " values ('" + id + "','" + addvcd_city + "','" + addvcd_area + "','" + addvcd_town + "','" + addvcd_village + "','" + river_id + "','" + section_id + "','" + type + "','" + side + "','"
                                        + userid + "','" + cycle + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')" + ",to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'),'" + area_section + "','" + town_section + "','" + river_type + "','4')";

                                        sqlList.Add(sql);
                                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                                    }
                                    else
                                    {
                                        String base_id = dtOldBaseInfo.Rows[0][0].ToString();
                                        sql = "update RTD_BASE set CREATE_DATE=sysdate,start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss'),end_time=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') where id='" + base_id + "'";

                                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                                    }
                                }
                            }
                        }
                    }
                }

                //lblInfo.Text = dtRiver.Rows[i][0].ToString();
                bgWork.ReportProgress(i + 1);
            }
        }

        private List<Dictionary<String, Object>> getCycleList()
        {
            //本日
            /*String day_startTm = DateTime.Now.Date.ToString("yyyy-MM-dd 00:00:00");
            String day_endTm = DateTime.Now.Date.ToString("yyyy-MM-dd 23:59:59");
            Dictionary<String, Object> dayDic = new Dictionary<String, Object>();
            dayDic.Add("startTm", day_startTm);
            dayDic.Add("endTm", day_endTm);
            dayDic.Add("cycle", "本日");*/

            //本周
            String week_startTm = DateTime.Now.AddDays(1 - Convert.ToInt16(DateTime.Now.DayOfWeek)).ToString("yyyy-MM-dd 00:00:00");
            String week_endTm = DateTime.Now.AddDays(7 - Convert.ToInt16(DateTime.Now.DayOfWeek)).ToString("yyyy-MM-dd 23:59:59");
            Dictionary<String, Object> weekDic = new Dictionary<String, Object>();
            weekDic.Add("startTm", week_startTm);
            weekDic.Add("endTm", week_endTm);
            weekDic.Add("cycle", "本周");

            //本旬
            //本月
            String month_startTm = DateTime.Now.ToString("yyyy-MM-01 00:00:00");
            String month_endTm = DateTime.Parse(DateTime.Now.AddMonths(1).ToString("yyyy-MM-01")).AddDays(-1).ToString("yyyy-MM-dd 23:59:59");
            Dictionary<String, Object> monthDic = new Dictionary<String, Object>();
            monthDic.Add("startTm", month_startTm);
            monthDic.Add("endTm", month_endTm);
            monthDic.Add("cycle", "本月");

            //本季度
            /*String quarter_startTm = DateTime.Now.AddMonths(0 - ((DateTime.Now.Month - 1) % 3)).ToString("yyyy-MM-01 00:00:00");
            String quarter_endTm = DateTime.Parse(DateTime.Now.AddMonths(3 - ((DateTime.Now.Month - 1) % 3)).ToString("yyyy-MM-01")).AddDays(-1).ToString("yyyy-MM-dd 23:59:59");
            Dictionary<String, Object> quarterDic = new Dictionary<String, Object>();
            quarterDic.Add("startTm", quarter_startTm);
            quarterDic.Add("endTm", quarter_endTm);
            quarterDic.Add("cycle", "本季度");*/

            //本年度
            String year_startTm = DateTime.Now.ToString("yyyy-01-01 00:00:00");
            String year_endTm = DateTime.Parse(DateTime.Now.ToString("yyyy-01-01")).AddYears(1).AddDays(-1).ToString("yyyy-MM-dd 23:59:59");
            Dictionary<String, Object> yearDic = new Dictionary<String, Object>();
            yearDic.Add("startTm", year_startTm);
            yearDic.Add("endTm", year_endTm);
            yearDic.Add("cycle", "本年度");

            //上日
            String lastDay_startTm = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 00:00:00");
            String lastDay_endTm = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");
            Dictionary<String, Object> lastDayDic = new Dictionary<String, Object>();
            lastDayDic.Add("startTm", lastDay_startTm);
            lastDayDic.Add("endTm", lastDay_endTm);
            lastDayDic.Add("cycle", "上日");

            //上周  
            /*String lastWeek_startTm = DateTime.Now.AddDays(1 - Convert.ToInt16(DateTime.Now.DayOfWeek) - 7).ToString("yyyy-MM-dd 00:00:00");
            String lastWeek_endTm = DateTime.Now.AddDays(7 - Convert.ToInt16(DateTime.Now.DayOfWeek) - 7).ToString("yyyy-MM-dd 23:59:59");
            Dictionary<String, Object> lastWeekDic = new Dictionary<String, Object>();
            lastWeekDic.Add("startTm", lastWeek_startTm);
            lastWeekDic.Add("endTm", lastWeek_endTm);
            lastWeekDic.Add("cycle", "上周");*/


            //上旬

            //上月
            String lastMonth_startTm = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-01 00:00:00");
            String lastMonth_endTm = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddDays(-1).ToString("yyyy-MM-dd 23:59:59");
            Dictionary<String, Object> lastMonthDic = new Dictionary<String, Object>();
            lastMonthDic.Add("startTm", lastMonth_startTm);
            lastMonthDic.Add("endTm", lastMonth_endTm);
            lastMonthDic.Add("cycle", "上月");

            //上个季度
            /*String lastQuarter_startTm = DateTime.Now.AddMonths(-3 - ((DateTime.Now.Month - 1) % 3)).ToString("yyyy-MM-01 00:00:00");
            String lastQuarter_endTm = DateTime.Parse(DateTime.Now.AddMonths(0 - ((DateTime.Now.Month - 1) % 3)).ToString("yyyy-MM-01")).AddDays(-1).ToString("yyyy-MM-dd 23:59:59");
            Dictionary<String, Object> lastQuarterDic = new Dictionary<String, Object>();
            lastQuarterDic.Add("startTm", lastQuarter_startTm);
            lastQuarterDic.Add("endTm", lastQuarter_endTm);
            lastQuarterDic.Add("cycle", "上个季度");*/

            //上个年度
            String lastYear_startTm = DateTime.Parse(DateTime.Now.ToString("yyyy-01-01")).AddYears(-1).ToString("yyyy-MM-dd 00:00:00");
            String lastYear_endTm = DateTime.Parse(DateTime.Now.ToString("yyyy-01-01")).AddDays(-1).ToString("yyyy-MM-dd 23:59:59");
            Dictionary<String, Object> lastYearDic = new Dictionary<String, Object>();
            lastYearDic.Add("startTm", lastYear_startTm);
            lastYearDic.Add("endTm", lastYear_endTm);
            lastYearDic.Add("cycle", "上个年度");

            List<Dictionary<String, Object>> cycleList = new List<Dictionary<String, Object>>();
            //cycleList.Add(dayDic);
            cycleList.Add(weekDic);
            cycleList.Add(monthDic);
            //cycleList.Add(quarterDic);
            cycleList.Add(yearDic);
            cycleList.Add(lastDayDic);
            //cycleList.Add(lastWeekDic);
            cycleList.Add(lastMonthDic);
            //cycleList.Add(lastQuarterDic);
            cycleList.Add(lastYearDic);

            return cycleList;
        }

        private void workerBase_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (pBar_Base.Value < pBar_Base.Maximum)
                pBar_Base.Value += 1;
        }

        void workerBase_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pBar_Base.Value = pBar_Base.Maximum;
        }

        private void btn_footPatrol_Click(object sender, EventArgs e)
        {
            pBarFootPatrol.Value = 0;
            pBarFootPatrol.Minimum = 0;

            bg_footPatrol.DoWork += new DoWorkEventHandler(worker_DoWorkFootPatrol);
            bg_footPatrol.ProgressChanged += new ProgressChangedEventHandler(workerFootPatrol_ProgressChanged);
            bg_footPatrol.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerFootPatrol_RunWorkerCompleted);

            bg_footPatrol.RunWorkerAsync();
        }

        void worker_DoWorkFootPatrol(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(徒步巡河:RTD_FOOT_PATROL)...";
            //writeInfo(lblInfo.Text);
            this.BeginInvoke(wt, new Object[] { lblInfo.Text });
            writeProgressInfo(lblInfo.Text, rtbFootPatrol);

            runFootPatrol(pBarFootPatrol, bg_footPatrol);

            lblInfo.Text = "数据处理完成(徒步巡河:RTD_FOOT_PATROL)";
            //writeInfo(lblInfo.Text);
            this.BeginInvoke(wt, new Object[] { lblInfo.Text });
            writeProgressInfo(lblInfo.Text, rtbFootPatrol);
        }

        public void runFootPatrol(ProgressBar pBar, BackgroundWorker bgWork)
        {
            //DateTime dt_start = Convert.ToDateTime(start_time);
            DateTime dt_end = Convert.ToDateTime(end_time);

            String sql = "";

            sql = "select id,type,addvcd_area,addvcd_town,addvcd_village,substr(parent_ids, 0, instr(parent_ids, ',') - 1) river_id,river_side,parent_ids from rm_river_lake";

            String riverType = "";
            if (citySwitch)
                riverType += "'1','2','9','13',";
            if (areaSwitch)
                riverType += "'2','3','6','9','10','13',";
            if (townSwitch)
                riverType += "'2','4','7','9','11','13',";
            if (villageSwitch)
                riverType += "'2','5','8','9','12','13'";

            riverType = riverType.TrimEnd(',');

            sql += " where type in(" + riverType + ")";

            DataTable dtRiver = SqlHelper4.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dtRiver.Rows.Count;
            for (var i = 0; i < dtRiver.Rows.Count; i++)
            {
                String type = dtRiver.Rows[i]["type"].ToString();
                String river_id = dtRiver.Rows[i]["river_id"].ToString();
                String section_id = dtRiver.Rows[i]["id"].ToString();
                String side = dtRiver.Rows[i]["river_side"].ToString();

                String parent_ids = dtRiver.Rows[i]["parent_ids"].ToString();
                String[] ids = parent_ids.Split(',');

                String area_section = null;
                String town_section = null;

                if (ids.Length >= 2)
                {
                    area_section = ids[1];
                }

                if (ids.Length >= 3)
                {
                    town_section = ids[2];
                }

                String addvcd_city = "440100000000";
                String addvcd_area = dtRiver.Rows[i]["addvcd_area"].ToString();
                String addvcd_town = dtRiver.Rows[i]["addvcd_town"].ToString();
                String addvcd_village = dtRiver.Rows[i]["addvcd_village"].ToString();

                //writeInfo((i+1)+":"+section_id);
                //this.BeginInvoke(wt, new Object[] { (i + 1) + ":" + section_id });
                writeProgressInfo((i + 1) + "/" + dtRiver.Rows.Count + ":" + section_id, rtbFootPatrol);

                //市级河段 每季度不少于1次
                DateTime dt_start = Convert.ToDateTime(start_time);

                while (dt_start <= dt_end)
                {
                    if ((type.Equals("1") || type.Equals("2") || type.Equals("9") || type.Equals("13")) && citySwitch)
                    {
                        if (dt_start.Day != 1 || (dt_start.Month == 2 || dt_start.Month == 3 || dt_start.Month == 5 || dt_start.Month == 6 || dt_start.Month == 8 || dt_start.Month == 9 || dt_start.Month == 11 || dt_start.Month == 12))
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddMonths(-3).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getCityChiefPeriod(dt_start.AddMonths(-3));
                        String cycle = dt_start.AddMonths(-3).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('1','27','3','2','39','4','28','29','30','40','41','42') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper4.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper4.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();

                            //================徒步巡河=====================
                            footPatrolCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true,type);
                        }
                    }
                    //区级河段 一般河湖：每月不少于1次；黑臭河湖：每月巡查不少于一次，每次巡查完成不低于本辖区黑臭水体总条数的15%，每半年完成一轮全覆盖巡查；
                    if ((type.Equals("3") || type.Equals("2") || type.Equals("6") || type.Equals("9") || type.Equals("10") || type.Equals("13")) && areaSwitch)
                    {
                        if (dt_start.Day != 1 || (dt_start.Month == 2 || dt_start.Month == 4 || dt_start.Month == 6 || dt_start.Month == 8 || dt_start.Month == 10 || dt_start.Month == 12))
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddMonths(-2).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getAreaChiefPeriod(dt_start.AddMonths(-2));
                        String cycle = dt_start.AddMonths(-2).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('7','8','9','31','32','33','43','44','45') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper4.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper4.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();

                            //================徒步巡河=====================
                            footPatrolCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                        }
                    }
                    //镇级河段 一般河湖：每旬巡查不少于一次；黑臭河湖：每周巡查不少于一次，每次完成不低于本辖区黑臭水体总数的30%，每月完成一轮全覆盖巡查；
                    if ((type.Equals("4") || type.Equals("2") || type.Equals("7") || type.Equals("9") || type.Equals("11") || type.Equals("13")) && townSwitch)
                    {
                        if (Convert.ToInt32(dt_start.DayOfWeek) != 1)
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getWeek(dt_start.AddDays(-7));
                        String cycle = dt_start.AddDays(-7).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('12','50','13','34','35','36','46','47','48') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper4.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper4.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();

                            //================徒步巡河=====================
                            footPatrolCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                        }
                    }
                    //村级河段 一般河湖：每周巡查不少于一次；黑臭河湖：每个工作日一巡，每周完成黑臭水体全覆盖巡查；
                    if ((type.Equals("5") || type.Equals("2") || type.Equals("8") || type.Equals("9") || type.Equals("12") || type.Equals("13")) && villageSwitch)
                    {
                        String startTm = dt_start.ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.ToString("yyyy-MM-dd 23:59:59");

                        int periods = dt_start.DayOfYear;
                        String cycle = dt_start.Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('16','37','49') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper4.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();

                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper4.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();
                            //================徒步巡河=====================
                            footPatrolCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                        }
                    }

                    dt_start = dt_start.AddDays(1);
                }

                //==============================跑固定周期的数据====================================
                if (fixedPeriodWitch)//跑固定周期数据开关是否勾上
                {
                    List<Dictionary<String, Object>> cycleList = getCycleList();
                    for (int index = 0; index < cycleList.Count; index++)
                    {

                        String startTm = cycleList[index]["startTm"].ToString();
                        String endTm = cycleList[index]["endTm"].ToString();
                        String cycle = cycleList[index]["cycle"].ToString();

                        if ((type.Equals("1") || type.Equals("2") || type.Equals("9") || type.Equals("13")) && citySwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('1','27','3','2','39','4','28','29','30','40','41','42') and del_flag='0' and "
                                    + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper4.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper4.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================徒步巡河=====================
                                footPatrolCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                            }
                        }
                        if ((type.Equals("3") || type.Equals("2") || type.Equals("6") || type.Equals("9") || type.Equals("10") || type.Equals("13")) && areaSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('7','8','9','31','32','33','43','44','45') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper4.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();

                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper4.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================徒步巡河=====================
                                footPatrolCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                            }
                        }
                        if ((type.Equals("4") || type.Equals("2") || type.Equals("7") || type.Equals("9") || type.Equals("11") || type.Equals("13")) && townSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('12','50','13','34','35','36','46','47','48') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper4.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper4.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================徒步巡河=====================
                                footPatrolCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                            }
                        }
                        if ((type.Equals("5") || type.Equals("2") || type.Equals("8") || type.Equals("9") || type.Equals("12") || type.Equals("13")) && villageSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('16','37','49') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper4.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper4.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================徒步巡河=====================
                                footPatrolCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                            }
                        }
                    }
                }

                //lblInfo.Text = dtRiver.Rows[i][0].ToString();
                bgWork.ReportProgress(i + 1);
            }
        }

        //徒步巡河统计
        private void footPatrolCount(String userId, String cycle, String river_id, String section_id, String base_id, String startTm, String endTm, Boolean ifDelOld,String section_type)
        {
            //获取base_id
            //String sql = "select id from rtd_base where user_id='" + userId + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

            //DataTable dtBaseInfo = SqlHelper4.GetDataTable(sql, null);
            //if (dtBaseInfo.Rows.Count <= 0)
            //    return;
            //String base_id = dtBaseInfo.Rows[0][0].ToString();

            //================市水务局徒步巡河问题上报=======================
            //流办问题数	珠办问题数	农水处问题数	水资源处问题数	建管问题数	排水处问题数	规划处问题数	计财处问题数	河道处问题数	工程督办组问题数	质安组问题数	科信处问题数
            String lbwts = "0";
            String zbwts = "0";
            String nscwts = "0";
            String szycwts = "0";
            String jgwts = "0";
            String pscwts = "0";
            String ghcwts = "0";
            String jccwts = "0";
            String hdcwts = "0";
            String gcdbzwts = "0";
            String zazwts = "0";
            String kxcwts = "0";
            String sql = "select userid,count(problemid) nums from ptl_reportproblem where state<>'-1' and sectionid in"
                + " (select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
                + " and userid in('e704436c3d8d438ba2ae721fda3910a4','5fb8fd0763bf463f84c42caa0f33eea6','5f84adebf9d64d41b72234638b32e0f5','e1ce64d29f2147a5abceaabb888de9b8','942da5c0d84a4dfc8907b7310bd5a23e','b691a70fce0c4f21b16db1e373503d63','4c60a3207fff4cc898c35e179618cc87','93085878f11e486c87894209006f96cf','998c4373d34644ec85254eb13ab4d1ef','d97e1d225700431e8923ecaa5fd6707f','1be3e5425ca647e4b4ba79c5c5474688','30100006')"
                + " and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') group by userid";

            DataTable dtFootProblemNums = SqlHelper4.GetDataTable(sql, null);
            for (int k = 0; k < dtFootProblemNums.Rows.Count; k++)
            {
                String foot_userid = dtFootProblemNums.Rows[k]["userid"].ToString();

                switch (foot_userid)
                {
                    case "30100006"://科技信息处
                        kxcwts = dtFootProblemNums.Rows[k]["nums"].ToString();
                        break;
                    case "d97e1d225700431e8923ecaa5fd6707f"://河道处
                        hdcwts = dtFootProblemNums.Rows[k]["nums"].ToString();
                        break;
                    case "1be3e5425ca647e4b4ba79c5c5474688"://工程督办组
                        gcdbzwts = dtFootProblemNums.Rows[k]["nums"].ToString();
                        break;
                    case "93085878f11e486c87894209006f96cf"://农村水利处
                        nscwts = dtFootProblemNums.Rows[k]["nums"].ToString();
                        break;
                    case "4c60a3207fff4cc898c35e179618cc87"://排水管理处
                        pscwts = dtFootProblemNums.Rows[k]["nums"].ToString();
                        break;
                    case "998c4373d34644ec85254eb13ab4d1ef"://规划计划处
                        ghcwts = dtFootProblemNums.Rows[k]["nums"].ToString();
                        break;
                    case "e1ce64d29f2147a5abceaabb888de9b8"://建设管理处
                        jgwts = dtFootProblemNums.Rows[k]["nums"].ToString();
                        break;
                    case "5f84adebf9d64d41b72234638b32e0f5"://水资源处
                        szycwts = dtFootProblemNums.Rows[k]["nums"].ToString();
                        break;
                    case "5fb8fd0763bf463f84c42caa0f33eea6"://质量安全处
                        zazwts = dtFootProblemNums.Rows[k]["nums"].ToString();
                        break;
                    case "b691a70fce0c4f21b16db1e373503d63"://财务审计处
                        jccwts = dtFootProblemNums.Rows[k]["nums"].ToString();
                        break;
                    case "942da5c0d84a4dfc8907b7310bd5a23e"://市珠江堤防管理处
                        zbwts = dtFootProblemNums.Rows[k]["nums"].ToString();
                        break;
                    case "e704436c3d8d438ba2ae721fda3910a4"://流办
                        lbwts = dtFootProblemNums.Rows[k]["nums"].ToString();
                        break;
                }
            }

            //插入RTD_FOOT_PATROL表
            String footPatrolId = Guid.NewGuid().ToString().Replace("-", "");

            if (if_delAndInDB)
            {
                if (ifDelOld)
                {
                    sql = "delete from RTD_FOOT_PATROL where base_id='" + base_id + "'";

                    SqlHelper4.ExecuteNonQuery(sql, CommandType.Text, null);
                }

                sql = "Insert into RTD_FOOT_PATROL(ID,BASE_ID,LIUXI_RIVER_OFFICE,ZHUJIANG_RIVER_OFFICE,AGRICULTURE_AND_WATER_OFFICE,WATER_RESOURCES_OFFICE,"
                    +"CONSTRUCTION_MANAGEMENT_OFFICE,DRAINAGE_OFFICE,PLANNING_OFFICE,PLANNING_FINANCE_OFFICE,RIVERWAY_OFFICE,ENGINEERING_OFFICE,"
                    +"QUALITY_SAFETY_OFFICE,SCIENCE_INFORMATION_OFFICE,START_TIME,END_TIME,user_id,cycle,river_id,section_id,section_type)"
                    + " values ('" + footPatrolId + "','" + base_id + "','" + lbwts + "','" + zbwts + "','" + nscwts + "','" + szycwts + "','" + jgwts + "','" + pscwts + "','" + ghcwts + "','" + jccwts + "','" + hdcwts + "','"
                    + gcdbzwts + "','" + zazwts + "','" + kxcwts + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss'),to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'),'" + userId + "','"
                    + cycle + "','" + river_id + "','" + section_id + "','" + section_type + "')";
                
                SqlHelper4.ExecuteNonQuery(sql, CommandType.Text, null);
            }

           
        }

        private void workerFootPatrol_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (pBarFootPatrol.Value < pBarFootPatrol.Maximum)
                pBarFootPatrol.Value += 1;
        }

        void workerFootPatrol_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pBarFootPatrol.Value = pBarFootPatrol.Maximum;
        }

        private void btnProblemType_Click(object sender, EventArgs e)
        {
            pBarProblemType.Value = 0;
            pBarProblemType.Minimum = 0;

            bg_problemType.DoWork += new DoWorkEventHandler(worker_ProblemType);
            bg_problemType.ProgressChanged += new ProgressChangedEventHandler(workerProblemType_ProgressChanged);
            bg_problemType.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerProblemType_RunWorkerCompleted);

            bg_problemType.RunWorkerAsync();
        }

        void worker_ProblemType(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(问题类型:RTD_PROBLEM_TYPE)...";
            //writeInfo(lblInfo.Text);
            this.BeginInvoke(wt, new Object[] { lblInfo.Text });
            writeProgressInfo(lblInfo.Text, rtbProblemType);

            runProblemType(pBarProblemType, bg_problemType);

            lblInfo.Text = "数据处理完成(问题类型:RTD_PROBLEM_TYPE)";
            //writeInfo(lblInfo.Text);
            this.BeginInvoke(wt, new Object[] { lblInfo.Text });
            writeProgressInfo(lblInfo.Text, rtbProblemType);
        }

        public void runProblemType(ProgressBar pBar, BackgroundWorker bgWork)
        {
            //DateTime dt_start = Convert.ToDateTime(start_time);
            end_time = dtpEnd.Text;
            DateTime dt_end = Convert.ToDateTime(end_time);

            String sql = "";

            sql = "select id,type,addvcd_area,addvcd_town,addvcd_village,substr(parent_ids, 0, instr(parent_ids, ',') - 1) river_id,river_side,parent_ids from rm_river_lake";

            String riverType = "";
            if (citySwitch)
                riverType += "'1','2','9','13',";
            if (areaSwitch)
                riverType += "'2','3','6','9','10','13',";
            if (townSwitch)
                riverType += "'2','4','7','9','11','13',";
            if (villageSwitch)
                riverType += "'2','5','8','9','12','13'";

            if (!citySwitch && !areaSwitch && !townSwitch && !villageSwitch)
            {
                return;
            }

            riverType = riverType.TrimEnd(',');

            sql += " where type in(" + riverType + ")";

            DataTable dtRiver = SqlHelper5.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dtRiver.Rows.Count;
            //sStopwatch sw = new Stopwatch(); 
            for (var i = 0; i < dtRiver.Rows.Count; i++)
            {
                //sw.Start();
                String type = dtRiver.Rows[i]["type"].ToString();
                String river_id = dtRiver.Rows[i]["river_id"].ToString();
                String section_id = dtRiver.Rows[i]["id"].ToString();
                String side = dtRiver.Rows[i]["river_side"].ToString();

                String parent_ids = dtRiver.Rows[i]["parent_ids"].ToString();
                String[] ids = parent_ids.Split(',');

                String area_section = null;
                String town_section = null;

                if (ids.Length >= 2)
                {
                    area_section = ids[1];
                }

                if (ids.Length >= 3)
                {
                    town_section = ids[2];
                }

                String addvcd_city = "440100000000";
                String addvcd_area = dtRiver.Rows[i]["addvcd_area"].ToString();
                String addvcd_town = dtRiver.Rows[i]["addvcd_town"].ToString();
                String addvcd_village = dtRiver.Rows[i]["addvcd_village"].ToString();

                //writeInfo((i+1)+":"+section_id);
                //this.BeginInvoke(wt, new Object[] { (i + 1) + ":" + section_id });
                writeProgressInfo((i + 1) + "/" + dtRiver.Rows.Count + ":" + section_id, rtbProblemType);

                //市级河段 每季度不少于1次
                start_time = dtpStart.Text;
                DateTime dt_start = Convert.ToDateTime(start_time);

                while (dt_start <= dt_end)
                {
                    if ((type.Equals("1") || type.Equals("2") || type.Equals("9") || type.Equals("13")) && citySwitch)
                    {
                        if (dt_start.Day != 1 || (dt_start.Month == 2 || dt_start.Month == 3 || dt_start.Month == 5 || dt_start.Month == 6 || dt_start.Month == 8 || dt_start.Month == 9 || dt_start.Month == 11 || dt_start.Month == 12))
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddMonths(-3).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getCityChiefPeriod(dt_start.AddMonths(-3));
                        String cycle = dt_start.AddMonths(-3).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('1','27','3','2','39','4','28','29','30','40','41','42') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper5.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper5.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();

                            //================问题类型=====================
                            problemTypeCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true,type);
                        }
                    }
                    //区级河段 一般河湖：每月不少于1次；黑臭河湖：每月巡查不少于一次，每次巡查完成不低于本辖区黑臭水体总条数的15%，每半年完成一轮全覆盖巡查；
                    if ((type.Equals("3") || type.Equals("2") || type.Equals("6") || type.Equals("9") || type.Equals("10") || type.Equals("13")) && areaSwitch)
                    {
                        if (dt_start.Day != 1 || (dt_start.Month == 2 || dt_start.Month == 4 || dt_start.Month == 6 || dt_start.Month == 8 || dt_start.Month == 10 || dt_start.Month == 12))
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddMonths(-2).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getAreaChiefPeriod(dt_start.AddMonths(-2));
                        String cycle = dt_start.AddMonths(-2).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('7','8','9','31','32','33','43','44','45') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper5.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper5.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();

                            //================问题类型=====================
                            problemTypeCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                        }
                    }
                    //镇级河段 一般河湖：每旬巡查不少于一次；黑臭河湖：每周巡查不少于一次，每次完成不低于本辖区黑臭水体总数的30%，每月完成一轮全覆盖巡查；
                    if ((type.Equals("4") || type.Equals("2") || type.Equals("7") || type.Equals("9") || type.Equals("11") || type.Equals("13")) && townSwitch)
                    {
                        if (Convert.ToInt32(dt_start.DayOfWeek) != 1)
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getWeek(dt_start.AddDays(-7));
                        String cycle = dt_start.AddDays(-7).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('12','50','13','34','35','36','46','47','48') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper5.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper5.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();

                            //================问题类型=====================
                            problemTypeCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                        }
                    }
                    //村级河段 一般河湖：每周巡查不少于一次；黑臭河湖：每个工作日一巡，每周完成黑臭水体全覆盖巡查；
                    if ((type.Equals("5") || type.Equals("2") || type.Equals("8") || type.Equals("9") || type.Equals("12") || type.Equals("13")) && villageSwitch)
                    {
                        String startTm = dt_start.ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.ToString("yyyy-MM-dd 23:59:59");

                        int periods = dt_start.DayOfYear;
                        String cycle = dt_start.Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('16','37','49') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper5.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();

                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper5.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();
                            //================问题类型=====================
                            problemTypeCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                        }
                    }

                    dt_start = dt_start.AddDays(1);
                }

                //writeProgressInfo((i + 1) + "/" + dtRiver.Rows.Count + ":" + section_id, rtbProblemType);
                //==============================跑固定周期的数据====================================
                if (fixedPeriodWitch)//跑固定周期数据开关是否勾上
                {
                    List<Dictionary<String, Object>> cycleList = getCycleList();
                    for (int index = 0; index < cycleList.Count; index++)
                    {

                        String startTm = cycleList[index]["startTm"].ToString();
                        String endTm = cycleList[index]["endTm"].ToString();
                        String cycle = cycleList[index]["cycle"].ToString();

                        if ((type.Equals("1") || type.Equals("2") || type.Equals("9") || type.Equals("13")) && citySwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('1','27','3','2','39','4','28','29','30','40','41','42') and del_flag='0' and "
                                    + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper5.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper5.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题类型=====================
                                problemTypeCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                            }
                        }
                        if ((type.Equals("3") || type.Equals("2") || type.Equals("6") || type.Equals("9") || type.Equals("10") || type.Equals("13")) && areaSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('7','8','9','31','32','33','43','44','45') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper5.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();

                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper5.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题类型=====================
                                problemTypeCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                            }
                        }
                        if ((type.Equals("4") || type.Equals("2") || type.Equals("7") || type.Equals("9") || type.Equals("11") || type.Equals("13")) && townSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('12','50','13','34','35','36','46','47','48') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper5.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper5.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题类型=====================
                                problemTypeCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                            }
                        }
                        if ((type.Equals("5") || type.Equals("2") || type.Equals("8") || type.Equals("9") || type.Equals("12") || type.Equals("13")) && villageSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('16','37','49') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper5.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper5.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题类型=====================
                                problemTypeCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                            }
                        }
                    }
                }

                //sw.Stop();
                //writeProgressInfo("[" + sw.Elapsed.TotalSeconds + "]" + (i + 1) + "/" + dtRiver.Rows.Count + ":" + section_id, rtbProblemType);
                //lblInfo.Text = dtRiver.Rows[i][0].ToString();
                bgWork.ReportProgress(i + 1);
            }
        }

        //问题类型统计
        private void problemTypeCount(String userId, String cycle, String river_id, String section_id, String base_id, String startTm, String endTm, Boolean ifDelOld,String section_type)
        {
            //获取base_id
            //String sql = "select id from rtd_base where user_id='" + userId + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

            //DataTable dtBaseInfo = SqlHelper5.GetDataTable(sql, null);
            //if (dtBaseInfo.Rows.Count <= 0)
            //    return;
            //String base_id = dtBaseInfo.Rows[0][0].ToString();

            //工业废水排放	养殖污染	违法建设	排水设施	农家乐	建筑废弃物	堆场码头	生活垃圾	工程维护	其他
            String gyfs = "0";
            String yzwr = "0";
            String wfjs = "0";
            String psss = "0";
            String njl = "0";
            String zjfxw = "0";
            String dcmt = "0";
            String shlj = "0";
            String gcwh = "0";
            String other = "0";

            String sql = "select type,count(problemid) nums from ptl_reportproblem where state<>'-1' and sectionid in"
                + "(select id from rm_river_lake where parent_ids like '%" + section_id + "%')"
                + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                + "group by type";

            DataTable dtProblemTypeNums = SqlHelper5.GetDataTable(sql, null);
            for (int k = 0; k < dtProblemTypeNums.Rows.Count; k++)
            {
                String problem_type = dtProblemTypeNums.Rows[k]["type"].ToString();

                switch (problem_type)
                {
                    case "0"://工业废水排放
                        gyfs = dtProblemTypeNums.Rows[k]["nums"].ToString();
                        break;
                    case "1"://养殖污染
                        yzwr = dtProblemTypeNums.Rows[k]["nums"].ToString();
                        break;
                    case "2"://排水设施
                        psss = dtProblemTypeNums.Rows[k]["nums"].ToString();
                        break;
                    case "3"://违法建设
                        wfjs = dtProblemTypeNums.Rows[k]["nums"].ToString();
                        break;
                    case "4"://农家乐
                        njl = dtProblemTypeNums.Rows[k]["nums"].ToString();
                        break;
                    case "5"://建筑废弃物
                        zjfxw = dtProblemTypeNums.Rows[k]["nums"].ToString();
                        break;
                    case "6"://堆场码头
                        dcmt = dtProblemTypeNums.Rows[k]["nums"].ToString();
                        break;
                    case "7"://工程维护
                        gcwh = dtProblemTypeNums.Rows[k]["nums"].ToString();
                        break;
                    case "8"://生活垃圾
                        shlj = dtProblemTypeNums.Rows[k]["nums"].ToString();
                        break;
                    case "10"://其他
                        other = dtProblemTypeNums.Rows[k]["nums"].ToString();
                        break;
                }
            }

            //===========================问题整改-按问题类型=============================

            //工业废水排放问题办结数	
            sql = "select count(problemid) nums,'工业废水排放问题办结数' type,1 orders from ptl_reportproblem where type='0' and state='4' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            sql += " union ";
            //工业废水排放问题超期未办结数	
            sql += "select count(problemid) nums,'工业废水排放问题超期未办结数' type,2 orders from ptl_reportproblem where type='0' and state<>'-1'and round(to_number(sysdate-time))"
             + ">(select value from sys_dict where type='problem_deadline' and label=type) and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            sql += " union ";
            //工业废水排放问题挂账数	
            sql += "select count(problemid) nums,'工业废水排放问题挂账数' type,3 orders from ptl_reportproblem where type='0' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and problemid in(select problem_id from ptl_problem_account where "
              + "apply_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and apply_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            sql += " union ";
            //养殖污染问题办结数	
            sql += "select count(problemid) nums,'养殖污染问题办结数' type,4 orders from ptl_reportproblem where type='1' and state='4' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            sql += " union ";
            //养殖污染问题超期未办结数	
            sql += "select count(problemid) nums,'养殖污染问题超期未办结数' type,5 orders from ptl_reportproblem where type='1' and state<>'-1'and round(to_number(sysdate-time))"
             + ">(select value from sys_dict where type='problem_deadline' and label=type) and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            sql += " union ";
            //养殖污染问题挂账数	
            sql += "select count(problemid) nums,'养殖污染问题挂账数' type,6 orders from ptl_reportproblem where type='1' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and problemid in(select problem_id from ptl_problem_account where "
              + "apply_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and apply_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            sql += " union ";
            //违法建设问题办结数	
            sql += "select count(problemid) nums,'违法建设问题办结数' type,7 orders from ptl_reportproblem where type='3' and state='4' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            sql += " union ";
            //违法建设问题超期未办结数	
            sql += "select count(problemid) nums,'违法建设问题超期未办结数' type,8 orders from ptl_reportproblem where type='3' and state<>'-1'and round(to_number(sysdate-time))"
             + ">(select value from sys_dict where type='problem_deadline' and label=type) and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            sql += " union ";
            //违法建设问题挂账数	
            sql += "select count(problemid) nums,'违法建设问题挂账数' type,9 orders from ptl_reportproblem where type='3' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and problemid in(select problem_id from ptl_problem_account where "
              + "apply_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and apply_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            sql += " union ";
            // 排水设施问题办结数	
            sql += "select count(problemid) nums,'排水设施问题办结数' type,10 orders from ptl_reportproblem where type='2' and state='4' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            sql += " union ";
            //排水设施问题超期未办结数	
            sql += "select count(problemid) nums,'排水设施问题超期未办结数' type,11 orders from ptl_reportproblem where type='2' and state<>'-1'and round(to_number(sysdate-time))"
             + ">(select value from sys_dict where type='problem_deadline' and label=type) and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            sql += " union ";
            //排水设施问题挂账数	
            sql += "select count(problemid) nums,'排水设施问题挂账数' type,12 orders from ptl_reportproblem where type='2' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and problemid in(select problem_id from ptl_problem_account where "
              + "apply_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and apply_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            sql += " union ";
            //农家乐问题办结数	
            sql += "select count(problemid) nums,'农家乐问题办结数' type,13 orders from ptl_reportproblem where type='4' and state='4' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            sql += " union ";
            //农家乐问题超期未办结数	
            sql += "select count(problemid) nums,'农家乐问题超期未办结数' type,14 orders from ptl_reportproblem where type='4' and state<>'-1'and round(to_number(sysdate-time))"
             + ">(select value from sys_dict where type='problem_deadline' and label=type) and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            sql += " union ";
            //农家乐问题挂账数	
            sql += "select count(problemid) nums,'农家乐问题挂账数' type,15 orders from ptl_reportproblem where type='4' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and problemid in(select problem_id from ptl_problem_account where "
              + "apply_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and apply_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            sql += " union ";
            //建筑废弃物问题办结数	
            sql += "select count(problemid) nums,'建筑废弃物问题办结数' type,16 orders from ptl_reportproblem where type='5' and state='4' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            sql += " union ";
            //建筑废弃物问题超期未办结数	
            sql += "select count(problemid) nums,'建筑废弃物问题超期未办结数' type,17 orders from ptl_reportproblem where type='5' and state<>'-1'and round(to_number(sysdate-time))"
             + ">(select value from sys_dict where type='problem_deadline' and label=type) and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            sql += " union ";
            //建筑废弃物问题挂账数	
            sql += "select count(problemid) nums,'建筑废弃物问题挂账数' type,18 orders from ptl_reportproblem where type='5' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and problemid in(select problem_id from ptl_problem_account where "
              + "apply_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and apply_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            sql += " union ";
            //堆场码头问题办结数	
            sql += "select count(problemid) nums,'堆场码头问题办结数' type,19 orders from ptl_reportproblem where type='6' and state='4' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            sql += " union ";
            //堆场码头问题超期未办结数	
            sql += "select count(problemid) nums,'堆场码头问题超期未办结数' type,20 orders from ptl_reportproblem where type='6' and state<>'-1'and round(to_number(sysdate-time))"
             + ">(select value from sys_dict where type='problem_deadline' and label=type) and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            sql += " union ";
            //堆场码头问题挂账数	
            sql += "select count(problemid) nums,'堆场码头问题挂账数' type,21 orders from ptl_reportproblem where type='6' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and problemid in(select problem_id from ptl_problem_account where "
              + "apply_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and apply_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            sql += " union ";
            //生活垃圾问题办结数	
            sql += "select count(problemid) nums,'生活垃圾问题办结数' type,22 orders from ptl_reportproblem where type='8' and state='4' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            sql += " union ";
            //生活垃圾问题超期未办结数	
            sql += "select count(problemid) nums,'生活垃圾问题超期未办结数' type,23 orders from ptl_reportproblem where type='8' and state<>'-1'and round(to_number(sysdate-time))"
             + ">(select value from sys_dict where type='problem_deadline' and label=type) and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            sql += " union ";
            //生活垃圾问题挂账数	
            sql += "select count(problemid) nums,'生活垃圾问题挂账数' type,24 orders from ptl_reportproblem where type='8' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and problemid in(select problem_id from ptl_problem_account where "
              + "apply_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and apply_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            sql += " union ";
            //工程维护问题办结数	
            sql += "select count(problemid) nums,'工程维护问题办结数' type,25 orders from ptl_reportproblem where type='7' and state='4' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            sql += " union ";
            //工程维护问题超期未办结数	
            sql += "select count(problemid) nums,'工程维护问题超期未办结数' type,26 orders from ptl_reportproblem where type='7' and state<>'-1'and round(to_number(sysdate-time))"
             + ">(select value from sys_dict where type='problem_deadline' and label=type) and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            sql += " union ";
            //工程维护问题挂账数
            sql += "select count(problemid) nums,'工程维护问题挂账数' type,27 orders from ptl_reportproblem where type='7' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like  '%" + section_id + "%')"
              + " and problemid in(select problem_id from ptl_problem_account where "
              + "apply_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and apply_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            sql += " order by orders";
            DataTable dtData = SqlHelper5.GetDataTable(sql, null);


            String industrialWaterFinishNums = dtData.Rows[0][0].ToString();
            String industrialWaterOverDueNums = dtData.Rows[1][0].ToString();
            String industrialWaterHangUpNums = dtData.Rows[2][0].ToString();

            String breedFinishNums = dtData.Rows[3][0].ToString();
            String breedOverDueNums = dtData.Rows[4][0].ToString();
            String breedHangUpNums = dtData.Rows[5][0].ToString();

            String illigalBuildingFinishNums = dtData.Rows[6][0].ToString();
            String illigalBuildingOverDueNums = dtData.Rows[7][0].ToString();
            String illigalBuildingHangUpNums = dtData.Rows[8][0].ToString();

            String drainFinishNums = dtData.Rows[9][0].ToString();
            String drainOverDueNums = dtData.Rows[10][0].ToString();
            String drianHangUpNums = dtData.Rows[11][0].ToString();

            String agritainmentFinishNums = dtData.Rows[12][0].ToString();
            String agritainmentOverDueNums = dtData.Rows[13][0].ToString();
            String agritainmentHangUpNums = dtData.Rows[14][0].ToString();

            String buildingWasteFinishNums = dtData.Rows[15][0].ToString();
            String buildingWasteOverDueNums = dtData.Rows[16][0].ToString();
            String buildingWasteHangUpNums = dtData.Rows[17][0].ToString();

            String yardWharfFinishNums = dtData.Rows[18][0].ToString();
            String yardWharfOverDueNums = dtData.Rows[19][0].ToString();
            String yardWharfHangUpNums = dtData.Rows[20][0].ToString();

            String garbageFinishNums = dtData.Rows[21][0].ToString();
            String garbageOverDueNums = dtData.Rows[22][0].ToString();
            String garbageHangUpNums = dtData.Rows[23][0].ToString();

            String engineeringMaintenanceFinishNums = dtData.Rows[24][0].ToString();
            String engineeringMaintenanceOverDueNums = dtData.Rows[25][0].ToString();
            String engineeringMaintenanceHangUpNums = dtData.Rows[26][0].ToString();

            //插入RTD_PROBLEM_TYPE 表
            String problemTypeId = Guid.NewGuid().ToString().Replace("-", "");

            if (if_delAndInDB)
            {
                if (ifDelOld)
                {
                    sql = "delete from RTD_PROBLEM_TYPE where base_id='" + base_id + "'";

                    SqlHelper5.ExecuteNonQuery(sql, CommandType.Text, null);
                }

                sql = "Insert into RTD_PROBLEM_TYPE (ID,BASE_ID,WASTEWATER_DISCHARGE,FARMING_POLLUT,ILLEGAL_CONSTRUCTION,DRAINAGE_FACILITIES,FARM_STAY,"
                    + "CONSTRUCTION_WASTE,YARD_WHARF,GARBAGE,PROJECT_MAINTENANCE,OTHERS,WASTEWATER_DISCHARGE_FINISH,WASTEWATER_DISCHARGE_OVERDUE,WASTEWATER_DISCHARGE_ACCOUNT,FARMING_POLLUT_FINISH,FARMING_POLLUT_OVERDUE,FARMING_POLLUT_ACCOUNT,"
                    + "ILLEGAL_CONSTRUCTION_FINISH,ILLEGAL_CONSTRUCTION_OVERDUE,ILLEGAL_CONSTRUCTION_ACCOUNT,DRAINAGE_FACILITIES_FINISH,DRAINAGE_FACILITIES_OVERDUE,DRAINAGE_FACILITIES_ACCOUNT,FARM_STAY_FINISH,FARM_STAY_OVERDUE,"
                    + "FARM_STAY_ACCOUNT,CONSTRUCTION_WASTE_FINISH,CONSTRUCTION_WASTE_OVERDUE,CONSTRUCTION_WASTE_ACCOUNT,YARD_WHARF_FINISH,YARD_WHARF_OVERDUE,YARD_WHARF_ACCOUNT,GARBAGE_FINISH,GARBAGE_OVERDUE,GARBAGE_ACCOUNT,"
                    + "PROJECT_MAINTENANCE_FINISH,PROJECT_MAINTENANCE_OVERDUE,PROJECT_MAINTENANCE_ACCOUNT,START_TIME,END_TIME,user_id,river_id,section_id,cycle,section_type)"
                    + " values ('" + problemTypeId + "','" + base_id + "','" + gyfs + "','" + yzwr + "','" + wfjs + "','" + psss + "','" + njl + "','" + zjfxw + "','" + dcmt + "','" + shlj + "','" + gcwh + "','" + other + "','" + industrialWaterFinishNums + "','" + industrialWaterOverDueNums + "','" + industrialWaterHangUpNums + "','" + breedFinishNums + "','" + breedOverDueNums + "','" + breedHangUpNums + "','" + illigalBuildingFinishNums + "','" + illigalBuildingOverDueNums + "','" + illigalBuildingHangUpNums
                    + "','" + drainFinishNums + "','" + drainOverDueNums + "','" + drianHangUpNums + "','" + agritainmentFinishNums + "','" + agritainmentOverDueNums + "','" + agritainmentHangUpNums + "','" + buildingWasteFinishNums + "','" + buildingWasteOverDueNums + "','" + buildingWasteHangUpNums + "','" + yardWharfFinishNums + "','" + yardWharfOverDueNums + "','" + yardWharfHangUpNums + "','" + garbageFinishNums + "','" + garbageOverDueNums + "','" + garbageHangUpNums + "','"
                    + engineeringMaintenanceFinishNums + "','" + engineeringMaintenanceOverDueNums + "','" + engineeringMaintenanceHangUpNums + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')" + ",to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + ",'" + userId + "','" + river_id + "','" + section_id + "','" + cycle + "','" + section_type + "')";

                SqlHelper5.ExecuteNonQuery(sql, CommandType.Text, null);
            }


        }

        private void workerProblemType_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (pBarProblemType.Value < pBarProblemType.Maximum)
                pBarProblemType.Value += 1;
        }

        void workerProblemType_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pBarProblemType.Value = pBarProblemType.Maximum;
        }

        //===============四个查清=================================
        private void btnFourCheck_Click(object sender, EventArgs e)
        {
            //if (dtpStart.Value < Convert.ToDateTime("2018-07-16 00:00:00"))
            //    dtpStart.Text = "2018-07-16 00:00:00";
            pBarFourCheck.Value = 0;
            pBarFourCheck.Minimum = 0;

            bg_fourCheck.DoWork += new DoWorkEventHandler(worker_FourCheck);
            bg_fourCheck.ProgressChanged += new ProgressChangedEventHandler(workerFourCheck_ProgressChanged);
            bg_fourCheck.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerFourCheck_RunWorkerCompleted);

            bg_fourCheck.RunWorkerAsync();
        }

        void worker_FourCheck(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(问题类型:RTD_FOUR_CHECK)...";
            //writeInfo(lblInfo.Text);
            this.BeginInvoke(wt, new Object[] { lblInfo.Text });
            writeProgressInfo(lblInfo.Text, rtbFourCheck);

            runFourCheck(pBarFourCheck, bg_fourCheck);

            lblInfo.Text = "数据处理完成(问题类型:RTD_FOUR_CHECK)";
            //writeInfo(lblInfo.Text);
            this.BeginInvoke(wt, new Object[] { lblInfo.Text });
            writeProgressInfo(lblInfo.Text, rtbFourCheck);
        }

        public void runFourCheck(ProgressBar pBar, BackgroundWorker bgWork)
        {
            //DateTime dt_start = Convert.ToDateTime(start_time);
            DateTime dt_end = Convert.ToDateTime(end_time);

            String sql = "";

            sql = "select id,type,addvcd_area,addvcd_town,addvcd_village,substr(parent_ids, 0, instr(parent_ids, ',') - 1) river_id,river_side,parent_ids from rm_river_lake";

            String riverType = "";
            if (citySwitch)
                riverType += "'1','2','9','13',";
            if (areaSwitch)
                riverType += "'2','3','6','9','10','13',";
            if (townSwitch)
                riverType += "'2','4','7','9','11','13',";
            if (villageSwitch)
                riverType += "'2','5','8','9','12','13'";

            riverType = riverType.TrimEnd(',');

            sql += " where type in(" + riverType + ")";

            DataTable dtRiver = SqlHelper6.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dtRiver.Rows.Count;

            sql = "select * from ptl_fourcheck_phase where end_date>=to_date('"+start_time+"','yyyy-MM-dd HH24:mi:ss') "
                +"and start_date<=to_date('"+end_time+"','yyyy-MM-dd HH24:mi:ss')";

            DataTable dtPhase = SqlHelper6.GetDataTable(sql, null);
            for (var i = 0; i < dtRiver.Rows.Count; i++)
            {
                String type = dtRiver.Rows[i]["type"].ToString();
                String river_id = dtRiver.Rows[i]["river_id"].ToString();
                String section_id = dtRiver.Rows[i]["id"].ToString();
                String side = dtRiver.Rows[i]["river_side"].ToString();

                String parent_ids = dtRiver.Rows[i]["parent_ids"].ToString();
                String[] ids = parent_ids.Split(',');

                String area_section = null;
                String town_section = null;

                if (ids.Length >= 2)
                {
                    area_section = ids[1];
                }

                if (ids.Length >= 3)
                {
                    town_section = ids[2];
                }

                String addvcd_city = "440100000000";
                String addvcd_area = dtRiver.Rows[i]["addvcd_area"].ToString();
                String addvcd_town = dtRiver.Rows[i]["addvcd_town"].ToString();
                String addvcd_village = dtRiver.Rows[i]["addvcd_village"].ToString();

                //writeInfo((i + 1) + ":" + section_id);
                //this.BeginInvoke(wt, new Object[] { (i + 1) + ":" + section_id });
                writeProgressInfo((i + 1) + "/" + dtRiver.Rows.Count + ":" + section_id, rtbFourCheck);

                for(int index=0;index<dtPhase.Rows.Count;index++)
                {
                    String cycle = dtPhase.Rows[index]["phase"].ToString()+"期";
                    String startTm = dtPhase.Rows[index]["start_date"].ToString();
                    String endTm = dtPhase.Rows[index]["end_date"].ToString();
                    String chiefPhaseType=dtPhase.Rows[index]["type"].ToString();

                    DateTime dt_start = Convert.ToDateTime(startTm);
                    //区级河段 一般河湖：每月不少于1次；黑臭河湖：每月巡查不少于一次，每次巡查完成不低于本辖区黑臭水体总条数的15%，每半年完成一轮全覆盖巡查；
                    if ((type.Equals("3") || type.Equals("2") || type.Equals("6") || type.Equals("9") || type.Equals("10") || type.Equals("13")) && chiefPhaseType.Equals("3") && areaSwitch)
                    {
                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('7','8','9','31','32','33','43','44','45') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper6.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            String river_type = dtUserInfo.Rows[j]["river_type"].ToString();

                            //================四个查清=====================
                            fourCheckCount(userid, river_type, null, addvcd_city, addvcd_area, addvcd_town, addvcd_village, type, side, area_section, town_section, cycle, river_id, section_id, startTm, endTm, true);
                        }
                    }
                    //镇级河段 一般河湖：每旬巡查不少于一次；黑臭河湖：每周巡查不少于一次，每次完成不低于本辖区黑臭水体总数的30%，每月完成一轮全覆盖巡查；
                    if ((type.Equals("4") || type.Equals("2") || type.Equals("7") || type.Equals("9") || type.Equals("11") || type.Equals("13")) && chiefPhaseType.Equals("2") && townSwitch)
                    {
                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('12','50','13','34','35','36','46','47','48') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper6.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            String river_type = dtUserInfo.Rows[j]["river_type"].ToString();

                            //================问题类型=====================
                            fourCheckCount(userid, river_type, null, addvcd_city, addvcd_area, addvcd_town, addvcd_village, type, side, area_section, town_section, cycle, river_id, section_id, startTm, endTm, true);
                        }
                    }
                    //村级河段 一般河湖：每周巡查不少于一次；黑臭河湖：每个工作日一巡，每周完成黑臭水体全覆盖巡查；
                    if ((type.Equals("5") || type.Equals("2") || type.Equals("8") || type.Equals("9") || type.Equals("12") || type.Equals("13")) && chiefPhaseType.Equals("1") && villageSwitch)
                    {
                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('16','37','49') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper6.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            String river_type = dtUserInfo.Rows[j]["river_type"].ToString();

                            //================问题类型=====================
                            fourCheckCount(userid, river_type, null, addvcd_city, addvcd_area, addvcd_town, addvcd_village, type, side, area_section, town_section, cycle, river_id, section_id, startTm, endTm, true);
                        }
                    }
                }

                //==============================跑固定周期的数据====================================
                if (fixedPeriodWitch)//跑固定周期数据开关是否勾上
                {
                    List<Dictionary<String, Object>> cycleList = getCycleList();
                    for (int index = 0; index < cycleList.Count; index++)
                    {

                        String startTm = cycleList[index]["startTm"].ToString();
                        String endTm = cycleList[index]["endTm"].ToString();
                        String cycle = cycleList[index]["cycle"].ToString();

                        if ((type.Equals("3") || type.Equals("2") || type.Equals("6") || type.Equals("9") || type.Equals("10") || type.Equals("13")) && areaSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('7','8','9','31','32','33','43','44','45') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper6.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                String river_type = dtUserInfo.Rows[j]["river_type"].ToString();

                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper6.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题类型=====================
                                fourCheckCount(userid, river_type, base_id, addvcd_city, addvcd_area, addvcd_town, addvcd_village, type, side, area_section, town_section, cycle, river_id, section_id, startTm, endTm, true);
                            }
                        }
                        if ((type.Equals("4") || type.Equals("2") || type.Equals("7") || type.Equals("9") || type.Equals("11") || type.Equals("13")) && townSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('12','50','13','34','35','36','46','47','48') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper6.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                String river_type = dtUserInfo.Rows[j]["river_type"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper6.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题类型=====================
                                fourCheckCount(userid, river_type, base_id, addvcd_city, addvcd_area, addvcd_town, addvcd_village, type, side, area_section, town_section, cycle, river_id, section_id, startTm, endTm, true);
                            }
                        }
                        if ((type.Equals("5") || type.Equals("2") || type.Equals("8") || type.Equals("9") || type.Equals("12") || type.Equals("13")) && villageSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('16','37','49') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper6.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                String river_type = dtUserInfo.Rows[j]["river_type"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper6.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题类型=====================
                                fourCheckCount(userid, river_type, base_id, addvcd_city, addvcd_area, addvcd_town, addvcd_village, type, side, area_section, town_section, cycle, river_id, section_id, startTm, endTm, true);
                            }
                        }
                    }
                }

                //lblInfo.Text = dtRiver.Rows[i][0].ToString();
                bgWork.ReportProgress(i + 1);
            }
        }

        //四个查清统计
        private void fourCheckCount(String userId,String river_type, String base_id,String addvcd_city, String addvcd_area, String addvcd_town, String addvcd_village, String section_type,String side,String area_section,String town_section,String cycle, String river_id, String section_id, String startTm, String endTm, Boolean ifDelOld)
        {
            //===================四个查清====================			
            //四个查清完成次数
            String sql = "select start_date,count(check_type) nums from"
                + " ("
                + "     select start_date,check_type from ptl_fourcheck where user_id='" + userId + "'and create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                + "     group by start_date,check_type"
                + " ) group by start_date having count(check_type)>=4";

            DataTable dtFourCheckNums = SqlHelper6.GetDataTable(sql, null);
            String fourcheckNums = dtFourCheckNums.Rows.Count.ToString();
            //四个查清问题数
            sql = "select count(id) nums from ptl_fourcheck where user_id='" + userId + "'and create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                + " and HAVA_PROBLEM='1'";

            String fourcheckProblemNums = SqlHelper6.ExecuteScalar(sql, CommandType.Text, null).ToString();
            //两岸贯通查清次数
            sql = "select start_date,count(id) nums from ptl_fourcheck where user_id='" + userId + "'and create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                + " and check_type='1' group by start_date";

            DataTable dtLagtNums = SqlHelper6.GetDataTable(sql, null);
            String lagtNums = dtLagtNums.Rows.Count.ToString();

            //两岸通道贯通上报问题数量
            sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                + " and user_id='" + userId + "' and check_type='1' and hava_problem>=1 and section_id='" + section_id + "'";

            String not_connect_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();

            //排水口查清次数
            sql = "select start_date,count(id) nums from ptl_fourcheck where user_id='" + userId + "'and create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                + " and check_type='4' group by start_date";

            DataTable dtPskNums = SqlHelper6.GetDataTable(sql, null);
            String pskNums = dtPskNums.Rows.Count.ToString();

            //异常排水口上报问题数量
            String outfall_exception_cnt = "0";

            sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                + " and user_id='" + userId + "' and check_type='4' and hava_problem>=1 and section_id='" + section_id + "'";

            outfall_exception_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
            //散乱污场所查清次数
            sql = "select start_date,count(id) nums from ptl_fourcheck where user_id='" + userId + "'and create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                + " and check_type='3' group by start_date";

            DataTable dtSlwNums = SqlHelper6.GetDataTable(sql, null);
            String slwNums = dtSlwNums.Rows.Count.ToString();

            //散乱污场所上报问题数量
            String debunching_place_cnt = "0";

            sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                + " and user_id='" + userId + "' and check_type='3' and hava_problem>=1 and section_id='" + section_id + "'";

            debunching_place_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
            //疑似违建查清次数
            sql = "select start_date,count(id) nums from ptl_fourcheck where user_id='" + userId + "'and create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                + " and check_type='2' group by start_date";

            DataTable dtYswjNums = SqlHelper6.GetDataTable(sql, null);
            String yswjNums = dtYswjNums.Rows.Count.ToString();

            //疑似违法建筑上报问题数量
            String illegal_building_cnt = "0";

            sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                + " and user_id='" + userId + "' and check_type='2' and hava_problem>=1 and section_id='" + section_id + "'";

            illegal_building_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
            //插入RTD_FOUR_CHECK 表
            String fourCheckId = Guid.NewGuid().ToString().Replace("-", "");

            if (if_delAndInDB)
            {
                if (base_id == null)
                {
                    //获取old base_id
                    sql = "select id from rtd_base where type='2' and START_TIME=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and cycle='" + cycle + "' and user_id='" + userId + "' and section_id='" + section_id + "'";

                    DataTable dtOldBaseInfo = SqlHelper6.GetDataTable(sql, null);

                    if (dtOldBaseInfo.Rows.Count > 0)
                    {
                        base_id = dtOldBaseInfo.Rows[0][0].ToString();
                    }
                    else
                    {
                        base_id = Guid.NewGuid().ToString().Replace("-", "");
                        sql = "Insert into RTD_BASE (ID,ADDVCD_CITY,ADDVCD_AREA,ADDVCD_TOWN,ADDVCD_VILLAGE,RIVER_ID,SECTION_ID,SECTION_TYPE,BANK,USER_ID,CYCLE,start_time,end_time,area_section,town_section,type,river_type)"
                                           + " values ('" + base_id + "','" + addvcd_city + "','" + addvcd_area + "','" + addvcd_town + "','" + addvcd_village + "','" + river_id + "','" + section_id + "','" + section_type + "','" + side + "','"
                                           + userId + "','" + cycle + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')" + ",to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'),'" + area_section + "','" + town_section + "','2','" + river_type + "')";

                        SqlHelper6.ExecuteNonQuery(sql, CommandType.Text, null);
                    }
                }

                //是否删除老数据
                if (ifDelOld)
                {
                    sql = "delete RTD_FOUR_CHECK where base_id='" + base_id + "'";
                    SqlHelper6.ExecuteNonQuery(sql, CommandType.Text, null);
                }

                sql = "Insert into RTD_FOUR_CHECK (ID,BASE_ID,FINISH_TIMES,REPORT_PROBLEMS,BANKS_LINK_UP_TIMES,OUTFALL_TIMES,DISORDERLY_DIRT_TIMES,ILLEGAL_BUILDING_TIMES,start_time,end_time"
                     + ",user_id,cycle,river_id,section_id,section_type,BANKS_LINK_UP_PROBLEMS,OUTFALL_PROBLEMS,DISORDERLY_DIRT_PROBLEMS,ILLEGAL_BUILDING_PROBLEMS)"
                     + " values ('" + fourCheckId + "','" + base_id + "','" + fourcheckNums + "','" + fourcheckProblemNums + "','" + lagtNums + "','" + pskNums + "','" + slwNums + "','" + yswjNums + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')" + ",to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                     + ",'" + userId + "','" + cycle + "','" + river_id + "','" + section_id + "','" + section_type + "','" + not_connect_cnt + "','" + outfall_exception_cnt + "','" + debunching_place_cnt + "','" + illegal_building_cnt + "')";

                SqlHelper6.ExecuteNonQuery(sql, CommandType.Text, null);
            }
        }

        private void workerFourCheck_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (pBarFourCheck.Value < pBarFourCheck.Maximum)
                pBarFourCheck.Value += 1;
        }

        void workerFourCheck_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pBarFourCheck.Value = pBarFourCheck.Maximum;
        }

        //===========================问题交办、重大问题上报、整改================================
        private void btnProblemAssign_Click(object sender, EventArgs e)
        {
            pBarProblemAssign.Value = 0;
            pBarProblemAssign.Minimum = 0;

            bg_problemAssign.DoWork += new DoWorkEventHandler(worker_ProblemAssign);
            bg_problemAssign.ProgressChanged += new ProgressChangedEventHandler(workerProblemAssign_ProgressChanged);
            bg_problemAssign.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerProblemAssign_RunWorkerCompleted);

            bg_problemAssign.RunWorkerAsync();
        }

        void worker_ProblemAssign(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(问题交办、重大问题上报、整改:RTD_PROBLEM_ASSIGN)...";
            //writeInfo(lblInfo.Text);
            this.BeginInvoke(wt, new Object[] { lblInfo.Text });
            writeProgressInfo(lblInfo.Text, rtbProblemAssign);

            runProblemAssign(pBarProblemAssign, bg_problemAssign);

            lblInfo.Text = "数据处理完成(问题交办、重大问题上报、整改:RTD_PROBLEM_ASSIGN)";
            //writeInfo(lblInfo.Text);
            this.BeginInvoke(wt, new Object[] { lblInfo.Text });
            writeProgressInfo(lblInfo.Text, rtbProblemAssign);
        }

        public void runProblemAssign(ProgressBar pBar, BackgroundWorker bgWork)
        {
            //DateTime dt_start = Convert.ToDateTime(start_time);
            DateTime dt_end = Convert.ToDateTime(end_time);

            String sql = "";

            sql = "select id,type,addvcd_area,addvcd_town,addvcd_village,substr(parent_ids, 0, instr(parent_ids, ',') - 1) river_id,river_side,parent_ids from rm_river_lake";

            String riverType = "";
            if (citySwitch)
                riverType += "'1','2','9','13',";
            if (areaSwitch)
                riverType += "'2','3','6','9','10','13',";
            if (townSwitch)
                riverType += "'2','4','7','9','11','13',";
            if (villageSwitch)
                riverType += "'2','5','8','9','12','13'";

            riverType = riverType.TrimEnd(',');

            sql += " where type in(" + riverType + ")";

            DataTable dtRiver = SqlHelper7.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dtRiver.Rows.Count;
            for (var i = 0; i < dtRiver.Rows.Count; i++)
            {
                String type = dtRiver.Rows[i]["type"].ToString();
                String river_id = dtRiver.Rows[i]["river_id"].ToString();
                String section_id = dtRiver.Rows[i]["id"].ToString();
                String side = dtRiver.Rows[i]["river_side"].ToString();

                String parent_ids = dtRiver.Rows[i]["parent_ids"].ToString();
                String[] ids = parent_ids.Split(',');

                String area_section = null;
                String town_section = null;

                if (ids.Length >= 2)
                {
                    area_section = ids[1];
                }

                if (ids.Length >= 3)
                {
                    town_section = ids[2];
                }

                String addvcd_city = "440100000000";
                String addvcd_area = dtRiver.Rows[i]["addvcd_area"].ToString();
                String addvcd_town = dtRiver.Rows[i]["addvcd_town"].ToString();
                String addvcd_village = dtRiver.Rows[i]["addvcd_village"].ToString();

                //writeInfo((i + 1) + ":" + section_id);
                //this.BeginInvoke(wt, new Object[] { (i + 1) + ":" + section_id });
                writeProgressInfo((i + 1) + "/" + dtRiver.Rows.Count + ":" + section_id, rtbProblemAssign);

                //市级河段 每季度不少于1次
                DateTime dt_start = Convert.ToDateTime(start_time);

                while (dt_start <= dt_end)
                {
                    if ((type.Equals("1") || type.Equals("2") || type.Equals("9") || type.Equals("13")) && citySwitch)
                    {
                        if (dt_start.Day != 1 || (dt_start.Month == 2 || dt_start.Month == 3 || dt_start.Month == 5 || dt_start.Month == 6 || dt_start.Month == 8 || dt_start.Month == 9 || dt_start.Month == 11 || dt_start.Month == 12))
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddMonths(-3).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getCityChiefPeriod(dt_start.AddMonths(-3));
                        String cycle = dt_start.AddMonths(-3).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('1','27','3','2','39','4','28','29','30','40','41','42') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper7.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper7.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();

                            //================问题类型=====================
                            problemAssignCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true,type);
                        }
                    }
                    //区级河段 一般河湖：每月不少于1次；黑臭河湖：每月巡查不少于一次，每次巡查完成不低于本辖区黑臭水体总条数的15%，每半年完成一轮全覆盖巡查；
                    if ((type.Equals("3") || type.Equals("2") || type.Equals("6") || type.Equals("9") || type.Equals("10") || type.Equals("13")) && areaSwitch)
                    {
                        if (dt_start.Day != 1 || (dt_start.Month == 2 || dt_start.Month == 4 || dt_start.Month == 6 || dt_start.Month == 8 || dt_start.Month == 10 || dt_start.Month == 12))
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddMonths(-2).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getAreaChiefPeriod(dt_start.AddMonths(-2));
                        String cycle = dt_start.AddMonths(-2).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('7','8','9','31','32','33','43','44','45') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper7.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper7.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();

                            //================问题类型=====================
                            problemAssignCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                        }
                    }
                    //镇级河段 一般河湖：每旬巡查不少于一次；黑臭河湖：每周巡查不少于一次，每次完成不低于本辖区黑臭水体总数的30%，每月完成一轮全覆盖巡查；
                    if ((type.Equals("4") || type.Equals("2") || type.Equals("7") || type.Equals("9") || type.Equals("11") || type.Equals("13")) && townSwitch)
                    {
                        if (Convert.ToInt32(dt_start.DayOfWeek) != 1)
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getWeek(dt_start.AddDays(-7));
                        String cycle = dt_start.AddDays(-7).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('12','50','13','34','35','36','46','47','48') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper7.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper7.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();

                            //================问题类型=====================
                            problemAssignCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                        }
                    }
                    //村级河段 一般河湖：每周巡查不少于一次；黑臭河湖：每个工作日一巡，每周完成黑臭水体全覆盖巡查；
                    if ((type.Equals("5") || type.Equals("2") || type.Equals("8") || type.Equals("9") || type.Equals("12") || type.Equals("13")) && villageSwitch)
                    {
                        String startTm = dt_start.ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.ToString("yyyy-MM-dd 23:59:59");

                        int periods = dt_start.DayOfYear;
                        String cycle = dt_start.Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('16','37','49') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper7.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();

                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper7.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();
                            //================问题类型=====================
                            problemAssignCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                        }
                    }

                    dt_start = dt_start.AddDays(1);
                }

                //==============================跑固定周期的数据====================================
                if (fixedPeriodWitch)//跑固定周期数据开关是否勾上
                {
                    List<Dictionary<String, Object>> cycleList = getCycleList();
                    for (int index = 0; index < cycleList.Count; index++)
                    {

                        String startTm = cycleList[index]["startTm"].ToString();
                        String endTm = cycleList[index]["endTm"].ToString();
                        String cycle = cycleList[index]["cycle"].ToString();

                        if ((type.Equals("1") || type.Equals("2") || type.Equals("9") || type.Equals("13")) && citySwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('1','27','3','2','39','4','28','29','30','40','41','42') and del_flag='0' and "
                                    + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper7.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper7.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题类型=====================
                                problemAssignCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                            }
                        }
                        if ((type.Equals("3") || type.Equals("2") || type.Equals("6") || type.Equals("9") || type.Equals("10") || type.Equals("13")) && areaSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('7','8','9','31','32','33','43','44','45') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper7.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();

                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper7.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题类型=====================
                                problemAssignCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                            }
                        }
                        if ((type.Equals("4") || type.Equals("2") || type.Equals("7") || type.Equals("9") || type.Equals("11") || type.Equals("13")) && townSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('12','50','13','34','35','36','46','47','48') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper7.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper7.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题类型=====================
                                problemAssignCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                            }
                        }
                        if ((type.Equals("5") || type.Equals("2") || type.Equals("8") || type.Equals("9") || type.Equals("12") || type.Equals("13")) && villageSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('16','37','49') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper7.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper7.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题类型=====================
                                problemAssignCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true, type);
                            }
                        }
                    }
                }

                //lblInfo.Text = dtRiver.Rows[i][0].ToString();
                bgWork.ReportProgress(i + 1);
            }
        }

        //问题交办统计
        private void problemAssignCount(String userId, String cycle, String river_id, String section_id, String base_id, String startTm, String endTm, Boolean ifDelOld,String section_type)
        {
            //获取base_id
            //String sql = "select id from rtd_base where user_id='" + userId + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

            //DataTable dtBaseInfo = SqlHelper7.GetDataTable(sql, null);
            //if (dtBaseInfo.Rows.Count <= 0)
            //    return;
            //String base_id = dtBaseInfo.Rows[0][0].ToString();

            //===============交办问题==================

            String allSql = "";
            //书面交办问题数--卡交办时间			
            String sql = "select count(problemid) nums,'书面交办问题数--卡交办时间' type,1 orders from ptl_reportproblem where state<>'-1' and sectionid in"
                + " (select id from rm_river_lake where parent_ids like '%" + river_id + "%')"
                + " and problemid in(select problem_id from ptl_problem_paper_assign where "
                + "create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";   

            allSql += sql + " union ";
            //APP交办问题数--卡交办时间
            sql = "select count(problemid) nums,'APP交办问题数--卡交办时间' type,2 orders from ptl_reportproblem where state<>'-1' and sectionid in"
                + " (select id from rm_river_lake where parent_ids like '%" + river_id + "%')"
                + " and problemid in(select problem_id from ptl_problem_assigned where "
               + " assigned_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and assigned_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            allSql += sql + " union ";
            //书面交办问题办结数
            sql = "select count(problemid) nums,'书面交办问题办结数' type,3 orders from ptl_reportproblem where state='4' and sectionid in"
                + " (select id from rm_river_lake where parent_ids like '%" + river_id + "%')"
                + " and problemid in(select problem_id from ptl_problem_paper_assign)"
                + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //APP交办问题办结数
            sql = "select count(problemid) nums,'交办问题办结数' type,4 orders from ptl_reportproblem where state<>'-1' and sectionid in"
                + " (select id from rm_river_lake where parent_ids like '%" + river_id + "%')"
                + " and problemid in(select problem_id from ptl_problem_assigned)"
                + "and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            //===========================重大问题上报=============================
            allSql += sql + " union ";
            //河长上报数
            sql = sql = "select count(problemid) nums,'河长上报数' type,5 orders from ptl_reportproblem where sectionid in"
            + " (select id from rm_river_lake where parent_ids like  '%" + river_id + "%')"
            + " and problemid in(select problem_id from ptl_problem_assigned where "
            + "ASSIGNED_DATE>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and ASSIGNED_DATE<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            allSql += sql + " union ";
            //公众投诉数
            sql = "select count(problemid) nums,'公众投诉数' type,6 orders from ptl_reportproblem where state<>'-1' and sectionid in"
              + " (select id from rm_river_lake where parent_ids like '%" + river_id + "%')"
              + " and IS_CITIZEN='Y'and problemid in(select problem_id from ptl_problem_assigned where "
              + "ASSIGNED_DATE>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and ASSIGNED_DATE<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            allSql += sql + " union ";
            //市巡查投诉数
            sql = "select count(problemid) nums,'市巡查投诉数' type,7 orders from ptl_reportproblem where state<>'-1' and sectionid in"
             + " (select id from rm_river_lake where parent_ids like  '%" + river_id + "%')"
             + " and (userid in(select id from sys_user_b where river_type='21') or IS_CITIZEN='N')"
             + " and problemid in(select problem_id from ptl_problem_assigned where "
             + "ASSIGNED_DATE>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and ASSIGNED_DATE<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            //===========================重大问题整改=============================
            allSql += sql + " union ";
            //重大问题办结数	
            sql = "select count(problemid) nums,'重大问题办结数' type,8 orders from ptl_reportproblem where state='4' and sectionid in"
            + " (select id from rm_river_lake where parent_ids like  '%" + river_id + "%')"
            + " and problemid in(select problem_id from ptl_problem_assigned where "
            + "ASSIGNED_DATE>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and ASSIGNED_DATE<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            allSql += sql + " union ";
            //重大问题超期未办结数	
            sql = "select count(problemid) nums,'重大问题超期未办结数' type,9 orders from ptl_reportproblem where state<>'-1' and round(to_number(sysdate-time))"
            + ">(select value from sys_dict where type='problem_deadline' and label=type) and sectionid in"
            + " (select id from rm_river_lake where parent_ids like  '%" + river_id + "%')"
            + " and problemid in(select problem_id from ptl_problem_assigned where "
            + "ASSIGNED_DATE>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and ASSIGNED_DATE<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            allSql += sql + " union ";
            //重大问题挂账数
            sql = "select count(problemid) nums,'重大问题挂账数' type,10 orders from ptl_reportproblem where state<>'-1' and sectionid in"
               + " (select id from rm_river_lake where parent_ids like  '%" + river_id + "%')"
               + " and problemid in(select problem_id from ptl_problem_assigned where "
               + "ASSIGNED_DATE>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and ASSIGNED_DATE<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))"
               + " and problemid in(select problem_id from ptl_problem_account where "
               + "apply_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and apply_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'))";

            allSql += sql + " order by orders";
            DataTable dtData = SqlHelper7.GetDataTable(allSql, null);

            String paperProblemNums = dtData.Rows[0][0].ToString();
            String appAssignedProblemNums = dtData.Rows[1][0].ToString();
            String paperProblemFinishNums = dtData.Rows[2][0].ToString();
            String appAssignedProblemFinishNums = dtData.Rows[3][0].ToString();
            String problemNums = dtData.Rows[4][0].ToString();
            String important_citizen_report_nums = dtData.Rows[5][0].ToString();
            String important_citizen_patrol_nums = dtData.Rows[6][0].ToString();
            String important_problem_finish_nums = dtData.Rows[7][0].ToString();
            String important_problem_overDue_nums = dtData.Rows[8][0].ToString();
            String important_problem_hangUp_nums = dtData.Rows[9][0].ToString();

            if (if_delAndInDB)
            {
                if (ifDelOld)
                {
                    sql = "delete from RTD_PROBLEM_ASSIGN where base_id='" + base_id + "'";

                    SqlHelper7.ExecuteNonQuery(sql, CommandType.Text, null);
                }

                //插入RTD_PROBLEM_ASSIGN 表
                String problemAssignId = Guid.NewGuid().ToString().Replace("-", "");

                sql = "Insert into RTD_PROBLEM_ASSIGN (ID,BASE_ID,PAPER_PROBLEMS,APP_PROBLEMS,PAPERT_PROBLEMS_FINISH,APP_PROBLEMS_FINISH,CHIEF_REPORT,CITIZEN_REPORT,CITY_PATROL_REPORT,IMPORTANT_PROBLEMS_FINISH,IMPORTANT_PROBLEMS_OVERDUE,IMPORTANT_PROBLEMS_ACCOUNT,start_time,end_time,user_id,river_id,section_id,cycle,section_type)"
                    + " values ('" + problemAssignId + "','" + base_id + "','" + paperProblemNums + "','" + appAssignedProblemNums + "','" + paperProblemFinishNums + "','" + appAssignedProblemFinishNums + "','" + problemNums + "','" + important_citizen_report_nums + "','"
                    + important_citizen_patrol_nums + "','" + important_problem_finish_nums + "','" + important_problem_overDue_nums + "','" + important_problem_hangUp_nums + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')" + ",to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + ",'" + userId + "','" + river_id + "','" + section_id + "','" + cycle + "','" + section_type + "')";

                SqlHelper7.ExecuteNonQuery(sql, CommandType.Text, null);
            }
        }

        private void workerProblemAssign_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (pBarProblemAssign.Value < pBarProblemAssign.Maximum)
                pBarProblemAssign.Value += 1;
        }

        void workerProblemAssign_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pBarProblemAssign.Value = pBarProblemAssign.Maximum;
        }

        //==========================水质=============================
        private void btnWaterQuality_Click(object sender, EventArgs e)
        {

        }

        //=========================APP使用=======================
        private void btnAppUse_Click(object sender, EventArgs e)
        {
            pBarAppUse.Value = 0;
            pBarAppUse.Minimum = 0;

            bg_appUse.DoWork += new DoWorkEventHandler(worker_AppUse);
            bg_appUse.ProgressChanged += new ProgressChangedEventHandler(workerAppUse_ProgressChanged);
            bg_appUse.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerAppUse_RunWorkerCompleted);

            bg_appUse.RunWorkerAsync();
        }

        void worker_AppUse(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(APP使用:RTD_APP_USE)...";
            //writeInfo(lblInfo.Text);
            this.BeginInvoke(wt, new Object[] { lblInfo.Text });
            writeProgressInfo(lblInfo.Text, rtbAppUse);

            runAppUse(pBarAppUse, bg_appUse);

            lblInfo.Text = "数据处理完成(APP使用:RTD_APP_USE)";
            //writeInfo(lblInfo.Text);
            this.BeginInvoke(wt, new Object[] { lblInfo.Text });
            writeProgressInfo(lblInfo.Text, rtbAppUse);
        }

        public void runAppUse(ProgressBar pBar, BackgroundWorker bgWork)
        {
            //DateTime dt_start = Convert.ToDateTime(start_time);
            DateTime dt_end = Convert.ToDateTime(end_time);

            String sql = "";

            sql = "select id,type,addvcd_area,addvcd_town,addvcd_village,substr(parent_ids, 0, instr(parent_ids, ',') - 1) river_id,river_side,parent_ids from rm_river_lake";

            String riverType = "";
            if (citySwitch)
                riverType += "'1','2','9','13',";
            if (areaSwitch)
                riverType += "'2','3','6','9','10','13',";
            if (townSwitch)
                riverType += "'2','4','7','9','11','13',";
            if (villageSwitch)
                riverType += "'2','5','8','9','12','13'";

            riverType = riverType.TrimEnd(',');

            sql += " where type in(" + riverType + ")";

            DataTable dtRiver = SqlHelper9.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dtRiver.Rows.Count;
            for (var i = 0; i < dtRiver.Rows.Count; i++)
            {
                String type = dtRiver.Rows[i]["type"].ToString();
                String river_id = dtRiver.Rows[i]["river_id"].ToString();
                String section_id = dtRiver.Rows[i]["id"].ToString();
                String side = dtRiver.Rows[i]["river_side"].ToString();

                String parent_ids = dtRiver.Rows[i]["parent_ids"].ToString();
                String[] ids = parent_ids.Split(',');

                String area_section = null;
                String town_section = null;

                if (ids.Length >= 2)
                {
                    area_section = ids[1];
                }

                if (ids.Length >= 3)
                {
                    town_section = ids[2];
                }

                String addvcd_city = "440100000000";
                String addvcd_area = dtRiver.Rows[i]["addvcd_area"].ToString();
                String addvcd_town = dtRiver.Rows[i]["addvcd_town"].ToString();
                String addvcd_village = dtRiver.Rows[i]["addvcd_village"].ToString();

                //writeInfo((i + 1) + ":" + section_id);
                //this.BeginInvoke(wt, new Object[] { (i + 1) + ":" + section_id });
                writeProgressInfo((i + 1) + "/" + dtRiver.Rows.Count + ":" + section_id, rtbAppUse);

                //市级河段 每季度不少于1次
                DateTime dt_start = Convert.ToDateTime(start_time);

                while (dt_start <= dt_end)
                {
                    if ((type.Equals("1") || type.Equals("2") || type.Equals("9") || type.Equals("13")) && citySwitch)
                    {
                        if (dt_start.Day != 1 || (dt_start.Month == 2 || dt_start.Month == 3 || dt_start.Month == 5 || dt_start.Month == 6 || dt_start.Month == 8 || dt_start.Month == 9 || dt_start.Month == 11 || dt_start.Month == 12))
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddMonths(-3).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getCityChiefPeriod(dt_start.AddMonths(-3));
                        String cycle = dt_start.AddMonths(-3).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('1','27','3','2','39','4','28','29','30','40','41','42') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper9.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper9.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();

                            //================问题类型=====================
                            appUseCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true);
                        }
                    }
                    //区级河段 一般河湖：每月不少于1次；黑臭河湖：每月巡查不少于一次，每次巡查完成不低于本辖区黑臭水体总条数的15%，每半年完成一轮全覆盖巡查；
                    if ((type.Equals("3") || type.Equals("2") || type.Equals("6") || type.Equals("9") || type.Equals("10") || type.Equals("13")) && areaSwitch)
                    {
                        if (dt_start.Day != 1 || (dt_start.Month == 2 || dt_start.Month == 4 || dt_start.Month == 6 || dt_start.Month == 8 || dt_start.Month == 10 || dt_start.Month == 12))
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddMonths(-2).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getAreaChiefPeriod(dt_start.AddMonths(-2));
                        String cycle = dt_start.AddMonths(-2).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('7','8','9','31','32','33','43','44','45') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper9.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper9.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();

                            //================问题类型=====================
                            appUseCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true);
                        }
                    }
                    //镇级河段 一般河湖：每旬巡查不少于一次；黑臭河湖：每周巡查不少于一次，每次完成不低于本辖区黑臭水体总数的30%，每月完成一轮全覆盖巡查；
                    if ((type.Equals("4") || type.Equals("2") || type.Equals("7") || type.Equals("9") || type.Equals("11") || type.Equals("13")) && townSwitch)
                    {
                        if (Convert.ToInt32(dt_start.DayOfWeek) != 1)
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getWeek(dt_start.AddDays(-7));
                        String cycle = dt_start.AddDays(-7).Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('12','50','13','34','35','36','46','47','48') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper9.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper9.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();

                            //================问题类型=====================
                            appUseCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true);
                        }
                    }
                    //村级河段 一般河湖：每周巡查不少于一次；黑臭河湖：每个工作日一巡，每周完成黑臭水体全覆盖巡查；
                    if ((type.Equals("5") || type.Equals("2") || type.Equals("8") || type.Equals("9") || type.Equals("12") || type.Equals("13")) && villageSwitch)
                    {
                        String startTm = dt_start.ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.ToString("yyyy-MM-dd 23:59:59");

                        int periods = dt_start.DayOfYear;
                        String cycle = dt_start.Year + periods.ToString("D3");

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('16','37','49') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper9.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();

                            //获取base_id
                            sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

                            DataTable dtBaseInfo = SqlHelper9.GetDataTable(sql, null);
                            if (dtBaseInfo.Rows.Count <= 0)
                                continue;
                            String base_id = dtBaseInfo.Rows[0][0].ToString();
                            //================问题类型=====================
                            appUseCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true);
                        }
                    }

                    dt_start = dt_start.AddDays(1);
                }

                //==============================跑固定周期的数据====================================
                if (fixedPeriodWitch)//跑固定周期数据开关是否勾上
                {
                    List<Dictionary<String, Object>> cycleList = getCycleList();
                    for (int index = 0; index < cycleList.Count; index++)
                    {

                        String startTm = cycleList[index]["startTm"].ToString();
                        String endTm = cycleList[index]["endTm"].ToString();
                        String cycle = cycleList[index]["cycle"].ToString();

                        if ((type.Equals("1") || type.Equals("2") || type.Equals("9") || type.Equals("13")) && citySwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('1','27','3','2','39','4','28','29','30','40','41','42') and del_flag='0' and "
                                    + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper9.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper9.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题类型=====================
                                appUseCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true);
                            }
                        }
                        if ((type.Equals("3") || type.Equals("2") || type.Equals("6") || type.Equals("9") || type.Equals("10") || type.Equals("13")) && areaSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('7','8','9','31','32','33','43','44','45') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper9.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();

                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper9.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题类型=====================
                                appUseCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true);
                            }
                        }
                        else if (type.Equals("4") && areaSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('12','50','13','34','35','36','46','47','48') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper9.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper9.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题类型=====================
                                appUseCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true);
                            }
                        }
                        if ((type.Equals("5") || type.Equals("2") || type.Equals("8") || type.Equals("9") || type.Equals("12") || type.Equals("13")) && villageSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date from sys_user_b where river_type in('16','37','49') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper9.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper9.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================问题类型=====================
                                appUseCount(userid, cycle, river_id, section_id, base_id, startTm, endTm, true);
                            }
                        }
                    }
                }

                //lblInfo.Text = dtRiver.Rows[i][0].ToString();
                bgWork.ReportProgress(i + 1);
            }
        }

        //APP使用
        private void appUseCount(String userId, String cycle, String river_id, String section_id, String base_id, String startTm, String endTm, Boolean ifDelOld)
        {
            //获取base_id
            //String sql = "select id from rtd_base where user_id='" + userId + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='1'";

            //DataTable dtBaseInfo = SqlHelper9.GetDataTable(sql, null);
            //if (dtBaseInfo.Rows.Count <= 0)
            //    return;
            //String base_id = dtBaseInfo.Rows[0][0].ToString();

            String allSql = "";
            //签到天数	
            String sql = "select count(distinct to_char(time, 'YYYY-MM-DD' )) nums,'签到天数' type,1 orders from sys_usersign where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')"
                + "and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                 + " and userid='" + userId + "'";

            allSql += sql + " union ";
            //签到次数	
            sql = "select count(id) nums,'签到次数' type,2 orders from sys_usersign where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')"
                + "and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                 + " and userid='" + userId + "'";

            allSql += sql + " union ";
            //签到率	
            sql = "select round(count(distinct to_char(time, 'YYYY-MM-DD' )) /to_char(to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')-to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')),2) nums,'签到率' type,3 orders "
                 + "from sys_usersign where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')"
                 + "and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                 + " and userid='" + userId + "'";

            //登录时长	

            allSql += sql + " union ";
            //阅读专栏文章篇数	
            sql = "select count(distinct problemid) nums,'阅读专栏文章篇数' type,4 orders from INFO_READINGPRAISE where userid='" + userId + "'"
                  + " and PROBLEMID in(select id from info_news where CREATEDATE>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')"
                  + "and CREATEDATE<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and status in('0','12','3','2','5','7','8','14','15'))";

            allSql += sql + " union ";
            //专栏文章点击次数	
            sql = "select count(problemid) nums,'专栏文章点击次数' type,5 orders from INFO_READINGPRAISE where userid='" + userId + "'"
                  + " and PROBLEMID in(select id from info_news where CREATEDATE>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')"
                  + "and CREATEDATE<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and status in('0','12','3','2','5','7','8','14','15'))";

            //新闻动态阅读时长	

            //通知公告阅读时长	

            //经验交流阅读时长 	

            //河长须知阅读时长	

            //他山之石阅读时长	

            //河长周报阅读时长

            //河长周报留言数	

            //河长周报市民投诉数

            allSql += sql + " order by orders ";
            DataTable dtData = SqlHelper9.GetDataTable(allSql, null);
            String signInDaysNums = dtData.Rows[0][0].ToString();
            String signInCountsNums = dtData.Rows[1][0].ToString();
            String signInRateNums = dtData.Rows[2][0].ToString();
            String readColumNums = dtData.Rows[3][0].ToString();
            String readColumCountsNums = dtData.Rows[4][0].ToString();
            
            if (if_delAndInDB)
            {
                if (ifDelOld)
                {
                    sql = "delete from RTD_APP_USE where base_id='" + base_id + "'";

                    SqlHelper9.ExecuteNonQuery(sql, CommandType.Text, null);
                }

                //插入RTD_APP_USE 表
                String appUseId = Guid.NewGuid().ToString().Replace("-", "");
                sql = "Insert into RTD_APP_USE (ID,BASE_ID,SIGN_DAYS,SIGN_TIMES,SIGN_RATE,READ_NEWS,NEWS_CLICKS,WEEK_REPORT_MSG,start_time,end_time,user_id,river_id,section_id,cycle)"
                    + " values ('" + appUseId + "','" + base_id + "','" + signInDaysNums + "','" + signInCountsNums + "','" + signInRateNums + "','" + readColumNums + "','" + readColumCountsNums + "','" + null + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')" + ",to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + ",'" + userId + "','" + river_id + "','" + section_id + "','" + cycle + "')";

                SqlHelper9.ExecuteNonQuery(sql, CommandType.Text, null);
            }
        }

        private void workerAppUse_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (pBarAppUse.Value < pBarAppUse.Maximum)
                pBarAppUse.Value += 1;
        }

        void workerAppUse_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pBarAppUse.Value = pBarAppUse.Maximum;
        }

        //==================河长积分===========================
        private void btnScore_Click(object sender, EventArgs e)
        {
            pBarScore.Value = 0;
            pBarScore.Minimum = 0;

            bg_score.DoWork += new DoWorkEventHandler(worker_Score);
            bg_score.ProgressChanged += new ProgressChangedEventHandler(workerScore_ProgressChanged);
            bg_score.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerScore_RunWorkerCompleted);

            bg_score.RunWorkerAsync();
        }

        void worker_Score(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(河长积分:RTD_SCORE)...";
            //writeInfo(lblInfo.Text);
            this.BeginInvoke(wt, new Object[] { lblInfo.Text });
            writeProgressInfo(lblInfo.Text, rtbScore);

            runScore(pBarScore, bg_score);

            lblInfo.Text = "数据处理完成(河长积分:RTD_SCORE)";
            //writeInfo(lblInfo.Text);
            this.BeginInvoke(wt, new Object[] { lblInfo.Text });
            writeProgressInfo(lblInfo.Text, rtbScore);
        }

        public void runScore(ProgressBar pBar, BackgroundWorker bgWork)
        {
            //DateTime dt_start = Convert.ToDateTime(start_time);
            DateTime dt_end = Convert.ToDateTime(end_time);

            String sql = "";

            sql = "select id,type,addvcd_area,addvcd_town,addvcd_village,substr(parent_ids, 0, instr(parent_ids, ',') - 1) river_id,river_side,parent_ids from rm_river_lake";

            String riverType = "";
            if (citySwitch)
                riverType += "'1','2','9','13',";
            if (areaSwitch)
                riverType += "'2','3','6','9','10','13',";
            if (townSwitch)
                riverType += "'2','4','7','9','11','13',";
            if (villageSwitch)
                riverType += "'2','5','8','9','12','13'";

            riverType = riverType.TrimEnd(',');

            sql += " where type in(" + riverType + ")";

            DataTable dtRiver = SqlHelper10.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dtRiver.Rows.Count;
            for (var i = 0; i < dtRiver.Rows.Count; i++)
            {
                String type = dtRiver.Rows[i]["type"].ToString();
                String river_id = dtRiver.Rows[i]["river_id"].ToString();
                String section_id = dtRiver.Rows[i]["id"].ToString();
                String side = dtRiver.Rows[i]["river_side"].ToString();

                String parent_ids = dtRiver.Rows[i]["parent_ids"].ToString();
                String[] ids = parent_ids.Split(',');

                String area_section = null;
                String town_section = null;

                if (ids.Length >= 2)
                {
                    area_section = ids[1];
                }

                if (ids.Length >= 3)
                {
                    town_section = ids[2];
                }

                String addvcd_city = "440100000000";
                String addvcd_area = dtRiver.Rows[i]["addvcd_area"].ToString();
                String addvcd_town = dtRiver.Rows[i]["addvcd_town"].ToString();
                String addvcd_village = dtRiver.Rows[i]["addvcd_village"].ToString();

                //writeInfo((i + 1) + ":" + section_id);
                //this.BeginInvoke(wt, new Object[] { (i + 1) + ":" + section_id });
                writeProgressInfo((i + 1) + "/" + dtRiver.Rows.Count + ":" + section_id, rtbScore);

                //市级河段 每季度不少于1次
                DateTime dt_start = Convert.ToDateTime(start_time);

                while (dt_start <= dt_end)
                {
                    if ((type.Equals("1") || type.Equals("2") || type.Equals("9") || type.Equals("13")) && citySwitch)
                    {
                        if (Convert.ToInt32(dt_start.DayOfWeek) != 1)
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getWeek(dt_start.AddDays(-7));

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('1','27','3','2','39','4','28','29','30','40','41','42') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper10.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            String river_type = dtUserInfo.Rows[j]["river_type"].ToString();

                            //================积分统计=====================
                            score(userid, river_type,addvcd_city, addvcd_area, addvcd_town, addvcd_village, periods + "周", river_id, section_id, type, area_section, town_section, side, null, startTm, endTm, true);
                        }
                    }
                    //区级河段 一般河湖：每月不少于1次；黑臭河湖：每月巡查不少于一次，每次巡查完成不低于本辖区黑臭水体总条数的15%，每半年完成一轮全覆盖巡查；
                    if ((type.Equals("3") || type.Equals("2") || type.Equals("6") || type.Equals("9") || type.Equals("10") || type.Equals("13")) && areaSwitch)
                    {
                        if (Convert.ToInt32(dt_start.DayOfWeek) != 1)
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getWeek(dt_start.AddDays(-7));

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('7','8','9','31','32','33','43','44','45') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper10.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            String river_type = dtUserInfo.Rows[j]["river_type"].ToString();
                            //================积分统计=====================
                            score(userid, river_type, addvcd_city, addvcd_area, addvcd_town, addvcd_village, periods + "周", river_id, section_id, type, area_section, town_section, side, null, startTm, endTm, true);
                        }
                    }
                    //镇级河段 一般河湖：每旬巡查不少于一次；黑臭河湖：每周巡查不少于一次，每次完成不低于本辖区黑臭水体总数的30%，每月完成一轮全覆盖巡查；
                    if ((type.Equals("4") || type.Equals("2") || type.Equals("7") || type.Equals("9") || type.Equals("11") || type.Equals("13")) && townSwitch)
                    {
                        if (Convert.ToInt32(dt_start.DayOfWeek) != 1)
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getWeek(dt_start.AddDays(-7));

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('12','50','13','34','35','36','46','47','48') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper10.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            String river_type = dtUserInfo.Rows[j]["river_type"].ToString();
                            //================积分统计=====================
                            score(userid, river_type, addvcd_city, addvcd_area, addvcd_town, addvcd_village, periods + "周", river_id, section_id, type, area_section, town_section, side, null, startTm, endTm, true);
                        }
                    }
                    //村级河段 一般河湖：每周巡查不少于一次；黑臭河湖：每个工作日一巡，每周完成黑臭水体全覆盖巡查；
                    if ((type.Equals("5") || type.Equals("2") || type.Equals("8") || type.Equals("9") || type.Equals("12") || type.Equals("13")) && villageSwitch)
                    {
                        if (Convert.ToInt32(dt_start.DayOfWeek) != 1)
                        {
                            dt_start = dt_start.AddDays(1);
                            continue;
                        }

                        String startTm = dt_start.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                        String endTm = dt_start.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                        int periods = getWeek(dt_start.AddDays(-7));

                        sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('16','37','49') and del_flag='0' and "
                            + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                        DataTable dtUserInfo = SqlHelper10.GetDataTable(sql, null);

                        for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                        {
                            String userid = dtUserInfo.Rows[j]["id"].ToString();
                            String river_type = dtUserInfo.Rows[j]["river_type"].ToString();
                            //================积分统计=====================
                            score(userid, river_type, addvcd_city, addvcd_area, addvcd_town, addvcd_village, periods + "周", river_id, section_id, type, area_section, town_section, side, null, startTm, endTm, true);
                        }
                    }

                    dt_start = dt_start.AddDays(1);
                }

                //==============================跑固定周期的数据====================================
                if (fixedPeriodWitch)//跑固定周期数据开关是否勾上
                {
                    List<Dictionary<String, Object>> cycleList = getCycleList();
                    for (int index = 0; index < cycleList.Count; index++)
                    {

                        String startTm = cycleList[index]["startTm"].ToString();
                        String endTm = cycleList[index]["endTm"].ToString();
                        String cycle = cycleList[index]["cycle"].ToString();

                        if ((type.Equals("1") || type.Equals("2") || type.Equals("9") || type.Equals("13")) && citySwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('1','27','3','2','39','4','28','29','30','40','41','42') and del_flag='0' and "
                                    + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper10.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                String river_type = dtUserInfo.Rows[j]["river_type"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper10.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================积分统计=====================
                                score(userid, river_type, addvcd_city, addvcd_area, addvcd_town, addvcd_village, cycle, river_id, section_id, type, area_section, town_section, side, base_id, startTm, endTm, true);
                            }
                        }
                        if ((type.Equals("3") || type.Equals("2") || type.Equals("6") || type.Equals("9") || type.Equals("10") || type.Equals("13")) && areaSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('7','8','9','31','32','33','43','44','45') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper10.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                String river_type = dtUserInfo.Rows[j]["river_type"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper10.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================积分统计=====================
                                score(userid, river_type, addvcd_city, addvcd_area, addvcd_town, addvcd_village, cycle, river_id, section_id, type, area_section, town_section, side, base_id, startTm, endTm, true);
                            }
                        }
                        if ((type.Equals("4") || type.Equals("2") || type.Equals("7") || type.Equals("9") || type.Equals("11") || type.Equals("13")) && townSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('12','50','13','34','35','36','46','47','48') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper10.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                String river_type = dtUserInfo.Rows[j]["river_type"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper10.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================积分统计=====================
                                score(userid, river_type, addvcd_city, addvcd_area, addvcd_town, addvcd_village, cycle, river_id, section_id, type, area_section, town_section, side, base_id, startTm, endTm, true);
                            }
                        }
                        if ((type.Equals("5") || type.Equals("2") || type.Equals("8") || type.Equals("9") || type.Equals("12") || type.Equals("13")) && villageSwitch)
                        {
                            sql = "select id,addvcd_area,addvcd_towm,addvcd_village,create_date,update_date,river_type from sys_user_b where river_type in('16','37','49') and del_flag='0' and "
                                + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                            DataTable dtUserInfo = SqlHelper10.GetDataTable(sql, null);

                            for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                            {
                                String userid = dtUserInfo.Rows[j]["id"].ToString();
                                String river_type = dtUserInfo.Rows[j]["river_type"].ToString();
                                //获取base_id
                                sql = "select id from rtd_base where user_id='" + userid + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='4'";

                                DataTable dtBaseInfo = SqlHelper10.GetDataTable(sql, null);
                                if (dtBaseInfo.Rows.Count <= 0)
                                    continue;
                                String base_id = dtBaseInfo.Rows[0][0].ToString();

                                //================积分统计=====================
                                score(userid, river_type, addvcd_city, addvcd_area, addvcd_town, addvcd_village, cycle, river_id, section_id, type, area_section, town_section, side, base_id, startTm, endTm, true);
                            }
                        }
                    }
                }

                //lblInfo.Text = dtRiver.Rows[i][0].ToString();
                bgWork.ReportProgress(i + 1);
            }
        }

        //河长积分
        private void score(String userId, String river_type,String addvcd_city,String addvcd_area,String addvcd_town,String addvcd_village,String cycle, String river_id, String section_id, String section_type,String area_section,String town_section,String side,String base_id, String startTm, String endTm, Boolean ifDelOld)
        {
            //=======================================河长积分===========================================

            String allSql = "";
            //河长总积分分数	
            String sql = "select nvl(sum(CHANGE_SCORE),0) nums,'河长总积分分数' type,1 orders from user_score_r where user_id='" + userId
                + "' and CREATE_TIME>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and CREATE_TIME<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //巡河积分分数	
            sql = "select nvl(sum(CHANGE_SCORE),0) nums,'巡河积分分数' type,2 orders from user_score_r where type in  (select id  from sys_scroe_setting where type='1' ) and user_id='" + userId
                + "' and CREATE_TIME>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and CREATE_TIME<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //发现及处理问题积分分数	
            sql = "select nvl(sum(CHANGE_SCORE),0) nums,'发现及处理问题积分分数' type,3 orders from user_score_r where type in  (select id  from sys_scroe_setting where type='4' ) and user_id='" + userId
                 + "' and CREATE_TIME>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and CREATE_TIME<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //登录签到积分分数	
            sql = "select nvl(sum(CHANGE_SCORE),0) nums,'登录签到积分分数' type,4 orders from user_score_r where type in  (select id  from sys_scroe_setting where type='3' ) and user_id='" + userId
                     + "' and CREATE_TIME>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and CREATE_TIME<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //阅读专栏积分分数	
            sql = "select nvl(sum(CHANGE_SCORE),0) nums,'阅读专栏积分分数' type,5 orders from user_score_r where type in  (select id  from sys_scroe_setting where type='2' ) and user_id='" + userId
                 + "' and CREATE_TIME>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and CREATE_TIME<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //红黑榜积分	
            sql = "select nvl(sum(SCORE),0) nums,'红黑榜积分' type,6 orders from user_score_R_187 where EVALUATION_TYPE='红黑榜'and user_id='" + userId + "'"
                     + " and start_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')and end_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

            allSql += sql + " union ";
            //187条履职评价分数 
            sql = "select nvl(sum(SCORE),0) nums,'187条履职评价分数' type,7 orders from user_score_R_187 where user_id='" + userId + "'"
                     + " and start_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')and end_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and section_id='"+section_id+"'";

            allSql += sql + " order by orders";
            DataTable dtData = SqlHelper10.GetDataTable(allSql, null);

            String totalIntegralNums = dtData.Rows[0][0].ToString();
            String patrolIntegralNums = dtData.Rows[1][0].ToString();
            String findAndSolveIntegralNums = dtData.Rows[2][0].ToString();
            String signInIntegralNums = dtData.Rows[3][0].ToString();
            String readingIntegralNums = dtData.Rows[4][0].ToString();
            String redAndBlackIntegralNums = dtData.Rows[5][0].ToString();
            String performanceEvaluationNums = dtData.Rows[6][0].ToString();


            Boolean have_old = true;
            if (if_delAndInDB)
            {
                if (base_id == null)//固定周期的用原来的base_id,周期的用新的id，type为3
                {
                    //获取base_id
                    sql = "select id from rtd_base where user_id='" + userId + "' and cycle='" + cycle + "' and section_id='" + section_id + "' and type='3'";

                    DataTable dtBaseInfo = SqlHelper10.GetDataTable(sql, null);
                    if (dtBaseInfo.Rows.Count <= 0)
                    {
                        base_id = Guid.NewGuid().ToString().Replace("-", "");

                        sql = "Insert into RTD_BASE (ID,ADDVCD_CITY,ADDVCD_AREA,ADDVCD_TOWN,ADDVCD_VILLAGE,RIVER_ID,SECTION_ID,SECTION_TYPE,BANK,USER_ID,CYCLE,start_time,end_time,area_section,town_section,type)"
                                        + " values ('" + base_id + "','" + addvcd_city + "','" + addvcd_area + "','" + addvcd_town + "','" + addvcd_village + "','" + river_id + "','" + section_id + "','" + section_type + "','" + side + "','"
                                        + userId + "','" + cycle + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')" + ",to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss'),'" + area_section + "','" + town_section + "','" + "3" + "')";

                        SqlHelper10.ExecuteNonQuery(sql, CommandType.Text, null);

                        have_old = false;
                    }
                    else
                    {
                        base_id = dtBaseInfo.Rows[0][0].ToString();
                    }
                }

                if (ifDelOld && have_old)
                {
                    sql = "delete from RTD_SCORE where base_id='" + base_id + "'";

                    SqlHelper10.ExecuteNonQuery(sql, CommandType.Text, null);
                }

                //插入RTD_SCORE 表
                String scoreId = Guid.NewGuid().ToString().Replace("-", "");
                sql = "Insert into RTD_SCORE (ID,BASE_ID,TOTAL_SCORE,PATROL_SCORE,REPORT_AND_DEAL_SCORE,SIGN_SCORE,READ_NEWS_SCORE,RED_BLACK_SCORE,SCORE_187,start_time,end_time,user_id,river_id,section_id,cycle)"
                    + " values ('" + scoreId + "','" + base_id + "','" + totalIntegralNums + "','" + patrolIntegralNums + "','" + findAndSolveIntegralNums + "','" + signInIntegralNums + "','" + readingIntegralNums + "','" + redAndBlackIntegralNums + "','" + performanceEvaluationNums + "',to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')" + ",to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + ",'" + userId + "','" + river_id + "','" + section_id + "','" + cycle + "')";

                SqlHelper10.ExecuteNonQuery(sql, CommandType.Text, null);
            }
        }

        private void workerScore_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (pBarScore.Value < pBarScore.Maximum)
                pBarScore.Value += 1;
        }

        void workerScore_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pBarScore.Value = pBarScore.Maximum;
        }

        private void chkContiuneJob_CheckedChanged(object sender, EventArgs e)
        {
            auto_mode = chkContiuneJob.Checked;
        }

        private void chkCitySwitch_CheckedChanged(object sender, EventArgs e)
        {
            citySwitch = chkCitySwitch.Checked;
        }

        private void chkAreaSwitch_CheckedChanged(object sender, EventArgs e)
        {
            areaSwitch = chkAreaSwitch.Checked;
        }

        private void chkTownSwitch_CheckedChanged(object sender, EventArgs e)
        {
            townSwitch = chkTownSwitch.Checked;
        }

        private void chkVillageSwitch_CheckedChanged(object sender, EventArgs e)
        {
            villageSwitch = chkVillageSwitch.Checked;
        }

        public Boolean if_timerStart = false;
        private Boolean is_running = false;
        private void timer_start_Tick(object sender, EventArgs e)
        {
            dtpStart.Value = DateTime.Now.AddDays(-1);
            dtpEnd.Value = DateTime.Now.AddDays(-1);

            if (!if_timerStart)
            {
                writeInfo("定时任务启动成功...");

                if_timerStart = true;
            }

            //周一三点开始跑数据
            if (DateTime.Now.Hour == 0 && DateTime.Now.Minute == 10 && !is_running)//(!is_running)Monday,Thursday,Friday,Saturday
            {
                is_running = true;
                lblInfo.Text = "任务进行中...";

                btnBase_Click(null, null);
            }
            else if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 0)//周二凌晨1点恢复
            {
                is_running = false;

                lblInfo.Text = "任务等待中...";
            }
        }

        private void btn_startAuto_Click(object sender, EventArgs e)
        {
            if (btn_startAuto.Text == "开启定时跑数据")
            {
                timer_start.Start();
                lblInfo.Text = "定时跑数据开启成功...";
                btn_startAuto.Text = "停止定时跑数据";
            }
            else
            {
                timer_start.Stop();
                lblInfo.Text = "定时跑数据已被关闭！！！";
                btn_startAuto.Text = "定时跑数据已关闭！！！";
            }
        }

        private void rtbBase_TextChanged(object sender, EventArgs e)
        {
            RichTextBox rtb = (RichTextBox)sender;
            rtb.SelectionStart = rtb.TextLength;

            // Scrolls the contents of the control to the current caret position.
            rtb.ScrollToCaret(); //Caret意思：脱字符号；插入符号; (^)
        }

        private void chkFixedPeriod_CheckedChanged(object sender, EventArgs e)
        {
            fixedPeriodWitch = chkFixedPeriod.Checked;
        }
    }
}
