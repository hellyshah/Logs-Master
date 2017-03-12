using System;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
//using Logs.WCF_SS;
//using Logs.newWEBWCF;
//using Logs.WCF;
using InsertSqlServerWCF;
using System.ServiceModel;

namespace Logs
{
    public partial class Logs : ServiceBase
    {
        UserDetails objuse = new UserDetails();
        Service1Client obj = new Service1Client();
        //WCF.Service1Client objService = new WCF.Service1Client();
        CommonModule objCom = new CommonModule();
        SqlHelper objSql = new SqlHelper();
        //UserDetails UserInfo = new UserDetails();
        public Logs()
        {
            InitializeComponent();
        }
        protected override void OnStart(string[] args)
        {
            objCom.WriteToEventLog("Logs Event Started Successfully...");
        }
        protected override void OnStop()
        {
            //Application.Exit();
            objCom.WriteToEventLog("Logs Event Stopped Successfully...");
            //objCom.WriteToEventLog("Stop Event....");
        }
        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            int mode = 0, clockId = 0, clock = 0, id = 0;
            DateTime timestamp = DateTime.MaxValue;
            string remarks = string.Empty;
            string username = string.Empty;
            timestamp = DateTime.Now;
            string cQuery = String.Empty;
            string idQuery = String.Empty;
            
            try
            {
                switch (changeDescription.Reason)
                {
                    case SessionChangeReason.SessionLogon:
                        objCom.WriteToEventLog("session Log On Starting......");
                        int mode_remote;
                        string un_remQue = "select mode from tblMutex Order By TimeStamp Desc Limit 1";
                        try
                        {
                            mode_remote = Convert.ToInt32(objSql.ExecuteScalar(objSql.connString(), CommandType.Text, un_remQue));
                            objCom.WriteToEventLog("mode_remote" + mode_remote);
                            if (mode_remote == 5)
                            {
                                string Query = "select username from tblMutex where mode = 5 Order By TimeStamp Desc Limit 1";
                                try
                                {
                                    username = objSql.ExecuteScalar(objSql.connString(), CommandType.Text, Query).ToString();
                                   
                                }
                                catch (Exception h)
                                {
                                    objCom.WriteToEventLog("cant get username in remote & logon..."+h.Message);
                                }
                            }
                            else
                            {
                                username = objCom.GetInfo();
                            }
                        }
                        catch (Exception f)
                        {
                            objCom.WriteToEventLog("Cant get mode_remote..."+f.Message);
                        }
                        
                        cQuery = "select ClockStatus from tblMutex Order By TimeStamp Desc Limit 1";
                        idQuery = "select Id from tblMutex where Mode = 0 order by TimeStamp Desc Limit 1";
                        try
                        {
                            clock = Convert.ToInt32(objSql.ExecuteScalar(objSql.connString(), CommandType.Text, cQuery));
                            id = Convert.ToInt32(objSql.ExecuteScalar(objSql.connString(), CommandType.Text, idQuery));
                        }
                        catch (Exception q)
                        {
                            objCom.WriteToEventLog("Exception In Session LogIn Inner Exception..." + q.Message);
                        }
                        mode = Convert.ToInt32(CommonModule.sessionEvents.Logon);
                        clockId = Convert.ToInt32(CommonModule.Clock.clockin);
                        if (clock == 1)
                        {
                            try
                            {
                                string updateQuery = "update tblMutex set ClockStatus = 0 where Id = " + id;
                                objSql.ExecuteNonQuery(objSql.connString(), CommandType.Text, updateQuery);
                            }
                            catch (Exception p)
                            {
                                objCom.WriteToEventLog("Cant Find ID in Session Log Event...." + p.Message);
                            }
                        }
                        try
                        {
                            string query = "insert into tblmutex (userName,TimeStamp,Mode,ClockStatus) values ('" + username + "','" + timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "','" + mode + "','" + clockId + "')";
                            objSql.ExecuteNonQuery(objSql.connString(), CommandType.Text, query);
                            objuse.Mode = mode;
                            objuse.UserName = username;
                            objuse.TimeStamp = timestamp;
                            objuse.ClockStatus = clockId;
                            objCom.WriteToEventLog("Property Got in WCF Login....");
                            try
                            {
                                obj.InsertUserDetails(objuse);
                            }
                            catch (Exception o)
                            {
                                objCom.WriteToEventLog("Error in SQLSERVER LogIn...."+o.Message);
                            }
                            //UserInfo.UserName = username;
                            //UserInfo.TimeStamp = timestamp;
                            //UserInfo.Mode = mode;
                            //UserInfo.ClockStatus = clockId;
                            //objService.InsertUserDetails(UserInfo);
                            //objService.InsertUserDetails(userinfo);
                        }
                        catch (Exception r)
                        {
                            objCom.WriteToEventLog("Cant Insert in to Table in Session Login..."+ r.Message);
                        }
                        objCom.WriteToEventLog("session Log On : " + username, "Logs", EventLogEntryType.Information);
                        objCom.WriteToEventLog("session Log On Stop......");
                        break;
                    case SessionChangeReason.SessionLogoff:
                        objCom.WriteToEventLog("session LogOff Starting......");
                        string user = String.Empty;
                        string Que1 = "select username from tblmutex order by timestamp desc limit 1 ";
                        cQuery = "select ClockStatus from tblmutex order by timestamp desc limit 1";

                        try
                        {
                            clock = Convert.ToInt32(objSql.ExecuteScalar(objSql.connString(), CommandType.Text, cQuery));
                        }
                        catch (Exception p)
                        {
                            objCom.WriteToEventLog("cant find ClockId in Session LogOff Event..." + p.Message);
                        }
                        mode = Convert.ToInt32(CommonModule.sessionEvents.Logout);
                        clockId = Convert.ToInt32(CommonModule.Clock.clockout);
                        if (clock == 1)
                        {
                            try
                            {
                                user = objSql.ExecuteScalar(objSql.connString(), CommandType.Text, Que1).ToString();
                                string query1 = "insert into tblMutex (userName,TimeStamp,Mode,ClockStatus) values ('" + user + "','" + timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "','" + mode + "','" + clockId + "')";
                                try
                                {
                                    objSql.ExecuteNonQuery(objSql.connString(), CommandType.Text, query1);
                                    objuse.Mode = mode;
                                    objuse.UserName = username;
                                    objuse.TimeStamp = timestamp;
                                    objuse.ClockStatus = clockId;
                                    objCom.WriteToEventLog("Property Got in WCF logOff clock==1....");
                                    try
                                    {
                                        obj.InsertUserDetails(objuse);
                                    }
                                    catch (Exception o)
                                    {
                                        objCom.WriteToEventLog("Error in SQLSERVER logOff clock==1...."+o.Message);
                                    }
                                }
                                catch (Exception v)
                                {
                                    objCom.WriteToEventLog("cant get name in sessssssssion log out...."+v.Message);
                                }
                            }
                            catch (Exception r)
                            {
                                objCom.WriteToEventLog("Cant Insert in to Table in Session LogOff Clock == 1..." + r.Message);
                            }
                        }
                        else
                        {
                            try
                            {
                                user = "select username from tblmutex where mode = 0 and ClockStatus = 0 order by timestamp desc limit 1";
                                user = objSql.ExecuteScalar(objSql.connString(), CommandType.Text, user).ToString();
                                string query1 = "insert into tblMutex (userName,TimeStamp,Mode,ClockStatus) values ('" + user + "','" + timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "','" + mode + "','" + clockId + "')";
                                objSql.ExecuteNonQuery(objSql.connString(), CommandType.Text, query1);
                                objuse.Mode = mode;
                                objuse.UserName = username;
                                objuse.TimeStamp = timestamp;
                                objuse.ClockStatus = clockId;
                                objCom.WriteToEventLog("Property Got in WCF logOff clock!=1....");
                                try
                                {
                                    obj.InsertUserDetails(objuse);
                                }
                                catch (Exception o)
                                {
                                    objCom.WriteToEventLog("Error in SQLSERVER logOff clock!=1...."+o.Message);
                                }
                            }
                            catch (Exception z)
                            {
                                objCom.WriteToEventLog("Cant Insert in to Table in Session LogOff ClockStatus==1..." + z.Message);
                            }
                        }
                        objCom.WriteToEventLog("session Log Off i.e shutdown or signout : " + username, "Logs", EventLogEntryType.Information);
                        objCom.WriteToEventLog("Session LogOff Stop...");
                        break;
                    case SessionChangeReason.SessionLock:
                        objCom.WriteToEventLog("Session Lock Is starting...");
                        try 
                        {
                            string unnameQue = "select username from tblmutex order by timestamp desc";
                            string user_lock = objSql.ExecuteScalar(objSql.connString(), CommandType.Text, unnameQue).ToString();
                            mode = Convert.ToInt32(CommonModule.sessionEvents.Lock);
                            clockId = Convert.ToInt32(CommonModule.Clock.clockin);
                            try
                            {
                                string query1 = "insert into tblMutex (userName,TimeStamp,Mode,ClockStatus) values ('" + user_lock + "','" + timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "','" + mode + "','" + clockId + "')";
                                objSql.ExecuteNonQuery(objSql.connString(), CommandType.Text, query1);
                                objuse.Mode = mode;
                                objuse.UserName = user_lock;
                                objuse.TimeStamp = timestamp;
                                objuse.ClockStatus = clockId;
                                objCom.WriteToEventLog("Property Got in WCF logck....");
                                try
                                {
                                    obj.InsertUserDetails(objuse);
                                }
                                catch (Exception o)
                                {
                                    objCom.WriteToEventLog("Error in SQLSERVER lock...."+o.StackTrace);
                                }
                            }
                            catch(Exception i)
                            {
                                objCom.WriteToEventLog("cant insert into table in lock ...."+i.Message);
                            }
                        }
                        catch (Exception u)
                        {
                            objCom.WriteToEventLog("Error in Session Lock username..."+u.Message);
                        }
                        objCom.WriteToEventLog("session Lock : " + username, "Logs", EventLogEntryType.Information);
                        objCom.WriteToEventLog("Session Lock Stop...");
                        break;
                        
                        //back up code....
                        //try
                        //{
                        //    string que = "select mode from tblMutex order by timestamp desc limit 1";
                        //    int lmode = Convert.ToInt32(objSql.ExecuteScalar(objSql.connString(),CommandType.Text,que));
                        //    objCom.WriteToEventLog("lmode in session lock event////   " + lmode);
                        //    if (lmode == 0)
                        //    {                                
                        //        try
                        //        {
                        //            username = objCom.GetInfo(); 
                        //            clockId = Convert.ToInt32(CommonModule.Clock.clockin);
                        //            mode = Convert.ToInt32(CommonModule.sessionEvents.Lock);
                        //            string query2 = "insert into tblMutex (userName,TimeStamp,Mode,ClockStatus) values ('" + username + "','" + timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "','" + mode + "','" + clockId + "')";
                        //            objSql.ExecuteNonQuery(objSql.connString(), CommandType.Text, query2);
                        //        }
                        //        catch (Exception z)
                        //        {
                        //            objCom.WriteToEventLog("Cant Insert in to Table in SessionLock..." + z.Message);
                        //        }
                        //    }
                        //    else
                        //    {                                
                        //        try
                        //        {
                        //            string query_uname = "select username from tblMutex order by timestamp desc limit 1";
                        //            clockId = Convert.ToInt32(CommonModule.Clock.clockin);
                        //            mode = Convert.ToInt32(CommonModule.sessionEvents.Lock);
                        //            username = objSql.ExecuteScalar(objSql.connString(), CommandType.Text, query_uname).ToString();
                        //            objCom.WriteToEventLog("username got in lock phase");
                        //            try
                        //            {
                        //                mode = Convert.ToInt32(CommonModule.sessionEvents.Lock);
                        //                clockId = Convert.ToInt32(CommonModule.Clock.clockin);
                        //                string query2 = "insert into tblMutex (userName,TimeStamp,Mode,ClockStatus) values ('" + username + "','" + timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "','" + mode + "','" + clockId + "')";
                        //                objSql.ExecuteNonQuery(objSql.connString(), CommandType.Text, query2);
                        //            }
                        //            catch (Exception z)
                        //            {
                        //                objCom.WriteToEventLog("Cant Insert in to Table in SessionLock..." + z.Message);
                        //            }
                        //        }
                        //        catch (Exception r)
                        //        {
                        //            objCom.WriteToEventLog("Cant get username in sessionlock else block..."+r.Message);
                        //        }  
                        //    }
                        //}
                        //catch (Exception r)
                        //{
                        //    objCom.WriteToEventLog("Cant get mode in session lock for remote login ..."+r.Message);
                        //}                        
                        //objCom.WriteToEventLog("session Lock : " + username, "Logs", EventLogEntryType.Information);
                        //objCom.WriteToEventLog("Session Lock Stop...");
                        //break;
                    case SessionChangeReason.SessionUnlock:
                        objCom.WriteToEventLog("Session Lock Is starting...");
                        string quelock = "select username from tblMutex order by timestamp desc limit 1";
                        //int lockmode = 0;
                        mode = Convert.ToInt32(CommonModule.sessionEvents.UnLock);
                        clockId = Convert.ToInt32(CommonModule.Clock.clockin);
                        //username = objCom.GetInfo();
                        try
                        {
                            username = objSql.ExecuteScalar(objSql.connString(), CommandType.Text, quelock).ToString();
                            try
                            {
                                string query3 = "insert into tblMutex (userName,TimeStamp,Mode,ClockStatus) values ('" + username + "','" + timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "','" + mode + "','" + clockId + "')";
                                objSql.ExecuteNonQuery(objSql.connString(), CommandType.Text, query3);
                                objuse.Mode = mode;
                                objuse.UserName = username;
                                objuse.TimeStamp = timestamp;
                                objuse.ClockStatus = clockId;
                                objCom.WriteToEventLog("Property Got in WCF Unlock....");
                                try
                                {
                                    obj.InsertUserDetails(objuse);
                                }
                                catch (Exception o)
                                {
                                    objCom.WriteToEventLog("Error in SQLSERVER Unlock...."+o.StackTrace);
                                }
                            }
                            catch (Exception z)
                            {
                                objCom.WriteToEventLog("Cant Insert in to Table in SessionUnLock..." + z.Message);
                            }
                            objCom.WriteToEventLog("session unLock : " + username, "Logs", EventLogEntryType.Information);
                        }
                        catch (Exception o)
                        {
                            objCom.WriteToEventLog("Error in session lock username"+o.Message);
                        }
                        objCom.WriteToEventLog("Session Lock Is stop...");
                        break;







                        //Back Up.....
                        //objCom.WriteToEventLog("Session Lock Is starting...");
                        //string quelock = "select mode from tblMutex order by timestamp desc limit 1";
                        //int lockmode = 0;
                        //clockId = Convert.ToInt32(CommonModule.Clock.clockin);
                        //username = objCom.GetInfo();
                        //mode = Convert.ToInt32(CommonModule.sessionEvents.UnLock);
                        //try
                        //{
                        //    string query3 = "insert into tblMutex (userName,TimeStamp,Mode,ClockStatus) values ('" + username + "','" + timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "','" + mode + "','" + clockId + "')";
                        //    objSql.ExecuteNonQuery(objSql.connString(), CommandType.Text, query3);
                        //}
                        //catch (Exception z)
                        //{
                        //    objCom.WriteToEventLog("Cant Insert in to Table in SessionUnLock..." + z.Message);
                        //}
                        //objCom.WriteToEventLog("session unLock : " + username, "Logs", EventLogEntryType.Information);
                        //objCom.WriteToEventLog("Session Lock Is stop...");
                        //break;
                    case SessionChangeReason.RemoteConnect:
                        objCom.WriteToEventLog("Session RemoteConnect Is starting...");
                        //username = objCom.GetInfo();
                        int cId = 0;                        
                        mode = Convert.ToInt32(CommonModule.sessionEvents.RemoteConnect);
                        string clockQue = "select ClockStatus from tblMutex order by timestamp desc limit 1";
                        try
                        {
                            cId =Convert.ToInt32( objSql.ExecuteScalar(objSql.connString(), CommandType.Text, clockQue));
                        }
                        catch (Exception t)
                        {
                            objCom.WriteToEventLog("Exception in Remoteconnect clockstatus Query..." + t.Message);
                        }
                        if (cId == 0)
                        {
                            string clockQueIn = "select UserName from tblMutex where Clockstatus = 1 order by timestamp desc limit 1 ";
                            string uname_In;
                            
                            try
                            {
                                try
                                {
                                    uname_In = objSql.ExecuteScalar(objSql.connString(), CommandType.Text, clockQueIn).ToString();
                                    try
                                    {
                                        clockId = Convert.ToInt32(CommonModule.Clock.clockin);
                                        string query4 = "insert into tblMutex (userName,TimeStamp,Mode,ClockStatus) values ('" + uname_In + "','" + timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "','" + mode + "','" + clockId + "')";
                                        objSql.ExecuteNonQuery(objSql.connString(), CommandType.Text, query4);
                                        objuse.Mode = mode;
                                        objuse.UserName = username;
                                        objuse.TimeStamp = timestamp;
                                        objuse.ClockStatus = clockId;
                                        objCom.WriteToEventLog("Property Got in WCF Remote cId == 0....");
                                        try
                                        {
                                            obj.InsertUserDetails(objuse);
                                        }
                                        catch (Exception o)
                                        {
                                            objCom.WriteToEventLog("Error in SQLSERVER Remote cId == 0........"+o.Message);
                                        }
                                    }
                                    catch (Exception i)
                                    {
                                        objCom.WriteToEventLog("Cant insert in to CID= 0 Inner block..."+i.Message);
                                    }                                    
                                }
                                catch (Exception y)
                                {
                                    objCom.WriteToEventLog("Username is not found in remote connect" +y.Message);
                                }
                                
                            }
                            catch (Exception t)
                            {
                                objCom.WriteToEventLog("Exception in Remoteconnect clockstatus=0 Query..." + t.Message);
                            }
                        }
                        else
                        {
                            string clockQueIn = "select UserName from tblMutex where Clockstatus = 1 order by timestamp desc limit 1 ";
                            string uname_In = String.Empty;
                            clockId = Convert.ToInt32(CommonModule.Clock.clockin);
                            try
                            {
                                uname_In = objSql.ExecuteScalar(objSql.connString(), CommandType.Text, clockQueIn).ToString();
                                try
                                {                                    
                                    string query4 = "insert into tblMutex (userName,TimeStamp,Mode,ClockStatus) values ('" + uname_In + "','" + timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "','" + mode + "','" + clockId + "')";
                                    objSql.ExecuteNonQuery(objSql.connString(), CommandType.Text, query4);
                                    objuse.Mode = mode;
                                    objuse.UserName = username;
                                    objuse.TimeStamp = timestamp;
                                    objuse.ClockStatus = clockId;
                                    objCom.WriteToEventLog("Property Got in WCF Remote cId !=0........");
                                    try
                                    {
                                        obj.InsertUserDetails(objuse);
                                    }
                                    catch (Exception o)
                                    {
                                        objCom.WriteToEventLog("Error in SQLSERVER Remote cId !=0...."+o.Message);
                                    }
                                }
                                catch (Exception r)
                                {
                                    objCom.WriteToEventLog("Cant insert in to Remote Connect clockstatus=1 ..." + r.Message);
                                }                                
                            }
                            catch (Exception t)
                            {
                                objCom.WriteToEventLog("Exception in Remoteconnect clockstatus=1 Query..." + t.Message);
                            }
                        }
                        objCom.WriteToEventLog("session RemoteConnect : " + username, "Logs", EventLogEntryType.Information);
                        objCom.WriteToEventLog("Session RemoteConnect Is Stopp...");
                        break;
                    case SessionChangeReason.RemoteDisconnect:
                        objCom.WriteToEventLog("Session RemoteDisconnect Is Started...");
                        mode = Convert.ToInt32(CommonModule.sessionEvents.RemoteDisconnect);
                        clockId = Convert.ToInt32(CommonModule.Clock.clockin);
                        try
                        {
                            string unQue = string.Empty;
                            string uname = string.Empty;
                            unQue = "select username from tblMutex where mode = 5 order by timestamp desc limit 1";
                            try
                            {
                                uname = objSql.ExecuteScalar(objSql.connString(), CommandType.Text, unQue).ToString();
                            }
                            catch (Exception i)
                            {
                                objCom.WriteToEventLog("Exception in remote disconnect username is not found..."+i.Message);
                            }
                            try
                            {
                                string query5 = "insert into tblMutex (userName,TimeStamp,Mode,ClockStatus) values ('" + uname + "','" + timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "','" + mode + "','" + clockId + "')";
                                objSql.ExecuteNonQuery(objSql.connString(), CommandType.Text, query5);
                                objuse.Mode = mode;
                                objuse.UserName = username;
                                objuse.TimeStamp = timestamp;
                                objuse.ClockStatus = clockId;
                                objCom.WriteToEventLog("Property Got in WCF RemoteDis....");
                                try
                                {
                                    obj.InsertUserDetails(objuse);
                                }
                                catch (Exception o)
                                {
                                    objCom.WriteToEventLog("Error in SQLSERVER RemoteDis...."+o.Message);
                                }
                            }
                            catch (Exception i)
                            { 
                                objCom.WriteToEventLog("Exception in remote disconnect NonQuery is not found..."+i.Message);
                            }
                            
                        }
                        catch (Exception p)
                        {
                            objCom.WriteToEventLog("Cant Insert in to Table in Remote Disconnect..." + p.Message);
                        }
                        objCom.WriteToEventLog("session RemoteDisconnect : " + username, "Logs", EventLogEntryType.Information);
                        objCom.WriteToEventLog("Session RemoteDisconnect Is Stopp...");
                        break;
                    default:
                        string modeQue = "Select mode from tblMutex order by timestamp desc limit 1";
                        string unm = "select username from tblMutex order by timestamp desc limit 1";
                        string time_rcon = "select timestamp from tblMutex where mode = 5 order by timestamp desc limit 1;";
                        string time_rdiscon = "select timestamp from tblMutex where mode = 5 order by timestamp desc limit 1;";
                        DateTime remcon, remdiscon;
                        try
                        {
                            try
                            {
                                remcon = (DateTime)(objSql.ExecuteScalar(objSql.connString(), CommandType.Text, time_rcon));
                                try
                                {
                                    remdiscon = (DateTime)(objSql.ExecuteScalar(objSql.connString(), CommandType.Text, time_rdiscon));
                                    if (remcon.Second - remdiscon.Second == 1)
                                    {
                                        objCom.WriteToEventLog("remconn and rem discconn error thrown...");
                                        break;
                                    }
                                    else
                                    {
                                        int mode_deflt = Convert.ToInt32(objSql.ExecuteScalar(objSql.connString(), CommandType.Text, modeQue));
                                        int clock_in = Convert.ToInt32(CommonModule.Clock.clockin);
                                        int mode_remote1 = Convert.ToInt32(CommonModule.sessionEvents.Logon);
                                        string user_name = string.Empty;
                                        try
                                        {
                                            user_name = objSql.ExecuteScalar(objSql.connString(), CommandType.Text, unm).ToString();
                                        }
                                        catch (Exception f)
                                        {
                                            objCom.WriteToEventLog("cant find user name in default case..." + f.Message);
                                        }
                                        switch (mode_deflt)
                                        {
                                            case 6:

                                                string query6 = "insert into tblMutex (userName,TimeStamp,Mode,ClockStatus) values ('" + user_name + "','" + timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "','" + mode_remote1 + "','" + clock_in + "')";
                                                objSql.ExecuteNonQuery(objSql.connString(), CommandType.Text, query6);
                                                objCom.WriteToEventLog("Default Statement : Data Entered in default Statement.....");
                                                objuse.Mode = mode;
                                                objuse.UserName = username;
                                                objuse.TimeStamp = timestamp;
                                                objuse.ClockStatus = clockId;
                                                objCom.WriteToEventLog("Property Got in WCF case6 default....");
                                                try
                                                {
                                                    obj.InsertUserDetails(objuse);
                                                }
                                                catch (Exception o)
                                                {
                                                    objCom.WriteToEventLog("Error in SQLSERVER case6 default...."+o.Message);
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
                                catch (Exception y)
                                {
                                    objCom.WriteToEventLog("Error in getting remdiscon ..."+y.Message);
                                }
                            }
                            catch (Exception e)
                            {
                                objCom.WriteToEventLog("error to get remcon...in default" +e.Message);
                            }
                            
                        }
                        catch (Exception e)
                        {
                            objCom.WriteToEventLog("Error in gettin time of connect and disconnect...."+e.Message);
                        }
                        
                        
                        objCom.WriteToEventLog("Default Statement :" + username, "Logs", EventLogEntryType.Information);
                        break;                        
                }
            }
            catch (Exception z)
            {
                objCom.WriteToEventLog("OnSessionChange Z : " + z.Message, "Logs", EventLogEntryType.Error);
            }
        }
    }
}
