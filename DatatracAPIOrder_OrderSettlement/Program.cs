using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DatatracAPIOrder_OrderSettlement
{
    public static class Program
    {
        static void Main(string[] args)
        {
            RunService();
            clsCommon objCommon = new clsCommon();
            string AppName = objCommon.GetConfigValue("ApplicationName");
            if (clsCommon.IsException)
            {
                string strEmailSubject = "Got Exception while running " + AppName + " on " + DateTime.Now.ToString("yyyyMMdd");
                string strEmailBody = strEmailSubject + System.Environment.NewLine + "Requesting you to please go and check error log file for :" + DateTime.Now.ToString("yyyyMMdd");
                objCommon.SendExceptionMail(strEmailSubject, strEmailBody);
            }

        }
        private static void RunService()
        {
            clsCommon objCommon = new clsCommon();
            string AppName = objCommon.GetConfigValue("ApplicationName");
            var msg = "Exception in RunService";
            try
            {
                string[] subDirectories;
                string strInputFilePath;
                string strExecutionLogMessage;
                string strExecutionLogFileLocation;
                DirectoryInfo dir;
                FileInfo[] XLSfiles;
                strExecutionLogFileLocation = objCommon.GetConfigValue("ExecutionLogFileLocation");

                strExecutionLogMessage = "Beginning the new instance for " + AppName + " processing ";

                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                strInputFilePath = objCommon.GetConfigValue("AutomationFileLocation");
                subDirectories = Directory.GetDirectories(strInputFilePath, "*", SearchOption.TopDirectoryOnly);
                foreach (var subDirectory in subDirectories)
                {
                    try
                    {
                        string Location = Right(subDirectory, (subDirectory.Length - (strInputFilePath.Length + 1)));

                        string SubdirLocation = strInputFilePath + @"\" + Location;

                        subDirectories = Directory.GetDirectories(SubdirLocation, "*", SearchOption.TopDirectoryOnly);
                        foreach (var subDirectoryLocation in subDirectories)
                        {
                            //break;
                            string subDirFolder = Right(subDirectoryLocation, (subDirectoryLocation.Length - (SubdirLocation.Length + 1)));
                            if (subDirFolder.ToUpper() == "ORDER")
                            {

                                string OrderfilePath = strInputFilePath + @"\" + Location + @"\" + subDirFolder;
                                subDirectories = Directory.GetDirectories(OrderfilePath, "*", SearchOption.TopDirectoryOnly);
                                foreach (var subDirectoryOrder in subDirectories)
                                {

                                    string OrderSubdir = Right(subDirectoryOrder, (subDirectoryOrder.Length - (OrderfilePath.Length + 1)));
                                    if (OrderSubdir.ToUpper() == "ADD")
                                    {

                                        string OrderAddfilePath = OrderfilePath + @"\" + OrderSubdir;
                                        dir = new DirectoryInfo(OrderAddfilePath);
                                        XLSfiles = dir.GetFiles("*.xlsx");
                                        if (XLSfiles.Count() > 0)
                                        {
                                            ProcessAddOrderFiles(OrderAddfilePath, Location);
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "No Excel Files found for this location: " + Location + ", for Path is :" + OrderAddfilePath;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                        }
                                    }
                                    else if (OrderSubdir.ToUpper() == "MODIFY")
                                    {

                                        string OrderUpdatefilePath = OrderfilePath + @"\" + OrderSubdir;
                                        dir = new DirectoryInfo(OrderUpdatefilePath);
                                        XLSfiles = dir.GetFiles("*.xlsx");
                                        if (XLSfiles.Count() > 0)
                                        {
                                            ProcessUpdateOrderFiles(OrderUpdatefilePath, Location);
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "No Excel Files found for this location: " + Location + ", Path is :" + OrderUpdatefilePath;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                        }
                                    }
                                }
                            }
                            else if (subDirFolder.ToUpper() == "ORDERSETTLEMENT")
                            {

                                string OrderSettlememtfilePath = strInputFilePath + @"\" + Location + @"\" + subDirFolder;
                                subDirectories = Directory.GetDirectories(OrderSettlememtfilePath, "*", SearchOption.TopDirectoryOnly);
                                foreach (var subDirectoryOrder in subDirectories)
                                {

                                    string OrderSubdir = Right(subDirectoryOrder, (subDirectoryOrder.Length - (OrderSettlememtfilePath.Length + 1)));
                                    if (OrderSubdir.ToUpper() == "MODIFY")
                                    {
                                        string OrderSettlememtUpdatefilePath = OrderSettlememtfilePath + @"\" + OrderSubdir;
                                        dir = new DirectoryInfo(OrderSettlememtUpdatefilePath);
                                        XLSfiles = dir.GetFiles("*.xlsx");
                                        if (XLSfiles.Count() > 0)
                                        {
                                            ProcessUpdateOrderSettlementFiles(OrderSettlememtUpdatefilePath, Location);
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "No Excel Files found for this location: " + Location + ", Path is :" + OrderSettlememtUpdatefilePath;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                        }
                                    }
                                }
                            }
                            else if (subDirFolder.ToUpper() == "ROUTEHEADER")
                            {

                                string OrderfilePath = strInputFilePath + @"\" + Location + @"\" + subDirFolder;
                                subDirectories = Directory.GetDirectories(OrderfilePath, "*", SearchOption.TopDirectoryOnly);
                                foreach (var subDirectoryOrder in subDirectories)
                                {

                                    string OrderSubdir = Right(subDirectoryOrder, (subDirectoryOrder.Length - (OrderfilePath.Length + 1)));
                                    if (OrderSubdir.ToUpper() == "ADD")
                                    {
                                        string OrderAddfilePath = OrderfilePath + @"\" + OrderSubdir;
                                        dir = new DirectoryInfo(OrderAddfilePath);
                                        XLSfiles = dir.GetFiles("*.xlsx");
                                        if (XLSfiles.Count() > 0)
                                        {

                                            // ProcessAddRouteHeaderFiles(OrderAddfilePath, Location);
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "No Excel Files found for this location: " + Location + ", Path is :" + OrderAddfilePath;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                        }
                                    }

                                }
                            }
                            else if (subDirFolder.ToUpper() == "ROUTESTOP")
                            {

                                string filePath = strInputFilePath + @"\" + Location + @"\" + subDirFolder;
                                subDirectories = Directory.GetDirectories(filePath, "*", SearchOption.TopDirectoryOnly);
                                foreach (var subDirectoryOrder in subDirectories)
                                {
                                    // break;
                                    string Subdir = Right(subDirectoryOrder, (subDirectoryOrder.Length - (filePath.Length + 1)));
                                    if (Subdir.ToUpper() == "ADD")
                                    {
                                        string AddfilePath = filePath + @"\" + Subdir;
                                        dir = new DirectoryInfo(AddfilePath);
                                        XLSfiles = dir.GetFiles("*.xlsx");
                                        if (XLSfiles.Count() > 0)
                                        {
                                            ProcessAddRouteStopFiles(AddfilePath, Location);
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "No Excel Files found for this location: " + Location + ", Path is :" + AddfilePath;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                        }
                                    }
                                    else if (Subdir.ToUpper() == "MODIFY")
                                    {
                                        string UpdatefilePath = filePath + @"\" + Subdir;
                                        dir = new DirectoryInfo(UpdatefilePath);
                                        XLSfiles = dir.GetFiles("*.xlsx");
                                        if (XLSfiles.Count() > 0)
                                        {
                                            ProcessUpdateRouteStopFiles(UpdatefilePath, Location);
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "No Excel Files found for this location: " + Location + ", Path is :" + UpdatefilePath;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                        }
                                    }

                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        objCommon.WriteErrorLog(ex);
                    }
                }

                strExecutionLogMessage = "Finished processing all the files for all locations ";
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

            }
            catch (Exception ex)
            {
                objCommon.WriteErrorLog(ex, AppName + msg.ToString());
            }
            finally
            {

            }
        }
        public static string Right(this string str, int length)
        {
            return str.Substring(str.Length - length, length);
        }
        private static void ProcessAddOrderFiles(string strInputFilePath, string strLocationFolder)
        {
            clsCommon objCommon = new clsCommon();

            try
            {
                System.Configuration.AppSettingsReader reader = new System.Configuration.AppSettingsReader();
                DirectoryInfo dir;
                FileInfo[] XLSfiles;
                string strFileName;
                //  var strFileName = "";
                // string strInputFilePath;
                string strBillingHistoryFileLocation;
                string strExecutionLogMessage;
                string strExecutionLogFileLocation;
                string strDatetime;
                //   DataTable dtDataTable;
                //  string strSheetName;
                var ReferenceId = "";
                strExecutionLogFileLocation = objCommon.GetConfigValue("ExecutionLogFileLocation");

                strExecutionLogMessage = "Processing the Add Order data for " + strInputFilePath;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                //strInputFilePath = objCommon.GetConfigValue("AutomationFileLocation") + @"\Order\Add";
                // strInputFilePath = strInputFilePath + @"\" + strLocationFolder;
                strBillingHistoryFileLocation = strInputFilePath + @"\HistoricalFiles";

                strExecutionLogMessage = "The input file Path is: " + strInputFilePath + "." + System.Environment.NewLine + "The Historical File Path is:" + strBillingHistoryFileLocation;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                dir = new DirectoryInfo(strInputFilePath);
                XLSfiles = dir.GetFiles("*.xlsx");

                strExecutionLogMessage = "Found # of Excel Files: " + XLSfiles.Count();
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                foreach (var file in XLSfiles)
                {
                    strFileName = file.ToString();
                    // dtDataTable = new System.Data.DataTable();

                    try
                    {

                        DataSet dsExcel = new DataSet();
                        dsExcel = clsExcelHelper.ImportExcelXLSX(strInputFilePath + @"\" + strFileName, false);
                        if (dsExcel.Tables.Count > 0)
                        {

                            //strExecutionLogMessage = "Processing File Move to History  : " + strFileName + "." + System.Environment.NewLine;
                            //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                            objCommon.MoveTheFileToHistoryFolder(strBillingHistoryFileLocation, file);
                            //strExecutionLogMessage = "Processing File Moved to History completed  : " + strFileName + "." + System.Environment.NewLine;
                            //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                            strDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");
                            for (int i = dsExcel.Tables[0].Rows.Count - 1; i >= 0; i--)
                            {
                                DataRow dr = dsExcel.Tables[0].Rows[i];
                                if (dr["Company"] == DBNull.Value)
                                    dr.Delete();
                            }
                            dsExcel.Tables[0].AcceptChanges();
                            // For BBB Customer
                            string CustomerName = "";
                            try
                            {
                                CustomerName = strFileName.Split('-')[0].ToUpper();
                            }
                            catch (Exception e)
                            {
                                CustomerName = "";
                            }
                            finally
                            {

                            }

                            int noofrowspertable = Convert.ToInt16(objCommon.GetConfigValue("DevideToProcessParallelly"));

                            if (CustomerName == "BBB")
                            {
                                noofrowspertable = 0;
                            }

                            List<DataTable> splitdt = clsCommon.SplitTable(dsExcel.Tables[0], noofrowspertable, strFileName, strDatetime);

                            strExecutionLogMessage = "Parallelly Processing Started for the  file : " + strFileName + "." + System.Environment.NewLine + "Number of processess are going to exicute is :" + noofrowspertable;
                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                            objCommon.CleanSplittedOutputFilesWorkingFolder();

                            Parallel.ForEach(splitdt, currentDatatable =>
                            {
                                var fileName = currentDatatable.TableName;
                                var processingFileName = currentDatatable.TableName;
                                strExecutionLogMessage = "Current Processing File is  : " + fileName + "." + System.Environment.NewLine;
                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                DataTable datatable; //  = currentDatatable;

                                if (CustomerName == "BBB")
                                {
                                    datatable = RemoveDuplicateRows(dsExcel.Tables[0], "Customer Reference");
                                }
                                else
                                {
                                    datatable = currentDatatable;
                                }

                                foreach (DataRow dr in datatable.Rows)
                                {
                                    ReferenceId = Convert.ToString(dr["Customer Reference"]);
                                    strExecutionLogMessage = "Customer Reference is : " + ReferenceId + "." + System.Environment.NewLine;
                                    //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                    objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                    try
                                    {
                                        orderdetails objorderdetails = new orderdetails();
                                        order objOrder = new order();
                                        List<order_line_item> objorder_line_itemList = new List<order_line_item>();
                                        if (CustomerName == "BBB")
                                        {
                                            DataTable dtBBB = currentDatatable.Select("[Customer Reference]= '" + dr["Customer Reference"] + "'").CopyToDataTable();

                                            DataView view = new DataView(dtBBB);
                                            DataTable dtdistinctDeliveryDate = view.ToTable(true, "Delivery Date");
                                            //for (int i = dtdistinctDeldate.Rows.Count - 1; i >= 0; i--)
                                            //{
                                            //    DataRow dr1 = dtdistinctDeldate.Rows[i];
                                            //    if (dr1["Delivery Date"] == DBNull.Value)
                                            //        dr1.Delete();
                                            //}
                                            //dtdistinctDeldate.AcceptChanges();

                                            foreach (DataRow drow1 in dtdistinctDeliveryDate.Rows)
                                            {
                                                dtBBB = currentDatatable.Select("[Customer Reference]= '" + dr["Customer Reference"] + "' AND [Delivery Date]= '" + drow1["Delivery Date"] + "'").CopyToDataTable();

                                                noofrowspertable = Convert.ToInt16(objCommon.GetConfigValue("Maxlimit_order_line_item"));// 999;
                                                List<DataTable> splitdtBBB = clsCommon.SplitTable(dtBBB, noofrowspertable, strFileName, strDatetime);

                                                foreach (DataTable curDatatable in splitdtBBB)
                                                {
                                                    DataTable datatable1 = new DataTable();
                                                    var firstRow = curDatatable.AsEnumerable().First();
                                                    datatable1 = new[] { firstRow }.CopyToDataTable();

                                                    objorderdetails = new orderdetails();
                                                    objOrder = new order();
                                                    objorder_line_itemList = new List<order_line_item>();

                                                    foreach (DataRow drow in datatable1.Rows)
                                                    {
                                                        ReferenceId = Convert.ToString(drow["Customer Reference"]);
                                                        DataRow[] drItemresult = curDatatable.Select("[Customer Reference]= '" + drow["Customer Reference"] + "'");
                                                        foreach (DataRow drItems in drItemresult)
                                                        {
                                                            order_line_item objitems = new order_line_item();
                                                            if (drItems.Table.Columns.Contains("Item Number"))
                                                            {
                                                                objitems.item_number = Convert.ToString(drItems["Item Number"]);
                                                            }
                                                            if (drItems.Table.Columns.Contains("Item Description"))
                                                            {
                                                                objitems.item_description = Convert.ToString(drItems["Item Description"]);
                                                            }
                                                            if (drItems.Table.Columns.Contains("Dim Height"))
                                                            {
                                                                objitems.dim_height = Convert.ToInt32(Convert.ToDouble(drItems["Dim Height"]));
                                                            }
                                                            if (drItems.Table.Columns.Contains("Dim Length"))
                                                            {
                                                                objitems.dim_length = Convert.ToInt32(Convert.ToDouble(drItems["Dim Length"]));
                                                            }
                                                            if (drItems.Table.Columns.Contains("Dim Width"))
                                                            {
                                                                objitems.dim_width = Convert.ToInt32(Convert.ToDouble(drItems["Dim Width"]));
                                                            }
                                                            objorder_line_itemList.Add(objitems);
                                                        }
                                                        objOrder.number_of_pieces = Convert.ToInt32(drItemresult.Length);

                                                        objOrder.line_items = objorder_line_itemList;

                                                        objOrder.company_number = Convert.ToInt32(drow["Company"]);
                                                        objOrder.service_level = Convert.ToInt32(drow["Service Type"]);
                                                        objOrder.customer_number = Convert.ToInt32(drow["Billing Customer Number"]);
                                                        objOrder.reference = Convert.ToString(drow["Customer Reference"]);
                                                        //  DateTime dtValue = Convert.ToDateTime(dr["Delivery Date"]);

                                                        if (drow.Table.Columns.Contains("BOL Number"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["BOL Number"])))
                                                            {
                                                                objOrder.bol_number = Convert.ToString(drow["BOL Number"]);
                                                            }
                                                        }

                                                        if (drow.Table.Columns.Contains("Delivery Date"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Delivery Date"])))
                                                            {
                                                                DateTime dtValue = Convert.ToDateTime(drow["Delivery Date"]);
                                                                objOrder.pickup_requested_date = dtValue.ToString("yyyy-MM-dd");
                                                                objOrder.pickup_actual_date = dtValue.ToString("yyyy-MM-dd");
                                                                objOrder.deliver_requested_date = dtValue.ToString("yyyy-MM-dd");
                                                                objOrder.deliver_actual_date = dtValue.ToString("yyyy-MM-dd");
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Pickup actual arrival time"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Pickup actual arrival time"])))
                                                            {
                                                                DateTime dtValue = Convert.ToDateTime(drow["Pickup actual arrival time"]);
                                                                objOrder.pickup_actual_arr_time = dtValue.ToString("HH:mm");
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Pickup actual depart time"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Pickup actual depart time"])))
                                                            {
                                                                DateTime dtValue = Convert.ToDateTime(drow["Pickup actual depart time"]);
                                                                objOrder.pickup_actual_dep_time = dtValue.ToString("HH:mm");
                                                            }
                                                        }
                                                        //    dtValue = Convert.ToDateTime(dr["Pickup actual depart time"]);
                                                        //  objOrder.pickup_actual_dep_time = dtValue.ToString("HH:mm");
                                                        if (drow.Table.Columns.Contains("Pickup no later than"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Pickup no later than"])))
                                                            {
                                                                DateTime dtValue = Convert.ToDateTime(drow["Pickup no later than"]);
                                                                objOrder.pickup_requested_dep_time = dtValue.ToString("HH:mm");
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Pickup name"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Pickup name"])))
                                                            {
                                                                objOrder.pickup_name = Convert.ToString(drow["Pickup name"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Pickup address"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Pickup address"])))
                                                            {
                                                                objOrder.pickup_address = Convert.ToString(drow["Pickup address"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Pickup city"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Pickup city"])))
                                                            {
                                                                objOrder.pickup_city = Convert.ToString(drow["Pickup city"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Pickup state/province"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Pickup state/province"])))
                                                            {
                                                                objOrder.pickup_state = Convert.ToString(drow["Pickup state/province"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Pickup zip/postal code"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Pickup zip/postal code"])))
                                                            {
                                                                // objOrder.pickup_zip = Convert.ToString(dr["Pickup zip/postal code"]);

                                                                string strZip = Convert.ToString(drow["Pickup zip/postal code"]);
                                                                strZip = Regex.Replace(strZip, @"\t", "");
                                                                if (strZip.Length > 5)
                                                                {
                                                                    objOrder.pickup_zip = strZip.Substring(0, 5) + "-" + strZip.Substring(5, strZip.Length - 5); ;
                                                                }
                                                                else
                                                                {
                                                                    objOrder.pickup_zip = Convert.ToString(drow["Pickup zip/postal code"]);
                                                                }
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Deliver no earlier than"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Deliver no earlier than"])))
                                                            {
                                                                DateTime dtValue = Convert.ToDateTime(drow["Deliver no earlier than"]);
                                                                objOrder.deliver_requested_arr_time = dtValue.ToString("HH:mm");
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Deliver no later than"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Deliver no later than"])))
                                                            {
                                                                DateTime dtValue = Convert.ToDateTime(drow["Deliver no later than"]);
                                                                objOrder.deliver_requested_dep_time = dtValue.ToString("HH:mm");
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Delivery actual arrive time"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Delivery actual arrive time"])))
                                                            {
                                                                DateTime dtValue = Convert.ToDateTime(drow["Delivery actual arrive time"]);
                                                                objOrder.deliver_actual_arr_time = dtValue.ToString("HH:mm");
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Delivery actual depart time"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Delivery actual depart time"])))
                                                            {
                                                                DateTime dtValue = Convert.ToDateTime(drow["Delivery actual depart time"]);
                                                                objOrder.deliver_actual_dep_time = dtValue.ToString("HH:mm");
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Customer Name"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Customer Name"])))
                                                            {
                                                                objOrder.deliver_name = Convert.ToString(drow["Customer Name"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Address"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Address"])))
                                                            {
                                                                objOrder.deliver_address = Convert.ToString(drow["Address"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("City"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["City"])))
                                                            {
                                                                objOrder.deliver_city = Convert.ToString(drow["City"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("State"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["State"])))
                                                            {
                                                                objOrder.deliver_state = Convert.ToString(drow["State"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Zip"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Zip"])))
                                                            {
                                                                string strZip = Convert.ToString(drow["Zip"]);
                                                                strZip = Regex.Replace(strZip, @"\t", "");
                                                                if (strZip.Length > 5)
                                                                {
                                                                    objOrder.deliver_zip = strZip.Substring(0, 5) + "-" + strZip.Substring(5, strZip.Length - 5); ;
                                                                }
                                                                else
                                                                {
                                                                    objOrder.deliver_zip = Convert.ToString(drow["Zip"]);
                                                                }
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Delivery text signature"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Delivery text signature"])))
                                                            {
                                                                objOrder.signature = Convert.ToString(drow["Delivery text signature"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Bill Rate"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Bill Rate"])))
                                                            {
                                                                objOrder.rate_buck_amt1 = Convert.ToDouble(drow["Bill Rate"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Pieces ACC"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Pieces ACC"])))
                                                            {
                                                                objOrder.rate_buck_amt3 = Convert.ToDouble(drow["Pieces ACC"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("FSC"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["FSC"])))
                                                            {
                                                                objOrder.rate_buck_amt10 = Convert.ToDouble(drow["FSC"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Pieces"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Pieces"])))
                                                            {
                                                                objOrder.number_of_pieces = Convert.ToInt32(drow["Pieces"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Miles"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Miles"])))
                                                            {
                                                                objOrder.rate_miles = Convert.ToInt32(Convert.ToDouble(drow["Miles"]));
                                                            }
                                                        }
                                                        //    string driver1 = null;
                                                        if (drow.Table.Columns.Contains("Correct Driver Number"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Correct Driver Number"])))
                                                            {
                                                                objOrder.driver1 = Convert.ToInt32(drow["Correct Driver Number"]);
                                                                //driver1 = Convert.ToString(dr["Correct Driver Number"]);
                                                            }

                                                        }
                                                        //if (dr.Table.Columns.Contains("Requested by"))
                                                        //{
                                                        //    if (!string.IsNullOrEmpty(Convert.ToString(dr["Requested by"])))
                                                        //    {
                                                        //        objOrder.ordered_by = Convert.ToString(dr["Requested by"]);
                                                        //    }
                                                        //}

                                                        objOrder.ordered_by = Convert.ToString(drow["Requested by"]);
                                                        objOrder.csr = Convert.ToString(drow["Entered by"]);
                                                        if (drow.Table.Columns.Contains("Pickup Delivery Transfer Flag"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Pickup Delivery Transfer Flag"])))
                                                            {
                                                                objOrder.pick_del_trans_flag = Convert.ToString(drow["Pickup Delivery Transfer Flag"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Pickup text signature"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Pickup text signature"])))
                                                            {
                                                                objOrder.pickup_signature = Convert.ToString(drow["Pickup text signature"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Weight"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Weight"])))
                                                            {
                                                                objOrder.weight = Convert.ToInt32(Convert.ToDouble(drow["Weight"]));
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Insurance Amount"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Insurance Amount"])))
                                                            {
                                                                objOrder.insurance_amount = Convert.ToInt32(Convert.ToDouble(drow["Insurance Amount"]));
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Master airway bill number"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Master airway bill number"])))
                                                            {
                                                                objOrder.master_airway_bill_number = Convert.ToString(drow["Master airway bill number"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("PO Number"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["PO Number"])))
                                                            {
                                                                objOrder.po_number = Convert.ToString(drow["PO Number"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("House airway bill number"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["House airway bill number"])))
                                                            {
                                                                objOrder.house_airway_bill_number = Convert.ToString(drow["House airway bill number"]);
                                                            }
                                                        }

                                                        if (drow.Table.Columns.Contains("Delivery Phone"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Delivery Phone"])))
                                                            {
                                                                objOrder.deliver_phone = Convert.ToString(drow["Delivery Phone"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Pickup Room"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Pickup Room"])))
                                                            {
                                                                objOrder.pickup_room = Convert.ToString(drow["Pickup Room"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Pickup Attention"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Pickup Attention"])))
                                                            {
                                                                objOrder.pickup_attention = Convert.ToString(drow["Pickup Attention"]);
                                                            }
                                                        }
                                                        if (drow.Table.Columns.Contains("Deliver Attention"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Deliver Attention"])))
                                                            {
                                                                objOrder.deliver_attention = Convert.ToString(drow["Deliver Attention"]);
                                                            }
                                                        }
                                                        objorderdetails.order = objOrder;
                                                        clsDatatrac objclsDatatrac = new clsDatatrac();
                                                        clsCommon.ReturnResponse objresponse = new clsCommon.ReturnResponse();
                                                        string request = JsonConvert.SerializeObject(objorderdetails);
                                                        string data = Regex.Replace(request, @"\\t", "");
                                                        request = Regex.Replace(data, @"\\""", "");
                                                        objresponse = objclsDatatrac.CallDataTracOrderPostAPI(objorderdetails);
                                                        //objresponse.ResponseVal = true;
                                                        //objresponse.Reason = "{\"002018775030\": {\"verified_weight\": null, \"roundtrip_actual_latitude\": null, \"pickup_special_instructions4\": null, \"fuel_miles\": null, \"pickup_dispatch_zone\": null, \"pickup_zip\": \"23150\", \"pickup_actual_arr_time\": \"08:00\", \"cod_accept_company_check\": false, \"add_charge_occur9\": null, \"pickup_omw_latitude\": null, \"service_level_text\": \"Threshold Delivery\", \"service_level\": 57, \"exception_sign_required\": false, \"pickup_phone_ext\": null, \"roundtrip_actual_pieces\": null, \"bringg_send_sms\": false, \"az_equip2\": null, \"hist_inv_date\": null, \"date_order_entered\": \"2021-07-21\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"pickup_city\": \"SANDSTON\", \"pickup_phone\": null, \"pickup_sign_req\": true, \"deliver_route_sequence\": null, \"deliver_phone\": null, \"deliver_omw_longitude\": null, \"roundtrip_actual_longitude\": null, \"page_number\": 1, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"add_charge_code9\": null, \"pickup_eta_time\": null, \"record_type\": 0, \"add_charge_occur11\": null, \"push_partner_order_id\": null, \"deliver_country\": null, \"customer_name\": \"MXD/RYDER\", \"bol_number\": null, \"pickup_latitude\": 37.53250820, \"add_charge_code4\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"pu_arrive_notification_sent\": false, \"distribution_shift_id\": null, \"pickup_special_instr_long\": null, \"id\": \"002018775030\", \"callback_to\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"ordered_by\": \"RYDER\", \"add_charge_code12\": null, \"pickup_route_seq\": null, \"deliver_city\": \"HANOVER\", \"add_charge_occur5\": null, \"edi_acknowledgement_required\": false, \"rescheduled_ctrl_number\": null, \"driver2\": null, \"deliver_room\": null, \"deliver_actual_arr_time\": \"08:00\", \"fuel_price_zone\": null, \"add_charge_amt9\": null, \"add_charge_amt4\": null, \"delivery_address_point_number_text\": \"PAMELA YORK\", \"delivery_address_point_number\": 24254, \"deliver_actual_longitude\": null, \"add_charge_amt2\": null, \"additional_drivers\": false, \"pickup_pricing_zone\": 1, \"hazmat\": false, \"pickup_address\": \"540 EASTPARK CT\", \"pickup_route_code\": null, \"callback_userid\": null, \"pickup_point_customer\": 31025, \"rate_buck_amt1\": 52.00, \"add_charge_amt8\": null, \"callback_time\": null, \"csr\": \"DX*\", \"roundtrip_actual_depart_time\": null, \"customers_etrac_partner_id\": \"96609250\", \"manual_notepad\": false, \"add_charge_code8\": null, \"bringg_order_id\": null, \"deliver_omw_latitude\": null, \"pickup_longitude\": -77.33035820, \"etrac_number\": null, \"distribution_unique_id\": 0, \"vehicle_type\": null, \"roundtrip_actual_arrival_time\": null, \"delivery_longitude\": -77.28769600, \"pu_actual_location_accuracy\": null, \"deliver_actual_date\": \"2021-07-14\", \"exception_timestamp\": null, \"deliver_zip\": \"23069\", \"roundtrip_wait_time\": null, \"add_charge_occur8\": null, \"dl_arrive_notification_sent\": false, \"pickup_special_instructions1\": null, \"ordered_by_phone_number\": null, \"deliver_requested_arr_time\": \"08:00\", \"rate_miles\": null, \"holiday_groups\": null, \"pickup_email_notification_sent\": false, \"add_charge_code3\": null, \"dispatch_id\": null, \"add_charge_occur10\": null, \"dispatch_time\": null, \"deliver_wait_time\": null, \"invoice_period_end_date\": null, \"add_charge_occur12\": null, \"fuel_plan\": null, \"return_svc_level\": null, \"pickup_actual_date\": \"2021-07-14\", \"send_new_order_alert\": false, \"pickup_room\": null, \"rate_buck_amt8\": null, \"add_charge_amt10\": null, \"insurance_amount\": null, \"add_charge_amt3\": null, \"add_charge_amt6\": null, \"pickup_special_instructions3\": null, \"pickup_requested_date\": \"2021-07-14\", \"roundtrip_sign_req\": false, \"actual_miles\": null, \"pickup_address_point_number_text\": \"1STOPBEDROOMS\", \"pickup_address_point_number\": 19845, \"deliver_actual_latitude\": null, \"deliver_phone_ext\": null, \"deliver_route_code\": null, \"add_charge_code10\": null, \"delivery_airport_code\": null, \"reference\": null, \"photos_exist\": false, \"master_airway_bill_number\": null, \"control_number\": 1877503, \"cod_text\": \"No\", \"cod\": \"N\", \"rate_buck_amt11\": null, \"cod_amount\": null, \"pickup_omw_timestamp\": null, \"signature_images\": [], \"deliver_special_instructions1\": null, \"quote_amount\": null, \"total_pages\": 1, \"rate_buck_amt4\": null, \"line_items\": [], \"delivery_latitude\": 37.80844290, \"progress\": [{\"status_time\": \"15:57:00\", \"status_date\": \"2021-07-21\", \"status_text\": \"Entered in carrier's system\"}, {\"status_time\": \"08:30:00\", \"status_date\": \"2021-07-14\", \"status_text\": \"Picked up\"}, {\"status_time\": \"08:15:00\", \"status_date\": \"2021-07-14\", \"status_text\": \"Delivered\"}], \"add_charge_code1\": null, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"deliver_special_instr_long\": null, \"deliver_address\": \"205 HARDWOOD LN\", \"add_charge_occur4\": null, \"deliver_eta_date\": null, \"pickup_actual_dep_time\": \"08:30\", \"deliver_requested_dep_time\": \"17:00\", \"add_charge_code7\": null, \"deliver_actual_dep_time\": \"08:15\", \"cod_accept_cashiers_check\": false, \"bringg_last_loc_sent\": null, \"az_equip3\": null, \"driver1_text\": \"WALT DARBY\", \"driver1\": 3215, \"pickup_actual_latitude\": null, \"add_charge_occur2\": null, \"order_automatically_quoted\": false, \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"hours\": \"15\", \"frequent_caller_id\": null, \"rate_buck_amt6\": null, \"rate_chart_used\": 0, \"deliver_actual_pieces\": null, \"add_charge_code5\": null, \"pickup_omw_longitude\": null, \"notes\": [], \"delivery_point_customer\": 31025, \"add_charge_occur7\": null, \"rate_buck_amt5\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"add_charge_code11\": null, \"pickup_name\": \"1STOPBEDROOMS\", \"callback_date\": null, \"add_charge_code2\": null, \"house_airway_bill_number\": null, \"deliver_name\": \"PAMELA YORK\", \"number_of_pieces\": 1, \"deliver_eta_time\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"rate_special_instructions\": null, \"add_charge_occur3\": null, \"pickup_eta_date\": null, \"deliver_special_instructions4\": null, \"custom_special_instr_long\": null, \"deliver_special_instructions2\": null, \"pickup_signature\": \"SOF\", \"az_equip1\": null, \"add_charge_amt12\": null, \"settlements\": [{\"vendor_invoice_number\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"voucher_date\": null, \"voucher_amount\": null, \"fuel_price_source\": null, \"settlement_bucket6_pct\": null, \"pre_book_percentage\": true, \"settlement_bucket4_pct\": null, \"settlement_period_end_date\": null, \"charge5\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"settlement_bucket5_pct\": null, \"voucher_number\": null, \"charge6\": null, \"settlement_bucket2_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"settlement_pct\": 100.00, \"vendor_employee_numer\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"driver_number_text\": \"WALT DARBY\", \"driver_number\": 3215, \"fuel_plan\": null, \"agents_etrac_partner_id\": null, \"charge2\": null, \"charge3\": null, \"settlement_bucket1_pct\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"id\": \"002018775030D1\", \"record_type\": 0, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"pay_chart_used\": null, \"fuel_price_zone\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"charge1\": null, \"control_number\": 1877503, \"adjustment_type\": null, \"order_date\": \"2021-07-14\", \"time_last_updated\": \"14:57\", \"agent_etrac_transaction_number\": null, \"date_last_updated\": \"2021-07-21\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"settlement_bucket3_pct\": null}], \"calc_add_on_chgs\": false, \"original_schedule_number\": null, \"blocks\": null, \"del_actual_location_accuracy\": null, \"zone_set_used\": 1, \"pickup_country\": null, \"pickup_state\": \"VA\", \"add_charge_amt7\": null, \"email_addresses\": null, \"add_charge_occur1\": null, \"pickup_wait_time\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"distribution_branch_id\": null, \"rate_buck_amt9\": null, \"add_charge_amt1\": null, \"pickup_requested_dep_time\": \"09:00\", \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"deliver_state\": \"VA\", \"deliver_dispatch_zone\": null, \"image_sign_req\": false, \"add_charge_code6\": null, \"deliver_requested_date\": \"2021-07-14\", \"add_charge_amt5\": null, \"time_order_entered\": \"15:57\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"pickup_attention\": null, \"rate_buck_amt7\": null, \"add_charge_occur6\": null, \"fuel_price_source\": null, \"pickup_airport_code\": null, \"rate_buck_amt2\": null, \"rate_buck_amt3\": null, \"deliver_omw_timestamp\": null, \"exception_code\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"weight\": null, \"signature_required\": true, \"rate_buck_amt10\": null, \"hist_inv_number\": 0, \"deliver_pricing_zone\": 1, \"pickup_actual_longitude\": null, \"push_services\": null, \"add_charge_amt11\": null, \"rt_actual_location_accuracy\": null, \"roundtrip_actual_date\": null, \"pickup_requested_arr_time\": null, \"deliver_attention\": null, \"deliver_special_instructions3\": null, \"pickup_actual_pieces\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"roundtrip_signature\": null, \"po_number\": null, \"signature\": \"SOF\", \"pickup_special_instructions2\": null, \"original_ctrl_number\": null, \"previous_ctrl_number\": null}}";
                                                        // objresponse.Reason = "{\"002018724440\": {\"verified_weight\": null, \"roundtrip_actual_latitude\": null, \"pickup_special_instructions4\": null, \"fuel_miles\": null, \"pickup_dispatch_zone\": null, \"pickup_zip\": \"23150\", \"pickup_actual_arr_time\": \"08:00\", \"cod_accept_company_check\": false, \"add_charge_occur9\": null, \"pickup_omw_latitude\": null, \"service_level_text\": \"Room of Choice\", \"service_level\": 55, \"exception_sign_required\": false, \"pickup_phone_ext\": null, \"roundtrip_actual_pieces\": null, \"bringg_send_sms\": false, \"az_equip2\": null, \"hist_inv_date\": null, \"date_order_entered\": \"2021-07-08\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"pickup_city\": \"SANDSTON\", \"pickup_phone\": null, \"pickup_sign_req\": true, \"deliver_route_sequence\": null, \"deliver_phone\": null, \"deliver_omw_longitude\": null, \"roundtrip_actual_longitude\": null, \"page_number\": 1, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"add_charge_code9\": null, \"pickup_eta_time\": null, \"record_type\": 0, \"add_charge_occur11\": null, \"push_partner_order_id\": null, \"deliver_country\": null, \"customer_name\": \"MXD/RYDER\", \"bol_number\": null, \"pickup_latitude\": 37.53250820, \"add_charge_code4\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"pu_arrive_notification_sent\": false, \"distribution_shift_id\": null, \"pickup_special_instr_long\": null, \"id\": \"002018724440\", \"callback_to\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"ordered_by\": \"RYDER\", \"add_charge_code12\": null, \"pickup_route_seq\": null, \"deliver_city\": \"CHESAPEAKE\", \"add_charge_occur5\": null, \"edi_acknowledgement_required\": false, \"rescheduled_ctrl_number\": null, \"driver2\": null, \"deliver_room\": null, \"deliver_actual_arr_time\": \"08:00\", \"fuel_price_zone\": null, \"add_charge_amt9\": null, \"add_charge_amt4\": null, \"delivery_address_point_number_text\": \"ANNE BAILEY\", \"delivery_address_point_number\": 26312, \"deliver_actual_longitude\": null, \"add_charge_amt2\": null, \"additional_drivers\": false, \"pickup_pricing_zone\": 1, \"hazmat\": false, \"pickup_address\": \"540 EASTPARK CT\", \"pickup_route_code\": null, \"callback_userid\": null, \"pickup_point_customer\": 31025, \"rate_buck_amt1\": 57.00, \"add_charge_amt8\": null, \"callback_time\": null, \"csr\": \"DX*\", \"roundtrip_actual_depart_time\": null, \"customers_etrac_partner_id\": \"96609250\", \"manual_notepad\": false, \"add_charge_code8\": null, \"bringg_order_id\": null, \"deliver_omw_latitude\": null, \"pickup_longitude\": -77.33035820, \"etrac_number\": null, \"distribution_unique_id\": 0, \"vehicle_type\": null, \"roundtrip_actual_arrival_time\": null, \"delivery_longitude\": -76.34760620, \"pu_actual_location_accuracy\": null, \"deliver_actual_date\": \"2021-06-28\", \"exception_timestamp\": null, \"deliver_zip\": \"23323\", \"roundtrip_wait_time\": null, \"add_charge_occur8\": null, \"dl_arrive_notification_sent\": false, \"pickup_special_instructions1\": null, \"ordered_by_phone_number\": null, \"deliver_requested_arr_time\": \"08:00\", \"rate_miles\": null, \"holiday_groups\": null, \"pickup_email_notification_sent\": false, \"add_charge_code3\": null, \"dispatch_id\": null, \"add_charge_occur10\": null, \"dispatch_time\": null, \"deliver_wait_time\": null, \"invoice_period_end_date\": null, \"add_charge_occur12\": null, \"fuel_plan\": null, \"return_svc_level\": null, \"pickup_actual_date\": \"2021-06-28\", \"send_new_order_alert\": false, \"pickup_room\": null, \"rate_buck_amt8\": null, \"add_charge_amt10\": null, \"insurance_amount\": null, \"add_charge_amt3\": null, \"add_charge_amt6\": null, \"pickup_special_instructions3\": null, \"pickup_requested_date\": \"2021-06-28\", \"roundtrip_sign_req\": false, \"actual_miles\": null, \"pickup_address_point_number_text\": \"BIG LOTS\", \"pickup_address_point_number\": 19864, \"deliver_actual_latitude\": null, \"deliver_phone_ext\": null, \"deliver_route_code\": null, \"add_charge_code10\": null, \"delivery_airport_code\": null, \"reference_text\": \"2125095801\", \"reference\": \"2125095801\", \"photos_exist\": false, \"master_airway_bill_number\": null, \"control_number\": 1872444, \"cod_text\": \"No\", \"cod\": \"N\", \"rate_buck_amt11\": null, \"cod_amount\": null, \"pickup_omw_timestamp\": null, \"signature_images\": [], \"deliver_special_instructions1\": null, \"quote_amount\": null, \"total_pages\": 1, \"rate_buck_amt4\": null, \"line_items\": [], \"delivery_latitude\": 36.78396970, \"progress\": [{\"status_time\": \"06:02:00\", \"status_date\": \"2021-07-08\", \"status_text\": \"Entered in carrier's system\"}, {\"status_time\": \"08:30:00\", \"status_date\": \"2021-06-28\", \"status_text\": \"Picked up\"}, {\"status_time\": \"08:15:00\", \"status_date\": \"2021-06-28\", \"status_text\": \"Delivered\"}], \"add_charge_code1\": null, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"deliver_special_instr_long\": null, \"deliver_address\": \"2920 AARON DR\", \"add_charge_occur4\": null, \"deliver_eta_date\": null, \"pickup_actual_dep_time\": \"08:30\", \"deliver_requested_dep_time\": \"17:00\", \"add_charge_code7\": null, \"deliver_actual_dep_time\": \"08:15\", \"cod_accept_cashiers_check\": false, \"bringg_last_loc_sent\": null, \"az_equip3\": null, \"driver1_text\": \"LAVERT KENDALL MORRIS\", \"driver1\": 3001, \"pickup_actual_latitude\": null, \"add_charge_occur2\": null, \"order_automatically_quoted\": false, \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"hours\": \"15\", \"frequent_caller_id\": null, \"rate_buck_amt6\": null, \"rate_chart_used\": 0, \"deliver_actual_pieces\": null, \"add_charge_code5\": null, \"pickup_omw_longitude\": null, \"notes\": [], \"delivery_point_customer\": 31025, \"add_charge_occur7\": null, \"rate_buck_amt5\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"add_charge_code11\": null, \"pickup_name\": \"BIG LOTS\", \"callback_date\": null, \"add_charge_code2\": null, \"house_airway_bill_number\": null, \"deliver_name\": \"ANNE BAILEY\", \"number_of_pieces\": 3, \"deliver_eta_time\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"rate_special_instructions\": null, \"add_charge_occur3\": null, \"pickup_eta_date\": null, \"deliver_special_instructions4\": null, \"custom_special_instr_long\": null, \"deliver_special_instructions2\": null, \"pickup_signature\": \"SOF\", \"az_equip1\": null, \"add_charge_amt12\": null, \"settlements\": [{\"vendor_invoice_number\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"voucher_date\": null, \"voucher_amount\": null, \"fuel_price_source\": null, \"settlement_bucket6_pct\": null, \"pre_book_percentage\": true, \"settlement_bucket4_pct\": null, \"settlement_period_end_date\": null, \"charge5\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"settlement_bucket5_pct\": null, \"voucher_number\": null, \"charge6\": null, \"settlement_bucket2_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"settlement_pct\": 100.00, \"vendor_employee_numer\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"driver_number_text\": \"LAVERT KENDALL MORRIS\", \"driver_number\": 3001, \"fuel_plan\": null, \"agents_etrac_partner_id\": null, \"charge2\": null, \"charge3\": null, \"settlement_bucket1_pct\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"id\": \"002018724440D1\", \"record_type\": 0, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"pay_chart_used\": null, \"fuel_price_zone\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"charge1\": null, \"control_number\": 1872444, \"adjustment_type\": null, \"order_date\": \"2021-06-28\", \"time_last_updated\": \"05:02\", \"agent_etrac_transaction_number\": null, \"date_last_updated\": \"2021-07-08\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"settlement_bucket3_pct\": null}], \"calc_add_on_chgs\": false, \"original_schedule_number\": null, \"blocks\": null, \"del_actual_location_accuracy\": null, \"zone_set_used\": 1, \"pickup_country\": null, \"pickup_state\": \"VA\", \"add_charge_amt7\": null, \"email_addresses\": null, \"add_charge_occur1\": null, \"pickup_wait_time\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"distribution_branch_id\": null, \"rate_buck_amt9\": null, \"add_charge_amt1\": null, \"pickup_requested_dep_time\": \"09:00\", \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"deliver_state\": \"VA\", \"deliver_dispatch_zone\": null, \"image_sign_req\": false, \"add_charge_code6\": null, \"deliver_requested_date\": \"2021-06-28\", \"add_charge_amt5\": null, \"time_order_entered\": \"06:02\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"pickup_attention\": null, \"rate_buck_amt7\": null, \"add_charge_occur6\": null, \"fuel_price_source\": null, \"pickup_airport_code\": null, \"rate_buck_amt2\": null, \"rate_buck_amt3\": null, \"deliver_omw_timestamp\": null, \"exception_code\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"weight\": null, \"signature_required\": true, \"rate_buck_amt10\": 2.34, \"hist_inv_number\": 0, \"deliver_pricing_zone\": 1, \"pickup_actual_longitude\": null, \"push_services\": null, \"add_charge_amt11\": null, \"rt_actual_location_accuracy\": null, \"roundtrip_actual_date\": null, \"pickup_requested_arr_time\": \"07:00\", \"deliver_attention\": null, \"deliver_special_instructions3\": null, \"pickup_actual_pieces\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"roundtrip_signature\": null, \"po_number\": null, \"signature\": \"SOF\", \"pickup_special_instructions2\": null, \"original_ctrl_number\": null, \"previous_ctrl_number\": null}}";
                                                        //  objresponse.Reason = "{\"002018724450\": {\"roundtrip_actual_date\": null, \"notes\": [], \"pickup_phone_ext\": null, \"holiday_groups\": null, \"deliver_eta_time\": null, \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"add_charge_occur4\": null, \"deliver_state\": \"VA\", \"quote_amount\": null, \"cod_text\": \"No\", \"cod\": \"N\", \"additional_drivers\": false, \"rescheduled_ctrl_number\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"pickup_actual_pieces\": null, \"record_type\": 0, \"pickup_special_instr_long\": null, \"pickup_special_instructions3\": null, \"exception_timestamp\": null, \"deliver_actual_arr_time\": \"08:00\", \"house_airway_bill_number\": null, \"deliver_pricing_zone\": 1, \"total_pages\": 1, \"add_charge_occur11\": null, \"deliver_omw_latitude\": null, \"callback_userid\": null, \"rate_buck_amt1\": 57.00, \"pickup_point_customer\": 31025, \"pickup_eta_time\": null, \"add_charge_occur8\": null, \"invoice_period_end_date\": null, \"pickup_special_instructions1\": null, \"rate_buck_amt2\": null, \"pickup_special_instructions4\": null, \"manual_notepad\": false, \"edi_acknowledgement_required\": false, \"pickup_name\": \"BIG LOTS\", \"ordered_by_phone_number\": null, \"add_charge_amt12\": null, \"delivery_point_customer\": 31025, \"deliver_actual_dep_time\": \"08:15\", \"email_addresses\": null, \"pickup_address\": \"540 EASTPARK CT\", \"driver2\": null, \"signature_images\": [], \"rate_buck_amt11\": null, \"delivery_latitude\": 37.48366600, \"pickup_attention\": null, \"date_order_entered\": \"2021-07-08\", \"vehicle_type\": null, \"add_charge_amt9\": null, \"pickup_phone\": null, \"rate_miles\": null, \"customers_etrac_partner_id\": \"96609250\", \"order_type_text\": \"One way\", \"order_type\": \"O\", \"dl_arrive_notification_sent\": false, \"add_charge_code3\": null, \"etrac_number\": null, \"pickup_requested_arr_time\": \"07:00\", \"rate_buck_amt3\": null, \"pickup_actual_dep_time\": \"08:30\", \"line_items\": [], \"pickup_sign_req\": true, \"add_charge_code10\": null, \"deliver_city\": \"LANEXA\", \"fuel_plan\": null, \"add_charge_amt10\": null, \"roundtrip_actual_depart_time\": null, \"control_number\": 1872445, \"pickup_dispatch_zone\": null, \"send_new_order_alert\": false, \"settlements\": [{\"settlement_bucket4_pct\": null, \"charge1\": null, \"date_last_updated\": \"2021-07-08\", \"fuel_price_zone\": null, \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"charge4\": null, \"settlement_period_end_date\": null, \"charge5\": null, \"time_last_updated\": \"05:06\", \"charge6\": null, \"driver_number_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver_number\": 3208, \"control_number\": 1872445, \"settlement_bucket2_pct\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"voucher_date\": null, \"agent_etrac_transaction_number\": null, \"settlement_bucket5_pct\": null, \"record_type\": 0, \"voucher_number\": null, \"voucher_amount\": null, \"pay_chart_used\": null, \"settlement_pct\": 100.00, \"vendor_invoice_number\": null, \"settlement_bucket3_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"pre_book_percentage\": true, \"charge3\": null, \"settlement_bucket6_pct\": null, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"adjustment_type\": null, \"id\": \"002018724450D1\", \"agents_etrac_partner_id\": null, \"fuel_plan\": null, \"fuel_price_source\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"vendor_employee_numer\": null, \"settlement_bucket1_pct\": null, \"order_date\": \"2021-06-28\", \"charge2\": null}], \"deliver_actual_latitude\": null, \"fuel_price_zone\": null, \"verified_weight\": null, \"deliver_requested_dep_time\": \"17:00\", \"pickup_airport_code\": null, \"dispatch_time\": null, \"deliver_attention\": null, \"time_order_entered\": \"06:06\", \"rate_buck_amt4\": null, \"roundtrip_wait_time\": null, \"add_charge_amt2\": null, \"az_equip3\": null, \"progress\": [{\"status_date\": \"2021-07-08\", \"status_text\": \"Entered in carrier's system\", \"status_time\": \"06:06:00\"}, {\"status_date\": \"2021-06-28\", \"status_text\": \"Picked up\", \"status_time\": \"08:30:00\"}, {\"status_date\": \"2021-06-28\", \"status_text\": \"Delivered\", \"status_time\": \"08:15:00\"}], \"page_number\": 1, \"roundtrip_sign_req\": false, \"add_charge_amt1\": null, \"add_charge_code8\": null, \"weight\": null, \"rate_buck_amt6\": null, \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"bringg_send_sms\": false, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"custom_special_instr_long\": null, \"deliver_requested_arr_time\": \"08:00\", \"service_level_text\": \"Room of Choice\", \"service_level\": 55, \"az_equip1\": null, \"add_charge_code4\": null, \"bringg_order_id\": null, \"delivery_address_point_number_text\": \"JOSEPH FESSMAN\", \"delivery_address_point_number\": 26313, \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"deliver_special_instructions1\": null, \"pickup_wait_time\": null, \"add_charge_occur5\": null, \"push_partner_order_id\": null, \"deliver_route_sequence\": null, \"pickup_country\": null, \"pickup_state\": \"VA\", \"original_schedule_number\": null, \"frequent_caller_id\": null, \"distribution_unique_id\": 0, \"fuel_miles\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"rate_buck_amt5\": null, \"exception_sign_required\": false, \"pickup_route_code\": null, \"deliver_dispatch_zone\": null, \"delivery_longitude\": -76.90426400, \"pickup_pricing_zone\": 1, \"zone_set_used\": 1, \"deliver_special_instructions2\": null, \"add_charge_amt3\": null, \"deliver_phone\": null, \"pickup_email_notification_sent\": false, \"add_charge_occur12\": null, \"reference_text\": \"2125617401\", \"reference\": \"2125617401\", \"deliver_requested_date\": \"2021-06-28\", \"deliver_actual_longitude\": null, \"image_sign_req\": false, \"pickup_eta_date\": null, \"deliver_phone_ext\": null, \"pickup_omw_longitude\": null, \"original_ctrl_number\": null, \"pickup_special_instructions2\": null, \"order_automatically_quoted\": false, \"bol_number\": null, \"rate_buck_amt10\": 2.34, \"callback_time\": null, \"hazmat\": false, \"distribution_shift_id\": null, \"pickup_latitude\": 37.53250820, \"ordered_by\": \"RYDER\", \"insurance_amount\": null, \"cod_accept_cashiers_check\": false, \"add_charge_amt4\": null, \"add_charge_code7\": null, \"deliver_actual_pieces\": null, \"deliver_address\": \"15400 STAGE RD\", \"cod_accept_company_check\": false, \"signature\": \"SOF\", \"previous_ctrl_number\": null, \"deliver_zip\": \"23089\", \"deliver_special_instructions3\": null, \"rate_buck_amt7\": null, \"hist_inv_number\": 0, \"callback_date\": null, \"deliver_special_instr_long\": null, \"po_number\": null, \"pickup_actual_arr_time\": \"08:00\", \"pickup_requested_date\": \"2021-06-28\", \"number_of_pieces\": 2, \"dispatch_id\": null, \"photos_exist\": false, \"pickup_actual_latitude\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"id\": \"002018724450\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"del_actual_location_accuracy\": null, \"add_charge_occur7\": null, \"add_charge_occur9\": null, \"roundtrip_actual_latitude\": null, \"add_charge_occur6\": null, \"pickup_actual_longitude\": null, \"pickup_omw_timestamp\": null, \"bringg_last_loc_sent\": null, \"add_charge_code5\": null, \"deliver_country\": null, \"master_airway_bill_number\": null, \"pickup_route_seq\": null, \"roundtrip_signature\": null, \"calc_add_on_chgs\": false, \"deliver_actual_date\": \"2021-06-28\", \"cod_amount\": null, \"add_charge_code12\": null, \"rt_actual_location_accuracy\": null, \"rate_chart_used\": 0, \"pickup_longitude\": -77.33035820, \"pickup_signature\": \"SOF\", \"add_charge_amt5\": null, \"pu_arrive_notification_sent\": false, \"pickup_actual_date\": \"2021-06-28\", \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"push_services\": null, \"deliver_eta_date\": null, \"driver1_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver1\": 3208, \"deliver_omw_longitude\": null, \"deliver_wait_time\": null, \"pickup_room\": null, \"deliver_special_instructions4\": null, \"add_charge_amt7\": null, \"az_equip2\": null, \"hours\": \"15\", \"add_charge_code2\": null, \"exception_code\": null, \"roundtrip_actual_pieces\": null, \"rate_special_instructions\": null, \"roundtrip_actual_arrival_time\": null, \"add_charge_occur1\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"delivery_airport_code\": null, \"distribution_branch_id\": null, \"hist_inv_date\": null, \"add_charge_code1\": null, \"pickup_requested_dep_time\": \"09:00\", \"deliver_route_code\": null, \"roundtrip_actual_longitude\": null, \"pickup_city\": \"SANDSTON\", \"rate_buck_amt8\": null, \"pickup_omw_latitude\": null, \"deliver_omw_timestamp\": null, \"rate_buck_amt9\": null, \"deliver_room\": null, \"add_charge_code6\": null, \"add_charge_occur3\": null, \"blocks\": null, \"add_charge_code9\": null, \"actual_miles\": null, \"add_charge_occur10\": null, \"add_charge_code11\": null, \"pickup_address_point_number_text\": \"BIG LOTS\", \"pickup_address_point_number\": 19864, \"customer_name\": \"MXD/RYDER\", \"pu_actual_location_accuracy\": null, \"deliver_name\": \"JOSEPH FESSMAN\", \"add_charge_amt6\": null, \"signature_required\": true, \"csr\": \"DX*\", \"add_charge_amt8\": null, \"callback_to\": null, \"fuel_price_source\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"pickup_zip\": \"23150\", \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"return_svc_level\": null, \"add_charge_amt11\": null, \"add_charge_occur2\": null}}";
                                                        //objresponse.Reason = "{\"error\": \"Unable to perform API call object with RecordID:WS_OPORDR-9990111870 - The record may have been deleted.\", \"status\": \"error\", \"code\": \"U.RecordNotFound\"}";
                                                        // objresponse.Reason = "{\"999000115280\": {\"deliver_eta_time\": null, \"deliver_special_instructions4\": null, \"del_actual_location_accuracy\": null, \"add_charge_amt8\": null, \"callback_to\": null, \"rate_chart_used\": 1, \"pickup_actual_latitude\": null, \"signature_images\": [], \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"add_charge_occur8\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"hours\": null, \"distribution_shift_id\": null, \"pickup_longitude\": null, \"hist_inv_number\": 0, \"add_charge_amt7\": null, \"dispatch_id\": null, \"dl_arrive_notification_sent\": false, \"add_charge_code8\": null, \"roundtrip_sign_req\": false, \"exception_timestamp\": null, \"pickup_city\": null, \"delivery_airport_code\": null, \"progress\": [{\"status_time\": \"08:34:00\", \"status_date\": \"2022-03-10\", \"status_text\": \"Entered in carrier's system\"}], \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"add_charge_amt2\": null, \"pickup_special_instructions1\": null, \"time_order_entered\": \"08:34\", \"distribution_unique_id\": 0, \"pickup_eta_date\": null, \"rate_buck_amt2\": null, \"rate_buck_amt9\": null, \"deliver_zip\": \"48150\", \"rate_buck_amt7\": null, \"customer_name\": \"TEST\", \"deliver_phone_ext\": null, \"roundtrip_actual_depart_time\": null, \"add_charge_code10\": null, \"add_charge_occur2\": null, \"deliver_state\": \"MI\", \"pickup_wait_time\": null, \"pickup_requested_arr_time\": null, \"csr\": \"RG\", \"add_charge_amt4\": null, \"holiday_groups\": null, \"total_pages\": 1, \"delivery_longitude\": -83.35663860, \"bringg_last_loc_sent\": null, \"deliver_name\": \"TANTARA\", \"deliver_actual_longitude\": null, \"distribution_branch_id\": null, \"deliver_wait_time\": null, \"add_charge_occur11\": null, \"deliver_omw_timestamp\": null, \"add_charge_amt3\": null, \"add_charge_amt10\": null, \"rate_buck_amt3\": null, \"rescheduled_ctrl_number\": null, \"add_charge_occur10\": null, \"deliver_address\": \"31782 ENTERPRISE DR\", \"pickup_latitude\": null, \"rate_buck_amt1\": null, \"pickup_phone\": null, \"pickup_actual_date\": \"2022-01-03\", \"previous_ctrl_number\": null, \"control_number\": 11528, \"rate_buck_amt11\": null, \"fuel_price_source\": null, \"add_charge_code9\": null, \"add_charge_occur3\": null, \"fuel_price_zone\": null, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"rate_buck_amt10\": null, \"add_charge_code12\": null, \"quote_amount\": null, \"deliver_phone\": null, \"ordered_by_phone_number\": null, \"cod_accept_company_check\": false, \"callback_time\": null, \"deliver_dispatch_zone\": null, \"hazmat\": false, \"az_equip2\": null, \"add_charge_occur1\": null, \"pickup_email_notification_sent\": false, \"deliver_requested_dep_time\": null, \"deliver_special_instructions1\": null, \"deliver_actual_date\": null, \"rt_actual_location_accuracy\": null, \"signature_required\": false, \"pickup_attention\": null, \"pu_actual_location_accuracy\": null, \"rate_special_instructions\": null, \"pickup_special_instructions2\": null, \"driver2\": null, \"deliver_route_sequence\": null, \"add_charge_code2\": null, \"pickup_state\": null, \"add_charge_code1\": null, \"deliver_actual_pieces\": null, \"pickup_country\": null, \"signature\": null, \"add_charge_occur12\": null, \"reference_text\": \"FEDX01032022\", \"reference\": \"FEDX01032022\", \"pickup_pricing_zone\": null, \"pickup_route_seq\": null, \"pickup_actual_arr_time\": null, \"date_order_entered\": \"2022-03-10\", \"rate_buck_amt5\": null, \"number_of_pieces\": null, \"add_charge_code11\": null, \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"pickup_omw_timestamp\": null, \"delivery_point_customer\": 1, \"roundtrip_actual_latitude\": null, \"rate_buck_amt4\": null, \"pickup_requested_date\": \"2022-01-03\", \"po_number\": null, \"origin_code_text\": \"Web-Carrier UI\", \"origin_code\": \"X\", \"add_charge_occur7\": null, \"exception_sign_required\": false, \"id\": \"999000115280\", \"pickup_route_code\": null, \"pickup_airport_code\": null, \"roundtrip_actual_date\": null, \"roundtrip_signature\": null, \"roundtrip_actual_pieces\": null, \"pickup_phone_ext\": null, \"deliver_city\": \"LIVONIA\", \"deliver_omw_latitude\": null, \"service_level_text\": \"REGULAR\", \"service_level\": 1, \"order_timeliness_text\": \"Open\", \"order_timeliness\": \"5\", \"roundtrip_actual_arrival_time\": null, \"deliver_actual_arr_time\": null, \"etrac_number\": null, \"add_charge_occur9\": null, \"az_equip1\": null, \"rate_miles\": null, \"frequent_caller_id\": null, \"pickup_sign_req\": false, \"customer_type\": null, \"pickup_omw_latitude\": null, \"actual_miles\": null, \"add_charge_code3\": null, \"deliver_eta_date\": null, \"fuel_miles\": null, \"pickup_special_instructions4\": null, \"house_airway_bill_number\": null, \"vehicle_type\": null, \"cod_accept_cashiers_check\": false, \"settlements\": [], \"pickup_address\": null, \"pickup_room\": null, \"weight\": null, \"pickup_actual_longitude\": null, \"rate_buck_amt8\": null, \"delivery_address_point_number_text\": \"TANTARA\", \"delivery_address_point_number\": 10, \"status_code_text\": \"Entered\", \"status_code\": \"E\", \"master_airway_bill_number\": null, \"delivery_latitude\": 42.36977420, \"bringg_order_id\": null, \"add_charge_code7\": null, \"roundtrip_wait_time\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"cod_text\": \"No\", \"cod\": \"N\", \"deliver_room\": null, \"rate_buck_amt6\": null, \"pu_arrive_notification_sent\": false, \"deliver_actual_latitude\": null, \"blocks\": null, \"callback_userid\": null, \"edi_acknowledgement_required\": false, \"add_charge_code4\": null, \"driver1\": null, \"calc_add_on_chgs\": false, \"fuel_plan\": null, \"add_charge_amt9\": null, \"pickup_point_customer\": 0, \"zone_set_used\": 1, \"exception_code\": null, \"invoice_period_end_date\": null, \"push_partner_order_id\": null, \"verified_weight\": null, \"pickup_actual_pieces\": null, \"notes\": [], \"add_charge_amt12\": null, \"deliver_special_instructions3\": null, \"deliver_special_instructions2\": null, \"customer_number_text\": \"test\", \"customer_number\": 1, \"return_svc_level\": null, \"add_charge_amt1\": null, \"az_equip3\": null, \"image_sign_req\": false, \"pickup_omw_longitude\": null, \"deliver_requested_arr_time\": null, \"pickup_name\": null, \"pickup_address_point_number\": null, \"pickup_signature\": null, \"pickup_special_instructions3\": null, \"original_ctrl_number\": null, \"add_charge_code6\": null, \"deliver_omw_longitude\": null, \"additional_drivers\": false, \"deliver_country\": null, \"add_charge_amt5\": null, \"insurance_amount\": null, \"cod_amount\": null, \"email_addresses\": null, \"pickup_actual_dep_time\": null, \"page_number\": 1, \"dispatch_time\": null, \"callback_date\": null, \"add_charge_occur5\": null, \"add_charge_occur6\": null, \"company_number_text\": \"TEST COMPANY\", \"company_number\": 999, \"pickup_dispatch_zone\": null, \"deliver_attention\": null, \"record_type\": 0, \"deliver_pricing_zone\": 1, \"deliver_requested_date\": \"2022-01-03\", \"push_services\": null, \"add_charge_amt6\": null, \"order_automatically_quoted\": false, \"custom_special_instr_long\": null, \"bol_number\": \"FEDX01032022\", \"hist_inv_date\": null, \"roundtrip_actual_longitude\": null, \"add_charge_amt11\": null, \"bringg_send_sms\": false, \"pickup_special_instr_long\": null, \"ordered_by\": \"DET\", \"deliver_special_instr_long\": null, \"pickup_zip\": null, \"pickup_requested_dep_time\": null, \"deliver_route_code\": null, \"deliver_actual_dep_time\": null, \"customers_etrac_partner_id\": null, \"add_charge_code5\": null, \"photos_exist\": false, \"original_schedule_number\": null, \"add_charge_occur4\": null, \"send_new_order_alert\": false, \"manual_notepad\": false, \"line_items\": [], \"pickup_eta_time\": null, \"_utc_offset\": \"-06:00\"}}";
                                                        if (objresponse.ResponseVal)
                                                        {
                                                            strExecutionLogMessage = "OrderPostAPI Success " + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                            DataSet dsOrderResponse = objCommon.jsonToDataSet(objresponse.Reason, "OrderPost");
                                                            var UniqueId = Convert.ToString(dsOrderResponse.Tables["id"].Rows[0]["id"]);
                                                            try
                                                            {
                                                                if (dsOrderResponse.Tables.Contains("id"))
                                                                {
                                                                    List<Id> idList = new List<Id>();
                                                                    for (int i = 0; i < dsOrderResponse.Tables["id"].Rows.Count; i++)
                                                                    {
                                                                        DataTable dt = dsOrderResponse.Tables["id"];
                                                                        Id objIds = new Id();
                                                                        // objIds.verified_weight = dt.Rows[i]["verified_weight"];
                                                                        // objIds.roundtrip_actual_latitude = dt.Rows[i]["roundtrip_actual_latitude"];
                                                                        // objIds.pickup_special_instructions4 = dt.Rows[i]["pickup_special_instructions4"];
                                                                        // objIds.fuel_miles = dt.Rows[i]["fuel_miles"];
                                                                        // objIds.pickup_dispatch_zone = dt.Rows[i]["pickup_dispatch_zone"];
                                                                        if (dt.Columns.Contains("pickup_zip"))
                                                                        {
                                                                            objIds.pickup_zip = dt.Rows[i]["pickup_zip"];
                                                                        }
                                                                        if (dt.Columns.Contains("pickup_actual_arr_time"))
                                                                        {
                                                                            objIds.pickup_actual_arr_time = dt.Rows[i]["pickup_actual_arr_time"];
                                                                        }
                                                                        //objIds.cod_accept_company_check = dt.Rows[i]["cod_accept_company_check"];
                                                                        // objIds.add_charge_occur9 = dt.Rows[i]["add_charge_occur9"];
                                                                        //objIds.pickup_omw_latitude = dt.Rows[i]["pickup_omw_latitude"];
                                                                        // objIds.service_level_text = dt.Rows[i]["service_level_text"];
                                                                        if (dt.Columns.Contains("service_level"))
                                                                        {
                                                                            objIds.service_level = dt.Rows[i]["service_level"];
                                                                        }
                                                                        //objIds.exception_sign_required = dt.Rows[i]["exception_sign_required"];
                                                                        //objIds.pickup_phone_ext = dt.Rows[i]["pickup_phone_ext"];
                                                                        //objIds.roundtrip_actual_pieces = dt.Rows[i]["roundtrip_actual_pieces"];
                                                                        //objIds.bringg_send_sms = dt.Rows[i]["bringg_send_sms"];
                                                                        //objIds.az_equip2 = dt.Rows[i]["az_equip2"];

                                                                        //objIds.hist_inv_date = dt.Rows[i]["hist_inv_date"];
                                                                        //objIds.date_order_entered = dt.Rows[i]["date_order_entered"];
                                                                        //objIds.powerpage_status_text = dt.Rows[i]["powerpage_status_text"];
                                                                        //objIds.powerpage_status = dt.Rows[i]["powerpage_status"];
                                                                        if (dt.Columns.Contains("pickup_city"))
                                                                        {
                                                                            objIds.pickup_city = dt.Rows[i]["pickup_city"];
                                                                        }
                                                                        //objIds.pickup_phone = dt.Rows[i]["pickup_phone"];
                                                                        //objIds.pickup_sign_req = dt.Rows[i]["pickup_sign_req"];

                                                                        //objIds.deliver_phone = dt.Rows[i]["deliver_phone"];
                                                                        //objIds.deliver_omw_longitude = dt.Rows[i]["deliver_omw_longitude"];
                                                                        //objIds.roundtrip_actual_longitude = dt.Rows[i]["roundtrip_actual_longitude"];
                                                                        //objIds.page_number = dt.Rows[i]["page_number"];
                                                                        //objIds.order_type_text = dt.Rows[i]["order_type_text"];
                                                                        //objIds.order_type = dt.Rows[i]["order_type"];
                                                                        //objIds.add_charge_code9 = dt.Rows[i]["add_charge_code9"];
                                                                        //objIds.pickup_eta_time = dt.Rows[i]["pickup_eta_time"];

                                                                        //objIds.record_type = dt.Rows[i]["record_type"];
                                                                        //objIds.add_charge_occur11 = dt.Rows[i]["add_charge_occur11"];
                                                                        //objIds.push_partner_order_id = dt.Rows[i]["push_partner_order_id"];
                                                                        //objIds.deliver_country = dt.Rows[i]["deliver_country"];
                                                                        //objIds.customer_name = dt.Rows[i]["customer_name"];
                                                                        if (dt.Columns.Contains("bol_number"))
                                                                        {
                                                                            objIds.bol_number = dt.Rows[i]["bol_number"];
                                                                        }
                                                                        //objIds.pickup_latitude = dt.Rows[i]["pickup_latitude"];
                                                                        //objIds.add_charge_code4 = dt.Rows[i]["add_charge_code4"];

                                                                        //objIds.exception_order_action_text = dt.Rows[i]["exception_order_action_text"];
                                                                        //objIds.exception_order_action = dt.Rows[i]["exception_order_action"];
                                                                        //objIds.pu_arrive_notification_sent = dt.Rows[i]["pu_arrive_notification_sent"];
                                                                        //objIds.distribution_shift_id = dt.Rows[i]["distribution_shift_id"];
                                                                        //objIds.pickup_special_instr_long = dt.Rows[i]["pickup_special_instr_long"];
                                                                        if (dt.Columns.Contains("id"))
                                                                        {
                                                                            objIds.id = dt.Rows[i]["id"];
                                                                        }
                                                                        //objIds.callback_to = dt.Rows[i]["callback_to"];
                                                                        //objIds.customer_number_text = dt.Rows[i]["customer_number_text"];
                                                                        if (dt.Columns.Contains("customer_number"))
                                                                        {
                                                                            objIds.customer_number = dt.Rows[i]["customer_number"];
                                                                        }
                                                                        //objIds.ordered_by = dt.Rows[i]["ordered_by"];
                                                                        //objIds.add_charge_code12 = dt.Rows[i]["add_charge_code12"];
                                                                        //objIds.pickup_route_seq = dt.Rows[i]["pickup_route_seq"];
                                                                        if (dt.Columns.Contains("deliver_city"))
                                                                        {
                                                                            objIds.deliver_city = dt.Rows[i]["deliver_city"];
                                                                        }

                                                                        //objIds.add_charge_occur5 = dt.Rows[i]["add_charge_occur5"];
                                                                        //objIds.edi_acknowledgement_required = dt.Rows[i]["edi_acknowledgement_required"];
                                                                        //objIds.rescheduled_ctrl_number = dt.Rows[i]["rescheduled_ctrl_number"];
                                                                        //objIds.driver2 = dt.Rows[i]["driver2"];
                                                                        //objIds.deliver_room = dt.Rows[i]["deliver_room"];
                                                                        if (dt.Columns.Contains("deliver_actual_arr_time"))
                                                                        {
                                                                            objIds.deliver_actual_arr_time = dt.Rows[i]["deliver_actual_arr_time"];
                                                                        }
                                                                        //objIds.fuel_price_zone = dt.Rows[i]["fuel_price_zone"];
                                                                        //objIds.add_charge_amt9 = dt.Rows[i]["add_charge_amt9"];
                                                                        //objIds.add_charge_amt4 = dt.Rows[i]["add_charge_amt4"];
                                                                        //objIds.delivery_address_point_number_text = dt.Rows[i]["delivery_address_point_number_text"];
                                                                        //objIds.delivery_address_point_number = dt.Rows[i]["delivery_address_point_number"];

                                                                        //objIds.deliver_actual_longitude = dt.Rows[i]["deliver_actual_longitude"];
                                                                        //objIds.add_charge_amt2 = dt.Rows[i]["add_charge_amt2"];
                                                                        //objIds.additional_drivers = dt.Rows[i]["additional_drivers"];
                                                                        //objIds.pickup_pricing_zone = dt.Rows[i]["pickup_pricing_zone"];
                                                                        //objIds.hazmat = dt.Rows[i]["hazmat"];
                                                                        if (dt.Columns.Contains("pickup_address"))
                                                                        {
                                                                            objIds.pickup_address = dt.Rows[i]["pickup_address"];
                                                                        }
                                                                        //objIds.pickup_route_code = dt.Rows[i]["pickup_route_code"];
                                                                        //objIds.callback_userid = dt.Rows[i]["callback_userid"];
                                                                        //objIds.pickup_point_customer = dt.Rows[i]["pickup_point_customer"];

                                                                        //objIds.rate_buck_amt1 = dt.Rows[i]["rate_buck_amt1"];
                                                                        //objIds.add_charge_amt8 = dt.Rows[i]["add_charge_amt8"];
                                                                        //objIds.callback_time = dt.Rows[i]["callback_time"];
                                                                        //objIds.csr = dt.Rows[i]["csr"];
                                                                        //objIds.roundtrip_actual_depart_time = dt.Rows[i]["roundtrip_actual_depart_time"];
                                                                        //objIds.customers_etrac_partner_id = dt.Rows[i]["customers_etrac_partner_id"];
                                                                        //objIds.manual_notepad = dt.Rows[i]["manual_notepad"];
                                                                        //objIds.add_charge_code8 = dt.Rows[i]["add_charge_code8"];
                                                                        //objIds.bringg_order_id = dt.Rows[i]["bringg_order_id"];
                                                                        //objIds.deliver_omw_latitude = dt.Rows[i]["deliver_omw_latitude"];
                                                                        //objIds.pickup_longitude = dt.Rows[i]["pickup_longitude"];
                                                                        //objIds.etrac_number = dt.Rows[i]["etrac_number"];

                                                                        //objIds.distribution_unique_id = dt.Rows[i]["distribution_unique_id"];
                                                                        //objIds.vehicle_type = dt.Rows[i]["vehicle_type"];
                                                                        //objIds.roundtrip_actual_arrival_time = dt.Rows[i]["roundtrip_actual_arrival_time"];
                                                                        //objIds.delivery_longitude = dt.Rows[i]["delivery_longitude"];
                                                                        //objIds.pu_actual_location_accuracy = dt.Rows[i]["pu_actual_location_accuracy"];
                                                                        if (dt.Columns.Contains("deliver_actual_date"))
                                                                        {
                                                                            objIds.deliver_actual_date = dt.Rows[i]["deliver_actual_date"];
                                                                        }
                                                                        //objIds.exception_timestamp = dt.Rows[i]["exception_timestamp"];
                                                                        if (dt.Columns.Contains("deliver_zip"))
                                                                        {
                                                                            objIds.deliver_zip = dt.Rows[i]["deliver_zip"];
                                                                        }
                                                                        //objIds.roundtrip_wait_time = dt.Rows[i]["roundtrip_wait_time"];
                                                                        //objIds.add_charge_occur8 = dt.Rows[i]["add_charge_occur8"];
                                                                        //objIds.dl_arrive_notification_sent = dt.Rows[i]["dl_arrive_notification_sent"];
                                                                        //objIds.pickup_special_instructions1 = dt.Rows[i]["pickup_special_instructions1"];
                                                                        //objIds.ordered_by_phone_number = dt.Rows[i]["ordered_by_phone_number"];
                                                                        if (dt.Columns.Contains("deliver_requested_arr_time"))
                                                                        {
                                                                            objIds.deliver_requested_arr_time = dt.Rows[i]["deliver_requested_arr_time"];
                                                                        }

                                                                        //objIds.rate_miles = dt.Rows[i]["rate_miles"];
                                                                        //objIds.holiday_groups = dt.Rows[i]["holiday_groups"];
                                                                        //objIds.pickup_email_notification_sent = dt.Rows[i]["pickup_email_notification_sent"];
                                                                        //objIds.add_charge_code3 = dt.Rows[i]["add_charge_code3"];
                                                                        //objIds.dispatch_id = dt.Rows[i]["dispatch_id"];
                                                                        //objIds.add_charge_occur10 = dt.Rows[i]["add_charge_occur10"];
                                                                        //objIds.dispatch_time = dt.Rows[i]["dispatch_time"];
                                                                        //objIds.deliver_wait_time = dt.Rows[i]["deliver_wait_time"];
                                                                        //objIds.invoice_period_end_date = dt.Rows[i]["invoice_period_end_date"];
                                                                        //objIds.add_charge_occur12 = dt.Rows[i]["add_charge_occur12"];

                                                                        //objIds.fuel_plan = dt.Rows[i]["fuel_plan"];
                                                                        //objIds.return_svc_level = dt.Rows[i]["return_svc_level"];
                                                                        if (dt.Columns.Contains("pickup_actual_date"))
                                                                        {
                                                                            objIds.pickup_actual_date = dt.Rows[i]["pickup_actual_date"];
                                                                        }
                                                                        //objIds.send_new_order_alert = dt.Rows[i]["send_new_order_alert"];
                                                                        //objIds.pickup_room = dt.Rows[i]["pickup_room"];
                                                                        //objIds.rate_buck_amt8 = dt.Rows[i]["rate_buck_amt8"];
                                                                        //objIds.add_charge_amt10 = dt.Rows[i]["add_charge_amt10"];
                                                                        //objIds.insurance_amount = dt.Rows[i]["insurance_amount"];
                                                                        //objIds.add_charge_amt3 = dt.Rows[i]["add_charge_amt3"];
                                                                        //objIds.add_charge_amt6 = dt.Rows[i]["add_charge_amt6"];
                                                                        //objIds.pickup_special_instructions3 = dt.Rows[i]["pickup_special_instructions3"];
                                                                        if (dt.Columns.Contains("pickup_requested_date"))
                                                                        {
                                                                            objIds.pickup_requested_date = dt.Rows[i]["pickup_requested_date"];
                                                                        }
                                                                        //objIds.roundtrip_sign_req = dt.Rows[i]["roundtrip_sign_req"];
                                                                        //objIds.actual_miles = dt.Rows[i]["actual_miles"];
                                                                        //objIds.pickup_address_point_number_text = dt.Rows[i]["pickup_address_point_number_text"];
                                                                        //if (dt.Columns.Contains("pickup_address_point_number_text"))
                                                                        //{
                                                                        //    objIds.pickup_address_point_number_text = dt.Rows[i]["pickup_address_point_number_text"];
                                                                        //}
                                                                        //objIds.pickup_address_point_number = dt.Rows[i]["pickup_address_point_number"];
                                                                        //objIds.deliver_actual_latitude = dt.Rows[i]["deliver_actual_latitude"];
                                                                        //objIds.deliver_phone_ext = dt.Rows[i]["deliver_phone_ext"];
                                                                        //objIds.deliver_route_code = dt.Rows[i]["deliver_route_code"];
                                                                        //objIds.add_charge_code10 = dt.Rows[i]["add_charge_code10"];
                                                                        //objIds.delivery_airport_code = dt.Rows[i]["delivery_airport_code"];
                                                                        if (dt.Columns.Contains("reference_text"))
                                                                        {
                                                                            objIds.reference_text = dt.Rows[i]["reference_text"];
                                                                        }
                                                                        if (dt.Columns.Contains("reference"))
                                                                        {
                                                                            objIds.reference = dt.Rows[i]["reference"];
                                                                        }
                                                                        //objIds.photos_exist = dt.Rows[i]["photos_exist"];
                                                                        //objIds.master_airway_bill_number = dt.Rows[i]["master_airway_bill_number"];
                                                                        if (dt.Columns.Contains("control_number"))
                                                                        {
                                                                            objIds.control_number = dt.Rows[i]["control_number"];
                                                                        }
                                                                        //objIds.cod_text = dt.Rows[i]["cod_text"];
                                                                        //objIds.cod = dt.Rows[i]["cod"];
                                                                        //objIds.rate_buck_amt11 = dt.Rows[i]["rate_buck_amt11"];
                                                                        //objIds.pickup_omw_timestamp = dt.Rows[i]["pickup_omw_timestamp"];
                                                                        //objIds.deliver_special_instructions1 = dt.Rows[i]["deliver_special_instructions1"];
                                                                        //objIds.quote_amount = dt.Rows[i]["quote_amount"];
                                                                        //objIds.total_pages = dt.Rows[i]["total_pages"];
                                                                        //objIds.rate_buck_amt4 = dt.Rows[i]["rate_buck_amt4"];
                                                                        //objIds.delivery_latitude = dt.Rows[i]["delivery_latitude"];
                                                                        //objIds.add_charge_code1 = dt.Rows[i]["add_charge_code1"];


                                                                        //objIds.order_timeliness_text = dt.Rows[i]["order_timeliness_text"];
                                                                        //objIds.order_timeliness = dt.Rows[i]["order_timeliness"];
                                                                        //objIds.deliver_special_instr_long = dt.Rows[i]["deliver_special_instr_long"];
                                                                        if (dt.Columns.Contains("deliver_address"))
                                                                        {
                                                                            objIds.deliver_address = dt.Rows[i]["deliver_address"];
                                                                        }
                                                                        //objIds.add_charge_occur4 = dt.Rows[i]["add_charge_occur4"];
                                                                        //objIds.deliver_eta_date = dt.Rows[i]["deliver_eta_date"];
                                                                        if (dt.Columns.Contains("pickup_actual_dep_time"))
                                                                        {
                                                                            objIds.pickup_actual_dep_time = dt.Rows[i]["pickup_actual_dep_time"];
                                                                        }
                                                                        if (dt.Columns.Contains("deliver_requested_dep_time"))
                                                                        {
                                                                            objIds.deliver_requested_dep_time = dt.Rows[i]["deliver_requested_dep_time"];
                                                                        }
                                                                        if (dt.Columns.Contains("deliver_actual_dep_time"))
                                                                        {
                                                                            objIds.deliver_actual_dep_time = dt.Rows[i]["deliver_actual_dep_time"];
                                                                        }

                                                                        //objIds.bringg_last_loc_sent = dt.Rows[i]["bringg_last_loc_sent"];
                                                                        //objIds.az_equip3 = dt.Rows[i]["az_equip3"];
                                                                        //objIds.driver1_text = dt.Rows[i]["driver1_text"];
                                                                        //if (dt.Columns.Contains("driver1_text"))
                                                                        //{
                                                                        //    objIds.driver1_text = dt.Rows[i]["driver1_text"];
                                                                        //}
                                                                        if (dt.Columns.Contains("driver1"))
                                                                        {
                                                                            objIds.driver1 = dt.Rows[i]["driver1"];
                                                                        }
                                                                        //objIds.pickup_actual_latitude = dt.Rows[i]["pickup_actual_latitude"];
                                                                        //objIds.add_charge_occur2 = dt.Rows[i]["add_charge_occur2"];
                                                                        //objIds.order_automatically_quoted = dt.Rows[i]["order_automatically_quoted"];
                                                                        //objIds.callback_required = dt.Rows[i]["callback_required_text"];
                                                                        //objIds.frequent_caller_id = dt.Rows[i]["frequent_caller_id"];
                                                                        //objIds.rate_buck_amt6 = dt.Rows[i]["rate_buck_amt6"];
                                                                        //objIds.rate_chart_used = dt.Rows[i]["rate_chart_used"];
                                                                        if (dt.Columns.Contains("deliver_actual_pieces"))
                                                                        {
                                                                            objIds.deliver_actual_pieces = dt.Rows[i]["deliver_actual_pieces"];
                                                                        }

                                                                        //objIds.add_charge_code5 = dt.Rows[i]["add_charge_code5"];
                                                                        //objIds.pickup_omw_longitude = dt.Rows[i]["pickup_omw_longitude"];
                                                                        //objIds.delivery_point_customer = dt.Rows[i]["delivery_point_customer"];
                                                                        //objIds.add_charge_occur7 = dt.Rows[i]["add_charge_occur7"];
                                                                        //objIds.rate_buck_amt5 = dt.Rows[i]["rate_buck_amt5"];
                                                                        //objIds.fuel_update_freq_text = dt.Rows[i]["fuel_update_freq_text"];
                                                                        //objIds.fuel_update_freq = dt.Rows[i]["fuel_update_freq"];
                                                                        //objIds.add_charge_code11 = dt.Rows[i]["add_charge_code11"];
                                                                        if (dt.Columns.Contains("pickup_name"))
                                                                        {
                                                                            objIds.pickup_name = dt.Rows[i]["pickup_name"];
                                                                        }
                                                                        //objIds.callback_date = dt.Rows[i]["callback_date"];
                                                                        //objIds.add_charge_code2 = dt.Rows[i]["add_charge_code2"];
                                                                        //objIds.house_airway_bill_number = dt.Rows[i]["house_airway_bill_number"];
                                                                        if (dt.Columns.Contains("deliver_name"))
                                                                        {
                                                                            objIds.deliver_name = dt.Rows[i]["deliver_name"];
                                                                        }
                                                                        if (dt.Columns.Contains("number_of_pieces"))
                                                                        {
                                                                            objIds.number_of_pieces = dt.Rows[i]["number_of_pieces"];
                                                                        }
                                                                        //objIds.deliver_eta_time = dt.Rows[i]["deliver_eta_time"];
                                                                        //objIds.origin_code_text = dt.Rows[i]["origin_code_text"];
                                                                        //objIds.origin_code = dt.Rows[i]["origin_code"];
                                                                        //objIds.rate_special_instructions = dt.Rows[i]["rate_special_instructions"];
                                                                        //objIds.add_charge_occur3 = dt.Rows[i]["add_charge_occur3"];
                                                                        //objIds.pickup_eta_date = dt.Rows[i]["pickup_eta_date"];
                                                                        //objIds.deliver_special_instructions4 = dt.Rows[i]["deliver_special_instructions4"];
                                                                        //objIds.custom_special_instr_long = dt.Rows[i]["custom_special_instr_long"];
                                                                        //objIds.deliver_special_instructions2 = dt.Rows[i]["deliver_special_instructions2"];
                                                                        if (dt.Columns.Contains("pickup_signature"))
                                                                        {
                                                                            objIds.pickup_signature = dt.Rows[i]["pickup_signature"];
                                                                        }
                                                                        //objIds.az_equip1 = dt.Rows[i]["az_equip1"];
                                                                        //objIds.add_charge_amt12 = dt.Rows[i]["add_charge_amt12"];
                                                                        //objIds.calc_add_on_chgs = dt.Rows[i]["calc_add_on_chgs"];
                                                                        //objIds.original_schedule_number = dt.Rows[i]["original_schedule_number"];
                                                                        //objIds.blocks = dt.Rows[i]["blocks"];
                                                                        //objIds.del_actual_location_accuracy = dt.Rows[i]["del_actual_location_accuracy"];
                                                                        //objIds.zone_set_used = dt.Rows[i]["zone_set_used"];

                                                                        // objIds.pickup_country = dt.Rows[i]["pickup_country"];
                                                                        if (dt.Columns.Contains("pickup_state"))
                                                                        {
                                                                            objIds.pickup_state = dt.Rows[i]["pickup_state"];
                                                                        }

                                                                        //objIds.add_charge_amt7 = dt.Rows[i]["add_charge_amt7"];
                                                                        //objIds.email_addresses = dt.Rows[i]["email_addresses"];
                                                                        //objIds.add_charge_occur1 = dt.Rows[i]["add_charge_occur1"];
                                                                        //objIds.pickup_wait_time = dt.Rows[i]["pickup_wait_time"];
                                                                        //objIds.company_number_text = dt.Rows[i]["company_number_text"];
                                                                        if (dt.Columns.Contains("company_number"))
                                                                        {
                                                                            objIds.company_number = dt.Rows[i]["company_number"];
                                                                        }
                                                                        //objIds.distribution_branch_id = dt.Rows[i]["distribution_branch_id"];
                                                                        //objIds.rate_buck_amt9 = dt.Rows[i]["rate_buck_amt9"];
                                                                        //objIds.add_charge_amt1 = dt.Rows[i]["add_charge_amt1"];
                                                                        if (dt.Columns.Contains("pickup_requested_dep_time"))
                                                                        {
                                                                            objIds.pickup_requested_dep_time = dt.Rows[i]["pickup_requested_dep_time"];
                                                                        }
                                                                        //objIds.customer_type_text = dt.Rows[i]["customer_type_text"];
                                                                        //if (dt.Columns.Contains("customer_type_text"))
                                                                        //{
                                                                        //    objIds.customer_type_text = dt.Rows[i]["customer_type_text"];
                                                                        //}
                                                                        //objIds.customer_type = dt.Rows[i]["customer_type"];
                                                                        if (dt.Columns.Contains("deliver_state"))
                                                                        {
                                                                            objIds.deliver_state = dt.Rows[i]["deliver_state"];
                                                                        }
                                                                        //objIds.deliver_dispatch_zone = dt.Rows[i]["deliver_dispatch_zone"];
                                                                        //objIds.image_sign_req = dt.Rows[i]["image_sign_req"];
                                                                        //objIds.add_charge_code6 = dt.Rows[i]["add_charge_code6"];
                                                                        if (dt.Columns.Contains("deliver_requested_date"))
                                                                        {
                                                                            objIds.deliver_requested_date = dt.Rows[i]["deliver_requested_date"];
                                                                        }
                                                                        // objIds.add_charge_amt5 = dt.Rows[i]["add_charge_amt5"];
                                                                        if (dt.Columns.Contains("time_order_entered"))
                                                                        {
                                                                            objIds.time_order_entered = dt.Rows[i]["time_order_entered"];
                                                                        }
                                                                        //objIds.pick_del_trans_flag_text = dt.Rows[i]["pick_del_trans_flag_text"];
                                                                        //objIds.pick_del_trans_flag = dt.Rows[i]["pick_del_trans_flag"];
                                                                        //objIds.pickup_attention = dt.Rows[i]["pickup_attention"];
                                                                        //objIds.rate_buck_amt7 = dt.Rows[i]["rate_buck_amt7"];
                                                                        //objIds.add_charge_occur6 = dt.Rows[i]["add_charge_occur6"];
                                                                        //objIds.fuel_price_source = dt.Rows[i]["fuel_price_source"];
                                                                        //objIds.pickup_airport_code = dt.Rows[i]["pickup_airport_code"];
                                                                        //objIds.rate_buck_amt2 = dt.Rows[i]["rate_buck_amt2"];
                                                                        //objIds.rate_buck_amt3 = dt.Rows[i]["rate_buck_amt3"];
                                                                        //objIds.deliver_omw_timestamp = dt.Rows[i]["deliver_omw_timestamp"];
                                                                        //objIds.exception_code = dt.Rows[i]["exception_code"];
                                                                        //objIds.status_code_text = dt.Rows[i]["status_code_text"];
                                                                        //objIds.status_code = dt.Rows[i]["status_code"];
                                                                        //objIds.weight = dt.Rows[i]["weight"];
                                                                        //objIds.signature_required = dt.Rows[i]["signature_required"];
                                                                        //objIds.rate_buck_amt10 = dt.Rows[i]["rate_buck_amt10"];
                                                                        //objIds.hist_inv_number = dt.Rows[i]["hist_inv_number"];
                                                                        //objIds.deliver_pricing_zone = dt.Rows[i]["deliver_pricing_zone"];
                                                                        //objIds.pickup_actual_longitude = dt.Rows[i]["pickup_actual_longitude"];
                                                                        //objIds.push_services = dt.Rows[i]["push_services"];
                                                                        //objIds.add_charge_amt11 = dt.Rows[i]["add_charge_amt11"];
                                                                        //objIds.rt_actual_location_accuracy = dt.Rows[i]["rt_actual_location_accuracy"];
                                                                        //objIds.roundtrip_actual_date = dt.Rows[i]["roundtrip_actual_date"];
                                                                        if (dt.Columns.Contains("pickup_requested_arr_time"))
                                                                        {
                                                                            objIds.pickup_requested_arr_time = dt.Rows[i]["pickup_requested_arr_time"];
                                                                        }
                                                                        //objIds.deliver_attention = dt.Rows[i]["deliver_attention"];
                                                                        //objIds.deliver_special_instructions3 = dt.Rows[i]["deliver_special_instructions3"];
                                                                        //objIds.pickup_actual_pieces = dt.Rows[i]["pickup_actual_pieces"];
                                                                        //objIds.edi_order_accepted_or_rejected_text = dt.Rows[i]["edi_order_accepted_or_rejected_text"];
                                                                        //objIds.edi_order_accepted_or_rejected = dt.Rows[i]["edi_order_accepted_or_rejected"];
                                                                        //objIds.roundtrip_signature = dt.Rows[i]["roundtrip_signature"];
                                                                        //objIds.po_number = dt.Rows[i]["po_number"];
                                                                        if (dt.Columns.Contains("signature"))
                                                                        {
                                                                            objIds.signature = dt.Rows[i]["signature"];
                                                                        }
                                                                        //objIds.pickup_special_instructions2 = dt.Rows[i]["pickup_special_instructions2"];
                                                                        //objIds.original_ctrl_number = dt.Rows[i]["original_ctrl_number"];
                                                                        //objIds.previous_ctrl_number = dt.Rows[i]["previous_ctrl_number"];
                                                                        //if (dt.Columns.Contains("Id"))
                                                                        //{
                                                                        //    objIds.id = dt.Rows[i]["Id"];
                                                                        //}
                                                                        idList.Add(objIds);

                                                                    }
                                                                    objCommon.SaveOutputDataToCsvFileParallely(idList, "Order-Create",
                                                                       processingFileName, strDatetime);
                                                                }
                                                                if (dsOrderResponse.Tables.Contains("settlements"))
                                                                {
                                                                    List<Settlement> settelmentList = new List<Settlement>();
                                                                    for (int i = 0; i < dsOrderResponse.Tables["settlements"].Rows.Count; i++)
                                                                    {
                                                                        DataTable dt = dsOrderResponse.Tables["settlements"];
                                                                        Settlement objsettlements = new Settlement();
                                                                        //objsettlements.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                                        if (dt.Columns.Contains("company_number"))
                                                                        {
                                                                            objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                                        }
                                                                        //objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                                        //objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                                        //objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                                        //objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                                        //objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                                        //objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                                        if (dt.Columns.Contains("order_date"))
                                                                        {
                                                                            objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                                        }
                                                                        //objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                                        //objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                                        //objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                                        //objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                                        //objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                                        //objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                                        //objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                                        //objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                                        //objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                                        //objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                                        //objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                                        //objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                                        //objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                                        if (dt.Columns.Contains("driver_company_number"))
                                                                        {
                                                                            objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                                        }
                                                                        //objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                                        //objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                                        //objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                                        //objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                                        //objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                                        //objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                                        if (dt.Columns.Contains("id"))
                                                                        {
                                                                            objsettlements.id = (dt.Rows[i]["id"]);
                                                                        }
                                                                        if (dt.Columns.Contains("date_last_updated"))
                                                                        {
                                                                            objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                                        }
                                                                        //objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                                        //objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                                        //objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                                        //objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                                        if (dt.Columns.Contains("driver_number"))
                                                                        {
                                                                            objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                                        }
                                                                        //objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                                        //objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                                        //objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                                        //objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                                        //objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                                        //objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                                        //objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                                        //objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                                        //objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                                        //objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                                        if (dt.Columns.Contains("control_number"))
                                                                        {
                                                                            objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                                        }
                                                                        //objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                                        //objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                                        //objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                                        //objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);
                                                                        settelmentList.Add(objsettlements);
                                                                    }

                                                                    objCommon.SaveOutputDataToCsvFileParallely(settelmentList, "Order-Settlements-AddRecord",
                                                                        processingFileName, strDatetime);

                                                                }
                                                                if (dsOrderResponse.Tables.Contains("progress"))
                                                                {

                                                                    List<Progress> progressList = new List<Progress>();
                                                                    for (int i = 0; i < dsOrderResponse.Tables["progress"].Rows.Count; i++)
                                                                    {
                                                                        Progress progress = new Progress();
                                                                        DataTable dt = dsOrderResponse.Tables["progress"];
                                                                        if (dt.Columns.Contains("status_date"))
                                                                        {
                                                                            progress.status_date = (dt.Rows[i]["status_date"]);
                                                                        }
                                                                        if (dt.Columns.Contains("status_text"))
                                                                        {
                                                                            progress.status_text = (dt.Rows[i]["status_text"]);
                                                                        }
                                                                        if (dt.Columns.Contains("status_time"))
                                                                        {
                                                                            progress.status_time = (dt.Rows[i]["status_time"]);
                                                                        }
                                                                        if (dt.Columns.Contains("id"))
                                                                        {
                                                                            progress.id = (dt.Rows[i]["id"]);
                                                                        }
                                                                        progressList.Add(progress);
                                                                    }

                                                                    objCommon.SaveOutputDataToCsvFileParallely(progressList, "Order-Progress",
                                                                       processingFileName, strDatetime);
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                strExecutionLogMessage = "ProcessAddOrderFiles Exception -" + ex.Message + System.Environment.NewLine;
                                                                strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                                                strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                                                strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                                //objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                                                //objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                                objCommon.WriteErrorLogParallelly(ex, fileName, strExecutionLogMessage);

                                                                ErrorResponse objErrorResponse = new ErrorResponse();
                                                                objErrorResponse.error = ex.Message;
                                                                objErrorResponse.status = "Error";
                                                                objErrorResponse.code = "Exception while writing the response into csv";
                                                                objErrorResponse.reference = ReferenceId;
                                                                string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                                dsFailureResponse.Tables[0].TableName = "OrderFailure";
                                                                objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                            strInputFilePath, processingFileName, strDatetime);

                                                            }
                                                            //  if (driver1 != null) 
                                                            if (objOrder.driver1 != null)
                                                            {
                                                                if (dsOrderResponse.Tables.Contains("settlements"))
                                                                {
                                                                    UniqueId = Convert.ToString(dsOrderResponse.Tables["settlements"].Rows[0]["id"]);

                                                                    string ordersettlementputrequest = null;

                                                                    int company_number = Convert.ToInt32(dsOrderResponse.Tables[0].Rows[0]["company_number"]);
                                                                    int control_number = Convert.ToInt32(dsOrderResponse.Tables[0].Rows[0]["control_number"]);

                                                                    int record_type = Convert.ToInt32(objCommon.GetConfigValue("OrderSettlement_record_type"));
                                                                    string transaction_type = objCommon.GetConfigValue("OrderSettlement_transaction_type"); // 
                                                                    string driver_sequence = objCommon.GetConfigValue("OrderSettlement_driver_sequence");

                                                                    ordersettlementputrequest = @"'company_number': " + company_number + ",";
                                                                    ordersettlementputrequest = ordersettlementputrequest + @"'control_number': " + control_number + ",";

                                                                    if (record_type != null)
                                                                    {
                                                                        ordersettlementputrequest = ordersettlementputrequest + @"'record_type': " + record_type + ",";
                                                                    }

                                                                    if (drow.Table.Columns.Contains("Transaction Type"))
                                                                    {
                                                                        if (!string.IsNullOrEmpty(Convert.ToString(drow["Transaction Type"])))
                                                                        {
                                                                            transaction_type = Convert.ToString(drow["Transaction Type"]);
                                                                        }
                                                                    }

                                                                    ordersettlementputrequest = ordersettlementputrequest + @"'transaction_type': '" + transaction_type + "',";

                                                                    if (driver_sequence != null)
                                                                    {
                                                                        ordersettlementputrequest = ordersettlementputrequest + @"'driver_sequence': '" + driver_sequence + "',";
                                                                    }
                                                                    else
                                                                    {
                                                                        driver_sequence = "0";
                                                                        ordersettlementputrequest = ordersettlementputrequest + @"'driver_sequence': '" + driver_sequence + "',";
                                                                    }
                                                                    if (drow.Table.Columns.Contains("Carrier Base Pay"))
                                                                    {
                                                                        if (!string.IsNullOrEmpty(Convert.ToString(drow["Carrier Base Pay"])))
                                                                        {
                                                                            ordersettlementputrequest = ordersettlementputrequest + @"'charge1': " + drow["Carrier Base Pay"] + ",";
                                                                        }
                                                                        else
                                                                        {
                                                                            strExecutionLogMessage = "OrderPost-OrderSettlementPut Error " + System.Environment.NewLine;
                                                                            strExecutionLogMessage += "Carrier Base Pay Not found in the file -" + strFileName + System.Environment.NewLine;
                                                                            strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                                            //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                                                            objErrorResponse.error = "Carrier Base Pay Value Missing for this record";
                                                                            objErrorResponse.status = "Error";
                                                                            objErrorResponse.code = "Carrier Base Pay Value Missing";
                                                                            objErrorResponse.reference = ReferenceId;
                                                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                                            dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                                            objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                                        strInputFilePath, processingFileName, strDatetime);
                                                                            continue;
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        strExecutionLogMessage = "OrderPost-OrderSettlementPut Error " + System.Environment.NewLine;
                                                                        strExecutionLogMessage += "Carrier Base Pay Not found in the file -" + strFileName + System.Environment.NewLine;
                                                                        strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                                        objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                                        // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                                                        ErrorResponse objErrorResponse = new ErrorResponse();
                                                                        objErrorResponse.error = "Carrier Base Pay column not found for this record";
                                                                        objErrorResponse.status = "Error";
                                                                        objErrorResponse.code = "Carrier Base Pay column Missing";
                                                                        objErrorResponse.reference = ReferenceId;
                                                                        string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                                        DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                                        dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                                        objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                                    strInputFilePath, processingFileName, strDatetime);
                                                                        continue;

                                                                    }
                                                                    if (drow.Table.Columns.Contains("Carrier ACC"))
                                                                    {
                                                                        if (!string.IsNullOrEmpty(Convert.ToString(drow["Carrier ACC"])))
                                                                        {
                                                                            ordersettlementputrequest = ordersettlementputrequest + @"'charge5': " + drow["Carrier ACC"] + ",";
                                                                        }
                                                                        else
                                                                        {
                                                                            strExecutionLogMessage = "OrderPost-OrderSettlementPut Error " + System.Environment.NewLine;
                                                                            strExecutionLogMessage += "Carrier ACC Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                                            strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                                                            objErrorResponse.error = "Carrier ACC value not found for this record";
                                                                            objErrorResponse.status = "Error";
                                                                            objErrorResponse.code = "Carrier ACC value Missing";
                                                                            objErrorResponse.reference = ReferenceId;
                                                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                                            dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                                            objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                                        strInputFilePath, processingFileName, strDatetime);
                                                                            continue;
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        strExecutionLogMessage = "OrderPost-OrderSettlementPut Error " + System.Environment.NewLine;
                                                                        strExecutionLogMessage += "Carrier ACC Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                                        strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                                        objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                                        // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                                        ErrorResponse objErrorResponse = new ErrorResponse();
                                                                        objErrorResponse.error = "Carrier ACC column not found for this record";
                                                                        objErrorResponse.status = "Error";
                                                                        objErrorResponse.code = "Carrier ACC column Missing";
                                                                        objErrorResponse.reference = ReferenceId;
                                                                        string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                                        DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                                        dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                                        objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                                    strInputFilePath, processingFileName, strDatetime);
                                                                        continue;
                                                                    }

                                                                    if (dr.Table.Columns.Contains("Carrier FSC"))
                                                                    {
                                                                        if (!string.IsNullOrEmpty(Convert.ToString(dr["Carrier FSC"])))
                                                                        {
                                                                            ordersettlementputrequest = ordersettlementputrequest + @"'charge6': " + Convert.ToDouble(dr["Carrier FSC"]) + ",";
                                                                        }
                                                                    }

                                                                    ordersettlementputrequest = @"{" + ordersettlementputrequest + "}";
                                                                    string order_settlementObject = @"{'order_settlement': " + ordersettlementputrequest + "}";
                                                                    JObject jsonobj = JObject.Parse(order_settlementObject);
                                                                    request = jsonobj.ToString();

                                                                    clsCommon.ReturnResponse objresponseOrdersettlement = new clsCommon.ReturnResponse();
                                                                    objresponseOrdersettlement = objclsDatatrac.CallDataTracOrderSettlementPutAPI(UniqueId, order_settlementObject, objorderdetails.order.csr);
                                                                    // objresponseOrdersettlement.Reason = "{\"002018724450D1\": {\"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"time_last_updated\": \"05:10\", \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"order_date\": \"2021-06-28\", \"agent_etrac_transaction_number\": null, \"settlement_bucket1_pct\": null, \"settlement_bucket2_pct\": null, \"vendor_employee_numer\": null, \"fuel_price_zone\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"charge1\": 35.91, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"adjustment_type\": null, \"agents_etrac_partner_id\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"vendor_invoice_number\": null, \"charge6\": null, \"settlement_bucket4_pct\": null, \"fuel_plan\": null, \"record_type\": 0, \"voucher_amount\": null, \"id\": \"002018724450D1\", \"date_last_updated\": \"2021-07-08\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"voucher_number\": null, \"driver_number_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver_number\": 3208, \"settlement_bucket6_pct\": null, \"settlement_pct\": 100.00, \"charge3\": null, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"voucher_date\": null, \"fuel_price_source\": null, \"pay_chart_used\": null, \"settlement_bucket5_pct\": null, \"charge2\": null, \"control_number\": 1872445, \"settlement_bucket3_pct\": null, \"charge5\": null, \"pre_book_percentage\": true, \"settlement_period_end_date\": null}}";
                                                                    // objresponseOrdersettlement.ResponseVal = true;
                                                                    if (objresponseOrdersettlement.ResponseVal)
                                                                    {
                                                                        strExecutionLogMessage = "OrderPost-OrderSettlementPutAPI Success " + System.Environment.NewLine;
                                                                        strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                                        strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                                                        objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                                        //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                                                        DataSet dsOrderSettlementResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                                                                        dsOrderSettlementResponse.Tables[0].TableName = "OrderSettlement";
                                                                        try
                                                                        {
                                                                            List<ResponseOrderSettlements> orderSettlementstList = new List<ResponseOrderSettlements>();
                                                                            for (int i = 0; i < dsOrderSettlementResponse.Tables["OrderSettlement"].Rows.Count; i++)
                                                                            {
                                                                                DataTable dt = dsOrderSettlementResponse.Tables["OrderSettlement"];
                                                                                ResponseOrderSettlements objsettlements = new ResponseOrderSettlements();
                                                                                //objsettlements.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                                                if (dt.Columns.Contains("company_number"))
                                                                                {
                                                                                    objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                                                }
                                                                                //objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                                                //objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                                                //objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                                                //objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                                                //objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                                                //objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                                                if (dt.Columns.Contains("order_date"))
                                                                                {
                                                                                    objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                                                }
                                                                                //objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                                                //objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                                                //objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                                                //objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                                                //objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                                                //objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                                                //objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                                                //objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                                                //objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                                                //objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                                                //objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                                                //objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                                                //objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                                                if (dt.Columns.Contains("driver_company_number"))
                                                                                {
                                                                                    objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                                                }
                                                                                //objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                                                //objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                                                //objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                                                //objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                                                //objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                                                //objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                                                if (dt.Columns.Contains("id"))
                                                                                {
                                                                                    objsettlements.id = (dt.Rows[i]["id"]);
                                                                                }
                                                                                if (dt.Columns.Contains("date_last_updated"))
                                                                                {
                                                                                    objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                                                }
                                                                                //objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                                                //objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                                                //objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                                                //objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                                                if (dt.Columns.Contains("driver_number"))
                                                                                {
                                                                                    objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                                                }
                                                                                //objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                                                //objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                                                //objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                                                //objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                                                //objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                                                //objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                                                //objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                                                //objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                                                //objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                                                //objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                                                if (dt.Columns.Contains("control_number"))
                                                                                {
                                                                                    objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                                                }
                                                                                //objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                                                //objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                                                //objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                                                //objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);


                                                                                //objsettlements.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                                                if (dt.Columns.Contains("company_number"))
                                                                                {
                                                                                    objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                                                }
                                                                                //objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                                                //objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                                                //objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                                                //objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                                                //objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                                                //objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                                                if (dt.Columns.Contains("order_date"))
                                                                                {
                                                                                    objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                                                }
                                                                                //objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                                                //objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                                                //objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                                                //objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                                                //objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                                                //objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                                                //objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                                                //objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                                                //objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                                                //objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                                                //objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                                                //objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                                                //objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                                                if (dt.Columns.Contains("driver_company_number"))
                                                                                {
                                                                                    objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                                                }
                                                                                //objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                                                //objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                                                //objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                                                //objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                                                //objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                                                //objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                                                if (dt.Columns.Contains("id"))
                                                                                {
                                                                                    objsettlements.id = (dt.Rows[i]["id"]);
                                                                                }
                                                                                if (dt.Columns.Contains("date_last_updated"))
                                                                                {
                                                                                    objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                                                }
                                                                                //objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                                                //objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                                                //objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                                                //objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                                                if (dt.Columns.Contains("driver_number"))
                                                                                {
                                                                                    objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                                                }
                                                                                //objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                                                //objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                                                //objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                                                //objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                                                //objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                                                //objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                                                //objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                                                //objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                                                //objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                                                //objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                                                if (dt.Columns.Contains("control_number"))
                                                                                {
                                                                                    objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                                                }
                                                                                //objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                                                //objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                                                //objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                                                //objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);
                                                                                orderSettlementstList.Add(objsettlements);
                                                                            }

                                                                            objCommon.SaveOutputDataToCsvFileParallely(orderSettlementstList, "OrderSettlements-UpdatedRecord",
                                                                                processingFileName, strDatetime);
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            strExecutionLogMessage = "OrderPost-OrderSettlementPutAPI Success Exception -" + ex.Message + System.Environment.NewLine;
                                                                            strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                                                            strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                                                            strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                                            //objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                                                            //objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                                            objCommon.WriteErrorLogParallelly(ex, fileName, strExecutionLogMessage);

                                                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                                                            objErrorResponse.error = ex.Message;
                                                                            objErrorResponse.status = "Error";
                                                                            objErrorResponse.code = "Exception while writing OrderPost-OrderSettlementPutAPI Success response into csv";
                                                                            objErrorResponse.reference = ReferenceId;
                                                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                                            dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                                            objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                                        strInputFilePath, processingFileName, strDatetime);
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        strExecutionLogMessage = "OrderPost-OrderSettlementPutAPI Failed " + System.Environment.NewLine;
                                                                        strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                                        strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                                                        objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                                        // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                                        DataSet dsOrderPutFailureResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                                                                        dsOrderPutFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                                        dsOrderPutFailureResponse.Tables[0].Columns.Add("UniqueId", typeof(System.String));
                                                                        foreach (DataRow row in dsOrderPutFailureResponse.Tables[0].Rows)
                                                                        {
                                                                            row["UniqueId"] = UniqueId;
                                                                        }
                                                                        objCommon.WriteDataToCsvFileParallely(dsOrderPutFailureResponse.Tables[0],
                                                                        strInputFilePath, processingFileName, strDatetime);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            request = JsonConvert.SerializeObject(objorderdetails);
                                                            data = Regex.Replace(request, @"\\t", "");
                                                            request = Regex.Replace(data, @"\\""", "");
                                                            strExecutionLogMessage = "OrderPostAPI Failed " + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(objresponse.Reason);
                                                            dsFailureResponse.Tables[0].TableName = "OrderFailure";
                                                            dsFailureResponse.Tables[0].Columns.Add("Customer Reference", typeof(System.String));
                                                            foreach (DataRow row in dsFailureResponse.Tables[0].Rows)
                                                            {
                                                                row["Customer Reference"] = objOrder.reference;
                                                            }
                                                            objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                        strInputFilePath, processingFileName, strDatetime);

                                                        }

                                                    }
                                                }
                                            }



                                        }
                                        else
                                        {

                                            objOrder.line_items = objorder_line_itemList;
                                            objOrder.company_number = Convert.ToInt32(dr["Company"]);
                                            objOrder.service_level = Convert.ToInt32(dr["Service Type"]);
                                            objOrder.customer_number = Convert.ToInt32(dr["Billing Customer Number"]);
                                            objOrder.reference = Convert.ToString(dr["Customer Reference"]);
                                            //  DateTime dtValue = Convert.ToDateTime(dr["Delivery Date"]);

                                            if (dr.Table.Columns.Contains("BOL Number"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["BOL Number"])))
                                                {
                                                    objOrder.bol_number = Convert.ToString(dr["BOL Number"]);
                                                }
                                            }

                                            if (dr.Table.Columns.Contains("Delivery Date"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Delivery Date"])))
                                                {
                                                    DateTime dtValue = Convert.ToDateTime(dr["Delivery Date"]);
                                                    objOrder.pickup_requested_date = dtValue.ToString("yyyy-MM-dd");
                                                    objOrder.pickup_actual_date = dtValue.ToString("yyyy-MM-dd");
                                                    objOrder.deliver_requested_date = dtValue.ToString("yyyy-MM-dd");
                                                    objOrder.deliver_actual_date = dtValue.ToString("yyyy-MM-dd");
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Pickup actual arrival time"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup actual arrival time"])))
                                                {
                                                    DateTime dtValue = Convert.ToDateTime(dr["Pickup actual arrival time"]);
                                                    objOrder.pickup_actual_arr_time = dtValue.ToString("HH:mm");
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Pickup actual depart time"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup actual depart time"])))
                                                {
                                                    DateTime dtValue = Convert.ToDateTime(dr["Pickup actual depart time"]);
                                                    objOrder.pickup_actual_dep_time = dtValue.ToString("HH:mm");
                                                }
                                            }
                                            //    dtValue = Convert.ToDateTime(dr["Pickup actual depart time"]);
                                            //  objOrder.pickup_actual_dep_time = dtValue.ToString("HH:mm");
                                            if (dr.Table.Columns.Contains("Pickup no later than"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup no later than"])))
                                                {
                                                    DateTime dtValue = Convert.ToDateTime(dr["Pickup no later than"]);
                                                    objOrder.pickup_requested_dep_time = dtValue.ToString("HH:mm");
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Pickup name"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup name"])))
                                                {
                                                    objOrder.pickup_name = Convert.ToString(dr["Pickup name"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Pickup address"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup address"])))
                                                {
                                                    objOrder.pickup_address = Convert.ToString(dr["Pickup address"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Pickup city"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup city"])))
                                                {
                                                    objOrder.pickup_city = Convert.ToString(dr["Pickup city"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Pickup state/province"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup state/province"])))
                                                {
                                                    objOrder.pickup_state = Convert.ToString(dr["Pickup state/province"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Pickup zip/postal code"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup zip/postal code"])))
                                                {
                                                    // objOrder.pickup_zip = Convert.ToString(dr["Pickup zip/postal code"]);

                                                    string strZip = Convert.ToString(dr["Pickup zip/postal code"]);
                                                    strZip = Regex.Replace(strZip, @"\t", "");
                                                    if (strZip.Length > 5)
                                                    {
                                                        objOrder.pickup_zip = strZip.Substring(0, 5) + "-" + strZip.Substring(5, strZip.Length - 5); ;
                                                    }
                                                    else
                                                    {
                                                        objOrder.pickup_zip = Convert.ToString(dr["Pickup zip/postal code"]);
                                                    }
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Deliver no earlier than"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Deliver no earlier than"])))
                                                {
                                                    DateTime dtValue = Convert.ToDateTime(dr["Deliver no earlier than"]);
                                                    objOrder.deliver_requested_arr_time = dtValue.ToString("HH:mm");
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Deliver no later than"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Deliver no later than"])))
                                                {
                                                    DateTime dtValue = Convert.ToDateTime(dr["Deliver no later than"]);
                                                    objOrder.deliver_requested_dep_time = dtValue.ToString("HH:mm");
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Delivery actual arrive time"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Delivery actual arrive time"])))
                                                {
                                                    DateTime dtValue = Convert.ToDateTime(dr["Delivery actual arrive time"]);
                                                    objOrder.deliver_actual_arr_time = dtValue.ToString("HH:mm");
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Delivery actual depart time"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Delivery actual depart time"])))
                                                {
                                                    DateTime dtValue = Convert.ToDateTime(dr["Delivery actual depart time"]);
                                                    objOrder.deliver_actual_dep_time = dtValue.ToString("HH:mm");
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Customer Name"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Customer Name"])))
                                                {
                                                    objOrder.deliver_name = Convert.ToString(dr["Customer Name"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Address"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Address"])))
                                                {
                                                    objOrder.deliver_address = Convert.ToString(dr["Address"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("City"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["City"])))
                                                {
                                                    objOrder.deliver_city = Convert.ToString(dr["City"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("State"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["State"])))
                                                {
                                                    objOrder.deliver_state = Convert.ToString(dr["State"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Zip"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Zip"])))
                                                {
                                                    string strZip = Convert.ToString(dr["Zip"]);
                                                    strZip = Regex.Replace(strZip, @"\t", "");
                                                    if (strZip.Length > 5)
                                                    {
                                                        objOrder.deliver_zip = strZip.Substring(0, 5) + "-" + strZip.Substring(5, strZip.Length - 5); ;
                                                    }
                                                    else
                                                    {
                                                        objOrder.deliver_zip = Convert.ToString(dr["Zip"]);
                                                    }
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Delivery text signature"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Delivery text signature"])))
                                                {
                                                    objOrder.signature = Convert.ToString(dr["Delivery text signature"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Bill Rate"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Bill Rate"])))
                                                {
                                                    objOrder.rate_buck_amt1 = Convert.ToDouble(dr["Bill Rate"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Pieces ACC"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Pieces ACC"])))
                                                {
                                                    objOrder.rate_buck_amt3 = Convert.ToDouble(dr["Pieces ACC"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("FSC"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["FSC"])))
                                                {
                                                    objOrder.rate_buck_amt10 = Convert.ToDouble(dr["FSC"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Pieces"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Pieces"])))
                                                {
                                                    objOrder.number_of_pieces = Convert.ToInt32(dr["Pieces"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Miles"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Miles"])))
                                                {
                                                    objOrder.rate_miles = Convert.ToInt32(Convert.ToDouble(dr["Miles"]));
                                                }
                                            }
                                            //    string driver1 = null;
                                            if (dr.Table.Columns.Contains("Correct Driver Number"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Correct Driver Number"])))
                                                {
                                                    objOrder.driver1 = Convert.ToInt32(dr["Correct Driver Number"]);
                                                    //driver1 = Convert.ToString(dr["Correct Driver Number"]);
                                                }

                                            }
                                            //if (dr.Table.Columns.Contains("Requested by"))
                                            //{
                                            //    if (!string.IsNullOrEmpty(Convert.ToString(dr["Requested by"])))
                                            //    {
                                            //        objOrder.ordered_by = Convert.ToString(dr["Requested by"]);
                                            //    }
                                            //}

                                            objOrder.ordered_by = Convert.ToString(dr["Requested by"]);
                                            objOrder.csr = Convert.ToString(dr["Entered by"]);
                                            if (dr.Table.Columns.Contains("Pickup Delivery Transfer Flag"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup Delivery Transfer Flag"])))
                                                {
                                                    objOrder.pick_del_trans_flag = Convert.ToString(dr["Pickup Delivery Transfer Flag"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Pickup text signature"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup text signature"])))
                                                {
                                                    objOrder.pickup_signature = Convert.ToString(dr["Pickup text signature"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Weight"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Weight"])))
                                                {
                                                    objOrder.weight = Convert.ToInt32(Convert.ToDouble(dr["Weight"]));
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Insurance Amount"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Insurance Amount"])))
                                                {
                                                    objOrder.insurance_amount = Convert.ToInt32(Convert.ToDouble(dr["Insurance Amount"]));
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Master airway bill number"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Master airway bill number"])))
                                                {
                                                    objOrder.master_airway_bill_number = Convert.ToString(dr["Master airway bill number"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("PO Number"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["PO Number"])))
                                                {
                                                    objOrder.po_number = Convert.ToString(dr["PO Number"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("House airway bill number"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["House airway bill number"])))
                                                {
                                                    objOrder.house_airway_bill_number = Convert.ToString(dr["House airway bill number"]);
                                                }
                                            }

                                            if (dr.Table.Columns.Contains("Delivery Phone"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Delivery Phone"])))
                                                {
                                                    objOrder.deliver_phone = Convert.ToString(dr["Delivery Phone"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Pickup Room"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup Room"])))
                                                {
                                                    objOrder.pickup_room = Convert.ToString(dr["Pickup Room"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Pickup Attention"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup Attention"])))
                                                {
                                                    objOrder.pickup_attention = Convert.ToString(dr["Pickup Attention"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("Deliver Attention"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["Deliver Attention"])))
                                                {
                                                    objOrder.deliver_attention = Convert.ToString(dr["Deliver Attention"]);
                                                }
                                            }
                                            objorderdetails.order = objOrder;
                                            clsDatatrac objclsDatatrac = new clsDatatrac();
                                            clsCommon.ReturnResponse objresponse = new clsCommon.ReturnResponse();
                                            string request = JsonConvert.SerializeObject(objorderdetails);
                                            string data = Regex.Replace(request, @"\\t", "");
                                            request = Regex.Replace(data, @"\\""", "");
                                            objresponse = objclsDatatrac.CallDataTracOrderPostAPI(objorderdetails);
                                            //objresponse.ResponseVal = true;
                                            //objresponse.Reason = "{\"002018775030\": {\"verified_weight\": null, \"roundtrip_actual_latitude\": null, \"pickup_special_instructions4\": null, \"fuel_miles\": null, \"pickup_dispatch_zone\": null, \"pickup_zip\": \"23150\", \"pickup_actual_arr_time\": \"08:00\", \"cod_accept_company_check\": false, \"add_charge_occur9\": null, \"pickup_omw_latitude\": null, \"service_level_text\": \"Threshold Delivery\", \"service_level\": 57, \"exception_sign_required\": false, \"pickup_phone_ext\": null, \"roundtrip_actual_pieces\": null, \"bringg_send_sms\": false, \"az_equip2\": null, \"hist_inv_date\": null, \"date_order_entered\": \"2021-07-21\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"pickup_city\": \"SANDSTON\", \"pickup_phone\": null, \"pickup_sign_req\": true, \"deliver_route_sequence\": null, \"deliver_phone\": null, \"deliver_omw_longitude\": null, \"roundtrip_actual_longitude\": null, \"page_number\": 1, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"add_charge_code9\": null, \"pickup_eta_time\": null, \"record_type\": 0, \"add_charge_occur11\": null, \"push_partner_order_id\": null, \"deliver_country\": null, \"customer_name\": \"MXD/RYDER\", \"bol_number\": null, \"pickup_latitude\": 37.53250820, \"add_charge_code4\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"pu_arrive_notification_sent\": false, \"distribution_shift_id\": null, \"pickup_special_instr_long\": null, \"id\": \"002018775030\", \"callback_to\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"ordered_by\": \"RYDER\", \"add_charge_code12\": null, \"pickup_route_seq\": null, \"deliver_city\": \"HANOVER\", \"add_charge_occur5\": null, \"edi_acknowledgement_required\": false, \"rescheduled_ctrl_number\": null, \"driver2\": null, \"deliver_room\": null, \"deliver_actual_arr_time\": \"08:00\", \"fuel_price_zone\": null, \"add_charge_amt9\": null, \"add_charge_amt4\": null, \"delivery_address_point_number_text\": \"PAMELA YORK\", \"delivery_address_point_number\": 24254, \"deliver_actual_longitude\": null, \"add_charge_amt2\": null, \"additional_drivers\": false, \"pickup_pricing_zone\": 1, \"hazmat\": false, \"pickup_address\": \"540 EASTPARK CT\", \"pickup_route_code\": null, \"callback_userid\": null, \"pickup_point_customer\": 31025, \"rate_buck_amt1\": 52.00, \"add_charge_amt8\": null, \"callback_time\": null, \"csr\": \"DX*\", \"roundtrip_actual_depart_time\": null, \"customers_etrac_partner_id\": \"96609250\", \"manual_notepad\": false, \"add_charge_code8\": null, \"bringg_order_id\": null, \"deliver_omw_latitude\": null, \"pickup_longitude\": -77.33035820, \"etrac_number\": null, \"distribution_unique_id\": 0, \"vehicle_type\": null, \"roundtrip_actual_arrival_time\": null, \"delivery_longitude\": -77.28769600, \"pu_actual_location_accuracy\": null, \"deliver_actual_date\": \"2021-07-14\", \"exception_timestamp\": null, \"deliver_zip\": \"23069\", \"roundtrip_wait_time\": null, \"add_charge_occur8\": null, \"dl_arrive_notification_sent\": false, \"pickup_special_instructions1\": null, \"ordered_by_phone_number\": null, \"deliver_requested_arr_time\": \"08:00\", \"rate_miles\": null, \"holiday_groups\": null, \"pickup_email_notification_sent\": false, \"add_charge_code3\": null, \"dispatch_id\": null, \"add_charge_occur10\": null, \"dispatch_time\": null, \"deliver_wait_time\": null, \"invoice_period_end_date\": null, \"add_charge_occur12\": null, \"fuel_plan\": null, \"return_svc_level\": null, \"pickup_actual_date\": \"2021-07-14\", \"send_new_order_alert\": false, \"pickup_room\": null, \"rate_buck_amt8\": null, \"add_charge_amt10\": null, \"insurance_amount\": null, \"add_charge_amt3\": null, \"add_charge_amt6\": null, \"pickup_special_instructions3\": null, \"pickup_requested_date\": \"2021-07-14\", \"roundtrip_sign_req\": false, \"actual_miles\": null, \"pickup_address_point_number_text\": \"1STOPBEDROOMS\", \"pickup_address_point_number\": 19845, \"deliver_actual_latitude\": null, \"deliver_phone_ext\": null, \"deliver_route_code\": null, \"add_charge_code10\": null, \"delivery_airport_code\": null, \"reference\": null, \"photos_exist\": false, \"master_airway_bill_number\": null, \"control_number\": 1877503, \"cod_text\": \"No\", \"cod\": \"N\", \"rate_buck_amt11\": null, \"cod_amount\": null, \"pickup_omw_timestamp\": null, \"signature_images\": [], \"deliver_special_instructions1\": null, \"quote_amount\": null, \"total_pages\": 1, \"rate_buck_amt4\": null, \"line_items\": [], \"delivery_latitude\": 37.80844290, \"progress\": [{\"status_time\": \"15:57:00\", \"status_date\": \"2021-07-21\", \"status_text\": \"Entered in carrier's system\"}, {\"status_time\": \"08:30:00\", \"status_date\": \"2021-07-14\", \"status_text\": \"Picked up\"}, {\"status_time\": \"08:15:00\", \"status_date\": \"2021-07-14\", \"status_text\": \"Delivered\"}], \"add_charge_code1\": null, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"deliver_special_instr_long\": null, \"deliver_address\": \"205 HARDWOOD LN\", \"add_charge_occur4\": null, \"deliver_eta_date\": null, \"pickup_actual_dep_time\": \"08:30\", \"deliver_requested_dep_time\": \"17:00\", \"add_charge_code7\": null, \"deliver_actual_dep_time\": \"08:15\", \"cod_accept_cashiers_check\": false, \"bringg_last_loc_sent\": null, \"az_equip3\": null, \"driver1_text\": \"WALT DARBY\", \"driver1\": 3215, \"pickup_actual_latitude\": null, \"add_charge_occur2\": null, \"order_automatically_quoted\": false, \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"hours\": \"15\", \"frequent_caller_id\": null, \"rate_buck_amt6\": null, \"rate_chart_used\": 0, \"deliver_actual_pieces\": null, \"add_charge_code5\": null, \"pickup_omw_longitude\": null, \"notes\": [], \"delivery_point_customer\": 31025, \"add_charge_occur7\": null, \"rate_buck_amt5\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"add_charge_code11\": null, \"pickup_name\": \"1STOPBEDROOMS\", \"callback_date\": null, \"add_charge_code2\": null, \"house_airway_bill_number\": null, \"deliver_name\": \"PAMELA YORK\", \"number_of_pieces\": 1, \"deliver_eta_time\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"rate_special_instructions\": null, \"add_charge_occur3\": null, \"pickup_eta_date\": null, \"deliver_special_instructions4\": null, \"custom_special_instr_long\": null, \"deliver_special_instructions2\": null, \"pickup_signature\": \"SOF\", \"az_equip1\": null, \"add_charge_amt12\": null, \"settlements\": [{\"vendor_invoice_number\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"voucher_date\": null, \"voucher_amount\": null, \"fuel_price_source\": null, \"settlement_bucket6_pct\": null, \"pre_book_percentage\": true, \"settlement_bucket4_pct\": null, \"settlement_period_end_date\": null, \"charge5\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"settlement_bucket5_pct\": null, \"voucher_number\": null, \"charge6\": null, \"settlement_bucket2_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"settlement_pct\": 100.00, \"vendor_employee_numer\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"driver_number_text\": \"WALT DARBY\", \"driver_number\": 3215, \"fuel_plan\": null, \"agents_etrac_partner_id\": null, \"charge2\": null, \"charge3\": null, \"settlement_bucket1_pct\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"id\": \"002018775030D1\", \"record_type\": 0, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"pay_chart_used\": null, \"fuel_price_zone\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"charge1\": null, \"control_number\": 1877503, \"adjustment_type\": null, \"order_date\": \"2021-07-14\", \"time_last_updated\": \"14:57\", \"agent_etrac_transaction_number\": null, \"date_last_updated\": \"2021-07-21\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"settlement_bucket3_pct\": null}], \"calc_add_on_chgs\": false, \"original_schedule_number\": null, \"blocks\": null, \"del_actual_location_accuracy\": null, \"zone_set_used\": 1, \"pickup_country\": null, \"pickup_state\": \"VA\", \"add_charge_amt7\": null, \"email_addresses\": null, \"add_charge_occur1\": null, \"pickup_wait_time\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"distribution_branch_id\": null, \"rate_buck_amt9\": null, \"add_charge_amt1\": null, \"pickup_requested_dep_time\": \"09:00\", \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"deliver_state\": \"VA\", \"deliver_dispatch_zone\": null, \"image_sign_req\": false, \"add_charge_code6\": null, \"deliver_requested_date\": \"2021-07-14\", \"add_charge_amt5\": null, \"time_order_entered\": \"15:57\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"pickup_attention\": null, \"rate_buck_amt7\": null, \"add_charge_occur6\": null, \"fuel_price_source\": null, \"pickup_airport_code\": null, \"rate_buck_amt2\": null, \"rate_buck_amt3\": null, \"deliver_omw_timestamp\": null, \"exception_code\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"weight\": null, \"signature_required\": true, \"rate_buck_amt10\": null, \"hist_inv_number\": 0, \"deliver_pricing_zone\": 1, \"pickup_actual_longitude\": null, \"push_services\": null, \"add_charge_amt11\": null, \"rt_actual_location_accuracy\": null, \"roundtrip_actual_date\": null, \"pickup_requested_arr_time\": null, \"deliver_attention\": null, \"deliver_special_instructions3\": null, \"pickup_actual_pieces\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"roundtrip_signature\": null, \"po_number\": null, \"signature\": \"SOF\", \"pickup_special_instructions2\": null, \"original_ctrl_number\": null, \"previous_ctrl_number\": null}}";
                                            // objresponse.Reason = "{\"002018724440\": {\"verified_weight\": null, \"roundtrip_actual_latitude\": null, \"pickup_special_instructions4\": null, \"fuel_miles\": null, \"pickup_dispatch_zone\": null, \"pickup_zip\": \"23150\", \"pickup_actual_arr_time\": \"08:00\", \"cod_accept_company_check\": false, \"add_charge_occur9\": null, \"pickup_omw_latitude\": null, \"service_level_text\": \"Room of Choice\", \"service_level\": 55, \"exception_sign_required\": false, \"pickup_phone_ext\": null, \"roundtrip_actual_pieces\": null, \"bringg_send_sms\": false, \"az_equip2\": null, \"hist_inv_date\": null, \"date_order_entered\": \"2021-07-08\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"pickup_city\": \"SANDSTON\", \"pickup_phone\": null, \"pickup_sign_req\": true, \"deliver_route_sequence\": null, \"deliver_phone\": null, \"deliver_omw_longitude\": null, \"roundtrip_actual_longitude\": null, \"page_number\": 1, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"add_charge_code9\": null, \"pickup_eta_time\": null, \"record_type\": 0, \"add_charge_occur11\": null, \"push_partner_order_id\": null, \"deliver_country\": null, \"customer_name\": \"MXD/RYDER\", \"bol_number\": null, \"pickup_latitude\": 37.53250820, \"add_charge_code4\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"pu_arrive_notification_sent\": false, \"distribution_shift_id\": null, \"pickup_special_instr_long\": null, \"id\": \"002018724440\", \"callback_to\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"ordered_by\": \"RYDER\", \"add_charge_code12\": null, \"pickup_route_seq\": null, \"deliver_city\": \"CHESAPEAKE\", \"add_charge_occur5\": null, \"edi_acknowledgement_required\": false, \"rescheduled_ctrl_number\": null, \"driver2\": null, \"deliver_room\": null, \"deliver_actual_arr_time\": \"08:00\", \"fuel_price_zone\": null, \"add_charge_amt9\": null, \"add_charge_amt4\": null, \"delivery_address_point_number_text\": \"ANNE BAILEY\", \"delivery_address_point_number\": 26312, \"deliver_actual_longitude\": null, \"add_charge_amt2\": null, \"additional_drivers\": false, \"pickup_pricing_zone\": 1, \"hazmat\": false, \"pickup_address\": \"540 EASTPARK CT\", \"pickup_route_code\": null, \"callback_userid\": null, \"pickup_point_customer\": 31025, \"rate_buck_amt1\": 57.00, \"add_charge_amt8\": null, \"callback_time\": null, \"csr\": \"DX*\", \"roundtrip_actual_depart_time\": null, \"customers_etrac_partner_id\": \"96609250\", \"manual_notepad\": false, \"add_charge_code8\": null, \"bringg_order_id\": null, \"deliver_omw_latitude\": null, \"pickup_longitude\": -77.33035820, \"etrac_number\": null, \"distribution_unique_id\": 0, \"vehicle_type\": null, \"roundtrip_actual_arrival_time\": null, \"delivery_longitude\": -76.34760620, \"pu_actual_location_accuracy\": null, \"deliver_actual_date\": \"2021-06-28\", \"exception_timestamp\": null, \"deliver_zip\": \"23323\", \"roundtrip_wait_time\": null, \"add_charge_occur8\": null, \"dl_arrive_notification_sent\": false, \"pickup_special_instructions1\": null, \"ordered_by_phone_number\": null, \"deliver_requested_arr_time\": \"08:00\", \"rate_miles\": null, \"holiday_groups\": null, \"pickup_email_notification_sent\": false, \"add_charge_code3\": null, \"dispatch_id\": null, \"add_charge_occur10\": null, \"dispatch_time\": null, \"deliver_wait_time\": null, \"invoice_period_end_date\": null, \"add_charge_occur12\": null, \"fuel_plan\": null, \"return_svc_level\": null, \"pickup_actual_date\": \"2021-06-28\", \"send_new_order_alert\": false, \"pickup_room\": null, \"rate_buck_amt8\": null, \"add_charge_amt10\": null, \"insurance_amount\": null, \"add_charge_amt3\": null, \"add_charge_amt6\": null, \"pickup_special_instructions3\": null, \"pickup_requested_date\": \"2021-06-28\", \"roundtrip_sign_req\": false, \"actual_miles\": null, \"pickup_address_point_number_text\": \"BIG LOTS\", \"pickup_address_point_number\": 19864, \"deliver_actual_latitude\": null, \"deliver_phone_ext\": null, \"deliver_route_code\": null, \"add_charge_code10\": null, \"delivery_airport_code\": null, \"reference_text\": \"2125095801\", \"reference\": \"2125095801\", \"photos_exist\": false, \"master_airway_bill_number\": null, \"control_number\": 1872444, \"cod_text\": \"No\", \"cod\": \"N\", \"rate_buck_amt11\": null, \"cod_amount\": null, \"pickup_omw_timestamp\": null, \"signature_images\": [], \"deliver_special_instructions1\": null, \"quote_amount\": null, \"total_pages\": 1, \"rate_buck_amt4\": null, \"line_items\": [], \"delivery_latitude\": 36.78396970, \"progress\": [{\"status_time\": \"06:02:00\", \"status_date\": \"2021-07-08\", \"status_text\": \"Entered in carrier's system\"}, {\"status_time\": \"08:30:00\", \"status_date\": \"2021-06-28\", \"status_text\": \"Picked up\"}, {\"status_time\": \"08:15:00\", \"status_date\": \"2021-06-28\", \"status_text\": \"Delivered\"}], \"add_charge_code1\": null, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"deliver_special_instr_long\": null, \"deliver_address\": \"2920 AARON DR\", \"add_charge_occur4\": null, \"deliver_eta_date\": null, \"pickup_actual_dep_time\": \"08:30\", \"deliver_requested_dep_time\": \"17:00\", \"add_charge_code7\": null, \"deliver_actual_dep_time\": \"08:15\", \"cod_accept_cashiers_check\": false, \"bringg_last_loc_sent\": null, \"az_equip3\": null, \"driver1_text\": \"LAVERT KENDALL MORRIS\", \"driver1\": 3001, \"pickup_actual_latitude\": null, \"add_charge_occur2\": null, \"order_automatically_quoted\": false, \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"hours\": \"15\", \"frequent_caller_id\": null, \"rate_buck_amt6\": null, \"rate_chart_used\": 0, \"deliver_actual_pieces\": null, \"add_charge_code5\": null, \"pickup_omw_longitude\": null, \"notes\": [], \"delivery_point_customer\": 31025, \"add_charge_occur7\": null, \"rate_buck_amt5\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"add_charge_code11\": null, \"pickup_name\": \"BIG LOTS\", \"callback_date\": null, \"add_charge_code2\": null, \"house_airway_bill_number\": null, \"deliver_name\": \"ANNE BAILEY\", \"number_of_pieces\": 3, \"deliver_eta_time\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"rate_special_instructions\": null, \"add_charge_occur3\": null, \"pickup_eta_date\": null, \"deliver_special_instructions4\": null, \"custom_special_instr_long\": null, \"deliver_special_instructions2\": null, \"pickup_signature\": \"SOF\", \"az_equip1\": null, \"add_charge_amt12\": null, \"settlements\": [{\"vendor_invoice_number\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"voucher_date\": null, \"voucher_amount\": null, \"fuel_price_source\": null, \"settlement_bucket6_pct\": null, \"pre_book_percentage\": true, \"settlement_bucket4_pct\": null, \"settlement_period_end_date\": null, \"charge5\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"settlement_bucket5_pct\": null, \"voucher_number\": null, \"charge6\": null, \"settlement_bucket2_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"settlement_pct\": 100.00, \"vendor_employee_numer\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"driver_number_text\": \"LAVERT KENDALL MORRIS\", \"driver_number\": 3001, \"fuel_plan\": null, \"agents_etrac_partner_id\": null, \"charge2\": null, \"charge3\": null, \"settlement_bucket1_pct\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"id\": \"002018724440D1\", \"record_type\": 0, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"pay_chart_used\": null, \"fuel_price_zone\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"charge1\": null, \"control_number\": 1872444, \"adjustment_type\": null, \"order_date\": \"2021-06-28\", \"time_last_updated\": \"05:02\", \"agent_etrac_transaction_number\": null, \"date_last_updated\": \"2021-07-08\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"settlement_bucket3_pct\": null}], \"calc_add_on_chgs\": false, \"original_schedule_number\": null, \"blocks\": null, \"del_actual_location_accuracy\": null, \"zone_set_used\": 1, \"pickup_country\": null, \"pickup_state\": \"VA\", \"add_charge_amt7\": null, \"email_addresses\": null, \"add_charge_occur1\": null, \"pickup_wait_time\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"distribution_branch_id\": null, \"rate_buck_amt9\": null, \"add_charge_amt1\": null, \"pickup_requested_dep_time\": \"09:00\", \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"deliver_state\": \"VA\", \"deliver_dispatch_zone\": null, \"image_sign_req\": false, \"add_charge_code6\": null, \"deliver_requested_date\": \"2021-06-28\", \"add_charge_amt5\": null, \"time_order_entered\": \"06:02\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"pickup_attention\": null, \"rate_buck_amt7\": null, \"add_charge_occur6\": null, \"fuel_price_source\": null, \"pickup_airport_code\": null, \"rate_buck_amt2\": null, \"rate_buck_amt3\": null, \"deliver_omw_timestamp\": null, \"exception_code\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"weight\": null, \"signature_required\": true, \"rate_buck_amt10\": 2.34, \"hist_inv_number\": 0, \"deliver_pricing_zone\": 1, \"pickup_actual_longitude\": null, \"push_services\": null, \"add_charge_amt11\": null, \"rt_actual_location_accuracy\": null, \"roundtrip_actual_date\": null, \"pickup_requested_arr_time\": \"07:00\", \"deliver_attention\": null, \"deliver_special_instructions3\": null, \"pickup_actual_pieces\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"roundtrip_signature\": null, \"po_number\": null, \"signature\": \"SOF\", \"pickup_special_instructions2\": null, \"original_ctrl_number\": null, \"previous_ctrl_number\": null}}";
                                            //  objresponse.Reason = "{\"002018724450\": {\"roundtrip_actual_date\": null, \"notes\": [], \"pickup_phone_ext\": null, \"holiday_groups\": null, \"deliver_eta_time\": null, \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"add_charge_occur4\": null, \"deliver_state\": \"VA\", \"quote_amount\": null, \"cod_text\": \"No\", \"cod\": \"N\", \"additional_drivers\": false, \"rescheduled_ctrl_number\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"pickup_actual_pieces\": null, \"record_type\": 0, \"pickup_special_instr_long\": null, \"pickup_special_instructions3\": null, \"exception_timestamp\": null, \"deliver_actual_arr_time\": \"08:00\", \"house_airway_bill_number\": null, \"deliver_pricing_zone\": 1, \"total_pages\": 1, \"add_charge_occur11\": null, \"deliver_omw_latitude\": null, \"callback_userid\": null, \"rate_buck_amt1\": 57.00, \"pickup_point_customer\": 31025, \"pickup_eta_time\": null, \"add_charge_occur8\": null, \"invoice_period_end_date\": null, \"pickup_special_instructions1\": null, \"rate_buck_amt2\": null, \"pickup_special_instructions4\": null, \"manual_notepad\": false, \"edi_acknowledgement_required\": false, \"pickup_name\": \"BIG LOTS\", \"ordered_by_phone_number\": null, \"add_charge_amt12\": null, \"delivery_point_customer\": 31025, \"deliver_actual_dep_time\": \"08:15\", \"email_addresses\": null, \"pickup_address\": \"540 EASTPARK CT\", \"driver2\": null, \"signature_images\": [], \"rate_buck_amt11\": null, \"delivery_latitude\": 37.48366600, \"pickup_attention\": null, \"date_order_entered\": \"2021-07-08\", \"vehicle_type\": null, \"add_charge_amt9\": null, \"pickup_phone\": null, \"rate_miles\": null, \"customers_etrac_partner_id\": \"96609250\", \"order_type_text\": \"One way\", \"order_type\": \"O\", \"dl_arrive_notification_sent\": false, \"add_charge_code3\": null, \"etrac_number\": null, \"pickup_requested_arr_time\": \"07:00\", \"rate_buck_amt3\": null, \"pickup_actual_dep_time\": \"08:30\", \"line_items\": [], \"pickup_sign_req\": true, \"add_charge_code10\": null, \"deliver_city\": \"LANEXA\", \"fuel_plan\": null, \"add_charge_amt10\": null, \"roundtrip_actual_depart_time\": null, \"control_number\": 1872445, \"pickup_dispatch_zone\": null, \"send_new_order_alert\": false, \"settlements\": [{\"settlement_bucket4_pct\": null, \"charge1\": null, \"date_last_updated\": \"2021-07-08\", \"fuel_price_zone\": null, \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"charge4\": null, \"settlement_period_end_date\": null, \"charge5\": null, \"time_last_updated\": \"05:06\", \"charge6\": null, \"driver_number_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver_number\": 3208, \"control_number\": 1872445, \"settlement_bucket2_pct\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"voucher_date\": null, \"agent_etrac_transaction_number\": null, \"settlement_bucket5_pct\": null, \"record_type\": 0, \"voucher_number\": null, \"voucher_amount\": null, \"pay_chart_used\": null, \"settlement_pct\": 100.00, \"vendor_invoice_number\": null, \"settlement_bucket3_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"pre_book_percentage\": true, \"charge3\": null, \"settlement_bucket6_pct\": null, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"adjustment_type\": null, \"id\": \"002018724450D1\", \"agents_etrac_partner_id\": null, \"fuel_plan\": null, \"fuel_price_source\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"vendor_employee_numer\": null, \"settlement_bucket1_pct\": null, \"order_date\": \"2021-06-28\", \"charge2\": null}], \"deliver_actual_latitude\": null, \"fuel_price_zone\": null, \"verified_weight\": null, \"deliver_requested_dep_time\": \"17:00\", \"pickup_airport_code\": null, \"dispatch_time\": null, \"deliver_attention\": null, \"time_order_entered\": \"06:06\", \"rate_buck_amt4\": null, \"roundtrip_wait_time\": null, \"add_charge_amt2\": null, \"az_equip3\": null, \"progress\": [{\"status_date\": \"2021-07-08\", \"status_text\": \"Entered in carrier's system\", \"status_time\": \"06:06:00\"}, {\"status_date\": \"2021-06-28\", \"status_text\": \"Picked up\", \"status_time\": \"08:30:00\"}, {\"status_date\": \"2021-06-28\", \"status_text\": \"Delivered\", \"status_time\": \"08:15:00\"}], \"page_number\": 1, \"roundtrip_sign_req\": false, \"add_charge_amt1\": null, \"add_charge_code8\": null, \"weight\": null, \"rate_buck_amt6\": null, \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"bringg_send_sms\": false, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"custom_special_instr_long\": null, \"deliver_requested_arr_time\": \"08:00\", \"service_level_text\": \"Room of Choice\", \"service_level\": 55, \"az_equip1\": null, \"add_charge_code4\": null, \"bringg_order_id\": null, \"delivery_address_point_number_text\": \"JOSEPH FESSMAN\", \"delivery_address_point_number\": 26313, \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"deliver_special_instructions1\": null, \"pickup_wait_time\": null, \"add_charge_occur5\": null, \"push_partner_order_id\": null, \"deliver_route_sequence\": null, \"pickup_country\": null, \"pickup_state\": \"VA\", \"original_schedule_number\": null, \"frequent_caller_id\": null, \"distribution_unique_id\": 0, \"fuel_miles\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"rate_buck_amt5\": null, \"exception_sign_required\": false, \"pickup_route_code\": null, \"deliver_dispatch_zone\": null, \"delivery_longitude\": -76.90426400, \"pickup_pricing_zone\": 1, \"zone_set_used\": 1, \"deliver_special_instructions2\": null, \"add_charge_amt3\": null, \"deliver_phone\": null, \"pickup_email_notification_sent\": false, \"add_charge_occur12\": null, \"reference_text\": \"2125617401\", \"reference\": \"2125617401\", \"deliver_requested_date\": \"2021-06-28\", \"deliver_actual_longitude\": null, \"image_sign_req\": false, \"pickup_eta_date\": null, \"deliver_phone_ext\": null, \"pickup_omw_longitude\": null, \"original_ctrl_number\": null, \"pickup_special_instructions2\": null, \"order_automatically_quoted\": false, \"bol_number\": null, \"rate_buck_amt10\": 2.34, \"callback_time\": null, \"hazmat\": false, \"distribution_shift_id\": null, \"pickup_latitude\": 37.53250820, \"ordered_by\": \"RYDER\", \"insurance_amount\": null, \"cod_accept_cashiers_check\": false, \"add_charge_amt4\": null, \"add_charge_code7\": null, \"deliver_actual_pieces\": null, \"deliver_address\": \"15400 STAGE RD\", \"cod_accept_company_check\": false, \"signature\": \"SOF\", \"previous_ctrl_number\": null, \"deliver_zip\": \"23089\", \"deliver_special_instructions3\": null, \"rate_buck_amt7\": null, \"hist_inv_number\": 0, \"callback_date\": null, \"deliver_special_instr_long\": null, \"po_number\": null, \"pickup_actual_arr_time\": \"08:00\", \"pickup_requested_date\": \"2021-06-28\", \"number_of_pieces\": 2, \"dispatch_id\": null, \"photos_exist\": false, \"pickup_actual_latitude\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"id\": \"002018724450\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"del_actual_location_accuracy\": null, \"add_charge_occur7\": null, \"add_charge_occur9\": null, \"roundtrip_actual_latitude\": null, \"add_charge_occur6\": null, \"pickup_actual_longitude\": null, \"pickup_omw_timestamp\": null, \"bringg_last_loc_sent\": null, \"add_charge_code5\": null, \"deliver_country\": null, \"master_airway_bill_number\": null, \"pickup_route_seq\": null, \"roundtrip_signature\": null, \"calc_add_on_chgs\": false, \"deliver_actual_date\": \"2021-06-28\", \"cod_amount\": null, \"add_charge_code12\": null, \"rt_actual_location_accuracy\": null, \"rate_chart_used\": 0, \"pickup_longitude\": -77.33035820, \"pickup_signature\": \"SOF\", \"add_charge_amt5\": null, \"pu_arrive_notification_sent\": false, \"pickup_actual_date\": \"2021-06-28\", \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"push_services\": null, \"deliver_eta_date\": null, \"driver1_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver1\": 3208, \"deliver_omw_longitude\": null, \"deliver_wait_time\": null, \"pickup_room\": null, \"deliver_special_instructions4\": null, \"add_charge_amt7\": null, \"az_equip2\": null, \"hours\": \"15\", \"add_charge_code2\": null, \"exception_code\": null, \"roundtrip_actual_pieces\": null, \"rate_special_instructions\": null, \"roundtrip_actual_arrival_time\": null, \"add_charge_occur1\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"delivery_airport_code\": null, \"distribution_branch_id\": null, \"hist_inv_date\": null, \"add_charge_code1\": null, \"pickup_requested_dep_time\": \"09:00\", \"deliver_route_code\": null, \"roundtrip_actual_longitude\": null, \"pickup_city\": \"SANDSTON\", \"rate_buck_amt8\": null, \"pickup_omw_latitude\": null, \"deliver_omw_timestamp\": null, \"rate_buck_amt9\": null, \"deliver_room\": null, \"add_charge_code6\": null, \"add_charge_occur3\": null, \"blocks\": null, \"add_charge_code9\": null, \"actual_miles\": null, \"add_charge_occur10\": null, \"add_charge_code11\": null, \"pickup_address_point_number_text\": \"BIG LOTS\", \"pickup_address_point_number\": 19864, \"customer_name\": \"MXD/RYDER\", \"pu_actual_location_accuracy\": null, \"deliver_name\": \"JOSEPH FESSMAN\", \"add_charge_amt6\": null, \"signature_required\": true, \"csr\": \"DX*\", \"add_charge_amt8\": null, \"callback_to\": null, \"fuel_price_source\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"pickup_zip\": \"23150\", \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"return_svc_level\": null, \"add_charge_amt11\": null, \"add_charge_occur2\": null}}";
                                            //  objresponse.Reason = "{\"error\": \"Unable to perform API call object with RecordID:WS_OPORDR-9990111870 - The record may have been deleted.\", \"status\": \"error\", \"code\": \"U.RecordNotFound\"}";
                                            // objresponse.Reason = "{\"999000115280\": {\"deliver_eta_time\": null, \"deliver_special_instructions4\": null, \"del_actual_location_accuracy\": null, \"add_charge_amt8\": null, \"callback_to\": null, \"rate_chart_used\": 1, \"pickup_actual_latitude\": null, \"signature_images\": [], \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"add_charge_occur8\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"hours\": null, \"distribution_shift_id\": null, \"pickup_longitude\": null, \"hist_inv_number\": 0, \"add_charge_amt7\": null, \"dispatch_id\": null, \"dl_arrive_notification_sent\": false, \"add_charge_code8\": null, \"roundtrip_sign_req\": false, \"exception_timestamp\": null, \"pickup_city\": null, \"delivery_airport_code\": null, \"progress\": [{\"status_time\": \"08:34:00\", \"status_date\": \"2022-03-10\", \"status_text\": \"Entered in carrier's system\"}], \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"add_charge_amt2\": null, \"pickup_special_instructions1\": null, \"time_order_entered\": \"08:34\", \"distribution_unique_id\": 0, \"pickup_eta_date\": null, \"rate_buck_amt2\": null, \"rate_buck_amt9\": null, \"deliver_zip\": \"48150\", \"rate_buck_amt7\": null, \"customer_name\": \"TEST\", \"deliver_phone_ext\": null, \"roundtrip_actual_depart_time\": null, \"add_charge_code10\": null, \"add_charge_occur2\": null, \"deliver_state\": \"MI\", \"pickup_wait_time\": null, \"pickup_requested_arr_time\": null, \"csr\": \"RG\", \"add_charge_amt4\": null, \"holiday_groups\": null, \"total_pages\": 1, \"delivery_longitude\": -83.35663860, \"bringg_last_loc_sent\": null, \"deliver_name\": \"TANTARA\", \"deliver_actual_longitude\": null, \"distribution_branch_id\": null, \"deliver_wait_time\": null, \"add_charge_occur11\": null, \"deliver_omw_timestamp\": null, \"add_charge_amt3\": null, \"add_charge_amt10\": null, \"rate_buck_amt3\": null, \"rescheduled_ctrl_number\": null, \"add_charge_occur10\": null, \"deliver_address\": \"31782 ENTERPRISE DR\", \"pickup_latitude\": null, \"rate_buck_amt1\": null, \"pickup_phone\": null, \"pickup_actual_date\": \"2022-01-03\", \"previous_ctrl_number\": null, \"control_number\": 11528, \"rate_buck_amt11\": null, \"fuel_price_source\": null, \"add_charge_code9\": null, \"add_charge_occur3\": null, \"fuel_price_zone\": null, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"rate_buck_amt10\": null, \"add_charge_code12\": null, \"quote_amount\": null, \"deliver_phone\": null, \"ordered_by_phone_number\": null, \"cod_accept_company_check\": false, \"callback_time\": null, \"deliver_dispatch_zone\": null, \"hazmat\": false, \"az_equip2\": null, \"add_charge_occur1\": null, \"pickup_email_notification_sent\": false, \"deliver_requested_dep_time\": null, \"deliver_special_instructions1\": null, \"deliver_actual_date\": null, \"rt_actual_location_accuracy\": null, \"signature_required\": false, \"pickup_attention\": null, \"pu_actual_location_accuracy\": null, \"rate_special_instructions\": null, \"pickup_special_instructions2\": null, \"driver2\": null, \"deliver_route_sequence\": null, \"add_charge_code2\": null, \"pickup_state\": null, \"add_charge_code1\": null, \"deliver_actual_pieces\": null, \"pickup_country\": null, \"signature\": null, \"add_charge_occur12\": null, \"reference_text\": \"FEDX01032022\", \"reference\": \"FEDX01032022\", \"pickup_pricing_zone\": null, \"pickup_route_seq\": null, \"pickup_actual_arr_time\": null, \"date_order_entered\": \"2022-03-10\", \"rate_buck_amt5\": null, \"number_of_pieces\": null, \"add_charge_code11\": null, \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"pickup_omw_timestamp\": null, \"delivery_point_customer\": 1, \"roundtrip_actual_latitude\": null, \"rate_buck_amt4\": null, \"pickup_requested_date\": \"2022-01-03\", \"po_number\": null, \"origin_code_text\": \"Web-Carrier UI\", \"origin_code\": \"X\", \"add_charge_occur7\": null, \"exception_sign_required\": false, \"id\": \"999000115280\", \"pickup_route_code\": null, \"pickup_airport_code\": null, \"roundtrip_actual_date\": null, \"roundtrip_signature\": null, \"roundtrip_actual_pieces\": null, \"pickup_phone_ext\": null, \"deliver_city\": \"LIVONIA\", \"deliver_omw_latitude\": null, \"service_level_text\": \"REGULAR\", \"service_level\": 1, \"order_timeliness_text\": \"Open\", \"order_timeliness\": \"5\", \"roundtrip_actual_arrival_time\": null, \"deliver_actual_arr_time\": null, \"etrac_number\": null, \"add_charge_occur9\": null, \"az_equip1\": null, \"rate_miles\": null, \"frequent_caller_id\": null, \"pickup_sign_req\": false, \"customer_type\": null, \"pickup_omw_latitude\": null, \"actual_miles\": null, \"add_charge_code3\": null, \"deliver_eta_date\": null, \"fuel_miles\": null, \"pickup_special_instructions4\": null, \"house_airway_bill_number\": null, \"vehicle_type\": null, \"cod_accept_cashiers_check\": false, \"settlements\": [], \"pickup_address\": null, \"pickup_room\": null, \"weight\": null, \"pickup_actual_longitude\": null, \"rate_buck_amt8\": null, \"delivery_address_point_number_text\": \"TANTARA\", \"delivery_address_point_number\": 10, \"status_code_text\": \"Entered\", \"status_code\": \"E\", \"master_airway_bill_number\": null, \"delivery_latitude\": 42.36977420, \"bringg_order_id\": null, \"add_charge_code7\": null, \"roundtrip_wait_time\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"cod_text\": \"No\", \"cod\": \"N\", \"deliver_room\": null, \"rate_buck_amt6\": null, \"pu_arrive_notification_sent\": false, \"deliver_actual_latitude\": null, \"blocks\": null, \"callback_userid\": null, \"edi_acknowledgement_required\": false, \"add_charge_code4\": null, \"driver1\": null, \"calc_add_on_chgs\": false, \"fuel_plan\": null, \"add_charge_amt9\": null, \"pickup_point_customer\": 0, \"zone_set_used\": 1, \"exception_code\": null, \"invoice_period_end_date\": null, \"push_partner_order_id\": null, \"verified_weight\": null, \"pickup_actual_pieces\": null, \"notes\": [], \"add_charge_amt12\": null, \"deliver_special_instructions3\": null, \"deliver_special_instructions2\": null, \"customer_number_text\": \"test\", \"customer_number\": 1, \"return_svc_level\": null, \"add_charge_amt1\": null, \"az_equip3\": null, \"image_sign_req\": false, \"pickup_omw_longitude\": null, \"deliver_requested_arr_time\": null, \"pickup_name\": null, \"pickup_address_point_number\": null, \"pickup_signature\": null, \"pickup_special_instructions3\": null, \"original_ctrl_number\": null, \"add_charge_code6\": null, \"deliver_omw_longitude\": null, \"additional_drivers\": false, \"deliver_country\": null, \"add_charge_amt5\": null, \"insurance_amount\": null, \"cod_amount\": null, \"email_addresses\": null, \"pickup_actual_dep_time\": null, \"page_number\": 1, \"dispatch_time\": null, \"callback_date\": null, \"add_charge_occur5\": null, \"add_charge_occur6\": null, \"company_number_text\": \"TEST COMPANY\", \"company_number\": 999, \"pickup_dispatch_zone\": null, \"deliver_attention\": null, \"record_type\": 0, \"deliver_pricing_zone\": 1, \"deliver_requested_date\": \"2022-01-03\", \"push_services\": null, \"add_charge_amt6\": null, \"order_automatically_quoted\": false, \"custom_special_instr_long\": null, \"bol_number\": \"FEDX01032022\", \"hist_inv_date\": null, \"roundtrip_actual_longitude\": null, \"add_charge_amt11\": null, \"bringg_send_sms\": false, \"pickup_special_instr_long\": null, \"ordered_by\": \"DET\", \"deliver_special_instr_long\": null, \"pickup_zip\": null, \"pickup_requested_dep_time\": null, \"deliver_route_code\": null, \"deliver_actual_dep_time\": null, \"customers_etrac_partner_id\": null, \"add_charge_code5\": null, \"photos_exist\": false, \"original_schedule_number\": null, \"add_charge_occur4\": null, \"send_new_order_alert\": false, \"manual_notepad\": false, \"line_items\": [], \"pickup_eta_time\": null, \"_utc_offset\": \"-06:00\"}}";
                                            if (objresponse.ResponseVal)
                                            {
                                                strExecutionLogMessage = "OrderPostAPI Success " + System.Environment.NewLine;
                                                strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                                // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                DataSet dsOrderResponse = objCommon.jsonToDataSet(objresponse.Reason, "OrderPost");
                                                var UniqueId = Convert.ToString(dsOrderResponse.Tables["id"].Rows[0]["id"]);
                                                try
                                                {
                                                    if (dsOrderResponse.Tables.Contains("id"))
                                                    {
                                                        List<Id> idList = new List<Id>();
                                                        for (int i = 0; i < dsOrderResponse.Tables["id"].Rows.Count; i++)
                                                        {
                                                            DataTable dt = dsOrderResponse.Tables["id"];
                                                            Id objIds = new Id();
                                                            // objIds.verified_weight = dt.Rows[i]["verified_weight"];
                                                            // objIds.roundtrip_actual_latitude = dt.Rows[i]["roundtrip_actual_latitude"];
                                                            // objIds.pickup_special_instructions4 = dt.Rows[i]["pickup_special_instructions4"];
                                                            // objIds.fuel_miles = dt.Rows[i]["fuel_miles"];
                                                            // objIds.pickup_dispatch_zone = dt.Rows[i]["pickup_dispatch_zone"];
                                                            if (dt.Columns.Contains("pickup_zip"))
                                                            {
                                                                objIds.pickup_zip = dt.Rows[i]["pickup_zip"];
                                                            }
                                                            if (dt.Columns.Contains("pickup_actual_arr_time"))
                                                            {
                                                                objIds.pickup_actual_arr_time = dt.Rows[i]["pickup_actual_arr_time"];
                                                            }
                                                            //objIds.cod_accept_company_check = dt.Rows[i]["cod_accept_company_check"];
                                                            // objIds.add_charge_occur9 = dt.Rows[i]["add_charge_occur9"];
                                                            //objIds.pickup_omw_latitude = dt.Rows[i]["pickup_omw_latitude"];
                                                            // objIds.service_level_text = dt.Rows[i]["service_level_text"];
                                                            if (dt.Columns.Contains("service_level"))
                                                            {
                                                                objIds.service_level = dt.Rows[i]["service_level"];
                                                            }
                                                            //objIds.exception_sign_required = dt.Rows[i]["exception_sign_required"];
                                                            //objIds.pickup_phone_ext = dt.Rows[i]["pickup_phone_ext"];
                                                            //objIds.roundtrip_actual_pieces = dt.Rows[i]["roundtrip_actual_pieces"];
                                                            //objIds.bringg_send_sms = dt.Rows[i]["bringg_send_sms"];
                                                            //objIds.az_equip2 = dt.Rows[i]["az_equip2"];

                                                            //objIds.hist_inv_date = dt.Rows[i]["hist_inv_date"];
                                                            //objIds.date_order_entered = dt.Rows[i]["date_order_entered"];
                                                            //objIds.powerpage_status_text = dt.Rows[i]["powerpage_status_text"];
                                                            //objIds.powerpage_status = dt.Rows[i]["powerpage_status"];
                                                            if (dt.Columns.Contains("pickup_city"))
                                                            {
                                                                objIds.pickup_city = dt.Rows[i]["pickup_city"];
                                                            }
                                                            //objIds.pickup_phone = dt.Rows[i]["pickup_phone"];
                                                            //objIds.pickup_sign_req = dt.Rows[i]["pickup_sign_req"];

                                                            //objIds.deliver_phone = dt.Rows[i]["deliver_phone"];
                                                            //objIds.deliver_omw_longitude = dt.Rows[i]["deliver_omw_longitude"];
                                                            //objIds.roundtrip_actual_longitude = dt.Rows[i]["roundtrip_actual_longitude"];
                                                            //objIds.page_number = dt.Rows[i]["page_number"];
                                                            //objIds.order_type_text = dt.Rows[i]["order_type_text"];
                                                            //objIds.order_type = dt.Rows[i]["order_type"];
                                                            //objIds.add_charge_code9 = dt.Rows[i]["add_charge_code9"];
                                                            //objIds.pickup_eta_time = dt.Rows[i]["pickup_eta_time"];

                                                            //objIds.record_type = dt.Rows[i]["record_type"];
                                                            //objIds.add_charge_occur11 = dt.Rows[i]["add_charge_occur11"];
                                                            //objIds.push_partner_order_id = dt.Rows[i]["push_partner_order_id"];
                                                            //objIds.deliver_country = dt.Rows[i]["deliver_country"];
                                                            //objIds.customer_name = dt.Rows[i]["customer_name"];
                                                            if (dt.Columns.Contains("bol_number"))
                                                            {
                                                                objIds.bol_number = dt.Rows[i]["bol_number"];
                                                            }
                                                            //objIds.pickup_latitude = dt.Rows[i]["pickup_latitude"];
                                                            //objIds.add_charge_code4 = dt.Rows[i]["add_charge_code4"];

                                                            //objIds.exception_order_action_text = dt.Rows[i]["exception_order_action_text"];
                                                            //objIds.exception_order_action = dt.Rows[i]["exception_order_action"];
                                                            //objIds.pu_arrive_notification_sent = dt.Rows[i]["pu_arrive_notification_sent"];
                                                            //objIds.distribution_shift_id = dt.Rows[i]["distribution_shift_id"];
                                                            //objIds.pickup_special_instr_long = dt.Rows[i]["pickup_special_instr_long"];
                                                            if (dt.Columns.Contains("id"))
                                                            {
                                                                objIds.id = dt.Rows[i]["id"];
                                                            }
                                                            //objIds.callback_to = dt.Rows[i]["callback_to"];
                                                            //objIds.customer_number_text = dt.Rows[i]["customer_number_text"];
                                                            if (dt.Columns.Contains("customer_number"))
                                                            {
                                                                objIds.customer_number = dt.Rows[i]["customer_number"];
                                                            }
                                                            //objIds.ordered_by = dt.Rows[i]["ordered_by"];
                                                            //objIds.add_charge_code12 = dt.Rows[i]["add_charge_code12"];
                                                            //objIds.pickup_route_seq = dt.Rows[i]["pickup_route_seq"];
                                                            if (dt.Columns.Contains("deliver_city"))
                                                            {
                                                                objIds.deliver_city = dt.Rows[i]["deliver_city"];
                                                            }

                                                            //objIds.add_charge_occur5 = dt.Rows[i]["add_charge_occur5"];
                                                            //objIds.edi_acknowledgement_required = dt.Rows[i]["edi_acknowledgement_required"];
                                                            //objIds.rescheduled_ctrl_number = dt.Rows[i]["rescheduled_ctrl_number"];
                                                            //objIds.driver2 = dt.Rows[i]["driver2"];
                                                            //objIds.deliver_room = dt.Rows[i]["deliver_room"];
                                                            if (dt.Columns.Contains("deliver_actual_arr_time"))
                                                            {
                                                                objIds.deliver_actual_arr_time = dt.Rows[i]["deliver_actual_arr_time"];
                                                            }
                                                            //objIds.fuel_price_zone = dt.Rows[i]["fuel_price_zone"];
                                                            //objIds.add_charge_amt9 = dt.Rows[i]["add_charge_amt9"];
                                                            //objIds.add_charge_amt4 = dt.Rows[i]["add_charge_amt4"];
                                                            //objIds.delivery_address_point_number_text = dt.Rows[i]["delivery_address_point_number_text"];
                                                            //objIds.delivery_address_point_number = dt.Rows[i]["delivery_address_point_number"];

                                                            //objIds.deliver_actual_longitude = dt.Rows[i]["deliver_actual_longitude"];
                                                            //objIds.add_charge_amt2 = dt.Rows[i]["add_charge_amt2"];
                                                            //objIds.additional_drivers = dt.Rows[i]["additional_drivers"];
                                                            //objIds.pickup_pricing_zone = dt.Rows[i]["pickup_pricing_zone"];
                                                            //objIds.hazmat = dt.Rows[i]["hazmat"];
                                                            if (dt.Columns.Contains("pickup_address"))
                                                            {
                                                                objIds.pickup_address = dt.Rows[i]["pickup_address"];
                                                            }
                                                            //objIds.pickup_route_code = dt.Rows[i]["pickup_route_code"];
                                                            //objIds.callback_userid = dt.Rows[i]["callback_userid"];
                                                            //objIds.pickup_point_customer = dt.Rows[i]["pickup_point_customer"];

                                                            //objIds.rate_buck_amt1 = dt.Rows[i]["rate_buck_amt1"];
                                                            //objIds.add_charge_amt8 = dt.Rows[i]["add_charge_amt8"];
                                                            //objIds.callback_time = dt.Rows[i]["callback_time"];
                                                            //objIds.csr = dt.Rows[i]["csr"];
                                                            //objIds.roundtrip_actual_depart_time = dt.Rows[i]["roundtrip_actual_depart_time"];
                                                            //objIds.customers_etrac_partner_id = dt.Rows[i]["customers_etrac_partner_id"];
                                                            //objIds.manual_notepad = dt.Rows[i]["manual_notepad"];
                                                            //objIds.add_charge_code8 = dt.Rows[i]["add_charge_code8"];
                                                            //objIds.bringg_order_id = dt.Rows[i]["bringg_order_id"];
                                                            //objIds.deliver_omw_latitude = dt.Rows[i]["deliver_omw_latitude"];
                                                            //objIds.pickup_longitude = dt.Rows[i]["pickup_longitude"];
                                                            //objIds.etrac_number = dt.Rows[i]["etrac_number"];

                                                            //objIds.distribution_unique_id = dt.Rows[i]["distribution_unique_id"];
                                                            //objIds.vehicle_type = dt.Rows[i]["vehicle_type"];
                                                            //objIds.roundtrip_actual_arrival_time = dt.Rows[i]["roundtrip_actual_arrival_time"];
                                                            //objIds.delivery_longitude = dt.Rows[i]["delivery_longitude"];
                                                            //objIds.pu_actual_location_accuracy = dt.Rows[i]["pu_actual_location_accuracy"];
                                                            if (dt.Columns.Contains("deliver_actual_date"))
                                                            {
                                                                objIds.deliver_actual_date = dt.Rows[i]["deliver_actual_date"];
                                                            }
                                                            //objIds.exception_timestamp = dt.Rows[i]["exception_timestamp"];
                                                            if (dt.Columns.Contains("deliver_zip"))
                                                            {
                                                                objIds.deliver_zip = dt.Rows[i]["deliver_zip"];
                                                            }
                                                            //objIds.roundtrip_wait_time = dt.Rows[i]["roundtrip_wait_time"];
                                                            //objIds.add_charge_occur8 = dt.Rows[i]["add_charge_occur8"];
                                                            //objIds.dl_arrive_notification_sent = dt.Rows[i]["dl_arrive_notification_sent"];
                                                            //objIds.pickup_special_instructions1 = dt.Rows[i]["pickup_special_instructions1"];
                                                            //objIds.ordered_by_phone_number = dt.Rows[i]["ordered_by_phone_number"];
                                                            if (dt.Columns.Contains("deliver_requested_arr_time"))
                                                            {
                                                                objIds.deliver_requested_arr_time = dt.Rows[i]["deliver_requested_arr_time"];
                                                            }

                                                            //objIds.rate_miles = dt.Rows[i]["rate_miles"];
                                                            //objIds.holiday_groups = dt.Rows[i]["holiday_groups"];
                                                            //objIds.pickup_email_notification_sent = dt.Rows[i]["pickup_email_notification_sent"];
                                                            //objIds.add_charge_code3 = dt.Rows[i]["add_charge_code3"];
                                                            //objIds.dispatch_id = dt.Rows[i]["dispatch_id"];
                                                            //objIds.add_charge_occur10 = dt.Rows[i]["add_charge_occur10"];
                                                            //objIds.dispatch_time = dt.Rows[i]["dispatch_time"];
                                                            //objIds.deliver_wait_time = dt.Rows[i]["deliver_wait_time"];
                                                            //objIds.invoice_period_end_date = dt.Rows[i]["invoice_period_end_date"];
                                                            //objIds.add_charge_occur12 = dt.Rows[i]["add_charge_occur12"];

                                                            //objIds.fuel_plan = dt.Rows[i]["fuel_plan"];
                                                            //objIds.return_svc_level = dt.Rows[i]["return_svc_level"];
                                                            if (dt.Columns.Contains("pickup_actual_date"))
                                                            {
                                                                objIds.pickup_actual_date = dt.Rows[i]["pickup_actual_date"];
                                                            }
                                                            //objIds.send_new_order_alert = dt.Rows[i]["send_new_order_alert"];
                                                            //objIds.pickup_room = dt.Rows[i]["pickup_room"];
                                                            //objIds.rate_buck_amt8 = dt.Rows[i]["rate_buck_amt8"];
                                                            //objIds.add_charge_amt10 = dt.Rows[i]["add_charge_amt10"];
                                                            //objIds.insurance_amount = dt.Rows[i]["insurance_amount"];
                                                            //objIds.add_charge_amt3 = dt.Rows[i]["add_charge_amt3"];
                                                            //objIds.add_charge_amt6 = dt.Rows[i]["add_charge_amt6"];
                                                            //objIds.pickup_special_instructions3 = dt.Rows[i]["pickup_special_instructions3"];
                                                            if (dt.Columns.Contains("pickup_requested_date"))
                                                            {
                                                                objIds.pickup_requested_date = dt.Rows[i]["pickup_requested_date"];
                                                            }
                                                            //objIds.roundtrip_sign_req = dt.Rows[i]["roundtrip_sign_req"];
                                                            //objIds.actual_miles = dt.Rows[i]["actual_miles"];
                                                            //objIds.pickup_address_point_number_text = dt.Rows[i]["pickup_address_point_number_text"];
                                                            //if (dt.Columns.Contains("pickup_address_point_number_text"))
                                                            //{
                                                            //    objIds.pickup_address_point_number_text = dt.Rows[i]["pickup_address_point_number_text"];
                                                            //}
                                                            //objIds.pickup_address_point_number = dt.Rows[i]["pickup_address_point_number"];
                                                            //objIds.deliver_actual_latitude = dt.Rows[i]["deliver_actual_latitude"];
                                                            //objIds.deliver_phone_ext = dt.Rows[i]["deliver_phone_ext"];
                                                            //objIds.deliver_route_code = dt.Rows[i]["deliver_route_code"];
                                                            //objIds.add_charge_code10 = dt.Rows[i]["add_charge_code10"];
                                                            //objIds.delivery_airport_code = dt.Rows[i]["delivery_airport_code"];
                                                            if (dt.Columns.Contains("reference_text"))
                                                            {
                                                                objIds.reference_text = dt.Rows[i]["reference_text"];
                                                            }
                                                            if (dt.Columns.Contains("reference"))
                                                            {
                                                                objIds.reference = dt.Rows[i]["reference"];
                                                            }
                                                            //objIds.photos_exist = dt.Rows[i]["photos_exist"];
                                                            //objIds.master_airway_bill_number = dt.Rows[i]["master_airway_bill_number"];
                                                            if (dt.Columns.Contains("control_number"))
                                                            {
                                                                objIds.control_number = dt.Rows[i]["control_number"];
                                                            }
                                                            //objIds.cod_text = dt.Rows[i]["cod_text"];
                                                            //objIds.cod = dt.Rows[i]["cod"];
                                                            //objIds.rate_buck_amt11 = dt.Rows[i]["rate_buck_amt11"];
                                                            //objIds.pickup_omw_timestamp = dt.Rows[i]["pickup_omw_timestamp"];
                                                            //objIds.deliver_special_instructions1 = dt.Rows[i]["deliver_special_instructions1"];
                                                            //objIds.quote_amount = dt.Rows[i]["quote_amount"];
                                                            //objIds.total_pages = dt.Rows[i]["total_pages"];
                                                            //objIds.rate_buck_amt4 = dt.Rows[i]["rate_buck_amt4"];
                                                            //objIds.delivery_latitude = dt.Rows[i]["delivery_latitude"];
                                                            //objIds.add_charge_code1 = dt.Rows[i]["add_charge_code1"];


                                                            //objIds.order_timeliness_text = dt.Rows[i]["order_timeliness_text"];
                                                            //objIds.order_timeliness = dt.Rows[i]["order_timeliness"];
                                                            //objIds.deliver_special_instr_long = dt.Rows[i]["deliver_special_instr_long"];
                                                            if (dt.Columns.Contains("deliver_address"))
                                                            {
                                                                objIds.deliver_address = dt.Rows[i]["deliver_address"];
                                                            }
                                                            //objIds.add_charge_occur4 = dt.Rows[i]["add_charge_occur4"];
                                                            //objIds.deliver_eta_date = dt.Rows[i]["deliver_eta_date"];
                                                            if (dt.Columns.Contains("pickup_actual_dep_time"))
                                                            {
                                                                objIds.pickup_actual_dep_time = dt.Rows[i]["pickup_actual_dep_time"];
                                                            }
                                                            if (dt.Columns.Contains("deliver_requested_dep_time"))
                                                            {
                                                                objIds.deliver_requested_dep_time = dt.Rows[i]["deliver_requested_dep_time"];
                                                            }
                                                            if (dt.Columns.Contains("deliver_actual_dep_time"))
                                                            {
                                                                objIds.deliver_actual_dep_time = dt.Rows[i]["deliver_actual_dep_time"];
                                                            }

                                                            //objIds.bringg_last_loc_sent = dt.Rows[i]["bringg_last_loc_sent"];
                                                            //objIds.az_equip3 = dt.Rows[i]["az_equip3"];
                                                            //objIds.driver1_text = dt.Rows[i]["driver1_text"];
                                                            //if (dt.Columns.Contains("driver1_text"))
                                                            //{
                                                            //    objIds.driver1_text = dt.Rows[i]["driver1_text"];
                                                            //}
                                                            if (dt.Columns.Contains("driver1"))
                                                            {
                                                                objIds.driver1 = dt.Rows[i]["driver1"];
                                                            }
                                                            //objIds.pickup_actual_latitude = dt.Rows[i]["pickup_actual_latitude"];
                                                            //objIds.add_charge_occur2 = dt.Rows[i]["add_charge_occur2"];
                                                            //objIds.order_automatically_quoted = dt.Rows[i]["order_automatically_quoted"];
                                                            //objIds.callback_required = dt.Rows[i]["callback_required_text"];
                                                            //objIds.frequent_caller_id = dt.Rows[i]["frequent_caller_id"];
                                                            //objIds.rate_buck_amt6 = dt.Rows[i]["rate_buck_amt6"];
                                                            //objIds.rate_chart_used = dt.Rows[i]["rate_chart_used"];
                                                            if (dt.Columns.Contains("deliver_actual_pieces"))
                                                            {
                                                                objIds.deliver_actual_pieces = dt.Rows[i]["deliver_actual_pieces"];
                                                            }

                                                            //objIds.add_charge_code5 = dt.Rows[i]["add_charge_code5"];
                                                            //objIds.pickup_omw_longitude = dt.Rows[i]["pickup_omw_longitude"];
                                                            //objIds.delivery_point_customer = dt.Rows[i]["delivery_point_customer"];
                                                            //objIds.add_charge_occur7 = dt.Rows[i]["add_charge_occur7"];
                                                            //objIds.rate_buck_amt5 = dt.Rows[i]["rate_buck_amt5"];
                                                            //objIds.fuel_update_freq_text = dt.Rows[i]["fuel_update_freq_text"];
                                                            //objIds.fuel_update_freq = dt.Rows[i]["fuel_update_freq"];
                                                            //objIds.add_charge_code11 = dt.Rows[i]["add_charge_code11"];
                                                            if (dt.Columns.Contains("pickup_name"))
                                                            {
                                                                objIds.pickup_name = dt.Rows[i]["pickup_name"];
                                                            }
                                                            //objIds.callback_date = dt.Rows[i]["callback_date"];
                                                            //objIds.add_charge_code2 = dt.Rows[i]["add_charge_code2"];
                                                            //objIds.house_airway_bill_number = dt.Rows[i]["house_airway_bill_number"];
                                                            if (dt.Columns.Contains("deliver_name"))
                                                            {
                                                                objIds.deliver_name = dt.Rows[i]["deliver_name"];
                                                            }
                                                            if (dt.Columns.Contains("number_of_pieces"))
                                                            {
                                                                objIds.number_of_pieces = dt.Rows[i]["number_of_pieces"];
                                                            }
                                                            //objIds.deliver_eta_time = dt.Rows[i]["deliver_eta_time"];
                                                            //objIds.origin_code_text = dt.Rows[i]["origin_code_text"];
                                                            //objIds.origin_code = dt.Rows[i]["origin_code"];
                                                            //objIds.rate_special_instructions = dt.Rows[i]["rate_special_instructions"];
                                                            //objIds.add_charge_occur3 = dt.Rows[i]["add_charge_occur3"];
                                                            //objIds.pickup_eta_date = dt.Rows[i]["pickup_eta_date"];
                                                            //objIds.deliver_special_instructions4 = dt.Rows[i]["deliver_special_instructions4"];
                                                            //objIds.custom_special_instr_long = dt.Rows[i]["custom_special_instr_long"];
                                                            //objIds.deliver_special_instructions2 = dt.Rows[i]["deliver_special_instructions2"];
                                                            if (dt.Columns.Contains("pickup_signature"))
                                                            {
                                                                objIds.pickup_signature = dt.Rows[i]["pickup_signature"];
                                                            }
                                                            //objIds.az_equip1 = dt.Rows[i]["az_equip1"];
                                                            //objIds.add_charge_amt12 = dt.Rows[i]["add_charge_amt12"];
                                                            //objIds.calc_add_on_chgs = dt.Rows[i]["calc_add_on_chgs"];
                                                            //objIds.original_schedule_number = dt.Rows[i]["original_schedule_number"];
                                                            //objIds.blocks = dt.Rows[i]["blocks"];
                                                            //objIds.del_actual_location_accuracy = dt.Rows[i]["del_actual_location_accuracy"];
                                                            //objIds.zone_set_used = dt.Rows[i]["zone_set_used"];

                                                            // objIds.pickup_country = dt.Rows[i]["pickup_country"];
                                                            if (dt.Columns.Contains("pickup_state"))
                                                            {
                                                                objIds.pickup_state = dt.Rows[i]["pickup_state"];
                                                            }

                                                            //objIds.add_charge_amt7 = dt.Rows[i]["add_charge_amt7"];
                                                            //objIds.email_addresses = dt.Rows[i]["email_addresses"];
                                                            //objIds.add_charge_occur1 = dt.Rows[i]["add_charge_occur1"];
                                                            //objIds.pickup_wait_time = dt.Rows[i]["pickup_wait_time"];
                                                            //objIds.company_number_text = dt.Rows[i]["company_number_text"];
                                                            if (dt.Columns.Contains("company_number"))
                                                            {
                                                                objIds.company_number = dt.Rows[i]["company_number"];
                                                            }
                                                            //objIds.distribution_branch_id = dt.Rows[i]["distribution_branch_id"];
                                                            //objIds.rate_buck_amt9 = dt.Rows[i]["rate_buck_amt9"];
                                                            //objIds.add_charge_amt1 = dt.Rows[i]["add_charge_amt1"];
                                                            if (dt.Columns.Contains("pickup_requested_dep_time"))
                                                            {
                                                                objIds.pickup_requested_dep_time = dt.Rows[i]["pickup_requested_dep_time"];
                                                            }
                                                            //objIds.customer_type_text = dt.Rows[i]["customer_type_text"];
                                                            //if (dt.Columns.Contains("customer_type_text"))
                                                            //{
                                                            //    objIds.customer_type_text = dt.Rows[i]["customer_type_text"];
                                                            //}
                                                            //objIds.customer_type = dt.Rows[i]["customer_type"];
                                                            if (dt.Columns.Contains("deliver_state"))
                                                            {
                                                                objIds.deliver_state = dt.Rows[i]["deliver_state"];
                                                            }
                                                            //objIds.deliver_dispatch_zone = dt.Rows[i]["deliver_dispatch_zone"];
                                                            //objIds.image_sign_req = dt.Rows[i]["image_sign_req"];
                                                            //objIds.add_charge_code6 = dt.Rows[i]["add_charge_code6"];
                                                            if (dt.Columns.Contains("deliver_requested_date"))
                                                            {
                                                                objIds.deliver_requested_date = dt.Rows[i]["deliver_requested_date"];
                                                            }
                                                            // objIds.add_charge_amt5 = dt.Rows[i]["add_charge_amt5"];
                                                            if (dt.Columns.Contains("time_order_entered"))
                                                            {
                                                                objIds.time_order_entered = dt.Rows[i]["time_order_entered"];
                                                            }
                                                            //objIds.pick_del_trans_flag_text = dt.Rows[i]["pick_del_trans_flag_text"];
                                                            //objIds.pick_del_trans_flag = dt.Rows[i]["pick_del_trans_flag"];
                                                            //objIds.pickup_attention = dt.Rows[i]["pickup_attention"];
                                                            //objIds.rate_buck_amt7 = dt.Rows[i]["rate_buck_amt7"];
                                                            //objIds.add_charge_occur6 = dt.Rows[i]["add_charge_occur6"];
                                                            //objIds.fuel_price_source = dt.Rows[i]["fuel_price_source"];
                                                            //objIds.pickup_airport_code = dt.Rows[i]["pickup_airport_code"];
                                                            //objIds.rate_buck_amt2 = dt.Rows[i]["rate_buck_amt2"];
                                                            //objIds.rate_buck_amt3 = dt.Rows[i]["rate_buck_amt3"];
                                                            //objIds.deliver_omw_timestamp = dt.Rows[i]["deliver_omw_timestamp"];
                                                            //objIds.exception_code = dt.Rows[i]["exception_code"];
                                                            //objIds.status_code_text = dt.Rows[i]["status_code_text"];
                                                            //objIds.status_code = dt.Rows[i]["status_code"];
                                                            //objIds.weight = dt.Rows[i]["weight"];
                                                            //objIds.signature_required = dt.Rows[i]["signature_required"];
                                                            //objIds.rate_buck_amt10 = dt.Rows[i]["rate_buck_amt10"];
                                                            //objIds.hist_inv_number = dt.Rows[i]["hist_inv_number"];
                                                            //objIds.deliver_pricing_zone = dt.Rows[i]["deliver_pricing_zone"];
                                                            //objIds.pickup_actual_longitude = dt.Rows[i]["pickup_actual_longitude"];
                                                            //objIds.push_services = dt.Rows[i]["push_services"];
                                                            //objIds.add_charge_amt11 = dt.Rows[i]["add_charge_amt11"];
                                                            //objIds.rt_actual_location_accuracy = dt.Rows[i]["rt_actual_location_accuracy"];
                                                            //objIds.roundtrip_actual_date = dt.Rows[i]["roundtrip_actual_date"];
                                                            if (dt.Columns.Contains("pickup_requested_arr_time"))
                                                            {
                                                                objIds.pickup_requested_arr_time = dt.Rows[i]["pickup_requested_arr_time"];
                                                            }
                                                            //objIds.deliver_attention = dt.Rows[i]["deliver_attention"];
                                                            //objIds.deliver_special_instructions3 = dt.Rows[i]["deliver_special_instructions3"];
                                                            //objIds.pickup_actual_pieces = dt.Rows[i]["pickup_actual_pieces"];
                                                            //objIds.edi_order_accepted_or_rejected_text = dt.Rows[i]["edi_order_accepted_or_rejected_text"];
                                                            //objIds.edi_order_accepted_or_rejected = dt.Rows[i]["edi_order_accepted_or_rejected"];
                                                            //objIds.roundtrip_signature = dt.Rows[i]["roundtrip_signature"];
                                                            //objIds.po_number = dt.Rows[i]["po_number"];
                                                            if (dt.Columns.Contains("signature"))
                                                            {
                                                                objIds.signature = dt.Rows[i]["signature"];
                                                            }
                                                            //objIds.pickup_special_instructions2 = dt.Rows[i]["pickup_special_instructions2"];
                                                            //objIds.original_ctrl_number = dt.Rows[i]["original_ctrl_number"];
                                                            //objIds.previous_ctrl_number = dt.Rows[i]["previous_ctrl_number"];
                                                            //if (dt.Columns.Contains("Id"))
                                                            //{
                                                            //    objIds.id = dt.Rows[i]["Id"];
                                                            //}
                                                            idList.Add(objIds);

                                                        }
                                                        objCommon.SaveOutputDataToCsvFileParallely(idList, "Order-Create",
                                                           processingFileName, strDatetime);
                                                    }
                                                    if (dsOrderResponse.Tables.Contains("settlements"))
                                                    {
                                                        List<Settlement> settelmentList = new List<Settlement>();
                                                        for (int i = 0; i < dsOrderResponse.Tables["settlements"].Rows.Count; i++)
                                                        {
                                                            DataTable dt = dsOrderResponse.Tables["settlements"];
                                                            Settlement objsettlements = new Settlement();
                                                            //objsettlements.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                            if (dt.Columns.Contains("company_number"))
                                                            {
                                                                objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                            }
                                                            //objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                            //objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                            //objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                            //objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                            //objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                            //objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                            if (dt.Columns.Contains("order_date"))
                                                            {
                                                                objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                            }
                                                            //objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                            //objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                            //objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                            //objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                            //objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                            //objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                            //objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                            //objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                            //objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                            //objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                            //objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                            //objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                            //objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                            if (dt.Columns.Contains("driver_company_number"))
                                                            {
                                                                objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                            }
                                                            //objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                            //objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                            //objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                            //objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                            //objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                            //objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                            if (dt.Columns.Contains("id"))
                                                            {
                                                                objsettlements.id = (dt.Rows[i]["id"]);
                                                            }
                                                            if (dt.Columns.Contains("date_last_updated"))
                                                            {
                                                                objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                            }
                                                            //objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                            //objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                            //objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                            //objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                            if (dt.Columns.Contains("driver_number"))
                                                            {
                                                                objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                            }
                                                            //objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                            //objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                            //objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                            //objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                            //objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                            //objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                            //objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                            //objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                            //objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                            //objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                            if (dt.Columns.Contains("control_number"))
                                                            {
                                                                objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                            }
                                                            //objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                            //objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                            //objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                            //objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);
                                                            settelmentList.Add(objsettlements);
                                                        }

                                                        objCommon.SaveOutputDataToCsvFileParallely(settelmentList, "Order-Settlements-AddRecord",
                                                            processingFileName, strDatetime);

                                                    }
                                                    if (dsOrderResponse.Tables.Contains("progress"))
                                                    {

                                                        List<Progress> progressList = new List<Progress>();
                                                        for (int i = 0; i < dsOrderResponse.Tables["progress"].Rows.Count; i++)
                                                        {
                                                            Progress progress = new Progress();
                                                            DataTable dt = dsOrderResponse.Tables["progress"];
                                                            if (dt.Columns.Contains("status_date"))
                                                            {
                                                                progress.status_date = (dt.Rows[i]["status_date"]);
                                                            }
                                                            if (dt.Columns.Contains("status_text"))
                                                            {
                                                                progress.status_text = (dt.Rows[i]["status_text"]);
                                                            }
                                                            if (dt.Columns.Contains("status_time"))
                                                            {
                                                                progress.status_time = (dt.Rows[i]["status_time"]);
                                                            }
                                                            if (dt.Columns.Contains("id"))
                                                            {
                                                                progress.id = (dt.Rows[i]["id"]);
                                                            }
                                                            progressList.Add(progress);
                                                        }

                                                        objCommon.SaveOutputDataToCsvFileParallely(progressList, "Order-Progress",
                                                           processingFileName, strDatetime);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    strExecutionLogMessage = "ProcessAddOrderFiles Exception -" + ex.Message + System.Environment.NewLine;
                                                    strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                                    strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                                    strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                    //objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                                    //objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                    objCommon.WriteErrorLogParallelly(ex, fileName, strExecutionLogMessage);

                                                    ErrorResponse objErrorResponse = new ErrorResponse();
                                                    objErrorResponse.error = ex.Message;
                                                    objErrorResponse.status = "Error";
                                                    objErrorResponse.code = "Exception while writing the response into csv";
                                                    objErrorResponse.reference = ReferenceId;
                                                    string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                    DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                    dsFailureResponse.Tables[0].TableName = "OrderFailure";
                                                    objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                strInputFilePath, processingFileName, strDatetime);

                                                }
                                                //  if (driver1 != null) 
                                                if (objOrder.driver1 != null)
                                                {
                                                    if (dsOrderResponse.Tables.Contains("settlements"))
                                                    {
                                                        UniqueId = Convert.ToString(dsOrderResponse.Tables["settlements"].Rows[0]["id"]);

                                                        string ordersettlementputrequest = null;

                                                        int company_number = Convert.ToInt32(dsOrderResponse.Tables[0].Rows[0]["company_number"]);
                                                        int control_number = Convert.ToInt32(dsOrderResponse.Tables[0].Rows[0]["control_number"]);

                                                        int record_type = Convert.ToInt32(objCommon.GetConfigValue("OrderSettlement_record_type"));
                                                        string transaction_type = objCommon.GetConfigValue("OrderSettlement_transaction_type"); // 
                                                        string driver_sequence = objCommon.GetConfigValue("OrderSettlement_driver_sequence");

                                                        ordersettlementputrequest = @"'company_number': " + company_number + ",";
                                                        ordersettlementputrequest = ordersettlementputrequest + @"'control_number': " + control_number + ",";

                                                        if (record_type != null)
                                                        {
                                                            ordersettlementputrequest = ordersettlementputrequest + @"'record_type': " + record_type + ",";
                                                        }

                                                        if (dr.Table.Columns.Contains("Transaction Type"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Transaction Type"])))
                                                            {
                                                                transaction_type = Convert.ToString(dr["Transaction Type"]);
                                                            }
                                                        }

                                                        ordersettlementputrequest = ordersettlementputrequest + @"'transaction_type': '" + transaction_type + "',";

                                                        if (driver_sequence != null)
                                                        {
                                                            ordersettlementputrequest = ordersettlementputrequest + @"'driver_sequence': '" + driver_sequence + "',";
                                                        }
                                                        else
                                                        {
                                                            driver_sequence = "0";
                                                            ordersettlementputrequest = ordersettlementputrequest + @"'driver_sequence': '" + driver_sequence + "',";
                                                        }
                                                        if (dr.Table.Columns.Contains("Carrier Base Pay"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Carrier Base Pay"])))
                                                            {
                                                                ordersettlementputrequest = ordersettlementputrequest + @"'charge1': " + dr["Carrier Base Pay"] + ",";
                                                            }
                                                            else
                                                            {
                                                                strExecutionLogMessage = "OrderPost-OrderSettlementPut Error " + System.Environment.NewLine;
                                                                strExecutionLogMessage += "Carrier Base Pay Not found in the file -" + strFileName + System.Environment.NewLine;
                                                                strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                                //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                                                ErrorResponse objErrorResponse = new ErrorResponse();
                                                                objErrorResponse.error = "Carrier Base Pay Value Missing for this record";
                                                                objErrorResponse.status = "Error";
                                                                objErrorResponse.code = "Carrier Base Pay Value Missing";
                                                                objErrorResponse.reference = ReferenceId;
                                                                string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                                dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                                objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                            strInputFilePath, processingFileName, strDatetime);
                                                                continue;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            strExecutionLogMessage = "OrderPost-OrderSettlementPut Error " + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Carrier Base Pay Not found in the file -" + strFileName + System.Environment.NewLine;
                                                            strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                                            objErrorResponse.error = "Carrier Base Pay column not found for this record";
                                                            objErrorResponse.status = "Error";
                                                            objErrorResponse.code = "Carrier Base Pay column Missing";
                                                            objErrorResponse.reference = ReferenceId;
                                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                            dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                            objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                        strInputFilePath, processingFileName, strDatetime);
                                                            continue;

                                                        }
                                                        if (dr.Table.Columns.Contains("Carrier ACC"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Carrier ACC"])))
                                                            {
                                                                ordersettlementputrequest = ordersettlementputrequest + @"'charge5': " + dr["Carrier ACC"] + ",";
                                                            }
                                                            else
                                                            {
                                                                strExecutionLogMessage = "OrderPost-OrderSettlementPut Error " + System.Environment.NewLine;
                                                                strExecutionLogMessage += "Carrier ACC Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                                strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                                // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                                ErrorResponse objErrorResponse = new ErrorResponse();
                                                                objErrorResponse.error = "Carrier ACC value not found for this record";
                                                                objErrorResponse.status = "Error";
                                                                objErrorResponse.code = "Carrier ACC value Missing";
                                                                objErrorResponse.reference = ReferenceId;
                                                                string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                                dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                                objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                            strInputFilePath, processingFileName, strDatetime);
                                                                continue;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            strExecutionLogMessage = "OrderPost-OrderSettlementPut Error " + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Carrier ACC Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                            strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                                            objErrorResponse.error = "Carrier ACC column not found for this record";
                                                            objErrorResponse.status = "Error";
                                                            objErrorResponse.code = "Carrier ACC column Missing";
                                                            objErrorResponse.reference = ReferenceId;
                                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                            dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                            objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                        strInputFilePath, processingFileName, strDatetime);
                                                            continue;
                                                        }

                                                        if (dr.Table.Columns.Contains("Carrier FSC"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Carrier FSC"])))
                                                            {
                                                                ordersettlementputrequest = ordersettlementputrequest + @"'charge6': " + Convert.ToDouble(dr["Carrier FSC"]) + ",";
                                                            }
                                                        }

                                                        ordersettlementputrequest = @"{" + ordersettlementputrequest + "}";
                                                        string order_settlementObject = @"{'order_settlement': " + ordersettlementputrequest + "}";
                                                        JObject jsonobj = JObject.Parse(order_settlementObject);
                                                        request = jsonobj.ToString();

                                                        clsCommon.ReturnResponse objresponseOrdersettlement = new clsCommon.ReturnResponse();
                                                        objresponseOrdersettlement = objclsDatatrac.CallDataTracOrderSettlementPutAPI(UniqueId, order_settlementObject, objorderdetails.order.csr);
                                                        // objresponseOrdersettlement.Reason = "{\"002018724450D1\": {\"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"time_last_updated\": \"05:10\", \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"order_date\": \"2021-06-28\", \"agent_etrac_transaction_number\": null, \"settlement_bucket1_pct\": null, \"settlement_bucket2_pct\": null, \"vendor_employee_numer\": null, \"fuel_price_zone\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"charge1\": 35.91, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"adjustment_type\": null, \"agents_etrac_partner_id\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"vendor_invoice_number\": null, \"charge6\": null, \"settlement_bucket4_pct\": null, \"fuel_plan\": null, \"record_type\": 0, \"voucher_amount\": null, \"id\": \"002018724450D1\", \"date_last_updated\": \"2021-07-08\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"voucher_number\": null, \"driver_number_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver_number\": 3208, \"settlement_bucket6_pct\": null, \"settlement_pct\": 100.00, \"charge3\": null, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"voucher_date\": null, \"fuel_price_source\": null, \"pay_chart_used\": null, \"settlement_bucket5_pct\": null, \"charge2\": null, \"control_number\": 1872445, \"settlement_bucket3_pct\": null, \"charge5\": null, \"pre_book_percentage\": true, \"settlement_period_end_date\": null}}";
                                                        // objresponseOrdersettlement.ResponseVal = true;
                                                        if (objresponseOrdersettlement.ResponseVal)
                                                        {
                                                            strExecutionLogMessage = "OrderPost-OrderSettlementPutAPI Success " + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                            //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                                            DataSet dsOrderSettlementResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                                                            dsOrderSettlementResponse.Tables[0].TableName = "OrderSettlement";
                                                            try
                                                            {
                                                                List<ResponseOrderSettlements> orderSettlementstList = new List<ResponseOrderSettlements>();
                                                                for (int i = 0; i < dsOrderSettlementResponse.Tables["OrderSettlement"].Rows.Count; i++)
                                                                {
                                                                    DataTable dt = dsOrderSettlementResponse.Tables["OrderSettlement"];
                                                                    ResponseOrderSettlements objsettlements = new ResponseOrderSettlements();
                                                                    //objsettlements.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                                    if (dt.Columns.Contains("company_number"))
                                                                    {
                                                                        objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                                    }
                                                                    //objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                                    //objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                                    //objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                                    //objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                                    //objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                                    //objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                                    if (dt.Columns.Contains("order_date"))
                                                                    {
                                                                        objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                                    }
                                                                    //objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                                    //objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                                    //objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                                    //objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                                    //objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                                    //objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                                    //objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                                    //objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                                    //objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                                    //objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                                    //objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                                    //objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                                    //objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                                    if (dt.Columns.Contains("driver_company_number"))
                                                                    {
                                                                        objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                                    }
                                                                    //objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                                    //objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                                    //objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                                    //objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                                    //objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                                    //objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                                    if (dt.Columns.Contains("id"))
                                                                    {
                                                                        objsettlements.id = (dt.Rows[i]["id"]);
                                                                    }
                                                                    if (dt.Columns.Contains("date_last_updated"))
                                                                    {
                                                                        objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                                    }
                                                                    //objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                                    //objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                                    //objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                                    //objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                                    if (dt.Columns.Contains("driver_number"))
                                                                    {
                                                                        objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                                    }
                                                                    //objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                                    //objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                                    //objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                                    //objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                                    //objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                                    //objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                                    //objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                                    //objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                                    //objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                                    //objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                                    if (dt.Columns.Contains("control_number"))
                                                                    {
                                                                        objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                                    }
                                                                    //objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                                    //objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                                    //objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                                    //objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);


                                                                    //objsettlements.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                                    if (dt.Columns.Contains("company_number"))
                                                                    {
                                                                        objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                                    }
                                                                    //objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                                    //objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                                    //objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                                    //objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                                    //objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                                    //objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                                    if (dt.Columns.Contains("order_date"))
                                                                    {
                                                                        objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                                    }
                                                                    //objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                                    //objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                                    //objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                                    //objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                                    //objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                                    //objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                                    //objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                                    //objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                                    //objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                                    //objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                                    //objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                                    //objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                                    //objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                                    if (dt.Columns.Contains("driver_company_number"))
                                                                    {
                                                                        objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                                    }
                                                                    //objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                                    //objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                                    //objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                                    //objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                                    //objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                                    //objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                                    if (dt.Columns.Contains("id"))
                                                                    {
                                                                        objsettlements.id = (dt.Rows[i]["id"]);
                                                                    }
                                                                    if (dt.Columns.Contains("date_last_updated"))
                                                                    {
                                                                        objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                                    }
                                                                    //objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                                    //objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                                    //objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                                    //objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                                    if (dt.Columns.Contains("driver_number"))
                                                                    {
                                                                        objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                                    }
                                                                    //objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                                    //objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                                    //objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                                    //objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                                    //objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                                    //objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                                    //objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                                    //objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                                    //objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                                    //objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                                    if (dt.Columns.Contains("control_number"))
                                                                    {
                                                                        objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                                    }
                                                                    //objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                                    //objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                                    //objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                                    //objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);
                                                                    orderSettlementstList.Add(objsettlements);
                                                                }

                                                                objCommon.SaveOutputDataToCsvFileParallely(orderSettlementstList, "OrderSettlements-UpdatedRecord",
                                                                    processingFileName, strDatetime);
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                strExecutionLogMessage = "OrderPost-OrderSettlementPutAPI Success Exception -" + ex.Message + System.Environment.NewLine;
                                                                strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                                                strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                                                strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                                //objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                                                //objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                                objCommon.WriteErrorLogParallelly(ex, fileName, strExecutionLogMessage);

                                                                ErrorResponse objErrorResponse = new ErrorResponse();
                                                                objErrorResponse.error = ex.Message;
                                                                objErrorResponse.status = "Error";
                                                                objErrorResponse.code = "Exception while writing OrderPost-OrderSettlementPutAPI Success response into csv";
                                                                objErrorResponse.reference = ReferenceId;
                                                                string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                                dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                                objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                            strInputFilePath, processingFileName, strDatetime);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            strExecutionLogMessage = "OrderPost-OrderSettlementPutAPI Failed " + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                            DataSet dsOrderPutFailureResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                                                            dsOrderPutFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                            dsOrderPutFailureResponse.Tables[0].Columns.Add("UniqueId", typeof(System.String));
                                                            foreach (DataRow row in dsOrderPutFailureResponse.Tables[0].Rows)
                                                            {
                                                                row["UniqueId"] = UniqueId;
                                                            }
                                                            objCommon.WriteDataToCsvFileParallely(dsOrderPutFailureResponse.Tables[0],
                                                            strInputFilePath, processingFileName, strDatetime);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                request = JsonConvert.SerializeObject(objorderdetails);
                                                data = Regex.Replace(request, @"\\t", "");
                                                request = Regex.Replace(data, @"\\""", "");
                                                strExecutionLogMessage = "OrderPostAPI Failed " + System.Environment.NewLine;
                                                strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                                // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(objresponse.Reason);
                                                dsFailureResponse.Tables[0].TableName = "OrderFailure";
                                                dsFailureResponse.Tables[0].Columns.Add("Customer Reference", typeof(System.String));
                                                foreach (DataRow row in dsFailureResponse.Tables[0].Rows)
                                                {
                                                    row["Customer Reference"] = objOrder.reference;
                                                }
                                                objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                            strInputFilePath, processingFileName, strDatetime);

                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        strExecutionLogMessage = "ProcessAddOrderFiles Exception -" + ex.Message + System.Environment.NewLine;
                                        strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                        strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                        strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                        //  objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                        //objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                        objCommon.WriteErrorLogParallelly(ex, fileName, strExecutionLogMessage);

                                        ErrorResponse objErrorResponse = new ErrorResponse();
                                        objErrorResponse.error = ex.Message;
                                        objErrorResponse.status = "Error";
                                        objErrorResponse.code = "Exception while generating the Order Post Request";
                                        objErrorResponse.reference = ReferenceId;
                                        string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                        DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                        dsFailureResponse.Tables[0].TableName = "OrderFailure";
                                        objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                    strInputFilePath, processingFileName, strDatetime);
                                    }
                                }

                            });

                            objCommon.MergeSplittedOutputFiles(strFileName, "Order-Create", strDatetime);
                            objCommon.MergeSplittedOutputFiles(strFileName, "Order-Progress", strDatetime);
                            objCommon.MergeSplittedOutputFiles(strFileName, "Order-Settlements-AddRecord", strDatetime);
                            objCommon.MergeSplittedOutputFiles(strFileName, "OrderSettlements-UpdatedRecord", strDatetime);
                            objCommon.MergeSplittedOutputFiles(strFileName, "OrderFailure", strDatetime);
                            objCommon.MergeSplittedOutputFiles(strFileName, "OrderSettlementFailure", strDatetime);
                            objCommon.MoveMergedOutputFilesToOutputLocation(strInputFilePath);
                            objCommon.CleanSplittedOutputFilesWorkingFolder();

                            strExecutionLogMessage = "Parallelly Processing  finished for the  file : " + strFileName + "." + System.Environment.NewLine;
                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                        }
                        else
                        {
                            strExecutionLogMessage = "Template sheet data not found for the file " + strInputFilePath + @"\" + strFileName;
                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        strExecutionLogMessage = "ProcessAddOrderFiles Exception -" + ex.Message + System.Environment.NewLine;
                        strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                        strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                        strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                        // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                        objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                    }
                }
                strExecutionLogMessage = "Finished processing all the files for the location " + strInputFilePath;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
            }
            catch (Exception ex)
            {
                // MsgBox(ex.Message).ToString()
                objCommon.WriteErrorLog(ex);
                throw new Exception("Error in ProcessAddOrderFiles -->" + ex.Message + ex.StackTrace);
            }
        }

        private static void ProcessUpdateOrderFiles(string strInputFilePath, string strLocationFolder)
        {
            clsCommon objCommon = new clsCommon();
            try
            {

                System.Configuration.AppSettingsReader reader = new System.Configuration.AppSettingsReader();
                DirectoryInfo dir;
                FileInfo[] XLSfiles;
                string strFileName;
                //string strInputFilePath;
                string strBillingHistoryFileLocation;
                string strExecutionLogMessage;
                string strExecutionLogFileLocation;
                DataTable dtDataTable;
                string strDatetime;
                //  string strSheetName;
                //string ReferenceId = null;
                string UniqueId = null;
                strExecutionLogFileLocation = objCommon.GetConfigValue("ExecutionLogFileLocation");

                strExecutionLogMessage = "Processing the Order Put Data for the location " + strLocationFolder;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                // strInputFilePath = objCommon.GetConfigValue("AutomationFileLocation") + @"\Order\Update"; ;
                //  strInputFilePath = strInputFilePath + @"\" + strLocationFolder;
                strBillingHistoryFileLocation = strInputFilePath + @"\HistoricalFiles";

                strExecutionLogMessage = "The input file Path is: " + strInputFilePath + "." + System.Environment.NewLine + "The Historical File Path is:" + strBillingHistoryFileLocation;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                dir = new DirectoryInfo(strInputFilePath);
                XLSfiles = dir.GetFiles("*.xlsx");

                strExecutionLogMessage = "Found # of Excel Files: " + XLSfiles.Count();
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                foreach (var file in XLSfiles)
                {
                    strFileName = file.ToString();
                    dtDataTable = new System.Data.DataTable();

                    try
                    {

                        DataSet dsExcel = new DataSet();
                        dsExcel = clsExcelHelper.ImportExcelXLSX(strInputFilePath + @"\" + strFileName, false);
                        if (dsExcel.Tables.Count > 0)
                        {

                            objCommon.MoveTheFileToHistoryFolder(strBillingHistoryFileLocation, file);
                            strDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");
                            for (int i = dsExcel.Tables[0].Rows.Count - 1; i >= 0; i--)
                            {
                                DataRow dr = dsExcel.Tables[0].Rows[i];
                                if (dr["Company"] == DBNull.Value && dr["Control Number"] == DBNull.Value)
                                    dr.Delete();
                            }
                            dsExcel.Tables[0].AcceptChanges();

                            int noofrowspertable = Convert.ToInt16(objCommon.GetConfigValue("DevideToProcessParallelly"));
                            List<DataTable> splitdt = clsCommon.SplitTable(dsExcel.Tables[0], noofrowspertable, strFileName, strDatetime);

                            strExecutionLogMessage = "Parallelly Processing Statred for the  file : " + strFileName + "." + System.Environment.NewLine + "Number of processess are going to exicute is :" + noofrowspertable;
                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                            objCommon.CleanSplittedOutputFilesWorkingFolder();


                            Parallel.ForEach(splitdt, currentDatatable =>
                            {
                                var fileName = currentDatatable.TableName;
                                var processingFileName = currentDatatable.TableName;
                                strExecutionLogMessage = "Current Processing File is  : " + fileName + "." + System.Environment.NewLine;
                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                var datatable = currentDatatable;
                                int rowindex = 1;
                                foreach (DataRow dr in datatable.Rows)
                                {
                                    //object value = dr["Company"];
                                    //if (value == DBNull.Value)
                                    //    break;

                                    try
                                    {
                                        clsDatatrac objclsDatatrac = new clsDatatrac();
                                        string orderputrequest = null;
                                        if (dr.Table.Columns.Contains("Company"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Company"])))
                                            {
                                                orderputrequest = @"'company_number': " + dr["Company"] + ",";
                                            }
                                            else
                                            {
                                                strExecutionLogMessage = "OrderPut Error " + System.Environment.NewLine;
                                                strExecutionLogMessage += "Company Number Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                strExecutionLogMessage += "For row  number -" + rowindex + System.Environment.NewLine;
                                                // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                ErrorResponse objErrorResponse = new ErrorResponse();
                                                objErrorResponse.error = "Company Number not found for this record";
                                                objErrorResponse.status = "Error";
                                                objErrorResponse.code = "Company Value Missing";
                                                objErrorResponse.reference = "For row  number -" + rowindex;
                                                string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                dsFailureResponse.Tables[0].TableName = "OrderPutFailure";
                                                objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                            strInputFilePath, processingFileName, strDatetime);
                                                rowindex++;
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "OrderPut Error " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Company Number Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                            //  objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                            objErrorResponse.error = "Company column not found for this file record";
                                            objErrorResponse.status = "Error";
                                            objErrorResponse.code = "Company column Missing";
                                            objErrorResponse.reference = "For row  number -" + rowindex;
                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                            dsFailureResponse.Tables[0].TableName = "OrderPutFailure";
                                            objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                        strInputFilePath, processingFileName, strDatetime);
                                            rowindex++;
                                            continue;
                                        }
                                        if (dr.Table.Columns.Contains("Control Number"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Control Number"])))
                                            {
                                                orderputrequest = orderputrequest + @"'control_number': " + Convert.ToInt32(dr["Control Number"]) + ",";
                                            }
                                            else
                                            {
                                                strExecutionLogMessage = "OrderPut Error " + System.Environment.NewLine;
                                                strExecutionLogMessage += "Control Number Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                                //  objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);


                                                ErrorResponse objErrorResponse = new ErrorResponse();
                                                objErrorResponse.error = "Control Number value not found for this  record";
                                                objErrorResponse.status = "Error";
                                                objErrorResponse.code = "Control Number Value Missing";
                                                objErrorResponse.reference = "For row  number -" + rowindex;
                                                string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                dsFailureResponse.Tables[0].TableName = "OrderPutFailure";
                                                objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                            strInputFilePath, processingFileName, strDatetime);
                                                rowindex++;
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "OrderPut Error " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Control Number Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                            //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                            objErrorResponse.error = "Control Number column not found for this record";
                                            objErrorResponse.status = "Error";
                                            objErrorResponse.code = "Control Number column Missing";
                                            objErrorResponse.reference = "For row  number -" + rowindex;
                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                            dsFailureResponse.Tables[0].TableName = "OrderPutFailure";
                                            objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                        strInputFilePath, processingFileName, strDatetime);
                                            rowindex++;
                                            continue;
                                        }


                                        if (dr.Table.Columns.Contains("Service Type"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Service Type"])))
                                            {
                                                orderputrequest = orderputrequest + @"'service_level': " + Convert.ToInt32(dr["Service Type"]) + ",";
                                            }
                                        }
                                        if (dr.Table.Columns.Contains("Billing Customer Number"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Billing Customer Number"])))
                                            {
                                                orderputrequest = orderputrequest + @"'customer_number': " + dr["Billing Customer Number"] + ", ";
                                            }
                                        }

                                        //  objOrder.reference = Convert.ToString(dr["Customer Reference"]);
                                        if (dr.Table.Columns.Contains("Customer Reference"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Customer Reference"])))
                                            {
                                                orderputrequest = orderputrequest + @"'reference': '" + Convert.ToString(dr["Customer Reference"]) + "',";
                                            }
                                        }

                                        //  DateTime dtValue = Convert.ToDateTime(dr["Delivery Date"]);
                                        // objOrder.pickup_requested_date = dtValue.ToString("yyyy-MM-dd");
                                        // dtValue = Convert.ToDateTime(dr["Delivery Date"]);
                                        // objOrder.pickup_actual_date = dtValue.ToString("yyyy-MM-dd");

                                        if (dr.Table.Columns.Contains("Delivery Date"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Delivery Date"])))
                                            {
                                                DateTime dtValue = Convert.ToDateTime(dr["Delivery Date"]);
                                                // objOrder.pickup_requested_date = dtValue.ToString("yyyy-MM-dd");
                                                orderputrequest = orderputrequest + @"'pickup_requested_date': '" + dtValue.ToString("yyyy-MM-dd") + "',";
                                                orderputrequest = orderputrequest + @"'pickup_actual_date': '" + dtValue.ToString("yyyy-MM-dd") + "',";
                                                orderputrequest = orderputrequest + @"'deliver_requested_date': '" + dtValue.ToString("yyyy-MM-dd") + "',";
                                                orderputrequest = orderputrequest + @"'deliver_actual_date': '" + dtValue.ToString("yyyy-MM-dd") + "',";
                                            }
                                        }

                                        if (dr.Table.Columns.Contains("Pickup actual arrival time"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup actual arrival time"])))
                                            {
                                                DateTime dtValue = Convert.ToDateTime(dr["Pickup actual arrival time"]);
                                                orderputrequest = orderputrequest + @"'pickup_actual_arr_time': '" + dtValue.ToString("HH:mm") + "', ";
                                            }
                                        }

                                        if (dr.Table.Columns.Contains("Pickup actual depart time"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup actual depart time"])))
                                            {
                                                DateTime dtValue = Convert.ToDateTime(dr["Pickup actual depart time"]);
                                                orderputrequest = orderputrequest + @"'pickup_actual_dep_time': '" + dtValue.ToString("HH:mm") + "', ";
                                            }
                                        }

                                        if (dr.Table.Columns.Contains("Pickup will be ready by"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup will be ready by"])))
                                            {
                                                DateTime dtValue = Convert.ToDateTime(dr["Pickup will be ready by"]);
                                                orderputrequest = orderputrequest + @"'pickup_requested_arr_time': '" + dtValue.ToString("HH:mm") + "', ";
                                            }
                                        }

                                        if (dr.Table.Columns.Contains("Pickup no later than"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup no later than"])))
                                            {
                                                DateTime dtValue = Convert.ToDateTime(dr["Pickup no later than"]);
                                                orderputrequest = orderputrequest + @"'pickup_requested_dep_time': '" + dtValue.ToString("HH:mm") + "', ";
                                            }
                                        }

                                        //  objOrder.pickup_name = Convert.ToString(dr["Store Code"]);
                                        if (dr.Table.Columns.Contains("Pickup name"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup name"])))
                                            {
                                                orderputrequest = orderputrequest + @"'pickup_name': '" + Convert.ToString(dr["Pickup name"]) + "',";
                                            }
                                        }

                                        //objOrder.pickup_address = Convert.ToString(dsLocationDetails.Tables[0].Rows[0]["Pickup address"]);

                                        if (dr.Table.Columns.Contains("Pickup address"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup address"])))
                                            {
                                                orderputrequest = orderputrequest + @"'pickup_address': '" + Convert.ToString(dr["Pickup address"]) + "',";
                                            }
                                        }

                                        // objOrder.pickup_city = Convert.ToString(dsLocationDetails.Tables[0].Rows[0]["Pickup city"]);

                                        if (dr.Table.Columns.Contains("Pickup city"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup city"])))
                                            {
                                                orderputrequest = orderputrequest + @"'pickup_city': '" + Convert.ToString(dr["Pickup city"]) + "',";
                                            }
                                        }

                                        //  objOrder.pickup_state = Convert.ToString(dsLocationDetails.Tables[0].Rows[0]["Pickup state/province"]);

                                        if (dr.Table.Columns.Contains("Pickup state/province"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup state/province"])))
                                            {
                                                orderputrequest = orderputrequest + @"'pickup_state': '" + Convert.ToString(dr["Pickup state/province"]) + "',";
                                            }
                                        }

                                        // objOrder.pickup_zip = Convert.ToString(dsLocationDetails.Tables[0].Rows[0]["Pickup zip/postal code"]);

                                        if (dr.Table.Columns.Contains("Pickup zip/postal code"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup zip/postal code"])))
                                            {
                                                string strPickupzip = Convert.ToString(dr["Pickup zip/postal code"]);
                                                if (strPickupzip.Length > 5)
                                                {
                                                    strPickupzip = strPickupzip.Substring(0, 5) + "-" + strPickupzip.Substring(5, strPickupzip.Length - 5); ;
                                                }
                                                orderputrequest = orderputrequest + @"'pickup_zip': '" + strPickupzip + "',";
                                            }
                                        }

                                        //dtValue = Convert.ToDateTime(dr["Delivery Date"]);
                                        // objOrder.deliver_requested_date = dtValue.ToString("yyyy-MM-dd");

                                        //dtValue = Convert.ToDateTime(dsLocationDetails.Tables[0].Rows[0]["Deliver no earlier than"]);
                                        //objOrder.deliver_requested_arr_time = dtValue.ToString("HH:mm");

                                        if (dr.Table.Columns.Contains("Deliver no earlier than"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Deliver no earlier than"])))
                                            {
                                                DateTime dtValue = Convert.ToDateTime(dr["Deliver no earlier than"]);
                                                orderputrequest = orderputrequest + @"'deliver_requested_arr_time': '" + dtValue.ToString("HH:mm") + "', ";
                                            }
                                        }

                                        //dtValue = Convert.ToDateTime(dsLocationDetails.Tables[0].Rows[0]["Deliver no later than"]);
                                        //objOrder.deliver_requested_dep_time = dtValue.ToString("HH:mm");

                                        if (dr.Table.Columns.Contains("Deliver no later than"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Deliver no later than"])))
                                            {
                                                DateTime dtValue = Convert.ToDateTime(dr["Deliver no later than"]);
                                                orderputrequest = orderputrequest + @"'deliver_requested_dep_time': '" + dtValue.ToString("HH:mm") + "', ";
                                            }
                                        }

                                        //dtValue = Convert.ToDateTime(dr["Delivery Date"]);
                                        //objOrder.deliver_actual_date = dtValue.ToString("yyyy-MM-dd");

                                        //dtValue = Convert.ToDateTime(dsLocationDetails.Tables[0].Rows[0]["Delivery actual arrive time"]);
                                        //objOrder.deliver_actual_arr_time = dtValue.ToString("HH:mm");

                                        if (dr.Table.Columns.Contains("Delivery actual arrive time"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Delivery actual arrive time"])))
                                            {
                                                DateTime dtValue = Convert.ToDateTime(dr["Delivery actual arrive time"]);
                                                orderputrequest = orderputrequest + @"'deliver_actual_arr_time': '" + dtValue.ToString("HH:mm") + "', ";
                                            }
                                        }

                                        //dtValue = Convert.ToDateTime(dsLocationDetails.Tables[0].Rows[0]["Delivery actual depart time"]);
                                        //objOrder.deliver_actual_dep_time = dtValue.ToString("HH:mm");

                                        if (dr.Table.Columns.Contains("Delivery actual depart time"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Delivery actual depart time"])))
                                            {
                                                DateTime dtValue = Convert.ToDateTime(dr["Delivery actual depart time"]);
                                                orderputrequest = orderputrequest + @"'deliver_actual_dep_time': '" + dtValue.ToString("HH:mm") + "', ";
                                            }
                                        }

                                        // objOrder.deliver_name = Convert.ToString(dr["Customer Name"]);

                                        if (dr.Table.Columns.Contains("Customer Name"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Customer Name"])))
                                            {
                                                orderputrequest = orderputrequest + @"'deliver_name': '" + Convert.ToString(dr["Customer Name"]) + "',";
                                            }
                                        }

                                        //  objOrder.deliver_address = Convert.ToString(dr["Address"]);

                                        if (dr.Table.Columns.Contains("Address"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Address"])))
                                            {
                                                orderputrequest = orderputrequest + @"'deliver_address': '" + Convert.ToString(dr["Address"]) + "',";
                                            }
                                        }

                                        // objOrder.deliver_city = Convert.ToString(dr["City"]);

                                        if (dr.Table.Columns.Contains("City"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["City"])))
                                            {
                                                orderputrequest = orderputrequest + @"'deliver_city': '" + Convert.ToString(dr["City"]) + "',";
                                            }
                                        }

                                        //objOrder.deliver_state = Convert.ToString(dr["State"]);

                                        if (dr.Table.Columns.Contains("State"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["State"])))
                                            {
                                                orderputrequest = orderputrequest + @"'deliver_state': '" + Convert.ToString(dr["State"]) + "',";
                                            }
                                        }

                                        //  objOrder.deliver_zip = Convert.ToString(dr["Zip"]);

                                        if (dr.Table.Columns.Contains("Zip"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Zip"])))
                                            {
                                                string strzip = Convert.ToString(dr["Zip"]);
                                                if (strzip.Length > 5)
                                                {
                                                    strzip = strzip.Substring(0, 5) + "-" + strzip.Substring(5, strzip.Length - 5); ;
                                                }
                                                orderputrequest = orderputrequest + @"'deliver_zip': '" + strzip + "',";
                                            }
                                        }


                                        // objOrder.signature = Convert.ToString(dsLocationDetails.Tables[0].Rows[0]["Delivery text signature"]);

                                        if (dr.Table.Columns.Contains("Delivery actual depart time"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Delivery actual depart time"])))
                                            {
                                                orderputrequest = orderputrequest + @"'signature': '" + Convert.ToString(dr["Delivery actual depart time"]) + "', ";
                                            }
                                        }


                                        //  objOrder.rate_buck_amt1 = Convert.ToDouble(dr["Bill Rate"]);

                                        if (dr.Table.Columns.Contains("Bill Rate"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Bill Rate"])))
                                            {
                                                orderputrequest = orderputrequest + @"'rate_buck_amt1': " + Convert.ToDouble(dr["Bill Rate"]) + ",";
                                            }
                                        }

                                        //objOrder.rate_buck_amt3 = Convert.ToDouble(dr["Pieces ACC"]);

                                        if (dr.Table.Columns.Contains("Pieces ACC"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Pieces ACC"])))
                                            {
                                                orderputrequest = orderputrequest + @"'rate_buck_amt3': " + Convert.ToDouble(dr["Pieces ACC"]) + ",";
                                            }
                                        }

                                        //  objOrder.rate_buck_amt10 = Convert.ToDouble(dr["FSC"]);

                                        if (dr.Table.Columns.Contains("FSC"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["FSC"])))
                                            {
                                                orderputrequest = orderputrequest + @"'rate_buck_amt10': " + Convert.ToDouble(dr["FSC"]) + ",";
                                            }
                                        }

                                        //objOrder.number_of_pieces = Convert.ToInt32(dr["Pieces"]);

                                        if (dr.Table.Columns.Contains("Pieces"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Pieces"])))
                                            {
                                                orderputrequest = orderputrequest + @"'number_of_pieces': " + Convert.ToInt32(dr["Pieces"]) + ",";
                                            }
                                        }

                                        // objOrder.rate_miles = Convert.ToInt32(dr["Miles"]);

                                        if (dr.Table.Columns.Contains("Miles"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Miles"])))
                                            {
                                                //  orderputrequest = orderputrequest + @"'rate_miles': " + Convert.ToInt32(dr["Miles"]) + ",";
                                                orderputrequest = orderputrequest + @"'rate_miles': " + Convert.ToInt32(Convert.ToDouble(dr["Miles"])) + ",";
                                            }
                                        }

                                        //if (!string.IsNullOrEmpty(Convert.ToString(dr["Correct Driver Number"])))
                                        //{
                                        //    objOrder.driver1 = Convert.ToInt32(dr["Correct Driver Number"]);
                                        //}

                                        string driver1 = null;

                                        if (dr.Table.Columns.Contains("Correct Driver Number"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Correct Driver Number"])))
                                            {
                                                driver1 = Convert.ToString(dr["Correct Driver Number"]);
                                                orderputrequest = orderputrequest + @"'driver1': " + Convert.ToInt32(dr["Correct Driver Number"]) + ",";
                                            }

                                        }

                                        //objOrder.ordered_by = Convert.ToString(dsLocationDetails.Tables[0].Rows[0]["Requested by"]);

                                        if (dr.Table.Columns.Contains("Requested by"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Requested by"])))
                                            {
                                                orderputrequest = orderputrequest + @"'ordered_by': '" + Convert.ToString(dr["Requested by"]) + "', ";
                                            }
                                        }

                                        // objOrder.csr = Convert.ToString(dsLocationDetails.Tables[0].Rows[0]["Entered by"]);

                                        if (dr.Table.Columns.Contains("Entered by"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Entered by"])))
                                            {
                                                orderputrequest = orderputrequest + @"'csr': '" + Convert.ToString(dr["Entered by"]) + "', ";
                                            }
                                        }

                                        //objOrder.add_charge_amt1 = Convert.ToDouble(dr["Carrier Base Pay"]);

                                        //if (dr.Table.Columns.Contains("Carrier Base Pay"))
                                        //{
                                        //    if (!string.IsNullOrEmpty(Convert.ToString(dr["Carrier Base Pay"])))
                                        //    {
                                        //        orderputrequest = orderputrequest + @"'add_charge_amt1': " + Convert.ToDouble(dr["Carrier Base Pay"]) + ",";
                                        //    }
                                        //}

                                        //objOrder.add_charge_amt5 = Convert.ToDouble(dr["Carrier ACC"]);

                                        //if (dr.Table.Columns.Contains("Carrier ACC"))
                                        //{
                                        //    if (!string.IsNullOrEmpty(Convert.ToString(dr["Carrier ACC"])))
                                        //    {
                                        //        orderputrequest = orderputrequest + @"'add_charge_amt5': " + Convert.ToDouble(dr["Carrier ACC"]) + ",";
                                        //    }
                                        //}

                                        // objOrder.pick_del_trans_flag = Convert.ToString(dsLocationDetails.Tables[0].Rows[0]["Pickup Delivery Transfer Flag"]);

                                        if (dr.Table.Columns.Contains("Pickup Delivery Transfer Flag"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup Delivery Transfer Flag"])))
                                            {
                                                orderputrequest = orderputrequest + @"'pick_del_trans_flag': '" + Convert.ToString(dr["Pickup Delivery Transfer Flag"]) + "', ";
                                            }
                                        }

                                        //  objOrder.pickup_signature = Convert.ToString(dsLocationDetails.Tables[0].Rows[0]["Pickup signature"]);

                                        if (dr.Table.Columns.Contains("Pickup text signature"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Pickup text signature"])))
                                            {
                                                orderputrequest = orderputrequest + @"'pickup_signature': '" + Convert.ToString(dr["Pickup text signature"]) + "', ";
                                            }
                                        }
                                        //objorderdetails.order = objOrder;
                                        string status_code = null;
                                        if (dr.Table.Columns.Contains("status_code"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["status_code"])))
                                            {
                                                status_code = Convert.ToString(dr["status_code"]);
                                                orderputrequest = orderputrequest + @"'status_code': '" + Convert.ToString(dr["status_code"]) + "', ";
                                            }
                                        }
                                        orderputrequest = @"{" + orderputrequest + "}";
                                        string orderObject = @"{'order': " + orderputrequest + "}";

                                        if (status_code != null && status_code.ToUpper() == "C")
                                        {
                                            clsCommon.ReturnResponse objresponse = new clsCommon.ReturnResponse();
                                            UniqueId = objclsDatatrac.GenerateUniqueNumber(Convert.ToInt32(dr["Company"]), Convert.ToInt32(dr["Control Number"]));
                                            objresponse = objclsDatatrac.CallDataTracOrderCancelAPI(UniqueId);
                                            if (objresponse.ResponseVal)
                                            {
                                                strExecutionLogMessage = "OrderCancelAPI Success " + System.Environment.NewLine;
                                                strExecutionLogMessage += "UniqueId -" + UniqueId + System.Environment.NewLine;
                                                strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                                DataTable dtsuccess = new DataTable();
                                                dtsuccess.Clear();
                                                dtsuccess.Columns.Add("UniqueId");
                                                dtsuccess.Columns.Add("Status");
                                                DataRow _newRow = dtsuccess.NewRow();
                                                _newRow["UniqueId"] = UniqueId;
                                                _newRow["Status"] = objresponse.Reason;
                                                dtsuccess.Rows.Add(_newRow);
                                                dtsuccess.TableName = "OrderCancelSuccess";

                                                objCommon.WriteDataToCsvFileParallely(dtsuccess,
                                                            strInputFilePath, processingFileName, strDatetime);
                                            }
                                            else
                                            {
                                                strExecutionLogMessage = "OrderCancelAPI Failed " + System.Environment.NewLine;
                                                strExecutionLogMessage += "UniqueId -" + UniqueId + System.Environment.NewLine;
                                                strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(objresponse.Reason);
                                                dsFailureResponse.Tables[0].TableName = "OrderCancelFailure";
                                                dsFailureResponse.Tables[0].Columns.Add("UniqueId", typeof(System.String));
                                                foreach (DataRow row in dsFailureResponse.Tables[0].Rows)
                                                {
                                                    row["UniqueId"] = UniqueId;
                                                }
                                                objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                            strInputFilePath, processingFileName, strDatetime);
                                            }
                                        }
                                        else
                                        {
                                            JObject jsonobj = JObject.Parse(orderObject);
                                            string request = jsonobj.ToString();
                                            clsCommon.ReturnResponse objresponse = new clsCommon.ReturnResponse();
                                            UniqueId = objclsDatatrac.GenerateUniqueNumber(Convert.ToInt32(dr["Company"]), Convert.ToInt32(dr["Control Number"]));
                                            objresponse = objclsDatatrac.CallDataTracOrderPutAPI(UniqueId, orderObject);
                                            //objresponse.Reason = "{\"error\": \"Unable to perform API call object with RecordID:WS_OPORDR-9990111870 - The record may have been deleted.\", \"status\": \"error\", \"code\": \"U.RecordNotFound\"}";
                                            //objresponse.ResponseVal = false;
                                            // objresponse.Reason = "{\"002018716980\": {\"csr\": \"DX*\", \"cod_text\": \"No\", \"cod\": \"N\", \"edi_acknowledgement_required\": false, \"control_number\": 1871698, \"deliver_requested_arr_time\": \"08:00\", \"return_svc_level\": null, \"pickup_phone_ext\": null, \"customers_etrac_partner_id\": \"96609250\", \"distribution_unique_id\": 0, \"custom_special_instr_long\": null, \"exception_sign_required\": false, \"po_number\": null, \"exception_code\": null, \"add_charge_occur4\": null, \"frequent_caller_id\": null, \"push_services\": null, \"pickup_omw_longitude\": null, \"deliver_special_instructions2\": null, \"deliver_actual_dep_time\": \"08:15\", \"house_airway_bill_number\": null, \"invoice_period_end_date\": null, \"line_items\": [], \"pickup_eta_date\": null, \"dl_arrive_notification_sent\": false, \"hist_inv_date\": null, \"add_charge_occur6\": null, \"add_charge_code12\": null, \"add_charge_occur5\": null, \"pickup_route_seq\": null, \"add_charge_code9\": null, \"callback_time\": null, \"add_charge_amt12\": null, \"add_charge_occur3\": null, \"deliver_eta_date\": null, \"ordered_by\": \"RYDER\", \"deliver_actual_latitude\": null, \"pickup_airport_code\": null, \"rate_buck_amt9\": null, \"deliver_pricing_zone\": 1, \"add_charge_code4\": null, \"rate_buck_amt5\": null, \"total_pages\": 1, \"roundtrip_actual_arrival_time\": null, \"pickup_requested_date\": \"2021-05-10\", \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"pickup_wait_time\": null, \"pickup_special_instructions4\": null, \"deliver_special_instructions1\": null, \"deliver_room\": null, \"roundtrip_actual_date\": null, \"rescheduled_ctrl_number\": null, \"insurance_amount\": null, \"deliver_city\": \"STAFFORD\", \"progress\": [{\"status_text\": \"Entered in carrier's system\", \"status_time\": \"12:27:00\", \"status_date\": \"2021-07-05\"}, {\"status_text\": \"Picked up\", \"status_time\": \"08:30:00\", \"status_date\": \"2021-05-10\"}, {\"status_text\": \"Delivered\", \"status_time\": \"08:15:00\", \"status_date\": \"2021-05-10\"}], \"deliver_eta_time\": null, \"rate_buck_amt2\": null, \"previous_ctrl_number\": null, \"pickup_city\": \"SANDSTON\", \"pickup_special_instructions1\": null, \"deliver_route_sequence\": null, \"pickup_actual_pieces\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"signature_required\": true, \"deliver_actual_date\": \"2021-05-10\", \"callback_userid\": null, \"pickup_requested_dep_time\": \"09:00\", \"cod_accept_cashiers_check\": false, \"signature_images\": [], \"add_charge_amt8\": null, \"add_charge_amt3\": null, \"pickup_zip\": \"23150\", \"original_ctrl_number\": null, \"calc_add_on_chgs\": false, \"original_schedule_number\": null, \"email_addresses\": null, \"pickup_actual_dep_time\": \"08:30\", \"pickup_special_instructions3\": null, \"etrac_number\": \"026-1k19-1h0-q08-z93\", \"fuel_plan\": null, \"pickup_address_point_number_text\": \"HUMAN TOUCH\", \"pickup_address_point_number\": 19891, \"pickup_latitude\": 37.53250820, \"add_charge_amt5\": null, \"verified_weight\": null, \"pickup_sign_req\": true, \"exception_timestamp\": null, \"add_charge_occur10\": null, \"deliver_special_instructions3\": null, \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"add_charge_code1\": null, \"deliver_name\": \"MICHAEL BROWN\", \"az_equip2\": null, \"rate_buck_amt3\": null, \"bringg_send_sms\": false, \"pickup_actual_date\": \"2021-05-10\", \"page_number\": 1, \"pickup_signature\": \"SOF\", \"bringg_order_id\": null, \"pu_arrive_notification_sent\": false, \"roundtrip_actual_pieces\": null, \"pickup_actual_latitude\": null, \"manual_notepad\": false, \"roundtrip_sign_req\": false, \"deliver_zip\": \"22554\", \"rate_buck_amt8\": null, \"add_charge_occur11\": null, \"holiday_groups\": null, \"delivery_latitude\": 38.37859180, \"rate_buck_amt4\": null, \"pickup_point_customer\": 31025, \"rate_miles\": null, \"pickup_special_instructions2\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"bol_number\": null, \"rate_chart_used\": 0, \"rate_buck_amt6\": null, \"add_charge_occur1\": null, \"deliver_phone\": null, \"rate_buck_amt11\": null, \"deliver_actual_pieces\": null, \"deliver_attention\": null, \"driver1_text\": \"LAVERT KENDALL MORRIS\", \"driver1\": 3001, \"zone_set_used\": 1, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"add_charge_amt4\": null, \"pickup_pricing_zone\": 1, \"pickup_country\": null, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"deliver_omw_timestamp\": null, \"weight\": null, \"fuel_price_source\": null, \"pickup_omw_timestamp\": null, \"add_charge_code6\": null, \"photos_exist\": false, \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"pickup_room\": null, \"deliver_omw_latitude\": null, \"callback_date\": null, \"actual_miles\": null, \"delivery_longitude\": -77.31366340, \"image_sign_req\": false, \"additional_drivers\": false, \"pickup_attention\": null, \"reference_text\": \"DAYA1\", \"reference\": \"DAYA1\", \"roundtrip_wait_time\": null, \"add_charge_code11\": null, \"pickup_special_instr_long\": null, \"add_charge_code3\": null, \"add_charge_amt1\": null, \"callback_to\": null, \"date_order_entered\": \"2021-07-05\", \"rate_special_instructions\": null, \"hist_inv_number\": 0, \"roundtrip_signature\": null, \"bringg_last_loc_sent\": null, \"pickup_route_code\": null, \"pickup_requested_arr_time\": \"07:00\", \"deliver_address\": \"134 CANTERBURY DR\", \"roundtrip_actual_longitude\": null, \"add_charge_occur8\": null, \"delivery_airport_code\": null, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"add_charge_code10\": null, \"customer_name\": \"MXD/RYDER\", \"rate_buck_amt1\": 80.00, \"delivery_address_point_number_text\": \"MICHAEL BROWN\", \"delivery_address_point_number\": 25113, \"cod_amount\": null, \"roundtrip_actual_depart_time\": null, \"pickup_dispatch_zone\": null, \"service_level_text\": \"White Glove Delivery\", \"service_level\": 58, \"deliver_actual_longitude\": null, \"pickup_actual_arr_time\": \"08:00\", \"add_charge_amt6\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"pickup_longitude\": -77.33035820, \"driver2\": null, \"distribution_branch_id\": null, \"add_charge_amt9\": null, \"add_charge_code8\": null, \"blocks\": null, \"hazmat\": false, \"add_charge_code7\": null, \"deliver_requested_dep_time\": \"17:00\", \"signature\": \"SOF\", \"master_airway_bill_number\": null, \"cod_accept_company_check\": false, \"delivery_point_customer\": 31025, \"add_charge_occur2\": null, \"quote_amount\": null, \"add_charge_code2\": null, \"deliver_requested_date\": \"2021-05-10\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"record_type\": 0, \"deliver_special_instr_long\": null, \"push_partner_order_id\": null, \"pickup_address\": \"540 EASTPARK CT\", \"add_charge_occur9\": null, \"distribution_shift_id\": null, \"settlements\": [{\"voucher_amount\": null, \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"settlement_bucket6_pct\": null, \"fuel_price_zone\": null, \"settlement_period_end_date\": null, \"driver_number_text\": \"LAVERT KENDALL MORRIS\", \"driver_number\": 3001, \"fuel_price_source\": null, \"settlement_bucket3_pct\": null, \"agent_etrac_transaction_number\": null, \"voucher_date\": null, \"charge6\": null, \"settlement_bucket2_pct\": null, \"charge4\": null, \"pay_chart_used\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"adjustment_type\": null, \"voucher_number\": null, \"charge5\": null, \"charge2\": null, \"time_last_updated\": \"11:52\", \"settlement_bucket1_pct\": null, \"settlement_bucket5_pct\": null, \"settlement_bucket4_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"settlement_pct\": 100.00, \"pre_book_percentage\": true, \"date_last_updated\": \"2021-07-05\", \"vendor_invoice_number\": null, \"control_number\": 1871698, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"fuel_plan\": null, \"charge3\": null, \"vendor_employee_numer\": null, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"charge1\": null, \"order_date\": \"2021-05-10\", \"id\": \"002018716980D1\", \"agents_etrac_partner_id\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"record_type\": 0}], \"hours\": \"15\", \"pu_actual_location_accuracy\": null, \"fuel_price_zone\": null, \"rt_actual_location_accuracy\": null, \"deliver_phone_ext\": null, \"vehicle_type\": null, \"del_actual_location_accuracy\": null, \"ordered_by_phone_number\": null, \"deliver_country\": null, \"az_equip3\": null, \"add_charge_amt7\": null, \"add_charge_amt10\": null, \"notes\": [{\"user_id\": \"DX*\", \"note_line\": \"** Driver #1: 0 -> 3001\", \"control_number\": 1871698, \"note_code\": \"11\", \"id\": \"00201871698020210705125237DX*\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"entry_date\": \"2021-07-05\", \"print_on_ticket\": false, \"show_to_cust\": false, \"entry_time\": \"12:52:37\"}, {\"user_id\": \"DX*\", \"note_line\": \"** Driver #1 setl%: .00% -> 100.00%\", \"control_number\": 1871698, \"note_code\": \" 6\", \"id\": \"00201871698020210705125238DX*\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"entry_date\": \"2021-07-05\", \"print_on_ticket\": false, \"show_to_cust\": false, \"entry_time\": \"12:52:38\"}], \"pickup_eta_time\": null, \"deliver_state\": \"VA\", \"add_charge_code5\": null, \"deliver_wait_time\": null, \"pickup_omw_latitude\": null, \"fuel_miles\": null, \"add_charge_occur12\": null, \"deliver_dispatch_zone\": null, \"rate_buck_amt10\": 2.16, \"order_automatically_quoted\": false, \"deliver_actual_arr_time\": \"08:00\", \"deliver_special_instructions4\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"az_equip1\": null, \"time_order_entered\": \"12:27\", \"rate_buck_amt7\": null, \"roundtrip_actual_latitude\": null, \"add_charge_amt2\": null, \"add_charge_occur7\": null, \"pickup_email_notification_sent\": false, \"dispatch_time\": null, \"pickup_actual_longitude\": null, \"add_charge_amt11\": null, \"pickup_name\": \"HUMAN TOUCH\", \"dispatch_id\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"send_new_order_alert\": false, \"id\": \"002018716980\", \"deliver_omw_longitude\": null, \"pickup_state\": \"VA\", \"deliver_route_code\": null, \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"pickup_phone\": null, \"number_of_pieces\": 1}}";
                                            //  objresponse.ResponseVal = true;
                                            if (objresponse.ResponseVal)
                                            {
                                                strExecutionLogMessage = "OrderPut API Success " + System.Environment.NewLine;
                                                strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                                // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                DataSet dsOrderPutResponse = objCommon.jsonToDataSet(objresponse.Reason, "OrderPost");
                                                try
                                                {
                                                    if (dsOrderPutResponse.Tables.Contains("id"))
                                                    {
                                                        List<Id> idList = new List<Id>();
                                                        for (int i = 0; i < dsOrderPutResponse.Tables["id"].Rows.Count; i++)
                                                        {
                                                            DataTable dt = dsOrderPutResponse.Tables["id"];
                                                            Id objIds = new Id();
                                                            //objIds.verified_weight = dt.Rows[i]["verified_weight"];
                                                            //objIds.roundtrip_actual_latitude = dt.Rows[i]["roundtrip_actual_latitude"];
                                                            //objIds.pickup_special_instructions4 = dt.Rows[i]["pickup_special_instructions4"];
                                                            //objIds.fuel_miles = dt.Rows[i]["fuel_miles"];
                                                            //objIds.pickup_dispatch_zone = dt.Rows[i]["pickup_dispatch_zone"];
                                                            if (dt.Columns.Contains("pickup_zip"))
                                                            {
                                                                objIds.pickup_zip = dt.Rows[i]["pickup_zip"];
                                                            }
                                                            if (dt.Columns.Contains("pickup_actual_arr_time"))
                                                            {
                                                                objIds.pickup_actual_arr_time = dt.Rows[i]["pickup_actual_arr_time"];
                                                            }
                                                            //objIds.cod_accept_company_check = dt.Rows[i]["cod_accept_company_check"];
                                                            //objIds.add_charge_occur9 = dt.Rows[i]["add_charge_occur9"];
                                                            //objIds.pickup_omw_latitude = dt.Rows[i]["pickup_omw_latitude"];
                                                            //objIds.service_level_text = dt.Rows[i]["service_level_text"];
                                                            if (dt.Columns.Contains("service_level"))
                                                            {
                                                                objIds.service_level = dt.Rows[i]["service_level"];
                                                            }
                                                            //objIds.exception_sign_required = dt.Rows[i]["exception_sign_required"];
                                                            //objIds.pickup_phone_ext = dt.Rows[i]["pickup_phone_ext"];
                                                            //objIds.roundtrip_actual_pieces = dt.Rows[i]["roundtrip_actual_pieces"];
                                                            //objIds.bringg_send_sms = dt.Rows[i]["bringg_send_sms"];
                                                            //objIds.az_equip2 = dt.Rows[i]["az_equip2"];

                                                            //objIds.hist_inv_date = dt.Rows[i]["hist_inv_date"];
                                                            if (dt.Columns.Contains("date_order_entered"))
                                                            {
                                                                objIds.date_order_entered = dt.Rows[i]["date_order_entered"];
                                                            }
                                                            //objIds.powerpage_status_text = dt.Rows[i]["powerpage_status_text"];
                                                            //objIds.powerpage_status = dt.Rows[i]["powerpage_status"];
                                                            if (dt.Columns.Contains("pickup_city"))
                                                            {
                                                                objIds.pickup_city = dt.Rows[i]["pickup_city"];
                                                            }
                                                            //objIds.pickup_phone = dt.Rows[i]["pickup_phone"];
                                                            //objIds.pickup_sign_req = dt.Rows[i]["pickup_sign_req"];

                                                            //objIds.deliver_phone = dt.Rows[i]["deliver_phone"];
                                                            //objIds.deliver_omw_longitude = dt.Rows[i]["deliver_omw_longitude"];
                                                            //objIds.roundtrip_actual_longitude = dt.Rows[i]["roundtrip_actual_longitude"];
                                                            //objIds.page_number = dt.Rows[i]["page_number"];
                                                            //objIds.order_type_text = dt.Rows[i]["order_type_text"];
                                                            //objIds.order_type = dt.Rows[i]["order_type"];
                                                            //objIds.add_charge_code9 = dt.Rows[i]["add_charge_code9"];
                                                            //objIds.pickup_eta_time = dt.Rows[i]["pickup_eta_time"];

                                                            //objIds.record_type = dt.Rows[i]["record_type"];
                                                            //objIds.add_charge_occur11 = dt.Rows[i]["add_charge_occur11"];
                                                            //objIds.push_partner_order_id = dt.Rows[i]["push_partner_order_id"];
                                                            //objIds.deliver_country = dt.Rows[i]["deliver_country"];
                                                            //objIds.customer_name = dt.Rows[i]["customer_name"];
                                                            if (dt.Columns.Contains("bol_number"))
                                                            {
                                                                objIds.bol_number = dt.Rows[i]["bol_number"];
                                                            }
                                                            //objIds.pickup_latitude = dt.Rows[i]["pickup_latitude"];
                                                            //objIds.add_charge_code4 = dt.Rows[i]["add_charge_code4"];

                                                            //objIds.exception_order_action_text = dt.Rows[i]["exception_order_action_text"];
                                                            //objIds.exception_order_action = dt.Rows[i]["exception_order_action"];
                                                            //objIds.pu_arrive_notification_sent = dt.Rows[i]["pu_arrive_notification_sent"];
                                                            //objIds.distribution_shift_id = dt.Rows[i]["distribution_shift_id"];
                                                            //objIds.pickup_special_instr_long = dt.Rows[i]["pickup_special_instr_long"];
                                                            if (dt.Columns.Contains("id"))
                                                            {
                                                                objIds.id = dt.Rows[i]["id"];
                                                            }
                                                            //objIds.callback_to = dt.Rows[i]["callback_to"];
                                                            //objIds.customer_number_text = dt.Rows[i]["customer_number_text"];

                                                            if (dt.Columns.Contains("customer_number"))
                                                            {
                                                                objIds.customer_number = dt.Rows[i]["customer_number"];
                                                            }
                                                            //objIds.ordered_by = dt.Rows[i]["ordered_by"];
                                                            //objIds.add_charge_code12 = dt.Rows[i]["add_charge_code12"];
                                                            //objIds.pickup_route_seq = dt.Rows[i]["pickup_route_seq"];
                                                            if (dt.Columns.Contains("deliver_city"))
                                                            {
                                                                objIds.deliver_city = dt.Rows[i]["deliver_city"];
                                                            }

                                                            //objIds.add_charge_occur5 = dt.Rows[i]["add_charge_occur5"];
                                                            //objIds.edi_acknowledgement_required = dt.Rows[i]["edi_acknowledgement_required"];
                                                            //objIds.rescheduled_ctrl_number = dt.Rows[i]["rescheduled_ctrl_number"];
                                                            //objIds.driver2 = dt.Rows[i]["driver2"];
                                                            //objIds.deliver_room = dt.Rows[i]["deliver_room"];

                                                            if (dt.Columns.Contains("deliver_actual_arr_time"))
                                                            {
                                                                objIds.deliver_actual_arr_time = dt.Rows[i]["deliver_actual_arr_time"];
                                                            }
                                                            //objIds.fuel_price_zone = dt.Rows[i]["fuel_price_zone"];
                                                            //objIds.add_charge_amt9 = dt.Rows[i]["add_charge_amt9"];
                                                            //objIds.add_charge_amt4 = dt.Rows[i]["add_charge_amt4"];
                                                            //objIds.delivery_address_point_number_text = dt.Rows[i]["delivery_address_point_number_text"];
                                                            //objIds.delivery_address_point_number = dt.Rows[i]["delivery_address_point_number"];

                                                            //objIds.deliver_actual_longitude = dt.Rows[i]["deliver_actual_longitude"];
                                                            //objIds.add_charge_amt2 = dt.Rows[i]["add_charge_amt2"];
                                                            //objIds.additional_drivers = dt.Rows[i]["additional_drivers"];
                                                            //objIds.pickup_pricing_zone = dt.Rows[i]["pickup_pricing_zone"];
                                                            //objIds.hazmat = dt.Rows[i]["hazmat"];
                                                            if (dt.Columns.Contains("pickup_address"))
                                                            {
                                                                objIds.pickup_address = dt.Rows[i]["pickup_address"];
                                                            }
                                                            //objIds.pickup_route_code = dt.Rows[i]["pickup_route_code"];
                                                            //objIds.callback_userid = dt.Rows[i]["callback_userid"];
                                                            //objIds.pickup_point_customer = dt.Rows[i]["pickup_point_customer"];

                                                            //objIds.rate_buck_amt1 = dt.Rows[i]["rate_buck_amt1"];
                                                            //objIds.add_charge_amt8 = dt.Rows[i]["add_charge_amt8"];
                                                            //objIds.callback_time = dt.Rows[i]["callback_time"];
                                                            //objIds.csr = dt.Rows[i]["csr"];
                                                            //objIds.roundtrip_actual_depart_time = dt.Rows[i]["roundtrip_actual_depart_time"];
                                                            //objIds.customers_etrac_partner_id = dt.Rows[i]["customers_etrac_partner_id"];
                                                            //objIds.manual_notepad = dt.Rows[i]["manual_notepad"];
                                                            //objIds.add_charge_code8 = dt.Rows[i]["add_charge_code8"];
                                                            //objIds.bringg_order_id = dt.Rows[i]["bringg_order_id"];
                                                            //objIds.deliver_omw_latitude = dt.Rows[i]["deliver_omw_latitude"];
                                                            //objIds.pickup_longitude = dt.Rows[i]["pickup_longitude"];
                                                            //objIds.etrac_number = dt.Rows[i]["etrac_number"];

                                                            //objIds.distribution_unique_id = dt.Rows[i]["distribution_unique_id"];
                                                            //objIds.vehicle_type = dt.Rows[i]["vehicle_type"];
                                                            //objIds.roundtrip_actual_arrival_time = dt.Rows[i]["roundtrip_actual_arrival_time"];
                                                            //objIds.delivery_longitude = dt.Rows[i]["delivery_longitude"];
                                                            //objIds.pu_actual_location_accuracy = dt.Rows[i]["pu_actual_location_accuracy"];
                                                            if (dt.Columns.Contains("deliver_actual_date"))
                                                            {
                                                                objIds.deliver_actual_date = dt.Rows[i]["deliver_actual_date"];
                                                            }
                                                            //objIds.exception_timestamp = dt.Rows[i]["exception_timestamp"];
                                                            if (dt.Columns.Contains("deliver_zip"))
                                                            {
                                                                objIds.deliver_zip = dt.Rows[i]["deliver_zip"];
                                                            }
                                                            //objIds.roundtrip_wait_time = dt.Rows[i]["roundtrip_wait_time"];
                                                            //objIds.add_charge_occur8 = dt.Rows[i]["add_charge_occur8"];
                                                            //objIds.dl_arrive_notification_sent = dt.Rows[i]["dl_arrive_notification_sent"];
                                                            //objIds.pickup_special_instructions1 = dt.Rows[i]["pickup_special_instructions1"];
                                                            //objIds.ordered_by_phone_number = dt.Rows[i]["ordered_by_phone_number"];
                                                            if (dt.Columns.Contains("deliver_requested_arr_time"))
                                                            {
                                                                objIds.deliver_requested_arr_time = dt.Rows[i]["deliver_requested_arr_time"];
                                                            }
                                                            //objIds.rate_miles = dt.Rows[i]["rate_miles"];
                                                            //objIds.holiday_groups = dt.Rows[i]["holiday_groups"];
                                                            //objIds.pickup_email_notification_sent = dt.Rows[i]["pickup_email_notification_sent"];
                                                            //objIds.add_charge_code3 = dt.Rows[i]["add_charge_code3"];
                                                            //objIds.dispatch_id = dt.Rows[i]["dispatch_id"];
                                                            //objIds.add_charge_occur10 = dt.Rows[i]["add_charge_occur10"];
                                                            //objIds.dispatch_time = dt.Rows[i]["dispatch_time"];
                                                            //objIds.deliver_wait_time = dt.Rows[i]["deliver_wait_time"];
                                                            //objIds.invoice_period_end_date = dt.Rows[i]["invoice_period_end_date"];
                                                            //objIds.add_charge_occur12 = dt.Rows[i]["add_charge_occur12"];

                                                            //objIds.fuel_plan = dt.Rows[i]["fuel_plan"];
                                                            //objIds.return_svc_level = dt.Rows[i]["return_svc_level"];
                                                            if (dt.Columns.Contains("pickup_actual_date"))
                                                            {
                                                                objIds.pickup_actual_date = dt.Rows[i]["pickup_actual_date"];
                                                            }
                                                            //objIds.send_new_order_alert = dt.Rows[i]["send_new_order_alert"];
                                                            //objIds.pickup_room = dt.Rows[i]["pickup_room"];
                                                            //objIds.rate_buck_amt8 = dt.Rows[i]["rate_buck_amt8"];
                                                            //objIds.add_charge_amt10 = dt.Rows[i]["add_charge_amt10"];
                                                            //objIds.insurance_amount = dt.Rows[i]["insurance_amount"];
                                                            //objIds.add_charge_amt3 = dt.Rows[i]["add_charge_amt3"];
                                                            //objIds.add_charge_amt6 = dt.Rows[i]["add_charge_amt6"];
                                                            //objIds.pickup_special_instructions3 = dt.Rows[i]["pickup_special_instructions3"];
                                                            if (dt.Columns.Contains("pickup_requested_date"))
                                                            {
                                                                objIds.pickup_requested_date = dt.Rows[i]["pickup_requested_date"];
                                                            }
                                                            //objIds.roundtrip_sign_req = dt.Rows[i]["roundtrip_sign_req"];
                                                            //objIds.actual_miles = dt.Rows[i]["actual_miles"];
                                                            //objIds.pickup_address_point_number_text = dt.Rows[i]["pickup_address_point_number_text"];
                                                            //if (dt.Columns.Contains("pickup_address_point_number_text"))
                                                            //{
                                                            //    objIds.pickup_address_point_number_text = dt.Rows[i]["pickup_address_point_number_text"];
                                                            //}
                                                            //objIds.pickup_address_point_number = dt.Rows[i]["pickup_address_point_number"];
                                                            //objIds.deliver_actual_latitude = dt.Rows[i]["deliver_actual_latitude"];
                                                            //objIds.deliver_phone_ext = dt.Rows[i]["deliver_phone_ext"];
                                                            //objIds.deliver_route_code = dt.Rows[i]["deliver_route_code"];
                                                            //objIds.add_charge_code10 = dt.Rows[i]["add_charge_code10"];
                                                            //objIds.delivery_airport_code = dt.Rows[i]["delivery_airport_code"];

                                                            if (dt.Columns.Contains("reference_text"))
                                                            {
                                                                objIds.reference_text = dt.Rows[i]["reference_text"];
                                                            }
                                                            if (dt.Columns.Contains("reference"))
                                                            {
                                                                objIds.reference = dt.Rows[i]["reference"];
                                                            }
                                                            //objIds.photos_exist = dt.Rows[i]["photos_exist"];
                                                            //objIds.master_airway_bill_number = dt.Rows[i]["master_airway_bill_number"];
                                                            if (dt.Columns.Contains("control_number"))
                                                            {
                                                                objIds.control_number = dt.Rows[i]["control_number"];
                                                            }
                                                            //objIds.cod_text = dt.Rows[i]["cod_text"];
                                                            //objIds.cod = dt.Rows[i]["cod"];
                                                            //objIds.rate_buck_amt11 = dt.Rows[i]["rate_buck_amt11"];
                                                            //objIds.pickup_omw_timestamp = dt.Rows[i]["pickup_omw_timestamp"];
                                                            //objIds.deliver_special_instructions1 = dt.Rows[i]["deliver_special_instructions1"];
                                                            //objIds.quote_amount = dt.Rows[i]["quote_amount"];
                                                            //objIds.total_pages = dt.Rows[i]["total_pages"];
                                                            //objIds.rate_buck_amt4 = dt.Rows[i]["rate_buck_amt4"];
                                                            //objIds.delivery_latitude = dt.Rows[i]["delivery_latitude"];
                                                            //objIds.add_charge_code1 = dt.Rows[i]["add_charge_code1"];


                                                            //objIds.order_timeliness_text = dt.Rows[i]["order_timeliness_text"];
                                                            //objIds.order_timeliness = dt.Rows[i]["order_timeliness"];
                                                            //objIds.deliver_special_instr_long = dt.Rows[i]["deliver_special_instr_long"];
                                                            if (dt.Columns.Contains("deliver_address"))
                                                            {
                                                                objIds.deliver_address = dt.Rows[i]["deliver_address"];
                                                            }
                                                            //objIds.add_charge_occur4 = dt.Rows[i]["add_charge_occur4"];
                                                            //objIds.deliver_eta_date = dt.Rows[i]["deliver_eta_date"];
                                                            if (dt.Columns.Contains("pickup_actual_dep_time"))
                                                            {
                                                                objIds.pickup_actual_dep_time = dt.Rows[i]["pickup_actual_dep_time"];
                                                            }
                                                            if (dt.Columns.Contains("deliver_requested_dep_time"))
                                                            {
                                                                objIds.deliver_requested_dep_time = dt.Rows[i]["deliver_requested_dep_time"];
                                                            }
                                                            if (dt.Columns.Contains("deliver_actual_dep_time"))
                                                            {
                                                                objIds.deliver_actual_dep_time = dt.Rows[i]["deliver_actual_dep_time"];
                                                            }
                                                            //objIds.bringg_last_loc_sent = dt.Rows[i]["bringg_last_loc_sent"];
                                                            //objIds.az_equip3 = dt.Rows[i]["az_equip3"];
                                                            ////  objIds.driver1_text = dt.Rows[i]["driver1_text"];
                                                            //if (dt.Columns.Contains("driver1_text"))
                                                            //{
                                                            //    objIds.driver1_text = dt.Rows[i]["driver1_text"];
                                                            //}
                                                            if (dt.Columns.Contains("driver1"))
                                                            {
                                                                objIds.driver1 = dt.Rows[i]["driver1"];
                                                            }
                                                            //objIds.pickup_actual_latitude = dt.Rows[i]["pickup_actual_latitude"];
                                                            //objIds.add_charge_occur2 = dt.Rows[i]["add_charge_occur2"];
                                                            //objIds.order_automatically_quoted = dt.Rows[i]["order_automatically_quoted"];
                                                            //objIds.callback_required = dt.Rows[i]["callback_required_text"];
                                                            //objIds.frequent_caller_id = dt.Rows[i]["frequent_caller_id"];
                                                            //objIds.rate_buck_amt6 = dt.Rows[i]["rate_buck_amt6"];
                                                            //objIds.rate_chart_used = dt.Rows[i]["rate_chart_used"];
                                                            if (dt.Columns.Contains("deliver_actual_pieces"))
                                                            {
                                                                objIds.deliver_actual_pieces = dt.Rows[i]["deliver_actual_pieces"];
                                                            }
                                                            //objIds.add_charge_code5 = dt.Rows[i]["add_charge_code5"];
                                                            //objIds.pickup_omw_longitude = dt.Rows[i]["pickup_omw_longitude"];
                                                            //objIds.delivery_point_customer = dt.Rows[i]["delivery_point_customer"];
                                                            //objIds.add_charge_occur7 = dt.Rows[i]["add_charge_occur7"];
                                                            //objIds.rate_buck_amt5 = dt.Rows[i]["rate_buck_amt5"];
                                                            //objIds.fuel_update_freq_text = dt.Rows[i]["fuel_update_freq_text"];
                                                            //objIds.fuel_update_freq = dt.Rows[i]["fuel_update_freq"];
                                                            //objIds.add_charge_code11 = dt.Rows[i]["add_charge_code11"];
                                                            if (dt.Columns.Contains("pickup_name"))
                                                            {
                                                                objIds.pickup_name = dt.Rows[i]["pickup_name"];
                                                            }
                                                            //objIds.callback_date = dt.Rows[i]["callback_date"];
                                                            //objIds.add_charge_code2 = dt.Rows[i]["add_charge_code2"];
                                                            //objIds.house_airway_bill_number = dt.Rows[i]["house_airway_bill_number"];
                                                            if (dt.Columns.Contains("deliver_name"))
                                                            {
                                                                objIds.deliver_name = dt.Rows[i]["deliver_name"];
                                                            }
                                                            if (dt.Columns.Contains("number_of_pieces"))
                                                            {
                                                                objIds.number_of_pieces = dt.Rows[i]["number_of_pieces"];
                                                            }
                                                            //objIds.deliver_eta_time = dt.Rows[i]["deliver_eta_time"];
                                                            //objIds.origin_code_text = dt.Rows[i]["origin_code_text"];
                                                            //objIds.origin_code = dt.Rows[i]["origin_code"];
                                                            //objIds.rate_special_instructions = dt.Rows[i]["rate_special_instructions"];
                                                            //objIds.add_charge_occur3 = dt.Rows[i]["add_charge_occur3"];
                                                            //objIds.pickup_eta_date = dt.Rows[i]["pickup_eta_date"];
                                                            //objIds.deliver_special_instructions4 = dt.Rows[i]["deliver_special_instructions4"];
                                                            //objIds.custom_special_instr_long = dt.Rows[i]["custom_special_instr_long"];
                                                            //objIds.deliver_special_instructions2 = dt.Rows[i]["deliver_special_instructions2"];
                                                            if (dt.Columns.Contains("pickup_signature"))
                                                            {
                                                                objIds.pickup_signature = dt.Rows[i]["pickup_signature"];
                                                            }
                                                            //objIds.az_equip1 = dt.Rows[i]["az_equip1"];
                                                            //objIds.add_charge_amt12 = dt.Rows[i]["add_charge_amt12"];
                                                            //objIds.calc_add_on_chgs = dt.Rows[i]["calc_add_on_chgs"];
                                                            //objIds.original_schedule_number = dt.Rows[i]["original_schedule_number"];
                                                            //objIds.blocks = dt.Rows[i]["blocks"];
                                                            //objIds.del_actual_location_accuracy = dt.Rows[i]["del_actual_location_accuracy"];
                                                            //objIds.zone_set_used = dt.Rows[i]["zone_set_used"];

                                                            //objIds.pickup_country = dt.Rows[i]["pickup_country"];
                                                            if (dt.Columns.Contains("pickup_state"))
                                                            {
                                                                objIds.pickup_state = dt.Rows[i]["pickup_state"];
                                                            }
                                                            //objIds.add_charge_amt7 = dt.Rows[i]["add_charge_amt7"];
                                                            //objIds.email_addresses = dt.Rows[i]["email_addresses"];
                                                            //objIds.add_charge_occur1 = dt.Rows[i]["add_charge_occur1"];
                                                            //objIds.pickup_wait_time = dt.Rows[i]["pickup_wait_time"];
                                                            //objIds.company_number_text = dt.Rows[i]["company_number_text"];
                                                            if (dt.Columns.Contains("company_number"))
                                                            {
                                                                objIds.company_number = dt.Rows[i]["company_number"];
                                                            }
                                                            //objIds.distribution_branch_id = dt.Rows[i]["distribution_branch_id"];
                                                            //objIds.rate_buck_amt9 = dt.Rows[i]["rate_buck_amt9"];
                                                            //objIds.add_charge_amt1 = dt.Rows[i]["add_charge_amt1"];
                                                            if (dt.Columns.Contains("pickup_requested_dep_time"))
                                                            {
                                                                objIds.pickup_requested_dep_time = dt.Rows[i]["pickup_requested_dep_time"];
                                                            }
                                                            //    objIds.customer_type_text = dt.Rows[i]["customer_type_text"];
                                                            //if (dt.Columns.Contains("customer_type_text"))
                                                            //{
                                                            //    objIds.customer_type_text = dt.Rows[i]["customer_type_text"];
                                                            //}
                                                            //objIds.customer_type = dt.Rows[i]["customer_type"];
                                                            if (dt.Columns.Contains("deliver_state"))
                                                            {
                                                                objIds.deliver_state = dt.Rows[i]["deliver_state"];
                                                            }
                                                            //objIds.deliver_dispatch_zone = dt.Rows[i]["deliver_dispatch_zone"];
                                                            //objIds.image_sign_req = dt.Rows[i]["image_sign_req"];
                                                            //objIds.add_charge_code6 = dt.Rows[i]["add_charge_code6"];
                                                            if (dt.Columns.Contains("deliver_requested_date"))
                                                            {
                                                                objIds.deliver_requested_date = dt.Rows[i]["deliver_requested_date"];
                                                            }
                                                            //objIds.add_charge_amt5 = dt.Rows[i]["add_charge_amt5"];
                                                            if (dt.Columns.Contains("time_order_entered"))
                                                            {
                                                                objIds.time_order_entered = dt.Rows[i]["time_order_entered"];
                                                            }
                                                            //objIds.pick_del_trans_flag_text = dt.Rows[i]["pick_del_trans_flag_text"];
                                                            //objIds.pick_del_trans_flag = dt.Rows[i]["pick_del_trans_flag"];
                                                            //objIds.pickup_attention = dt.Rows[i]["pickup_attention"];
                                                            //objIds.rate_buck_amt7 = dt.Rows[i]["rate_buck_amt7"];
                                                            //objIds.add_charge_occur6 = dt.Rows[i]["add_charge_occur6"];
                                                            //objIds.fuel_price_source = dt.Rows[i]["fuel_price_source"];
                                                            //objIds.pickup_airport_code = dt.Rows[i]["pickup_airport_code"];
                                                            //objIds.rate_buck_amt2 = dt.Rows[i]["rate_buck_amt2"];
                                                            //objIds.rate_buck_amt3 = dt.Rows[i]["rate_buck_amt3"];
                                                            //objIds.deliver_omw_timestamp = dt.Rows[i]["deliver_omw_timestamp"];
                                                            //objIds.exception_code = dt.Rows[i]["exception_code"];
                                                            //objIds.status_code_text = dt.Rows[i]["status_code_text"];
                                                            //objIds.status_code = dt.Rows[i]["status_code"];
                                                            //objIds.weight = dt.Rows[i]["weight"];
                                                            //objIds.signature_required = dt.Rows[i]["signature_required"];
                                                            //objIds.rate_buck_amt10 = dt.Rows[i]["rate_buck_amt10"];
                                                            //objIds.hist_inv_number = dt.Rows[i]["hist_inv_number"];
                                                            //objIds.deliver_pricing_zone = dt.Rows[i]["deliver_pricing_zone"];
                                                            //objIds.pickup_actual_longitude = dt.Rows[i]["pickup_actual_longitude"];
                                                            //objIds.push_services = dt.Rows[i]["push_services"];
                                                            //objIds.add_charge_amt11 = dt.Rows[i]["add_charge_amt11"];
                                                            //objIds.rt_actual_location_accuracy = dt.Rows[i]["rt_actual_location_accuracy"];
                                                            //objIds.roundtrip_actual_date = dt.Rows[i]["roundtrip_actual_date"];
                                                            //objIds.pickup_requested_arr_time = dt.Rows[i]["pickup_requested_arr_time"];
                                                            //objIds.deliver_attention = dt.Rows[i]["deliver_attention"];
                                                            //objIds.deliver_special_instructions3 = dt.Rows[i]["deliver_special_instructions3"];
                                                            //objIds.pickup_actual_pieces = dt.Rows[i]["pickup_actual_pieces"];
                                                            //objIds.edi_order_accepted_or_rejected_text = dt.Rows[i]["edi_order_accepted_or_rejected_text"];
                                                            //objIds.edi_order_accepted_or_rejected = dt.Rows[i]["edi_order_accepted_or_rejected"];
                                                            //objIds.roundtrip_signature = dt.Rows[i]["roundtrip_signature"];
                                                            //objIds.po_number = dt.Rows[i]["po_number"];
                                                            if (dt.Columns.Contains("signature"))
                                                            {
                                                                objIds.signature = dt.Rows[i]["signature"];
                                                            }
                                                            //objIds.pickup_special_instructions2 = dt.Rows[i]["pickup_special_instructions2"];
                                                            //objIds.original_ctrl_number = dt.Rows[i]["original_ctrl_number"];
                                                            //objIds.previous_ctrl_number = dt.Rows[i]["previous_ctrl_number"];
                                                            //if (dt.Columns.Contains("Id"))
                                                            //{

                                                            //    objIds.id = dt.Rows[i]["Id"];
                                                            //}
                                                            idList.Add(objIds);

                                                        }

                                                        //objCommon.SaveOutputDataToCsvFile(idList, "OrderPut-Create",
                                                        //   strInputFilePath, UniqueId, strFileName, strDatetime);

                                                        objCommon.SaveOutputDataToCsvFileParallely(idList, "OrderPut-Create",
                                                          processingFileName, strDatetime);

                                                    }
                                                    if (dsOrderPutResponse.Tables.Contains("settlements"))
                                                    {
                                                        List<Settlement> settelmentList = new List<Settlement>();
                                                        for (int i = 0; i < dsOrderPutResponse.Tables["settlements"].Rows.Count; i++)
                                                        {
                                                            DataTable dt = dsOrderPutResponse.Tables["settlements"];
                                                            Settlement objsettlements = new Settlement();
                                                            //objsettlements.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                            if (dt.Columns.Contains("company_number"))
                                                            {
                                                                objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                            }
                                                            //objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                            //objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                            //objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                            //objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                            //objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                            //objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                            if (dt.Columns.Contains("order_date"))
                                                            {
                                                                objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                            }
                                                            //objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                            //objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                            //objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                            //objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                            //objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                            //objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                            //objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                            //objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                            //objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                            //objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                            //objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                            //objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                            //objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                            if (dt.Columns.Contains("driver_company_number"))
                                                            {
                                                                objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                            }
                                                            //objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                            //objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                            //objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                            //objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                            //objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                            //objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                            if (dt.Columns.Contains("id"))
                                                            {
                                                                objsettlements.id = (dt.Rows[i]["id"]);
                                                            }
                                                            if (dt.Columns.Contains("date_last_updated"))
                                                            {
                                                                objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                            }
                                                            //objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                            //objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                            //objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                            //objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                            if (dt.Columns.Contains("driver_number"))
                                                            {
                                                                objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                            }
                                                            //objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                            //objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                            //objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                            //objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                            //objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                            //objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                            //objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                            //objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                            //objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                            //objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                            if (dt.Columns.Contains("control_number"))
                                                            {
                                                                objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                            }
                                                            //objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                            //objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                            //objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                            //objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);
                                                            settelmentList.Add(objsettlements);
                                                        }

                                                        //objCommon.SaveOutputDataToCsvFile(settelmentList, "OrderPut-Settlements",
                                                        //   strInputFilePath, UniqueId, strFileName, strDatetime);
                                                        objCommon.SaveOutputDataToCsvFileParallely(settelmentList, "OrderPut-Settlements",
                                                         processingFileName, strDatetime);
                                                    }
                                                    if (dsOrderPutResponse.Tables.Contains("progress"))
                                                    {

                                                        List<Progress> progressList = new List<Progress>();
                                                        for (int i = 0; i < dsOrderPutResponse.Tables["progress"].Rows.Count; i++)
                                                        {
                                                            Progress progress = new Progress();
                                                            DataTable dt = dsOrderPutResponse.Tables["progress"];
                                                            if (dt.Columns.Contains("status_date"))
                                                            {
                                                                progress.status_date = (dt.Rows[i]["status_date"]);
                                                            }
                                                            if (dt.Columns.Contains("status_text"))
                                                            {
                                                                progress.status_text = (dt.Rows[i]["status_text"]);
                                                            }
                                                            if (dt.Columns.Contains("status_time"))
                                                            {
                                                                progress.status_time = (dt.Rows[i]["status_time"]);
                                                            }
                                                            if (dt.Columns.Contains("id"))
                                                            {
                                                                progress.id = (dt.Rows[i]["id"]);
                                                            }
                                                            progressList.Add(progress);
                                                        }

                                                        //objCommon.SaveOutputDataToCsvFile(progressList, "OrderPut-Progress",
                                                        //   strInputFilePath, UniqueId, strFileName, strDatetime);
                                                        objCommon.SaveOutputDataToCsvFileParallely(progressList, "OrderPut-Progress",
                                                        processingFileName, strDatetime);

                                                    }
                                                    if (dsOrderPutResponse.Tables.Contains("notes"))
                                                    {

                                                        List<Note> noteList = new List<Note>();
                                                        for (int i = 0; i < dsOrderPutResponse.Tables["notes"].Rows.Count; i++)
                                                        {
                                                            Note note = new Note();
                                                            DataTable dt = dsOrderPutResponse.Tables["notes"];
                                                            if (dt.Columns.Contains("note_line"))
                                                            {
                                                                note.user_id = (dt.Rows[i]["note_line"]);
                                                            }
                                                            if (dt.Columns.Contains("note_line"))
                                                            {
                                                                note.note_line = (dt.Rows[i]["note_line"]);
                                                            }
                                                            if (dt.Columns.Contains("control_number"))
                                                            {
                                                                note.control_number = (dt.Rows[i]["control_number"]);
                                                            }
                                                            if (dt.Columns.Contains("note_code"))
                                                            {
                                                                note.note_code = (dt.Rows[i]["note_code"]);
                                                            }
                                                            if (dt.Columns.Contains("company_number_text"))
                                                            {
                                                                note.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                            }
                                                            if (dt.Columns.Contains("company_number"))
                                                            {
                                                                note.company_number = (dt.Rows[i]["company_number"]);
                                                            }
                                                            if (dt.Columns.Contains("entry_date"))
                                                            {
                                                                note.entry_date = (dt.Rows[i]["entry_date"]);
                                                            }
                                                            if (dt.Columns.Contains("print_on_ticket"))
                                                            {
                                                                note.print_on_ticket = (dt.Rows[i]["print_on_ticket"]);
                                                            }
                                                            if (dt.Columns.Contains("entry_time"))
                                                            {
                                                                note.show_to_cust = (dt.Rows[i]["entry_time"]);
                                                            }
                                                            if (dt.Columns.Contains("entry_time"))
                                                            {
                                                                note.entry_time = (dt.Rows[i]["entry_time"]);
                                                            }
                                                            if (dt.Columns.Contains("id"))
                                                            {
                                                                note.id = (dt.Rows[i]["id"]);
                                                            }
                                                            noteList.Add(note);
                                                        }

                                                        //objCommon.SaveOutputDataToCsvFile(noteList, "OrderPut-Note",
                                                        //   strInputFilePath, UniqueId, strFileName, strDatetime);
                                                        objCommon.SaveOutputDataToCsvFileParallely(noteList, "OrderPut-Note",
                                                       processingFileName, strDatetime);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    strExecutionLogMessage = "ProcessUpdateOrderFiles Exception -" + ex.Message + System.Environment.NewLine;
                                                    strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                                    strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                                    strExecutionLogMessage += "For UniqueId-" + UniqueId + System.Environment.NewLine;
                                                    strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                                    //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                    //objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                                    objCommon.WriteErrorLogParallelly(ex, fileName, strExecutionLogMessage);

                                                    ErrorResponse objErrorResponse = new ErrorResponse();
                                                    objErrorResponse.error = ex.Message;
                                                    objErrorResponse.status = "Error";
                                                    objErrorResponse.code = "Exception while writing Order-Put response into csv";
                                                    objErrorResponse.reference = UniqueId;
                                                    string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                    DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                    dsFailureResponse.Tables[0].TableName = "OrderPutFailure";
                                                    objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                strInputFilePath, processingFileName, strDatetime);

                                                }
                                                if (driver1 != null)
                                                {
                                                    if (dsOrderPutResponse.Tables.Contains("settlements"))
                                                    {
                                                        UniqueId = Convert.ToString(dsOrderPutResponse.Tables["settlements"].Rows[0]["id"]);

                                                        string ordersettlementputrequest = null;

                                                        int company_number = Convert.ToInt32(dsOrderPutResponse.Tables["settlements"].Rows[0]["company_number"]);
                                                        int control_number = Convert.ToInt32(dsOrderPutResponse.Tables["settlements"].Rows[0]["control_number"]);

                                                        int record_type = Convert.ToInt32(objCommon.GetConfigValue("OrderSettlement_record_type"));
                                                        string transaction_type = objCommon.GetConfigValue("OrderSettlement_transaction_type"); // 
                                                        string driver_sequence = objCommon.GetConfigValue("OrderSettlement_driver_sequence");

                                                        ordersettlementputrequest = @"'company_number': " + company_number + ",";
                                                        ordersettlementputrequest = ordersettlementputrequest + @"'control_number': " + control_number + ",";

                                                        if (record_type != null)
                                                        {
                                                            ordersettlementputrequest = ordersettlementputrequest + @"'record_type': " + record_type + ",";
                                                        }

                                                        if (dr.Table.Columns.Contains("Transaction Type"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Transaction Type"])))
                                                            {
                                                                transaction_type = Convert.ToString(dr["Transaction Type"]);
                                                            }
                                                        }

                                                        ordersettlementputrequest = ordersettlementputrequest + @"'transaction_type': '" + transaction_type + "',";

                                                        if (driver_sequence != null)
                                                        {
                                                            ordersettlementputrequest = ordersettlementputrequest + @"'driver_sequence': '" + driver_sequence + "',";
                                                        }
                                                        else
                                                        {
                                                            driver_sequence = "0";
                                                            ordersettlementputrequest = ordersettlementputrequest + @"'driver_sequence': '" + driver_sequence + "',";

                                                        }

                                                        if (dr.Table.Columns.Contains("Carrier Base Pay"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Carrier Base Pay"])))
                                                            {
                                                                ordersettlementputrequest = ordersettlementputrequest + @"'charge1': " + dr["Carrier Base Pay"] + ",";
                                                            }
                                                            else
                                                            {
                                                                strExecutionLogMessage = "OrderPut-OrderSettlementPut Error " + System.Environment.NewLine;
                                                                strExecutionLogMessage += "Carrier Base Pay Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                                strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                                                strExecutionLogMessage += "And For Unique Id -" + UniqueId + System.Environment.NewLine;
                                                                //  objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                                                ErrorResponse objErrorResponse = new ErrorResponse();
                                                                objErrorResponse.error = "Carrier Base Pay value not found for this record";
                                                                objErrorResponse.status = "Error";
                                                                objErrorResponse.code = "Carrier Base Pay values Missing";
                                                                objErrorResponse.reference = UniqueId;
                                                                string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                                dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                                objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                            strInputFilePath, processingFileName, strDatetime);
                                                                rowindex++;
                                                                continue;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            strExecutionLogMessage = "OrderPut-OrderSettlementPut Error " + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Carrier Base Pay Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                            strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                                            strExecutionLogMessage += "And For Unique Id -" + UniqueId + System.Environment.NewLine;
                                                            //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                                            objErrorResponse.error = "Carrier Base Pay column not found for this record";
                                                            objErrorResponse.status = "Error";
                                                            objErrorResponse.code = "Carrier Base Pay column Missing";
                                                            objErrorResponse.reference = UniqueId;
                                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                            dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                            objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                        strInputFilePath, processingFileName, strDatetime);
                                                            rowindex++;
                                                            continue;
                                                        }
                                                        if (dr.Table.Columns.Contains("Carrier ACC"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Carrier ACC"])))
                                                            {
                                                                ordersettlementputrequest = ordersettlementputrequest + @"'charge5': " + dr["Carrier ACC"] + ",";
                                                            }
                                                            else
                                                            {
                                                                strExecutionLogMessage = "OrderPut-OrderSettlementPut Error " + System.Environment.NewLine;
                                                                strExecutionLogMessage += "Carrier ACC Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                                strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                                                strExecutionLogMessage += "And For Unique Id -" + UniqueId + System.Environment.NewLine;
                                                                // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                                ErrorResponse objErrorResponse = new ErrorResponse();
                                                                objErrorResponse.error = "Carrier ACC value not found for this record";
                                                                objErrorResponse.status = "Error";
                                                                objErrorResponse.code = "Carrier ACC column Missing";
                                                                objErrorResponse.reference = UniqueId;
                                                                string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                                dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                                objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                            strInputFilePath, processingFileName, strDatetime);
                                                                rowindex++;
                                                                continue;

                                                            }
                                                        }
                                                        else
                                                        {
                                                            strExecutionLogMessage = "OrderPut-OrderSettlementPut Error " + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Carrier ACC Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                            strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                                            strExecutionLogMessage += "And For Unique Id -" + UniqueId + System.Environment.NewLine;
                                                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                                            objErrorResponse.error = "Carrier ACC value not found for this record";
                                                            objErrorResponse.status = "Error";
                                                            objErrorResponse.code = "Carrier ACC Value  Missing";
                                                            objErrorResponse.reference = UniqueId;
                                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                            dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                            objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                        strInputFilePath, processingFileName, strDatetime);
                                                            rowindex++;
                                                            continue;
                                                        }

                                                        if (dr.Table.Columns.Contains("Carrier FSC"))
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Carrier FSC"])))
                                                            {
                                                                ordersettlementputrequest = ordersettlementputrequest + @"'charge6': " + Convert.ToDouble(dr["Carrier FSC"]) + ",";
                                                            }
                                                        }

                                                        ordersettlementputrequest = @"{" + ordersettlementputrequest + "}";
                                                        string order_settlementObject = @"{'order_settlement': " + ordersettlementputrequest + "}";
                                                        jsonobj = JObject.Parse(order_settlementObject);
                                                        request = jsonobj.ToString();

                                                        clsCommon.ReturnResponse objresponseOrdersettlement = new clsCommon.ReturnResponse();
                                                        objresponseOrdersettlement = objclsDatatrac.CallDataTracOrderSettlementPutAPI(UniqueId, order_settlementObject);
                                                        //objresponseOrdersettlement.Reason = "{\"002018724450D1\": {\"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"time_last_updated\": \"05:10\", \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"order_date\": \"2021-06-28\", \"agent_etrac_transaction_number\": null, \"settlement_bucket1_pct\": null, \"settlement_bucket2_pct\": null, \"vendor_employee_numer\": null, \"fuel_price_zone\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"charge1\": 35.91, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"adjustment_type\": null, \"agents_etrac_partner_id\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"vendor_invoice_number\": null, \"charge6\": null, \"settlement_bucket4_pct\": null, \"fuel_plan\": null, \"record_type\": 0, \"voucher_amount\": null, \"id\": \"002018724450D1\", \"date_last_updated\": \"2021-07-08\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"voucher_number\": null, \"driver_number_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver_number\": 3208, \"settlement_bucket6_pct\": null, \"settlement_pct\": 100.00, \"charge3\": null, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"voucher_date\": null, \"fuel_price_source\": null, \"pay_chart_used\": null, \"settlement_bucket5_pct\": null, \"charge2\": null, \"control_number\": 1872445, \"settlement_bucket3_pct\": null, \"charge5\": null, \"pre_book_percentage\": true, \"settlement_period_end_date\": null}}";
                                                        //objresponseOrdersettlement.ResponseVal = true;
                                                        if (objresponseOrdersettlement.ResponseVal)
                                                        {
                                                            // request = JsonConvert.SerializeObject(objresponseOrdersettlement);
                                                            strExecutionLogMessage = "OrderPut-OrderSettlementPutAPI Success " + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                                            //   objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                            DataSet dsOrderSettlementResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                                                            dsOrderSettlementResponse.Tables[0].TableName = "OrderSettlement";

                                                            try
                                                            {
                                                                List<ResponseOrderSettlements> orderSettlementstList = new List<ResponseOrderSettlements>();
                                                                for (int i = 0; i < dsOrderSettlementResponse.Tables["OrderSettlement"].Rows.Count; i++)
                                                                {
                                                                    DataTable dt = dsOrderSettlementResponse.Tables["OrderSettlement"];
                                                                    ResponseOrderSettlements objsettlements = new ResponseOrderSettlements();
                                                                    //objsettlements.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                                    if (dt.Columns.Contains("company_number"))
                                                                    {
                                                                        objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                                    }
                                                                    //objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                                    //objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                                    //objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                                    //objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                                    //objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                                    //objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                                    if (dt.Columns.Contains("order_date"))
                                                                    {
                                                                        objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                                    }
                                                                    //objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                                    //objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                                    //objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                                    //objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                                    //objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                                    //objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                                    //objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                                    //objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                                    //objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                                    //objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                                    //objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                                    //objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                                    //objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                                    if (dt.Columns.Contains("driver_company_number"))
                                                                    {
                                                                        objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                                    }
                                                                    //objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                                    //objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                                    //objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                                    //objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                                    //objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                                    //objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                                    if (dt.Columns.Contains("id"))
                                                                    {
                                                                        objsettlements.id = (dt.Rows[i]["id"]);
                                                                    }
                                                                    if (dt.Columns.Contains("date_last_updated"))
                                                                    {
                                                                        objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                                    }
                                                                    //objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                                    //objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                                    //objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                                    //objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                                    if (dt.Columns.Contains("driver_number"))
                                                                    {
                                                                        objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                                    }
                                                                    //objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                                    //objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                                    //objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                                    //objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                                    //objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                                    //objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                                    //objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                                    //objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                                    //objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                                    //objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                                    if (dt.Columns.Contains("control_number"))
                                                                    {
                                                                        objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                                    }
                                                                    //objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                                    //objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                                    //objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                                    //objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);
                                                                    orderSettlementstList.Add(objsettlements);
                                                                }

                                                                //objCommon.SaveOutputDataToCsvFile(orderSettlementstList, "OrderSettlement",
                                                                //   strInputFilePath, UniqueId, strFileName, strDatetime);

                                                                objCommon.SaveOutputDataToCsvFileParallely(orderSettlementstList, "OrderSettlement",
                                                             processingFileName, strDatetime);
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                ErrorResponse objErrorResponse = new ErrorResponse();
                                                                objErrorResponse.error = ex.Message;
                                                                objErrorResponse.status = "Error";
                                                                objErrorResponse.code = "Exception while writing the response into csv";
                                                                objErrorResponse.reference = UniqueId;
                                                                string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                                dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                                objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                            strInputFilePath, processingFileName, strDatetime);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            strExecutionLogMessage = "OrderPut-OrderSettlementPutAPI Failed " + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                            DataSet dsOrderPutFailureResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                                                            dsOrderPutFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                            dsOrderPutFailureResponse.Tables[0].Columns.Add("UniqueId", typeof(System.String));
                                                            foreach (DataRow row in dsOrderPutFailureResponse.Tables[0].Rows)
                                                            {
                                                                row["UniqueId"] = UniqueId;
                                                            }
                                                            //    objCommon.WriteDataToCsvFile(dsOrderPutFailureResponse.Tables[0],
                                                            //strInputFilePath, UniqueId, strFileName, strDatetime);

                                                            objCommon.WriteDataToCsvFileParallely(dsOrderPutFailureResponse.Tables[0],
                                                            strInputFilePath, processingFileName, strDatetime);

                                                        }
                                                    }
                                                }

                                            }
                                            else
                                            {
                                                //request = JsonConvert.SerializeObject(objorderdetails);
                                                strExecutionLogMessage = "OrderPutAPI Failed " + System.Environment.NewLine;
                                                strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(objresponse.Reason);
                                                dsFailureResponse.Tables[0].TableName = "OrderPutFailure";
                                                dsFailureResponse.Tables[0].Columns.Add("UniqueId", typeof(System.String));
                                                foreach (DataRow row in dsFailureResponse.Tables[0].Rows)
                                                {
                                                    row["UniqueId"] = UniqueId;
                                                }
                                                objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                            strInputFilePath, processingFileName, strDatetime);
                                            }
                                        }
                                        rowindex++;
                                    }
                                    catch (Exception ex)
                                    {
                                        strExecutionLogMessage = "ProcessUpdateOrderFiles Exception -" + ex.Message + System.Environment.NewLine;
                                        strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                        strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                        strExecutionLogMessage += "For UniqueId-" + UniqueId + System.Environment.NewLine;
                                        strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                        // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                        // objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                        objCommon.WriteErrorLogParallelly(ex, fileName, strExecutionLogMessage);

                                        ErrorResponse objErrorResponse = new ErrorResponse();
                                        objErrorResponse.error = ex.Message;
                                        objErrorResponse.status = "Error";
                                        objErrorResponse.code = "Exception while generating the request";
                                        objErrorResponse.reference = UniqueId;
                                        string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                        DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                        dsFailureResponse.Tables[0].TableName = "OrderPutFailure";
                                        objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                    strInputFilePath, processingFileName, strDatetime);
                                    }
                                }
                            });
                            // objCommon.MoveOutputFilesToOutputLocation(strInputFilePath);

                            objCommon.MergeSplittedOutputFiles(strFileName, "OrderPut-Create", strDatetime);
                            objCommon.MergeSplittedOutputFiles(strFileName, "OrderPut-Settlements", strDatetime);
                            objCommon.MergeSplittedOutputFiles(strFileName, "OrderPut-Progress", strDatetime);
                            objCommon.MergeSplittedOutputFiles(strFileName, "OrderPut-Note", strDatetime);
                            objCommon.MergeSplittedOutputFiles(strFileName, "OrderSettlement", strDatetime);
                            objCommon.MergeSplittedOutputFiles(strFileName, "OrderPutFailure", strDatetime);
                            objCommon.MergeSplittedOutputFiles(strFileName, "OrderSettlementFailure", strDatetime);
                            objCommon.MergeSplittedOutputFiles(strFileName, "OrderCancelSuccess", strDatetime);
                            objCommon.MergeSplittedOutputFiles(strFileName, "OrderCancelFailure", strDatetime);
                            objCommon.MoveMergedOutputFilesToOutputLocation(strInputFilePath);
                            objCommon.CleanSplittedOutputFilesWorkingFolder();

                            strExecutionLogMessage = "Parallelly Processing  finished for the  file : " + strFileName + "." + System.Environment.NewLine;
                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                        }
                        else
                        {
                            strExecutionLogMessage = "Template sheet data not found for the file " + strInputFilePath + @"\" + strFileName;
                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        strExecutionLogMessage = "ProcessUpdateOrderFiles Exception -" + ex.Message + System.Environment.NewLine;
                        strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                        strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                        strExecutionLogMessage += "For UniqueId-" + UniqueId + System.Environment.NewLine;
                        // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                        objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                    }
                }

                strExecutionLogMessage = "Finished processing all the files for the location " + strLocationFolder;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
            }
            catch (Exception ex)
            {
                // MsgBox(ex.Message).ToString()
                objCommon.WriteErrorLog(ex);
                throw new Exception("Error in ProcessUpdateOrderFiles -->" + ex.Message + ex.StackTrace);
            }
        }

        private static void ProcessUpdateOrderSettlementFiles(string strInputFilePath, string strLocationFolder)
        {
            clsCommon objCommon = new clsCommon();
            try
            {
                System.Configuration.AppSettingsReader reader = new System.Configuration.AppSettingsReader();
                DirectoryInfo dir;
                FileInfo[] XLSfiles;
                string strFileName;
                // string strInputFilePath;
                string strBillingHistoryFileLocation;
                string strExecutionLogMessage;
                string strExecutionLogFileLocation;
                DataTable dtDataTable;
                //  string strSheetName;
                string strDatetime;
                string UniqueId = null;
                // string ReferenceId = null;
                strExecutionLogFileLocation = objCommon.GetConfigValue("ExecutionLogFileLocation");

                strExecutionLogMessage = "Processing the Order Settlement data for " + strInputFilePath;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                // strInputFilePath = objCommon.GetConfigValue("AutomationFileLocation") + @"\OrderSettlement";
                // strInputFilePath = strInputFilePath + @"\" + strLocationFolder;
                strBillingHistoryFileLocation = strInputFilePath + @"\HistoricalFiles";

                strExecutionLogMessage = "The input file Path is: " + strInputFilePath + "." + System.Environment.NewLine + "The Historical File Path is:" + strBillingHistoryFileLocation;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                dir = new DirectoryInfo(strInputFilePath);
                XLSfiles = dir.GetFiles("*.xlsx");

                strExecutionLogMessage = "Found # of Excel Files: " + XLSfiles.Count();
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                foreach (var file in XLSfiles)
                {
                    strFileName = file.ToString();
                    dtDataTable = new System.Data.DataTable();

                    try
                    {
                        // strExecutionLogMessage = "Getting ready to insert the Scanning Data into the WrkBillingLocation table ";
                        //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                        DataSet dsExcel = new DataSet();
                        dsExcel = clsExcelHelper.ImportExcelXLSX(strInputFilePath + @"\" + strFileName, false);
                        if (dsExcel.Tables.Count > 0)
                        {

                            objCommon.MoveTheFileToHistoryFolder(strBillingHistoryFileLocation, file);
                            strDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");

                            for (int i = dsExcel.Tables[0].Rows.Count - 1; i >= 0; i--)
                            {
                                DataRow dr = dsExcel.Tables[0].Rows[i];
                                if (dr["Company"] == DBNull.Value && dr["Control Number"] == DBNull.Value)
                                    dr.Delete();
                            }
                            dsExcel.Tables[0].AcceptChanges();

                            int noofrowspertable = Convert.ToInt16(objCommon.GetConfigValue("DevideToProcessParallelly"));
                            List<DataTable> splitdt = clsCommon.SplitTable(dsExcel.Tables[0], noofrowspertable, strFileName, strDatetime);

                            strExecutionLogMessage = "Parallelly Processing Statred for the  file : " + strFileName + "." + System.Environment.NewLine + "Number of processess are going to exicute is :" + noofrowspertable;
                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                            objCommon.CleanSplittedOutputFilesWorkingFolder();

                            Parallel.ForEach(splitdt, currentDatatable =>
                            {
                                var fileName = currentDatatable.TableName;
                                var processingFileName = currentDatatable.TableName;
                                strExecutionLogMessage = "Current Processing File is  : " + fileName + "." + System.Environment.NewLine;
                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                var datatable = currentDatatable;
                                int rowindex = 1;
                                foreach (DataRow dr in datatable.Rows)
                                {

                                    try
                                    {
                                        clsDatatrac objclsDatatrac = new clsDatatrac();

                                        string ordersettlementputrequest = null;

                                        if (dr.Table.Columns.Contains("Company"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Company"])))
                                            {
                                                ordersettlementputrequest = @"'company_number': " + dr["Company"] + ",";
                                            }
                                            else
                                            {
                                                strExecutionLogMessage = "OrderSettlementPutAPI Error " + System.Environment.NewLine;
                                                strExecutionLogMessage += "Company Number Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                                // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                                ErrorResponse objErrorResponse = new ErrorResponse();
                                                objErrorResponse.error = "Company value not found for this record ";
                                                objErrorResponse.status = "Error";
                                                objErrorResponse.code = "Company Value  Missing";
                                                objErrorResponse.reference = "For row number -" + rowindex;
                                                string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                            strInputFilePath, processingFileName, strDatetime);
                                                rowindex++;
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "OrderSettlementPutAPI Error " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Company Number Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                            //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                            objErrorResponse.error = "Company column not found for this record";
                                            objErrorResponse.status = "Error";
                                            objErrorResponse.code = "Company column  Missing";
                                            objErrorResponse.reference = "For row number -" + rowindex;
                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                            dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                            objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                        strInputFilePath, processingFileName, strDatetime);
                                            rowindex++;
                                            continue;
                                        }
                                        if (dr.Table.Columns.Contains("Control Number"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Control Number"])))
                                            {
                                                ordersettlementputrequest = ordersettlementputrequest + @"'control_number': " + Convert.ToInt32(dr["Control Number"]) + ",";
                                            }
                                            else
                                            {
                                                strExecutionLogMessage = "OrderSettlementPutAPI Error " + System.Environment.NewLine;
                                                strExecutionLogMessage += "Control Number Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                                // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                                ErrorResponse objErrorResponse = new ErrorResponse();
                                                objErrorResponse.error = "Control Number column not found for this record";
                                                objErrorResponse.status = "Error";
                                                objErrorResponse.code = "Control Number column Missing";
                                                objErrorResponse.reference = "For row number -" + rowindex;
                                                string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                            strInputFilePath, processingFileName, strDatetime);
                                                rowindex++;
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "OrderSettlementPutAPI Error " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Control Number Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                            //  objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                            objErrorResponse.error = "Control Number value not found for this record";
                                            objErrorResponse.status = "Error";
                                            objErrorResponse.code = "Control Number  Value  Missing";
                                            objErrorResponse.reference = "For row number -" + rowindex;
                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                            dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                            objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                        strInputFilePath, processingFileName, strDatetime);
                                            rowindex++;
                                            continue;
                                        }

                                        string OrderSettlement_UniqueIdSuffix = Convert.ToString(dr["OrderSettlement_UniqueIdSuffix"]);
                                        UniqueId = objclsDatatrac.GenerateUniqueNumber(Convert.ToInt32(dr["Company"]), Convert.ToInt32(dr["Control Number"]), OrderSettlement_UniqueIdSuffix);

                                        int record_type = Convert.ToInt32(objCommon.GetConfigValue("OrderSettlement_record_type"));
                                        string transaction_type = objCommon.GetConfigValue("OrderSettlement_transaction_type"); // 
                                        string driver_sequence = objCommon.GetConfigValue("OrderSettlement_driver_sequence");

                                        if (record_type != null)
                                        {
                                            ordersettlementputrequest = ordersettlementputrequest + @"'record_type': " + record_type + ",";
                                        }

                                        if (dr.Table.Columns.Contains("Transaction Type"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Transaction Type"])))
                                            {
                                                transaction_type = Convert.ToString(dr["Transaction Type"]);
                                            }
                                        }

                                        ordersettlementputrequest = ordersettlementputrequest + @"'transaction_type': '" + transaction_type + "',";

                                        if (driver_sequence != null)
                                        {
                                            ordersettlementputrequest = ordersettlementputrequest + @"'driver_sequence': '" + driver_sequence + "',";
                                        }
                                        else
                                        {
                                            driver_sequence = "0";
                                            ordersettlementputrequest = ordersettlementputrequest + @"'driver_sequence': '" + driver_sequence + "',";
                                        }
                                        if (dr.Table.Columns.Contains("Carrier Base Pay"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Carrier Base Pay"])))
                                            {
                                                ordersettlementputrequest = ordersettlementputrequest + @"'charge1': " + dr["Carrier Base Pay"] + ",";
                                            }
                                            else
                                            {
                                                strExecutionLogMessage = "OrderSettlementPut Error " + System.Environment.NewLine;
                                                strExecutionLogMessage += "Carrier Base Pay Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                                strExecutionLogMessage += "For Unique  Reference -" + UniqueId + System.Environment.NewLine;
                                                // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                                ErrorResponse objErrorResponse = new ErrorResponse();
                                                objErrorResponse.error = "Carrier Base Pay value not found for this record";
                                                objErrorResponse.status = "Error";
                                                objErrorResponse.code = "Carrier Base Pay Value  Missing";
                                                objErrorResponse.reference = UniqueId;
                                                string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                            strInputFilePath, processingFileName, strDatetime);
                                                rowindex++;
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "OrderSettlementPut Error " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Carrier Base Pay Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                            strExecutionLogMessage += "For Unique Reference -" + UniqueId + System.Environment.NewLine;
                                            //  objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                            objErrorResponse.error = "Carrier Base Pay column not found for this record";
                                            objErrorResponse.status = "Error";
                                            objErrorResponse.code = "Carrier Base Pay column  Missing";
                                            objErrorResponse.reference = UniqueId;
                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                            dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                            objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                        strInputFilePath, processingFileName, strDatetime);
                                            rowindex++;
                                            continue;
                                        }
                                        if (dr.Table.Columns.Contains("Carrier ACC"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Carrier ACC"])))
                                            {
                                                ordersettlementputrequest = ordersettlementputrequest + @"'charge5': " + dr["Carrier ACC"] + ",";
                                            }
                                            else
                                            {
                                                strExecutionLogMessage = "OrderSettlementPut Error " + System.Environment.NewLine;
                                                strExecutionLogMessage += "Carrier ACC Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                                strExecutionLogMessage += "For Unique Reference -" + UniqueId + System.Environment.NewLine;
                                                //  objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                                ErrorResponse objErrorResponse = new ErrorResponse();
                                                objErrorResponse.error = "Carrier ACC value not found for this  record";
                                                objErrorResponse.status = "Error";
                                                objErrorResponse.code = "Carrier ACC Value Missing";
                                                objErrorResponse.reference = UniqueId;
                                                string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                            strInputFilePath, processingFileName, strDatetime);
                                                rowindex++;
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "OrderSettlementPut Error " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Carrier ACC Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                            strExecutionLogMessage += "For Unique Reference -" + UniqueId + System.Environment.NewLine;
                                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                            objErrorResponse.error = "Carrier ACC column not found for this file record";
                                            objErrorResponse.status = "Error";
                                            objErrorResponse.code = "Carrier ACC column Missing";
                                            objErrorResponse.reference = UniqueId;
                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                            dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                            objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                        strInputFilePath, processingFileName, strDatetime);
                                            rowindex++;
                                            continue;
                                        }

                                        if (dr.Table.Columns.Contains("Carrier FSC"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Carrier FSC"])))
                                            {
                                                ordersettlementputrequest = ordersettlementputrequest + @"'charge6': " + Convert.ToDouble(dr["Carrier FSC"]) + ",";
                                            }
                                        }

                                        ordersettlementputrequest = @"{" + ordersettlementputrequest + "}";
                                        string order_settlementObject = @"{'order_settlement': " + ordersettlementputrequest + "}";
                                        JObject jsonobj = JObject.Parse(order_settlementObject);
                                        string request = jsonobj.ToString();

                                        clsCommon.ReturnResponse objresponseOrdersettlement = new clsCommon.ReturnResponse();
                                        objresponseOrdersettlement = objclsDatatrac.CallDataTracOrderSettlementPutAPI(UniqueId, order_settlementObject);

                                        //objresponseOrdersettlement.Reason = "{\"002018724450D1\": {\"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"time_last_updated\": \"05:10\", \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"order_date\": \"2021-06-28\", \"agent_etrac_transaction_number\": null, \"settlement_bucket1_pct\": null, \"settlement_bucket2_pct\": null, \"vendor_employee_numer\": null, \"fuel_price_zone\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"charge1\": 35.91, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"adjustment_type\": null, \"agents_etrac_partner_id\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"vendor_invoice_number\": null, \"charge6\": null, \"settlement_bucket4_pct\": null, \"fuel_plan\": null, \"record_type\": 0, \"voucher_amount\": null, \"id\": \"002018724450D1\", \"date_last_updated\": \"2021-07-08\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"voucher_number\": null, \"driver_number_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver_number\": 3208, \"settlement_bucket6_pct\": null, \"settlement_pct\": 100.00, \"charge3\": null, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"voucher_date\": null, \"fuel_price_source\": null, \"pay_chart_used\": null, \"settlement_bucket5_pct\": null, \"charge2\": null, \"control_number\": 1872445, \"settlement_bucket3_pct\": null, \"charge5\": null, \"pre_book_percentage\": true, \"settlement_period_end_date\": null}}";
                                        //objresponseOrdersettlement.ResponseVal = true;

                                        //objresponseOrdersettlement.Reason = "{\"error\": \"Unable to perform API call object with RecordID:WS_OPORDR-9990111870 - The record may have been deleted.\", \"status\": \"error\", \"code\": \"U.RecordNotFound\"}";
                                        //objresponseOrdersettlement.ResponseVal = false;
                                        if (objresponseOrdersettlement.ResponseVal)
                                        {
                                            // request = JsonConvert.SerializeObject(objresponseOrdersettlement);
                                            strExecutionLogMessage = "OrderSettlementPutAPI Success " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                            strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                            DataSet dsOrderSettlementResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                                            dsOrderSettlementResponse.Tables[0].TableName = "OrderSettlement";

                                            List<ResponseOrderSettlements> orderSettlementstList = new List<ResponseOrderSettlements>();
                                            for (int i = 0; i < dsOrderSettlementResponse.Tables["OrderSettlement"].Rows.Count; i++)
                                            {
                                                DataTable dt = dsOrderSettlementResponse.Tables["OrderSettlement"];
                                                ResponseOrderSettlements objsettlements = new ResponseOrderSettlements();
                                                //objsettlements.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                if (dt.Columns.Contains("company_number"))
                                                {
                                                    objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                }
                                                //objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                //objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                //objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                //objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                //objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                //objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                if (dt.Columns.Contains("order_date"))
                                                {
                                                    objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                }
                                                //objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                //objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                //objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                //objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                //objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                //objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                //objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                //objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                //objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                //objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                //objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                //objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                //objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                if (dt.Columns.Contains("driver_company_number"))
                                                {
                                                    objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                }
                                                //objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                //objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                //objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                //objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                //objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                //objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                if (dt.Columns.Contains("id"))
                                                {
                                                    objsettlements.id = (dt.Rows[i]["id"]);
                                                }
                                                if (dt.Columns.Contains("date_last_updated"))
                                                {
                                                    objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                }
                                                //objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                //objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                //objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                //objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                if (dt.Columns.Contains("driver_number"))
                                                {
                                                    objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                }
                                                //objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                //objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                //objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                //objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                //objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                //objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                //objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                //objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                //objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                //objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                if (dt.Columns.Contains("control_number"))
                                                {
                                                    objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                }
                                                //objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                //objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                //objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                //objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);


                                                //objsettlements.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                if (dt.Columns.Contains("company_number"))
                                                {
                                                    objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                }
                                                //objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                //objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                //objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                //objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                //objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                //objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                if (dt.Columns.Contains("order_date"))
                                                {
                                                    objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                }
                                                //objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                //objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                //objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                //objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                //objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                //objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                //objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                //objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                //objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                //objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                //objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                //objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                //objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                if (dt.Columns.Contains("driver_company_number"))
                                                {
                                                    objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                }
                                                //objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                //objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                //objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                //objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                //objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                //objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                if (dt.Columns.Contains("id"))
                                                {
                                                    objsettlements.id = (dt.Rows[i]["id"]);
                                                }
                                                if (dt.Columns.Contains("date_last_updated"))
                                                {
                                                    objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                }
                                                //objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                //objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                //objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                //objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                if (dt.Columns.Contains("driver_number"))
                                                {
                                                    objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                }
                                                //objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                //objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                //objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                //objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                //objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                //objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                //objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                //objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                //objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                //objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                if (dt.Columns.Contains("control_number"))
                                                {
                                                    objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                }
                                                //objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                //objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                //objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                //objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);

                                                orderSettlementstList.Add(objsettlements);
                                            }

                                            //objCommon.SaveOutputDataToCsvFile(orderSettlementstList, "OrderSettlement",
                                            //   strInputFilePath, UniqueId, strFileName, strDatetime);

                                            objCommon.SaveOutputDataToCsvFileParallely(orderSettlementstList, "OrderSettlement",
                                                     processingFileName, strDatetime);
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "OrderSettlementPutAPI Failed " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                            strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                            DataSet dsOrderPutFailureResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                                            dsOrderPutFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                            dsOrderPutFailureResponse.Tables[0].Columns.Add("UniqueId", typeof(System.String));
                                            foreach (DataRow row in dsOrderPutFailureResponse.Tables[0].Rows)
                                            {
                                                row["UniqueId"] = UniqueId;
                                            }
                                            //    objCommon.WriteDataToCsvFile(dsOrderPutFailureResponse.Tables[0],
                                            //strInputFilePath, UniqueId, strFileName, strDatetime);

                                            objCommon.WriteDataToCsvFileParallely(dsOrderPutFailureResponse.Tables[0],
                                                        strInputFilePath, processingFileName, strDatetime);

                                        }
                                        rowindex++;
                                    }
                                    catch (Exception ex)
                                    {
                                        strExecutionLogMessage = "ProcessUpdateOrderSettlementFiles Exception - " + ex.Message + System.Environment.NewLine;
                                        strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                        strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                        strExecutionLogMessage += "For UniqueId -" + UniqueId + System.Environment.NewLine;
                                        strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                        //  objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                        //objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                        objCommon.WriteErrorLogParallelly(ex, fileName, strExecutionLogMessage);


                                        ErrorResponse objErrorResponse = new ErrorResponse();
                                        objErrorResponse.error = ex.Message;
                                        objErrorResponse.status = "Error";
                                        objErrorResponse.code = "Exception while generating the request";
                                        objErrorResponse.reference = UniqueId;
                                        string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                        DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                        dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                        objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                    strInputFilePath, processingFileName, strDatetime);
                                    }
                                }

                            });

                            //objCommon.MoveOutputFilesToOutputLocation(strInputFilePath);


                            objCommon.MergeSplittedOutputFiles(strFileName, "OrderSettlement", strDatetime);
                            objCommon.MergeSplittedOutputFiles(strFileName, "OrderSettlementFailure", strDatetime);
                            objCommon.MoveMergedOutputFilesToOutputLocation(strInputFilePath);
                            objCommon.CleanSplittedOutputFilesWorkingFolder();

                            strExecutionLogMessage = "Parallelly Processing  finished for the  file : " + strFileName + "." + System.Environment.NewLine;
                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                        }
                        else
                        {
                            strExecutionLogMessage = "Template sheet data not found for the file " + strInputFilePath + @"\" + strFileName;
                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        strExecutionLogMessage = "ProcessUpdateOrderSettlementFiles Exception - " + ex.Message + System.Environment.NewLine;
                        strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                        strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                        strExecutionLogMessage += "For UniqueId -" + UniqueId + System.Environment.NewLine;
                        //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                        objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                    }
                }

                strExecutionLogMessage = "Finished processing all the files for the file " + strInputFilePath + ",location " + strInputFilePath;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
            }
            catch (Exception ex)
            {
                objCommon.WriteErrorLog(ex);
                throw new Exception("Error in ProcessUpdateOrderSettlementFiles -->" + ex.Message + ex.StackTrace);
            }
        }

        private static void ProcessAddRouteHeaderFiles(string strInputFilePath, string strLocationFolder)
        {
            clsCommon objCommon = new clsCommon();

            try
            {
                System.Configuration.AppSettingsReader reader = new System.Configuration.AppSettingsReader();
                DirectoryInfo dir;
                FileInfo[] XLSfiles;
                string strFileName;
                // string strInputFilePath;
                string strBillingHistoryFileLocation;
                string strExecutionLogMessage;
                string strExecutionLogFileLocation;
                string strDatetime;
                DataTable dtDataTable;
                //  string strSheetName;
                string ReferenceId = null;
                strExecutionLogFileLocation = objCommon.GetConfigValue("ExecutionLogFileLocation");

                strExecutionLogMessage = "Processing the Add Route Header data for " + strInputFilePath;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                //strInputFilePath = objCommon.GetConfigValue("AutomationFileLocation") + @"\Order\Add";
                // strInputFilePath = strInputFilePath + @"\" + strLocationFolder;
                strBillingHistoryFileLocation = strInputFilePath + @"\HistoricalFiles";

                strExecutionLogMessage = "The input file Path is: " + strInputFilePath + "." + System.Environment.NewLine + "The Historical File Path is:" + strBillingHistoryFileLocation;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                dir = new DirectoryInfo(strInputFilePath);
                XLSfiles = dir.GetFiles("*.xlsx");

                strExecutionLogMessage = "Found # of Excel Files: " + XLSfiles.Count();
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                foreach (var file in XLSfiles)
                {
                    strFileName = file.ToString();
                    dtDataTable = new System.Data.DataTable();

                    try
                    {

                        DataSet dsExcel = new DataSet();
                        dsExcel = clsExcelHelper.ImportExcelXLSX(strInputFilePath + @"\" + strFileName, false);
                        if (dsExcel.Tables.Count > 0)
                        {
                            //objCommon.MoveTheFileToHistoryFolder(strBillingHistoryFileLocation, file);

                            strDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");
                            int j = 0;
                            foreach (DataTable table in dsExcel.Tables)
                            {
                                if (j == 1)
                                {
                                    break;
                                }
                                foreach (DataRow dr in table.Rows)
                                {

                                    object value = dr["Company"];
                                    if (value == DBNull.Value)
                                        break;

                                    // ReferenceId = Convert.ToString(dr["Customer Reference"]);

                                    try
                                    {
                                        route_headerdetails objroute_headerdetails = new route_headerdetails();
                                        route_header objheader = new route_header();

                                        objheader.company_number = Convert.ToString(dr["Company"]);

                                        DateTime dtValue = Convert.ToDateTime(dr["Route Date"]);
                                        objheader.route_date = dtValue.ToString("yyyy-MM-dd");
                                        objheader.route_code = Convert.ToString(dr["Route Code"]);
                                        objheader.billing_level = Convert.ToString(dr["Billing Level"]);
                                        objheader.billing_method = Convert.ToString(dr["Billing Method"]);

                                        objheader.labor_allocation_method = Convert.ToString(dr["Labor Allocation Method"]);
                                        objheader.overhead_allocation_method = Convert.ToString(dr["Overhead Allocation method"]);
                                        objheader.route_service_method = Convert.ToString(dr["Route Service Method"]);
                                        objheader.route_type = Convert.ToString(dr["Route Type"]);
                                        objheader.settlement_level = Convert.ToString(dr["Settlement Level"]);
                                        objheader.shipper_type = Convert.ToString(dr["Shipper Type"]);
                                        objheader.vehicle_allocation_method = Convert.ToString(dr["Vehicle Allocation Method"]);


                                        // objheader.route_stops = Convert.ToString(dr["route_stops"]);
                                        //objheader.actual_billing_amount = Convert.ToDouble(dr["actual_billing_amount"]);
                                        //objheader.actual_cost_allocation = Convert.ToDouble(dr["actual_cost_allocation"]);
                                        //objheader.actual_driver_agent = Convert.ToInt32(dr["actual_driver_agent"]);
                                        //objheader.actual_miles = Convert.ToInt32(dr["actual_miles"]);
                                        //objheader.actual_settlement_amount = Convert.ToDouble(dr["actual_settlement_amount"]);
                                        //objheader.actual_stops = Convert.ToInt32(dr["actual_stops"]);
                                        //objheader.actual_total_pieces = Convert.ToInt32(dr["actual_total_pieces"]);
                                        //objheader.actual_total_weight = Convert.ToInt32(dr["actual_total_weight"]);
                                        //objheader.actual_vehicle = Convert.ToString(dr["actual_vehicle"]);
                                        //objheader.amazon_order_number = Convert.ToInt32(dr["amazon_order_number"]);
                                        //objheader.assigned_driver_agent = Convert.ToInt32(dr["assigned_driver_agent"]);
                                        //objheader.assigned_vehicle = Convert.ToString(dr["assigned_vehicle"]);
                                        //objheader.az_equip1 = Convert.ToInt32(dr["az_equip1"]);
                                        //objheader.az_equip2 = Convert.ToInt32(dr["az_equip2"]);
                                        //objheader.az_equip3 = Convert.ToInt32(dr["az_equip3"]);

                                        //objheader.break_time = Convert.ToString(dr["break_time"]);
                                        //objheader.calc_eta = Convert.ToBoolean(dr["calc_eta"]);

                                        //objheader.close_time = Convert.ToString(dr["close_time"]);
                                        //objheader.created_by = Convert.ToString(dr["created_by"]);
                                        //objheader.created_date = Convert.ToString(dr["created_date"]);
                                        //objheader.created_time = Convert.ToString(dr["created_time"]);
                                        //objheader.dispatcher_id = Convert.ToString(dr["dispatcher_id"]);
                                        //objheader.end_date = Convert.ToString(dr["end_date"]);
                                        //objheader.end_location = Convert.ToString(dr["end_location"]);
                                        //objheader.end_time = Convert.ToString(dr["end_time"]);
                                        //objheader.ending_location = Convert.ToString(dr["ending_location"]);
                                        //objheader.expire_time = Convert.ToString(dr["expire_time"]);
                                        //objheader.hours = Convert.ToString(dr["hours"]);

                                        //objheader.labor_cost = Convert.ToDouble(dr["labor_cost"]);
                                        //objheader.last_updated_by = Convert.ToString(dr["last_updated_by"]);
                                        //objheader.last_updated_date = Convert.ToString(dr["last_updated_date"]);
                                        //objheader.last_updated_time = Convert.ToString(dr["last_updated_time"]);
                                        //objheader.miles = Convert.ToInt32(dr["miles"]);
                                        //  objheader.notes = Convert.ToString(dr["notes"]);
                                        //objheader.open_time = Convert.ToString(dr["open_time"]);

                                        // objheader.overhead_cost = Convert.ToDouble(dr["overhead_cost"]);
                                        //objheader.per_stop_billing_amount = Convert.ToDouble(dr["per_stop_billing_amount"]);
                                        // objheader.per_stop_settlement_amount = Convert.ToDouble(dr["per_stop_settlement_amount"]);
                                        // objheader.posted_by = Convert.ToString(dr["posted_by"]);

                                        // objheader.posted_date = Convert.ToString(dr["posted_date"]);

                                        //objheader.posted_status = Convert.ToInt32(dr["posted_status"]);
                                        //objheader.posted_time = Convert.ToString(dr["posted_time"]);
                                        //objheader.push_services = Convert.ToString(dr["push_services"]);
                                        //objheader.route_addon_amount = Convert.ToDouble(dr["route_addon_amount"]);
                                        //objheader.route_closed = Convert.ToInt32(dr["route_closed"]);
                                        //objheader.route_comments = Convert.ToString(dr["route_comments"]);
                                        //objheader.route_late_start_code = Convert.ToString(dr["route_late_start_code"]);

                                        //objheader.rtn_trans_route = Convert.ToString(dr["rtn_trans_route"]);
                                        //objheader.scan_expire_days = Convert.ToInt32(dr["scan_expire_days"]);
                                        //objheader.send_to_pt = Convert.ToInt32(dr["send_to_pt"]);
                                        //objheader.service_level = Convert.ToInt32(dr["service_level"]);

                                        //objheader.service_time = Convert.ToInt32(dr["service_time"]);



                                        //objheader.shipper_facility = Convert.ToString(dr["shipper_facility"]);
                                        //objheader.shipper_route = Convert.ToString(dr["shipper_route"]);

                                        //objheader.start_date = Convert.ToString(dr["start_date"]);
                                        //objheader.start_location = Convert.ToString(dr["start_location"]);
                                        //objheader.start_time = Convert.ToString(dr["start_time"]);
                                        //objheader.starting_location = Convert.ToString(dr["starting_location"]);
                                        //objheader.stops = Convert.ToInt32(dr["stops"]);
                                        //objheader.time_to_reseq = Convert.ToString(dr["time_to_reseq"]);
                                        //objheader.total_billing_amount = Convert.ToDouble(dr["total_billing_amount"]);

                                        //objheader.total_break_minutes = Convert.ToInt32(dr["total_break_minutes"]);
                                        //objheader.total_break_time = Convert.ToString(dr["total_break_time"]);
                                        //objheader.total_route_minutes = Convert.ToInt32(dr["total_route_minutes"]);
                                        //objheader.total_route_time = Convert.ToString(dr["total_route_time"]);
                                        //objheader.total_settlement_amount = Convert.ToDouble(dr["total_settlement_amount"]);
                                        //objheader.transfer_to_branch = Convert.ToString(dr["transfer_to_branch"]);
                                        //objheader.transfer_to_company = Convert.ToInt32(dr["transfer_to_company"]);
                                        //objheader.transfer_to_shift = Convert.ToString(dr["transfer_to_shift"]);
                                        //objheader.unique_control_id = Convert.ToInt32(dr["unique_control_id"]);
                                        //objheader.updated_by = Convert.ToString(dr["updated_by"]);
                                        //objheader.updated_date = Convert.ToString(dr["updated_date"]);
                                        //objheader.updated_time = Convert.ToString(dr["updated_time"]);

                                        // objheader.vehicle_cost = Convert.ToDouble(dr["vehicle_cost"]);

                                        objroute_headerdetails.route_header = objheader;
                                        clsRoute objclsRoute = new clsRoute();
                                        clsCommon.ReturnResponse objresponse = new clsCommon.ReturnResponse();
                                        string request = JsonConvert.SerializeObject(objroute_headerdetails);
                                        objresponse = objclsRoute.CallDataTracRouteHeaderPostAPI(request);
                                        // objresponse.ResponseVal = true;
                                        //objresponse.Reason = "{\"999      20200101UNROUT\": {\"end_date\": null, \"start_location\": null, \"scan_expire_days\": 0, \"shipper_facility\": null, \"route_late_start_code\": null, \"actual_settlement_amount\": 0, \"az_equip1\": null, \"posted_by\": null, \"per_stop_billing_amount\": 0, \"open_time\": null, \"assigned_vehicle\": null, \"actual_billing_amount\": 0, \"settlement_level_text\": \"None\", \"settlement_level\": \"0\", \"actual_cost_allocation\": 0, \"branch_id\": null, \"route_date\": \"2020-01-01\", \"total_route_minutes\": 0, \"total_break_time\": null, \"last_updated_by\": null, \"actual_miles\": 0, \"start_time\": null, \"actual_total_pieces\": 0, \"hours\": null, \"push_services\": null, \"end_location\": null, \"total_break_minutes\": 0, \"updated_date\": null, \"total_billing_amount\": 0, \"time_to_reseq\": null, \"route_closed\": 0, \"created_date\": \"2021-08-17\", \"last_updated_time\": null, \"overhead_allocation_method_text\": \"None\", \"overhead_allocation_method\": \"0\", \"vehicle_allocation_method_text\": \"None\", \"vehicle_allocation_method\": \"0\", \"expire_time\": null, \"shift_id\": null, \"rtn_trans_route\": null, \"id\": \"999      20200101UNROUT\", \"created_time\": \"09:56:28\", \"az_equip2\": null, \"actual_total_weight\": 0, \"route_code\": \"UNROUT\", \"actual_vehicle\": null, \"starting_location\": null, \"route_type_text\": \"Delivery\", \"route_type\": \"DEL\", \"overhead_cost\": 0, \"last_updated_date\": null, \"calc_eta\": false, \"labor_allocation_method_text\": \"None\", \"labor_allocation_method\": \"0\", \"route_addon_amount\": 0, \"posted_time\": null, \"company_number_text\": \"TEST COMPANY\", \"company_number\": 999, \"close_time\": null, \"billing_level_text\": \"None\", \"billing_level\": \"0\", \"notes\": [], \"service_level\": 0, \"service_time\": 0, \"route_service_method_text\": \"Default\", \"route_service_method\": \"0\", \"stops\": 0, \"route_stops\": [], \"created_by\": \"DX*\", \"assigned_driver_agent\": null, \"transfer_to_shift\": null, \"break_time\": null, \"total_settlement_amount\": 0, \"ending_location\": null, \"unique_control_id\": 1010056, \"shipper_type_text\": \"Default\", \"shipper_type\": \"0\", \"posted_date\": null, \"dispatcher_id\": null, \"shipper_route\": null, \"total_route_time\": null, \"send_to_pt\": false, \"updated_time\": null, \"posted_status\": 0, \"start_date\": null, \"amazon_order_number\": null, \"route_comments\": null, \"updated_by\": null, \"vehicle_cost\": 0, \"miles\": 0, \"billing_method_text\": \"None\", \"billing_method\": \"0\", \"actual_driver_agent\": null, \"labor_cost\": 0, \"az_equip3\": null, \"per_stop_settlement_amount\": 0, \"transfer_to_branch\": null, \"actual_stops\": 0, \"end_time\": null, \"transfer_to_company\": 0}}";
                                        if (objresponse.ResponseVal)
                                        {
                                            strExecutionLogMessage = "RouteHeaderPostAPI Success " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                            strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            DataSet dsResponse = objCommon.jsonToDataSet(objresponse.Reason, "RouteHeaderPost");
                                            var UniqueId = Convert.ToString(dsResponse.Tables[0].Rows[0]["id"]);
                                            try
                                            {
                                                if (dsResponse.Tables.Contains(UniqueId))
                                                {
                                                    List<ResponseRouteHeader> idList = new List<ResponseRouteHeader>();
                                                    for (int i = 0; i < dsResponse.Tables[0].Rows.Count; i++)
                                                    {
                                                        DataTable dt = dsResponse.Tables[0];
                                                        ResponseRouteHeader objIds = new ResponseRouteHeader();

                                                        objIds.settlement_level_text = dt.Rows[i]["settlement_level_text"];
                                                        objIds.settlement_level = dt.Rows[i]["settlement_level"];
                                                        objIds.route_type_text = dt.Rows[i]["route_type_text"];
                                                        objIds.route_type = dt.Rows[i]["route_type"];
                                                        objIds.posted_date = dt.Rows[i]["posted_date"];
                                                        objIds.end_time = dt.Rows[i]["end_time"];

                                                        objIds.scan_expire_days = dt.Rows[i]["scan_expire_days"];
                                                        objIds.actual_stops = dt.Rows[i]["actual_stops"];
                                                        objIds.service_level = dt.Rows[i]["service_level"];
                                                        objIds.actual_cost_allocation = dt.Rows[i]["actual_cost_allocation"];
                                                        objIds.actual_miles = dt.Rows[i]["actual_miles"];
                                                        objIds.posted_status = dt.Rows[i]["posted_status"];

                                                        objIds.rtn_trans_route = dt.Rows[i]["rtn_trans_route"];
                                                        objIds.shipper_route = dt.Rows[i]["shipper_route"];
                                                        objIds.actual_driver_agent = dt.Rows[i]["actual_driver_agent"];
                                                        objIds.route_addon_amount = dt.Rows[i]["route_addon_amount"];
                                                        objIds.open_time = dt.Rows[i]["open_time"];
                                                        objIds.created_by = dt.Rows[i]["created_by"];
                                                        objIds.last_updated_by = dt.Rows[i]["last_updated_by"];

                                                        objIds.az_equip2 = dt.Rows[i]["az_equip2"];
                                                        objIds.actual_total_pieces = dt.Rows[i]["actual_total_pieces"];
                                                        objIds.unique_control_id = dt.Rows[i]["unique_control_id"];
                                                        objIds.actual_vehicle = dt.Rows[i]["actual_vehicle"];
                                                        objIds.starting_location = dt.Rows[i]["starting_location"];
                                                        objIds.per_stop_settlement_amount = dt.Rows[i]["per_stop_settlement_amount"];
                                                        objIds.ending_location = dt.Rows[i]["ending_location"];

                                                        objIds.transfer_to_shift = dt.Rows[i]["transfer_to_shift"];
                                                        objIds.expire_time = dt.Rows[i]["expire_time"];
                                                        objIds.actual_billing_amount = dt.Rows[i]["actual_billing_amount"];
                                                        objIds.total_route_time = dt.Rows[i]["total_route_time"];
                                                        objIds.send_to_pt = dt.Rows[i]["send_to_pt"];
                                                        objIds.miles = dt.Rows[i]["miles"];
                                                        objIds.labor_cost = dt.Rows[i]["labor_cost"];
                                                        objIds.labor_allocation_method_text = dt.Rows[i]["labor_allocation_method_text"];

                                                        objIds.labor_allocation_method = dt.Rows[i]["labor_allocation_method"];
                                                        objIds.vehicle_cost = dt.Rows[i]["vehicle_cost"];
                                                        objIds.company_number_text = dt.Rows[i]["company_number_text"];
                                                        objIds.company_number = dt.Rows[i]["company_number"];
                                                        objIds.route_code = dt.Rows[i]["route_code"];
                                                        objIds.overhead_allocation_method_text = dt.Rows[i]["overhead_allocation_method_text"];
                                                        objIds.overhead_allocation_method = dt.Rows[i]["overhead_allocation_method"];
                                                        objIds.updated_by = dt.Rows[i]["updated_by"];
                                                        objIds.id = dt.Rows[i]["id"];
                                                        objIds.dispatcher_id = dt.Rows[i]["dispatcher_id"];
                                                        objIds.az_equip3 = dt.Rows[i]["az_equip3"];
                                                        objIds.posted_time = dt.Rows[i]["posted_time"];
                                                        objIds.updated_date = dt.Rows[i]["updated_date"];
                                                        objIds.start_time = dt.Rows[i]["start_time"];
                                                        objIds.created_time = dt.Rows[i]["created_time"];
                                                        objIds.stops = dt.Rows[i]["stops"];
                                                        objIds.posted_by = dt.Rows[i]["posted_by"];
                                                        objIds.billing_method_text = dt.Rows[i]["billing_method_text"];
                                                        objIds.billing_method = dt.Rows[i]["billing_method"];
                                                        objIds.billing_level_text = dt.Rows[i]["billing_level_text"];
                                                        objIds.billing_level = dt.Rows[i]["billing_level"];
                                                        objIds.vehicle_allocation_method_text = dt.Rows[i]["vehicle_allocation_method_text"];
                                                        objIds.vehicle_allocation_method = dt.Rows[i]["vehicle_allocation_method"];

                                                        objIds.assigned_driver_agent = dt.Rows[i]["assigned_driver_agent"];
                                                        objIds.shipper_type_text = dt.Rows[i]["shipper_type_text"];
                                                        objIds.shipper_type = dt.Rows[i]["shipper_type"];
                                                        objIds.total_break_time = dt.Rows[i]["total_break_time"];
                                                        objIds.transfer_to_company = dt.Rows[i]["transfer_to_company"];
                                                        objIds.hours = dt.Rows[i]["hours"];
                                                        objIds.actual_total_weight = dt.Rows[i]["actual_total_weight"];
                                                        objIds.branch_id = dt.Rows[i]["branch_id"];
                                                        objIds.service_time = dt.Rows[i]["service_time"];
                                                        objIds.close_time = dt.Rows[i]["close_time"];
                                                        objIds.route_comments = dt.Rows[i]["route_comments"];
                                                        objIds.total_settlement_amount = dt.Rows[i]["total_settlement_amount"];
                                                        objIds.route_date = dt.Rows[i]["route_date"];

                                                        objIds.actual_settlement_amount = dt.Rows[i]["actual_settlement_amount"];
                                                        objIds.route_late_start_code = dt.Rows[i]["route_late_start_code"];
                                                        objIds.shift_id = dt.Rows[i]["shift_id"];
                                                        objIds.start_location = dt.Rows[i]["start_location"];
                                                        objIds.transfer_to_branch = dt.Rows[i]["transfer_to_branch"];
                                                        objIds.updated_time = dt.Rows[i]["updated_time"];
                                                        objIds.route_closed = dt.Rows[i]["route_closed"];
                                                        objIds.az_equip1 = dt.Rows[i]["az_equip1"];

                                                        objIds.time_to_reseq = dt.Rows[i]["time_to_reseq"];
                                                        objIds.overhead_cost = dt.Rows[i]["overhead_cost"];
                                                        objIds.assigned_vehicle = dt.Rows[i]["assigned_vehicle"];
                                                        objIds.per_stop_billing_amount = dt.Rows[i]["per_stop_billing_amount"];
                                                        objIds.last_updated_date = dt.Rows[i]["last_updated_date"];
                                                        objIds.total_break_minutes = dt.Rows[i]["total_break_minutes"];
                                                        objIds.total_billing_amount = dt.Rows[i]["total_billing_amount"];
                                                        objIds.push_services = dt.Rows[i]["push_services"];
                                                        objIds.break_time = dt.Rows[i]["break_time"];
                                                        objIds.calc_eta = dt.Rows[i]["calc_eta"];
                                                        objIds.route_service_method_text = dt.Rows[i]["route_service_method_text"];
                                                        objIds.route_service_method = dt.Rows[i]["route_service_method"];
                                                        objIds.shipper_facility = dt.Rows[i]["shipper_facility"];
                                                        objIds.start_date = dt.Rows[i]["start_date"];
                                                        objIds.last_updated_time = dt.Rows[i]["last_updated_time"];
                                                        objIds.end_date = dt.Rows[i]["end_date"];
                                                        objIds.created_date = dt.Rows[i]["created_date"];
                                                        objIds.end_location = dt.Rows[i]["end_location"];
                                                        objIds.total_route_minutes = dt.Rows[i]["total_route_minutes"];
                                                        //  public List<object> notes { get; set; }
                                                        //  public List<object> route_stops { get; set; }
                                                        idList.Add(objIds);
                                                    }
                                                    objCommon.SaveOutputDataToCsvFile(idList, "Route-Header",
                                       strInputFilePath, ReferenceId, strFileName, strDatetime);
                                                }

                                                //if (dsResponse.Tables.Contains("route_stops"))
                                                //{
                                                //    List<Settlement> settelmentList = new List<Settlement>();
                                                //    for (int i = 0; i < dsResponse.Tables["route_stops"].Rows.Count; i++)
                                                //    {
                                                //        DataTable dt = dsResponse.Tables["route_stops"];
                                                //        Settlement objsettlements = new Settlement();
                                                //        objsettlements.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                //        objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                //        objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                //        objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                //        objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                //        objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                //        objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                //        objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                //        objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                //        objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                //        objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                //        objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                //        objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                //        objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                //        objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                //        objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                //        objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                //        objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                //        objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                //        objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                //        objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                //        objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                //        objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                //        objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                //        objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                //        objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                //        objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                //        objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                //        objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                //        objsettlements.id = (dt.Rows[i]["id"]);
                                                //        objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                //        objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                //        objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                //        objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                //        objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                //        objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                //        objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                //        objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                //        objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                //        objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                //        objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                //        objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                //        objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                //        objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                //        objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                //        objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                //        objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                //        objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                //        objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                //        objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                //        objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);
                                                //        settelmentList.Add(objsettlements);
                                                //    }

                                                //    objCommon.SaveOutputDataToCsvFile(settelmentList, "RouteHeaderPost-RouteStop",
                                                //       strInputFilePath, UniqueId, strFileName, strDatetime);

                                                //}

                                                //if (dsResponse.Tables.Contains("notes"))
                                                //{

                                                //    List<Note> noteList = new List<Note>();
                                                //    for (int i = 0; i < dsResponse.Tables["notes"].Rows.Count; i++)
                                                //    {
                                                //        Note note = new Note();
                                                //        DataTable dt = dsResponse.Tables["notes"];

                                                //        note.user_id = (dt.Rows[i]["user_id"]);
                                                //        note.note_line = (dt.Rows[i]["note_line"]);
                                                //        note.control_number = (dt.Rows[i]["control_number"]);
                                                //        note.note_code = (dt.Rows[i]["note_code"]);
                                                //        note.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                //        note.company_number = (dt.Rows[i]["company_number"]);
                                                //        note.entry_date = (dt.Rows[i]["entry_date"]);
                                                //        note.print_on_ticket = (dt.Rows[i]["print_on_ticket"]);
                                                //        note.show_to_cust = (dt.Rows[i]["show_to_cust"]);
                                                //        note.entry_time = (dt.Rows[i]["entry_time"]);
                                                //        note.id = (dt.Rows[i]["id"]);
                                                //        noteList.Add(note);
                                                //    }

                                                //    objCommon.SaveOutputDataToCsvFile(noteList, "RouteHeaderPost-Note",
                                                //       strInputFilePath, UniqueId, strFileName, strDatetime);

                                                //}

                                            }
                                            catch (Exception ex)
                                            {
                                                strExecutionLogMessage = "ProcessAddRouteHeaderFiles Exception -" + ex.Message + System.Environment.NewLine;
                                                strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                                strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                                strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                objCommon.WriteErrorLog(ex, strExecutionLogMessage);

                                                ErrorResponse objErrorResponse = new ErrorResponse();
                                                objErrorResponse.error = "Found exception while processing the record";
                                                objErrorResponse.status = "Error";
                                                objErrorResponse.code = "Exception while processing this record";
                                                objErrorResponse.reference = UniqueId;
                                                string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                dsFailureResponse.Tables[0].TableName = "RouteHeaderFailure";
                                                objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                       strInputFilePath, ReferenceId, strFileName, strDatetime);
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "RouteHeaderPostAPI Failed " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                            strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(objresponse.Reason);
                                            dsFailureResponse.Tables[0].TableName = "RouteHeaderFailure";
                                            dsFailureResponse.Tables[0].Columns.Add("Reference", typeof(System.String));
                                            foreach (DataRow row in dsFailureResponse.Tables[0].Rows)
                                            {
                                                row["Reference"] = ReferenceId;
                                            }
                                            objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                        strInputFilePath, ReferenceId, strFileName, strDatetime);

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        strExecutionLogMessage = "ProcessAddRouteHeaderFiles Exception -" + ex.Message + System.Environment.NewLine;
                                        strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                        strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                        strExecutionLogMessage += "For Reference -" + ReferenceId + System.Environment.NewLine;
                                        //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                        objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                    }
                                }
                                j++;
                            }

                            objCommon.MoveOutputFilesToOutputLocation(strInputFilePath);
                        }
                        else
                        {
                            strExecutionLogMessage = "Template sheet data not found for the file " + strInputFilePath + @"\" + strFileName;
                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        strExecutionLogMessage = "ProcessAddRouteHeaderFiles Exception -" + ex.Message + System.Environment.NewLine;
                        strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                        strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                        strExecutionLogMessage += "For Reference -" + ReferenceId + System.Environment.NewLine;
                        // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                        objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                    }
                }

                strExecutionLogMessage = "Finished processing all the files for the location " + strInputFilePath;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
            }
            catch (Exception ex)
            {
                // MsgBox(ex.Message).ToString()
                objCommon.WriteErrorLog(ex);
                throw new Exception("Error in ProcessAddRouteHeaderFiles -->" + ex.Message + ex.StackTrace);
            }
        }

        private static void ProcessAddRouteStopFiles(string strInputFilePath, string strLocationFolder)
        {
            clsCommon objCommon = new clsCommon();

            try
            {
                System.Configuration.AppSettingsReader reader = new System.Configuration.AppSettingsReader();
                DirectoryInfo dir;
                FileInfo[] XLSfiles;
                string strFileName;
                // string strInputFilePath;
                string strBillingHistoryFileLocation;
                string strExecutionLogMessage;
                string strExecutionLogFileLocation;
                string strDatetime;
                DataTable dtDataTable;
                //  string strSheetName;
                string ReferenceId = null;
                strExecutionLogFileLocation = objCommon.GetConfigValue("ExecutionLogFileLocation");

                strExecutionLogMessage = "Processing the Add Route Stop data for " + strInputFilePath;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                //strInputFilePath = objCommon.GetConfigValue("AutomationFileLocation") + @"\Order\Add";
                // strInputFilePath = strInputFilePath + @"\" + strLocationFolder;
                strBillingHistoryFileLocation = strInputFilePath + @"\HistoricalFiles";

                strExecutionLogMessage = "The input file Path is: " + strInputFilePath + "." + System.Environment.NewLine + "The Historical File Path is:" + strBillingHistoryFileLocation;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                dir = new DirectoryInfo(strInputFilePath);
                XLSfiles = dir.GetFiles("*.xlsx");

                strExecutionLogMessage = "Found # of Excel Files: " + XLSfiles.Count();
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                foreach (var file in XLSfiles)
                {
                    strFileName = file.ToString();
                    dtDataTable = new System.Data.DataTable();

                    try
                    {

                        DataSet dsExcel = new DataSet();
                        dsExcel = clsExcelHelper.ImportExcelXLSX(strInputFilePath + @"\" + strFileName, false);
                        if (dsExcel.Tables.Count > 0)
                        {

                            objCommon.MoveTheFileToHistoryFolder(strBillingHistoryFileLocation, file);

                            strDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");
                            dtDataTable = dsExcel.Tables[0];


                            //      var distinctValues = dtDataTable.Select(r => new { r.con, r.attribute2_name }).Distinct();

                            //    var distinctValues = dtDataTable.AsEnumerable()
                            //.Select(row => new {
                            //    Company = row.Field<string>("Company"),
                            //    Reference = row.Field<string>("Reference")
                            //})
                            //.Distinct();

                            DataView view = new DataView(dtDataTable);
                            DataTable dtdistinctValues = view.ToTable(true, "Customer_Reference");
                            clsRoute objclsRoute = new clsRoute();

                            string CustomerName = strFileName.Split('-')[0].ToUpper();
                            string LocationCode = strFileName.Split('-')[1].ToUpper();
                            string ProductCode = strFileName.Split('-')[2].ToUpper();

                            clsCommon.DSResponse objDsResponse = new clsCommon.DSResponse();
                            objDsResponse = objclsRoute.GetRouteStopDetails(CustomerName, LocationCode, ProductCode);
                            if (objDsResponse.dsResp.ResponseVal)
                            {
                                DataTable dtCustomerMapping = objDsResponse.DS.Tables[0];
                                DataTable dtServiceTypes;
                                clsCommon.DSResponse objDsServviceTypeResponse = new clsCommon.DSResponse();
                                objDsServviceTypeResponse = objclsRoute.GetServiceTypeDetails(Convert.ToString(dtCustomerMapping.Rows[0]["Company"]), Convert.ToString(dtCustomerMapping.Rows[0]["CustomerNumber"]));
                                if (objDsServviceTypeResponse.dsResp.ResponseVal)
                                {
                                    //objroute_stop.service_level = Convert.ToInt32(objDsServviceTypeResponse.DS.Tables[0].Rows[0]["service_type"]);
                                    dtServiceTypes = objDsServviceTypeResponse.DS.Tables[0];
                                }
                                else
                                {
                                    strExecutionLogMessage = "RouteStopPostAPI Service Type Mapping Not Found " + System.Environment.NewLine;
                                    strExecutionLogMessage += "CustomerName -" + CustomerName + System.Environment.NewLine;
                                    strExecutionLogMessage += "LocationCode -" + LocationCode + System.Environment.NewLine;
                                    strExecutionLogMessage += "Company -" + Convert.ToString(dtCustomerMapping.Rows[0]["Company"]) + System.Environment.NewLine;
                                    strExecutionLogMessage += "CustomerNumber -" + Convert.ToString(dtCustomerMapping.Rows[0]["CustomerNumber"]) + System.Environment.NewLine;

                                    strExecutionLogMessage += "Please Put entry for this service type in Service Type mapping" + System.Environment.NewLine;
                                    objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                    ErrorResponse objErrorResponse = new ErrorResponse();
                                    objErrorResponse.error = " Service Type Mapping Missing";
                                    objErrorResponse.status = "Error";
                                    objErrorResponse.code = "Service Type Mapping Missing";
                                    objErrorResponse.reference = ReferenceId;
                                    string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                    DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                    dsFailureResponse.Tables[0].TableName = "RouteStopFailure";
                                    objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                   strInputFilePath, ReferenceId, strFileName, strDatetime);
                                    continue;

                                }

                                foreach (DataRow dr in dtdistinctValues.Rows)
                                {

                                    object value = dr["Customer_Reference"];
                                    if (value == DBNull.Value)
                                        break;
                                    ReferenceId = Convert.ToString(dr["Customer_Reference"]);
                                    try
                                    {

                                        DataRow[] drresult = dtDataTable.Select("Customer_Reference= '" + dr["Customer_Reference"] + "'");

                                        route_stopdetails objroute_stopdetails = new route_stopdetails();
                                        route_stop objroute_stop = new route_stop();

                                        int service_level = 0;

                                        DataRow[] drservice_level = dtServiceTypes.Select("CustomerServiceLevelCode= '" + drresult[0]["Service Type"] + "'");

                                        if (drservice_level.Length > 0)
                                        {
                                            //service_level
                                            service_level = Convert.ToInt32(drservice_level[0]["Service_Type"]);

                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "RouteStopPostAPI Service Type Mapping Not Found " + System.Environment.NewLine;
                                            strExecutionLogMessage += "CustomerName -" + CustomerName + System.Environment.NewLine;
                                            strExecutionLogMessage += "LocationCode -" + LocationCode + System.Environment.NewLine;
                                            strExecutionLogMessage += "Company -" + Convert.ToString(dtCustomerMapping.Rows[0]["Company"]) + System.Environment.NewLine;
                                            strExecutionLogMessage += "Service Type -" + Convert.ToString(drresult[0]["Service Type"]) + System.Environment.NewLine;
                                            strExecutionLogMessage += "Please Put entry for this service type in Service Type mapping" + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                            objErrorResponse.error = "Service Type Mapping not done for service type - :" + Convert.ToString(drresult[0]["Service Type"]);
                                            objErrorResponse.status = "Error";
                                            objErrorResponse.code = "Service Type Mapping Entry Missing";
                                            objErrorResponse.reference = ReferenceId;
                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                            dsFailureResponse.Tables[0].TableName = "RouteStopFailure";
                                            objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                           strInputFilePath, ReferenceId, strFileName, strDatetime);
                                            continue;
                                        }
                                        Boolean boolReturn = false;
                                        //DateTime dtValue;
                                        DateTime? dtValue = null;

                                        List<items> objitemsList = new List<items>();
                                        foreach (DataRow drItems in drresult)
                                        {
                                            items objitems = new items();
                                            objitems.company_number = Convert.ToInt32(dtCustomerMapping.Rows[0]["Company"]);
                                            // objitems.unique_id = Convert.ToInt32(dr["unique_id"]);
                                            objitems.actual_cod_type = Convert.ToString(dtCustomerMapping.Rows[0]["actual_cod_type"]);

                                            if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["barcodes_unique"])))
                                                objitems.barcodes_unique = Convert.ToString(dtCustomerMapping.Rows[0]["barcodes_unique"]);

                                            if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["cod_type"])))
                                                objitems.cod_type = Convert.ToString(dtCustomerMapping.Rows[0]["cod_type"]);

                                            objitems.photos_exist = Convert.ToString(dtCustomerMapping.Rows[0]["photos_exist"]);
                                            //objitems.@return = Convert.ToString(drItems["Return"]);
                                            objitems.expected_pieces = Convert.ToInt32(drItems["Pieces"]);
                                            //   objitems.expected_weight = Convert.ToInt32(drItems["Weight"]);
                                            if (drItems.Table.Columns.Contains("Weight"))
                                            {
                                                if (!String.IsNullOrEmpty(Convert.ToString(drItems["Weight"])))
                                                {
                                                    objitems.expected_weight = Convert.ToInt32(Convert.ToDouble(drItems["Weight"]));
                                                }
                                                else
                                                {
                                                    if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["item_expected_weight"])))
                                                        objitems.expected_weight = Convert.ToInt32(dtCustomerMapping.Rows[0]["item_expected_weight"]);
                                                }
                                            }
                                            else
                                            {
                                                if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["item_expected_weight"])))
                                                    objitems.expected_weight = Convert.ToInt32(dtCustomerMapping.Rows[0]["item_expected_weight"]);
                                            }

                                            objitems.item_description = StripUnicodeCharactersFromString(Convert.ToString(drItems["Item Description"]));
                                            objitems.item_number = Convert.ToString(drItems["Item Number"]);
                                            //  objitems.container_id = Convert.ToString(drItems["Container Id"]);
                                            objitems.reference = Convert.ToString(drItems["Customer_Reference"]);

                                            if (drItems.Table.Columns.Contains("Return"))
                                            {
                                                if (!String.IsNullOrEmpty(Convert.ToString(drItems["Return"])))
                                                {
                                                    string str = Convert.ToString(drItems["Return"]);
                                                    if (Convert.ToString(drItems["Return"]).ToUpper() == "YES")
                                                    {
                                                        str = "Y";
                                                        if (!boolReturn)
                                                            boolReturn = true;
                                                    }
                                                    else if (Convert.ToString(drItems["Return"]).ToUpper() == "Y")
                                                    {
                                                        str = "Y";
                                                        if (!boolReturn)
                                                            boolReturn = true;
                                                    }
                                                    else if (Convert.ToString(drItems["Return"]) == "1")
                                                    {
                                                        str = "Y";
                                                        if (!boolReturn)
                                                            boolReturn = true;
                                                    }
                                                    else
                                                    {
                                                        str = "N";
                                                    }
                                                    objitems.@return = str;
                                                }
                                            }

                                            if (drItems.Table.Columns.Contains("Bol Number"))
                                            {
                                                if (!String.IsNullOrEmpty(Convert.ToString(drresult[0]["Bol Number"])))
                                                {
                                                    objroute_stop.bol_number = StripUnicodeCharactersFromString(Convert.ToString(drresult[0]["Bol Number"]));
                                                }
                                            }
                                            if (drItems.Table.Columns.Contains("RequestedDeliveryDate"))
                                            {
                                                if (!String.IsNullOrEmpty(Convert.ToString(drresult[0]["RequestedDeliveryDate"])))
                                                {
                                                    dtValue = Convert.ToDateTime(drresult[0]["RequestedDeliveryDate"]);
                                                }
                                            }

                                            objitemsList.Add(objitems);
                                        }

                                        objroute_stop.items = objitemsList;

                                        // objroute_stop.service_level = Convert.ToInt32(objDsServviceTypeResponse.DS.Tables[0].Rows[0]["service_type"]);
                                        objroute_stop.service_level = Convert.ToInt32(service_level);
                                        objroute_stop.address_name = StripUnicodeCharactersFromString(Convert.ToString(drresult[0]["Delivery Name"]));
                                        objroute_stop.address = StripUnicodeCharactersFromString(Convert.ToString(drresult[0]["Delivery Address"]));
                                        objroute_stop.city = StripUnicodeCharactersFromString(Convert.ToString(drresult[0]["Delivery City"]));
                                        objroute_stop.state = Convert.ToString(drresult[0]["Delivery State"]);
                                        objroute_stop.zip_code = Convert.ToString(drresult[0]["Delivery Zip"]);
                                        objroute_stop.reference = Convert.ToString(drresult[0]["Customer_Reference"]);
                                        objroute_stop.phone = Convert.ToString(drresult[0]["Delivery Phone Number"]);

                                        objroute_stop.company_number = Convert.ToString(dtCustomerMapping.Rows[0]["Company"]);
                                        //objroute_stop.unique_id = Convert.ToInt32(objCommon.GeneareteUnigueId()); // Convert.ToInt32(dr["Unique Id"]);
                                        objroute_stop.actual_cod_type = Convert.ToString(dtCustomerMapping.Rows[0]["actual_cod_type"]);
                                        objroute_stop.callback_required = Convert.ToString(dtCustomerMapping.Rows[0]["callback_required"]);

                                        objroute_stop.customer_number = Convert.ToInt32(dtCustomerMapping.Rows[0]["CustomerNumber"]);
                                        objroute_stop.origin_code = Convert.ToString(dtCustomerMapping.Rows[0]["origin_code"]);
                                        objroute_stop.photos_exist = Convert.ToString(dtCustomerMapping.Rows[0]["photos_exist"]);
                                        objroute_stop.posted_status = Convert.ToString(dtCustomerMapping.Rows[0]["posted_status"]);
                                        objroute_stop.required_signature_type = Convert.ToString(dtCustomerMapping.Rows[0]["required_signature_type"]);
                                        if (!dtValue.HasValue)
                                        {
                                            dtValue = System.DateTime.Now.AddDays(Convert.ToDouble(dtCustomerMapping.Rows[0]["route_date_DaysAddInToDay"]));
                                        }
                                        objroute_stop.route_date = Convert.ToDateTime(dtValue).ToString("yyyy-MM-dd");
                                        objroute_stop.sent_to_phone = Convert.ToString(dtCustomerMapping.Rows[0]["required_signature_type"]);
                                        if (boolReturn)
                                        {
                                            objroute_stop.stop_type = "P";
                                        }
                                        else
                                        {
                                            objroute_stop.stop_type = Convert.ToString(dtCustomerMapping.Rows[0]["stop_type"]);
                                        }
                                        objroute_stop.verification_id_type = Convert.ToString(dtCustomerMapping.Rows[0]["verification_id_type"]);


                                        objroute_stop.branch_id = Convert.ToString(dtCustomerMapping.Rows[0]["LocationCode"]);



                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["route_code"])))
                                            objroute_stop.route_code = Convert.ToString(dtCustomerMapping.Rows[0]["route_code"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["cod_type"])))
                                            objroute_stop.cod_type = Convert.ToString(dtCustomerMapping.Rows[0]["cod_type"]);

                                        if (String.IsNullOrEmpty(objroute_stop.bol_number))
                                        {
                                            if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["bol_number"])))
                                            {
                                                objroute_stop.bol_number = Convert.ToString(dtCustomerMapping.Rows[0]["bol_number"]);
                                            }
                                        }

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["actual_arrival_time"])))
                                        {
                                            objroute_stop.actual_arrival_time = Convert.ToString(dtCustomerMapping.Rows[0]["actual_arrival_time"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["actual_billing_amt"])))
                                        {
                                            objroute_stop.actual_billing_amt = Convert.ToDouble(dtCustomerMapping.Rows[0]["actual_billing_amt"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["actual_cod_amt"])))
                                        {
                                            objroute_stop.actual_cod_amt = Convert.ToDouble(dtCustomerMapping.Rows[0]["actual_cod_amt"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["actual_delivery_date"])))
                                        {
                                            objroute_stop.actual_delivery_date = Convert.ToString(dtCustomerMapping.Rows[0]["actual_delivery_date"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["actual_depart_time"])))
                                        {
                                            objroute_stop.actual_depart_time = Convert.ToString(dtCustomerMapping.Rows[0]["actual_depart_time"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["actual_latitude"])))
                                        {
                                            objroute_stop.actual_latitude = Convert.ToDouble(dtCustomerMapping.Rows[0]["actual_latitude"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["actual_longitude"])))
                                        {
                                            objroute_stop.actual_longitude = Convert.ToDouble(dtCustomerMapping.Rows[0]["actual_longitude"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["actual_pieces"])))
                                        {
                                            objroute_stop.actual_pieces = Convert.ToInt32(dtCustomerMapping.Rows[0]["actual_pieces"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["actual_settlement_amt"])))
                                        {
                                            objroute_stop.actual_settlement_amt = Convert.ToDouble(dtCustomerMapping.Rows[0]["actual_settlement_amt"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["actual_weight"])))
                                        {
                                            objroute_stop.actual_weight = Convert.ToInt32(dtCustomerMapping.Rows[0]["actual_weight"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["additional_instructions"])))
                                        {
                                            objroute_stop.additional_instructions = Convert.ToString(dtCustomerMapping.Rows[0]["additional_instructions"]);
                                        }

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code1"])))
                                        {
                                            objroute_stop.addl_charge_code1 = Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code1"]);
                                        }

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code2"])))
                                        {
                                            objroute_stop.addl_charge_code2 = Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code2"]);
                                        }

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code3"])))
                                        {
                                            objroute_stop.addl_charge_code3 = Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code3"]);
                                        }

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code4"])))
                                        {
                                            objroute_stop.addl_charge_code4 = Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code4"]);
                                        }

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code5"])))
                                        {
                                            objroute_stop.addl_charge_code5 = Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code5"]);
                                        }

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code6"])))
                                        {
                                            objroute_stop.addl_charge_code6 = Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code6"]);
                                        }

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code7"])))
                                        {
                                            objroute_stop.addl_charge_code7 = Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code7"]);
                                        }

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code8"])))
                                        {
                                            objroute_stop.addl_charge_code8 = Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code8"]);
                                        }

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code9"])))
                                        {
                                            objroute_stop.addl_charge_code9 = Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code9"]);
                                        }

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code10"])))
                                        {
                                            objroute_stop.addl_charge_code10 = Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code10"]);
                                        }

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code11"])))
                                        {
                                            objroute_stop.addl_charge_code11 = Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code11"]);
                                        }

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code12"])))
                                        {
                                            objroute_stop.addl_charge_code12 = Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_code12"]);
                                        }

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_occur1"])))
                                        {
                                            objroute_stop.addl_charge_occur1 = Convert.ToInt32(dtCustomerMapping.Rows[0]["addl_charge_occur1"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_occur2"])))
                                        {
                                            objroute_stop.addl_charge_occur2 = Convert.ToInt32(dtCustomerMapping.Rows[0]["addl_charge_occur2"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_occur3"])))
                                        {
                                            objroute_stop.addl_charge_occur3 = Convert.ToInt32(dtCustomerMapping.Rows[0]["addl_charge_occur3"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_occur4"])))
                                        {
                                            objroute_stop.addl_charge_occur4 = Convert.ToInt32(dtCustomerMapping.Rows[0]["addl_charge_occur4"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_occur5"])))
                                        {
                                            objroute_stop.addl_charge_occur5 = Convert.ToInt32(dtCustomerMapping.Rows[0]["addl_charge_occur5"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_occur6"])))
                                        {
                                            objroute_stop.addl_charge_occur6 = Convert.ToInt32(dtCustomerMapping.Rows[0]["addl_charge_occur6"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_occur7"])))
                                        {
                                            objroute_stop.addl_charge_occur7 = Convert.ToInt32(dtCustomerMapping.Rows[0]["addl_charge_occur7"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_occur8"])))
                                        {

                                            objroute_stop.addl_charge_occur8 = Convert.ToInt32(dtCustomerMapping.Rows[0]["addl_charge_occur8"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_occur9"])))
                                        {
                                            objroute_stop.addl_charge_occur9 = Convert.ToInt32(dtCustomerMapping.Rows[0]["addl_charge_occur9"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_occur10"])))
                                        {
                                            objroute_stop.addl_charge_occur10 = Convert.ToInt32(dtCustomerMapping.Rows[0]["addl_charge_occur10"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_occur11"])))
                                        {
                                            objroute_stop.addl_charge_occur11 = Convert.ToInt32(dtCustomerMapping.Rows[0]["addl_charge_occur11"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addl_charge_occur12"])))
                                        {
                                            objroute_stop.addl_charge_occur12 = Convert.ToInt32(dtCustomerMapping.Rows[0]["addl_charge_occur12"]);
                                        }

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["addon_billing_amt"])))
                                        {
                                            objroute_stop.addon_billing_amt = Convert.ToDouble(dtCustomerMapping.Rows[0]["addon_billing_amt"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["address_point"])))
                                        {
                                            objroute_stop.address_point = Convert.ToInt32(dtCustomerMapping.Rows[0]["address_point"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["address_point_customer"])))
                                        {
                                            objroute_stop.address_point_customer = Convert.ToInt32(dtCustomerMapping.Rows[0]["address_point_customer"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["alt_lookup"])))
                                        {
                                            objroute_stop.alt_lookup = Convert.ToString(dtCustomerMapping.Rows[0]["alt_lookup"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["arrival_time"])))
                                        {
                                            objroute_stop.arrival_time = Convert.ToString(dtCustomerMapping.Rows[0]["arrival_time"]);
                                        }
                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["asn_sent"])))
                                        //{
                                        //    objroute_stop.asn_sent = Convert.ToInt32(dtCustomerMapping.Rows[0]["asn_sent"]);
                                        //}
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["attention"])))
                                        {
                                            objroute_stop.attention = Convert.ToString(dtCustomerMapping.Rows[0]["attention"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["billing_override_amt"])))
                                        {
                                            objroute_stop.billing_override_amt = Convert.ToDouble(dtCustomerMapping.Rows[0]["billing_override_amt"]);
                                        }

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["c2_paperwork"])))
                                        {
                                            objroute_stop.c2_paperwork = Convert.ToString(dtCustomerMapping.Rows[0]["c2_paperwork"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["cases"])))
                                        {
                                            objroute_stop.cases = Convert.ToInt32(dtCustomerMapping.Rows[0]["cases"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["cod_amount"])))
                                        {
                                            objroute_stop.cod_amount = Convert.ToDouble(dtCustomerMapping.Rows[0]["cod_amount"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["cod_check_no"])))
                                        {
                                            objroute_stop.cod_check_no = Convert.ToString(dtCustomerMapping.Rows[0]["cod_check_no"]);
                                        }
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["combine_data"])))
                                            objroute_stop.combine_data = Convert.ToString(dtCustomerMapping.Rows[0]["combine_data"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["comments"])))
                                            objroute_stop.comments = Convert.ToString(dtCustomerMapping.Rows[0]["comments"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["created_by"])))
                                        //    objroute_stop.created_by = Convert.ToString(dtCustomerMapping.Rows[0]["created_by"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["created_date"])))
                                        //    objroute_stop.created_date = Convert.ToString(dtCustomerMapping.Rows[0]["created_date"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["created_time"])))
                                        //    objroute_stop.created_time = Convert.ToString(dtCustomerMapping.Rows[0]["created_time"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["departure_time"])))
                                            objroute_stop.departure_time = Convert.ToString(dtCustomerMapping.Rows[0]["departure_time"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["dispatch_zone"])))
                                            objroute_stop.dispatch_zone = Convert.ToString(dtCustomerMapping.Rows[0]["dispatch_zone"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["driver_app_status"])))
                                            objroute_stop.driver_app_status = Convert.ToString(dtCustomerMapping.Rows[0]["driver_app_status"]);


                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["eta"])))
                                            objroute_stop.eta = Convert.ToString(dtCustomerMapping.Rows[0]["eta"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["eta_date"])))
                                            objroute_stop.eta_date = Convert.ToString(dtCustomerMapping.Rows[0]["eta_date"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["exception_code"])))
                                            objroute_stop.exception_code = Convert.ToString(dtCustomerMapping.Rows[0]["exception_code"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["expected_pieces"])))
                                            objroute_stop.expected_pieces = Convert.ToInt32(dtCustomerMapping.Rows[0]["expected_pieces"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["expected_weight"])))
                                            objroute_stop.expected_weight = Convert.ToInt32(dtCustomerMapping.Rows[0]["expected_weight"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["height"])))
                                            objroute_stop.height = Convert.ToInt32(dtCustomerMapping.Rows[0]["height"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["image_sign_req"])))
                                            objroute_stop.image_sign_req = Convert.ToString(dtCustomerMapping.Rows[0]["image_sign_req"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["insurance_value"])))
                                            objroute_stop.insurance_value = Convert.ToInt32(dtCustomerMapping.Rows[0]["insurance_value"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["invoice_number"])))
                                            objroute_stop.invoice_number = Convert.ToString(dtCustomerMapping.Rows[0]["invoice_number"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["item_scans_required"])))
                                            objroute_stop.item_scans_required = Convert.ToString(dtCustomerMapping.Rows[0]["item_scans_required"]);


                                        //// objroute_stop.items = Convert.ToString(dtCustomerMapping.Rows[0]["items"]); // already added
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["late_notice_date"])))
                                            objroute_stop.late_notice_date = Convert.ToString(dtCustomerMapping.Rows[0]["late_notice_date"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["late_notice_time"])))
                                            objroute_stop.late_notice_time = Convert.ToString(dtCustomerMapping.Rows[0]["late_notice_time"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["latitude"])))
                                            objroute_stop.latitude = Convert.ToDouble(dtCustomerMapping.Rows[0]["latitude"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["length"])))
                                            objroute_stop.length = Convert.ToInt32(dtCustomerMapping.Rows[0]["length"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["loaded_pieces"])))
                                            objroute_stop.loaded_pieces = Convert.ToInt32(dtCustomerMapping.Rows[0]["loaded_pieces"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["location_accuracy"])))
                                            objroute_stop.location_accuracy = Convert.ToInt32(dtCustomerMapping.Rows[0]["location_accuracy"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["longitude"])))
                                            objroute_stop.longitude = Convert.ToDouble(dtCustomerMapping.Rows[0]["longitude"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["minutes_late"])))
                                            objroute_stop.minutes_late = Convert.ToInt32(dtCustomerMapping.Rows[0]["minutes_late"]);

                                        ////  objroute_stop.notes = Convert.ToString(dtCustomerMapping.Rows[0]["notes"]);
                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["ordered_by"])))
                                            objroute_stop.ordered_by = Convert.ToString(dtCustomerMapping.Rows[0]["ordered_by"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["orig_order_number"])))
                                        //    objroute_stop.orig_order_number = Convert.ToInt32(dtCustomerMapping.Rows[0]["orig_order_number"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["original_id"])))
                                        //    objroute_stop.original_id = Convert.ToInt32(dtCustomerMapping.Rows[0]["original_id"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["override_settle_percent"])))
                                            objroute_stop.override_settle_percent = Convert.ToDouble(dtCustomerMapping.Rows[0]["override_settle_percent"]);


                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["phone_ext"])))
                                            objroute_stop.phone_ext = Convert.ToInt32(dtCustomerMapping.Rows[0]["phone_ext"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["posted_by"])))
                                        //    objroute_stop.posted_by = Convert.ToString(dtCustomerMapping.Rows[0]["posted_by"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["posted_date"])))
                                        //    objroute_stop.posted_date = Convert.ToString(dtCustomerMapping.Rows[0]["posted_date"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["posted_time"])))
                                            objroute_stop.posted_time = Convert.ToString(dtCustomerMapping.Rows[0]["posted_time"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["pricing_zone"])))
                                            objroute_stop.pricing_zone = Convert.ToInt32(dtCustomerMapping.Rows[0]["pricing_zone"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["progress"])))   
                                        //    objroute_stop.progress = Convert.ToString(dtCustomerMapping.Rows[0]["progress"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["received_branch"])))
                                        //    objroute_stop.received_branch = Convert.ToString(dtCustomerMapping.Rows[0]["received_branch"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["received_company"])))
                                        //    objroute_stop.received_company = Convert.ToInt32(dtCustomerMapping.Rows[0]["received_company"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["received_pieces"])))
                                            objroute_stop.received_pieces = Convert.ToInt32(dtCustomerMapping.Rows[0]["received_pieces"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["received_route"])))
                                        //    objroute_stop.received_route = Convert.ToString(dtCustomerMapping.Rows[0]["received_route"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["received_sequence"])))
                                        //    objroute_stop.received_sequence = Convert.ToString(dtCustomerMapping.Rows[0]["received_sequence"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["received_shift"])))
                                        //    objroute_stop.received_shift = Convert.ToString(dtCustomerMapping.Rows[0]["received_shift"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["received_unique_id"])))
                                        //    objroute_stop.received_unique_id = Convert.ToInt32(dtCustomerMapping.Rows[0]["received_unique_id"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["redelivery"])))
                                        //    objroute_stop.redelivery = Convert.ToString(dtCustomerMapping.Rows[0]["redelivery"]);


                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["return"])))
                                        //    objroute_stop.@return= Convert.ToString(dtCustomerMapping.Rows[0]["return"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["return_redel_id"])))
                                            objroute_stop.return_redel_id = Convert.ToInt32(dtCustomerMapping.Rows[0]["return_redel_id"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["return_redelivery_date"])))
                                            objroute_stop.return_redelivery_date = Convert.ToString(dtCustomerMapping.Rows[0]["return_redelivery_date"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["return_redelivery_flag"])))
                                            objroute_stop.return_redelivery_flag = Convert.ToString(dtCustomerMapping.Rows[0]["return_redelivery_flag"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["room"])))
                                            objroute_stop.room = Convert.ToString(dtCustomerMapping.Rows[0]["room"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["schedule_stop_id"])))
                                            objroute_stop.schedule_stop_id = Convert.ToInt32(dtCustomerMapping.Rows[0]["schedule_stop_id"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["service_time"])))
                                            objroute_stop.service_time = Convert.ToInt32(dtCustomerMapping.Rows[0]["service_time"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["settlement_override_amt"])))
                                            objroute_stop.settlement_override_amt = Convert.ToDouble(dtCustomerMapping.Rows[0]["settlement_override_amt"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["shift_id"])))
                                            objroute_stop.shift_id = Convert.ToString(dtCustomerMapping.Rows[0]["shift_id"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["signature"])))
                                            objroute_stop.signature = Convert.ToString(dtCustomerMapping.Rows[0]["signature"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["signature_filename"])))
                                            objroute_stop.signature_filename = Convert.ToString(dtCustomerMapping.Rows[0]["signature_filename"]);


                                        //  objroute_stop.signature_images = Convert.ToString(dtCustomerMapping.Rows[0]["signature_images"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["signature_required"])))
                                            objroute_stop.signature_required = Convert.ToString(dtCustomerMapping.Rows[0]["signature_required"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["special_instructions1"])))
                                            objroute_stop.special_instructions1 = Convert.ToString(dtCustomerMapping.Rows[0]["special_instructions1"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["special_instructions2"])))
                                            objroute_stop.special_instructions2 = Convert.ToString(dtCustomerMapping.Rows[0]["special_instructions2"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["special_instructions3"])))
                                            objroute_stop.special_instructions3 = Convert.ToString(dtCustomerMapping.Rows[0]["special_instructions3"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["special_instructions4"])))
                                            objroute_stop.special_instructions4 = Convert.ToString(dtCustomerMapping.Rows[0]["special_instructions4"]);


                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["stop_sequence"])))
                                            objroute_stop.stop_sequence = Convert.ToString(dtCustomerMapping.Rows[0]["stop_sequence"]);


                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["times_sent"])))
                                            objroute_stop.times_sent = Convert.ToInt32(dtCustomerMapping.Rows[0]["times_sent"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["totes"])))
                                            objroute_stop.totes = Convert.ToInt32(dtCustomerMapping.Rows[0]["totes"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["transfer_to_route"])))
                                            objroute_stop.transfer_to_route = Convert.ToString(dtCustomerMapping.Rows[0]["transfer_to_route"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["transfer_to_sequence"])))
                                            objroute_stop.transfer_to_sequence = Convert.ToString(dtCustomerMapping.Rows[0]["transfer_to_sequence"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["updated_by"])))
                                        //    objroute_stop.updated_by = Convert.ToString(dtCustomerMapping.Rows[0]["updated_by"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["updated_by_scanner"])))
                                            objroute_stop.updated_by_scanner = Convert.ToString(dtCustomerMapping.Rows[0]["updated_by_scanner"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["updated_date"])))
                                        //    objroute_stop.updated_date = Convert.ToString(dtCustomerMapping.Rows[0]["updated_date"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["updated_time"])))
                                        //    objroute_stop.updated_time = Convert.ToString(dtCustomerMapping.Rows[0]["updated_time"]);

                                        //if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["upload_time"])))
                                        //    objroute_stop.upload_time = Convert.ToString(dtCustomerMapping.Rows[0]["upload_time"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["vehicle"])))
                                            objroute_stop.vehicle = Convert.ToString(dtCustomerMapping.Rows[0]["vehicle"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["verification_id_details"])))
                                            objroute_stop.verification_id_details = Convert.ToString(dtCustomerMapping.Rows[0]["verification_id_details"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["width"])))
                                            objroute_stop.width = Convert.ToInt32(dtCustomerMapping.Rows[0]["width"]);

                                        if (!String.IsNullOrEmpty(Convert.ToString(dtCustomerMapping.Rows[0]["zip_code"])))
                                            objroute_stop.zip_code = Convert.ToString(dtCustomerMapping.Rows[0]["zip_code"]);


                                        objroute_stopdetails.route_stop = objroute_stop;

                                        clsCommon.ReturnResponse objresponse = new clsCommon.ReturnResponse();
                                        string request = JsonConvert.SerializeObject(objroute_stopdetails);
                                        objresponse = objclsRoute.CallDataTracRouteStopPostAPI(request);
                                        // objresponse.ResponseVal = true;
                                        //  objresponse.Reason = "{\"00100035009\":{\"room\":null,\"unique_id\":35009,\"c2_paperwork\":false,\"company_number_text\":\"EASTRN TIME ON CENTRAL SERVER\",\"company_number\":1,\"addl_charge_code11\":null,\"billing_override_amt\":null,\"addl_charge_occur1\":null,\"updated_time\":null,\"stop_sequence\":\"0010\",\"phone\":null,\"city\":\"Alpharetta\",\"created_by\":\"DX*\",\"signature_images\":[],\"pricing_zone\":null,\"signature_filename\":null,\"addl_charge_code10\":null,\"cod_check_no\":null,\"length\":null,\"expected_weight\":null,\"actual_settlement_amt\":null,\"actual_pieces\":null,\"updated_date\":null,\"schedule_stop_id\":null,\"photos_exist\":false,\"stop_type_text\":\"Delivery\",\"stop_type\":\"D\",\"return\":false,\"addl_charge_code6\":null,\"dispatch_zone\":null,\"upload_time\":null,\"actual_cod_amt\":null,\"location_accuracy\":null,\"progress\":[{\"status_time\":\"10:22:02\",\"status_text\":\"Entered in carrier's system\",\"status_date\":\"2021-08-04\"}],\"received_route\":null,\"override_settle_percent\":null,\"cod_amount\":null,\"addl_charge_code9\":null,\"eta_date\":null,\"cod_type_text\":\"None\",\"cod_type\":\"0\",\"addl_charge_occur3\":null,\"reference\":null,\"sent_to_phone\":false,\"addl_charge_occur12\":null,\"callback_required_text\":\"No\",\"callback_required\":\"N\",\"service_level_text\":\"1HR RUSH SERVICE\",\"service_level\":6,\"original_id\":null,\"width\":null,\"received_sequence\":null,\"transfer_to_sequence\":null,\"cases\":null,\"times_sent\":0,\"transfer_to_route\":null,\"zip_code\":null,\"settlement_override_amt\":null,\"driver_app_status_text\":\"\",\"driver_app_status\":\"0\",\"route_code_text\":\"NAPA\",\"route_code\":\"NAPA\",\"received_shift\":null,\"addl_charge_occur6\":null,\"addl_charge_occur11\":null,\"vehicle\":null,\"addl_charge_code5\":null,\"addl_charge_occur9\":null,\"eta\":null,\"departure_time\":null,\"combine_data\":null,\"actual_latitude\":null,\"posted_by\":null,\"insurance_value\":null,\"return_redel_id\":null,\"addl_charge_code1\":null,\"origin_code_text\":\"Added using API\",\"origin_code\":\"A\",\"ordered_by\":null,\"posted_date\":null,\"actual_billing_amt\":null,\"created_date\":\"2021-08-04\",\"latitude\":null,\"received_pieces\":null,\"addl_charge_code7\":null,\"totes\":null,\"asn_sent\":0,\"comments\":null,\"verification_id_type_text\":\"None\",\"verification_id_type\":\"0\",\"posted_time\":null,\"item_scans_required\":true,\"shift_id\":null,\"addon_billing_amt\":null,\"actual_delivery_date\":null,\"id\":\"00100035009\",\"actual_arrival_time\":null,\"signature_required\":true,\"longitude\":null,\"expected_pieces\":null,\"loaded_pieces\":null,\"alt_lookup\":null,\"customer_number_text\":\"Routing Customer\",\"customer_number\":4999,\"created_time\":\"10:22:02\",\"addl_charge_code8\":null,\"signature\":null,\"actual_depart_time\":null,\"bol_number\":null,\"actual_cod_type_text\":\"None\",\"actual_cod_type\":\"0\",\"invoice_number\":null,\"branch_id\":null,\"special_instructions2\":null,\"updated_by\":null,\"verification_id_details\":null,\"required_signature_type_text\":\"Any signature\",\"required_signature_type\":\"0\",\"addl_charge_occur7\":null,\"orig_order_number\":null,\"special_instructions1\":null,\"notes\":[],\"image_sign_req\":true,\"attention\":null,\"minutes_late\":0,\"late_notice_time\":null,\"received_unique_id\":null,\"exception_code\":null,\"addl_charge_code4\":null,\"addl_charge_occur4\":null,\"redelivery\":false,\"addl_charge_occur10\":null,\"upload_date\":null,\"special_instructions4\":null,\"address_name\":null,\"addl_charge_occur8\":null,\"address_point_customer\":null,\"received_branch\":null,\"items\":[],\"return_redelivery_date\":null,\"height\":null,\"actual_longitude\":null,\"service_time\":null,\"phone_ext\":null,\"addl_charge_occur2\":null,\"late_notice_date\":null,\"address\":\"123 Stop Address Street\",\"arrival_time\":null,\"posted_status\":false,\"route_date\":\"2021-08-03\",\"addl_charge_code12\":null,\"addl_charge_code3\":null,\"return_redelivery_flag_text\":\"None\",\"return_redelivery_flag\":\"N\",\"additional_instructions\":null,\"updated_by_scanner\":false,\"special_instructions3\":null,\"addl_charge_occur5\":null,\"address_point\":0,\"actual_weight\":null,\"received_company\":null,\"addl_charge_code2\":null,\"state\":\"GA\"}}";
                                        // objresponse.Reason = "{\"00204352124\": {\"posted_by\": null, \"addon_billing_amt\": null, \"minutes_late\": 0, \"insurance_value\": null, \"addl_charge_occur5\": null, \"actual_pieces\": null, \"actual_depart_time\": null, \"created_time\": \"08:43:34\", \"cod_amount\": null, \"special_instructions3\": null, \"width\": null, \"ordered_by\": null, \"addl_charge_code1\": null, \"signature_filename\": null, \"updated_date\": null, \"latitude\": null, \"signature\": null, \"received_branch\": null, \"late_notice_time\": null, \"route_code_text\": \"HDHOLD\", \"route_code\": \"HDHOLD\", \"phone_ext\": null, \"addl_charge_occur1\": null, \"received_sequence\": null, \"address_name\": \"TEST1\", \"address_point_customer\": null, \"actual_cod_amt\": null, \"signature_required\": true, \"stop_type_text\": \"Delivery\", \"stop_type\": \"D\", \"origin_code_text\": \"Added using API\", \"origin_code\": \"A\", \"invoice_number\": null, \"addl_charge_code11\": null, \"addl_charge_code12\": null, \"length\": null, \"vehicle\": null, \"item_scans_required\": true, \"updated_by_scanner\": false, \"addl_charge_code10\": null, \"unique_id\": 4352124, \"attention\": null, \"items\": [{\"item_number\": \"item1\", \"item_description\": \"first item\", \"reference\": \"1\", \"rma_route\": null, \"upload_time\": null, \"rma_stop_id\": 0, \"width\": null, \"redelivery\": false, \"received_pieces\": null, \"cod_amount\": null, \"height\": null, \"comments\": null, \"actual_pieces\": null, \"actual_cod_amount\": null, \"rma_number\": null, \"manually_updated\": 0, \"unique_id\": 4352124, \"cod_type_text\": \"None\", \"cod_type\": \"0\", \"barcodes_unique\": false, \"actual_cod_type_text\": \"None\", \"actual_cod_type\": \"0\", \"return_redel_seq\": 0, \"expected_pieces\": 3, \"signature\": null, \"exception_code\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"updated_date\": null, \"expected_weight\": 50, \"created_date\": \"2021-08-26\", \"rma_origin\": null, \"created_by\": \"DX*\", \"loaded_pieces\": null, \"return_redelivery_flag_text\": \"\", \"return_redelivery_flag\": null, \"original_id\": 0, \"container_id\": \"container1\", \"return\": false, \"length\": null, \"notes\": [], \"actual_weight\": null, \"updated_by\": null, \"photos_exist\": false, \"second_container_id\": null, \"return_redel_id\": 0, \"asn_sent\": 0, \"actual_departure_time\": null, \"updated_time\": null, \"return_redelivery_date\": null, \"actual_arrival_time\": null, \"item_sequence\": 1, \"pallet_number\": null, \"actual_date\": null, \"insurance_value\": null, \"created_time\": \"08:43:34\", \"upload_date\": null, \"scans\": [], \"id\": \"002043521240001\", \"truck_id\": 0}, {\"item_number\": \"item2\", \"item_description\": \"first item\", \"reference\": \"1\", \"rma_route\": null, \"upload_time\": null, \"rma_stop_id\": 0, \"width\": null, \"redelivery\": false, \"received_pieces\": null, \"cod_amount\": null, \"height\": null, \"comments\": null, \"actual_pieces\": null, \"actual_cod_amount\": null, \"rma_number\": null, \"manually_updated\": 0, \"unique_id\": 4352124, \"cod_type_text\": \"None\", \"cod_type\": \"0\", \"barcodes_unique\": false, \"actual_cod_type_text\": \"None\", \"actual_cod_type\": \"0\", \"return_redel_seq\": 0, \"expected_pieces\": 1, \"signature\": null, \"exception_code\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"updated_date\": null, \"expected_weight\": 150, \"created_date\": \"2021-08-26\", \"rma_origin\": null, \"created_by\": \"DX*\", \"loaded_pieces\": null, \"return_redelivery_flag_text\": \"\", \"return_redelivery_flag\": null, \"original_id\": 0, \"container_id\": \"container2\", \"return\": false, \"length\": null, \"notes\": [], \"actual_weight\": null, \"updated_by\": null, \"photos_exist\": false, \"second_container_id\": null, \"return_redel_id\": 0, \"asn_sent\": 0, \"actual_departure_time\": null, \"updated_time\": null, \"return_redelivery_date\": null, \"actual_arrival_time\": null, \"item_sequence\": 2, \"pallet_number\": null, \"actual_date\": null, \"insurance_value\": null, \"created_time\": \"08:43:34\", \"upload_date\": null, \"scans\": [], \"id\": \"002043521240002\", \"truck_id\": 0}, {\"item_number\": \"item3\", \"item_description\": \"first item\", \"reference\": \"1\", \"rma_route\": null, \"upload_time\": null, \"rma_stop_id\": 0, \"width\": null, \"redelivery\": false, \"received_pieces\": null, \"cod_amount\": null, \"height\": null, \"comments\": null, \"actual_pieces\": null, \"actual_cod_amount\": null, \"rma_number\": null, \"manually_updated\": 0, \"unique_id\": 4352124, \"cod_type_text\": \"None\", \"cod_type\": \"0\", \"barcodes_unique\": false, \"actual_cod_type_text\": \"None\", \"actual_cod_type\": \"0\", \"return_redel_seq\": 0, \"expected_pieces\": 1, \"signature\": null, \"exception_code\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"updated_date\": null, \"expected_weight\": 250, \"created_date\": \"2021-08-26\", \"rma_origin\": null, \"created_by\": \"DX*\", \"loaded_pieces\": null, \"return_redelivery_flag_text\": \"\", \"return_redelivery_flag\": null, \"original_id\": 0, \"container_id\": \"container3\", \"return\": false, \"length\": null, \"notes\": [], \"actual_weight\": null, \"updated_by\": null, \"photos_exist\": false, \"second_container_id\": null, \"return_redel_id\": 0, \"asn_sent\": 0, \"actual_departure_time\": null, \"updated_time\": null, \"return_redelivery_date\": null, \"actual_arrival_time\": null, \"item_sequence\": 3, \"pallet_number\": null, \"actual_date\": null, \"insurance_value\": null, \"created_time\": \"08:43:35\", \"upload_date\": null, \"scans\": [], \"id\": \"002043521240003\", \"truck_id\": 0}, {\"item_number\": \"item4\", \"item_description\": \"first item\", \"reference\": \"1\", \"rma_route\": null, \"upload_time\": null, \"rma_stop_id\": 0, \"width\": null, \"redelivery\": false, \"received_pieces\": null, \"cod_amount\": null, \"height\": null, \"comments\": null, \"actual_pieces\": null, \"actual_cod_amount\": null, \"rma_number\": null, \"manually_updated\": 0, \"unique_id\": 4352124, \"cod_type_text\": \"None\", \"cod_type\": \"0\", \"barcodes_unique\": false, \"actual_cod_type_text\": \"None\", \"actual_cod_type\": \"0\", \"return_redel_seq\": 0, \"expected_pieces\": 21, \"signature\": null, \"exception_code\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"updated_date\": null, \"expected_weight\": 350, \"created_date\": \"2021-08-26\", \"rma_origin\": null, \"created_by\": \"DX*\", \"loaded_pieces\": null, \"return_redelivery_flag_text\": \"\", \"return_redelivery_flag\": null, \"original_id\": 0, \"container_id\": \"container4\", \"return\": false, \"length\": null, \"notes\": [], \"actual_weight\": null, \"updated_by\": null, \"photos_exist\": false, \"second_container_id\": null, \"return_redel_id\": 0, \"asn_sent\": 0, \"actual_departure_time\": null, \"updated_time\": null, \"return_redelivery_date\": null, \"actual_arrival_time\": null, \"item_sequence\": 4, \"pallet_number\": null, \"actual_date\": null, \"insurance_value\": null, \"created_time\": \"08:43:36\", \"upload_date\": null, \"scans\": [], \"id\": \"002043521240004\", \"truck_id\": 0}], \"addl_charge_occur10\": null, \"verification_id_type_text\": \"None\", \"verification_id_type\": \"0\", \"addl_charge_occur7\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"posted_time\": null, \"c2_paperwork\": false, \"original_id\": null, \"progress\": [{\"status_time\": \"08:43:34\", \"status_date\": \"2021-08-26\", \"status_text\": \"Entered in carrier's system\"}], \"service_level_text\": \"Basic Delivery\", \"service_level\": 56, \"created_by\": \"DX*\", \"required_signature_type_text\": \"Any signature\", \"required_signature_type\": \"0\", \"special_instructions1\": null, \"actual_billing_amt\": null, \"branch_id_text\": \"JWL Baltimore, MD\", \"branch_id\": \"BWI\", \"actual_cod_type_text\": \"None\", \"actual_cod_type\": \"0\", \"pricing_zone\": null, \"state\": \"TX\", \"signature_images\": [], \"special_instructions4\": null, \"photos_exist\": false, \"height\": null, \"eta_date\": null, \"upload_date\": null, \"zip_code\": \"75034\", \"actual_latitude\": null, \"override_settle_percent\": null, \"notes\": [{\"entry_time\": \"08:43:34\", \"note_text\": \"** Expected pieces: 0 -> 3\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"item_sequence\": null, \"entry_date\": \"2021-08-26\", \"user_entered\": false, \"show_to_cust\": false, \"note_type_text\": \"Stop\", \"note_type\": \"0\", \"unique_id\": 4352124, \"id\": \"00204352124    0020210826084334DX* 24\", \"user_id\": \"DX*\"}, {\"entry_time\": \"08:43:34\", \"note_text\": \"** Expected weight:      0 ->      50\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"item_sequence\": null, \"entry_date\": \"2021-08-26\", \"user_entered\": false, \"show_to_cust\": false, \"note_type_text\": \"Stop\", \"note_type\": \"0\", \"unique_id\": 4352124, \"id\": \"00204352124    0020210826084334DX* 25\", \"user_id\": \"DX*\"}, {\"entry_time\": \"08:43:35\", \"note_text\": \"** Expected pieces: 3 -> 4\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"item_sequence\": null, \"entry_date\": \"2021-08-26\", \"user_entered\": false, \"show_to_cust\": false, \"note_type_text\": \"Stop\", \"note_type\": \"0\", \"unique_id\": 4352124, \"id\": \"00204352124    0020210826084335DX* 24\", \"user_id\": \"DX*\"}, {\"entry_time\": \"08:43:35\", \"note_text\": \"** Expected weight:     50 ->     200\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"item_sequence\": null, \"entry_date\": \"2021-08-26\", \"user_entered\": false, \"show_to_cust\": false, \"note_type_text\": \"Stop\", \"note_type\": \"0\", \"unique_id\": 4352124, \"id\": \"00204352124    0020210826084335DX* 25\", \"user_id\": \"DX*\"}, {\"entry_time\": \"08:43:36\", \"note_text\": \"** Expected pieces: 4 -> 5\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"item_sequence\": null, \"entry_date\": \"2021-08-26\", \"user_entered\": false, \"show_to_cust\": false, \"note_type_text\": \"Stop\", \"note_type\": \"0\", \"unique_id\": 4352124, \"id\": \"00204352124    0020210826084336DX* 24\", \"user_id\": \"DX*\"}, {\"entry_time\": \"08:43:36\", \"note_text\": \"** Expected weight:    200 ->     450\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"item_sequence\": null, \"entry_date\": \"2021-08-26\", \"user_entered\": false, \"show_to_cust\": false, \"note_type_text\": \"Stop\", \"note_type\": \"0\", \"unique_id\": 4352124, \"id\": \"00204352124    0020210826084336DX* 25\", \"user_id\": \"DX*\"}, {\"entry_time\": \"08:43:37\", \"note_text\": \"** Expected pieces: 5 -> 26\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"item_sequence\": null, \"entry_date\": \"2021-08-26\", \"user_entered\": false, \"show_to_cust\": false, \"note_type_text\": \"Stop\", \"note_type\": \"0\", \"unique_id\": 4352124, \"id\": \"00204352124    0020210826084337DX* 24\", \"user_id\": \"DX*\"}, {\"entry_time\": \"08:43:37\", \"note_text\": \"** Expected weight:    450 ->     800\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"item_sequence\": null, \"entry_date\": \"2021-08-26\", \"user_entered\": false, \"show_to_cust\": false, \"note_type_text\": \"Stop\", \"note_type\": \"0\", \"unique_id\": 4352124, \"id\": \"00204352124    0020210826084337DX* 25\", \"user_id\": \"DX*\"}], \"additional_instructions\": null, \"addl_charge_occur6\": null, \"driver_app_status_text\": \"\", \"driver_app_status\": \"0\", \"combine_data\": null, \"addl_charge_code2\": null, \"service_time\": null, \"city\": \"FRISCO\", \"room\": null, \"addl_charge_code7\": null, \"billing_override_amt\": null, \"totes\": null, \"sent_to_phone\": false, \"address\": \"1000 PARKWOOD BLVD\", \"posted_date\": null, \"phone\": \"111-111-1111\", \"late_notice_date\": null, \"received_route\": null, \"bol_number\": \"1\", \"asn_sent\": 0, \"addl_charge_occur3\": null, \"departure_time\": null, \"received_unique_id\": null, \"orig_order_number\": null, \"reference\": \"1\", \"comments\": null, \"updated_by\": null, \"customer_number_text\": \"HD - BWI 21229\", \"customer_number\": 516, \"addl_charge_code4\": null, \"addl_charge_code9\": null, \"location_accuracy\": null, \"verification_id_details\": null, \"cases\": null, \"actual_arrival_time\": null, \"received_company\": null, \"addl_charge_code5\": null, \"addl_charge_occur11\": null, \"addl_charge_code6\": null, \"actual_settlement_amt\": null, \"addl_charge_occur12\": null, \"cod_check_no\": null, \"updated_time\": null, \"expected_pieces\": 26, \"times_sent\": 0, \"addl_charge_occur9\": null, \"id\": \"00204352124\", \"route_date\": \"2021-08-31\", \"schedule_stop_id\": null, \"return\": false, \"addl_charge_occur4\": null, \"image_sign_req\": false, \"created_date\": \"2021-08-26\", \"longitude\": null, \"redelivery\": false, \"actual_weight\": null, \"cod_type_text\": \"None\", \"cod_type\": \"0\", \"eta\": null, \"transfer_to_sequence\": null, \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"alt_lookup\": null, \"addl_charge_occur8\": null, \"posted_status\": false, \"addl_charge_occur2\": null, \"transfer_to_route\": null, \"shift_id\": null, \"addl_charge_code8\": null, \"upload_time\": null, \"received_shift\": null, \"return_redel_id\": null, \"addl_charge_code3\": null, \"stop_sequence\": \"0010\", \"dispatch_zone\": null, \"expected_weight\": 800, \"special_instructions2\": null, \"actual_longitude\": null, \"settlement_override_amt\": null, \"actual_delivery_date\": null, \"arrival_time\": null, \"return_redelivery_flag_text\": \"None\", \"return_redelivery_flag\": \"N\", \"loaded_pieces\": null, \"exception_code\": null, \"address_point\": 0, \"return_redelivery_date\": null, \"received_pieces\": null, \"_utc_offset\": \"-04:00\"}}";
                                        if (objresponse.ResponseVal)
                                        {
                                            strExecutionLogMessage = "RouteStopPostAPI Success " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                            strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            // DataSet dsOrderResponse = objCommon.jsonToDataSet(objresponse.Reason, "RouteStopPostAPI");
                                            DataSet dsResponse = objCommon.jsonToDataSet(objresponse.Reason, "RouteStopPostAPI");
                                            var UniqueId = Convert.ToString(dsResponse.Tables[0].Rows[0]["id"]);
                                            try
                                            {
                                                if (dsResponse.Tables.Contains(UniqueId))
                                                {
                                                    List<ResponseRouteStop> idList = new List<ResponseRouteStop>();
                                                    for (int i = 0; i < dsResponse.Tables[0].Rows.Count; i++)
                                                    {
                                                        DataTable dt = new DataTable();
                                                        dt = dsResponse.Tables[0];
                                                        ResponseRouteStop objIds = new ResponseRouteStop();

                                                        objIds.room = dt.Rows[i]["room"];
                                                        objIds.unique_id = dt.Rows[i]["unique_id"];

                                                        objIds.c2_paperwork = dt.Rows[i]["c2_paperwork"];
                                                        objIds.company_number_text = dt.Rows[i]["company_number_text"];
                                                        objIds.company_number = dt.Rows[i]["company_number"];
                                                        objIds.addl_charge_code11 = dt.Rows[i]["addl_charge_code11"];
                                                        objIds.billing_override_amt = dt.Rows[i]["billing_override_amt"];
                                                        objIds.addl_charge_occur1 = dt.Rows[i]["addl_charge_occur1"];
                                                        objIds.updated_time = dt.Rows[i]["updated_time"];
                                                        objIds.stop_sequence = dt.Rows[i]["stop_sequence"];

                                                        objIds.phone = dt.Rows[i]["phone"];
                                                        objIds.city = dt.Rows[i]["city"];
                                                        objIds.created_by = dt.Rows[i]["created_by"];
                                                        objIds.pricing_zone = dt.Rows[i]["pricing_zone"];
                                                        objIds.signature_filename = dt.Rows[i]["signature_filename"];
                                                        objIds.addl_charge_code10 = dt.Rows[i]["addl_charge_code10"];
                                                        objIds.cod_check_no = dt.Rows[i]["cod_check_no"];
                                                        objIds.length = dt.Rows[i]["length"];

                                                        objIds.expected_weight = dt.Rows[i]["expected_weight"];
                                                        objIds.actual_settlement_amt = dt.Rows[i]["actual_settlement_amt"];
                                                        objIds.actual_pieces = dt.Rows[i]["actual_pieces"];
                                                        objIds.updated_date = dt.Rows[i]["updated_date"];
                                                        objIds.schedule_stop_id = dt.Rows[i]["schedule_stop_id"];
                                                        objIds.photos_exist = dt.Rows[i]["photos_exist"];
                                                        objIds.stop_type_text = dt.Rows[i]["stop_type_text"];
                                                        objIds.stop_type = dt.Rows[i]["stop_type"];
                                                        objIds.@return = dt.Rows[i]["return"];
                                                        objIds.addl_charge_code6 = dt.Rows[i]["addl_charge_code6"];
                                                        objIds.dispatch_zone = dt.Rows[i]["dispatch_zone"];
                                                        objIds.upload_time = dt.Rows[i]["upload_time"];
                                                        objIds.actual_cod_amt = dt.Rows[i]["actual_cod_amt"];
                                                        objIds.location_accuracy = dt.Rows[i]["location_accuracy"];
                                                        objIds.received_route = dt.Rows[i]["received_route"];
                                                        objIds.override_settle_percent = dt.Rows[i]["override_settle_percent"];
                                                        objIds.cod_amount = dt.Rows[i]["cod_amount"];
                                                        objIds.addl_charge_code9 = dt.Rows[i]["addl_charge_code9"];
                                                        objIds.eta_date = dt.Rows[i]["eta_date"];
                                                        objIds.cod_type_text = dt.Rows[i]["cod_type_text"];
                                                        objIds.cod_type = dt.Rows[i]["cod_type"];
                                                        objIds.addl_charge_occur3 = dt.Rows[i]["addl_charge_occur3"];
                                                        objIds.reference = dt.Rows[i]["reference"];
                                                        objIds.sent_to_phone = dt.Rows[i]["sent_to_phone"];
                                                        objIds.addl_charge_occur12 = dt.Rows[i]["addl_charge_occur12"];
                                                        objIds.callback_required_text = dt.Rows[i]["callback_required_text"];
                                                        objIds.callback_required = dt.Rows[i]["callback_required"];
                                                        objIds.service_level_text = dt.Rows[i]["service_level_text"];
                                                        objIds.service_level = dt.Rows[i]["service_level"];
                                                        objIds.original_id = dt.Rows[i]["original_id"];
                                                        objIds.width = dt.Rows[i]["width"];
                                                        objIds.received_sequence = dt.Rows[i]["received_sequence"];
                                                        objIds.transfer_to_sequence = dt.Rows[i]["transfer_to_sequence"];
                                                        objIds.cases = dt.Rows[i]["cases"];
                                                        objIds.times_sent = dt.Rows[i]["times_sent"];
                                                        objIds.transfer_to_route = dt.Rows[i]["transfer_to_route"];
                                                        objIds.zip_code = dt.Rows[i]["zip_code"];
                                                        objIds.settlement_override_amt = dt.Rows[i]["settlement_override_amt"];
                                                        objIds.driver_app_status_text = dt.Rows[i]["driver_app_status_text"];
                                                        objIds.driver_app_status = dt.Rows[i]["driver_app_status"];
                                                        objIds.route_code_text = dt.Rows[i]["route_code_text"];
                                                        objIds.route_code = dt.Rows[i]["route_code"];
                                                        objIds.received_shift = dt.Rows[i]["received_shift"];
                                                        objIds.addl_charge_occur6 = dt.Rows[i]["addl_charge_occur6"];
                                                        objIds.addl_charge_occur11 = dt.Rows[i]["addl_charge_occur11"];
                                                        objIds.vehicle = dt.Rows[i]["vehicle"];
                                                        objIds.addl_charge_code5 = dt.Rows[i]["addl_charge_code5"];
                                                        objIds.addl_charge_occur9 = dt.Rows[i]["addl_charge_occur9"];

                                                        objIds.eta = dt.Rows[i]["eta"];
                                                        objIds.departure_time = dt.Rows[i]["departure_time"];
                                                        objIds.combine_data = dt.Rows[i]["combine_data"];
                                                        objIds.actual_latitude = dt.Rows[i]["actual_latitude"];
                                                        objIds.posted_by = dt.Rows[i]["posted_by"];
                                                        objIds.insurance_value = dt.Rows[i]["insurance_value"];
                                                        objIds.return_redel_id = dt.Rows[i]["return_redel_id"];
                                                        objIds.addl_charge_code1 = dt.Rows[i]["addl_charge_code1"];
                                                        objIds.origin_code_text = dt.Rows[i]["origin_code_text"];
                                                        objIds.origin_code = dt.Rows[i]["origin_code"];
                                                        objIds.ordered_by = dt.Rows[i]["ordered_by"];
                                                        objIds.posted_date = dt.Rows[i]["posted_date"];
                                                        objIds.actual_billing_amt = dt.Rows[i]["actual_billing_amt"];
                                                        objIds.created_date = dt.Rows[i]["created_date"];
                                                        objIds.latitude = dt.Rows[i]["latitude"];
                                                        objIds.received_pieces = dt.Rows[i]["received_pieces"];
                                                        objIds.addl_charge_code7 = dt.Rows[i]["addl_charge_code7"];
                                                        objIds.totes = dt.Rows[i]["totes"];
                                                        objIds.asn_sent = dt.Rows[i]["asn_sent"];
                                                        objIds.comments = dt.Rows[i]["comments"];
                                                        objIds.verification_id_type_text = dt.Rows[i]["verification_id_type_text"];
                                                        objIds.verification_id_type = dt.Rows[i]["verification_id_type"];
                                                        objIds.posted_time = dt.Rows[i]["posted_time"];
                                                        objIds.item_scans_required = dt.Rows[i]["item_scans_required"];
                                                        objIds.shift_id = dt.Rows[i]["shift_id"];
                                                        objIds.addon_billing_amt = dt.Rows[i]["addon_billing_amt"];
                                                        objIds.actual_delivery_date = dt.Rows[i]["actual_delivery_date"];
                                                        objIds.id = dt.Rows[i]["id"];
                                                        objIds.actual_arrival_time = dt.Rows[i]["actual_arrival_time"];
                                                        objIds.signature_required = dt.Rows[i]["signature_required"];
                                                        objIds.longitude = dt.Rows[i]["longitude"];
                                                        objIds.expected_pieces = dt.Rows[i]["expected_pieces"];
                                                        objIds.loaded_pieces = dt.Rows[i]["loaded_pieces"];
                                                        objIds.alt_lookup = dt.Rows[i]["alt_lookup"];
                                                        objIds.customer_number_text = dt.Rows[i]["customer_number_text"];
                                                        objIds.customer_number = dt.Rows[i]["customer_number"];
                                                        objIds.created_time = dt.Rows[i]["created_time"];
                                                        objIds.addl_charge_code8 = dt.Rows[i]["addl_charge_code8"];
                                                        objIds.signature = dt.Rows[i]["signature"];
                                                        objIds.actual_depart_time = dt.Rows[i]["actual_depart_time"];
                                                        objIds.bol_number = dt.Rows[i]["bol_number"];
                                                        objIds.actual_cod_type_text = dt.Rows[i]["actual_cod_type_text"];
                                                        objIds.actual_cod_type = dt.Rows[i]["actual_cod_type"];
                                                        objIds.invoice_number = dt.Rows[i]["invoice_number"];
                                                        objIds.branch_id = dt.Rows[i]["branch_id"];
                                                        objIds.special_instructions2 = dt.Rows[i]["special_instructions2"];
                                                        objIds.updated_by = dt.Rows[i]["updated_by"];
                                                        objIds.verification_id_details = dt.Rows[i]["verification_id_details"];
                                                        objIds.required_signature_type_text = dt.Rows[i]["required_signature_type_text"];
                                                        objIds.required_signature_type = dt.Rows[i]["required_signature_type"];
                                                        objIds.addl_charge_occur7 = dt.Rows[i]["addl_charge_occur7"];
                                                        objIds.orig_order_number = dt.Rows[i]["orig_order_number"];
                                                        objIds.special_instructions1 = dt.Rows[i]["special_instructions1"];
                                                        objIds.image_sign_req = dt.Rows[i]["image_sign_req"];
                                                        objIds.attention = dt.Rows[i]["attention"];
                                                        objIds.minutes_late = dt.Rows[i]["minutes_late"];
                                                        objIds.late_notice_time = dt.Rows[i]["late_notice_time"];
                                                        objIds.received_unique_id = dt.Rows[i]["received_unique_id"];
                                                        objIds.exception_code = dt.Rows[i]["exception_code"];
                                                        objIds.addl_charge_code4 = dt.Rows[i]["addl_charge_code4"];
                                                        objIds.addl_charge_occur4 = dt.Rows[i]["addl_charge_occur4"];
                                                        objIds.redelivery = dt.Rows[i]["redelivery"];
                                                        objIds.addl_charge_occur10 = dt.Rows[i]["addl_charge_occur10"];
                                                        objIds.upload_date = dt.Rows[i]["upload_date"];
                                                        objIds.special_instructions4 = dt.Rows[i]["special_instructions4"];
                                                        objIds.address_name = dt.Rows[i]["address_name"];
                                                        objIds.addl_charge_occur8 = dt.Rows[i]["addl_charge_occur8"];
                                                        objIds.address_point_customer = dt.Rows[i]["address_point_customer"];
                                                        objIds.received_branch = dt.Rows[i]["received_branch"];
                                                        objIds.return_redelivery_date = dt.Rows[i]["return_redelivery_date"];
                                                        objIds.height = dt.Rows[i]["height"];
                                                        objIds.actual_longitude = dt.Rows[i]["actual_longitude"];
                                                        objIds.service_time = dt.Rows[i]["service_time"];
                                                        objIds.phone_ext = dt.Rows[i]["phone_ext"];
                                                        objIds.addl_charge_occur2 = dt.Rows[i]["addl_charge_occur2"];
                                                        objIds.late_notice_date = dt.Rows[i]["late_notice_date"];
                                                        objIds.address = dt.Rows[i]["address"];
                                                        objIds.arrival_time = dt.Rows[i]["arrival_time"];
                                                        objIds.posted_status = dt.Rows[i]["posted_status"];
                                                        objIds.route_date = dt.Rows[i]["route_date"];
                                                        objIds.addl_charge_code12 = dt.Rows[i]["addl_charge_code12"];
                                                        objIds.addl_charge_code3 = dt.Rows[i]["addl_charge_code3"];
                                                        objIds.return_redelivery_flag_text = dt.Rows[i]["return_redelivery_flag_text"];
                                                        objIds.return_redelivery_flag = dt.Rows[i]["return_redelivery_flag"];
                                                        objIds.additional_instructions = dt.Rows[i]["additional_instructions"];
                                                        objIds.updated_by_scanner = dt.Rows[i]["updated_by_scanner"];
                                                        objIds.special_instructions3 = dt.Rows[i]["special_instructions3"];
                                                        objIds.addl_charge_occur5 = dt.Rows[i]["addl_charge_occur5"];
                                                        objIds.address_point = dt.Rows[i]["address_point"];
                                                        objIds.actual_weight = dt.Rows[i]["actual_weight"];
                                                        objIds.received_company = dt.Rows[i]["received_company"];
                                                        objIds.addl_charge_code2 = dt.Rows[i]["addl_charge_code2"];
                                                        objIds.state = dt.Rows[i]["state"];
                                                        // public object @return { get; set; }
                                                        idList.Add(objIds);
                                                    }
                                                    objCommon.SaveOutputDataToCsvFile(idList, "RouteStop-Create",
                                       strInputFilePath, ReferenceId, strFileName, strDatetime);
                                                }

                                                if (dsResponse.Tables.Contains("progress"))
                                                {

                                                    List<RouteStopResponseProgress> progressList = new List<RouteStopResponseProgress>();
                                                    for (int i = 0; i < dsResponse.Tables["progress"].Rows.Count; i++)
                                                    {
                                                        RouteStopResponseProgress progress = new RouteStopResponseProgress();
                                                        DataTable dt = new DataTable();
                                                        dt = dsResponse.Tables["progress"];

                                                        progress.status_date = (dt.Rows[i]["status_date"]);
                                                        progress.status_text = (dt.Rows[i]["status_text"]);
                                                        progress.status_time = (dt.Rows[i]["status_time"]);
                                                        progress.id = (dt.Rows[i]["id"]);
                                                        progressList.Add(progress);
                                                    }

                                                    objCommon.SaveOutputDataToCsvFile(progressList, "RouteStop-Progress",
                                                         strInputFilePath, UniqueId, strFileName, strDatetime);
                                                }

                                                //  public List<object> signature_images { get; set; }
                                                // public List<Progress> progress { get; set; }
                                                // public List<object> notes { get; set; }
                                                // public List<object> items { get; set; }

                                                if (dsResponse.Tables.Contains("notes"))
                                                {

                                                    List<RouteStopResponseNote> noteList = new List<RouteStopResponseNote>();
                                                    for (int i = 0; i < dsResponse.Tables["notes"].Rows.Count; i++)
                                                    {
                                                        RouteStopResponseNote note = new RouteStopResponseNote();
                                                        DataTable dt = new DataTable();
                                                        dt = dsResponse.Tables["notes"];
                                                        note.entry_time = (dt.Rows[i]["entry_time"]);
                                                        note.note_text = (dt.Rows[i]["note_text"]);
                                                        note.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                        note.company_number = (dt.Rows[i]["company_number"]);
                                                        note.item_sequence = (dt.Rows[i]["item_sequence"]);
                                                        note.user_id = (dt.Rows[i]["user_id"]);
                                                        note.entry_date = (dt.Rows[i]["entry_date"]);
                                                        note.user_entered = (dt.Rows[i]["user_entered"]);
                                                        note.show_to_cust = (dt.Rows[i]["show_to_cust"]);
                                                        note.note_type_text = (dt.Rows[i]["note_type_text"]);
                                                        note.note_type = (dt.Rows[i]["note_type"]);
                                                        note.unique_id = (dt.Rows[i]["unique_id"]);
                                                        note.id = (dt.Rows[i]["id"]);
                                                        noteList.Add(note);
                                                    }

                                                    objCommon.SaveOutputDataToCsvFile(noteList, "RouteStop-Note",
                                                       strInputFilePath, UniqueId, strFileName, strDatetime);

                                                }

                                                if (dsResponse.Tables.Contains("items"))
                                                {

                                                    List<RouteStopResponseItem> itemList = new List<RouteStopResponseItem>();
                                                    for (int i = 0; i < dsResponse.Tables["items"].Rows.Count; i++)
                                                    {
                                                        RouteStopResponseItem item = new RouteStopResponseItem();
                                                        DataTable dt = new DataTable();
                                                        dt = dsResponse.Tables["items"];

                                                        item.item_number = (dt.Rows[i]["item_number"]);
                                                        item.item_description = (dt.Rows[i]["item_description"]);
                                                        item.reference = (dt.Rows[i]["reference"]);
                                                        item.rma_route = (dt.Rows[i]["rma_route"]);
                                                        item.upload_time = (dt.Rows[i]["upload_time"]);
                                                        item.rma_stop_id = (dt.Rows[i]["rma_stop_id"]);
                                                        item.width = (dt.Rows[i]["width"]);
                                                        item.redelivery = (dt.Rows[i]["redelivery"]);
                                                        item.received_pieces = (dt.Rows[i]["received_pieces"]);
                                                        item.cod_amount = (dt.Rows[i]["cod_amount"]);
                                                        item.height = (dt.Rows[i]["height"]);
                                                        item.comments = (dt.Rows[i]["comments"]);
                                                        item.actual_pieces = (dt.Rows[i]["actual_pieces"]);
                                                        item.actual_cod_amount = (dt.Rows[i]["actual_cod_amount"]);
                                                        item.rma_number = (dt.Rows[i]["rma_number"]);
                                                        item.manually_updated = (dt.Rows[i]["manually_updated"]);
                                                        item.unique_id = (dt.Rows[i]["unique_id"]);
                                                        item.cod_type_text = (dt.Rows[i]["cod_type_text"]);
                                                        item.cod_type = (dt.Rows[i]["cod_type"]);
                                                        item.barcodes_unique = (dt.Rows[i]["barcodes_unique"]);
                                                        item.actual_cod_type = (dt.Rows[i]["actual_cod_type"]);
                                                        item.return_redel_seq = (dt.Rows[i]["return_redel_seq"]);
                                                        item.expected_pieces = (dt.Rows[i]["expected_pieces"]);
                                                        item.signature = (dt.Rows[i]["signature"]);
                                                        item.exception_code = (dt.Rows[i]["exception_code"]);
                                                        item.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                        item.company_number = (dt.Rows[i]["company_number"]);
                                                        item.updated_date = (dt.Rows[i]["updated_date"]);
                                                        item.expected_weight = (dt.Rows[i]["expected_weight"]);
                                                        item.created_date = (dt.Rows[i]["created_date"]);
                                                        item.rma_origin = (dt.Rows[i]["rma_origin"]);
                                                        item.created_by = (dt.Rows[i]["created_by"]);
                                                        item.loaded_pieces = (dt.Rows[i]["loaded_pieces"]);
                                                        item.return_redelivery_flag_text = (dt.Rows[i]["return_redelivery_flag_text"]);
                                                        item.return_redelivery_flag = (dt.Rows[i]["return_redelivery_flag"]);
                                                        item.original_id = (dt.Rows[i]["original_id"]);
                                                        item.container_id = (dt.Rows[i]["container_id"]);
                                                        item.@return = (dt.Rows[i]["return"]);
                                                        item.length = (dt.Rows[i]["length"]);
                                                        item.actual_weight = (dt.Rows[i]["actual_weight"]);
                                                        item.updated_by = (dt.Rows[i]["updated_by"]);
                                                        item.photos_exist = (dt.Rows[i]["photos_exist"]);
                                                        item.second_container_id = (dt.Rows[i]["second_container_id"]);
                                                        item.return_redel_id = (dt.Rows[i]["return_redel_id"]);
                                                        item.asn_sent = (dt.Rows[i]["asn_sent"]);
                                                        item.actual_departure_time = (dt.Rows[i]["actual_departure_time"]);
                                                        item.updated_time = (dt.Rows[i]["updated_time"]);
                                                        item.return_redelivery_date = (dt.Rows[i]["return_redelivery_date"]);
                                                        item.actual_arrival_time = (dt.Rows[i]["actual_arrival_time"]);
                                                        item.item_sequence = (dt.Rows[i]["item_sequence"]);
                                                        item.pallet_number = (dt.Rows[i]["pallet_number"]);
                                                        item.actual_date = (dt.Rows[i]["actual_date"]);
                                                        item.insurance_value = (dt.Rows[i]["insurance_value"]);
                                                        item.created_time = (dt.Rows[i]["created_time"]);
                                                        item.upload_date = (dt.Rows[i]["upload_date"]);
                                                        item.id = (dt.Rows[i]["id"]);
                                                        item.truck_id = (dt.Rows[i]["truck_id"]);

                                                        // public List<object> notes { get; set; }
                                                        // public List<object> scans { get; set; }
                                                        itemList.Add(item);
                                                    }

                                                    objCommon.SaveOutputDataToCsvFile(itemList, "RouteStop-Item",
                                                       strInputFilePath, UniqueId, strFileName, strDatetime);

                                                }

                                            }
                                            catch (Exception ex)
                                            {
                                                strExecutionLogMessage = "RouteStopPostFiles Exception -" + ex.Message + System.Environment.NewLine;
                                                strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                                strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                                strExecutionLogMessage += "For Reference -" + ReferenceId + System.Environment.NewLine;
                                                //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                objCommon.WriteErrorLog(ex, strExecutionLogMessage);

                                                ErrorResponse objErrorResponse = new ErrorResponse();
                                                objErrorResponse.error = "Found exception while processing the record";
                                                objErrorResponse.status = "Error";
                                                objErrorResponse.code = "Excception while procesing the record.";
                                                objErrorResponse.reference = ReferenceId;
                                                string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                dsFailureResponse.Tables[0].TableName = "RouteStopFailure";
                                                objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                               strInputFilePath, ReferenceId, strFileName, strDatetime);
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "RouteStopPostAPI Failed " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                            strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(objresponse.Reason);
                                            dsFailureResponse.Tables[0].TableName = "RouteStopFailure";
                                            dsFailureResponse.Tables[0].Columns.Add("Reference", typeof(System.String));
                                            foreach (DataRow row in dsFailureResponse.Tables[0].Rows)
                                            {
                                                row["Reference"] = ReferenceId;
                                            }
                                            objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                        strInputFilePath, ReferenceId, strFileName, strDatetime);

                                        }


                                    }
                                    catch (Exception ex)
                                    {
                                        strExecutionLogMessage = "ProcessAddRouteStopFiles Exception -" + ex.Message + System.Environment.NewLine;
                                        strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                        strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                        strExecutionLogMessage += "For Reference -" + ReferenceId + System.Environment.NewLine;
                                        //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                        objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                    }
                                }
                            }
                            else
                            {
                                strExecutionLogMessage = "RouteHeaderPostAPI RouteStop Customer Mapping Not Found " + System.Environment.NewLine;
                                strExecutionLogMessage += "CustomerName -" + CustomerName + System.Environment.NewLine;
                                strExecutionLogMessage += "LocationCode -" + LocationCode + System.Environment.NewLine;
                                strExecutionLogMessage += "Please Put entry for this customer in Route Stop Customer mapping" + System.Environment.NewLine;
                                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                break;

                            }


                            objCommon.MoveOutputFilesToOutputLocation(strInputFilePath);
                        }
                        else
                        {
                            strExecutionLogMessage = "Template sheet data not found for the file " + strInputFilePath + @"\" + strFileName;
                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        strExecutionLogMessage = "ProcessAddRouteStopFiles Exception -" + ex.Message + System.Environment.NewLine;
                        strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                        strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                        strExecutionLogMessage += "For Reference -" + ReferenceId + System.Environment.NewLine;
                        // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                        objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                    }
                }

                strExecutionLogMessage = "Finished processing all the files for the location " + strInputFilePath;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
            }
            catch (Exception ex)
            {
                objCommon.WriteErrorLog(ex);
                throw new Exception("Error in ProcessAddRouteStopFiles -->" + ex.Message + ex.StackTrace);
            }
        }

        public static IEnumerable<IEnumerable<T>> ToChunks<T>(this IEnumerable<T> enumerable, int chunkSize)
        {
            int itemsReturned = 0;
            var list = enumerable.ToList(); // Prevent multiple execution of IEnumerable.
            int count = list.Count;
            while (itemsReturned < count)
            {
                int currentChunkSize = Math.Min(chunkSize, count - itemsReturned);
                yield return list.GetRange(itemsReturned, currentChunkSize);
                itemsReturned += currentChunkSize;
            }
        }

        private static void ProcessUpdateRouteStopFiles(string strInputFilePath, string strLocationFolder)
        {
            clsCommon objCommon = new clsCommon();

            try
            {
                //System.Configuration.AppSettingsReader reader = new System.Configuration.AppSettingsReader();
                DirectoryInfo dir;
                FileInfo[] XLSfiles;
                string strFileName;
                // string strInputFilePath;
                string strBillingHistoryFileLocation;
                string strExecutionLogMessage;
                string strExecutionLogFileLocation;
                string strDatetime;
                DataTable dtDataTable;
                //  string strSheetName;
                string ReferenceId = null;
                strExecutionLogFileLocation = objCommon.GetConfigValue("ExecutionLogFileLocation");
                string GeneratedUniqueId = null;
                strExecutionLogMessage = "Processing the Add Route Stop data for " + strInputFilePath;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                //strInputFilePath = objCommon.GetConfigValue("AutomationFileLocation") + @"\Order\Add";
                // strInputFilePath = strInputFilePath + @"\" + strLocationFolder;
                strBillingHistoryFileLocation = strInputFilePath + @"\HistoricalFiles";

                strExecutionLogMessage = "The input file Path is: " + strInputFilePath + "." + System.Environment.NewLine + "The Historical File Path is:" + strBillingHistoryFileLocation;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                dir = new DirectoryInfo(strInputFilePath);
                XLSfiles = dir.GetFiles("*.xlsx");

                strExecutionLogMessage = "Found # of Excel Files: " + XLSfiles.Count();
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                foreach (var file in XLSfiles)
                {
                    strFileName = file.ToString();
                    dtDataTable = new System.Data.DataTable();

                    try
                    {

                        DataSet dsExcel = new DataSet();
                        dsExcel = clsExcelHelper.ImportExcelXLSX(strInputFilePath + @"\" + strFileName, false);
                        if (dsExcel.Tables.Count > 0)
                        {

                            objCommon.MoveTheFileToHistoryFolder(strBillingHistoryFileLocation, file);

                            strDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");

                            for (int i = dsExcel.Tables[0].Rows.Count - 1; i >= 0; i--)
                            {
                                DataRow dr = dsExcel.Tables[0].Rows[i];
                                if (dr["company_number"] == DBNull.Value && dr["unique_id"] == DBNull.Value
                                    && dr["route_code"] == DBNull.Value
                                    && dr["route_date"] == DBNull.Value
                                    && dr["signature"] == DBNull.Value)
                                    dr.Delete();
                            }
                            dsExcel.Tables[0].AcceptChanges();

                            dtDataTable = dsExcel.Tables[0];
                            // DataView view = new DataView(dtDataTable);
                            // DataTable dtdistinctValues = view.ToTable(true, "Customer_Reference");
                            clsRoute objclsRoute = new clsRoute();
                            int rowindex = 1;
                            foreach (DataRow dr in dtDataTable.Rows)
                            {

                                //object value = dr["company_number"];
                                //if (value == DBNull.Value)
                                //    break;
                                ReferenceId = Convert.ToString(dr["company_number"]) + "-" + Convert.ToString(dr["unique_id"]);
                                try
                                {

                                    //  DataRow[] drresult = dtDataTable.Select("Customer_Reference= '" + dr["Customer_Reference"] + "'");

                                    // route_stopdetails objroute_stopdetails = new route_stopdetails();
                                    //  route_stop objroute_stop = new route_stop();

                                    string routeStopPutrequest = null;
                                    if (dr.Table.Columns.Contains("company_number"))
                                    {
                                        if (!string.IsNullOrEmpty(Convert.ToString(dr["company_number"])))
                                        {
                                            routeStopPutrequest = @"'company_number': " + dr["company_number"] + ",";
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "RouteStopPut Error " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Company Number Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For row  number -" + rowindex + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                            //    objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                            objErrorResponse.error = "Company Number not found for this record";
                                            objErrorResponse.status = "Error";
                                            objErrorResponse.code = "Company Value Missing";
                                            objErrorResponse.reference = "For row  number -" + rowindex;
                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                            dsFailureResponse.Tables[0].TableName = "RouteStopPutFailure";
                                            //    objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                            //strInputFilePath, processingFileName, strDatetime);
                                            objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                 strInputFilePath, ReferenceId, strFileName, strDatetime);
                                            rowindex++;
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        strExecutionLogMessage = "RouteStopPut Error " + System.Environment.NewLine;
                                        strExecutionLogMessage += "Company Number Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                        strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                        objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                        // objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                        ErrorResponse objErrorResponse = new ErrorResponse();
                                        objErrorResponse.error = "Company column not found for this file record";
                                        objErrorResponse.status = "Error";
                                        objErrorResponse.code = "Company column Missing";
                                        objErrorResponse.reference = "For row  number -" + rowindex;
                                        string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                        DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                        dsFailureResponse.Tables[0].TableName = "RouteStopPutFailure";
                                        //    objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                        //strInputFilePath, processingFileName, strDatetime);
                                        objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                 strInputFilePath, ReferenceId, strFileName, strDatetime);
                                        rowindex++;
                                        continue;
                                    }

                                    string unique_id;
                                    if (dr.Table.Columns.Contains("unique_id"))
                                    {
                                        if (!string.IsNullOrEmpty(Convert.ToString(dr["unique_id"])))
                                        {
                                            routeStopPutrequest = routeStopPutrequest + @"'unique_id': " + dr["unique_id"] + ",";
                                            unique_id = Convert.ToString(dr["unique_id"]);
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "RouteStopPut Error " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Uniqie Id Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            // objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);


                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                            objErrorResponse.error = "Uniqie Id value not found for this  record";
                                            objErrorResponse.status = "Error";
                                            objErrorResponse.code = "Uniqie Id Value Missing";
                                            objErrorResponse.reference = "For row  number -" + rowindex;
                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                            dsFailureResponse.Tables[0].TableName = "RouteStopPutFailure";
                                            //    objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                            //strInputFilePath, processingFileName, strDatetime);
                                            objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                 strInputFilePath, ReferenceId, strFileName, strDatetime);
                                            rowindex++;
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        strExecutionLogMessage = "RouteStopPut Error " + System.Environment.NewLine;
                                        strExecutionLogMessage += "Uniqie Id Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                        strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                        objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                        //objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                        ErrorResponse objErrorResponse = new ErrorResponse();
                                        objErrorResponse.error = "Uniqie Id column not found for this record";
                                        objErrorResponse.status = "Error";
                                        objErrorResponse.code = "Uniqie Id column Missing";
                                        objErrorResponse.reference = "For row  number -" + rowindex;
                                        string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                        DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                        dsFailureResponse.Tables[0].TableName = "RouteStopPutFailure";
                                        //    objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                        //strInputFilePath, processingFileName, strDatetime);

                                        objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                   strInputFilePath, ReferenceId, strFileName, strDatetime);
                                        rowindex++;
                                        continue;
                                    }

                                    if (dr.Table.Columns.Contains("route_code"))
                                    {
                                        if (!string.IsNullOrEmpty(Convert.ToString(dr["route_code"])))
                                        {
                                            routeStopPutrequest = routeStopPutrequest + @"'route_code': '" + dr["route_code"] + "',";
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "RouteStopPut Error " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Route Code Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            // objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);


                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                            objErrorResponse.error = "Route Code value not found for this  record";
                                            objErrorResponse.status = "Error";
                                            objErrorResponse.code = "Route Code Value Missing";
                                            objErrorResponse.reference = "For row  number -" + rowindex;
                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                            dsFailureResponse.Tables[0].TableName = "RouteStopPutFailure";
                                            //    objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                            //strInputFilePath, processingFileName, strDatetime);
                                            objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                 strInputFilePath, ReferenceId, strFileName, strDatetime);
                                            rowindex++;
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        strExecutionLogMessage = "RouteStopPut Error " + System.Environment.NewLine;
                                        strExecutionLogMessage += "Route Code Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                        strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                        objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                        //objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                        ErrorResponse objErrorResponse = new ErrorResponse();
                                        objErrorResponse.error = "Route Code column not found for this record";
                                        objErrorResponse.status = "Error";
                                        objErrorResponse.code = "Route Code column Missing";
                                        objErrorResponse.reference = "For row  number -" + rowindex;
                                        string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                        DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                        dsFailureResponse.Tables[0].TableName = "RouteStopPutFailure";
                                        //    objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                        //strInputFilePath, processingFileName, strDatetime);

                                        objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                   strInputFilePath, ReferenceId, strFileName, strDatetime);
                                        rowindex++;
                                        continue;
                                    }

                                    //if (dr.Table.Columns.Contains("route_date"))
                                    //{
                                    //    if (!string.IsNullOrEmpty(Convert.ToString(dr["route_date"])))
                                    //    {
                                    //        routeStopPutrequest = routeStopPutrequest + @"'route_date': " + dr["route_date"] + ", ";
                                    //    }
                                    //}


                                    if (dr.Table.Columns.Contains("route_date"))
                                    {
                                        if (!string.IsNullOrEmpty(Convert.ToString(dr["route_date"])))
                                        {
                                            DateTime dtValue = Convert.ToDateTime(dr["route_date"]);
                                            routeStopPutrequest = routeStopPutrequest + @"'route_date': '" + dtValue.ToString("yyyy-MM-dd") + "',";
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "RouteStopPut Error " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Route Date Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            // objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);


                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                            objErrorResponse.error = "Route Date value not found for this  record";
                                            objErrorResponse.status = "Error";
                                            objErrorResponse.code = "Route Date Value Missing";
                                            objErrorResponse.reference = "For row  number -" + rowindex;
                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                            dsFailureResponse.Tables[0].TableName = "RouteStopPutFailure";
                                            //    objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                            //strInputFilePath, processingFileName, strDatetime);
                                            objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                 strInputFilePath, ReferenceId, strFileName, strDatetime);
                                            rowindex++;
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        strExecutionLogMessage = "RouteStopPut Error " + System.Environment.NewLine;
                                        strExecutionLogMessage += "Route Date Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                        strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                        objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                        //objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                        ErrorResponse objErrorResponse = new ErrorResponse();
                                        objErrorResponse.error = "Route Date column not found for this record";
                                        objErrorResponse.status = "Error";
                                        objErrorResponse.code = "Route Date column Missing";
                                        objErrorResponse.reference = "For row  number -" + rowindex;
                                        string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                        DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                        dsFailureResponse.Tables[0].TableName = "RouteStopPutFailure";
                                        //    objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                        //strInputFilePath, processingFileName, strDatetime);

                                        objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                   strInputFilePath, ReferenceId, strFileName, strDatetime);
                                        rowindex++;
                                        continue;
                                    }
                                    if (dr.Table.Columns.Contains("signature"))
                                    {
                                        if (!string.IsNullOrEmpty(Convert.ToString(dr["signature"])))
                                        {
                                            routeStopPutrequest = routeStopPutrequest + @"'signature': '" + dr["signature"] + "',";
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "RouteStopPut Error " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Signature Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            // objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);


                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                            objErrorResponse.error = "Signature value not found for this  record";
                                            objErrorResponse.status = "Error";
                                            objErrorResponse.code = "Signature Value Missing";
                                            objErrorResponse.reference = "For row  number -" + rowindex;
                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                            dsFailureResponse.Tables[0].TableName = "RouteStopPutFailure";
                                            //    objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                            //strInputFilePath, processingFileName, strDatetime);
                                            objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                 strInputFilePath, ReferenceId, strFileName, strDatetime);
                                            rowindex++;
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        strExecutionLogMessage = "RouteStopPut Error " + System.Environment.NewLine;
                                        strExecutionLogMessage += "Signature Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                        strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                        objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                        //objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                        ErrorResponse objErrorResponse = new ErrorResponse();
                                        objErrorResponse.error = "Signature column not found for this record";
                                        objErrorResponse.status = "Signature";
                                        objErrorResponse.code = "signature column Missing";
                                        objErrorResponse.reference = "For row  number -" + rowindex;
                                        string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                        DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                        dsFailureResponse.Tables[0].TableName = "RouteStopPutFailure";
                                        //strInputFilePath, processingFileName, strDatetime);

                                        objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                   strInputFilePath, ReferenceId, strFileName, strDatetime);
                                        rowindex++;
                                        continue;
                                    }



                                    if (dr.Table.Columns.Contains("customer_number"))
                                    {
                                        if (!string.IsNullOrEmpty(Convert.ToString(dr["customer_number"])))
                                        {
                                            routeStopPutrequest = routeStopPutrequest + @"'customer_number': " + dr["customer_number"] + ",";
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "RouteStopPut Error " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Customer Number Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            // objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);


                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                            objErrorResponse.error = "Customer Number value not found for this  record";
                                            objErrorResponse.status = "Error";
                                            objErrorResponse.code = "Customer Number Value Missing";
                                            objErrorResponse.reference = "For row  number -" + rowindex;
                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                            dsFailureResponse.Tables[0].TableName = "RouteStopPutFailure";
                                            //    objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                            //strInputFilePath, processingFileName, strDatetime);
                                            objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                 strInputFilePath, ReferenceId, strFileName, strDatetime);
                                            rowindex++;
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        strExecutionLogMessage = "RouteStopPut Error " + System.Environment.NewLine;
                                        strExecutionLogMessage += "Customer Number Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                        strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                        objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                        //objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                        ErrorResponse objErrorResponse = new ErrorResponse();
                                        objErrorResponse.error = "Customer Number column not found for this record";
                                        objErrorResponse.status = "Customer Number";
                                        objErrorResponse.code = "Customer Number column Missing";
                                        objErrorResponse.reference = "For row  number -" + rowindex;
                                        string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                        DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                        dsFailureResponse.Tables[0].TableName = "RouteStopPutFailure";
                                        //strInputFilePath, processingFileName, strDatetime);

                                        objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                   strInputFilePath, ReferenceId, strFileName, strDatetime);
                                        rowindex++;
                                        continue;
                                    }


                                    // objroute_stop.service_level = Convert.ToInt32(objDsServviceTypeResponse.DS.Tables[0].Rows[0]["service_type"]);

                                    routeStopPutrequest = @"{" + routeStopPutrequest + "}";
                                    string routeStopPutrequestObject = @"{'route_stop': " + routeStopPutrequest + "}";
                                    JObject jsonobj = JObject.Parse(routeStopPutrequestObject);
                                    string request = jsonobj.ToString();

                                    clsDatatrac objclsDatatrac = new clsDatatrac();

                                    GeneratedUniqueId = objclsDatatrac.GenerateUniqueNumber(Convert.ToInt32(dr["company_number"]), Convert.ToInt32(dr["unique_id"]));


                                    clsCommon.ReturnResponse objresponse = new clsCommon.ReturnResponse();
                                    // string request = JsonConvert.SerializeObject(routeStopPutrequest);
                                    objresponse = objclsRoute.CallDataTracRouteStopPutAPI(GeneratedUniqueId, routeStopPutrequestObject);
                                    // objresponse.ResponseVal = true;
                                    //objresponse.Reason = "{\"00100035009\":{\"room\":null,\"unique_id\":35009,\"c2_paperwork\":false,\"company_number_text\":\"EASTRN TIME ON CENTRAL SERVER\",\"company_number\":1,\"addl_charge_code11\":null,\"billing_override_amt\":null,\"addl_charge_occur1\":null,\"updated_time\":null,\"stop_sequence\":\"0010\",\"phone\":null,\"city\":\"Alpharetta\",\"created_by\":\"DX*\",\"signature_images\":[],\"pricing_zone\":null,\"signature_filename\":null,\"addl_charge_code10\":null,\"cod_check_no\":null,\"length\":null,\"expected_weight\":null,\"actual_settlement_amt\":null,\"actual_pieces\":null,\"updated_date\":null,\"schedule_stop_id\":null,\"photos_exist\":false,\"stop_type_text\":\"Delivery\",\"stop_type\":\"D\",\"return\":false,\"addl_charge_code6\":null,\"dispatch_zone\":null,\"upload_time\":null,\"actual_cod_amt\":null,\"location_accuracy\":null,\"progress\":[{\"status_time\":\"10:22:02\",\"status_text\":\"Entered in carrier's system\",\"status_date\":\"2021-08-04\"}],\"received_route\":null,\"override_settle_percent\":null,\"cod_amount\":null,\"addl_charge_code9\":null,\"eta_date\":null,\"cod_type_text\":\"None\",\"cod_type\":\"0\",\"addl_charge_occur3\":null,\"reference\":null,\"sent_to_phone\":false,\"addl_charge_occur12\":null,\"callback_required_text\":\"No\",\"callback_required\":\"N\",\"service_level_text\":\"1HR RUSH SERVICE\",\"service_level\":6,\"original_id\":null,\"width\":null,\"received_sequence\":null,\"transfer_to_sequence\":null,\"cases\":null,\"times_sent\":0,\"transfer_to_route\":null,\"zip_code\":null,\"settlement_override_amt\":null,\"driver_app_status_text\":\"\",\"driver_app_status\":\"0\",\"route_code_text\":\"NAPA\",\"route_code\":\"NAPA\",\"received_shift\":null,\"addl_charge_occur6\":null,\"addl_charge_occur11\":null,\"vehicle\":null,\"addl_charge_code5\":null,\"addl_charge_occur9\":null,\"eta\":null,\"departure_time\":null,\"combine_data\":null,\"actual_latitude\":null,\"posted_by\":null,\"insurance_value\":null,\"return_redel_id\":null,\"addl_charge_code1\":null,\"origin_code_text\":\"Added using API\",\"origin_code\":\"A\",\"ordered_by\":null,\"posted_date\":null,\"actual_billing_amt\":null,\"created_date\":\"2021-08-04\",\"latitude\":null,\"received_pieces\":null,\"addl_charge_code7\":null,\"totes\":null,\"asn_sent\":0,\"comments\":null,\"verification_id_type_text\":\"None\",\"verification_id_type\":\"0\",\"posted_time\":null,\"item_scans_required\":true,\"shift_id\":null,\"addon_billing_amt\":null,\"actual_delivery_date\":null,\"id\":\"00100035009\",\"actual_arrival_time\":null,\"signature_required\":true,\"longitude\":null,\"expected_pieces\":null,\"loaded_pieces\":null,\"alt_lookup\":null,\"customer_number_text\":\"Routing Customer\",\"customer_number\":4999,\"created_time\":\"10:22:02\",\"addl_charge_code8\":null,\"signature\":null,\"actual_depart_time\":null,\"bol_number\":null,\"actual_cod_type_text\":\"None\",\"actual_cod_type\":\"0\",\"invoice_number\":null,\"branch_id\":null,\"special_instructions2\":null,\"updated_by\":null,\"verification_id_details\":null,\"required_signature_type_text\":\"Any signature\",\"required_signature_type\":\"0\",\"addl_charge_occur7\":null,\"orig_order_number\":null,\"special_instructions1\":null,\"notes\":[],\"image_sign_req\":true,\"attention\":null,\"minutes_late\":0,\"late_notice_time\":null,\"received_unique_id\":null,\"exception_code\":null,\"addl_charge_code4\":null,\"addl_charge_occur4\":null,\"redelivery\":false,\"addl_charge_occur10\":null,\"upload_date\":null,\"special_instructions4\":null,\"address_name\":null,\"addl_charge_occur8\":null,\"address_point_customer\":null,\"received_branch\":null,\"items\":[],\"return_redelivery_date\":null,\"height\":null,\"actual_longitude\":null,\"service_time\":null,\"phone_ext\":null,\"addl_charge_occur2\":null,\"late_notice_date\":null,\"address\":\"123 Stop Address Street\",\"arrival_time\":null,\"posted_status\":false,\"route_date\":\"2021-08-03\",\"addl_charge_code12\":null,\"addl_charge_code3\":null,\"return_redelivery_flag_text\":\"None\",\"return_redelivery_flag\":\"N\",\"additional_instructions\":null,\"updated_by_scanner\":false,\"special_instructions3\":null,\"addl_charge_occur5\":null,\"address_point\":0,\"actual_weight\":null,\"received_company\":null,\"addl_charge_code2\":null,\"state\":\"GA\"}}";
                                    // objresponse.Reason = "{\"00204352124\": {\"posted_by\": null, \"addon_billing_amt\": null, \"minutes_late\": 0, \"insurance_value\": null, \"addl_charge_occur5\": null, \"actual_pieces\": null, \"actual_depart_time\": null, \"created_time\": \"08:43:34\", \"cod_amount\": null, \"special_instructions3\": null, \"width\": null, \"ordered_by\": null, \"addl_charge_code1\": null, \"signature_filename\": null, \"updated_date\": null, \"latitude\": null, \"signature\": null, \"received_branch\": null, \"late_notice_time\": null, \"route_code_text\": \"HDHOLD\", \"route_code\": \"HDHOLD\", \"phone_ext\": null, \"addl_charge_occur1\": null, \"received_sequence\": null, \"address_name\": \"TEST1\", \"address_point_customer\": null, \"actual_cod_amt\": null, \"signature_required\": true, \"stop_type_text\": \"Delivery\", \"stop_type\": \"D\", \"origin_code_text\": \"Added using API\", \"origin_code\": \"A\", \"invoice_number\": null, \"addl_charge_code11\": null, \"addl_charge_code12\": null, \"length\": null, \"vehicle\": null, \"item_scans_required\": true, \"updated_by_scanner\": false, \"addl_charge_code10\": null, \"unique_id\": 4352124, \"attention\": null, \"items\": [{\"item_number\": \"item1\", \"item_description\": \"first item\", \"reference\": \"1\", \"rma_route\": null, \"upload_time\": null, \"rma_stop_id\": 0, \"width\": null, \"redelivery\": false, \"received_pieces\": null, \"cod_amount\": null, \"height\": null, \"comments\": null, \"actual_pieces\": null, \"actual_cod_amount\": null, \"rma_number\": null, \"manually_updated\": 0, \"unique_id\": 4352124, \"cod_type_text\": \"None\", \"cod_type\": \"0\", \"barcodes_unique\": false, \"actual_cod_type_text\": \"None\", \"actual_cod_type\": \"0\", \"return_redel_seq\": 0, \"expected_pieces\": 3, \"signature\": null, \"exception_code\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"updated_date\": null, \"expected_weight\": 50, \"created_date\": \"2021-08-26\", \"rma_origin\": null, \"created_by\": \"DX*\", \"loaded_pieces\": null, \"return_redelivery_flag_text\": \"\", \"return_redelivery_flag\": null, \"original_id\": 0, \"container_id\": \"container1\", \"return\": false, \"length\": null, \"notes\": [], \"actual_weight\": null, \"updated_by\": null, \"photos_exist\": false, \"second_container_id\": null, \"return_redel_id\": 0, \"asn_sent\": 0, \"actual_departure_time\": null, \"updated_time\": null, \"return_redelivery_date\": null, \"actual_arrival_time\": null, \"item_sequence\": 1, \"pallet_number\": null, \"actual_date\": null, \"insurance_value\": null, \"created_time\": \"08:43:34\", \"upload_date\": null, \"scans\": [], \"id\": \"002043521240001\", \"truck_id\": 0}, {\"item_number\": \"item2\", \"item_description\": \"first item\", \"reference\": \"1\", \"rma_route\": null, \"upload_time\": null, \"rma_stop_id\": 0, \"width\": null, \"redelivery\": false, \"received_pieces\": null, \"cod_amount\": null, \"height\": null, \"comments\": null, \"actual_pieces\": null, \"actual_cod_amount\": null, \"rma_number\": null, \"manually_updated\": 0, \"unique_id\": 4352124, \"cod_type_text\": \"None\", \"cod_type\": \"0\", \"barcodes_unique\": false, \"actual_cod_type_text\": \"None\", \"actual_cod_type\": \"0\", \"return_redel_seq\": 0, \"expected_pieces\": 1, \"signature\": null, \"exception_code\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"updated_date\": null, \"expected_weight\": 150, \"created_date\": \"2021-08-26\", \"rma_origin\": null, \"created_by\": \"DX*\", \"loaded_pieces\": null, \"return_redelivery_flag_text\": \"\", \"return_redelivery_flag\": null, \"original_id\": 0, \"container_id\": \"container2\", \"return\": false, \"length\": null, \"notes\": [], \"actual_weight\": null, \"updated_by\": null, \"photos_exist\": false, \"second_container_id\": null, \"return_redel_id\": 0, \"asn_sent\": 0, \"actual_departure_time\": null, \"updated_time\": null, \"return_redelivery_date\": null, \"actual_arrival_time\": null, \"item_sequence\": 2, \"pallet_number\": null, \"actual_date\": null, \"insurance_value\": null, \"created_time\": \"08:43:34\", \"upload_date\": null, \"scans\": [], \"id\": \"002043521240002\", \"truck_id\": 0}, {\"item_number\": \"item3\", \"item_description\": \"first item\", \"reference\": \"1\", \"rma_route\": null, \"upload_time\": null, \"rma_stop_id\": 0, \"width\": null, \"redelivery\": false, \"received_pieces\": null, \"cod_amount\": null, \"height\": null, \"comments\": null, \"actual_pieces\": null, \"actual_cod_amount\": null, \"rma_number\": null, \"manually_updated\": 0, \"unique_id\": 4352124, \"cod_type_text\": \"None\", \"cod_type\": \"0\", \"barcodes_unique\": false, \"actual_cod_type_text\": \"None\", \"actual_cod_type\": \"0\", \"return_redel_seq\": 0, \"expected_pieces\": 1, \"signature\": null, \"exception_code\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"updated_date\": null, \"expected_weight\": 250, \"created_date\": \"2021-08-26\", \"rma_origin\": null, \"created_by\": \"DX*\", \"loaded_pieces\": null, \"return_redelivery_flag_text\": \"\", \"return_redelivery_flag\": null, \"original_id\": 0, \"container_id\": \"container3\", \"return\": false, \"length\": null, \"notes\": [], \"actual_weight\": null, \"updated_by\": null, \"photos_exist\": false, \"second_container_id\": null, \"return_redel_id\": 0, \"asn_sent\": 0, \"actual_departure_time\": null, \"updated_time\": null, \"return_redelivery_date\": null, \"actual_arrival_time\": null, \"item_sequence\": 3, \"pallet_number\": null, \"actual_date\": null, \"insurance_value\": null, \"created_time\": \"08:43:35\", \"upload_date\": null, \"scans\": [], \"id\": \"002043521240003\", \"truck_id\": 0}, {\"item_number\": \"item4\", \"item_description\": \"first item\", \"reference\": \"1\", \"rma_route\": null, \"upload_time\": null, \"rma_stop_id\": 0, \"width\": null, \"redelivery\": false, \"received_pieces\": null, \"cod_amount\": null, \"height\": null, \"comments\": null, \"actual_pieces\": null, \"actual_cod_amount\": null, \"rma_number\": null, \"manually_updated\": 0, \"unique_id\": 4352124, \"cod_type_text\": \"None\", \"cod_type\": \"0\", \"barcodes_unique\": false, \"actual_cod_type_text\": \"None\", \"actual_cod_type\": \"0\", \"return_redel_seq\": 0, \"expected_pieces\": 21, \"signature\": null, \"exception_code\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"updated_date\": null, \"expected_weight\": 350, \"created_date\": \"2021-08-26\", \"rma_origin\": null, \"created_by\": \"DX*\", \"loaded_pieces\": null, \"return_redelivery_flag_text\": \"\", \"return_redelivery_flag\": null, \"original_id\": 0, \"container_id\": \"container4\", \"return\": false, \"length\": null, \"notes\": [], \"actual_weight\": null, \"updated_by\": null, \"photos_exist\": false, \"second_container_id\": null, \"return_redel_id\": 0, \"asn_sent\": 0, \"actual_departure_time\": null, \"updated_time\": null, \"return_redelivery_date\": null, \"actual_arrival_time\": null, \"item_sequence\": 4, \"pallet_number\": null, \"actual_date\": null, \"insurance_value\": null, \"created_time\": \"08:43:36\", \"upload_date\": null, \"scans\": [], \"id\": \"002043521240004\", \"truck_id\": 0}], \"addl_charge_occur10\": null, \"verification_id_type_text\": \"None\", \"verification_id_type\": \"0\", \"addl_charge_occur7\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"posted_time\": null, \"c2_paperwork\": false, \"original_id\": null, \"progress\": [{\"status_time\": \"08:43:34\", \"status_date\": \"2021-08-26\", \"status_text\": \"Entered in carrier's system\"}], \"service_level_text\": \"Basic Delivery\", \"service_level\": 56, \"created_by\": \"DX*\", \"required_signature_type_text\": \"Any signature\", \"required_signature_type\": \"0\", \"special_instructions1\": null, \"actual_billing_amt\": null, \"branch_id_text\": \"JWL Baltimore, MD\", \"branch_id\": \"BWI\", \"actual_cod_type_text\": \"None\", \"actual_cod_type\": \"0\", \"pricing_zone\": null, \"state\": \"TX\", \"signature_images\": [], \"special_instructions4\": null, \"photos_exist\": false, \"height\": null, \"eta_date\": null, \"upload_date\": null, \"zip_code\": \"75034\", \"actual_latitude\": null, \"override_settle_percent\": null, \"notes\": [{\"entry_time\": \"08:43:34\", \"note_text\": \"** Expected pieces: 0 -> 3\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"item_sequence\": null, \"entry_date\": \"2021-08-26\", \"user_entered\": false, \"show_to_cust\": false, \"note_type_text\": \"Stop\", \"note_type\": \"0\", \"unique_id\": 4352124, \"id\": \"00204352124    0020210826084334DX* 24\", \"user_id\": \"DX*\"}, {\"entry_time\": \"08:43:34\", \"note_text\": \"** Expected weight:      0 ->      50\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"item_sequence\": null, \"entry_date\": \"2021-08-26\", \"user_entered\": false, \"show_to_cust\": false, \"note_type_text\": \"Stop\", \"note_type\": \"0\", \"unique_id\": 4352124, \"id\": \"00204352124    0020210826084334DX* 25\", \"user_id\": \"DX*\"}, {\"entry_time\": \"08:43:35\", \"note_text\": \"** Expected pieces: 3 -> 4\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"item_sequence\": null, \"entry_date\": \"2021-08-26\", \"user_entered\": false, \"show_to_cust\": false, \"note_type_text\": \"Stop\", \"note_type\": \"0\", \"unique_id\": 4352124, \"id\": \"00204352124    0020210826084335DX* 24\", \"user_id\": \"DX*\"}, {\"entry_time\": \"08:43:35\", \"note_text\": \"** Expected weight:     50 ->     200\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"item_sequence\": null, \"entry_date\": \"2021-08-26\", \"user_entered\": false, \"show_to_cust\": false, \"note_type_text\": \"Stop\", \"note_type\": \"0\", \"unique_id\": 4352124, \"id\": \"00204352124    0020210826084335DX* 25\", \"user_id\": \"DX*\"}, {\"entry_time\": \"08:43:36\", \"note_text\": \"** Expected pieces: 4 -> 5\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"item_sequence\": null, \"entry_date\": \"2021-08-26\", \"user_entered\": false, \"show_to_cust\": false, \"note_type_text\": \"Stop\", \"note_type\": \"0\", \"unique_id\": 4352124, \"id\": \"00204352124    0020210826084336DX* 24\", \"user_id\": \"DX*\"}, {\"entry_time\": \"08:43:36\", \"note_text\": \"** Expected weight:    200 ->     450\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"item_sequence\": null, \"entry_date\": \"2021-08-26\", \"user_entered\": false, \"show_to_cust\": false, \"note_type_text\": \"Stop\", \"note_type\": \"0\", \"unique_id\": 4352124, \"id\": \"00204352124    0020210826084336DX* 25\", \"user_id\": \"DX*\"}, {\"entry_time\": \"08:43:37\", \"note_text\": \"** Expected pieces: 5 -> 26\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"item_sequence\": null, \"entry_date\": \"2021-08-26\", \"user_entered\": false, \"show_to_cust\": false, \"note_type_text\": \"Stop\", \"note_type\": \"0\", \"unique_id\": 4352124, \"id\": \"00204352124    0020210826084337DX* 24\", \"user_id\": \"DX*\"}, {\"entry_time\": \"08:43:37\", \"note_text\": \"** Expected weight:    450 ->     800\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"item_sequence\": null, \"entry_date\": \"2021-08-26\", \"user_entered\": false, \"show_to_cust\": false, \"note_type_text\": \"Stop\", \"note_type\": \"0\", \"unique_id\": 4352124, \"id\": \"00204352124    0020210826084337DX* 25\", \"user_id\": \"DX*\"}], \"additional_instructions\": null, \"addl_charge_occur6\": null, \"driver_app_status_text\": \"\", \"driver_app_status\": \"0\", \"combine_data\": null, \"addl_charge_code2\": null, \"service_time\": null, \"city\": \"FRISCO\", \"room\": null, \"addl_charge_code7\": null, \"billing_override_amt\": null, \"totes\": null, \"sent_to_phone\": false, \"address\": \"1000 PARKWOOD BLVD\", \"posted_date\": null, \"phone\": \"111-111-1111\", \"late_notice_date\": null, \"received_route\": null, \"bol_number\": \"1\", \"asn_sent\": 0, \"addl_charge_occur3\": null, \"departure_time\": null, \"received_unique_id\": null, \"orig_order_number\": null, \"reference\": \"1\", \"comments\": null, \"updated_by\": null, \"customer_number_text\": \"HD - BWI 21229\", \"customer_number\": 516, \"addl_charge_code4\": null, \"addl_charge_code9\": null, \"location_accuracy\": null, \"verification_id_details\": null, \"cases\": null, \"actual_arrival_time\": null, \"received_company\": null, \"addl_charge_code5\": null, \"addl_charge_occur11\": null, \"addl_charge_code6\": null, \"actual_settlement_amt\": null, \"addl_charge_occur12\": null, \"cod_check_no\": null, \"updated_time\": null, \"expected_pieces\": 26, \"times_sent\": 0, \"addl_charge_occur9\": null, \"id\": \"00204352124\", \"route_date\": \"2021-08-31\", \"schedule_stop_id\": null, \"return\": false, \"addl_charge_occur4\": null, \"image_sign_req\": false, \"created_date\": \"2021-08-26\", \"longitude\": null, \"redelivery\": false, \"actual_weight\": null, \"cod_type_text\": \"None\", \"cod_type\": \"0\", \"eta\": null, \"transfer_to_sequence\": null, \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"alt_lookup\": null, \"addl_charge_occur8\": null, \"posted_status\": false, \"addl_charge_occur2\": null, \"transfer_to_route\": null, \"shift_id\": null, \"addl_charge_code8\": null, \"upload_time\": null, \"received_shift\": null, \"return_redel_id\": null, \"addl_charge_code3\": null, \"stop_sequence\": \"0010\", \"dispatch_zone\": null, \"expected_weight\": 800, \"special_instructions2\": null, \"actual_longitude\": null, \"settlement_override_amt\": null, \"actual_delivery_date\": null, \"arrival_time\": null, \"return_redelivery_flag_text\": \"None\", \"return_redelivery_flag\": \"N\", \"loaded_pieces\": null, \"exception_code\": null, \"address_point\": 0, \"return_redelivery_date\": null, \"received_pieces\": null, \"_utc_offset\": \"-04:00\"}}";
                                    if (objresponse.ResponseVal)
                                    {
                                        strExecutionLogMessage = "RouteStopPutAPI Success " + System.Environment.NewLine;
                                        strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                        strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                        objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                        // DataSet dsOrderResponse = objCommon.jsonToDataSet(objresponse.Reason, "RouteStopPostAPI");
                                        DataSet dsResponse = objCommon.jsonToDataSet(objresponse.Reason, "RouteStopPutAPI");
                                        var UniqueId = Convert.ToString(dsResponse.Tables[0].Rows[0]["id"]);
                                        try
                                        {
                                            if (dsResponse.Tables.Contains(UniqueId))
                                            {
                                                List<ResponseRouteStop> idList = new List<ResponseRouteStop>();
                                                for (int i = 0; i < dsResponse.Tables[0].Rows.Count; i++)
                                                {
                                                    DataTable dt = new DataTable();
                                                    dt = dsResponse.Tables[0];
                                                    ResponseRouteStop objIds = new ResponseRouteStop();

                                                    objIds.room = dt.Rows[i]["room"];
                                                    objIds.unique_id = dt.Rows[i]["unique_id"];

                                                    objIds.c2_paperwork = dt.Rows[i]["c2_paperwork"];
                                                    objIds.company_number_text = dt.Rows[i]["company_number_text"];
                                                    objIds.company_number = dt.Rows[i]["company_number"];
                                                    objIds.addl_charge_code11 = dt.Rows[i]["addl_charge_code11"];
                                                    objIds.billing_override_amt = dt.Rows[i]["billing_override_amt"];
                                                    objIds.addl_charge_occur1 = dt.Rows[i]["addl_charge_occur1"];
                                                    objIds.updated_time = dt.Rows[i]["updated_time"];
                                                    objIds.stop_sequence = dt.Rows[i]["stop_sequence"];

                                                    objIds.phone = dt.Rows[i]["phone"];
                                                    objIds.city = dt.Rows[i]["city"];
                                                    objIds.created_by = dt.Rows[i]["created_by"];
                                                    objIds.pricing_zone = dt.Rows[i]["pricing_zone"];
                                                    objIds.signature_filename = dt.Rows[i]["signature_filename"];
                                                    objIds.addl_charge_code10 = dt.Rows[i]["addl_charge_code10"];
                                                    objIds.cod_check_no = dt.Rows[i]["cod_check_no"];
                                                    objIds.length = dt.Rows[i]["length"];

                                                    objIds.expected_weight = dt.Rows[i]["expected_weight"];
                                                    objIds.actual_settlement_amt = dt.Rows[i]["actual_settlement_amt"];
                                                    objIds.actual_pieces = dt.Rows[i]["actual_pieces"];
                                                    objIds.updated_date = dt.Rows[i]["updated_date"];
                                                    objIds.schedule_stop_id = dt.Rows[i]["schedule_stop_id"];
                                                    objIds.photos_exist = dt.Rows[i]["photos_exist"];
                                                    objIds.stop_type_text = dt.Rows[i]["stop_type_text"];
                                                    objIds.stop_type = dt.Rows[i]["stop_type"];
                                                    objIds.@return = dt.Rows[i]["return"];
                                                    objIds.addl_charge_code6 = dt.Rows[i]["addl_charge_code6"];
                                                    objIds.dispatch_zone = dt.Rows[i]["dispatch_zone"];
                                                    objIds.upload_time = dt.Rows[i]["upload_time"];
                                                    objIds.actual_cod_amt = dt.Rows[i]["actual_cod_amt"];
                                                    objIds.location_accuracy = dt.Rows[i]["location_accuracy"];
                                                    objIds.received_route = dt.Rows[i]["received_route"];
                                                    objIds.override_settle_percent = dt.Rows[i]["override_settle_percent"];
                                                    objIds.cod_amount = dt.Rows[i]["cod_amount"];
                                                    objIds.addl_charge_code9 = dt.Rows[i]["addl_charge_code9"];
                                                    objIds.eta_date = dt.Rows[i]["eta_date"];
                                                    objIds.cod_type_text = dt.Rows[i]["cod_type_text"];
                                                    objIds.cod_type = dt.Rows[i]["cod_type"];
                                                    objIds.addl_charge_occur3 = dt.Rows[i]["addl_charge_occur3"];
                                                    objIds.reference = dt.Rows[i]["reference"];
                                                    objIds.sent_to_phone = dt.Rows[i]["sent_to_phone"];
                                                    objIds.addl_charge_occur12 = dt.Rows[i]["addl_charge_occur12"];
                                                    objIds.callback_required_text = dt.Rows[i]["callback_required_text"];
                                                    objIds.callback_required = dt.Rows[i]["callback_required"];
                                                    objIds.service_level_text = dt.Rows[i]["service_level_text"];
                                                    objIds.service_level = dt.Rows[i]["service_level"];
                                                    objIds.original_id = dt.Rows[i]["original_id"];
                                                    objIds.width = dt.Rows[i]["width"];
                                                    objIds.received_sequence = dt.Rows[i]["received_sequence"];
                                                    objIds.transfer_to_sequence = dt.Rows[i]["transfer_to_sequence"];
                                                    objIds.cases = dt.Rows[i]["cases"];
                                                    objIds.times_sent = dt.Rows[i]["times_sent"];
                                                    objIds.transfer_to_route = dt.Rows[i]["transfer_to_route"];
                                                    objIds.zip_code = dt.Rows[i]["zip_code"];
                                                    objIds.settlement_override_amt = dt.Rows[i]["settlement_override_amt"];
                                                    objIds.driver_app_status_text = dt.Rows[i]["driver_app_status_text"];
                                                    objIds.driver_app_status = dt.Rows[i]["driver_app_status"];
                                                    objIds.route_code_text = dt.Rows[i]["route_code_text"];
                                                    objIds.route_code = dt.Rows[i]["route_code"];
                                                    objIds.received_shift = dt.Rows[i]["received_shift"];
                                                    objIds.addl_charge_occur6 = dt.Rows[i]["addl_charge_occur6"];
                                                    objIds.addl_charge_occur11 = dt.Rows[i]["addl_charge_occur11"];
                                                    objIds.vehicle = dt.Rows[i]["vehicle"];
                                                    objIds.addl_charge_code5 = dt.Rows[i]["addl_charge_code5"];
                                                    objIds.addl_charge_occur9 = dt.Rows[i]["addl_charge_occur9"];

                                                    objIds.eta = dt.Rows[i]["eta"];
                                                    objIds.departure_time = dt.Rows[i]["departure_time"];
                                                    objIds.combine_data = dt.Rows[i]["combine_data"];
                                                    objIds.actual_latitude = dt.Rows[i]["actual_latitude"];
                                                    objIds.posted_by = dt.Rows[i]["posted_by"];
                                                    objIds.insurance_value = dt.Rows[i]["insurance_value"];
                                                    objIds.return_redel_id = dt.Rows[i]["return_redel_id"];
                                                    objIds.addl_charge_code1 = dt.Rows[i]["addl_charge_code1"];
                                                    objIds.origin_code_text = dt.Rows[i]["origin_code_text"];
                                                    objIds.origin_code = dt.Rows[i]["origin_code"];
                                                    objIds.ordered_by = dt.Rows[i]["ordered_by"];
                                                    objIds.posted_date = dt.Rows[i]["posted_date"];
                                                    objIds.actual_billing_amt = dt.Rows[i]["actual_billing_amt"];
                                                    objIds.created_date = dt.Rows[i]["created_date"];
                                                    objIds.latitude = dt.Rows[i]["latitude"];
                                                    objIds.received_pieces = dt.Rows[i]["received_pieces"];
                                                    objIds.addl_charge_code7 = dt.Rows[i]["addl_charge_code7"];
                                                    objIds.totes = dt.Rows[i]["totes"];
                                                    objIds.asn_sent = dt.Rows[i]["asn_sent"];
                                                    objIds.comments = dt.Rows[i]["comments"];
                                                    objIds.verification_id_type_text = dt.Rows[i]["verification_id_type_text"];
                                                    objIds.verification_id_type = dt.Rows[i]["verification_id_type"];
                                                    objIds.posted_time = dt.Rows[i]["posted_time"];
                                                    objIds.item_scans_required = dt.Rows[i]["item_scans_required"];
                                                    objIds.shift_id = dt.Rows[i]["shift_id"];
                                                    objIds.addon_billing_amt = dt.Rows[i]["addon_billing_amt"];
                                                    objIds.actual_delivery_date = dt.Rows[i]["actual_delivery_date"];
                                                    objIds.id = dt.Rows[i]["id"];
                                                    objIds.actual_arrival_time = dt.Rows[i]["actual_arrival_time"];
                                                    objIds.signature_required = dt.Rows[i]["signature_required"];
                                                    objIds.longitude = dt.Rows[i]["longitude"];
                                                    objIds.expected_pieces = dt.Rows[i]["expected_pieces"];
                                                    objIds.loaded_pieces = dt.Rows[i]["loaded_pieces"];
                                                    objIds.alt_lookup = dt.Rows[i]["alt_lookup"];
                                                    objIds.customer_number_text = dt.Rows[i]["customer_number_text"];
                                                    objIds.customer_number = dt.Rows[i]["customer_number"];
                                                    objIds.created_time = dt.Rows[i]["created_time"];
                                                    objIds.addl_charge_code8 = dt.Rows[i]["addl_charge_code8"];
                                                    objIds.signature = dt.Rows[i]["signature"];
                                                    objIds.actual_depart_time = dt.Rows[i]["actual_depart_time"];
                                                    objIds.bol_number = dt.Rows[i]["bol_number"];
                                                    objIds.actual_cod_type_text = dt.Rows[i]["actual_cod_type_text"];
                                                    objIds.actual_cod_type = dt.Rows[i]["actual_cod_type"];
                                                    objIds.invoice_number = dt.Rows[i]["invoice_number"];
                                                    objIds.branch_id = dt.Rows[i]["branch_id"];
                                                    objIds.special_instructions2 = dt.Rows[i]["special_instructions2"];
                                                    objIds.updated_by = dt.Rows[i]["updated_by"];
                                                    objIds.verification_id_details = dt.Rows[i]["verification_id_details"];
                                                    objIds.required_signature_type_text = dt.Rows[i]["required_signature_type_text"];
                                                    objIds.required_signature_type = dt.Rows[i]["required_signature_type"];
                                                    objIds.addl_charge_occur7 = dt.Rows[i]["addl_charge_occur7"];
                                                    objIds.orig_order_number = dt.Rows[i]["orig_order_number"];
                                                    objIds.special_instructions1 = dt.Rows[i]["special_instructions1"];
                                                    objIds.image_sign_req = dt.Rows[i]["image_sign_req"];
                                                    objIds.attention = dt.Rows[i]["attention"];
                                                    objIds.minutes_late = dt.Rows[i]["minutes_late"];
                                                    objIds.late_notice_time = dt.Rows[i]["late_notice_time"];
                                                    objIds.received_unique_id = dt.Rows[i]["received_unique_id"];
                                                    objIds.exception_code = dt.Rows[i]["exception_code"];
                                                    objIds.addl_charge_code4 = dt.Rows[i]["addl_charge_code4"];
                                                    objIds.addl_charge_occur4 = dt.Rows[i]["addl_charge_occur4"];
                                                    objIds.redelivery = dt.Rows[i]["redelivery"];
                                                    objIds.addl_charge_occur10 = dt.Rows[i]["addl_charge_occur10"];
                                                    objIds.upload_date = dt.Rows[i]["upload_date"];
                                                    objIds.special_instructions4 = dt.Rows[i]["special_instructions4"];
                                                    objIds.address_name = dt.Rows[i]["address_name"];
                                                    objIds.addl_charge_occur8 = dt.Rows[i]["addl_charge_occur8"];
                                                    objIds.address_point_customer = dt.Rows[i]["address_point_customer"];
                                                    objIds.received_branch = dt.Rows[i]["received_branch"];
                                                    objIds.return_redelivery_date = dt.Rows[i]["return_redelivery_date"];
                                                    objIds.height = dt.Rows[i]["height"];
                                                    objIds.actual_longitude = dt.Rows[i]["actual_longitude"];
                                                    objIds.service_time = dt.Rows[i]["service_time"];
                                                    objIds.phone_ext = dt.Rows[i]["phone_ext"];
                                                    objIds.addl_charge_occur2 = dt.Rows[i]["addl_charge_occur2"];
                                                    objIds.late_notice_date = dt.Rows[i]["late_notice_date"];
                                                    objIds.address = dt.Rows[i]["address"];
                                                    objIds.arrival_time = dt.Rows[i]["arrival_time"];
                                                    objIds.posted_status = dt.Rows[i]["posted_status"];
                                                    objIds.route_date = dt.Rows[i]["route_date"];
                                                    objIds.addl_charge_code12 = dt.Rows[i]["addl_charge_code12"];
                                                    objIds.addl_charge_code3 = dt.Rows[i]["addl_charge_code3"];
                                                    objIds.return_redelivery_flag_text = dt.Rows[i]["return_redelivery_flag_text"];
                                                    objIds.return_redelivery_flag = dt.Rows[i]["return_redelivery_flag"];
                                                    objIds.additional_instructions = dt.Rows[i]["additional_instructions"];
                                                    objIds.updated_by_scanner = dt.Rows[i]["updated_by_scanner"];
                                                    objIds.special_instructions3 = dt.Rows[i]["special_instructions3"];
                                                    objIds.addl_charge_occur5 = dt.Rows[i]["addl_charge_occur5"];
                                                    objIds.address_point = dt.Rows[i]["address_point"];
                                                    objIds.actual_weight = dt.Rows[i]["actual_weight"];
                                                    objIds.received_company = dt.Rows[i]["received_company"];
                                                    objIds.addl_charge_code2 = dt.Rows[i]["addl_charge_code2"];
                                                    objIds.state = dt.Rows[i]["state"];
                                                    // public object @return { get; set; }
                                                    idList.Add(objIds);
                                                }
                                                objCommon.SaveOutputDataToCsvFile(idList, "RouteStop-Put",
                                   strInputFilePath, ReferenceId, strFileName, strDatetime);
                                            }

                                            if (dsResponse.Tables.Contains("progress"))
                                            {

                                                List<RouteStopResponseProgress> progressList = new List<RouteStopResponseProgress>();
                                                for (int i = 0; i < dsResponse.Tables["progress"].Rows.Count; i++)
                                                {
                                                    RouteStopResponseProgress progress = new RouteStopResponseProgress();
                                                    DataTable dt = new DataTable();
                                                    dt = dsResponse.Tables["progress"];

                                                    progress.status_date = (dt.Rows[i]["status_date"]);
                                                    progress.status_text = (dt.Rows[i]["status_text"]);
                                                    progress.status_time = (dt.Rows[i]["status_time"]);
                                                    // progress.id = (dt.Rows[i]["id"]);
                                                    progressList.Add(progress);
                                                }

                                                objCommon.SaveOutputDataToCsvFile(progressList, "RouteStopPut-Progress",
                                                     strInputFilePath, UniqueId, strFileName, strDatetime);
                                            }

                                            //  public List<object> signature_images { get; set; }
                                            // public List<Progress> progress { get; set; }
                                            // public List<object> notes { get; set; }
                                            // public List<object> items { get; set; }

                                            if (dsResponse.Tables.Contains("notes"))
                                            {

                                                List<RouteStopResponseNote> noteList = new List<RouteStopResponseNote>();
                                                for (int i = 0; i < dsResponse.Tables["notes"].Rows.Count; i++)
                                                {
                                                    RouteStopResponseNote note = new RouteStopResponseNote();
                                                    DataTable dt = new DataTable();
                                                    dt = dsResponse.Tables["notes"];
                                                    note.entry_time = (dt.Rows[i]["entry_time"]);
                                                    note.note_text = (dt.Rows[i]["note_text"]);
                                                    note.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                    note.company_number = (dt.Rows[i]["company_number"]);
                                                    note.item_sequence = (dt.Rows[i]["item_sequence"]);
                                                    note.user_id = (dt.Rows[i]["user_id"]);
                                                    note.entry_date = (dt.Rows[i]["entry_date"]);
                                                    note.user_entered = (dt.Rows[i]["user_entered"]);
                                                    note.show_to_cust = (dt.Rows[i]["show_to_cust"]);
                                                    note.note_type_text = (dt.Rows[i]["note_type_text"]);
                                                    note.note_type = (dt.Rows[i]["note_type"]);
                                                    note.unique_id = (dt.Rows[i]["unique_id"]);
                                                    note.id = (dt.Rows[i]["id"]);
                                                    noteList.Add(note);
                                                }

                                                objCommon.SaveOutputDataToCsvFile(noteList, "RouteStopPut-Note",
                                                   strInputFilePath, UniqueId, strFileName, strDatetime);

                                            }

                                            if (dsResponse.Tables.Contains("items"))
                                            {

                                                List<RouteStopResponseItem> itemList = new List<RouteStopResponseItem>();
                                                for (int i = 0; i < dsResponse.Tables["items"].Rows.Count; i++)
                                                {
                                                    RouteStopResponseItem item = new RouteStopResponseItem();
                                                    DataTable dt = new DataTable();
                                                    dt = dsResponse.Tables["items"];

                                                    item.item_number = (dt.Rows[i]["item_number"]);
                                                    item.item_description = (dt.Rows[i]["item_description"]);
                                                    item.reference = (dt.Rows[i]["reference"]);
                                                    item.rma_route = (dt.Rows[i]["rma_route"]);
                                                    item.upload_time = (dt.Rows[i]["upload_time"]);
                                                    item.rma_stop_id = (dt.Rows[i]["rma_stop_id"]);
                                                    item.width = (dt.Rows[i]["width"]);
                                                    item.redelivery = (dt.Rows[i]["redelivery"]);
                                                    item.received_pieces = (dt.Rows[i]["received_pieces"]);
                                                    item.cod_amount = (dt.Rows[i]["cod_amount"]);
                                                    item.height = (dt.Rows[i]["height"]);
                                                    item.comments = (dt.Rows[i]["comments"]);
                                                    item.actual_pieces = (dt.Rows[i]["actual_pieces"]);
                                                    item.actual_cod_amount = (dt.Rows[i]["actual_cod_amount"]);
                                                    item.rma_number = (dt.Rows[i]["rma_number"]);
                                                    item.manually_updated = (dt.Rows[i]["manually_updated"]);
                                                    item.unique_id = (dt.Rows[i]["unique_id"]);
                                                    item.cod_type_text = (dt.Rows[i]["cod_type_text"]);
                                                    item.cod_type = (dt.Rows[i]["cod_type"]);
                                                    item.barcodes_unique = (dt.Rows[i]["barcodes_unique"]);
                                                    item.actual_cod_type = (dt.Rows[i]["actual_cod_type"]);
                                                    item.return_redel_seq = (dt.Rows[i]["return_redel_seq"]);
                                                    item.expected_pieces = (dt.Rows[i]["expected_pieces"]);
                                                    item.signature = (dt.Rows[i]["signature"]);
                                                    item.exception_code = (dt.Rows[i]["exception_code"]);
                                                    item.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                    item.company_number = (dt.Rows[i]["company_number"]);
                                                    item.updated_date = (dt.Rows[i]["updated_date"]);
                                                    item.expected_weight = (dt.Rows[i]["expected_weight"]);
                                                    item.created_date = (dt.Rows[i]["created_date"]);
                                                    item.rma_origin = (dt.Rows[i]["rma_origin"]);
                                                    item.created_by = (dt.Rows[i]["created_by"]);
                                                    item.loaded_pieces = (dt.Rows[i]["loaded_pieces"]);
                                                    item.return_redelivery_flag_text = (dt.Rows[i]["return_redelivery_flag_text"]);
                                                    item.return_redelivery_flag = (dt.Rows[i]["return_redelivery_flag"]);
                                                    item.original_id = (dt.Rows[i]["original_id"]);
                                                    item.container_id = (dt.Rows[i]["container_id"]);
                                                    item.@return = (dt.Rows[i]["return"]);
                                                    item.length = (dt.Rows[i]["length"]);
                                                    item.actual_weight = (dt.Rows[i]["actual_weight"]);
                                                    item.updated_by = (dt.Rows[i]["updated_by"]);
                                                    item.photos_exist = (dt.Rows[i]["photos_exist"]);
                                                    item.second_container_id = (dt.Rows[i]["second_container_id"]);
                                                    item.return_redel_id = (dt.Rows[i]["return_redel_id"]);
                                                    item.asn_sent = (dt.Rows[i]["asn_sent"]);
                                                    item.actual_departure_time = (dt.Rows[i]["actual_departure_time"]);
                                                    item.updated_time = (dt.Rows[i]["updated_time"]);
                                                    item.return_redelivery_date = (dt.Rows[i]["return_redelivery_date"]);
                                                    item.actual_arrival_time = (dt.Rows[i]["actual_arrival_time"]);
                                                    item.item_sequence = (dt.Rows[i]["item_sequence"]);
                                                    item.pallet_number = (dt.Rows[i]["pallet_number"]);
                                                    item.actual_date = (dt.Rows[i]["actual_date"]);
                                                    item.insurance_value = (dt.Rows[i]["insurance_value"]);
                                                    item.created_time = (dt.Rows[i]["created_time"]);
                                                    item.upload_date = (dt.Rows[i]["upload_date"]);
                                                    item.id = (dt.Rows[i]["id"]);
                                                    item.truck_id = (dt.Rows[i]["truck_id"]);

                                                    // public List<object> notes { get; set; }
                                                    // public List<object> scans { get; set; }
                                                    itemList.Add(item);
                                                }

                                                objCommon.SaveOutputDataToCsvFile(itemList, "RouteStopPut-Item",
                                                   strInputFilePath, UniqueId, strFileName, strDatetime);

                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            strExecutionLogMessage = "RouteStopPutFiles Exception -" + ex.Message + System.Environment.NewLine;
                                            strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                            strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For Reference -" + ReferenceId + System.Environment.NewLine;
                                            //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            objCommon.WriteErrorLog(ex, strExecutionLogMessage);

                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                            objErrorResponse.error = "Found exception while processing the record";
                                            objErrorResponse.status = "Error";
                                            objErrorResponse.code = "Excception while procesing the record.";
                                            objErrorResponse.reference = ReferenceId;
                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                            dsFailureResponse.Tables[0].TableName = "RouteStopPutFailure";
                                            objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                           strInputFilePath, ReferenceId, strFileName, strDatetime);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        strExecutionLogMessage = "RouteStopPutAPI Failed " + System.Environment.NewLine;
                                        strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                        strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                        objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                        DataSet dsFailureResponse = objCommon.jsonToDataSet(objresponse.Reason);
                                        dsFailureResponse.Tables[0].TableName = "RouteStopPutFailure";
                                        dsFailureResponse.Tables[0].Columns.Add("Reference", typeof(System.String));
                                        foreach (DataRow row in dsFailureResponse.Tables[0].Rows)
                                        {
                                            row["Reference"] = ReferenceId;
                                        }
                                        objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                    strInputFilePath, ReferenceId, strFileName, strDatetime);

                                    }


                                }
                                catch (Exception ex)
                                {
                                    strExecutionLogMessage = "ProcessUpdateRouteStopFiles Exception -" + ex.Message + System.Environment.NewLine;
                                    strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                    strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                    strExecutionLogMessage += "For Reference -" + ReferenceId + System.Environment.NewLine;
                                    //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                    objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                }
                                rowindex++;
                            }

                            objCommon.MoveOutputFilesToOutputLocation(strInputFilePath);
                        }
                        else
                        {
                            strExecutionLogMessage = "Template sheet data not found for the file " + strInputFilePath + @"\" + strFileName;
                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        strExecutionLogMessage = "ProcessUpdateRouteStopFiles Exception -" + ex.Message + System.Environment.NewLine;
                        strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                        strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                        strExecutionLogMessage += "For Reference -" + ReferenceId + System.Environment.NewLine;
                        // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                        objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                    }
                }

                strExecutionLogMessage = "Finished processing all the files for the location " + strInputFilePath;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
            }
            catch (Exception ex)
            {
                objCommon.WriteErrorLog(ex);
                throw new Exception("Error in ProcessUpdateRouteStopFiles -->" + ex.Message + ex.StackTrace);
            }
        }
        public static String StripUnicodeCharactersFromString(string inputValue)
        {
            return Encoding.ASCII.GetString(Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding(Encoding.ASCII.EncodingName, new EncoderReplacementFallback(String.Empty), new DecoderExceptionFallback()), Encoding.UTF8.GetBytes(inputValue)));
        }

        public static DataTable RemoveDuplicateRows(DataTable dTable, string colName)
        {
            Hashtable hTable = new Hashtable();
            ArrayList duplicateList = new ArrayList();

            //Add list of all the unique item value to hashtable, which stores combination of key, value pair.
            //And add duplicate item value in arraylist.
            foreach (DataRow drow in dTable.Rows)
            {
                if (hTable.Contains(drow[colName]))
                    duplicateList.Add(drow);
                else
                    hTable.Add(drow[colName], string.Empty);
            }

            //Removing a list of duplicate items from datatable.
            foreach (DataRow dRow in duplicateList)
                dTable.Rows.Remove(dRow);

            //Datatable which contains unique records will be return as output.
            return dTable;
        }
    }
}
