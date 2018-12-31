using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sunny.Misc;
using System.Threading;
using System.Globalization;
using Oracle.ManagedDataAccess.Client;

namespace RiverImport
{
    public partial class frmWeek : Form
    {
        public frmWeek()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;
        }

        public String startTm = "";//"2018-06-18 00:00:00";
        public String endTm = "";//"2018-06-24 23:59:59";
        public bool auto_mode = false;
        public RichTextBox rtxInfo;
        public bool if_delAndInDB=false;

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            startTm = dtpStart.Text;
        }

        private void dtpEnd_ValueChanged(object sender, EventArgs e)
        {
            endTm = dtpEnd.Text;
        }

        public void writeInfo(String msg)
        {
            if (rtxInfo != null)
                rtxInfo.AppendText("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + msg + "\r\n");
        }

        //河长详情
        public void button2_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            worker.DoWork += new DoWorkEventHandler(worker_DoWorkRiverchiefInfo);
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            worker.RunWorkerAsync();
        }

        //河长详情
        void worker_DoWorkRiverchiefInfo(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(河长详情:appweek_rm_info)...";
            writeInfo(lblInfo.Text);

            runRiverchiefInfo(progressBar1, worker);

            lblInfo.Text = "数据处理完成(河长详情:appweek_rm_info)...";
            writeInfo(lblInfo.Text);

            if (auto_mode)
            { 
                //继续跑我的巡河
                button3_Click(null, null);
            }
        }

        public void runRiverchiefInfo(ProgressBar pBar, BackgroundWorker bgWork)
        {
            //String startTm = "2018-06-18 00:00:00";
            //String endTm = "2018-06-24 23:59:59";
            String sql = "";

            if (if_delAndInDB)
            {
                sql = "delete from appweek_rm_info where start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";
                if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                    sql += " and rm_app_id='" + lblChiefID.Text.Trim() + "'";

                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
            }

            sql = "select id,name,(case when to_number(river_type)=4 then '4' when to_number(river_type) in('7','8','9') then '3' when to_number(river_type) in('12','13') then '2' when to_number(river_type)=16 then '1' else '' end) role,"
                + "(select name from sys_area_b where id=addvcd_area) || '-' || (select name from sys_area_b where id=addvcd_towm) || '-' || (select name from sys_area_b where id=addvcd_village) address"
                + " from sys_user_b where river_type in('4','7','8','9','12','13','16') and del_flag='0'";

            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "' order by river_type desc";
            else
                sql += " order by river_type desc";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String id = Guid.NewGuid().ToString();

                String rm_app_id = dt.Rows[i]["id"].ToString();
                String name = dt.Rows[i]["name"].ToString();
                String role = dt.Rows[i]["role"].ToString();
                String address = dt.Rows[i]["address"].ToString();

                //查询责任河段数量
                sql = "select count(distinct RIVER_SECTION_ID) resp_river_section_cnt from rm_riverchief_section_r where user_id='" + rm_app_id + "' and is_old='0'";

                int resp_river_section_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //是否187河长
                //sql = "select count(distinct id) nums from rm_river_lake where id in(select distinct river_section_id from rm_riverchief_section_r where user_id='" + rm_app_id + "' and is_old='0')"
                //    +" and (is_hc35 is not null or is_hc152 is not null)";
                sql = "select count(distinct id) nums from rm_river_lake where id in"
                    + " (select distinct river_section_id from rm_riverchief_section_r where user_id='" + rm_app_id + "' and is_old='0'"
                    + " and rivet_rm_id in (select id from rm_river_lake where is_hc152 is not null or is_hc35 is not null)) ";

                int has_187river = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //总积分
                sql = "select sum(CHANGE_SCORE) total_score from user_score_r where user_id='" + rm_app_id + "'";

                DataTable dtTabel = SqlHelper.GetDataTable(sql, null);
                Double total_score = dtTabel.Rows[0][0] == DBNull.Value ? 0 : Convert.ToDouble(dtTabel.Rows[0][0]);

                //全市村居河长的人数
                sql = "select count(id) city_rm_cnt from sys_user_b where river_type='16'";

                int city_rm_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //本周积分 score_this_week
                sql = "select sum(CHANGE_SCORE) score_this_week from user_score_r where user_id='" + rm_app_id + "' and CREATE_TIME>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + " and CREATE_TIME<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') ";

                dtTabel = SqlHelper.GetDataTable(sql, null);
                Double score_this_week = dtTabel.Rows[0][0] == DBNull.Value ? 0 : Convert.ToDouble(dtTabel.Rows[0][0]);

                //本周红黑榜
                sql = "select type,count(id) nums from INFO_NEWS_REDBLACK_USER where user_id='" + rm_app_id + "' and create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + " and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')  group by type";

                dtTabel = SqlHelper.GetDataTable(sql, null);
                String black_or_red = "0";
                if (dtTabel.Rows.Count > 0)
                {
                    if (dtTabel.Rows.Count == 1)
                    {
                        String type = dtTabel.Rows[0]["type"].ToString();
                        if (type.Equals("0"))//红榜
                            black_or_red = "1";
                        else
                            black_or_red = "2";
                    }
                    else if (dtTabel.Rows.Count == 2)
                        black_or_red = "3";
                }

                //问题完结率
                sql = "select (case when ybj_nums=0 and all_nums=0 then -1 when ybj_nums=0 and all_nums>0 then 0 else round(ybj_nums/all_nums,2) end) PROBLEM_FINISH_PERCENT from"
                    + "("
                    + "    select"
                    + "   ("
                    + "       select count(nums) from "
                    + "           ("
                    + "               select count(problemid) nums,reachid from ptl_reportproblem where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + "               and state='4' group by reachid"
                    + "           ) t where t.reachid in(select RIVER_SECTION_ID from RM_RIVERCHIEF_SECTION_R where is_old='0' and user_id='50001063')"
                    + "   ) ybj_nums,"
                    + "   ("
                    + "       select count(nums) from "
                    + "       ("
                    + "           select count(problemid) nums,reachid from ptl_reportproblem where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + "           group by reachid"
                    + "       ) t where t.reachid in(select RIVER_SECTION_ID from RM_RIVERCHIEF_SECTION_R where is_old='0' and user_id='50001063')"
                    + "   ) all_nums from dual"
                    + ") t";

                dtTabel = SqlHelper.GetDataTable(sql, null);
                Double PROBLEM_FINISH_PERCENT = dtTabel.Rows[0][0] == DBNull.Value ? 0 : Convert.ToDouble(dtTabel.Rows[0][0]);

                String is_read = "0";

                if (if_delAndInDB)
                {
                    sql = "insert into appweek_rm_info(id,rm_app_id,name,role,address,resp_river_section_cnt,start_time,end_time,total_score,city_rm_rank,city_rm_cnt,score_this_week,black_or_red,score_time,PROBLEM_FINISH_PERCENT,is_read,finish_rate_rank,has_187river)"
                        + " values ('" + id + "','" + rm_app_id + "','" + name + "','" + role + "','" + address + "'," + resp_river_section_cnt + ",to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss'),'" + total_score
                        + "',null," + city_rm_cnt + ",'" + score_this_week + "','" + black_or_red + "',sysdate," + PROBLEM_FINISH_PERCENT + ",'" + is_read + "',null,'" + has_187river + "')";

                    SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                }

                bgWork.ReportProgress(i + 1);
            }
        }

        //我的下级村河长履职情况
        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgWorkDownChief.DoWork += new DoWorkEventHandler(worker_DoWorkForLastLevel);
            bgWorkDownChief.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgWorkDownChief.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgWorkDownChief.RunWorkerAsync();
        }

        //我的下级村河长履职情况
        void worker_DoWorkForLastLevel(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(我的下级村河长履职情况:APPWEEK_LOWER_LEVEL_SITUATION)...";
            writeInfo(lblInfo.Text);

            runForLastLevel(progressBar1, bgWorkDownChief);

            lblInfo.Text = "处理完成(我的下级村河长履职情况:APPWEEK_LOWER_LEVEL_SITUATION)";
            writeInfo(lblInfo.Text);

            if (auto_mode)
            {
                //继续跑四个查清
                button5_Click(null, null);
            }
        }

        public void runForLastLevel(ProgressBar pBar, BackgroundWorker bgWork)
        {
            //String startTm = ""+startTm+"";
            //String endTm = ""+endTm+"";

            String sql = "";

            if (if_delAndInDB)
            {
                sql = "delete from APPWEEK_LOWER_LEVEL_SITUATION where rm_info_id in(select id from appweek_rm_info where role='2' and start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";
                if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                    sql += " and rm_app_id='" + lblChiefID.Text.Trim() + "')";
                else
                    sql += ")";

                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
            }

            sql = "select id,addvcd_area from sys_user_b where river_type in('12','13') and del_flag='0'"; // and id='d0e2b4d2c8c041afab22a675a6e41391'  and id='50000137'  and id='50000742'
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();
                String addvcd_area = dt.Rows[i]["addvcd_area"].ToString();

                String id = Guid.NewGuid().ToString();               

                sql = "select id from appweek_rm_info where start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')"
                    + "and rm_app_id='" + userid + "'";

                DataTable dtInfo = SqlHelper.GetDataTable(sql, null);

                if (dtInfo.Rows.Count <= 0)
                    continue;

                String rm_info_id = dtInfo.Rows[0][0].ToString();

                sql = "select count(distinct user_id) nums from "
                    + "("
                    + "     select t.*,(select river_type from sys_user_b where id=user_id) river_type from rm_riverchief_section_r t where del_flags='0' and river_section_id in"
                    + "    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0' and del_flags='0'))"
                    + ") t_chief where river_type='16' and user_id in(select id from sys_user_b where addvcd_area='" + addvcd_area + "' and del_flag='0')";

                //村河长人数统计
                int lower_level_rm_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //计算0巡河人数
                sql = "select count(user_id) from"
                    + "("
                    + "    select t_user.user_id,t_patrol.nums from"
                    + "    ("
                    + "                    select distinct user_id,river_type from rm_riverchief_section_r t_r"
                    + "                    left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                    where del_flags='0' and river_section_id in"
                    + "                    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0' and del_flags='0'))"
                    + "                and (select river_type from sys_user_b where id=t_user.id)='16'"
                    + "    ) t_user"
                    + "    left join"
                    + "    ("
                    + "        select userid,count(patrolid) nums from ptl_patrol_r where strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + "        and userid in("
                    + "                    select distinct user_id from rm_riverchief_section_r t_r"
                    + "                    left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                    where del_flags='0' and river_section_id in"
                    + "                    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0' and del_flags='0'))"
                    + "                and (select river_type from sys_user_b where id=t_user.id)='16'"
                    + "        ) group by userid"
                    + "    ) t_patrol"
                    + "    on t_user.user_id=t_patrol.userid"
                    + ") where nums=0 or nums is null and user_id in(select id from sys_user_b where addvcd_area='" + addvcd_area + "')";

                int ZERO_PATROLLING_CNT = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //计算0上报人数
                sql = "select count(user_id) from"
                    + "("
                    + "    select t_user.user_id,t_patrol.nums from"
                    + "    ("
                    + "                    select distinct user_id,river_type from rm_riverchief_section_r t_r"
                    + "                    left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                    where del_flags='0' and river_section_id in"
                    + "                    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0' and del_flags='0'))"
                    + "                and (select river_type from sys_user_b where id=t_user.id)='16'"
                    + "    ) t_user"
                    + "    left join"
                    + "    ("
                    + "        select userid,count(problemid) nums from ptl_reportproblem where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + "        and userid in("
                    + "                    select distinct user_id from rm_riverchief_section_r t_r"
                    + "                    left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                    where del_flags='0' and river_section_id in"
                    + "                    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0' and del_flags='0'))"
                    + "                    and (select river_type from sys_user_b where id=t_user.id)='16'"
                    + "        ) group by userid"
                    + "    ) t_patrol"
                    + "    on t_user.user_id=t_patrol.userid"
                    + ") where nums=0 or nums is null and user_id in(select id from sys_user_b where addvcd_area='"+addvcd_area+"')";

                int ZERO_SUBMIT_PROBLEM_CNT = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //计算下级村河长巡河达标率
                sql = "select round(sum(nums)/(count(userid)),2) rate from "
                    + "("
                    + "    select userid,(case when count(userid)>=7 then 1 else 0 end) nums  from"
                    + "    ("
                    + "        select userid,count(patrolid) nums from ptl_patrol_r where strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + "        and userid in("
                    + "                        select distinct user_id from rm_riverchief_section_r t_r"
                    + "                        left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                        where del_flags='0' and river_section_id in"
                    + "                        (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0' and del_flags='0'))"
                    + "                    and (select river_type from sys_user_b where id=t_user.id)='16'"
                    + "        ) and duration>=10 group by userid,to_char(strtime,'yyyy-MM-dd')"
                    + "    )t where userid in(select id from sys_user_b where addvcd_area='"+addvcd_area+"') group by userid"
                    + ")";

                DataTable dtTabel = SqlHelper.GetDataTable(sql, null);

                Double LOWER_LEVEL_RM_WORK_STAND_RATE = dtTabel.Rows[0][0] == DBNull.Value ? 0 : Convert.ToDouble(dtTabel.Rows[0][0]);

                //下级村河长巡河达标人数 lower_level_rm_std_cnt
                sql = "select distinct user_id from rm_riverchief_section_r t_r"
                    + "                    left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                    where del_flags='0' and river_section_id in"
                    + "                    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0' and del_flags='0'))"
                    + "                and (select river_type from sys_user_b where id=t_user.id)='16' and user_id in(select id from sys_user_b where addvcd_area='" + addvcd_area + "')";

                DataTable dtTabeUser = SqlHelper.GetDataTable(sql, null);

                int db_person = 0;
                lblInfo.Text = userid;
                for (int j = 0; j < dtTabeUser.Rows.Count; j++)
                {
                    String low_user = dtTabeUser.Rows[j][0].ToString();

                    sql = "select count(userid) nums from "
                        + "("
                        + "    select userid,to_char(strtime,'yyyy-MM-dd') tm,count(patrolid) nums from ptl_patrol_r where"
                        + "    strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                        + "    and duration>=10 and userid='" + low_user + "'"
                        + "    group by userid,to_char(strtime,'yyyy-MM-dd')"
                        + ")";

                    DataTable dtPerson = SqlHelper.GetDataTable(sql, null);

                    int if_dbts = Convert.ToInt32(dtPerson.Rows[0][0].ToString());//达标天数

                    if (if_dbts >= 7)
                        db_person++;
                }

                int lower_level_rm_std_cnt = db_person;
                //计算下级村河长问题上报数量
                sql = "select count(problemid) nums from ptl_reportproblem "
                    + "where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + "and userid in("
                    + "                    select distinct user_id from rm_riverchief_section_r t_r"
                    + "                    left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                    where del_flags='0' and river_section_id in"
                    + "                    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0' and del_flags='0'))"
                    + "                and (select river_type from sys_user_b where id=t_user.id)='16' and user_id in(select id from sys_user_b where addvcd_area='" + addvcd_area + "')"
                    + ")";

                int LOWER_LEVEL_RM_PROBLEM_SUB_CNT = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //本周下级村河长进入红榜人数
                sql = "select count(id) nums from INFO_NEWS_REDBLACK_USER "
                    + "where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + " and type='0' and user_id in("
                    + "                    select distinct user_id from rm_riverchief_section_r t_r"
                    + "                    left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                    where del_flags='0' and river_section_id in"
                    + "                    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0' and del_flags='0'))"
                    + "                and (select river_type from sys_user_b where id=t_user.id)='16' and user_id in(select id from sys_user_b where addvcd_area='" + addvcd_area + "')"
                    + ")";

                int lower_level_rm_black_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //本周下级村河长进入黑榜人数
                sql = "select count(id) nums from INFO_NEWS_REDBLACK_USER "
                    + "where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + " and type='1' and user_id in("
                    + "                    select distinct user_id from rm_riverchief_section_r t_r"
                    + "                    left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                    where del_flags='0' and river_section_id in"
                    + "                    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0') and del_flags='0')"
                    + "                and (select river_type from sys_user_b where id=t_user.id)='16' and user_id in(select id from sys_user_b where addvcd_area='" + addvcd_area + "')"
                    + ")";
                int lower_level_rm_red_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                if (if_delAndInDB)
                {
                    //插入数据至APPWEEK_LOWER_LEVEL_SITUATION表
                    sql = "insert into APPWEEK_LOWER_LEVEL_SITUATION(id,LOWER_LEVEL_RM_BLACK_CNT,LOWER_LEVEL_RM_CNT,LOWER_LEVEL_RM_PROBLEM_SUB_CNT,LOWER_LEVEL_RM_WORK_STAND_RATE"
                        + ",LOWER_LEVEL_RM_RED_CNT,RM_INFO_ID,LOWER_LEVEL_187RM_AVG_SCORE,ZERO_PATROLLING_CNT,ZERO_SUBMIT_PROBLEM_CNT,NO_ACHIEVE_CNT,LOWER_LEVEL_RM_STD_CNT)"
                        + " values('" + id + "','" + lower_level_rm_black_cnt + "','" + lower_level_rm_cnt + "','" + LOWER_LEVEL_RM_PROBLEM_SUB_CNT + "','" + LOWER_LEVEL_RM_WORK_STAND_RATE
                        + "','" + lower_level_rm_red_cnt + "','" + rm_info_id + "',null,'" + ZERO_PATROLLING_CNT + "','" + ZERO_SUBMIT_PROBLEM_CNT + "',null," + lower_level_rm_std_cnt + ")";

                    SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                }

                bgWork.ReportProgress(i + 1);
            }
        }

        //我的巡河
        public void button3_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgWorkPatrol.DoWork += new DoWorkEventHandler(worker_DoWorkXunHe);
            bgWorkPatrol.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgWorkPatrol.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgWorkPatrol.RunWorkerAsync();
        }

        //我的巡河
        void worker_DoWorkXunHe(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(我的巡河:appweek_my_river_patrolling)...";
            writeInfo(lblInfo.Text);

            runMyXunHe(progressBar1, bgWorkPatrol);

            lblInfo.Text = "数据处理完成(我的巡河:appweek_my_river_patrolling)";
            writeInfo(lblInfo.Text);

            if (auto_mode)
            {
                //继续跑我的APP使用情况
                button4_Click(null, null);
            }
        }

        public void runMyXunHe(ProgressBar pBar, BackgroundWorker bgWork)
        {
            //String startTm = "2018-06-18 00:00:00";
            //String endTm = "2018-06-24 23:59:59";

            String sql = "";
            if (if_delAndInDB)
            {
                sql = "delete from appweek_my_river_patrolling where rm_info_id in(select id from appweek_rm_info where start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";
                if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                    sql += " and rm_app_id='" + lblChiefID.Text.Trim() + "')";
                else
                    sql += ")";

                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
            }

            sql = "select id,river_type from sys_user_b where river_type in('7','8','9','12','13','16') and del_flag='0'";//
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();
                String river_type = dt.Rows[i]["river_type"].ToString();

                String id = Guid.NewGuid().ToString();

                sql = "select id from appweek_rm_info where start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')"
                    + "and rm_app_id='" + userid + "'";

                DataTable dtInfo = SqlHelper.GetDataTable(sql, null);

                if (dtInfo.Rows.Count <= 0)
                {
                    bgWorkPatrol.ReportProgress(i + 1);
                    continue;
                }

                String rm_info_id = dtInfo.Rows[0][0].ToString();

                Dictionary<string, string> patrol_date = getAreaChiefPatrolRegion(Convert.ToInt32(startTm.Substring(5, 2)),Convert.ToInt32(startTm.Substring(0, 4)));
                String patrol_start = patrol_date["start_tm"];
                String patrol_end = patrol_date["end_tm"];
                //valid_patrolling	有效巡河次数
                if(river_type.Equals("7") || river_type.Equals("8") || river_type.Equals("9"))
                    sql = "select nvl(count(patrolid),0) nums from ptl_patrol_r where userid='" + userid + "' and strtime>=to_date('" + patrol_start + "','yyyy-MM-dd HH24:mi:ss') and strtime<to_date('" + patrol_end + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and duration>=10";
                else
                    sql = "select nvl(count(patrolid),0) nums from ptl_patrol_r where userid='" + userid + "' and strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and duration>=10";

                int valid_patrolling = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);
                //diversify_patrolling	多样化巡河次数
                if (river_type.Equals("7") || river_type.Equals("8") || river_type.Equals("9"))
                    sql = "select nvl(count(id),0) nums from ptl_patrol_other where user_id='" + userid + "' and tm>=to_date('" + patrol_start + "','yyyy-MM-dd HH24:mi:ss') and tm<to_date('" + patrol_end + "','yyyy-MM-dd HH24:mi:ss')";
                else
                    sql = "select nvl(count(id),0) nums from ptl_patrol_other where user_id='" + userid + "' and tm>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and tm<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

                int diversify_patrolling = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //本期常规巡河次数（区） normal_patrolling
                int normal_patrolling = valid_patrolling;
                valid_patrolling += diversify_patrolling;
                //patrolling_coordinate_strs	巡河轨迹
                if (river_type.Equals("7") || river_type.Equals("8") || river_type.Equals("9"))
                    sql = "select USERID USER_ID,"
                        + " XMLAGG(XMLELEMENT(E, JSON_PATH || ',')).EXTRACT('//text()').getclobval() AS patrolling_coordinate_strs"
                        + " from ptl_patroltrack_r where TIME>=to_date('" + patrol_start + "','yyyy-MM-dd HH24:mi:ss') and TIME<to_date('" + patrol_end + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and userid='" + userid + "' and patrolid in(select patrolid from ptl_patrol_r where duration>=10) group by userid";
                else
                    sql = "select USERID USER_ID,"
                        + " XMLAGG(XMLELEMENT(E, JSON_PATH || ',')).EXTRACT('//text()').getclobval() AS patrolling_coordinate_strs"
                        + " from ptl_patroltrack_r where TIME>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and TIME<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and userid='" + userid + "' and patrolid in(select patrolid from ptl_patrol_r where duration>=10) group by userid";

                DataTable dtPath = SqlHelper.GetDataTable(sql, null);

                String patrolling_coordinate_strs = null;
                if (dtPath.Rows.Count > 0)
                    patrolling_coordinate_strs = dtPath.Rows[0][1].ToString();
                //巡河轨迹对应日期
                /*sql = "select USERID USER_ID,"
                    + " XMLAGG(XMLELEMENT(E, to_char(time,'yyyy-MM-dd') || ',')).EXTRACT('//text()').getclobval() AS patrolling_coordinate_strs"
                    + " from ptl_patroltrack_r where TIME>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and TIME<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + " and userid='" + userid + "' group by userid";*/
                if (river_type.Equals("7") || river_type.Equals("8") || river_type.Equals("9"))
                    sql = "select wm_concat(to_char(strtime,'yyyy-MM-dd')) AS patrolling_coordinate_strs from "
                        +"("
                        +"    select userid,strtime,endtime from ptl_patrol_r "
                        +"    where strtime>=to_date('"+patrol_start+"','yyyy-MM-dd HH24:mi:ss') and strtime<to_date('"+patrol_end+"','yyyy-MM-dd HH24:mi:ss')"
                        + "    and userid='" + userid + "' and duration>=10 order by strtime asc"
                        +")";
                else
                    sql = "select wm_concat(to_char(strtime,'yyyy-MM-dd')) AS patrolling_coordinate_strs from "
                        + "("
                        + "    select userid,strtime,endtime from ptl_patrol_r "
                        + "    where strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                        + "    and userid='" + userid + "' and duration>=10 order by strtime asc"
                        + ")";

                DataTable dtPathDate = SqlHelper.GetDataTable(sql, null);

                String patrolling_trail_date = null;
                if (dtPathDate.Rows.Count > 0)
                    patrolling_trail_date = dtPathDate.Rows[0][0] == DBNull.Value ? null : dtPathDate.Rows[0][0].ToString();

                //巡河对应开始时间和结束时间patrolling_startTime_endTime
                if (river_type.Equals("7") || river_type.Equals("8") || river_type.Equals("9"))
                    sql = "select wm_concat(to_char(strtime,'HH24:mi') || '-' || to_char(endtime,'HH24:mi')) AS patrolling_startTime_endTime from "
                        +"("
                        +"    select userid,strtime,endtime from ptl_patrol_r "
                        + "    where strtime>=to_date('" + patrol_start + "','yyyy-MM-dd HH24:mi:ss') and strtime<to_date('" + patrol_end + "','yyyy-MM-dd HH24:mi:ss')"
                        + "    and userid='" + userid + "' and duration>=10 order by strtime asc"
                        +")";
                else
                    sql = "select wm_concat(to_char(strtime,'HH24:mi') || '-' || to_char(endtime,'HH24:mi')) AS patrolling_startTime_endTime from "
                        + "("
                        + "    select userid,strtime,endtime from ptl_patrol_r "
                        + "    where strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                        + "    and userid='" + userid + "' and duration>=10 order by strtime asc"
                        + ")";

                DataTable dtPatrolTime = SqlHelper.GetDataTable(sql, null);

                String patrolling_startTime_endTime = null;
                if (dtPatrolTime.Rows.Count > 0)
                    patrolling_startTime_endTime = dtPatrolTime.Rows[0][0] == DBNull.Value ? null : dtPatrolTime.Rows[0][0].ToString(); ;
                //重大问题上报数
                int import_problem_submit = 0;
                if (river_type.Equals("16"))
                {
                    sql = "select nvl(count(id),0) nums from ptl_problem_paper_assign where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                                + "and problem_id in(select problemid from ptl_reportproblem where userid='" + userid + "'and sectionid in(select river_section_id from rm_riverchief_section_r where user_id='"+userid+"' and is_old='0'))";
                }
                else if (river_type.Equals("12") || river_type.Equals("13"))
                {
                    sql = "select nvl(count(id),0) nums from ptl_problem_paper_assign where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                                + "and problem_id in(select problemid from ptl_reportproblem where userid='" + userid + "')";
                }
                else if (river_type.Equals("7") || river_type.Equals("8") || river_type.Equals("9"))
                {
                    sql = "select nvl(count(id),0) nums from ptl_problem_paper_assign where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                                + "and problem_id in(select problemid from ptl_reportproblem where userid='" + userid + "')";
                }

                import_problem_submit = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);
                //this_week_times	本周有效巡河分钟数
                if (river_type.Equals("7") || river_type.Equals("8") || river_type.Equals("9"))
                    sql = "select round(nvl(sum(duration),0),2) nums from ptl_patrol_r where userid='" + userid + "' and strtime>=to_date('" + patrol_start + "','yyyy-MM-dd HH24:mi:ss') and strtime<to_date('" + patrol_end + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and duration>=10 and duration<=240 and distance<=20";
                else
                    sql = "select round(nvl(sum(duration),0),2) nums from ptl_patrol_r where userid='" + userid + "' and strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and duration>=10 and duration<=240 and distance<=20";

                DataTable dtDuration = SqlHelper.GetDataTable(sql, null);

                Double this_week_times = 0;
                if (dtDuration.Rows.Count > 0)
                    this_week_times = Convert.ToDouble(dtDuration.Rows[0][0]);
                //this_week_kilometers	本周有效巡河公里数
                if (river_type.Equals("7") || river_type.Equals("8") || river_type.Equals("9"))
                    sql = "select round(nvl(sum(distance),0),2) nums from ptl_patrol_r where userid='" + userid + "' and strtime>=to_date('" + patrol_start + "','yyyy-MM-dd HH24:mi:ss') and strtime<to_date('" + patrol_end + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and duration>=10 and duration<=240 and distance<=20";
                else
                    sql = "select round(nvl(sum(distance),0),2) nums from ptl_patrol_r where userid='" + userid + "' and strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and duration>=10 and duration<=240 and distance<=20";

                DataTable dtdistance = SqlHelper.GetDataTable(sql, null);

                Double this_week_kilometers = 0;
                if (dtdistance.Rows.Count > 0)
                    this_week_kilometers = Convert.ToDouble(dtdistance.Rows[0][0]);
                //total_times	本年度累计巡河分钟数
                sql = "select round(nvl(sum(duration),0),2) nums from ptl_patrol_r where userid='" + userid + "' and strtime>=to_date('" + startTm.Substring(0, 4) + "-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + startTm.Substring(0, 4) + "-12-31 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + " and duration>=10 and duration<=240 and distance<=20";

                DataTable dtTotalTimes = SqlHelper.GetDataTable(sql, null);

                Double total_times = 0;
                if (dtTotalTimes.Rows.Count > 0)
                    total_times = Convert.ToDouble(dtTotalTimes.Rows[0][0]);

                //total_kilometers	本年度累计巡河公里数
                sql = "select round(nvl(sum(distance),0),2) nums from ptl_patrol_r where userid='" + userid + "' and strtime>=to_date('" + startTm.Substring(0, 4) + "-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + startTm.Substring(0, 4) + "-12-31 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + " and duration>=10 and duration<=240 and distance<=20";

                DataTable dtTotalDistance = SqlHelper.GetDataTable(sql, null);

                Double total_kilometers = 0;
                if (dtTotalDistance.Rows.Count > 0)
                    total_kilometers = Convert.ToDouble(dtTotalDistance.Rows[0][0]);
                //thisWeek_problem_cnt	本周上报问题数量
                sql = "";
                if (river_type.Equals("16"))
                {
                    sql = "select count(problemid) nums from ptl_reportproblem where userid='" + userid + "' and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and sectionid in(select river_section_id from rm_riverchief_section_r where user_id='"+userid+"' and is_old='0') and state<>-1";
                }
                else if (river_type.Equals("12") || river_type.Equals("13"))
                {
                    sql = "select count(problemid) nums from ptl_reportproblem where userid='" + userid + "' and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and state<>-1";
                }
                else if (river_type.Equals("7") || river_type.Equals("8") || river_type.Equals("9"))
                {
                    sql = "select count(problemid) nums from ptl_reportproblem where userid='" + userid + "' and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and state<>-1";
                }

                int thisWeek_problem_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);
                //total_problem_cnt	累计上报问题数量
                sql = "";
                if (river_type.Equals("16"))
                {
                    sql = "select count(problemid) nums from ptl_reportproblem where userid='" + userid + "' and sectionid in(select river_section_id from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0') and state<>-1 and time<=to_date('"+endTm+"','yyyy-MM-dd HH24:mi:ss')";
                }
                else if (river_type.Equals("12") || river_type.Equals("13"))
                {
                    sql = "select count(problemid) nums from ptl_reportproblem where userid='" + userid + "' and state<>-1 and time<=to_date('"+endTm+"','yyyy-MM-dd HH24:mi:ss')";
                }
                else if (river_type.Equals("7") || river_type.Equals("8") || river_type.Equals("9"))
                {
                    sql = "select count(problemid) nums from ptl_reportproblem where userid='" + userid + "' and state<>-1 and time<=to_date('"+endTm+"','yyyy-MM-dd HH24:mi:ss')";
                }

                int total_problem_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //thisWeek_finish_problem_cnt	本周办结问题数量
                sql = "select count(problemid) nums from ptl_reportproblem where userid='" + userid + "' and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and state='4'";

                int thisWeek_finish_problem_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);
                //total_finish_problem_cnt	累计办结问题数量
                sql = "select count(problemid) nums from ptl_reportproblem where userid='" + userid + "' and state='4' and time<=to_date('"+endTm+"','yyyy-MM-dd HH24:mi:ss')";

                int total_finish_problem_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);
                //thisWeek_assign_problem_cnt	本周被列为交办问题数量
                sql = "select count(*) thisWeek_assign_problem_cnt from"
                    + " PTL_REPORTPROBLEM"
                    + " where   TIME>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and TIME<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + " AND problemid IN ( SELECT problem_id FROM ptl_problem_paper_assign ) and userid='" + userid + "'";

                int thisWeek_assign_problem_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);
                //thisWeek_citizen_complaint_cnt	本周市民投诉问题数量
                sql = "select count(problemid) nums from ptl_reportproblem where pro_resource='2'  and IS_CITIZEN='Y' and "
                    + " TIME>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and TIME<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + " and reachid in"
                    + "("
                    + "    select distinct RIVER_SECTION_ID resp_river_section from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0'"
                    + ")";

                int thisWeek_citizen_complaint_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);
                //valid_patrolling_strs	有效巡河日期
                sql = "select USERID USER_ID,SF_SPLIT_ACCOUNT_ID_LIST("
                    + " XMLAGG(XMLELEMENT(E, to_char(to_number(to_char(STRTIME,'D'))) ||"
                    + "',')).EXTRACT('//text()').getclobval()"
                    + ") valid_patrolling_strs from PTL_PATROL_R where  strTime IS NOT NULL AND duration >= 10"
                    + " and STRTIME>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and STRTIME<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and userid='" + userid + "'"
                    + " group by USERID";

                DataTable dtPatrolday = SqlHelper.GetDataTable(sql, null);

                String valid_patrolling_strs = null;
                if (dtPatrolday.Rows.Count > 0)
                    valid_patrolling_strs = dtPatrolday.Rows[0][1].ToString();
                //city_submit_problem_cmt	市巡查单位上报问题数量
                sql = "select count(problemid) nums from "
                    + "("
                    + "    select problemid from ptl_reportproblem where "
                    + "    userid in(select id from sys_user_b where river_type='21') and  "
                    + "    TIME>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and TIME<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + "    and sectionid in"
                    + "    ("
                    + "        select distinct RIVER_SECTION_ID resp_river_section from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0'"
                    + "    )"
                    + "    union"
                    + "    select problemid from ptl_reportproblem where pro_resource='2' and is_citizen='N' and"
                    + "    TIME>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and TIME<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + "    and reachid in"
                    + "    ("
                    + "        select distinct RIVER_SECTION_ID resp_river_section from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0'"
                    + "    )"
                    + ")";

                int city_submit_problem_cmt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //本周发现问题数（区）thisWeek_problem_findCnt
                int thisWeek_problem_findCnt = 0;

                sql = "select count(problemid) from ptl_reportproblem where sectionid in("
                    + "select distinct river_section_id from rm_riverchief_section_r where area_section_id in(select river_section_id from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0')"
                    +") and TIME>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and TIME<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    +" and state<>'-1'";

                thisWeek_problem_findCnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);
                //累计发现问题数（区）total_problem_findCnt
                int total_problem_findCnt = 0;

                sql = "select count(problemid) from ptl_reportproblem where sectionid in("
                    + "select distinct river_section_id from rm_riverchief_section_r where area_section_id in(select river_section_id from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0')"
                    + ") and state<>'-1' and time<=to_date('"+endTm+"','yyyy-MM-dd HH24:mi:ss')";

                total_problem_findCnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //本周重大问题数（区）thisWeek_important_cnt
                int thisWeek_important_cnt = 0;
                sql = "select nvl(count(id),0) nums from ptl_problem_paper_assign where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                                + "and problem_id in(select problemid from ptl_reportproblem where sectionid in(select distinct river_section_id from rm_riverchief_section_r where area_section_id in(select river_section_id from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0')))";

                thisWeek_important_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //累计重大问题数（区）thisWeek_important_cnt
                int total_important_cnt = 0;
                sql = "select nvl(count(id),0) nums from ptl_problem_paper_assign where "
                                + "problem_id in(select problemid from ptl_reportproblem where sectionid in(select distinct river_section_id from rm_riverchief_section_r where area_section_id in(select river_section_id from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0')))"
                                + " and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

                total_important_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //累计问题办结率（区）total_problem_finish_rate
                int total_problem_finish = 0;

                sql = "select count(problemid) from ptl_reportproblem where sectionid in("
                    + "select distinct river_section_id from rm_riverchief_section_r where area_section_id in(select river_section_id from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0')"
                    + ") and state='4' and time<=to_date('"+endTm+"','yyyy-MM-dd HH24:mi:ss')";

                total_problem_finish = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                double total_problem_finish_rate = 0.0;
                if(total_problem_finish!=0)
                    total_problem_finish_rate = Math.Round(Convert.ToDouble(total_problem_finish) / Convert.ToDouble(total_problem_findCnt), 2);

                //巡河周期开始时间和截至时间（区） period_startTime_endTime
                String period_startTime_endTime = startTm.Substring(0, 10) + "至" + DateTime.Now.ToString("yyyy-MM-dd");
                period_startTime_endTime = period_startTime_endTime.Replace('/', '.').Replace('-', '.').Replace('至','-');

                //有效巡河天数 VALID_PATROLLING_DAY
                if (river_type.Equals("7") || river_type.Equals("8") || river_type.Equals("9"))
                {
                    sql = "select count(day) nums from "
                    + "("
                    + "    select to_char(strtime,'yyyy-MM-dd') day,count(patrolid) patrol_nums from ptl_patrol_r where duration>=10 and userid='" + userid + "'"
                    + "    and strtime>=to_date('" + patrol_start + "','yyyy-MM-dd HH24:mi:ss') and strtime<to_date('" + patrol_end + "','yyyy-MM-dd HH24:mi:ss')"
                    + "    group by to_char(strtime,'yyyy-MM-dd') "
                    + ") where patrol_nums>0";
                }
                else
                {
                    sql = "select count(day) nums from "
                    + "("
                    + "    select to_char(strtime,'yyyy-MM-dd') day,count(patrolid) patrol_nums from ptl_patrol_r where duration>=10 and userid='" + userid + "'"
                    + "    and strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + "    group by to_char(strtime,'yyyy-MM-dd') "
                    + ") where patrol_nums>0";
                }
                

                int VALID_PATROLLING_DAY = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //paper_assign_problem_cnt	累计纸质交办问题数(区)
                sql = "select count(problemid) nums from ptl_reportproblem where sectionid in("
                    +"select distinct river_section_id from rm_riverchief_section_r where area_section_id in(select river_section_id from rm_riverchief_section_r where user_id='"+userid+"' and is_old='0')"
                    +") and state<>'-1' and problemid in(select problem_id from ptl_problem_paper_assign) and time<=to_date('"+endTm+"','yyyy-MM-dd HH24:mi:ss')";

                int paper_assign_problem_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);
                //total_paper_assign_fin_problem_cnt	累计纸质交办问题办结数量（区）
                sql = "select count(problemid) nums from ptl_reportproblem where sectionid in("
                    + "select distinct river_section_id from rm_riverchief_section_r where area_section_id in(select river_section_id from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0')"
                    + ") and state='4' and problemid in(select problem_id from ptl_problem_paper_assign) and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

                int total_paper_assign_fin_problem_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);
                //total_paper_assign_fin_problem_rate	累计纸质交办问题累计办结率（区）
                Double total_paper_assign_fin_problem_rate=0;
                if (total_paper_assign_fin_problem_cnt == 0)
                    total_paper_assign_fin_problem_rate=0;
                else
                    total_paper_assign_fin_problem_rate = Math.Round(Convert.ToDouble(total_paper_assign_fin_problem_cnt) / Convert.ToDouble(paper_assign_problem_cnt), 2);
                //app_paper_assign_problem_cnt	APP累计交办问题数（区）
                sql = "select count(problemid) nums from ptl_reportproblem where sectionid in("
                    + "select distinct river_section_id from rm_riverchief_section_r where area_section_id in(select river_section_id from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0')"
                    + ") and state<>'-1' and problemid in(select problem_id from PTL_PROBLEM_ASSIGNED) and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

                int app_paper_assign_problem_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);
                //app_paper_assign_fin_problem_cnt	APP累计交办问题办结数量（区）
                sql = "select count(problemid) nums from ptl_reportproblem where sectionid in("
                    + "select distinct river_section_id from rm_riverchief_section_r where area_section_id in(select river_section_id from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0')"
                    + ") and state='4' and problemid in(select problem_id from PTL_PROBLEM_ASSIGNED) and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

                int app_paper_assign_fin_problem_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);
                //app_paper_assign_fin_problem_rate	APP累计交办问题累计办结率（区）
                Double app_paper_assign_fin_problem_rate = 0;
                if (app_paper_assign_fin_problem_cnt == 0)
                    app_paper_assign_fin_problem_rate = 0;
                else
                    app_paper_assign_fin_problem_rate = Math.Round(Convert.ToDouble(app_paper_assign_fin_problem_cnt) / Convert.ToDouble(app_paper_assign_problem_cnt), 2);

                //合并巡河轨迹
                if (river_type.Equals("7") || river_type.Equals("8") || river_type.Equals("9"))
                {
                    sql = "select strtime,endtime,duration,round(distance,2) distance,t_track.json_path from ptl_patrol_r t_r"
                        +" left join ptl_patroltrack_r t_track on t_track.patrolid=t_r.patrolid"
                        +" where strtime>=to_date('"+patrol_start+"','yyyy-MM-dd HH24:mi:ss')" 
                        +" and strtime<to_date('"+patrol_end+"','yyyy-MM-dd HH24:mi:ss') and t_r.userid='"+userid+"' and duration>=10"
                        +" order by t_r.strtime asc";
                }
                else
                {
                    sql = "select strtime,endtime,duration,round(distance,2) distance,t_track.json_path from ptl_patrol_r t_r"
                        + " left join ptl_patroltrack_r t_track on t_track.patrolid=t_r.patrolid"
                        + " where strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and strtime<to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and t_r.userid='" + userid + "' and duration>=10"
                        + " order by t_r.strtime asc";
                }

                DataTable dtPatrolInfo = SqlHelper.GetDataTable(sql, null);
                String patrolling_coordinate_strs2 = "";
                for (int k = 0; k < dtPatrolInfo.Rows.Count; k++)
                {
                    DateTime startPatrol = Convert.ToDateTime(dtPatrolInfo.Rows[k]["strtime"].ToString());
                    DateTime endPatrol = Convert.ToDateTime(dtPatrolInfo.Rows[k]["endtime"].ToString());
                    String duration = dtPatrolInfo.Rows[k]["duration"].ToString();
                    String distance = dtPatrolInfo.Rows[k]["distance"].ToString();
                    String path = dtPatrolInfo.Rows[k]["json_path"].ToString();

                    patrolling_coordinate_strs2 += path + ";";

                    patrolling_coordinate_strs2 += startPatrol.ToString("MM月dd日") + ";";
                    patrolling_coordinate_strs2 += startPatrol.ToString("HH:mm") + "-" + endPatrol.ToString("HH:mm") + ";";
                    patrolling_coordinate_strs2 += duration + "分钟/";
                    patrolling_coordinate_strs2 += distance + "公里" + ",";
                }
                patrolling_coordinate_strs2 = patrolling_coordinate_strs2.Trim(',');

                if (if_delAndInDB)
                {
                    sql = "INSERT INTO appweek_my_river_patrolling ("
                   + "		id,"
                   + "		rm_info_id,"
                   + "		valid_patrolling, "
                   + "		diversify_patrolling,"
                   + "		patrolling_coordinate_strs,patrolling_coordinate_strs2,"
                   + "		this_week_times,"
                   + "		this_week_kilometers,"
                   + "		total_times, "
                   + "		total_kilometers,  "
                   + "		thisweek_problem_cnt,  "
                   + "		total_problem_cnt, "
                   + "		thisweek_finish_problem_cnt,"
                   + "		total_finish_problem_cnt, "
                   + "		thisweek_assign_problem_cnt,"
                   + "		thisweek_citizen_complaint_cnt,"
                   + "		valid_patrolling_strs,"
                   + "      city_submit_problem_cmt,patrolling_trail_date,patrolling_startTime_endTime,import_problem_submit,"
                   + "      thisWeek_problem_findCnt,total_problem_findCnt,total_problem_finish_rate,thisWeek_important_cnt,total_important_cnt,period_startTime_endTime,"
                   + "       normal_patrolling,VALID_PATROLLING_DAY,PAPER_ASSIGN_PRO_CNT,TOT_PAPER_ASSIGN_PRO_CNT,TOT_PAPER_ASSIGN_PRO_RATE,"
                   + "        APP_PAPER_ASSIGN_PRO_CNT,APP_TOT_PAPER_ASSIGN_PRO_CNT,APP_TOT_PAPER_ASSIGN_PRO_RATE"
                   + "		) values("
                   + "'" + id + "','" + rm_info_id + "','" + valid_patrolling + "','" + diversify_patrolling + "','" + patrolling_coordinate_strs + "',:patrolling_coordinate_strs2"
                   + ",'" + this_week_times + "','" + this_week_kilometers + "','" + total_times + "','" + total_kilometers + "','" + thisWeek_problem_cnt
                   + "','" + total_problem_cnt + "','" + thisWeek_finish_problem_cnt + "','" + total_finish_problem_cnt + "','" + thisWeek_assign_problem_cnt
                   + "','" + thisWeek_citizen_complaint_cnt + "','" + valid_patrolling_strs + "','" + city_submit_problem_cmt + "','" + patrolling_trail_date + "','" + patrolling_startTime_endTime + "','" + import_problem_submit + "'"
                   + ",'" + thisWeek_problem_findCnt + "','" + total_problem_findCnt + "','" + total_problem_finish_rate + "','" + thisWeek_important_cnt + "','" + total_important_cnt
                   + "','" + period_startTime_endTime + "','" + normal_patrolling + "','" + VALID_PATROLLING_DAY + "','" + paper_assign_problem_cnt + "','" + total_paper_assign_fin_problem_cnt + "','" + total_paper_assign_fin_problem_rate
                   + "','" + app_paper_assign_problem_cnt + "','" + app_paper_assign_fin_problem_cnt + "','" + app_paper_assign_fin_problem_rate + "')";

                    //clob字段处理
                    OracleParameter[] parameter = new OracleParameter[1];
                    parameter[0] = new OracleParameter("patrolling_coordinate_strs2", OracleDbType.Clob, patrolling_coordinate_strs2, ParameterDirection.Input);

                    SqlHelper.ExecuteNonQuery(sql, CommandType.Text, parameter);
                }

                bgWork.ReportProgress(i + 1);
            }
        }

        private Dictionary<String, String> getAreaChiefPatrolRegion(int currentMonth,int currentYear)
        {
            Dictionary<String, String> result = new Dictionary<string, string>();

            //int currentMonth = DateTime.Now.Month;

            if (currentMonth == 1 || currentMonth == 2)
            {
                result.Add("start_tm", DateTime.Now.Year+"-01-01 00:00:00");
                result.Add("end_tm", DateTime.Now.Year + "-03-01 00:00:00");
            }
            else if (currentMonth == 3 || currentMonth == 4)
            {
                result.Add("start_tm", DateTime.Now.Year + "-03-01 00:00:00");
                result.Add("end_tm", DateTime.Now.Year + "-05-01 00:00:00");
            }
            else if (currentMonth == 5 || currentMonth == 6)
            {
                result.Add("start_tm", DateTime.Now.Year + "-05-01 00:00:00");
                result.Add("end_tm", DateTime.Now.Year + "-07-01 00:00:00");
            }
            else if (currentMonth == 7 || currentMonth == 8)
            {
                result.Add("start_tm", DateTime.Now.Year + "-07-01 00:00:00");
                result.Add("end_tm", DateTime.Now.Year + "-09-01 00:00:00");
            }
            else if (currentMonth == 9 || currentMonth == 10)
            {
                result.Add("start_tm", DateTime.Now.Year + "-09-01 00:00:00");
                result.Add("end_tm", DateTime.Now.Year + "-11-01 00:00:00");
            }
            else if (currentMonth == 11 || currentMonth == 12)
            {
                result.Add("start_tm", DateTime.Now.Year + "-11-01 00:00:00");
                result.Add("end_tm", DateTime.Now.Year + "-01-01 00:00:00");
            }

            return result;
        }

        //我的app使用情况
        private void button4_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgWorkMyAppUse.DoWork += new DoWorkEventHandler(worker_DoWorkAppUse);
            bgWorkMyAppUse.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgWorkMyAppUse.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgWorkMyAppUse.RunWorkerAsync();
        }

        //我的app使用情况
        void worker_DoWorkAppUse(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(我的app使用情况:appweek_my_app_use_situation)...";
            writeInfo(lblInfo.Text);

            runAppUse(progressBar1, bgWorkMyAppUse);

            lblInfo.Text = "数据处理完成(我的app使用情况:appweek_my_app_use_situation)";
            writeInfo(lblInfo.Text);

            if (auto_mode)
            {
                //继续跑我的下级村河长履职情况
                button1_Click(null, null);
            }
        }

        public void runAppUse(ProgressBar pBar, BackgroundWorker bgWork)
        {
            //String startTm = "2018-06-18 00:00:00";
            //String endTm = "2018-06-24 23:59:59";

            String sql = "";

            if (if_delAndInDB)
            {
                sql = "delete from appweek_my_app_use_situation where rm_info_id in(select id from appweek_rm_info where start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";
                if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                    sql += " and rm_app_id='" + lblChiefID.Text.Trim() + "')";
                else
                    sql += ")";

                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
            }

            sql = "select id from sys_user_b where river_type in('7','8','9','12','13','16')"; //// and id='60001536'  and id='60001620'  and id='50000475'
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();

                String id = Guid.NewGuid().ToString();

                sql = "select id from appweek_rm_info where start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')"
                    + "and rm_app_id='" + userid + "'";

                DataTable dtInfo = SqlHelper.GetDataTable(sql, null);

                if (dtInfo.Rows.Count <= 0)
                    continue;

                String rm_info_id = dtInfo.Rows[0][0].ToString();

                //本周登录签到4天this_week_login_str
                sql = "select userid,wm_concat(to_char(time, 'YYYY-MM-DD' )) sign from sys_usersign where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                   + " and userid='" + userid + "' group by userid";

                dtInfo = SqlHelper.GetDataTable(sql, null);
                String this_week_login_str = null;
                if (dtInfo.Rows.Count > 0)
                    this_week_login_str = dtInfo.Rows[0][1].ToString();

                //this_week_read_cnt	本周阅读专栏文章数量
                sql = "select count(distinct problemid) this_week_read_cnt from INFO_READINGPRAISE where tm>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and tm<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    //+ "    and type in('0','12','3','2','5','7','8','14','15')"
                   + "    and userid='" + userid + "'"
                   + " and PROBLEMID in(select id from info_news where CREATEDATE>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and CREATEDATE<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and status in('0','12','3','2','5','7','8','14','15'))";

                String this_week_read_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();

                //weekpub_cmt	本周发布以下六项专栏数量
                sql = "select count(id) nums from info_news where createdate>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and createdate<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                   + " and status in('0','12','3','2','5','7','8','14','15')";

                String weekpub_cmt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
                //leave_word_cmt	本周专栏留言数量
                sql = "select count(id) nums from INFO_READINGPRAISE where type='3' and tm>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and tm<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + " and userid='" + userid + "'"
                    + " and problemid in("
                    + "select id from info_news where CREATEDATE>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + " and CREATEDATE<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + ")";

                String leave_word_cmt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
                //news_cmt	阅读新闻动态数量
                sql = "select count(distinct problemid) news_cmt from INFO_READINGPRAISE where tm>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and tm<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    //+ " and type in('0')"
                   + " and userid='" + userid + "'"
                   + " and PROBLEMID in(select id from info_news where CREATEDATE>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and CREATEDATE<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and status='0')";

                String news_cmt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
                //weekpub_news_cmt	本周发布新闻动态数量
                sql = "select count(id) weekpub_news_cmt from info_news where createdate>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and createdate<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                   + " and status in('0')";

                String weekpub_news_cmt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
                //notice_cmt	阅读通知公告数量
                sql = "select count(distinct problemid) news_cmt from INFO_READINGPRAISE where tm>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and tm<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    //+ " and type in('12')"
                   + " and userid='" + userid + "'"
                   + " and PROBLEMID in(select id from info_news where CREATEDATE>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and CREATEDATE<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and status='12')";

                String notice_cmt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
                //weekpub_notice_cmt	本周发布通知公告数量
                sql = "select count(id) nums from info_news where createdate>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and createdate<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                   + " and status in('12')";

                String weekpub_notice_cmt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
                //experience_cmt	阅读经验交流数量
                sql = "select count(distinct problemid) experience_cmt from INFO_READINGPRAISE where tm>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and tm<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    //+ " and type in('14','3')"
                  + " and userid='" + userid + "'"
                  + " and PROBLEMID in(select id from info_news where CREATEDATE>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and CREATEDATE<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and status in('14','3'))";

                String experience_cmt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
                //weekpub_experience_cmt	本周发布经验交流数量
                sql = "select count(id) nums from info_news where createdate>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and createdate<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                   + " and status in('14','3')";

                String weekpub_experience_cmt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
                //tashanzhishi_cmt	阅读他山之石数量
                sql = "select count(distinct problemid) news_cmt from INFO_READINGPRAISE where tm>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and tm<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    //+ " and type in('2')"
                  + " and userid='" + userid + "'"
                  + " and PROBLEMID in(select id from info_news where CREATEDATE>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and CREATEDATE<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and status in('2'))";

                String tashanzhishi_cmt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
                //weekpub_tashanzhishi_cmt	本周发布他山之石数量
                sql = "select count(id) nums from info_news where createdate>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and createdate<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                   + " and status in('2')";

                String weekpub_tashanzhishi_cmt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
                //river_man_notice_cmt	阅读河长须知数量
                sql = "select count(distinct problemid) news_cmt from INFO_READINGPRAISE where tm>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and tm<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    //+ " and type in('5','15')"
                  + " and userid='" + userid + "'"
                  + " and PROBLEMID in(select id from info_news where CREATEDATE>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and CREATEDATE<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and status in('5','15'))";

                String river_man_notice_cmt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
                //weekpub_rm_notice_cmt	本周发布河长须知数量
                sql = "select count(id) nums from info_news where createdate>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and createdate<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                   + " and status in('5','15')";

                String weekpub_rm_notice_cmt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
                //black_or_red_cmt	阅读红黑榜数量
                sql = "select count(distinct problemid) news_cmt from INFO_READINGPRAISE where tm>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and tm<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    //+ " and type in('7','8')"
                  + " and userid='" + userid + "'"
                  + " and PROBLEMID in(select id from info_news where CREATEDATE>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and CREATEDATE<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and status in('7','8'))";

                String black_or_red_cmt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
                //weekpub_redblack_cmt	本周发布红黑榜数量
                sql = "select count(id) nums from info_news where createdate>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and createdate<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                   + " and status in('7','8')";

                String weekpub_redblack_cmt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();

                if (if_delAndInDB)
                {
                    sql = "insert into appweek_my_app_use_situation (id,RM_INFO_ID,this_week_login_str,this_week_read_cnt,weekpub_cmt,leave_word_cmt,news_cmt,week_pub_news_cmt,notice_cmt,"
                       + "week_pub_notice_cmt,experience_cmt,week_pub_experience_cmt,tashanzhishi_cmt,week_pub_tashanzhishi_cmt,river_man_notice_cmt,week_pub_rmnotice_cmt,black_or_red_cmt,week_put_redblack_cmt) "
                       + "values('" + id + "','" + rm_info_id + "','" + this_week_login_str + "','" + this_week_read_cnt + "','" + weekpub_cmt + "','" + leave_word_cmt + "','" + news_cmt + "','" + weekpub_news_cmt
                       + "','" + notice_cmt + "','" + weekpub_notice_cmt + "','" + experience_cmt + "','" + weekpub_experience_cmt + "','" + tashanzhishi_cmt + "','" + weekpub_tashanzhishi_cmt
                       + "','" + river_man_notice_cmt + "','" + weekpub_rm_notice_cmt + "','" + black_or_red_cmt + "','" + weekpub_redblack_cmt + "')";

                    SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                }

                bgWork.ReportProgress(i + 1);
            }
        }

        //四个查清
        private void button5_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgWorkFourCheck.DoWork += new DoWorkEventHandler(worker_DoWorkFourCheck);
            bgWorkFourCheck.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgWorkFourCheck.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgWorkFourCheck.RunWorkerAsync();
        }

        //四个查清
        void worker_DoWorkFourCheck(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(四个查清:APPWEEK_CHECK_CLEAR)...";
            writeInfo(lblInfo.Text);

            runFourCheck(progressBar1, bgWorkFourCheck);

            lblInfo.Text = "数据处理完成(四个查清:APPWEEK_CHECK_CLEAR)";
            writeInfo(lblInfo.Text);

            if (auto_mode)
            {
                //继续跑红黑榜
                btnRed_BlakList_Click(null, null);
            }
        }

        public void runFourCheck(ProgressBar pBar, BackgroundWorker bgWork)
        {
            String sql = "";

            if (if_delAndInDB)
            {
                sql = "delete from APPWEEK_CHECK_CLEAR_NEW where rm_info_id in(select id from appweek_rm_info where start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";
                if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                    sql += " and rm_app_id='" + lblChiefID.Text.Trim() + "')";
                else
                    sql += ")";

                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
            }

            sql = "select id,river_type from sys_user_b where river_type in('7','8','9','12','13','16') and del_flag='0'";// and id='60001971'
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();
                String riverType = dt.Rows[i]["river_type"].ToString();

                sql = "select id from appweek_rm_info where start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')"
                    + "and rm_app_id='" + userid + "'";

                DataTable dtInfo = SqlHelper.GetDataTable(sql, null);

                if (dtInfo.Rows.Count <= 0)
                    continue;

                String rm_info_id = dtInfo.Rows[0][0].ToString();

                sql = "select distinct river_section_id section_id,(select name || (case to_char(river_side) when '1' then '(左岸)' when '2' then '(右岸)' else '' end) from rm_river_lake where id=river_section_id) name from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0'";

                DataTable dtRiver = SqlHelper.GetDataTable(sql, null);

                Dictionary<String, String> timeRegion = getTimeRegion(riverType, startTm);
                String cqStart_time = timeRegion["startTime"];
                String cqEnd_time = timeRegion["endTime"];
                for (int idx = 0; idx < dtRiver.Rows.Count; idx++)
                {
                    String section_id = dtRiver.Rows[idx]["section_id"].ToString();
                    String section_name = dtRiver.Rows[idx]["name"].ToString();

                    //两岸通道贯通是否查清 1--查清  0--未查清
                    String not_connect = "0";
                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='1'";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        not_connect = "1";
                    //两岸通道贯通上报问题数量
                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='1' and hava_problem>=1 and section_id='" + section_id + "'";

                    String not_connect_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
                    //疑似违法建筑是否查清
                    String illegal_building = "0";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='2'";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        illegal_building = "1";
                    //疑似违法建筑上报问题数量
                    String illegal_building_cnt = "0";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='2' and hava_problem>=1 and section_id='" + section_id + "'";

                    illegal_building_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
                    //散乱污场所是否查清
                    String debunching_place = "0";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='3'";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        debunching_place = "1";
                    //散乱污场所上报问题数量
                    String debunching_place_cnt = "0";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='3' and hava_problem>=1 and section_id='" + section_id + "'";

                    debunching_place_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
                    //异常排水口是否查清
                    String outfall_exception = "0";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='4'";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        outfall_exception = "1";
                    //异常排水口上报问题数量
                    String outfall_exception_cnt = "0";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='4' and hava_problem>=1 and section_id='" + section_id + "'";

                    outfall_exception_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();

                    String id = Guid.NewGuid().ToString();

                    //四个查清的时间 check_clear_time
                    String check_clear_time = cqStart_time.Substring(0, 10) + "至" + cqEnd_time.Substring(0, 10);
                    check_clear_time = check_clear_time.Replace('/', '.').Replace('-', '.').Replace('至', '-');
                    //四个查清期数  check_clear_period
                    int check_clear_period = 1;
                    if (riverType.Equals("16"))
                        sql = "select phase from ptl_fourcheck_phase where start_date<=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and type='1'";
                    else if (riverType.Equals("13"))
                        sql = "select phase from ptl_fourcheck_phase where start_date<=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and type='2'";
                    else if (riverType.Equals("9"))
                        sql = "select phase from ptl_fourcheck_phase where start_date<=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and type='3'";

                    check_clear_period = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                    if (if_delAndInDB)
                    {
                        sql = "insert into APPWEEK_CHECK_CLEAR_NEW(id,RM_INFO_ID,RIVER_ID,river_name,not_connect,not_connect_cnt,illegal_building,illegal_building_cnt,debunching_place,debunching_place_cnt,outfall_exception,outfall_exception_cnt,check_clear_time,check_clear_period)"
                            + " values("
                            + "'" + id + "','" + rm_info_id + "','" + section_id + "','" + section_name + "','" + not_connect + "','" + not_connect_cnt + "','" + illegal_building + "','" + illegal_building_cnt
                            + "','" + debunching_place + "','" + debunching_place_cnt + "','" + outfall_exception + "','" + outfall_exception_cnt + "','" + check_clear_time + "','" + check_clear_period + "'"
                            + ")";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                    }
                }

                /*//区级河长每半年一次，镇级河长每2个月一次，村居级河长每两周完成一次，2018年7月16日开始
                for (int j = 1; j <= 4; j++)
                {
                    //类别
                    String type = j + "";

                    //week_find_cnt	本期发现问题数量
                    sql = "";
                    int week_find_cnt = 0;
                    //week_handle_cnt	本期处理问题数量
                    int week_handle_cnt = 0;
                    //addup_find_cnt	所有累计发现数量
                    int addup_find_cnt = 0;
                    //find_all_or_not	本期是否查清
                    int find_all_or_not = 0;

                    String id = Guid.NewGuid().ToString();

                    sql = "insert into APPWEEK_CHECK_CLEAR(id,RM_INFO_ID,TYPE,WEEK_FIND_CNT,ADDUP_FIND_CNT,FIND_ALL_OR_NOT,FIND_RATE,WEEK_HANDLE_CNT)"
                        + " values("
                        + "'" + id + "','" + rm_info_id + "','" + type + "','" + week_find_cnt + "','" + addup_find_cnt + "','" + find_all_or_not + "',null,'" + week_handle_cnt + "'"
                        + ")";

                    SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                }*/

                bgWork.ReportProgress(i + 1);
            }
        }

        private void btnRed_BlakList_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgWorkRedBlack.DoWork += new DoWorkEventHandler(worker_DoWork);
            bgWorkRedBlack.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgWorkRedBlack.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgWorkRedBlack.RunWorkerAsync();
        }

        //红黑榜
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(红黑榜:APPWEEK_BLACK_RED)...";
            writeInfo(lblInfo.Text);
            
            runRedBlack(progressBar1,bgWorkRedBlack);

            lblInfo.Text = "数据处理完成(红黑榜:APPWEEK_BLACK_RED)";
            writeInfo(lblInfo.Text);

            if (auto_mode)
            {
                //继续跑我的履职评价
                btnMyWorkScore_Click(null, null);
            }
        }

        public void runRedBlack(ProgressBar pBar, BackgroundWorker bgWork)
        {
            String sql = "";

            if (if_delAndInDB)
            {
                sql = "delete from APPWEEK_BLACK_RED where rm_info_id in(select id from appweek_rm_info where start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";
                if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                    sql += " and rm_app_id='" + lblChiefID.Text.Trim() + "')";
                else
                    sql += ")";

                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
            }

            sql = "select id from sys_user_b where river_type in('7','8','9','12','13','16') and del_flag='0'";// and id='50000340'
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();

                String id = Guid.NewGuid().ToString();

                sql = "select id from appweek_rm_info where start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')"
                    + "and rm_app_id='" + userid + "'";

                DataTable dtInfo = SqlHelper.GetDataTable(sql, null);

                if (dtInfo.Rows.Count <= 0)
                    continue;

                String rm_info_id = dtInfo.Rows[0][0].ToString();

                //black_cnt	本周进入黑榜次数
                sql = "select count(id) nums from info_news_redblack_user where user_id='" + userid + "' and create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                   + " and type='1'";

                int black_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);
                //red_cnt	本周进入红榜次数
                sql = "select count(id) nums from info_news_redblack_user where user_id='" + userid + "' and create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                   + " and type='0'";

                int red_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                if (if_delAndInDB)
                {
                    sql = "insert into APPWEEK_BLACK_RED(id,rm_info_id,black_cnt,red_cnt) values('"
                        + id + "','" + rm_info_id + "'," + black_cnt + "," + red_cnt + ")";

                    SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                }

                bgWork.ReportProgress(i + 1);
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(progressBar1.Value<progressBar1.Maximum)
                progressBar1.Value += 1;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = progressBar1.Maximum;
        }

        private void btnMyWorkScore_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgWorkMyScore.DoWork += new DoWorkEventHandler(worker_DoWorkMyWorkScore);
            bgWorkMyScore.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgWorkMyScore.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgWorkMyScore.RunWorkerAsync();
        }

        //我的履职评价
        void worker_DoWorkMyWorkScore(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(我的履职评价:APPWEEK_MY_SCORE)...";
            writeInfo(lblInfo.Text);

            runMyScore(progressBar1, bgWorkMyScore);

            lblInfo.Text = "数据处理完成(我的履职评价:APPWEEK_MY_SCORE)";
            writeInfo(lblInfo.Text);

            if (auto_mode)
            {
                //继续跑我的水质变化
                btnMyWaterQuality_Click(null, null);
            }
        }

        public void runMyScore(ProgressBar pBar, BackgroundWorker bgWork)
        {
            String sql = "";
            if (if_delAndInDB)
            {
                sql = "delete from APPWEEK_MY_SCORE where rm_info_id in(select id from appweek_rm_info where start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";
                if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                    sql += " and rm_app_id='" + lblChiefID.Text.Trim() + "')";
                else
                    sql += ")";

                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
            }

            sql = "select id from sys_user_b where river_type in('7','8','9','12','13','16') and del_flag='0'"; // and id='236c25e9fddc43f78a647e25ebc7e381'  and id='50000628'  and id='50000978'  and id='50000401'
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();

                String id = Guid.NewGuid().ToString();

                sql = "select id from appweek_rm_info where start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')"
                    + "and rm_app_id='" + userid + "'";

                DataTable dtInfo = SqlHelper.GetDataTable(sql, null);

                if (dtInfo.Rows.Count <= 0)
                    continue;

                String rm_info_id = dtInfo.Rows[0][0].ToString();

                //rm_this_week_score	本周评分（187履职平均得分（按河道数平均））
                sql = "select round(nvl((sum(score))/count(section_id),0),2) avg_score from "
                    + "("
                    + "    select section_id,sum(score)+60 score from "
                    + "    ("
                    + "        select section_id,EVALUATION_type,avg(score) score from user_score_r_187 where user_id='" + userid + "' and start_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') "
                    + "        and end_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + "        group by section_id,EVALUATION_type"
                    + "    ) t"
                    + "    group by section_id"
                    + ")";

                String rm_this_week_score = SqlHelper.GetDataTable(sql, null).Rows[0][0].ToString();
                //rm_score	本周河长积分（村镇）
                sql = "select round(nvl(sum(score)/count(id),0),2) score from user_score_r_187 where user_id='" + userid + "' and start_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + " and end_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and EVALUATION_type='河长积分(统计日上一周的积分X)'";

                String rm_score = SqlHelper.GetDataTable(sql, null).Rows[0][0].ToString();
                //week_pollution_contri_score	本周污染源贡献量（镇）
                sql = "select round(nvl(sum(score)/count(id),0),1) score from user_score_r_187 where user_id='" + userid + "' and start_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + " and end_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and EVALUATION_type='污染贡献量'";

                String week_pollution_contri_score = SqlHelper.GetDataTable(sql, null).Rows[0][0].ToString();
                //lower_level_situa_score	下级河长（镇）
                sql = "select round(nvl(sum(score)/count(id),0),2) score from user_score_r_187 where user_id='" + userid + "' and start_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + " and end_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and EVALUATION_type='下级河长'";

                DataTable dtLevel = SqlHelper.GetDataTable(sql, null);
                String lower_level_situa_score = dtLevel.Rows[0][0].ToString();
                //patrol_rate_score	巡河率（村）
                sql = "select round(nvl(sum(score)/count(id),0),2) score from user_score_r_187 where user_id='" + userid + "' and start_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + " and end_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and EVALUATION_type='巡河率'";

                String patrol_rate_score = SqlHelper.GetDataTable(sql, null).Rows[0][0].ToString();
                //problem_submit_score	问题上报率（村镇）
                sql = "select round(nvl(sum(score)/count(id),0),2) score from user_score_r_187 where user_id='" + userid + "' and start_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + " and end_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and EVALUATION_type='问题上报率'";

                String problem_submit_score = SqlHelper.GetDataTable(sql, null).Rows[0][0].ToString();
                //important_problem_submit_score	重大问题上报（村镇）
                sql = "select round(nvl(sum(score)/count(id),0),2) score from user_score_r_187 where user_id='" + userid + "' and start_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + " and end_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and EVALUATION_type='重大问题上报数'";

                String important_problem_submit_score = SqlHelper.GetDataTable(sql, null).Rows[0][0].ToString();
                //black_or_red_score	红黑榜（累加）（村镇）
                sql = "select round(nvl(sum(score)/count(id),0),2) score from user_score_r_187 where user_id='" + userid + "' and start_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + " and end_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and EVALUATION_type='红黑榜'";

                String black_or_red_score = SqlHelper.GetDataTable(sql, null).Rows[0][0].ToString();

                //PROBLEM_DEAL_RATE 问题完结率
                sql = "select round(nvl(sum(score)/count(id),0),2) score from user_score_r_187 where user_id='" + userid + "' and start_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss')"
                   + " and end_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and EVALUATION_type='问题完结率'";

                String PROBLEM_DEAL_RATE = SqlHelper.GetDataTable(sql, null).Rows[0][0].ToString();

                double total = 60 + Convert.ToDouble(rm_score) + Convert.ToDouble(week_pollution_contri_score) + Convert.ToDouble(lower_level_situa_score)
                    + Convert.ToDouble(patrol_rate_score) + Convert.ToDouble(black_or_red_score)
                    + Convert.ToDouble(problem_submit_score) + Convert.ToDouble(important_problem_submit_score) + Convert.ToDouble(PROBLEM_DEAL_RATE);

                if (if_delAndInDB)
                {
                    sql = "insert into APPWEEK_MY_SCORE(id, rm_info_id, rm_this_week_score, rm_score, week_pollution_contri_score, lower_level_situa_score,"
                        + "patrol_rate_score, problem_submit_score, important_problem_submit_score, black_or_red_score,PROBLEM_DEAL_RATE) values('"
                        + id + "','" + rm_info_id + "','" + total + "','" + rm_score + "','" + week_pollution_contri_score + "','" + lower_level_situa_score
                        + "','" + patrol_rate_score + "','" + problem_submit_score + "','" + important_problem_submit_score + "','" + black_or_red_score + "','" + PROBLEM_DEAL_RATE + "')";

                    SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                }

                bgWork.ReportProgress(i + 1);
            }
        }

        //水质
        private void btnMyWaterQuality_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgWorkMyWater.DoWork += new DoWorkEventHandler(worker_DoWorkMyWaterQuality);
            bgWorkMyWater.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgWorkMyWater.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgWorkMyWater.RunWorkerAsync();
        }

        //我的水质变化
        void worker_DoWorkMyWaterQuality(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(我的水质变化:APPWEEK_MY_WATER_QUALITY)...";
            writeInfo(lblInfo.Text);

            runMyWaterQuality(progressBar1, bgWorkMyWater);

            lblInfo.Text = "数据处理完成(我的水质变化:APPWEEK_MY_WATER_QUALITY)";
            writeInfo(lblInfo.Text);

            if (auto_mode)
            {
                //继续跑我的下级镇河长履职情况
                button6_Click(null, null);
            }
        }

        public void runMyWaterQuality(ProgressBar pBar, BackgroundWorker bgWork)
        {
            String sql = "";
            if (if_delAndInDB)
            {
                sql = "delete from APPWEEK_MY_WATER_QUALITY where rm_info_id in(select id from appweek_rm_info where start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";
                if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                    sql += " and rm_app_id='" + lblChiefID.Text.Trim() + "')";
                else
                    sql += ")";

                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
            }

            sql = "select id from sys_user_b where river_type in('7','8','9','12','13') and del_flag='0'"; // and id='236c25e9fddc43f78a647e25ebc7e381'  and id='50000628'
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();

                sql = "select id from appweek_rm_info where start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')"
                    + "and rm_app_id='" + userid + "'";

                DataTable dtInfo = SqlHelper.GetDataTable(sql, null);

                if (dtInfo.Rows.Count <= 0)
                {
                    bgWorkMyWater.ReportProgress(i + 1);
                    continue;
                }

                String rm_info_id = dtInfo.Rows[0][0].ToString();

                sql = "select distinct (select name from rm_river_lake where id=t_section_r.river_section_id) name,type,t_trend.create_date,t_section_r.river_section_id from "
                    + "("
                    + "    select river_id,"
                    + "    (case change_trend when '恶化' then '0' when '持平' then '1' else '2' end) TYPE,"
                    + "    create_date"
                    + "    from rm_river_rated t where RIVER_ID in"
                    + "    ("
                    + "        select distinct rivet_rm_id from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0' and rivet_rm_id in"
                    + "        (select id from rm_river_lake where is_hc152 is not null or is_hc35 is not null)"
                    + "    ) and create_date=(select max(create_date) from rm_river_rated where river_id=t.river_id)"
                    + ") t_trend "
                    + "left join "
                    + "rm_riverchief_section_r t_section_r on t_trend.river_id=t_section_r.rivet_rm_id and user_id='" + userid + "'";

                DataTable dtSection = SqlHelper.GetDataTable(sql, null);

                for (int j = 0; j < dtSection.Rows.Count; j++)
                {
                    String id = Guid.NewGuid().ToString();
                    String river_name = dtSection.Rows[j]["name"].ToString();
                    String type = dtSection.Rows[j]["type"].ToString();
                    String WATERQ_CREATE_TIME = dtSection.Rows[j]["create_date"].ToString();
                    String RIVER_ID = dtSection.Rows[j]["river_section_id"].ToString();

                    if (if_delAndInDB)
                    {
                        sql = "insert into APPWEEK_MY_WATER_QUALITY(id, rm_info_id, river_name, type,RIVER_ID,WATERQ_CREATE_TIME) values('"
                            + id + "','" + rm_info_id + "','" + river_name + "','" + type + "','" + RIVER_ID + "',to_date('" + WATERQ_CREATE_TIME + "','yyyy-MM-dd HH24:mi:ss'))";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                    }
                }
                //river_name	河涌名称
                //river_name_situation	河道状况

                bgWork.ReportProgress(i + 1);
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            //dtpStart.Text = "2018-07-05 00:00:00";
            //dtpEnd.Text = "2018-07-06 23:59:59";

            dtpStart.Value=DateTime.Now.AddDays(1 - Convert.ToInt16(DateTime.Now.DayOfWeek) - 7);

            dtpEnd.Value = DateTime.Now.AddDays(7 - Convert.ToInt16(DateTime.Now.DayOfWeek) - 7);
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            /*long time_JAVA_Long = 1531670400000;//java长整型日期，毫秒为单位                
            DateTime dt_1970 = new DateTime(1970, 1, 1, 0, 0, 0);                
            long tricks_1970 = dt_1970.Ticks;//1970年1月1日刻度                         
            long time_tricks = tricks_1970 + time_JAVA_Long * 10000;//日志日期刻度                         
            DateTime dt = new DateTime(time_tricks).AddHours(8);//转化为DateTime

             Dictionary<String, String> result=getTimeRegion("16", dtpStart.Text);
             lblInfo.Text = "开始：" + result["startTime"] + "   结束：" + result["endTime"];//GetTime("2018-07-16 00:00:00").ToString();*/

            String sql = @"INSERT INTO appweek_my_river_patrolling (		id,		rm_info_id,		valid_patrolling, 		diversify_patrolling,		patrolling_coordinate_strs,patrolling_coordinate_strs2,		this_week_times,		this_week_kilometers,		total_times, 		total_kilometers,  		thisweek_problem_cnt,  		total_problem_cnt, 		thisweek_finish_problem_cnt,		total_finish_problem_cnt, 		thisweek_assign_problem_cnt,		thisweek_citizen_complaint_cnt,		valid_patrolling_strs,      city_submit_problem_cmt,patrolling_trail_date,patrolling_startTime_endTime,import_problem_submit,      thisWeek_problem_findCnt,total_problem_findCnt,total_problem_finish_rate,thisWeek_important_cnt,total_important_cnt,period_startTime_endTime,       normal_patrolling,VALID_PATROLLING_DAY,PAPER_ASSIGN_PRO_CNT,TOT_PAPER_ASSIGN_PRO_CNT,TOT_PAPER_ASSIGN_PRO_RATE,        APP_PAPER_ASSIGN_PRO_CNT,APP_TOT_PAPER_ASSIGN_PRO_CNT,APP_TOT_PAPER_ASSIGN_PRO_RATE		) values('39f2c22c-9f2d-4ee3-aa19-75b797192289','c1505baf-6644-486d-891c-d13753cf0f6c','45','0','\JsonFile\2018-9\2018090315444056830000357.json,\JsonFile\2018-10\2018102217331495730000357.json,\JsonFile\2018-10\2018102216473352430000357.json,\JsonFile\2018-10\2018102011460904330000357.json,\JsonFile\2018-10\2018100511043479530000357.json,\JsonFile\2018-10\2018100510304838530000357.json,\JsonFile\2018-10\2018101112431085530000357.json,\JsonFile\2018-10\2018101916455663830000357.json,\JsonFile\2018-10\2018101911154393630000357.json,\JsonFile\2018-10\2018100511574062730000357.json,\JsonFile\2018-10\2018101915515283530000357.json,\JsonFile\2018-10\2018101910264964130000357.json,\JsonFile\2018-10\2018101612304809730000357.json,\JsonFile\2018-10\2018100910341506330000357.json,\JsonFile\2018-10\2018100916220858630000357.json,\JsonFile\2018-9\2018092716521413730000357.json,\JsonFile\2018-9\2018093016583438130000357.json,\JsonFile\2018-10\2018101212443398130000357.json,\JsonFile\2018-9\2018092716024072630000357.json,\JsonFile\2018-9\2018092710283527530000357.json,\JsonFile\2018-9\2018091016053424630000357.json,\JsonFile\2018-9\2018092711301778130000357.json,\JsonFile\2018-9\2018091817215855030000357.json,\JsonFile\2018-9\2018091816473653430000357.json,\JsonFile\2018-9\2018091017243319830000357.json,\JsonFile\2018-9\2018092012294556730000357.json,\JsonFile\2018-9\2018092610290753530000357.json,\JsonFile\2018-9\2018090617583630030000357.json,\JsonFile\2018-9\2018092711045580230000357.json,\JsonFile\2018-9\2018092709275633830000357.json,\JsonFile\2018-9\2018092611092307130000357.json,\JsonFile\2018-9\2018091016413934730000357.json,\JsonFile\2018-9\2018091116025662930000357.json,\JsonFile\2018-9\2018091116025514430000357.json,\JsonFile\2018-9\2018090716383413030000357.json,\JsonFile\2018-9\2018091112001699230000357.json,\JsonFile\2018-9\2018092010355440030000357.json,\JsonFile\2018-9\2018091817262399830000357.json,\JsonFile\2018-9\2018091817031084530000357.json,\JsonFile\2018-9\2018091018061021430000357.json,\JsonFile\2018-9\2018090517280513430000357.json,\JsonFile\2018-9\2018090417314808230000357.json,\JsonFile\2018-9\2018090516252627130000357.json,\JsonFile\2018-9\2018090416504656730000357.json,\JsonFile\2018-9\2018090311411667130000357.json,',:patrolling_coordinate_strs2,'1884.97','120.55','5393.79','347.66','9','167','9','166','0','0','3,6,7','0','2018-09-03,2018-09-03,2018-09-04,2018-09-04,2018-09-05,2018-09-05,2018-09-06,2018-09-07,2018-09-10,2018-09-10,2018-09-10,2018-09-10,2018-09-11,2018-09-11,2018-09-11,2018-09-18,2018-09-18,2018-09-18,2018-09-18,2018-09-20,2018-09-20,2018-09-26,2018-09-26,2018-09-26,2018-09-27,2018-09-27,2018-09-27,2018-09-27,2018-09-27,2018-09-30,2018-10-05,2018-10-05,2018-10-05,2018-10-09,2018-10-09,2018-10-11,2018-10-12,2018-10-16,2018-10-19,2018-10-19,2018-10-19,2018-10-19,2018-10-20,2018-10-22,2018-10-22','10:27-11:39,15:24-15:43,16:09-16:50,17:13-17:31,15:51-16:23,16:53-17:28,16:54-17:58,16:04-16:37,15:30-16:04,16:05-16:40,16:57-17:23,17:36-18:06,09:53-10:56,11:08-11:33,11:38-11:58,15:36-16:46,16:46-17:03,16:59-17:26,17:06-17:20,10:12-10:34,10:56-12:29,09:37-10:29,10:35-11:09,11:09-11:33,09:53-10:21,10:21-11:04,11:15-11:28,15:13-16:01,16:12-16:52,16:12-16:56,09:40-10:30,10:37-11:04,11:28-11:55,10:00-10:34,15:50-16:22,10:45-12:41,10:52-12:42,10:34-12:28,09:42-10:26,10:38-11:15,15:16-15:50,16:21-16:44,11:35-11:46,15:24-16:45,17:00-17:31','0','15','310','0.82','0','0','2018.10.15-2018.10.23','45','20','0','0','0','9','8','0.89')";

            String content=@"\JsonFile\2018-9\2018090311411667130000357.json;09月03日;10:27-11:39;72.48分钟/5.50公里,\JsonFile\2018-9\2018090315444056830000357.json;09月03日;15:24-15:43;19.18分钟/2.42公里,\JsonFile\2018-9\2018090416504656730000357.json;09月04日;16:09-16:50;41.73分钟/3.32公里,\JsonFile\2018-9\2018090417314808230000357.json;09月04日;17:13-17:31;18.53分钟/0.89公里,\JsonFile\2018-9\2018090516252627130000357.json;09月05日;15:51-16:23;32.12分钟/1.35公里,\JsonFile\2018-9\2018090517280513430000357.json;09月05日;16:53-17:28;35.05分钟/5.38公里,\JsonFile\2018-9\2018090617583630030000357.json;09月06日;16:54-17:58;63.93分钟/2.65公里,\JsonFile\2018-9\2018090716383413030000357.json;09月07日;16:04-16:37;33.07分钟/1.31公里,\JsonFile\2018-9\2018091016053424630000357.json;09月10日;15:30-16:04;33.48分钟/1.35公里,\JsonFile\2018-9\2018091016413934730000357.json;09月10日;16:05-16:40;34.42分钟/3.07公里,\JsonFile\2018-9\2018091017243319830000357.json;09月10日;16:57-17:23;25.83分钟/0.27公里,\JsonFile\2018-9\2018091018061021430000357.json;09月10日;17:36-18:06;30.02分钟/3.14公里,\JsonFile\2018-9\2018091116025662930000357.json;09月11日;09:53-10:56;62.22分钟/3.49公里,\JsonFile\2018-9\2018091116025514430000357.json;09月11日;11:08-11:33;24.3分钟/1.08公里,\JsonFile\2018-9\2018091112001699230000357.json;09月11日;11:38-11:58;20.45分钟/1.17公里,\JsonFile\2018-9\2018091816473653430000357.json;09月18日;15:36-16:46;69.9分钟/3.55公里,\JsonFile\2018-9\2018091817031084530000357.json;09月18日;16:46-17:03;16.68分钟/1.14公里,\JsonFile\2018-9\2018091817262399830000357.json;09月18日;16:59-17:26;26.52分钟/1.13公里,\JsonFile\2018-9\2018091817215855030000357.json;09月18日;17:06-17:20;13.67分钟/1.19公里,\JsonFile\2018-9\2018092010355440030000357.json;09月20日;10:12-10:34;22.28分钟/1.03公里,\JsonFile\2018-9\2018092012294556730000357.json;09月20日;10:56-12:29;92.85分钟/4.95公里,\JsonFile\2018-9\2018092610290753530000357.json;09月26日;09:37-10:29;51.18分钟/3.68公里,\JsonFile\2018-9\2018092611092307130000357.json;09月26日;10:35-11:09;33.87分钟/3.76公里,\JsonFile\2018-9\2018092709275633830000357.json;09月26日;11:09-11:33;23.53分钟/1.65公里,\JsonFile\2018-9\2018092710283527530000357.json;09月27日;09:53-10:21;28.17分钟/0.86公里,\JsonFile\2018-9\2018092711045580230000357.json;09月27日;10:21-11:04;43.1分钟/1.01公里,\JsonFile\2018-9\2018092711301778130000357.json;09月27日;11:15-11:28;12.75分钟/1.18公里,\JsonFile\2018-9\2018092716024072630000357.json;09月27日;15:13-16:01;47.63分钟/1.26公里,\JsonFile\2018-9\2018092716521413730000357.json;09月27日;16:12-16:52;39.65分钟/1.45公里,\JsonFile\2018-9\2018093016583438130000357.json;09月30日;16:12-16:56;44.15分钟/5.19公里,\JsonFile\2018-10\2018100510304838530000357.json;10月05日;09:40-10:30;50.52分钟/8.64公里,\JsonFile\2018-10\2018100511043479530000357.json;10月05日;10:37-11:04;27分钟/0.97公里,\JsonFile\2018-10\2018100511574062730000357.json;10月05日;11:28-11:55;27.08分钟/1.96公里,\JsonFile\2018-10\2018100910341506330000357.json;10月09日;10:00-10:34;33.95分钟/3.45公里,\JsonFile\2018-10\2018100916220858630000357.json;10月09日;15:50-16:22;31.23分钟/1.73公里,\JsonFile\2018-10\2018101112431085530000357.json;10月11日;10:45-12:41;116.22分钟/3.75公里,\JsonFile\2018-10\2018101212443398130000357.json;10月12日;10:52-12:42;110.52分钟/10公里,\JsonFile\2018-10\2018101612304809730000357.json;10月16日;10:34-12:28;114.22分钟/3.89公里,\JsonFile\2018-10\2018101910264964130000357.json;10月19日;09:42-10:26;44.5分钟/2.28公里,\JsonFile\2018-10\2018101911154393630000357.json;10月19日;10:38-11:15;37.32分钟/2.60公里,\JsonFile\2018-10\2018101915515283530000357.json;10月19日;15:16-15:50;33.57分钟/3.42公里,\JsonFile\2018-10\2018101916455663830000357.json;10月19日;16:21-16:44;22.92分钟/1.87公里,\JsonFile\2018-10\2018102011460904330000357.json;10月20日;11:35-11:46;10.72分钟/2.38公里,\JsonFile\2018-10\2018102216473352430000357.json;10月22日;15:24-16:45;81.43分钟/2.12公里,\JsonFile\2018-10\2018102217331495730000357.json;10月22日;17:00-17:31;31.03分钟/2.06公里";
            OracleParameter[] parameter=new OracleParameter[1];
            parameter[0] = new OracleParameter("patrolling_coordinate_strs2", OracleDbType.Clob, content, ParameterDirection.Input);
            SqlHelper.ExecuteNonQuery(sql,CommandType.Text,parameter);
        }

        private static Dictionary<String, String> getTimeRegion(String riverType, String time) {
            Dictionary<String, String> result = new Dictionary<String, String>();

            //SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
            //2018-07-16 00:00:00 时间戳 精算到毫秒 000
            long startTime = 1531670400000;
            Dictionary<Int64,String> town = new Dictionary<Int64,String>();
            town.Add(1,"07-16,09-16");
            town.Add(2,"09-16,11-16");
            town.Add(3,"11-16,01-16");
            town.Add(4,"01-16,03-16");
            town.Add(5,"03-16,05-16");
            town.Add(6,"05-16,07-16");
            long toWeek;
            //获取目标时间戳
            long aims = GetTime(time);//sdf.parse(time).getTime();
            string[] yearMonthDay = time.Split(' ');
            string[] yearMonthDaySplit = yearMonthDay[0].Split('-');

            if (riverType.Equals("16"))//村居级河长每两周完成一次
            {
                //两周时间戳
                toWeek = 14 * 24 * 60 * 60 * 1000;
                //获取第几期
                long qi = (aims - startTime) / toWeek;
                if (qi >= 0) {
                    //开始时间戳
                    long Start_Time = qi * toWeek + startTime;

                    //开始时间
                    String StartStr = LongDateTimeToDateTimeString(Start_Time.ToString(), false);
                    //结束时间
                    String EndStr = LongDateTimeToDateTimeString((Start_Time + toWeek).ToString(), true);//sdf.format(new Date(Long.parseLong(String.valueOf(StartTime + toWeek))));
                    result.Add("startTime", StartStr);
                    result.Add("endTime", EndStr);
                    result.Add("qi", qi+"");
                }
                else
                {
                    result.Add("startTime", "2018-07-16 00:00:00");
                    result.Add("endTime", "2018-07-29 23:59:59");
                    result.Add("qi", "1");
                }
            } 
            else if (riverType.Equals("12") || riverType.Equals("13"))//镇级河长每2个月一次
            {
                if(aims>startTime) {
                    foreach (Int64 key in town.Keys) {
                        DateTime start = Convert.ToDateTime(yearMonthDaySplit[0] + "-" + town[key].Split(',')[0] + " 00:00:00");
                        DateTime end = Convert.ToDateTime(yearMonthDaySplit[0] + "-" + town[key].Split(',')[1] + " 00:00:00");
                        if(Convert.ToInt64(town[key].Split(',')[0].Split('-')[0])>=11){
                            end = Convert.ToDateTime((Convert.ToUInt64(yearMonthDaySplit[0])+1) + "-" + town[key].Split(',')[1] + " 00:00:00");
                        }
                        if (aims > GetTime(start.ToString()) && aims < GetTime(end.ToString()))
                        {
                            //开始时间
                            String StartStr = start.ToString();//sdf.format(start);
                            //结束时间
                            String EndStr = end.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");//sdf.format(end);

                            result.Add("startTime", StartStr);
                            result.Add("endTime", EndStr);
                            result.Add("qi", "1");
                        }
                    }
                }
                else
                {
                    result.Add("startTime", "2018-07-16 00:00:00");
                    result.Add("endTime", "2018-09-15 23:59:59");
                    result.Add("qi", "1");
                }
            } 
            else if (riverType.Equals("7") || riverType.Equals("8") || riverType.Equals("9"))//区级河长每半年一次
            {
                if(aims>startTime) {
                    Int64 year =Convert.ToInt64(yearMonthDaySplit[0]);
                    Int64 month =Convert.ToInt64(yearMonthDaySplit[1]);
                    Int64 day =Convert.ToInt64(yearMonthDaySplit[2]);
                    DateTime start = new DateTime();
                    DateTime end = new DateTime();
                    if(month==7 && day>=16 || month>7){
                        start = Convert.ToDateTime(  year+"-"+"07-16"+" 00:00:00");
                        end = Convert.ToDateTime(  (year+1)+"-"+"01-16"+" 00:00:00");
                    }else{
                        if(year<=2018){
                            start = Convert.ToDateTime(  year+"-"+"07-16"+" 00:00:00");
                            end = Convert.ToDateTime(  (year+1)+"-"+"01-16"+" 00:00:00");

                        }else{
                            start = Convert.ToDateTime(  year+"-"+"01-16"+" 00:00:00");
                            end = Convert.ToDateTime(  year+"-"+"07-16"+" 00:00:00");
                        }
                    }
                    //开始时间
                    String StartStr = start.ToString();
                    //结束时间
                    String EndStr = end.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                    result.Add("startTime", StartStr);
                    result.Add("endTime", EndStr);
                }
                else
                {
                    result.Add("startTime", "2018-07-16 00:00:00");
                    result.Add("endTime", "2019-01-15 23:59:59");
                }
            }

            return result;
        }

        public static long GetTime(String tm)
        {
            long retval = 0;
            DateTime st = new DateTime(1970, 1, 1);
            TimeSpan t = (Convert.ToDateTime(tm).ToUniversalTime() - st);
            retval = (long)(t.TotalMilliseconds + 0.5);
            return retval;
        }

        /// <summary>
        /// 将时间戳转换为日期类型，并格式化
        /// </summary>
        /// <param name="longDateTime"></param>
        /// <returns></returns>
        private static string LongDateTimeToDateTimeString(string longDateTime,bool is_end)
        {
            //用来格式化long类型时间的,声明的变量
            long unixDate;
            DateTime start;
            DateTime date;
            //ENd
 
            unixDate = long.Parse(longDateTime);
            start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            date = start.AddMilliseconds(unixDate).ToLocalTime();

            if (is_end)
                date = date.AddDays(-1);

            if(is_end)
                return date.ToString("yyyy-MM-dd 23:59:59");
            else
                return date.ToString("yyyy-MM-dd HH:mm:ss");
 
        }

        private void btnCheckSign_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgWorkCheckSign.DoWork += new DoWorkEventHandler(worker_DoWorkCheckSign);
            bgWorkCheckSign.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgWorkCheckSign.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgWorkCheckSign.RunWorkerAsync();
        }

        //补齐签到数据
        void worker_DoWorkCheckSign(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(补齐签到数据)...";
            String sql = "";

            sql = "select id from sys_user_b where river_type in('7','8','9','12','13','16') and del_flag='0' and id<>'60001975'"; // and id='236c25e9fddc43f78a647e25ebc7e381'  and id='50000628'
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            progressBar1.Maximum = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();

                sql = "select t_p.userid,TO_CHAR(t_p.strtime, 'YYYY-MM-DD HH24:MI:SS') patrol_time,t_sign.time sign_time from ptl_patrol_r t_p"
                    + " left join sys_usersign t_sign on t_p.userid=t_sign.userid and to_char(strtime,'yyyy-MM-dd')=to_char(time,'yyyy-MM-dd')"
                    + " where t_p.userid='" + userid + "' and strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

                DataTable dtPatrolAndSign = SqlHelper.GetDataTable(sql, null);

                for (int j = 0; j < dtPatrolAndSign.Rows.Count; j++)
                {
                    String patrol_time = dtPatrolAndSign.Rows[j]["patrol_time"].ToString().Replace('/', '-');
                    String sign_time = dtPatrolAndSign.Rows[j]["sign_time"].ToString();

                    if (!String.IsNullOrEmpty(patrol_time) && String.IsNullOrEmpty(sign_time))
                    {
                        String id = Guid.NewGuid().ToString();
                        sql = "INSERT INTO sys_usersign( id, state, time, userid ) VALUES ( '" + id + "', '1', to_date('"+patrol_time.Substring(0,10)+" 00:00:00','yyyy-mm-dd hh24:mi:ss'), '" + userid + "') ";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                    }
                }

                bgWorkCheckSign.ReportProgress(i + 1); 
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgAreaChiefDownChief.DoWork += new DoWorkEventHandler(worker_DoWorkAreaChiefDownChief);
            bgAreaChiefDownChief.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgAreaChiefDownChief.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgAreaChiefDownChief.RunWorkerAsync();
        }

        //我的下级镇河长履职情况
        void worker_DoWorkAreaChiefDownChief(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(我的下级镇河长履职情况:APPWEEK_LOWER_LEVEL_SITUATION)...";
            writeInfo(lblInfo.Text);

            runAreaChiefDownChief(progressBar1, bgAreaChiefDownChief);

            lblInfo.Text = "处理完成(我的下级镇河长履职情况:APPWEEK_LOWER_LEVEL_SITUATION)";
            writeInfo(lblInfo.Text);

            if (auto_mode)
            {
                //继续跑 （区级单条河涌详情）APPWEEK_RIVER_RM_INFO
                btn_single_rm_info_Click(null, null);
            }
        }

        public void runAreaChiefDownChief(ProgressBar pBar, BackgroundWorker bgWork)
        {
            //String startTm = ""+startTm+"";
            //String endTm = ""+endTm+"";

            String sql = "";

            if (if_delAndInDB)
            {
                sql = "delete from APPWEEK_LOWER_LEVEL_SITUATION where rm_info_id in(select id from appweek_rm_info where role='3' and start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";
                if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                    sql += " and rm_app_id='" + lblChiefID.Text.Trim() + "')";
                else
                    sql += ")";

                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
            }

            sql = "select id,addvcd_area from sys_user_b where river_type in('7','8','9') and del_flag='0'"; // and id='d0e2b4d2c8c041afab22a675a6e41391'  and id='50000137'  and id='50000742'
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();
                String addvcd_area = dt.Rows[i][1].ToString();

                String id = Guid.NewGuid().ToString();

                sql = "select id from appweek_rm_info where start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')"
                    + "and rm_app_id='" + userid + "'";

                DataTable dtInfo = SqlHelper.GetDataTable(sql, null);

                if (dtInfo.Rows.Count <= 0)
                    continue;

                String rm_info_id = dtInfo.Rows[0][0].ToString();

                sql = "select count(distinct user_id) nums from "
                    + "("
                    + "     select t.*,(select river_type from sys_user_b where id=user_id) river_type,(select addvcd_area from sys_user_b where id=user_id) addvcd_area from rm_riverchief_section_r t where del_flags='0' and river_section_id in"
                    + "    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0'))"
                    + ") t_chief where river_type='13' and addvcd_area='" + addvcd_area + "'";

                //镇河长人数统计
                int lower_level_rm_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //计算0巡河人数
                sql = "select count(user_id) from"
                    + "("
                    + "    select t_user.user_id,t_patrol.nums from"
                    + "    ("
                    + "                    select distinct user_id,river_type from rm_riverchief_section_r t_r"
                    + "                    left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                    where del_flags='0' and river_section_id in"
                    + "                    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0' and del_flags='0'))"
                    + "                and (select river_type from sys_user_b where id=t_user.id)='13'"
                    + "    ) t_user"
                    + "    left join"
                    + "    ("
                    + "        select userid,count(patrolid) nums from ptl_patrol_r where strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + "        and userid in("
                    + "                    select distinct user_id from rm_riverchief_section_r t_r"
                    + "                    left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                    where del_flags='0' and river_section_id in"
                    + "                    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0' and del_flags='0'))"
                    + "                and (select river_type from sys_user_b where id=t_user.id)='13'"
                    + "        ) group by userid"
                    + "    ) t_patrol"
                    + "    on t_user.user_id=t_patrol.userid"
                    + ") where nums=0 or nums is null";

                int ZERO_PATROLLING_CNT = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //计算0上报人数
                sql = "select count(user_id) from"
                    + "("
                    + "    select t_user.user_id,t_patrol.nums from"
                    + "    ("
                    + "                    select distinct user_id,river_type from rm_riverchief_section_r t_r"
                    + "                    left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                    where del_flags='0' and river_section_id in"
                    + "                    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0' and del_flags='0'))"
                    + "                and (select river_type from sys_user_b where id=t_user.id)='13'"
                    + "    ) t_user"
                    + "    left join"
                    + "    ("
                    + "        select userid,count(problemid) nums from ptl_reportproblem where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + "        and userid in("
                    + "                    select distinct user_id from rm_riverchief_section_r t_r"
                    + "                    left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                    where del_flags='0' and river_section_id in"
                    + "                    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0' and del_flags='0'))"
                    + "                    and (select river_type from sys_user_b where id=t_user.id)='13'"
                    + "        ) group by userid"
                    + "    ) t_patrol"
                    + "    on t_user.user_id=t_patrol.userid"
                    + ") where nums=0 or nums is null";

                int ZERO_SUBMIT_PROBLEM_CNT = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //计算下级镇河长巡河达标率
                sql = "select round(sum(nums)/(count(userid)),2) rate from "
                    + "("
                    + "    select userid,(case when count(userid)>=1 then 1 else 0 end) nums  from"
                    + "    ("
                    + "        select userid,count(patrolid) nums from ptl_patrol_r where strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + "        and userid in("
                    + "                        select distinct user_id from rm_riverchief_section_r t_r"
                    + "                        left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                        where del_flags='0' and river_section_id in"
                    + "                        (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0' and del_flags='0'))"
                    + "                    and (select river_type from sys_user_b where id=t_user.id)='13'"
                    + "        ) and duration>=10 group by userid,to_char(strtime,'yyyy-MM-dd')"
                    + "    )t group by userid"
                    + ")";

                DataTable dtTabel = SqlHelper.GetDataTable(sql, null);

                Double LOWER_LEVEL_RM_WORK_STAND_RATE = dtTabel.Rows[0][0] == DBNull.Value ? 0 : Convert.ToDouble(dtTabel.Rows[0][0]);

                //下级镇河长巡河达标人数 lower_level_rm_std_cnt
                sql = "select distinct user_id from rm_riverchief_section_r t_r"
                    + "                    left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                    where del_flags='0' and river_section_id in"
                    + "                    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0' and del_flags='0'))"
                    + "                and (select river_type from sys_user_b where id=t_user.id)='13'";

                DataTable dtTabeUser = SqlHelper.GetDataTable(sql, null);

                int db_person = 0;
                int lower_level_check_clear = 0;
                String no_check_clear_app_id = "";//未完成四个查清河长ID，用逗号分隔
                lblInfo.Text = userid;
                for (int j = 0; j < dtTabeUser.Rows.Count; j++)
                {
                    String low_user = dtTabeUser.Rows[j][0].ToString();

                    sql = "select count(userid) nums from "
                        + "("
                        + "    select userid,to_char(strtime,'yyyy-MM-dd') tm,count(patrolid) nums from ptl_patrol_r where"
                        + "    strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                        + "    and duration>=10 and userid='" + low_user + "'"
                        + "    group by userid,to_char(strtime,'yyyy-MM-dd')"
                        + ")";

                    DataTable dtPerson = SqlHelper.GetDataTable(sql, null);

                    int if_dbts = Convert.ToInt32(dtPerson.Rows[0][0].ToString());//达标天数

                    if (if_dbts >= 1)
                        db_person++;

                    //计算未完成四个查清河长人数（区）
                    sql = "select distinct river_section_id section_id,(select name || (case to_char(river_side) when '1' then '(左岸)' when '2' then '(右岸)' else '' end) from rm_river_lake where id=river_section_id) name from rm_riverchief_section_r where user_id='" + low_user + "' and is_old='0' and del_flags='0'";

                    DataTable dtRiver = SqlHelper.GetDataTable(sql, null);

                    Dictionary<String, String> timeRegion = getTimeRegion("13", startTm);
                    String cqStart_time = timeRegion["startTime"];
                    String cqEnd_time = timeRegion["endTime"];

                    String not_connect = "0";
                    String illegal_building = "0";
                    String debunching_place = "0";
                    String outfall_exception = "0";
                    //for (int idx = 0; idx < dtRiver.Rows.Count; idx++)
                    //{
                        //String section_id = dtRiver.Rows[idx]["section_id"].ToString();
                        //String section_name = dtRiver.Rows[idx]["name"].ToString();

                        //两岸通道贯通是否查清 1--查清  0--未查清
                        sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and user_id='" + low_user + "' and check_type='1'";

                        if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                            not_connect = "1";
                        
                        //疑似违法建筑是否查清
                        sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and user_id='" + low_user + "' and check_type='2'";

                        if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                            illegal_building = "1";
                        
                        //散乱污场所是否查清
                        sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and user_id='" + low_user + "' and check_type='3'";

                        if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                            debunching_place = "1";
                        //异常排水口是否查清
                        sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and user_id='" + low_user + "' and check_type='4'";

                        if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                            outfall_exception = "1";
                    //}

                    if (not_connect.Equals("0") || illegal_building.Equals("0") || debunching_place.Equals("0") || outfall_exception.Equals("0"))
                    {
                        lower_level_check_clear++;
                        no_check_clear_app_id += low_user + ",";
                    }
                }

                int lower_level_rm_std_cnt = db_person;
                //计算下级镇河长问题上报数量
                sql = "select count(problemid) nums from ptl_reportproblem "
                    + "where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + "and userid in("
                    + "                    select distinct user_id from rm_riverchief_section_r t_r"
                    + "                    left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                    where del_flags='0' and river_section_id in"
                    + "                    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0' and del_flags='0'))"
                    + "                and (select river_type from sys_user_b where id=t_user.id)='13'"
                    + ")";

                int LOWER_LEVEL_RM_PROBLEM_SUB_CNT = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //本周下级镇河长进入红榜人数
                sql = "select count(id) nums from INFO_NEWS_REDBLACK_USER "
                    + "where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + " and type='0' and user_id in("
                    + "                    select distinct user_id from rm_riverchief_section_r t_r"
                    + "                    left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                    where del_flags='0' and river_section_id in"
                    + "                    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0' and del_flags='0'))"
                    + "                and (select river_type from sys_user_b where id=t_user.id)='13'"
                    + ")";

                int lower_level_rm_black_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                //本周下级镇河长进入黑榜人数
                sql = "select count(id) nums from INFO_NEWS_REDBLACK_USER "
                    + "where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                    + " and type='1' and user_id in("
                    + "                    select distinct user_id from rm_riverchief_section_r t_r"
                    + "                    left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                    where del_flags='0' and river_section_id in"
                    + "                    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0' and del_flags='0'))"
                    + "                and (select river_type from sys_user_b where id=t_user.id)='13'"
                    + ")";
                int lower_level_rm_red_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                if (if_delAndInDB)
                {
                    //插入数据至APPWEEK_LOWER_LEVEL_SITUATION表
                    sql = "insert into APPWEEK_LOWER_LEVEL_SITUATION(id,LOWER_LEVEL_RM_BLACK_CNT,LOWER_LEVEL_RM_CNT,LOWER_LEVEL_RM_PROBLEM_SUB_CNT,LOWER_LEVEL_RM_WORK_STAND_RATE"
                        + ",LOWER_LEVEL_RM_RED_CNT,RM_INFO_ID,LOWER_LEVEL_187RM_AVG_SCORE,ZERO_PATROLLING_CNT,ZERO_SUBMIT_PROBLEM_CNT,NO_ACHIEVE_CNT,LOWER_LEVEL_RM_STD_CNT,lower_level_check_clear,no_check_clear_app_id)"
                        + " values('" + id + "','" + lower_level_rm_black_cnt + "','" + lower_level_rm_cnt + "','" + LOWER_LEVEL_RM_PROBLEM_SUB_CNT + "','" + LOWER_LEVEL_RM_WORK_STAND_RATE
                        + "','" + lower_level_rm_red_cnt + "','" + rm_info_id + "',null,'" + ZERO_PATROLLING_CNT + "','" + ZERO_SUBMIT_PROBLEM_CNT + "',null," + lower_level_rm_std_cnt + ",'" + lower_level_check_clear + "','" + no_check_clear_app_id + "')";

                    SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                }

                bgWork.ReportProgress(i + 1);
            }
        }

        public void btnStartVillage_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgWorkVillage.DoWork += new DoWorkEventHandler(worker_DoWorkVillage);
            bgWorkVillage.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgWorkVillage.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgWorkVillage.RunWorkerAsync();
        }

        //村级河长
        public void worker_DoWorkVillage(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(村级河长)...";
            writeInfo("数据处理中(村级河长)...");

            run187_Village(progressBar1, bgWorkVillage);

            lblInfo.Text = "数据处理完成(村级河长)...";
            writeInfo("数据处理完成(村级河长)...");

            if (auto_mode)
            {
                //继续跑镇河长
                btnStartTown_Click(null, null);
            }
        }

        public void run187_Village(ProgressBar pBar, BackgroundWorker bgWork)
        {
            //String startTm = "2018-06-18 00:00:00";
            //String endTm = "2018-06-24 23:59:59";
            String sql = "";

            String week = startTm.Substring(0, 4) + CommonUtils.GetWeekOfYear(Convert.ToDateTime(startTm));

            //sql = "delete from USER_SCORE_B_187 where id in(select score_b_id from USER_SCORE_R_187 where START_DATE=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_DATE=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss'))";
            if (if_delAndInDB)
            {
                sql = "delete from user_score_b_187 where cycle='" + week + "'";

                if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                    sql += " and user_id='" + lblChiefID.Text.Trim() + "'";

                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                sql = "delete from USER_SCORE_R_187 where START_DATE=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_DATE=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";
                if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                    sql += " and user_id='" + lblChiefID.Text.Trim() + "'";

                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
            }

            sql = "select id from sys_user_b where river_type='16' and del_flag='0'";// and mobile like '%135%' and id='60001483'  and id='60000064'  and id='60000716'  and id='60001185'
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userId = dt.Rows[i]["id"].ToString();

                //查询用户责任河涌
                sql = "select distinct rivet_rm_id,river_section_id,nvl(is_hc152,'n') is_hc152,nvl(is_hc35,'n') is_hc35 "
                    + "from RM_RIVERCHIEF_SECTION_R t_r left join rm_river_lake t_river on t_r.river_section_id=t_river.id "
                    + "where t_r.user_id='" + userId + "' and t_r.is_old='0' and del_flags='0' and t_r.rivet_rm_id in "
                    + "(select id from rm_river_lake where is_hc152 is not null or is_hc35 is not null)";

                DataTable dtRiver = SqlHelper.GetDataTable(sql, null);

                //String week = startTm.Substring(0,4)+CommonUtils.GetWeekOfYear(Convert.ToDateTime(startTm));


                for (int j = 0; j < dtRiver.Rows.Count; j++)
                {
                    //最差得分项统计
                    int downItem = 0;

                    String b_id = Guid.NewGuid().ToString();

                    String river_id = dtRiver.Rows[j]["rivet_rm_id"].ToString();
                    String section_id = dtRiver.Rows[j]["river_section_id"].ToString();

                    String hc35 = dtRiver.Rows[j]["is_hc35"].ToString();
                    String hc152 = dtRiver.Rows[j]["is_hc152"].ToString();

                    String scoreType = "1";
                    double total = 0;//60;
                    if (!hc35.Equals("n")) //35条不黑不臭
                    {
                        scoreType = "1";
                        //河长积分
                        String item = "河长积分(统计日上一周的积分X)";
                        //select sum(change_score) from user_score_r where user_id='60000059' and create_time>=to_date('2018-06-25 00:00:00','yyyy-MM-dd HH24:mi:ss') and create_time<=to_date('2018-07-01 23:59:59','yyyy-MM-dd HH24:mi:ss')
                        sql = "select nvl(sum(change_score),0) score from user_score_r where user_id='" + userId + "' and create_time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

                        double scoreRiver = Convert.ToDouble(SqlHelper.GetDataTable(sql, null).Rows[0][0].ToString()) / 7.0;

                        int score = 0;
                        String subItem = "";
                        if (scoreRiver <= 0)
                        {
                            downItem++;

                            score = -5;
                            subItem = "X <= 0,扣5分";
                        }
                        else if (scoreRiver > 0 && scoreRiver <= 0.5)
                        {
                            score = 1;
                            subItem = "0 < X <= 0.5,加1分";
                        }
                        else if (scoreRiver > 0.5 && scoreRiver <= 1.5)
                        {
                            score = 2;
                            subItem = "0.5 < X <= 1.5,加2分";
                        }
                        else if (scoreRiver > 1.5 && scoreRiver <= 2.5)
                        {
                            score = 3;
                            subItem = "1.5 < X <= 2.5,加3分";
                        }
                        else if (scoreRiver > 2.5 && scoreRiver <= 3.5)
                        {
                            score = 4;
                            subItem = "2.5 < X <= 3.5,加4分";
                        }
                        else if (scoreRiver > 3.5 && scoreRiver <= 5.5)
                        {
                            score = 5;
                            subItem = "3.5 < X <= 5.5,加5分";
                        }
                        else if (scoreRiver > 5.5 && scoreRiver <= 7.5)
                        {
                            score = 6;
                            subItem = "5.5 < X <= 7.5,加6分";
                        }
                        else if (scoreRiver > 7.5 && scoreRiver <= 9.5)
                        {
                            score = 8;
                            subItem = "7.5 < X <= 9.5,加8分";
                        }
                        else if (scoreRiver > 9.5 && scoreRiver <= 11.5)
                        {
                            score = 10;
                            subItem = "9.5 < X <= 11.5,加10分";
                        }
                        else if (scoreRiver > 11.5 && scoreRiver <= 13.5)
                        {
                            score = 12;
                            subItem = "11.5 < X <= 13.5,加12分";
                        }
                        else if (scoreRiver > 13.5)
                        {
                            score = 14;
                            subItem = "13.5 < X,加14分";
                        }

                        String id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;
                        //巡河率 有效巡河率（X） = 统计日前7天内的有效巡河次数 / 7天：=====================================
                        score = 0;
                        item = "巡河率";
                        sql = "select nvl(count(tm),0) nums from (select userid,to_char(strtime,'yyyy-MM-dd') tm,count(patrolid) nums from ptl_patrol_r where duration>=10 "
                            + "and strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and userid='" + userId + "' "
                            + "group by userid,to_char(strtime,'yyyy-MM-dd') order by tm asc)";

                        int xunHeDay = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        double xunheRate = Convert.ToDouble(xunHeDay) / 7.0;
                        if (xunheRate == 0)
                        {
                            downItem++;

                            score = -10;
                            subItem = "X = 0 ，扣10分";
                        }
                        else if (xunheRate > 0 && xunheRate <= 0.3)
                        {
                            score = -7;
                            subItem = "0%< X <= 30% ，扣7分";
                        }
                        else if (xunheRate > 0.3 && xunheRate <= 0.6)
                        {
                            score = -3;
                            subItem = "30%< X <= 60% ，扣3分";
                        }
                        else if (xunheRate > 0.6 && xunheRate <= 0.8)
                        {
                            score = -2;
                            subItem = "60%< X <= 80% ，扣2分";
                        }
                        else if (xunheRate > 0.8 && xunheRate < 1)
                        {
                            score = 2;
                            subItem = "80%< X <100% ，加2分";
                        }
                        else if (xunheRate >= 1)
                        {
                            score = 12;
                            subItem = "X = 100%,加12分";
                        }

                        id = Guid.NewGuid().ToString();

                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;
                        //问题上报率
                        score = 0;
                        item = "问题上报率";

                        //河长上报问题总数
                        sql = "select nvl(count(problemid),0) nums from ptl_reportproblem where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and userid='" + userId + "' and sectionid='" + section_id + "'";

                        int problemSum = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        //责任河段内问题的总数
                        sql = "select nvl(count(problemid),0) nums from ptl_reportproblem where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and sectionid='" + section_id + "'";

                        int totalProblem = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        if (problemSum == 0 && totalProblem == 0)
                        {
                            subItem = "当Y = O，Z = 0，不加分不扣分";
                            score = 0;
                        }
                        else if (problemSum == 0 && totalProblem > 0)
                        {
                            downItem++;

                            subItem = "当Y = 0，若Z>0，扣7分";
                            score = -7;
                        }
                        else if (problemSum > 0)
                        {
                            double rate = problemSum / totalProblem;
                            if (rate > 0 && rate <= 0.2)
                            {
                                subItem = "当Y>0，0% < X <= 20%，扣5分";
                                score = -5;
                            }
                            else if (rate > 0.2 && rate <= 0.4)
                            {
                                subItem = "当Y>0，20% < X <= 40%，扣3分";
                                score = -3;
                            }
                            else if (rate > 0.4 && rate <= 0.5)
                            {
                                subItem = "当Y>0，40% < X <= 50%,扣1分";
                                score = -1;
                            }
                            else if (rate > 0.5 && rate <= 0.7)
                            {
                                subItem = "当Y>0，50% < X <= 70%，加1分";
                                score = 1;
                            }
                            else if (rate > 0.7 && rate <= 0.8)
                            {
                                subItem = "当Y>0，70% < X <= 80%,加3分";
                                score = 3;
                            }
                            else if (rate > 0.8 && rate <= 0.9)
                            {
                                subItem = "当Y>0，80% < X <= 90%,加6分";
                                score = 6;
                            }
                            else if (rate > 0.9)
                            {
                                subItem = "当Y>0，X > 90%,加8分";
                                score = 8;
                            }
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;
                        //重大问题上报数
                        score = 0;
                        item = "重大问题上报数";

                        //河长发现重大问题数
                        sql = "select nvl(count(id),0) nums from ptl_problem_paper_assign where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + "and problem_id in(select problemid from ptl_reportproblem where sectionid='" + section_id + "' and userid='" + userId + "')";

                        int riverCheifFindProblem = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);
                        //市民、市巡查发现重大问题数
                        sql = "select count(id) nums from ptl_problem_paper_assign where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + "and problem_id in(select problemid from ptl_reportproblem where sectionid='" + section_id + "' and (pro_resource='2' or userid in(select id from sys_user_b where river_type='21')))";

                        int cityAndCheckProblem = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        int submitNum = riverCheifFindProblem - cityAndCheckProblem;

                        if (submitNum < -2)
                        {
                            downItem++;

                            score = -6;
                            subItem = "X < -2,扣6分";
                        }
                        else if (submitNum == -2)
                        {
                            score = -5;
                            subItem = "X = -2，扣5分";
                        }
                        else if (submitNum == -1)
                        {
                            score = -3;
                            subItem = "X = -1,扣3分";
                        }
                        else if (submitNum == 0)
                        {
                            score = 0;
                            subItem = "X = 0,不加分不扣分";
                        }
                        else if (submitNum == 1)
                        {
                            score = 3;
                            subItem = "X = 1,加3分";
                        }
                        else if (submitNum == 2)
                        {
                            score = 5;
                            subItem = "X = 2,加5分";
                        }
                        else if (submitNum > 2)
                        {
                            score = 6;
                            subItem = "X > 2,加6分";
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;
                        //红黑榜
                        score = 0;
                        item = "红黑榜";

                        //红榜
                        sql = "select nvl(count(id),0) nums from info_news_redblack_user where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + " and user_id='" + userId + "' and type='0'";

                        int redNums = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        sql = "select nvl(count(id),0) nums from info_news_redblack_user where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + " and user_id='" + userId + "' and type='1'";

                        int blackNums = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        score = redNums * 10 - blackNums * 10;

                        subItem = "本周上红榜" + redNums + "次，上黑榜" + blackNums + "次";

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_B_187 (ID,USER_ID,CYCLE,SCORE,CREATE_DATE,SCORE_LEVEL,RIVER_ID,SECTION_ID) values "
                                + "('" + b_id + "','" + userId + "','" + week + "'," + total + ",sysdate," + downItem + ",'" + river_id + "','" + section_id + "')";

                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                    }
                    else if (!hc152.Equals("n"))//152条黑臭
                    {
                        scoreType = "0";

                        //河长积分
                        String item = "河长积分(统计日上一周的积分X)";
                        //select sum(change_score) from user_score_r where user_id='60000059' and create_time>=to_date('2018-06-25 00:00:00','yyyy-MM-dd HH24:mi:ss') and create_time<=to_date('2018-07-01 23:59:59','yyyy-MM-dd HH24:mi:ss')
                        sql = "select nvl(sum(change_score),0) score from user_score_r where user_id='" + userId + "' and create_time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

                        double scoreRiver = Convert.ToDouble(SqlHelper.GetDataTable(sql, null).Rows[0][0].ToString()) / 7.0;

                        int score = 0;
                        String subItem = "";
                        if (scoreRiver <= 0)
                        {
                            downItem++;

                            score = -5;
                            subItem = "X <= 0,扣5分";
                        }
                        else if (scoreRiver > 0 && scoreRiver <= 0.5)
                        {
                            score = 1;
                            subItem = "0 < X <= 0.5,加1分";
                        }
                        else if (scoreRiver > 0.5 && scoreRiver <= 1.5)
                        {
                            score = 2;
                            subItem = "0.5 < X <= 1.5,加2分";
                        }
                        else if (scoreRiver > 1.5 && scoreRiver <= 2.5)
                        {
                            score = 3;
                            subItem = "1.5 < X <= 2.5,加3分";
                        }
                        else if (scoreRiver > 2.5 && scoreRiver <= 3.5)
                        {
                            score = 4;
                            subItem = "2.5 < X <= 3.5,加4分";
                        }
                        else if (scoreRiver > 3.5 && scoreRiver <= 5.5)
                        {
                            score = 5;
                            subItem = "3.5 < X <= 5.5,加5分";
                        }
                        else if (scoreRiver > 5.5 && scoreRiver <= 7.5)
                        {
                            score = 6;
                            subItem = "5.5 < X <= 7.5,加6分";
                        }
                        else if (scoreRiver > 7.5 && scoreRiver <= 9.5)
                        {
                            score = 7;
                            subItem = "7.5 < X <= 9.5,加7分";
                        }
                        else if (scoreRiver > 9.5 && scoreRiver <= 11.5)
                        {
                            score = 8;
                            subItem = "9.5 < X <= 11.5,加8分";
                        }
                        else if (scoreRiver > 11.5 && scoreRiver <= 13.5)
                        {
                            score = 9;
                            subItem = "11.5 < X <= 13.5,加9分";
                        }
                        else if (scoreRiver > 13.5)
                        {
                            score = 10;
                            subItem = "13.5 < X,加10分";
                        }

                        String id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;
                        //巡河率 有效巡河率（X） = 统计日前7天内的有效巡河次数 / 7天：
                        score = 0;
                        item = "巡河率";
                        sql = "select nvl(count(tm),0) nums from (select userid,to_char(strtime,'yyyy-MM-dd') tm,count(patrolid) nums from ptl_patrol_r where duration>=10 "
                            + "and strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and userid='" + userId + "' "
                            + "group by userid,to_char(strtime,'yyyy-MM-dd') order by tm asc)";

                        int xunHeDay = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        double xunheRate = Convert.ToDouble(xunHeDay) / 7.0;
                        if (xunheRate == 0)
                        {
                            downItem++;

                            score = -10;
                            subItem = "X = 0 ，扣10分";
                        }
                        else if (xunheRate > 0 && xunheRate <= 0.3)
                        {
                            score = -7;
                            subItem = "0%< X <= 30% ，扣7分";
                        }
                        else if (xunheRate > 0.3 && xunheRate <= 0.6)
                        {
                            score = -3;
                            subItem = "30%< X <= 60% ，扣3分";
                        }
                        else if (xunheRate > 0.6 && xunheRate <= 0.8)
                        {
                            score = -2;
                            subItem = "60%< X <= 80% ，扣2分";
                        }
                        else if (xunheRate > 0.8 && xunheRate < 1)
                        {
                            score = 2;
                            subItem = "80%< X <100% ，加2分";
                        }
                        else if (xunheRate >= 1)
                        {
                            score = 10;
                            subItem = "X = 100%,加10分";
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;
                        //问题上报率
                        score = 0;
                        item = "问题上报率";

                        //河长上报问题总数
                        sql = "select nvl(count(problemid),0) nums from ptl_reportproblem where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and userid='" + userId + "' and sectionid='" + section_id + "'";

                        int problemSum = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        //责任河段内问题的总数
                        sql = "select nvl(count(problemid),0) nums from ptl_reportproblem where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and sectionid='" + section_id + "'";

                        int totalProblem = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        if (problemSum == 0 && totalProblem == 0)
                        {
                            subItem = "当Y = O，Z = 0，不加分不扣分";
                            score = 0;
                        }
                        else if (problemSum == 0 && totalProblem > 0)
                        {
                            downItem++;

                            subItem = "当Y = 0，若Z>0，，扣10分";
                            score = -10;
                        }
                        else if (problemSum > 0)
                        {
                            double rate = Convert.ToDouble(problemSum) / Convert.ToDouble(totalProblem);
                            if (rate > 0 && rate <= 0.2)
                            {
                                subItem = "当Y>0，0% < X <= 20%，扣7分";
                                score = -7;
                            }
                            else if (rate > 0.2 && rate <= 0.4)
                            {
                                subItem = "当Y>0，20% < X <= 40%，扣5分";
                                score = -5;
                            }
                            else if (rate > 0.4 && rate <= 0.5)
                            {
                                subItem = "当Y>0，40% < X <= 50%,扣3分";
                                score = -3;
                            }
                            else if (rate > 0.5 && rate <= 0.7)
                            {
                                subItem = "当Y>0，50% < X <= 70%,加3分";
                                score = 3;
                            }
                            else if (rate > 0.7 && rate <= 0.8)
                            {
                                subItem = "当Y>0，70% < X <= 80%,加5分";
                                score = 5;
                            }
                            else if (rate > 0.8 && rate <= 0.9)
                            {
                                subItem = "当Y>0，80% < X <= 90%,加7分";
                                score = 7;
                            }
                            else if (rate > 0.9)
                            {
                                subItem = "当Y>0，X > 90%,加10分";
                                score = 10;
                            }
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;
                        //重大问题上报数
                        score = 0;
                        item = "重大问题上报数";

                        //河长发现重大问题数
                        sql = "select nvl(count(id),0) nums from ptl_problem_paper_assign where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + "and problem_id in(select problemid from ptl_reportproblem where sectionid='" + section_id + "' and userid='" + userId + "')";

                        int riverCheifFindProblem = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);
                        //市民、市巡查发现重大问题数
                        sql = "select count(id) nums from ptl_problem_paper_assign where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + "and problem_id in(select problemid from ptl_reportproblem where sectionid='" + section_id + "' and (pro_resource='2' or userid in(select id from sys_user_b where river_type='21')))";

                        int cityAndCheckProblem = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        int submitNum = riverCheifFindProblem - cityAndCheckProblem;

                        if (submitNum < -2)
                        {
                            downItem++;

                            score = -10;
                            subItem = "X < -2,扣10分";
                        }
                        else if (submitNum == -2)
                        {
                            score = -8;
                            subItem = "X = -2，扣8分";
                        }
                        else if (submitNum == -1)
                        {
                            score = -5;
                            subItem = "X = -1,扣5分";
                        }
                        else if (submitNum == 0)
                        {
                            score = 0;
                            subItem = "X = 0,不加分不扣分";
                        }
                        else if (submitNum == 1)
                        {
                            score = 5;
                            subItem = "X = 1,加5分";
                        }
                        else if (submitNum == 2)
                        {
                            score = 8;
                            subItem = "X = 2,加8分";
                        }
                        else if (submitNum > 2)
                        {
                            score = 10;
                            subItem = "X > 2,加10分";
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;
                        //红黑榜
                        score = 0;
                        item = "红黑榜";

                        //红榜
                        sql = "select nvl(count(id),0) nums from info_news_redblack_user where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + " and user_id='" + userId + "' and type='0'";

                        int redNums = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        sql = "select nvl(count(id),0) nums from info_news_redblack_user where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + " and user_id='" + userId + "' and type='1'";

                        int blackNums = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        score = redNums * 10 - blackNums * 10;

                        subItem = "本周上红榜" + redNums + "次，上黑榜" + blackNums + "次";

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_B_187 (ID,USER_ID,CYCLE,SCORE,CREATE_DATE,SCORE_LEVEL,RIVER_ID,SECTION_ID) values "
                                + "('" + b_id + "','" + userId + "','" + week + "'," + total + ",sysdate," + downItem + ",'" + river_id + "','" + section_id + "')";

                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                    }
                }
                bgWork.ReportProgress(i + 1);
            }
        }

        private void btnStartTown_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgWorkTown.DoWork += new DoWorkEventHandler(worker_DoWorkTown);
            bgWorkTown.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgWorkTown.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgWorkTown.RunWorkerAsync();
        }

        //镇级河长
        void worker_DoWorkTown(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(镇级河长)...";
            writeInfo("数据处理中(镇级河长)...");

            run187_town(progressBar1, bgWorkTown);

            lblInfo.Text = "数据处理完成(镇级河长)...";
            writeInfo("数据处理完成(镇级河长)...");

            if (auto_mode)
            {
                //继续跑区河长
                btnStartArea_Click(null, null);
            }
        }

        private void run187_town(ProgressBar pBar, BackgroundWorker bgWork)
        {
            //String startTm = "2018-06-18 00:00:00";
            //String endTm = "2018-06-24 23:59:59";

            String sql = "";
            //sql = "delete from USER_SCORE_B_187 where id in(select id from USER_SCORE_R_187 where START_DATE=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_DATE=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss'))";

            //SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

            //sql = "delete from USER_SCORE_R_187 where START_DATE=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_DATE=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";

            //SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

            sql = "select id from sys_user_b where river_type in('12','13') and del_flag='0'";
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userId = dt.Rows[i]["id"].ToString();

                //查询用户责任河涌
                sql = "select distinct rivet_rm_id,river_section_id,nvl(is_hc152,'n') is_hc152,nvl(is_hc35,'n') is_hc35 "
                    + "from RM_RIVERCHIEF_SECTION_R t_r left join rm_river_lake t_river on t_r.river_section_id=t_river.id "
                    + "where t_r.user_id='" + userId + "' and t_r.is_old='0' and del_flags='0' and t_r.rivet_rm_id in "
                    + "(select id from rm_river_lake where is_hc152 is not null or is_hc35 is not null)";

                DataTable dtRiver = SqlHelper.GetDataTable(sql, null);

                String week = startTm.Substring(0, 4) + CommonUtils.GetWeekOfYear(Convert.ToDateTime(startTm));

                for (int j = 0; j < dtRiver.Rows.Count; j++)
                {
                    int downItem = 0;
                    String b_id = Guid.NewGuid().ToString();

                    String river_id = dtRiver.Rows[j]["rivet_rm_id"].ToString();
                    String section_id = dtRiver.Rows[j]["river_section_id"].ToString();

                    String hc35 = dtRiver.Rows[j]["is_hc35"].ToString();
                    String hc152 = dtRiver.Rows[j]["is_hc152"].ToString();

                    String scoreType = "1";
                    double total = 0;//60;
                    if (!hc35.Equals("n")) //35条不黑不臭
                    {
                        scoreType = "1";
                        //污染贡献量
                        String item = "污染贡献量";
                        sql = "select CHANGE_TREND from rm_river_rated where river_id='" + river_id + "' and CREATE_DATE=(select max(CREATE_DATE) from rm_river_rated where river_id='" + river_id + "')";

                        String dtWry = null;
                        DataTable tbWry = SqlHelper.GetDataTable(sql, null);
                        if (tbWry.Rows.Count > 0)
                            dtWry = tbWry.Rows[0][0].ToString();

                        int score = 0;
                        String subItem = "";
                        if (dtWry == null)
                        {
                            score = 0;
                            subItem = "无河涌相关水质数据，加0分";
                        }
                        else if (dtWry.Equals("好转"))
                        {
                            score = 12;
                            subItem = "水质好转，加12分";
                        }
                        else if (dtWry.Equals("持平"))
                        {
                            score = 5;
                            subItem = "水质持平，加5分";
                        }
                        else if (dtWry.Equals("恶化"))
                        {
                            downItem++;

                            score = -12;
                            subItem = "水质恶化，减12分";
                        }

                        String id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        //巡河率=====================================
                        score = 0;
                        item = "巡河率";
                        sql = "select nvl(count(tm),0) nums from (select userid,to_char(strtime,'yyyy-MM-dd') tm,count(patrolid) nums from ptl_patrol_r where duration>=10 "
                            + "and strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and userid='" + userId + "' "
                            + "group by userid,to_char(strtime,'yyyy-MM-dd') order by tm asc)";

                        int xunHeDay = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        double xunheRate = Convert.ToDouble(xunHeDay) / 1.0;
                        if (xunheRate == 0)
                        {
                            downItem++;

                            score = -2;
                            subItem = "X = 0 ，扣2分";
                        }
                        else if (xunheRate >= 1)
                        {
                            score = 2;
                            subItem = "X = 100%,加2分";
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;
                        //河长积分=============================
                        score = 0;
                        item = "河长积分(统计日上一周的积分X)";
                        //select sum(change_score) from user_score_r where user_id='60000059' and create_time>=to_date('2018-06-25 00:00:00','yyyy-MM-dd HH24:mi:ss') and create_time<=to_date('2018-07-01 23:59:59','yyyy-MM-dd HH24:mi:ss')
                        sql = "select nvl(sum(change_score),0) score from user_score_r where user_id='" + userId + "' and create_time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

                        double scoreRiver = Convert.ToDouble(SqlHelper.GetDataTable(sql, null).Rows[0][0].ToString()) / 7.0;

                        if (scoreRiver <= 0)
                        {
                            downItem++;

                            score = -5;
                            subItem = "X <= 0,扣5分";
                        }
                        else if (scoreRiver > 0 && scoreRiver <= 3.5)
                        {
                            score = 1;
                            subItem = "0 < X <= 3.5,加1分";
                        }
                        else if (scoreRiver > 3.5 && scoreRiver <= 7)
                        {
                            score = 2;
                            subItem = "3.5 < X <= 7,加2分";
                        }
                        else if (scoreRiver > 7 && scoreRiver <= 10.5)
                        {
                            score = 3;
                            subItem = "7 < X <= 10.5,加3分";
                        }
                        else if (scoreRiver > 10.5 && scoreRiver <= 11.5)
                        {
                            score = 4;
                            subItem = "10.5 < X <= 11.5,加4分";
                        }
                        else if (scoreRiver > 11.5 && scoreRiver <= 13.5)
                        {
                            score = 5;
                            subItem = "11.5 < X <= 13.5,加5分";
                        }
                        else if (scoreRiver > 13.5 && scoreRiver <= 17.5)
                        {
                            score = 6;
                            subItem = "13.5 < X <= 17.5,加6分";
                        }
                        else if (scoreRiver > 17.5 && scoreRiver <= 19.5)
                        {
                            score = 7;
                            subItem = "17.5 < X <= 19.5,加7分";
                        }
                        else if (scoreRiver > 19.5)
                        {
                            score = 8;
                            subItem = "13.5 < X,加8分";
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;
                        //问题上报率
                        score = 0;
                        item = "问题上报率";

                        //河长上报问题总数
                        sql = "select nvl(count(problemid),0) nums from ptl_reportproblem where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and userid='" + userId + "' and sectionid='" + section_id + "'";

                        int problemSum = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        //责任河段内问题的总数
                        sql = "select nvl(count(problemid),0) nums from ptl_reportproblem where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and sectionid='" + section_id + "'";

                        int totalProblem = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        if (problemSum == 0 && totalProblem == 0)
                        {
                            subItem = "当Y = O，Z = 0，不加分不扣分";
                            score = 0;
                        }
                        else if (problemSum == 0 && totalProblem > 0)
                        {
                            downItem++;

                            subItem = "当Y = 0，若Z>0，，扣4分";
                            score = -4;
                        }
                        else if (problemSum > 0)
                        {
                            double rate = problemSum / totalProblem;
                            if (rate > 0 && rate <= 0.2)
                            {
                                subItem = "当Y>0，0% < X <= 20%，扣3分";
                                score = -3;
                            }
                            else if (rate > 0.2 && rate <= 0.4)
                            {
                                subItem = "当Y>0，20% < X <= 40%，扣2分";
                                score = -2;
                            }
                            else if (rate > 0.4 && rate <= 0.5)
                            {
                                subItem = "当Y>0，40% < X <= 50%,扣2分";
                                score = -2;
                            }
                            else if (rate > 0.5 && rate <= 0.7)
                            {
                                subItem = "当Y>0，50% < X <= 70%，加2分";
                                score = 2;
                            }
                            else if (rate > 0.7 && rate <= 0.8)
                            {
                                subItem = "当Y>0，70% < X <= 80%,加3分";
                                score = 3;
                            }
                            else if (rate > 0.8 && rate <= 0.9)
                            {
                                subItem = "当Y>0，80% < X <= 90%,加3分";
                                score = 3;
                            }
                            else if (rate > 0.9)
                            {
                                subItem = "当Y>0，X > 90%,加4分";
                                score = 4;
                            }
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;
                        //重大问题上报数
                        score = 0;
                        item = "重大问题上报数";

                        //河长发现重大问题数
                        sql = "select nvl(count(id),0) nums from ptl_problem_paper_assign where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + "and problem_id in(select problemid from ptl_reportproblem where sectionid='" + section_id + "' and userid='" + userId + "')";

                        int riverCheifFindProblem = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);
                        //市民、市巡查发现重大问题数
                        sql = "select count(id) nums from ptl_problem_paper_assign where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + "and problem_id in(select problemid from ptl_reportproblem where sectionid='" + section_id + "' and (pro_resource='2' or userid in(select id from sys_user_b where river_type='21')))";

                        int cityAndCheckProblem = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        int submitNum = riverCheifFindProblem - cityAndCheckProblem;

                        if (submitNum <= -2)
                        {
                            downItem++;

                            score = -4;
                            subItem = "X <= -2,扣4分";
                        }
                        else if (submitNum == -1)
                        {
                            score = -3;
                            subItem = "X = -1，扣3分";
                        }
                        else if (submitNum == -1)
                        {
                            score = -3;
                            subItem = "X = -1,扣3分";
                        }
                        else if (submitNum == 0)
                        {
                            score = 0;
                            subItem = "X = 0,不加分不扣分";
                        }
                        else if (submitNum == 1)
                        {
                            score = 3;
                            subItem = "X = 1,加3分";
                        }
                        else if (submitNum >= 2)
                        {
                            score = 4;
                            subItem = "X = 2,加4分";
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        //下级河长
                        score = 0;
                        item = "下级河长";

                        //查询有无下级河长
                        sql = "select count(distinct user_id) person_nums from rm_riverchief_section_r where del_flags='0' and river_section_id in "
                            + "(select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userId + "' and is_old='0' and del_flags='0')) "
                            + "and user_id in(select user_id from user_score_b_187)";

                        int next_chiefCount = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        if (next_chiefCount <= 0)
                        {
                            subItem = "无下级河长，不加分不减分";
                            score = 0;
                        }
                        else
                        {

                            sql = "select round(nvl(avg(score+60),0),1) score from user_score_b_187 where user_id in "
                                + "( "
                                + "    select distinct user_id from rm_riverchief_section_r where del_flags='0' and river_section_id in "
                                + "    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userId + "' and is_old='0' and del_flags='0'))"
                                + "    and user_id in(select user_id from user_score_b_187)"
                                + ")";

                            double nextRiverChiefScore = Convert.ToDouble(SqlHelper.GetDataTable(sql, null).Rows[0][0].ToString());

                            if (nextRiverChiefScore >= 0 && nextRiverChiefScore <= 30)
                            {
                                downItem++;

                                subItem = "0<= X <= 30，扣6分";
                                score = -6;
                            }
                            else if (nextRiverChiefScore > 30 && nextRiverChiefScore <= 60)
                            {
                                subItem = "30 < X <= 60,扣4分";
                                score = -4;
                            }
                            else if (nextRiverChiefScore > 60 && nextRiverChiefScore <= 90)
                            {
                                subItem = "60 < X <= 90,加4分";
                                score = 4;
                            }
                            else if (nextRiverChiefScore > 60 && nextRiverChiefScore <= 90)
                            {
                                subItem = "X > 90,加10分";
                                score = 10;
                            }
                        }

                        id = Guid.NewGuid().ToString();
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        //红黑榜
                        score = 0;
                        item = "红黑榜";

                        //红榜
                        sql = "select nvl(count(id),0) nums from info_news_redblack_user where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + " and user_id='" + userId + "' and type='0'";

                        int redNums = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        sql = "select nvl(count(id),0) nums from info_news_redblack_user where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + " and user_id='" + userId + "' and type='1'";

                        int blackNums = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        score = redNums * 10 - blackNums * 10;

                        subItem = "本周上红榜" + redNums + "次，上黑榜" + blackNums + "次";

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_B_187 (ID,USER_ID,CYCLE,SCORE,CREATE_DATE,SCORE_LEVEL,RIVER_ID,SECTION_ID) values "
                                + "('" + b_id + "','" + userId + "','" + week + "'," + total + ",sysdate," + downItem + ",'" + river_id + "','" + section_id + "')";

                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                    }
                    else if (!hc152.Equals("n"))//152条黑臭
                    {
                        scoreType = "0";

                        //污染贡献量
                        String item = "污染贡献量";
                        sql = "select CHANGE_TREND from rm_river_rated where river_id='" + river_id + "' and CREATE_DATE=(select max(CREATE_DATE) from rm_river_rated where river_id='" + river_id + "')";

                        String dtWry = null;
                        DataTable tbWry = SqlHelper.GetDataTable(sql, null);
                        if (tbWry.Rows.Count > 0)
                            dtWry = tbWry.Rows[0][0].ToString();

                        int score = 0;
                        String subItem = "";
                        if (dtWry == null)
                        {
                            score = 0;
                            subItem = "无河涌相关水质数据，加0分";
                        }
                        else if (dtWry.Equals("好转"))
                        {
                            score = 4;
                            subItem = "水质持平，加4分";
                        }
                        else if (dtWry.Equals("持平"))
                        {
                            score = 2;
                            subItem = "水质持平，加2分";
                        }
                        else if (dtWry.Equals("恶化"))
                        {
                            downItem++;

                            score = -4;
                            subItem = "水质恶化，减4分";
                        }

                        String id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        //巡河率=====================================
                        score = 0;
                        item = "巡河率";
                        sql = "select nvl(count(tm),0) nums from (select userid,to_char(strtime,'yyyy-MM-dd') tm,count(patrolid) nums from ptl_patrol_r where duration>=10 "
                            + "and strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and userid='" + userId + "' "
                            + "group by userid,to_char(strtime,'yyyy-MM-dd') order by tm asc)";

                        int xunHeDay = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        double xunheRate = Convert.ToDouble(xunHeDay) / 1.0;
                        if (xunheRate == 0)
                        {
                            downItem++;

                            score = -2;
                            subItem = "X = 0 ，扣2分";
                        }
                        else if (xunheRate >= 1)
                        {
                            score = 2;
                            subItem = "X = 100%,加2分";
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;
                        //河长积分========================
                        score = 0;
                        item = "河长积分(统计日上一周的积分X)";
                        //select sum(change_score) from user_score_r where user_id='60000059' and create_time>=to_date('2018-06-25 00:00:00','yyyy-MM-dd HH24:mi:ss') and create_time<=to_date('2018-07-01 23:59:59','yyyy-MM-dd HH24:mi:ss')
                        sql = "select nvl(sum(change_score),0) score from user_score_r where user_id='" + userId + "' and create_time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

                        double scoreRiver = Convert.ToDouble(SqlHelper.GetDataTable(sql, null).Rows[0][0].ToString()) / 7.0;

                        if (scoreRiver <= 0)
                        {
                            downItem++;

                            score = -4;
                            subItem = "X <= 0,扣4分";
                        }
                        else if (scoreRiver > 0 && scoreRiver <= 3.5)
                        {
                            score = 1;
                            subItem = "0 < X <= 3.5,加1分";
                        }
                        else if (scoreRiver > 3.5 && scoreRiver <= 7)
                        {
                            score = 3;
                            subItem = "3.5 < X <= 7,加3分";
                        }
                        else if (scoreRiver > 7 && scoreRiver <= 10.5)
                        {
                            score = 5;
                            subItem = "7 < X <= 10.5,加5分";
                        }
                        else if (scoreRiver > 10.5 && scoreRiver <= 11.5)
                        {
                            score = 7;
                            subItem = "10.5 < X <= 11.5,加7分";
                        }
                        else if (scoreRiver > 11.5 && scoreRiver <= 13.5)
                        {
                            score = 8;
                            subItem = "11.5 < X <= 13.5,加8分";
                        }
                        else if (scoreRiver > 13.5 && scoreRiver <= 17.5)
                        {
                            score = 9;
                            subItem = "13.5 < X <= 17.5,加9分";
                        }
                        else if (scoreRiver > 17.5 && scoreRiver <= 19.5)
                        {
                            score = 10;
                            subItem = "17.5 < X <= 19.5,加10分";
                        }
                        else if (scoreRiver > 19.5)
                        {
                            score = 12;
                            subItem = "13.5 < X,加12分";
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;
                        //问题上报率
                        score = 0;
                        item = "问题上报率";

                        //河长上报问题总数
                        //sql = "select nvl(count(problemid),0) nums from ptl_reportproblem where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                        //    + " and userid='" + userId + "' and sectionid='" + section_id + "'";
                        sql = "select nvl(count(problemid),0) nums from ptl_reportproblem where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and userid='" + userId + "' and sectionid in(select id from rm_river_lake where parent_id='" + section_id + "')";

                        int problemSum = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        //责任河段内问题的总数
                        sql = "select nvl(count(problemid),0) nums from ptl_reportproblem where time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and sectionid in(select id from rm_river_lake where parent_id='" + section_id + "')";

                        int totalProblem = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        if (problemSum == 0 && totalProblem == 0)
                        {
                            subItem = "当Y = O，Z = 0，不加分不扣分";
                            score = 0;
                        }
                        else if (problemSum == 0 && totalProblem > 0)
                        {
                            downItem++;

                            subItem = "当Y = 0，若Z>0，，扣4分";
                            score = -4;
                        }
                        else if (problemSum > 0)
                        {
                            double rate = Convert.ToDouble(problemSum) / Convert.ToDouble(totalProblem);
                            if (rate > 0 && rate <= 0.3)
                            {
                                subItem = "当Y>0，0% < X <= 30%，扣3分";
                                score = -3;
                            }
                            else if (rate > 0.3 && rate <= 0.5)
                            {
                                subItem = "当Y>0，30% < X <= 50%，扣1分";
                                score = -1;
                            }
                            else if (rate > 0.5 && rate <= 0.7)
                            {
                                subItem = "当Y>0，40% < X <= 50%,加1分";
                                score = 1;
                            }
                            else if (rate > 0.7 && rate <= 0.9)
                            {
                                subItem = "当Y>0，70% < X <= 90%，加3分";
                                score = 2;
                            }
                            else if (rate > 0.9)
                            {
                                subItem = "当Y>0，X > 90%,加4分";
                                score = 4;
                            }
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;
                        //重大问题上报数
                        score = 0;
                        item = "重大问题上报数";

                        //河长发现重大问题数
                        sql = "select nvl(count(id),0) nums from ptl_problem_paper_assign where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + "and problem_id in(select problemid from ptl_reportproblem where sectionid='" + section_id + "' and userid='" + userId + "')";

                        int riverCheifFindProblem = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);
                        //市民、市巡查发现重大问题数
                        sql = "select count(id) nums from ptl_problem_paper_assign where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + "and problem_id in(select problemid from ptl_reportproblem where sectionid='" + section_id + "' and (pro_resource='2' or userid in(select id from sys_user_b where river_type='21')))";

                        int cityAndCheckProblem = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        int submitNum = riverCheifFindProblem - cityAndCheckProblem;

                        if (submitNum <= -2)
                        {
                            downItem++;

                            score = -4;
                            subItem = "X <= -2,扣4分";
                        }
                        else if (submitNum == -1)
                        {
                            score = -3;
                            subItem = "X = -1，扣3分";
                        }
                        else if (submitNum == -1)
                        {
                            score = -3;
                            subItem = "X = -1,扣3分";
                        }
                        else if (submitNum == 0)
                        {
                            score = 0;
                            subItem = "X = 0,不加分不扣分";
                        }
                        else if (submitNum == 1)
                        {
                            score = 3;
                            subItem = "X = 1,加3分";
                        }
                        else if (submitNum >= 2)
                        {
                            score = 4;
                            subItem = "X = 2,加4分";
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        //下级河长
                        score = 0;
                        item = "下级河长";
                        
                        //查询有无下级河长
                        sql = "select count(distinct user_id) person_nums from rm_riverchief_section_r where del_flags='0' and river_section_id in "
                            + "(select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userId + "' and is_old='0' and del_flags='0')) "
                            + "and user_id in(select user_id from user_score_b_187 and cycle='" + week + "')";

                        int next_chiefCount = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        if (next_chiefCount <= 0)
                        {
                            subItem = "无下级河长，不加分不减分";
                            score = 0;
                        }
                        else
                        {
                            sql = "select round(nvl(avg(score+60),0),1) score from user_score_b_187 where user_id in "
                                + "( "
                                + "    select distinct user_id from rm_riverchief_section_r where del_flags='0' and river_section_id in "
                                + "    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userId + "' and is_old='0' and del_flags='0'))"
                                + "    and user_id in(select user_id from user_score_b_187)"
                                + ") and cycle='" + week + "'";

                            double nextRiverChiefScore = Convert.ToDouble(SqlHelper.GetDataTable(sql, null).Rows[0][0].ToString());

                            if (nextRiverChiefScore >= 0 && nextRiverChiefScore <= 30)
                            {
                                downItem++;

                                subItem = "0<= X <= 30，扣6分";
                                score = -6;
                            }
                            else if (nextRiverChiefScore > 30 && nextRiverChiefScore <= 60)
                            {
                                subItem = "30 < X <= 60,扣4分";
                                score = -4;
                            }
                            else if (nextRiverChiefScore > 60 && nextRiverChiefScore <= 90)
                            {
                                subItem = "60 < X <= 90,加10分";
                                score = 10;
                            }
                            else if (nextRiverChiefScore > 90)
                            {
                                subItem = "X > 90,加14分";
                                score = 14;
                            }
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        //红黑榜
                        score = 0;
                        item = "红黑榜";

                        //红榜
                        sql = "select nvl(count(id),0) nums from info_news_redblack_user where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + " and user_id='" + userId + "' and type='0'";

                        int redNums = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        sql = "select nvl(count(id),0) nums from info_news_redblack_user where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + " and user_id='" + userId + "' and type='1'";

                        int blackNums = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        score = redNums * 10 - blackNums * 10;

                        subItem = "本周上红榜" + redNums + "次，上黑榜" + blackNums + "次";

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_B_187 (ID,USER_ID,CYCLE,SCORE,CREATE_DATE,SCORE_LEVEL,RIVER_ID,SECTION_ID) values "
                                + "('" + b_id + "','" + userId + "','" + week + "'," + total + ",sysdate," + downItem + ",'" + river_id + "','" + section_id + "')";

                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                    }
                }
                bgWork.ReportProgress(i + 1);
            }
        }

        private void btnStartArea_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgWorkArea.DoWork += new DoWorkEventHandler(worker_DoWorkArea);
            bgWorkArea.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgWorkArea.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgWorkArea.RunWorkerAsync();
        }

        //区级河长
        void worker_DoWorkArea(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(区级河长)...";
            writeInfo("数据处理中(区级河长)...");

            run187_area(progressBar1, bgWorkArea);

            lblInfo.Text = "数据处理完成(区级河长)...";
            writeInfo("数据处理完成(区级河长)...");

            if (auto_mode)
            {
                //继续跑河长详情
                button2_Click(null, null);
            }
        }

        private void run187_area(ProgressBar pBar, BackgroundWorker bgWork)
        {
            //String startTm = "2018-06-18 00:00:00";
            //String endTm = "2018-06-24 23:59:59";

            String sql = "";
            //sql = "delete from USER_SCORE_B_187 where id in(select id from USER_SCORE_R_187 where START_DATE=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_DATE=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss'))";

            //SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

            //sql = "delete from USER_SCORE_R_187 where START_DATE=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_DATE=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";

            //SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

            sql = "select id from sys_user_b where river_type in('7','8','9') and del_flag='0'"; // and id='3ad82a04abe84702925ce2fef35be9a9'
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userId = dt.Rows[i]["id"].ToString();

                //查询用户责任河涌
                sql = "select distinct rivet_rm_id,river_section_id,nvl(is_hc152,'n') is_hc152,nvl(is_hc35,'n') is_hc35 "
                    + "from RM_RIVERCHIEF_SECTION_R t_r left join rm_river_lake t_river on t_r.river_section_id=t_river.id "
                    + "where t_r.user_id='" + userId + "' and t_r.is_old='0' and t_r.del_flags='0' and t_r.rivet_rm_id in "
                    + "(select id from rm_river_lake where is_hc152 is not null or is_hc35 is not null)";

                DataTable dtRiver = SqlHelper.GetDataTable(sql, null);

                String week = startTm.Substring(0, 4) + CommonUtils.GetWeekOfYear(Convert.ToDateTime(startTm));

                for (int j = 0; j < dtRiver.Rows.Count; j++)
                {
                    int downItem = 0;
                    String b_id = Guid.NewGuid().ToString();

                    String river_id = dtRiver.Rows[j]["rivet_rm_id"].ToString();
                    String section_id = dtRiver.Rows[j]["river_section_id"].ToString();

                    String hc35 = dtRiver.Rows[j]["is_hc35"].ToString();
                    String hc152 = dtRiver.Rows[j]["is_hc152"].ToString();

                    String scoreType = "1";
                    double total = 0;//60;
                    if (!hc35.Equals("n")) //35条不黑不臭
                    {
                        scoreType = "1";
                        //污染贡献量====================================
                        String item = "污染贡献量";
                        sql = "select CHANGE_TREND from rm_river_rated where river_id='" + river_id + "' and CREATE_DATE=(select max(CREATE_DATE) from rm_river_rated where river_id='" + river_id + "')";

                        String dtWry = null;
                        DataTable tbWry = SqlHelper.GetDataTable(sql, null);

                        if (tbWry.Rows.Count > 0)
                            dtWry = tbWry.Rows[0][0].ToString();

                        int score = 0;
                        String subItem = "";

                        if (dtWry == null)
                        {
                            score = 0;
                            subItem = "无此河涌水质数据，加0分";
                        }
                        else if (dtWry.Equals("好转"))
                        {
                            score = 12;
                            subItem = "水质好转，加14分";
                        }
                        else if (dtWry.Equals("持平"))
                        {
                            score = 6;
                            subItem = "水质持平，加7分";
                        }
                        else if (dtWry.Equals("恶化"))
                        {
                            downItem++;

                            score = -12;
                            subItem = "水质恶化，减14分";
                        }

                        String id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        //巡河率====================================================
                        score = 0;
                        item = "巡河率";

                        String patrolStart_time = "";
                        String patrolEnd_time = "";
                        if (dtpStart.Value.Month == 1 || dtpStart.Value.Month == 2)
                        {
                            patrolStart_time = dtpStart.Value.Year + "-01-01 00:00:00";
                            patrolEnd_time = dtpStart.Value.Year + "-03-01 00:00:00";
                        }
                        else if (dtpStart.Value.Month == 3 || dtpStart.Value.Month == 4)
                        {
                            patrolStart_time = dtpStart.Value.Year + "-03-01 00:00:00";
                            patrolEnd_time = dtpStart.Value.Year + "-05-01 00:00:00";
                        }
                        else if (dtpStart.Value.Month == 5 || dtpStart.Value.Month == 6)
                        {
                            patrolStart_time = dtpStart.Value.Year + "-05-01 00:00:00";
                            patrolEnd_time = dtpStart.Value.Year + "-07-01 00:00:00";
                        }
                        else if (dtpStart.Value.Month == 7 || dtpStart.Value.Month == 8)
                        {
                            patrolStart_time = dtpStart.Value.Year + "-07-01 00:00:00";
                            patrolEnd_time = dtpStart.Value.Year + "-09-01 00:00:00";
                        }
                        else if (dtpStart.Value.Month == 9 || dtpStart.Value.Month == 10)
                        {
                            patrolStart_time = dtpStart.Value.Year + "-09-01 00:00:00";
                            patrolEnd_time = dtpStart.Value.Year + "-11-01 00:00:00";
                        }
                        else if (dtpStart.Value.Month == 11 || dtpStart.Value.Month == 12)
                        {
                            patrolStart_time = dtpStart.Value.Year + "-11-01 00:00:00";
                            patrolEnd_time = (dtpStart.Value.Year + 1) + "-01-01 00:00:00";
                        }
                        sql = "select nvl(count(tm),0) nums from (select userid,to_char(strtime,'yyyy-MM-dd') tm,count(patrolid) nums from ptl_patrol_r where duration>=10 "
                            + "and strtime>=to_date('" + patrolStart_time + "','yyyy-MM-dd HH24:mi:ss') and strtime<to_date('" + patrolEnd_time + "','yyyy-MM-dd HH24:mi:ss') and userid='" + userId + "' "
                            + "group by userid,to_char(strtime,'yyyy-MM-dd') order by tm asc)";

                        int xunHeDay = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        double xunheRate = Convert.ToDouble(xunHeDay) / 1.0;
                        if (xunheRate == 0)
                        {
                            downItem++;

                            score = -2;
                            subItem = "X = 0 ，扣2分";
                        }
                        else
                        {
                            score = 2;
                            subItem = "X = 100%，加2分";
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        //问题上报率======================================================
                        score = 0;
                        item = "问题完结率";

                        //本周已完结问题
                        sql = "select count(problemid) nums from ptl_reportproblem where sectionid in("
                            + "    select distinct id from rm_river_lake where parent_ids like '%" + section_id + "%' or id='" + section_id + "'"
                            + ") and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                            + "and state='4'";

                        int endProblemNums = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        //本周总问题数
                        sql = "select count(problemid) nums from ptl_reportproblem where sectionid in("
                            + "    select distinct id from rm_river_lake where parent_ids like '%" + section_id + "%' or id='" + section_id + "'"
                            + ") and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

                        int totalProblemNums = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        double endRate = 0;
                        if (totalProblemNums == 0 || endProblemNums == 0)
                            endRate = 0;
                        else
                            endRate = endProblemNums / totalProblemNums;
                        if (endRate <= 0.4)
                        {
                            downItem++;

                            subItem = "X <=40%,加0分";
                            score = 0;
                        }
                        else if (endRate > 0.4 && endRate <= 0.5)
                        {
                            subItem = "40% < X <= 50%,加2分";
                            score = 2;
                        }
                        else if (endRate > 0.5 && endRate <= 0.6)
                        {
                            subItem = "50% < X <= 60%,加4分";
                            score = 4;
                        }
                        else if (endRate > 0.6 && endRate <= 0.9)
                        {
                            subItem = "60% < X <= 90%，加7分";
                            score = 7;
                        }
                        else if (endRate > 0.9)
                        {
                            subItem = "90% < X，加8分";
                            score = 8;
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        //下级河长============================
                        score = 0;
                        item = "下级河长";

                        //查询有无下级河长
                        sql = "select count(distinct user_id) person_nums from rm_riverchief_section_r where del_flags='0' and river_section_id in "
                            + "(select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userId + "' and is_old='0' and del_flags='0')) "
                            + "and user_id in(select user_id from user_score_b_187)";

                        int next_chiefCount = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        if (next_chiefCount <= 0)
                        {
                            subItem = "无下级河长，不加分不减分";
                            score = 0;
                        }
                        else
                        {
                            sql = "select round(nvl(avg(score+60),0),1) score from user_score_b_187 where user_id in "
                                + "( "
                                + "    select distinct user_id from rm_riverchief_section_r where del_flags='0' and river_section_id in "
                                + "    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userId + "' and is_old='0' and del_flags='0'))"
                                + "    and user_id in(select user_id from user_score_b_187)"
                                + ")";

                            double nextRiverChiefScore = Convert.ToDouble(SqlHelper.GetDataTable(sql, null).Rows[0][0].ToString());

                            if (nextRiverChiefScore >= 0 && nextRiverChiefScore <= 30)
                            {
                                downItem++;

                                subItem = "0<= X <= 30，加5分";
                                score = 5;
                            }
                            else if (nextRiverChiefScore > 30 && nextRiverChiefScore <= 50)
                            {
                                subItem = "30 < X <= 50,加8分";
                                score = 8;
                            }
                            else if (nextRiverChiefScore > 50 && nextRiverChiefScore <= 70)
                            {
                                subItem = "50 < X <= 70,加14分";
                                score = 14;
                            }
                            else if (nextRiverChiefScore > 70 && nextRiverChiefScore <= 90)
                            {
                                subItem = "70 < X <= 90,加16分";
                                score = 16;
                            }
                            else if (nextRiverChiefScore > 90)
                            {
                                subItem = "X > 90,加18分";
                                score = 18;
                            }
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        //红黑榜===========================
                        score = 0;
                        item = "红黑榜";

                        //红榜
                        sql = "select nvl(count(id),0) nums from info_news_redblack_user where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + " and user_id='" + userId + "' and type='0'";

                        int redNums = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        sql = "select nvl(count(id),0) nums from info_news_redblack_user where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + " and user_id='" + userId + "' and type='1'";

                        int blackNums = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        score = redNums * 10 - blackNums * 10;

                        subItem = "本周上红榜" + redNums + "次，上黑榜" + blackNums + "次";

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_B_187 (ID,USER_ID,CYCLE,SCORE,CREATE_DATE,SCORE_LEVEL,RIVER_ID,SECTION_ID) values "
                                + "('" + b_id + "','" + userId + "','" + week + "'," + total + ",sysdate," + downItem + ",'" + river_id + "','" + section_id + "')";

                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                    }
                    else if (!hc152.Equals("n"))//152条黑臭
                    {
                        scoreType = "0";

                        //污染贡献量
                        String item = "污染贡献量";
                        sql = "select CHANGE_TREND from rm_river_rated where river_id='" + river_id + "' and CREATE_DATE=(select max(CREATE_DATE) from rm_river_rated where river_id='" + river_id + "')";

                        String dtWry = null;
                        DataTable tbWry = SqlHelper.GetDataTable(sql, null);

                        if (tbWry.Rows.Count > 0)
                            dtWry = tbWry.Rows[0][0].ToString();

                        int score = 0;
                        String subItem = "";
                        if (dtWry == null)
                        {
                            score = 0;
                            subItem = "无此河涌水质数据，加0分";
                        }
                        else if (dtWry.Equals("好转"))
                        {
                            score = 4;
                            subItem = "水质好转，加6分";
                        }
                        else if (dtWry.Equals("持平"))
                        {
                            score = -3;
                            subItem = "水质持平，减3分";
                        }
                        else if (dtWry.Equals("恶化"))
                        {
                            downItem++;

                            score = -4;
                            subItem = "水质恶化，减6分";
                        }

                        String id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        //巡河率====================================================
                        score = 0;
                        item = "巡河率";

                        String patrolStart_time = "";
                        String patrolEnd_time = "";
                        if (dtpStart.Value.Month == 1 || dtpStart.Value.Month == 2)
                        {
                            patrolStart_time = dtpStart.Value.Year + "-01-01 00:00:00";
                            patrolEnd_time = dtpStart.Value.Year + "-03-01 00:00:00";
                        }
                        else if (dtpStart.Value.Month == 3 || dtpStart.Value.Month == 4)
                        {
                            patrolStart_time = dtpStart.Value.Year + "-03-01 00:00:00";
                            patrolEnd_time = dtpStart.Value.Year + "-05-01 00:00:00";
                        }
                        else if (dtpStart.Value.Month == 5 || dtpStart.Value.Month == 6)
                        {
                            patrolStart_time = dtpStart.Value.Year + "-05-01 00:00:00";
                            patrolEnd_time = dtpStart.Value.Year + "-07-01 00:00:00";
                        }
                        else if (dtpStart.Value.Month == 7 || dtpStart.Value.Month == 8)
                        {
                            patrolStart_time = dtpStart.Value.Year + "-07-01 00:00:00";
                            patrolEnd_time = dtpStart.Value.Year + "-09-01 00:00:00";
                        }
                        else if (dtpStart.Value.Month == 9 || dtpStart.Value.Month == 10)
                        {
                            patrolStart_time = dtpStart.Value.Year + "-09-01 00:00:00";
                            patrolEnd_time = dtpStart.Value.Year + "-11-01 00:00:00";
                        }
                        else if (dtpStart.Value.Month == 11 || dtpStart.Value.Month == 12)
                        {
                            patrolStart_time = dtpStart.Value.Year + "-11-01 00:00:00";
                            patrolEnd_time = (dtpStart.Value.Year + 1) + "-01-01 00:00:00";
                        }
                        sql = "select nvl(count(tm),0) nums from (select userid,to_char(strtime,'yyyy-MM-dd') tm,count(patrolid) nums from ptl_patrol_r where duration>=10 "
                            + "and strtime>=to_date('" + patrolStart_time + "','yyyy-MM-dd HH24:mi:ss') and strtime<to_date('" + patrolEnd_time + "','yyyy-MM-dd HH24:mi:ss') and userid='" + userId + "' "
                            + "group by userid,to_char(strtime,'yyyy-MM-dd') order by tm asc)";

                        int xunHeDay = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        double xunheRate = Convert.ToDouble(xunHeDay) / 1.0;
                        if (xunheRate == 0)
                        {
                            downItem++;

                            score = -2;
                            subItem = "X = 0 ，扣2分";
                        }
                        else
                        {
                            score = 2;
                            subItem = "X = 100%，加2分";
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        //问题上报率===========================
                        score = 0;
                        item = "问题完结率";

                        //本周已完结问题
                        sql = "select count(problemid) nums from ptl_reportproblem where sectionid in("
                            + "    select distinct id from rm_river_lake where parent_ids like '%" + section_id + "%' or id='" + section_id + "'"
                            + ") and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')"
                            + "and state='4'";

                        int endProblemNums = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        //本周总问题数
                        sql = "select count(problemid) nums from ptl_reportproblem where sectionid in("
                            + "    select distinct id from rm_river_lake where parent_ids like '%" + section_id + "%' or id='" + section_id + "'"
                            + ") and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss')";

                        int totalProblemNums = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        double endRate = 0;
                        if (totalProblemNums == 0 || endProblemNums == 0)
                            endRate = 0;
                        else
                            endRate = endProblemNums / totalProblemNums;
                        if (endRate <= 0.4)
                        {
                            downItem++;

                            subItem = "X <=40%,加0分";
                            score = 0;
                        }
                        else if (endRate > 0.4 && endRate <= 0.5)
                        {
                            subItem = "40% < X <= 50%,加2分";
                            score = 2;
                        }
                        else if (endRate > 0.5 && endRate <= 0.6)
                        {
                            subItem = "50% < X <= 60%,加4分";
                            score = 4;
                        }
                        else if (endRate > 0.6 && endRate <= 0.9)
                        {
                            subItem = "60% < X <= 90%，加7分";
                            score = 7;
                        }
                        else if (endRate > 0.9)
                        {
                            subItem = "90% < X，加10分";
                            score = 10;
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        //下级河长==========================
                        score = 0;
                        item = "下级河长";

                        //查询有无下级河长
                        sql = "select count(distinct user_id) person_nums from rm_riverchief_section_r where del_flags='0' and river_section_id in "
                            + "(select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userId + "' and is_old='0')) "
                            + "and user_id in(select user_id from user_score_b_187)";

                        int next_chiefCount = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        if (next_chiefCount <= 0)
                        {
                            subItem = "无下级河长，不加分不减分";
                            score = 0;
                        }
                        else
                        {
                            sql = "select round(nvl(avg(score+60),0),1) score from user_score_b_187 where user_id in "
                                + "( "
                                + "    select distinct user_id from rm_riverchief_section_r where del_flags='0' and river_section_id in "
                                + "    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userId + "' and is_old='0' and del_flags='0'))"
                                + "    and user_id in(select user_id from user_score_b_187)"
                                + ")";

                            double nextRiverChiefScore = Convert.ToDouble(SqlHelper.GetDataTable(sql, null).Rows[0][0].ToString());

                            if (nextRiverChiefScore >= 0 && nextRiverChiefScore <= 30)
                            {
                                downItem++;

                                subItem = "0<= X <= 30，加3分";
                                score = 3;
                            }
                            else if (nextRiverChiefScore > 30 && nextRiverChiefScore <= 50)
                            {
                                subItem = "30 < X <= 50,加7分";
                                score = 7;
                            }
                            else if (nextRiverChiefScore > 50 && nextRiverChiefScore <= 70)
                            {
                                subItem = "50 < X <= 70,加10分";
                                score = 10;
                            }
                            else if (nextRiverChiefScore > 70 && nextRiverChiefScore <= 90)
                            {
                                subItem = "70 < X <= 90,加18分";
                                score = 18;
                            }
                            else if (nextRiverChiefScore > 90)
                            {
                                subItem = "X > 90,加24分";
                                score = 24;
                            }
                        }

                        id = Guid.NewGuid().ToString();
                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        //红黑榜============================
                        score = 0;
                        item = "红黑榜";

                        //红榜
                        sql = "select nvl(count(id),0) nums from info_news_redblack_user where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + " and user_id='" + userId + "' and type='0'";

                        int redNums = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        sql = "select nvl(count(id),0) nums from info_news_redblack_user where create_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') "
                            + " and user_id='" + userId + "' and type='1'";

                        int blackNums = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                        score = redNums * 10 - blackNums * 10;

                        subItem = "本周上红榜" + redNums + "次，上黑榜" + blackNums + "次";

                        id = Guid.NewGuid().ToString();
                        {
                            sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                                + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }

                        total += score;

                        if (if_delAndInDB)
                        {
                            sql = "Insert into USER_SCORE_B_187 (ID,USER_ID,CYCLE,SCORE,CREATE_DATE,SCORE_LEVEL,RIVER_ID,SECTION_ID) values "
                                + "('" + b_id + "','" + userId + "','" + week + "'," + total + ",sysdate," + downItem + ",'" + river_id + "','" + section_id + "')";

                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                    }
                }

                bgWork.ReportProgress(i + 1);
            }
        }

        private void chkDelAndInDB_CheckedChanged(object sender, EventArgs e)
        {
            if_delAndInDB = chkDelAndInDB.Checked;
        }

        //（区级单条河涌详情）APPWEEK_RIVER_RM_INFO
        private void btn_single_rm_info_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgSingleRmInfo.DoWork += new DoWorkEventHandler(worker_DoWorkSingleRmInfo);
            bgSingleRmInfo.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgSingleRmInfo.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgSingleRmInfo.RunWorkerAsync();
        }

        //（区级单条河涌详情）
        void worker_DoWorkSingleRmInfo(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(区级单条河涌详情:APPWEEK_RIVER_RM_INFO)...";
            writeInfo(lblInfo.Text);

            runSingleRmInfo(progressBar1, bgSingleRmInfo);

            lblInfo.Text = "数据处理完成(区级单条河涌详情:APPWEEK_RIVER_RM_INFO)...";
            writeInfo(lblInfo.Text);

            if (auto_mode)
            {
                //继续跑 我的履职评价（187河）APPWEEK_RIVER_MY_SCORE
                btn_single_my_score_Click(null, null);
            }
        }

        public void runSingleRmInfo(ProgressBar pBar, BackgroundWorker bgWork)
        {
            String sql = "";

            if (if_delAndInDB)
            {
                sql = "delete from APPWEEK_RIVER_RM_INFO where rm_start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and rm_end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";
                if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                    sql += " and rm_app_id='" + lblChiefID.Text.Trim() + "'";

                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
            }

            sql = "select id,name,"
                + "(select name from sys_area_b where id=addvcd_area) address"
                + " from sys_user_b where river_type in('7','8','9') and del_flag='0'";

            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "' order by river_type desc";
            else
                sql += " order by river_type desc";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String rm_app_id = dt.Rows[i]["id"].ToString();
                String rm_name = dt.Rows[i]["name"].ToString();
                String address = dt.Rows[i]["address"].ToString();

                sql = "select id from appweek_rm_info where start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')"
                    + "and rm_app_id='" + rm_app_id + "'";

                DataTable dtInfo = SqlHelper.GetDataTable(sql, null);

                if (dtInfo.Rows.Count <= 0)
                {
                    bgSingleRmInfo.ReportProgress(i + 1);
                    continue;
                }

                //河长详情id 对应RM_INFO河长详情表中的id
                String rm_info_id = dtInfo.Rows[0][0].ToString();

                //周报开始时间
                String rm_start_time = startTm;
                //周报结束时间
                String rm_end_time = endTm.Substring(0, 10) + " 00:00:00";
                  

                //查询责任河段数量
                sql = "select distinct RIVER_SECTION_ID,(select name from rm_river_lake where id=RIVER_SECTION_ID) name from rm_riverchief_section_r where user_id='" + rm_app_id + "' and is_old='0' and del_flags='0'";

                DataTable dtTableRiver = SqlHelper.GetDataTable(sql, null);
                
                //循环所有责任河段
                for (int j = 0; j < dtTableRiver.Rows.Count; j++)
                {
                    String id = Guid.NewGuid().ToString();
                    String section_id = dtTableRiver.Rows[j]["RIVER_SECTION_ID"].ToString();

                    //河段全称
                    String rm_river_name = dtTableRiver.Rows[j]["name"].ToString();
                    //该条河涌是否属于187河 0代表该河段不属于187河，1代表该河段属于187河
                    String has_187river = "0";

                    sql = "select distinct rivet_rm_id,is_hc152,is_hc35 from rm_riverchief_section_r t_r"
                        +" left join rm_river_lake river on river.id=t_r.rivet_rm_id"
                        + " where river_section_id='" + section_id + "'";

                    DataTable dtTable187River = SqlHelper.GetDataTable(sql, null);

                    if (dtTable187River.Rows[0]["is_hc152"] == DBNull.Value && dtTable187River.Rows[0]["is_hc152"] == DBNull.Value)
                        has_187river = "0";
                    else
                        has_187river = "1";

                    if (if_delAndInDB)
                    {
                        sql = "insert into APPWEEK_RIVER_RM_INFO(id,rm_app_id,rm_info_id,rm_start_time,rm_end_time,rm_name,address,has_187river,rm_river_name,river_flow_id)"
                            + " values ('" + id + "','" + rm_app_id + "','" + rm_info_id + "',to_date('" + rm_start_time + "','yyyy-MM-dd HH24:mi:ss'),to_date('" + rm_end_time + "','yyyy-MM-dd HH24:mi:ss'),'" + rm_name + "','" + address + "','" + has_187river + "','" + rm_river_name + "','" + section_id + "')";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                    }
                }

                bgWork.ReportProgress(i + 1);
            }
        }

        private void btn_single_my_score_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgSingleMyScore.DoWork += new DoWorkEventHandler(worker_DoWorkSingleMyScore);
            bgSingleMyScore.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgSingleMyScore.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgSingleMyScore.RunWorkerAsync();
        }

        //我的履职评价（187河）
        void worker_DoWorkSingleMyScore(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(我的履职评价（187河）APPWEEK_RIVER_MY_SCORE)...";
            writeInfo(lblInfo.Text);

            SingleMyScore(progressBar1, bgSingleMyScore);

            lblInfo.Text = "数据处理完成(我的履职评价（187河）APPWEEK_RIVER_MY_SCORE)...";
            writeInfo(lblInfo.Text);

            if (auto_mode)
            {
                //继续跑 我的水质变化APPWEEK_RIVER_MY_WATER_QUALITY
                btn_single_water_quality_Click(null, null);
            }
        }

        public void SingleMyScore(ProgressBar pBar, BackgroundWorker bgWork)
        {
            String sql = "";

            if (if_delAndInDB)
            {
                sql = "delete from APPWEEK_RIVER_MY_SCORE where rm_river_id in(select id from APPWEEK_RIVER_RM_INFO where rm_start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and rm_end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";
                if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                    sql += " and rm_app_id='" + lblChiefID.Text.Trim() + "')";
                else
                    sql += ")";

                int delNums = SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
            }

            sql = "select id from sys_user_b where river_type in('7','8','9') and del_flag='0'"; // and id='236c25e9fddc43f78a647e25ebc7e381'  and id='50000628'  and id='50000978'  and id='50000401'
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();

                //查询责任河段数量
                sql = "select distinct RIVER_SECTION_ID,(select name from rm_river_lake where id=RIVER_SECTION_ID) name from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0' and del_flags='0'";

                DataTable dtTableRiver = SqlHelper.GetDataTable(sql, null);
                
                //循环所有责任河段
                for (int j = 0; j < dtTableRiver.Rows.Count; j++)
                {
                    String id = Guid.NewGuid().ToString();
                    String section_id = dtTableRiver.Rows[j]["RIVER_SECTION_ID"].ToString();

                    sql = "select id from APPWEEK_RIVER_RM_INFO where rm_start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and rm_end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')"
                    + "and rm_app_id='" + userid + "' and river_flow_id='" + section_id + "'";

                    DataTable dtInfo = SqlHelper.GetDataTable(sql, null);

                    if (dtInfo.Rows.Count <= 0)
                        continue;

                    //单条河涌详情id
                    String rm_river_id = dtInfo.Rows[0][0].ToString();
                    //周报开始时间
                    String rm_start_time = startTm;
                    //周报结束时间
                    String rm_end_time = endTm.Substring(0, 10) + " 00:00:00";

                    //rm_this_week_score	本周评分
                    sql = "select sum(avg(score))+60 score from user_score_r_187 where user_id='" + userid + "' and section_id='" + section_id + "'"
                        + " and start_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') group by evaluation_item";

                    DataTable dtThisWeekScore = SqlHelper.GetDataTable(sql, null);
                    Double rm_this_week_score = dtThisWeekScore.Rows[0][0] == DBNull.Value ? 0.0 : Convert.ToDouble(dtThisWeekScore.Rows[0][0].ToString());

                    //同比上周 rise_or_decline_score
                    String lastWeekEndTime = Convert.ToDateTime(startTm).AddDays(-1).ToShortDateString()+" 00:00:00";

                    sql = "select sum(avg(score))+60 score from user_score_r_187 where user_id='" + userid + "' and section_id='" + section_id + "'"
                        + " and end_date=to_date('" + lastWeekEndTime + "','yyyy-MM-dd HH24:mi:ss') group by evaluation_item";

                    DataTable dtLastWeekScore = SqlHelper.GetDataTable(sql, null);
                    Double rm_last_week_score = dtLastWeekScore.Rows[0][0] == DBNull.Value ? 0.0 : Convert.ToDouble(dtLastWeekScore.Rows[0][0].ToString());

                    Double rise_or_decline_score = rm_this_week_score - rm_last_week_score;
                    //week_pollution_contri_score	该河段本周污染源贡献量得分
                    sql = "select score from user_score_r_187 where user_id='" + userid + "' and section_id='" + section_id + "'"
                        + " and start_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and EVALUATION_type='污染贡献量'";

                    DataTable dtWeek_pollution_contri_score = SqlHelper.GetDataTable(sql, null);
                    String week_pollution_contri_score = dtWeek_pollution_contri_score.Rows.Count <= 0 ? "0" : dtWeek_pollution_contri_score.Rows[0][0].ToString();
                    //patrol_pass_rate	巡河达标率
                    sql = "select score from user_score_r_187 where user_id='" + userid + "' and section_id='" + section_id + "'"
                        + " and start_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and EVALUATION_type='巡河率'";

                    DataTable dt_patrol_pass_rate = SqlHelper.GetDataTable(sql, null);
                    String patrol_pass_rate = dt_patrol_pass_rate.Rows.Count <= 0 ? "0" : dt_patrol_pass_rate.Rows[0][0].ToString();
                    //problem_finish_rate 问题完结率
                    sql = "select score from user_score_r_187 where user_id='" + userid + "' and section_id='" + section_id + "'"
                        + " and start_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and EVALUATION_type='问题完结率'";

                    DataTable dt_problem_finish_rate = SqlHelper.GetDataTable(sql, null);
                    String problem_finish_rate = dt_problem_finish_rate.Rows.Count <= 0 ? "0" : dt_problem_finish_rate.Rows[0][0].ToString();
                    //lower_level_situa_score	下级河长（镇）
                    sql = "select score from user_score_r_187 where user_id='" + userid + "' and section_id='" + section_id + "'"
                        + " and start_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and EVALUATION_type='下级河长'";

                    DataTable dtLevel = SqlHelper.GetDataTable(sql, null);
                    String lower_level_situa_score = dtLevel.Rows.Count <= 0 ? "0" : dtLevel.Rows[0][0].ToString();
                    //black_or_red_score	红黑榜（累加）（村镇）
                    sql = "select score from user_score_r_187 where user_id='" + userid + "' and section_id='" + section_id + "'"
                        + " and start_date>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_date<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and EVALUATION_type='红黑榜'";

                    DataTable dt_black_or_red_score = SqlHelper.GetDataTable(sql, null);
                    String black_or_red_score = dt_black_or_red_score.Rows.Count <= 0 ? "0" : dt_black_or_red_score.Rows[0][0].ToString();

                    if (if_delAndInDB)
                    {
                        sql = "insert into APPWEEK_RIVER_MY_SCORE(id, rm_river_id, rm_start_time, rm_end_time, rm_this_week_score, rise_or_decline_score, week_pollution_contri_score,"
                            + "patrol_pass_rate, problem_finish_rate, lower_level_situa_score, black_or_red_score) values('"
                            + id + "','" + rm_river_id + "',to_date('" + rm_start_time + "','yyyy-MM-dd HH24:mi:ss'),to_date('" + rm_end_time + "','yyyy-MM-dd HH24:mi:ss'),'" + rm_this_week_score + "','" + rise_or_decline_score + "','" + week_pollution_contri_score
                            + "','" + patrol_pass_rate + "','" + problem_finish_rate + "','" + lower_level_situa_score + "','" + black_or_red_score + "')";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                    }
                }

                bgWork.ReportProgress(i + 1);
            }
        }

        //我的水质变化APPWEEK_RIVER_MY_WATER_QUALITY
        private void btn_single_water_quality_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgSingleWaterQuality.DoWork += new DoWorkEventHandler(worker_DoWorkSingle_water_quality);
            bgSingleWaterQuality.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgSingleWaterQuality.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgSingleWaterQuality.RunWorkerAsync();
        }

        //我的水质变化APPWEEK_RIVER_MY_WATER_QUALITY
        void worker_DoWorkSingle_water_quality(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(区级单条河涌水质变化:APPWEEK_RIVER_MY_WATER_QUALITY)...";
            writeInfo(lblInfo.Text);

            runSingle_water_quality(progressBar1, bgSingleWaterQuality);

            lblInfo.Text = "数据处理完成(区级单条河涌水质变化:APPWEEK_RIVER_MY_WATER_QUALITY)";
            writeInfo(lblInfo.Text);

            if (auto_mode)
            {
                //继续跑 管辖河段问题发现与处理APPWEEK_RIVER_JURISDICTION_PRO
                btn_single_river_problem_Click(null, null);
            }
        }

        public void runSingle_water_quality(ProgressBar pBar, BackgroundWorker bgWork)
        {
            String sql = "";
            if (if_delAndInDB)
            {
                sql = "delete from APPWEEK_RIVER_MY_WATER_QUALITY where rm_river_id in(select id from APPWEEK_RIVER_RM_INFO where rm_start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and rm_end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";
                if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                    sql += " and rm_app_id='" + lblChiefID.Text.Trim() + "')";
                else
                    sql += ")";

                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
            }

            sql = "select id from sys_user_b where river_type in('7','8','9') and del_flag='0'"; // and id='236c25e9fddc43f78a647e25ebc7e381'  and id='50000628'
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;

            sql = "select to_char(add_months(sysdate,-4),'yyyy-MM'),to_char(add_months(sysdate,-1),'yyyy-MM') from dual";
            DataTable dtDate = SqlHelper.GetDataTable(sql, null);

            //水质开始时间和结束时间 water_start_end_time
            String water_start_end_time = dtDate.Rows[0][0].ToString().Replace('-', '年') + "月" + "-" + dtDate.Rows[0][1].ToString().Replace('-', '年') + "月";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();

                

                //查询责任河段数量
                sql = "select distinct RIVER_SECTION_ID,(select name from rm_river_lake where id=RIVER_SECTION_ID) name from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0' and del_flags='0'";

                DataTable dtTableRiver = SqlHelper.GetDataTable(sql, null);
                
                //循环所有责任河段
                for (int j = 0; j < dtTableRiver.Rows.Count; j++)
                {
                    String id = Guid.NewGuid().ToString();
                    String section_id = dtTableRiver.Rows[j]["RIVER_SECTION_ID"].ToString();

                    sql = "select id from APPWEEK_RIVER_RM_INFO where rm_start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and rm_end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')"
                    + "and rm_app_id='" + userid + "' and river_flow_id='"+section_id+"'";

                    DataTable dtInfo = SqlHelper.GetDataTable(sql, null);

                    if (dtInfo.Rows.Count <= 0)
                    {
                        continue;
                    }

                    //单条河涌详情id
                    String rm_river_id = dtInfo.Rows[0][0].ToString();
                    //周报开始时间
                    String rm_start_time = startTm;
                    //周报结束时间
                    String rm_end_time = endTm.Substring(0, 10) + " 00:00:00";

                    sql = "select to_char(create_date,'yyyy.MM') || '&' || (case black_odor when '重度黑臭' then '0' when '轻度黑臭' then '1' else '2' end) from rm_river_rated where river_id=(select distinct rivet_rm_id from rm_riverchief_section_r where river_section_id='"+section_id+"' and del_flags='0')"
                        + " and create_date>=to_date(to_char(add_months(to_date('"+startTm+"','yyyy-MM-dd HH24:mi:ss'),-4),'yyyy-MM') || '-01','yyyy-MM-dd')"
                        + " and create_date<to_date(to_char(to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss'),'yyyy-MM') || '-01','yyyy-MM-dd')"
                        +" order by create_date";

                    DataTable dtBefore4Months = SqlHelper.GetDataTable(sql, null);

                    if (dtBefore4Months.Rows.Count <= 0)
                        continue;

                    //时间&水质状态 time_type
                    String time_type="";
                    for (int index = 0; index < dtBefore4Months.Rows.Count; index++)
                    {
                        time_type += "," + dtBefore4Months.Rows[index][0].ToString();
                    }

                    time_type = time_type.Trim(',');

                    if (if_delAndInDB)
                    {
                        sql = "insert into APPWEEK_RIVER_MY_WATER_QUALITY(id, rm_river_id, water_start_end_time, time_type,rm_start_time,rm_end_time) values('"
                            + id + "','" + rm_river_id + "','" + water_start_end_time + "','" + time_type + "',to_date('" + rm_start_time + "','yyyy-MM-dd HH24:mi:ss'),to_date('" + rm_end_time + "','yyyy-MM-dd HH24:mi:ss'))";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                    } 
                }

                bgWork.ReportProgress(i + 1);
            }
        }

        //管辖河段问题发现与处理APPWEEK_RIVER_JURISDICTION_PRO
        private void btn_single_river_problem_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgSingleProblem.DoWork += new DoWorkEventHandler(worker_DoWorkSingleProblem);
            bgSingleProblem.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgSingleProblem.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgSingleProblem.RunWorkerAsync();
        }

        //管辖河段问题发现与处理APPWEEK_RIVER_JURISDICTION_PRO
        void worker_DoWorkSingleProblem(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(区级单条管辖河段问题发现与处理:APPWEEK_RIVER_JURISDICTION_PRO)...";
            writeInfo(lblInfo.Text);

            runSingleProblem(progressBar1, bgSingleProblem);

            lblInfo.Text = "数据处理完成(区级单条管辖河段问题发现与处理:APPWEEK_RIVER_JURISDICTION_PRO)...";
            writeInfo(lblInfo.Text);

            if (auto_mode)
            {
                //继续跑 我的下级镇街河长履职APPWEEK_RIVER_LOWER_LEVEL_SITUATION
                btn_single_lower_level_Click(null, null);
            }
        }

        //管辖河段问题发现与处理APPWEEK_RIVER_JURISDICTION_PRO
        public void runSingleProblem(ProgressBar pBar, BackgroundWorker bgWork)
        {
            String sql = "";
            if (if_delAndInDB)
            {
                sql = "delete from APPWEEK_RIVER_JURISDICTION_PRO where rm_river_id in(select id from APPWEEK_RIVER_RM_INFO where rm_start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and rm_end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";
                if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                    sql += " and rm_app_id='" + lblChiefID.Text.Trim() + "')";
                else
                    sql += ")";

                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
            }

            sql = "select id from sys_user_b where river_type in('7','8','9') and del_flag='0'"; // and id='236c25e9fddc43f78a647e25ebc7e381'  and id='50000628'
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();

                //周报开始时间
                String rm_start_time = startTm;
                //周报结束时间
                String rm_end_time = endTm.Substring(0, 10) + " 00:00:00";

                //查询责任河段数量
                sql = "select distinct RIVER_SECTION_ID,(select name from rm_river_lake where id=RIVER_SECTION_ID) name from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0' and del_flags='0'";

                DataTable dtTableRiver = SqlHelper.GetDataTable(sql, null);

                //循环所有责任河段
                for (int j = 0; j < dtTableRiver.Rows.Count; j++)
                {
                    String id = Guid.NewGuid().ToString();
                    String section_id = dtTableRiver.Rows[j]["RIVER_SECTION_ID"].ToString();

                    sql = "select id from APPWEEK_RIVER_RM_INFO where rm_start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and rm_end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')"
                    + "and rm_app_id='" + userid + "' and river_flow_id='" + section_id + "'";

                    DataTable dtInfo = SqlHelper.GetDataTable(sql, null);

                    if (dtInfo.Rows.Count <= 0)
                    {
                        continue;
                    }

                    //单条河涌详情id
                    String rm_river_id = dtInfo.Rows[0][0].ToString();

                    sql = "select * from "
                        +"("
                        +"    select to_char(time,'yyyy-MM') p_month,(select name from PTL_PATROLMATTER where mattertype='2' and id=type) type,count(problemid) nums from ptl_reportproblem"
                        +"    where sectionid in"
                        +"    ("
                        +"        select distinct river_section_id from rm_riverchief_section_r"
                        + "        where area_section_id='"+section_id+"'"
                        +"    )"
                        + "    and TIME>=to_date(to_char(add_months(to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss'),-4),'yyyy-MM') || '-01','yyyy-MM-dd')"
                        + "    and TIME<to_date(to_char(to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss'),'yyyy-MM') || '-01','yyyy-MM-dd') and state<>'-1'"
                        +"    group by to_char(time,'yyyy-MM'),type"
                        +") order by P_month,nums desc";

                    DataTable dtBefore4Months = SqlHelper.GetDataTable(sql, null);

                    DateTime dtEnd = DateTime.Parse(startTm);
                    DateTime dtStart = dtEnd.AddMonths(-3);

                    //发现问题时间和数量 find_time_value
                    String find_time_value = "";
                    //一段时间内多发问题类型和数量 
                    String multiple_time_value = "";
                    while (DateTime.Compare(dtStart, dtEnd) <= 0)
                    {
                        String dtStartMonth = dtStart.ToString("yyyy-MM");
                        DataRow[] rows=dtBefore4Months.Select("p_month='" + dtStartMonth + "'");

                        int totalProblem = 0;
                        int firstNums = 0;
                        for (int index = 0; index < rows.Length; index++)
                        {
                            totalProblem+=Convert.ToInt32(rows[index][2]);
                            int thisNums = Convert.ToInt32(rows[index][2]); 

                            if (index == 0)
                            {
                                multiple_time_value += rows[0][1] + "&" + dtStartMonth.Replace('-','.') + "&" + rows[0][2]+",";


                                firstNums = Convert.ToInt32(rows[index][2]);
                            }
                            else if (thisNums == firstNums)
                            {
                                multiple_time_value = multiple_time_value.Trim(',');
                                multiple_time_value += "-" + rows[index][1] + "&" + dtStartMonth.Replace('-', '.') + "&" + rows[index][2] + ",";
                            }
                        }

                        if(rows.Length<=0)
                            multiple_time_value += "0" + "&" + dtStartMonth.Replace('-', '.') + "&" + "0" + ",";

                        find_time_value += dtStartMonth.Replace('-', '.') + "&" + totalProblem + ",";

                        dtStart=dtStart.AddMonths(1);
                    }

                    //重大问题数已办结 total_finish_problem_cnt
                    sql = "select count(problemid) nums from ptl_reportproblem "
                        +" where sectionid in"
                        +" ("
                        +"    select distinct river_section_id from rm_riverchief_section_r "
                        +"    where area_section_id='"+section_id+"'"
                        +" ) "
                        +" and TIME<to_date('"+endTm+"','yyyy-MM-dd HH24:mi:ss') and state='4'"
                        +" and problemid in(select problem_id from ptl_problem_paper_assign)";

                    int total_finish_problem_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                    //重大问题数未办结 total_no_finish_noproblem_cnt
                    sql = "select count(problemid) nums from ptl_reportproblem "
                        + " where sectionid in"
                        + " ("
                        + "    select distinct river_section_id from rm_riverchief_section_r "
                        + "    where area_section_id='" + section_id + "'"
                        + " ) "
                        + " and TIME<to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and state<>'4' and state<>'-1'"
                        + " and problemid in(select problem_id from ptl_problem_paper_assign)";

                    int total_no_finish_noproblem_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                    if (if_delAndInDB)
                    {
                        sql = "insert into APPWEEK_RIVER_JURISDICTION_PRO(id, rm_river_id, find_time_value, multiple_time_value,total_finish_problem_cnt,total_no_finish_noproblem_cnt,rm_start_time,rm_end_time) values('"
                            + id + "','" + rm_river_id + "','" + find_time_value + "','" + multiple_time_value + "','" + total_finish_problem_cnt + "','" + total_no_finish_noproblem_cnt + "',to_date('" + rm_start_time + "','yyyy-MM-dd HH24:mi:ss'),to_date('" + rm_end_time + "','yyyy-MM-dd HH24:mi:ss'))";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                    }
                }

                bgWork.ReportProgress(i + 1);
            }
        }

        //我的下级镇街河长履职APPWEEK_RIVER_LOWER_LEVEL_SITUATION
        private void btn_single_lower_level_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgSingleLowerLevel.DoWork += new DoWorkEventHandler(worker_DoWork_single_lower_level);
            bgSingleLowerLevel.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgSingleLowerLevel.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgSingleLowerLevel.RunWorkerAsync();
        }

        //我的下级镇街河长履职APPWEEK_RIVER_LOWER_LEVEL_SITUATION
        void worker_DoWork_single_lower_level(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(区级单条 我的下级镇街河长履职APPWEEK_RIVER_LOWER_LEVEL_SITUATION)...";
            writeInfo(lblInfo.Text);

            run_single_lower_level(progressBar1, bgSingleLowerLevel);

            lblInfo.Text = "数据处理完成(区级单条 我的下级镇街河长履职APPWEEK_RIVER_LOWER_LEVEL_SITUATION)...";
            writeInfo(lblInfo.Text);

            if (auto_mode)
            {
                //继续跑 我的下级镇街河长履职（187履职评价）APPWEEK_RIVER_LOWER_MY_SCORE
                btn_single_lower_my_score_Click(null, null);
            }
        }

        //我的下级镇街河长履职APPWEEK_RIVER_LOWER_LEVEL
        public void run_single_lower_level(ProgressBar pBar, BackgroundWorker bgWork)
        {
            String sql = "";
            if (if_delAndInDB)
            {
                sql = "delete from APPWEEK_RIVER_LOWER_LEVEL where rm_river_id in(select id from APPWEEK_RIVER_RM_INFO where rm_start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and rm_end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";
                if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                    sql += " and rm_app_id='" + lblChiefID.Text.Trim() + "')";
                else
                    sql += ")";

                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
            }

            sql = "select id from sys_user_b where river_type in('7','8','9') and del_flag='0'"; // and id='236c25e9fddc43f78a647e25ebc7e381'  and id='50000628'
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();
                lblInfo.Text = userid;

                //查询责任河段数量
                sql = "select distinct RIVER_SECTION_ID,(select name from rm_river_lake where id=RIVER_SECTION_ID) name from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0' and del_flags='0'";

                DataTable dtTableRiver = SqlHelper.GetDataTable(sql, null);

                //循环所有责任河段
                for (int j = 0; j < dtTableRiver.Rows.Count; j++)
                {
                    String id = Guid.NewGuid().ToString();
                    String section_id = dtTableRiver.Rows[j]["RIVER_SECTION_ID"].ToString();

                    sql = "select id from APPWEEK_RIVER_RM_INFO where rm_start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and rm_end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')"
                    + "and rm_app_id='" + userid + "' and river_flow_id='"+section_id+"'";

                    DataTable dtInfo = SqlHelper.GetDataTable(sql, null);

                    if (dtInfo.Rows.Count <= 0)
                    {
                        continue;
                    }

                    //单条河涌详情id
                    String rm_river_id = dtInfo.Rows[0][0].ToString();
                    //周报开始时间
                    String rm_start_time = startTm;
                    //周报结束时间
                    String rm_end_time = endTm.Substring(0, 10) + " 00:00:00";

                    //下级镇街河长人数 lower_level_rm_cnt
                    sql = "select count(distinct user_id) nums from rm_riverchief_section_r where area_section_id='" + section_id + "'"
                        + " and user_id in(select id from sys_user_b where river_type in('12','13') and del_flag='0')";

                    DataTable dtLowerLevelChiefNums = SqlHelper.GetDataTable(sql, null);
                    int lower_level_rm_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);
                    //巡河不达标 lower_level_no_achieve_cnt
                    sql = "select count(user_id) nums from"
                        +" ("
                        +"    select user_id,nvl(nums,0) nums from"
                        +"    ("
                        + "        select distinct user_id from rm_riverchief_section_r where area_section_id='" + section_id + "'"
                        +"        and user_id in(select id from sys_user_b where river_type in('12','13') and del_flag='0')"
                        +"    ) t_u "
                        +"    left join"
                        +"    ("
                        +"        select userid,count(patrolid) nums from ptl_patrol_r where userid in"
                        +"        ("
                        + "            select distinct user_id from rm_riverchief_section_r where area_section_id='" + section_id + "'"
                        +"            and user_id in(select id from sys_user_b where river_type in('12','13') and del_flag='0')"
                        + "        ) and strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and duration>=10"
                        +"        group by userid"
                        +"    ) t_p on t_u.user_id=t_p.userid"
                        +") where nums<1";

                    int lower_level_no_achieve_cnt = SqlHelper.GetDataTable(sql, null).Rows.Count;

                    //0上报 zero_submit_problem_cnt
                    sql = "select count(user_id) from"
                        +" ("
                        +"    select user_id,nvl(nums,0) nums from"
                        +"    ("
                        +"        select distinct user_id from rm_riverchief_section_r where area_section_id='"+section_id+"'"
                        +"        and user_id in(select id from sys_user_b where river_type in('12','13') and del_flag='0')"
                        +"    ) t_u "
                        +"    left join"
                        +"    ("
                        +"        select userid,count(problemid) nums from ptl_reportproblem where userid in"
                        +"        ("
                        + "            select distinct user_id from rm_riverchief_section_r where area_section_id='" + section_id + "'"
                        +"            and user_id in(select id from sys_user_b where river_type in('12','13') and del_flag='0')"
                        +"        ) and time>=to_date('"+startTm+"','yyyy-MM-dd HH24:mi:ss') and time<=to_date('"+endTm+"','yyyy-MM-dd HH24:mi:ss') and state<>'-1'"
                        +"        group by userid"
                        +"    ) t_p on t_u.user_id=t_p.userid"
                        +") where nums<1";

                    int zero_submit_problem_cnt = SqlHelper.ExecuteScalar(sql, CommandType.Text, null);

                    //四个查清时间 check_clear_time
                    Dictionary<String, String> timeRegion = getTimeRegion("13", startTm);
                    String cqStart_time = timeRegion["startTime"];
                    String cqEnd_time = timeRegion["endTime"];
                    String check_clear_time = cqStart_time + "至" + cqEnd_time;
                    check_clear_time = check_clear_time.Replace('/', '.').Replace('-', '.').Replace('至', '-');

                    //未完成四个查清人数 lower_level_check_clear
                    sql = "select distinct user_id from rm_riverchief_section_r where area_section_id='" + section_id + "'"
                        + " and user_id in(select id from sys_user_b where river_type in('12','13') and del_flag='0')";

                    DataTable dtTabeUser = SqlHelper.GetDataTable(sql, null);

                    int lower_level_check_clear = 0;
                    for (int index = 0; index < dtTabeUser.Rows.Count; index++)
                    {
                        String low_user = dtTabeUser.Rows[index][0].ToString();

                        String not_connect = "0";
                        String illegal_building = "0";
                        String debunching_place = "0";
                        String outfall_exception = "0";

                        //两岸通道贯通是否查清 1--查清  0--未查清
                        sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and user_id='" + low_user + "' and check_type='1'";

                        if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                            not_connect = "1";

                        //疑似违法建筑是否查清
                        sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and user_id='" + low_user + "' and check_type='2'";

                        if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                            illegal_building = "1";

                        //散乱污场所是否查清
                        sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and user_id='" + low_user + "' and check_type='3'";

                        if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                            debunching_place = "1";
                        //异常排水口是否查清
                        sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and user_id='" + low_user + "' and check_type='4'";

                        if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                            outfall_exception = "1";
                        //}

                        if (not_connect.Equals("0") || illegal_building.Equals("0") || debunching_place.Equals("0") || outfall_exception.Equals("0"))
                        {
                            lower_level_check_clear++;
                        }
                    }

                    if (if_delAndInDB)
                    {
                        sql = "insert into APPWEEK_RIVER_LOWER_LEVEL(id, rm_river_id, lower_level_rm_cnt, lower_level_no_achieve_cnt,zero_submit_problem_cnt,check_clear_time,lower_level_check_clear,rm_start_time,rm_end_time) values('"
                            + id + "','" + rm_river_id + "','" + lower_level_rm_cnt + "','" + lower_level_no_achieve_cnt + "','" + zero_submit_problem_cnt + "','" + check_clear_time + "','" + lower_level_check_clear + "',to_date('" + rm_start_time + "','yyyy-MM-dd HH24:mi:ss'),to_date('" + rm_end_time + "','yyyy-MM-dd HH24:mi:ss'))";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                    }
                }

                bgWork.ReportProgress(i + 1);
            }
        }

        //我的下级镇街河长履职（187履职评价）APPWEEK_RIVER_LOWER_MY_SCORE
        private void btn_single_lower_my_score_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgSingleLowerMyScore.DoWork += new DoWorkEventHandler(worker_DoWorkSingleLowerMyScore);
            bgSingleLowerMyScore.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgSingleLowerMyScore.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgSingleLowerMyScore.RunWorkerAsync();
        }

        //我的下级镇街河长履职（187履职评价）APPWEEK_RIVER_LOWER_MY_SCORE
        void worker_DoWorkSingleLowerMyScore(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(区级单条 我的下级镇街河长履职（187履职评价）APPWEEK_RIVER_LOWER_MY_SCORE)...";
            writeInfo(lblInfo.Text);

            runSingleLowerMyScore(progressBar1, bgSingleLowerMyScore);

            lblInfo.Text = "数据处理完成(区级单条 我的下级镇街河长履职（187履职评价）APPWEEK_RIVER_LOWER_MY_SCORE)...";
            writeInfo(lblInfo.Text);

            if (auto_mode)
            {
                //继续跑 我的下级镇街河长履职弹窗APPWEEK_RIVER_POPUP_LOWER_LEVEL
                btn_single_popup_Click(null, null);
            }
        }

        //我的下级镇街河长履职（187履职评价）APPWEEK_RIVER_LOWER_MY_SCORE
        public void runSingleLowerMyScore(ProgressBar pBar, BackgroundWorker bgWork)
        {
            String sql = "";
            if (if_delAndInDB)
            {
                sql = "delete from APPWEEK_RIVER_LOWER_MY_SCORE where rm_river_id in(select id from APPWEEK_RIVER_RM_INFO where rm_start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and rm_end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";
                if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                    sql += " and rm_app_id='" + lblChiefID.Text.Trim() + "')";
                else
                    sql += ")";

                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
            }

            sql = "select id from sys_user_b where river_type in('7','8','9') and del_flag='0'"; // and id='236c25e9fddc43f78a647e25ebc7e381'  and id='50000628'
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();

                //查询责任河段数量
                sql = "select distinct RIVER_SECTION_ID,(select name from rm_river_lake where id=RIVER_SECTION_ID) name,rivet_rm_id from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0' and del_flags='0'";

                DataTable dtTableRiver = SqlHelper.GetDataTable(sql, null);

                //循环所有责任河段
                for (int j = 0; j < dtTableRiver.Rows.Count; j++)
                {
                    String section_id = dtTableRiver.Rows[j]["RIVER_SECTION_ID"].ToString();
                    String river_id = dtTableRiver.Rows[j]["rivet_rm_id"].ToString();

                    sql = "select id from APPWEEK_RIVER_RM_INFO where rm_start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and rm_end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')"
                    + "and rm_app_id='" + userid + "' and river_flow_id='"+section_id+"'";

                    DataTable dtInfo = SqlHelper.GetDataTable(sql, null);

                    if (dtInfo.Rows.Count <= 0)
                    {
                        continue;
                    }

                    //单条河涌详情id
                    String rm_river_id = dtInfo.Rows[0][0].ToString();
                    //周报开始时间
                    String rm_start_time = startTm;
                    //周报结束时间
                    String rm_end_time = endTm.Substring(0, 10) + " 00:00:00";

                    /*sql = "select user_id,(select name from sys_user_b where id=user_id) name,(select (select name from sys_area_b where id=addvcd_towm) from sys_user_b where id=user_id) town,sum(score)+60 score from user_score_r_187 where user_id in "
                        +"("
                        +"    select distinct user_id from rm_riverchief_section_r where area_section_id='"+section_id+"'"
                        +"    and user_id in(select id from sys_user_b where river_type in('12','13') and del_flag='0')"
                        +") and start_date=to_date('"+startTm+"','yyyy-MM-dd HH24:mi:ss') and river_id='"+river_id+"' group by user_id order by score desc";*/

                    sql = "select user_id,(select name from sys_user_b where id=user_id) name,(select (select name from sys_area_b where id=addvcd_towm) from sys_user_b where id=user_id) town"
                        +",round(avg(score),2) score from"
                        +"("
                        +"    select user_id,sum(score)+60 score from user_score_r_187 where user_id in"
                        +"    ("
                        +"        select distinct user_id from rm_riverchief_section_r where area_section_id='"+section_id+"'"
                        +"        and user_id in(select id from sys_user_b where river_type in('12','13') and del_flag='0')"
                        +"    ) and start_date=to_date('"+startTm+"','yyyy-MM-dd HH24:mi:ss') and river_id='"+river_id+"' group by user_id,section_id"
                        +") group by user_id order by score desc";

                    DataTable dtTabeUser = SqlHelper.GetDataTable(sql, null);

                    for (int index = 0; index < dtTabeUser.Rows.Count; index++)
                    {
                        String id = Guid.NewGuid().ToString();
                        String low_user = dtTabeUser.Rows[index][0].ToString();
                        //该区级河段的下级镇街河段责任河长的姓名
                        String low_user_name = dtTabeUser.Rows[index][1].ToString();

                        //同比上周上升名次 rise_rank
                        String lastWeekEndTime = Convert.ToDateTime(startTm).AddDays(-1).ToShortDateString() + " 00:00:00";

                        sql = "select id from"
                            +"("
                            +"    select rownum id,user_id,score from"
                            +"    ("
                            +"        select user_id,round(avg(score),2) score from"
                            +"        ("
                            +"                 select user_id,sum(score)+60 score from user_score_r_187 where user_id in"
                            +"                 ("
                            +"                     select distinct user_id from rm_riverchief_section_r where area_section_id='"+section_id+"' "
                            +"                     and user_id in(select id from sys_user_b where river_type in('12','13') and del_flag='0')"
                            + "                ) and end_date=to_date('" + lastWeekEndTime + "','yyyy-MM-dd HH24:mi:ss') and river_id='" + river_id + "' group by user_id,section_id"
                            +"     ) group by user_id order by score desc"
                            +"    )"
                            +")  where user_id='"+low_user+"'";

                        int lastWeekRank = Convert.ToInt32(SqlHelper.ExecuteScalar(sql, CommandType.Text, null));
                        int rise_rank = index+1 - lastWeekRank;
                        //镇街 town
                        String town = dtTabeUser.Rows[index][2].ToString();
                        //分数 score
                        String score = dtTabeUser.Rows[index][3].ToString();
                        //排名 rank
                        String rank = index +1+ "";

                        if (if_delAndInDB)
                        {
                            sql = "insert into APPWEEK_RIVER_LOWER_MY_SCORE(id, rm_river_id, name, rise_rank,town,score,rank,rm_start_time,rm_end_time) values('"
                                + id + "','" + rm_river_id + "','" + low_user_name + "','" + rise_rank + "','" + town + "','" + score + "','" + rank + "',to_date('" + rm_start_time + "','yyyy-MM-dd HH24:mi:ss'),to_date('" + rm_end_time + "','yyyy-MM-dd HH24:mi:ss'))";

                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                    }
                }

                bgWork.ReportProgress(i + 1);
            }
        }

        //我的下级镇街河长履职弹窗APPWEEK_RIVER_POPUP_LOWER_LEVEL
        private void btn_single_popup_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgSinglePopup.DoWork += new DoWorkEventHandler(worker_DoWorkSinglePopup);
            bgSinglePopup.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgSinglePopup.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgSinglePopup.RunWorkerAsync();
        }

        //我的下级镇街河长履职（187履职评价）APPWEEK_RIVER_LOWER_MY_SCORE
        void worker_DoWorkSinglePopup(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(区级单条 我的下级镇街河长履职弹窗APPWEEK_RIVER_POPUP_LOWER_LEVEL)...";
            writeInfo(lblInfo.Text);

            runSinglePopup(progressBar1, bgSinglePopup);

            lblInfo.Text = "数据处理完成(区级单条 我的下级镇街河长履职弹窗APPWEEK_RIVER_POPUP_LOWER_LEVEL)...";
            writeInfo(lblInfo.Text);

            if (auto_mode)
            {
                //继续跑
                //btn_single_popup_Click(null, null);
            }
        }

        //我的下级镇街河长履职弹窗APPWEEK_RIVER_POPUP_LOWER_LEVEL
        public void runSinglePopup(ProgressBar pBar, BackgroundWorker bgWork)
        {
            String sql = "";
            if (if_delAndInDB)
            {
                sql = "delete from APPWEEK_RIVER_POPUP_LOWE where rm_start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and rm_end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";
                //if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                //    sql += " and rm_app_id='" + lblChiefID.Text.Trim() + "')";
                //else
                //    sql += ")";

                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
            }

            sql = "select id from sys_user_b where river_type in('7','8','9') and del_flag='0'"; // and id='236c25e9fddc43f78a647e25ebc7e381'  and id='50000628'
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dt.Rows.Count;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();

                

                //查询责任河段数量
                sql = "select distinct RIVER_SECTION_ID,(select name from rm_river_lake where id=RIVER_SECTION_ID) name,rivet_rm_id from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0' and del_flags='0'";

                DataTable dtTableRiver = SqlHelper.GetDataTable(sql, null);

                //循环所有责任河段
                for (int j = 0; j < dtTableRiver.Rows.Count; j++)
                {
                    String section_id = dtTableRiver.Rows[j]["RIVER_SECTION_ID"].ToString();
                    String river_id = dtTableRiver.Rows[j]["rivet_rm_id"].ToString();

                    sql = "select id from APPWEEK_RIVER_RM_INFO where rm_start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and rm_end_time=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')"
                    + "and rm_app_id='" + userid + "' and river_flow_id='"+section_id+"'";

                    DataTable dtInfo = SqlHelper.GetDataTable(sql, null);

                    if (dtInfo.Rows.Count <= 0)
                    {
                        continue;
                    }

                    //单条河涌详情id
                    String rm_river_id = dtInfo.Rows[0][0].ToString();
                    //周报开始时间
                    String rm_start_time = startTm;
                    //周报结束时间
                    String rm_end_time = endTm.Substring(0, 10) + " 00:00:00";

                    //下级镇街级河长详情
                    sql = "select distinct user_id,(select name from sys_user_b where id=user_id) name,"
                        +"(select (select name from sys_area_b where id=addvcd_area) from sys_user_b where id=user_id) || '-' || (select (select name from sys_area_b where id=addvcd_towm) from sys_user_b where id=user_id) address"
                        +" from rm_riverchief_section_r where area_section_id='"+section_id+"'"
                        +" and user_id in(select id from sys_user_b where river_type in('12','13') and del_flag='0')";

                    DataTable dtTabeUser = SqlHelper.GetDataTable(sql, null);

                    for (int index = 0; index < dtTabeUser.Rows.Count; index++)
                    {
                        String id = Guid.NewGuid().ToString();
                        //rm_name	河长名称
                        String rm_name = dtTabeUser.Rows[index]["name"].ToString();
                        //rm_address	地址
                        String rm_address = dtTabeUser.Rows[index]["address"].ToString();
                        //data_type_cnt	数量
                        String data_type_cnt = null;
                        //type	类型
                        String type = "1";

                        if (if_delAndInDB)
                        {
                            sql = "insert into APPWEEK_RIVER_POPUP_LOWE(id, rm_river_id, rm_name, rm_address,data_type_cnt,type,rm_start_time,rm_end_time) values('"
                                + id + "','" + rm_river_id + "','" + rm_name + "','" + rm_address + "','" + data_type_cnt + "','" + type + "',to_date('" + rm_start_time + "','yyyy-MM-dd HH24:mi:ss'),to_date('" + rm_end_time + "','yyyy-MM-dd HH24:mi:ss'))";

                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                    }

                    //巡河不达标河长详情
                    sql = "select user_id,(select name from sys_user_b where id=user_id) name,"
                        +"(select (select name from sys_area_b where id=addvcd_area) from sys_user_b where id=user_id) || '-' || (select (select name from sys_area_b where id=addvcd_towm) from sys_user_b where id=user_id) address"
                        +",nums"
                        + " from ("
                        + "    select user_id,nvl(nums,0) nums from"
                        + "    ("
                        + "        select distinct user_id from rm_riverchief_section_r where area_section_id='" + section_id + "'"
                        + "        and user_id in(select id from sys_user_b where river_type in('12','13') and del_flag='0')"
                        + "    ) t_u "
                        + "    left join"
                        + "    ("
                        + "        select userid,count(patrolid) nums from ptl_patrol_r where userid in"
                        + "        ("
                        + "            select distinct user_id from rm_riverchief_section_r where area_section_id='" + section_id + "'"
                        + "            and user_id in(select id from sys_user_b where river_type in('12','13') and del_flag='0')"
                        + "        ) and strtime>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and duration>=10"
                        + "        group by userid"
                        + "    ) t_p on t_u.user_id=t_p.userid"
                        + ") where nums<1";

                    DataTable dtTabePatrol = SqlHelper.GetDataTable(sql, null);

                    for (int index = 0; index < dtTabePatrol.Rows.Count; index++)
                    {
                        String id = Guid.NewGuid().ToString();
                        //rm_name	河长名称
                        String rm_name = dtTabePatrol.Rows[index]["name"].ToString();
                        //rm_address	地址
                        String rm_address = dtTabePatrol.Rows[index]["address"].ToString();
                        //data_type_cnt	数量
                        String data_type_cnt = dtTabePatrol.Rows[index]["nums"].ToString();
                        //type	类型
                        String type = "2";

                        if (if_delAndInDB)
                        {
                            sql = "insert into APPWEEK_RIVER_POPUP_LOWE(id, rm_river_id, rm_name, rm_address,data_type_cnt,type,rm_start_time,rm_end_time) values('"
                                + id + "','" + rm_river_id + "','" + rm_name + "','" + rm_address + "','" + data_type_cnt + "','" + type + "',to_date('" + rm_start_time + "','yyyy-MM-dd HH24:mi:ss'),to_date('" + rm_end_time + "','yyyy-MM-dd HH24:mi:ss'))";

                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                    }

                    //0上报人数详情
                    sql = "select user_id,(select name from sys_user_b where id=user_id) name,"
                        + "(select (select name from sys_area_b where id=addvcd_area) from sys_user_b where id=user_id) || '-' || (select (select name from sys_area_b where id=addvcd_towm) from sys_user_b where id=user_id) address"
                       + " from ("
                       + "    select user_id,nvl(nums,0) nums from"
                       + "    ("
                       + "        select distinct user_id from rm_riverchief_section_r where area_section_id='" + section_id + "'"
                       + "        and user_id in(select id from sys_user_b where river_type in('12','13') and del_flag='0')"
                       + "    ) t_u "
                       + "    left join"
                       + "    ("
                       + "        select userid,count(problemid) nums from ptl_reportproblem where userid in"
                       + "        ("
                       + "            select distinct user_id from rm_riverchief_section_r where area_section_id='" + section_id + "'"
                       + "            and user_id in(select id from sys_user_b where river_type in('12','13') and del_flag='0')"
                       + "        ) and time>=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and time<=to_date('" + endTm + "','yyyy-MM-dd HH24:mi:ss') and state<>'-1'"
                       + "        group by userid"
                       + "    ) t_p on t_u.user_id=t_p.userid"
                       + ") where nums<1";

                    DataTable dtTabeZeroReport = SqlHelper.GetDataTable(sql, null);

                    for (int index = 0; index < dtTabeZeroReport.Rows.Count; index++)
                    {
                        String id = Guid.NewGuid().ToString();
                        //rm_name	河长名称
                        String rm_name = dtTabeZeroReport.Rows[index]["name"].ToString();
                        //rm_address	地址
                        String rm_address = dtTabeZeroReport.Rows[index]["address"].ToString();
                        //data_type_cnt	数量
                        String data_type_cnt = null;
                        //type	类型
                        String type = "3";

                        if (if_delAndInDB)
                        {
                            sql = "insert into APPWEEK_RIVER_POPUP_LOWE(id, rm_river_id, rm_name, rm_address,data_type_cnt,type,rm_start_time,rm_end_time) values('"
                                + id + "','" + rm_river_id + "','" + rm_name + "','" + rm_address + "','" + data_type_cnt + "','" + type + "',to_date('" + rm_start_time + "','yyyy-MM-dd HH24:mi:ss'),to_date('" + rm_end_time + "','yyyy-MM-dd HH24:mi:ss'))";

                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                    }

                    //未完成四个查清河长详情
                    sql = "select distinct user_id,(select name from sys_user_b where id=user_id) name,"
                        + "(select (select name from sys_area_b where id=addvcd_area) from sys_user_b where id=user_id) || '-' || (select (select name from sys_area_b where id=addvcd_towm) from sys_user_b where id=user_id) address"
                        +" from rm_riverchief_section_r where area_section_id='" + section_id + "'"
                        + " and user_id in(select id from sys_user_b where river_type in('12','13') and del_flag='0')";

                    DataTable dtTabeFourCheck = SqlHelper.GetDataTable(sql, null);

                    //四个查清时间 check_clear_time
                    Dictionary<String, String> timeRegion = getTimeRegion("13", startTm);
                    String cqStart_time = timeRegion["startTime"];
                    String cqEnd_time = timeRegion["endTime"];

                    for (int index = 0; index < dtTabeFourCheck.Rows.Count; index++)
                    {
                        String low_user = dtTabeFourCheck.Rows[index][0].ToString();

                        String not_connect = "0";
                        String illegal_building = "0";
                        String debunching_place = "0";
                        String outfall_exception = "0";

                        //两岸通道贯通是否查清 1--查清  0--未查清
                        sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and user_id='" + low_user + "' and check_type='1'";

                        if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                            not_connect = "1";

                        //疑似违法建筑是否查清
                        sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and user_id='" + low_user + "' and check_type='2'";

                        if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                            illegal_building = "1";

                        //散乱污场所是否查清
                        sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and user_id='" + low_user + "' and check_type='3'";

                        if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                            debunching_place = "1";
                        //异常排水口是否查清
                        sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                            + " and user_id='" + low_user + "' and check_type='4'";

                        if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                            outfall_exception = "1";
                        //}

                        if (not_connect.Equals("0") || illegal_building.Equals("0") || debunching_place.Equals("0") || outfall_exception.Equals("0"))
                        {
                            String id = Guid.NewGuid().ToString();
                            //rm_name	河长名称
                            String rm_name = dtTabeFourCheck.Rows[index]["name"].ToString();
                            //rm_address	地址
                            String rm_address = dtTabeFourCheck.Rows[index]["address"].ToString();
                            //data_type_cnt	数量
                            String data_type_cnt = null;
                            //type	类型
                            String type = "4";

                            if (if_delAndInDB)
                            {
                                sql = "insert into APPWEEK_RIVER_POPUP_LOWE(id, rm_river_id, rm_name, rm_address,data_type_cnt,type,rm_start_time,rm_end_time) values('"
                                    + id + "','" + rm_river_id + "','" + rm_name + "','" + rm_address + "','" + data_type_cnt + "','" + type + "',to_date('" + rm_start_time + "','yyyy-MM-dd HH24:mi:ss'),to_date('" + rm_end_time + "','yyyy-MM-dd HH24:mi:ss'))";

                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                            }
                        }
                    }
                }

                bgWork.ReportProgress(i + 1);
            }
        }

        private void chkContiuneJob_CheckedChanged(object sender, EventArgs e)
        {
            auto_mode = chkContiuneJob.Checked;
        }
    }
}
