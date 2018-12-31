using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using Sunny.Misc;

namespace RiverImport
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //importRiver();
            //importSection_new();
            //importRiverChief();
            //import152();
            //import35();
            //importHuPo();
            //importShuiKu();
            //importRiverChiefForHuPo();
            //importRiverChiefForShuiKu();

            importProvinceRiverChief();
        }

        private void importRiver()
        {
            openFileDialog1.FileName = "模板";
            openFileDialog1.Filter = "Excel文件(*.xls,*.xlsx)|*.xls;*.xlsx";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DataTable dtData = GetDataFromExcelSheet(openFileDialog1.FileName, "河流全属性");
                if (dtData != null && dtData.Rows.Count > 0)
                {
                    DataTable dtArea = SqlHelper.GetDataTable("select id,name from sys_area_b", null);

                    List<Dictionary<String, String>> result = new List<Dictionary<string, string>>();
                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        Dictionary<String, String> temp = new Dictionary<string, string>();

                        DataRow item = dtData.Rows[i];
                        String id = item["河流代码"].ToString();
                        temp.Add("id", id);
                        String name = item["河流名称"].ToString();
                        temp.Add("name", name);

                        String river_length = item["河流长度"].ToString().Trim();
                        temp.Add("river_length", river_length);

                        DataRow[] row;
                        String addvcd_city = "广州市";//item["行政区"].ToString().Trim();
                        if (!String.IsNullOrEmpty(addvcd_city))
                        {
                            row = dtArea.Select("name='" + addvcd_city + "'");
                            if (row != null && row.Length == 1)
                                addvcd_city = row[0][0].ToString();
                            else
                                addvcd_city = null;
                        }
                        else
                        {
                            addvcd_city = null;
                        }

                        temp.Add("addvcd_city", addvcd_city);

                        String addvcd_area = item["行政区"].ToString().Trim();
                        if (!String.IsNullOrEmpty(addvcd_area) && addvcd_area.IndexOf("|")<0)
                        {
                            row = dtArea.Select("name='" + addvcd_area + "'");
                            if (row != null && row.Length == 1)
                                addvcd_area = row[0][0].ToString();
                            else
                                addvcd_area = null;
                        }
                        else
                        {
                            addvcd_area = null;
                        }

                        temp.Add("addvcd_area", addvcd_area);

                        String river_system = item["所属流域名称"].ToString().Trim();
                        temp.Add("river_system", river_system);

                        String start_addr = item["河源位置"].ToString().Trim();//qzAddrs[0];
                        temp.Add("start_addr", start_addr);

                        String end_addr = item["河口位置"].ToString().Trim(); ;
                        temp.Add("end_addr", start_addr);

                        String parent_id = "0";
                        temp.Add("parent_id", parent_id);

                        String parent_ids = id+",";
                        String type = "1";
                        
                        temp.Add("parent_ids", parent_ids);
                        temp.Add("type", type);

                        String del_flag = "0";
                        temp.Add("del_flag", del_flag);

                        String addvcd = null;
                        if (addvcd_area != null)
                            addvcd = addvcd_area;
                        else
                            addvcd = addvcd_city;

                        temp.Add("addvcd", addvcd);

                        String river_level = item["河涌分类"].ToString().Trim();
                        temp.Add("river_level", river_level);

                        String qulity_target = item["水功能区水质目标"].ToString().Trim();
                        temp.Add("qulity_target", qulity_target);

                        String sql = "insert into rm_river_lake(id,name,river_length,addvcd_city,addvcd_area,"
                        + "river_system,start_addr,end_addr,parent_id,parent_ids,type,"
                        + "addvcd,river_level,qulity_target) values('" + id + "','" + name + "','" + river_length + "','" + addvcd_city + "','" + addvcd_area + "','"
                        + river_system + "','" + start_addr + "','" + end_addr + "','" + parent_id + "','" + parent_ids + "','" + type + "','"
                        + addvcd + "','" + river_level + "','" + qulity_target + "')";

                        SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);

                        //String riverchief = item["经纬度"].ToString().Trim();


                        //河段/湖泊类型，1：河流，2：湖泊，3：河流区段，4：河流镇（街）段，5：河流村（居）段，6：湖泊区段，7：湖泊镇（街）段，8：湖泊村（居）段；9：水库

                        result.Add(temp);

                        listBox1.Items.Add("id=" + id + ",name=" + name 
                            + ",river_length=" + river_length + ",addvcd_city=" + addvcd_city
                            + ",addvcd_area=" + addvcd_area 
                             + ",river_system=" + river_system + ",start_addr=" + start_addr + ",end_addr=" + end_addr
                               + ",parent_id=" + parent_id
                               + ",parent_ids=" + parent_ids + ",type=" + type
                               + ",addvcd=" + addvcd + ",river_level=" + river_level + ",qulity_target=" + qulity_target);
                    }

                    //MessageBox.Show(countSd.ToString());


                }
            }
        }

        private void importSection()
        {
            openFileDialog1.FileName = "模板";
            openFileDialog1.Filter = "Excel文件(*.xls,*.xlsx)|*.xls;*.xlsx";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DataTable dtData = GetDataFromExcelSheet(openFileDialog1.FileName, "河流名录");
                if (dtData != null && dtData.Rows.Count > 0)
                {
                    DataTable dtArea = SqlHelper.GetDataTable("select id,name,type,parent_id from sys_area_b", null);
                    //DataTable dtArea = SqlHelper.GetDataTable("select id,name from sys_area_b", null);

                    List<Dictionary<String, String>> result = new List<Dictionary<string, string>>();
                    int countSd = 0;
                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        Dictionary<String, String> temp = new Dictionary<string, string>();

                        DataRow item = dtData.Rows[i];
                        String id = item["广州市编号"].ToString();
                        temp.Add("id", id);
                        String name = item["河流（段）名称"].ToString();
                        temp.Add("name", name);
                        String river_side = item["岸别"].ToString().Trim();

                        switch (river_side)
                        {
                            case "左岸":
                                river_side = "1";
                                break;
                            case "右岸":
                                river_side = "2";
                                break;
                            default:
                                river_side = "0";
                                break;
                        }

                        temp.Add("river_side", river_side);

                        String river_length = item["河段长度（km）"].ToString().Trim();
                        temp.Add("river_length", river_length);

                        DataRow[] row;
                        String addvcd_city = item["市级"].ToString().Trim();
                        if (!String.IsNullOrEmpty(addvcd_city))
                        {
                            row = dtArea.Select("name='" + addvcd_city + "'");
                            if (row != null && row.Length == 1)
                                addvcd_city = row[0][0].ToString();
                            else
                                addvcd_city = null;
                        }
                        else
                        {
                            addvcd_city = null;
                        }

                        temp.Add("addvcd_city", addvcd_city);

                        String addvcd_areaStr = item["县级"].ToString().Trim();
                        String addvcd_area;
                        if (!String.IsNullOrEmpty(addvcd_areaStr))
                        {
                            row = dtArea.Select("name='" + addvcd_areaStr + "'");
                            if (row != null && row.Length == 1)
                                addvcd_area = row[0][0].ToString();
                            else
                                addvcd_area = null;
                        }
                        else
                        {
                            addvcd_area = null;
                        }

                        temp.Add("addvcd_area", addvcd_area);
                        temp.Add("addvcd_areaStr", addvcd_areaStr);

                        String addvcd_townStr = item["镇级"].ToString().Trim();
                        String addvcd_town;
                        if (addvcd_townStr.EndsWith("办"))
                        {
                            addvcd_town = addvcd_townStr.Substring(0, addvcd_townStr.Length - 2);
                        }
                        if (!String.IsNullOrEmpty(addvcd_townStr))
                        {
                            row = dtArea.Select("name like '%" + addvcd_townStr + "%'");
                            if (row != null && row.Length == 1)
                                addvcd_town = row[0][0].ToString();
                            else
                                addvcd_town = null;
                        }
                        else
                        {
                            addvcd_town = null;
                        }
                        temp.Add("addvcd_town", addvcd_town);
                        temp.Add("addvcd_townStr", addvcd_townStr);

                        String addvcd_villageStr = item["村级"].ToString().Trim();
                        String addvcd_village;
                        if (addvcd_villageStr.EndsWith("村"))
                        {
                            addvcd_village = addvcd_villageStr.Substring(0, addvcd_villageStr.Length - 2);
                        }
                        if (!String.IsNullOrEmpty(addvcd_villageStr))
                        {
                            row = dtArea.Select("name like '%" + addvcd_villageStr + "%'");
                            if (row == null || row.Length == 0)
                            {
                                row = dtArea.Select("name like '%" + addvcd_villageStr.Substring(0, 2) + "%' and type='6' and parent_id='" + addvcd_town + "'");
                            }
                            if (row != null && row.Length >= 1)
                                addvcd_village = row[0][0].ToString();
                            else
                                addvcd_village = null;
                        }
                        else
                        {
                            addvcd_village = null;
                        }
                        temp.Add("addvcd_village", addvcd_village);
                        temp.Add("addvcd_villageStr", addvcd_villageStr);

                        String river_system = item["水系"].ToString().Trim();
                        temp.Add("river_system", river_system);

                        String qzAddr = item["河段起止"].ToString().Trim();
                        String[] qzAddrs = qzAddr.Split('，');

                        String start_addr = qzAddrs[0];
                        temp.Add("start_addr", start_addr);
                        String end_addr = null;
                        if (qzAddrs.Length >= 2)
                            end_addr = qzAddrs[1];
                        temp.Add("end_addr", start_addr);

                        String jwd = item["经纬度"].ToString().Trim();
                        String[] jwds = jwd.Split('、');

                        String[] jds = jwds[0].Split(',');

                        String east_longitude = jds[0].Replace("起点(", "").Replace("°", "");
                        temp.Add("east_longitude", east_longitude);
                        String east_latitude = null;
                        if (jds.Length >= 2)
                            east_latitude = jds[1].Replace("°)", "");
                        temp.Add("east_latitude", east_latitude);

                        String parent_id = "0";
                        if (!String.IsNullOrEmpty(addvcd_areaStr) && String.IsNullOrEmpty(addvcd_townStr) && String.IsNullOrEmpty(addvcd_villageStr))//区级
                        {
                            parent_id = id.Split('-')[0];
                        }
                        else if (!String.IsNullOrEmpty(addvcd_areaStr) && !String.IsNullOrEmpty(addvcd_townStr) && String.IsNullOrEmpty(addvcd_villageStr))//镇级
                        {
                            for (int j = i-1; j >=0; j--)
                            {
                                String last_town = result[j]["addvcd_townStr"];
                                String last_id = result[j]["id"];
                                if (String.IsNullOrEmpty(last_town))
                                {
                                    parent_id = last_id;
                                    break;
                                }
                            }
                        }
                        else if (!String.IsNullOrEmpty(addvcd_areaStr) && !String.IsNullOrEmpty(addvcd_townStr) && !String.IsNullOrEmpty(addvcd_villageStr))//村级
                        {
                            for (int j = i - 1; j >= 0; j--)
                            {
                                String last_village = result[j]["addvcd_villageStr"];
                                String last_id = result[j]["id"];
                                if (String.IsNullOrEmpty(last_village))
                                {
                                    parent_id = last_id;
                                    break;
                                }
                            }
                        }
                        temp.Add("parent_id", parent_id);

                        String parent_ids = null;
                        String type = null;
                        if (String.IsNullOrEmpty(addvcd_areaStr))//市段
                        {
                            parent_ids = id + ",";
                            type = "1";
                            countSd++;
                        }
                        else if (!String.IsNullOrEmpty(addvcd_areaStr) && String.IsNullOrEmpty(addvcd_townStr))//区段
                        {
                            //parent_ids = result[i - 1]["id"] + "," + id + ",";
                            parent_ids = parent_id + ",";

                            parent_ids += id + ",";
                            
                            type = "3";
                        }
                        else if (!String.IsNullOrEmpty(addvcd_townStr) && String.IsNullOrEmpty(addvcd_villageStr))//镇段
                        {
                            //parent_ids = result[i - 2]["id"] + "," + result[i - 1]["id"] + "," + id + ",";
                            for (int j = i - 1; j >= 0; j--)
                            {
                                String last_town = result[j]["addvcd_townStr"];
                                String last_ids = result[j]["parent_ids"];
                                if (String.IsNullOrEmpty(last_town))
                                {
                                    parent_ids = last_ids + id + ",";
                                    break;
                                }
                            }

                            type = "4";
                        }
                        else if (!String.IsNullOrEmpty(addvcd_villageStr))//村段
                        {
                            //parent_ids = result[i - 3]["id"] + "," + result[i - 2]["id"] + "," + result[i - 1]["id"] + "," + id + ",";
                            for (int j = i - 1; j >= 0; j--)
                            {
                                String last_village = result[j]["addvcd_villageStr"];
                                String last_ids = result[j]["parent_ids"];
                                if (String.IsNullOrEmpty(last_village))
                                {
                                    parent_ids = last_ids + id + ",";
                                    break;
                                }
                            }

                            type = "5";
                        }

                        temp.Add("parent_ids", parent_ids);
                        temp.Add("type", type);

                        String south_longitude = null;
                        String south_latitude = null;
                        if (jwds.Length >= 2)
                        {
                            String[] wds = jwds[1].Split(',');

                            south_longitude = wds[0].Replace("止点(", "").Replace("°", "");
                            temp.Add("south_longitude", south_longitude);
                            if (wds.Length >= 2)
                                south_latitude = wds[1].Replace("°)", "");
                        }
                        temp.Add("south_latitude", south_latitude);

                        String del_flag = "0";
                        temp.Add("del_flag", del_flag);

                        String addvcd = null;
                        if (addvcd_village != null)
                            addvcd = addvcd_village;
                        else if (addvcd_town != null)
                            addvcd = addvcd_town;
                        else if (addvcd_area != null)
                            addvcd = addvcd_area;
                        else
                            addvcd = addvcd_city;

                        temp.Add("addvcd", addvcd);

                        String river_level = item["备注（河流管理级别"].ToString().Trim();
                        temp.Add("river_level", river_level);

                        String sql = "insert into rm_river_lake(id,name,river_side,river_length,addvcd_city,addvcd_area,addvcd_town,addvcd_village,"
                        + "river_system,start_addr,end_addr,east_longitude,east_latitude,parent_id,parent_ids,type,south_longitude,south_latitude,"
                        + "addvcd,river_level) values('" + id + "','" + name + "','" + river_side + "','" + river_length + "','" + addvcd_city + "','" + addvcd_area + "','" + addvcd_town + "','" + addvcd_village + "','"
                        + river_system + "','" + start_addr + "','" + end_addr + "','" + east_longitude + "','" + east_latitude + "','" + parent_id + "','" + parent_ids + "','" + type + "','" + south_longitude + "','" + south_latitude + "','"
                        + addvcd + "','" + river_level + "')";

                        try
                        {
                            if (addvcd_area != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //String riverchief = item["经纬度"].ToString().Trim();


                        //河段/湖泊类型，1：河流，2：湖泊，3：河流区段，4：河流镇（街）段，5：河流村（居）段，6：湖泊区段，7：湖泊镇（街）段，8：湖泊村（居）段；9：水库

                        result.Add(temp);

                        //listBox1.Items.Add("id=" + id + ",name=" + name + ",river_side=" + river_side
                        //    + ",river_length=" + river_length + ",addvcd_city=" + addvcd_city
                        //    + ",addvcd_area=" + addvcd_area + ",addvcd_town=" + addvcd_town + ",addvcd_village=" + addvcd_village
                        //     + ",river_system=" + river_system + ",start_addr=" + start_addr + ",end_addr=" + end_addr
                        //      + ",east_longitude=" + east_longitude + ",east_latitude=" + east_latitude + ",parent_id=" + parent_id
                        //       + ",parent_ids=" + parent_ids + ",type=" + type + ",south_longitude=" + south_longitude + ",south_latitude=" + south_latitude
                        //       + ",addvcd=" + addvcd + ",river_level=" + river_level);
                    }

                    //MessageBox.Show(countSd.ToString());


                }
            }
        }

        private void importSection_new()
        {
            openFileDialog1.FileName = "模板";
            openFileDialog1.Filter = "Excel文件(*.xls,*.xlsx)|*.xls;*.xlsx";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DataTable dtData = GetDataFromExcelSheet(openFileDialog1.FileName, "河流名录");
                if (dtData != null && dtData.Rows.Count > 0)
                {
                    //DataTable dtArea = SqlHelper.GetDataTable("select id,name,type,parent_id from sys_area_b where 1=1", null);
                    //DataTable dtArea = SqlHelper.GetDataTable("select id,name from sys_area_b", null);

                    List<Dictionary<String, String>> result = new List<Dictionary<string, string>>();
                    int countSd = 0;
                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        Dictionary<String, String> temp = new Dictionary<string, string>();

                        DataRow item = dtData.Rows[i];
                        String code = item["编号"].ToString();

                        String sql = "select id from rm_river_lake where plotting_id='"+code+"'";
                        DataTable dtGZ = SqlHelper.GetDataTable(sql, null);

                        String id = null;
                        if (dtGZ.Rows.Count > 0)
                            id = dtGZ.Rows[0][0].ToString();
                        temp.Add("id", id);

                        String name = item["河流（段）名称"].ToString();
                        temp.Add("name", name);

                        String river_side = item["岸别"].ToString().Trim();

                        switch (river_side)
                        {
                            case "左岸":
                                river_side = "1";
                                break;
                            case "右岸":
                                river_side = "2";
                                break;
                            default:
                                river_side = "0";
                                break;
                        }

                        temp.Add("river_side", river_side);

                        String river_length = item["河段长度（km）"].ToString().Trim();
                        temp.Add("river_length", river_length);

                        DataRow[] row;
                        String addvcd_city = item["市级"].ToString().Trim();
                        if (!String.IsNullOrEmpty(addvcd_city))
                        {
                            //row = dtArea.Select("name='" + addvcd_city + "'");
                            row = SqlHelper.GetDataTable("select id,name,type,parent_id from sys_area_b where name='" + addvcd_city + "'").Select("name='" + addvcd_city + "'");
                            if (row != null && row.Length == 1)
                                addvcd_city = row[0][0].ToString();
                            else
                                addvcd_city = null;
                        }
                        else
                        {
                            addvcd_city = null;
                        }

                        temp.Add("addvcd_city", addvcd_city);

                        String addvcd_areaStr = item["县级"].ToString().Trim();
                        String addvcd_area;
                        if (!String.IsNullOrEmpty(addvcd_areaStr))
                        {
                            //row = dtArea.Select("name='" + addvcd_areaStr + "'");
                            row = SqlHelper.GetDataTable("select id,name,type,parent_id from sys_area_b where name='" + addvcd_areaStr + "'").Select("name='" + addvcd_areaStr + "'");
                            if (row != null && row.Length == 1)
                                addvcd_area = row[0][0].ToString();
                            else
                                addvcd_area = null;
                        }
                        else
                        {
                            addvcd_area = null;
                        }

                        temp.Add("addvcd_area", addvcd_area);
                        temp.Add("addvcd_areaStr", addvcd_areaStr);

                        String addvcd_townStr = item["镇级"].ToString().Trim();
                        String addvcd_town;
                        if (addvcd_townStr.EndsWith("办"))
                        {
                            addvcd_town = addvcd_townStr.Substring(0, addvcd_townStr.Length - 2);
                        }
                        if (!String.IsNullOrEmpty(addvcd_townStr))
                        {
                            //row = dtArea.Select("name like '%" + addvcd_townStr + "%'");
                            row = SqlHelper.GetDataTable("select id,name,type,parent_id from sys_area_b where name like '%" + addvcd_townStr + "%'").Select("name like '%" + addvcd_townStr + "%'");
                            if (row != null && row.Length == 1)
                                addvcd_town = row[0][0].ToString();
                            else
                                addvcd_town = null;
                        }
                        else
                        {
                            addvcd_town = null;
                        }
                        temp.Add("addvcd_town", addvcd_town);
                        temp.Add("addvcd_townStr", addvcd_townStr);

                        String addvcd_villageStr = item["村级"].ToString().Trim();
                        String addvcd_village;
                        if (addvcd_villageStr.EndsWith("村"))
                        {
                            addvcd_village = addvcd_villageStr.Substring(0, addvcd_villageStr.Length - 2);
                        }
                        if (!String.IsNullOrEmpty(addvcd_villageStr))
                        {
                            //row = dtArea.Select("name like '%" + addvcd_villageStr + "%'");
                            row = SqlHelper.GetDataTable("select id,name,type,parent_id from sys_area_b where name like '%" + addvcd_villageStr + "%'").Select("name like '%" + addvcd_villageStr + "%'");
                            if (row == null || row.Length == 0)
                            {
                                //row = dtArea.Select("name like '%" + addvcd_villageStr.Substring(0, 2) + "%' and type='6' and parent_id='" + addvcd_town + "'");
                                row = SqlHelper.GetDataTable("select id,name,type,parent_id from sys_area_b where name like '%" + addvcd_villageStr.Substring(0, 2) + "%' and type='6' and parent_id='" + addvcd_town + "'").Select("name like '%" + addvcd_villageStr.Substring(0, 2) + "%' and type='6' and parent_id='" + addvcd_town + "'");
                            }
                            if (row != null && row.Length == 1)
                                addvcd_village = row[0][0].ToString();
                            else
                                addvcd_village = null;
                        }
                        else
                        {
                            addvcd_village = null;
                        }
                        temp.Add("addvcd_village", addvcd_village);
                        temp.Add("addvcd_villageStr", addvcd_villageStr);

                        String river_system = item["水系"].ToString().Trim();
                        temp.Add("river_system", river_system);

                        String qzAddr = item["河段起止"].ToString().Trim();
                        String[] qzAddrs = qzAddr.Split('，');

                        String start_addr = qzAddrs[0];
                        temp.Add("start_addr", start_addr);
                        String end_addr = null;
                        if (qzAddrs.Length >= 2)
                            end_addr = qzAddrs[1];
                        temp.Add("end_addr", start_addr);

                        String jwd = item["经纬度"].ToString().Trim();
                        String[] jwds = jwd.Split('、');

                        String[] jds = jwds[0].Split('，');
                        if(jds.Length==1)
                            jds = jwds[0].Split(',');

                        String east_longitude = jds[0].Replace("起点(", "").Replace("°", "").Replace("起点（","");
                        temp.Add("east_longitude", east_longitude);
                        String east_latitude = null;
                        if (jds.Length >= 2)
                            east_latitude = jds[1].Replace("°)", "").Replace("°）","");
                        temp.Add("east_latitude", east_latitude);

                        String type = null;
                        if (String.IsNullOrEmpty(addvcd_areaStr))//市段
                        {
                            type = "1";
                        }
                        else if (!String.IsNullOrEmpty(addvcd_areaStr) && String.IsNullOrEmpty(addvcd_townStr))//区段
                        {
                            type = "3";
                        }
                        else if (!String.IsNullOrEmpty(addvcd_townStr) && String.IsNullOrEmpty(addvcd_villageStr))//镇段
                        {
                            type = "4";
                        }
                        else if (!String.IsNullOrEmpty(addvcd_villageStr))//村段
                        {
                            type = "5";
                        }

                        String addvcd = null;
                        if (addvcd_village != null)
                            addvcd = addvcd_village;
                        else if (addvcd_town != null)
                            addvcd = addvcd_town;
                        else if (addvcd_area != null)
                            addvcd = addvcd_area;
                        else
                            addvcd = addvcd_city;

                        if (id == null)
                        {
                            if(type.Equals("1"))
                                sql = "select id from rm_river_lake where name like '%" + name.Replace("广州市段", "") + "%' and type='" + type + "'";
                            else
                                sql = "select id from rm_river_lake where name like '%" + name.Replace("广州市段", "") + "%' and type='" + type + "' and addvcd='" + addvcd + "'";

                            DataTable DtId = SqlHelper.GetDataTable(sql);
                            if (DtId.Rows.Count == 1)
                                id = DtId.Rows[0][0].ToString();
                            else
                                id = code;
                        }

                        String parent_id = "0";
                        if (!String.IsNullOrEmpty(addvcd_areaStr) && String.IsNullOrEmpty(addvcd_townStr) && String.IsNullOrEmpty(addvcd_villageStr))//区级
                        {
                            parent_id = id.Split('-')[0];
                        }
                        else if (!String.IsNullOrEmpty(addvcd_areaStr) && !String.IsNullOrEmpty(addvcd_townStr) && String.IsNullOrEmpty(addvcd_villageStr))//镇级
                        {
                            for (int j = i - 1; j >= 0; j--)
                            {
                                String last_town = result[j]["addvcd_townStr"];
                                String last_id = result[j]["id"];
                                if (String.IsNullOrEmpty(last_town))
                                {
                                    parent_id = last_id;
                                    break;
                                }
                            }
                        }
                        else if (!String.IsNullOrEmpty(addvcd_areaStr) && !String.IsNullOrEmpty(addvcd_townStr) && !String.IsNullOrEmpty(addvcd_villageStr))//村级
                        {
                            for (int j = i - 1; j >= 0; j--)
                            {
                                String last_village = result[j]["addvcd_villageStr"];
                                String last_id = result[j]["id"];
                                if (String.IsNullOrEmpty(last_village))
                                {
                                    parent_id = last_id;
                                    break;
                                }
                            }
                        }
                        temp.Add("parent_id", parent_id);

                        String parent_ids = null;                      
                        if (String.IsNullOrEmpty(addvcd_areaStr))//市段
                        {
                            parent_ids = id + ",";
                            countSd++;
                        }
                        else if (!String.IsNullOrEmpty(addvcd_areaStr) && String.IsNullOrEmpty(addvcd_townStr))//区段
                        {
                            //parent_ids = result[i - 1]["id"] + "," + id + ",";
                            parent_ids = parent_id + ",";

                            parent_ids += id + ",";
                        }
                        else if (!String.IsNullOrEmpty(addvcd_townStr) && String.IsNullOrEmpty(addvcd_villageStr))//镇段
                        {
                            //parent_ids = result[i - 2]["id"] + "," + result[i - 1]["id"] + "," + id + ",";
                            for (int j = i - 1; j >= 0; j--)
                            {
                                String last_town = result[j]["addvcd_townStr"];
                                String last_ids = result[j]["parent_ids"];
                                if (String.IsNullOrEmpty(last_town))
                                {
                                    parent_ids = last_ids + id + ",";
                                    break;
                                }
                            }
                        }
                        else if (!String.IsNullOrEmpty(addvcd_villageStr))//村段
                        {
                            //parent_ids = result[i - 3]["id"] + "," + result[i - 2]["id"] + "," + result[i - 1]["id"] + "," + id + ",";
                            for (int j = i - 1; j >= 0; j--)
                            {
                                String last_village = result[j]["addvcd_villageStr"];
                                String last_ids = result[j]["parent_ids"];
                                if (String.IsNullOrEmpty(last_village))
                                {
                                    parent_ids = last_ids + id + ",";
                                    break;
                                }
                            }
                        }

                        temp.Add("parent_ids", parent_ids);
                        temp.Add("type", type);

                        String south_longitude = null;
                        String south_latitude = null;
                        if (jwds.Length >= 2)
                        {
                            String[] wds = jwds[1].Split(',');
                            if (wds.Length < 2)
                                wds = jwds[1].Split('，');
                            south_longitude = wds[0].Replace("止点(", "").Replace("°", "").Replace("止点（","");
                            temp.Add("south_longitude", south_longitude);
                            if (wds.Length >= 2)
                                south_latitude = wds[1].Replace("°)", "").Replace("°）","");
                        }
                        temp.Add("south_latitude", south_latitude);

                        String del_flag = "0";
                        temp.Add("del_flag", del_flag);

                        temp.Add("addvcd", addvcd);

                        String river_level = item["备注(河流管理级别)"].ToString().Trim();
                        temp.Add("river_level", river_level);

                        String rain_area = item["集雨面积（km2）"].ToString();
                        String province_id = item["省级编码"].ToString();

                        String cityCode = item["市编码"].ToString();
                        if (!String.IsNullOrEmpty(cityCode))
                        {
                            id = cityCode;
                        }

                        sql = "insert into rm_river_lake_back(id,name,river_side,river_length,addvcd_city,addvcd_area,addvcd_town,addvcd_village,"
                        + "river_system,start_addr,end_addr,east_longitude,east_latitude,parent_id,parent_ids,type,south_longitude,south_latitude,"
                        + "addvcd,river_level,lake_area,plotting_id,province_id) values('" + id + "','" + name + "','" + river_side + "','" + river_length + "','" + addvcd_city + "','" + addvcd_area + "','" + addvcd_town + "','" + addvcd_village + "','"
                        + river_system + "','" + start_addr + "','" + end_addr + "','" + east_longitude + "','" + east_latitude + "','" + parent_id + "','" + parent_ids + "','" + type + "','" + south_longitude + "','" + south_latitude + "','"
                        + addvcd + "','" + river_level + "','" + rain_area + "','" + code + "','" + province_id + "')";

                        try
                        {
                            //if (addvcd_area != null)

                            if (code.LastIndexOf("A") >= 0)
                            {
                                rtbInfo.AppendText("code=" + code + "\n");
                            }
                            else
                            {
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                            }
                        }
                        catch (Exception)
                        {
                            //listBox1.Items.Add("code=" + code+" : "+sql);
                            rtbInfo.AppendText("code=" + code+" : "+sql + "\n");
                            //throw;
                        }

                        //String riverchief = item["经纬度"].ToString().Trim();


                        //河段/湖泊类型，1：河流，2：湖泊，3：河流区段，4：河流镇（街）段，5：河流村（居）段，6：湖泊区段，7：湖泊镇（街）段，8：湖泊村（居）段；9：水库

                        result.Add(temp);

                        //listBox1.Items.Add("id=" + id + ",name=" + name + ",river_side=" + river_side
                        //    + ",river_length=" + river_length + ",addvcd_city=" + addvcd_city
                        //    + ",addvcd_area=" + addvcd_area + ",addvcd_town=" + addvcd_town + ",addvcd_village=" + addvcd_village
                        //     + ",river_system=" + river_system + ",start_addr=" + start_addr + ",end_addr=" + end_addr
                        //      + ",east_longitude=" + east_longitude + ",east_latitude=" + east_latitude + ",parent_id=" + parent_id
                        //       + ",parent_ids=" + parent_ids + ",type=" + type + ",south_longitude=" + south_longitude + ",south_latitude=" + south_latitude
                        //       + ",addvcd=" + addvcd + ",river_level=" + river_level);
                    }

                    //MessageBox.Show(countSd.ToString());


                }
            }
        }

        private void importHuPo()
        {
            openFileDialog1.FileName = "模板";
            openFileDialog1.Filter = "Excel文件(*.xls,*.xlsx)|*.xls;*.xlsx";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DataTable dtData = GetDataFromExcelSheet(openFileDialog1.FileName, "湖泊名录");
                if (dtData != null && dtData.Rows.Count > 0)
                {
                    DataTable dtArea = SqlHelper.GetDataTable("select id,name from sys_area_b", null);

                    List<Dictionary<String, String>> result = new List<Dictionary<string, string>>();
                    int countSd = 0;
                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        Dictionary<String, String> temp = new Dictionary<string, string>();

                        DataRow item = dtData.Rows[i];
                        String id = item["广州市编号"].ToString();
                        temp.Add("id", id);
                        String name = item["湖泊名称"].ToString();
                        temp.Add("name", name);

                        DataRow[] row;
                        String addvcd_city = item["市"].ToString().Trim();
                        if (!String.IsNullOrEmpty(addvcd_city))
                        {
                            row = dtArea.Select("name='" + addvcd_city + "'");
                            if (row != null && row.Length == 1)
                                addvcd_city = row[0][0].ToString();
                            else
                                addvcd_city = null;
                        }
                        else
                        {
                            addvcd_city = null;
                        }

                        temp.Add("addvcd_city", addvcd_city);

                        String addvcd_area = item["县（区）"].ToString().Trim();
                        if (!String.IsNullOrEmpty(addvcd_area))
                        {
                            row = dtArea.Select("name='" + addvcd_area + "'");
                            if (row != null && row.Length == 1)
                                addvcd_area = row[0][0].ToString();
                            else
                                addvcd_area = null;
                        }
                        else
                        {
                            addvcd_area = null;
                        }

                        temp.Add("addvcd_area", addvcd_area);

                        String addvcd_town = item["镇（街）"].ToString().Trim();
                        if (addvcd_town.EndsWith("办"))
                        {
                            addvcd_town = addvcd_town.Substring(0, addvcd_town.Length - 2);
                        }
                        if (!String.IsNullOrEmpty(addvcd_town))
                        {
                            row = dtArea.Select("name like '%" + addvcd_town + "%'");
                            if (row != null && row.Length == 1)
                                addvcd_town = row[0][0].ToString();
                            else
                                addvcd_town = null;
                        }
                        else
                        {
                            addvcd_town = null;
                        }
                        temp.Add("addvcd_town", addvcd_town);

                        String addvcd_village = item["村（居委会）"].ToString().Trim();
                        if (addvcd_village.EndsWith("村"))
                        {
                            addvcd_village = addvcd_village.Substring(0, addvcd_village.Length - 2);
                        }
                        if (!String.IsNullOrEmpty(addvcd_village))
                        {
                            row = dtArea.Select("name like '%" + addvcd_village + "%'");
                            if (row != null && row.Length >= 1)
                                addvcd_village = row[0][0].ToString();
                            else
                                addvcd_village = null;
                        }
                        else
                        {
                            addvcd_village = null;
                        }
                        temp.Add("addvcd_village", addvcd_village);

                        String river_system = item["所在河流（水系）名称"].ToString().Trim();
                        temp.Add("river_system", river_system);

                        String lake_area = item["常年水面面积（km2）"].ToString().Trim();
                        String lake_max_depth = item["最大水深（m）"].ToString().Trim();

                        String lake_is_salt = item["咸淡水属性"].ToString().Trim();

                        if (String.IsNullOrEmpty(lake_is_salt))
                            lake_is_salt = null;
                        else if (lake_is_salt == "淡水")
                            lake_is_salt = "0";
                        else
                            lake_is_salt = "1";

                        String east_longitude = item["最东点坐标经度"].ToString().Trim().Replace("°", "");
                        String east_latitude = item["最东点坐标纬度"].ToString().Trim().Replace("°", "");
                        String south_longitude = item["最南点坐标经度"].ToString().Trim().Replace("°", "");
                        String south_latitude = item["最南点坐标纬度"].ToString().Trim().Replace("°", "");
                        String west_longitude = item["最西点坐标经度"].ToString().Trim().Replace("°", "");
                        String west_latitude = item["最西点坐标纬度"].ToString().Trim().Replace("°", "");
                        String north_longitude = item["最北点坐标经度"].ToString().Trim().Replace("°", "");
                        String north_latitude = item["最北点坐标纬度"].ToString().Trim().Replace("°", "");

                        String parent_id = "0";
                        if (name.Equals("凤凰湖"))
                        {
                            parent_id = "HHC011833001";
                        }
                        temp.Add("parent_id", parent_id);

                        String parent_ids = id + ",";
                        if (name.Equals("凤凰湖"))
                        {
                            parent_ids = "HHC011833001," + parent_ids;
                        }

                        String type = "2";
                        
                        temp.Add("parent_ids", parent_ids);
                        temp.Add("type", type);

                        String del_flag = "0";
                        temp.Add("del_flag", del_flag);

                        String addvcd = null;
                        if (addvcd_village != null)
                            addvcd = addvcd_village;
                        else if (addvcd_town != null)
                            addvcd = addvcd_town;
                        else if (addvcd_area != null)
                            addvcd = addvcd_area;
                        else
                            addvcd = addvcd_city;

                        temp.Add("addvcd", addvcd);

                        String sql = "insert into rm_river_lake(id,name,addvcd_city,addvcd_area,addvcd_town,addvcd_village,"
                        + "river_system,east_longitude,east_latitude,parent_id,parent_ids,type,south_longitude,south_latitude,"
                        + "addvcd,west_longitude,west_latitude,north_longitude,north_latitude,del_flag) values('" + id + "','" + name + "','" + addvcd_city + "','" + addvcd_area + "','" + addvcd_town + "','" + addvcd_village + "','"
                        + river_system + "','" + east_longitude + "','" + east_latitude + "','" + parent_id + "','" + parent_ids + "','" + type + "','" + south_longitude + "','" + south_latitude + "','"
                        + addvcd + "','" + west_longitude + "','" + west_latitude + "','" + north_longitude + "','" + north_latitude + "','" + del_flag + "')";

                        try
                        {
                            if (addvcd_area != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //String riverchief = item["经纬度"].ToString().Trim();


                        //河段/湖泊类型，1：河流，2：湖泊，3：河流区段，4：河流镇（街）段，5：河流村（居）段，6：湖泊区段，7：湖泊镇（街）段，8：湖泊村（居）段；9：水库

                        result.Add(temp);

                        //listBox1.Items.Add("id=" + id + ",name=" + name + ",river_side=" + river_side
                        //    + ",river_length=" + river_length + ",addvcd_city=" + addvcd_city
                        //    + ",addvcd_area=" + addvcd_area + ",addvcd_town=" + addvcd_town + ",addvcd_village=" + addvcd_village
                        //     + ",river_system=" + river_system + ",start_addr=" + start_addr + ",end_addr=" + end_addr
                        //      + ",east_longitude=" + east_longitude + ",east_latitude=" + east_latitude + ",parent_id=" + parent_id
                        //       + ",parent_ids=" + parent_ids + ",type=" + type + ",south_longitude=" + south_longitude + ",south_latitude=" + south_latitude
                        //       + ",addvcd=" + addvcd + ",river_level=" + river_level);
                    }

                    //MessageBox.Show(countSd.ToString());


                }
            }
        }

        private void importShuiKu()
        {
            openFileDialog1.FileName = "模板";
            openFileDialog1.Filter = "Excel文件(*.xls,*.xlsx)|*.xls;*.xlsx";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DataTable dtData = GetDataFromExcelSheet(openFileDialog1.FileName, "水库名录");
                if (dtData != null && dtData.Rows.Count > 0)
                {
                    DataTable dtArea = SqlHelper.GetDataTable("select id,name from sys_area_b", null);

                    List<Dictionary<String, String>> result = new List<Dictionary<string, string>>();
                    int countSd = 0;
                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        Dictionary<String, String> temp = new Dictionary<string, string>();

                        DataRow item = dtData.Rows[i];
                        String id = item["编码"].ToString();
                        temp.Add("id", id);
                        String name = item["水库名称"].ToString();
                        temp.Add("name", name);

                        DataRow[] row;
                        String addvcd_city = item["市"].ToString().Trim();
                        if (!String.IsNullOrEmpty(addvcd_city))
                        {
                            row = dtArea.Select("name='" + addvcd_city + "'");
                            if (row != null && row.Length == 1)
                                addvcd_city = row[0][0].ToString();
                            else
                                addvcd_city = null;
                        }
                        else
                        {
                            addvcd_city = null;
                        }

                        temp.Add("addvcd_city", addvcd_city);

                        String addvcd_area = item["县（区）"].ToString().Trim();
                        if (!String.IsNullOrEmpty(addvcd_area))
                        {
                            row = dtArea.Select("name='" + addvcd_area + "'");
                            if (row != null && row.Length == 1)
                                addvcd_area = row[0][0].ToString();
                            else
                                addvcd_area = null;
                        }
                        else
                        {
                            addvcd_area = null;
                        }

                        temp.Add("addvcd_area", addvcd_area);

                        String addvcd_town = item["镇（街）"].ToString().Trim();
                        if (addvcd_town.EndsWith("办"))
                        {
                            addvcd_town = addvcd_town.Substring(0, addvcd_town.Length - 2);
                        }
                        if (!String.IsNullOrEmpty(addvcd_town))
                        {
                            row = dtArea.Select("name like '%" + addvcd_town + "%'");
                            if (row != null && row.Length == 1)
                                addvcd_town = row[0][0].ToString();
                            else
                                addvcd_town = null;
                        }
                        else
                        {
                            addvcd_town = null;
                        }
                        temp.Add("addvcd_town", addvcd_town);

                        String addvcd_village = item["村（居委会）"].ToString().Trim();
                        if (addvcd_village.EndsWith("村"))
                        {
                            addvcd_village = addvcd_village.Substring(0, addvcd_village.Length - 2);
                        }
                        if (!String.IsNullOrEmpty(addvcd_village))
                        {
                            row = dtArea.Select("name like '%" + addvcd_village + "%'");
                            if (row != null && row.Length >= 1)
                                addvcd_village = row[0][0].ToString();
                            else
                                addvcd_village = null;
                        }
                        else
                        {
                            addvcd_village = null;
                        }
                        temp.Add("addvcd_village", addvcd_village);

                        String river_system = item["所在河流名称"].ToString().Trim();
                        temp.Add("river_system", river_system);

                        String capacity = item["总库容（万m3）"].ToString().Trim();
                        String material = item["挡水主坝类型按材料分"].ToString().Trim();

                        String east_longitude = item["经度"].ToString().Trim().Replace("°", "");
                        String east_latitude = item["纬度"].ToString().Trim().Replace("°", "");

                        String parent_id = "0";
                        temp.Add("parent_id", parent_id);

                        String parent_ids = id + ",";

                        String type = "9";

                        temp.Add("parent_ids", parent_ids);
                        temp.Add("type", type);

                        String del_flag = "0";
                        temp.Add("del_flag", del_flag);

                        String addvcd = null;
                        if (addvcd_village != null)
                            addvcd = addvcd_village;
                        else if (addvcd_town != null)
                            addvcd = addvcd_town;
                        else if (addvcd_area != null)
                            addvcd = addvcd_area;
                        else
                            addvcd = addvcd_city;

                        temp.Add("addvcd", addvcd);

                        String sql = "insert into rm_river_lake(id,name,addvcd_city,addvcd_area,addvcd_town,addvcd_village,"
                        + "river_system,east_longitude,east_latitude,parent_id,parent_ids,type,"
                        + "addvcd,capacity,material,del_flag) values('" + id + "','" + name + "','" + addvcd_city + "','" + addvcd_area + "','" + addvcd_town + "','" + addvcd_village + "','"
                        + river_system + "','" + east_longitude + "','" + east_latitude + "','" + parent_id + "','" + parent_ids + "','" + type + "','"
                        + addvcd + "','" + capacity + "','" + material + "','" + del_flag + "')";

                        try
                        {
                            if (addvcd_area != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //String riverchief = item["经纬度"].ToString().Trim();


                        //河段/湖泊类型，1：河流，2：湖泊，3：河流区段，4：河流镇（街）段，5：河流村（居）段，6：湖泊区段，7：湖泊镇（街）段，8：湖泊村（居）段；9：水库

                        result.Add(temp);

                        //listBox1.Items.Add("id=" + id + ",name=" + name + ",river_side=" + river_side
                        //    + ",river_length=" + river_length + ",addvcd_city=" + addvcd_city
                        //    + ",addvcd_area=" + addvcd_area + ",addvcd_town=" + addvcd_town + ",addvcd_village=" + addvcd_village
                        //     + ",river_system=" + river_system + ",start_addr=" + start_addr + ",end_addr=" + end_addr
                        //      + ",east_longitude=" + east_longitude + ",east_latitude=" + east_latitude + ",parent_id=" + parent_id
                        //       + ",parent_ids=" + parent_ids + ",type=" + type + ",south_longitude=" + south_longitude + ",south_latitude=" + south_latitude
                        //       + ",addvcd=" + addvcd + ",river_level=" + river_level);
                    }

                    //MessageBox.Show(countSd.ToString());


                }
            }
        }

        private void import152()
        {
            openFileDialog1.FileName = "模板";
            openFileDialog1.Filter = "Excel文件(*.xls,*.xlsx)|*.xls;*.xlsx";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DataTable dtData = GetDataFromExcelSheet(openFileDialog1.FileName, "152");
                if (dtData != null && dtData.Rows.Count > 0)
                {
                    //DataTable dtArea = SqlHelper.GetDataTable("select id,name from sys_area_b", null);

                    List<Dictionary<String, String>> result = new List<Dictionary<string, string>>();
                    int countSd = 0;
                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        Dictionary<String, String> temp = new Dictionary<string, string>();

                        DataRow item = dtData.Rows[i];
                        String id = item["河流代码"].ToString();
                        temp.Add("id", id);
                        String no = item["152编号"].ToString();
                        temp.Add("name", no);

                        String sql = "update rm_river_lake set IS_HC152='"+no+"' where id='" + id + "'";

                        try
                        {
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //String riverchief = item["经纬度"].ToString().Trim();


                        //河段/湖泊类型，1：河流，2：湖泊，3：河流区段，4：河流镇（街）段，5：河流村（居）段，6：湖泊区段，7：湖泊镇（街）段，8：湖泊村（居）段；9：水库

                        result.Add(temp);

                        //listBox1.Items.Add("id=" + id + ",name=" + name + ",river_side=" + river_side
                        //    + ",river_length=" + river_length + ",addvcd_city=" + addvcd_city
                        //    + ",addvcd_area=" + addvcd_area + ",addvcd_town=" + addvcd_town + ",addvcd_village=" + addvcd_village
                        //     + ",river_system=" + river_system + ",start_addr=" + start_addr + ",end_addr=" + end_addr
                        //      + ",east_longitude=" + east_longitude + ",east_latitude=" + east_latitude + ",parent_id=" + parent_id
                        //       + ",parent_ids=" + parent_ids + ",type=" + type + ",south_longitude=" + south_longitude + ",south_latitude=" + south_latitude
                        //       + ",addvcd=" + addvcd + ",river_level=" + river_level);
                    }

                    //MessageBox.Show(countSd.ToString());


                }
            }
        }

        private void import35()
        {
            openFileDialog1.FileName = "模板";
            openFileDialog1.Filter = "Excel文件(*.xls,*.xlsx)|*.xls;*.xlsx";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DataTable dtData = GetDataFromExcelSheet(openFileDialog1.FileName, "河流全属性");
                if (dtData != null && dtData.Rows.Count > 0)
                {
                    //DataTable dtArea = SqlHelper.GetDataTable("select id,name from sys_area_b", null);

                    List<Dictionary<String, String>> result = new List<Dictionary<string, string>>();
                    int countSd = 0;
                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        Dictionary<String, String> temp = new Dictionary<string, string>();

                        DataRow item = dtData.Rows[i];
                        String id = item["河流代码"].ToString();
                        temp.Add("id", id);
                        String no = item["35条黑臭河涌编号"].ToString().Trim();
                        temp.Add("name", no);

                        if (String.IsNullOrEmpty(no) || no == "<Null>")
                        {
                            continue;
                        }

                        String sql = "update rm_river_lake set IS_HC35='" + no + "' where id='" + id + "'";

                        try
                        {
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //String riverchief = item["经纬度"].ToString().Trim();


                        //河段/湖泊类型，1：河流，2：湖泊，3：河流区段，4：河流镇（街）段，5：河流村（居）段，6：湖泊区段，7：湖泊镇（街）段，8：湖泊村（居）段；9：水库

                        result.Add(temp);

                        //listBox1.Items.Add("id=" + id + ",name=" + name + ",river_side=" + river_side
                        //    + ",river_length=" + river_length + ",addvcd_city=" + addvcd_city
                        //    + ",addvcd_area=" + addvcd_area + ",addvcd_town=" + addvcd_town + ",addvcd_village=" + addvcd_village
                        //     + ",river_system=" + river_system + ",start_addr=" + start_addr + ",end_addr=" + end_addr
                        //      + ",east_longitude=" + east_longitude + ",east_latitude=" + east_latitude + ",parent_id=" + parent_id
                        //       + ",parent_ids=" + parent_ids + ",type=" + type + ",south_longitude=" + south_longitude + ",south_latitude=" + south_latitude
                        //       + ",addvcd=" + addvcd + ",river_level=" + river_level);
                    }

                    //MessageBox.Show(countSd.ToString());


                }
            }
        }

        private void importRiverChief()
        {
            openFileDialog1.FileName = "模板";
            openFileDialog1.Filter = "Excel文件(*.xls,*.xlsx)|*.xls;*.xlsx";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DataTable dtData = GetDataFromExcelSheet(openFileDialog1.FileName, "河流名录");
                if (dtData != null && dtData.Rows.Count > 0)
                {
                    DataTable dtArea = SqlHelper.GetDataTable("select id,name from sys_area_b", null);
                    

                    List<Dictionary<String, String>> result = new List<Dictionary<string, string>>();
                    int countSd = 0;
                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        Dictionary<String, String> temp = new Dictionary<string, string>();

                        DataRow item = dtData.Rows[i];
                        String id = item["广州市编号"].ToString();
                        temp.Add("id", id);

                        String name = item["河长姓名"].ToString().Trim();
                        name = name.Replace("  ", "");
                        name = name.Replace(" ", "");
                        DataRow[] row;

                        String addvcd_city = item["市级"].ToString().Trim();
                        if (!String.IsNullOrEmpty(addvcd_city))
                        {
                            row = dtArea.Select("name='" + addvcd_city + "'");
                            if (row != null && row.Length == 1)
                                addvcd_city = row[0][0].ToString();
                            else
                                addvcd_city = null;
                        }
                        else
                        {
                            addvcd_city = null;
                        }

                        temp.Add("addvcd_city", addvcd_city);

                        String addvcd_area = item["县级"].ToString().Trim();
                        if (!String.IsNullOrEmpty(addvcd_area))
                        {
                            row = dtArea.Select("name='" + addvcd_area + "'");
                            if (row != null && row.Length == 1)
                                addvcd_area = row[0][0].ToString();
                            else
                                addvcd_area = null;
                        }
                        else
                        {
                            addvcd_area = null;
                        }

                        temp.Add("addvcd_area", addvcd_area);

                        String addvcd_town = item["镇级"].ToString().Trim();
                        if (addvcd_town.EndsWith("办"))
                        {
                            addvcd_town = addvcd_town.Substring(0, addvcd_town.Length - 2);
                        }
                        if (!String.IsNullOrEmpty(addvcd_town))
                        {
                            row = dtArea.Select("name like '%" + addvcd_town + "%'");
                            if (row != null && row.Length == 1)
                                addvcd_town = row[0][0].ToString();
                            else
                                addvcd_town = null;
                        }
                        else
                        {
                            addvcd_town = null;
                        }
                        temp.Add("addvcd_town", addvcd_town);

                        String addvcd_village = item["村级"].ToString().Trim();
                        if (addvcd_village.EndsWith("村"))
                        {
                            addvcd_village = addvcd_village.Substring(0, addvcd_village.Length - 2);
                        }
                        if (!String.IsNullOrEmpty(addvcd_village))
                        {
                            row = dtArea.Select("name like '%" + addvcd_village + "%'");
                            if (row != null && row.Length >= 1)
                                addvcd_village = row[0][0].ToString();
                            else
                                addvcd_village = null;
                        }
                        else
                        {
                            addvcd_village = null;
                        }
                        temp.Add("addvcd_village", addvcd_village);

                        String sql = "select id,name from sys_user_b where name='" + name + "' and is_riverchief='Y'";
                        if (addvcd_area != null)
                            sql += " and addvcd_area='" + addvcd_area + "'";
                        //if (addvcd_town != null)
                        //    sql += " and addvcd_towm='" + addvcd_town + "'";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_village='" + addvcd_village + "'";

                        DataTable dtUser = SqlHelper.GetDataTable(sql, null);
                        if (dtUser == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            continue;
                        }
                        row = dtUser.Select("name='"+name+"'");
                        if (row == null || row.Length <= 0)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            continue;
                        }
                        temp.Add("name", name);

                        String userId = null;
                        if (row != null && row.Length >= 1)
                            userId = row[0][0].ToString();
                        else
                            userId = null;

                        String river_id = id;
                        if (river_id.IndexOf("-") >= 0)
                        {
                            String[] river_ids = river_id.Split('-');
                            river_id = river_ids[0];
                        }

                        String keyId = System.Guid.NewGuid().ToString("N");
                        sql = "insert into rm_riverchief_section_r(sec_id,river_section_id,rivet_rm_id,user_id)"
                        + " values('" + keyId + "','" + id + "','" + river_id + "','" + userId + "')";

                        if (userId == null)
                        {
                            listBox1.Items.Add("id=" + id+",name="+name);
                            continue;
                        }

                        try
                        {
                            if (userId != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //String riverchief = item["经纬度"].ToString().Trim();


                        //河段/湖泊类型，1：河流，2：湖泊，3：河流区段，4：河流镇（街）段，5：河流村（居）段，6：湖泊区段，7：湖泊镇（街）段，8：湖泊村（居）段；9：水库

                        result.Add(temp);

                        //listBox1.Items.Add("id=" + id + ",name=" + name + ",river_side=" + river_side
                        //    + ",river_length=" + river_length + ",addvcd_city=" + addvcd_city
                        //    + ",addvcd_area=" + addvcd_area + ",addvcd_town=" + addvcd_town + ",addvcd_village=" + addvcd_village
                        //     + ",river_system=" + river_system + ",start_addr=" + start_addr + ",end_addr=" + end_addr
                        //      + ",east_longitude=" + east_longitude + ",east_latitude=" + east_latitude + ",parent_id=" + parent_id
                        //       + ",parent_ids=" + parent_ids + ",type=" + type + ",south_longitude=" + south_longitude + ",south_latitude=" + south_latitude
                        //       + ",addvcd=" + addvcd + ",river_level=" + river_level);
                    }

                    //MessageBox.Show(countSd.ToString());


                }
            }
        }

        private void importRiverChiefForHuPo()
        {
            openFileDialog1.FileName = "模板";
            openFileDialog1.Filter = "Excel文件(*.xls,*.xlsx)|*.xls;*.xlsx";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DataTable dtData = GetDataFromExcelSheet(openFileDialog1.FileName, "湖泊名录");
                if (dtData != null && dtData.Rows.Count > 0)
                {
                    DataTable dtArea = SqlHelper.GetDataTable("select id,name from sys_area_b", null);


                    List<Dictionary<String, String>> result = new List<Dictionary<string, string>>();
                    int countSd = 0;
                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        Dictionary<String, String> temp = new Dictionary<string, string>();

                        DataRow item = dtData.Rows[i];
                        String id = item["广州市编号"].ToString();
                        temp.Add("id", id);

                        String name = item["市级总河长信息姓名"].ToString().Trim();
                        name = name.Replace("  ", "");
                        name = name.Replace(" ", "");
                        DataRow[] row;

                        String addvcd_city = item["市"].ToString().Trim();
                        if (!String.IsNullOrEmpty(addvcd_city))
                        {
                            row = dtArea.Select("name='" + addvcd_city + "'");
                            if (row != null && row.Length == 1)
                                addvcd_city = row[0][0].ToString();
                            else
                                addvcd_city = null;
                        }
                        else
                        {
                            addvcd_city = null;
                        }

                        temp.Add("addvcd_city", addvcd_city);

                        String addvcd_area = item["县（区）"].ToString().Trim();
                        if (!String.IsNullOrEmpty(addvcd_area))
                        {
                            row = dtArea.Select("name='" + addvcd_area + "'");
                            if (row != null && row.Length == 1)
                                addvcd_area = row[0][0].ToString();
                            else
                                addvcd_area = null;
                        }
                        else
                        {
                            addvcd_area = null;
                        }

                        temp.Add("addvcd_area", addvcd_area);

                        String addvcd_town = item["镇（街）"].ToString().Trim();
                        if (addvcd_town.EndsWith("办"))
                        {
                            addvcd_town = addvcd_town.Substring(0, addvcd_town.Length - 2);
                        }
                        if (!String.IsNullOrEmpty(addvcd_town))
                        {
                            row = dtArea.Select("name like '%" + addvcd_town + "%'");
                            if (row != null && row.Length == 1)
                                addvcd_town = row[0][0].ToString();
                            else
                                addvcd_town = null;
                        }
                        else
                        {
                            addvcd_town = null;
                        }
                        temp.Add("addvcd_town", addvcd_town);

                        String addvcd_village = item["村（居委会）"].ToString().Trim();
                        if (addvcd_village.EndsWith("村"))
                        {
                            addvcd_village = addvcd_village.Substring(0, addvcd_village.Length - 2);
                        }
                        if (!String.IsNullOrEmpty(addvcd_village))
                        {
                            row = dtArea.Select("name like '%" + addvcd_village + "%'");
                            if (row != null && row.Length >= 1)
                                addvcd_village = row[0][0].ToString();
                            else
                                addvcd_village = null;
                        }
                        else
                        {
                            addvcd_village = null;
                        }
                        temp.Add("addvcd_village", addvcd_village);

                        String sql = "select id,name from sys_user_b where name='" + name + "' and river_type not in('17','25','23','22','19','18')";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_area='" + addvcd_area + "'";
                        //if (addvcd_town != null)
                        //    sql += " and addvcd_towm='" + addvcd_town + "'";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_village='" + addvcd_village + "'";

                        DataTable dtUser = SqlHelper.GetDataTable(sql, null);
                        if (dtUser == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        row = dtUser.Select("name='" + name + "'");
                        if (row == null || row.Length <= 0)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        temp.Add("name", name);

                        String userId = null;
                        if (row != null && row.Length >= 1)
                            userId = row[0][0].ToString();
                        else
                            userId = null;

                        String river_id = id;
                        if (river_id.IndexOf("-") >= 0)
                        {
                            String[] river_ids = river_id.Split('-');
                            river_id = river_ids[0];
                        }

                        String keyId = System.Guid.NewGuid().ToString("N");
                        sql = "insert into rm_riverchief_section_r(sec_id,river_section_id,rivet_rm_id,user_id)"
                        + " values('" + keyId + "','" + id + "','" + river_id + "','" + userId + "')";

                        if (userId == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }

                        try
                        {
                            if (userId != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //=========================
                        name = item["市级副总河长信息姓名"].ToString().Trim();
                        name = name.Replace("  ", "");
                        name = name.Replace(" ", "");

                        sql = "select id,name from sys_user_b where name='" + name + "' and river_type not in('17','25','23','22','19','18')";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_area='" + addvcd_area + "'";
                        //if (addvcd_town != null)
                        //    sql += " and addvcd_towm='" + addvcd_town + "'";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_village='" + addvcd_village + "'";

                        dtUser = SqlHelper.GetDataTable(sql, null);
                        if (dtUser == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        row = dtUser.Select("name='" + name + "'");
                        if (row == null || row.Length <= 0)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        temp.Add("name1", name);

                        userId = null;
                        if (row != null && row.Length >= 1)
                            userId = row[0][0].ToString();
                        else
                            userId = null;

                        river_id = id;
                        if (river_id.IndexOf("-") >= 0)
                        {
                            String[] river_ids = river_id.Split('-');
                            river_id = river_ids[0];
                        }

                        keyId = System.Guid.NewGuid().ToString("N");
                        sql = "insert into rm_riverchief_section_r(sec_id,river_section_id,rivet_rm_id,user_id)"
                        + " values('" + keyId + "','" + id + "','" + river_id + "','" + userId + "')";

                        if (userId == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }

                        try
                        {
                            if (userId != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //=========================
                        name = item["区级总河长信息姓名"].ToString().Trim();
                        name = name.Replace("  ", "");
                        name = name.Replace(" ", "");

                        sql = "select id,name from sys_user_b where name='" + name + "' and river_type not in('17','25','23','22','19','18')";
                        if (addvcd_area != null)
                            sql += " and addvcd_area='" + addvcd_area + "'";
                        //if (addvcd_town != null)
                        //    sql += " and addvcd_towm='" + addvcd_town + "'";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_village='" + addvcd_village + "'";

                        dtUser = SqlHelper.GetDataTable(sql, null);
                        if (dtUser == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        row = dtUser.Select("name='" + name + "'");
                        if (row == null || row.Length <= 0)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        temp.Add("name2", name);

                        userId = null;
                        if (row != null && row.Length >= 1)
                            userId = row[0][0].ToString();
                        else
                            userId = null;

                        river_id = id;
                        if (river_id.IndexOf("-") >= 0)
                        {
                            String[] river_ids = river_id.Split('-');
                            river_id = river_ids[0];
                        }

                        keyId = System.Guid.NewGuid().ToString("N");
                        sql = "insert into rm_riverchief_section_r(sec_id,river_section_id,rivet_rm_id,user_id)"
                        + " values('" + keyId + "','" + id + "','" + river_id + "','" + userId + "')";

                        if (userId == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }

                        try
                        {
                            if (userId != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //=========================
                        name = item["区级副总河长信息姓名"].ToString().Trim();
                        name = name.Replace("  ", "");
                        name = name.Replace(" ", "");

                        sql = "select id,name from sys_user_b where name='" + name + "' and river_type not in('17','25','23','22','19','18')";
                        if (addvcd_area != null)
                            sql += " and addvcd_area='" + addvcd_area + "'";
                        //if (addvcd_town != null)
                        //    sql += " and addvcd_towm='" + addvcd_town + "'";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_village='" + addvcd_village + "'";

                        dtUser = SqlHelper.GetDataTable(sql, null);
                        if (dtUser == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        row = dtUser.Select("name='" + name + "'");
                        if (row == null || row.Length <= 0)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        temp.Add("name3", name);

                        userId = null;
                        if (row != null && row.Length >= 1)
                            userId = row[0][0].ToString();
                        else
                            userId = null;

                        river_id = id;
                        if (river_id.IndexOf("-") >= 0)
                        {
                            String[] river_ids = river_id.Split('-');
                            river_id = river_ids[0];
                        }

                        keyId = System.Guid.NewGuid().ToString("N");
                        sql = "insert into rm_riverchief_section_r(sec_id,river_section_id,rivet_rm_id,user_id)"
                        + " values('" + keyId + "','" + id + "','" + river_id + "','" + userId + "')";

                        if (userId == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }

                        try
                        {
                            if (userId != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //=========================
                        name = item["区级河长信息姓名"].ToString().Trim();
                        name = name.Replace("  ", "");
                        name = name.Replace(" ", "");

                        sql = "select id,name from sys_user_b where name='" + name + "' and is_riverchief='Y'";
                        if (addvcd_area != null)
                            sql += " and addvcd_area='" + addvcd_area + "'";
                        //if (addvcd_town != null)
                        //    sql += " and addvcd_towm='" + addvcd_town + "'";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_village='" + addvcd_village + "'";

                        dtUser = SqlHelper.GetDataTable(sql, null);
                        if (dtUser == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        row = dtUser.Select("name='" + name + "'");
                        if (row == null || row.Length <= 0)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        temp.Add("name4", name);

                        userId = null;
                        if (row != null && row.Length >= 1)
                            userId = row[0][0].ToString();
                        else
                            userId = null;

                        river_id = id;
                        if (river_id.IndexOf("-") >= 0)
                        {
                            String[] river_ids = river_id.Split('-');
                            river_id = river_ids[0];
                        }

                        keyId = System.Guid.NewGuid().ToString("N");
                        sql = "insert into rm_riverchief_section_r(sec_id,river_section_id,rivet_rm_id,user_id)"
                        + " values('" + keyId + "','" + id + "','" + river_id + "','" + userId + "')";

                        if (userId == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }

                        try
                        {
                            if (userId != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //=========================
                        name = item["镇级总河长信息姓名"].ToString().Trim();
                        name = name.Replace("  ", "");
                        name = name.Replace(" ", "");

                        sql = "select id,name from sys_user_b where name='" + name + "' and river_type not in('17','25','23','22','19','18')";
                        if (addvcd_area != null)
                            sql += " and addvcd_area='" + addvcd_area + "'";
                        //if (addvcd_town != null)
                        //    sql += " and addvcd_towm='" + addvcd_town + "'";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_village='" + addvcd_village + "'";

                        dtUser = SqlHelper.GetDataTable(sql, null);
                        if (dtUser == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        row = dtUser.Select("name='" + name + "'");
                        if (row == null || row.Length <= 0)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        temp.Add("name5", name);

                        userId = null;
                        if (row != null && row.Length >= 1)
                            userId = row[0][0].ToString();
                        else
                            userId = null;

                        river_id = id;
                        if (river_id.IndexOf("-") >= 0)
                        {
                            String[] river_ids = river_id.Split('-');
                            river_id = river_ids[0];
                        }

                        keyId = System.Guid.NewGuid().ToString("N");
                        sql = "insert into rm_riverchief_section_r(sec_id,river_section_id,rivet_rm_id,user_id)"
                        + " values('" + keyId + "','" + id + "','" + river_id + "','" + userId + "')";

                        if (userId == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }

                        try
                        {
                            if (userId != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //=========================
                        name = item["镇级副总河长信息姓名"].ToString().Trim();
                        name = name.Replace("  ", "");
                        name = name.Replace(" ", "");

                        sql = "select id,name from sys_user_b where name='" + name + "' and river_type not in('17','25','23','22','19','18')";
                        if (addvcd_area != null)
                            sql += " and addvcd_area='" + addvcd_area + "'";
                        //if (addvcd_town != null)
                        //    sql += " and addvcd_towm='" + addvcd_town + "'";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_village='" + addvcd_village + "'";

                        dtUser = SqlHelper.GetDataTable(sql, null);
                        if (dtUser == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        row = dtUser.Select("name='" + name + "'");
                        if (row == null || row.Length <= 0)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        temp.Add("name6", name);

                        userId = null;
                        if (row != null && row.Length >= 1)
                            userId = row[0][0].ToString();
                        else
                            userId = null;

                        river_id = id;
                        if (river_id.IndexOf("-") >= 0)
                        {
                            String[] river_ids = river_id.Split('-');
                            river_id = river_ids[0];
                        }

                        keyId = System.Guid.NewGuid().ToString("N");
                        sql = "insert into rm_riverchief_section_r(sec_id,river_section_id,rivet_rm_id,user_id)"
                        + " values('" + keyId + "','" + id + "','" + river_id + "','" + userId + "')";

                        if (userId == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }

                        try
                        {
                            if (userId != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //=========================
                        name = item["镇级河长信息姓名"].ToString().Trim();
                        name = name.Replace("  ", "");
                        name = name.Replace(" ", "");

                        sql = "select id,name from sys_user_b where name='" + name + "' and is_riverchief='Y'";
                        if (addvcd_area != null)
                            sql += " and addvcd_area='" + addvcd_area + "'";
                        //if (addvcd_town != null)
                        //    sql += " and addvcd_towm='" + addvcd_town + "'";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_village='" + addvcd_village + "'";

                        dtUser = SqlHelper.GetDataTable(sql, null);
                        if (dtUser == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        row = dtUser.Select("name='" + name + "'");
                        if (row == null || row.Length <= 0)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        temp.Add("name7", name);

                        userId = null;
                        if (row != null && row.Length >= 1)
                            userId = row[0][0].ToString();
                        else
                            userId = null;

                        river_id = id;
                        if (river_id.IndexOf("-") >= 0)
                        {
                            String[] river_ids = river_id.Split('-');
                            river_id = river_ids[0];
                        }

                        keyId = System.Guid.NewGuid().ToString("N");
                        sql = "insert into rm_riverchief_section_r(sec_id,river_section_id,rivet_rm_id,user_id)"
                        + " values('" + keyId + "','" + id + "','" + river_id + "','" + userId + "')";

                        if (userId == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }

                        try
                        {
                            if (userId != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //=========================
                        name = item["村级河长信息姓名"].ToString().Trim();
                        name = name.Replace("  ", "");
                        name = name.Replace(" ", "");

                        sql = "select id,name from sys_user_b where name='" + name + "' and is_riverchief='Y'";
                        if (addvcd_area != null)
                            sql += " and addvcd_area='" + addvcd_area + "'";
                        //if (addvcd_town != null)
                        //    sql += " and addvcd_towm='" + addvcd_town + "'";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_village='" + addvcd_village + "'";

                        dtUser = SqlHelper.GetDataTable(sql, null);
                        if (dtUser == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        row = dtUser.Select("name='" + name + "'");
                        if (row == null || row.Length <= 0)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        temp.Add("name8", name);

                        userId = null;
                        if (row != null && row.Length >= 1)
                            userId = row[0][0].ToString();
                        else
                            userId = null;

                        river_id = id;
                        if (river_id.IndexOf("-") >= 0)
                        {
                            String[] river_ids = river_id.Split('-');
                            river_id = river_ids[0];
                        }

                        keyId = System.Guid.NewGuid().ToString("N");
                        sql = "insert into rm_riverchief_section_r(sec_id,river_section_id,rivet_rm_id,user_id)"
                        + " values('" + keyId + "','" + id + "','" + river_id + "','" + userId + "')";

                        if (userId == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }

                        try
                        {
                            if (userId != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //String riverchief = item["经纬度"].ToString().Trim();


                        //河段/湖泊类型，1：河流，2：湖泊，3：河流区段，4：河流镇（街）段，5：河流村（居）段，6：湖泊区段，7：湖泊镇（街）段，8：湖泊村（居）段；9：水库

                        result.Add(temp);

                        //listBox1.Items.Add("id=" + id + ",name=" + name + ",river_side=" + river_side
                        //    + ",river_length=" + river_length + ",addvcd_city=" + addvcd_city
                        //    + ",addvcd_area=" + addvcd_area + ",addvcd_town=" + addvcd_town + ",addvcd_village=" + addvcd_village
                        //     + ",river_system=" + river_system + ",start_addr=" + start_addr + ",end_addr=" + end_addr
                        //      + ",east_longitude=" + east_longitude + ",east_latitude=" + east_latitude + ",parent_id=" + parent_id
                        //       + ",parent_ids=" + parent_ids + ",type=" + type + ",south_longitude=" + south_longitude + ",south_latitude=" + south_latitude
                        //       + ",addvcd=" + addvcd + ",river_level=" + river_level);
                    }

                    //MessageBox.Show(countSd.ToString());


                }
            }
        }

        private void importRiverChiefForShuiKu()
        {
            openFileDialog1.FileName = "模板";
            openFileDialog1.Filter = "Excel文件(*.xls,*.xlsx)|*.xls;*.xlsx";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DataTable dtData = GetDataFromExcelSheet(openFileDialog1.FileName, "水库名录");
                if (dtData != null && dtData.Rows.Count > 0)
                {
                    DataTable dtArea = SqlHelper.GetDataTable("select id,name from sys_area_b", null);


                    List<Dictionary<String, String>> result = new List<Dictionary<string, string>>();
                    int countSd = 0;
                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        Dictionary<String, String> temp = new Dictionary<string, string>();

                        DataRow item = dtData.Rows[i];
                        String id = item["编码"].ToString();
                        temp.Add("id", id);

                        String name = item["县级总河长信息姓名"].ToString().Trim();
                        name = name.Replace("  ", "");
                        name = name.Replace(" ", "");
                        DataRow[] row;

                        String addvcd_city = item["市"].ToString().Trim();
                        if (!String.IsNullOrEmpty(addvcd_city))
                        {
                            row = dtArea.Select("name='" + addvcd_city + "'");
                            if (row != null && row.Length == 1)
                                addvcd_city = row[0][0].ToString();
                            else
                                addvcd_city = null;
                        }
                        else
                        {
                            addvcd_city = null;
                        }

                        temp.Add("addvcd_city", addvcd_city);

                        String addvcd_area = item["县（区）"].ToString().Trim();
                        if (!String.IsNullOrEmpty(addvcd_area))
                        {
                            row = dtArea.Select("name='" + addvcd_area + "'");
                            if (row != null && row.Length == 1)
                                addvcd_area = row[0][0].ToString();
                            else
                                addvcd_area = null;
                        }
                        else
                        {
                            addvcd_area = null;
                        }

                        temp.Add("addvcd_area", addvcd_area);

                        String addvcd_town = item["镇（街）"].ToString().Trim();
                        if (addvcd_town.EndsWith("办"))
                        {
                            addvcd_town = addvcd_town.Substring(0, addvcd_town.Length - 2);
                        }
                        if (!String.IsNullOrEmpty(addvcd_town))
                        {
                            row = dtArea.Select("name like '%" + addvcd_town + "%'");
                            if (row != null && row.Length == 1)
                                addvcd_town = row[0][0].ToString();
                            else
                                addvcd_town = null;
                        }
                        else
                        {
                            addvcd_town = null;
                        }
                        temp.Add("addvcd_town", addvcd_town);

                        String addvcd_village = item["村（居委会）"].ToString().Trim();
                        if (addvcd_village.EndsWith("村"))
                        {
                            addvcd_village = addvcd_village.Substring(0, addvcd_village.Length - 2);
                        }
                        if (!String.IsNullOrEmpty(addvcd_village))
                        {
                            row = dtArea.Select("name like '%" + addvcd_village + "%'");
                            if (row != null && row.Length >= 1)
                                addvcd_village = row[0][0].ToString();
                            else
                                addvcd_village = null;
                        }
                        else
                        {
                            addvcd_village = null;
                        }
                        temp.Add("addvcd_village", addvcd_village);

                        String sql = "select id,name from sys_user_b where name='" + name + "' and river_type not in('17','25','23','22','19','18')";
                        if (addvcd_area != null)
                            sql += " and addvcd_area='" + addvcd_area + "'";
                        //if (addvcd_town != null)
                        //    sql += " and addvcd_towm='" + addvcd_town + "'";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_village='" + addvcd_village + "'";

                        DataTable dtUser = SqlHelper.GetDataTable(sql, null);
                        if (dtUser == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        row = dtUser.Select("name='" + name + "'");
                        if (row == null || row.Length <= 0)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        temp.Add("name", name);

                        String userId = null;
                        if (row != null && row.Length >= 1)
                            userId = row[0][0].ToString();
                        else
                            userId = null;

                        String river_id = id;
                        if (river_id.IndexOf("-") >= 0)
                        {
                            String[] river_ids = river_id.Split('-');
                            river_id = river_ids[0];
                        }

                        String keyId = System.Guid.NewGuid().ToString("N");
                        sql = "insert into rm_riverchief_section_r(sec_id,river_section_id,rivet_rm_id,user_id)"
                        + " values('" + keyId + "','" + id + "','" + river_id + "','" + userId + "')";

                        if (userId == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }

                        try
                        {
                            if (userId != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //=========================
                        name = item["县级副总河长信息姓名"].ToString().Trim();
                        name = name.Replace("  ", "");
                        name = name.Replace(" ", "");

                        sql = "select id,name from sys_user_b where name='" + name + "' and river_type not in('17','25','23','22','19','18')";
                        if (addvcd_area != null)
                            sql += " and addvcd_area='" + addvcd_area + "'";
                        //if (addvcd_town != null)
                        //    sql += " and addvcd_towm='" + addvcd_town + "'";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_village='" + addvcd_village + "'";

                        dtUser = SqlHelper.GetDataTable(sql, null);
                        if (dtUser == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        row = dtUser.Select("name='" + name + "'");
                        if (row == null || row.Length <= 0)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        temp.Add("name1", name);

                        userId = null;
                        if (row != null && row.Length >= 1)
                            userId = row[0][0].ToString();
                        else
                            userId = null;

                        river_id = id;
                        if (river_id.IndexOf("-") >= 0)
                        {
                            String[] river_ids = river_id.Split('-');
                            river_id = river_ids[0];
                        }

                        keyId = System.Guid.NewGuid().ToString("N");
                        sql = "insert into rm_riverchief_section_r(sec_id,river_section_id,rivet_rm_id,user_id)"
                        + " values('" + keyId + "','" + id + "','" + river_id + "','" + userId + "')";

                        if (userId == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }

                        try
                        {
                            if (userId != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //=========================
                        name = item["县级河长信息姓名"].ToString().Trim();
                        name = name.Replace("  ", "");
                        name = name.Replace(" ", "");

                        sql = "select id,name from sys_user_b where name='" + name + "' and river_type not in('17','25','23','22','19','18')";
                        if (addvcd_area != null)
                            sql += " and addvcd_area='" + addvcd_area + "'";
                        //if (addvcd_town != null)
                        //    sql += " and addvcd_towm='" + addvcd_town + "'";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_village='" + addvcd_village + "'";

                        dtUser = SqlHelper.GetDataTable(sql, null);
                        if (dtUser == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        row = dtUser.Select("name='" + name + "'");
                        if (row == null || row.Length <= 0)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        temp.Add("name2", name);

                        userId = null;
                        if (row != null && row.Length >= 1)
                            userId = row[0][0].ToString();
                        else
                            userId = null;

                        river_id = id;
                        if (river_id.IndexOf("-") >= 0)
                        {
                            String[] river_ids = river_id.Split('-');
                            river_id = river_ids[0];
                        }

                        keyId = System.Guid.NewGuid().ToString("N");
                        sql = "insert into rm_riverchief_section_r(sec_id,river_section_id,rivet_rm_id,user_id)"
                        + " values('" + keyId + "','" + id + "','" + river_id + "','" + userId + "')";

                        if (userId == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }

                        try
                        {
                            if (userId != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //=========================
                        name = item["镇级总河长信息姓名"].ToString().Trim();
                        name = name.Replace("  ", "");
                        name = name.Replace(" ", "");

                        sql = "select id,name from sys_user_b where name='" + name + "' and river_type not in('17','25','23','22','19','18')";
                        if (addvcd_area != null)
                            sql += " and addvcd_area='" + addvcd_area + "'";
                        //if (addvcd_town != null)
                        //    sql += " and addvcd_towm='" + addvcd_town + "'";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_village='" + addvcd_village + "'";

                        dtUser = SqlHelper.GetDataTable(sql, null);
                        if (dtUser == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        row = dtUser.Select("name='" + name + "'");
                        if (row == null || row.Length <= 0)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        temp.Add("name3", name);

                        userId = null;
                        if (row != null && row.Length >= 1)
                            userId = row[0][0].ToString();
                        else
                            userId = null;

                        river_id = id;
                        if (river_id.IndexOf("-") >= 0)
                        {
                            String[] river_ids = river_id.Split('-');
                            river_id = river_ids[0];
                        }

                        keyId = System.Guid.NewGuid().ToString("N");
                        sql = "insert into rm_riverchief_section_r(sec_id,river_section_id,rivet_rm_id,user_id)"
                        + " values('" + keyId + "','" + id + "','" + river_id + "','" + userId + "')";

                        if (userId == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }

                        try
                        {
                            if (userId != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //=========================
                        name = item["镇级副总河长信息姓名"].ToString().Trim();
                        name = name.Replace("  ", "");
                        name = name.Replace(" ", "");

                        sql = "select id,name from sys_user_b where name='" + name + "' and is_riverchief='Y'";
                        if (addvcd_area != null)
                            sql += " and addvcd_area='" + addvcd_area + "'";
                        //if (addvcd_town != null)
                        //    sql += " and addvcd_towm='" + addvcd_town + "'";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_village='" + addvcd_village + "'";

                        dtUser = SqlHelper.GetDataTable(sql, null);
                        if (dtUser == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        row = dtUser.Select("name='" + name + "'");
                        if (row == null || row.Length <= 0)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        temp.Add("name4", name);

                        userId = null;
                        if (row != null && row.Length >= 1)
                            userId = row[0][0].ToString();
                        else
                            userId = null;

                        river_id = id;
                        if (river_id.IndexOf("-") >= 0)
                        {
                            String[] river_ids = river_id.Split('-');
                            river_id = river_ids[0];
                        }

                        keyId = System.Guid.NewGuid().ToString("N");
                        sql = "insert into rm_riverchief_section_r(sec_id,river_section_id,rivet_rm_id,user_id)"
                        + " values('" + keyId + "','" + id + "','" + river_id + "','" + userId + "')";

                        if (userId == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }

                        try
                        {
                            if (userId != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //=========================
                        name = item["镇级河长信息姓名"].ToString().Trim();
                        name = name.Replace("  ", "");
                        name = name.Replace(" ", "");

                        sql = "select id,name from sys_user_b where name='" + name + "' and river_type not in('17','25','23','22','19','18')";
                        if (addvcd_area != null)
                            sql += " and addvcd_area='" + addvcd_area + "'";
                        //if (addvcd_town != null)
                        //    sql += " and addvcd_towm='" + addvcd_town + "'";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_village='" + addvcd_village + "'";

                        dtUser = SqlHelper.GetDataTable(sql, null);
                        if (dtUser == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        row = dtUser.Select("name='" + name + "'");
                        if (row == null || row.Length <= 0)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        temp.Add("name5", name);

                        userId = null;
                        if (row != null && row.Length >= 1)
                            userId = row[0][0].ToString();
                        else
                            userId = null;

                        river_id = id;
                        if (river_id.IndexOf("-") >= 0)
                        {
                            String[] river_ids = river_id.Split('-');
                            river_id = river_ids[0];
                        }

                        keyId = System.Guid.NewGuid().ToString("N");
                        sql = "insert into rm_riverchief_section_r(sec_id,river_section_id,rivet_rm_id,user_id)"
                        + " values('" + keyId + "','" + id + "','" + river_id + "','" + userId + "')";

                        if (userId == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }

                        try
                        {
                            if (userId != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //=========================
                        name = item["村级河长信息姓名"].ToString().Trim();
                        name = name.Replace("  ", "");
                        name = name.Replace(" ", "");

                        sql = "select id,name from sys_user_b where name='" + name + "' and is_riverchief='Y'";
                        if (addvcd_area != null)
                            sql += " and addvcd_area='" + addvcd_area + "'";
                        //if (addvcd_town != null)
                        //    sql += " and addvcd_towm='" + addvcd_town + "'";
                        //if (addvcd_area != null)
                        //    sql += " and addvcd_village='" + addvcd_village + "'";

                        dtUser = SqlHelper.GetDataTable(sql, null);
                        if (dtUser == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        row = dtUser.Select("name='" + name + "'");
                        if (row == null || row.Length <= 0)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }
                        temp.Add("name6", name);

                        userId = null;
                        if (row != null && row.Length >= 1)
                            userId = row[0][0].ToString();
                        else
                            userId = null;

                        river_id = id;
                        if (river_id.IndexOf("-") >= 0)
                        {
                            String[] river_ids = river_id.Split('-');
                            river_id = river_ids[0];
                        }

                        keyId = System.Guid.NewGuid().ToString("N");
                        sql = "insert into rm_riverchief_section_r(sec_id,river_section_id,rivet_rm_id,user_id)"
                        + " values('" + keyId + "','" + id + "','" + river_id + "','" + userId + "')";

                        if (userId == null)
                        {
                            listBox1.Items.Add("id=" + id + ",name=" + name);
                            //continue;
                        }

                        try
                        {
                            if (userId != null)
                                SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("id=" + id);
                            //throw;
                        }

                        //String riverchief = item["经纬度"].ToString().Trim();


                        //河段/湖泊类型，1：河流，2：湖泊，3：河流区段，4：河流镇（街）段，5：河流村（居）段，6：湖泊区段，7：湖泊镇（街）段，8：湖泊村（居）段；9：水库

                        result.Add(temp);

                        //listBox1.Items.Add("id=" + id + ",name=" + name + ",river_side=" + river_side
                        //    + ",river_length=" + river_length + ",addvcd_city=" + addvcd_city
                        //    + ",addvcd_area=" + addvcd_area + ",addvcd_town=" + addvcd_town + ",addvcd_village=" + addvcd_village
                        //     + ",river_system=" + river_system + ",start_addr=" + start_addr + ",end_addr=" + end_addr
                        //      + ",east_longitude=" + east_longitude + ",east_latitude=" + east_latitude + ",parent_id=" + parent_id
                        //       + ",parent_ids=" + parent_ids + ",type=" + type + ",south_longitude=" + south_longitude + ",south_latitude=" + south_latitude
                        //       + ",addvcd=" + addvcd + ",river_level=" + river_level);
                    }

                    //MessageBox.Show(countSd.ToString());


                }
            }
        }

        private void importProvinceRiverChief()
        {
            openFileDialog1.FileName = "模板";
            openFileDialog1.Filter = "Excel文件(*.xls,*.xlsx)|*.xls;*.xlsx";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DataTable dtData = GetDataFromExcelSheet(openFileDialog1.FileName, "Sheet1");
                if (dtData != null && dtData.Rows.Count > 0)
                {
                    List<Dictionary<String, String>> result = new List<Dictionary<string, string>>();
                    String sql = "";
                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        Dictionary<String, String> temp = new Dictionary<string, string>();

                        DataRow item = dtData.Rows[i];
                        String id = item["U_ID"].ToString();

                        String name = item["REALNAME"].ToString();
                        name = name.Replace(" ", "");
                        String adcd = item["adcd"].ToString();
                        String areaname=item["areaname"].ToString();
                        String phone = item["CELLPHONE"].ToString();

                        String roldId = item["ROLEIDS"].ToString();

                        string role = "";
                        switch (roldId)
                        {
                            case "4":
                                role = "市河长办";
                                break;
                            case "5":
                                role = "市河长";
                                break;
                            case "6":
                                role = "区河长办";
                                break;
                            case "7":
                                role = "区河长";
                                break;
                            case "8":
                                role = "镇河长办";
                                break;
                            case "9":
                                role = "镇河长";
                                break;
                            case "10":
                                role = "村河长";
                                break;
                            case "11":
                                role = "职能部门";
                                break;
                            case "12":
                                role = "河管员";
                                break;
                            case "13":
                                role = "普通用户";
                                break;
                        }

                        if (adcd.Length == 6)
                            adcd += "000000";
                        else if (adcd.Length == 9)
                            adcd += "000";

                        String city = "";
                        if (adcd.Equals("440100000000"))
                            city = "广州市";
                        else
                        {
                            sql = "select name from sys_area_b where id='" + adcd.Substring(0, 6) + "000000" + "'";

                            DataTable dtArea = SqlHelper.GetDataTable(sql, null);

                            if (dtArea.Rows.Count > 0)
                                city = dtArea.Rows[0][0].ToString();
                            else
                                city = adcd;
                        }
                        
                        //rtbInfo.AppendText(adcd +"\n");

                        sql = "select id from sys_user_b where name='" + name + "' and addvcd='"+adcd+"'";

                        if(roldId.Equals("10"))
                            sql = "select id from sys_user_b where name='" + name + "' and addvcd_towm='" + adcd + "'";

                        DataTable dt = SqlHelper.GetDataTable(sql, null);

                        if (dt.Rows.Count == 1)
                        {

                        }
                        else
                        {
                            rtbInfo.AppendText(city + "," + areaname + "," + name + "," + role + "," + phone + "\n");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 从Excel中读取数据
        /// </summary>
        /// <param name="strPath">Excel文件路径</param>
        /// <param name="strSheetName">读取的Sheet名</param>
        /// <returns></returns>
        public static DataTable GetDataFromExcelSheet(string strPath, string strSheetName = "")
        {
            string strConn = GetExcelConnString(strPath);
            using (OleDbConnection conn = new OleDbConnection(strConn))
            {
                conn.Open();
                DataTable dtSheetName = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "Table" });
                if (dtSheetName == null || dtSheetName.Rows.Count <= 0)
                {
                    return null;
                }

                if (strSheetName.Length <= 0)
                    strSheetName = ConvertHelper.ToString(dtSheetName.Rows[0]["TABLE_NAME"]);
                else //update by lhl 2018-01-28增加后缀
                    strSheetName += "$";
                OleDbDataAdapter myCommand = null;
                DataSet ds = null;
                string strExcel = string.Format("select * from [{0}]", strSheetName);
                myCommand = new OleDbDataAdapter(strExcel, strConn);
                ds = new DataSet();
                myCommand.Fill(ds, "table1");
                return ds.Tables[0];
            }
        }

        private static string GetExcelConnString(string strPath)
        {
            string strSuffix = ".xlsx";
            System.IO.FileInfo fi = new System.IO.FileInfo(strPath);
            strSuffix = fi.Extension;
            if (strSuffix == ".xls")
            {
                return "Provider=Microsoft.Jet.OLEDB.4.0;Data Source = " + strPath + ";Extended Properties ='Excel 8.0;HDR=YES;IMEX=1'";

            }
            else
                return "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + strPath + ";Extended Properties = \"Excel 12.0 Xml; HDR = No\"";
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
