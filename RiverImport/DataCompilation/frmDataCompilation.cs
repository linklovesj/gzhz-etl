using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sunny.Misc;

namespace RiverImport.DataCompilation
{
    public partial class frmDataCompilation : Form
    {
        public frmDataCompilation()
        {
            InitializeComponent();
        }

        public String startTm = "";//"2018-06-18 00:00:00";
        public String endTm = "";//"2018-06-24 23:59:59";
        public bool auto_mode = false;
        public RichTextBox rtxInfo;
        public bool if_delAndInDB = false;

        private void frmDataCompilation_Load(object sender, EventArgs e)
        {
            dtpStart.Value = DateTime.Now.AddDays(1 - Convert.ToInt16(DateTime.Now.DayOfWeek) - 7);

            dtpEnd.Value = DateTime.Now.AddDays(7 - Convert.ToInt16(DateTime.Now.DayOfWeek) - 7);
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            startTm = dtpStart.Text;
        }

        private void dtpEnd_ValueChanged(object sender, EventArgs e)
        {
            endTm = dtpEnd.Text;
        }
        

        private void btnStart_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;

            bgWork.DoWork += new DoWorkEventHandler(worker_DoWorkAppUse);
            bgWork.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            bgWork.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            bgWork.RunWorkerAsync();
        }

        void worker_DoWorkAppUse(object sender, DoWorkEventArgs e)
        {
            lblInfo.Text = "数据处理中(我的app使用情况:appweek_my_app_use_situation)...";
            writeInfo(lblInfo.Text);

            runAppUse(progressBar1, bgWork);

            lblInfo.Text = "数据处理完成(我的app使用情况:appweek_my_app_use_situation)";
            writeInfo(lblInfo.Text);
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
            if (rtxInfo != null)
                rtxInfo.AppendText("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + msg + "\r\n");
        }

        public void runAppUse(ProgressBar pBar, BackgroundWorker bgWork)
        {
            String sql = "";

            sql = "select id,type,addvcd_area,addvcd_town,addvcd_village,substr(parent_ids, 0, instr(parent_ids, ',') - 1) river_id,river_side from rm_river_lake"
                + " where name like '%沙河涌%' order by id,addvcd_area,addvcd_town";

            DataTable dtRiver = SqlHelper.GetDataTable(sql, null);

            pBar.Value = 0;
            pBar.Maximum = dtRiver.Rows.Count;
            for (var i = 0; i < dtRiver.Rows.Count; i++)
            {
                String type = dtRiver.Rows[i]["type"].ToString();
                String river_id = dtRiver.Rows[i]["river_id"].ToString();
                String section_id = dtRiver.Rows[i]["id"].ToString();
                String side = dtRiver.Rows[i]["river_side"].ToString();
                String cycle = "1";

                String addvcd_city = "440100000000";
                String addvcd_area = dtRiver.Rows[i]["addvcd_area"].ToString();
                String addvcd_town = dtRiver.Rows[i]["addvcd_town"].ToString();
                String addvcd_village = dtRiver.Rows[i]["addvcd_village"].ToString();

                //市级河段
                if(type.Equals("1"))
                {
                    sql = "select id,addvcd_area,addvcd_towm,addvcd_village from sys_user_b where river_type='4' and del_flag='0' and "
                        + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                    DataTable dtUserInfo = SqlHelper.GetDataTable(sql, null);

                    for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                    {
                        String userid = dtUserInfo.Rows[j]["id"].ToString();
                        String id=Guid.NewGuid().ToString().Replace("-","");

                        sql = "Insert into RTD_BASE (ID,ADDVCD_CITY,ADDVCD_AREA,ADDVCD_TOWN,ADDVCD_VILLAGE,RIVER_ID,SECTION_ID,SECTION_TYPE,BANK,USER_ID,CYCLE)"
                            + " values ('" + id + "','" + addvcd_city + "','" + addvcd_area + "','" + addvcd_town + "','" + addvcd_village + "','" + river_id + "','" + section_id + "','" + type + "','"+side+"','"+userid+"','"+cycle+"')";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                        //====================巡河数据=======================
                        //上月
                        String start_time = "2018-09-01 00:00:00";
                        String end_time = "2018-09-30 23:59:59";

                        			
                        //巡河天数
                        sql = "select count(distinct to_char(strtime,'yyyy-MM-dd')) nums from ptl_patrol_r where userid='" + userid + "' and strtime>='" + startTm + "' and endtime<='" + endTm + "'";

                        DataTable dtXunHeTianShu = SqlHelper.GetDataTable(sql, null);
                        String partol_day = dtXunHeTianShu.Rows[0][0].ToString();
                        //巡河总时间	巡河总里程	巡河次数
                        sql = "select sum(duration) total_time,sum(distance) total_distance,count(patrolid) total_times from ptl_patrol_r where userid='" + userid + "' and strtime>='" + startTm + "' and endtime<='" + endTm + "'";

                        DataTable dtXunHeAll = SqlHelper.GetDataTable(sql, null);
                        String total_time = dtXunHeAll.Rows[0]["total_time"].ToString();
                        String total_distance = dtXunHeAll.Rows[0]["total_distance"].ToString();
                        String total_times = dtXunHeAll.Rows[0]["total_times"].ToString();
                        //有效巡河次数 有效巡河时长 有效巡河里程
                        sql = "select sum(duration) total_time,sum(distance) total_distance,count(patrolid) total_times from ptl_patrol_r where duration>=10 and userid='" + userid + "' and strtime>='" + startTm + "' and endtime<='" + endTm + "'";

                        DataTable dtXunHeValidAll = SqlHelper.GetDataTable(sql, null);
                        String valid_total_time = dtXunHeValidAll.Rows[0]["total_time"].ToString();
                        String valid_total_distance = dtXunHeValidAll.Rows[0]["total_distance"].ToString();
                        String valid_total_times = dtXunHeValidAll.Rows[0]["total_times"].ToString();
                        //有效巡河天数
                        sql = "select count(distinct to_char(strtime,'yyyy-MM-dd')) nums from ptl_patrol_r where duration>=10 and userid='" + userid + "' and strtime>='" + startTm + "' and endtime<='" + endTm + "'";

                        DataTable dtValidXunHeTianShu = SqlHelper.GetDataTable(sql, null);
                        String valid_partol_day = dtValidXunHeTianShu.Rows[0][0].ToString();
                        //无效巡河次数
                        sql = "select count(patrolid) total_times from ptl_patrol_r where duration<10 and userid='" + userid + "' and strtime>='" + startTm + "' and endtime<='" + endTm + "'";

                        DataTable dtInvalidXunHeCiShu = SqlHelper.GetDataTable(sql, null);
                        String invalid_partol_times = dtInvalidXunHeCiShu.Rows[0][0].ToString();
                        //多样化巡河次数
                        sql = "select count(id) from ptl_patrol_other where user_id='" + userid + "' and tm>='" + startTm + "' and tm<='" + endTm + "'";

                        DataTable dtOtherXunHeCiShu = SqlHelper.GetDataTable(sql, null);
                        String others_partol_times = dtOtherXunHeCiShu.Rows[0][0].ToString();

                        //================问题=====================
                        //河长上报问题数									
                        sql = "select count(problemid) from ptl_reportproblem where state<>'-1' and reachid in"
                            +" (select id from rm_river_lake where parent_ids like '%AHD030212000000%')"
                            +" and userid in(select id from sys_user_b where is_riverchief='Y')"
                            +" and time>='2018-09-01 00:00:00' and time<='2018-09-30 23:59:59'";

                        DataTable dtChiefReportNums = SqlHelper.GetDataTable(sql, null);
                        String chief_report_nums = dtChiefReportNums.Rows[0][0].ToString();

                        //民间河长上报问题数
                        sql = "select count(problemid) nums from ptl_reportproblem where state<>'-1' and reachid in"
                            +" (select id from rm_river_lake where parent_ids like '%AHD030212000000%')"
                            +" and userid in(select id from sys_user_b where river_type='25')"
                            +" and time>='2018-09-01 00:00:00' and time<='2018-09-30 23:59:59'";

                        DataTable dtFolkChiefReportNums = SqlHelper.GetDataTable(sql, null);
                        String folkchief_report_nums = dtFolkChiefReportNums.Rows[0][0].ToString();

                        //市民投诉问题数
                        sql = "select count(problemid) nums from ptl_reportproblem where state<>'-1' and reachid in"
                            + " (select id from rm_river_lake where parent_ids like '%AHD030212000000%')"
                            + " and IS_CITIZEN='Y'"
                            + " and time>='2018-09-01 00:00:00' and time<='2018-09-30 23:59:59'";

                        DataTable dtCitizenReportNums = SqlHelper.GetDataTable(sql, null);
                        String citizen_report_nums = dtCitizenReportNums.Rows[0][0].ToString();

                        //市巡查投诉问题数
                        sql = "select count(problemid) nums from ptl_reportproblem where state<>'-1' and reachid in"
                            +" (select id from rm_river_lake where parent_ids like '%AHD030212000000%')"
                            +" and (userid in(select id from sys_user_b where river_type='21') or IS_CITIZEN='N')"
                            +" and time>='2018-01-01 00:00:00' and time<='2018-09-30 23:59:59'";

                        DataTable dtCitizenPatrolReportNums = SqlHelper.GetDataTable(sql, null);
                        String citizen_patrol_report_nums = dtCitizenPatrolReportNums.Rows[0][0].ToString();

                        //媒体曝光问题数
                        sql = "select count(problemid) nums from ptl_reportproblem where state<>'-1' and reachid in"
                            +" (select id from rm_river_lake where parent_ids like '%AHD030212000000%')"
                            +" and pro_resource='5'"
                            +" and time>='2018-01-01 00:00:00' and time<='2018-09-30 23:59:59'";

                        DataTable dtMediaReportNums = SqlHelper.GetDataTable(sql, null);
                        String media_report_nums = dtMediaReportNums.Rows[0][0].ToString();

                        //舆情问题数
                        sql = "select count(problemid) nums from ptl_reportproblem where state<>'-1' and reachid in"
                            + " (select id from rm_river_lake where parent_ids like '%AHD030212000000%')"
                            + " and pro_resource='3'"
                            + " and time>='2018-01-01 00:00:00' and time<='2018-09-30 23:59:59'";

                        DataTable dtPublicReportNums = SqlHelper.GetDataTable(sql, null);
                        String public_report_nums = dtPublicReportNums.Rows[0][0].ToString();

                        //暗访发现问题问题数
                        sql = "select count(problemid) nums from ptl_reportproblem where state<>'-1' and reachid in"
                            + " (select id from rm_river_lake where parent_ids like '%AHD030212000000%')"
                            + " and pro_resource='6'"
                            + " and time>='2018-01-01 00:00:00' and time<='2018-09-30 23:59:59'";

                        DataTable dtSecretReportNums = SqlHelper.GetDataTable(sql, null);
                        String secret_report_nums = dtSecretReportNums.Rows[0][0].ToString();

                        //微信投诉问题数

                        //电话投诉问题数
                        sql = "select count(problemid) nums from ptl_reportproblem where state<>'-1' and reachid in"
                            + " (select id from rm_river_lake where parent_ids like '%AHD030212000000%')"
                            + " and pro_resource='1'"
                            + " and time>='2018-01-01 00:00:00' and time<='2018-09-30 23:59:59'";

                        DataTable dtPhoneReportNums = SqlHelper.GetDataTable(sql, null);
                        String phone_report_nums = dtPhoneReportNums.Rows[0][0].ToString();

                        //自行处理问题数
                        sql = "select count(problemid) nums from ptl_reportproblem where state<>'-1' and reachid in"
                            + " (select id from rm_river_lake where parent_ids like '%AHD030212000000%')"
                            + " and NATIVEPROCESSING='1'"
                            + " and time>='2018-01-01 00:00:00' and time<='2018-09-30 23:59:59'";

                        DataTable dtNativeProcessNums = SqlHelper.GetDataTable(sql, null);
                        String native_process_nums = dtPhoneReportNums.Rows[0][0].ToString();

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
                        sql = "select userid,count(problemid) nums from ptl_reportproblem where state<>'-1' and reachid in"
                            + " (select id from rm_river_lake where parent_ids like '%AHD030212000000%')"
                            + " and userid in('e704436c3d8d438ba2ae721fda3910a4','5fb8fd0763bf463f84c42caa0f33eea6','5f84adebf9d64d41b72234638b32e0f5','e1ce64d29f2147a5abceaabb888de9b8','942da5c0d84a4dfc8907b7310bd5a23e','b691a70fce0c4f21b16db1e373503d63','4c60a3207fff4cc898c35e179618cc87','93085878f11e486c87894209006f96cf','998c4373d34644ec85254eb13ab4d1ef','d97e1d225700431e8923ecaa5fd6707f','1be3e5425ca647e4b4ba79c5c5474688','30100006')"
                            + " and time>='2018-01-01 00:00:00' and time<='2018-09-30 23:59:59' group by userid";

                        DataTable dtFootProblemNums = SqlHelper.GetDataTable(sql, null);
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

                        //=============问题上报数据-问题类型===============
                        //工业废水排放	养殖污染	违法建设	排水设施	农家乐	建筑废弃物	堆场码头	生活垃圾	工程维护	其他
                        String gyfs = "0";
                        String yzwr = "0";
                        String wfjs = "0";
                        String psss = "0";
                        String njl = "0";
                        String zjfxw="0";
                        String dcmt = "0";
                        String shlj = "0";
                        String gcwh = "0";
                        String other = "0";

                        sql = "select type,count(problemid) nums from ptl_reportproblem where state<>'-1' and reachid in"
                            +"(select id from rm_river_lake where parent_ids like '%AHD030212000000%')"
                            +"and time>='2018-09-01 00:00:00' and time<='2018-09-30 23:59:59'"
                            +"group by type";

                        DataTable dtProblemTypeNums = SqlHelper.GetDataTable(sql, null);
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

                        //===================四个查清====================			
                        //四个查清完成次数
                        sql = "select start_date,count(check_type) nums from"
                            +" ("
                            +"     select start_date,check_type from ptl_fourcheck where user_id='"+userid+"' and create_date>='"+start_time+"' and create_date<='"+end_time+"'"
                            +"     group by start_date,check_type"
                            +" ) group by start_date having count(check_type)>=4";

                        DataTable dtFourCheckNums = SqlHelper.GetDataTable(sql, null);
                        String fourcheckNums = dtFourCheckNums.Rows.Count.ToString();
                        //四个查清问题数
                        sql = "select count(id) nums from ptl_fourcheck where user_id='"+userid+"' and create_date>='"+start_time+"' and create_date<='"+end_time+"'"
                            +" and HAVA_PROBLEM='1'";

                        String fourcheckProblemNums = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();
                        //两岸贯通查清次数
                        sql = "select start_date,count(id) nums from ptl_fourcheck where user_id='"+userid+"' and create_date>='"+start_time+"' and create_date<='"+end_time+"'"
                            +" and check_type='1' group by start_date";

                        DataTable dtLagtNums = SqlHelper.GetDataTable(sql, null);
                        String lagtNums = dtLagtNums.Rows.Count.ToString();

                        //排水口查清次数
                        sql = "select start_date,count(id) nums from ptl_fourcheck where user_id='" + userid + "' and create_date>='" + start_time + "' and create_date<='" + end_time + "'"
                            + " and check_type='4' group by start_date";

                        DataTable dtPskNums = SqlHelper.GetDataTable(sql, null);
                        String pskNums = dtPskNums.Rows.Count.ToString();

                        //散乱污场所查清次数
                        sql = "select start_date,count(id) nums from ptl_fourcheck where user_id='" + userid + "' and create_date>='" + start_time + "' and create_date<='" + end_time + "'"
                            + " and check_type='3' group by start_date";

                        DataTable dtSlwNums = SqlHelper.GetDataTable(sql, null);
                        String slwNums = dtSlwNums.Rows.Count.ToString();

                        //疑似违建查清次数
                        sql = "select start_date,count(id) nums from ptl_fourcheck where user_id='" + userid + "' and create_date>='" + start_time + "' and create_date<='" + end_time + "'"
                            + " and check_type='2' group by start_date";

                        DataTable dtYswjNums = SqlHelper.GetDataTable(sql, null);
                        String yswjNums = dtYswjNums.Rows.Count.ToString();


                        //===============交办问题==================
                        //书面交办问题数			
                        sql = "select count(problemid) nums from ptl_reportproblem where state<>'-1' and reachid in"
                            +" (select id from rm_river_lake where parent_ids like '%AHD030212000000%')"
                            +" and problemid in(select problem_id from ptl_problem_paper_assign)"
                            +" and time>='2018-01-01 00:00:00' and time<='2018-09-30 23:59:59'";

                        DataTable dtPaperProblemNums = SqlHelper.GetDataTable(sql, null);
                        String paperProblemNums = dtPaperProblemNums.Rows[0][0].ToString();

                        //APP交办问题数
                        sql = "select count(problemid) nums from ptl_reportproblem where state<>'-1' and reachid in"
                            + " (select id from rm_river_lake where parent_ids like '%AHD030212000000%')"
                            + " and problemid in(select problem_id from ptl_problem_assigned)"
                            + " and time>='2018-01-01 00:00:00' and time<='2018-09-30 23:59:59'";

                        DataTable dtAppAssignedProblemNums = SqlHelper.GetDataTable(sql, null);
                        String appAssignedProblemNums = dtAppAssignedProblemNums.Rows[0][0].ToString();

                        //书面交办问题办结数
                        sql = "select count(problemid) nums from ptl_reportproblem where state='4' and reachid in"
                            + " (select id from rm_river_lake where parent_ids like '%AHD030212000000%')"
                            + " and problemid in(select problem_id from ptl_problem_paper_assign)"
                            + " and time>='2018-01-01 00:00:00' and time<='2018-09-30 23:59:59'";

                        DataTable dtPaperProblemFinishNums = SqlHelper.GetDataTable(sql, null);
                        String paperProblemFinishNums = dtPaperProblemFinishNums.Rows[0][0].ToString();

                        //APP交办问题办结数
                        sql = "select count(problemid) nums from ptl_reportproblem where state<>'-1' and reachid in"
                            + " (select id from rm_river_lake where parent_ids like '%AHD030212000000%')"
                            + " and problemid in(select problem_id from ptl_problem_assigned)"
                            + " and time>='2018-01-01 00:00:00' and time<='2018-09-30 23:59:59'";

                        DataTable dtAppAssignedProblemFinishNums = SqlHelper.GetDataTable(sql, null);
                        String appAssignedProblemFinishNums = dtAppAssignedProblemFinishNums.Rows[0][0].ToString();

                    }
                }
                //区级河段
                else if (type.Equals("3"))
                {
                    sql = "select id,addvcd_area,addvcd_towm,addvcd_village from sys_user_b where river_type in('7','8','9') and del_flag='0' and "
                        + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                    DataTable dtUserInfo = SqlHelper.GetDataTable(sql, null);

                    for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                    {
                        String userid = dtUserInfo.Rows[j]["id"].ToString();
                        String id = Guid.NewGuid().ToString().Replace("-", "");

                        sql = "Insert into RTD_BASE (ID,ADDVCD_CITY,ADDVCD_AREA,ADDVCD_TOWN,ADDVCD_VILLAGE,RIVER_ID,SECTION_ID,SECTION_TYPE,BANK,USER_ID,CYCLE)"
                            + " values ('" + id + "','" + addvcd_city + "','" + addvcd_area + "','" + addvcd_town + "','" + addvcd_village + "','" + river_id + "','" + section_id + "','" + type + "','" + side + "','" + userid + "','" + cycle + "')";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                    }
                }
                //镇级河段
                else if (type.Equals("14"))
                {
                    sql = "select id,addvcd_area,addvcd_towm,addvcd_village from sys_user_b where river_type in('12','13') and del_flag='0' and "
                        + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                    DataTable dtUserInfo = SqlHelper.GetDataTable(sql, null);

                    for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                    {
                        String userid = dtUserInfo.Rows[j]["id"].ToString();
                        String id = Guid.NewGuid().ToString().Replace("-", "");

                        sql = "Insert into RTD_BASE (ID,ADDVCD_CITY,ADDVCD_AREA,ADDVCD_TOWN,ADDVCD_VILLAGE,RIVER_ID,SECTION_ID,SECTION_TYPE,BANK,USER_ID,CYCLE)"
                            + " values ('" + id + "','" + addvcd_city + "','" + addvcd_area + "','" + addvcd_town + "','" + addvcd_village + "','" + river_id + "','" + section_id + "','" + type + "','" + side + "','" + userid + "','" + cycle + "')";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                    }
                }
                //村级河段
                else if (type.Equals("5"))
                {
                    sql = "select id,addvcd_area,addvcd_towm,addvcd_village from sys_user_b where river_type in('16') and del_flag='0' and "
                        + "id in(select distinct user_id from rm_riverchief_section_r where river_section_id='" + section_id + "' and del_flags='0')";

                    DataTable dtUserInfo = SqlHelper.GetDataTable(sql, null);

                    for (var j = 0; j < dtUserInfo.Rows.Count; j++)
                    {
                        String userid = dtUserInfo.Rows[j]["id"].ToString();
                        String id = Guid.NewGuid().ToString().Replace("-", "");

                        sql = "Insert into RTD_BASE (ID,ADDVCD_CITY,ADDVCD_AREA,ADDVCD_TOWN,ADDVCD_VILLAGE,RIVER_ID,SECTION_ID,SECTION_TYPE,BANK,USER_ID,CYCLE)"
                            + " values ('" + id + "','" + addvcd_city + "','" + addvcd_area + "','" + addvcd_town + "','" + addvcd_village + "','" + river_id + "','" + section_id + "','" + type + "','" + side + "','" + userid + "','" + cycle + "')";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                    }
                }


                lblInfo.Text = dtRiver.Rows[i][0].ToString();
                bgWork.ReportProgress(i + 1);
            }
        }
    }
}
