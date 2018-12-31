using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sunny.Misc;

namespace RiverImport
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //getVillage("440113000000");
            //getTown("440113000000");
            //getArea("440113000000");

            //getVillage("440106000000");
            //getTown("440106000000");
            //getArea("440106000000");

            //getVillage("440112000000");
            //getTown("440112000000");
            //getArea("440112000000");

            //getVillage("440115000000");
            //getTown("440115000000");
            //getArea("440115000000");

            //getVillage("440104000000");
            //getTown("440104000000");
            //getArea("440104000000");

            //getVillage("440105000000");
            //getTown("440105000000");
            //getArea("440105000000");

            //getVillage("440103000000");
            //getTown("440103000000");
            //getArea("440103000000");

            //getVillage("440118000000");
            //getTown("440118000000");
            //getArea("440118000000");

            //getVillage("440114000000");
            //getTown("440114000000");
            //getArea("440114000000");

            //getVillage("440117000000");
            //getTown("440117000000");
            //getArea("440117000000");

            //getVillage("440111000000");
            //getTown("440111000000");
            //getArea("440111000000");

            getAllInOneForVillage();
            //getAllInOneForTown();
        }

        private void getAllInOneForVillage()
        {
             String startTm = "2018-08-20 00:00:00";
            String endTm = "2018-09-02 23:59:59";

            String sql = "select id,name "
                + "from sys_user_b where del_flag='0' and river_type='16' order by addvcd_area,addvcd_towm"; // order by addvcd_area,addvcd_towm // and addvcd_area='440104000000'
            
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();
                String name = dt.Rows[i]["name"].ToString();

                sql = "select (select name from sys_area_b where id=addvcd_area) area,"
                + "(select name from sys_area_b where id=addvcd_towm) town,"
                + "(select name from sys_area_b where id=addvcd_village) village,nvl(create_date,to_date('2017-09-09 00:00:00','yyyy-MM-dd HH24:mi:ss')) in_time from sys_user_b where id='" + userid + "' ";

                DataTable dtPerson = SqlHelper.GetDataTable(sql, null);
                String area = dtPerson.Rows[0]["area"].ToString();
                String town = dtPerson.Rows[0]["town"].ToString();
                String village = dtPerson.Rows[0]["village"].ToString();
                String in_time = dtPerson.Rows[0]["in_time"].ToString();

                sql = "select count(nums) numss,round(count(nums)/(to_date('2018-09-07 00:00:00','yyyy-MM-dd HH24:mi:ss')-to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss')),2) rate from "
                    +"("
                    +"    select to_char(strtime,'yyyy-MM-dd') tm,count(patrolid) nums from ptl_patrol_r where userid in(select id from sys_user_b where river_type in('16'))"
                    +"    and strtime>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-09-02 00:00:00','yyyy-MM-dd HH24:mi:ss')"
                    +"    and duration>=10 and userid='"+userid+"'"
                    +"    group by to_char(strtime,'yyyy-MM-dd')"
                    +") t order by count(nums) desc";

                DataTable dtPatrol = SqlHelper.GetDataTable(sql, null);

                String patrol_times = dtPatrol.Rows[0]["numss"].ToString();
                String patrol_rate = dtPatrol.Rows[0]["rate"].ToString();

                sql = "select nvl(count(problemid),0) nums from ptl_reportproblem where userid='"+userid+"'" 
                    +" and time>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and time<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss') ";

                DataTable dtProblem = SqlHelper.GetDataTable(sql, null);
                String problem_nums = dtProblem.Rows[0]["nums"].ToString();

                //四个查清
                int cleanTimes = 0;
                sql = "select distinct river_section_id section_id,(select name || (case to_char(river_side) when '1' then '(左岸)' when '2' then '(右岸)' else '' end) from rm_river_lake where id=river_section_id) name from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0'";

                DataTable dtRiver = SqlHelper.GetDataTable(sql, null);

                String cqStart_time = "2018-07-16 00:00:00";
                String cqEnd_time = "2018-07-29 23:59:00";
                Boolean if_finish = true;
                for (int idx = 0; idx < dtRiver.Rows.Count; idx++)
                {
                    String section_id = dtRiver.Rows[idx]["section_id"].ToString();
                    String section_name = dtRiver.Rows[idx]["name"].ToString();

                    //两岸通道贯通是否查清 1--查清  0--未查清
                    String not_connect = "0";
                    //sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                    //    + " and user_id='" + userid + "' and check_type='1' and (hava_problem=0 or (hava_problem>=1 and section_id='" + section_id + "'))";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='1')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        not_connect = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                    //疑似违法建筑是否查清
                    String illegal_building = "0";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='2')";

                    //sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                    //    + " and user_id='" + userid + "' and check_type='2' and (hava_problem=0 or (hava_problem>=1 and section_id='" + section_id + "'))";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        illegal_building = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                    //散乱污场所是否查清
                    String debunching_place = "0";

                    //sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                    //    + " and user_id='" + userid + "' and check_type='3' and (hava_problem=0 or (hava_problem>=1 and section_id='" + section_id + "'))";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='3')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        debunching_place = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                    //异常排水口是否查清
                    String outfall_exception = "0";

                    //sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                    //    + " and user_id='" + userid + "' and check_type='4' and (hava_problem=0 or (hava_problem>=1 and section_id='" + section_id + "'))";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='4')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        outfall_exception = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                }

                if (if_finish)
                    cleanTimes++;

                if_finish = true;
                cqStart_time = "2018-07-30 00:00:00";
                cqEnd_time = "2018-08-12 23:59:59";
                for (int idx = 0; idx < dtRiver.Rows.Count; idx++)
                {
                    String section_id = dtRiver.Rows[idx]["section_id"].ToString();
                    String section_name = dtRiver.Rows[idx]["name"].ToString();

                    //两岸通道贯通是否查清 1--查清  0--未查清
                    String not_connect = "0";
                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='1')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        not_connect = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                    //疑似违法建筑是否查清
                    String illegal_building = "0";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='2')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        illegal_building = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                    //散乱污场所是否查清
                    String debunching_place = "0";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='3')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        debunching_place = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                    //异常排水口是否查清
                    String outfall_exception = "0";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='4')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        outfall_exception = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                }

                if (if_finish)
                    cleanTimes++;

                if_finish = true;
                cqStart_time = "2018-08-13 00:00:00";
                cqEnd_time = "2018-08-26 23:59:59";
                for (int idx = 0; idx < dtRiver.Rows.Count; idx++)
                {
                    String section_id = dtRiver.Rows[idx]["section_id"].ToString();
                    String section_name = dtRiver.Rows[idx]["name"].ToString();

                    //两岸通道贯通是否查清 1--查清  0--未查清
                    String not_connect = "0";
                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='1')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        not_connect = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                    //疑似违法建筑是否查清
                    String illegal_building = "0";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='2')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        illegal_building = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                    //散乱污场所是否查清
                    String debunching_place = "0";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='3')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        debunching_place = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                    //异常排水口是否查清
                    String outfall_exception = "0";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='4')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        outfall_exception = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                }

                if (if_finish)
                    cleanTimes++;

                if_finish = true;
                cqStart_time = "2018-08-27 00:00:00";
                cqEnd_time = "2018-09-09 23:59:59";
                for (int idx = 0; idx < dtRiver.Rows.Count; idx++)
                {
                    String section_id = dtRiver.Rows[idx]["section_id"].ToString();
                    String section_name = dtRiver.Rows[idx]["name"].ToString();

                    //两岸通道贯通是否查清 1--查清  0--未查清
                    String not_connect = "0";
                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='1')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        not_connect = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                    //疑似违法建筑是否查清
                    String illegal_building = "0";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='2')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        illegal_building = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                    //散乱污场所是否查清
                    String debunching_place = "0";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='3')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        debunching_place = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                    //异常排水口是否查清
                    String outfall_exception = "0";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='4')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        outfall_exception = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                }

                if (if_finish)
                    cleanTimes++;

                //履职评价
                sql = "select '1' as times,round(avg(score),2)+60 score from "
                    +" ("
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-07-09 00:00:00','yyyy-MM-dd HH24:mi:ss') and" 
                    +"     user_id='"+userid+"' group by SECTION_ID"
                    +" )"
                    +" union"
                    +" select '2' as times,round(avg(score),2)+60 score from"
                    +" ("
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-07-16 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    +" )"
                    +" union"
                    +" select '3' as times,round(avg(score),2)+60 score from"
                    +" ("
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-07-23 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    +" )"
                    +" union"
                    +" select '4' as times,round(avg(score),2)+60 score from"
                    +" ("
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-07-30 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    +" )"
                    +" union"
                    +" select '5' as times,round(avg(score),2)+60 score from"
                    +" ( "
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-08-06 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID "
                    +" ) "
                    +" union"
                    +" select '6' as times,round(avg(score),2)+60 score from"
                    +" ("
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-08-13 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    +" )"
                    +" union"
                    +" select '7' as times,round(avg(score),2)+60 score from"
                    +" ( "
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-08-20 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    +" )"
                    + " union"
                    + " select '8' as times,round(avg(score),2)+60 score from"
                    + " ( "
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-08-27 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    + " )";

                DataTable dt187 = SqlHelper.GetDataTable(sql, null);
                String week1 = dt187.Rows[0][1] == DBNull.Value ? "" : dt187.Rows[0][1].ToString();
                String week2 = dt187.Rows[1][1] == DBNull.Value ? "" : dt187.Rows[1][1].ToString();
                String week3 = dt187.Rows[2][1] == DBNull.Value ? "" : dt187.Rows[2][1].ToString();
                String week4 = dt187.Rows[3][1] == DBNull.Value ? "" : dt187.Rows[3][1].ToString();
                String week5 = dt187.Rows[4][1] == DBNull.Value ? "" : dt187.Rows[4][1].ToString();
                String week6 = dt187.Rows[5][1] == DBNull.Value ? "" : dt187.Rows[5][1].ToString();
                String week7 = dt187.Rows[6][1] == DBNull.Value ? "" : dt187.Rows[6][1].ToString();
                String week8 = dt187.Rows[7][1] == DBNull.Value ? "" : dt187.Rows[7][1].ToString();
                
                //市巡查及公众投诉
                sql = "select nvl(count(distinct problemid),0) nums from "
                    +"("
                    +"    select problemid from ptl_reportproblem where userid in(select id from sys_user_b where river_type='21')"
                    + "    and sectionid in(select river_section_id from rm_riverchief_section_r where user_id='" + userid + "')"
                    +"    and time>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and time<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    +"    and state<>'-1'"
                    +"    union"
                    +"    select problemid from ptl_reportproblem where pro_resource='2'"
                    + "    and sectionid in(select river_section_id from rm_riverchief_section_r where user_id='" + userid + "')"
                    + "    and time>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and time<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    +"    and userid not in(select id from sys_user_b where river_type='21')"
                    +"    and state<>'-1'"
                    +")";

                String citizen_report = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();

                //巡河时间过长的次数占有效巡河的比例
                sql = "select"
                    + " round((select count(patrolid) valid_times from ptl_patrol_r where userid='"+userid+"'"
                    + " and strtime>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + " and duration>60)/"
                    + " (select count(patrolid) valid_times from ptl_patrol_r where userid='"+userid+"'"
                    + " and strtime>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + " and duration>=10),2) rate"
                    + " from dual";

                DataTable dtLongTimes = SqlHelper.GetDataTable(sql, null);
                String longtime_patrol_rate = dtLongTimes == null ? "" : dtLongTimes.Rows[0][0].ToString();

                //巡河时间短（15分钟以下）、巡河距离长（4公里以上）占有效巡河比例
                sql = "select"
                    + " round((select count(patrolid) valid_times from ptl_patrol_r where userid='" + userid + "'"
                    + " and strtime>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + " and duration<=15 and distance>4)/"
                    + " (select count(patrolid) valid_times from ptl_patrol_r where userid='" + userid + "'"
                    + " and strtime>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + " and duration>=10),2) rate"
                    + " from dual";

                DataTable dtLongDistance = SqlHelper.GetDataTable(sql, null);
                String longdistance_patrol_rate = dtLongDistance == null ? "" : dtLongDistance.Rows[0][0].ToString();

                richTextBox1.AppendText("村河长姓名：" + name + ",区：" + area + ",镇：" + town + ",村：" + village
                     + ",入职时间：" + in_time + ",达标天数：" + patrol_times + ",达标率：" + patrol_rate
                      + ",问题上报：" + problem_nums + ",四个查清：" + cleanTimes + ",履职评价2018-07-09：" + week1
                       + ",履职评价2018-07-16：" + week2 + ",履职评价2018-07-23：" + week3 + ",履职评价2018-07-30：" + week4
                        + ",履职评价2018-08-06：" + week5 + ",履职评价2018-08-13：" + week6 + ",履职评价2018-08-20：" + week7 + ",履职评价2018-08-27：" + week8
                         + ",市巡查及公众投诉：" + citizen_report + ",巡河时间过长比例：" + longtime_patrol_rate
                         + ",巡河时间短距离长比例：" + longdistance_patrol_rate
                    + "\n");
            }
        }

        private void getAllInOneForTown()
        {
            String startTm = "2018-08-20 00:00:00";
            String endTm = "2018-09-02 23:59:59";

            String sql = "select id,name "
                + "from sys_user_b where del_flag='0' and river_type in('12','13') order by addvcd_area,addvcd_towm"; // order by addvcd_area,addvcd_towm

            DataTable dt = SqlHelper.GetDataTable(sql, null);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();
                String name = dt.Rows[i]["name"].ToString();

                sql = "select (select name from sys_area_b where id=addvcd_area) area,"
                + "(select name from sys_area_b where id=addvcd_towm) town,"
                + "nvl(create_date,to_date('2017-09-09 00:00:00','yyyy-MM-dd HH24:mi:ss')) in_time from sys_user_b where id='" + userid + "' ";

                DataTable dtPerson = SqlHelper.GetDataTable(sql, null);
                String area = dtPerson.Rows[0]["area"].ToString();
                String town = dtPerson.Rows[0]["town"].ToString();
                String in_time = dtPerson.Rows[0]["in_time"].ToString();

                sql = "select count(nums) numss,round(count(nums)/round((to_date('2018-09-02 00:00:00','yyyy-MM-dd HH24:mi:ss')-to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss'))/7,0),2) rate from"
                    +"("
                    +"    select to_char(strtime,'iw') tm,count(patrolid) nums from ptl_patrol_r where userid='"+userid+"'"
                    +"    and strtime>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    +"    group by to_char(strtime,'iw')"
                    +")";

                DataTable dtPatrol = SqlHelper.GetDataTable(sql, null);

                String patrol_times = dtPatrol.Rows[0]["numss"].ToString();
                String patrol_rate = dtPatrol.Rows[0]["rate"].ToString();

                sql = "select nvl(count(problemid),0) nums from ptl_reportproblem where userid='" + userid + "'"
                    + " and time>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and time<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss') ";

                DataTable dtProblem = SqlHelper.GetDataTable(sql, null);
                String problem_nums = dtProblem.Rows[0]["nums"].ToString();

                //四个查清
                int cleanTimes = 0;
                sql = "select distinct river_section_id section_id,(select name || (case to_char(river_side) when '1' then '(左岸)' when '2' then '(右岸)' else '' end) from rm_river_lake where id=river_section_id) name from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0'";

                DataTable dtRiver = SqlHelper.GetDataTable(sql, null);

                String cqStart_time = "2018-07-16 00:00:00";
                String cqEnd_time = "2018-09-16 59:59:00";
                Boolean if_finish = true;
                for (int idx = 0; idx < dtRiver.Rows.Count; idx++)
                {
                    String section_id = dtRiver.Rows[idx]["section_id"].ToString();
                    String section_name = dtRiver.Rows[idx]["name"].ToString();

                    //两岸通道贯通是否查清 1--查清  0--未查清
                    String not_connect = "0";
                    //sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                    //    + " and user_id='" + userid + "' and check_type='1' and (hava_problem=0 or (hava_problem>=1 and section_id='" + section_id + "'))";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='1')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        not_connect = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                    //疑似违法建筑是否查清
                    String illegal_building = "0";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='2')";

                    //sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                    //    + " and user_id='" + userid + "' and check_type='2' and (hava_problem=0 or (hava_problem>=1 and section_id='" + section_id + "'))";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        illegal_building = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                    //散乱污场所是否查清
                    String debunching_place = "0";

                    //sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                    //    + " and user_id='" + userid + "' and check_type='3' and (hava_problem=0 or (hava_problem>=1 and section_id='" + section_id + "'))";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='3')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        debunching_place = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                    //异常排水口是否查清
                    String outfall_exception = "0";

                    //sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                    //    + " and user_id='" + userid + "' and check_type='4' and (hava_problem=0 or (hava_problem>=1 and section_id='" + section_id + "'))";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='4')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        outfall_exception = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                }

                if (if_finish)
                    cleanTimes++;

                //履职评价
                sql = "select '1' as times,round(avg(score),2)+60 score from "
                    + " ("
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-07-09 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    + " )"
                    + " union"
                    + " select '2' as times,round(avg(score),2)+60 score from"
                    + " ("
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-07-16 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    + " )"
                    + " union"
                    + " select '3' as times,round(avg(score),2)+60 score from"
                    + " ("
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-07-23 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    + " )"
                    + " union"
                    + " select '4' as times,round(avg(score),2)+60 score from"
                    + " ("
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-07-30 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    + " )"
                    + " union"
                    + " select '5' as times,round(avg(score),2)+60 score from"
                    + " ( "
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-08-06 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID "
                    + " ) "
                    + " union"
                    + " select '6' as times,round(avg(score),2)+60 score from"
                    + " ("
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-08-13 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    + " )"
                    + " union"
                    + " select '7' as times,round(avg(score),2)+60 score from"
                    + " ( "
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-08-20 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    + " )"
                    + " union"
                    + " select '8' as times,round(avg(score),2)+60 score from"
                    + " ( "
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-08-27 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    + " )";

                DataTable dt187 = SqlHelper.GetDataTable(sql, null);
                String week1 = dt187.Rows[0][1] == DBNull.Value ? "" : dt187.Rows[0][1].ToString();
                String week2 = dt187.Rows[1][1] == DBNull.Value ? "" : dt187.Rows[1][1].ToString();
                String week3 = dt187.Rows[2][1] == DBNull.Value ? "" : dt187.Rows[2][1].ToString();
                String week4 = dt187.Rows[3][1] == DBNull.Value ? "" : dt187.Rows[3][1].ToString();
                String week5 = dt187.Rows[4][1] == DBNull.Value ? "" : dt187.Rows[4][1].ToString();
                String week6 = dt187.Rows[5][1] == DBNull.Value ? "" : dt187.Rows[5][1].ToString();
                String week7 = dt187.Rows[6][1] == DBNull.Value ? "" : dt187.Rows[6][1].ToString();
                String week8 = dt187.Rows[7][1] == DBNull.Value ? "" : dt187.Rows[7][1].ToString();

                //市巡查及公众投诉
                sql = "select nvl(count(distinct problemid),0) nums from "
                    + "("
                    + "    select problemid from ptl_reportproblem where userid in(select id from sys_user_b where river_type='21')"
                    + "    and sectionid in(select river_section_id from rm_riverchief_section_r where user_id='" + userid + "')"
                    + "    and time>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and time<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + "    and state<>'-1'"
                    + "    union"
                    + "    select problemid from ptl_reportproblem where pro_resource='2'"
                    + "    and sectionid in(select river_section_id from rm_riverchief_section_r where user_id='" + userid + "')"
                    + "    and time>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and time<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + "    and userid not in(select id from sys_user_b where river_type='21')"
                    + "    and state<>'-1'"
                    + ")";

                String citizen_report = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();

                //巡河时间过长的次数占有效巡河的比例
                sql = "select"
                    + " round((select count(patrolid) valid_times from ptl_patrol_r where userid='" + userid + "'"
                    + " and strtime>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + " and duration>60)/"
                    + " (select count(patrolid) valid_times from ptl_patrol_r where userid='" + userid + "'"
                    + " and strtime>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + " and duration>=10),2) rate"
                    + " from dual";

                DataTable dtLongTimes = SqlHelper.GetDataTable(sql, null);
                String longtime_patrol_rate = dtLongTimes==null?"":dtLongTimes.Rows[0][0].ToString();

                //巡河时间短（15分钟以下）、巡河距离长（4公里以上）占有效巡河比例
                sql = "select"
                    + " round((select count(patrolid) valid_times from ptl_patrol_r where userid='" + userid + "'"
                    + " and strtime>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + " and duration<=15 and distance>4)/"
                    + " (select count(patrolid) valid_times from ptl_patrol_r where userid='" + userid + "'"
                    + " and strtime>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + " and duration>=10),2) rate"
                    + " from dual";

                DataTable dtLongDistance = SqlHelper.GetDataTable(sql, null);
                String longdistance_patrol_rate = dtLongDistance==null?"":dtLongDistance.Rows[0][0].ToString();

                richTextBox1.AppendText("村河长姓名：" + name + ",区：" + area + ",镇：" + town
                     + ",入职时间：" + in_time + ",达标周数：" + patrol_times + ",达标率：" + patrol_rate
                      + ",问题上报：" + problem_nums + ",四个查清：" + cleanTimes + ",履职评价2018-07-09：" + week1
                       + ",履职评价2018-07-16：" + week2 + ",履职评价2018-07-23：" + week3 + ",履职评价2018-07-30：" + week4
                        + ",履职评价2018-08-06：" + week5 + ",履职评价2018-08-13：" + week6 + ",履职评价2018-08-20：" + week7 + ",履职评价2018-08-27：" + week8
                         + ",市巡查及公众投诉：" + citizen_report + ",巡河时间过长比例：" + longtime_patrol_rate
                         + ",巡河时间短距离长比例：" + longdistance_patrol_rate
                    + "\n");
            }
        }

        private void getAllInOneForArea()
        {
            String startTm = "2018-08-20 00:00:00";
            String endTm = "2018-09-02 23:59:59";

            String sql = "select id,name "
                + "from sys_user_b where del_flag='0' and river_type in('9') order by addvcd_area,addvcd_towm"; // order by addvcd_area,addvcd_towm

            DataTable dt = SqlHelper.GetDataTable(sql, null);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();
                String name = dt.Rows[i]["name"].ToString();

                sql = "select (select name from sys_area_b where id=addvcd_area) area,"
                + "(select name from sys_area_b where id=addvcd_towm) town,"
                + "nvl(create_date,to_date('2017-09-09 00:00:00','yyyy-MM-dd HH24:mi:ss')) in_time from sys_user_b where id='" + userid + "' ";

                DataTable dtPerson = SqlHelper.GetDataTable(sql, null);
                String area = dtPerson.Rows[0]["area"].ToString();
                String in_time = dtPerson.Rows[0]["in_time"].ToString();

                sql = "select count(nums) numss,round(count(nums)/round((to_date('2018-09-02 00:00:00','yyyy-MM-dd HH24:mi:ss')-to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss'))/7,0),2) rate from"
                    + "("
                    + "    select to_char(strtime,'iw') tm,count(patrolid) nums from ptl_patrol_r where userid='" + userid + "'"
                    + "    and strtime>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + "    group by to_char(strtime,'iw')"
                    + ")";

                DataTable dtPatrol = SqlHelper.GetDataTable(sql, null);

                String patrol_times = dtPatrol.Rows[0]["numss"].ToString();
                String patrol_rate = dtPatrol.Rows[0]["rate"].ToString();

                sql = "select nvl(count(problemid),0) nums from ptl_reportproblem where userid='" + userid + "'"
                    + " and time>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and time<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss') ";

                DataTable dtProblem = SqlHelper.GetDataTable(sql, null);
                String problem_nums = dtProblem.Rows[0]["nums"].ToString();

                //四个查清
                int cleanTimes = 0;
                sql = "select distinct river_section_id section_id,(select name || (case to_char(river_side) when '1' then '(左岸)' when '2' then '(右岸)' else '' end) from rm_river_lake where id=river_section_id) name from rm_riverchief_section_r where user_id='" + userid + "' and is_old='0'";

                DataTable dtRiver = SqlHelper.GetDataTable(sql, null);

                String cqStart_time = "2018-07-16 00:00:00";
                String cqEnd_time = "2019-01-16 59:59:00";
                Boolean if_finish = true;
                for (int idx = 0; idx < dtRiver.Rows.Count; idx++)
                {
                    String section_id = dtRiver.Rows[idx]["section_id"].ToString();
                    String section_name = dtRiver.Rows[idx]["name"].ToString();

                    //两岸通道贯通是否查清 1--查清  0--未查清
                    String not_connect = "0";
                    //sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                    //    + " and user_id='" + userid + "' and check_type='1' and (hava_problem=0 or (hava_problem>=1 and section_id='" + section_id + "'))";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='1')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        not_connect = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                    //疑似违法建筑是否查清
                    String illegal_building = "0";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='2')";

                    //sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                    //    + " and user_id='" + userid + "' and check_type='2' and (hava_problem=0 or (hava_problem>=1 and section_id='" + section_id + "'))";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        illegal_building = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                    //散乱污场所是否查清
                    String debunching_place = "0";

                    //sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                    //    + " and user_id='" + userid + "' and check_type='3' and (hava_problem=0 or (hava_problem>=1 and section_id='" + section_id + "'))";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='3')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        debunching_place = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                    //异常排水口是否查清
                    String outfall_exception = "0";

                    //sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                    //    + " and user_id='" + userid + "' and check_type='4' and (hava_problem=0 or (hava_problem>=1 and section_id='" + section_id + "'))";

                    sql = "select count(id) nums from ptl_fourcheck where create_date>=to_date('" + cqStart_time + "','yyyy-MM-dd HH24:mi:ss') and create_date<=to_date('" + cqEnd_time + "','yyyy-MM-dd HH24:mi:ss')"
                        + " and user_id='" + userid + "' and check_type='4')";

                    if (SqlHelper.ExecuteScalar(sql, CommandType.Text, null) > 0)
                        outfall_exception = "1";
                    else
                    {
                        if_finish = false;
                        break;
                    }
                }

                if (if_finish)
                    cleanTimes++;

                //履职评价
                sql = "select '1' as times,round(avg(score),2)+60 score from "
                    + " ("
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-07-09 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    + " )"
                    + " union"
                    + " select '2' as times,round(avg(score),2)+60 score from"
                    + " ("
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-07-16 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    + " )"
                    + " union"
                    + " select '3' as times,round(avg(score),2)+60 score from"
                    + " ("
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-07-23 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    + " )"
                    + " union"
                    + " select '4' as times,round(avg(score),2)+60 score from"
                    + " ("
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-07-30 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    + " )"
                    + " union"
                    + " select '5' as times,round(avg(score),2)+60 score from"
                    + " ( "
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-08-06 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID "
                    + " ) "
                    + " union"
                    + " select '6' as times,round(avg(score),2)+60 score from"
                    + " ("
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-08-13 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    + " )"
                    + " union"
                    + " select '7' as times,round(avg(score),2)+60 score from"
                    + " ( "
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-08-20 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    + " )"
                    + " union"
                    + " select '8' as times,round(avg(score),2)+60 score from"
                    + " ( "
                    + "     select SECTION_ID,sum(score) score from user_score_r_187 where start_date=to_date('2018-08-27 00:00:00','yyyy-MM-dd HH24:mi:ss') and"
                    + "     user_id='" + userid + "' group by SECTION_ID"
                    + " )";

                DataTable dt187 = SqlHelper.GetDataTable(sql, null);
                String week1 = dt187.Rows[0][1] == DBNull.Value ? "" : dt187.Rows[0][1].ToString();
                String week2 = dt187.Rows[1][1] == DBNull.Value ? "" : dt187.Rows[1][1].ToString();
                String week3 = dt187.Rows[2][1] == DBNull.Value ? "" : dt187.Rows[2][1].ToString();
                String week4 = dt187.Rows[3][1] == DBNull.Value ? "" : dt187.Rows[3][1].ToString();
                String week5 = dt187.Rows[4][1] == DBNull.Value ? "" : dt187.Rows[4][1].ToString();
                String week6 = dt187.Rows[5][1] == DBNull.Value ? "" : dt187.Rows[5][1].ToString();
                String week7 = dt187.Rows[6][1] == DBNull.Value ? "" : dt187.Rows[6][1].ToString();
                String week8 = dt187.Rows[7][1] == DBNull.Value ? "" : dt187.Rows[7][1].ToString();

                //市巡查及公众投诉
                sql = "select nvl(count(distinct problemid),0) nums from "
                    + "("
                    + "    select problemid from ptl_reportproblem where userid in(select id from sys_user_b where river_type='21')"
                    + "    and sectionid in(select river_section_id from rm_riverchief_section_r where user_id='" + userid + "')"
                    + "    and time>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and time<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + "    and state<>'-1'"
                    + "    union"
                    + "    select problemid from ptl_reportproblem where pro_resource='2'"
                    + "    and sectionid in(select river_section_id from rm_riverchief_section_r where user_id='" + userid + "')"
                    + "    and time>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and time<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + "    and userid not in(select id from sys_user_b where river_type='21')"
                    + "    and state<>'-1'"
                    + ")";

                String citizen_report = SqlHelper.ExecuteScalar(sql, CommandType.Text, null).ToString();

                //巡河时间过长的次数占有效巡河的比例
                sql = "select"
                    + " round((select count(patrolid) valid_times from ptl_patrol_r where userid='" + userid + "'"
                    + " and strtime>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + " and duration>60)/"
                    + " (select count(patrolid) valid_times from ptl_patrol_r where userid='" + userid + "'"
                    + " and strtime>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + " and duration>=10),2) rate"
                    + " from dual";

                DataTable dtLongTimes = SqlHelper.GetDataTable(sql, null);
                String longtime_patrol_rate = dtLongTimes == null ? "" : dtLongTimes.Rows[0][0].ToString();

                //巡河时间短（15分钟以下）、巡河距离长（4公里以上）占有效巡河比例
                sql = "select"
                    + " round((select count(patrolid) valid_times from ptl_patrol_r where userid='" + userid + "'"
                    + " and strtime>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + " and duration<=15 and distance>4)/"
                    + " (select count(patrolid) valid_times from ptl_patrol_r where userid='" + userid + "'"
                    + " and strtime>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-09-07 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                    + " and duration>=10),2) rate"
                    + " from dual";

                DataTable dtLongDistance = SqlHelper.GetDataTable(sql, null);
                String longdistance_patrol_rate = dtLongDistance == null ? "" : dtLongDistance.Rows[0][0].ToString();

                richTextBox1.AppendText("村河长姓名：" + name + ",区：" + area
                     + ",入职时间：" + in_time + ",达标周数：" + patrol_times + ",达标率：" + patrol_rate
                      + ",问题上报：" + problem_nums + ",四个查清：" + cleanTimes + ",履职评价2018-07-09：" + week1
                       + ",履职评价2018-07-16：" + week2 + ",履职评价2018-07-23：" + week3 + ",履职评价2018-07-30：" + week4
                        + ",履职评价2018-08-06：" + week5 + ",履职评价2018-08-13：" + week6 + ",履职评价2018-08-20：" + week7 + ",履职评价2018-08-27：" + week8
                         + ",市巡查及公众投诉：" + citizen_report + ",巡河时间过长比例：" + longtime_patrol_rate
                         + ",巡河时间短距离长比例：" + longdistance_patrol_rate
                    + "\n");
            }
        }

        private void getNextLevel()
        {
            String startTm = "2018-08-20 00:00:00";
            String endTm = "2018-09-02 23:59:59";

            String sql = "select id,name from sys_user_b where river_type='13' and del_flag='0'"; // and id='d0e2b4d2c8c041afab22a675a6e41391'  and id='50000137'  and id='50000742'
            
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userid = dt.Rows[i]["id"].ToString();
                String name = dt.Rows[i]["name"].ToString();

                //查询用户责任河涌
                sql = "select rivet_rm_id,river_section_id,nvl(is_hc152,'n') is_hc152,nvl(is_hc35,'n') is_hc35 "
                    + "from RM_RIVERCHIEF_SECTION_R t_r left join rm_river_lake t_river on t_r.river_section_id=t_river.id "
                    + "where t_r.user_id='" + userid + "' and t_r.is_old='0' and t_r.rivet_rm_id in "
                    + "(select id from rm_river_lake where is_hc152 is not null or is_hc35 is not null)";

                DataTable dtRiver = SqlHelper.GetDataTable(sql, null);

                if (dtRiver.Rows.Count <= 0)
                    continue;
                //下级村河长巡河达标人数 lower_level_rm_std_cnt
                sql = "select distinct user_id from rm_riverchief_section_r t_r"
                    + "                    left join sys_user_b t_user on t_user.id=t_r.user_id"
                    + "                    where river_section_id in"
                    + "                    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userid + "' and is_old='0'))"
                    + "                and (select river_type from sys_user_b where id=t_user.id)='16'";

                DataTable dtTabeUser = SqlHelper.GetDataTable(sql, null);

                int db_person = 0;
                double sumScore = 0;
                for (int j = 0; j < dtTabeUser.Rows.Count; j++)
                {
                    String low_user = dtTabeUser.Rows[j][0].ToString();

                    sql = "select rm_this_week_score score from appweek_my_score where rm_info_id=(select id from appweek_rm_info where rm_app_id='" + low_user + "' and start_time=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss'))";

                    DataTable dtScore = SqlHelper.GetDataTable(sql, null);
                    Double score = 0;
                    if (dtScore.Rows.Count>0)
                       score = Convert.ToDouble(dtScore.Rows[0][0].ToString());
                    sumScore += score;//分数

                    db_person++;
                }

                double avgScore = sumScore / db_person;

                richTextBox1.AppendText("镇河长姓名：" + name + ",下级河长平均分：" + Math.Round(avgScore, 2) + "\n");
            }
        }

        private void insertChief()
        {
            String sql = "select id from rm_river_lake where del_flag='0'"; //where id='HHD030203002'

            DataTable dt = SqlHelper.GetDataTable(sql, null);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String section_id = dt.Rows[i]["id"].ToString();

                sql = "SELECT DISTINCT a.user_id id,u.name name,u.river_type riverType "
                    + "FROM"
                    + "    rm_riverchief_section_r a"
                    + "    LEFT JOIN sys_user_b u ON u.id = a.user_id "
                    + "WHERE"
                    + "    a.river_section_id IN ("
                    + "        SELECT parent_ids FROM"
                    + "        ("
                    + "            SELECT DISTINCT id,regexp_substr(parent_ids,'[^,]+',1,level) parent_ids"
                    + "            FROM"
                    + "                rm_river_lake"
                    + "            WHERE parent_ids IS NOT NULL AND id ='" + section_id + "'"
                    + "            CONNECT BY level <= regexp_count(parent_ids,',') + 1"
                    + "            AND id = PRIOR id AND PRIOR dbms_random.value IS NOT NULL"
                    + "        ) WHERE parent_ids IS NOT NULL"
                    + "    )";

                DataTable dtChief = SqlHelper.GetDataTable(sql, null);

                String city_chief = null;
                String area_chief = null;
                String town_chief = null;
                String village_chief = null;
                for (var j = 0; j < dtChief.Rows.Count; j++)
                {
                    String riverType = dtChief.Rows[j]["riverType"].ToString();
                    String userId = dtChief.Rows[j]["id"].ToString();

                    if (riverType.Equals("4"))
                    {
                        city_chief = userId;
                    }
                    else if (riverType.Equals("7") || riverType.Equals("8") || riverType.Equals("9"))
                    {
                        area_chief = userId;
                    }
                    else if (riverType.Equals("12") || riverType.Equals("13"))
                    {
                        town_chief = userId;
                    }
                    else if (riverType.Equals("16"))
                    {
                        village_chief = userId;
                    }
                }

                String id = Guid.NewGuid().ToString();
                sql = "insert into rm_riverchief_r (id,section_id,city_chief,area_chief,town_chief,village_chief) values('" + id + "','" + section_id + "','" + city_chief + "','" + area_chief + "','" + town_chief + "','" + village_chief + "')";

                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
            }
        }

        private void getTown(String addvcd)
        {
            String sql = " select to_char(strtime,'iw') tm,count(distinct userid) nums from (select to_char(userid) userid,strtime from ptl_patrol_r union select to_char(user_id) userid,tm strtime from ptl_patrol_other) where userid in(select id from sys_user_b where river_type in('12','13') and addvcd_area='" + addvcd + "')"
                + " and strtime>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-07-01 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                + " group by to_char(strtime,'iw') order by  to_char(strtime,'iw')";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            DateTime dtStart = new DateTime(2017, 12, 31);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Double patrol_person = Convert.ToDouble(dt.Rows[i]["nums"].ToString());
                String day = dt.Rows[i]["tm"].ToString();

                dtStart = dtStart.AddDays(7);
                sql = "select count(id) nums from sys_user_b where (create_date<=to_date('" + dtStart.ToShortDateString() + " 23:59:59','yyyy-MM-dd HH24:mi:ss') or create_date is null) and del_flag='0' and river_type in('12','13') and addvcd_area='"+addvcd+"'";

                DataTable dtPersons = SqlHelper.GetDataTable(sql, null);

                Double totalPerson = Convert.ToDouble(dtPersons.Rows[0][0].ToString());
                Double noPatrolPerson = totalPerson - patrol_person;

                if (noPatrolPerson < 0)
                    noPatrolPerson = 0;

                Double rate = noPatrolPerson / totalPerson;

                richTextBox1.AppendText("周数：" + day + "," + "未巡河人数：" + noPatrolPerson + "," + "总人数：" + totalPerson + ",未巡河率：" + Math.Round(rate, 2) + "\n");
            }
        }

        private void getVillage(String addvcd)
        {
            String sql = "select to_char(strtime,'yyyy-MM-dd') tm,count(distinct userid) nums from (select to_char(userid) userid,strtime from ptl_patrol_r union select to_char(user_id) userid,tm strtime from ptl_patrol_other) where userid in(select id from sys_user_b where river_type in('16') and addvcd_area='" + addvcd + "')"
                + " and strtime>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-07-01 00:00:00','yyyy-MM-dd HH24:mi:ss')"
                + " group by to_char(strtime,'yyyy-MM-dd') order by tm asc";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Double patrol_person = Convert.ToDouble(dt.Rows[i]["nums"].ToString());
                String day = dt.Rows[i]["tm"].ToString();

                sql = "select count(id) nums from sys_user_b where (create_date<=to_date('" + day + " 23:59:59','yyyy-MM-dd HH24:mi:ss') or create_date is null) and del_flag='0' and river_type='16' and addvcd_area='" + addvcd + "'";

                DataTable dtPersons = SqlHelper.GetDataTable(sql, null);

                Double totalPerson = Convert.ToDouble(dtPersons.Rows[0][0].ToString());

                Double noPatrolPerson = totalPerson - patrol_person;

                if (noPatrolPerson < 0)
                    noPatrolPerson = 0;

                Double rate = noPatrolPerson / totalPerson;

                richTextBox1.AppendText("日期：" + day + "," + "未巡河人数：" + noPatrolPerson + "," + "总人数：" + totalPerson + ",未巡河率：" + Math.Round(rate, 2) + "\n");
            }
        }

        private void getArea(String addvcd)
        {
            String sql = "select '1-2' month, count(id) nums from "
                +"("
                +"    select t_user.id,nvl(numss,0) numsss from sys_user_b t_user"
                +"    left join"
                +"    ("
                +"        select userid,nvl(count(nums),0) numss from"
                +"        ("
                + "            select userid,to_char(strtime,'yyyy-MM') tm,count(patrolid) nums from (select patrolid,to_char(userid) userid,strtime from ptl_patrol_r union select id patrolid,to_char(user_id) userid,tm strtime from ptl_patrol_other) where userid in(select id from sys_user_b where river_type in('7','8','9') and addvcd_area='" + addvcd + "')"
                +"            and strtime>=to_date('2018-01-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-02-28 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                +"            group by userid,to_char(strtime,'yyyy-MM')"
                +"        ) t group by userid order by count(nums) desc"
                +"    ) t_numss on t_numss.userid=t_user.id where t_user.river_type in('7','8','9') and t_user.addvcd_area='"+addvcd+"'"
                +") t where t.numsss<1"
                +" union"
                +" select '3-4' month,count(id) nums from"
                +"("
                +"    select t_user.id,nvl(numss,0) numsss from sys_user_b t_user"
                +"    left join"
                +"    ("
                +"        select userid,nvl(count(nums),0) numss from"
                +"        ("
                + "            select userid,to_char(strtime,'yyyy-MM') tm,count(patrolid) nums from (select patrolid,to_char(userid) userid,strtime from ptl_patrol_r union select id patrolid,to_char(user_id) userid,tm strtime from ptl_patrol_other) where userid in(select id from sys_user_b where river_type in('7','8','9') and addvcd_area='" + addvcd + "')"
                +"            and strtime>=to_date('2018-03-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-04-30 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                +"            group by userid,to_char(strtime,'yyyy-MM')"
                +"        ) t group by userid order by count(nums) desc"
                +"    ) t_numss on t_numss.userid=t_user.id where t_user.river_type in('7','8','9') and t_user.addvcd_area='"+addvcd+"'"
                +") t where t.numsss<1"
                +" union"
                +" select '5-6' month, count(id) nums from"
                +"("
                +"    select t_user.id,nvl(numss,0) numsss from sys_user_b t_user"
                +"    left join"
                +"    ("
                +"        select userid,nvl(count(nums),0) numss from"
                +"        ("
                + "            select userid,to_char(strtime,'yyyy-MM') tm,count(patrolid) nums from (select patrolid,to_char(userid) userid,strtime from ptl_patrol_r union select id patrolid,to_char(user_id) userid,tm strtime from ptl_patrol_other) where userid in(select id from sys_user_b where river_type in('7','8','9') and addvcd_area='" + addvcd + "')"
                +"            and strtime>=to_date('2018-05-01 00:00:00','yyyy-MM-dd HH24:mi:ss') and strtime<=to_date('2018-06-30 23:59:59','yyyy-MM-dd HH24:mi:ss')"
                +"            group by userid,to_char(strtime,'yyyy-MM')"
                +"        ) t group by userid order by count(nums) desc"
                +"    ) t_numss on t_numss.userid=t_user.id where t_user.river_type in('7','8','9') and t_user.addvcd_area='"+addvcd+"'"
                +") t where t.numsss<1";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Double noPatrolPerson = Convert.ToDouble(dt.Rows[i]["nums"].ToString());
                String month = dt.Rows[i]["month"].ToString();

                String day = "";
                if (month.Equals("1-2"))
                {
                    day = "2018-02-28";
                }
                else if (month.Equals("3-4"))
                {
                    day = "2018-04-30";
                }
                else if (month.Equals("5-6"))
                {
                    day = "2018-06-30";
                }

                sql = "select count(id) nums from sys_user_b where (create_date<=to_date('" + day + " 23:59:59','yyyy-MM-dd HH24:mi:ss') or create_date is null) and del_flag='0' and river_type in('7','8','9') and addvcd_area='" + addvcd + "'";

                DataTable dtPersons = SqlHelper.GetDataTable(sql, null);

                Double totalPerson = Convert.ToDouble(dtPersons.Rows[0][0].ToString());

                //Double noPatrolPerson = totalPerson - patrol_person;

                Double rate = noPatrolPerson / totalPerson;

                richTextBox1.AppendText("日期：" + day + "," + "未巡河人数：" + noPatrolPerson + "," + "总人数：" + totalPerson + ",未巡河率：" + Math.Round(rate, 2) + "\n");
            }
        }
    }
}
