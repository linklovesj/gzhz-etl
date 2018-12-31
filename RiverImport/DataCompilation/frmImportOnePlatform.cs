using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RiverImport.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RiverImport.DataCompilation
{
    public partial class frmImportOnePlatform : Form
    {
        public frmImportOnePlatform()
        {
            InitializeComponent();
        }

        private void frmImportOnePlatform_Load(object sender, EventArgs e)
        {

        }

        private void btnHuanBaoData_Click(object sender, EventArgs e)
        {
            String jsonStr = "";
            String url = "http://10.194.170.78/agsupport/catalog/environment/huanbao?token=edc670a737307f07&jcny=2018-10&hcmc=";
            //jsonStr = HttpUtility.UrlEncode(jsonStr);//字符串进行编码，参数中有中文时一定需要这一步转换，否则接口接收的到参数会乱码
            jsonStr = UtilTool.doHttpPost(url, null);
            JObject jo = (JObject)JsonConvert.DeserializeObject(jsonStr);
            writeInfo(jo["row"].ToString());
        }

        public void writeInfo(String msg)
        {
            rtbInfo.AppendText("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + msg + "\r\n");
        }
    }
}
