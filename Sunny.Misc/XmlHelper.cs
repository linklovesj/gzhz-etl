using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;

namespace Sunny.Misc
{
    /// <summary>
    /// 类名：XmlHelper
    /// 功能：Xml操作帮助类
    /// 详细：Xml相关操作
    /// 创建者：DZT
    /// 创建日期：2014-9-18
    /// 修改日期：
    /// 说明：
    /// </summary>
    public class XmlHelper
    {

        #region 字段定义
        /// <summary>
        /// XML文件的物理路径
        /// </summary>
        private string _filePath = string.Empty;
        /// <summary>
        /// Xml文档
        /// </summary>
        private XmlDocument _xml;
        /// <summary>
        /// XML的根节点
        /// </summary>
        private XmlElement _root;
        #endregion

        #region 构造方法
        /// <summary>
        /// 实例化XmlHelper对象
        /// </summary>
        /// <param name="xmlFilePath">Xml文件的相对路径</param>
        public XmlHelper()
        {
            //获取XML文件的绝对路径
            _xml = new XmlDocument();
        }

        #endregion

        #region 加载XML
        /// <summary>
        /// 加载XML
        /// </summary>
        /// <param name="xml"></param>
        public void LoadXml(string xml)
        {
            _xml.LoadXml(xml);
        }
        #endregion

        #region 创建根节点
        /// <summary>
        /// 创建根节点
        /// </summary>
        /// <param name="rootName"></param>
        /// <returns></returns>
        public void CreateRootDoc(string rootName)
        {

            //xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0","UTF-8", null));
            _xml.AppendChild(_xml.CreateXmlDeclaration("1.0", "gb2312", null));
            XmlElement eleOrderList = _xml.CreateElement(rootName);
            _xml.AppendChild(eleOrderList);


        }
        #endregion

        #region 获取指定XPath表达式的节点对象
        /// <summary>
        /// 获取指定XPath表达式的节点对象
        /// </summary>        
        /// <param name="xPath">XPath表达式,
        /// 范例1: @"Skill/First/SkillItem", 等效于 @"//Skill/First/SkillItem"
        /// 范例2: @"Table[USERNAME='a']" , []表示筛选,USERNAME是Table下的一个子节点.
        /// 范例3: @"ApplyPost/Item[@itemName='岗位编号']",@itemName是Item节点的属性.
        /// </param>
        public XmlNode GetNode(string xPath)
        {
            //返回XPath节点
            return _root.SelectSingleNode(xPath);
        }
        #endregion

        #region 获取根节点
        /// <summary>
        /// 获取根节点
        /// </summary>
        public XmlElement RootNode
        {
            get
            {
                //返回根节点
                return _root;
            }
        }
        #endregion

        #region 获取根节点的所有下级子节点
        /// <summary>
        /// 获取根节点的所有下级子节点
        /// </summary>
        public XmlNodeList ChildNodes
        {
            get
            {
                //返回根节点
                return _root.ChildNodes;
            }
        }
        #endregion

        #region 获取指定XPath表达式节点的值
        /// <summary>
        /// 获取指定XPath表达式节点的值
        /// </summary>
        /// <param name="xPath">XPath表达式,
        /// 范例1: @"Skill/First/SkillItem", 等效于 @"//Skill/First/SkillItem"
        /// 范例2: @"Table[USERNAME='a']" , []表示筛选,USERNAME是Table下的一个子节点.
        /// 范例3: @"ApplyPost/Item[@itemName='岗位编号']",@itemName是Item节点的属性.
        /// </param>
        public string GetValue(string xPath)
        {
            //返回XPath节点的值
            return _root.SelectSingleNode(xPath).InnerText;
        }
        #endregion

        #region 获取指定XPath表达式节点的属性值
        /// <summary>
        /// 获取指定XPath表达式节点的属性值
        /// </summary>
        /// <param name="xPath">XPath表达式,
        /// 范例1: @"Skill/First/SkillItem", 等效于 @"//Skill/First/SkillItem"
        /// 范例2: @"Table[USERNAME='a']" , []表示筛选,USERNAME是Table下的一个子节点.
        /// 范例3: @"ApplyPost/Item[@itemName='岗位编号']",@itemName是Item节点的属性.
        /// </param>
        /// <param name="attributeName">属性名</param>
        public string GetAttributeValue(string xPath, string attributeName)
        {
            //返回XPath节点的属性值
            return _root.SelectSingleNode(xPath).Attributes[attributeName].Value;
        }
        #endregion

        #region 获取节点的属性
        /// <summary>
        /// 获取节点的属性
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public string GetAttributeValue(XmlNode node, string attributeName)
        {
            //返回XPath节点的属性值
            string strRet = "";
            try
            {
                strRet = node.Attributes[attributeName].Value;
            }
            finally
            { }
            return strRet;
        }
        #endregion



        #region 新增节点
        /// <summary>
        /// 1. 功能：新增节点。
        /// 2. 使用条件：将任意节点插入到当前Xml文件中。
        /// </summary>        
        /// <param name="xmlNode">要插入的Xml节点</param>
        /// <param name="parentNode">父节点</param>
        public void AppendNode(XmlNode newNode, XmlNode parentNode)
        {
            parentNode.AppendChild(newNode);
        }

        /// <summary>
        /// 1. 功能：新增节点。
        /// 2. 使用条件：将DataSet中的第一条记录插入Xml文件中。
        /// </summary>        
        /// <param name="ds">DataSet的实例，该DataSet中应该只有一条记录</param>
        /// <param name="rNode">父节点</param>
        public void AppendNode(DataSet ds, XmlNode parentNode)
        {
            //创建XmlDataDocument对象
            XmlDataDocument xmlDataDocument = new XmlDataDocument(ds);

            //导入节点
            XmlNode node = xmlDataDocument.DocumentElement.FirstChild;

            //将节点插入到根节点下
            AppendNode(node, parentNode);
        }
        #endregion

        #region 删除节点
        /// <summary>
        /// 删除指定XPath表达式的节点
        /// </summary>        
        /// <param name="xPath">XPath表达式,
        /// 范例1: @"Skill/First/SkillItem", 等效于 @"//Skill/First/SkillItem"
        /// 范例2: @"Table[USERNAME='a']" , []表示筛选,USERNAME是Table下的一个子节点.
        /// 范例3: @"ApplyPost/Item[@itemName='岗位编号']",@itemName是Item节点的属性.
        /// </param>
        public void RemoveNode(string xPath)
        {
            //获取要删除的节点
            XmlNode node = _xml.SelectSingleNode(xPath);
            //删除节点
            _root.RemoveChild(node);
        }
        #endregion //删除节点

        #region 保存XML文件
        /// <summary>
        /// 保存XML文件
        /// </summary>        
        public void Save()
        {
            //保存XML文件
            _xml.Save(this._filePath);
        }
        #endregion //保存XML文件

        #region 静态方法

        #region 获取节点内xpath子节点
        /// <summary>
        /// 获取节点内xpath子节点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public static XmlNodeList GetXmlNodeList(XmlNode node, string xpath)
        {
            if (node == null)
                return null;
            return node.SelectNodes(xpath);
        }
        #endregion

        public static string GetAttribute(XmlNode node, string attr)
        {
            if (node.Attributes[attr] != null)
            {
                return node.Attributes[attr].Value.ToString();
            }
            return "";
        }

        #endregion

    }
}
