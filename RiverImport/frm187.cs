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
    public partial class frm187 : Form
    {
        public frm187()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;
        }

        public String startTm = "";
        public String endTm = "";
        public bool auto_mode = false;
        public RichTextBox rtxInfo;
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            startTm = dtpStart.Text;
        }

        private void dtpEnd_ValueChanged(object sender, EventArgs e)
        {
            endTm = dtpEnd.Text;
        }

        public void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (progressBar1.Value < progressBar1.Maximum)
                progressBar1.Value += 1;
        }

        public void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = progressBar1.Maximum;
        }

        public void writeInfo(String msg)
        {
            if(rtxInfo != null)
                rtxInfo.AppendText("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + msg + "\r\n");
        }

        //村级
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

            run187_Village(progressBar1,bgWorkVillage);
            
            lblInfo.Text = "数据处理完成(村级河长)...";
            writeInfo("数据处理完成(村级河长)...");

            if (auto_mode)
            {
                //继续跑镇河长
                btnStartTown_Click(null, null);
            }
        }

        public void run187_Village(ProgressBar pBar,BackgroundWorker bgWork)
        {
            //String startTm = "2018-06-18 00:00:00";
            //String endTm = "2018-06-24 23:59:59";
            String sql = "";

            String week = startTm.Substring(0, 4) + CommonUtils.GetWeekOfYear(Convert.ToDateTime(startTm));

            //sql = "delete from USER_SCORE_B_187 where id in(select score_b_id from USER_SCORE_R_187 where START_DATE=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_DATE=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss'))";
            sql = "delete from user_score_b_187 where cycle='" + week + "'";

            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and user_id='" + lblChiefID.Text.Trim() + "'";

            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

            sql = "delete from USER_SCORE_R_187 where START_DATE=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_DATE=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and user_id='" + lblChiefID.Text.Trim() + "'";

            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

            //return;

            sql = "select id from sys_user_b where river_type='16' and del_flag='0'";// and id='60001483'  and id='60000064'  and id='60000716'  and id='60001185'
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Maximum = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userId = dt.Rows[i]["id"].ToString();

                //查询用户责任河涌
                sql = "select rivet_rm_id,river_section_id,nvl(is_hc152,'n') is_hc152,nvl(is_hc35,'n') is_hc35 "
                    + "from RM_RIVERCHIEF_SECTION_R t_r left join rm_river_lake t_river on t_r.river_section_id=t_river.id "
                    + "where t_r.user_id='" + userId + "' and t_r.is_old='0' and t_r.rivet_rm_id in "
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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                        total += score;

                        sql = "Insert into USER_SCORE_B_187 (ID,USER_ID,CYCLE,SCORE,CREATE_DATE,SCORE_LEVEL,RIVER_ID,SECTION_ID) values "
                            + "('" + b_id + "','" + userId + "','" + week + "'," + total + ",sysdate," + downItem + ",'" + river_id + "','" + section_id + "')";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                        bgWork.ReportProgress(i + 1);
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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                        total += score;

                        sql = "Insert into USER_SCORE_B_187 (ID,USER_ID,CYCLE,SCORE,CREATE_DATE,SCORE_LEVEL,RIVER_ID,SECTION_ID) values "
                            + "('" + b_id + "','" + userId + "','" + week + "'," + total + ",sysdate," + downItem + ",'" + river_id + "','" + section_id + "')";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                        bgWork.ReportProgress(i + 1);
                    }
                }
            }
        }

        //镇级河长
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

            sql = "delete from USER_SCORE_R_187 where START_DATE=to_date('" + startTm + "','yyyy-MM-dd HH24:mi:ss') and end_DATE=to_date('" + endTm.Substring(0, 10) + " 00:00:00','yyyy-MM-dd HH24:mi:ss')";

            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

            sql = "select id from sys_user_b where river_type in('12','13') and del_flag='0'";
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Maximum = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userId = dt.Rows[i]["id"].ToString();

                //查询用户责任河涌
                sql = "select rivet_rm_id,river_section_id,nvl(is_hc152,'n') is_hc152,nvl(is_hc35,'n') is_hc35 "
                    + "from RM_RIVERCHIEF_SECTION_R t_r left join rm_river_lake t_river on t_r.river_section_id=t_river.id "
                    + "where t_r.user_id='" + userId + "' and t_r.is_old='0' and t_r.rivet_rm_id in "
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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                        total += score;

                        //下级河长
                        score = 0;
                        item = "下级河长";

                        //查询有无下级河长
                        sql = "select count(distinct user_id) person_nums from rm_riverchief_section_r where river_section_id in "
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
                                + "    select distinct user_id from rm_riverchief_section_r where river_section_id in "
                                + "    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userId + "' and is_old='0'))"
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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                        total += score;

                        sql = "Insert into USER_SCORE_B_187 (ID,USER_ID,CYCLE,SCORE,CREATE_DATE,SCORE_LEVEL,RIVER_ID,SECTION_ID) values "
                            + "('" + b_id + "','" + userId + "','" + week + "'," + total + ",sysdate," + downItem + ",'" + river_id + "','" + section_id + "')";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                        bgWork.ReportProgress(i + 1);
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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                        total += score;

                        //下级河长
                        score = 0;
                        item = "下级河长";

                        //查询有无下级河长
                        sql = "select count(distinct user_id) person_nums from rm_riverchief_section_r where river_section_id in "
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
                                + "    select distinct user_id from rm_riverchief_section_r where river_section_id in "
                                + "    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userId + "' and is_old='0'))"
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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                        total += score;

                        sql = "Insert into USER_SCORE_B_187 (ID,USER_ID,CYCLE,SCORE,CREATE_DATE,SCORE_LEVEL,RIVER_ID,SECTION_ID) values "
                            + "('" + b_id + "','" + userId + "','" + week + "'," + total + ",sysdate," + downItem + ",'" + river_id + "','" + section_id + "')";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                        bgWork.ReportProgress(i + 1);
                    }
                }
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

            sql = "select id from sys_user_b where river_type in('9') and del_flag='0'"; // and id='3ad82a04abe84702925ce2fef35be9a9'
            if (!String.IsNullOrEmpty(lblChiefID.Text.Trim()))
                sql += " and id='" + lblChiefID.Text.Trim() + "'";

            DataTable dt = SqlHelper.GetDataTable(sql, null);

            pBar.Maximum = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String userId = dt.Rows[i]["id"].ToString();

                //查询用户责任河涌
                sql = "select rivet_rm_id,river_section_id,nvl(is_hc152,'n') is_hc152,nvl(is_hc35,'n') is_hc35 "
                    + "from RM_RIVERCHIEF_SECTION_R t_r left join rm_river_lake t_river on t_r.river_section_id=t_river.id "
                    + "where t_r.user_id='" + userId + "' and t_r.is_old='0' and t_r.rivet_rm_id in "
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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        else if (dtpStart.Value.Month == 3 || dtpStart.Value.Month == 4)
                        {
                            patrolStart_time = dtpStart.Value.Year + "-05-01 00:00:00";
                            patrolEnd_time = dtpStart.Value.Year + "-07-01 00:00:00";
                        }
                        else if (dtpStart.Value.Month == 3 || dtpStart.Value.Month == 4)
                        {
                            patrolStart_time = dtpStart.Value.Year + "-07-01 00:00:00";
                            patrolEnd_time = dtpStart.Value.Year + "-09-01 00:00:00";
                        }
                        else if (dtpStart.Value.Month == 3 || dtpStart.Value.Month == 4)
                        {
                            patrolStart_time = dtpStart.Value.Year + "-09-01 00:00:00";
                            patrolEnd_time = dtpStart.Value.Year + "-11-01 00:00:00";
                        }
                        else if (dtpStart.Value.Month == 3 || dtpStart.Value.Month == 4)
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
                            score = 12;
                            subItem = "X = 100%，加2分";
                        }

                        id = Guid.NewGuid().ToString();
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                        total += score;

                        //下级河长============================
                        score = 0;
                        item = "下级河长";

                        //查询有无下级河长
                        sql = "select count(distinct user_id) person_nums from rm_riverchief_section_r where river_section_id in "
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
                                + "    select distinct user_id from rm_riverchief_section_r where river_section_id in "
                                + "    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userId + "' and is_old='0'))"
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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                        total += score;

                        sql = "Insert into USER_SCORE_B_187 (ID,USER_ID,CYCLE,SCORE,CREATE_DATE,SCORE_LEVEL,RIVER_ID,SECTION_ID) values "
                            + "('" + b_id + "','" + userId + "','" + week + "'," + total + ",sysdate," + downItem + ",'" + river_id + "','" + section_id + "')";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                        bgWork.ReportProgress(i + 1);
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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        else if (dtpStart.Value.Month == 3 || dtpStart.Value.Month == 4)
                        {
                            patrolStart_time = dtpStart.Value.Year + "-05-01 00:00:00";
                            patrolEnd_time = dtpStart.Value.Year + "-07-01 00:00:00";
                        }
                        else if (dtpStart.Value.Month == 3 || dtpStart.Value.Month == 4)
                        {
                            patrolStart_time = dtpStart.Value.Year + "-07-01 00:00:00";
                            patrolEnd_time = dtpStart.Value.Year + "-09-01 00:00:00";
                        }
                        else if (dtpStart.Value.Month == 3 || dtpStart.Value.Month == 4)
                        {
                            patrolStart_time = dtpStart.Value.Year + "-09-01 00:00:00";
                            patrolEnd_time = dtpStart.Value.Year + "-11-01 00:00:00";
                        }
                        else if (dtpStart.Value.Month == 3 || dtpStart.Value.Month == 4)
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
                            score = 12;
                            subItem = "X = 100%，加2分";
                        }

                        id = Guid.NewGuid().ToString();
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                        total += score;

                        //下级河长==========================
                        score = 0;
                        item = "下级河长";

                        //查询有无下级河长
                        sql = "select count(distinct user_id) person_nums from rm_riverchief_section_r where river_section_id in "
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
                                + "    select distinct user_id from rm_riverchief_section_r where river_section_id in "
                                + "    (select id from rm_river_lake where parent_id in(select river_section_id from rm_riverchief_section_r where user_id ='" + userId + "' and is_old='0'))"
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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

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
                        sql = "Insert into USER_SCORE_R_187 (ID,USER_ID,EVALUATION_ITEM,SCORE,CREATE_DATE,SCORE_TYPE,EVALUATION_TYPE,START_DATE,END_DATE,CYCLE,RIVER_ID,SECTION_ID,SCORE_B_ID) values"
                            + "('" + id + "','" + userId + "','" + subItem + "'," + score + ",sysdate,'" + scoreType + "','" + item + "',to_date('" + startTm + "','YYYY-MM-DD HH24:MI:SS'),to_date('" + endTm.Substring(0, 10) + " 00:00:00','YYYY-MM-DD HH24:MI:SS'),'" + week + "','" + river_id + "','" + section_id + "','" + b_id + "')";
                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                        total += score;

                        sql = "Insert into USER_SCORE_B_187 (ID,USER_ID,CYCLE,SCORE,CREATE_DATE,SCORE_LEVEL,RIVER_ID,SECTION_ID) values "
                            + "('" + b_id + "','" + userId + "','" + week + "'," + total + ",sysdate," + downItem + ",'" + river_id + "','" + section_id + "')";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                        bgWork.ReportProgress(i + 1);
                    }
                }
            }
        }

        private void frm187_Load(object sender, EventArgs e)
        {
            dtpStart.Value = DateTime.Now.AddDays(1 - Convert.ToInt16(DateTime.Now.DayOfWeek) - 7);

            dtpEnd.Value = DateTime.Now.AddDays(7 - Convert.ToInt16(DateTime.Now.DayOfWeek) - 7);
        }
    }
}
