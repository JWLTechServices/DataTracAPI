using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatatracAPIOrder_OrderSettlement
{
    class clsBURL :clsCommon
    {
        private static void BURLProcessAddOrderFiles(DataSet dsExcel, string strFileName, string strDatetime, string strInputFilePath, string strLocationFolder)
        {
            clsCommon objCommon = new clsCommon();
            string strExecutionLogMessage = "";
            string strExecutionLogFileLocation = "";
            string ReferenceId = "";
            try
            {
                int noofrowspertable = 0;
                List<DataTable> splitdt = clsCommon.SplitTable(dsExcel.Tables[0], noofrowspertable, strFileName, strDatetime);

                strExecutionLogMessage = "Parallelly Processing Started for the  file : " + strFileName + "." + System.Environment.NewLine + "Number of processess are going to exicute is :" + noofrowspertable;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                objCommon.CleanSplittedOutputFilesWorkingFolder();

                DataTable dtEBusy = new DataTable();

                DataTable dataTableEBusy = new DataTable();
                dataTableEBusy.Clear();
                dataTableEBusy.Columns.Add("CustomerReference");

                Parallel.ForEach(splitdt, currentDatatable =>
                {
                    var fileName = currentDatatable.TableName;
                    var processingFileName = currentDatatable.TableName;
                    strExecutionLogMessage = "Current Processing File is  : " + fileName + "." + System.Environment.NewLine;
                    objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                    //DataTable datatable; //  = currentDatatable;
                    //if (CustomerName == "BBB" || CustomerName == "BURL" || CustomerName == "COCH")
                    //{
                    //    datatable = RemoveDuplicateRows(dsExcel.Tables[0], "Customer Reference");
                    //}
                    //else
                    //{
                    //    datatable = currentDatatable;
                    //}

                    DataTable datatable = RemoveDuplicateRows(dsExcel.Tables[0], "Customer Reference");

                    DataTable dtFSCRates = new DataTable();

                    string strFscRateDetailsfilepath = objCommon.GetConfigValue("FSCRatesCustomerMappingFilepath");

                    DataSet dsFscData = clsExcelHelper.ImportExcelXLSXToDataSet_FSCRATES_All(strFscRateDetailsfilepath, true);
                    if (dsFscData != null && dsFscData.Tables[0].Rows.Count > 0)
                    {
                        dtFSCRates = dsFscData.Tables["FSCRatesMapping$"];

                        for (int i = dtFSCRates.Rows.Count - 1; i >= 0; i--)
                        {
                            DataRow dr = dtFSCRates.Rows[i];
                            if (dr["Company"] == DBNull.Value && dr["CustomerNumber"] == DBNull.Value)
                                dr.Delete();
                        }
                        dtFSCRates.AcceptChanges();
                    }
                    else
                    {
                        strExecutionLogMessage = "Diesel price data not found in this file " + strFscRateDetailsfilepath + System.Environment.NewLine;
                        strExecutionLogMessage += "So not able to process this file, please update the fsc sheet with appropriate values";
                        strExecutionLogMessage += "Found exception while processing the file, filename  -" + fileName + System.Environment.NewLine;
                        strExecutionLogMessage += "Please look into this immediately." + System.Environment.NewLine;
                        //  objCommon.WriteExecutionLog(strExecutionLogMessage);
                        objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                        string fromMail = objCommon.GetConfigValue("FromMailID");
                        string fromPassword = objCommon.GetConfigValue("FromMailPasssword");
                        string disclaimer = objCommon.GetConfigValue("MailDisclaimer");
                        string toMail = objCommon.GetConfigValue("SendDieselPriceMissingEmail");
                        string subject = "Diesel price data not found in this file " + strFscRateDetailsfilepath;
                        objCommon.SendMail(fromMail, fromPassword, disclaimer, toMail, "", subject, strExecutionLogMessage, "");
                        throw new NullReferenceException("Diesel price data not found in this file " + strFscRateDetailsfilepath);
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

                            DataTable dtBBB = currentDatatable.Select("[Customer Reference]= '" + dr["Customer Reference"] + "'").CopyToDataTable();

                            DataView view = new DataView(dtBBB);
                            DataTable dtdistinctDeliveryDate = view.ToTable(true, "Delivery Date");

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

                                    DataTable dtFSCRatesfromDB = new DataTable();
                                    DataTable tblFSCRatesFiltered = new DataTable();
                                    DataTable dtDeficitWeightRating = new DataTable();
                                    DataTable dtDeficitWeightRatingPayable = new DataTable();
                                    DataTable dtStoreBands = new DataTable();
                                    DataTable dtCarrierFSCRatesfromDB = new DataTable();
                                    DataTable tblCarrierFSCRatesFiltered = new DataTable();
                                    DataTable dtBillingRates = new DataTable();
                                    DataTable dtPayableRates = new DataTable();
                                    DataTable dtBillingRatesBreakdown = new DataTable();

                                    objOrder.company_number = Convert.ToInt32(datatable1.Rows[0]["Company"]);
                                    objOrder.customer_number = Convert.ToInt32(datatable1.Rows[0]["Billing Customer Number"]);

                                    clsDBContext objclsDBContext = new clsDBContext();

                                    clsCommon.DSResponse objDeficitRatesResponse = new clsCommon.DSResponse();
                                    objDeficitRatesResponse = objclsDBContext.GetDeficitWeightRatingDetails(objOrder.company_number, objOrder.customer_number);
                                    if (objDeficitRatesResponse.dsResp.ResponseVal)
                                    {
                                        if (objDeficitRatesResponse.DS.Tables.Count > 0)
                                        {
                                            dtStoreBands = objDeficitRatesResponse.DS.Tables[0];
                                        }
                                        if (objDeficitRatesResponse.DS.Tables.Count > 1)
                                        {
                                            dtDeficitWeightRating = objDeficitRatesResponse.DS.Tables[1];
                                        }
                                        if (objDeficitRatesResponse.DS.Tables.Count > 2)
                                        {
                                            dtDeficitWeightRatingPayable = objDeficitRatesResponse.DS.Tables[2];
                                        }
                                    }
                                    else
                                    {
                                        if (objDeficitRatesResponse.dsResp.Reason.Contains("Exception"))
                                        {
                                            strExecutionLogMessage = "Found exception while getting  Deficit Weight Rating details for  this file " + fileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "So not able to process this file,Please look into this immediately";
                                            strExecutionLogMessage += "Found exception while processing the file, filename  -" + fileName + System.Environment.NewLine;
                                            strExecutionLogMessage += " " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Error : " + objDeficitRatesResponse.dsResp.Reason;
                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                            string fromMail = objCommon.GetConfigValue("FromMailID");
                                            string fromPassword = objCommon.GetConfigValue("FromMailPasssword");
                                            string disclaimer = objCommon.GetConfigValue("MailDisclaimer");
                                            string toMail = objCommon.GetConfigValue("ToMailID");
                                            string subject = "Found exception while processing the file - " + fileName;
                                            objCommon.SendMail(fromMail, fromPassword, disclaimer, toMail, "", subject, strExecutionLogMessage, "");
                                            throw new NullReferenceException("Found exception while processing the file - " + fileName);
                                        }
                                    }

                                    clsCommon.DSResponse objfscRatesResponse = new clsCommon.DSResponse();
                                    objfscRatesResponse = objclsDBContext.GetFSCRates_MappingDetails(objOrder.company_number, objOrder.customer_number);
                                    if (objfscRatesResponse.dsResp.ResponseVal)
                                    {
                                        if (objfscRatesResponse.DS.Tables.Count > 0)
                                        {
                                            dtFSCRatesfromDB = objfscRatesResponse.DS.Tables[0];
                                        }
                                        if (objfscRatesResponse.DS.Tables.Count > 1)
                                        {
                                            dtCarrierFSCRatesfromDB = objfscRatesResponse.DS.Tables[1];
                                        }
                                    }
                                    else
                                    {
                                        if (objfscRatesResponse.dsResp.Reason.Contains("Exception"))
                                        {
                                            strExecutionLogMessage = "Found exception while getting FSC Rate details for  this file " + fileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "So not able to process this file,Please look into this immediately";
                                            strExecutionLogMessage += "Found exception while processing the file, filename  -" + fileName + System.Environment.NewLine;
                                            strExecutionLogMessage += " " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Error : " + objfscRatesResponse.dsResp.Reason;
                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                            string fromMail = objCommon.GetConfigValue("FromMailID");
                                            string fromPassword = objCommon.GetConfigValue("FromMailPasssword");
                                            string disclaimer = objCommon.GetConfigValue("MailDisclaimer");
                                            string toMail = objCommon.GetConfigValue("ToMailID");
                                            string subject = "Found exception while processing the file - " + fileName;
                                            objCommon.SendMail(fromMail, fromPassword, disclaimer, toMail, "", subject, strExecutionLogMessage, "");
                                            throw new NullReferenceException("Found exception while processing the file - " + fileName);
                                        }
                                    }

                                    clsCommon.DSResponse objBPRatesResponse = new clsCommon.DSResponse();
                                    objBPRatesResponse = objclsDBContext.GetBillingRatesAndPayableRates_CustomerMappingDetails(objOrder.company_number, objOrder.customer_number);
                                    if (objBPRatesResponse.dsResp.ResponseVal)
                                    {

                                        if (objBPRatesResponse.DS.Tables.Count > 0)
                                        {
                                            dtBillingRates = objBPRatesResponse.DS.Tables[0].Copy();
                                        }
                                        if (objBPRatesResponse.DS.Tables.Count > 1)
                                        {
                                            dtPayableRates = objBPRatesResponse.DS.Tables[1].Copy();
                                        }
                                    }
                                    else
                                    {
                                        if (objBPRatesResponse.dsResp.Reason.Contains("Exception"))
                                        {
                                            strExecutionLogMessage = "Found exception while getting Billing/Payable Rate details for  this file " + fileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "So not able to process this file,Please look into this immediately";
                                            strExecutionLogMessage += "Found exception while processing the file, filename  -" + fileName + System.Environment.NewLine;
                                            strExecutionLogMessage += " " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Error : " + objBPRatesResponse.dsResp.Reason;
                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                            string fromMail = objCommon.GetConfigValue("FromMailID");
                                            string fromPassword = objCommon.GetConfigValue("FromMailPasssword");
                                            string disclaimer = objCommon.GetConfigValue("MailDisclaimer");
                                            string toMail = objCommon.GetConfigValue("ToMailID");
                                            string subject = "Found exception while processing the file - " + fileName;
                                            objCommon.SendMail(fromMail, fromPassword, disclaimer, toMail, "", subject, strExecutionLogMessage, "");
                                            throw new NullReferenceException("Found exception while processing the file - " + fileName);
                                        }
                                    }



                                    // DataTable dtPayableRatesBreakdown = new DataTable();
                                    clsCommon.DSResponse objBPRatesBreakdownResponse = new clsCommon.DSResponse();
                                    objBPRatesBreakdownResponse = objclsDBContext.GetBilling_Payable_RateBreakdown_Details(objOrder.company_number, objOrder.customer_number);
                                    if (objBPRatesBreakdownResponse.dsResp.ResponseVal)
                                    {
                                        if (objBPRatesBreakdownResponse.DS.Tables.Count > 0)
                                        {
                                            dtBillingRatesBreakdown = objBPRatesBreakdownResponse.DS.Tables[0].Copy();
                                        }
                                        //if (objBPRatesBreakdownResponse.DS.Tables.Count > 1)
                                        //{
                                        //    dtPayableRatesBreakdown = objBPRatesBreakdownResponse.DS.Tables[1].Copy();
                                        //}
                                    }
                                    else
                                    {
                                        if (objBPRatesBreakdownResponse.dsResp.Reason.Contains("Exception"))
                                        {
                                            strExecutionLogMessage = "Found exception while getting Billing/Payable rate Breakdown details for  this file " + fileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "So not able to process this file,Please look into this immediately";
                                            strExecutionLogMessage += "Found exception while processing the file, filename  -" + fileName + System.Environment.NewLine;
                                            strExecutionLogMessage += " " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Error : " + objBPRatesBreakdownResponse.dsResp.Reason;
                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                            string fromMail = objCommon.GetConfigValue("FromMailID");
                                            string fromPassword = objCommon.GetConfigValue("FromMailPasssword");
                                            string disclaimer = objCommon.GetConfigValue("MailDisclaimer");
                                            string toMail = objCommon.GetConfigValue("ToMailID");
                                            string subject = "Found exception while processing the file - " + fileName;
                                            objCommon.SendMail(fromMail, fromPassword, disclaimer, toMail, "", subject, strExecutionLogMessage, "");
                                            throw new NullReferenceException("Found exception while processing the file - " + fileName);
                                        }
                                    }


                                    foreach (DataRow drow in datatable1.Rows)
                                    {
                                        double carrierBasepay = 0;
                                        double billrate = 0;
                                        double carrierFSC = 0;
                                        double billingdeliveryrate = 0;
                                        try
                                        {
                                            ReferenceId = Convert.ToString(drow["Customer Reference"]);
                                            objOrder.reference = Convert.ToString(drow["Customer Reference"]);
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

                                                if (drItems.Table.Columns.Contains("item_name"))
                                                {
                                                    objitems.item_name = Convert.ToString(drItems["item_name"]);
                                                }

                                                if (drItems.Table.Columns.Contains("item_price"))
                                                {
                                                    objitems.item_price = Convert.ToDouble(drItems["item_price"]);
                                                }

                                                if (drItems.Table.Columns.Contains("item_long_desc"))
                                                {
                                                    objitems.item_long_desc = Convert.ToString(drItems["item_long_desc"]);
                                                }

                                                objorder_line_itemList.Add(objitems);
                                            }
                                            objOrder.number_of_pieces = Convert.ToInt32(drItemresult.Length);

                                            objOrder.line_items = objorder_line_itemList;

                                            objOrder.company_number = Convert.ToInt32(drow["Company"]);
                                            objOrder.service_level = Convert.ToInt32(drow["Service Type"]);
                                            objOrder.customer_number = Convert.ToInt32(drow["Billing Customer Number"]);

                                            //  DateTime dtValue = Convert.ToDateTime(dr["Delivery Date"]);

                                            if (drow.Table.Columns.Contains("BOL Number"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(drow["BOL Number"])))
                                                {
                                                    objOrder.bol_number = Convert.ToString(drow["BOL Number"]);
                                                }
                                            }

                                            DateTime dtdeliveryDate = new DateTime();
                                            bool deliverydate = false;

                                            if (drow.Table.Columns.Contains("Delivery Date"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(drow["Delivery Date"])))
                                                {
                                                    DateTime dtValue = Convert.ToDateTime(drow["Delivery Date"]);
                                                    dtdeliveryDate = Convert.ToDateTime(Regex.Replace(dtValue.ToString(), @"\t", ""));
                                                    deliverydate = true;
                                                    objOrder.pickup_requested_date = dtValue.ToString("yyyy-MM-dd");
                                                    objOrder.pickup_actual_date = dtValue.ToString("yyyy-MM-dd");
                                                    objOrder.deliver_requested_date = dtValue.ToString("yyyy-MM-dd");
                                                    objOrder.deliver_actual_date = dtValue.ToString("yyyy-MM-dd");

                                                }
                                            }

                                            if (drow.Table.Columns.Contains("Pickup requested date"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(drow["Pickup requested date"])))
                                                {
                                                    DateTime dtValue = Convert.ToDateTime(drow["Pickup requested date"]);
                                                    objOrder.pickup_requested_date = dtValue.ToString("yyyy-MM-dd");
                                                }
                                            }

                                            if (drow.Table.Columns.Contains("Pickup actual date"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(drow["Pickup actual date"])))
                                                {
                                                    DateTime dtValue = Convert.ToDateTime(drow["Pickup actual date"]);
                                                    objOrder.pickup_actual_date = dtValue.ToString("yyyy-MM-dd");
                                                }
                                            }

                                            if (drow.Table.Columns.Contains("Delivery requested date"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(drow["Delivery requested date"])))
                                                {
                                                    DateTime dtValue = Convert.ToDateTime(drow["Delivery requested date"]);
                                                    objOrder.deliver_requested_date = dtValue.ToString("yyyy-MM-dd");
                                                }
                                            }

                                            if (drow.Table.Columns.Contains("Delivery actual date"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(drow["Delivery actual date"])))
                                                {
                                                    DateTime dtValue = Convert.ToDateTime(drow["Delivery actual date"]);
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
                                                        if (strZip.Contains("-"))
                                                        {
                                                            objOrder.pickup_zip = strZip;
                                                        }
                                                        else
                                                        {
                                                            objOrder.pickup_zip = strZip.Substring(0, 5) + "-" + strZip.Substring(5, strZip.Length - 5); ;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        objOrder.pickup_zip = strZip;
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
                                                        if (strZip.Contains("-"))
                                                        {
                                                            objOrder.deliver_zip = strZip;
                                                        }
                                                        else
                                                        {
                                                            objOrder.deliver_zip = strZip.Substring(0, 5) + "-" + strZip.Substring(5, strZip.Length - 5); ;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        objOrder.deliver_zip = strZip;
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
                                            if (drow.Table.Columns.Contains("rate_buck_amt2"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(drow["rate_buck_amt2"])))
                                                {
                                                    objOrder.rate_buck_amt2 = Convert.ToDouble(drow["rate_buck_amt2"]);
                                                }
                                            }
                                            if (drow.Table.Columns.Contains("rate_buck_amt4"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(drow["rate_buck_amt4"])))
                                                {
                                                    objOrder.rate_buck_amt4 = Convert.ToDouble(drow["rate_buck_amt4"]);
                                                }
                                            }
                                            if (drow.Table.Columns.Contains("rate_buck_amt5"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(drow["rate_buck_amt5"])))
                                                {
                                                    objOrder.rate_buck_amt5 = Convert.ToDouble(drow["rate_buck_amt5"]);
                                                }
                                            }
                                            if (drow.Table.Columns.Contains("rate_buck_amt6"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(drow["rate_buck_amt6"])))
                                                {
                                                    objOrder.rate_buck_amt6 = Convert.ToDouble(drow["rate_buck_amt6"]);
                                                }
                                            }
                                            if (drow.Table.Columns.Contains("rate_buck_amt7"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(drow["rate_buck_amt7"])))
                                                {
                                                    objOrder.rate_buck_amt7 = Convert.ToDouble(drow["rate_buck_amt7"]);
                                                }
                                            }
                                            if (drow.Table.Columns.Contains("rate_buck_amt8"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(drow["rate_buck_amt8"])))
                                                {
                                                    objOrder.rate_buck_amt8 = Convert.ToDouble(drow["rate_buck_amt8"]);
                                                }
                                            }
                                            if (drow.Table.Columns.Contains("rate_buck_amt9"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(drow["rate_buck_amt9"])))
                                                {
                                                    objOrder.rate_buck_amt9 = Convert.ToDouble(drow["rate_buck_amt9"]);
                                                }
                                            }
                                            if (drow.Table.Columns.Contains("rate_buck_amt11"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(drow["rate_buck_amt11"])))
                                                {
                                                    objOrder.rate_buck_amt11 = Convert.ToDouble(drow["rate_buck_amt11"]);
                                                }
                                            }

                                            if (drow.Table.Columns.Contains("Weight"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(drow["Weight"])))
                                                {
                                                    //  objOrder.weight = Convert.ToInt32(Convert.ToDouble(drow["Weight"]));
                                                    objOrder.weight = Convert.ToInt32(Math.Round(Convert.ToDouble(drow["Weight"])));
                                                }
                                            }


                                            if (drow.Table.Columns.Contains("Pieces"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(drow["Pieces"])))
                                                {
                                                    objOrder.number_of_pieces = Convert.ToInt32(drow["Pieces"]);
                                                }
                                                else
                                                {
                                                    // to set billing rates in case of detailed data

                                                    var totalWeight = drItemresult.Sum(r => Convert.ToDouble(r.Field<string>("Weight")));

                                                    objOrder.weight = Convert.ToInt32(Math.Round(Convert.ToDouble(totalWeight)));

                                                    string storenumber = objOrder.deliver_name;
                                                    int band = 0;

                                                    int billingRates_ID = 0;
                                                    //int pieces = 0;
                                                    double weight_pieces = 0;
                                                    bool billrateCalculationBasedOnPieces = false;

                                                    if (!deliverydate)
                                                    {
                                                        dtdeliveryDate = Convert.ToDateTime(Regex.Replace(objOrder.pickup_requested_date.ToString(), @"\t", ""));
                                                    }

                                                    //if (locationCode == "SEA" || locationCode == "PDX")
                                                    //{
                                                    //    DateTime dtSEAandPDXDeliveryDateCalculationBasedOnPieces = Convert.ToDateTime(objCommon.GetConfigValue("BURLSEAandPDXDeliveryDateCalculationBasedOnPieces"));

                                                    //    if (dtdeliveryDate >= dtSEAandPDXDeliveryDateCalculationBasedOnPieces)
                                                    //    {
                                                    //        billrateCalculationBasedOnPieces = true;
                                                    //        string SEAandPDXLocationCodeCalculationBasedOnPieces = objCommon.GetConfigValue("BURLSEAandPDXLocationCodeCalculationBasedOnPieces");
                                                    //        if (SEAandPDXLocationCodeCalculationBasedOnPieces.Split(',').Contains(locationCode))
                                                    //        {
                                                    //            billrateCalculationBasedOnPieces = false;
                                                    //        }
                                                    //    }
                                                    //}
                                                    //else
                                                    //{
                                                    //    DateTime dtOtherThanSEAandPDXDeliveryDateCalculationBasedOnPieces = Convert.ToDateTime(objCommon.GetConfigValue("BURLOtherThanSEAandPDXDeliveryDateCalculationBasedOnPieces"));

                                                    //    if (dtdeliveryDate >= dtOtherThanSEAandPDXDeliveryDateCalculationBasedOnPieces)
                                                    //    {
                                                    //        billrateCalculationBasedOnPieces = true;
                                                    //        string otherThanSEAandPDXLocationCodeCalculationBasedOnPieces = objCommon.GetConfigValue("BURLOtherThanSEAandPDXLocationCodeCalculationBasedOnPieces");
                                                    //        if (otherThanSEAandPDXLocationCodeCalculationBasedOnPieces.Split(',').Contains(locationCode))
                                                    //        {
                                                    //            billrateCalculationBasedOnPieces = false;
                                                    //        }
                                                    //    }
                                                    //}

                                                    string storenumberfordbverification = objOrder.deliver_name;
                                                    if (IsDigitsOnly(storenumber))
                                                    {
                                                        storenumberfordbverification = Convert.ToString(Convert.ToInt32(storenumber));
                                                    }

                                                    string deliveryName = objOrder.deliver_name.Replace("'", "");


                                                    double weight = totalWeight;

                                                    if (billrateCalculationBasedOnPieces)
                                                    {
                                                        weight_pieces = objOrder.number_of_pieces;
                                                    }
                                                    else
                                                    {
                                                        weight_pieces = weight;
                                                    }

                                                    var invCulture = System.Globalization.CultureInfo.InvariantCulture;

                                                    DataTable tblBillRatesFiltered = new DataTable();
                                                    IEnumerable<DataRow> billratesfilteredRows = dtBillingRates.AsEnumerable()
                                                    .Where(row => (row.Field<DateTime>("EffectiveStartDate") <= dtdeliveryDate) && (dtdeliveryDate <= row.Field<DateTime>("EffectiveEndDate")));

                                                    if (billratesfilteredRows.Any())
                                                    {
                                                        tblBillRatesFiltered = billratesfilteredRows.CopyToDataTable();
                                                        billingRates_ID = Convert.ToInt16(tblBillRatesFiltered.Rows[0]["BillingRates_ID"]);
                                                    }

                                                    DataTable tblPayableRatesFiltered = new DataTable();
                                                    IEnumerable<DataRow> payableratesfilteredRows = dtPayableRates.AsEnumerable()
                                                    .Where(row => (row.Field<DateTime>("EffectiveStartDate") <= dtdeliveryDate) && (dtdeliveryDate <= row.Field<DateTime>("EffectiveEndDate")));

                                                    if (payableratesfilteredRows.Any())
                                                    {
                                                        tblPayableRatesFiltered = payableratesfilteredRows.CopyToDataTable();
                                                    }

                                                    DataTable dtstorebandsfiltered = new DataTable();

                                                    //IEnumerable<DataRow> storebandsfilteredRows = dtBBBStoreBands.AsEnumerable()
                                                    //                                      .Where(row => row.Field<string>("Store") == storenumberfordbverification && row.Field<string>("IsActive") == "Y");

                                                    IEnumerable<DataRow> storebandsfilteredRows = dtStoreBands.AsEnumerable()
                                                                              .Where(row => (row.Field<string>("Store") == storenumberfordbverification) && (row.Field<string>("IsActive") == "Y")
                                               && (row.Field<DateTime>("EffectiveStartDate") <= dtdeliveryDate) && (dtdeliveryDate <= row.Field<DateTime>("EffectiveEndDate")));

                                                    if (storebandsfilteredRows.Any())
                                                    {
                                                        dtstorebandsfiltered = storebandsfilteredRows.CopyToDataTable();
                                                    }

                                                    if (dtstorebandsfiltered.Rows.Count > 0)
                                                    {
                                                        band = Convert.ToInt16(dtstorebandsfiltered.Rows[0]["Band"]);
                                                    }


                                                    DataTable dtDeficitWeightRatingfiltered = new DataTable();
                                                    IEnumerable<DataRow> deficitWeightRatingfilteredRows = dtDeficitWeightRating.AsEnumerable()
                                                                                                              .Where(row => (row.Field<int>("Band") == band) && (row.Field<int>("BillingRates_ID") == billingRates_ID)
                                                                                                              && (row.Field<int>("WeightFrom") <= weight_pieces)
                                                                                                              && (weight_pieces <= row.Field<int>("WeightTo")) && row.Field<string>("IsActive") == "Y");


                                                    if (deficitWeightRatingfilteredRows.Any())
                                                    {
                                                        dtDeficitWeightRatingfiltered = deficitWeightRatingfilteredRows.CopyToDataTable();
                                                    }

                                                    if (dtDeficitWeightRatingfiltered.Rows.Count > 0)
                                                    {

                                                        // billrate = Convert.ToDouble(dtDeficitWeightRatingfiltered.Rows[0]["Rate"]);
                                                        // billrate = (weight / 100.00) * Convert.ToDouble(dtDeficitWeightRatingfiltered.Rows[0]["Rate"]);

                                                        if (billrateCalculationBasedOnPieces)
                                                        {
                                                            billrate = objOrder.number_of_pieces * Convert.ToDouble(dtDeficitWeightRatingfiltered.Rows[0]["Rate"]);
                                                        }
                                                        else
                                                        {
                                                            // old logic
                                                            billrate = (weight / 100.00) * Convert.ToDouble(dtDeficitWeightRatingfiltered.Rows[0]["Rate"]);
                                                        }
                                                    }


                                                    DataTable dtDeficitWeightRatingPayablefiltered = new DataTable();
                                                    IEnumerable<DataRow> deficitWeightRatingPayablefilteredRows = dtDeficitWeightRatingPayable.AsEnumerable()
                                                                                                              .Where(row => (row.Field<int>("Band") == band)
                                                                                                              && (row.Field<int>("WeightFrom") <= weight_pieces)
                                                                                                              && (weight_pieces <= row.Field<int>("WeightTo")) && row.Field<string>("IsActive") == "Y");

                                                    if (deficitWeightRatingPayablefilteredRows.Any())
                                                    {
                                                        dtDeficitWeightRatingPayablefiltered = deficitWeightRatingPayablefilteredRows.CopyToDataTable();
                                                    }

                                                    if (dtDeficitWeightRatingPayablefiltered.Rows.Count > 0)
                                                    {
                                                        //carrierBasepay = Convert.ToDouble(dtDeficitWeightRatingPayablefiltered.Rows[0]["Rate"]);
                                                        // carrierBasepay = (weight / 100.00) * Convert.ToDouble(dtDeficitWeightRatingPayablefiltered.Rows[0]["Rate"]);
                                                        if (billrateCalculationBasedOnPieces)
                                                        {
                                                            carrierBasepay = objOrder.number_of_pieces * Convert.ToDouble(dtDeficitWeightRatingPayablefiltered.Rows[0]["Rate"]);
                                                        }
                                                        else
                                                        {
                                                            carrierBasepay = (weight / 100.00) * Convert.ToDouble(dtDeficitWeightRatingPayablefiltered.Rows[0]["Rate"]);
                                                        }
                                                    }

                                                    DataTable tblFSCBillRatesFiltered = new DataTable();
                                                    double fscratePercentage = 0;
                                                    double carrierfscratePercentage = 0;

                                                    string fscratetype = string.Empty;
                                                    string carrierfscratetype = string.Empty;

                                                    //  IEnumerable<DataRow> fscbillratesfilteredRows = dtFSCRates.AsEnumerable()
                                                    //.Where(row => (row.Field<DateTime>("EffectiveStartDate") <= dtdeliveryDate)


                                                    IEnumerable<DataRow> fscbillratesfilteredRows = dtFSCRates.AsEnumerable()
                                                .Where(row => (row.Field<double>("Company") == objOrder.company_number)
                                                && (row.Field<double>("CustomerNumber") == objOrder.customer_number)
                                                && (row.Field<DateTime>("EffectiveStartDate") <= dtdeliveryDate)
                                                && (dtdeliveryDate <= row.Field<DateTime>("EffectiveEndDate")));

                                                    if (fscbillratesfilteredRows.Any())
                                                    {
                                                        tblFSCBillRatesFiltered = fscbillratesfilteredRows.CopyToDataTable();

                                                        decimal fuelcharge = 0;
                                                        if (!string.IsNullOrEmpty(Convert.ToString(tblFSCBillRatesFiltered.Rows[0]["fuelcharge"])))
                                                        {
                                                            fuelcharge = Convert.ToDecimal(Convert.ToString(tblFSCBillRatesFiltered.Rows[0]["fuelcharge"]));
                                                        }

                                                        if (dtFSCRatesfromDB.Rows.Count > 0)
                                                        {
                                                            IEnumerable<DataRow> fscratesfilteredRows = dtFSCRatesfromDB.AsEnumerable()
                                                            .Where(row => (row.Field<decimal>("Start") <= fuelcharge) && (fuelcharge <= row.Field<decimal>("Stop"))
                                                             && row.Field<string>("IsActive") == "Y");

                                                            if (fscratesfilteredRows.Any())
                                                            {
                                                                tblFSCRatesFiltered = fscratesfilteredRows.CopyToDataTable();
                                                                fscratePercentage = Convert.ToDouble(tblFSCRatesFiltered.Rows[0]["Percent_FSCAMount"]);
                                                                fscratetype = Convert.ToString(tblFSCRatesFiltered.Rows[0]["Type"]);
                                                            }
                                                            else
                                                            {

                                                                fscratesfilteredRows = dtFSCRatesfromDB.AsEnumerable()
                                                               .Where(row => (row.Field<decimal>("Start") <= fuelcharge) && (fuelcharge <= row.Field<decimal>("Stop"))
                                                                && row.Field<string>("DeliveryName") is null && row.Field<string>("IsActive") == "Y");
                                                                if (fscratesfilteredRows.Any())
                                                                {
                                                                    tblFSCRatesFiltered = fscratesfilteredRows.CopyToDataTable();
                                                                    fscratePercentage = Convert.ToDouble(tblFSCRatesFiltered.Rows[0]["Percent_FSCAMount"]);
                                                                    fscratetype = Convert.ToString(tblFSCRatesFiltered.Rows[0]["Type"]);
                                                                }
                                                            }
                                                        }
                                                        if (dtCarrierFSCRatesfromDB.Rows.Count > 0)
                                                        {
                                                            IEnumerable<DataRow> CarrierfscratesfilteredRows = dtCarrierFSCRatesfromDB.AsEnumerable()
                                                            .Where(row => (row.Field<decimal>("Start") <= fuelcharge)
                                                            && (fuelcharge <= row.Field<decimal>("Stop"))
                                                            && row.Field<string>("DeliveryName") == deliveryName && row.Field<string>("IsActive") == "Y");

                                                            if (CarrierfscratesfilteredRows.Any())
                                                            {
                                                                tblCarrierFSCRatesFiltered = CarrierfscratesfilteredRows.CopyToDataTable();
                                                                carrierfscratePercentage = Convert.ToDouble(tblCarrierFSCRatesFiltered.Rows[0]["Percent_FSCAMount"]);
                                                                carrierfscratetype = Convert.ToString(tblCarrierFSCRatesFiltered.Rows[0]["Type"]);
                                                            }
                                                            else
                                                            {
                                                                CarrierfscratesfilteredRows = dtCarrierFSCRatesfromDB.AsEnumerable()
                                                            .Where(row => (row.Field<decimal>("Start") <= fuelcharge)
                                                            && (fuelcharge <= row.Field<decimal>("Stop"))
                                                            && row.Field<string>("DeliveryName") is null && row.Field<string>("IsActive") == "Y");
                                                                if (CarrierfscratesfilteredRows.Any())
                                                                {
                                                                    tblCarrierFSCRatesFiltered = CarrierfscratesfilteredRows.CopyToDataTable();
                                                                    carrierfscratePercentage = Convert.ToDouble(tblCarrierFSCRatesFiltered.Rows[0]["Percent_FSCAMount"]);
                                                                    carrierfscratetype = Convert.ToString(tblCarrierFSCRatesFiltered.Rows[0]["Type"]);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {

                                                        strExecutionLogMessage = "Diesel price data not found in this file " + strFscRateDetailsfilepath + System.Environment.NewLine;
                                                        strExecutionLogMessage += "So not able to process this file, please update the fsc sheet with appropriate values";
                                                        strExecutionLogMessage += "Found exception while processing the file, filename  -" + fileName + System.Environment.NewLine;
                                                        strExecutionLogMessage += "Please look into this immediately." + System.Environment.NewLine;
                                                        //  objCommon.WriteExecutionLog(strExecutionLogMessage);
                                                        objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                                        string fromMail = objCommon.GetConfigValue("FromMailID");
                                                        string fromPassword = objCommon.GetConfigValue("FromMailPasssword");
                                                        string disclaimer = objCommon.GetConfigValue("MailDisclaimer");
                                                        string toMail = objCommon.GetConfigValue("SendDieselPriceMissingEmail");
                                                        string subject = "Diesel price data not found in this file " + strFscRateDetailsfilepath;
                                                        objCommon.SendMail(fromMail, fromPassword, disclaimer, toMail, "", subject, strExecutionLogMessage, "");
                                                        throw new NullReferenceException("Diesel price data not found in this file " + strFscRateDetailsfilepath);
                                                    }


                                                    DataTable dtbillingrateBreakDownfiltered = new DataTable();

                                                    if (billrateCalculationBasedOnPieces)
                                                    {
                                                        IEnumerable<DataRow> billingrateBreakDownfilteredRows = dtBillingRatesBreakdown.AsEnumerable()
                                                                                                                       .Where(row => (row.Field<string>("StoreNumber") == storenumberfordbverification) && (row.Field<int>("BillingRates_ID") == billingRates_ID)
                                                                                        && (row.Field<DateTime>("EffectiveStartDate") <= dtdeliveryDate) && (dtdeliveryDate <= row.Field<DateTime>("EffectiveEndDate")));

                                                        if (billingrateBreakDownfilteredRows.Any())
                                                        {
                                                            dtbillingrateBreakDownfiltered = billingrateBreakDownfilteredRows.CopyToDataTable();
                                                        }

                                                        if (dtbillingrateBreakDownfiltered.Rows.Count > 0)
                                                        {
                                                            billingdeliveryrate = Convert.ToDouble(dtbillingrateBreakDownfiltered.Rows[0]["DeliveryRate"]);
                                                        }
                                                        else
                                                        {
                                                            strExecutionLogMessage = "Billing Rate Breakdown Info missing for store number : " + storenumber + " - Band: " + band + " - BillingRates_ID: " + billingRates_ID + System.Environment.NewLine;
                                                            strExecutionLogMessage += "So not able to process this file, please update the fsc sheet with appropriate values";
                                                            strExecutionLogMessage += "Found exception while processing the file, filename  -" + fileName + System.Environment.NewLine;
                                                            strExecutionLogMessage += "Please look into this immediately." + System.Environment.NewLine;
                                                            //  objCommon.WriteExecutionLog(strExecutionLogMessage);
                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);


                                                            string fromMail = objCommon.GetConfigValue("FromMailID");
                                                            string fromPassword = objCommon.GetConfigValue("FromMailPasssword");
                                                            string disclaimer = objCommon.GetConfigValue("MailDisclaimer");
                                                            string toMail = objCommon.GetConfigValue("SendDieselPriceMissingEmail");
                                                            string subject = "Billing Rate Breakdown Info missing found while processing this file" + processingFileName;
                                                            objCommon.SendMail(fromMail, fromPassword, disclaimer, toMail, "", subject, strExecutionLogMessage, "");
                                                            throw new NullReferenceException("Billing Rate Breakdown Info missing while processing this file " + processingFileName);

                                                        }
                                                    }

                                                    if (!string.IsNullOrEmpty(Convert.ToString(fscratePercentage)))
                                                    {
                                                        if (fscratetype.ToString().ToUpper() == "F")
                                                        {
                                                            objOrder.rate_buck_amt10 = Math.Round(Convert.ToDouble(fscratePercentage), 2);
                                                        }
                                                        else
                                                        {
                                                            if (billrateCalculationBasedOnPieces)
                                                            {
                                                                objOrder.rate_buck_amt10 = Math.Round(Convert.ToDouble(billingdeliveryrate * fscratePercentage * objOrder.number_of_pieces) / 100, 2);
                                                            }
                                                            else
                                                            {
                                                                objOrder.rate_buck_amt10 = Math.Round(Convert.ToDouble(billrate * fscratePercentage) / 100, 2);
                                                            }

                                                        }
                                                    }

                                                    if (!string.IsNullOrEmpty(Convert.ToString(carrierfscratePercentage)))
                                                    {
                                                        if (carrierfscratetype.ToString().ToUpper() == "F")
                                                        {
                                                            carrierFSC = Math.Round(Convert.ToDouble(carrierfscratePercentage), 2);
                                                        }
                                                        else
                                                        {
                                                            carrierFSC = Math.Round(Convert.ToDouble(carrierBasepay * carrierfscratePercentage) / 100, 2);
                                                        }
                                                    }



                                                    objOrder.rate_buck_amt1 = billrate;

                                                    if (drow.Table.Columns.Contains("Pieces ACC"))
                                                    {
                                                        if (!string.IsNullOrEmpty(Convert.ToString(drow["Pieces ACC"])))
                                                        {
                                                            objOrder.rate_buck_amt3 = objOrder.number_of_pieces * Convert.ToDouble(drow["Pieces ACC"]);
                                                        }
                                                    }

                                                    if (drow.Table.Columns.Contains("rate_buck_amt2"))
                                                    {
                                                        if (!string.IsNullOrEmpty(Convert.ToString(drow["rate_buck_amt2"])))
                                                        {
                                                            objOrder.rate_buck_amt2 = objOrder.number_of_pieces * Convert.ToDouble(drow["rate_buck_amt2"]);
                                                        }
                                                    }
                                                    if (drow.Table.Columns.Contains("rate_buck_amt4"))
                                                    {
                                                        if (!string.IsNullOrEmpty(Convert.ToString(drow["rate_buck_amt4"])))
                                                        {
                                                            objOrder.rate_buck_amt4 = objOrder.number_of_pieces * Convert.ToDouble(drow["rate_buck_amt4"]);
                                                        }
                                                    }
                                                    if (drow.Table.Columns.Contains("rate_buck_amt5"))
                                                    {
                                                        if (!string.IsNullOrEmpty(Convert.ToString(drow["rate_buck_amt5"])))
                                                        {
                                                            objOrder.rate_buck_amt5 = objOrder.number_of_pieces * Convert.ToDouble(drow["rate_buck_amt5"]);
                                                        }
                                                    }
                                                    if (drow.Table.Columns.Contains("rate_buck_amt6"))
                                                    {
                                                        if (!string.IsNullOrEmpty(Convert.ToString(drow["rate_buck_amt6"])))
                                                        {
                                                            objOrder.rate_buck_amt6 = objOrder.number_of_pieces * Convert.ToDouble(drow["rate_buck_amt6"]);
                                                        }
                                                    }
                                                    if (drow.Table.Columns.Contains("rate_buck_amt7"))
                                                    {
                                                        if (!string.IsNullOrEmpty(Convert.ToString(drow["rate_buck_amt7"])))
                                                        {
                                                            objOrder.rate_buck_amt7 = objOrder.number_of_pieces * Convert.ToDouble(drow["rate_buck_amt7"]);
                                                        }
                                                    }
                                                    if (drow.Table.Columns.Contains("rate_buck_amt8"))
                                                    {
                                                        if (!string.IsNullOrEmpty(Convert.ToString(drow["rate_buck_amt8"])))
                                                        {
                                                            objOrder.rate_buck_amt8 = objOrder.number_of_pieces * Convert.ToDouble(drow["rate_buck_amt8"]);
                                                        }
                                                    }
                                                    if (drow.Table.Columns.Contains("rate_buck_amt9"))
                                                    {
                                                        if (!string.IsNullOrEmpty(Convert.ToString(drow["rate_buck_amt9"])))
                                                        {
                                                            objOrder.rate_buck_amt9 = objOrder.number_of_pieces * Convert.ToDouble(drow["rate_buck_amt9"]);
                                                        }
                                                    }
                                                    if (drow.Table.Columns.Contains("rate_buck_amt11"))
                                                    {
                                                        if (!string.IsNullOrEmpty(Convert.ToString(drow["rate_buck_amt11"])))
                                                        {
                                                            objOrder.rate_buck_amt11 = objOrder.number_of_pieces * Convert.ToDouble(drow["rate_buck_amt11"]);
                                                        }
                                                    }
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

                                            if (drow.Table.Columns.Contains("Pickup special instr long"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(drow["Pickup special instr long"])))
                                                {
                                                    string strpickup_special_instr_long = Convert.ToString(drow["Pickup special instr long"]);
                                                    objOrder.pickup_special_instr_long = "#INPUTAPIFILE:" + strFileName + " " + strpickup_special_instr_long.Trim();
                                                }
                                                else
                                                {
                                                    objOrder.pickup_special_instr_long = "#INPUTAPIFILE:" + strFileName;
                                                }
                                            }
                                            else
                                            {
                                                objOrder.pickup_special_instr_long = "#INPUTAPIFILE:" + strFileName;
                                            }

                                            if (dr.Table.Columns.Contains("add_charge_amt1"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["add_charge_amt1"])))
                                                {
                                                    objOrder.add_charge_amt1 = Convert.ToDouble(dr["add_charge_amt1"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("add_charge_amt2"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["add_charge_amt2"])))
                                                {
                                                    objOrder.add_charge_amt2 = Convert.ToDouble(dr["add_charge_amt2"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("add_charge_amt3"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["add_charge_amt3"])))
                                                {
                                                    objOrder.add_charge_amt3 = Convert.ToDouble(dr["add_charge_amt3"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("add_charge_amt4"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["add_charge_amt4"])))
                                                {
                                                    objOrder.add_charge_amt4 = Convert.ToDouble(dr["add_charge_amt4"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("add_charge_amt5"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["add_charge_amt5"])))
                                                {
                                                    objOrder.add_charge_amt5 = Convert.ToDouble(dr["add_charge_amt5"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("add_charge_amt6"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["add_charge_amt6"])))
                                                {
                                                    objOrder.add_charge_amt6 = Convert.ToDouble(dr["add_charge_amt6"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("add_charge_code1"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["add_charge_code1"])))
                                                {
                                                    objOrder.add_charge_code1 = Convert.ToString(dr["add_charge_code1"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("add_charge_code2"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["add_charge_code2"])))
                                                {
                                                    objOrder.add_charge_code2 = Convert.ToString(dr["add_charge_code2"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("add_charge_code3"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["add_charge_code3"])))
                                                {
                                                    objOrder.add_charge_code3 = Convert.ToString(dr["add_charge_code3"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("add_charge_code4"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["add_charge_code4"])))
                                                {
                                                    objOrder.add_charge_code4 = Convert.ToString(dr["add_charge_code4"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("add_charge_code5"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["add_charge_code5"])))
                                                {
                                                    objOrder.add_charge_code5 = Convert.ToString(dr["add_charge_code5"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("add_charge_code6"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["add_charge_code6"])))
                                                {
                                                    objOrder.add_charge_code6 = Convert.ToString(dr["add_charge_code6"]);
                                                }
                                            }

                                            if (dr.Table.Columns.Contains("status_code"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["status_code"])))
                                                {
                                                    objOrder.status_code = Convert.ToString(dr["status_code"]);
                                                }
                                            }

                                            if (dr.Table.Columns.Contains("pickup_route_code"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["pickup_route_code"])))
                                                {
                                                    objOrder.pickup_route_code = Convert.ToString(dr["pickup_route_code"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("pickup_route_seq"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["pickup_route_seq"])))
                                                {
                                                    objOrder.pickup_route_seq = Convert.ToString(dr["pickup_route_seq"]);
                                                }
                                            }
                                            if (dr.Table.Columns.Contains("pu_arrive_notification_sent"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dr["pu_arrive_notification_sent"])))
                                                {
                                                    objOrder.pu_arrive_notification_sent = Convert.ToString(dr["pu_arrive_notification_sent"]);
                                                }
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            strExecutionLogMessage = "ProcessAddOrderFiles Exception -" + ex.Message + System.Environment.NewLine;
                                            strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                            strExecutionLogMessage += "Found exception while generating the file, filename  -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "For Customer Reference -" + objOrder.reference + System.Environment.NewLine;
                                            objCommon.WriteErrorLogParallelly(ex, fileName, strExecutionLogMessage);

                                            ErrorResponse objErrorResponse = new ErrorResponse();
                                            objErrorResponse.error = ex.Message;
                                            objErrorResponse.status = "Error";
                                            objErrorResponse.code = "Exception while generating the Order Post Request";
                                            objErrorResponse.reference = objOrder.reference;
                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                            dsFailureResponse.Tables[0].TableName = "OrderFailure";
                                            objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                        strInputFilePath, processingFileName, strDatetime);
                                            continue;
                                        }

                                        objorderdetails.order = objOrder;
                                        clsDatatrac objclsDatatrac = new clsDatatrac();
                                        clsCommon.ReturnResponse objresponse = new clsCommon.ReturnResponse();
                                        string request = JsonConvert.SerializeObject(objorderdetails);
                                        string data = Regex.Replace(request, @"\\t", "");
                                        request = Regex.Replace(data, @"\\""", "");
                                        objresponse = objclsDatatrac.CallDataTracOrderPostAPI(objorderdetails);
                                        //objresponse.ResponseVal = true;
                                        // objresponse.Reason = "{\"002018775030\": {\"verified_weight\": null, \"roundtrip_actual_latitude\": null, \"pickup_special_instructions4\": null, \"fuel_miles\": null, \"pickup_dispatch_zone\": null, \"pickup_zip\": \"23150\", \"pickup_actual_arr_time\": \"08:00\", \"cod_accept_company_check\": false, \"add_charge_occur9\": null, \"pickup_omw_latitude\": null, \"service_level_text\": \"Threshold Delivery\", \"service_level\": 57, \"exception_sign_required\": false, \"pickup_phone_ext\": null, \"roundtrip_actual_pieces\": null, \"bringg_send_sms\": false, \"az_equip2\": null, \"hist_inv_date\": null, \"date_order_entered\": \"2021-07-21\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"pickup_city\": \"SANDSTON\", \"pickup_phone\": null, \"pickup_sign_req\": true, \"deliver_route_sequence\": null, \"deliver_phone\": null, \"deliver_omw_longitude\": null, \"roundtrip_actual_longitude\": null, \"page_number\": 1, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"add_charge_code9\": null, \"pickup_eta_time\": null, \"record_type\": 0, \"add_charge_occur11\": null, \"push_partner_order_id\": null, \"deliver_country\": null, \"customer_name\": \"MXD/RYDER\", \"bol_number\": null, \"pickup_latitude\": 37.53250820, \"add_charge_code4\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"pu_arrive_notification_sent\": false, \"distribution_shift_id\": null, \"pickup_special_instr_long\": null, \"id\": \"002018775030\", \"callback_to\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"ordered_by\": \"RYDER\", \"add_charge_code12\": null, \"pickup_route_seq\": null, \"deliver_city\": \"HANOVER\", \"add_charge_occur5\": null, \"edi_acknowledgement_required\": false, \"rescheduled_ctrl_number\": null, \"driver2\": null, \"deliver_room\": null, \"deliver_actual_arr_time\": \"08:00\", \"fuel_price_zone\": null, \"add_charge_amt9\": null, \"add_charge_amt4\": null, \"delivery_address_point_number_text\": \"PAMELA YORK\", \"delivery_address_point_number\": 24254, \"deliver_actual_longitude\": null, \"add_charge_amt2\": null, \"additional_drivers\": false, \"pickup_pricing_zone\": 1, \"hazmat\": false, \"pickup_address\": \"540 EASTPARK CT\", \"pickup_route_code\": null, \"callback_userid\": null, \"pickup_point_customer\": 31025, \"rate_buck_amt1\": 52.00, \"add_charge_amt8\": null, \"callback_time\": null, \"csr\": \"DX*\", \"roundtrip_actual_depart_time\": null, \"customers_etrac_partner_id\": \"96609250\", \"manual_notepad\": false, \"add_charge_code8\": null, \"bringg_order_id\": null, \"deliver_omw_latitude\": null, \"pickup_longitude\": -77.33035820, \"etrac_number\": null, \"distribution_unique_id\": 0, \"vehicle_type\": null, \"roundtrip_actual_arrival_time\": null, \"delivery_longitude\": -77.28769600, \"pu_actual_location_accuracy\": null, \"deliver_actual_date\": \"2021-07-14\", \"exception_timestamp\": null, \"deliver_zip\": \"23069\", \"roundtrip_wait_time\": null, \"add_charge_occur8\": null, \"dl_arrive_notification_sent\": false, \"pickup_special_instructions1\": null, \"ordered_by_phone_number\": null, \"deliver_requested_arr_time\": \"08:00\", \"rate_miles\": null, \"holiday_groups\": null, \"pickup_email_notification_sent\": false, \"add_charge_code3\": null, \"dispatch_id\": null, \"add_charge_occur10\": null, \"dispatch_time\": null, \"deliver_wait_time\": null, \"invoice_period_end_date\": null, \"add_charge_occur12\": null, \"fuel_plan\": null, \"return_svc_level\": null, \"pickup_actual_date\": \"2021-07-14\", \"send_new_order_alert\": false, \"pickup_room\": null, \"rate_buck_amt8\": null, \"add_charge_amt10\": null, \"insurance_amount\": null, \"add_charge_amt3\": null, \"add_charge_amt6\": null, \"pickup_special_instructions3\": null, \"pickup_requested_date\": \"2021-07-14\", \"roundtrip_sign_req\": false, \"actual_miles\": null, \"pickup_address_point_number_text\": \"1STOPBEDROOMS\", \"pickup_address_point_number\": 19845, \"deliver_actual_latitude\": null, \"deliver_phone_ext\": null, \"deliver_route_code\": null, \"add_charge_code10\": null, \"delivery_airport_code\": null, \"reference\": null, \"photos_exist\": false, \"master_airway_bill_number\": null, \"control_number\": 1877503, \"cod_text\": \"No\", \"cod\": \"N\", \"rate_buck_amt11\": null, \"cod_amount\": null, \"pickup_omw_timestamp\": null, \"signature_images\": [], \"deliver_special_instructions1\": null, \"quote_amount\": null, \"total_pages\": 1, \"rate_buck_amt4\": null, \"line_items\": [], \"delivery_latitude\": 37.80844290, \"progress\": [{\"status_time\": \"15:57:00\", \"status_date\": \"2021-07-21\", \"status_text\": \"Entered in carrier's system\"}, {\"status_time\": \"08:30:00\", \"status_date\": \"2021-07-14\", \"status_text\": \"Picked up\"}, {\"status_time\": \"08:15:00\", \"status_date\": \"2021-07-14\", \"status_text\": \"Delivered\"}], \"add_charge_code1\": null, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"deliver_special_instr_long\": null, \"deliver_address\": \"205 HARDWOOD LN\", \"add_charge_occur4\": null, \"deliver_eta_date\": null, \"pickup_actual_dep_time\": \"08:30\", \"deliver_requested_dep_time\": \"17:00\", \"add_charge_code7\": null, \"deliver_actual_dep_time\": \"08:15\", \"cod_accept_cashiers_check\": false, \"bringg_last_loc_sent\": null, \"az_equip3\": null, \"driver1_text\": \"WALT DARBY\", \"driver1\": 3215, \"pickup_actual_latitude\": null, \"add_charge_occur2\": null, \"order_automatically_quoted\": false, \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"hours\": \"15\", \"frequent_caller_id\": null, \"rate_buck_amt6\": null, \"rate_chart_used\": 0, \"deliver_actual_pieces\": null, \"add_charge_code5\": null, \"pickup_omw_longitude\": null, \"notes\": [], \"delivery_point_customer\": 31025, \"add_charge_occur7\": null, \"rate_buck_amt5\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"add_charge_code11\": null, \"pickup_name\": \"1STOPBEDROOMS\", \"callback_date\": null, \"add_charge_code2\": null, \"house_airway_bill_number\": null, \"deliver_name\": \"PAMELA YORK\", \"number_of_pieces\": 1, \"deliver_eta_time\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"rate_special_instructions\": null, \"add_charge_occur3\": null, \"pickup_eta_date\": null, \"deliver_special_instructions4\": null, \"custom_special_instr_long\": null, \"deliver_special_instructions2\": null, \"pickup_signature\": \"SOF\", \"az_equip1\": null, \"add_charge_amt12\": null, \"settlements\": [{\"vendor_invoice_number\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"voucher_date\": null, \"voucher_amount\": null, \"fuel_price_source\": null, \"settlement_bucket6_pct\": null, \"pre_book_percentage\": true, \"settlement_bucket4_pct\": null, \"settlement_period_end_date\": null, \"charge5\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"settlement_bucket5_pct\": null, \"voucher_number\": null, \"charge6\": null, \"settlement_bucket2_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"settlement_pct\": 100.00, \"vendor_employee_numer\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"driver_number_text\": \"WALT DARBY\", \"driver_number\": 3215, \"fuel_plan\": null, \"agents_etrac_partner_id\": null, \"charge2\": null, \"charge3\": null, \"settlement_bucket1_pct\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"id\": \"002018775030D1\", \"record_type\": 0, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"pay_chart_used\": null, \"fuel_price_zone\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"charge1\": null, \"control_number\": 1877503, \"adjustment_type\": null, \"order_date\": \"2021-07-14\", \"time_last_updated\": \"14:57\", \"agent_etrac_transaction_number\": null, \"date_last_updated\": \"2021-07-21\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"settlement_bucket3_pct\": null}], \"calc_add_on_chgs\": false, \"original_schedule_number\": null, \"blocks\": null, \"del_actual_location_accuracy\": null, \"zone_set_used\": 1, \"pickup_country\": null, \"pickup_state\": \"VA\", \"add_charge_amt7\": null, \"email_addresses\": null, \"add_charge_occur1\": null, \"pickup_wait_time\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"distribution_branch_id\": null, \"rate_buck_amt9\": null, \"add_charge_amt1\": null, \"pickup_requested_dep_time\": \"09:00\", \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"deliver_state\": \"VA\", \"deliver_dispatch_zone\": null, \"image_sign_req\": false, \"add_charge_code6\": null, \"deliver_requested_date\": \"2021-07-14\", \"add_charge_amt5\": null, \"time_order_entered\": \"15:57\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"pickup_attention\": null, \"rate_buck_amt7\": null, \"add_charge_occur6\": null, \"fuel_price_source\": null, \"pickup_airport_code\": null, \"rate_buck_amt2\": null, \"rate_buck_amt3\": null, \"deliver_omw_timestamp\": null, \"exception_code\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"weight\": null, \"signature_required\": true, \"rate_buck_amt10\": null, \"hist_inv_number\": 0, \"deliver_pricing_zone\": 1, \"pickup_actual_longitude\": null, \"push_services\": null, \"add_charge_amt11\": null, \"rt_actual_location_accuracy\": null, \"roundtrip_actual_date\": null, \"pickup_requested_arr_time\": null, \"deliver_attention\": null, \"deliver_special_instructions3\": null, \"pickup_actual_pieces\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"roundtrip_signature\": null, \"po_number\": null, \"signature\": \"SOF\", \"pickup_special_instructions2\": null, \"original_ctrl_number\": null, \"previous_ctrl_number\": null}}";
                                        // objresponse.Reason = "{\"002018724440\": {\"verified_weight\": null, \"roundtrip_actual_latitude\": null, \"pickup_special_instructions4\": null, \"fuel_miles\": null, \"pickup_dispatch_zone\": null, \"pickup_zip\": \"23150\", \"pickup_actual_arr_time\": \"08:00\", \"cod_accept_company_check\": false, \"add_charge_occur9\": null, \"pickup_omw_latitude\": null, \"service_level_text\": \"Room of Choice\", \"service_level\": 55, \"exception_sign_required\": false, \"pickup_phone_ext\": null, \"roundtrip_actual_pieces\": null, \"bringg_send_sms\": false, \"az_equip2\": null, \"hist_inv_date\": null, \"date_order_entered\": \"2021-07-08\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"pickup_city\": \"SANDSTON\", \"pickup_phone\": null, \"pickup_sign_req\": true, \"deliver_route_sequence\": null, \"deliver_phone\": null, \"deliver_omw_longitude\": null, \"roundtrip_actual_longitude\": null, \"page_number\": 1, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"add_charge_code9\": null, \"pickup_eta_time\": null, \"record_type\": 0, \"add_charge_occur11\": null, \"push_partner_order_id\": null, \"deliver_country\": null, \"customer_name\": \"MXD/RYDER\", \"bol_number\": null, \"pickup_latitude\": 37.53250820, \"add_charge_code4\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"pu_arrive_notification_sent\": false, \"distribution_shift_id\": null, \"pickup_special_instr_long\": null, \"id\": \"002018724440\", \"callback_to\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"ordered_by\": \"RYDER\", \"add_charge_code12\": null, \"pickup_route_seq\": null, \"deliver_city\": \"CHESAPEAKE\", \"add_charge_occur5\": null, \"edi_acknowledgement_required\": false, \"rescheduled_ctrl_number\": null, \"driver2\": null, \"deliver_room\": null, \"deliver_actual_arr_time\": \"08:00\", \"fuel_price_zone\": null, \"add_charge_amt9\": null, \"add_charge_amt4\": null, \"delivery_address_point_number_text\": \"ANNE BAILEY\", \"delivery_address_point_number\": 26312, \"deliver_actual_longitude\": null, \"add_charge_amt2\": null, \"additional_drivers\": false, \"pickup_pricing_zone\": 1, \"hazmat\": false, \"pickup_address\": \"540 EASTPARK CT\", \"pickup_route_code\": null, \"callback_userid\": null, \"pickup_point_customer\": 31025, \"rate_buck_amt1\": 57.00, \"add_charge_amt8\": null, \"callback_time\": null, \"csr\": \"DX*\", \"roundtrip_actual_depart_time\": null, \"customers_etrac_partner_id\": \"96609250\", \"manual_notepad\": false, \"add_charge_code8\": null, \"bringg_order_id\": null, \"deliver_omw_latitude\": null, \"pickup_longitude\": -77.33035820, \"etrac_number\": null, \"distribution_unique_id\": 0, \"vehicle_type\": null, \"roundtrip_actual_arrival_time\": null, \"delivery_longitude\": -76.34760620, \"pu_actual_location_accuracy\": null, \"deliver_actual_date\": \"2021-06-28\", \"exception_timestamp\": null, \"deliver_zip\": \"23323\", \"roundtrip_wait_time\": null, \"add_charge_occur8\": null, \"dl_arrive_notification_sent\": false, \"pickup_special_instructions1\": null, \"ordered_by_phone_number\": null, \"deliver_requested_arr_time\": \"08:00\", \"rate_miles\": null, \"holiday_groups\": null, \"pickup_email_notification_sent\": false, \"add_charge_code3\": null, \"dispatch_id\": null, \"add_charge_occur10\": null, \"dispatch_time\": null, \"deliver_wait_time\": null, \"invoice_period_end_date\": null, \"add_charge_occur12\": null, \"fuel_plan\": null, \"return_svc_level\": null, \"pickup_actual_date\": \"2021-06-28\", \"send_new_order_alert\": false, \"pickup_room\": null, \"rate_buck_amt8\": null, \"add_charge_amt10\": null, \"insurance_amount\": null, \"add_charge_amt3\": null, \"add_charge_amt6\": null, \"pickup_special_instructions3\": null, \"pickup_requested_date\": \"2021-06-28\", \"roundtrip_sign_req\": false, \"actual_miles\": null, \"pickup_address_point_number_text\": \"BIG LOTS\", \"pickup_address_point_number\": 19864, \"deliver_actual_latitude\": null, \"deliver_phone_ext\": null, \"deliver_route_code\": null, \"add_charge_code10\": null, \"delivery_airport_code\": null, \"reference_text\": \"2125095801\", \"reference\": \"2125095801\", \"photos_exist\": false, \"master_airway_bill_number\": null, \"control_number\": 1872444, \"cod_text\": \"No\", \"cod\": \"N\", \"rate_buck_amt11\": null, \"cod_amount\": null, \"pickup_omw_timestamp\": null, \"signature_images\": [], \"deliver_special_instructions1\": null, \"quote_amount\": null, \"total_pages\": 1, \"rate_buck_amt4\": null, \"line_items\": [], \"delivery_latitude\": 36.78396970, \"progress\": [{\"status_time\": \"06:02:00\", \"status_date\": \"2021-07-08\", \"status_text\": \"Entered in carrier's system\"}, {\"status_time\": \"08:30:00\", \"status_date\": \"2021-06-28\", \"status_text\": \"Picked up\"}, {\"status_time\": \"08:15:00\", \"status_date\": \"2021-06-28\", \"status_text\": \"Delivered\"}], \"add_charge_code1\": null, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"deliver_special_instr_long\": null, \"deliver_address\": \"2920 AARON DR\", \"add_charge_occur4\": null, \"deliver_eta_date\": null, \"pickup_actual_dep_time\": \"08:30\", \"deliver_requested_dep_time\": \"17:00\", \"add_charge_code7\": null, \"deliver_actual_dep_time\": \"08:15\", \"cod_accept_cashiers_check\": false, \"bringg_last_loc_sent\": null, \"az_equip3\": null, \"driver1_text\": \"LAVERT KENDALL MORRIS\", \"driver1\": 3001, \"pickup_actual_latitude\": null, \"add_charge_occur2\": null, \"order_automatically_quoted\": false, \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"hours\": \"15\", \"frequent_caller_id\": null, \"rate_buck_amt6\": null, \"rate_chart_used\": 0, \"deliver_actual_pieces\": null, \"add_charge_code5\": null, \"pickup_omw_longitude\": null, \"notes\": [], \"delivery_point_customer\": 31025, \"add_charge_occur7\": null, \"rate_buck_amt5\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"add_charge_code11\": null, \"pickup_name\": \"BIG LOTS\", \"callback_date\": null, \"add_charge_code2\": null, \"house_airway_bill_number\": null, \"deliver_name\": \"ANNE BAILEY\", \"number_of_pieces\": 3, \"deliver_eta_time\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"rate_special_instructions\": null, \"add_charge_occur3\": null, \"pickup_eta_date\": null, \"deliver_special_instructions4\": null, \"custom_special_instr_long\": null, \"deliver_special_instructions2\": null, \"pickup_signature\": \"SOF\", \"az_equip1\": null, \"add_charge_amt12\": null, \"settlements\": [{\"vendor_invoice_number\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"voucher_date\": null, \"voucher_amount\": null, \"fuel_price_source\": null, \"settlement_bucket6_pct\": null, \"pre_book_percentage\": true, \"settlement_bucket4_pct\": null, \"settlement_period_end_date\": null, \"charge5\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"settlement_bucket5_pct\": null, \"voucher_number\": null, \"charge6\": null, \"settlement_bucket2_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"settlement_pct\": 100.00, \"vendor_employee_numer\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"driver_number_text\": \"LAVERT KENDALL MORRIS\", \"driver_number\": 3001, \"fuel_plan\": null, \"agents_etrac_partner_id\": null, \"charge2\": null, \"charge3\": null, \"settlement_bucket1_pct\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"id\": \"002018724440D1\", \"record_type\": 0, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"pay_chart_used\": null, \"fuel_price_zone\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"charge1\": null, \"control_number\": 1872444, \"adjustment_type\": null, \"order_date\": \"2021-06-28\", \"time_last_updated\": \"05:02\", \"agent_etrac_transaction_number\": null, \"date_last_updated\": \"2021-07-08\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"settlement_bucket3_pct\": null}], \"calc_add_on_chgs\": false, \"original_schedule_number\": null, \"blocks\": null, \"del_actual_location_accuracy\": null, \"zone_set_used\": 1, \"pickup_country\": null, \"pickup_state\": \"VA\", \"add_charge_amt7\": null, \"email_addresses\": null, \"add_charge_occur1\": null, \"pickup_wait_time\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"distribution_branch_id\": null, \"rate_buck_amt9\": null, \"add_charge_amt1\": null, \"pickup_requested_dep_time\": \"09:00\", \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"deliver_state\": \"VA\", \"deliver_dispatch_zone\": null, \"image_sign_req\": false, \"add_charge_code6\": null, \"deliver_requested_date\": \"2021-06-28\", \"add_charge_amt5\": null, \"time_order_entered\": \"06:02\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"pickup_attention\": null, \"rate_buck_amt7\": null, \"add_charge_occur6\": null, \"fuel_price_source\": null, \"pickup_airport_code\": null, \"rate_buck_amt2\": null, \"rate_buck_amt3\": null, \"deliver_omw_timestamp\": null, \"exception_code\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"weight\": null, \"signature_required\": true, \"rate_buck_amt10\": 2.34, \"hist_inv_number\": 0, \"deliver_pricing_zone\": 1, \"pickup_actual_longitude\": null, \"push_services\": null, \"add_charge_amt11\": null, \"rt_actual_location_accuracy\": null, \"roundtrip_actual_date\": null, \"pickup_requested_arr_time\": \"07:00\", \"deliver_attention\": null, \"deliver_special_instructions3\": null, \"pickup_actual_pieces\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"roundtrip_signature\": null, \"po_number\": null, \"signature\": \"SOF\", \"pickup_special_instructions2\": null, \"original_ctrl_number\": null, \"previous_ctrl_number\": null}}";
                                        //  objresponse.Reason = "{\"002018724450\": {\"roundtrip_actual_date\": null, \"notes\": [], \"pickup_phone_ext\": null, \"holiday_groups\": null, \"deliver_eta_time\": null, \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"add_charge_occur4\": null, \"deliver_state\": \"VA\", \"quote_amount\": null, \"cod_text\": \"No\", \"cod\": \"N\", \"additional_drivers\": false, \"rescheduled_ctrl_number\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"pickup_actual_pieces\": null, \"record_type\": 0, \"pickup_special_instr_long\": null, \"pickup_special_instructions3\": null, \"exception_timestamp\": null, \"deliver_actual_arr_time\": \"08:00\", \"house_airway_bill_number\": null, \"deliver_pricing_zone\": 1, \"total_pages\": 1, \"add_charge_occur11\": null, \"deliver_omw_latitude\": null, \"callback_userid\": null, \"rate_buck_amt1\": 57.00, \"pickup_point_customer\": 31025, \"pickup_eta_time\": null, \"add_charge_occur8\": null, \"invoice_period_end_date\": null, \"pickup_special_instructions1\": null, \"rate_buck_amt2\": null, \"pickup_special_instructions4\": null, \"manual_notepad\": false, \"edi_acknowledgement_required\": false, \"pickup_name\": \"BIG LOTS\", \"ordered_by_phone_number\": null, \"add_charge_amt12\": null, \"delivery_point_customer\": 31025, \"deliver_actual_dep_time\": \"08:15\", \"email_addresses\": null, \"pickup_address\": \"540 EASTPARK CT\", \"driver2\": null, \"signature_images\": [], \"rate_buck_amt11\": null, \"delivery_latitude\": 37.48366600, \"pickup_attention\": null, \"date_order_entered\": \"2021-07-08\", \"vehicle_type\": null, \"add_charge_amt9\": null, \"pickup_phone\": null, \"rate_miles\": null, \"customers_etrac_partner_id\": \"96609250\", \"order_type_text\": \"One way\", \"order_type\": \"O\", \"dl_arrive_notification_sent\": false, \"add_charge_code3\": null, \"etrac_number\": null, \"pickup_requested_arr_time\": \"07:00\", \"rate_buck_amt3\": null, \"pickup_actual_dep_time\": \"08:30\", \"line_items\": [], \"pickup_sign_req\": true, \"add_charge_code10\": null, \"deliver_city\": \"LANEXA\", \"fuel_plan\": null, \"add_charge_amt10\": null, \"roundtrip_actual_depart_time\": null, \"control_number\": 1872445, \"pickup_dispatch_zone\": null, \"send_new_order_alert\": false, \"settlements\": [{\"settlement_bucket4_pct\": null, \"charge1\": null, \"date_last_updated\": \"2021-07-08\", \"fuel_price_zone\": null, \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"charge4\": null, \"settlement_period_end_date\": null, \"charge5\": null, \"time_last_updated\": \"05:06\", \"charge6\": null, \"driver_number_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver_number\": 3208, \"control_number\": 1872445, \"settlement_bucket2_pct\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"voucher_date\": null, \"agent_etrac_transaction_number\": null, \"settlement_bucket5_pct\": null, \"record_type\": 0, \"voucher_number\": null, \"voucher_amount\": null, \"pay_chart_used\": null, \"settlement_pct\": 100.00, \"vendor_invoice_number\": null, \"settlement_bucket3_pct\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"pre_book_percentage\": true, \"charge3\": null, \"settlement_bucket6_pct\": null, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"adjustment_type\": null, \"id\": \"002018724450D1\", \"agents_etrac_partner_id\": null, \"fuel_plan\": null, \"fuel_price_source\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"vendor_employee_numer\": null, \"settlement_bucket1_pct\": null, \"order_date\": \"2021-06-28\", \"charge2\": null}], \"deliver_actual_latitude\": null, \"fuel_price_zone\": null, \"verified_weight\": null, \"deliver_requested_dep_time\": \"17:00\", \"pickup_airport_code\": null, \"dispatch_time\": null, \"deliver_attention\": null, \"time_order_entered\": \"06:06\", \"rate_buck_amt4\": null, \"roundtrip_wait_time\": null, \"add_charge_amt2\": null, \"az_equip3\": null, \"progress\": [{\"status_date\": \"2021-07-08\", \"status_text\": \"Entered in carrier's system\", \"status_time\": \"06:06:00\"}, {\"status_date\": \"2021-06-28\", \"status_text\": \"Picked up\", \"status_time\": \"08:30:00\"}, {\"status_date\": \"2021-06-28\", \"status_text\": \"Delivered\", \"status_time\": \"08:15:00\"}], \"page_number\": 1, \"roundtrip_sign_req\": false, \"add_charge_amt1\": null, \"add_charge_code8\": null, \"weight\": null, \"rate_buck_amt6\": null, \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"bringg_send_sms\": false, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"custom_special_instr_long\": null, \"deliver_requested_arr_time\": \"08:00\", \"service_level_text\": \"Room of Choice\", \"service_level\": 55, \"az_equip1\": null, \"add_charge_code4\": null, \"bringg_order_id\": null, \"delivery_address_point_number_text\": \"JOSEPH FESSMAN\", \"delivery_address_point_number\": 26313, \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"deliver_special_instructions1\": null, \"pickup_wait_time\": null, \"add_charge_occur5\": null, \"push_partner_order_id\": null, \"deliver_route_sequence\": null, \"pickup_country\": null, \"pickup_state\": \"VA\", \"original_schedule_number\": null, \"frequent_caller_id\": null, \"distribution_unique_id\": 0, \"fuel_miles\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"rate_buck_amt5\": null, \"exception_sign_required\": false, \"pickup_route_code\": null, \"deliver_dispatch_zone\": null, \"delivery_longitude\": -76.90426400, \"pickup_pricing_zone\": 1, \"zone_set_used\": 1, \"deliver_special_instructions2\": null, \"add_charge_amt3\": null, \"deliver_phone\": null, \"pickup_email_notification_sent\": false, \"add_charge_occur12\": null, \"reference_text\": \"2125617401\", \"reference\": \"2125617401\", \"deliver_requested_date\": \"2021-06-28\", \"deliver_actual_longitude\": null, \"image_sign_req\": false, \"pickup_eta_date\": null, \"deliver_phone_ext\": null, \"pickup_omw_longitude\": null, \"original_ctrl_number\": null, \"pickup_special_instructions2\": null, \"order_automatically_quoted\": false, \"bol_number\": null, \"rate_buck_amt10\": 2.34, \"callback_time\": null, \"hazmat\": false, \"distribution_shift_id\": null, \"pickup_latitude\": 37.53250820, \"ordered_by\": \"RYDER\", \"insurance_amount\": null, \"cod_accept_cashiers_check\": false, \"add_charge_amt4\": null, \"add_charge_code7\": null, \"deliver_actual_pieces\": null, \"deliver_address\": \"15400 STAGE RD\", \"cod_accept_company_check\": false, \"signature\": \"SOF\", \"previous_ctrl_number\": null, \"deliver_zip\": \"23089\", \"deliver_special_instructions3\": null, \"rate_buck_amt7\": null, \"hist_inv_number\": 0, \"callback_date\": null, \"deliver_special_instr_long\": null, \"po_number\": null, \"pickup_actual_arr_time\": \"08:00\", \"pickup_requested_date\": \"2021-06-28\", \"number_of_pieces\": 2, \"dispatch_id\": null, \"photos_exist\": false, \"pickup_actual_latitude\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"id\": \"002018724450\", \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"del_actual_location_accuracy\": null, \"add_charge_occur7\": null, \"add_charge_occur9\": null, \"roundtrip_actual_latitude\": null, \"add_charge_occur6\": null, \"pickup_actual_longitude\": null, \"pickup_omw_timestamp\": null, \"bringg_last_loc_sent\": null, \"add_charge_code5\": null, \"deliver_country\": null, \"master_airway_bill_number\": null, \"pickup_route_seq\": null, \"roundtrip_signature\": null, \"calc_add_on_chgs\": false, \"deliver_actual_date\": \"2021-06-28\", \"cod_amount\": null, \"add_charge_code12\": null, \"rt_actual_location_accuracy\": null, \"rate_chart_used\": 0, \"pickup_longitude\": -77.33035820, \"pickup_signature\": \"SOF\", \"add_charge_amt5\": null, \"pu_arrive_notification_sent\": false, \"pickup_actual_date\": \"2021-06-28\", \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"push_services\": null, \"deliver_eta_date\": null, \"driver1_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver1\": 3208, \"deliver_omw_longitude\": null, \"deliver_wait_time\": null, \"pickup_room\": null, \"deliver_special_instructions4\": null, \"add_charge_amt7\": null, \"az_equip2\": null, \"hours\": \"15\", \"add_charge_code2\": null, \"exception_code\": null, \"roundtrip_actual_pieces\": null, \"rate_special_instructions\": null, \"roundtrip_actual_arrival_time\": null, \"add_charge_occur1\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"delivery_airport_code\": null, \"distribution_branch_id\": null, \"hist_inv_date\": null, \"add_charge_code1\": null, \"pickup_requested_dep_time\": \"09:00\", \"deliver_route_code\": null, \"roundtrip_actual_longitude\": null, \"pickup_city\": \"SANDSTON\", \"rate_buck_amt8\": null, \"pickup_omw_latitude\": null, \"deliver_omw_timestamp\": null, \"rate_buck_amt9\": null, \"deliver_room\": null, \"add_charge_code6\": null, \"add_charge_occur3\": null, \"blocks\": null, \"add_charge_code9\": null, \"actual_miles\": null, \"add_charge_occur10\": null, \"add_charge_code11\": null, \"pickup_address_point_number_text\": \"BIG LOTS\", \"pickup_address_point_number\": 19864, \"customer_name\": \"MXD/RYDER\", \"pu_actual_location_accuracy\": null, \"deliver_name\": \"JOSEPH FESSMAN\", \"add_charge_amt6\": null, \"signature_required\": true, \"csr\": \"DX*\", \"add_charge_amt8\": null, \"callback_to\": null, \"fuel_price_source\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"pickup_zip\": \"23150\", \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"return_svc_level\": null, \"add_charge_amt11\": null, \"add_charge_occur2\": null}}";
                                        // objresponse.Reason = "{\"error\": \"Unable to perform API call object with RecordID:WS_OPORDR-9990111870 - The record may have been deleted.\", \"status\": \"error\", \"code\": \"U.RecordNotFound\"}";
                                        // objresponse.Reason = "{\"999000115280\": {\"deliver_eta_time\": null, \"deliver_special_instructions4\": null, \"del_actual_location_accuracy\": null, \"add_charge_amt8\": null, \"callback_to\": null, \"rate_chart_used\": 1, \"pickup_actual_latitude\": null, \"signature_images\": [], \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"add_charge_occur8\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"hours\": null, \"distribution_shift_id\": null, \"pickup_longitude\": null, \"hist_inv_number\": 0, \"add_charge_amt7\": null, \"dispatch_id\": null, \"dl_arrive_notification_sent\": false, \"add_charge_code8\": null, \"roundtrip_sign_req\": false, \"exception_timestamp\": null, \"pickup_city\": null, \"delivery_airport_code\": null, \"progress\": [{\"status_time\": \"08:34:00\", \"status_date\": \"2022-03-10\", \"status_text\": \"Entered in carrier's system\"}], \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"add_charge_amt2\": null, \"pickup_special_instructions1\": null, \"time_order_entered\": \"08:34\", \"distribution_unique_id\": 0, \"pickup_eta_date\": null, \"rate_buck_amt2\": null, \"rate_buck_amt9\": null, \"deliver_zip\": \"48150\", \"rate_buck_amt7\": null, \"customer_name\": \"TEST\", \"deliver_phone_ext\": null, \"roundtrip_actual_depart_time\": null, \"add_charge_code10\": null, \"add_charge_occur2\": null, \"deliver_state\": \"MI\", \"pickup_wait_time\": null, \"pickup_requested_arr_time\": null, \"csr\": \"RG\", \"add_charge_amt4\": null, \"holiday_groups\": null, \"total_pages\": 1, \"delivery_longitude\": -83.35663860, \"bringg_last_loc_sent\": null, \"deliver_name\": \"TANTARA\", \"deliver_actual_longitude\": null, \"distribution_branch_id\": null, \"deliver_wait_time\": null, \"add_charge_occur11\": null, \"deliver_omw_timestamp\": null, \"add_charge_amt3\": null, \"add_charge_amt10\": null, \"rate_buck_amt3\": null, \"rescheduled_ctrl_number\": null, \"add_charge_occur10\": null, \"deliver_address\": \"31782 ENTERPRISE DR\", \"pickup_latitude\": null, \"rate_buck_amt1\": null, \"pickup_phone\": null, \"pickup_actual_date\": \"2022-01-03\", \"previous_ctrl_number\": null, \"control_number\": 11528, \"rate_buck_amt11\": null, \"fuel_price_source\": null, \"add_charge_code9\": null, \"add_charge_occur3\": null, \"fuel_price_zone\": null, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"rate_buck_amt10\": null, \"add_charge_code12\": null, \"quote_amount\": null, \"deliver_phone\": null, \"ordered_by_phone_number\": null, \"cod_accept_company_check\": false, \"callback_time\": null, \"deliver_dispatch_zone\": null, \"hazmat\": false, \"az_equip2\": null, \"add_charge_occur1\": null, \"pickup_email_notification_sent\": false, \"deliver_requested_dep_time\": null, \"deliver_special_instructions1\": null, \"deliver_actual_date\": null, \"rt_actual_location_accuracy\": null, \"signature_required\": false, \"pickup_attention\": null, \"pu_actual_location_accuracy\": null, \"rate_special_instructions\": null, \"pickup_special_instructions2\": null, \"driver2\": null, \"deliver_route_sequence\": null, \"add_charge_code2\": null, \"pickup_state\": null, \"add_charge_code1\": null, \"deliver_actual_pieces\": null, \"pickup_country\": null, \"signature\": null, \"add_charge_occur12\": null, \"reference_text\": \"FEDX01032022\", \"reference\": \"FEDX01032022\", \"pickup_pricing_zone\": null, \"pickup_route_seq\": null, \"pickup_actual_arr_time\": null, \"date_order_entered\": \"2022-03-10\", \"rate_buck_amt5\": null, \"number_of_pieces\": null, \"add_charge_code11\": null, \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"pickup_omw_timestamp\": null, \"delivery_point_customer\": 1, \"roundtrip_actual_latitude\": null, \"rate_buck_amt4\": null, \"pickup_requested_date\": \"2022-01-03\", \"po_number\": null, \"origin_code_text\": \"Web-Carrier UI\", \"origin_code\": \"X\", \"add_charge_occur7\": null, \"exception_sign_required\": false, \"id\": \"999000115280\", \"pickup_route_code\": null, \"pickup_airport_code\": null, \"roundtrip_actual_date\": null, \"roundtrip_signature\": null, \"roundtrip_actual_pieces\": null, \"pickup_phone_ext\": null, \"deliver_city\": \"LIVONIA\", \"deliver_omw_latitude\": null, \"service_level_text\": \"REGULAR\", \"service_level\": 1, \"order_timeliness_text\": \"Open\", \"order_timeliness\": \"5\", \"roundtrip_actual_arrival_time\": null, \"deliver_actual_arr_time\": null, \"etrac_number\": null, \"add_charge_occur9\": null, \"az_equip1\": null, \"rate_miles\": null, \"frequent_caller_id\": null, \"pickup_sign_req\": false, \"customer_type\": null, \"pickup_omw_latitude\": null, \"actual_miles\": null, \"add_charge_code3\": null, \"deliver_eta_date\": null, \"fuel_miles\": null, \"pickup_special_instructions4\": null, \"house_airway_bill_number\": null, \"vehicle_type\": null, \"cod_accept_cashiers_check\": false, \"settlements\": [], \"pickup_address\": null, \"pickup_room\": null, \"weight\": null, \"pickup_actual_longitude\": null, \"rate_buck_amt8\": null, \"delivery_address_point_number_text\": \"TANTARA\", \"delivery_address_point_number\": 10, \"status_code_text\": \"Entered\", \"status_code\": \"E\", \"master_airway_bill_number\": null, \"delivery_latitude\": 42.36977420, \"bringg_order_id\": null, \"add_charge_code7\": null, \"roundtrip_wait_time\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"cod_text\": \"No\", \"cod\": \"N\", \"deliver_room\": null, \"rate_buck_amt6\": null, \"pu_arrive_notification_sent\": false, \"deliver_actual_latitude\": null, \"blocks\": null, \"callback_userid\": null, \"edi_acknowledgement_required\": false, \"add_charge_code4\": null, \"driver1\": null, \"calc_add_on_chgs\": false, \"fuel_plan\": null, \"add_charge_amt9\": null, \"pickup_point_customer\": 0, \"zone_set_used\": 1, \"exception_code\": null, \"invoice_period_end_date\": null, \"push_partner_order_id\": null, \"verified_weight\": null, \"pickup_actual_pieces\": null, \"notes\": [], \"add_charge_amt12\": null, \"deliver_special_instructions3\": null, \"deliver_special_instructions2\": null, \"customer_number_text\": \"test\", \"customer_number\": 1, \"return_svc_level\": null, \"add_charge_amt1\": null, \"az_equip3\": null, \"image_sign_req\": false, \"pickup_omw_longitude\": null, \"deliver_requested_arr_time\": null, \"pickup_name\": null, \"pickup_address_point_number\": null, \"pickup_signature\": null, \"pickup_special_instructions3\": null, \"original_ctrl_number\": null, \"add_charge_code6\": null, \"deliver_omw_longitude\": null, \"additional_drivers\": false, \"deliver_country\": null, \"add_charge_amt5\": null, \"insurance_amount\": null, \"cod_amount\": null, \"email_addresses\": null, \"pickup_actual_dep_time\": null, \"page_number\": 1, \"dispatch_time\": null, \"callback_date\": null, \"add_charge_occur5\": null, \"add_charge_occur6\": null, \"company_number_text\": \"TEST COMPANY\", \"company_number\": 999, \"pickup_dispatch_zone\": null, \"deliver_attention\": null, \"record_type\": 0, \"deliver_pricing_zone\": 1, \"deliver_requested_date\": \"2022-01-03\", \"push_services\": null, \"add_charge_amt6\": null, \"order_automatically_quoted\": false, \"custom_special_instr_long\": null, \"bol_number\": \"FEDX01032022\", \"hist_inv_date\": null, \"roundtrip_actual_longitude\": null, \"add_charge_amt11\": null, \"bringg_send_sms\": false, \"pickup_special_instr_long\": null, \"ordered_by\": \"DET\", \"deliver_special_instr_long\": null, \"pickup_zip\": null, \"pickup_requested_dep_time\": null, \"deliver_route_code\": null, \"deliver_actual_dep_time\": null, \"customers_etrac_partner_id\": null, \"add_charge_code5\": null, \"photos_exist\": false, \"original_schedule_number\": null, \"add_charge_occur4\": null, \"send_new_order_alert\": false, \"manual_notepad\": false, \"line_items\": [], \"pickup_eta_time\": null, \"_utc_offset\": \"-06:00\"}}";
                                        // objresponse.Reason = "{\"error\": \"Backoffice is currently too busy, please try again later.\", \"status\": \"error\", \"code\": \"E.Busy\"}";

                                        if (objresponse.ResponseVal)
                                        {
                                            strExecutionLogMessage = "OrderPostAPI Success " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                            strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                            // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                            DataSet dsOrderResponse = objCommon.jsonToDataSet(objresponse.Reason, "OrderPost");
                                            var UniqueId = Convert.ToString(dsOrderResponse.Tables["id"].Rows[0]["id"]);

                                            WriteOrderPostOutput(dsOrderResponse, processingFileName, strDatetime, ReferenceId, strInputFilePath, strFileName, fileName);


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
                                                            //  ordersettlementputrequest = ordersettlementputrequest + @"'charge1': " + drow["Carrier Base Pay"] + ",";
                                                            double charge1 = Convert.ToDouble(drow["Carrier Base Pay"]);
                                                            if (drow.Table.Columns.Contains("Pieces"))
                                                            {
                                                                if (string.IsNullOrEmpty(Convert.ToString(drow["Pieces"])))
                                                                {
                                                                    // charge1 = objOrder.number_of_pieces * charge1;
                                                                    charge1 = carrierBasepay;
                                                                }
                                                            }

                                                            ordersettlementputrequest = ordersettlementputrequest + @"'charge1': " + charge1 + ",";
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
                                                            //ordersettlementputrequest = ordersettlementputrequest + @"'charge5': " + drow["Carrier ACC"] + ",";
                                                            double charge5 = Convert.ToDouble(drow["Carrier ACC"]);
                                                            if (drow.Table.Columns.Contains("Pieces"))
                                                            {
                                                                if (string.IsNullOrEmpty(Convert.ToString(drow["Pieces"])))
                                                                {
                                                                    charge5 = objOrder.number_of_pieces * charge5;
                                                                }
                                                            }
                                                            ordersettlementputrequest = ordersettlementputrequest + @"'charge5': " + charge5 + ",";

                                                        }
                                                        //else
                                                        //{
                                                        //    strExecutionLogMessage = "OrderPost-OrderSettlementPut Error " + System.Environment.NewLine;
                                                        //    strExecutionLogMessage += "Carrier ACC Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                        //    strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                        //    objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                        //    // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                        //    ErrorResponse objErrorResponse = new ErrorResponse();
                                                        //    objErrorResponse.error = "Carrier ACC value not found for this record";
                                                        //    objErrorResponse.status = "Error";
                                                        //    objErrorResponse.code = "Carrier ACC value Missing";
                                                        //    objErrorResponse.reference = ReferenceId;
                                                        //    string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                        //    DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                        //    dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                        //    objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                        //strInputFilePath, processingFileName, strDatetime);
                                                        //    continue;
                                                        //}
                                                    }
                                                    //else
                                                    //{
                                                    //    strExecutionLogMessage = "OrderPost-OrderSettlementPut Error " + System.Environment.NewLine;
                                                    //    strExecutionLogMessage += "Carrier ACC Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                    //    strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                    //    objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                    //    // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                    //    ErrorResponse objErrorResponse = new ErrorResponse();
                                                    //    objErrorResponse.error = "Carrier ACC column not found for this record";
                                                    //    objErrorResponse.status = "Error";
                                                    //    objErrorResponse.code = "Carrier ACC column Missing";
                                                    //    objErrorResponse.reference = ReferenceId;
                                                    //    string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                    //    DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                    //    dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                    //    objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                    //strInputFilePath, processingFileName, strDatetime);
                                                    //    continue;
                                                    //}

                                                    if (drow.Table.Columns.Contains("Carrier FSC"))
                                                    {
                                                        if (!string.IsNullOrEmpty(Convert.ToString(drow["Carrier FSC"])))
                                                        {
                                                            //  ordersettlementputrequest = ordersettlementputrequest + @"'charge6': " + Convert.ToDouble(drow["Carrier FSC"]) + ",";
                                                            double charge6 = Convert.ToDouble(drow["Carrier FSC"]);
                                                            if (drow.Table.Columns.Contains("Pieces"))
                                                            {
                                                                if (string.IsNullOrEmpty(Convert.ToString(drow["Pieces"])))
                                                                {
                                                                    // charge6 = objOrder.number_of_pieces * charge6;
                                                                    charge6 = carrierFSC;
                                                                }
                                                            }
                                                            ordersettlementputrequest = ordersettlementputrequest + @"'charge6': " + charge6 + ",";

                                                        }
                                                    }
                                                    if (drow.Table.Columns.Contains("charge2"))
                                                    {
                                                        if (!string.IsNullOrEmpty(Convert.ToString(drow["charge2"])))
                                                        {
                                                            //   ordersettlementputrequest = ordersettlementputrequest + @"'charge2': " + Convert.ToDouble(drow["charge2"]) + ",";
                                                            double charge2 = Convert.ToDouble(drow["charge2"]);
                                                            if (drow.Table.Columns.Contains("Pieces"))
                                                            {
                                                                if (string.IsNullOrEmpty(Convert.ToString(drow["Pieces"])))
                                                                {
                                                                    charge2 = objOrder.number_of_pieces * charge2;
                                                                }
                                                            }
                                                            ordersettlementputrequest = ordersettlementputrequest + @"'charge2': " + charge2 + ",";

                                                        }
                                                    }
                                                    if (drow.Table.Columns.Contains("charge3"))
                                                    {
                                                        if (!string.IsNullOrEmpty(Convert.ToString(drow["charge3"])))
                                                        {
                                                            //  ordersettlementputrequest = ordersettlementputrequest + @"'charge3': " + Convert.ToDouble(drow["charge3"]) + ",";

                                                            double charge3 = Convert.ToDouble(drow["charge3"]);
                                                            if (drow.Table.Columns.Contains("Pieces"))
                                                            {
                                                                if (string.IsNullOrEmpty(Convert.ToString(drow["Pieces"])))
                                                                {
                                                                    charge3 = objOrder.number_of_pieces * charge3;
                                                                }
                                                            }
                                                            ordersettlementputrequest = ordersettlementputrequest + @"'charge3': " + charge3 + ",";

                                                        }
                                                    }

                                                    if (drow.Table.Columns.Contains("charge4"))
                                                    {
                                                        if (!string.IsNullOrEmpty(Convert.ToString(drow["charge4"])))
                                                        {
                                                            // ordersettlementputrequest = ordersettlementputrequest + @"'charge4': " + Convert.ToDouble(drow["charge4"]) + ",";
                                                            double charge4 = Convert.ToDouble(drow["charge4"]);
                                                            if (drow.Table.Columns.Contains("Pieces"))
                                                            {
                                                                if (string.IsNullOrEmpty(Convert.ToString(drow["Pieces"])))
                                                                {
                                                                    charge4 = objOrder.number_of_pieces * charge4;
                                                                }
                                                            }
                                                            ordersettlementputrequest = ordersettlementputrequest + @"'charge4': " + charge4 + ",";
                                                        }
                                                    }

                                                    if (drow.Table.Columns.Contains("settlement_pct"))
                                                    {
                                                        if (!string.IsNullOrEmpty(Convert.ToString(drow["settlement_pct"])))
                                                        {
                                                            double settlement_pct = Convert.ToDouble(drow["settlement_pct"]);
                                                            ordersettlementputrequest = ordersettlementputrequest + @"'settlement_pct': " + settlement_pct + ",";
                                                        }
                                                    }

                                                    ordersettlementputrequest = @"{" + ordersettlementputrequest + "}";
                                                    string order_settlementObject = @"{'order_settlement': " + ordersettlementputrequest + "}";
                                                    JObject jsonobj = JObject.Parse(order_settlementObject);
                                                    request = jsonobj.ToString();

                                                    clsCommon.ReturnResponse objresponseOrdersettlement = new clsCommon.ReturnResponse();
                                                    objresponseOrdersettlement = objclsDatatrac.CallDataTracOrderSettlementPutAPI(UniqueId, order_settlementObject, objorderdetails.order.csr);
                                                    // objresponseOrdersettlement.Reason = "{\"002018724450D1\": {\"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"charge4\": null, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"time_last_updated\": \"05:10\", \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"order_date\": \"2021-06-28\", \"agent_etrac_transaction_number\": null, \"settlement_bucket1_pct\": null, \"settlement_bucket2_pct\": null, \"vendor_employee_numer\": null, \"fuel_price_zone\": null, \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"charge1\": 35.91, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"adjustment_type\": null, \"agents_etrac_partner_id\": null, \"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"vendor_invoice_number\": null, \"charge6\": null, \"settlement_bucket4_pct\": null, \"fuel_plan\": null, \"record_type\": 0, \"voucher_amount\": null, \"id\": \"002018724450D1\", \"date_last_updated\": \"2021-07-08\", \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"voucher_number\": null, \"driver_number_text\": \"RIC GUY WITH A TRUCK 3208\", \"driver_number\": 3208, \"settlement_bucket6_pct\": null, \"settlement_pct\": 100.00, \"charge3\": null, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"voucher_date\": null, \"fuel_price_source\": null, \"pay_chart_used\": null, \"settlement_bucket5_pct\": null, \"charge2\": null, \"control_number\": 1872445, \"settlement_bucket3_pct\": null, \"charge5\": null, \"pre_book_percentage\": true, \"settlement_period_end_date\": null}}";
                                                    //  objresponseOrdersettlement.ResponseVal = true;
                                                    if (objresponseOrdersettlement.ResponseVal)
                                                    {
                                                        strExecutionLogMessage = "OrderPost-OrderSettlementPutAPI Success " + System.Environment.NewLine;
                                                        strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                        strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                                        objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                        //objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                                                        DataSet dsOrderSettlementResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                                                        dsOrderSettlementResponse.Tables[0].TableName = "OrderSettlement";
                                                        WriteOrderSettlementPutOutput(dsOrderSettlementResponse, processingFileName, strDatetime, ReferenceId, strInputFilePath, strFileName, fileName);

                                                        //try
                                                        //{
                                                        //    List<ResponseOrderSettlements> orderSettlementstList = new List<ResponseOrderSettlements>();
                                                        //    for (int i = 0; i < dsOrderSettlementResponse.Tables["OrderSettlement"].Rows.Count; i++)
                                                        //    {
                                                        //        DataTable dt = dsOrderSettlementResponse.Tables["OrderSettlement"];
                                                        //        ResponseOrderSettlements objsettlements = new ResponseOrderSettlements();
                                                        //        //objsettlements.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                        //        if (dt.Columns.Contains("company_number"))
                                                        //        {
                                                        //            objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                        //        }
                                                        //        //objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                        //        //objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                        //        //objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                        //        //objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                        //        //objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                        //        //objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                        //        if (dt.Columns.Contains("order_date"))
                                                        //        {
                                                        //            objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                        //        }
                                                        //        //objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                        //        //objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                        //        //objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                        //        //objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                        //        //objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                        //        //objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                        //        //objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                        //        //objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                        //        //objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                        //        //objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                        //        //objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                        //        //objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                        //        //objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                        //        if (dt.Columns.Contains("driver_company_number"))
                                                        //        {
                                                        //            objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                        //        }
                                                        //        //objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                        //        //objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                        //        //objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                        //        //objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                        //        //objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                        //        //objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                        //        if (dt.Columns.Contains("id"))
                                                        //        {
                                                        //            objsettlements.id = (dt.Rows[i]["id"]);
                                                        //        }
                                                        //        if (dt.Columns.Contains("date_last_updated"))
                                                        //        {
                                                        //            objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                        //        }
                                                        //        //objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                        //        //objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                        //        //objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                        //        //objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                        //        if (dt.Columns.Contains("driver_number"))
                                                        //        {
                                                        //            objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                        //        }
                                                        //        //objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                        //        //objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                        //        //objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                        //        //objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                        //        //objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                        //        //objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                        //        //objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                        //        //objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                        //        //objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                        //        //objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                        //        if (dt.Columns.Contains("control_number"))
                                                        //        {
                                                        //            objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                        //        }
                                                        //        //objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                        //        //objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                        //        //objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                        //        //objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);


                                                        //        //objsettlements.company_number_text = (dt.Rows[i]["company_number_text"]);
                                                        //        if (dt.Columns.Contains("company_number"))
                                                        //        {
                                                        //            objsettlements.company_number = (dt.Rows[i]["company_number"]);
                                                        //        }
                                                        //        //objsettlements.charge4 = dt.Rows[i]["charge4"];
                                                        //        //objsettlements.posting_status_text = (dt.Rows[i]["posting_status_text"]);
                                                        //        //objsettlements.posting_status = (dt.Rows[i]["posting_status"]);
                                                        //        //objsettlements.time_last_updated = (dt.Rows[i]["time_last_updated"]);
                                                        //        //objsettlements.fuel_update_freq_text = (dt.Rows[i]["fuel_update_freq_text"]);
                                                        //        //objsettlements.fuel_update_freq = (dt.Rows[i]["fuel_update_freq"]);
                                                        //        if (dt.Columns.Contains("order_date"))
                                                        //        {
                                                        //            objsettlements.order_date = (dt.Rows[i]["order_date"]);
                                                        //        }
                                                        //        //objsettlements.agent_etrac_transaction_number = (dt.Rows[i]["agent_etrac_transaction_number"]);
                                                        //        //objsettlements.settlement_bucket1_pct = (dt.Rows[i]["settlement_bucket1_pct"]);
                                                        //        //objsettlements.settlement_bucket2_pct = (dt.Rows[i]["settlement_bucket2_pct"]);
                                                        //        //objsettlements.vendor_employee_numer = (dt.Rows[i]["vendor_employee_numer"]);
                                                        //        //objsettlements.fuel_price_zone = (dt.Rows[i]["fuel_price_zone"]);
                                                        //        //objsettlements.agent_accepted_or_rejected_text = (dt.Rows[i]["agent_accepted_or_rejected_text"]);
                                                        //        //objsettlements.agent_accepted_or_rejected = (dt.Rows[i]["agent_accepted_or_rejected"]);
                                                        //        //objsettlements.charge1 = (dt.Rows[i]["charge1"]);
                                                        //        //objsettlements.file_status_text = (dt.Rows[i]["file_status_text"]);
                                                        //        //objsettlements.file_status = (dt.Rows[i]["file_status"]);
                                                        //        //objsettlements.adjustment_type = (dt.Rows[i]["adjustment_type"]);
                                                        //        //objsettlements.agents_etrac_partner_id = (dt.Rows[i]["agents_etrac_partner_id"]);
                                                        //        //objsettlements.driver_company_number_text = (dt.Rows[i]["driver_company_number_text"]);
                                                        //        if (dt.Columns.Contains("driver_company_number"))
                                                        //        {
                                                        //            objsettlements.driver_company_number = (dt.Rows[i]["driver_company_number"]);
                                                        //        }
                                                        //        //objsettlements.vendor_invoice_number = (dt.Rows[i]["vendor_invoice_number"]);
                                                        //        //objsettlements.charge6 = (dt.Rows[i]["charge6"]);
                                                        //        //objsettlements.settlement_bucket4_pct = (dt.Rows[i]["settlement_bucket4_pct"]);
                                                        //        //objsettlements.fuel_plan = (dt.Rows[i]["fuel_plan"]);
                                                        //        //objsettlements.record_type = (dt.Rows[i]["record_type"]);
                                                        //        //objsettlements.voucher_amount = (dt.Rows[i]["voucher_amount"]);
                                                        //        if (dt.Columns.Contains("id"))
                                                        //        {
                                                        //            objsettlements.id = (dt.Rows[i]["id"]);
                                                        //        }
                                                        //        if (dt.Columns.Contains("date_last_updated"))
                                                        //        {
                                                        //            objsettlements.date_last_updated = (dt.Rows[i]["date_last_updated"]);
                                                        //        }
                                                        //        //objsettlements.driver_sequence_text = (dt.Rows[i]["driver_sequence_text"]);
                                                        //        //objsettlements.driver_sequence = (dt.Rows[i]["driver_sequence"]);
                                                        //        //objsettlements.voucher_number = (dt.Rows[i]["voucher_number"]);
                                                        //        //objsettlements.driver_number_text = (dt.Rows[i]["driver_number_text"]);
                                                        //        if (dt.Columns.Contains("driver_number"))
                                                        //        {
                                                        //            objsettlements.driver_number = (dt.Rows[i]["driver_number"]);
                                                        //        }
                                                        //        //objsettlements.settlement_bucket6_pct = (dt.Rows[i]["settlement_bucket6_pct"]);
                                                        //        //objsettlements.settlement_pct = (dt.Rows[i]["settlement_pct"]);
                                                        //        //objsettlements.charge3 = (dt.Rows[i]["charge3"]);
                                                        //        //objsettlements.transaction_type_text = (dt.Rows[i]["transaction_type_text"]);
                                                        //        //objsettlements.transaction_type = (dt.Rows[i]["transaction_type"]);
                                                        //        //objsettlements.voucher_date = (dt.Rows[i]["voucher_date"]);
                                                        //        //objsettlements.fuel_price_source = (dt.Rows[i]["fuel_price_source"]);
                                                        //        //objsettlements.pay_chart_used = (dt.Rows[i]["pay_chart_used"]);
                                                        //        //objsettlements.settlement_bucket5_pct = (dt.Rows[i]["settlement_bucket5_pct"]);
                                                        //        //objsettlements.charge2 = (dt.Rows[i]["charge2"]);
                                                        //        if (dt.Columns.Contains("control_number"))
                                                        //        {
                                                        //            objsettlements.control_number = (dt.Rows[i]["control_number"]);
                                                        //        }
                                                        //        //objsettlements.settlement_bucket3_pct = (dt.Rows[i]["settlement_bucket3_pct"]);
                                                        //        //objsettlements.charge5 = (dt.Rows[i]["charge5"]);
                                                        //        //objsettlements.pre_book_percentage = (dt.Rows[i]["pre_book_percentage"]);
                                                        //        //objsettlements.settlement_period_end_date = (dt.Rows[i]["settlement_period_end_date"]);
                                                        //        orderSettlementstList.Add(objsettlements);
                                                        //    }

                                                        //    objCommon.SaveOutputDataToCsvFileParallely(orderSettlementstList, "OrderSettlements-UpdatedRecord",
                                                        //        processingFileName, strDatetime);
                                                        //}
                                                        //catch (Exception ex)
                                                        //{
                                                        //    strExecutionLogMessage = "OrderPost-OrderSettlementPutAPI Success Exception -" + ex.Message + System.Environment.NewLine;
                                                        //    strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                                        //    strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                                        //    strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                        //    //objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                                                        //    //objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                        //    objCommon.WriteErrorLogParallelly(ex, fileName, strExecutionLogMessage);

                                                        //    ErrorResponse objErrorResponse = new ErrorResponse();
                                                        //    objErrorResponse.error = ex.Message;
                                                        //    objErrorResponse.status = "Error";
                                                        //    objErrorResponse.code = "Exception while writing OrderPost-OrderSettlementPutAPI Success response into csv";
                                                        //    objErrorResponse.reference = ReferenceId;
                                                        //    string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                        //    DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                        //    dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                        //    objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                        //strInputFilePath, processingFileName, strDatetime);
                                                        //}
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

                                            if (dsFailureResponse.Tables[0].Columns.Contains("code"))
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(dsFailureResponse.Tables[0].Rows[0]["code"])))
                                                {
                                                    string code = Convert.ToString(dsFailureResponse.Tables[0].Rows[0]["code"]);
                                                    if (code.Contains("E.Busy"))
                                                    {
                                                        if (dtEBusy.Rows.Count > 0)
                                                        {
                                                            DataTable dtBusy = curDatatable.Select("[Customer Reference]= '" + objOrder.reference + "'").CopyToDataTable();
                                                            for (int row = 0; row < dtBusy.Rows.Count; row++)
                                                            {
                                                                DataRow dr1 = dtEBusy.NewRow();
                                                                for (int column = 0; column < dtBusy.Columns.Count; column++)
                                                                {
                                                                    dr1[column] = dtBusy.Rows[row][column];
                                                                }
                                                                dtEBusy.Rows.Add(dr1.ItemArray);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            dtEBusy = curDatatable.Select("[Customer Reference]= '" + objOrder.reference + "'").CopyToDataTable();
                                                        }
                                                    }
                                                    else if (code.Contains("E.ClientError"))
                                                    {
                                                        string error = Convert.ToString(dsFailureResponse.Tables[0].Rows[0]["error"]);

                                                        if (error.Contains("is not a valid zip/postal code"))
                                                        {

                                                            strExecutionLogMessage = "found Error : " + error + System.Environment.NewLine;
                                                            strExecutionLogMessage += "This is going to try next " + System.Environment.NewLine;
                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);

                                                            objOrder.deliver_state = null;
                                                            objOrder.deliver_zip = null;

                                                            objOrder.pickup_state = null;
                                                            objOrder.pickup_zip = null;

                                                            objorderdetails.order = objOrder;
                                                            // clsDatatrac objclsDatatrac = new clsDatatrac();
                                                            // clsCommon.ReturnResponse objresponse = new clsCommon.ReturnResponse();
                                                            request = JsonConvert.SerializeObject(objorderdetails);
                                                            data = Regex.Replace(request, @"\\t", "");
                                                            request = Regex.Replace(data, @"\\""", "");
                                                            objresponse = objclsDatatrac.CallDataTracOrderPostAPI(objorderdetails);
                                                            if (objresponse.ResponseVal)
                                                            {
                                                                strExecutionLogMessage = "OrderPostAPI Success " + System.Environment.NewLine;
                                                                strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                                strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                                                // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                                objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                                DataSet dsOrderResponse = objCommon.jsonToDataSet(objresponse.Reason, "OrderPost");
                                                                var UniqueId = Convert.ToString(dsOrderResponse.Tables["id"].Rows[0]["id"]);

                                                                WriteOrderPostOutput(dsOrderResponse, processingFileName, strDatetime, ReferenceId, strInputFilePath, strFileName, fileName);

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
                                                                                //  ordersettlementputrequest = ordersettlementputrequest + @"'charge1': " + drow["Carrier Base Pay"] + ",";
                                                                                double charge1 = Convert.ToDouble(drow["Carrier Base Pay"]);
                                                                                if (drow.Table.Columns.Contains("Pieces"))
                                                                                {
                                                                                    if (string.IsNullOrEmpty(Convert.ToString(drow["Pieces"])))
                                                                                    {
                                                                                        // charge1 = objOrder.number_of_pieces * charge1;
                                                                                        charge1 = carrierBasepay;
                                                                                    }
                                                                                }
                                                                                ordersettlementputrequest = ordersettlementputrequest + @"'charge1': " + charge1 + ",";
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
                                                                                dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
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
                                                                            dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                                            dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                                            objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                                        strInputFilePath, processingFileName, strDatetime);
                                                                            continue;

                                                                        }
                                                                        if (drow.Table.Columns.Contains("Carrier ACC"))
                                                                        {
                                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Carrier ACC"])))
                                                                            {
                                                                                //ordersettlementputrequest = ordersettlementputrequest + @"'charge5': " + drow["Carrier ACC"] + ",";
                                                                                double charge5 = Convert.ToDouble(drow["Carrier ACC"]);
                                                                                if (drow.Table.Columns.Contains("Pieces"))
                                                                                {
                                                                                    if (string.IsNullOrEmpty(Convert.ToString(drow["Pieces"])))
                                                                                    {
                                                                                        charge5 = objOrder.number_of_pieces * charge5;
                                                                                    }
                                                                                }
                                                                                ordersettlementputrequest = ordersettlementputrequest + @"'charge5': " + charge5 + ",";

                                                                            }
                                                                            //else
                                                                            //{
                                                                            //    strExecutionLogMessage = "OrderPost-OrderSettlementPut Error " + System.Environment.NewLine;
                                                                            //    strExecutionLogMessage += "Carrier ACC Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                                            //    strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                                            //    objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                                            //    // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                                            //    ErrorResponse objErrorResponse = new ErrorResponse();
                                                                            //    objErrorResponse.error = "Carrier ACC value not found for this record";
                                                                            //    objErrorResponse.status = "Error";
                                                                            //    objErrorResponse.code = "Carrier ACC value Missing";
                                                                            //    objErrorResponse.reference = ReferenceId;
                                                                            //    string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                                            //    dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                                            //    dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                                            //    objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                                            //strInputFilePath, processingFileName, strDatetime);
                                                                            //    continue;
                                                                            //}
                                                                        }
                                                                        //else
                                                                        //{
                                                                        //    strExecutionLogMessage = "OrderPost-OrderSettlementPut Error " + System.Environment.NewLine;
                                                                        //    strExecutionLogMessage += "Carrier ACC Not found in the sheet -" + strFileName + System.Environment.NewLine;
                                                                        //    strExecutionLogMessage += "For Customer Reference -" + ReferenceId + System.Environment.NewLine;
                                                                        //    objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                                        //    // objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                                                                        //    ErrorResponse objErrorResponse = new ErrorResponse();
                                                                        //    objErrorResponse.error = "Carrier ACC column not found for this record";
                                                                        //    objErrorResponse.status = "Error";
                                                                        //    objErrorResponse.code = "Carrier ACC column Missing";
                                                                        //    objErrorResponse.reference = ReferenceId;
                                                                        //    string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                                        //    dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                                        //    dsFailureResponse.Tables[0].TableName = "OrderSettlementFailure";
                                                                        //    objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                                        //strInputFilePath, processingFileName, strDatetime);
                                                                        //    continue;
                                                                        //}

                                                                        if (drow.Table.Columns.Contains("Carrier FSC"))
                                                                        {
                                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["Carrier FSC"])))
                                                                            {
                                                                                //  ordersettlementputrequest = ordersettlementputrequest + @"'charge6': " + Convert.ToDouble(drow["Carrier FSC"]) + ",";
                                                                                double charge6 = Convert.ToDouble(drow["Carrier FSC"]);
                                                                                if (drow.Table.Columns.Contains("Pieces"))
                                                                                {
                                                                                    if (string.IsNullOrEmpty(Convert.ToString(drow["Pieces"])))
                                                                                    {
                                                                                        charge6 = objOrder.number_of_pieces * charge6;
                                                                                    }
                                                                                }
                                                                                ordersettlementputrequest = ordersettlementputrequest + @"'charge6': " + charge6 + ",";

                                                                            }
                                                                        }
                                                                        if (drow.Table.Columns.Contains("charge2"))
                                                                        {
                                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["charge2"])))
                                                                            {
                                                                                //   ordersettlementputrequest = ordersettlementputrequest + @"'charge2': " + Convert.ToDouble(drow["charge2"]) + ",";
                                                                                double charge2 = Convert.ToDouble(drow["charge2"]);
                                                                                if (drow.Table.Columns.Contains("Pieces"))
                                                                                {
                                                                                    if (string.IsNullOrEmpty(Convert.ToString(drow["Pieces"])))
                                                                                    {
                                                                                        charge2 = objOrder.number_of_pieces * charge2;
                                                                                    }
                                                                                }
                                                                                ordersettlementputrequest = ordersettlementputrequest + @"'charge2': " + charge2 + ",";

                                                                            }
                                                                        }
                                                                        if (drow.Table.Columns.Contains("charge3"))
                                                                        {
                                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["charge3"])))
                                                                            {
                                                                                //  ordersettlementputrequest = ordersettlementputrequest + @"'charge3': " + Convert.ToDouble(drow["charge3"]) + ",";

                                                                                double charge3 = Convert.ToDouble(drow["charge3"]);
                                                                                if (drow.Table.Columns.Contains("Pieces"))
                                                                                {
                                                                                    if (string.IsNullOrEmpty(Convert.ToString(drow["Pieces"])))
                                                                                    {
                                                                                        charge3 = objOrder.number_of_pieces * charge3;
                                                                                    }
                                                                                }
                                                                                ordersettlementputrequest = ordersettlementputrequest + @"'charge3': " + charge3 + ",";

                                                                            }
                                                                        }

                                                                        if (drow.Table.Columns.Contains("charge4"))
                                                                        {
                                                                            if (!string.IsNullOrEmpty(Convert.ToString(drow["charge4"])))
                                                                            {
                                                                                // ordersettlementputrequest = ordersettlementputrequest + @"'charge4': " + Convert.ToDouble(drow["charge4"]) + ",";
                                                                                double charge4 = Convert.ToDouble(drow["charge4"]);
                                                                                if (drow.Table.Columns.Contains("Pieces"))
                                                                                {
                                                                                    if (string.IsNullOrEmpty(Convert.ToString(drow["Pieces"])))
                                                                                    {
                                                                                        charge4 = objOrder.number_of_pieces * charge4;
                                                                                    }
                                                                                }
                                                                                ordersettlementputrequest = ordersettlementputrequest + @"'charge4': " + charge4 + ",";
                                                                            }
                                                                        }

                                                                        ordersettlementputrequest = @"{" + ordersettlementputrequest + "}";
                                                                        string order_settlementObject = @"{'order_settlement': " + ordersettlementputrequest + "}";
                                                                        JObject jsonobj = JObject.Parse(order_settlementObject);
                                                                        request = jsonobj.ToString();

                                                                        clsCommon.ReturnResponse objresponseOrdersettlement = new clsCommon.ReturnResponse();
                                                                        objresponseOrdersettlement = objclsDatatrac.CallDataTracOrderSettlementPutAPI(UniqueId, order_settlementObject, objorderdetails.order.csr);
                                                                        if (objresponseOrdersettlement.ResponseVal)
                                                                        {
                                                                            strExecutionLogMessage = "OrderPost-OrderSettlementPutAPI Success " + System.Environment.NewLine;
                                                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                                            strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
                                                                            DataSet dsOrderSettlementResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                                                                            dsOrderSettlementResponse.Tables[0].TableName = "OrderSettlement";
                                                                            WriteOrderSettlementPutOutput(dsOrderSettlementResponse, processingFileName, strDatetime, ReferenceId, strInputFilePath, strFileName, fileName);

                                                                        }
                                                                        else
                                                                        {
                                                                            strExecutionLogMessage = "OrderPost-OrderSettlementPutAPI Failed " + System.Environment.NewLine;
                                                                            strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                                                            strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                                                                            objCommon.WriteExecutionLogParallelly(fileName, strExecutionLogMessage);
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
                                                                dsFailureResponse = objCommon.jsonToDataSet(objresponse.Reason);
                                                                dsFailureResponse.Tables[0].TableName = "OrderFailure";
                                                                dsFailureResponse.Tables[0].Columns.Add("Customer Reference", typeof(System.String));
                                                                foreach (DataRow row in dsFailureResponse.Tables[0].Rows)
                                                                {
                                                                    row["Customer Reference"] = objOrder.reference;
                                                                }
                                                                objCommon.WriteDataToCsvFileParallely(dsFailureResponse.Tables[0],
                                                            strInputFilePath, processingFileName, strDatetime);

                                                                if (dsFailureResponse.Tables[0].Columns.Contains("code"))
                                                                {
                                                                    if (!string.IsNullOrEmpty(Convert.ToString(dsFailureResponse.Tables[0].Rows[0]["code"])))
                                                                    {
                                                                        code = Convert.ToString(dsFailureResponse.Tables[0].Rows[0]["code"]);
                                                                        if (code.Contains("E.Busy"))
                                                                        {
                                                                            if (dtEBusy.Rows.Count > 0)
                                                                            {
                                                                                DataTable dtBusy = curDatatable.Select("[Customer Reference]= '" + objOrder.reference + "'").CopyToDataTable();
                                                                                for (int row = 0; row < dtBusy.Rows.Count; row++)
                                                                                {
                                                                                    DataRow dr1 = dtEBusy.NewRow();
                                                                                    for (int column = 0; column < dtBusy.Columns.Count; column++)
                                                                                    {
                                                                                        dr1[column] = dtBusy.Rows[row][column];
                                                                                    }
                                                                                    dtEBusy.Rows.Add(dr1.ItemArray);
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                dtEBusy = curDatatable.Select("[Customer Reference]= '" + objOrder.reference + "'").CopyToDataTable();
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
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

                if (dtEBusy.Rows.Count > 0)
                {
                    dtEBusy.TableName = "Template";
                    int noofrowsperdatatable = 0;
                    List<DataTable> splitdatattable = clsCommon.SplitTable(dtEBusy, noofrowsperdatatable, strFileName, strDatetime);
                    foreach (DataTable dataTable in splitdatattable)
                    {
                        string strfilename = dataTable.TableName;
                        dataTable.TableName = "Template";
                        objCommon.ExportDataTableToXLSX(dataTable, strInputFilePath, strfilename);
                    }
                }



                strExecutionLogMessage = "Parallelly Processing  finished for the  file : " + strFileName + "." + System.Environment.NewLine;
                objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);


            }
            catch (Exception ex)
            {
                // MsgBox(ex.Message).ToString()
                objCommon.WriteErrorLog(ex);
                throw new Exception("Error in ProcessAddOrderFiles -->" + ex.Message + ex.StackTrace);
            }
        }

    }
}
