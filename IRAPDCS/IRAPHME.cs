using Dapper;
using IRAPBase;
using IRAPBase.DTO;
using IRAPBase.Entities;
using IRAPDCS.Entities;
using IRAPShared;
using IRAPUtil;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IRAPDCS
{
    public class IRAPHME : IRAPWorkbench
    {

        public static string MongoDBName = "IRAPGDB";
        /// <summary>
        ///         开始DCS接口调用
        /// </summary>
        /// <param name="CommunitID"></param>
        /// <param name="T133LeafID"></param>
        /// <returns></returns>
        public string StartDCSInvoking(string ExCode, int CommunityID, int T133LeafID )
        {
            try
            {
                if (CommunityID == 0 )
                {
                    CommunityID = 57280;
                }  
                string ConnIRAP = ConfigurationManager.ConnectionStrings["IRAPMDMContext"].ConnectionString;
                #region 执行存储过程
                SqlConnection sqlCon = null;
                DataTable dt = new DataTable();
                SqlCommand sqlComm = null;
                string sqlName = "usp_IRAPUES_DCS_DC_StartDCSInvoking";
                int errCode  ;
                string errText  ;
                using (sqlCon = new SqlConnection(ConnIRAP))
                {
                    sqlCon.Open();
                    sqlComm = new SqlCommand(sqlName, sqlCon);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.Parameters.Add("@CommunityID", SqlDbType.Int);  //社区标识
                    sqlComm.Parameters.Add("@T133LeafID", SqlDbType.Int);  //设备信息
                    sqlComm.Parameters.Add("@SysLogID", SqlDbType.BigInt);  //系统登录标识
                    sqlComm.Parameters.Add("@ErrCode", SqlDbType.Int);
                    sqlComm.Parameters.Add("@ErrText", SqlDbType.NVarChar, 400);
                    sqlComm.Parameters["@CommunityID"].Value = CommunityID;
                    sqlComm.Parameters["@T133LeafID"].Value = T133LeafID;
                    sqlComm.Parameters["@SysLogID"].Value = 1; 
                    sqlComm.Parameters["@ErrCode"].Direction = ParameterDirection.Output;
                    sqlComm.Parameters["@ErrText"].Direction = ParameterDirection.Output;
                    sqlComm.ExecuteNonQuery();
                    errCode = int.Parse(sqlComm.Parameters["@ErrCode"].Value.ToString());
                    errText = sqlComm.Parameters["@ErrText"].Value.ToString();
                    sqlCon.Close();
                    if (errCode != 0)
                    {
                        APIBag.ErrCode = errCode;
                        APIBag.ErrText = errText;
                        return JsonConvert.SerializeObject(APIBag);
                    }
                }
                #endregion
                #region 写入MongoDB日志
                SaveFactMongodbLog(ExCode, CommunityID, T133LeafID, 0, 0, 0, 0, "", "", "", "");
                #endregion
                APIBag.Rows = dt;
                APIBag.ErrCode = 0;
                APIBag.ErrText = errText;
                return JsonConvert.SerializeObject(APIBag);
            }
            catch (Exception err) //异常捕获
            {
                APIBag.ErrCode = 9999;
                APIBag.ErrText = "发生异常：" + err.Message;
                return JsonConvert.SerializeObject(APIBag);
            }
        }
        /// <summary>
        ///    保存日志到MongoDB
        /// </summary>
        /// <param name="CommunityID"></param>
        /// <param name="T133LeafID"></param>
        /// <param name="SysLogID"></param>
        /// <param name="T216LeafID"></param>
        /// <param name="T102LeafID"></param>
        /// <param name="T107LeafID"></param>
        /// <param name="WIP_Code"></param>
        /// <param name="WIP_ID_Type_Code"></param>
        /// <param name="WIP_ID_Code"></param>
        /// <param name="Params"></param>
        /// <returns></returns>

        public string SaveFactMongodbLog(string ExCode, int CommunityID, int T133LeafID,Int64 SysLogID
                ,int T216LeafID ,int T102LeafID,int T107LeafID
                ,string WIP_Code,string WIP_ID_Type_Code, string WIP_ID_Code
                ,string Params)
        { 
            string sqlconnString = string.Empty;

            dynamic res = new System.Dynamic.ExpandoObject();
            try
            {  
                string UserCode = ""; 
                if (SysLogID.ToString() == null)
                {
                    SysLogID = 1;
                } 
                if (T216LeafID.ToString() == null)
                {
                    SysLogID = 0;
                } 
                if (T107LeafID.ToString() == null)
                {
                    SysLogID = 0;
                } 
                if (WIP_Code == null)
                {
                    WIP_Code = "";
                }
                if (WIP_ID_Type_Code == null)
                {
                    WIP_ID_Type_Code = "";
                } 
                if (WIP_ID_Code == null)
                {
                    WIP_ID_Code = "";
                } 
                if (Params == null)
                {
                    Params = "";
                }  
                string StoreTime = DateTime.Now.ToString();
                string strconn = "mongodb://192.168.57.12:27017";
                string SeqServerAddr = "192.168.57.13";
                ReadConfig.Read();
                if (ReadConfig.Parameters.ContainsKey("MongodbConnectStr"))
                {
                    strconn = ReadConfig.Parameters["MongodbConnectStr"].ToString();
                }
              /*  sqlconnString = ReadConfig.Parameters["ConnectionString"].ToString()*/; 
                if (ReadConfig.Parameters.ContainsKey("SeqServerAddr"))
                {
                    SeqServerAddr = ReadConfig.Parameters["SeqServerAddr"].ToString();
                } 
                //创建数据库链接
                MongoClient client = new MongoClient(strconn);
                //获得数据库cnblogs 
                IMongoDatabase db = client.GetDatabase(MongoDBName);
                IMongoCollection<GraphEntity> logTBL
                        = db.GetCollection<GraphEntity>("IRAPDCSLog"); 
                GraphEntity row = new GraphEntity();
                row.LogID = IRAPUtil.IRAPSocketClient.GetSequenceNo(SeqServerAddr, "NextSysLogID", 1);
                row.ExCode = ExCode;
                row.CommunityID = CommunityID;
                row.UserCode = UserCode;
                row.SysLogID = SysLogID;
                row.T133LeafID = T133LeafID;
                row.T216LeafID = T216LeafID;
                row.T102LeafID = T102LeafID;
                row.T107LeafID = T107LeafID;
                row.WIP_Code = WIP_Code;
                row.WIP_ID_Type_Code = WIP_ID_Type_Code;
                row.WIP_ID_Code = WIP_ID_Code;
                row.Params = Params;
                row.StoreTime = StoreTime; 
                logTBL.InsertOne(row);
                res.ErrCode = 0;
                res.ErrText = "保存成功！";
                return JsonConvert.SerializeObject(res);
            }
            catch (Exception err)
            {
                res.ErrCode = 9999;
                res.ErrText = "保存数据发生异常：" + err.Message;
                return JsonConvert.SerializeObject(res);
            }
        }
        /// <summary>
        ///    同步设备状态
        /// </summary>
        /// <param name="CommunityID">社区</param>
        /// <param name="UserCode">用户代码</param>
        /// <param name="SysLogID">系统登录号</param>
        /// <param name="T133LeafID">设备叶子</param>
        /// <param name="T216LeafID">工序叶子</param>
        /// <param name="T102LeafID">产品叶子</param>
        /// <param name="ParamXML">设备状态值</param>
        /// <returns></returns>
        public string GetOPCStatus(string ExCode,int CommunityID,string UserCode
            ,Int64 SysLogID,int T133LeafID, int T216LeafID, int T102LeafID
           , string ParamXML )
        {
            try
            {
                //解析设备状态值
                dynamic dn = ParamXML.GetSimpleObjectFromJson();
                Int64 Equipment_Running_Mode =( Boolean.Parse(dn.Equipment_Running_Mode.ToString()) == true)?1:0; // 设备运行模式 
                Int64 Equipment_Power_On = (Boolean.Parse(dn.Equipment_Power_On.ToString()) == true) ? 1 : 0; // 设备是否加电 
                Int64 Equipment_Fail = (Boolean.Parse(dn.Equipment_Fail.ToString()) == true) ? 1 : 0; // 设备是否失效 
                Int64 Tool_Fail = (Boolean.Parse(dn.Tool_Fail.ToString()) == true) ? 1 : 0;// 工装是否失效 
                Int64 Cycle_Started = (Boolean.Parse(dn.Cycle_Started.ToString()) == true) ? 1 : 0; // 工序循环是否开始 
                Int64 Equipment_Starvation = (Boolean.Parse(dn.Equipment_Starvation.ToString()) == true) ? 1 : 0;// 设备饥饿状态 

                //Int64 Status = Equipment_Running_Mode * Int64.Parse(Math.Pow(2, 7).ToString())
                //              + Equipment_Power_On * Int64.Parse(Math.Pow(2, 9).ToString())
                //              + Equipment_Fail * 1L
                //              + Tool_Fail * Int64.Parse(Math.Pow(2, 10).ToString())
                //              + Cycle_Started * Int64.Parse(Math.Pow(2, 11).ToString())
                //              + Equipment_Starvation * Int64.Parse(Math.Pow(2, 1).ToString());
                Int64 Status = Equipment_Running_Mode * 128L
                            + Equipment_Power_On * 512L
                            + Equipment_Fail * 1L
                            + Tool_Fail * 1024L
                            + Cycle_Started * 2048L
                            + Equipment_Starvation * 2L;
                if (CommunityID == 0)
                {
                    CommunityID = 57280;
                }
                long PartitioningKey = CommunityID * 10000L+133L;

                ReadConfig.Read();
                string conStr = ReadConfig.Parameters["ConnectionString"].ToString();

                #region 写入MongoDB日志
                SaveFactMongodbLog(ExCode, CommunityID, T133LeafID, SysLogID, T216LeafID, T102LeafID
                                , 0, "", "", "", ParamXML);
                #endregion

                IDbContext IRAPMDMContext = DBContextFactory.Instance.CreateContext("IRAPMDMContext");      //使用IRAPMDM数据库
                var _stb058 = IRAPMDMContext.Set<ETreeBizLeaf>()
                                            .Where(c => c.PartitioningKey == PartitioningKey && c.LeafID == T133LeafID)
                                            .FirstOrDefault();
                int T133EntityID = _stb058.EntityID; 
                string T133Code =   _stb058.Code;
                if (T133EntityID.ToString() == null)
                {
                    APIBag.ErrCode = 99999;
                    APIBag.ErrText = "无效的设备叶标识:" + T133EntityID + "，事件:GetOPCStatus加载完成";
                    return JsonConvert.SerializeObject(APIBag);
                }
                var _stb060 = IRAPMDMContext.Set<ETreeBiz060>()
                                            .Where(c => c.PartitioningKey == PartitioningKey && c.EntityID == T133EntityID)
                                            .FirstOrDefault();
                if (_stb060 == null)
                {
                    APIBag.ErrCode = 99998;
                    APIBag.ErrText = "设备:" + T133Code + "基础信息不完整，事件:GetOPCStatus加载完成";
                    return JsonConvert.SerializeObject(APIBag);
                }
                _stb060.EntityStatus = Status;
                IRAPMDMContext.SaveChanges(); 

                APIBag.ErrCode = 0;
                APIBag.ErrText = "设备:" + T133Code + "状态属性属性已经更新，事件:GetOPCStatus加载完成";
                return JsonConvert.SerializeObject(APIBag);
                /****
                 *调用脚本的方式
                int resultData = 0;
                //string updateSql = "update IRAPMDM..stb060 SET EntityStatus= "+ Status +
                //    "  WHERE PartitioningKey=" + PartitioningKey + " AND EntityID=" + T133EntityID;
                //using (var con = new SqlConnection(conStr))
                //{
                //    try
                //    {
                //        resultData = con.Execute(updateSql, null);
                //    }
                //    catch (Exception ex)
                //    {
                //        throw ex;
                //    }
                //}
                #region 写入MongoDB日志
                SaveFactMongodbLog(ExCode,CommunityID, T133LeafID, SysLogID, T216LeafID, T102LeafID
                                , 0, "", "", "", ParamXML);
                #endregion
                if (resultData > 0)
                {
                    APIBag.ErrCode = 0;
                    APIBag.ErrText = "设备:" + T133Code + "状态属性属性已经更新，事件:GetOPCStatus加载完成";
                    return JsonConvert.SerializeObject(APIBag);
                }
                else 
                {
                    APIBag.ErrCode = 9999;
                    APIBag.ErrText = "设备:" + T133Code + "状态属性属性更新0行，事件:GetOPCStatus加载完成";
                    return JsonConvert.SerializeObject(APIBag);
                }
                ***/ 
            }
            catch (Exception err) //异常捕获
            {
                APIBag.ErrCode = 9999;
                APIBag.ErrText = "发生异常：" + err.Message;
                return JsonConvert.SerializeObject(APIBag);
            }
        }
        /// <summary>
        ///    IDPBing事件
        /// </summary>
        /// <param name="ExCode"></param>
        /// <param name="CommunityID"></param>
        /// <param name="T133LeafID"></param>
        /// <returns></returns>
        public string IDBinding(string ExCode, int CommunityID, string UserCode
            ,Int64 SysLogID, int T133LeafID, int T216LeafID, int T102LeafID, int T107LeafID
            , string WIP_Code, string WIP_ID_Type_Code, string WIP_ID_Code
            ,string ParamXML)
        {
            try
            {
                if (CommunityID == 0)
                {
                    CommunityID = 57280;
                }
                string ConnIRAP = ConfigurationManager.ConnectionStrings["IRAPMDMContext"].ConnectionString;
                #region 执行存储过程
                SqlConnection sqlCon = null;
                DataTable dt = new DataTable();
                SqlCommand sqlComm = null;
                string sqlName = "usp_IRAPUES_DCS_IDBinding";
                int errCode;
                string errText;
                int part_Number_Feedback;
                using (sqlCon = new SqlConnection(ConnIRAP))
                {
                    sqlCon.Open();
                    sqlComm = new SqlCommand(sqlName, sqlCon);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.Parameters.Add("@CommunityID", SqlDbType.Int);  //社区标识 
                    sqlComm.Parameters.Add("@UserCode", SqlDbType.NVarChar, 400);  //设备信息
                    sqlComm.Parameters.Add("@SysLogID", SqlDbType.BigInt);  //系统登录标识
                    sqlComm.Parameters.Add("@T133LeafID", SqlDbType.Int);  //设备信息识
                    sqlComm.Parameters.Add("@T216LeafID", SqlDbType.Int);  //设备信息
                    sqlComm.Parameters.Add("@T102LeafID", SqlDbType.Int);  //设备信息
                    sqlComm.Parameters.Add("@T107LeafID", SqlDbType.Int);  //设备信息
                    sqlComm.Parameters.Add("@WIP_Code", SqlDbType.NVarChar, 400);
                    sqlComm.Parameters.Add("@WIP_ID_Type_Code", SqlDbType.NVarChar, 400);
                    sqlComm.Parameters.Add("@WIP_ID_Code", SqlDbType.NVarChar, 400);
                    sqlComm.Parameters.Add("@ParamXML", SqlDbType.Xml);
                    sqlComm.Parameters.Add("@Part_Number_Feedback", SqlDbType.Int);
                    sqlComm.Parameters.Add("@ErrCode", SqlDbType.Int);
                    sqlComm.Parameters.Add("@ErrText", SqlDbType.NVarChar, 400);
                    sqlComm.Parameters["@CommunityID"].Value = CommunityID;
                    sqlComm.Parameters["@UserCode"].Value = UserCode;
                    sqlComm.Parameters["@SysLogID"].Value = SysLogID;
                    sqlComm.Parameters["@T133LeafID"].Value = T133LeafID;
                    sqlComm.Parameters["@T216LeafID"].Value = T216LeafID;
                    sqlComm.Parameters["@T102LeafID"].Value = T102LeafID;
                    sqlComm.Parameters["@T107LeafID"].Value = T107LeafID;
                    sqlComm.Parameters["@WIP_Code"].Value = WIP_Code;
                    sqlComm.Parameters["@WIP_ID_Type_Code"].Value = WIP_ID_Type_Code;
                    sqlComm.Parameters["@WIP_ID_Code"].Value = WIP_ID_Code;
                    sqlComm.Parameters["@ParamXML"].Value = ParamXML;
                    sqlComm.Parameters["@Part_Number_Feedback"].Direction = ParameterDirection.Output;
                    sqlComm.Parameters["@ErrCode"].Direction = ParameterDirection.Output;
                    sqlComm.Parameters["@ErrText"].Direction = ParameterDirection.Output;
                    sqlComm.ExecuteNonQuery();
                    part_Number_Feedback = int.Parse(sqlComm.Parameters["@part_Number_Feedback"].Value.ToString());
                    errCode = int.Parse(sqlComm.Parameters["@ErrCode"].Value.ToString());
                    errText = sqlComm.Parameters["@ErrText"].Value.ToString();
                    sqlCon.Close();
                    if (errCode != 0)
                    {
                        APIBag.Part_Number_Feedback = part_Number_Feedback;
                        APIBag.ErrCode = errCode;
                        APIBag.ErrText = errText;
                        return JsonConvert.SerializeObject(APIBag);
                    }
                }
                #endregion
                #region 写入MongoDB日志
                SaveFactMongodbLog(ExCode, CommunityID, T133LeafID, SysLogID, T216LeafID, T102LeafID
                                , T107LeafID, WIP_Code,WIP_ID_Type_Code , WIP_ID_Code, ParamXML);
                #endregion
                APIBag.Part_Number_Feedback = part_Number_Feedback;
                APIBag.ErrCode = 0;
                APIBag.ErrText = errText;
                return JsonConvert.SerializeObject(APIBag);
            }
            catch (Exception err) //异常捕获
            {
                APIBag.Part_Number_Feedback = 0;
                APIBag.ErrCode = 9999;
                APIBag.ErrText = "发生异常：" + err.Message;
                return JsonConvert.SerializeObject(APIBag);
            }
        }

        /***
        public string WriteMongodbLog(string clientID, string msgFormat, string inParam)
        {
            dynamic dn = inParam.GetSimpleObjectFromJson();

            string sqlconnString = string.Empty;

            dynamic res = new System.Dynamic.ExpandoObject();
            try
            {
                int T133LeafID = int.Parse(dn.T133LeafID);
                int CommunitID = int.Parse(dn.CommunitID);
                string ExCode = dn.ExCode.ToString();
                string UserCode = dn.UserCode.ToString()?null :"";
                Int64 SysLogID = Int64.Parse(dn.SysLogID);
                if (SysLogID.ToString() == null)
                {
                    SysLogID = 1;
                }
                int T216LeafID = int.Parse(dn.T216LeafID);
                int T102LeafID = int.Parse(dn.T102LeafID);
                int T107LeafID = int.Parse(dn.T107LeafID);
                string WIP_Code = dn.WIP_Code.ToString() ? null : "";
                string WIP_ID_Type_Code = dn.WIP_ID_Type_Code.ToString() ? null : "";
                string WIP_ID_Code = dn.WIP_ID_Code.ToString() ? null : "";
                string Params = dn.Params.ToString() ? null : "";
                string StoreTime = DateTime.Now.ToString();  
                string strconn = "mongodb://192.168.57.12:27017";
                ReadConfig.Read();
                if (ReadConfig.Parameters.ContainsKey("MongodbConnectStr"))
                {
                    strconn = ReadConfig.Parameters["MongodbConnectStr"].ToString();
                }
                sqlconnString = ReadConfig.Parameters["ConnectionString"].ToString(); 
                //创建数据库链接
                MongoClient client = new MongoClient(strconn);
                //获得数据库cnblogs 
                IMongoDatabase db = client.GetDatabase(MongoDBName);

                IMongoCollection<GraphEntity> logTBL 
                        = db.GetCollection<GraphEntity>("IRAPDCSLog"); 
                //反序列化对象
                GraphEntity obj = JsonConvert.DeserializeObject<GraphEntity>(inParam);   
                XmlDocument doc = new XmlDocument();
                GraphEntity row = new GraphEntity(); 
                row.ExCode = ExCode;
                row.CommunityID = CommunityID;
                row.UserCode = UserCode;
                row.SysLogID = SysLogID;
                row.T133LeafID = T133LeafID;
                row.T216LeafID = T216LeafID;
                row.T102LeafID = T102LeafID;
                row.T107LeafID = T107LeafID;
                row.WIP_Code = WIP_Code;
                row.WIP_ID_Type_Code = WIP_ID_Type_Code;
                row.WIP_ID_Code = WIP_ID_Code;
                row.Params = Params;
                row.StoreTime = StoreTime;
                 
                logTBL.InsertOne(row);
                res.ErrCode = 0;
                res.ErrText = "保存成功！"; 
                return JsonConvert.SerializeObject(res);
            }
            catch (Exception err)
            {
                res.ErrCode = 9999;
                res.ErrText = "保存数据发生异常：" + err.Message;
                return JsonConvert.SerializeObject(res);
            }
        }
        /// <summary>
        /// 测试脚本
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public APIResult Test2(string access_token, string MDItemCode)
        { 
            try
            {
                IRAPLog log = new IRAPLog();
                if (log.GetLogIDByToken(access_token) == null)
                {
                    APIBag.ErrCode = 9999;
                    APIBag.ErrText = "access_token错误！";
                    return APIBag;
                }
                long sysID = log.GetLogIDByToken(access_token).SysLogID;
                int communityID = 0;
                if (access_token == "1")
                {
                    communityID = 57280;
                }
                else if (access_token == "2")
                {
                    communityID = 60037;
                }
                else
                {
                    communityID = GetCommunityID(access_token);
                }
                int TreeInfo = int.Parse(MDItemCode.Substring(1, 4));
                if (MDItemCode.Substring(0, 1) == "C")
                {
                    TreeInfo = -TreeInfo;
                }
                int RsInfo = MDItemCode.Length == 8 ? int.Parse(MDItemCode.Substring(6, 2)) : 0;
                string ConnIRAP = ConfigurationManager.ConnectionStrings["IRAPMDMContext"].ConnectionString;
                #region 执行存储过程
                SqlConnection sqlCon = null;
                DataTable dt = new DataTable();
                using (sqlCon = new SqlConnection(ConnIRAP))
                { 
                    sqlCon.Open();
                    SqlCommand cmd = sqlCon.CreateCommand();
                    cmd.CommandText = "EXEC IRAP..Test @communityID,@TreeInfo,@RsInfo,@SysLogID,@ErrCode OUT,@ErrText OUT";
                    cmd.CommandType = CommandType.Text;
                    SqlParameter p1 = new SqlParameter("@communityID", communityID);
                    SqlParameter p2 = new SqlParameter("@TreeInfo", TreeInfo);
                    SqlParameter p3 = new SqlParameter("@RsInfo", RsInfo);
                    SqlParameter p4 = new SqlParameter("@SysLogID", sysID); 
                    cmd.Parameters.Add(p1);
                    cmd.Parameters.Add(p2);
                    cmd.Parameters.Add(p3);
                    cmd.Parameters.Add(p4);
                    cmd.Parameters.Add("@ErrCode", SqlDbType.Int);
                    cmd.Parameters.Add("@ErrText", SqlDbType.NVarChar, 400);
                    cmd.Parameters["@ErrCode"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@ErrText"].Direction = ParameterDirection.Output;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(dt);
                    sqlCon.Close();
                }
                #endregion
                APIBag.Rows = dt;
                APIBag.ErrCode = 0;
                APIBag.ErrText = "返回成功！";
                return JsonConvert.SerializeObject(APIBag);
            }
            catch (Exception err)
            {
                APIBag.ErrCode = 9999;
                APIBag.ErrText = "发生异常：" + err.Message;
                return JsonConvert.SerializeObject(APIBag);
            } 
        }
        /// <summary>
        ///   测试结果
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="msgFormat"></param>
        /// <param name="inParam"></param>
        /// <returns></returns>
        public APIResult Test(string clientID, string msgFormat, string inParam)
        { 
            try
            {
                dynamic dn = inParam.GetSimpleObjectFromJson();
                string access_token = dn.access_token.ToString(); // 参数
                int communityID = Int32.Parse(dn.CommunityID.ToString()); // 参数
                int _partitioningKey = communityID * 10000;

                IRAPLog log = new IRAPLog();
                if (log.GetLogIDByToken(access_token) == null)
                {
                    APIBag.ErrCode = 9999;
                    APIBag.ErrText = "access_token错误！";
                    return APIBag;
                }
                long sysID = log.GetLogIDByToken(access_token).SysLogID;
                if (access_token == "1")
                {
                    communityID = 57280;
                }
                else if (access_token == "2")
                {
                    communityID = 60037;
                }
                else
                {
                    communityID = GetCommunityID(access_token);
                }
                long sysLogID = sysID;
                if (sysLogID == 0)
                {
                    APIBag.ErrCode = 999999;
                    APIBag.ErrText = "登录失效，请从新登录"; 
                } 
                string conStr = ConfigurationManager.ConnectionStrings["IRAPMDMContext"].ConnectionString;
                int errCode = 0;
                string errText = "";
                using (IDbConnection con = new SqlConnection(conStr))
                {
                    DynamicParameters dp = new DynamicParameters();
                    dp.Add("@CommunityID", communityID); 
                    dp.Add("@SysLogID", sysLogID);
                    dp.Add("@ErrCode", 0, DbType.Int32, ParameterDirection.Output);
                    dp.Add("@ErrText", "", DbType.String, ParameterDirection.Output);
                    con.ExecuteReader("IRAPDCS.dbo.ssp_Test", dp, null, null, CommandType.StoredProcedure);
                    errCode = dp.Get<Int32>("@ErrCode");
                    errText = dp.Get<string>("@ErrText");
                }
                if (errCode == 0)
                {
                    APIBag.ErrCode = errCode;
                    APIBag.ErrText = errText;
                    return APIBag;
                }
                else
                {
                    APIBag.ErrCode = 999;
                    APIBag.ErrText = "失败:" + errText;
                    return APIBag;
                } 
            }
            catch (Exception err)
            {
                APIBag.ErrCode = 9999;
                APIBag.ErrText = "提交异常：" + err.Message;
                return APIBag;
            }
        }
        ***/
    }
}
