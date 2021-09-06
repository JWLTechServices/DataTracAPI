using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DatatracAPIOrder_OrderSettlement
{
    public static class Program
    {
        static void Main(string[] args)
        {

            RunService();
            //clsCommon objCommon = new clsCommon();
            //string AppName = objCommon.GetConfigValue("ApplicationName");
            //if (clsCommon.IsException)
            //{
            //    string strEmailSubject = "Got Exception while running " + AppName + " on " + DateTime.Now.ToString("yyyyMMdd");
            //    string strEmailBody = strEmailSubject + System.Environment.NewLine + "Requesting you to please go and check error log file for :" + DateTime.Now.ToString("yyyyMMdd");
            //    objCommon.SendExceptionMail(strEmailSubject, strEmailBody);
            //}
            //try
            //{
            //    string strInputFilePath = @"C:\DatatracAPIAutomation\RIC\Order\Add";
            //    string strExecutionLogMessage;
            //    string strExecutionLogFileLocation = @"C:\JWLJobOutputs\BillingAutomation\ExecutionLog";

            //    string strwokingfolder = strInputFilePath + @"\WorkingFolder";

            //    string[] files = Directory.GetFiles(strwokingfolder, "*.xlsx");
            //    Parallel.ForEach(files, (currentFile) =>
            //    {

            //        var fileName = Path.GetFileName(currentFile);
            //        int fileExtPos = fileName.LastIndexOf(".");
            //        if (fileExtPos >= 0)
            //            fileName = fileName.Substring(0, fileExtPos);
            //        //fileName = fileName + "_" + val + "_" + datetime + ".xlsx";

            //        //string filePath = strExecutionLogFileLocation + "_" + strDatetime + ".txt";
            //        string filePath = strExecutionLogFileLocation + @"\" + fileName + ".txt";
            //        strExecutionLogMessage = "file Name  : " + fileName + "." + System.Environment.NewLine;
            //        objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
            //        //if (!File.Exists(filePath))
            //        //    File.Create(filePath).Dispose();

            //        //using (StreamWriter writetext = new StreamWriter(filePath, true))
            //        //{
            //        //    writetext.WriteLine(strExecutionLogMessage);
            //        //}
            //        strExecutionLogMessage = "Second msg for each  file  : " + fileName + "." + System.Environment.NewLine;
            //        objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
            //        //using (StreamWriter writetext = new StreamWriter(filePath, true))
            //        //{
            //        //    writetext.WriteLine(strExecutionLogMessage);
            //        //}
            //    });
            //}
            //catch (Exception ex)
            //{
            //    objCommon.WriteErrorLog(ex, "Error in Main");
            //}

        }
        private static void RunService()
        {
            clsCommon objCommon = new clsCommon();
            string AppName = objCommon.GetConfigValue("ApplicationName");
            var msg = "Exception in RunService";
            try
            {
                // objCommon.WriteToFile("Service is started at " + DateTime.Now);
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
                                            // ProcessUpdateOrderFiles(OrderUpdatefilePath, Location);
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
                                break;
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
                                            //ProcessUpdateOrderSettlementFiles(OrderSettlememtUpdatefilePath, Location);
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
                                break;
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
                                            //  ProcessAddRouteStopFiles(OrderAddfilePath, Location);
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
                                break;
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
                // string strFileName;
                var strFileName = "";
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

                            int rowspertable = Convert.ToInt16(objCommon.GetConfigValue("DevideToProcessParallelly"));
                            List<DataTable> splitdt = SplitTable(dsExcel.Tables[0], rowspertable, strFileName);

                            //for (var i = 0; i < splitdt.Count; i++)
                            //{
                            //    clsExcelHelper.ExportDataToXLSX(splitdt[i], strFileName, i, strDatetime);
                            //}

                           
                            string strwokingfolder = strInputFilePath + @"\WorkingFolder";

                            string[] files = Directory.GetFiles(strwokingfolder, "*.xlsx");
                            //string newDir = strInputFilePath + @"\Workingfolder";
                            //Directory.CreateDirectory(newDir);

                            strExecutionLogMessage = " Parallelly Processing  Statred for the  file : " + strFileName + "." + System.Environment.NewLine;
                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);


                            Parallel.ForEach(splitdt, currentDatatable =>
                            {
                                var fileName = currentDatatable.TableName;
                                var processingFileName = currentDatatable.TableName;
                                strExecutionLogMessage = "Current Processing File is  : " + fileName + "." + System.Environment.NewLine;
                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                var datatable = currentDatatable;

                                foreach (DataRow dr in datatable.Rows)
                                {
                                    object value = dr["Company"];
                                    if (value == DBNull.Value)
                                        break;

                                    ReferenceId = Convert.ToString(dr["Customer Reference"]);
                                    strExecutionLogMessage = "Customer Reference is : " + ReferenceId + "." + System.Environment.NewLine;
                                    //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                    objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                    try
                                    {
                                        orderdetails objorderdetails = new orderdetails();
                                        order objOrder = new order();
                                        objOrder.company_number = Convert.ToInt32(dr["Company"]);
                                        objOrder.service_level = Convert.ToInt32(dr["Service Type"]);
                                        objOrder.customer_number = Convert.ToInt32(dr["Billing Customer Number"]);
                                        objOrder.reference = Convert.ToString(dr["Customer Reference"]);
                                        DateTime dtValue = Convert.ToDateTime(dr["Delivery Date"]);
                                        objOrder.pickup_requested_date = dtValue.ToString("yyyy-MM-dd");

                                        dtValue = Convert.ToDateTime(dr["Delivery Date"]);
                                        objOrder.pickup_actual_date = dtValue.ToString("yyyy-MM-dd");

                                        dtValue = Convert.ToDateTime(dr["Pickup actual arrival time"]);
                                        objOrder.pickup_actual_arr_time = dtValue.ToString("HH:mm");

                                        dtValue = Convert.ToDateTime(dr["Pickup actual depart time"]);
                                        objOrder.pickup_actual_dep_time = dtValue.ToString("HH:mm");

                                        dtValue = Convert.ToDateTime(dr["Pickup actual depart time"]);
                                        objOrder.pickup_actual_dep_time = dtValue.ToString("HH:mm");

                                        dtValue = Convert.ToDateTime(dr["Pickup no later than"]);
                                        objOrder.pickup_requested_dep_time = dtValue.ToString("HH:mm");
                                        objOrder.pickup_name = Convert.ToString(dr["Store Code"]);
                                        objOrder.pickup_address = Convert.ToString(dr["Pickup address"]);
                                        objOrder.pickup_city = Convert.ToString(dr["Pickup city"]);
                                        objOrder.pickup_state = Convert.ToString(dr["Pickup state/province"]);
                                        objOrder.pickup_zip = Convert.ToString(dr["Pickup zip/postal code"]);

                                        dtValue = Convert.ToDateTime(dr["Delivery Date"]);
                                        objOrder.deliver_requested_date = dtValue.ToString("yyyy-MM-dd");

                                        dtValue = Convert.ToDateTime(dr["Deliver no earlier than"]);
                                        objOrder.deliver_requested_arr_time = dtValue.ToString("HH:mm");

                                        dtValue = Convert.ToDateTime(dr["Deliver no later than"]);
                                        objOrder.deliver_requested_dep_time = dtValue.ToString("HH:mm");

                                        dtValue = Convert.ToDateTime(dr["Delivery Date"]);
                                        objOrder.deliver_actual_date = dtValue.ToString("yyyy-MM-dd");

                                        dtValue = Convert.ToDateTime(dr["Delivery actual arrive time"]);
                                        objOrder.deliver_actual_arr_time = dtValue.ToString("HH:mm");

                                        dtValue = Convert.ToDateTime(dr["Delivery actual depart time"]);
                                        objOrder.deliver_actual_dep_time = dtValue.ToString("HH:mm");

                                        objOrder.deliver_name = Convert.ToString(dr["Customer Name"]);
                                        objOrder.deliver_address = Convert.ToString(dr["Address"]);
                                        objOrder.deliver_city = Convert.ToString(dr["City"]);
                                        objOrder.deliver_state = Convert.ToString(dr["State"]);
                                        objOrder.deliver_zip = Convert.ToString(dr["Zip"]);

                                        objOrder.signature = Convert.ToString(dr["Delivery text signature"]);
                                        objOrder.rate_buck_amt1 = Convert.ToDouble(dr["Bill Rate"]);
                                        objOrder.rate_buck_amt3 = Convert.ToDouble(dr["Pieces ACC"]);
                                        objOrder.rate_buck_amt10 = Convert.ToDouble(dr["FSC"]);
                                        objOrder.number_of_pieces = Convert.ToInt32(dr["Pieces"]);
                                        objOrder.rate_miles = Convert.ToInt32(Convert.ToDouble(dr["Miles"]));
                                        string driver1 = null;
                                        if (!string.IsNullOrEmpty(Convert.ToString(dr["Correct Driver Number"])))
                                        {
                                            objOrder.driver1 = Convert.ToInt32(dr["Correct Driver Number"]);
                                            driver1 = Convert.ToString(dr["Correct Driver Number"]);
                                        }
                                        objOrder.ordered_by = Convert.ToString(dr["Requested by"]);
                                        objOrder.csr = Convert.ToString(dr["Entered by"]);


                                        objOrder.pick_del_trans_flag = Convert.ToString(dr["Pickup Delivery Transfer Flag"]);

                                        objOrder.pickup_signature = Convert.ToString(dr["Pickup text signature"]);
                                        objorderdetails.order = objOrder;
                                        clsDatatrac objclsDatatrac = new clsDatatrac();
                                        clsCommon.ReturnResponse objresponse = new clsCommon.ReturnResponse();
                                        string request = JsonConvert.SerializeObject(objorderdetails);
                                        objresponse = objclsDatatrac.CallDataTracOrderPostAPI(objorderdetails);
                                        //objresponse.ResponseVal = true;
                                        // objresponse.Reason = "{\"002018775030\": {\"verified_weight\": null, \"roundtrip_actual_latitude\": null, \"pickup_special_instructions4\": null, \"fuel_miles\": null, \"pickup_dispatch_zone\": null, \"pickup_zip\": \"23150\", \"pickup_actual_arr_time\": \"08:00\", \"cod_accept_company_check\": false, \"add_charge_occur9\": null, \"pickup_omw_latitude\": null, \"service_level_text\": \"Threshold Delivery\", \"service_level\": 57, \"exception_sign_required\": false, \"pickup_phone_ext\": null, \"roundtrip_actual_pieces\": null, \"bringg_send_sms\": false, \"az_equip2\": null, \"hist_inv_date\": null, \"date_order_entered\": \"2021-07-21\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"pickup_city\": \"SANDSTON\", \"pickup_phone\": null, \"pickup_sign_req\": true, \"deliver_route_sequence\": null, \"deliver_phone\": null, \"deliver_omw_longitude\": null, \"roundtrip_actual_longitude\": null, \"page_number\": 1, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"add_charge_code9\": null, \"pickup_eta_time\": null, \"record_type\": 0, \"add_charge_occur11\": null, \"push_partner_order_id\": null, \"deliver_country\": null, \"customer_name\": \"MXD/RYDER\", \"bol_number\": null, \"pickup_latitude\": 37.53250820, \"add_charge_code4\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"pu_arrive_notification_sent\": false, \"distribution_shift_id\": null, \"pickup_special_instr_long\": null, \"id\": \"002018775030\", \"callback_to\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"ordered_by\": \"RYDER\", \"add_charge_code12\": null, \"pickup_route_seq\": null, \"deliver_city\": \"HANOVER\", \"add_charge_occur5\": null, \"edi_acknowledgement_required\": false, \"rescheduled_ctrl_number\": null, \"driver2\": null, \"deliver_room\": null, \"deliver_actual_arr_time\": \"08:00\", \"fuel_price_zone\": null, \"add_charge_amt9\": null, \"add_charge_amt4\": null, \"delivery_address_point_number_text\": \"PAMELA YORK\", \"delivery_address_point_number\": 24254, \"deliver_actual_longitude\": null, \"add_charge_amt2\": null, \"additional_drivers\": false, \"pickup_pricing_zone\": 1, \"hazmat\": false, \"pickup_address\": \"540 EASTPARK CT\", \"pickup_route_code\": null, \"callback_userid\": null, \"pickup_point_customer\": 31025, \"rate_buck_amt1\": 52.00, \"add_charge_amt8\": null, \"callback_time\": null, \"csr\": \"DX*\", \"roundtrip_actual_depart_time\": null, \"customers_etrac_partner_id\": \"96609250\", \"manual_notepad\": false, \"add_charge_code8\": null, \"bringg_order_id\": null, \"deliver_omw_latitude\": null, \"pickup_longitude\": -77.33035820, \"etrac_number\": null, \"distribution_unique_id\": 0, \"vehicle_type\": null, \"roundtrip_actual_arrival_time\": null, \"delivery_longitude\": -77.28769600, \"pu_actual_location_accuracy\": null, \"deliver_actual_date\": \"2021-07-14\", \"exception_timestamp\": null, \"deliver_zip\": \"23069\", \"roundtrip_wait_time\": null, \"add_charge_occur8\": null, \"dl_arrive_notification_sent\": false, \"pickup_special_instructions1\": null, \"ordered_by_phone_number\": null, \"deliver_requested_arr_time\": \"08:00\", \"rate_miles\": null, \"holiday_groups\": null, \"pickup_email_notification_sent\": false, \"add_charge_code3\": null, \"dispatch_id\": null, \"add_charge_occur10\": null, \"dispatch_time\": null, \"deliver_wait_time\": null, \"invoice_period_end_date\": null, \"add_charge_occur12\": null, \"fuel_plan\": null, \"return_svc_level\": null, \"pickup_actual_date\": \"2021-07-14\", \"send_new_order_alert\": false, \"pickup_room\": null, \"rate_buck_amt8\": null, \"add_charge_amt10\": null, \"insurance_amount\": null, \"add_charge_amt3\": null, \"add_charge_amt6\": null, \"pickup_special_instructions3\": null, \"pickup_requested_date\": \"2021-07-14\", \"roundtrip_sign_req\": false, \"actual_miles\": null, \"pickup_address_point_number_text\": \"1STOPBEDROOMS\", \"pickup_address_point_number\": 19845, \"deliver_actual_latitude\": null, \"deliver_phone_ext\": null, \"deliver_route_code\": null, \"add_charge_code10\": null, \"delivery_airport_code\": null, \"reference\": null, \"photos_exist\": false, \"master_airway_bill_number\": null, \"control_number\": 1877503, \"cod_text\": \"No\", \"cod\": \"N\", \"rate_buck_amt11\": null, \"cod_amount\": null, \"pickup_omw_timestamp\": null, \"signature_images\": [], \"deliver_special_instructions1\": null, \"quote_amount\": null, \"total_pages\": 1, \"rate_buck_amt4\": null, \"line_items\": [], \"delivery_latitude\": 37.80844290, \"progress\": [{\"status_time\": \"15:57:00\", \"status_date\": \"2021-07-21\", \"status_text\": \"Entered in carrier's system\"}, {\"status_time\": \"08:30:00\", \"status_date\": \"2021-07-14\", \"status_text\": \"Picked up\"}, {\"status_time\": \"08:15:00\", \"status_date\": \"2021-07-14\", \"status_text\": \"Delivered\"}], \"add_charge_code1\": null, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"deliver_special_instr_long\": null, \"deliver_address\": \"205 HARDWOOD LN\", \"add_charge_occur4\": null, \"deliver_eta_date\": null, \"pickup_actual_dep_time\": \"08:30\", \"deliver_requested_dep_time\": \"17:00\", \"add_charge_code7\": null, \"deliver_actual_dep_time\": \"08:15\", \"cod_accept_cashiers_check\": false, \"bringg_last_loc_sent\": null, \"az_equip3\": null, \"driver1_text\": \"WALT DARBY\", \"driver1\": 3215, \"pickup_actual_latitude\": null, \"add_charge_occur2\": null, \"order_automatically_quoted\": false, \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"hours\": \"15\", \"frequent_caller_id\": null, \"rate_buck_amt6\": null, \"rate_chart_used\": 0, \"deliver_actual_pieces\": null, \"add_charge_code5\": null, \"pickup_omw_longitude\": null, \"notes\": [], \"delivery_point_customer\": 31025, \"add_charge_occur7\": null, \"rate_buck_amt5\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"add_charge_code11\": null, \"pickup_name\": \"1STOPBEDROOMS\", \"callback_date\": null, \"add_charge_code2\": null, \"house_airway_bill_number\": null, \"deliver_name\": \"PAMELA YORK\", \"number_of_pieces\": 1, \"deliver_eta_time\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"rate_special_instructions\": null, \"add_charge_occur3\": null, \"pickup_eta_date\": null, \"deliver_special_instructions4\": null, \"custom_special_instr_long\": null, \"deliver_special_instructions2\": null, \"pickup_signature\": \"SOF\", \"az_equip1\": null, \"add_charge_amt12\": null, \"settlements\": [{\"vendor_invoice_number\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"voucher_date\": null, \"voucher_amount\": null, \"fuel_price_source\": null, \"settlement_bucket6_pct\": null, \"pre_book_percentage\": true, \"settlement_bucket4_pct\": null, \"settlement_period_end_date\": null, \"charge5\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"settlement_bucket5_pct\": null, \"voucher_number\": null, \"charge6\": null, \"settlement_bucket2_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"settlement_pct\": 100.00, \"vendor_employee_numer\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"driver_number_text\": \"WALT DARBY\", \"driver_number\": 3215, \"fuel_plan\": null, \"agents_etrac_partner_id\": null, \"charge2\": null, \"charge3\": null, \"settlement_bucket1_pct\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"id\": \"002018775030D1\", \"record_type\": 0, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"pay_chart_used\": null, \"fuel_price_zone\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"charge1\": null, \"control_number\": 1877503, \"adjustment_type\": null, \"order_date\": \"2021-07-14\", \"time_last_updated\": \"14:57\", \"agent_etrac_transaction_number\": null, \"date_last_updated\": \"2021-07-21\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"settlement_bucket3_pct\": null}], \"calc_add_on_chgs\": false, \"original_schedule_number\": null, \"blocks\": null, \"del_actual_location_accuracy\": null, \"zone_set_used\": 1, \"pickup_country\": null, \"pickup_state\": \"VA\", \"add_charge_amt7\": null, \"email_addresses\": null, \"add_charge_occur1\": null, \"pickup_wait_time\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"distribution_branch_id\": null, \"rate_buck_amt9\": null, \"add_charge_amt1\": null, \"pickup_requested_dep_time\": \"09:00\", \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"deliver_state\": \"VA\", \"deliver_dispatch_zone\": null, \"image_sign_req\": false, \"add_charge_code6\": null, \"deliver_requested_date\": \"2021-07-14\", \"add_charge_amt5\": null, \"time_order_entered\": \"15:57\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"pickup_attention\": null, \"rate_buck_amt7\": null, \"add_charge_occur6\": null, \"fuel_price_source\": null, \"pickup_airport_code\": null, \"rate_buck_amt2\": null, \"rate_buck_amt3\": null, \"deliver_omw_timestamp\": null, \"exception_code\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"weight\": null, \"signature_required\": true, \"rate_buck_amt10\": null, \"hist_inv_number\": 0, \"deliver_pricing_zone\": 1, \"pickup_actual_longitude\": null, \"push_services\": null, \"add_charge_amt11\": null, \"rt_actual_location_accuracy\": null, \"roundtrip_actual_date\": null, \"pickup_requested_arr_time\": null, \"deliver_attention\": null, \"deliver_special_instructions3\": null, \"pickup_actual_pieces\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"roundtrip_signature\": null, \"po_number\": null, \"signature\": \"SOF\", \"pickup_special_instructions2\": null, \"original_ctrl_number\": null, \"previous_ctrl_number\": null}}";
                                        // objresponse.Reason = "{\"002018724440\": {\"verified_weight\": null, \"roundtrip_actual_latitude\": null, \"pickup_special_instructions4\": null, \"fuel_miles\": null, \"pickup_dispatch_zone\": null, \"pickup_zip\": \"23150\", \"pickup_actual_arr_time\": \"08:00\", \"cod_accept_company_check\": false, \"add_charge_occur9\": null, \"pickup_omw_latitude\": null, \"service_level_text\": \"Room of Choice\", \"service_level\": 55, \"exception_sign_required\": false, \"pickup_phone_ext\": null, \"roundtrip_actual_pieces\": null, \"bringg_send_sms\": false, \"az_equip2\": null, \"hist_inv_date\": null, \"date_order_entered\": \"2021-07-08\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"pickup_city\": \"SANDSTON\", \"pickup_phone\": null, \"pickup_sign_req\": true, \"deliver_route_sequence\": null, \"deliver_phone\": null, \"deliver_omw_longitude\": null, \"roundtrip_actual_longitude\": null, \"page_number\": 1, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"add_charge_code9\": null, \"pickup_eta_time\": null, \"record_type\": 0, \"add_charge_occur11\": null, \"push_partner_order_id\": null, \"deliver_country\": null, \"customer_name\": \"MXD/RYDER\", \"bol_number\": null, \"pickup_latitude\": 37.53250820, \"add_charge_code4\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"pu_arrive_notification_sent\": false, \"distribution_shift_id\": null, \"pickup_special_instr_long\": null, \"id\": \"002018724440\", \"callback_to\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"ordered_by\": \"RYDER\", \"add_charge_code12\": null, \"pickup_route_seq\": null, \"deliver_city\": \"CHESAPEAKE\", \"add_charge_occur5\": null, \"edi_acknowledgement_required\": false, \"rescheduled_ctrl_number\": null, \"driver2\": null, \"deliver_room\": null, \"deliver_actual_arr_time\": \"08:00\", \"fuel_price_zone\": null, \"add_charge_amt9\": null, \"add_charge_amt4\": null, \"delivery_address_point_number_text\": \"ANNE BAILEY\", \"delivery_address_point_number\": 26312, \"deliver_actual_longitude\": null, \"add_charge_amt2\": null, \"additional_drivers\": false, \"pickup_pricing_zone\": 1, \"hazmat\": false, \"pickup_address\": \"540 EASTPARK CT\", \"pickup_route_code\": null, \"callback_userid\": null, \"pickup_point_customer\": 31025, \"rate_buck_amt1\": 57.00, \"add_charge_amt8\": null, \"callback_time\": null, \"csr\": \"DX*\", \"roundtrip_actual_depart_time\": null, \"customers_etrac_partner_id\": \"96609250\", \"manual_notepad\": false, \"add_charge_code8\": null, \"bringg_order_id\": null, \"deliver_omw_latitude\": null, \"pickup_longitude\": -77.33035820, \"etrac_number\": null, \"distribution_unique_id\": 0, \"vehicle_type\": null, \"roundtrip_actual_arrival_time\": null, \"delivery_longitude\": -76.34760620, \"pu_actual_location_accuracy\": null, \"deliver_actual_date\": \"2021-06-28\", \"exception_timestamp\": null, \"deliver_zip\": \"23323\", \"roundtrip_wait_time\": null, \"add_charge_occur8\": null, \"dl_arrive_notification_sent\": false, \"pickup_special_instructions1\": null, \"ordered_by_phone_number\": null, \"deliver_requested_arr_time\": \"08:00\", \"rate_miles\": null, \"holiday_groups\": null, \"pickup_email_notification_sent\": false, \"add_charge_code3\": null, \"dispatch_id\": null, \"add_charge_occur10\": null, \"dispatch_time\": null, \"deliver_wait_time\": null, \"invoice_period_end_date\": null, \"add_charge_occur12\": null, \"fuel_plan\": null, \"return_svc_level\": null, \"pickup_actual_date\": \"2021-06-28\", \"send_new_order_alert\": false, \"pickup_room\": null, \"rate_buck_amt8\": null, \"add_charge_amt10\": null, \"insurance_amount\": null, \"add_charge_amt3\": null, \"add_charge_amt6\": null, \"pickup_special_instructions3\": null, \"pickup_requested_date\": \"2021-06-28\", \"roundtrip_sign_req\": false, \"actual_miles\": null, \"pickup_address_point_number_text\": \"BIG LOTS\", \"pickup_address_point_number\": 19864, \"deliver_actual_latitude\": null, \"deliver_phone_ext\": null, \"deliver_route_code\": null, \"add_charge_code10\": null, \"delivery_airport_code\": null, \"reference_text\": \"2125095801\", \"reference\": \"2125095801\", \"photos_exist\": false, \"master_airway_bill_number\": null, \"control_number\": 1872444, \"cod_text\": \"No\", \"cod\": \"N\", \"rate_buck_amt11\": null, \"cod_amount\": null, \"pickup_omw_timestamp\": null, \"signature_images\": [], \"deliver_special_instructions1\": null, \"quote_amount\": null, \"total_pages\": 1, \"rate_buck_amt4\": null, \"line_items\": [], \"delivery_latitude\": 36.78396970, \"progress\": [{\"status_time\": \"06:02:00\", \"status_date\": \"2021-07-08\", \"status_text\": \"Entered in carrier's system\"}, {\"status_time\": \"08:30:00\", \"status_date\": \"2021-06-28\", \"status_text\": \"Picked up\"}, {\"status_time\": \"08:15:00\", \"status_date\": \"2021-06-28\", \"status_text\": \"Delivered\"}], \"add_charge_code1\": null, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"deliver_special_instr_long\": null, \"deliver_address\": \"2920 AARON DR\", \"add_charge_occur4\": null, \"deliver_eta_date\": null, \"pickup_actual_dep_time\": \"08:30\", \"deliver_requested_dep_time\": \"17:00\", \"add_charge_code7\": null, \"deliver_actual_dep_time\": \"08:15\", \"cod_accept_cashiers_check\": false, \"bringg_last_loc_sent\": null, \"az_equip3\": null, \"driver1_text\": \"LAVERT KENDALL MORRIS\", \"driver1\": 3001, \"pickup_actual_latitude\": null, \"add_charge_occur2\": null, \"order_automatically_quoted\": false, \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"hours\": \"15\", \"frequent_caller_id\": null, \"rate_buck_amt6\": null, \"rate_chart_used\": 0, \"deliver_actual_pieces\": null, \"add_charge_code5\": null, \"pickup_omw_longitude\": null, \"notes\": [], \"delivery_point_customer\": 31025, \"add_charge_occur7\": null, \"rate_buck_amt5\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"add_charge_code11\": null, \"pickup_name\": \"BIG LOTS\", \"callback_date\": null, \"add_charge_code2\": null, \"house_airway_bill_number\": null, \"deliver_name\": \"ANNE BAILEY\", \"number_of_pieces\": 3, \"deliver_eta_time\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"rate_special_instructions\": null, \"add_charge_occur3\": null, \"pickup_eta_date\": null, \"deliver_special_instructions4\": null, \"custom_special_instr_long\": null, \"deliver_special_instructions2\": null, \"pickup_signature\": \"SOF\", \"az_equip1\": null, \"add_charge_amt12\": null, \"settlements\": [{\"vendor_invoice_number\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"voucher_date\": null, \"voucher_amount\": null, \"fuel_price_source\": null, \"settlement_bucket6_pct\": null, \"pre_book_percentage\": true, \"settlement_bucket4_pct\": null, \"settlement_period_end_date\": null, \"charge5\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"settlement_bucket5_pct\": null, \"voucher_number\": null, \"charge6\": null, \"settlement_bucket2_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"settlement_pct\": 100.00, \"vendor_employee_numer\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"driver_number_text\": \"LAVERT KENDALL MORRIS\", \"driver_number\": 3001, \"fuel_plan\": null, \"agents_etrac_partner_id\": null, \"charge2\": null, \"charge3\": null, \"settlement_bucket1_pct\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"id\": \"002018724440D1\", \"record_type\": 0, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"pay_chart_used\": null, \"fuel_price_zone\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"charge1\": null, \"control_number\": 1872444, \"adjustment_type\": null, \"order_date\": \"2021-06-28\", \"time_last_updated\": \"05:02\", \"agent_etrac_transaction_number\": null, \"date_last_updated\": \"2021-07-08\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"settlement_bucket3_pct\": null}], \"calc_add_on_chgs\": false, \"original_schedule_number\": null, \"blocks\": null, \"del_actual_location_accuracy\": null, \"zone_set_used\": 1, \"pickup_country\": null, \"pickup_state\": \"VA\", \"add_charge_amt7\": null, \"email_addresses\": null, \"add_charge_occur1\": null, \"pickup_wait_time\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"distribution_branch_id\": null, \"rate_buck_amt9\": null, \"add_charge_amt1\": null, \"pickup_requested_dep_time\": \"09:00\", \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"deliver_state\": \"VA\", \"deliver_dispatch_zone\": null, \"image_sign_req\": false, \"add_charge_code6\": null, \"deliver_requested_date\": \"2021-06-28\", \"add_charge_amt5\": null, \"time_order_entered\": \"06:02\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"pickup_attention\": null, \"rate_buck_amt7\": null, \"add_charge_occur6\": null, \"fuel_price_source\": null, \"pickup_airport_code\": null, \"rate_buck_amt2\": null, \"rate_buck_amt3\": null, \"deliver_omw_timestamp\": null, \"exception_code\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"weight\": null, \"signature_required\": true, \"rate_buck_amt10\": 2.34, \"hist_inv_number\": 0, \"deliver_pricing_zone\": 1, \"pickup_actual_longitude\": null, \"push_services\": null, \"add_charge_amt11\": null, \"rt_actual_location_accuracy\": null, \"roundtrip_actual_date\": null, \"pickup_requested_arr_time\": \"07:00\", \"deliver_attention\": null, \"deliver_special_instructions3\": null, \"pickup_actual_pieces\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"roundtrip_signature\": null, \"po_number\": null, \"signature\": \"SOF\", \"pickup_special_instructions2\": null, \"original_ctrl_number\": null, \"previous_ctrl_number\": null}}";
                                        // objresponse.Reason = "{\"002018724450\": {\"roundtrip_actual_date\": null, \"notes\": [], \"pickup_phone_ext\": null, \"holiday_groups\": null, \"deliver_eta_time\": null, \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"add_charge_occur4\": null, \"deliver_state\": \"VA\", \"quote_amount\": null, \"cod_text\": \"No\", \"cod\": \"N\", \"additional_drivers\": false, \"rescheduled_ctrl_number\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"pickup_actual_pieces\": null, \"record_type\": 0, \"pickup_special_instr_long\": null, \"pickup_special_instructions3\": null, \"exception_timestamp\": null, \"deliver_actual_arr_time\": \"08:00\", \"house_airway_bill_number\": null, \"deliver_pricing_zone\": 1, \"total_pages\": 1, \"add_charge_occur11\": null, \"deliver_omw_latitude\": null, \"callback_userid\": null, \"rate_buck_amt1\": 57.00, \"pickup_point_customer\": 31025, \"pickup_eta_time\": null, \"add_charge_occur8\": null, \"invoice_period_end_date\": null, \"pickup_special_instructions1\": null, \"rate_buck_amt2\": null, \"pickup_special_instructions4\": null, \"manual_notepad\": false, \"edi_acknowledgement_required\": false, \"pickup_name\": \"BIG LOTS\", \"ordered_by_phone_number\": null, \"add_charge_amt12\": null, \"delivery_point_customer\": 31025, \"deliver_actual_dep_time\": \"08:15\", \"email_addresses\": null, \"pickup_address\": \"540 EASTPARK CT\", \"driver2\": null, \"signature_images\": [], \"rate_buck_amt11\": null, \"delivery_latitude\": 37.48366600, \"pickup_attention\": null, \"date_order_entered\": \"2021-07-08\", \"vehicle_type\": null, \"add_charge_amt9\": null, \"pickup_phone\": null, \"rate_miles\": null, \"customers_etrac_partner_id\": \"96609250\", \"order_type_text\": \"One way\", \"order_type\": \"O\", \"dl_arrive_notification_sent\": false, \"add_charge_code3\": null, \"etrac_number\": null, \"pickup_requested_arr_time\": \"07:00\", \"rate_buck_amt3\": null, \"pickup_actual_dep_time\": \"08:30\", \"line_items\": [], \"pickup_sign_req\": true, \"add_charge_code10\": null, \"deliver_city\": \"LANEXA\", \"fuel_plan\": null, \"add_charge_amt10\": null, \"roundtrip_actual_depart_time\": null, \"control_number\": 1872445, \"pickup_dispatch_zone\": null, \"send_new_order_alert\": false, \"settlements\": [{\"settlement_bucket4_pct\": null, \"charge1\": null, \"date_last_updated\": \"2021-07-08\", \"fuel_price_zone\": null, \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"charge4\": null, \"settlement_period_end_date\": null, \"charge5\": null, \"time_last_updated\": \"05:06\", \"charge6\": null, \"driver_number_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver_number\": 3208, \"control_number\": 1872445, \"settlement_bucket2_pct\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"voucher_date\": null, \"agent_etrac_transaction_number\": null, \"settlement_bucket5_pct\": null, \"record_type\": 0, \"voucher_number\": null, \"voucher_amount\": null, \"pay_chart_used\": null, \"settlement_pct\": 100.00, \"vendor_invoice_number\": null, \"settlement_bucket3_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"pre_book_percentage\": true, \"charge3\": null, \"settlement_bucket6_pct\": null, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"adjustment_type\": null, \"id\": \"002018724450D1\", \"agents_etrac_partner_id\": null, \"fuel_plan\": null, \"fuel_price_source\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"vendor_employee_numer\": null, \"settlement_bucket1_pct\": null, \"order_date\": \"2021-06-28\", \"charge2\": null}], \"deliver_actual_latitude\": null, \"fuel_price_zone\": null, \"verified_weight\": null, \"deliver_requested_dep_time\": \"17:00\", \"pickup_airport_code\": null, \"dispatch_time\": null, \"deliver_attention\": null, \"time_order_entered\": \"06:06\", \"rate_buck_amt4\": null, \"roundtrip_wait_time\": null, \"add_charge_amt2\": null, \"az_equip3\": null, \"progress\": [{\"status_date\": \"2021-07-08\", \"status_text\": \"Entered in carrier's system\", \"status_time\": \"06:06:00\"}, {\"status_date\": \"2021-06-28\", \"status_text\": \"Picked up\", \"status_time\": \"08:30:00\"}, {\"status_date\": \"2021-06-28\", \"status_text\": \"Delivered\", \"status_time\": \"08:15:00\"}], \"page_number\": 1, \"roundtrip_sign_req\": false, \"add_charge_amt1\": null, \"add_charge_code8\": null, \"weight\": null, \"rate_buck_amt6\": null, \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"bringg_send_sms\": false, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"custom_special_instr_long\": null, \"deliver_requested_arr_time\": \"08:00\", \"service_level_text\": \"Room of Choice\", \"service_level\": 55, \"az_equip1\": null, \"add_charge_code4\": null, \"bringg_order_id\": null, \"delivery_address_point_number_text\": \"JOSEPH FESSMAN\", \"delivery_address_point_number\": 26313, \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"deliver_special_instructions1\": null, \"pickup_wait_time\": null, \"add_charge_occur5\": null, \"push_partner_order_id\": null, \"deliver_route_sequence\": null, \"pickup_country\": null, \"pickup_state\": \"VA\", \"original_schedule_number\": null, \"frequent_caller_id\": null, \"distribution_unique_id\": 0, \"fuel_miles\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"rate_buck_amt5\": null, \"exception_sign_required\": false, \"pickup_route_code\": null, \"deliver_dispatch_zone\": null, \"delivery_longitude\": -76.90426400, \"pickup_pricing_zone\": 1, \"zone_set_used\": 1, \"deliver_special_instructions2\": null, \"add_charge_amt3\": null, \"deliver_phone\": null, \"pickup_email_notification_sent\": false, \"add_charge_occur12\": null, \"reference_text\": \"2125617401\", \"reference\": \"2125617401\", \"deliver_requested_date\": \"2021-06-28\", \"deliver_actual_longitude\": null, \"image_sign_req\": false, \"pickup_eta_date\": null, \"deliver_phone_ext\": null, \"pickup_omw_longitude\": null, \"original_ctrl_number\": null, \"pickup_special_instructions2\": null, \"order_automatically_quoted\": false, \"bol_number\": null, \"rate_buck_amt10\": 2.34, \"callback_time\": null, \"hazmat\": false, \"distribution_shift_id\": null, \"pickup_latitude\": 37.53250820, \"ordered_by\": \"RYDER\", \"insurance_amount\": null, \"cod_accept_cashiers_check\": false, \"add_charge_amt4\": null, \"add_charge_code7\": null, \"deliver_actual_pieces\": null, \"deliver_address\": \"15400 STAGE RD\", \"cod_accept_company_check\": false, \"signature\": \"SOF\", \"previous_ctrl_number\": null, \"deliver_zip\": \"23089\", \"deliver_special_instructions3\": null, \"rate_buck_amt7\": null, \"hist_inv_number\": 0, \"callback_date\": null, \"deliver_special_instr_long\": null, \"po_number\": null, \"pickup_actual_arr_time\": \"08:00\", \"pickup_requested_date\": \"2021-06-28\", \"number_of_pieces\": 2, \"dispatch_id\": null, \"photos_exist\": false, \"pickup_actual_latitude\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"id\": \"002018724450\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"del_actual_location_accuracy\": null, \"add_charge_occur7\": null, \"add_charge_occur9\": null, \"roundtrip_actual_latitude\": null, \"add_charge_occur6\": null, \"pickup_actual_longitude\": null, \"pickup_omw_timestamp\": null, \"bringg_last_loc_sent\": null, \"add_charge_code5\": null, \"deliver_country\": null, \"master_airway_bill_number\": null, \"pickup_route_seq\": null, \"roundtrip_signature\": null, \"calc_add_on_chgs\": false, \"deliver_actual_date\": \"2021-06-28\", \"cod_amount\": null, \"add_charge_code12\": null, \"rt_actual_location_accuracy\": null, \"rate_chart_used\": 0, \"pickup_longitude\": -77.33035820, \"pickup_signature\": \"SOF\", \"add_charge_amt5\": null, \"pu_arrive_notification_sent\": false, \"pickup_actual_date\": \"2021-06-28\", \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"push_services\": null, \"deliver_eta_date\": null, \"driver1_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver1\": 3208, \"deliver_omw_longitude\": null, \"deliver_wait_time\": null, \"pickup_room\": null, \"deliver_special_instructions4\": null, \"add_charge_amt7\": null, \"az_equip2\": null, \"hours\": \"15\", \"add_charge_code2\": null, \"exception_code\": null, \"roundtrip_actual_pieces\": null, \"rate_special_instructions\": null, \"roundtrip_actual_arrival_time\": null, \"add_charge_occur1\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"delivery_airport_code\": null, \"distribution_branch_id\": null, \"hist_inv_date\": null, \"add_charge_code1\": null, \"pickup_requested_dep_time\": \"09:00\", \"deliver_route_code\": null, \"roundtrip_actual_longitude\": null, \"pickup_city\": \"SANDSTON\", \"rate_buck_amt8\": null, \"pickup_omw_latitude\": null, \"deliver_omw_timestamp\": null, \"rate_buck_amt9\": null, \"deliver_room\": null, \"add_charge_code6\": null, \"add_charge_occur3\": null, \"blocks\": null, \"add_charge_code9\": null, \"actual_miles\": null, \"add_charge_occur10\": null, \"add_charge_code11\": null, \"pickup_address_point_number_text\": \"BIG LOTS\", \"pickup_address_point_number\": 19864, \"customer_name\": \"MXD/RYDER\", \"pu_actual_location_accuracy\": null, \"deliver_name\": \"JOSEPH FESSMAN\", \"add_charge_amt6\": null, \"signature_required\": true, \"csr\": \"DX*\", \"add_charge_amt8\": null, \"callback_to\": null, \"fuel_price_source\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"pickup_zip\": \"23150\", \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"return_svc_level\": null, \"add_charge_amt11\": null, \"add_charge_occur2\": null}}";
                                        //  objresponse.Reason = "{\"error\": \"Unable to perform API call object with RecordID:WS_OPORDR-9990111870 - The record may have been deleted.\", \"status\": \"error\", \"code\": \"U.RecordNotFound\"}";

                                        if (objresponse.ResponseVal)
                                        {
                                            strExecutionLogMessage = "OrderPostAPI Success " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                            strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                            //using (StreamWriter writer = new StreamWriter(filePath, true))
                                            //{
                                            //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                            //}

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
                                                        objIds.verified_weight = dt.Rows[i]["verified_weight"];
                                                        objIds.roundtrip_actual_latitude = dt.Rows[i]["roundtrip_actual_latitude"];
                                                        objIds.pickup_special_instructions4 = dt.Rows[i]["pickup_special_instructions4"];
                                                        objIds.fuel_miles = dt.Rows[i]["fuel_miles"];
                                                        objIds.pickup_dispatch_zone = dt.Rows[i]["pickup_dispatch_zone"];

                                                        objIds.pickup_zip = dt.Rows[i]["pickup_zip"];
                                                        objIds.pickup_actual_arr_time = dt.Rows[i]["pickup_actual_arr_time"];
                                                        objIds.cod_accept_company_check = dt.Rows[i]["cod_accept_company_check"];
                                                        objIds.add_charge_occur9 = dt.Rows[i]["add_charge_occur9"];
                                                        objIds.pickup_omw_latitude = dt.Rows[i]["pickup_omw_latitude"];
                                                        objIds.service_level_text = dt.Rows[i]["service_level_text"];
                                                        objIds.service_level = dt.Rows[i]["service_level"];
                                                        objIds.exception_sign_required = dt.Rows[i]["exception_sign_required"];
                                                        objIds.pickup_phone_ext = dt.Rows[i]["pickup_phone_ext"];
                                                        objIds.roundtrip_actual_pieces = dt.Rows[i]["roundtrip_actual_pieces"];
                                                        objIds.bringg_send_sms = dt.Rows[i]["bringg_send_sms"];
                                                        objIds.az_equip2 = dt.Rows[i]["az_equip2"];

                                                        objIds.hist_inv_date = dt.Rows[i]["hist_inv_date"];
                                                        objIds.date_order_entered = dt.Rows[i]["date_order_entered"];
                                                        objIds.powerpage_status_text = dt.Rows[i]["powerpage_status_text"];
                                                        objIds.powerpage_status = dt.Rows[i]["powerpage_status"];
                                                        objIds.pickup_city = dt.Rows[i]["pickup_city"];
                                                        objIds.pickup_phone = dt.Rows[i]["pickup_phone"];
                                                        objIds.pickup_sign_req = dt.Rows[i]["pickup_sign_req"];

                                                        objIds.deliver_phone = dt.Rows[i]["deliver_phone"];
                                                        objIds.deliver_omw_longitude = dt.Rows[i]["deliver_omw_longitude"];
                                                        objIds.roundtrip_actual_longitude = dt.Rows[i]["roundtrip_actual_longitude"];
                                                        objIds.page_number = dt.Rows[i]["page_number"];
                                                        objIds.order_type_text = dt.Rows[i]["order_type_text"];
                                                        objIds.order_type = dt.Rows[i]["order_type"];
                                                        objIds.add_charge_code9 = dt.Rows[i]["add_charge_code9"];
                                                        objIds.pickup_eta_time = dt.Rows[i]["pickup_eta_time"];

                                                        objIds.record_type = dt.Rows[i]["record_type"];
                                                        objIds.add_charge_occur11 = dt.Rows[i]["add_charge_occur11"];
                                                        objIds.push_partner_order_id = dt.Rows[i]["push_partner_order_id"];
                                                        objIds.deliver_country = dt.Rows[i]["deliver_country"];
                                                        objIds.customer_name = dt.Rows[i]["customer_name"];
                                                        objIds.bol_number = dt.Rows[i]["bol_number"];
                                                        objIds.pickup_latitude = dt.Rows[i]["pickup_latitude"];
                                                        objIds.add_charge_code4 = dt.Rows[i]["add_charge_code4"];

                                                        objIds.exception_order_action_text = dt.Rows[i]["exception_order_action_text"];
                                                        objIds.exception_order_action = dt.Rows[i]["exception_order_action"];
                                                        objIds.pu_arrive_notification_sent = dt.Rows[i]["pu_arrive_notification_sent"];
                                                        objIds.distribution_shift_id = dt.Rows[i]["distribution_shift_id"];
                                                        objIds.pickup_special_instr_long = dt.Rows[i]["pickup_special_instr_long"];
                                                        objIds.id = dt.Rows[i]["id"];
                                                        objIds.callback_to = dt.Rows[i]["callback_to"];
                                                        objIds.customer_number_text = dt.Rows[i]["customer_number_text"];

                                                        objIds.customer_number = dt.Rows[i]["customer_number"];
                                                        objIds.ordered_by = dt.Rows[i]["ordered_by"];
                                                        objIds.add_charge_code12 = dt.Rows[i]["add_charge_code12"];
                                                        objIds.pickup_route_seq = dt.Rows[i]["pickup_route_seq"];
                                                        objIds.deliver_city = dt.Rows[i]["deliver_city"];

                                                        objIds.add_charge_occur5 = dt.Rows[i]["add_charge_occur5"];
                                                        objIds.edi_acknowledgement_required = dt.Rows[i]["edi_acknowledgement_required"];
                                                        objIds.rescheduled_ctrl_number = dt.Rows[i]["rescheduled_ctrl_number"];
                                                        objIds.driver2 = dt.Rows[i]["driver2"];
                                                        objIds.deliver_room = dt.Rows[i]["deliver_room"];

                                                        objIds.deliver_actual_arr_time = dt.Rows[i]["deliver_actual_arr_time"];
                                                        objIds.fuel_price_zone = dt.Rows[i]["fuel_price_zone"];
                                                        objIds.add_charge_amt9 = dt.Rows[i]["add_charge_amt9"];
                                                        objIds.add_charge_amt4 = dt.Rows[i]["add_charge_amt4"];
                                                        objIds.delivery_address_point_number_text = dt.Rows[i]["delivery_address_point_number_text"];
                                                        objIds.delivery_address_point_number = dt.Rows[i]["delivery_address_point_number"];

                                                        objIds.deliver_actual_longitude = dt.Rows[i]["deliver_actual_longitude"];
                                                        objIds.add_charge_amt2 = dt.Rows[i]["add_charge_amt2"];
                                                        objIds.additional_drivers = dt.Rows[i]["additional_drivers"];
                                                        objIds.pickup_pricing_zone = dt.Rows[i]["pickup_pricing_zone"];
                                                        objIds.hazmat = dt.Rows[i]["hazmat"];
                                                        objIds.pickup_address = dt.Rows[i]["pickup_address"];
                                                        objIds.pickup_route_code = dt.Rows[i]["pickup_route_code"];
                                                        objIds.callback_userid = dt.Rows[i]["callback_userid"];
                                                        objIds.pickup_point_customer = dt.Rows[i]["pickup_point_customer"];

                                                        objIds.rate_buck_amt1 = dt.Rows[i]["rate_buck_amt1"];
                                                        objIds.add_charge_amt8 = dt.Rows[i]["add_charge_amt8"];
                                                        objIds.callback_time = dt.Rows[i]["callback_time"];
                                                        objIds.csr = dt.Rows[i]["csr"];
                                                        objIds.roundtrip_actual_depart_time = dt.Rows[i]["roundtrip_actual_depart_time"];
                                                        objIds.customers_etrac_partner_id = dt.Rows[i]["customers_etrac_partner_id"];
                                                        objIds.manual_notepad = dt.Rows[i]["manual_notepad"];
                                                        objIds.add_charge_code8 = dt.Rows[i]["add_charge_code8"];
                                                        objIds.bringg_order_id = dt.Rows[i]["bringg_order_id"];
                                                        objIds.deliver_omw_latitude = dt.Rows[i]["deliver_omw_latitude"];
                                                        objIds.pickup_longitude = dt.Rows[i]["pickup_longitude"];
                                                        objIds.etrac_number = dt.Rows[i]["etrac_number"];

                                                        objIds.distribution_unique_id = dt.Rows[i]["distribution_unique_id"];
                                                        objIds.vehicle_type = dt.Rows[i]["vehicle_type"];
                                                        objIds.roundtrip_actual_arrival_time = dt.Rows[i]["roundtrip_actual_arrival_time"];
                                                        objIds.delivery_longitude = dt.Rows[i]["delivery_longitude"];
                                                        objIds.pu_actual_location_accuracy = dt.Rows[i]["pu_actual_location_accuracy"];
                                                        objIds.deliver_actual_date = dt.Rows[i]["deliver_actual_date"];
                                                        objIds.exception_timestamp = dt.Rows[i]["exception_timestamp"];
                                                        objIds.deliver_zip = dt.Rows[i]["deliver_zip"];
                                                        objIds.roundtrip_wait_time = dt.Rows[i]["roundtrip_wait_time"];
                                                        objIds.add_charge_occur8 = dt.Rows[i]["add_charge_occur8"];
                                                        objIds.dl_arrive_notification_sent = dt.Rows[i]["dl_arrive_notification_sent"];
                                                        objIds.pickup_special_instructions1 = dt.Rows[i]["pickup_special_instructions1"];
                                                        objIds.ordered_by_phone_number = dt.Rows[i]["ordered_by_phone_number"];
                                                        objIds.deliver_requested_arr_time = dt.Rows[i]["deliver_requested_arr_time"];

                                                        objIds.rate_miles = dt.Rows[i]["rate_miles"];
                                                        objIds.holiday_groups = dt.Rows[i]["holiday_groups"];
                                                        objIds.pickup_email_notification_sent = dt.Rows[i]["pickup_email_notification_sent"];
                                                        objIds.add_charge_code3 = dt.Rows[i]["add_charge_code3"];
                                                        objIds.dispatch_id = dt.Rows[i]["dispatch_id"];
                                                        objIds.add_charge_occur10 = dt.Rows[i]["add_charge_occur10"];
                                                        objIds.dispatch_time = dt.Rows[i]["dispatch_time"];
                                                        objIds.deliver_wait_time = dt.Rows[i]["deliver_wait_time"];
                                                        objIds.invoice_period_end_date = dt.Rows[i]["invoice_period_end_date"];
                                                        objIds.add_charge_occur12 = dt.Rows[i]["add_charge_occur12"];

                                                        objIds.fuel_plan = dt.Rows[i]["fuel_plan"];
                                                        objIds.return_svc_level = dt.Rows[i]["return_svc_level"];
                                                        objIds.pickup_actual_date = dt.Rows[i]["pickup_actual_date"];
                                                        objIds.send_new_order_alert = dt.Rows[i]["send_new_order_alert"];
                                                        objIds.pickup_room = dt.Rows[i]["pickup_room"];
                                                        objIds.rate_buck_amt8 = dt.Rows[i]["rate_buck_amt8"];
                                                        objIds.add_charge_amt10 = dt.Rows[i]["add_charge_amt10"];
                                                        objIds.insurance_amount = dt.Rows[i]["insurance_amount"];
                                                        objIds.add_charge_amt3 = dt.Rows[i]["add_charge_amt3"];
                                                        objIds.add_charge_amt6 = dt.Rows[i]["add_charge_amt6"];
                                                        objIds.pickup_special_instructions3 = dt.Rows[i]["pickup_special_instructions3"];
                                                        objIds.pickup_requested_date = dt.Rows[i]["pickup_requested_date"];
                                                        objIds.roundtrip_sign_req = dt.Rows[i]["roundtrip_sign_req"];
                                                        objIds.actual_miles = dt.Rows[i]["actual_miles"];
                                                        objIds.pickup_address_point_number_text = dt.Rows[i]["pickup_address_point_number_text"];
                                                        objIds.pickup_address_point_number = dt.Rows[i]["pickup_address_point_number"];
                                                        objIds.deliver_actual_latitude = dt.Rows[i]["deliver_actual_latitude"];
                                                        objIds.deliver_phone_ext = dt.Rows[i]["deliver_phone_ext"];
                                                        objIds.deliver_route_code = dt.Rows[i]["deliver_route_code"];
                                                        objIds.add_charge_code10 = dt.Rows[i]["add_charge_code10"];
                                                        objIds.delivery_airport_code = dt.Rows[i]["delivery_airport_code"];

                                                        objIds.reference_text = dt.Rows[i]["reference_text"];
                                                        objIds.reference = dt.Rows[i]["reference"];
                                                        objIds.photos_exist = dt.Rows[i]["photos_exist"];
                                                        objIds.master_airway_bill_number = dt.Rows[i]["master_airway_bill_number"];
                                                        objIds.control_number = dt.Rows[i]["control_number"];
                                                        objIds.cod_text = dt.Rows[i]["cod_text"];
                                                        objIds.cod = dt.Rows[i]["cod"];
                                                        objIds.rate_buck_amt11 = dt.Rows[i]["rate_buck_amt11"];
                                                        objIds.pickup_omw_timestamp = dt.Rows[i]["pickup_omw_timestamp"];
                                                        objIds.deliver_special_instructions1 = dt.Rows[i]["deliver_special_instructions1"];
                                                        objIds.quote_amount = dt.Rows[i]["quote_amount"];
                                                        objIds.total_pages = dt.Rows[i]["total_pages"];
                                                        objIds.rate_buck_amt4 = dt.Rows[i]["rate_buck_amt4"];
                                                        objIds.delivery_latitude = dt.Rows[i]["delivery_latitude"];
                                                        objIds.add_charge_code1 = dt.Rows[i]["add_charge_code1"];


                                                        objIds.order_timeliness_text = dt.Rows[i]["order_timeliness_text"];
                                                        objIds.order_timeliness = dt.Rows[i]["order_timeliness"];
                                                        objIds.deliver_special_instr_long = dt.Rows[i]["deliver_special_instr_long"];
                                                        objIds.deliver_address = dt.Rows[i]["deliver_address"];
                                                        objIds.add_charge_occur4 = dt.Rows[i]["add_charge_occur4"];
                                                        objIds.deliver_eta_date = dt.Rows[i]["deliver_eta_date"];
                                                        objIds.pickup_actual_dep_time = dt.Rows[i]["pickup_actual_dep_time"];
                                                        objIds.deliver_requested_dep_time = dt.Rows[i]["deliver_requested_dep_time"];
                                                        objIds.deliver_actual_dep_time = dt.Rows[i]["deliver_actual_dep_time"];

                                                        objIds.bringg_last_loc_sent = dt.Rows[i]["bringg_last_loc_sent"];
                                                        objIds.az_equip3 = dt.Rows[i]["az_equip3"];
                                                        objIds.driver1_text = dt.Rows[i]["driver1_text"];
                                                        objIds.driver1 = dt.Rows[i]["driver1"];
                                                        objIds.pickup_actual_latitude = dt.Rows[i]["pickup_actual_latitude"];
                                                        objIds.add_charge_occur2 = dt.Rows[i]["add_charge_occur2"];
                                                        objIds.order_automatically_quoted = dt.Rows[i]["order_automatically_quoted"];
                                                        objIds.callback_required = dt.Rows[i]["callback_required_text"];
                                                        objIds.frequent_caller_id = dt.Rows[i]["frequent_caller_id"];
                                                        objIds.rate_buck_amt6 = dt.Rows[i]["rate_buck_amt6"];
                                                        objIds.rate_chart_used = dt.Rows[i]["rate_chart_used"];
                                                        objIds.deliver_actual_pieces = dt.Rows[i]["deliver_actual_pieces"];
                                                        objIds.add_charge_code5 = dt.Rows[i]["add_charge_code5"];
                                                        objIds.pickup_omw_longitude = dt.Rows[i]["pickup_omw_longitude"];
                                                        objIds.delivery_point_customer = dt.Rows[i]["delivery_point_customer"];
                                                        objIds.add_charge_occur7 = dt.Rows[i]["add_charge_occur7"];
                                                        objIds.rate_buck_amt5 = dt.Rows[i]["rate_buck_amt5"];
                                                        objIds.fuel_update_freq_text = dt.Rows[i]["fuel_update_freq_text"];
                                                        objIds.fuel_update_freq = dt.Rows[i]["fuel_update_freq"];
                                                        objIds.add_charge_code11 = dt.Rows[i]["add_charge_code11"];
                                                        objIds.pickup_name = dt.Rows[i]["pickup_name"];
                                                        objIds.callback_date = dt.Rows[i]["callback_date"];
                                                        objIds.add_charge_code2 = dt.Rows[i]["add_charge_code2"];
                                                        objIds.house_airway_bill_number = dt.Rows[i]["house_airway_bill_number"];
                                                        objIds.deliver_name = dt.Rows[i]["deliver_name"];
                                                        objIds.number_of_pieces = dt.Rows[i]["number_of_pieces"];
                                                        objIds.deliver_eta_time = dt.Rows[i]["deliver_eta_time"];
                                                        objIds.origin_code_text = dt.Rows[i]["origin_code_text"];
                                                        objIds.origin_code = dt.Rows[i]["origin_code"];
                                                        objIds.rate_special_instructions = dt.Rows[i]["rate_special_instructions"];
                                                        objIds.add_charge_occur3 = dt.Rows[i]["add_charge_occur3"];
                                                        objIds.pickup_eta_date = dt.Rows[i]["pickup_eta_date"];
                                                        objIds.deliver_special_instructions4 = dt.Rows[i]["deliver_special_instructions4"];
                                                        objIds.custom_special_instr_long = dt.Rows[i]["custom_special_instr_long"];
                                                        objIds.deliver_special_instructions2 = dt.Rows[i]["deliver_special_instructions2"];
                                                        objIds.pickup_signature = dt.Rows[i]["pickup_signature"];
                                                        objIds.az_equip1 = dt.Rows[i]["az_equip1"];
                                                        objIds.add_charge_amt12 = dt.Rows[i]["add_charge_amt12"];
                                                        objIds.calc_add_on_chgs = dt.Rows[i]["calc_add_on_chgs"];
                                                        objIds.original_schedule_number = dt.Rows[i]["original_schedule_number"];
                                                        objIds.blocks = dt.Rows[i]["blocks"];
                                                        objIds.del_actual_location_accuracy = dt.Rows[i]["del_actual_location_accuracy"];
                                                        objIds.zone_set_used = dt.Rows[i]["zone_set_used"];

                                                        objIds.pickup_country = dt.Rows[i]["pickup_country"];
                                                        objIds.pickup_state = dt.Rows[i]["pickup_state"];
                                                        objIds.add_charge_amt7 = dt.Rows[i]["add_charge_amt7"];
                                                        objIds.email_addresses = dt.Rows[i]["email_addresses"];
                                                        objIds.add_charge_occur1 = dt.Rows[i]["add_charge_occur1"];
                                                        objIds.pickup_wait_time = dt.Rows[i]["pickup_wait_time"];
                                                        objIds.company_number_text = dt.Rows[i]["company_number_text"];
                                                        objIds.company_number = dt.Rows[i]["company_number"];
                                                        objIds.distribution_branch_id = dt.Rows[i]["distribution_branch_id"];
                                                        objIds.rate_buck_amt9 = dt.Rows[i]["rate_buck_amt9"];
                                                        objIds.add_charge_amt1 = dt.Rows[i]["add_charge_amt1"];
                                                        objIds.pickup_requested_dep_time = dt.Rows[i]["pickup_requested_dep_time"];
                                                        objIds.customer_type_text = dt.Rows[i]["customer_type_text"];
                                                        objIds.customer_type = dt.Rows[i]["customer_type"];
                                                        objIds.deliver_state = dt.Rows[i]["deliver_state"];
                                                        objIds.deliver_dispatch_zone = dt.Rows[i]["deliver_dispatch_zone"];
                                                        objIds.image_sign_req = dt.Rows[i]["image_sign_req"];
                                                        objIds.add_charge_code6 = dt.Rows[i]["add_charge_code6"];
                                                        objIds.deliver_requested_date = dt.Rows[i]["deliver_requested_date"];
                                                        objIds.add_charge_amt5 = dt.Rows[i]["add_charge_amt5"];
                                                        objIds.time_order_entered = dt.Rows[i]["time_order_entered"];
                                                        objIds.pick_del_trans_flag_text = dt.Rows[i]["pick_del_trans_flag_text"];
                                                        objIds.pick_del_trans_flag = dt.Rows[i]["pick_del_trans_flag"];
                                                        objIds.pickup_attention = dt.Rows[i]["pickup_attention"];
                                                        objIds.rate_buck_amt7 = dt.Rows[i]["rate_buck_amt7"];
                                                        objIds.add_charge_occur6 = dt.Rows[i]["add_charge_occur6"];
                                                        objIds.fuel_price_source = dt.Rows[i]["fuel_price_source"];
                                                        objIds.pickup_airport_code = dt.Rows[i]["pickup_airport_code"];
                                                        objIds.rate_buck_amt2 = dt.Rows[i]["rate_buck_amt2"];
                                                        objIds.rate_buck_amt3 = dt.Rows[i]["rate_buck_amt3"];
                                                        objIds.deliver_omw_timestamp = dt.Rows[i]["deliver_omw_timestamp"];
                                                        objIds.exception_code = dt.Rows[i]["exception_code"];
                                                        objIds.status_code_text = dt.Rows[i]["status_code_text"];
                                                        objIds.status_code = dt.Rows[i]["status_code"];
                                                        objIds.weight = dt.Rows[i]["weight"];
                                                        objIds.signature_required = dt.Rows[i]["signature_required"];
                                                        objIds.rate_buck_amt10 = dt.Rows[i]["rate_buck_amt10"];
                                                        objIds.hist_inv_number = dt.Rows[i]["hist_inv_number"];
                                                        objIds.deliver_pricing_zone = dt.Rows[i]["deliver_pricing_zone"];
                                                        objIds.pickup_actual_longitude = dt.Rows[i]["pickup_actual_longitude"];
                                                        objIds.push_services = dt.Rows[i]["push_services"];
                                                        objIds.add_charge_amt11 = dt.Rows[i]["add_charge_amt11"];
                                                        objIds.rt_actual_location_accuracy = dt.Rows[i]["rt_actual_location_accuracy"];
                                                        objIds.roundtrip_actual_date = dt.Rows[i]["roundtrip_actual_date"];
                                                        objIds.pickup_requested_arr_time = dt.Rows[i]["pickup_requested_arr_time"];
                                                        objIds.deliver_attention = dt.Rows[i]["deliver_attention"];
                                                        objIds.deliver_special_instructions3 = dt.Rows[i]["deliver_special_instructions3"];
                                                        objIds.pickup_actual_pieces = dt.Rows[i]["pickup_actual_pieces"];
                                                        objIds.edi_order_accepted_or_rejected_text = dt.Rows[i]["edi_order_accepted_or_rejected_text"];
                                                        objIds.edi_order_accepted_or_rejected = dt.Rows[i]["edi_order_accepted_or_rejected"];
                                                        objIds.roundtrip_signature = dt.Rows[i]["roundtrip_signature"];
                                                        objIds.po_number = dt.Rows[i]["po_number"];
                                                        objIds.signature = dt.Rows[i]["signature"];
                                                        objIds.pickup_special_instructions2 = dt.Rows[i]["pickup_special_instructions2"];
                                                        objIds.original_ctrl_number = dt.Rows[i]["original_ctrl_number"];
                                                        objIds.previous_ctrl_number = dt.Rows[i]["previous_ctrl_number"];
                                                        objIds.id = dt.Rows[i]["Id"];
                                                        idList.Add(objIds);

                                                    }
                                                    objCommon.SaveOutputDataToCsvFile(idList, "Order-Create",
                                                       strInputFilePath, ReferenceId, strFileName, strDatetime);
                                                }
                                                if (dsOrderResponse.Tables.Contains("settlements"))
                                                {
                                                    List<Settlement> settelmentList = new List<Settlement>();
                                                    for (int i = 0; i < dsOrderResponse.Tables["settlements"].Rows.Count; i++)
                                                    {
                                                        DataTable dt = dsOrderResponse.Tables["settlements"];
                                                        Settlement objsettlements = new Settlement();
                                                        objsettlements.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                        objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                        objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                        objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                        objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                        objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                        objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                        objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                        objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                        objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                        objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                        objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                        objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                        objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                        objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                        objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                        objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                        objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                        objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                        objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                        objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                        objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                        objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                        objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                        objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                        objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                        objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                        objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                        objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                        objsettlements.id = (dt.Rows[i]["id"]);
                                                        objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                        objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                        objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                        objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                        objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                        objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                        objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                        objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                        objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                        objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                        objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                        objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                        objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                        objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                        objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                        objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                        objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                        objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                        objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                        objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                        objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);
                                                        settelmentList.Add(objsettlements);
                                                    }

                                                    objCommon.SaveOutputDataToCsvFile(settelmentList, "Order-Settlements-AddRecord",
                                                       strInputFilePath, ReferenceId, strFileName, strDatetime);

                                                }
                                                if (dsOrderResponse.Tables.Contains("progress"))
                                                {

                                                    List<Progress> progressList = new List<Progress>();
                                                    for (int i = 0; i < dsOrderResponse.Tables["progress"].Rows.Count; i++)
                                                    {
                                                        Progress progress = new Progress();
                                                        DataTable dt = dsOrderResponse.Tables["progress"];

                                                        progress.status_date = (dt.Rows[i]["status_date"]);
                                                        progress.status_text = (dt.Rows[i]["status_text"]);
                                                        progress.status_time = (dt.Rows[i]["status_time"]);
                                                        progress.id = (dt.Rows[i]["id"]);
                                                        progressList.Add(progress);
                                                    }

                                                    objCommon.SaveOutputDataToCsvFile(progressList, "Order-Progress",
                                                       strInputFilePath, ReferenceId, strFileName, strDatetime);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                strExecutionLogMessage = "ProcessAddOrderFiles Exception -" + ex.Message + System.Environment.NewLine;
                                                strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                                strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                                strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                //objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                //using (StreamWriter writer = new StreamWriter(filePath, true))
                                                //{
                                                //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                                //}
                                            }

                                            if (driver1 != null)
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
                                                            strExecutionLogMessage += "Carrier Base Pay Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                            strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                            //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                            //using (StreamWriter writer = new StreamWriter(filePath, true))
                                                            //{
                                                            //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                                            //}
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        strExecutionLogMessage = "OrderPost-OrderSettlementPut Error " + System.Environment.NewLine;
                                                        strExecutionLogMessage += "Carrier Base Pay Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                        strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                        objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                        // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                        //using (StreamWriter writer = new StreamWriter(filePath, true))
                                                        //{
                                                        //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                                        //}
                                                        break;
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

                                                            //using (StreamWriter writer = new StreamWriter(filePath, true))
                                                            //{
                                                            //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                                            //}
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        strExecutionLogMessage = "OrderPost-OrderSettlementPut Error " + System.Environment.NewLine;
                                                        strExecutionLogMessage += "Carrier ACC Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                        strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                        objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                        // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                        //using (StreamWriter writer = new StreamWriter(filePath, true))
                                                        //{
                                                        //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                                        //}
                                                        break;
                                                    }


                                                    ordersettlementputrequest = @"{" + ordersettlementputrequest + "}";
                                                    string order_settlementObject = @"{'order_settlement': " + ordersettlementputrequest + "}";
                                                    JObject jsonobj = JObject.Parse(order_settlementObject);
                                                    request = jsonobj.ToString();

                                                    clsCommon.ReturnResponse objresponseOrdersettlement = new clsCommon.ReturnResponse();
                                                    objresponseOrdersettlement = objclsDatatrac.CallDataTracOrderSettlementPutAPI(UniqueId, order_settlementObject);

                                                    //objresponseOrdersettlement.Reason = "{\"002018724450D1\": {\"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"time_last_updated\": \"05:10\", \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"order_date\": \"2021-06-28\", \"agent_etrac_transaction_number\": null, \"settlement_bucket1_pct\": null, \"settlement_bucket2_pct\": null, \"vendor_employee_numer\": null, \"fuel_price_zone\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"charge1\": 35.91, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"adjustment_type\": null, \"agents_etrac_partner_id\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"vendor_invoice_number\": null, \"charge6\": null, \"settlement_bucket4_pct\": null, \"fuel_plan\": null, \"record_type\": 0, \"voucher_amount\": null, \"id\": \"002018724450D1\", \"date_last_updated\": \"2021-07-08\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"voucher_number\": null, \"driver_number_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver_number\": 3208, \"settlement_bucket6_pct\": null, \"settlement_pct\": 100.00, \"charge3\": null, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"voucher_date\": null, \"fuel_price_source\": null, \"pay_chart_used\": null, \"settlement_bucket5_pct\": null, \"charge2\": null, \"control_number\": 1872445, \"settlement_bucket3_pct\": null, \"charge5\": null, \"pre_book_percentage\": true, \"settlement_period_end_date\": null}}";
                                                    // objresponseOrdersettlement.ResponseVal = true;

                                                    if (objresponseOrdersettlement.ResponseVal)
                                                    {
                                                        strExecutionLogMessage = "OrderPost-OrderSettlementPutAPI Success " + System.Environment.NewLine;
                                                        strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                        strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                                        objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                        //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                                        //using (StreamWriter writer = new StreamWriter(filePath, true))
                                                        //{
                                                        //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                                        //}

                                                        DataSet dsOrderSettlementResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                                                        dsOrderSettlementResponse.Tables[0].TableName = "OrderSettlement";


                                                        List<ResponseOrderSettlements> orderSettlementstList = new List<ResponseOrderSettlements>();
                                                        for (int i = 0; i < dsOrderSettlementResponse.Tables["OrderSettlement"].Rows.Count; i++)
                                                        {
                                                            DataTable dt = dsOrderSettlementResponse.Tables["OrderSettlement"];
                                                            ResponseOrderSettlements objsettlements = new ResponseOrderSettlements();
                                                            objsettlements.company_number_text = dt.Rows[i]["company_number_text"];
                                                            objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                            objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                            objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                            objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                            objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                            objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                            objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                            objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                            objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                            objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                            objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                            objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                            objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                            objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                            objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                            objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                            objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                            objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                            objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                            objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                            objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                            objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                            objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                            objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                            objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                            objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                            objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                            objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                            objsettlements.id = (dt.Rows[i]["id"]);
                                                            objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                            objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                            objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                            objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                            objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                            objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                            objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                            objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                            objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                            objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                            objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                            objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                            objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                            objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                            objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                            objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                            objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                            objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                            objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                            objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                            objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);
                                                            orderSettlementstList.Add(objsettlements);
                                                        }

                                                        objCommon.SaveOutputDataToCsvFile(orderSettlementstList, "OrderSettlements-UpdatedRecord",
                                                           strInputFilePath, UniqueId, strFileName, strDatetime);
                                                    }
                                                    else
                                                    {
                                                        strExecutionLogMessage = "OrderPost-OrderSettlementPutAPI Failed " + System.Environment.NewLine;
                                                        strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                        strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                                        objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                        // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                        //using (StreamWriter writer = new StreamWriter(filePath, true))
                                                        //{
                                                        //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                                        //}

                                                        DataSet dsOrderPutFailureResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                                                        dsOrderPutFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                        dsOrderPutFailureResponse.Tables[0].Columns.Add("UniqueId", typeof(System.String));
                                                        foreach (DataRow row in dsOrderPutFailureResponse.Tables[0].Rows)
                                                        {
                                                            row["UniqueId"] = UniqueId;
                                                        }
                                                        objCommon.WriteDataToCsvFile(dsOrderPutFailureResponse.Tables[0],
                                                        strInputFilePath, UniqueId, strFileName, strDatetime);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            request = JsonConvert.SerializeObject(objorderdetails);
                                            strExecutionLogMessage = "OrderPostAPI Failed " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                            strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                            //using (StreamWriter writer = new StreamWriter(filePath, true))
                                            //{
                                            //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                            //}

                                            DataSet dsOrderFailureResponse = objCommon.jsonToDataSet(objresponse.Reason);
                                            dsOrderFailureResponse.Tables[0].TableName = "OrderFailure";
                                            dsOrderFailureResponse.Tables[0].Columns.Add("Customer Reference", typeof(System.String));
                                            foreach (DataRow row in dsOrderFailureResponse.Tables[0].Rows)
                                            {
                                                row["Customer Reference"] = objOrder.reference;
                                            }
                                            objCommon.WriteDataToCsvFile(dsOrderFailureResponse.Tables[0],
                                        strInputFilePath, ReferenceId, processingFileName, strDatetime);

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        strExecutionLogMessage = "ProcessAddOrderFiles Exception -" + ex.Message + System.Environment.NewLine;
                                        strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                        strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                        strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                        objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                        //objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                        //using (StreamWriter writer = new StreamWriter(filePath, true))
                                        //{
                                        //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                        //}
                                    }
                                }

                            });

                            strExecutionLogMessage = " Parallelly Processing  finished for the  file : " + strFileName + "." + System.Environment.NewLine;
                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                            // objCommon.MoveOutputFilesToOutputLocation(strInputFilePath);

                            break;
                            Parallel.ForEach(files, (currentFile) =>
                            {
                                var fileName = Path.GetFileName(currentFile);
                                var processingFileName = Path.GetFileName(currentFile);
                                int fileExtPos = fileName.LastIndexOf(".");
                                if (fileExtPos >= 0)
                                    fileName = fileName.Substring(0, fileExtPos);
                                strExecutionLogMessage = "Current Processing File is  : " + fileName + "." + System.Environment.NewLine;
                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                DataSet dtExcel = new DataSet();
                                dtExcel = clsExcelHelper.ImportExcelXLSX(strwokingfolder + @"\" + processingFileName, false);
                                if (dtExcel.Tables.Count > 0)
                                {
                                    var datatable = dtExcel.Tables[0];

                                    foreach (DataRow dr in datatable.Rows)
                                    {
                                        object value = dr["Company"];
                                        if (value == DBNull.Value)
                                            break;

                                        ReferenceId = Convert.ToString(dr["Customer Reference"]);
                                        strExecutionLogMessage = "Customer Reference is : " + ReferenceId + "." + System.Environment.NewLine;
                                        //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                        objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                        try
                                        {
                                            orderdetails objorderdetails = new orderdetails();
                                            order objOrder = new order();
                                            objOrder.company_number = Convert.ToInt32(dr["Company"]);
                                            objOrder.service_level = Convert.ToInt32(dr["Service Type"]);
                                            objOrder.customer_number = Convert.ToInt32(dr["Billing Customer Number"]);
                                            objOrder.reference = Convert.ToString(dr["Customer Reference"]);
                                            DateTime dtValue = Convert.ToDateTime(dr["Delivery Date"]);
                                            objOrder.pickup_requested_date = dtValue.ToString("yyyy-MM-dd");

                                            dtValue = Convert.ToDateTime(dr["Delivery Date"]);
                                            objOrder.pickup_actual_date = dtValue.ToString("yyyy-MM-dd");

                                            dtValue = Convert.ToDateTime(dr["Pickup actual arrival time"]);
                                            objOrder.pickup_actual_arr_time = dtValue.ToString("HH:mm");

                                            dtValue = Convert.ToDateTime(dr["Pickup actual depart time"]);
                                            objOrder.pickup_actual_dep_time = dtValue.ToString("HH:mm");

                                            dtValue = Convert.ToDateTime(dr["Pickup actual depart time"]);
                                            objOrder.pickup_actual_dep_time = dtValue.ToString("HH:mm");

                                            dtValue = Convert.ToDateTime(dr["Pickup no later than"]);
                                            objOrder.pickup_requested_dep_time = dtValue.ToString("HH:mm");
                                            objOrder.pickup_name = Convert.ToString(dr["Store Code"]);
                                            objOrder.pickup_address = Convert.ToString(dr["Pickup address"]);
                                            objOrder.pickup_city = Convert.ToString(dr["Pickup city"]);
                                            objOrder.pickup_state = Convert.ToString(dr["Pickup state/province"]);
                                            objOrder.pickup_zip = Convert.ToString(dr["Pickup zip/postal code"]);

                                            dtValue = Convert.ToDateTime(dr["Delivery Date"]);
                                            objOrder.deliver_requested_date = dtValue.ToString("yyyy-MM-dd");

                                            dtValue = Convert.ToDateTime(dr["Deliver no earlier than"]);
                                            objOrder.deliver_requested_arr_time = dtValue.ToString("HH:mm");

                                            dtValue = Convert.ToDateTime(dr["Deliver no later than"]);
                                            objOrder.deliver_requested_dep_time = dtValue.ToString("HH:mm");

                                            dtValue = Convert.ToDateTime(dr["Delivery Date"]);
                                            objOrder.deliver_actual_date = dtValue.ToString("yyyy-MM-dd");

                                            dtValue = Convert.ToDateTime(dr["Delivery actual arrive time"]);
                                            objOrder.deliver_actual_arr_time = dtValue.ToString("HH:mm");

                                            dtValue = Convert.ToDateTime(dr["Delivery actual depart time"]);
                                            objOrder.deliver_actual_dep_time = dtValue.ToString("HH:mm");

                                            objOrder.deliver_name = Convert.ToString(dr["Customer Name"]);
                                            objOrder.deliver_address = Convert.ToString(dr["Address"]);
                                            objOrder.deliver_city = Convert.ToString(dr["City"]);
                                            objOrder.deliver_state = Convert.ToString(dr["State"]);
                                            objOrder.deliver_zip = Convert.ToString(dr["Zip"]);

                                            objOrder.signature = Convert.ToString(dr["Delivery text signature"]);
                                            objOrder.rate_buck_amt1 = Convert.ToDouble(dr["Bill Rate"]);
                                            objOrder.rate_buck_amt3 = Convert.ToDouble(dr["Pieces ACC"]);
                                            objOrder.rate_buck_amt10 = Convert.ToDouble(dr["FSC"]);
                                            objOrder.number_of_pieces = Convert.ToInt32(dr["Pieces"]);
                                            objOrder.rate_miles = Convert.ToInt32(Convert.ToDouble(dr["Miles"]));
                                            string driver1 = null;
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Correct Driver Number"])))
                                            {
                                                objOrder.driver1 = Convert.ToInt32(dr["Correct Driver Number"]);
                                                driver1 = Convert.ToString(dr["Correct Driver Number"]);
                                            }
                                            objOrder.ordered_by = Convert.ToString(dr["Requested by"]);
                                            objOrder.csr = Convert.ToString(dr["Entered by"]);


                                            objOrder.pick_del_trans_flag = Convert.ToString(dr["Pickup Delivery Transfer Flag"]);

                                            objOrder.pickup_signature = Convert.ToString(dr["Pickup text signature"]);
                                            objorderdetails.order = objOrder;
                                            clsDatatrac objclsDatatrac = new clsDatatrac();
                                            clsCommon.ReturnResponse objresponse = new clsCommon.ReturnResponse();
                                            string request = JsonConvert.SerializeObject(objorderdetails);
                                            objresponse = objclsDatatrac.CallDataTracOrderPostAPI(objorderdetails);
                                            //objresponse.ResponseVal = true;
                                            // objresponse.Reason = "{\"002018775030\": {\"verified_weight\": null, \"roundtrip_actual_latitude\": null, \"pickup_special_instructions4\": null, \"fuel_miles\": null, \"pickup_dispatch_zone\": null, \"pickup_zip\": \"23150\", \"pickup_actual_arr_time\": \"08:00\", \"cod_accept_company_check\": false, \"add_charge_occur9\": null, \"pickup_omw_latitude\": null, \"service_level_text\": \"Threshold Delivery\", \"service_level\": 57, \"exception_sign_required\": false, \"pickup_phone_ext\": null, \"roundtrip_actual_pieces\": null, \"bringg_send_sms\": false, \"az_equip2\": null, \"hist_inv_date\": null, \"date_order_entered\": \"2021-07-21\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"pickup_city\": \"SANDSTON\", \"pickup_phone\": null, \"pickup_sign_req\": true, \"deliver_route_sequence\": null, \"deliver_phone\": null, \"deliver_omw_longitude\": null, \"roundtrip_actual_longitude\": null, \"page_number\": 1, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"add_charge_code9\": null, \"pickup_eta_time\": null, \"record_type\": 0, \"add_charge_occur11\": null, \"push_partner_order_id\": null, \"deliver_country\": null, \"customer_name\": \"MXD/RYDER\", \"bol_number\": null, \"pickup_latitude\": 37.53250820, \"add_charge_code4\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"pu_arrive_notification_sent\": false, \"distribution_shift_id\": null, \"pickup_special_instr_long\": null, \"id\": \"002018775030\", \"callback_to\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"ordered_by\": \"RYDER\", \"add_charge_code12\": null, \"pickup_route_seq\": null, \"deliver_city\": \"HANOVER\", \"add_charge_occur5\": null, \"edi_acknowledgement_required\": false, \"rescheduled_ctrl_number\": null, \"driver2\": null, \"deliver_room\": null, \"deliver_actual_arr_time\": \"08:00\", \"fuel_price_zone\": null, \"add_charge_amt9\": null, \"add_charge_amt4\": null, \"delivery_address_point_number_text\": \"PAMELA YORK\", \"delivery_address_point_number\": 24254, \"deliver_actual_longitude\": null, \"add_charge_amt2\": null, \"additional_drivers\": false, \"pickup_pricing_zone\": 1, \"hazmat\": false, \"pickup_address\": \"540 EASTPARK CT\", \"pickup_route_code\": null, \"callback_userid\": null, \"pickup_point_customer\": 31025, \"rate_buck_amt1\": 52.00, \"add_charge_amt8\": null, \"callback_time\": null, \"csr\": \"DX*\", \"roundtrip_actual_depart_time\": null, \"customers_etrac_partner_id\": \"96609250\", \"manual_notepad\": false, \"add_charge_code8\": null, \"bringg_order_id\": null, \"deliver_omw_latitude\": null, \"pickup_longitude\": -77.33035820, \"etrac_number\": null, \"distribution_unique_id\": 0, \"vehicle_type\": null, \"roundtrip_actual_arrival_time\": null, \"delivery_longitude\": -77.28769600, \"pu_actual_location_accuracy\": null, \"deliver_actual_date\": \"2021-07-14\", \"exception_timestamp\": null, \"deliver_zip\": \"23069\", \"roundtrip_wait_time\": null, \"add_charge_occur8\": null, \"dl_arrive_notification_sent\": false, \"pickup_special_instructions1\": null, \"ordered_by_phone_number\": null, \"deliver_requested_arr_time\": \"08:00\", \"rate_miles\": null, \"holiday_groups\": null, \"pickup_email_notification_sent\": false, \"add_charge_code3\": null, \"dispatch_id\": null, \"add_charge_occur10\": null, \"dispatch_time\": null, \"deliver_wait_time\": null, \"invoice_period_end_date\": null, \"add_charge_occur12\": null, \"fuel_plan\": null, \"return_svc_level\": null, \"pickup_actual_date\": \"2021-07-14\", \"send_new_order_alert\": false, \"pickup_room\": null, \"rate_buck_amt8\": null, \"add_charge_amt10\": null, \"insurance_amount\": null, \"add_charge_amt3\": null, \"add_charge_amt6\": null, \"pickup_special_instructions3\": null, \"pickup_requested_date\": \"2021-07-14\", \"roundtrip_sign_req\": false, \"actual_miles\": null, \"pickup_address_point_number_text\": \"1STOPBEDROOMS\", \"pickup_address_point_number\": 19845, \"deliver_actual_latitude\": null, \"deliver_phone_ext\": null, \"deliver_route_code\": null, \"add_charge_code10\": null, \"delivery_airport_code\": null, \"reference\": null, \"photos_exist\": false, \"master_airway_bill_number\": null, \"control_number\": 1877503, \"cod_text\": \"No\", \"cod\": \"N\", \"rate_buck_amt11\": null, \"cod_amount\": null, \"pickup_omw_timestamp\": null, \"signature_images\": [], \"deliver_special_instructions1\": null, \"quote_amount\": null, \"total_pages\": 1, \"rate_buck_amt4\": null, \"line_items\": [], \"delivery_latitude\": 37.80844290, \"progress\": [{\"status_time\": \"15:57:00\", \"status_date\": \"2021-07-21\", \"status_text\": \"Entered in carrier's system\"}, {\"status_time\": \"08:30:00\", \"status_date\": \"2021-07-14\", \"status_text\": \"Picked up\"}, {\"status_time\": \"08:15:00\", \"status_date\": \"2021-07-14\", \"status_text\": \"Delivered\"}], \"add_charge_code1\": null, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"deliver_special_instr_long\": null, \"deliver_address\": \"205 HARDWOOD LN\", \"add_charge_occur4\": null, \"deliver_eta_date\": null, \"pickup_actual_dep_time\": \"08:30\", \"deliver_requested_dep_time\": \"17:00\", \"add_charge_code7\": null, \"deliver_actual_dep_time\": \"08:15\", \"cod_accept_cashiers_check\": false, \"bringg_last_loc_sent\": null, \"az_equip3\": null, \"driver1_text\": \"WALT DARBY\", \"driver1\": 3215, \"pickup_actual_latitude\": null, \"add_charge_occur2\": null, \"order_automatically_quoted\": false, \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"hours\": \"15\", \"frequent_caller_id\": null, \"rate_buck_amt6\": null, \"rate_chart_used\": 0, \"deliver_actual_pieces\": null, \"add_charge_code5\": null, \"pickup_omw_longitude\": null, \"notes\": [], \"delivery_point_customer\": 31025, \"add_charge_occur7\": null, \"rate_buck_amt5\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"add_charge_code11\": null, \"pickup_name\": \"1STOPBEDROOMS\", \"callback_date\": null, \"add_charge_code2\": null, \"house_airway_bill_number\": null, \"deliver_name\": \"PAMELA YORK\", \"number_of_pieces\": 1, \"deliver_eta_time\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"rate_special_instructions\": null, \"add_charge_occur3\": null, \"pickup_eta_date\": null, \"deliver_special_instructions4\": null, \"custom_special_instr_long\": null, \"deliver_special_instructions2\": null, \"pickup_signature\": \"SOF\", \"az_equip1\": null, \"add_charge_amt12\": null, \"settlements\": [{\"vendor_invoice_number\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"voucher_date\": null, \"voucher_amount\": null, \"fuel_price_source\": null, \"settlement_bucket6_pct\": null, \"pre_book_percentage\": true, \"settlement_bucket4_pct\": null, \"settlement_period_end_date\": null, \"charge5\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"settlement_bucket5_pct\": null, \"voucher_number\": null, \"charge6\": null, \"settlement_bucket2_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"settlement_pct\": 100.00, \"vendor_employee_numer\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"driver_number_text\": \"WALT DARBY\", \"driver_number\": 3215, \"fuel_plan\": null, \"agents_etrac_partner_id\": null, \"charge2\": null, \"charge3\": null, \"settlement_bucket1_pct\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"id\": \"002018775030D1\", \"record_type\": 0, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"pay_chart_used\": null, \"fuel_price_zone\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"charge1\": null, \"control_number\": 1877503, \"adjustment_type\": null, \"order_date\": \"2021-07-14\", \"time_last_updated\": \"14:57\", \"agent_etrac_transaction_number\": null, \"date_last_updated\": \"2021-07-21\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"settlement_bucket3_pct\": null}], \"calc_add_on_chgs\": false, \"original_schedule_number\": null, \"blocks\": null, \"del_actual_location_accuracy\": null, \"zone_set_used\": 1, \"pickup_country\": null, \"pickup_state\": \"VA\", \"add_charge_amt7\": null, \"email_addresses\": null, \"add_charge_occur1\": null, \"pickup_wait_time\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"distribution_branch_id\": null, \"rate_buck_amt9\": null, \"add_charge_amt1\": null, \"pickup_requested_dep_time\": \"09:00\", \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"deliver_state\": \"VA\", \"deliver_dispatch_zone\": null, \"image_sign_req\": false, \"add_charge_code6\": null, \"deliver_requested_date\": \"2021-07-14\", \"add_charge_amt5\": null, \"time_order_entered\": \"15:57\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"pickup_attention\": null, \"rate_buck_amt7\": null, \"add_charge_occur6\": null, \"fuel_price_source\": null, \"pickup_airport_code\": null, \"rate_buck_amt2\": null, \"rate_buck_amt3\": null, \"deliver_omw_timestamp\": null, \"exception_code\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"weight\": null, \"signature_required\": true, \"rate_buck_amt10\": null, \"hist_inv_number\": 0, \"deliver_pricing_zone\": 1, \"pickup_actual_longitude\": null, \"push_services\": null, \"add_charge_amt11\": null, \"rt_actual_location_accuracy\": null, \"roundtrip_actual_date\": null, \"pickup_requested_arr_time\": null, \"deliver_attention\": null, \"deliver_special_instructions3\": null, \"pickup_actual_pieces\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"roundtrip_signature\": null, \"po_number\": null, \"signature\": \"SOF\", \"pickup_special_instructions2\": null, \"original_ctrl_number\": null, \"previous_ctrl_number\": null}}";
                                            // objresponse.Reason = "{\"002018724440\": {\"verified_weight\": null, \"roundtrip_actual_latitude\": null, \"pickup_special_instructions4\": null, \"fuel_miles\": null, \"pickup_dispatch_zone\": null, \"pickup_zip\": \"23150\", \"pickup_actual_arr_time\": \"08:00\", \"cod_accept_company_check\": false, \"add_charge_occur9\": null, \"pickup_omw_latitude\": null, \"service_level_text\": \"Room of Choice\", \"service_level\": 55, \"exception_sign_required\": false, \"pickup_phone_ext\": null, \"roundtrip_actual_pieces\": null, \"bringg_send_sms\": false, \"az_equip2\": null, \"hist_inv_date\": null, \"date_order_entered\": \"2021-07-08\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"pickup_city\": \"SANDSTON\", \"pickup_phone\": null, \"pickup_sign_req\": true, \"deliver_route_sequence\": null, \"deliver_phone\": null, \"deliver_omw_longitude\": null, \"roundtrip_actual_longitude\": null, \"page_number\": 1, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"add_charge_code9\": null, \"pickup_eta_time\": null, \"record_type\": 0, \"add_charge_occur11\": null, \"push_partner_order_id\": null, \"deliver_country\": null, \"customer_name\": \"MXD/RYDER\", \"bol_number\": null, \"pickup_latitude\": 37.53250820, \"add_charge_code4\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"pu_arrive_notification_sent\": false, \"distribution_shift_id\": null, \"pickup_special_instr_long\": null, \"id\": \"002018724440\", \"callback_to\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"ordered_by\": \"RYDER\", \"add_charge_code12\": null, \"pickup_route_seq\": null, \"deliver_city\": \"CHESAPEAKE\", \"add_charge_occur5\": null, \"edi_acknowledgement_required\": false, \"rescheduled_ctrl_number\": null, \"driver2\": null, \"deliver_room\": null, \"deliver_actual_arr_time\": \"08:00\", \"fuel_price_zone\": null, \"add_charge_amt9\": null, \"add_charge_amt4\": null, \"delivery_address_point_number_text\": \"ANNE BAILEY\", \"delivery_address_point_number\": 26312, \"deliver_actual_longitude\": null, \"add_charge_amt2\": null, \"additional_drivers\": false, \"pickup_pricing_zone\": 1, \"hazmat\": false, \"pickup_address\": \"540 EASTPARK CT\", \"pickup_route_code\": null, \"callback_userid\": null, \"pickup_point_customer\": 31025, \"rate_buck_amt1\": 57.00, \"add_charge_amt8\": null, \"callback_time\": null, \"csr\": \"DX*\", \"roundtrip_actual_depart_time\": null, \"customers_etrac_partner_id\": \"96609250\", \"manual_notepad\": false, \"add_charge_code8\": null, \"bringg_order_id\": null, \"deliver_omw_latitude\": null, \"pickup_longitude\": -77.33035820, \"etrac_number\": null, \"distribution_unique_id\": 0, \"vehicle_type\": null, \"roundtrip_actual_arrival_time\": null, \"delivery_longitude\": -76.34760620, \"pu_actual_location_accuracy\": null, \"deliver_actual_date\": \"2021-06-28\", \"exception_timestamp\": null, \"deliver_zip\": \"23323\", \"roundtrip_wait_time\": null, \"add_charge_occur8\": null, \"dl_arrive_notification_sent\": false, \"pickup_special_instructions1\": null, \"ordered_by_phone_number\": null, \"deliver_requested_arr_time\": \"08:00\", \"rate_miles\": null, \"holiday_groups\": null, \"pickup_email_notification_sent\": false, \"add_charge_code3\": null, \"dispatch_id\": null, \"add_charge_occur10\": null, \"dispatch_time\": null, \"deliver_wait_time\": null, \"invoice_period_end_date\": null, \"add_charge_occur12\": null, \"fuel_plan\": null, \"return_svc_level\": null, \"pickup_actual_date\": \"2021-06-28\", \"send_new_order_alert\": false, \"pickup_room\": null, \"rate_buck_amt8\": null, \"add_charge_amt10\": null, \"insurance_amount\": null, \"add_charge_amt3\": null, \"add_charge_amt6\": null, \"pickup_special_instructions3\": null, \"pickup_requested_date\": \"2021-06-28\", \"roundtrip_sign_req\": false, \"actual_miles\": null, \"pickup_address_point_number_text\": \"BIG LOTS\", \"pickup_address_point_number\": 19864, \"deliver_actual_latitude\": null, \"deliver_phone_ext\": null, \"deliver_route_code\": null, \"add_charge_code10\": null, \"delivery_airport_code\": null, \"reference_text\": \"2125095801\", \"reference\": \"2125095801\", \"photos_exist\": false, \"master_airway_bill_number\": null, \"control_number\": 1872444, \"cod_text\": \"No\", \"cod\": \"N\", \"rate_buck_amt11\": null, \"cod_amount\": null, \"pickup_omw_timestamp\": null, \"signature_images\": [], \"deliver_special_instructions1\": null, \"quote_amount\": null, \"total_pages\": 1, \"rate_buck_amt4\": null, \"line_items\": [], \"delivery_latitude\": 36.78396970, \"progress\": [{\"status_time\": \"06:02:00\", \"status_date\": \"2021-07-08\", \"status_text\": \"Entered in carrier's system\"}, {\"status_time\": \"08:30:00\", \"status_date\": \"2021-06-28\", \"status_text\": \"Picked up\"}, {\"status_time\": \"08:15:00\", \"status_date\": \"2021-06-28\", \"status_text\": \"Delivered\"}], \"add_charge_code1\": null, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"deliver_special_instr_long\": null, \"deliver_address\": \"2920 AARON DR\", \"add_charge_occur4\": null, \"deliver_eta_date\": null, \"pickup_actual_dep_time\": \"08:30\", \"deliver_requested_dep_time\": \"17:00\", \"add_charge_code7\": null, \"deliver_actual_dep_time\": \"08:15\", \"cod_accept_cashiers_check\": false, \"bringg_last_loc_sent\": null, \"az_equip3\": null, \"driver1_text\": \"LAVERT KENDALL MORRIS\", \"driver1\": 3001, \"pickup_actual_latitude\": null, \"add_charge_occur2\": null, \"order_automatically_quoted\": false, \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"hours\": \"15\", \"frequent_caller_id\": null, \"rate_buck_amt6\": null, \"rate_chart_used\": 0, \"deliver_actual_pieces\": null, \"add_charge_code5\": null, \"pickup_omw_longitude\": null, \"notes\": [], \"delivery_point_customer\": 31025, \"add_charge_occur7\": null, \"rate_buck_amt5\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"add_charge_code11\": null, \"pickup_name\": \"BIG LOTS\", \"callback_date\": null, \"add_charge_code2\": null, \"house_airway_bill_number\": null, \"deliver_name\": \"ANNE BAILEY\", \"number_of_pieces\": 3, \"deliver_eta_time\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"rate_special_instructions\": null, \"add_charge_occur3\": null, \"pickup_eta_date\": null, \"deliver_special_instructions4\": null, \"custom_special_instr_long\": null, \"deliver_special_instructions2\": null, \"pickup_signature\": \"SOF\", \"az_equip1\": null, \"add_charge_amt12\": null, \"settlements\": [{\"vendor_invoice_number\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"voucher_date\": null, \"voucher_amount\": null, \"fuel_price_source\": null, \"settlement_bucket6_pct\": null, \"pre_book_percentage\": true, \"settlement_bucket4_pct\": null, \"settlement_period_end_date\": null, \"charge5\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"settlement_bucket5_pct\": null, \"voucher_number\": null, \"charge6\": null, \"settlement_bucket2_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"settlement_pct\": 100.00, \"vendor_employee_numer\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"driver_number_text\": \"LAVERT KENDALL MORRIS\", \"driver_number\": 3001, \"fuel_plan\": null, \"agents_etrac_partner_id\": null, \"charge2\": null, \"charge3\": null, \"settlement_bucket1_pct\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"id\": \"002018724440D1\", \"record_type\": 0, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"pay_chart_used\": null, \"fuel_price_zone\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"charge1\": null, \"control_number\": 1872444, \"adjustment_type\": null, \"order_date\": \"2021-06-28\", \"time_last_updated\": \"05:02\", \"agent_etrac_transaction_number\": null, \"date_last_updated\": \"2021-07-08\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"settlement_bucket3_pct\": null}], \"calc_add_on_chgs\": false, \"original_schedule_number\": null, \"blocks\": null, \"del_actual_location_accuracy\": null, \"zone_set_used\": 1, \"pickup_country\": null, \"pickup_state\": \"VA\", \"add_charge_amt7\": null, \"email_addresses\": null, \"add_charge_occur1\": null, \"pickup_wait_time\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"distribution_branch_id\": null, \"rate_buck_amt9\": null, \"add_charge_amt1\": null, \"pickup_requested_dep_time\": \"09:00\", \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"deliver_state\": \"VA\", \"deliver_dispatch_zone\": null, \"image_sign_req\": false, \"add_charge_code6\": null, \"deliver_requested_date\": \"2021-06-28\", \"add_charge_amt5\": null, \"time_order_entered\": \"06:02\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"pickup_attention\": null, \"rate_buck_amt7\": null, \"add_charge_occur6\": null, \"fuel_price_source\": null, \"pickup_airport_code\": null, \"rate_buck_amt2\": null, \"rate_buck_amt3\": null, \"deliver_omw_timestamp\": null, \"exception_code\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"weight\": null, \"signature_required\": true, \"rate_buck_amt10\": 2.34, \"hist_inv_number\": 0, \"deliver_pricing_zone\": 1, \"pickup_actual_longitude\": null, \"push_services\": null, \"add_charge_amt11\": null, \"rt_actual_location_accuracy\": null, \"roundtrip_actual_date\": null, \"pickup_requested_arr_time\": \"07:00\", \"deliver_attention\": null, \"deliver_special_instructions3\": null, \"pickup_actual_pieces\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"roundtrip_signature\": null, \"po_number\": null, \"signature\": \"SOF\", \"pickup_special_instructions2\": null, \"original_ctrl_number\": null, \"previous_ctrl_number\": null}}";
                                            // objresponse.Reason = "{\"002018724450\": {\"roundtrip_actual_date\": null, \"notes\": [], \"pickup_phone_ext\": null, \"holiday_groups\": null, \"deliver_eta_time\": null, \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"add_charge_occur4\": null, \"deliver_state\": \"VA\", \"quote_amount\": null, \"cod_text\": \"No\", \"cod\": \"N\", \"additional_drivers\": false, \"rescheduled_ctrl_number\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"pickup_actual_pieces\": null, \"record_type\": 0, \"pickup_special_instr_long\": null, \"pickup_special_instructions3\": null, \"exception_timestamp\": null, \"deliver_actual_arr_time\": \"08:00\", \"house_airway_bill_number\": null, \"deliver_pricing_zone\": 1, \"total_pages\": 1, \"add_charge_occur11\": null, \"deliver_omw_latitude\": null, \"callback_userid\": null, \"rate_buck_amt1\": 57.00, \"pickup_point_customer\": 31025, \"pickup_eta_time\": null, \"add_charge_occur8\": null, \"invoice_period_end_date\": null, \"pickup_special_instructions1\": null, \"rate_buck_amt2\": null, \"pickup_special_instructions4\": null, \"manual_notepad\": false, \"edi_acknowledgement_required\": false, \"pickup_name\": \"BIG LOTS\", \"ordered_by_phone_number\": null, \"add_charge_amt12\": null, \"delivery_point_customer\": 31025, \"deliver_actual_dep_time\": \"08:15\", \"email_addresses\": null, \"pickup_address\": \"540 EASTPARK CT\", \"driver2\": null, \"signature_images\": [], \"rate_buck_amt11\": null, \"delivery_latitude\": 37.48366600, \"pickup_attention\": null, \"date_order_entered\": \"2021-07-08\", \"vehicle_type\": null, \"add_charge_amt9\": null, \"pickup_phone\": null, \"rate_miles\": null, \"customers_etrac_partner_id\": \"96609250\", \"order_type_text\": \"One way\", \"order_type\": \"O\", \"dl_arrive_notification_sent\": false, \"add_charge_code3\": null, \"etrac_number\": null, \"pickup_requested_arr_time\": \"07:00\", \"rate_buck_amt3\": null, \"pickup_actual_dep_time\": \"08:30\", \"line_items\": [], \"pickup_sign_req\": true, \"add_charge_code10\": null, \"deliver_city\": \"LANEXA\", \"fuel_plan\": null, \"add_charge_amt10\": null, \"roundtrip_actual_depart_time\": null, \"control_number\": 1872445, \"pickup_dispatch_zone\": null, \"send_new_order_alert\": false, \"settlements\": [{\"settlement_bucket4_pct\": null, \"charge1\": null, \"date_last_updated\": \"2021-07-08\", \"fuel_price_zone\": null, \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"charge4\": null, \"settlement_period_end_date\": null, \"charge5\": null, \"time_last_updated\": \"05:06\", \"charge6\": null, \"driver_number_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver_number\": 3208, \"control_number\": 1872445, \"settlement_bucket2_pct\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"voucher_date\": null, \"agent_etrac_transaction_number\": null, \"settlement_bucket5_pct\": null, \"record_type\": 0, \"voucher_number\": null, \"voucher_amount\": null, \"pay_chart_used\": null, \"settlement_pct\": 100.00, \"vendor_invoice_number\": null, \"settlement_bucket3_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"pre_book_percentage\": true, \"charge3\": null, \"settlement_bucket6_pct\": null, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"adjustment_type\": null, \"id\": \"002018724450D1\", \"agents_etrac_partner_id\": null, \"fuel_plan\": null, \"fuel_price_source\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"vendor_employee_numer\": null, \"settlement_bucket1_pct\": null, \"order_date\": \"2021-06-28\", \"charge2\": null}], \"deliver_actual_latitude\": null, \"fuel_price_zone\": null, \"verified_weight\": null, \"deliver_requested_dep_time\": \"17:00\", \"pickup_airport_code\": null, \"dispatch_time\": null, \"deliver_attention\": null, \"time_order_entered\": \"06:06\", \"rate_buck_amt4\": null, \"roundtrip_wait_time\": null, \"add_charge_amt2\": null, \"az_equip3\": null, \"progress\": [{\"status_date\": \"2021-07-08\", \"status_text\": \"Entered in carrier's system\", \"status_time\": \"06:06:00\"}, {\"status_date\": \"2021-06-28\", \"status_text\": \"Picked up\", \"status_time\": \"08:30:00\"}, {\"status_date\": \"2021-06-28\", \"status_text\": \"Delivered\", \"status_time\": \"08:15:00\"}], \"page_number\": 1, \"roundtrip_sign_req\": false, \"add_charge_amt1\": null, \"add_charge_code8\": null, \"weight\": null, \"rate_buck_amt6\": null, \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"bringg_send_sms\": false, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"custom_special_instr_long\": null, \"deliver_requested_arr_time\": \"08:00\", \"service_level_text\": \"Room of Choice\", \"service_level\": 55, \"az_equip1\": null, \"add_charge_code4\": null, \"bringg_order_id\": null, \"delivery_address_point_number_text\": \"JOSEPH FESSMAN\", \"delivery_address_point_number\": 26313, \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"deliver_special_instructions1\": null, \"pickup_wait_time\": null, \"add_charge_occur5\": null, \"push_partner_order_id\": null, \"deliver_route_sequence\": null, \"pickup_country\": null, \"pickup_state\": \"VA\", \"original_schedule_number\": null, \"frequent_caller_id\": null, \"distribution_unique_id\": 0, \"fuel_miles\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"rate_buck_amt5\": null, \"exception_sign_required\": false, \"pickup_route_code\": null, \"deliver_dispatch_zone\": null, \"delivery_longitude\": -76.90426400, \"pickup_pricing_zone\": 1, \"zone_set_used\": 1, \"deliver_special_instructions2\": null, \"add_charge_amt3\": null, \"deliver_phone\": null, \"pickup_email_notification_sent\": false, \"add_charge_occur12\": null, \"reference_text\": \"2125617401\", \"reference\": \"2125617401\", \"deliver_requested_date\": \"2021-06-28\", \"deliver_actual_longitude\": null, \"image_sign_req\": false, \"pickup_eta_date\": null, \"deliver_phone_ext\": null, \"pickup_omw_longitude\": null, \"original_ctrl_number\": null, \"pickup_special_instructions2\": null, \"order_automatically_quoted\": false, \"bol_number\": null, \"rate_buck_amt10\": 2.34, \"callback_time\": null, \"hazmat\": false, \"distribution_shift_id\": null, \"pickup_latitude\": 37.53250820, \"ordered_by\": \"RYDER\", \"insurance_amount\": null, \"cod_accept_cashiers_check\": false, \"add_charge_amt4\": null, \"add_charge_code7\": null, \"deliver_actual_pieces\": null, \"deliver_address\": \"15400 STAGE RD\", \"cod_accept_company_check\": false, \"signature\": \"SOF\", \"previous_ctrl_number\": null, \"deliver_zip\": \"23089\", \"deliver_special_instructions3\": null, \"rate_buck_amt7\": null, \"hist_inv_number\": 0, \"callback_date\": null, \"deliver_special_instr_long\": null, \"po_number\": null, \"pickup_actual_arr_time\": \"08:00\", \"pickup_requested_date\": \"2021-06-28\", \"number_of_pieces\": 2, \"dispatch_id\": null, \"photos_exist\": false, \"pickup_actual_latitude\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"id\": \"002018724450\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"del_actual_location_accuracy\": null, \"add_charge_occur7\": null, \"add_charge_occur9\": null, \"roundtrip_actual_latitude\": null, \"add_charge_occur6\": null, \"pickup_actual_longitude\": null, \"pickup_omw_timestamp\": null, \"bringg_last_loc_sent\": null, \"add_charge_code5\": null, \"deliver_country\": null, \"master_airway_bill_number\": null, \"pickup_route_seq\": null, \"roundtrip_signature\": null, \"calc_add_on_chgs\": false, \"deliver_actual_date\": \"2021-06-28\", \"cod_amount\": null, \"add_charge_code12\": null, \"rt_actual_location_accuracy\": null, \"rate_chart_used\": 0, \"pickup_longitude\": -77.33035820, \"pickup_signature\": \"SOF\", \"add_charge_amt5\": null, \"pu_arrive_notification_sent\": false, \"pickup_actual_date\": \"2021-06-28\", \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"push_services\": null, \"deliver_eta_date\": null, \"driver1_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver1\": 3208, \"deliver_omw_longitude\": null, \"deliver_wait_time\": null, \"pickup_room\": null, \"deliver_special_instructions4\": null, \"add_charge_amt7\": null, \"az_equip2\": null, \"hours\": \"15\", \"add_charge_code2\": null, \"exception_code\": null, \"roundtrip_actual_pieces\": null, \"rate_special_instructions\": null, \"roundtrip_actual_arrival_time\": null, \"add_charge_occur1\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"delivery_airport_code\": null, \"distribution_branch_id\": null, \"hist_inv_date\": null, \"add_charge_code1\": null, \"pickup_requested_dep_time\": \"09:00\", \"deliver_route_code\": null, \"roundtrip_actual_longitude\": null, \"pickup_city\": \"SANDSTON\", \"rate_buck_amt8\": null, \"pickup_omw_latitude\": null, \"deliver_omw_timestamp\": null, \"rate_buck_amt9\": null, \"deliver_room\": null, \"add_charge_code6\": null, \"add_charge_occur3\": null, \"blocks\": null, \"add_charge_code9\": null, \"actual_miles\": null, \"add_charge_occur10\": null, \"add_charge_code11\": null, \"pickup_address_point_number_text\": \"BIG LOTS\", \"pickup_address_point_number\": 19864, \"customer_name\": \"MXD/RYDER\", \"pu_actual_location_accuracy\": null, \"deliver_name\": \"JOSEPH FESSMAN\", \"add_charge_amt6\": null, \"signature_required\": true, \"csr\": \"DX*\", \"add_charge_amt8\": null, \"callback_to\": null, \"fuel_price_source\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"pickup_zip\": \"23150\", \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"return_svc_level\": null, \"add_charge_amt11\": null, \"add_charge_occur2\": null}}";
                                            //  objresponse.Reason = "{\"error\": \"Unable to perform API call object with RecordID:WS_OPORDR-9990111870 - The record may have been deleted.\", \"status\": \"error\", \"code\": \"U.RecordNotFound\"}";

                                            if (objresponse.ResponseVal)
                                            {
                                                strExecutionLogMessage = "OrderPostAPI Success " + System.Environment.NewLine;
                                                strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                               // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                //using (StreamWriter writer = new StreamWriter(filePath, true))
                                                //{
                                                //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                                //}

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
                                                            objIds.verified_weight = dt.Rows[i]["verified_weight"];
                                                            objIds.roundtrip_actual_latitude = dt.Rows[i]["roundtrip_actual_latitude"];
                                                            objIds.pickup_special_instructions4 = dt.Rows[i]["pickup_special_instructions4"];
                                                            objIds.fuel_miles = dt.Rows[i]["fuel_miles"];
                                                            objIds.pickup_dispatch_zone = dt.Rows[i]["pickup_dispatch_zone"];

                                                            objIds.pickup_zip = dt.Rows[i]["pickup_zip"];
                                                            objIds.pickup_actual_arr_time = dt.Rows[i]["pickup_actual_arr_time"];
                                                            objIds.cod_accept_company_check = dt.Rows[i]["cod_accept_company_check"];
                                                            objIds.add_charge_occur9 = dt.Rows[i]["add_charge_occur9"];
                                                            objIds.pickup_omw_latitude = dt.Rows[i]["pickup_omw_latitude"];
                                                            objIds.service_level_text = dt.Rows[i]["service_level_text"];
                                                            objIds.service_level = dt.Rows[i]["service_level"];
                                                            objIds.exception_sign_required = dt.Rows[i]["exception_sign_required"];
                                                            objIds.pickup_phone_ext = dt.Rows[i]["pickup_phone_ext"];
                                                            objIds.roundtrip_actual_pieces = dt.Rows[i]["roundtrip_actual_pieces"];
                                                            objIds.bringg_send_sms = dt.Rows[i]["bringg_send_sms"];
                                                            objIds.az_equip2 = dt.Rows[i]["az_equip2"];

                                                            objIds.hist_inv_date = dt.Rows[i]["hist_inv_date"];
                                                            objIds.date_order_entered = dt.Rows[i]["date_order_entered"];
                                                            objIds.powerpage_status_text = dt.Rows[i]["powerpage_status_text"];
                                                            objIds.powerpage_status = dt.Rows[i]["powerpage_status"];
                                                            objIds.pickup_city = dt.Rows[i]["pickup_city"];
                                                            objIds.pickup_phone = dt.Rows[i]["pickup_phone"];
                                                            objIds.pickup_sign_req = dt.Rows[i]["pickup_sign_req"];

                                                            objIds.deliver_phone = dt.Rows[i]["deliver_phone"];
                                                            objIds.deliver_omw_longitude = dt.Rows[i]["deliver_omw_longitude"];
                                                            objIds.roundtrip_actual_longitude = dt.Rows[i]["roundtrip_actual_longitude"];
                                                            objIds.page_number = dt.Rows[i]["page_number"];
                                                            objIds.order_type_text = dt.Rows[i]["order_type_text"];
                                                            objIds.order_type = dt.Rows[i]["order_type"];
                                                            objIds.add_charge_code9 = dt.Rows[i]["add_charge_code9"];
                                                            objIds.pickup_eta_time = dt.Rows[i]["pickup_eta_time"];

                                                            objIds.record_type = dt.Rows[i]["record_type"];
                                                            objIds.add_charge_occur11 = dt.Rows[i]["add_charge_occur11"];
                                                            objIds.push_partner_order_id = dt.Rows[i]["push_partner_order_id"];
                                                            objIds.deliver_country = dt.Rows[i]["deliver_country"];
                                                            objIds.customer_name = dt.Rows[i]["customer_name"];
                                                            objIds.bol_number = dt.Rows[i]["bol_number"];
                                                            objIds.pickup_latitude = dt.Rows[i]["pickup_latitude"];
                                                            objIds.add_charge_code4 = dt.Rows[i]["add_charge_code4"];

                                                            objIds.exception_order_action_text = dt.Rows[i]["exception_order_action_text"];
                                                            objIds.exception_order_action = dt.Rows[i]["exception_order_action"];
                                                            objIds.pu_arrive_notification_sent = dt.Rows[i]["pu_arrive_notification_sent"];
                                                            objIds.distribution_shift_id = dt.Rows[i]["distribution_shift_id"];
                                                            objIds.pickup_special_instr_long = dt.Rows[i]["pickup_special_instr_long"];
                                                            objIds.id = dt.Rows[i]["id"];
                                                            objIds.callback_to = dt.Rows[i]["callback_to"];
                                                            objIds.customer_number_text = dt.Rows[i]["customer_number_text"];

                                                            objIds.customer_number = dt.Rows[i]["customer_number"];
                                                            objIds.ordered_by = dt.Rows[i]["ordered_by"];
                                                            objIds.add_charge_code12 = dt.Rows[i]["add_charge_code12"];
                                                            objIds.pickup_route_seq = dt.Rows[i]["pickup_route_seq"];
                                                            objIds.deliver_city = dt.Rows[i]["deliver_city"];

                                                            objIds.add_charge_occur5 = dt.Rows[i]["add_charge_occur5"];
                                                            objIds.edi_acknowledgement_required = dt.Rows[i]["edi_acknowledgement_required"];
                                                            objIds.rescheduled_ctrl_number = dt.Rows[i]["rescheduled_ctrl_number"];
                                                            objIds.driver2 = dt.Rows[i]["driver2"];
                                                            objIds.deliver_room = dt.Rows[i]["deliver_room"];

                                                            objIds.deliver_actual_arr_time = dt.Rows[i]["deliver_actual_arr_time"];
                                                            objIds.fuel_price_zone = dt.Rows[i]["fuel_price_zone"];
                                                            objIds.add_charge_amt9 = dt.Rows[i]["add_charge_amt9"];
                                                            objIds.add_charge_amt4 = dt.Rows[i]["add_charge_amt4"];
                                                            objIds.delivery_address_point_number_text = dt.Rows[i]["delivery_address_point_number_text"];
                                                            objIds.delivery_address_point_number = dt.Rows[i]["delivery_address_point_number"];

                                                            objIds.deliver_actual_longitude = dt.Rows[i]["deliver_actual_longitude"];
                                                            objIds.add_charge_amt2 = dt.Rows[i]["add_charge_amt2"];
                                                            objIds.additional_drivers = dt.Rows[i]["additional_drivers"];
                                                            objIds.pickup_pricing_zone = dt.Rows[i]["pickup_pricing_zone"];
                                                            objIds.hazmat = dt.Rows[i]["hazmat"];
                                                            objIds.pickup_address = dt.Rows[i]["pickup_address"];
                                                            objIds.pickup_route_code = dt.Rows[i]["pickup_route_code"];
                                                            objIds.callback_userid = dt.Rows[i]["callback_userid"];
                                                            objIds.pickup_point_customer = dt.Rows[i]["pickup_point_customer"];

                                                            objIds.rate_buck_amt1 = dt.Rows[i]["rate_buck_amt1"];
                                                            objIds.add_charge_amt8 = dt.Rows[i]["add_charge_amt8"];
                                                            objIds.callback_time = dt.Rows[i]["callback_time"];
                                                            objIds.csr = dt.Rows[i]["csr"];
                                                            objIds.roundtrip_actual_depart_time = dt.Rows[i]["roundtrip_actual_depart_time"];
                                                            objIds.customers_etrac_partner_id = dt.Rows[i]["customers_etrac_partner_id"];
                                                            objIds.manual_notepad = dt.Rows[i]["manual_notepad"];
                                                            objIds.add_charge_code8 = dt.Rows[i]["add_charge_code8"];
                                                            objIds.bringg_order_id = dt.Rows[i]["bringg_order_id"];
                                                            objIds.deliver_omw_latitude = dt.Rows[i]["deliver_omw_latitude"];
                                                            objIds.pickup_longitude = dt.Rows[i]["pickup_longitude"];
                                                            objIds.etrac_number = dt.Rows[i]["etrac_number"];

                                                            objIds.distribution_unique_id = dt.Rows[i]["distribution_unique_id"];
                                                            objIds.vehicle_type = dt.Rows[i]["vehicle_type"];
                                                            objIds.roundtrip_actual_arrival_time = dt.Rows[i]["roundtrip_actual_arrival_time"];
                                                            objIds.delivery_longitude = dt.Rows[i]["delivery_longitude"];
                                                            objIds.pu_actual_location_accuracy = dt.Rows[i]["pu_actual_location_accuracy"];
                                                            objIds.deliver_actual_date = dt.Rows[i]["deliver_actual_date"];
                                                            objIds.exception_timestamp = dt.Rows[i]["exception_timestamp"];
                                                            objIds.deliver_zip = dt.Rows[i]["deliver_zip"];
                                                            objIds.roundtrip_wait_time = dt.Rows[i]["roundtrip_wait_time"];
                                                            objIds.add_charge_occur8 = dt.Rows[i]["add_charge_occur8"];
                                                            objIds.dl_arrive_notification_sent = dt.Rows[i]["dl_arrive_notification_sent"];
                                                            objIds.pickup_special_instructions1 = dt.Rows[i]["pickup_special_instructions1"];
                                                            objIds.ordered_by_phone_number = dt.Rows[i]["ordered_by_phone_number"];
                                                            objIds.deliver_requested_arr_time = dt.Rows[i]["deliver_requested_arr_time"];

                                                            objIds.rate_miles = dt.Rows[i]["rate_miles"];
                                                            objIds.holiday_groups = dt.Rows[i]["holiday_groups"];
                                                            objIds.pickup_email_notification_sent = dt.Rows[i]["pickup_email_notification_sent"];
                                                            objIds.add_charge_code3 = dt.Rows[i]["add_charge_code3"];
                                                            objIds.dispatch_id = dt.Rows[i]["dispatch_id"];
                                                            objIds.add_charge_occur10 = dt.Rows[i]["add_charge_occur10"];
                                                            objIds.dispatch_time = dt.Rows[i]["dispatch_time"];
                                                            objIds.deliver_wait_time = dt.Rows[i]["deliver_wait_time"];
                                                            objIds.invoice_period_end_date = dt.Rows[i]["invoice_period_end_date"];
                                                            objIds.add_charge_occur12 = dt.Rows[i]["add_charge_occur12"];

                                                            objIds.fuel_plan = dt.Rows[i]["fuel_plan"];
                                                            objIds.return_svc_level = dt.Rows[i]["return_svc_level"];
                                                            objIds.pickup_actual_date = dt.Rows[i]["pickup_actual_date"];
                                                            objIds.send_new_order_alert = dt.Rows[i]["send_new_order_alert"];
                                                            objIds.pickup_room = dt.Rows[i]["pickup_room"];
                                                            objIds.rate_buck_amt8 = dt.Rows[i]["rate_buck_amt8"];
                                                            objIds.add_charge_amt10 = dt.Rows[i]["add_charge_amt10"];
                                                            objIds.insurance_amount = dt.Rows[i]["insurance_amount"];
                                                            objIds.add_charge_amt3 = dt.Rows[i]["add_charge_amt3"];
                                                            objIds.add_charge_amt6 = dt.Rows[i]["add_charge_amt6"];
                                                            objIds.pickup_special_instructions3 = dt.Rows[i]["pickup_special_instructions3"];
                                                            objIds.pickup_requested_date = dt.Rows[i]["pickup_requested_date"];
                                                            objIds.roundtrip_sign_req = dt.Rows[i]["roundtrip_sign_req"];
                                                            objIds.actual_miles = dt.Rows[i]["actual_miles"];
                                                            objIds.pickup_address_point_number_text = dt.Rows[i]["pickup_address_point_number_text"];
                                                            objIds.pickup_address_point_number = dt.Rows[i]["pickup_address_point_number"];
                                                            objIds.deliver_actual_latitude = dt.Rows[i]["deliver_actual_latitude"];
                                                            objIds.deliver_phone_ext = dt.Rows[i]["deliver_phone_ext"];
                                                            objIds.deliver_route_code = dt.Rows[i]["deliver_route_code"];
                                                            objIds.add_charge_code10 = dt.Rows[i]["add_charge_code10"];
                                                            objIds.delivery_airport_code = dt.Rows[i]["delivery_airport_code"];

                                                            objIds.reference_text = dt.Rows[i]["reference_text"];
                                                            objIds.reference = dt.Rows[i]["reference"];
                                                            objIds.photos_exist = dt.Rows[i]["photos_exist"];
                                                            objIds.master_airway_bill_number = dt.Rows[i]["master_airway_bill_number"];
                                                            objIds.control_number = dt.Rows[i]["control_number"];
                                                            objIds.cod_text = dt.Rows[i]["cod_text"];
                                                            objIds.cod = dt.Rows[i]["cod"];
                                                            objIds.rate_buck_amt11 = dt.Rows[i]["rate_buck_amt11"];
                                                            objIds.pickup_omw_timestamp = dt.Rows[i]["pickup_omw_timestamp"];
                                                            objIds.deliver_special_instructions1 = dt.Rows[i]["deliver_special_instructions1"];
                                                            objIds.quote_amount = dt.Rows[i]["quote_amount"];
                                                            objIds.total_pages = dt.Rows[i]["total_pages"];
                                                            objIds.rate_buck_amt4 = dt.Rows[i]["rate_buck_amt4"];
                                                            objIds.delivery_latitude = dt.Rows[i]["delivery_latitude"];
                                                            objIds.add_charge_code1 = dt.Rows[i]["add_charge_code1"];


                                                            objIds.order_timeliness_text = dt.Rows[i]["order_timeliness_text"];
                                                            objIds.order_timeliness = dt.Rows[i]["order_timeliness"];
                                                            objIds.deliver_special_instr_long = dt.Rows[i]["deliver_special_instr_long"];
                                                            objIds.deliver_address = dt.Rows[i]["deliver_address"];
                                                            objIds.add_charge_occur4 = dt.Rows[i]["add_charge_occur4"];
                                                            objIds.deliver_eta_date = dt.Rows[i]["deliver_eta_date"];
                                                            objIds.pickup_actual_dep_time = dt.Rows[i]["pickup_actual_dep_time"];
                                                            objIds.deliver_requested_dep_time = dt.Rows[i]["deliver_requested_dep_time"];
                                                            objIds.deliver_actual_dep_time = dt.Rows[i]["deliver_actual_dep_time"];

                                                            objIds.bringg_last_loc_sent = dt.Rows[i]["bringg_last_loc_sent"];
                                                            objIds.az_equip3 = dt.Rows[i]["az_equip3"];
                                                            objIds.driver1_text = dt.Rows[i]["driver1_text"];
                                                            objIds.driver1 = dt.Rows[i]["driver1"];
                                                            objIds.pickup_actual_latitude = dt.Rows[i]["pickup_actual_latitude"];
                                                            objIds.add_charge_occur2 = dt.Rows[i]["add_charge_occur2"];
                                                            objIds.order_automatically_quoted = dt.Rows[i]["order_automatically_quoted"];
                                                            objIds.callback_required = dt.Rows[i]["callback_required_text"];
                                                            objIds.frequent_caller_id = dt.Rows[i]["frequent_caller_id"];
                                                            objIds.rate_buck_amt6 = dt.Rows[i]["rate_buck_amt6"];
                                                            objIds.rate_chart_used = dt.Rows[i]["rate_chart_used"];
                                                            objIds.deliver_actual_pieces = dt.Rows[i]["deliver_actual_pieces"];
                                                            objIds.add_charge_code5 = dt.Rows[i]["add_charge_code5"];
                                                            objIds.pickup_omw_longitude = dt.Rows[i]["pickup_omw_longitude"];
                                                            objIds.delivery_point_customer = dt.Rows[i]["delivery_point_customer"];
                                                            objIds.add_charge_occur7 = dt.Rows[i]["add_charge_occur7"];
                                                            objIds.rate_buck_amt5 = dt.Rows[i]["rate_buck_amt5"];
                                                            objIds.fuel_update_freq_text = dt.Rows[i]["fuel_update_freq_text"];
                                                            objIds.fuel_update_freq = dt.Rows[i]["fuel_update_freq"];
                                                            objIds.add_charge_code11 = dt.Rows[i]["add_charge_code11"];
                                                            objIds.pickup_name = dt.Rows[i]["pickup_name"];
                                                            objIds.callback_date = dt.Rows[i]["callback_date"];
                                                            objIds.add_charge_code2 = dt.Rows[i]["add_charge_code2"];
                                                            objIds.house_airway_bill_number = dt.Rows[i]["house_airway_bill_number"];
                                                            objIds.deliver_name = dt.Rows[i]["deliver_name"];
                                                            objIds.number_of_pieces = dt.Rows[i]["number_of_pieces"];
                                                            objIds.deliver_eta_time = dt.Rows[i]["deliver_eta_time"];
                                                            objIds.origin_code_text = dt.Rows[i]["origin_code_text"];
                                                            objIds.origin_code = dt.Rows[i]["origin_code"];
                                                            objIds.rate_special_instructions = dt.Rows[i]["rate_special_instructions"];
                                                            objIds.add_charge_occur3 = dt.Rows[i]["add_charge_occur3"];
                                                            objIds.pickup_eta_date = dt.Rows[i]["pickup_eta_date"];
                                                            objIds.deliver_special_instructions4 = dt.Rows[i]["deliver_special_instructions4"];
                                                            objIds.custom_special_instr_long = dt.Rows[i]["custom_special_instr_long"];
                                                            objIds.deliver_special_instructions2 = dt.Rows[i]["deliver_special_instructions2"];
                                                            objIds.pickup_signature = dt.Rows[i]["pickup_signature"];
                                                            objIds.az_equip1 = dt.Rows[i]["az_equip1"];
                                                            objIds.add_charge_amt12 = dt.Rows[i]["add_charge_amt12"];
                                                            objIds.calc_add_on_chgs = dt.Rows[i]["calc_add_on_chgs"];
                                                            objIds.original_schedule_number = dt.Rows[i]["original_schedule_number"];
                                                            objIds.blocks = dt.Rows[i]["blocks"];
                                                            objIds.del_actual_location_accuracy = dt.Rows[i]["del_actual_location_accuracy"];
                                                            objIds.zone_set_used = dt.Rows[i]["zone_set_used"];

                                                            objIds.pickup_country = dt.Rows[i]["pickup_country"];
                                                            objIds.pickup_state = dt.Rows[i]["pickup_state"];
                                                            objIds.add_charge_amt7 = dt.Rows[i]["add_charge_amt7"];
                                                            objIds.email_addresses = dt.Rows[i]["email_addresses"];
                                                            objIds.add_charge_occur1 = dt.Rows[i]["add_charge_occur1"];
                                                            objIds.pickup_wait_time = dt.Rows[i]["pickup_wait_time"];
                                                            objIds.company_number_text = dt.Rows[i]["company_number_text"];
                                                            objIds.company_number = dt.Rows[i]["company_number"];
                                                            objIds.distribution_branch_id = dt.Rows[i]["distribution_branch_id"];
                                                            objIds.rate_buck_amt9 = dt.Rows[i]["rate_buck_amt9"];
                                                            objIds.add_charge_amt1 = dt.Rows[i]["add_charge_amt1"];
                                                            objIds.pickup_requested_dep_time = dt.Rows[i]["pickup_requested_dep_time"];
                                                            objIds.customer_type_text = dt.Rows[i]["customer_type_text"];
                                                            objIds.customer_type = dt.Rows[i]["customer_type"];
                                                            objIds.deliver_state = dt.Rows[i]["deliver_state"];
                                                            objIds.deliver_dispatch_zone = dt.Rows[i]["deliver_dispatch_zone"];
                                                            objIds.image_sign_req = dt.Rows[i]["image_sign_req"];
                                                            objIds.add_charge_code6 = dt.Rows[i]["add_charge_code6"];
                                                            objIds.deliver_requested_date = dt.Rows[i]["deliver_requested_date"];
                                                            objIds.add_charge_amt5 = dt.Rows[i]["add_charge_amt5"];
                                                            objIds.time_order_entered = dt.Rows[i]["time_order_entered"];
                                                            objIds.pick_del_trans_flag_text = dt.Rows[i]["pick_del_trans_flag_text"];
                                                            objIds.pick_del_trans_flag = dt.Rows[i]["pick_del_trans_flag"];
                                                            objIds.pickup_attention = dt.Rows[i]["pickup_attention"];
                                                            objIds.rate_buck_amt7 = dt.Rows[i]["rate_buck_amt7"];
                                                            objIds.add_charge_occur6 = dt.Rows[i]["add_charge_occur6"];
                                                            objIds.fuel_price_source = dt.Rows[i]["fuel_price_source"];
                                                            objIds.pickup_airport_code = dt.Rows[i]["pickup_airport_code"];
                                                            objIds.rate_buck_amt2 = dt.Rows[i]["rate_buck_amt2"];
                                                            objIds.rate_buck_amt3 = dt.Rows[i]["rate_buck_amt3"];
                                                            objIds.deliver_omw_timestamp = dt.Rows[i]["deliver_omw_timestamp"];
                                                            objIds.exception_code = dt.Rows[i]["exception_code"];
                                                            objIds.status_code_text = dt.Rows[i]["status_code_text"];
                                                            objIds.status_code = dt.Rows[i]["status_code"];
                                                            objIds.weight = dt.Rows[i]["weight"];
                                                            objIds.signature_required = dt.Rows[i]["signature_required"];
                                                            objIds.rate_buck_amt10 = dt.Rows[i]["rate_buck_amt10"];
                                                            objIds.hist_inv_number = dt.Rows[i]["hist_inv_number"];
                                                            objIds.deliver_pricing_zone = dt.Rows[i]["deliver_pricing_zone"];
                                                            objIds.pickup_actual_longitude = dt.Rows[i]["pickup_actual_longitude"];
                                                            objIds.push_services = dt.Rows[i]["push_services"];
                                                            objIds.add_charge_amt11 = dt.Rows[i]["add_charge_amt11"];
                                                            objIds.rt_actual_location_accuracy = dt.Rows[i]["rt_actual_location_accuracy"];
                                                            objIds.roundtrip_actual_date = dt.Rows[i]["roundtrip_actual_date"];
                                                            objIds.pickup_requested_arr_time = dt.Rows[i]["pickup_requested_arr_time"];
                                                            objIds.deliver_attention = dt.Rows[i]["deliver_attention"];
                                                            objIds.deliver_special_instructions3 = dt.Rows[i]["deliver_special_instructions3"];
                                                            objIds.pickup_actual_pieces = dt.Rows[i]["pickup_actual_pieces"];
                                                            objIds.edi_order_accepted_or_rejected_text = dt.Rows[i]["edi_order_accepted_or_rejected_text"];
                                                            objIds.edi_order_accepted_or_rejected = dt.Rows[i]["edi_order_accepted_or_rejected"];
                                                            objIds.roundtrip_signature = dt.Rows[i]["roundtrip_signature"];
                                                            objIds.po_number = dt.Rows[i]["po_number"];
                                                            objIds.signature = dt.Rows[i]["signature"];
                                                            objIds.pickup_special_instructions2 = dt.Rows[i]["pickup_special_instructions2"];
                                                            objIds.original_ctrl_number = dt.Rows[i]["original_ctrl_number"];
                                                            objIds.previous_ctrl_number = dt.Rows[i]["previous_ctrl_number"];
                                                            objIds.id = dt.Rows[i]["Id"];
                                                            idList.Add(objIds);

                                                        }
                                                        objCommon.SaveOutputDataToCsvFile(idList, "Order-Create",
                                                           strInputFilePath, ReferenceId, strFileName, strDatetime);
                                                    }
                                                    if (dsOrderResponse.Tables.Contains("settlements"))
                                                    {
                                                        List<Settlement> settelmentList = new List<Settlement>();
                                                        for (int i = 0; i < dsOrderResponse.Tables["settlements"].Rows.Count; i++)
                                                        {
                                                            DataTable dt = dsOrderResponse.Tables["settlements"];
                                                            Settlement objsettlements = new Settlement();
                                                            objsettlements.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                            objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                            objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                            objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                            objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                            objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                            objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                            objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                            objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                            objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                            objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                            objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                            objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                            objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                            objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                            objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                            objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                            objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                            objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                            objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                            objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                            objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                            objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                            objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                            objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                            objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                            objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                            objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                            objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                            objsettlements.id = (dt.Rows[i]["id"]);
                                                            objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                            objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                            objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                            objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                            objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                            objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                            objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                            objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                            objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                            objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                            objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                            objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                            objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                            objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                            objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                            objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                            objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                            objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                            objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                            objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                            objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);
                                                            settelmentList.Add(objsettlements);
                                                        }

                                                        objCommon.SaveOutputDataToCsvFile(settelmentList, "Order-Settlements-AddRecord",
                                                           strInputFilePath, ReferenceId, strFileName, strDatetime);

                                                    }
                                                    if (dsOrderResponse.Tables.Contains("progress"))
                                                    {

                                                        List<Progress> progressList = new List<Progress>();
                                                        for (int i = 0; i < dsOrderResponse.Tables["progress"].Rows.Count; i++)
                                                        {
                                                            Progress progress = new Progress();
                                                            DataTable dt = dsOrderResponse.Tables["progress"];

                                                            progress.status_date = (dt.Rows[i]["status_date"]);
                                                            progress.status_text = (dt.Rows[i]["status_text"]);
                                                            progress.status_time = (dt.Rows[i]["status_time"]);
                                                            progress.id = (dt.Rows[i]["id"]);
                                                            progressList.Add(progress);
                                                        }

                                                        objCommon.SaveOutputDataToCsvFile(progressList, "Order-Progress",
                                                           strInputFilePath, ReferenceId, strFileName, strDatetime);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    strExecutionLogMessage = "ProcessAddOrderFiles Exception -" + ex.Message + System.Environment.NewLine;
                                                    strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                                    strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                                    strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                    //objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                                    objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                    //using (StreamWriter writer = new StreamWriter(filePath, true))
                                                    //{
                                                    //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                                    //}
                                                }

                                                if (driver1 != null)
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
                                                                strExecutionLogMessage += "Carrier Base Pay Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                                strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                                //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                                //using (StreamWriter writer = new StreamWriter(filePath, true))
                                                                //{
                                                                //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                                                //}
                                                                break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            strExecutionLogMessage = "OrderPost-OrderSettlementPut Error " + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Carrier Base Pay Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                            strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                            //using (StreamWriter writer = new StreamWriter(filePath, true))
                                                            //{
                                                            //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                                            //}
                                                            break;
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

                                                                //using (StreamWriter writer = new StreamWriter(filePath, true))
                                                                //{
                                                                //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                                                //}
                                                                break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            strExecutionLogMessage = "OrderPost-OrderSettlementPut Error " + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Carrier ACC Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                            strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                            //using (StreamWriter writer = new StreamWriter(filePath, true))
                                                            //{
                                                            //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                                            //}
                                                            break;
                                                        }


                                                        ordersettlementputrequest = @"{" + ordersettlementputrequest + "}";
                                                        string order_settlementObject = @"{'order_settlement': " + ordersettlementputrequest + "}";
                                                        JObject jsonobj = JObject.Parse(order_settlementObject);
                                                        request = jsonobj.ToString();

                                                        clsCommon.ReturnResponse objresponseOrdersettlement = new clsCommon.ReturnResponse();
                                                        objresponseOrdersettlement = objclsDatatrac.CallDataTracOrderSettlementPutAPI(UniqueId, order_settlementObject);

                                                        //objresponseOrdersettlement.Reason = "{\"002018724450D1\": {\"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"time_last_updated\": \"05:10\", \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"order_date\": \"2021-06-28\", \"agent_etrac_transaction_number\": null, \"settlement_bucket1_pct\": null, \"settlement_bucket2_pct\": null, \"vendor_employee_numer\": null, \"fuel_price_zone\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"charge1\": 35.91, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"adjustment_type\": null, \"agents_etrac_partner_id\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"vendor_invoice_number\": null, \"charge6\": null, \"settlement_bucket4_pct\": null, \"fuel_plan\": null, \"record_type\": 0, \"voucher_amount\": null, \"id\": \"002018724450D1\", \"date_last_updated\": \"2021-07-08\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"voucher_number\": null, \"driver_number_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver_number\": 3208, \"settlement_bucket6_pct\": null, \"settlement_pct\": 100.00, \"charge3\": null, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"voucher_date\": null, \"fuel_price_source\": null, \"pay_chart_used\": null, \"settlement_bucket5_pct\": null, \"charge2\": null, \"control_number\": 1872445, \"settlement_bucket3_pct\": null, \"charge5\": null, \"pre_book_percentage\": true, \"settlement_period_end_date\": null}}";
                                                        // objresponseOrdersettlement.ResponseVal = true;

                                                        if (objresponseOrdersettlement.ResponseVal)
                                                        {
                                                            strExecutionLogMessage = "OrderPost-OrderSettlementPutAPI Success " + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                            //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                                            //using (StreamWriter writer = new StreamWriter(filePath, true))
                                                            //{
                                                            //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                                            //}

                                                            DataSet dsOrderSettlementResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                                                            dsOrderSettlementResponse.Tables[0].TableName = "OrderSettlement";


                                                            List<ResponseOrderSettlements> orderSettlementstList = new List<ResponseOrderSettlements>();
                                                            for (int i = 0; i < dsOrderSettlementResponse.Tables["OrderSettlement"].Rows.Count; i++)
                                                            {
                                                                DataTable dt = dsOrderSettlementResponse.Tables["OrderSettlement"];
                                                                ResponseOrderSettlements objsettlements = new ResponseOrderSettlements();
                                                                objsettlements.company_number_text = dt.Rows[i]["company_number_text"];
                                                                objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                                objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                                objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                                objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                                objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                                objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                                objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                                objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                                objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                                objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                                objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                                objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                                objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                                objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                                objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                                objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                                objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                                objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                                objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                                objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                                objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                                objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                                objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                                objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                                objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                                objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                                objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                                objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                                objsettlements.id = (dt.Rows[i]["id"]);
                                                                objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                                objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                                objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                                objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                                objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                                objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                                objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                                objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                                objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                                objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                                objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                                objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                                objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                                objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                                objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                                objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                                objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                                objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                                objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                                objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                                objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);
                                                                orderSettlementstList.Add(objsettlements);
                                                            }

                                                            objCommon.SaveOutputDataToCsvFile(orderSettlementstList, "OrderSettlements-UpdatedRecord",
                                                               strInputFilePath, UniqueId, strFileName, strDatetime);
                                                        }
                                                        else
                                                        {
                                                            strExecutionLogMessage = "OrderPost-OrderSettlementPutAPI Failed " + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                            //using (StreamWriter writer = new StreamWriter(filePath, true))
                                                            //{
                                                            //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                                            //}

                                                            DataSet dsOrderPutFailureResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                                                            dsOrderPutFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                            dsOrderPutFailureResponse.Tables[0].Columns.Add("UniqueId", typeof(System.String));
                                                            foreach (DataRow row in dsOrderPutFailureResponse.Tables[0].Rows)
                                                            {
                                                                row["UniqueId"] = UniqueId;
                                                            }
                                                            objCommon.WriteDataToCsvFile(dsOrderPutFailureResponse.Tables[0],
                                                            strInputFilePath, UniqueId, strFileName, strDatetime);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                request = JsonConvert.SerializeObject(objorderdetails);
                                                strExecutionLogMessage = "OrderPostAPI Failed " + System.Environment.NewLine;
                                                strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                               // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                //using (StreamWriter writer = new StreamWriter(filePath, true))
                                                //{
                                                //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                                //}

                                                DataSet dsOrderFailureResponse = objCommon.jsonToDataSet(objresponse.Reason);
                                                dsOrderFailureResponse.Tables[0].TableName = "OrderFailure";
                                                dsOrderFailureResponse.Tables[0].Columns.Add("Customer Reference", typeof(System.String));
                                                foreach (DataRow row in dsOrderFailureResponse.Tables[0].Rows)
                                                {
                                                    row["Customer Reference"] = objOrder.reference;
                                                }
                                                objCommon.WriteDataToCsvFile(dsOrderFailureResponse.Tables[0],
                                            strInputFilePath, ReferenceId, processingFileName, strDatetime);

                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            strExecutionLogMessage = "ProcessAddOrderFiles Exception -" + ex.Message + System.Environment.NewLine;
                                            strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                            strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                            //objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                            //using (StreamWriter writer = new StreamWriter(filePath, true))
                                            //{
                                            //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                                            //}
                                        }
                                    }
                                }
                            });

                            // return;
                            //Parallel.ForEach(splitdt, td =>
                            //{

                            //    // dtDataTable = td;
                            //    //var dtDataTable = td;
                            //    // strExecutionLogFileLocation = strExecutionLogFileLocation + "\\" + td;


                            //    strExecutionLogMessage = "xlsx file found in directory ,  Table Name : " + td + "." + System.Environment.NewLine;
                            //    // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                            //    //  string filePath = objCommon.GetConfigValue("ExecutionLogFileLocation") + "\\" + dtDataTable + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                            //    // string filePath = objCommon.GetConfigValue("ExecutionLogFileLocation") + DateTime.Now.ToString("yyyyMMdd") + ".txt";

                            //    // string filePath = strExecutionLogFileLocation + @"\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                            //    string filePath = strExecutionLogFileLocation + @"\" + td + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                            //    //if (!File.Exists(filePath))
                            //    //    File.Create(filePath).Dispose();

                            //    //using (StreamWriter writer = new StreamWriter(filePath, true))
                            //    //{
                            //    //    writer.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                            //    //}

                            //    FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate);
                            //    StreamWriter str = new StreamWriter(fs);
                            //    str.BaseStream.Seek(0, SeekOrigin.End);
                            //    str.WriteLine("Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine + strExecutionLogMessage);
                            //    str.Flush();
                            //    str.Close();
                            //    fs.Close();


                            //});

                            // return;
                            //int j = 0;
                            //foreach (DataTable table in dsExcel.Tables)
                            //{
                            //    if (j == 1)
                            //    {
                            //        break;
                            //    }

                            //    j++;
                            //}

                            //  objCommon.MoveOutputFilesToOutputLocation(strInputFilePath);
                        }
                        else
                        {
                            strExecutionLogMessage = "Template sheet data not found for the file " + strInputFilePath + @"\" + strFileName;
                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                            // break;
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
                        // strExecutionLogMessage = "Getting ready to insert the Scanning Data into the WrkBillingLocation table ";
                        //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                        //DataSet dsLocationDetails = new DataSet();
                        //dsLocationDetails = clsExcelHelper.ImportExcelXLSX(objCommon.GetConfigValue("FutureMappingFile"), true, true);
                        //if (dsLocationDetails.Tables.Count > 0)
                        //{
                        //DataTable dtlocation = dsLocationDetails.Tables[0];

                        //DataRow[] drLocations = dtlocation.Select("Location ='" + strLocationFolder + "'");
                        //if (drLocations.Length > 0)
                        //{

                        DataSet dsExcel = new DataSet();
                        dsExcel = clsExcelHelper.ImportExcelXLSX(strInputFilePath + @"\" + strFileName, false);
                        if (dsExcel.Tables.Count > 0)
                        {

                            //  strExecutionLogMessage = "Getting ready to move the file to History Folder location at " + strBillingHistoryFileLocation;
                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                            objCommon.MoveTheFileToHistoryFolder(strBillingHistoryFileLocation, file);

                            // strExecutionLogMessage = "Completed call to MoveTheFileToHistoryFolder for the Scanning Data file to the history folder location at " + strBillingHistoryFileLocation;
                            //  objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                            strDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");
                            int j = 0;
                            foreach (DataTable table in dsExcel.Tables)
                            {
                                if (j == 1)
                                {
                                    break;
                                }

                                int rowindex = 1;
                                foreach (DataRow dr in table.Rows)
                                {

                                    object value = dr["Company"];
                                    if (value == DBNull.Value)
                                        break;
                                    //  ReferenceId = Convert.ToString(dr["Customer Reference"]);

                                    // Billing_Customer_Number = Convert.ToString(dr["Billing Customer Number"]);
                                    //bool validated = false;


                                    // string Company_no = Convert.ToString(dr["Company"]);

                                    // DataRow[] results = dtlocation.Select("Location ='" + strLocationFolder + "' AND Billing_Customer_Number = '" + Billing_Customer_Number + "' AND Company_Number = '" + Company_no + "'");

                                    //if (results.Length > 0)
                                    //{
                                    //    validated = true;
                                    //}
                                    //else
                                    //{
                                    //    strExecutionLogMessage = "Billing Customer Number/Company Number not found in the future mapping file " + strInputFilePath + @"\" + strFileName + ", Location" + strLocationFolder;
                                    //    strExecutionLogMessage += "For Billing Customer Number is  -" + Billing_Customer_Number + System.Environment.NewLine;
                                    //    objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                    //}
                                    //if (validated)
                                    //{
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
                                                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "OrderPut Error " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Company Number Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            break;
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
                                                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "OrderPut Error " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Control Number Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            break;
                                        }

                                        int company_number = Convert.ToInt32(dr["Company"]);
                                        int control_number = Convert.ToInt32(dr["Control Number"]);
                                        UniqueId = objclsDatatrac.GenerateUniqueNumber(company_number, control_number);
                                        //objOrder.service_level = Convert.ToInt32(dr["Service Type"]);
                                        if (dr.Table.Columns.Contains("Service Type"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Service Type"])))
                                            {
                                                orderputrequest = orderputrequest + @"'service_level': " + Convert.ToInt32(dr["Service Type"]) + ",";
                                            }
                                        }


                                        //  objOrder.customer_number = Convert.ToInt32(dsLocationDetails.Tables[0].Rows[0]["Billing Customer Number"]);

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
                                        if (dr.Table.Columns.Contains("Store Code"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Store Code"])))
                                            {
                                                orderputrequest = orderputrequest + @"'pickup_name': '" + Convert.ToString(dr["Store Code"]) + "',";
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
                                                orderputrequest = orderputrequest + @"'pickup_zip': '" + Convert.ToString(dr["Pickup zip/postal code"]) + "',";
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
                                                orderputrequest = orderputrequest + @"'deliver_zip': '" + Convert.ToString(dr["Zip"]) + "',";
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


                                        orderputrequest = @"{" + orderputrequest + "}";

                                        string orderObject = @"{'order': " + orderputrequest + "}";


                                        JObject jsonobj = JObject.Parse(orderObject);
                                        string request = jsonobj.ToString();
                                        clsCommon.ReturnResponse objresponse = new clsCommon.ReturnResponse();
                                        objresponse = objclsDatatrac.CallDataTracOrderPutAPI(UniqueId, orderObject);
                                        //objresponse.Reason = "{\"error\": \"Unable to perform API call object with RecordID:WS_OPORDR-9990111870 - The record may have been deleted.\", \"status\": \"error\", \"code\": \"U.RecordNotFound\"}";
                                        //objresponse.ResponseVal = false;
                                        // objresponse.Reason = "{\"002018716980\": {\"csr\": \"DX*\", \"cod_text\": \"No\", \"cod\": \"N\", \"edi_acknowledgement_required\": false, \"control_number\": 1871698, \"deliver_requested_arr_time\": \"08:00\", \"return_svc_level\": null, \"pickup_phone_ext\": null, \"customers_etrac_partner_id\": \"96609250\", \"distribution_unique_id\": 0, \"custom_special_instr_long\": null, \"exception_sign_required\": false, \"po_number\": null, \"exception_code\": null, \"add_charge_occur4\": null, \"frequent_caller_id\": null, \"push_services\": null, \"pickup_omw_longitude\": null, \"deliver_special_instructions2\": null, \"deliver_actual_dep_time\": \"08:15\", \"house_airway_bill_number\": null, \"invoice_period_end_date\": null, \"line_items\": [], \"pickup_eta_date\": null, \"dl_arrive_notification_sent\": false, \"hist_inv_date\": null, \"add_charge_occur6\": null, \"add_charge_code12\": null, \"add_charge_occur5\": null, \"pickup_route_seq\": null, \"add_charge_code9\": null, \"callback_time\": null, \"add_charge_amt12\": null, \"add_charge_occur3\": null, \"deliver_eta_date\": null, \"ordered_by\": \"RYDER\", \"deliver_actual_latitude\": null, \"pickup_airport_code\": null, \"rate_buck_amt9\": null, \"deliver_pricing_zone\": 1, \"add_charge_code4\": null, \"rate_buck_amt5\": null, \"total_pages\": 1, \"roundtrip_actual_arrival_time\": null, \"pickup_requested_date\": \"2021-05-10\", \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"pickup_wait_time\": null, \"pickup_special_instructions4\": null, \"deliver_special_instructions1\": null, \"deliver_room\": null, \"roundtrip_actual_date\": null, \"rescheduled_ctrl_number\": null, \"insurance_amount\": null, \"deliver_city\": \"STAFFORD\", \"progress\": [{\"status_text\": \"Entered in carrier's system\", \"status_time\": \"12:27:00\", \"status_date\": \"2021-07-05\"}, {\"status_text\": \"Picked up\", \"status_time\": \"08:30:00\", \"status_date\": \"2021-05-10\"}, {\"status_text\": \"Delivered\", \"status_time\": \"08:15:00\", \"status_date\": \"2021-05-10\"}], \"deliver_eta_time\": null, \"rate_buck_amt2\": null, \"previous_ctrl_number\": null, \"pickup_city\": \"SANDSTON\", \"pickup_special_instructions1\": null, \"deliver_route_sequence\": null, \"pickup_actual_pieces\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"signature_required\": true, \"deliver_actual_date\": \"2021-05-10\", \"callback_userid\": null, \"pickup_requested_dep_time\": \"09:00\", \"cod_accept_cashiers_check\": false, \"signature_images\": [], \"add_charge_amt8\": null, \"add_charge_amt3\": null, \"pickup_zip\": \"23150\", \"original_ctrl_number\": null, \"calc_add_on_chgs\": false, \"original_schedule_number\": null, \"email_addresses\": null, \"pickup_actual_dep_time\": \"08:30\", \"pickup_special_instructions3\": null, \"etrac_number\": \"026-1k19-1h0-q08-z93\", \"fuel_plan\": null, \"pickup_address_point_number_text\": \"HUMAN TOUCH\", \"pickup_address_point_number\": 19891, \"pickup_latitude\": 37.53250820, \"add_charge_amt5\": null, \"verified_weight\": null, \"pickup_sign_req\": true, \"exception_timestamp\": null, \"add_charge_occur10\": null, \"deliver_special_instructions3\": null, \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"add_charge_code1\": null, \"deliver_name\": \"MICHAEL BROWN\", \"az_equip2\": null, \"rate_buck_amt3\": null, \"bringg_send_sms\": false, \"pickup_actual_date\": \"2021-05-10\", \"page_number\": 1, \"pickup_signature\": \"SOF\", \"bringg_order_id\": null, \"pu_arrive_notification_sent\": false, \"roundtrip_actual_pieces\": null, \"pickup_actual_latitude\": null, \"manual_notepad\": false, \"roundtrip_sign_req\": false, \"deliver_zip\": \"22554\", \"rate_buck_amt8\": null, \"add_charge_occur11\": null, \"holiday_groups\": null, \"delivery_latitude\": 38.37859180, \"rate_buck_amt4\": null, \"pickup_point_customer\": 31025, \"rate_miles\": null, \"pickup_special_instructions2\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"bol_number\": null, \"rate_chart_used\": 0, \"rate_buck_amt6\": null, \"add_charge_occur1\": null, \"deliver_phone\": null, \"rate_buck_amt11\": null, \"deliver_actual_pieces\": null, \"deliver_attention\": null, \"driver1_text\": \"LAVERT KENDALL MORRIS\", \"driver1\": 3001, \"zone_set_used\": 1, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"add_charge_amt4\": null, \"pickup_pricing_zone\": 1, \"pickup_country\": null, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"deliver_omw_timestamp\": null, \"weight\": null, \"fuel_price_source\": null, \"pickup_omw_timestamp\": null, \"add_charge_code6\": null, \"photos_exist\": false, \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"pickup_room\": null, \"deliver_omw_latitude\": null, \"callback_date\": null, \"actual_miles\": null, \"delivery_longitude\": -77.31366340, \"image_sign_req\": false, \"additional_drivers\": false, \"pickup_attention\": null, \"reference_text\": \"DAYA1\", \"reference\": \"DAYA1\", \"roundtrip_wait_time\": null, \"add_charge_code11\": null, \"pickup_special_instr_long\": null, \"add_charge_code3\": null, \"add_charge_amt1\": null, \"callback_to\": null, \"date_order_entered\": \"2021-07-05\", \"rate_special_instructions\": null, \"hist_inv_number\": 0, \"roundtrip_signature\": null, \"bringg_last_loc_sent\": null, \"pickup_route_code\": null, \"pickup_requested_arr_time\": \"07:00\", \"deliver_address\": \"134 CANTERBURY DR\", \"roundtrip_actual_longitude\": null, \"add_charge_occur8\": null, \"delivery_airport_code\": null, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"add_charge_code10\": null, \"customer_name\": \"MXD/RYDER\", \"rate_buck_amt1\": 80.00, \"delivery_address_point_number_text\": \"MICHAEL BROWN\", \"delivery_address_point_number\": 25113, \"cod_amount\": null, \"roundtrip_actual_depart_time\": null, \"pickup_dispatch_zone\": null, \"service_level_text\": \"White Glove Delivery\", \"service_level\": 58, \"deliver_actual_longitude\": null, \"pickup_actual_arr_time\": \"08:00\", \"add_charge_amt6\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"pickup_longitude\": -77.33035820, \"driver2\": null, \"distribution_branch_id\": null, \"add_charge_amt9\": null, \"add_charge_code8\": null, \"blocks\": null, \"hazmat\": false, \"add_charge_code7\": null, \"deliver_requested_dep_time\": \"17:00\", \"signature\": \"SOF\", \"master_airway_bill_number\": null, \"cod_accept_company_check\": false, \"delivery_point_customer\": 31025, \"add_charge_occur2\": null, \"quote_amount\": null, \"add_charge_code2\": null, \"deliver_requested_date\": \"2021-05-10\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"record_type\": 0, \"deliver_special_instr_long\": null, \"push_partner_order_id\": null, \"pickup_address\": \"540 EASTPARK CT\", \"add_charge_occur9\": null, \"distribution_shift_id\": null, \"settlements\": [{\"voucher_amount\": null, \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"settlement_bucket6_pct\": null, \"fuel_price_zone\": null, \"settlement_period_end_date\": null, \"driver_number_text\": \"LAVERT KENDALL MORRIS\", \"driver_number\": 3001, \"fuel_price_source\": null, \"settlement_bucket3_pct\": null, \"agent_etrac_transaction_number\": null, \"voucher_date\": null, \"charge6\": null, \"settlement_bucket2_pct\": null, \"charge4\": null, \"pay_chart_used\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"adjustment_type\": null, \"voucher_number\": null, \"charge5\": null, \"charge2\": null, \"time_last_updated\": \"11:52\", \"settlement_bucket1_pct\": null, \"settlement_bucket5_pct\": null, \"settlement_bucket4_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"settlement_pct\": 100.00, \"pre_book_percentage\": true, \"date_last_updated\": \"2021-07-05\", \"vendor_invoice_number\": null, \"control_number\": 1871698, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"fuel_plan\": null, \"charge3\": null, \"vendor_employee_numer\": null, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"charge1\": null, \"order_date\": \"2021-05-10\", \"id\": \"002018716980D1\", \"agents_etrac_partner_id\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"record_type\": 0}], \"hours\": \"15\", \"pu_actual_location_accuracy\": null, \"fuel_price_zone\": null, \"rt_actual_location_accuracy\": null, \"deliver_phone_ext\": null, \"vehicle_type\": null, \"del_actual_location_accuracy\": null, \"ordered_by_phone_number\": null, \"deliver_country\": null, \"az_equip3\": null, \"add_charge_amt7\": null, \"add_charge_amt10\": null, \"notes\": [{\"user_id\": \"DX*\", \"note_line\": \"** Driver #1: 0 -> 3001\", \"control_number\": 1871698, \"note_code\": \"11\", \"id\": \"00201871698020210705125237DX*\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"entry_date\": \"2021-07-05\", \"print_on_ticket\": false, \"show_to_cust\": false, \"entry_time\": \"12:52:37\"}, {\"user_id\": \"DX*\", \"note_line\": \"** Driver #1 setl%: .00% -> 100.00%\", \"control_number\": 1871698, \"note_code\": \" 6\", \"id\": \"00201871698020210705125238DX*\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"entry_date\": \"2021-07-05\", \"print_on_ticket\": false, \"show_to_cust\": false, \"entry_time\": \"12:52:38\"}], \"pickup_eta_time\": null, \"deliver_state\": \"VA\", \"add_charge_code5\": null, \"deliver_wait_time\": null, \"pickup_omw_latitude\": null, \"fuel_miles\": null, \"add_charge_occur12\": null, \"deliver_dispatch_zone\": null, \"rate_buck_amt10\": 2.16, \"order_automatically_quoted\": false, \"deliver_actual_arr_time\": \"08:00\", \"deliver_special_instructions4\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"az_equip1\": null, \"time_order_entered\": \"12:27\", \"rate_buck_amt7\": null, \"roundtrip_actual_latitude\": null, \"add_charge_amt2\": null, \"add_charge_occur7\": null, \"pickup_email_notification_sent\": false, \"dispatch_time\": null, \"pickup_actual_longitude\": null, \"add_charge_amt11\": null, \"pickup_name\": \"HUMAN TOUCH\", \"dispatch_id\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"send_new_order_alert\": false, \"id\": \"002018716980\", \"deliver_omw_longitude\": null, \"pickup_state\": \"VA\", \"deliver_route_code\": null, \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"pickup_phone\": null, \"number_of_pieces\": 1}}";
                                        // objresponse.ResponseVal = true;
                                        if (objresponse.ResponseVal)
                                        {
                                            strExecutionLogMessage = "OrderPut API Success " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                            strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
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
                                                        objIds.verified_weight = dt.Rows[i]["verified_weight"];
                                                        objIds.roundtrip_actual_latitude = dt.Rows[i]["roundtrip_actual_latitude"];
                                                        objIds.pickup_special_instructions4 = dt.Rows[i]["pickup_special_instructions4"];
                                                        objIds.fuel_miles = dt.Rows[i]["fuel_miles"];
                                                        objIds.pickup_dispatch_zone = dt.Rows[i]["pickup_dispatch_zone"];

                                                        objIds.pickup_zip = dt.Rows[i]["pickup_zip"];
                                                        objIds.pickup_actual_arr_time = dt.Rows[i]["pickup_actual_arr_time"];
                                                        objIds.cod_accept_company_check = dt.Rows[i]["cod_accept_company_check"];
                                                        objIds.add_charge_occur9 = dt.Rows[i]["add_charge_occur9"];
                                                        objIds.pickup_omw_latitude = dt.Rows[i]["pickup_omw_latitude"];
                                                        objIds.service_level_text = dt.Rows[i]["service_level_text"];
                                                        objIds.service_level = dt.Rows[i]["service_level"];
                                                        objIds.exception_sign_required = dt.Rows[i]["exception_sign_required"];
                                                        objIds.pickup_phone_ext = dt.Rows[i]["pickup_phone_ext"];
                                                        objIds.roundtrip_actual_pieces = dt.Rows[i]["roundtrip_actual_pieces"];
                                                        objIds.bringg_send_sms = dt.Rows[i]["bringg_send_sms"];
                                                        objIds.az_equip2 = dt.Rows[i]["az_equip2"];

                                                        objIds.hist_inv_date = dt.Rows[i]["hist_inv_date"];
                                                        objIds.date_order_entered = dt.Rows[i]["date_order_entered"];
                                                        objIds.powerpage_status_text = dt.Rows[i]["powerpage_status_text"];
                                                        objIds.powerpage_status = dt.Rows[i]["powerpage_status"];
                                                        objIds.pickup_city = dt.Rows[i]["pickup_city"];
                                                        objIds.pickup_phone = dt.Rows[i]["pickup_phone"];
                                                        objIds.pickup_sign_req = dt.Rows[i]["pickup_sign_req"];

                                                        objIds.deliver_phone = dt.Rows[i]["deliver_phone"];
                                                        objIds.deliver_omw_longitude = dt.Rows[i]["deliver_omw_longitude"];
                                                        objIds.roundtrip_actual_longitude = dt.Rows[i]["roundtrip_actual_longitude"];
                                                        objIds.page_number = dt.Rows[i]["page_number"];
                                                        objIds.order_type_text = dt.Rows[i]["order_type_text"];
                                                        objIds.order_type = dt.Rows[i]["order_type"];
                                                        objIds.add_charge_code9 = dt.Rows[i]["add_charge_code9"];
                                                        objIds.pickup_eta_time = dt.Rows[i]["pickup_eta_time"];

                                                        objIds.record_type = dt.Rows[i]["record_type"];
                                                        objIds.add_charge_occur11 = dt.Rows[i]["add_charge_occur11"];
                                                        objIds.push_partner_order_id = dt.Rows[i]["push_partner_order_id"];
                                                        objIds.deliver_country = dt.Rows[i]["deliver_country"];
                                                        objIds.customer_name = dt.Rows[i]["customer_name"];
                                                        objIds.bol_number = dt.Rows[i]["bol_number"];
                                                        objIds.pickup_latitude = dt.Rows[i]["pickup_latitude"];
                                                        objIds.add_charge_code4 = dt.Rows[i]["add_charge_code4"];

                                                        objIds.exception_order_action_text = dt.Rows[i]["exception_order_action_text"];
                                                        objIds.exception_order_action = dt.Rows[i]["exception_order_action"];
                                                        objIds.pu_arrive_notification_sent = dt.Rows[i]["pu_arrive_notification_sent"];
                                                        objIds.distribution_shift_id = dt.Rows[i]["distribution_shift_id"];
                                                        objIds.pickup_special_instr_long = dt.Rows[i]["pickup_special_instr_long"];
                                                        objIds.id = dt.Rows[i]["id"];
                                                        objIds.callback_to = dt.Rows[i]["callback_to"];
                                                        objIds.customer_number_text = dt.Rows[i]["customer_number_text"];

                                                        objIds.customer_number = dt.Rows[i]["customer_number"];
                                                        objIds.ordered_by = dt.Rows[i]["ordered_by"];
                                                        objIds.add_charge_code12 = dt.Rows[i]["add_charge_code12"];
                                                        objIds.pickup_route_seq = dt.Rows[i]["pickup_route_seq"];
                                                        objIds.deliver_city = dt.Rows[i]["deliver_city"];

                                                        objIds.add_charge_occur5 = dt.Rows[i]["add_charge_occur5"];
                                                        objIds.edi_acknowledgement_required = dt.Rows[i]["edi_acknowledgement_required"];
                                                        objIds.rescheduled_ctrl_number = dt.Rows[i]["rescheduled_ctrl_number"];
                                                        objIds.driver2 = dt.Rows[i]["driver2"];
                                                        objIds.deliver_room = dt.Rows[i]["deliver_room"];

                                                        objIds.deliver_actual_arr_time = dt.Rows[i]["deliver_actual_arr_time"];
                                                        objIds.fuel_price_zone = dt.Rows[i]["fuel_price_zone"];
                                                        objIds.add_charge_amt9 = dt.Rows[i]["add_charge_amt9"];
                                                        objIds.add_charge_amt4 = dt.Rows[i]["add_charge_amt4"];
                                                        objIds.delivery_address_point_number_text = dt.Rows[i]["delivery_address_point_number_text"];
                                                        objIds.delivery_address_point_number = dt.Rows[i]["delivery_address_point_number"];

                                                        objIds.deliver_actual_longitude = dt.Rows[i]["deliver_actual_longitude"];
                                                        objIds.add_charge_amt2 = dt.Rows[i]["add_charge_amt2"];
                                                        objIds.additional_drivers = dt.Rows[i]["additional_drivers"];
                                                        objIds.pickup_pricing_zone = dt.Rows[i]["pickup_pricing_zone"];
                                                        objIds.hazmat = dt.Rows[i]["hazmat"];
                                                        objIds.pickup_address = dt.Rows[i]["pickup_address"];
                                                        objIds.pickup_route_code = dt.Rows[i]["pickup_route_code"];
                                                        objIds.callback_userid = dt.Rows[i]["callback_userid"];
                                                        objIds.pickup_point_customer = dt.Rows[i]["pickup_point_customer"];

                                                        objIds.rate_buck_amt1 = dt.Rows[i]["rate_buck_amt1"];
                                                        objIds.add_charge_amt8 = dt.Rows[i]["add_charge_amt8"];
                                                        objIds.callback_time = dt.Rows[i]["callback_time"];
                                                        objIds.csr = dt.Rows[i]["csr"];
                                                        objIds.roundtrip_actual_depart_time = dt.Rows[i]["roundtrip_actual_depart_time"];
                                                        objIds.customers_etrac_partner_id = dt.Rows[i]["customers_etrac_partner_id"];
                                                        objIds.manual_notepad = dt.Rows[i]["manual_notepad"];
                                                        objIds.add_charge_code8 = dt.Rows[i]["add_charge_code8"];
                                                        objIds.bringg_order_id = dt.Rows[i]["bringg_order_id"];
                                                        objIds.deliver_omw_latitude = dt.Rows[i]["deliver_omw_latitude"];
                                                        objIds.pickup_longitude = dt.Rows[i]["pickup_longitude"];
                                                        objIds.etrac_number = dt.Rows[i]["etrac_number"];

                                                        objIds.distribution_unique_id = dt.Rows[i]["distribution_unique_id"];
                                                        objIds.vehicle_type = dt.Rows[i]["vehicle_type"];
                                                        objIds.roundtrip_actual_arrival_time = dt.Rows[i]["roundtrip_actual_arrival_time"];
                                                        objIds.delivery_longitude = dt.Rows[i]["delivery_longitude"];
                                                        objIds.pu_actual_location_accuracy = dt.Rows[i]["pu_actual_location_accuracy"];
                                                        objIds.deliver_actual_date = dt.Rows[i]["deliver_actual_date"];
                                                        objIds.exception_timestamp = dt.Rows[i]["exception_timestamp"];
                                                        objIds.deliver_zip = dt.Rows[i]["deliver_zip"];
                                                        objIds.roundtrip_wait_time = dt.Rows[i]["roundtrip_wait_time"];
                                                        objIds.add_charge_occur8 = dt.Rows[i]["add_charge_occur8"];
                                                        objIds.dl_arrive_notification_sent = dt.Rows[i]["dl_arrive_notification_sent"];
                                                        objIds.pickup_special_instructions1 = dt.Rows[i]["pickup_special_instructions1"];
                                                        objIds.ordered_by_phone_number = dt.Rows[i]["ordered_by_phone_number"];
                                                        objIds.deliver_requested_arr_time = dt.Rows[i]["deliver_requested_arr_time"];

                                                        objIds.rate_miles = dt.Rows[i]["rate_miles"];
                                                        objIds.holiday_groups = dt.Rows[i]["holiday_groups"];
                                                        objIds.pickup_email_notification_sent = dt.Rows[i]["pickup_email_notification_sent"];
                                                        objIds.add_charge_code3 = dt.Rows[i]["add_charge_code3"];
                                                        objIds.dispatch_id = dt.Rows[i]["dispatch_id"];
                                                        objIds.add_charge_occur10 = dt.Rows[i]["add_charge_occur10"];
                                                        objIds.dispatch_time = dt.Rows[i]["dispatch_time"];
                                                        objIds.deliver_wait_time = dt.Rows[i]["deliver_wait_time"];
                                                        objIds.invoice_period_end_date = dt.Rows[i]["invoice_period_end_date"];
                                                        objIds.add_charge_occur12 = dt.Rows[i]["add_charge_occur12"];

                                                        objIds.fuel_plan = dt.Rows[i]["fuel_plan"];
                                                        objIds.return_svc_level = dt.Rows[i]["return_svc_level"];
                                                        objIds.pickup_actual_date = dt.Rows[i]["pickup_actual_date"];
                                                        objIds.send_new_order_alert = dt.Rows[i]["send_new_order_alert"];
                                                        objIds.pickup_room = dt.Rows[i]["pickup_room"];
                                                        objIds.rate_buck_amt8 = dt.Rows[i]["rate_buck_amt8"];
                                                        objIds.add_charge_amt10 = dt.Rows[i]["add_charge_amt10"];
                                                        objIds.insurance_amount = dt.Rows[i]["insurance_amount"];
                                                        objIds.add_charge_amt3 = dt.Rows[i]["add_charge_amt3"];
                                                        objIds.add_charge_amt6 = dt.Rows[i]["add_charge_amt6"];
                                                        objIds.pickup_special_instructions3 = dt.Rows[i]["pickup_special_instructions3"];
                                                        objIds.pickup_requested_date = dt.Rows[i]["pickup_requested_date"];
                                                        objIds.roundtrip_sign_req = dt.Rows[i]["roundtrip_sign_req"];
                                                        objIds.actual_miles = dt.Rows[i]["actual_miles"];
                                                        objIds.pickup_address_point_number_text = dt.Rows[i]["pickup_address_point_number_text"];
                                                        objIds.pickup_address_point_number = dt.Rows[i]["pickup_address_point_number"];
                                                        objIds.deliver_actual_latitude = dt.Rows[i]["deliver_actual_latitude"];
                                                        objIds.deliver_phone_ext = dt.Rows[i]["deliver_phone_ext"];
                                                        objIds.deliver_route_code = dt.Rows[i]["deliver_route_code"];
                                                        objIds.add_charge_code10 = dt.Rows[i]["add_charge_code10"];
                                                        objIds.delivery_airport_code = dt.Rows[i]["delivery_airport_code"];

                                                        objIds.reference_text = dt.Rows[i]["reference_text"];
                                                        objIds.reference = dt.Rows[i]["reference"];
                                                        objIds.photos_exist = dt.Rows[i]["photos_exist"];
                                                        objIds.master_airway_bill_number = dt.Rows[i]["master_airway_bill_number"];
                                                        objIds.control_number = dt.Rows[i]["control_number"];
                                                        objIds.cod_text = dt.Rows[i]["cod_text"];
                                                        objIds.cod = dt.Rows[i]["cod"];
                                                        objIds.rate_buck_amt11 = dt.Rows[i]["rate_buck_amt11"];
                                                        objIds.pickup_omw_timestamp = dt.Rows[i]["pickup_omw_timestamp"];
                                                        objIds.deliver_special_instructions1 = dt.Rows[i]["deliver_special_instructions1"];
                                                        objIds.quote_amount = dt.Rows[i]["quote_amount"];
                                                        objIds.total_pages = dt.Rows[i]["total_pages"];
                                                        objIds.rate_buck_amt4 = dt.Rows[i]["rate_buck_amt4"];
                                                        objIds.delivery_latitude = dt.Rows[i]["delivery_latitude"];
                                                        objIds.add_charge_code1 = dt.Rows[i]["add_charge_code1"];


                                                        objIds.order_timeliness_text = dt.Rows[i]["order_timeliness_text"];
                                                        objIds.order_timeliness = dt.Rows[i]["order_timeliness"];
                                                        objIds.deliver_special_instr_long = dt.Rows[i]["deliver_special_instr_long"];
                                                        objIds.deliver_address = dt.Rows[i]["deliver_address"];
                                                        objIds.add_charge_occur4 = dt.Rows[i]["add_charge_occur4"];
                                                        objIds.deliver_eta_date = dt.Rows[i]["deliver_eta_date"];
                                                        objIds.pickup_actual_dep_time = dt.Rows[i]["pickup_actual_dep_time"];
                                                        objIds.deliver_requested_dep_time = dt.Rows[i]["deliver_requested_dep_time"];
                                                        objIds.deliver_actual_dep_time = dt.Rows[i]["deliver_actual_dep_time"];

                                                        objIds.bringg_last_loc_sent = dt.Rows[i]["bringg_last_loc_sent"];
                                                        objIds.az_equip3 = dt.Rows[i]["az_equip3"];
                                                        objIds.driver1_text = dt.Rows[i]["driver1_text"];
                                                        objIds.driver1 = dt.Rows[i]["driver1"];
                                                        objIds.pickup_actual_latitude = dt.Rows[i]["pickup_actual_latitude"];
                                                        objIds.add_charge_occur2 = dt.Rows[i]["add_charge_occur2"];
                                                        objIds.order_automatically_quoted = dt.Rows[i]["order_automatically_quoted"];
                                                        objIds.callback_required = dt.Rows[i]["callback_required_text"];
                                                        objIds.frequent_caller_id = dt.Rows[i]["frequent_caller_id"];
                                                        objIds.rate_buck_amt6 = dt.Rows[i]["rate_buck_amt6"];
                                                        objIds.rate_chart_used = dt.Rows[i]["rate_chart_used"];
                                                        objIds.deliver_actual_pieces = dt.Rows[i]["deliver_actual_pieces"];
                                                        objIds.add_charge_code5 = dt.Rows[i]["add_charge_code5"];
                                                        objIds.pickup_omw_longitude = dt.Rows[i]["pickup_omw_longitude"];
                                                        objIds.delivery_point_customer = dt.Rows[i]["delivery_point_customer"];
                                                        objIds.add_charge_occur7 = dt.Rows[i]["add_charge_occur7"];
                                                        objIds.rate_buck_amt5 = dt.Rows[i]["rate_buck_amt5"];
                                                        objIds.fuel_update_freq_text = dt.Rows[i]["fuel_update_freq_text"];
                                                        objIds.fuel_update_freq = dt.Rows[i]["fuel_update_freq"];
                                                        objIds.add_charge_code11 = dt.Rows[i]["add_charge_code11"];
                                                        objIds.pickup_name = dt.Rows[i]["pickup_name"];
                                                        objIds.callback_date = dt.Rows[i]["callback_date"];
                                                        objIds.add_charge_code2 = dt.Rows[i]["add_charge_code2"];
                                                        objIds.house_airway_bill_number = dt.Rows[i]["house_airway_bill_number"];
                                                        objIds.deliver_name = dt.Rows[i]["deliver_name"];
                                                        objIds.number_of_pieces = dt.Rows[i]["number_of_pieces"];
                                                        objIds.deliver_eta_time = dt.Rows[i]["deliver_eta_time"];
                                                        objIds.origin_code_text = dt.Rows[i]["origin_code_text"];
                                                        objIds.origin_code = dt.Rows[i]["origin_code"];
                                                        objIds.rate_special_instructions = dt.Rows[i]["rate_special_instructions"];
                                                        objIds.add_charge_occur3 = dt.Rows[i]["add_charge_occur3"];
                                                        objIds.pickup_eta_date = dt.Rows[i]["pickup_eta_date"];
                                                        objIds.deliver_special_instructions4 = dt.Rows[i]["deliver_special_instructions4"];
                                                        objIds.custom_special_instr_long = dt.Rows[i]["custom_special_instr_long"];
                                                        objIds.deliver_special_instructions2 = dt.Rows[i]["deliver_special_instructions2"];
                                                        objIds.pickup_signature = dt.Rows[i]["pickup_signature"];
                                                        objIds.az_equip1 = dt.Rows[i]["az_equip1"];
                                                        objIds.add_charge_amt12 = dt.Rows[i]["add_charge_amt12"];
                                                        objIds.calc_add_on_chgs = dt.Rows[i]["calc_add_on_chgs"];
                                                        objIds.original_schedule_number = dt.Rows[i]["original_schedule_number"];
                                                        objIds.blocks = dt.Rows[i]["blocks"];
                                                        objIds.del_actual_location_accuracy = dt.Rows[i]["del_actual_location_accuracy"];
                                                        objIds.zone_set_used = dt.Rows[i]["zone_set_used"];

                                                        objIds.pickup_country = dt.Rows[i]["pickup_country"];
                                                        objIds.pickup_state = dt.Rows[i]["pickup_state"];
                                                        objIds.add_charge_amt7 = dt.Rows[i]["add_charge_amt7"];
                                                        objIds.email_addresses = dt.Rows[i]["email_addresses"];
                                                        objIds.add_charge_occur1 = dt.Rows[i]["add_charge_occur1"];
                                                        objIds.pickup_wait_time = dt.Rows[i]["pickup_wait_time"];
                                                        objIds.company_number_text = dt.Rows[i]["company_number_text"];
                                                        objIds.company_number = dt.Rows[i]["company_number"];
                                                        objIds.distribution_branch_id = dt.Rows[i]["distribution_branch_id"];
                                                        objIds.rate_buck_amt9 = dt.Rows[i]["rate_buck_amt9"];
                                                        objIds.add_charge_amt1 = dt.Rows[i]["add_charge_amt1"];
                                                        objIds.pickup_requested_dep_time = dt.Rows[i]["pickup_requested_dep_time"];
                                                        objIds.customer_type_text = dt.Rows[i]["customer_type_text"];
                                                        objIds.customer_type = dt.Rows[i]["customer_type"];
                                                        objIds.deliver_state = dt.Rows[i]["deliver_state"];
                                                        objIds.deliver_dispatch_zone = dt.Rows[i]["deliver_dispatch_zone"];
                                                        objIds.image_sign_req = dt.Rows[i]["image_sign_req"];
                                                        objIds.add_charge_code6 = dt.Rows[i]["add_charge_code6"];
                                                        objIds.deliver_requested_date = dt.Rows[i]["deliver_requested_date"];
                                                        objIds.add_charge_amt5 = dt.Rows[i]["add_charge_amt5"];
                                                        objIds.time_order_entered = dt.Rows[i]["time_order_entered"];
                                                        objIds.pick_del_trans_flag_text = dt.Rows[i]["pick_del_trans_flag_text"];
                                                        objIds.pick_del_trans_flag = dt.Rows[i]["pick_del_trans_flag"];
                                                        objIds.pickup_attention = dt.Rows[i]["pickup_attention"];
                                                        objIds.rate_buck_amt7 = dt.Rows[i]["rate_buck_amt7"];
                                                        objIds.add_charge_occur6 = dt.Rows[i]["add_charge_occur6"];
                                                        objIds.fuel_price_source = dt.Rows[i]["fuel_price_source"];
                                                        objIds.pickup_airport_code = dt.Rows[i]["pickup_airport_code"];
                                                        objIds.rate_buck_amt2 = dt.Rows[i]["rate_buck_amt2"];
                                                        objIds.rate_buck_amt3 = dt.Rows[i]["rate_buck_amt3"];
                                                        objIds.deliver_omw_timestamp = dt.Rows[i]["deliver_omw_timestamp"];
                                                        objIds.exception_code = dt.Rows[i]["exception_code"];
                                                        objIds.status_code_text = dt.Rows[i]["status_code_text"];
                                                        objIds.status_code = dt.Rows[i]["status_code"];
                                                        objIds.weight = dt.Rows[i]["weight"];
                                                        objIds.signature_required = dt.Rows[i]["signature_required"];
                                                        objIds.rate_buck_amt10 = dt.Rows[i]["rate_buck_amt10"];
                                                        objIds.hist_inv_number = dt.Rows[i]["hist_inv_number"];
                                                        objIds.deliver_pricing_zone = dt.Rows[i]["deliver_pricing_zone"];
                                                        objIds.pickup_actual_longitude = dt.Rows[i]["pickup_actual_longitude"];
                                                        objIds.push_services = dt.Rows[i]["push_services"];
                                                        objIds.add_charge_amt11 = dt.Rows[i]["add_charge_amt11"];
                                                        objIds.rt_actual_location_accuracy = dt.Rows[i]["rt_actual_location_accuracy"];
                                                        objIds.roundtrip_actual_date = dt.Rows[i]["roundtrip_actual_date"];
                                                        objIds.pickup_requested_arr_time = dt.Rows[i]["pickup_requested_arr_time"];
                                                        objIds.deliver_attention = dt.Rows[i]["deliver_attention"];
                                                        objIds.deliver_special_instructions3 = dt.Rows[i]["deliver_special_instructions3"];
                                                        objIds.pickup_actual_pieces = dt.Rows[i]["pickup_actual_pieces"];
                                                        objIds.edi_order_accepted_or_rejected_text = dt.Rows[i]["edi_order_accepted_or_rejected_text"];
                                                        objIds.edi_order_accepted_or_rejected = dt.Rows[i]["edi_order_accepted_or_rejected"];
                                                        objIds.roundtrip_signature = dt.Rows[i]["roundtrip_signature"];
                                                        objIds.po_number = dt.Rows[i]["po_number"];
                                                        objIds.signature = dt.Rows[i]["signature"];
                                                        objIds.pickup_special_instructions2 = dt.Rows[i]["pickup_special_instructions2"];
                                                        objIds.original_ctrl_number = dt.Rows[i]["original_ctrl_number"];
                                                        objIds.previous_ctrl_number = dt.Rows[i]["previous_ctrl_number"];
                                                        objIds.id = dt.Rows[i]["Id"];
                                                        idList.Add(objIds);

                                                    }

                                                    objCommon.SaveOutputDataToCsvFile(idList, "OrderPut-Create",
                                                       strInputFilePath, UniqueId, strFileName, strDatetime);

                                                }
                                                if (dsOrderPutResponse.Tables.Contains("settlements"))
                                                {
                                                    List<Settlement> settelmentList = new List<Settlement>();
                                                    for (int i = 0; i < dsOrderPutResponse.Tables["settlements"].Rows.Count; i++)
                                                    {
                                                        DataTable dt = dsOrderPutResponse.Tables["settlements"];
                                                        Settlement objsettlements = new Settlement();
                                                        objsettlements.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                        objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                        objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                        objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                        objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                        objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                        objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                        objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                        objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                        objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                        objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                        objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                        objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                        objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                        objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                        objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                        objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                        objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                        objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                        objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                        objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                        objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                        objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                        objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                        objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                        objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                        objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                        objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                        objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                        objsettlements.id = (dt.Rows[i]["id"]);
                                                        objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                        objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                        objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                        objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                        objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                        objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                        objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                        objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                        objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                        objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                        objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                        objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                        objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                        objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                        objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                        objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                        objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                        objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                        objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                        objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                        objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);
                                                        settelmentList.Add(objsettlements);
                                                    }

                                                    objCommon.SaveOutputDataToCsvFile(settelmentList, "OrderPut-Settlements",
                                                       strInputFilePath, UniqueId, strFileName, strDatetime);

                                                }
                                                if (dsOrderPutResponse.Tables.Contains("progress"))
                                                {

                                                    List<Progress> progressList = new List<Progress>();
                                                    for (int i = 0; i < dsOrderPutResponse.Tables["progress"].Rows.Count; i++)
                                                    {
                                                        Progress progress = new Progress();
                                                        DataTable dt = dsOrderPutResponse.Tables["progress"];

                                                        progress.status_date = (dt.Rows[i]["status_date"]);
                                                        progress.status_text = (dt.Rows[i]["status_text"]);
                                                        progress.status_time = (dt.Rows[i]["status_time"]);
                                                        progress.id = (dt.Rows[i]["id"]);
                                                        progressList.Add(progress);
                                                    }

                                                    objCommon.SaveOutputDataToCsvFile(progressList, "OrderPut-Progress",
                                                       strInputFilePath, UniqueId, strFileName, strDatetime);


                                                }
                                                if (dsOrderPutResponse.Tables.Contains("notes"))
                                                {

                                                    List<Note> noteList = new List<Note>();
                                                    for (int i = 0; i < dsOrderPutResponse.Tables["notes"].Rows.Count; i++)
                                                    {
                                                        Note note = new Note();
                                                        DataTable dt = dsOrderPutResponse.Tables["notes"];

                                                        note.user_id = (dt.Rows[i]["user_id"]);
                                                        note.note_line = (dt.Rows[i]["note_line"]);
                                                        note.control_number = (dt.Rows[i]["control_number"]);
                                                        note.note_code = (dt.Rows[i]["note_code"]);
                                                        note.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                        note.company_number = (dt.Rows[i]["company_number"]);
                                                        note.entry_date = (dt.Rows[i]["entry_date"]);
                                                        note.print_on_ticket = (dt.Rows[i]["print_on_ticket"]);
                                                        note.show_to_cust = (dt.Rows[i]["show_to_cust"]);
                                                        note.entry_time = (dt.Rows[i]["entry_time"]);
                                                        note.id = (dt.Rows[i]["id"]);
                                                        noteList.Add(note);
                                                    }

                                                    objCommon.SaveOutputDataToCsvFile(noteList, "OrderPut-Note",
                                                       strInputFilePath, UniqueId, strFileName, strDatetime);

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
                                                objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                            }
                                            if (driver1 != null)
                                            {
                                                if (dsOrderPutResponse.Tables.Contains("settlements"))
                                                {
                                                    UniqueId = Convert.ToString(dsOrderPutResponse.Tables["settlements"].Rows[0]["id"]);

                                                    string ordersettlementputrequest = null;

                                                    company_number = Convert.ToInt32(dsOrderPutResponse.Tables["settlements"].Rows[0]["company_number"]);
                                                    control_number = Convert.ToInt32(dsOrderPutResponse.Tables["settlements"].Rows[0]["control_number"]);

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
                                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        strExecutionLogMessage = "OrderPut-OrderSettlementPut Error " + System.Environment.NewLine;
                                                        strExecutionLogMessage += "Carrier Base Pay Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                        strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                                        strExecutionLogMessage += "And For Unique Id -" + UniqueId + System.Environment.NewLine;
                                                        objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                        break;
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
                                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                            break;

                                                        }
                                                    }
                                                    else
                                                    {
                                                        strExecutionLogMessage = "OrderPut-OrderSettlementPut Error " + System.Environment.NewLine;
                                                        strExecutionLogMessage += "Carrier ACC Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                        strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                                        strExecutionLogMessage += "And For Unique Id -" + UniqueId + System.Environment.NewLine;
                                                        objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                        break;
                                                    }

                                                    ordersettlementputrequest = @"{" + ordersettlementputrequest + "}";
                                                    string order_settlementObject = @"{'order_settlement': " + ordersettlementputrequest + "}";
                                                    jsonobj = JObject.Parse(order_settlementObject);
                                                    request = jsonobj.ToString();

                                                    clsCommon.ReturnResponse objresponseOrdersettlement = new clsCommon.ReturnResponse();
                                                    objresponseOrdersettlement = objclsDatatrac.CallDataTracOrderSettlementPutAPI(UniqueId, order_settlementObject);
                                                    // objresponseOrdersettlement.Reason = "{\"002018724450D1\": {\"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"time_last_updated\": \"05:10\", \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"order_date\": \"2021-06-28\", \"agent_etrac_transaction_number\": null, \"settlement_bucket1_pct\": null, \"settlement_bucket2_pct\": null, \"vendor_employee_numer\": null, \"fuel_price_zone\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"charge1\": 35.91, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"adjustment_type\": null, \"agents_etrac_partner_id\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"vendor_invoice_number\": null, \"charge6\": null, \"settlement_bucket4_pct\": null, \"fuel_plan\": null, \"record_type\": 0, \"voucher_amount\": null, \"id\": \"002018724450D1\", \"date_last_updated\": \"2021-07-08\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"voucher_number\": null, \"driver_number_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver_number\": 3208, \"settlement_bucket6_pct\": null, \"settlement_pct\": 100.00, \"charge3\": null, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"voucher_date\": null, \"fuel_price_source\": null, \"pay_chart_used\": null, \"settlement_bucket5_pct\": null, \"charge2\": null, \"control_number\": 1872445, \"settlement_bucket3_pct\": null, \"charge5\": null, \"pre_book_percentage\": true, \"settlement_period_end_date\": null}}";
                                                    //bjresponseOrdersettlement.ResponseVal = true;
                                                    if (objresponseOrdersettlement.ResponseVal)
                                                    {
                                                        // request = JsonConvert.SerializeObject(objresponseOrdersettlement);
                                                        strExecutionLogMessage = "OrderPut-OrderSettlementPutAPI Success " + System.Environment.NewLine;
                                                        strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                        strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                                        objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                        DataSet dsOrderSettlementResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                                                        dsOrderSettlementResponse.Tables[0].TableName = "OrderSettlement";


                                                        List<ResponseOrderSettlements> orderSettlementstList = new List<ResponseOrderSettlements>();
                                                        for (int i = 0; i < dsOrderSettlementResponse.Tables["OrderSettlement"].Rows.Count; i++)
                                                        {
                                                            DataTable dt = dsOrderSettlementResponse.Tables["OrderSettlement"];
                                                            ResponseOrderSettlements objsettlements = new ResponseOrderSettlements();
                                                            objsettlements.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                            objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                            objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                            objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                            objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                            objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                            objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                            objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                            objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                            objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                            objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                            objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                            objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                            objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                            objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                            objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                            objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                            objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                            objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                            objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                            objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                            objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                            objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                            objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                            objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                            objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                            objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                            objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                            objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                            objsettlements.id = (dt.Rows[i]["id"]);
                                                            objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                            objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                            objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                            objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                            objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                            objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                            objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                            objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                            objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                            objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                            objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                            objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                            objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                            objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                            objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                            objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                            objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                            objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                            objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                            objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                            objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);
                                                            orderSettlementstList.Add(objsettlements);
                                                        }

                                                        objCommon.SaveOutputDataToCsvFile(orderSettlementstList, "OrderSettlement",
                                                           strInputFilePath, UniqueId, strFileName, strDatetime);
                                                    }
                                                    else
                                                    {
                                                        strExecutionLogMessage = "OrderPut-OrderSettlementPutAPI Failed " + System.Environment.NewLine;
                                                        strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                        strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                                        objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                        DataSet dsOrderPutFailureResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                                                        dsOrderPutFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                        dsOrderPutFailureResponse.Tables[0].Columns.Add("UniqueId", typeof(System.String));
                                                        foreach (DataRow row in dsOrderPutFailureResponse.Tables[0].Rows)
                                                        {
                                                            row["UniqueId"] = UniqueId;
                                                        }
                                                        objCommon.WriteDataToCsvFile(dsOrderPutFailureResponse.Tables[0],
                                                    strInputFilePath, UniqueId, strFileName, strDatetime);

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

                                            DataSet dsOrderFailureResponse = objCommon.jsonToDataSet(objresponse.Reason);
                                            dsOrderFailureResponse.Tables[0].TableName = "OrderPutFailure";
                                            dsOrderFailureResponse.Tables[0].Columns.Add("UniqueId", typeof(System.String));
                                            foreach (DataRow row in dsOrderFailureResponse.Tables[0].Rows)
                                            {
                                                row["UniqueId"] = UniqueId;
                                            }
                                            objCommon.WriteDataToCsvFile(dsOrderFailureResponse.Tables[0],
                                        strInputFilePath, UniqueId, strFileName, strDatetime);

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

                            //  strExecutionLogMessage = "Getting ready to move the file to History Folder location at " + strBillingHistoryFileLocation;
                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                            objCommon.MoveTheFileToHistoryFolder(strBillingHistoryFileLocation, file);

                            // strExecutionLogMessage = "Completed call to MoveTheFileToHistoryFolder for the Scanning Data file to the history folder location at " + strBillingHistoryFileLocation;
                            //  objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                            strDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");
                            int j = 0;
                            foreach (DataTable table in dsExcel.Tables)
                            {
                                if (j == 1)
                                {
                                    break;
                                }
                                var rowindex = 1;
                                foreach (DataRow dr in table.Rows)
                                {

                                    object value = dr["Company"];
                                    if (value == DBNull.Value)
                                        break;
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
                                                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "OrderSettlementPutAPI Error " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Company Number Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            break;
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
                                                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "OrderSettlementPutAPI Error " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Control Number Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For row number -" + rowindex + System.Environment.NewLine;

                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            break;
                                        }

                                        int company_number = Convert.ToInt32(dr["Company"]);
                                        int control_number = Convert.ToInt32(dr["Control Number"]);

                                        string OrderSettlement_UniqueIdSuffix = Convert.ToString(dr["OrderSettlement_UniqueIdSuffix"]);
                                        UniqueId = objclsDatatrac.GenerateUniqueNumber(company_number, control_number, OrderSettlement_UniqueIdSuffix);

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
                                                strExecutionLogMessage += "For Unique  Reference -" + UniqueId + System.Environment.NewLine;
                                                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "OrderSettlementPut Error " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Carrier Base Pay Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For Unique Reference -" + UniqueId + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            break;
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
                                                strExecutionLogMessage += "For Unique Reference -" + UniqueId + System.Environment.NewLine;
                                                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "OrderSettlementPut Error " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Carrier ACC Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For Unique Reference -" + UniqueId + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            break;
                                        }

                                        ordersettlementputrequest = @"{" + ordersettlementputrequest + "}";
                                        string order_settlementObject = @"{'order_settlement': " + ordersettlementputrequest + "}";
                                        JObject jsonobj = JObject.Parse(order_settlementObject);
                                        string request = jsonobj.ToString();

                                        clsCommon.ReturnResponse objresponseOrdersettlement = new clsCommon.ReturnResponse();
                                        objresponseOrdersettlement = objclsDatatrac.CallDataTracOrderSettlementPutAPI(UniqueId, order_settlementObject);

                                        // objresponseOrdersettlement.Reason = "{\"002018724450D1\": {\"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"time_last_updated\": \"05:10\", \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"order_date\": \"2021-06-28\", \"agent_etrac_transaction_number\": null, \"settlement_bucket1_pct\": null, \"settlement_bucket2_pct\": null, \"vendor_employee_numer\": null, \"fuel_price_zone\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"charge1\": 35.91, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"adjustment_type\": null, \"agents_etrac_partner_id\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"vendor_invoice_number\": null, \"charge6\": null, \"settlement_bucket4_pct\": null, \"fuel_plan\": null, \"record_type\": 0, \"voucher_amount\": null, \"id\": \"002018724450D1\", \"date_last_updated\": \"2021-07-08\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"voucher_number\": null, \"driver_number_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver_number\": 3208, \"settlement_bucket6_pct\": null, \"settlement_pct\": 100.00, \"charge3\": null, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"voucher_date\": null, \"fuel_price_source\": null, \"pay_chart_used\": null, \"settlement_bucket5_pct\": null, \"charge2\": null, \"control_number\": 1872445, \"settlement_bucket3_pct\": null, \"charge5\": null, \"pre_book_percentage\": true, \"settlement_period_end_date\": null}}";
                                        // objresponseOrdersettlement.ResponseVal = true;

                                        //objresponseOrdersettlement.Reason = "{\"error\": \"Unable to perform API call object with RecordID:WS_OPORDR-9990111870 - The record may have been deleted.\", \"status\": \"error\", \"code\": \"U.RecordNotFound\"}";
                                        //objresponseOrdersettlement.ResponseVal = false;
                                        if (objresponseOrdersettlement.ResponseVal)
                                        {
                                            // request = JsonConvert.SerializeObject(objresponseOrdersettlement);
                                            strExecutionLogMessage = "OrderSettlementPutAPI Success " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                            strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            DataSet dsOrderSettlementResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                                            dsOrderSettlementResponse.Tables[0].TableName = "OrderSettlement";

                                            List<ResponseOrderSettlements> orderSettlementstList = new List<ResponseOrderSettlements>();
                                            for (int i = 0; i < dsOrderSettlementResponse.Tables["OrderSettlement"].Rows.Count; i++)
                                            {
                                                DataTable dt = dsOrderSettlementResponse.Tables["OrderSettlement"];
                                                ResponseOrderSettlements objsettlements = new ResponseOrderSettlements();
                                                objsettlements.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                objsettlements.id = (dt.Rows[i]["id"]);
                                                objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);
                                                orderSettlementstList.Add(objsettlements);
                                            }

                                            objCommon.SaveOutputDataToCsvFile(orderSettlementstList, "OrderSettlement",
                                               strInputFilePath, UniqueId, strFileName, strDatetime);
                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "OrderSettlementPutAPI Failed " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                            strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            DataSet dsOrderPutFailureResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                                            dsOrderPutFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                            dsOrderPutFailureResponse.Tables[0].Columns.Add("UniqueId", typeof(System.String));
                                            foreach (DataRow row in dsOrderPutFailureResponse.Tables[0].Rows)
                                            {
                                                row["UniqueId"] = UniqueId;
                                            }
                                            objCommon.WriteDataToCsvFile(dsOrderPutFailureResponse.Tables[0],
                                        strInputFilePath, UniqueId, strFileName, strDatetime);

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

                strExecutionLogMessage = "Processing the Add Header data for " + strInputFilePath;
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

                                    ReferenceId = Convert.ToString(dr["Customer Reference"]);

                                    try
                                    {
                                        route_headerdetails objroute_headerdetails = new route_headerdetails();
                                        route_header objheader = new route_header();

                                        objheader.actual_billing_amount = Convert.ToDouble(dr["actual_billing_amount"]);
                                        objheader.actual_cost_allocation = Convert.ToDouble(dr["actual_cost_allocation"]);
                                        objheader.actual_driver_agent = Convert.ToInt32(dr["actual_driver_agent"]);
                                        objheader.actual_miles = Convert.ToInt32(dr["actual_miles"]);
                                        objheader.actual_settlement_amount = Convert.ToDouble(dr["actual_settlement_amount"]);
                                        objheader.actual_stops = Convert.ToInt32(dr["actual_stops"]);
                                        objheader.actual_total_pieces = Convert.ToInt32(dr["actual_total_pieces"]);
                                        objheader.actual_total_weight = Convert.ToInt32(dr["actual_total_weight"]);
                                        objheader.actual_vehicle = Convert.ToString(dr["actual_vehicle"]);
                                        objheader.amazon_order_number = Convert.ToInt32(dr["amazon_order_number"]);
                                        objheader.assigned_driver_agent = Convert.ToInt32(dr["assigned_driver_agent"]);
                                        objheader.assigned_vehicle = Convert.ToString(dr["assigned_vehicle"]);
                                        objheader.az_equip1 = Convert.ToInt32(dr["az_equip1"]);
                                        objheader.az_equip2 = Convert.ToInt32(dr["az_equip2"]);
                                        objheader.az_equip3 = Convert.ToInt32(dr["az_equip3"]);
                                        objheader.billing_level = Convert.ToString(dr["billing_level"]);
                                        objheader.billing_method = Convert.ToString(dr["billing_method"]);
                                        objheader.break_time = Convert.ToString(dr["break_time"]);
                                        objheader.calc_eta = Convert.ToBoolean(dr["calc_eta"]);

                                        objheader.close_time = Convert.ToString(dr["close_time"]);

                                        objheader.created_by = Convert.ToString(dr["created_by"]);

                                        objheader.created_date = Convert.ToString(dr["created_date"]);

                                        objheader.created_time = Convert.ToString(dr["created_time"]);

                                        objheader.dispatcher_id = Convert.ToString(dr["dispatcher_id"]);
                                        objheader.end_date = Convert.ToString(dr["end_date"]);
                                        objheader.end_location = Convert.ToString(dr["end_location"]);
                                        objheader.end_time = Convert.ToString(dr["end_time"]);
                                        objheader.ending_location = Convert.ToString(dr["ending_location"]);
                                        objheader.expire_time = Convert.ToString(dr["expire_time"]);
                                        objheader.hours = Convert.ToString(dr["hours"]);
                                        objheader.labor_allocation_method = Convert.ToString(dr["labor_allocation_method"]);
                                        objheader.labor_cost = Convert.ToDouble(dr["labor_cost"]);
                                        objheader.last_updated_by = Convert.ToString(dr["last_updated_by"]);
                                        objheader.last_updated_date = Convert.ToString(dr["last_updated_date"]);
                                        objheader.last_updated_time = Convert.ToString(dr["last_updated_time"]);
                                        objheader.miles = Convert.ToInt32(dr["miles"]);
                                        //  objheader.notes = Convert.ToString(dr["notes"]);
                                        objheader.open_time = Convert.ToString(dr["open_time"]);
                                        objheader.overhead_allocation_method = Convert.ToString(dr["overhead_allocation_method"]);
                                        objheader.overhead_cost = Convert.ToDouble(dr["overhead_cost"]);
                                        objheader.per_stop_billing_amount = Convert.ToDouble(dr["per_stop_billing_amount"]);
                                        objheader.per_stop_settlement_amount = Convert.ToDouble(dr["per_stop_settlement_amount"]);
                                        objheader.posted_by = Convert.ToString(dr["posted_by"]);

                                        objheader.posted_date = Convert.ToString(dr["posted_date"]);

                                        objheader.posted_status = Convert.ToInt32(dr["posted_status"]);
                                        objheader.posted_time = Convert.ToString(dr["posted_time"]);
                                        objheader.push_services = Convert.ToString(dr["push_services"]);
                                        objheader.route_addon_amount = Convert.ToDouble(dr["route_addon_amount"]);
                                        objheader.route_closed = Convert.ToInt32(dr["route_closed"]);
                                        objheader.route_comments = Convert.ToString(dr["route_comments"]);
                                        objheader.route_late_start_code = Convert.ToString(dr["route_late_start_code"]);
                                        objheader.route_service_method = Convert.ToString(dr["route_service_method"]);
                                        // objheader.route_stops = Convert.ToString(dr["route_stops"]);
                                        objheader.route_type = Convert.ToString(dr["route_type"]);
                                        objheader.rtn_trans_route = Convert.ToString(dr["rtn_trans_route"]);
                                        objheader.scan_expire_days = Convert.ToInt32(dr["scan_expire_days"]);
                                        objheader.send_to_pt = Convert.ToInt32(dr["send_to_pt"]);
                                        objheader.service_level = Convert.ToInt32(dr["service_level"]);

                                        objheader.service_time = Convert.ToInt32(dr["service_time"]);

                                        objheader.settlement_level = Convert.ToString(dr["settlement_level"]);
                                        objheader.shipper_facility = Convert.ToString(dr["shipper_facility"]);
                                        objheader.shipper_route = Convert.ToString(dr["shipper_route"]);
                                        objheader.shipper_type = Convert.ToString(dr["shipper_type"]);
                                        objheader.start_date = Convert.ToString(dr["start_date"]);
                                        objheader.start_location = Convert.ToString(dr["start_location"]);
                                        objheader.start_time = Convert.ToString(dr["start_time"]);
                                        objheader.starting_location = Convert.ToString(dr["starting_location"]);
                                        objheader.stops = Convert.ToInt32(dr["stops"]);
                                        objheader.time_to_reseq = Convert.ToString(dr["time_to_reseq"]);
                                        objheader.total_billing_amount = Convert.ToDouble(dr["total_billing_amount"]);

                                        objheader.total_break_minutes = Convert.ToInt32(dr["total_break_minutes"]);
                                        objheader.total_break_time = Convert.ToString(dr["total_break_time"]);
                                        objheader.total_route_minutes = Convert.ToInt32(dr["total_route_minutes"]);
                                        objheader.total_route_time = Convert.ToString(dr["total_route_time"]);
                                        objheader.total_settlement_amount = Convert.ToDouble(dr["total_settlement_amount"]);
                                        objheader.transfer_to_branch = Convert.ToString(dr["transfer_to_branch"]);
                                        objheader.transfer_to_company = Convert.ToInt32(dr["transfer_to_company"]);
                                        objheader.transfer_to_shift = Convert.ToString(dr["transfer_to_shift"]);
                                        objheader.unique_control_id = Convert.ToInt32(dr["unique_control_id"]);
                                        objheader.updated_by = Convert.ToString(dr["updated_by"]);
                                        objheader.updated_date = Convert.ToString(dr["updated_date"]);
                                        objheader.updated_time = Convert.ToString(dr["updated_time"]);
                                        objheader.vehicle_allocation_method = Convert.ToString(dr["vehicle_allocation_method"]);
                                        objheader.vehicle_cost = Convert.ToDouble(dr["vehicle_cost"]);

                                        objroute_headerdetails.route_header = objheader;
                                        clsRoute objclsRoute = new clsRoute();
                                        clsCommon.ReturnResponse objresponse = new clsCommon.ReturnResponse();
                                        string request = JsonConvert.SerializeObject(objroute_headerdetails);
                                        objresponse = objclsRoute.CallDataTracRouteHeaderPostAPI(request);
                                        //objresponse.ResponseVal = true;
                                        // objresponse.Reason = "{\"002018775030\": {\"verified_weight\": null, \"roundtrip_actual_latitude\": null, \"pickup_special_instructions4\": null, \"fuel_miles\": null, \"pickup_dispatch_zone\": null, \"pickup_zip\": \"23150\", \"pickup_actual_arr_time\": \"08:00\", \"cod_accept_company_check\": false, \"add_charge_occur9\": null, \"pickup_omw_latitude\": null, \"service_level_text\": \"Threshold Delivery\", \"service_level\": 57, \"exception_sign_required\": false, \"pickup_phone_ext\": null, \"roundtrip_actual_pieces\": null, \"bringg_send_sms\": false, \"az_equip2\": null, \"hist_inv_date\": null, \"date_order_entered\": \"2021-07-21\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"pickup_city\": \"SANDSTON\", \"pickup_phone\": null, \"pickup_sign_req\": true, \"deliver_route_sequence\": null, \"deliver_phone\": null, \"deliver_omw_longitude\": null, \"roundtrip_actual_longitude\": null, \"page_number\": 1, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"add_charge_code9\": null, \"pickup_eta_time\": null, \"record_type\": 0, \"add_charge_occur11\": null, \"push_partner_order_id\": null, \"deliver_country\": null, \"customer_name\": \"MXD/RYDER\", \"bol_number\": null, \"pickup_latitude\": 37.53250820, \"add_charge_code4\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"pu_arrive_notification_sent\": false, \"distribution_shift_id\": null, \"pickup_special_instr_long\": null, \"id\": \"002018775030\", \"callback_to\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"ordered_by\": \"RYDER\", \"add_charge_code12\": null, \"pickup_route_seq\": null, \"deliver_city\": \"HANOVER\", \"add_charge_occur5\": null, \"edi_acknowledgement_required\": false, \"rescheduled_ctrl_number\": null, \"driver2\": null, \"deliver_room\": null, \"deliver_actual_arr_time\": \"08:00\", \"fuel_price_zone\": null, \"add_charge_amt9\": null, \"add_charge_amt4\": null, \"delivery_address_point_number_text\": \"PAMELA YORK\", \"delivery_address_point_number\": 24254, \"deliver_actual_longitude\": null, \"add_charge_amt2\": null, \"additional_drivers\": false, \"pickup_pricing_zone\": 1, \"hazmat\": false, \"pickup_address\": \"540 EASTPARK CT\", \"pickup_route_code\": null, \"callback_userid\": null, \"pickup_point_customer\": 31025, \"rate_buck_amt1\": 52.00, \"add_charge_amt8\": null, \"callback_time\": null, \"csr\": \"DX*\", \"roundtrip_actual_depart_time\": null, \"customers_etrac_partner_id\": \"96609250\", \"manual_notepad\": false, \"add_charge_code8\": null, \"bringg_order_id\": null, \"deliver_omw_latitude\": null, \"pickup_longitude\": -77.33035820, \"etrac_number\": null, \"distribution_unique_id\": 0, \"vehicle_type\": null, \"roundtrip_actual_arrival_time\": null, \"delivery_longitude\": -77.28769600, \"pu_actual_location_accuracy\": null, \"deliver_actual_date\": \"2021-07-14\", \"exception_timestamp\": null, \"deliver_zip\": \"23069\", \"roundtrip_wait_time\": null, \"add_charge_occur8\": null, \"dl_arrive_notification_sent\": false, \"pickup_special_instructions1\": null, \"ordered_by_phone_number\": null, \"deliver_requested_arr_time\": \"08:00\", \"rate_miles\": null, \"holiday_groups\": null, \"pickup_email_notification_sent\": false, \"add_charge_code3\": null, \"dispatch_id\": null, \"add_charge_occur10\": null, \"dispatch_time\": null, \"deliver_wait_time\": null, \"invoice_period_end_date\": null, \"add_charge_occur12\": null, \"fuel_plan\": null, \"return_svc_level\": null, \"pickup_actual_date\": \"2021-07-14\", \"send_new_order_alert\": false, \"pickup_room\": null, \"rate_buck_amt8\": null, \"add_charge_amt10\": null, \"insurance_amount\": null, \"add_charge_amt3\": null, \"add_charge_amt6\": null, \"pickup_special_instructions3\": null, \"pickup_requested_date\": \"2021-07-14\", \"roundtrip_sign_req\": false, \"actual_miles\": null, \"pickup_address_point_number_text\": \"1STOPBEDROOMS\", \"pickup_address_point_number\": 19845, \"deliver_actual_latitude\": null, \"deliver_phone_ext\": null, \"deliver_route_code\": null, \"add_charge_code10\": null, \"delivery_airport_code\": null, \"reference\": null, \"photos_exist\": false, \"master_airway_bill_number\": null, \"control_number\": 1877503, \"cod_text\": \"No\", \"cod\": \"N\", \"rate_buck_amt11\": null, \"cod_amount\": null, \"pickup_omw_timestamp\": null, \"signature_images\": [], \"deliver_special_instructions1\": null, \"quote_amount\": null, \"total_pages\": 1, \"rate_buck_amt4\": null, \"line_items\": [], \"delivery_latitude\": 37.80844290, \"progress\": [{\"status_time\": \"15:57:00\", \"status_date\": \"2021-07-21\", \"status_text\": \"Entered in carrier's system\"}, {\"status_time\": \"08:30:00\", \"status_date\": \"2021-07-14\", \"status_text\": \"Picked up\"}, {\"status_time\": \"08:15:00\", \"status_date\": \"2021-07-14\", \"status_text\": \"Delivered\"}], \"add_charge_code1\": null, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"deliver_special_instr_long\": null, \"deliver_address\": \"205 HARDWOOD LN\", \"add_charge_occur4\": null, \"deliver_eta_date\": null, \"pickup_actual_dep_time\": \"08:30\", \"deliver_requested_dep_time\": \"17:00\", \"add_charge_code7\": null, \"deliver_actual_dep_time\": \"08:15\", \"cod_accept_cashiers_check\": false, \"bringg_last_loc_sent\": null, \"az_equip3\": null, \"driver1_text\": \"WALT DARBY\", \"driver1\": 3215, \"pickup_actual_latitude\": null, \"add_charge_occur2\": null, \"order_automatically_quoted\": false, \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"hours\": \"15\", \"frequent_caller_id\": null, \"rate_buck_amt6\": null, \"rate_chart_used\": 0, \"deliver_actual_pieces\": null, \"add_charge_code5\": null, \"pickup_omw_longitude\": null, \"notes\": [], \"delivery_point_customer\": 31025, \"add_charge_occur7\": null, \"rate_buck_amt5\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"add_charge_code11\": null, \"pickup_name\": \"1STOPBEDROOMS\", \"callback_date\": null, \"add_charge_code2\": null, \"house_airway_bill_number\": null, \"deliver_name\": \"PAMELA YORK\", \"number_of_pieces\": 1, \"deliver_eta_time\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"rate_special_instructions\": null, \"add_charge_occur3\": null, \"pickup_eta_date\": null, \"deliver_special_instructions4\": null, \"custom_special_instr_long\": null, \"deliver_special_instructions2\": null, \"pickup_signature\": \"SOF\", \"az_equip1\": null, \"add_charge_amt12\": null, \"settlements\": [{\"vendor_invoice_number\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"voucher_date\": null, \"voucher_amount\": null, \"fuel_price_source\": null, \"settlement_bucket6_pct\": null, \"pre_book_percentage\": true, \"settlement_bucket4_pct\": null, \"settlement_period_end_date\": null, \"charge5\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"settlement_bucket5_pct\": null, \"voucher_number\": null, \"charge6\": null, \"settlement_bucket2_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"settlement_pct\": 100.00, \"vendor_employee_numer\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"driver_number_text\": \"WALT DARBY\", \"driver_number\": 3215, \"fuel_plan\": null, \"agents_etrac_partner_id\": null, \"charge2\": null, \"charge3\": null, \"settlement_bucket1_pct\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"id\": \"002018775030D1\", \"record_type\": 0, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"pay_chart_used\": null, \"fuel_price_zone\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"charge1\": null, \"control_number\": 1877503, \"adjustment_type\": null, \"order_date\": \"2021-07-14\", \"time_last_updated\": \"14:57\", \"agent_etrac_transaction_number\": null, \"date_last_updated\": \"2021-07-21\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"settlement_bucket3_pct\": null}], \"calc_add_on_chgs\": false, \"original_schedule_number\": null, \"blocks\": null, \"del_actual_location_accuracy\": null, \"zone_set_used\": 1, \"pickup_country\": null, \"pickup_state\": \"VA\", \"add_charge_amt7\": null, \"email_addresses\": null, \"add_charge_occur1\": null, \"pickup_wait_time\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"distribution_branch_id\": null, \"rate_buck_amt9\": null, \"add_charge_amt1\": null, \"pickup_requested_dep_time\": \"09:00\", \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"deliver_state\": \"VA\", \"deliver_dispatch_zone\": null, \"image_sign_req\": false, \"add_charge_code6\": null, \"deliver_requested_date\": \"2021-07-14\", \"add_charge_amt5\": null, \"time_order_entered\": \"15:57\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"pickup_attention\": null, \"rate_buck_amt7\": null, \"add_charge_occur6\": null, \"fuel_price_source\": null, \"pickup_airport_code\": null, \"rate_buck_amt2\": null, \"rate_buck_amt3\": null, \"deliver_omw_timestamp\": null, \"exception_code\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"weight\": null, \"signature_required\": true, \"rate_buck_amt10\": null, \"hist_inv_number\": 0, \"deliver_pricing_zone\": 1, \"pickup_actual_longitude\": null, \"push_services\": null, \"add_charge_amt11\": null, \"rt_actual_location_accuracy\": null, \"roundtrip_actual_date\": null, \"pickup_requested_arr_time\": null, \"deliver_attention\": null, \"deliver_special_instructions3\": null, \"pickup_actual_pieces\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"roundtrip_signature\": null, \"po_number\": null, \"signature\": \"SOF\", \"pickup_special_instructions2\": null, \"original_ctrl_number\": null, \"previous_ctrl_number\": null}}";

                                        if (objresponse.ResponseVal)
                                        {
                                            strExecutionLogMessage = "RouteHeaderPostAPI Success " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                            strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            DataSet dsOrderResponse = objCommon.jsonToDataSet(objresponse.Reason, "RouteHeaderPostAPI");
                                            var UniqueId = Convert.ToString(dsOrderResponse.Tables["id"].Rows[0]["id"]);
                                            try
                                            {
                                                // Write output to csv

                                            }
                                            catch (Exception ex)
                                            {
                                                strExecutionLogMessage = "ProcessAddRouteHeaderFiles Exception -" + ex.Message + System.Environment.NewLine;
                                                strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                                strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                                strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                            }


                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "RouteHeaderPostAPI Failed " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                            strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                            DataSet dsOrderFailureResponse = objCommon.jsonToDataSet(objresponse.Reason);
                                            dsOrderFailureResponse.Tables[0].TableName = "RouteHeaderFailure";
                                            dsOrderFailureResponse.Tables[0].Columns.Add("Customer Reference", typeof(System.String));
                                            foreach (DataRow row in dsOrderFailureResponse.Tables[0].Rows)
                                            {
                                                row["Customer Reference"] = ReferenceId;
                                            }
                                            objCommon.WriteDataToCsvFile(dsOrderFailureResponse.Tables[0],
                                        strInputFilePath, ReferenceId, strFileName, strDatetime);

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        strExecutionLogMessage = "ProcessAddRouteHeaderFiles Exception -" + ex.Message + System.Environment.NewLine;
                                        strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                        strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                        strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
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

                strExecutionLogMessage = "Processing the Add Stop data for " + strInputFilePath;
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

                                    ReferenceId = Convert.ToString(dr["Customer Reference"]);

                                    try
                                    {
                                        route_stopdetails objroute_stopdetails = new route_stopdetails();
                                        route_stop objroute_stop = new route_stop();

                                        objroute_stop.actual_arrival_time = Convert.ToString(dr["actual_arrival_time"]);
                                        objroute_stop.actual_billing_amt = Convert.ToDouble(dr["actual_billing_amt"]);
                                        objroute_stop.actual_cod_amt = Convert.ToDouble(dr["actual_cod_amt"]);
                                        objroute_stop.actual_cod_type = Convert.ToString(dr["actual_cod_type"]);
                                        objroute_stop.actual_delivery_date = Convert.ToString(dr["actual_delivery_date"]);
                                        objroute_stop.actual_depart_time = Convert.ToString(dr["actual_depart_time"]);
                                        objroute_stop.actual_latitude = Convert.ToDouble(dr["actual_latitude"]);
                                        objroute_stop.actual_longitude = Convert.ToDouble(dr["actual_longitude"]);
                                        objroute_stop.actual_pieces = Convert.ToInt32(dr["actual_pieces"]);
                                        objroute_stop.actual_settlement_amt = Convert.ToDouble(dr["actual_settlement_amt"]);
                                        objroute_stop.actual_weight = Convert.ToInt32(dr["actual_weight"]);
                                        objroute_stop.additional_instructions = Convert.ToString(dr["additional_instructions"]);
                                        objroute_stop.addl_charge_code1 = Convert.ToString(dr["addl_charge_code1"]);
                                        objroute_stop.addl_charge_code10 = Convert.ToString(dr["addl_charge_code10"]);
                                        objroute_stop.addl_charge_code11 = Convert.ToString(dr["addl_charge_code11"]);
                                        objroute_stop.addl_charge_code12 = Convert.ToString(dr["addl_charge_code12"]);
                                        objroute_stop.addl_charge_code2 = Convert.ToString(dr["addl_charge_code2"]);
                                        objroute_stop.addl_charge_code3 = Convert.ToString(dr["addl_charge_code3"]);
                                        objroute_stop.addl_charge_code4 = Convert.ToString(dr["addl_charge_code4"]);
                                        objroute_stop.addl_charge_code5 = Convert.ToString(dr["addl_charge_code5"]);
                                        objroute_stop.addl_charge_code6 = Convert.ToString(dr["addl_charge_code6"]);
                                        objroute_stop.addl_charge_code7 = Convert.ToString(dr["addl_charge_code7"]);
                                        objroute_stop.addl_charge_code8 = Convert.ToString(dr["addl_charge_code8"]);
                                        objroute_stop.addl_charge_code9 = Convert.ToString(dr["addl_charge_code9"]);
                                        objroute_stop.addl_charge_occur1 = Convert.ToInt32(dr["addl_charge_occur1"]);
                                        objroute_stop.addl_charge_occur10 = Convert.ToInt32(dr["addl_charge_occur10"]);
                                        objroute_stop.addl_charge_occur11 = Convert.ToInt32(dr["addl_charge_occur11"]);
                                        objroute_stop.addl_charge_occur12 = Convert.ToInt32(dr["addl_charge_occur12"]);
                                        objroute_stop.addl_charge_occur2 = Convert.ToInt32(dr["addl_charge_occur2"]);
                                        objroute_stop.addl_charge_occur3 = Convert.ToInt32(dr["addl_charge_occur3"]);
                                        objroute_stop.addl_charge_occur4 = Convert.ToInt32(dr["addl_charge_occur4"]);
                                        objroute_stop.addl_charge_occur5 = Convert.ToInt32(dr["addl_charge_occur5"]);
                                        objroute_stop.addl_charge_occur6 = Convert.ToInt32(dr["addl_charge_occur6"]);
                                        objroute_stop.addl_charge_occur7 = Convert.ToInt32(dr["addl_charge_occur7"]);
                                        objroute_stop.addl_charge_occur8 = Convert.ToInt32(dr["addl_charge_occur8"]);
                                        objroute_stop.addl_charge_occur9 = Convert.ToInt32(dr["addl_charge_occur9"]);
                                        objroute_stop.addon_billing_amt = Convert.ToDouble(dr["addon_billing_amt"]);
                                        objroute_stop.address = Convert.ToString(dr["address"]);
                                        objroute_stop.address_name = Convert.ToString(dr["address_name"]);
                                        objroute_stop.address_point = Convert.ToInt32(dr["address_point"]);
                                        objroute_stop.address_point_customer = Convert.ToInt32(dr["address_point_customer"]);
                                        objroute_stop.alt_lookup = Convert.ToString(dr["alt_lookup"]);
                                        objroute_stop.arrival_time = Convert.ToString(dr["arrival_time"]);
                                        objroute_stop.asn_sent = Convert.ToInt32(dr["asn_sent"]);
                                        objroute_stop.attention = Convert.ToString(dr["attention"]);
                                        objroute_stop.billing_override_amt = Convert.ToDouble(dr["billing_override_amt"]);
                                        objroute_stop.bol_number = Convert.ToString(dr["bol_number"]);
                                        objroute_stop.branch_id = Convert.ToString(dr["branch_id"]);
                                        objroute_stop.c2_paperwork = Convert.ToBoolean(dr["c2_paperwork"]);
                                        objroute_stop.callback_required = Convert.ToString(dr["callback_required"]);
                                        objroute_stop.cases = Convert.ToInt32(dr["cases"]);
                                        objroute_stop.city = Convert.ToString(dr["city"]);
                                        objroute_stop.cod_amount = Convert.ToDouble(dr["cod_amount"]);
                                        objroute_stop.cod_check_no = Convert.ToString(dr["cod_check_no"]);
                                        objroute_stop.cod_type = Convert.ToString(dr["cod_type"]);
                                        objroute_stop.combine_data = Convert.ToString(dr["combine_data"]);
                                        objroute_stop.comments = Convert.ToString(dr["comments"]);
                                        objroute_stop.created_by = Convert.ToString(dr["created_by"]);
                                        objroute_stop.created_date = Convert.ToString(dr["created_date"]);
                                        objroute_stop.created_time = Convert.ToString(dr["created_time"]);
                                        objroute_stop.customer_number = Convert.ToInt32(dr["customer_number"]);
                                        objroute_stop.departure_time = Convert.ToString(dr["departure_time"]);
                                        objroute_stop.dispatch_zone = Convert.ToString(dr["dispatch_zone"]);
                                        objroute_stop.driver_app_status = Convert.ToString(dr["driver_app_status"]);
                                        objroute_stop.eta = Convert.ToString(dr["eta"]);
                                        objroute_stop.eta_date = Convert.ToString(dr["eta_date"]);
                                        objroute_stop.exception_code = Convert.ToString(dr["exception_code"]);
                                        objroute_stop.expected_pieces = Convert.ToInt32(dr["expected_pieces"]);
                                        objroute_stop.expected_weight = Convert.ToInt32(dr["expected_weight"]);
                                        objroute_stop.height = Convert.ToInt32(dr["height"]);
                                        objroute_stop.image_sign_req = Convert.ToBoolean(dr["image_sign_req"]);
                                        objroute_stop.insurance_value = Convert.ToInt32(dr["insurance_value"]);
                                        objroute_stop.invoice_number = Convert.ToString(dr["invoice_number"]);
                                        objroute_stop.item_scans_required = Convert.ToBoolean(dr["item_scans_required"]);
                                        // objroute_stop.items = Convert.ToString(dr["items"]);
                                        objroute_stop.late_notice_date = Convert.ToString(dr["late_notice_date"]);
                                        objroute_stop.late_notice_time = Convert.ToString(dr["late_notice_time"]);
                                        objroute_stop.latitude = Convert.ToDouble(dr["latitude"]);
                                        objroute_stop.length = Convert.ToInt32(dr["length"]);
                                        objroute_stop.loaded_pieces = Convert.ToInt32(dr["loaded_pieces"]);
                                        objroute_stop.location_accuracy = Convert.ToInt32(dr["location_accuracy"]);
                                        objroute_stop.longitude = Convert.ToDouble(dr["longitude"]);
                                        objroute_stop.minutes_late = Convert.ToInt32(dr["minutes_late"]);
                                        //  objroute_stop.notes = Convert.ToString(dr["notes"]);
                                        objroute_stop.ordered_by = Convert.ToString(dr["ordered_by"]);
                                        objroute_stop.orig_order_number = Convert.ToInt32(dr["orig_order_number"]);
                                        objroute_stop.origin_code = Convert.ToString(dr["origin_code"]);
                                        objroute_stop.original_id = Convert.ToInt32(dr["original_id"]);
                                        objroute_stop.override_settle_percent = Convert.ToDouble(dr["override_settle_percent"]);
                                        objroute_stop.phone = Convert.ToString(dr["phone"]);
                                        objroute_stop.phone_ext = Convert.ToInt32(dr["phone_ext"]);
                                        objroute_stop.photos_exist = Convert.ToBoolean(dr["photos_exist"]);
                                        objroute_stop.posted_by = Convert.ToString(dr["posted_by"]);
                                        objroute_stop.posted_date = Convert.ToString(dr["posted_date"]);
                                        objroute_stop.posted_status = Convert.ToBoolean(dr["posted_status"]);
                                        objroute_stop.posted_time = Convert.ToString(dr["posted_time"]);
                                        objroute_stop.pricing_zone = Convert.ToInt32(dr["pricing_zone"]);
                                        // objroute_stop.progress = Convert.ToString(dr["progress"]);
                                        objroute_stop.received_branch = Convert.ToString(dr["received_branch"]);
                                        objroute_stop.received_company = Convert.ToInt32(dr["received_company"]);
                                        objroute_stop.received_pieces = Convert.ToInt32(dr["received_pieces"]);
                                        objroute_stop.received_route = Convert.ToString(dr["received_route"]);
                                        objroute_stop.received_sequence = Convert.ToString(dr["received_sequence"]);
                                        objroute_stop.received_shift = Convert.ToString(dr["received_shift"]);
                                        objroute_stop.received_unique_id = Convert.ToInt32(dr["received_unique_id"]);
                                        objroute_stop.redelivery = Convert.ToBoolean(dr["redelivery"]);
                                        objroute_stop.reference = Convert.ToString(dr["reference"]);
                                        objroute_stop.required_signature_type = Convert.ToString(dr["required_signature_type"]);
                                        // objroute_stop.return= Convert.ToString(dr["return"]);
                                        objroute_stop.return_redel_id = Convert.ToInt32(dr["return_redel_id"]);
                                        objroute_stop.return_redelivery_date = Convert.ToString(dr["return_redelivery_date"]);
                                        objroute_stop.return_redelivery_flag = Convert.ToString(dr["return_redelivery_flag"]);
                                        objroute_stop.room = Convert.ToString(dr["room"]);
                                        objroute_stop.route_code = Convert.ToString(dr["route_code"]);
                                        objroute_stop.route_date = Convert.ToString(dr["route_date"]);
                                        objroute_stop.schedule_stop_id = Convert.ToInt32(dr["schedule_stop_id"]);
                                        objroute_stop.sent_to_phone = Convert.ToBoolean(dr["sent_to_phone"]);
                                        objroute_stop.service_level = Convert.ToInt32(dr["service_level"]);
                                        objroute_stop.service_time = Convert.ToInt32(dr["service_time"]);
                                        objroute_stop.settlement_override_amt = Convert.ToDouble(dr["settlement_override_amt"]);
                                        objroute_stop.shift_id = Convert.ToString(dr["shift_id"]);
                                        objroute_stop.signature = Convert.ToString(dr["signature"]);
                                        objroute_stop.signature_filename = Convert.ToString(dr["signature_filename"]);
                                        //  objroute_stop.signature_images = Convert.ToString(dr["signature_images"]);
                                        objroute_stop.signature_required = Convert.ToBoolean(dr["signature_required"]);
                                        objroute_stop.special_instructions1 = Convert.ToString(dr["special_instructions1"]);
                                        objroute_stop.special_instructions2 = Convert.ToString(dr["special_instructions2"]);
                                        objroute_stop.special_instructions3 = Convert.ToString(dr["special_instructions3"]);
                                        objroute_stop.special_instructions4 = Convert.ToString(dr["special_instructions4"]);
                                        objroute_stop.state = Convert.ToString(dr["state"]);
                                        objroute_stop.stop_sequence = Convert.ToString(dr["stop_sequence"]);
                                        objroute_stop.stop_type = Convert.ToString(dr["stop_type"]);
                                        objroute_stop.times_sent = Convert.ToInt32(dr["times_sent"]);
                                        objroute_stop.totes = Convert.ToInt32(dr["totes"]);
                                        objroute_stop.transfer_to_route = Convert.ToString(dr["transfer_to_route"]);
                                        objroute_stop.transfer_to_sequence = Convert.ToString(dr["transfer_to_sequence"]);
                                        objroute_stop.updated_by = Convert.ToString(dr["updated_by"]);
                                        objroute_stop.updated_by_scanner = Convert.ToBoolean(dr["updated_by_scanner"]);
                                        objroute_stop.updated_date = Convert.ToString(dr["updated_date"]);
                                        objroute_stop.updated_time = Convert.ToString(dr["updated_time"]);
                                        objroute_stop.upload_time = Convert.ToString(dr["upload_time"]);
                                        objroute_stop.vehicle = Convert.ToString(dr["vehicle"]);
                                        objroute_stop.verification_id_details = Convert.ToString(dr["verification_id_details"]);
                                        objroute_stop.verification_id_type = Convert.ToString(dr["verification_id_type"]);
                                        objroute_stop.width = Convert.ToInt32(dr["width"]);
                                        objroute_stop.zip_code = Convert.ToString(dr["zip_code"]);

                                        objroute_stopdetails.route_stop = objroute_stop;
                                        clsRoute objclsRoute = new clsRoute();


                                        clsCommon.ReturnResponse objresponse = new clsCommon.ReturnResponse();
                                        string request = JsonConvert.SerializeObject(objroute_stopdetails);
                                        objresponse = objclsRoute.CallDataTracRouteStopPostAPI(request);
                                        //objresponse.ResponseVal = true;
                                        // objresponse.Reason = "{\"002018775030\": {\"verified_weight\": null, \"roundtrip_actual_latitude\": null, \"pickup_special_instructions4\": null, \"fuel_miles\": null, \"pickup_dispatch_zone\": null, \"pickup_zip\": \"23150\", \"pickup_actual_arr_time\": \"08:00\", \"cod_accept_company_check\": false, \"add_charge_occur9\": null, \"pickup_omw_latitude\": null, \"service_level_text\": \"Threshold Delivery\", \"service_level\": 57, \"exception_sign_required\": false, \"pickup_phone_ext\": null, \"roundtrip_actual_pieces\": null, \"bringg_send_sms\": false, \"az_equip2\": null, \"hist_inv_date\": null, \"date_order_entered\": \"2021-07-21\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"pickup_city\": \"SANDSTON\", \"pickup_phone\": null, \"pickup_sign_req\": true, \"deliver_route_sequence\": null, \"deliver_phone\": null, \"deliver_omw_longitude\": null, \"roundtrip_actual_longitude\": null, \"page_number\": 1, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"add_charge_code9\": null, \"pickup_eta_time\": null, \"record_type\": 0, \"add_charge_occur11\": null, \"push_partner_order_id\": null, \"deliver_country\": null, \"customer_name\": \"MXD/RYDER\", \"bol_number\": null, \"pickup_latitude\": 37.53250820, \"add_charge_code4\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"pu_arrive_notification_sent\": false, \"distribution_shift_id\": null, \"pickup_special_instr_long\": null, \"id\": \"002018775030\", \"callback_to\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"ordered_by\": \"RYDER\", \"add_charge_code12\": null, \"pickup_route_seq\": null, \"deliver_city\": \"HANOVER\", \"add_charge_occur5\": null, \"edi_acknowledgement_required\": false, \"rescheduled_ctrl_number\": null, \"driver2\": null, \"deliver_room\": null, \"deliver_actual_arr_time\": \"08:00\", \"fuel_price_zone\": null, \"add_charge_amt9\": null, \"add_charge_amt4\": null, \"delivery_address_point_number_text\": \"PAMELA YORK\", \"delivery_address_point_number\": 24254, \"deliver_actual_longitude\": null, \"add_charge_amt2\": null, \"additional_drivers\": false, \"pickup_pricing_zone\": 1, \"hazmat\": false, \"pickup_address\": \"540 EASTPARK CT\", \"pickup_route_code\": null, \"callback_userid\": null, \"pickup_point_customer\": 31025, \"rate_buck_amt1\": 52.00, \"add_charge_amt8\": null, \"callback_time\": null, \"csr\": \"DX*\", \"roundtrip_actual_depart_time\": null, \"customers_etrac_partner_id\": \"96609250\", \"manual_notepad\": false, \"add_charge_code8\": null, \"bringg_order_id\": null, \"deliver_omw_latitude\": null, \"pickup_longitude\": -77.33035820, \"etrac_number\": null, \"distribution_unique_id\": 0, \"vehicle_type\": null, \"roundtrip_actual_arrival_time\": null, \"delivery_longitude\": -77.28769600, \"pu_actual_location_accuracy\": null, \"deliver_actual_date\": \"2021-07-14\", \"exception_timestamp\": null, \"deliver_zip\": \"23069\", \"roundtrip_wait_time\": null, \"add_charge_occur8\": null, \"dl_arrive_notification_sent\": false, \"pickup_special_instructions1\": null, \"ordered_by_phone_number\": null, \"deliver_requested_arr_time\": \"08:00\", \"rate_miles\": null, \"holiday_groups\": null, \"pickup_email_notification_sent\": false, \"add_charge_code3\": null, \"dispatch_id\": null, \"add_charge_occur10\": null, \"dispatch_time\": null, \"deliver_wait_time\": null, \"invoice_period_end_date\": null, \"add_charge_occur12\": null, \"fuel_plan\": null, \"return_svc_level\": null, \"pickup_actual_date\": \"2021-07-14\", \"send_new_order_alert\": false, \"pickup_room\": null, \"rate_buck_amt8\": null, \"add_charge_amt10\": null, \"insurance_amount\": null, \"add_charge_amt3\": null, \"add_charge_amt6\": null, \"pickup_special_instructions3\": null, \"pickup_requested_date\": \"2021-07-14\", \"roundtrip_sign_req\": false, \"actual_miles\": null, \"pickup_address_point_number_text\": \"1STOPBEDROOMS\", \"pickup_address_point_number\": 19845, \"deliver_actual_latitude\": null, \"deliver_phone_ext\": null, \"deliver_route_code\": null, \"add_charge_code10\": null, \"delivery_airport_code\": null, \"reference\": null, \"photos_exist\": false, \"master_airway_bill_number\": null, \"control_number\": 1877503, \"cod_text\": \"No\", \"cod\": \"N\", \"rate_buck_amt11\": null, \"cod_amount\": null, \"pickup_omw_timestamp\": null, \"signature_images\": [], \"deliver_special_instructions1\": null, \"quote_amount\": null, \"total_pages\": 1, \"rate_buck_amt4\": null, \"line_items\": [], \"delivery_latitude\": 37.80844290, \"progress\": [{\"status_time\": \"15:57:00\", \"status_date\": \"2021-07-21\", \"status_text\": \"Entered in carrier's system\"}, {\"status_time\": \"08:30:00\", \"status_date\": \"2021-07-14\", \"status_text\": \"Picked up\"}, {\"status_time\": \"08:15:00\", \"status_date\": \"2021-07-14\", \"status_text\": \"Delivered\"}], \"add_charge_code1\": null, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"deliver_special_instr_long\": null, \"deliver_address\": \"205 HARDWOOD LN\", \"add_charge_occur4\": null, \"deliver_eta_date\": null, \"pickup_actual_dep_time\": \"08:30\", \"deliver_requested_dep_time\": \"17:00\", \"add_charge_code7\": null, \"deliver_actual_dep_time\": \"08:15\", \"cod_accept_cashiers_check\": false, \"bringg_last_loc_sent\": null, \"az_equip3\": null, \"driver1_text\": \"WALT DARBY\", \"driver1\": 3215, \"pickup_actual_latitude\": null, \"add_charge_occur2\": null, \"order_automatically_quoted\": false, \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"hours\": \"15\", \"frequent_caller_id\": null, \"rate_buck_amt6\": null, \"rate_chart_used\": 0, \"deliver_actual_pieces\": null, \"add_charge_code5\": null, \"pickup_omw_longitude\": null, \"notes\": [], \"delivery_point_customer\": 31025, \"add_charge_occur7\": null, \"rate_buck_amt5\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"add_charge_code11\": null, \"pickup_name\": \"1STOPBEDROOMS\", \"callback_date\": null, \"add_charge_code2\": null, \"house_airway_bill_number\": null, \"deliver_name\": \"PAMELA YORK\", \"number_of_pieces\": 1, \"deliver_eta_time\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"rate_special_instructions\": null, \"add_charge_occur3\": null, \"pickup_eta_date\": null, \"deliver_special_instructions4\": null, \"custom_special_instr_long\": null, \"deliver_special_instructions2\": null, \"pickup_signature\": \"SOF\", \"az_equip1\": null, \"add_charge_amt12\": null, \"settlements\": [{\"vendor_invoice_number\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"voucher_date\": null, \"voucher_amount\": null, \"fuel_price_source\": null, \"settlement_bucket6_pct\": null, \"pre_book_percentage\": true, \"settlement_bucket4_pct\": null, \"settlement_period_end_date\": null, \"charge5\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"settlement_bucket5_pct\": null, \"voucher_number\": null, \"charge6\": null, \"settlement_bucket2_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"settlement_pct\": 100.00, \"vendor_employee_numer\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"driver_number_text\": \"WALT DARBY\", \"driver_number\": 3215, \"fuel_plan\": null, \"agents_etrac_partner_id\": null, \"charge2\": null, \"charge3\": null, \"settlement_bucket1_pct\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"id\": \"002018775030D1\", \"record_type\": 0, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"pay_chart_used\": null, \"fuel_price_zone\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"charge1\": null, \"control_number\": 1877503, \"adjustment_type\": null, \"order_date\": \"2021-07-14\", \"time_last_updated\": \"14:57\", \"agent_etrac_transaction_number\": null, \"date_last_updated\": \"2021-07-21\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"settlement_bucket3_pct\": null}], \"calc_add_on_chgs\": false, \"original_schedule_number\": null, \"blocks\": null, \"del_actual_location_accuracy\": null, \"zone_set_used\": 1, \"pickup_country\": null, \"pickup_state\": \"VA\", \"add_charge_amt7\": null, \"email_addresses\": null, \"add_charge_occur1\": null, \"pickup_wait_time\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"distribution_branch_id\": null, \"rate_buck_amt9\": null, \"add_charge_amt1\": null, \"pickup_requested_dep_time\": \"09:00\", \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"deliver_state\": \"VA\", \"deliver_dispatch_zone\": null, \"image_sign_req\": false, \"add_charge_code6\": null, \"deliver_requested_date\": \"2021-07-14\", \"add_charge_amt5\": null, \"time_order_entered\": \"15:57\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"pickup_attention\": null, \"rate_buck_amt7\": null, \"add_charge_occur6\": null, \"fuel_price_source\": null, \"pickup_airport_code\": null, \"rate_buck_amt2\": null, \"rate_buck_amt3\": null, \"deliver_omw_timestamp\": null, \"exception_code\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"weight\": null, \"signature_required\": true, \"rate_buck_amt10\": null, \"hist_inv_number\": 0, \"deliver_pricing_zone\": 1, \"pickup_actual_longitude\": null, \"push_services\": null, \"add_charge_amt11\": null, \"rt_actual_location_accuracy\": null, \"roundtrip_actual_date\": null, \"pickup_requested_arr_time\": null, \"deliver_attention\": null, \"deliver_special_instructions3\": null, \"pickup_actual_pieces\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"roundtrip_signature\": null, \"po_number\": null, \"signature\": \"SOF\", \"pickup_special_instructions2\": null, \"original_ctrl_number\": null, \"previous_ctrl_number\": null}}";

                                        if (objresponse.ResponseVal)
                                        {
                                            strExecutionLogMessage = "RouteStopPostAPI Success " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                            strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            DataSet dsOrderResponse = objCommon.jsonToDataSet(objresponse.Reason, "RouteStopPostAPI");
                                            var UniqueId = Convert.ToString(dsOrderResponse.Tables["id"].Rows[0]["id"]);
                                            try
                                            {
                                                // Write output to csv

                                            }
                                            catch (Exception ex)
                                            {
                                                strExecutionLogMessage = "ProcessAddRouteStopFiles Exception -" + ex.Message + System.Environment.NewLine;
                                                strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                                strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                                strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                            }


                                        }
                                        else
                                        {
                                            strExecutionLogMessage = "RouteHeaderPostAPI Failed " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                            strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                            DataSet dsOrderFailureResponse = objCommon.jsonToDataSet(objresponse.Reason);
                                            dsOrderFailureResponse.Tables[0].TableName = "RouteHeaderFailure";
                                            dsOrderFailureResponse.Tables[0].Columns.Add("Customer Reference", typeof(System.String));
                                            foreach (DataRow row in dsOrderFailureResponse.Tables[0].Rows)
                                            {
                                                row["Customer Reference"] = ReferenceId;
                                            }
                                            objCommon.WriteDataToCsvFile(dsOrderFailureResponse.Tables[0],
                                        strInputFilePath, ReferenceId, strFileName, strDatetime);

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        strExecutionLogMessage = "ProcessAddRouteStopFiles Exception -" + ex.Message + System.Environment.NewLine;
                                        strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                        strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                        strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
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
                        strExecutionLogMessage = "ProcessAddRouteStopFiles Exception -" + ex.Message + System.Environment.NewLine;
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


        private static List<DataTable> SplitTable(DataTable originalTable, int batchSize,string fileName)
        {
            List<DataTable> tables = new List<DataTable>();
            int i = 0;
            int j = 1;

            int fileExtPos = fileName.LastIndexOf(".");
            if (fileExtPos >= 0)
                fileName = fileName.Substring(0, fileExtPos);

            DataTable newDt = originalTable.Clone();
          //  newDt.TableName = "Table_" + j;
            newDt.TableName = fileName +"_" + j;
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
                    newDt.TableName = fileName + "_" + j;
                    newDt.Clear();
                    i = 0;
                }
            }
            return tables;
        }
    }
}
