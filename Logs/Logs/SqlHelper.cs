using System;
using System.Data;
using System.Data.SQLite;


namespace Logs
{
    class SqlHelper
    {
        private int vAccountId = 0;
        public int AccountId
        {
            get { return vAccountId; }
            set { vAccountId = value; }
        }

        public string connString()
        {
            return @"Data Source=D:\sqliteDB\Mutex.db;";
            //+ vAccountId + ";Uid=camsUser;Pwd=p0w3rp4ck;";
            //Return "Data Source=88.150.212.50,7937;Database=tmSUK_" & vAccountId & ";Uid=camsUser;Pwd=p0w3rp4ck;"
        }

        ///// <summary>
        ///// Execute a OleDbCommand (that returns no resultset) against the database specified in the connection string 
        ///// using the provided parameters.
        ///// </summary>
        ///// <remarks>
        ///// e.g.:  
        /////  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new OleDbParameter("@prodid", 24));
        ///// </remarks>
        ///// <param name="connectionString">a valid connection string for a OleDbConnection</param>
        ///// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        ///// <param name="commandText">the stored procedure name or T-SQL command</param>
        ///// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        ///// <returns>an int representing the number of rows affected by the command</returns>

        public int ExecuteNonQuery(string connString, CommandType cmdType, string cmdText, SQLiteParameter[] cmdParms = null)
        {

            SQLiteCommand cmd = new SQLiteCommand();
            SQLiteConnection conn = new SQLiteConnection(connString);
            //Dim trans As SqlTransaction = conn.BeginTransaction("BuilderTransaction")
            try
            {
                PrepareCommand(ref cmd, ref conn, ref cmdType, ref cmdText, ref cmdParms);
                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return val;
            }

            catch (SQLiteException ex)
            {
                throw new Exception("ExecuteNonQuery SQL Exception " + ex.Message);
            }
            catch (Exception exx)
            {
                throw new Exception("ExecuteNonQuery Function" + exx.Message, exx);
                //Add this for finally closing the connection and destroying the command
            }
            finally
            {
                conn.Close();
                cmd = null;
                cmdParms = null;
            }
        }

        public SQLiteConnection OpenDatabaseConnection()
        {
            try
            {
                SQLiteConnection conn = new SQLiteConnection(connString());
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                return conn;
            }
            catch (Exception ex)
            {
                throw new Exception("OpenDatabaseConnection Function" + ex.Message);
            }
        }

        public SQLiteConnection OpenDatabaseAccConnection()
        {
            try
            {
                SQLiteConnection conn = new SQLiteConnection("Data Source=.\\TIMEMANAGER;Database=tmUKAccountMaster;Uid=camsUser;Pwd=p0w3rp4ck;");
                //data source=88.150.212.50,7937;Database=tmUKAccountMaster;Uid=camsUser;Pwd=p0w3rp4ck;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                return conn;
            }
            catch (Exception ex)
            {
                throw new Exception("OpenDatabaseAccConnection Function" + ex.Message, ex);
            }
        }

        public void CloseDatabaseConncetion(SQLiteConnection conn)
        {
            try
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("CloseDatabaseConncetion Function" + ex.Message, ex);
            }

        }

        public SQLiteCommand CreateCommand(string cmdText, ref SQLiteConnection conn)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            SQLiteCommand cmd = new SQLiteCommand();
            try
            {
                cmd.Connection = conn;
                cmd.CommandText = cmdText;
                return cmd;
            }
            catch (Exception ex)
            {
                throw new Exception("PrepareCommand : ", ex);
            }
        }

        public int ExecuteParmStoredProcedure(ref SQLiteCommand cmd, ref SQLiteConnection conn, ref SQLiteParameter[] cmdParms)
        {

            try
            {
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                if (!((cmdParms == null)))
                {
                    //SQLiteParameter parm;
                    foreach (SQLiteParameter parm in cmdParms)
                    {
                        cmd.Parameters.Add(parm);
                    }
                }
                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return val;
            }
            catch (SQLiteException ex)
            {
                throw new Exception("SQL Exception ", ex);
            }
            catch (Exception exx)
            {
                throw new Exception("PrepareCommand : ", exx);
            }

        }

        //	/// <summary>
        ///// Execute a OleDbCommand that returns a resultset against the database specified in the connection string 
        ///// using the provided parameters.
        ///// </summary>
        ///// <remarks>
        //'/// e.g.:  
        /////  SQLiteDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new OleDbParameter("@prodid", 24));
        ///// </remarks>
        ///// <param name="connectionString">a valid connection string for a OleDbConnection</param>
        ///// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        ///// <param name="commandText">the stored procedure name or T-SQL command</param>
        ///// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        ///// <returns>A SQLiteDataReader containing the results</returns>

        public SQLiteDataReader ExecuteReader(SQLiteConnection conn, CommandType cmdType, string cmdText, SQLiteParameter[] cmdParms = null)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            //Dim conn As SQLiteConnection = New SQLiteConnection(connString)
            try
            {
                cmd.CommandTimeout = 0;
                PrepareCommand(ref cmd, ref conn, ref cmdType, ref cmdText, ref cmdParms);
                SQLiteDataReader rdr = cmd.ExecuteReader();
                //cmd.Parameters.Clear()
                return rdr;
            }
            catch (SQLiteException ex)
            {
                conn.Close();
                throw new Exception(ex.Message, ex);
            }
            catch (Exception exx)
            {
                throw new Exception(exx.Message, exx);
            }
            finally
            {
                cmd = null;
            }
        }

        public DataTable ExecuteTable(string connString, CommandType cmdType, string cmdText, SQLiteParameter[] cmdParms = null)
        {

            SQLiteCommand cmd = new SQLiteCommand();
            SQLiteConnection conn = new SQLiteConnection(connString);
            SQLiteDataAdapter oDataAdapter = new SQLiteDataAdapter();
            DataTable oDataTable = new DataTable();
            try
            {
                PrepareCommand(ref cmd, ref conn, ref cmdType, ref cmdText, ref cmdParms);
                //cmd.Parameters = cmdParms
                oDataAdapter.SelectCommand = cmd;
                oDataAdapter.Fill(oDataTable);
                cmd.Parameters.Clear();
                return oDataTable;

            }
            catch (SQLiteException ex)
            {
                throw new Exception("SQL Exception : " + ex.Message, ex);
            }
            catch (Exception exx)
            {
                throw new Exception("ExecuteTable Exception :" + exx.Message, exx);
            }
            finally
            {
                conn.Close();
                cmd = null;
                oDataAdapter = null;
            }
        }

        public DataSet ExecuteDataSet(string connString, CommandType cmdType, string cmdText, SQLiteParameter[] cmdParms = null)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            SQLiteConnection conn = new SQLiteConnection(connString);

            SQLiteDataAdapter oDataAdapter = new SQLiteDataAdapter();
            DataSet oDataSet = new DataSet();
            try
            {
                PrepareCommand(ref cmd, ref conn, ref cmdType, ref cmdText, ref cmdParms);
                oDataAdapter.SelectCommand = cmd;
                //cmd.Connection = conn
                oDataAdapter.Fill(oDataSet);
                cmd.Parameters.Clear();
                return oDataSet;

            }
            catch (SQLiteException ex)
            {
                throw new Exception("SQL Exception ", ex);
            }
            catch (Exception exx)
            {
                throw new Exception("ExecuteDataSet", exx);
            }
            finally
            {
                conn.Close();
                cmd = null;
                oDataAdapter = null;
            }
        }

        public DataRow ExecuteRow(string connString, CommandType cmdType, string cmdText, SQLiteParameter[] cmdParms = null)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            SQLiteConnection conn = new SQLiteConnection(connString);
            SQLiteDataAdapter oDataAdapter = new SQLiteDataAdapter();
            DataTable oDataTable = new DataTable();
            try
            {
                PrepareCommand(ref cmd, ref conn, ref cmdType, ref cmdText, ref cmdParms);
                oDataAdapter.SelectCommand = cmd;
                oDataAdapter.Fill(oDataTable);
                cmd.Parameters.Clear();
                if (oDataTable.Rows.Count == 0)
                {
                    return null;
                }
                else
                {
                    DataRow oRow = oDataTable.Rows[0];
                    return oRow;
                }

            }
            catch (SQLiteException ex)
            {
                throw new Exception("SQL Exception ", ex);
            }
            catch (Exception exx)
            {
                throw new Exception("ExecuteRow", exx);
            }
            finally
            {
                conn.Close();
                oDataTable = null;
                cmd = null;
                oDataAdapter = null;
            }
        }

        ///// <summary>
        ///// Execute a OleDbCommand that returns the first column of the first record against the database specified in the connection string 
        ///// using the provided parameters.
        ///// </summary>
        ///// <remarks>
        ///// e.g.:  
        /////  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new OleDbParameter("@prodid", 24));
        ///// </remarks>
        ///// <param name="connectionString">a valid connection string for a OleDbConnection</param>
        ///// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        ///// <param name="commandText">the stored procedure name or T-SQL command</param>
        ///// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        ///// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>

        public object ExecuteScalar(string connString, CommandType cmdType, string cmdText, SQLiteParameter[] cmdParms = null)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            SQLiteConnection conn = new SQLiteConnection(connString);
            try
            {
                PrepareCommand(ref cmd, ref conn, ref cmdType, ref cmdText, ref cmdParms);
                object val = cmd.ExecuteScalar();

                cmd.Parameters.Clear();
                return val;
            }
            catch (SQLiteException ex)
            {
                throw new Exception(ex.Message, ex);
            }
            catch (Exception exx)
            {
                throw new Exception(exx.Message, exx);
            }
            finally
            {
                conn.Close();
                conn = null;
                cmd = null;
            }
        }

        ///// <summary>
        ///// add parameter array to the cache
        ///// </summary>
        ///// <param name="cacheKey">Key to the parameter cache</param>
        ///// <param name="cmdParms">an array of SqlParamters to be cached</param>

        //Public Function CacheParameters(ByVal cacheKey As String, ByVal cmdParms As OleDbParameter())
        //    parmCache(cacheKey) = cmdParms
        //End Function

        ///// <summary>
        ///// Retrieve cached parameters
        ///// </summary>
        ///// <param name="cacheKey">key used to lookup parameters</param>
        ///// <returns>Cached SqlParamters array</returns>

        //Public Function GetCachedParameters(ByVal cacheKey As String) As OleDbParameter()
        //    Dim cachedParms As OleDbParameter() = parmCache(cacheKey)
        //    If IsNothing(cachedParms) Then
        //        Return Nothing
        //    End If

        //    Dim clonedParms() As OleDbParameter = New OleDbParameter("abc", cachedParms.Length)
        //    Dim i As Integer
        //    Dim j As Integer = cachedParms.Length
        //    For i = 0 To j < 1
        //		clonedParms(i) = (OleDbParameter)((ICloneable)cachedParms(i)).Clone();
        //        Return clonedParms
        //    Next


        ///// <summary>
        ///// Prepare a command for execution
        ///// </summary>
        ///// <param name="cmd">OleDbCommand object</param>
        ///// <param name="conn">OleDbConnection object</param>
        ///// <param name="trans">SqlTransaction object</param>
        ///// <param name="cmdType">Cmd type e.g. stored procedure or text</param>
        ///// <param name="cmdText">Command text, e.g. Select * from Products</param>
        ///// <param name="cmdParms">OleDbParameters to use in the command</param>

        public bool PrepareCommand(ref SQLiteCommand cmd, ref SQLiteConnection conn, ref CommandType cmdType, ref string cmdText, ref SQLiteParameter[] cmdParms)
        {
            if (!(conn.State == ConnectionState.Open))
            {
                //MsgBox("Connection open")
                conn.Open();
            }
            try
            {
                cmd.Connection = conn;
                cmd.CommandText = cmdText;
                cmd.Parameters.Clear();
                //  cmd.ParameterCheck = True
                cmd.CommandType = cmdType;
                if (!((cmdParms == null)))
                {
                    //SQLiteParameter parm = default(SQLiteParameter);
                    foreach (SQLiteParameter parm in cmdParms)
                    {
                        cmd.Parameters.Add(parm);
                    }
                }
                return true;
            }
            catch (SQLiteException ex)
            {
                throw new Exception("SQL Exception ", ex);
            }
            catch (Exception exx)
            {
                throw new Exception("PrepareCommand : ", exx);
            }
        }

        public bool ExcuteAdapter(string connString, DataTable oTable, string cmdText, ref long lngMaxID)
        {

            SQLiteConnection conn = default(SQLiteConnection);
            SQLiteDataAdapter oDataAdapter = new SQLiteDataAdapter();
            SQLiteCommand oSqlCmd = new SQLiteCommand();
            SQLiteCommandBuilder oCmdBuilder = default(SQLiteCommandBuilder);
            try
            {
                conn = new SQLiteConnection(connString);
                if (!(conn.State == ConnectionState.Open))
                {
                    conn.Open();
                }

                oSqlCmd.Connection = conn;
                oSqlCmd.CommandText = cmdText;
                oSqlCmd.CommandType = CommandType.Text;
                oDataAdapter.SelectCommand = oSqlCmd;
                oCmdBuilder = new SQLiteCommandBuilder(oDataAdapter);
                oCmdBuilder.GetUpdateCommand();
                oCmdBuilder.GetInsertCommand();
                oCmdBuilder.GetDeleteCommand();
                oDataAdapter.Update(oTable);
                oDataAdapter.SelectCommand = new SQLiteCommand("SELECT @@IDENTITY", conn);
                lngMaxID = Convert.ToInt64(oDataAdapter.SelectCommand.ExecuteScalar());
                return true;
            }
            catch (SQLiteException ex)
            {
                throw new Exception("SQL Exception ", ex);
            }
            catch (Exception exx)
            {
                throw new Exception("ExeculateAdapter", exx);
            }
            finally
            {
                // cmd.Connection.Close()
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                oSqlCmd = null;
                oDataAdapter = null;
                oCmdBuilder = null;
            }
        }

        ///' <summary>
        ///' return table Schema 
        ///' </summary>
        ///' <param name="connString"></param>
        ///' <param name="cmdText"></param>
        ///' <param name="strTableName"></param>
        ///' <returns></returns>
        //Public Shared Function FillSchema(ByVal connString As String, ByVal cmdText As String, ByVal strTableName As String
    }
}
