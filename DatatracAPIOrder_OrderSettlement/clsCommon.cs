using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Security.Cryptography;

namespace DatatracAPIOrder_OrderSettlement
{
    class clsCommon
    {

        public static bool IsException = false;
        JWLCryptography.CommonCryptography Crypto = new JWLCryptography.CommonCryptography();
        public string GetConfigValue(string Key)
        {
            string retVal = "";
            //retVal = ConfigurationManager.AppSettings[Key];
            retVal = Crypto.Decrypt(ConfigurationManager.AppSettings[Key]);
            return retVal;
        }

        public struct ReturnResponse
        {
            public bool ResponseVal;
            public string Reason;

            public ReturnResponse(bool boolResponse = false)
            {
                this.ResponseVal = boolResponse;
                this.Reason = "Some Error";
            }
        }

        public struct DSResponse
        {
            public ReturnResponse dsResp;
            public DataSet DS;
        }

        private bool CreateLog(string myLogName)
        {
            bool Result = false;
            try
            {
                System.Diagnostics.EventLog.CreateEventSource(myLogName, myLogName);
                System.Diagnostics.EventLog myEventLog = new System.Diagnostics.EventLog();
                myEventLog.Source = myLogName;
                myEventLog.Log = myLogName;
                myEventLog.Source = myLogName;
                Result = true;
            }
            catch
            {
                Result = false;
            }
            return Result;
        }

        public void LogEvents(Exception Ex, string Source, System.Diagnostics.EventLogEntryType EventType, int EventId, short TaskId = 1, bool IsSendMail = false)
        {
            string Message = "";

            string AppName = GetConfigValue("ApplicationName");
            System.Diagnostics.EventLog myEventLog = new System.Diagnostics.EventLog();
            try
            {

                if (!System.Diagnostics.EventLog.SourceExists(GetConfigValue("ErrorEventLog")))
                    this.CreateLog(GetConfigValue("ErrorEventLog"));
                myEventLog.Log = GetConfigValue("ErrorEventLog");
                myEventLog.Source = Source;
                Message = "Time : " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt") + Environment.NewLine;
                Message = Message + "Message : " + Ex.Message.ToString() + Environment.NewLine;
                Message = Message + "StackTrace : " + Ex.StackTrace.ToString() + Environment.NewLine;
                if (!(Ex.InnerException == null))
                {
                    Message = Message + "InnerException : " + Ex.InnerException.Message.ToString() + Environment.NewLine;
                }
                foreach (System.Collections.DictionaryEntry data in Ex.Data)
                {
                    Message += "\n " + data.Key + " : " + data.Value;
                }
                myEventLog.WriteEntry(Message, EventType, EventId, TaskId);

            }
            catch (Exception LogEx)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", LogEx.Message + Environment.NewLine + "Actual Error : " + Ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            finally
            {
                myEventLog.Dispose();
                myEventLog = null;
            }
            if (IsSendMail)
            {
                SendExceptionMail("Error in" + AppName, Message);
            }
        }

        public void LogEvents(string strException, string Source, System.Diagnostics.EventLogEntryType EventType, int EventId, short TaskId = 1, bool IsSendMail = false)
        {
            string AppName = GetConfigValue("ApplicationName");
            System.Diagnostics.EventLog myEventLog = new System.Diagnostics.EventLog();
            string Message = "";
            try
            {
                if (!System.Diagnostics.EventLog.SourceExists(GetConfigValue("ErrorEventLog")))
                    this.CreateLog(GetConfigValue("ErrorEventLog"));
                myEventLog.Log = GetConfigValue("ErrorEventLog");
                myEventLog.Source = Source;
                Message = "Time : " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt") + Environment.NewLine;
                Message = Message + "Message : " + strException.ToString() + Environment.NewLine;
                myEventLog.WriteEntry(Message, EventType, EventId);
            }
            catch (Exception LogEx)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", LogEx.Message + Environment.NewLine + "Actual Error : " + strException.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            finally
            {
                myEventLog.Dispose();
                myEventLog = null;
            }

            if (IsSendMail)
            {
                SendExceptionMail("Error in" + AppName, Message);
            }
        }

        public bool SendExceptionMail(string Subject, string Body)
        {
            try
            {
                string fromMail = GetConfigValue("FromMailID");
                string fromPassword = GetConfigValue("FromMailPasssword");
                string Disclaimer = GetConfigValue("MailDisclaimer");
                string toMail = GetConfigValue("ToMailID");
                return SendMail(fromMail, fromPassword, Disclaimer, toMail, "", Subject, Body, "");
            }
            catch (Exception ex)
            {
                // LogEvents(ex, "SendExceptionMail", System.Diagnostics.EventLogEntryType.Error, 190);
                WriteErrorLog(ex);
                return false;
            }
        }

        public bool SendMail(string fromMail, string fromPassword, string Disclaimer, string toMail, string ccMail, string Subject, string Body, string AttachmentPath)
        {
            try
            {
                string AppName = GetConfigValue("ApplicationName");
                SmtpClient smtpClient = new SmtpClient(GetConfigValue("MailSMTPHost"), Convert.ToInt32(GetConfigValue("MailSMTPPort")));
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(fromMail, fromPassword);
                smtpClient.EnableSsl = true;

                MailAddress fromAddress = new MailAddress(fromMail);

                MailMessage mailMsg = new MailMessage();
                mailMsg.From = fromAddress;

                string[] toAddress;
                toAddress = toMail.Split(',');
                foreach (string strTo in toAddress)
                {
                    mailMsg.To.Add(strTo);
                }

                if (ccMail != "")
                {
                    string[] ccAddress;
                    ccAddress = ccMail.Split(',');
                    foreach (string strCc in ccAddress)
                    {
                        mailMsg.CC.Add(strCc);
                    }
                }

                mailMsg.Subject = Subject;

                Body = Body.Replace(System.Environment.NewLine, "<br/>");

                Body = Body + "<br/><br/>Regards,<br/>" + AppName + " <br/>Support Team<br/><br/>";

                if (Disclaimer.Trim() != "")
                {
                    Body = Body + "<br/><br/>" + Disclaimer;
                }

                mailMsg.Body = Body;
                mailMsg.IsBodyHtml = true;

                if (AttachmentPath.Trim() != "")
                {
                    Attachment att = new Attachment(AttachmentPath);
                    mailMsg.Attachments.Add(att);
                }

                smtpClient.Send(mailMsg);
                return true;
            }
            catch (Exception ex)
            {
                //LogEvents(ex, "SendMail", System.Diagnostics.EventLogEntryType.Error, 190);
                WriteErrorLog(ex);
                return false;
            }
        }

        public void WriteExecutionLog(string strExecutionLogFilePath, string strExecutionLogMessage)
        {
            StreamWriter outFile = null;
            try
            {
                string AppName = GetConfigValue("ApplicationName");

                if (!System.IO.Directory.Exists(strExecutionLogFilePath + @"\"))
                    System.IO.Directory.CreateDirectory(strExecutionLogFilePath + @"\");

                //    string filepath = strExecutionLogFilePath + @"\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";

                string filepath = strExecutionLogFilePath + @"\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                //    string filepath = strExecutionLogFilePath + @"\" + DateTime.Now.ToString("yyyyMMdd") + ".csv";

                string Message = "Date/Time: " + DateTime.Now.ToString() + " " + strExecutionLogMessage + System.Environment.NewLine;

                //if (!File.Exists(filepath))
                //    File.Create(filepath).Dispose();

                //using (StreamWriter writetext = new StreamWriter(filepath,true))
                //{
                //    writetext.WriteLine(Message);
                //}

                //var file = @"C:\myOutput.csv";

                //using (var stream = File.CreateText(filepath))
                //{
                //    //for (int i = 0; i < reader.Count(); i++)
                //    //{
                //    //    string first = reader[i].ToString();
                //    //    string second = image.ToString();
                //    //    string csvRow = string.Format("{0},{1}", first, second);

                //    //    stream.WriteLine(csvRow);
                //    //}

                //    string first = Message;
                //    string second = "";
                //    string csvRow = string.Format("{0},{1}\n", first, second);
                //    stream.WriteLine(csvRow);

                //}

                //Task av = WriteFileAsync(strExecutionLogFilePath, strExecutionLogMessage);



                //File.AppendAllText(filepath, Message);


                //using (StreamWriter writer = new StreamWriter(filepath, true))
                //{
                //    writer.WriteLine(Message);
                //}


                //using (var stream = File.Open(filepath, FileMode.Open))
                //{
                //    using (StreamWriter sw = new StreamWriter(stream))
                //    {
                //        sw.WriteLine(Message);
                //    }
                //    // Use stream
                //}

                //FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                //using (StreamReader sr = new StreamReader(fs))
                //{
                //    using (StreamWriter sw = new StreamWriter(filepath))
                //    {
                //        sw.WriteLine(Message);
                //    }
                //}

                if (!File.Exists(filepath))
                {
                    // Create a file to write to.   
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        sw.WriteLine(Message);
                        sw.Flush();
                        sw.Close();

                    }

                }
                else
                {
                    using (StreamWriter sw = File.AppendText(filepath))
                    {
                        sw.WriteLine(Message);
                        sw.Flush();
                        sw.Close();
                    }
                }

                //// Exiting code commented as it is writing the events in same file
                //FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                //StreamWriter s = new StreamWriter(fs);
                //s.Close();
                //fs.Close();

                //// log it
                //FileStream fs1 = new FileStream(filepath, FileMode.Append, FileAccess.Write);
                //StreamWriter s1 = new StreamWriter(fs1);

                //// s1.Write("================================================" & vbCrLf)
                //s1.Write("Date/Time: " + DateTime.Now.ToString() + " " + strExecutionLogMessage + System.Environment.NewLine);
                //// s1.Write("Message: " & ex.Message & vbCrLf)
                //// s1.Write("Message: " & ex.StackTrace & vbCrLf)
                //// s1.Write("================================================" & vbCrLf)
                //s1.Close();
                //fs1.Close();


            }
            catch (Exception ex)
            {
                throw new Exception("Error in WriteExecutionLog -->" + ex.Message + ex.StackTrace);
            }
            finally
            {

            }
        }


        public void WriteExecutionLogParallelly(string fileName, string strExecutionLogMessage)
        {
            StreamWriter outFile = null;
            try
            {
                string strExecutionLogFilePath = GetConfigValue("ExecutionLogFileLocation");

                if (!System.IO.Directory.Exists(strExecutionLogFilePath + @"\"))
                    System.IO.Directory.CreateDirectory(strExecutionLogFilePath + @"\");
                string filepath = strExecutionLogFilePath + @"\" + fileName + ".txt";
                string Message = "Date/Time: " + DateTime.Now.ToString() + " " + strExecutionLogMessage + System.Environment.NewLine;

                if (!File.Exists(filepath))
                {
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        sw.WriteLine(Message);
                        sw.Flush();
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(filepath))
                    {
                        sw.WriteLine(Message);
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in WriteExecutionLogParallelly -->" + ex.Message + ex.StackTrace);
            }
        }


        public void WriteErrorLogParallelly(Exception ex, string fileName, string strExecutionLogMessage = null)
        {

            IsException = true;
            string strExecutionLogFilePath = GetConfigValue("ErrorLogFileLocation");

            if (!System.IO.Directory.Exists(strExecutionLogFilePath + @"\"))
                System.IO.Directory.CreateDirectory(strExecutionLogFilePath + @"\");
            string filepath = strExecutionLogFilePath + @"\" + fileName + ".txt";
            string Message = "Date/Time: " + DateTime.Now.ToString() + " " + strExecutionLogMessage + System.Environment.NewLine;

            string ExeMessage = "================================================" + System.Environment.NewLine;
            ExeMessage += "Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine;
            ExeMessage += "Message: " + ex.Message + System.Environment.NewLine;
            ExeMessage += "Message: " + ex.StackTrace + System.Environment.NewLine;
            if (strExecutionLogMessage != null)
            {
                ExeMessage += "ExecutionLogMessage: " + strExecutionLogMessage + System.Environment.NewLine;
            }
            ExeMessage += "================================================" + System.Environment.NewLine;
            if (!File.Exists(filepath))
            {
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(ExeMessage);
                    sw.Flush();
                    sw.Close();
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(ExeMessage);
                    sw.Flush();
                    sw.Close();
                }
            }

        }
        public static async Task WriteFileAsync(string strExecutionLogFilePath, string strExecutionLogMessage)
        {
            //  Console.WriteLine("Async Write File has started.");
            //  string dir = 
            //   string file = ,
            try
            {


                if (!System.IO.Directory.Exists(strExecutionLogFilePath + @"\"))
                    System.IO.Directory.CreateDirectory(strExecutionLogFilePath + @"\");

                //    string filepath = strExecutionLogFilePath + @"\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";

                string filepath = strExecutionLogFilePath + @"\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                string dir = strExecutionLogFilePath;
                string file = DateTime.Now.ToString("yyyyMMdd") + ".txt";
                string Message = "Date/Time: " + DateTime.Now.ToString() + " " + strExecutionLogMessage + System.Environment.NewLine;

                if (!File.Exists(filepath))
                    File.Create(filepath).Dispose();


                using (StreamWriter outputFile = new StreamWriter(Path.Combine(dir, file)))
                {
                    await outputFile.WriteAsync(Message);
                }
                // Console.WriteLine("Async Write File has completed.");
            }

            catch (Exception ex)
            {
                throw new Exception("Error in WriteFileAsync -->" + ex.Message + ex.StackTrace);
            }
        }

        public void WriteErrorLog(Exception ex, string strExecutionLogMessage = null)
        {

            IsException = true;
            string strErrorLogPath;

            strErrorLogPath = GetConfigValue("ErrorLogFileLocation");

            if (!System.IO.Directory.Exists(strErrorLogPath))
                System.IO.Directory.CreateDirectory(strErrorLogPath);

            string filepath = strErrorLogPath + @"\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            string ExeMessage = "================================================" + System.Environment.NewLine;
            ExeMessage += "Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine;
            ExeMessage += "Message: " + ex.Message + System.Environment.NewLine;
            ExeMessage += "Message: " + ex.StackTrace + System.Environment.NewLine;
            if (strExecutionLogMessage != null)
            {
                ExeMessage += "ExecutionLogMessage: " + strExecutionLogMessage + System.Environment.NewLine;
            }
            ExeMessage += "================================================" + System.Environment.NewLine;
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {

                    sw.WriteLine(ExeMessage);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(ExeMessage);
                }
            }
            // Exiting code commented as it is writing the events in same file
            //FileStream fs = new FileStream(strErrorLogPath + @"\ErrorLog.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //StreamWriter s = new StreamWriter(fs);
            //s.Close();
            //fs.Close();

            //// log it
            //FileStream fs1 = new FileStream(strErrorLogPath + @"\ErrorLog.txt", FileMode.Append, FileAccess.Write);
            //StreamWriter s1 = new StreamWriter(fs1);

            //s1.Write("================================================" + System.Environment.NewLine);
            //s1.Write("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine);
            //s1.Write("Message: " + ex.Message + System.Environment.NewLine);
            //s1.Write("Message: " + ex.StackTrace + System.Environment.NewLine);
            //if (strExecutionLogMessage != null)
            //{
            //    s1.Write("ExecutionLogMessage: " + strExecutionLogMessage + System.Environment.NewLine);
            //}
            //s1.Write("================================================" + System.Environment.NewLine);
            //s1.Close();
            //fs1.Close();
        }
        public void WriteToFile(string Message)
        {
            string path = GetConfigValue("LogfilePath");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = path + "Log_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
        public void MoveTheFileToHistoryFolder(string strFolderToCopyTheFilesTo, FileInfo workFile)
        {
            try
            {
                if (Directory.Exists(strFolderToCopyTheFilesTo + @"\"))
                {
                    if (File.Exists(strFolderToCopyTheFilesTo + @"\" + workFile.Name))
                    {
                        string fileName = workFile.Name;
                        int fileExtPos = fileName.LastIndexOf(".");
                        if (fileExtPos >= 0)
                            fileName = fileName.Substring(0, fileExtPos);
                        fileName = strFolderToCopyTheFilesTo + @"\" + fileName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                        workFile.MoveTo(fileName); ;
                    }
                    else
                    {
                        workFile.MoveTo(strFolderToCopyTheFilesTo + @"\" + workFile.Name);
                    }
                }
                else
                {
                    Directory.CreateDirectory(strFolderToCopyTheFilesTo + @"\");
                    workFile.MoveTo(strFolderToCopyTheFilesTo + @"\" + workFile.Name);
                }
            }
            catch (Exception ex)
            {
                // MsgBox(ex.Message.ToString)

                string strExecutionLogMessage = "Exception in MoveTheFileToHistoryFolder" + System.Environment.NewLine;
                WriteErrorLog(ex, strExecutionLogMessage);

                //LogEvents(ex, "MoveTheFileToHistoryFolder", System.Diagnostics.EventLogEntryType.Error, 190);
                //EmailTheTechTeamAboutTheError("File Move Error in BillingLocations Application", ex.Message + ex.StackTrace, strFolderToCopyTheFilesTo, workFile.Name);
                //throw new Exception("Error in MoveTheFileToHistoryFolder -->" + ex.Message + ex.StackTrace);
            }
        }

        public void EmailTheTechTeamAboutTheError_(string strEmailSubject, string strErrorMessageInfo, string strFolderToCopyTheFilesTo, string strFileName)
        {
            try
            {
                string strExecutionLogMessage;
                string strExecutionLogFileLocation;
                strExecutionLogFileLocation = GetConfigValue("ExecutionLogFileLocation");
                SendExceptionMail(strEmailSubject, "Error Occurred while moving the file " + strFileName + " to: " + strFolderToCopyTheFilesTo + " Error Code is: " + strErrorMessageInfo);
                strExecutionLogMessage = "Emailed the Tech Team about File Move Error.  File Name: " + strFileName + " located at: " + strFolderToCopyTheFilesTo;
                WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                //LogEvents(ex, "EmailTheTechTeamAboutTheError", System.Diagnostics.EventLogEntryType.Error, 100);
                // throw new Exception("Error in EmailTheTechTeamAboutTheError -->" + ex.Message + ex.StackTrace);
            }
        }

        public DataSet jsonToDataSet(string jsonString, string type = null)
        {
            DataSet ds = new DataSet();
            try
            {
                XmlDocument xd = new XmlDocument();
                jsonString = "{ \"rootNode\": {" + jsonString.Trim().TrimStart('{').TrimEnd('}') + "} }";
                xd = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonString);

                ds.ReadXml(new XmlNodeReader(xd));

                if (type == "OrderPost")
                {

                    var UniqueId = ds.Tables[0].TableName;

                    ds.Tables[0].TableName = "id";

                    ds.Tables[0].Columns.Add("Id", typeof(System.String));

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        //need to set value to NewColumn column
                        row["Id"] = UniqueId;   // or set it to some other value
                    }


                    for (int f = ds.Tables["Id"].ChildRelations.Count - 1; f >= 0; f--)
                    {
                        ds.Tables["Id"].ChildRelations[f].ChildTable.Constraints.Remove(ds.Tables["Id"].ChildRelations[f].RelationName);
                        ds.Tables["Id"].ChildRelations.RemoveAt(f);
                    }
                    ds.Tables["Id"].ChildRelations.Clear();
                    ds.Tables["Id"].ParentRelations.Clear();
                    ds.Tables["Id"].Constraints.Clear();


                    string columntoremove = UniqueId + "_Id";

                    if (ds.Tables.Contains("settlements"))
                    {
                        ds.Tables["settlements"].Columns.Remove(columntoremove);
                    }

                    if (ds.Tables.Contains("progress"))
                    {
                        var myTable = ds.Tables["progress"];

                        ds.Tables["progress"].Columns.Remove(ds.Tables["progress"].Columns[columntoremove]);


                        ds.Tables["progress"].Columns.Add("Id", typeof(System.String));
                        foreach (DataRow row in ds.Tables["progress"].Rows)
                        {
                            //need to set value to NewColumn column
                            row["Id"] = UniqueId;   // or set it to some other value
                        }
                    }
                }
                else if (type == "RouteStopPostAPI")
                {
                    var UniqueId = ds.Tables[0].TableName;

                    if (ds.Tables.Contains("progress"))
                    {
                        var myTable = ds.Tables["progress"];

                        //  ds.Tables["progress"].Columns.Remove(ds.Tables["progress"].Columns[columntoremove]);


                        ds.Tables["progress"].Columns.Add("id", typeof(System.String));
                        foreach (DataRow row in ds.Tables["progress"].Rows)
                        {
                            //need to set value to NewColumn column
                            row["id"] = UniqueId;   // or set it to some other value
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                //  LogEvents(ex, "jsonToDataSet", System.Diagnostics.EventLogEntryType.Error, 101);
                // throw new ArgumentException(ex.Message);

            }
            return ds;
        }

        public System.Data.DataTable JsonStringToDataTable(string jsonString)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            string[] jsonStringArray = Regex.Split(jsonString.Replace("[", "").Replace("]", ""), "},{");
            List<string> ColumnsName = new List<string>();
            foreach (string jSA in jsonStringArray)
            {
                string[] jsonStringData = Regex.Split(jSA.Replace("{", "").Replace("}", ""), ",");
                foreach (string ColumnsNameData in jsonStringData)
                {
                    try
                    {
                        int idx = ColumnsNameData.IndexOf(":");
                        string ColumnsNameString = ColumnsNameData.Substring(0, idx - 1).Replace("\"", "");
                        if (!ColumnsName.Contains(ColumnsNameString))
                        {
                            ColumnsName.Add(ColumnsNameString);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        throw new Exception(string.Format("Error Parsing Column Name : {0}", ColumnsNameData));
                    }
                }
                break;
            }
            foreach (string AddColumnName in ColumnsName)
            {
                dt.Columns.Add(AddColumnName);
            }
            foreach (string jSA in jsonStringArray)
            {
                string[] RowData = Regex.Split(jSA.Replace("{", "").Replace("}", ""), ",");
                DataRow nr = dt.NewRow();
                foreach (string rowData in RowData)
                {
                    try
                    {
                        int idx = rowData.IndexOf(":");
                        string RowColumns = rowData.Substring(0, idx - 1).Replace("\"", "");
                        string RowDataString = rowData.Substring(idx + 1).Replace("\"", "");
                        nr[RowColumns] = RowDataString;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        continue;
                    }
                }
                dt.Rows.Add(nr);
            }
            return dt;
        }

        public void WriteDatatracResponseToOutputFile(DataSet ds, string strInputFilePath, string referenceNumber, string type, string fileName)
        {
            var UniqueIdandRef = ds.Tables[0].TableName + "-" + referenceNumber;
            try
            {
                string strOutputFileLocation;
                string strOutputFile;
                string strExecutionLogFileLocation;

                strExecutionLogFileLocation = GetConfigValue("ExecutionLogFileLocation");
                strOutputFileLocation = strInputFilePath + @"\Outputs";

                if (!System.IO.Directory.Exists(strOutputFileLocation + @"\"))
                    System.IO.Directory.CreateDirectory(strOutputFileLocation + @"\");


                int fileExtPos = fileName.LastIndexOf(".");
                if (fileExtPos >= 0)
                    fileName = fileName.Substring(0, fileExtPos);

                if (type == "S")
                {
                    strOutputFile = fileName + "-Success-" + DateTime.Now.ToString("ddMMyyyy");// + ".xlsx";
                }
                else
                {
                    strOutputFile = fileName + "-Failure-" + DateTime.Now.ToString("ddMMyyyy");// + ".xlsx";
                }

                strOutputFile = strOutputFileLocation + @"\" + strOutputFile + ".xlsx"; // ".csv";

                if (File.Exists(strOutputFile))
                {
                    ExportDataSetToExcel(ds, strOutputFile);
                }
                else
                {
                    Microsoft.Office.Interop.Excel.ApplicationClass ExcelApp = new Microsoft.Office.Interop.Excel.ApplicationClass();
                    Workbook xlWorkbook = ExcelApp.Workbooks.Add(Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet);
                    // Loop over DataTables in DataSet.
                    DataTableCollection collection = ds.Tables;
                    for (int i = collection.Count; i > 0; i--)
                    {
                        Sheets xlSheets = null;
                        Worksheet xlWorksheet = null;

                        //Create Excel Sheets

                        xlSheets = ExcelApp.Sheets;
                        xlWorksheet = (Worksheet)xlSheets.Add(xlSheets[1],
                                       Type.Missing, Type.Missing, Type.Missing);

                        System.Data.DataTable table = collection[i - 1];
                        xlWorksheet.Name = table.TableName;
                        for (int j = 1; j < table.Columns.Count + 1; j++)
                        {
                            ExcelApp.Cells[1, j] = table.Columns[j - 1].ColumnName;
                        }

                        // Storing Each row and column value to excel sheet

                        for (int k = 0; k < table.Rows.Count; k++)
                        {
                            for (int l = 0; l < table.Columns.Count; l++)
                            {
                                ExcelApp.Cells[k + 2, l + 1] =
                                table.Rows[k].ItemArray[l].ToString();
                            }
                        }
                        ExcelApp.Columns.AutoFit();
                    }

                    ExcelApp.ActiveWorkbook.SaveAs(strOutputFile, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing,
                    false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                    ExcelApp.ActiveWorkbook.Close();

                    //((Worksheet)ExcelApp.ActiveWorkbook.Sheets[ExcelApp.ActiveWorkbook.Sheets.Count]).Delete();
                    //ExcelApp.Visible = true;
                    // strExecutionLogMessage = "Wrote the Billing file at " + strOutputFile;
                    // WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                }


            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "Exception in WriteDatatracResponseToOutputFile" + System.Environment.NewLine;
                WriteErrorLog(ex, strExecutionLogMessage);

                //LogEvents(ex, "WriteDatatracResponseToOutputFile", System.Diagnostics.EventLogEntryType.Error, 101);
                //EmailTheTechTeamAboutTheError("Error while trying to Write Datatrac response into Output File Path, -" + strInputFilePath + ",FileName -" + fileName + ",UniqueIdandRef-" + UniqueIdandRef, ex.Message + ex.StackTrace, "", "");
                //throw new Exception("Error in WriteOutputFileWithAddressInfo -->" + ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// This method takes DataSet as input paramenter and it exports the same to excel
        /// </summary>
        /// <param name="ds"></param>
        private void ExportDataSetToExcel(DataSet ds, string filepath)
        {
            try
            {
                //Creae an Excel application instance
                Excel.Application excelApp = new Excel.Application();

                //Create an Excel workbook instance and open it from the predefined location
                // Excel.Workbook excelWorkBook = excelApp.Workbooks.Open(@"C:\DatatracAPIAutomation\RIC\Order\Add\Output\MXD Billing Template WE 07.03.21 updated3-Success-12072021.xlsx");
                Excel.Workbook excelWorkBook = excelApp.Workbooks.Open(filepath);

                foreach (System.Data.DataTable table in ds.Tables)
                {
                    //Add a new worksheet to workbook with the Datatable name
                    // Excel.Worksheet excelWorkSheet = (Worksheet)excelWorkBook.Sheets.Add();




                    // Keeping track
                    bool found = false;
                    // Loop through all worksheets in the workbook
                    foreach (Excel.Worksheet sheet in excelWorkBook.Sheets)
                    {
                        // Check the name of the current sheet
                        if (sheet.Name == table.TableName)
                        {
                            found = true;
                            break; // Exit the loop now
                        }
                    }

                    if (found)
                    {
                        // Reference it by name
                        //Worksheet mySheet = wb.Sheets["Example"];
                        Excel.Worksheet excelWorkSheet = (Worksheet)excelWorkBook;
                        //excelWorkSheet.Name = table.TableName;
                        //(Worksheet)excelWorkBook.Name= table.TableName;
                        Worksheet xlWorksheet = null;

                        //for (int i = 1; i < table.Columns.Count + 1; i++)
                        //{
                        //    excelWorkSheet.Cells[1, i] = table.Columns[i - 1].ColumnName;
                        //}
                        for (int j = 0; j < table.Rows.Count; j++)
                        {
                            for (int k = 0; k < table.Columns.Count; k++)
                            {
                                excelWorkSheet.Cells[j + 2, k + 1] = table.Rows[j].ItemArray[k].ToString();

                                // excelApp.Cells[j + 2, k + 1] = table.Rows[j].ItemArray[k].ToString();
                            }
                        }
                        // excelApp.Columns.AutoFit();
                    }
                    else
                    {
                        // Create it
                    }


                }

                excelWorkBook.Save();
                excelWorkBook.Close();
                excelApp.Quit();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                //  LogEvents(ex, "ExportDataSetToExcel", System.Diagnostics.EventLogEntryType.Error, 101);
                // EmailTheTechTeamAboutTheError("Error while trying to Write Datatrac response into Output File Path, -" + strInputFilePath + ",FileName -" + fileName + ",UniqueIdandRef-" + UniqueIdandRef, ex.Message + ex.StackTrace, "", "");
                //throw new Exception("Error in ExportDataSetToExcel -->" + ex.Message + ex.StackTrace);
            }
        }

        public void ExportDataTableToXLSX(System.Data.DataTable dt, string strInputFilePath, string fileName)
        {
            clsCommon objCommon = new clsCommon();
            try
            {
                string strFilePath;

                if (!System.IO.Directory.Exists(strInputFilePath + @"\"))
                    System.IO.Directory.CreateDirectory(strInputFilePath + @"\");

                //int fileExtPos = fileName.LastIndexOf(".");
                //if (fileExtPos >= 0)
                //    fileName = fileName.Substring(0, fileExtPos);


                strFilePath = strInputFilePath + @"\" + fileName + ".xlsx"; // ".csv";

                Application oXL;
                Workbook oWB;
                Worksheet oSheet;
                Range oRange;

                try
                {
                    // Start Excel and get Application object. 
                    oXL = new Microsoft.Office.Interop.Excel.Application();

                    // Set some properties 
                    oXL.Visible = false;
                    oXL.DisplayAlerts = false;

                    // Get a new workbook. 
                    oWB = oXL.Workbooks.Add(Type.Missing);

                    // Get the Active sheet 
                    oSheet = (Microsoft.Office.Interop.Excel.Worksheet)oWB.ActiveSheet;
                    oSheet.Name = dt.TableName;

                    //  sda.Fill(dt);
                    //    System.Data.DataTable dt = ds.Tables[0];
                    int rowCount = 1;
                    foreach (DataRow dr in dt.Rows)
                    {
                        rowCount += 1;
                        for (int i = 1; i < dt.Columns.Count + 1; i++)
                        {
                            // Add the header the first time through 
                            if (rowCount == 2)
                            {
                                oSheet.Cells[1, i] = dt.Columns[i - 1].ColumnName;
                            }
                            oSheet.Cells[rowCount, i] = dr[i - 1].ToString();
                        }
                    }

                    // Resize the columns 
                    // Range c1 = oSheet.Cells[1, 1];
                    // Range c2 = oSheet.Cells[rowCount, dt.Columns.Count];
                    //  oRange = oSheet.get_Range(c1, c2);

                    oRange = oSheet.get_Range(oSheet.Cells[1, 1],
                             oSheet.Cells[rowCount, dt.Columns.Count]);

                    oRange.EntireColumn.AutoFit();

                    // Save the sheet and close 
                    oSheet = null;
                    oRange = null;

                    oWB.SaveAs(strFilePath, XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing,
    false, false, XlSaveAsAccessMode.xlNoChange,
    Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                    oWB.Close(Type.Missing, Type.Missing, Type.Missing);
                    oWB = null;
                    oXL.Quit();
                }
                catch (Exception ex)
                {
                    objCommon.WriteErrorLog(ex, "ExportDataTableToXLSX");
                    throw;
                }
                finally
                {
                    // Clean up 
                    // NOTE: When in release mode, this does the trick 
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }

            }
            catch (Exception ex)
            {
                objCommon.WriteErrorLog(ex, "ExportDataTableToXLSX");
            }
        }
        public void WriteDataToCsvFileParallely(System.Data.DataTable dataTable, string strInputFilePath, string fileName, string Datetime)
        {
            try
            {
                string strOutputFileLocation;
                string strOutputFile;

                strOutputFileLocation = GetConfigValue("SplittedOutputFilesWorkingFolder");

                if (!System.IO.Directory.Exists(strOutputFileLocation + @"\"))
                    System.IO.Directory.CreateDirectory(strOutputFileLocation + @"\");


                //int fileExtPos = fileName.LastIndexOf(".");
                //if (fileExtPos >= 0)
                //    fileName = fileName.Substring(0, fileExtPos);


                strOutputFile = fileName + "-" + dataTable.TableName + "-" + Datetime;// + ".xlsx";
                strOutputFile = strOutputFileLocation + @"\" + strOutputFile + ".csv"; // ".csv";

                StringBuilder fileContent = new StringBuilder();
                StringBuilder HeaderContent = new StringBuilder();

                if (!File.Exists(strOutputFile))
                {
                    foreach (var col in dataTable.Columns)
                    {
                        HeaderContent.Append(col.ToString() + ",");
                    }
                    HeaderContent.Replace(",", System.Environment.NewLine, HeaderContent.Length - 1, 1);
                    File.WriteAllText(strOutputFile, HeaderContent.ToString());
                }

                foreach (DataRow dr in dataTable.Rows)
                {
                    foreach (var column in dr.ItemArray)
                    {
                        fileContent.Append("\"" + column.ToString() + "\",");
                    }

                    fileContent.Replace(",", System.Environment.NewLine, fileContent.Length - 1, 1);

                }
                File.AppendAllText(strOutputFile, fileContent.ToString());
            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "Exception in WriteDataToCsvFile" + System.Environment.NewLine;
                WriteErrorLog(ex, strExecutionLogMessage);
            }
        }

        public void SaveOutputDataToCsvFileParallely<T>(List<T> reportData, string strDataRelatedName, string fileName, string Datetime)
        {
            string strOutputFileLocation;
            string strOutputFile;
            try
            {
                strOutputFileLocation = GetConfigValue("SplittedOutputFilesWorkingFolder");

                if (!System.IO.Directory.Exists(strOutputFileLocation + @"\"))
                    System.IO.Directory.CreateDirectory(strOutputFileLocation + @"\");


                //int fileExtPos = fileName.LastIndexOf(".");
                //if (fileExtPos >= 0)
                //    fileName = fileName.Substring(0, fileExtPos);


                strOutputFile = fileName + "-" + strDataRelatedName + "-" + Datetime;// + ".xlsx";
                strOutputFile = strOutputFileLocation + @"\" + strOutputFile + ".csv"; // ".csv";

                var lines = new List<string>();
                IEnumerable<PropertyDescriptor> props = TypeDescriptor.GetProperties(typeof(T)).OfType<PropertyDescriptor>();

                var header = string.Join(",", props.ToList().Select(x => x.Name));
                if (!File.Exists(strOutputFile))
                {
                    lines.Add(header);
                }
                var valueLines = reportData.Select(row => string.Join(",", header.Split(',').Select(a => row.GetType().GetProperty(a).GetValue(row, null))));
                lines.AddRange(valueLines);
                //File.WriteAllLines(path, lines.ToArray());

                File.AppendAllLines(strOutputFile, lines.ToArray());

            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "Exception in SaveOutputDataToCsvFileParallely" + System.Environment.NewLine;
                WriteErrorLog(ex, strExecutionLogMessage);
            }
        }


        public void WriteDataToCsvFile(System.Data.DataTable dataTable, string strInputFilePath, string referenceNumber, string fileName, string Datetime)
        {
            try
            {

                string strOutputFileLocation;
                string strOutputFile;

                strOutputFileLocation = GetConfigValue("OutputFilesWorkingFolder");

                if (!System.IO.Directory.Exists(strOutputFileLocation + @"\"))
                    System.IO.Directory.CreateDirectory(strOutputFileLocation + @"\");


                int fileExtPos = fileName.LastIndexOf(".");
                if (fileExtPos >= 0)
                    fileName = fileName.Substring(0, fileExtPos);


                strOutputFile = fileName + "-" + dataTable.TableName + "-" + Datetime;// + ".xlsx";
                strOutputFile = strOutputFileLocation + @"\" + strOutputFile + ".csv"; // ".csv";

                StringBuilder fileContent = new StringBuilder();
                StringBuilder HeaderContent = new StringBuilder();

                if (!File.Exists(strOutputFile))
                {
                    foreach (var col in dataTable.Columns)
                    {
                        HeaderContent.Append(col.ToString() + ",");
                    }
                    HeaderContent.Replace(",", System.Environment.NewLine, HeaderContent.Length - 1, 1);
                    File.WriteAllText(strOutputFile, HeaderContent.ToString());
                }

                foreach (DataRow dr in dataTable.Rows)
                {
                    foreach (var column in dr.ItemArray)
                    {
                        fileContent.Append("\"" + column.ToString() + "\",");
                    }

                    fileContent.Replace(",", System.Environment.NewLine, fileContent.Length - 1, 1);

                }
                File.AppendAllText(strOutputFile, fileContent.ToString());
            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "Exception in WriteDataToCsvFile" + System.Environment.NewLine;
                WriteErrorLog(ex, strExecutionLogMessage);

            }
        }

        public void SaveOutputDataToCsvFile<T>(List<T> reportData, string strName, string strInputFilePath, string referenceNumber, string fileName, string Datetime)
        {
            string strOutputFileLocation;
            string strOutputFile;
            try
            {

                strOutputFileLocation = GetConfigValue("OutputFilesWorkingFolder");

                if (!System.IO.Directory.Exists(strOutputFileLocation + @"\"))
                    System.IO.Directory.CreateDirectory(strOutputFileLocation + @"\");


                int fileExtPos = fileName.LastIndexOf(".");
                if (fileExtPos >= 0)
                    fileName = fileName.Substring(0, fileExtPos);


                strOutputFile = fileName + "-" + strName + "-" + Datetime;// + ".xlsx";
                strOutputFile = strOutputFileLocation + @"\" + strOutputFile + ".csv"; // ".csv";

                var lines = new List<string>();
                IEnumerable<PropertyDescriptor> props = TypeDescriptor.GetProperties(typeof(T)).OfType<PropertyDescriptor>();

                var header = string.Join(",", props.ToList().Select(x => x.Name));
                if (!File.Exists(strOutputFile))
                {
                    lines.Add(header);
                }
                var valueLines = reportData.Select(row => string.Join(",", header.Split(',').Select(a => row.GetType().GetProperty(a).GetValue(row, null))));
                lines.AddRange(valueLines);
                //File.WriteAllLines(path, lines.ToArray());

                File.AppendAllLines(strOutputFile, lines.ToArray());

            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "Exception in SaveOutputDataToCsvFile" + System.Environment.NewLine;
                WriteErrorLog(ex, strExecutionLogMessage);

            }
        }

        public void MoveOutputFilesToOutputLocation(string strInputFilePath)
        {
            try
            {
                string sourcePath = GetConfigValue("OutputFilesWorkingFolder");
                string destinationPath = strInputFilePath + @"\Outputs";

                if (!System.IO.Directory.Exists(destinationPath + @"\"))
                    System.IO.Directory.CreateDirectory(destinationPath + @"\");

                foreach (string sourceFile in Directory.GetFiles(sourcePath, "*.csv"))
                {
                    string fileName = Path.GetFileName(sourceFile);
                    string destinationFile = Path.Combine(destinationPath, fileName);
                    File.Move(sourceFile, destinationFile);
                }
            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "Exception in MoveOutputFilesToOutputLocation" + System.Environment.NewLine;
                WriteErrorLog(ex, strExecutionLogMessage);
            }
        }


        public void CleanSplittedOutputFilesWorkingFolder()
        {
            try
            {
                string strIsDelete = GetConfigValue("DeleteWorkingSplittedfiles");
                string sourcePath = GetConfigValue("SplittedOutputFilesWorkingFolder");

                if (strIsDelete == "Y")
                {
                    DirectoryInfo di = new DirectoryInfo(sourcePath);

                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                }
                else
                {
                    string destinationPath = GetConfigValue("SplittedOutputfilesHistory");

                    if (!Directory.Exists(destinationPath + @"\"))
                        Directory.CreateDirectory(destinationPath + @"\");

                    foreach (string sourceFile in Directory.GetFiles(sourcePath, "*.csv"))
                    {
                        string fileName = Path.GetFileName(sourceFile);
                        string destinationFile = Path.Combine(destinationPath, fileName);
                        File.Move(sourceFile, destinationFile);
                    }
                }
            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "Exception in CleanSplittedOutputFilesWorkingFolder" + System.Environment.NewLine;
                WriteErrorLog(ex, strExecutionLogMessage);
            }
        }
        public void MoveMergedOutputFilesToOutputLocation(string strInputFilePath)
        {
            try
            {

                string sourcePath = GetConfigValue("MergedOutputFilesWorkingFolder");
                //   string destinationPath = @"C:\Users\Chris\Documents\Excel\";

                string destinationPath = strInputFilePath + @"\Outputs";

                if (!System.IO.Directory.Exists(destinationPath + @"\"))
                    System.IO.Directory.CreateDirectory(destinationPath + @"\");

                foreach (string sourceFile in Directory.GetFiles(sourcePath, "*.csv"))
                {
                    string fileName = Path.GetFileName(sourceFile);
                    string destinationFile = Path.Combine(destinationPath, fileName);
                    File.Move(sourceFile, destinationFile);
                }
            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "Exception in MoveOutputFilesToOutputLocation" + System.Environment.NewLine;
                WriteErrorLog(ex, strExecutionLogMessage);
            }
        }


        public static List<System.Data.DataTable> SplitTable(System.Data.DataTable originalTable, int batchSize, string fileName, string strDatetime)
        {
            List<System.Data.DataTable> tables = new List<System.Data.DataTable>();
            int i = 0;
            int j = 1;

            int fileExtPos = fileName.LastIndexOf(".");
            if (fileExtPos >= 0)
                fileName = fileName.Substring(0, fileExtPos);

            System.Data.DataTable newDt = originalTable.Clone();
            //  newDt.TableName = "Table_" + j;
            newDt.TableName = fileName + "_" + j + "_" + strDatetime;
            newDt.Clear();
            foreach (DataRow row in originalTable.Rows)
            {
                DataRow newRow = newDt.NewRow();
                newRow.ItemArray = row.ItemArray;
                newDt.Rows.Add(newRow);
                i++;
                if (i == batchSize)
                {
                    tables.Add(newDt);
                    j++;
                    newDt = originalTable.Clone();
                    // newDt.TableName = "Table_" + j;
                    newDt.TableName = fileName + "_" + j + "_" + strDatetime;
                    newDt.Clear();
                    i = 0;
                }

            }
            if (newDt.Rows.Count > 0)
            {
                tables.Add(newDt);
                j++;
                newDt = originalTable.Clone();
                //newDt.TableName = "Table_" + j;
                newDt.TableName = fileName + "_" + j + "_" + strDatetime;
                newDt.Clear();

            }
            return tables;
        }


        public void MergeSplittedOutputFiles(string fileName, string filetype, string strDatetime)
        {
            try
            {

                string sourcePath = GetConfigValue("SplittedOutputFilesWorkingFolder");
                string strOutputFileLocation;
                string strOutputFile;
                strOutputFileLocation = GetConfigValue("MergedOutputFilesWorkingFolder");

                if (!System.IO.Directory.Exists(strOutputFileLocation + @"\"))
                    System.IO.Directory.CreateDirectory(strOutputFileLocation + @"\");


                int fileExtPos = fileName.LastIndexOf(".");
                if (fileExtPos >= 0)
                    fileName = fileName.Substring(0, fileExtPos);

                strOutputFile = fileName + "-" + filetype + "-" + strDatetime;// + ".xlsx";
                strOutputFile = strOutputFileLocation + @"\" + strOutputFile + ".csv"; // ".csv";


                var files = Directory.GetFiles(sourcePath, "*.csv", SearchOption.AllDirectories)
                                        .Select(x => new FileInfo(x))
                                        .Where(x => x.Name.Contains(filetype))
                                        // .Take(1)
                                        .ToList();

                if (files.Count > 0)
                {
                    StreamWriter fileDest = new StreamWriter(strOutputFile, true);

                    int i;
                    for (i = 0; i < files.Count; i++)
                    {
                        string file = files[i].FullName;

                        string[] lines = File.ReadAllLines(file);

                        if (i > 0)
                        {
                            lines = lines.Skip(1).ToArray(); // Skip header row for all but first file
                        }
                        foreach (string line in lines)
                        {
                            fileDest.WriteLine(line);
                        }
                    }

                    fileDest.Close();
                }
            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "Exception in MergeSplittedOutputFiles" + System.Environment.NewLine;
                WriteErrorLog(ex, strExecutionLogMessage);
            }
        }

        public string GeneareteUnigueId()
        {
            var bytes = new byte[4];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            uint random = BitConverter.ToUInt32(bytes, 0) % 100000000;
            return String.Format("{0:D8}", random); //return 8 digit unique id 
        }

        
    }

}
