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
    public partial class PlottingDataFrm : Form
    {
        public PlottingDataFrm()
        {
            InitializeComponent();
        }

        private void PlottingDataFrm_Load(object sender, EventArgs e)
        {
            load_data_village();
            //load_data_town();
            //load_data_area();
            //load_data_city();
        }

        private void load_data_village()
        {
            openFileDialog1.FileName = "模板";
            openFileDialog1.Filter = "Excel文件(*.xls,*.xlsx)|*.xls;*.xlsx";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DataTable dtData = GetDataFromExcelSheet(openFileDialog1.FileName, "point");
                if (dtData != null && dtData.Rows.Count > 0)
                {
                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        DataRow item = dtData.Rows[i];
                        String code = item["code"].ToString();
                        String name = item["name"].ToString();
                        String id = Guid.NewGuid().ToString();
                        String lgtd = item["x"].ToString();
                        String lttd = item["y"].ToString();
                        String addvcd_village = item["addvcd"].ToString();
                        String addvcd_town = addvcd_village.Substring(0, 9) + "000";
                        String addvcd_area = addvcd_village.Substring(0, 6) + "000000";

                        String sql = "insert into rm_river_lake_plotting(id,name,lgtd,lttd,plotting_id,addvcd,addvcd_village,addvcd_town,addvcd_area) values('" + id + "','" + name + "','" + lgtd + "','" + lttd
                            + "','" + code + "','" + addvcd_village + "','" + addvcd_village + "','" + addvcd_town + "','" + addvcd_area + "')";

                        try
                        {
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("code=" + code);
                            //throw;
                        }
                    }
                }
            }
        }

        private void load_data_town()
        {
            openFileDialog1.FileName = "模板";
            openFileDialog1.Filter = "Excel文件(*.xls,*.xlsx)|*.xls;*.xlsx";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DataTable dtData = GetDataFromExcelSheet(openFileDialog1.FileName, "point");
                if (dtData != null && dtData.Rows.Count > 0)
                {
                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        DataRow item = dtData.Rows[i];
                        String code = item["code"].ToString();
                        String name = item["name"].ToString();
                        String id = Guid.NewGuid().ToString();
                        String lgtd = item["x"].ToString();
                        String lttd = item["y"].ToString();
                        String addvcd_village = null;
                        String addvcd_town = item["addvcd"].ToString() + "000";
                        String addvcd_area = addvcd_town.Substring(0, 6) + "000000";

                        String addvcd = addvcd_town;

                        String sql = "insert into rm_river_lake_plotting(id,name,lgtd,lttd,plotting_id,addvcd,addvcd_village,addvcd_town,addvcd_area) values('" + id + "','" + name + "','" + lgtd + "','" + lttd
                            + "','" + code + "','" + addvcd + "','" + addvcd_village + "','" + addvcd_town + "','" + addvcd_area + "')";

                        try
                        {
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("code=" + code);
                            //throw;
                        }
                    }
                }
            }
        }

        private void load_data_area()
        {
            openFileDialog1.FileName = "模板";
            openFileDialog1.Filter = "Excel文件(*.xls,*.xlsx)|*.xls;*.xlsx";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DataTable dtData = GetDataFromExcelSheet(openFileDialog1.FileName, "point");
                if (dtData != null && dtData.Rows.Count > 0)
                {
                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        DataRow item = dtData.Rows[i];
                        String code = item["code"].ToString();
                        String name = item["name"].ToString();
                        String id = Guid.NewGuid().ToString();
                        String lgtd = item["x"].ToString();
                        String lttd = item["y"].ToString();
                        String addvcd_village = null;
                        String addvcd_town = null;
                        String addvcd_area = item["addvcd"].ToString() + "000000";

                        String addvcd = addvcd_area;

                        String sql = "insert into rm_river_lake_plotting(id,name,lgtd,lttd,plotting_id,addvcd,addvcd_village,addvcd_town,addvcd_area) values('" + id + "','" + name + "','" + lgtd + "','" + lttd
                            + "','" + code + "','" + addvcd + "','" + addvcd_village + "','" + addvcd_town + "','" + addvcd_area + "')";

                        try
                        {
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("code=" + code);
                            //throw;
                        }
                    }
                }
            }
        }

        private void load_data_city()
        {
            openFileDialog1.FileName = "模板";
            openFileDialog1.Filter = "Excel文件(*.xls,*.xlsx)|*.xls;*.xlsx";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DataTable dtData = GetDataFromExcelSheet(openFileDialog1.FileName, "point");
                if (dtData != null && dtData.Rows.Count > 0)
                {
                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        DataRow item = dtData.Rows[i];
                        String code = item["code"].ToString();
                        String name = item["name"].ToString();
                        String id = Guid.NewGuid().ToString();
                        String lgtd = item["x"].ToString();
                        String lttd = item["y"].ToString();
                        String addvcd_village = null;
                        String addvcd_town = null;
                        String addvcd_area = null;

                        String addvcd = item["addvcd"].ToString() + "000000";

                        String sql = "insert into rm_river_lake_plotting(id,name,lgtd,lttd,plotting_id,addvcd,addvcd_village,addvcd_town,addvcd_area) values('" + id + "','" + name + "','" + lgtd + "','" + lttd
                            + "','" + code + "','" + addvcd + "','" + addvcd_village + "','" + addvcd_town + "','" + addvcd_area + "')";

                        try
                        {
                            SqlHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                        }
                        catch (Exception)
                        {
                            listBox1.Items.Add("code=" + code);
                            //throw;
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
    }
}
