using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using RiverImport.Util;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace RiverImport.DataCompilation
{
    public partial class ProvinceDataFrm : Form
    {
        public ProvinceDataFrm()
        {
            InitializeComponent();
        }

        private void btnGetToken_Click(object sender, EventArgs e)
        {
            MakeRequest("http://47.94.240.212:8100/api/v1/sso/token", "", "post", "", "");

        }

        public void writeInfo(String msg)
        {
            rtbInfo.AppendText("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + msg + "\r\n");
        }

        public static string MakeRequest(string url, string query_string, string cookie_string, string method, string protocol)
        {
            //cookie
            //string cookie_string = null;
            //if (cookie != null && cookie.Count > 0)
            //{
            //    string[] arr_cookies = new string[cookie.Count];
            //    int i = 0;
            //    foreach (string key in cookie.Keys)
            //    {
            //        arr_cookies[i] = key + "=" + cookie[key];
            //        i++;
            //    }
            //    cookie_string = string.Join("; ", arr_cookies);
            //}
            //结果
            string result = "";
            //请求类
            HttpWebRequest request = null;
            //请求响应类
            HttpWebResponse response = null;
            //响应结果读取类
            StreamReader reader = null;
            //http连接数限制默认为2，多线程情况下可以增加该连接数，非多线程情况下可以注释掉此行代码
            ServicePointManager.DefaultConnectionLimit = 500;
            try
            {
                if (method.Equals("get", StringComparison.OrdinalIgnoreCase))
                {


                    if (url.IndexOf("?") > 0)
                    {
                        url = url + "&" + query_string;
                    }
                    else
                    {
                        url = url + "?" + query_string;
                    }
                    //如果是发送HTTPS请求   
                    if (protocol.Equals("https", StringComparison.OrdinalIgnoreCase))
                    {
                        request = WebRequest.Create(url) as HttpWebRequest;
                        request.ProtocolVersion = HttpVersion.Version10;
                    }
                    else
                    {
                        request = WebRequest.Create(url) as HttpWebRequest;
                    }
                    request.Method = "GET";
                    request.Timeout = 30000;
                }
                else
                {
                    //如果是发送HTTPS请求   
                    if (protocol.Equals("https", StringComparison.OrdinalIgnoreCase))
                    {
                        request = WebRequest.Create(url) as HttpWebRequest;
                        request.ProtocolVersion = HttpVersion.Version10;
                    }
                    else
                    {
                        request = WebRequest.Create(url) as HttpWebRequest;
                    }
                    //去掉“Expect: 100-Continue”请求头，不然会引起post（417） expectation failed
                    ServicePointManager.Expect100Continue = false;


                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.Timeout = 30000;
                    //POST数据   
                    byte[] data = Encoding.UTF8.GetBytes(query_string);
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                //cookie
                if (!string.IsNullOrEmpty(cookie_string))
                {
                    request.Headers.Add("Cookie", cookie_string);
                }
                //response
                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                //return
                result = reader.ReadToEnd();
            }
            finally
            {
                if (request != null)
                {
                    request.Abort();
                }
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                if (response != null)
                {
                    response.Close();
                }
            }
            return result;
        }

    }
}
