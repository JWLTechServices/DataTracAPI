using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DatatracAPIOrder_OrderSettlement
{
    class clsRoute : clsCommon
    {
        public ReturnResponse CallDataTracRouteHeaderPostAPI(string jsonreq)
        {
            ReturnResponse objresponse = new ReturnResponse();

            string json = string.Empty;
            clsCommon objCommon = new clsCommon();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                    string url = objCommon.GetConfigValue("DatatracURL") + "/route_header";
                    client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var Username = objCommon.GetConfigValue("DatatracUserName");
                    var Password = objCommon.GetConfigValue("DatatracPassword");

                    UTF8Encoding utf8 = new UTF8Encoding();

                    byte[] encodedBytes = utf8.GetBytes(Username + ":" + Password);
                    string userCredentialsEncoding = Convert.ToBase64String(encodedBytes);
                    client.DefaultRequestHeaders.Add("Authorization", "Basic " + userCredentialsEncoding);
                    JObject jsonobj = JObject.Parse(jsonreq);
                    string payload = jsonobj.ToString();
                    //string payload = JsonConvert.SerializeObject(objheaderdetails);
                    using (var content = new StringContent(payload, Encoding.UTF8, "application/json"))
                    {
                        content.Headers.ContentType.CharSet = "UTF-8";
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        var response = client.PostAsync(url, content).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            objresponse.Reason = response.Content.ReadAsStringAsync().Result;
                            objresponse.ResponseVal = true;
                        }
                        else
                        {
                            objresponse.ResponseVal = false;
                            objresponse.Reason = response.Content.ReadAsStringAsync().Result;
                            string strExecutionLogMessage = "Datatrac response failed for the customer reference number ";// + objorderdetails.order.reference + System.Environment.NewLine;
                            strExecutionLogMessage += "Request:" + payload + System.Environment.NewLine;
                            strExecutionLogMessage += "Response:" + objresponse.Reason;
                            objCommon.WriteExecutionLog(objCommon.GetConfigValue("ExecutionLogFileLocation"), strExecutionLogMessage);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "exception in CallDataTracRouteHeaderPostAPI " + ex;
                objresponse.Reason = strExecutionLogMessage;
                objresponse.ResponseVal = false;
                objCommon.WriteExecutionLog(objCommon.GetConfigValue("ExecutionLogFileLocation"), strExecutionLogMessage);
                objCommon.WriteErrorLog(ex, strExecutionLogMessage);
            }
            return objresponse;
        }

        public ReturnResponse CallDataTracRouteStopPostAPI(string jsonreq)
        {
            ReturnResponse objresponse = new ReturnResponse();

            string json = string.Empty;
            clsCommon objCommon = new clsCommon();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                    string url = objCommon.GetConfigValue("DatatracURL") + "/route_stop";
                    client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var Username = objCommon.GetConfigValue("DatatracUserName");
                    var Password = objCommon.GetConfigValue("DatatracPassword");

                    UTF8Encoding utf8 = new UTF8Encoding();

                    byte[] encodedBytes = utf8.GetBytes(Username + ":" + Password);
                    string userCredentialsEncoding = Convert.ToBase64String(encodedBytes);
                    client.DefaultRequestHeaders.Add("Authorization", "Basic " + userCredentialsEncoding);
                    JObject jsonobj = JObject.Parse(jsonreq);
                    string payload = jsonobj.ToString();
                    //string payload = JsonConvert.SerializeObject(objheaderdetails);
                    using (var content = new StringContent(payload, Encoding.UTF8, "application/json"))
                    {
                        content.Headers.ContentType.CharSet = "UTF-8";
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        var response = client.PostAsync(url, content).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            objresponse.Reason = response.Content.ReadAsStringAsync().Result;
                            objresponse.ResponseVal = true;
                        }
                        else
                        {
                            objresponse.ResponseVal = false;
                            objresponse.Reason = response.Content.ReadAsStringAsync().Result;
                            string strExecutionLogMessage = "Datatrac response failed for the customer reference number ";// + objorderdetails.order.reference + System.Environment.NewLine;
                            strExecutionLogMessage += "Request:" + payload + System.Environment.NewLine;
                            strExecutionLogMessage += "Response:" + objresponse.Reason;
                            objCommon.WriteExecutionLog(objCommon.GetConfigValue("ExecutionLogFileLocation"), strExecutionLogMessage);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "exception in CallDataTracRouteStopPostAPI " + ex;
                objresponse.Reason = strExecutionLogMessage;
                objresponse.ResponseVal = false;
                objCommon.WriteExecutionLog(objCommon.GetConfigValue("ExecutionLogFileLocation"), strExecutionLogMessage);
                objCommon.WriteErrorLog(ex, strExecutionLogMessage);
            }
            return objresponse;
        }


        public ReturnResponse CallDataTracRouteStopPutAPI(string UniqueId, string jsonreq)
        {
            ReturnResponse objresponse = new ReturnResponse();

            string json = string.Empty;
            clsCommon objCommon = new clsCommon();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                   // string url = objCommon.GetConfigValue("DatatracURL") + "/route_stop";
                    string url = objCommon.GetConfigValue("DatatracURL") + "/route_stop/" + UniqueId;
                    client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var Username = objCommon.GetConfigValue("DatatracUserName");
                    var Password = objCommon.GetConfigValue("DatatracPassword");

                    UTF8Encoding utf8 = new UTF8Encoding();

                    byte[] encodedBytes = utf8.GetBytes(Username + ":" + Password);
                    string userCredentialsEncoding = Convert.ToBase64String(encodedBytes);
                    client.DefaultRequestHeaders.Add("Authorization", "Basic " + userCredentialsEncoding);
                    JObject jsonobj = JObject.Parse(jsonreq);
                    string payload = jsonobj.ToString();
                    //string payload = JsonConvert.SerializeObject(objheaderdetails);
                    using (var content = new StringContent(payload, Encoding.UTF8, "application/json"))
                    {
                        content.Headers.ContentType.CharSet = "UTF-8";
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        var response = client.PutAsync(url, content).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            objresponse.Reason = response.Content.ReadAsStringAsync().Result;
                            objresponse.ResponseVal = true;
                        }
                        else
                        {
                            objresponse.ResponseVal = false;
                            objresponse.Reason = response.Content.ReadAsStringAsync().Result;
                            string strExecutionLogMessage = "Datatrac response failed for the customer reference number ";// + objorderdetails.order.reference + System.Environment.NewLine;
                            strExecutionLogMessage += "Request:" + payload + System.Environment.NewLine;
                            strExecutionLogMessage += "Response:" + objresponse.Reason;
                            objCommon.WriteExecutionLog(objCommon.GetConfigValue("ExecutionLogFileLocation"), strExecutionLogMessage);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "exception in CallDataTracRouteStopPutAPI " + ex;
                objresponse.Reason = strExecutionLogMessage;
                objresponse.ResponseVal = false;
                objCommon.WriteExecutionLog(objCommon.GetConfigValue("ExecutionLogFileLocation"), strExecutionLogMessage);
                objCommon.WriteErrorLog(ex, strExecutionLogMessage);
            }
            return objresponse;
        }

        public DSResponse GetRouteStopDetails(string CustomerName, string LocationCode, string ProductCode)
        {
            DSResponse objResponse = new DSResponse();
            try
            {
                DataSet dsDtls = new DataSet();

                SqlParameter paramCustomerName = new SqlParameter("@CustomerName", SqlDbType.VarChar);
                paramCustomerName.Value = CustomerName;

                SqlParameter paramLocationCode = new SqlParameter("@LocationCode", SqlDbType.VarChar);
                paramLocationCode.Value = LocationCode;

                SqlParameter paramProductCode = new SqlParameter("@ProductCode", SqlDbType.VarChar);
                paramProductCode.Value = ProductCode;

                //SqlParameter paramServiceType = new SqlParameter("@ServiceType", SqlDbType.VarChar);
                //paramServiceType.Value = ServiceType;

                dsDtls = SqlHelper.ExecuteDataset(GetConfigValue("DBConnection"), CommandType.StoredProcedure, "USP_S_ROUTESTOP_CUSTOMERMAPPING",
                    paramCustomerName, paramLocationCode, paramProductCode);
                if (dsDtls.Tables[0].Rows.Count > 0)
                {
                    objResponse.DS = dsDtls;
                    objResponse.dsResp.ResponseVal = true;
                }
                else
                {
                    objResponse.dsResp.ResponseVal = false;
                    objResponse.dsResp.Reason = "No Customer Details Found";
                }
            }
            catch (Exception ex)
            {
                objResponse.dsResp.ResponseVal = false;
                WriteErrorLog(ex, "GetRouteStopDetails");
                LogEvents(ex, "GetRouteStopDetails", System.Diagnostics.EventLogEntryType.Error, 0, 1);
            }
            return objResponse;
        }

        public DSResponse GetServiceTypeDetails(string CompanyNumber, string CustomerNumber)
        {
            DSResponse objResponse = new DSResponse();
            try
            {
                DataSet dsDtls = new DataSet();

                SqlParameter paramCompanyNumber = new SqlParameter("@CompanyNumber", SqlDbType.Int);
                paramCompanyNumber.Value = CompanyNumber;

                SqlParameter paramCustomerNumber = new SqlParameter("@CustomerNumber", SqlDbType.VarChar);
                paramCustomerNumber.Value = CustomerNumber;
                
                dsDtls = SqlHelper.ExecuteDataset(GetConfigValue("DBConnection"), CommandType.StoredProcedure, "USP_S_SERVICETYPE_CUSTOMERMAPPING",
                    paramCompanyNumber, paramCustomerNumber);
                if (dsDtls.Tables[0].Rows.Count > 0)
                {
                    objResponse.DS = dsDtls;
                    objResponse.dsResp.ResponseVal = true;
                }
                else
                {
                    objResponse.dsResp.ResponseVal = false;
                    objResponse.dsResp.Reason = "No Service Type Details Found";
                }
            }
            catch (Exception ex)
            {
                objResponse.dsResp.ResponseVal = false;
                WriteErrorLog(ex, "GetServiceTypeDetails");
                LogEvents(ex, "GetServiceTypeDetails", System.Diagnostics.EventLogEntryType.Error, 0, 1);
            }
            return objResponse;
        }
    }

    public class route_headerdetails
    {
        public route_header route_header { get; set; }
    }
    public class route_header
    {
        public string company_number { get; set; }
        public string route_date { get; set; }
        public string route_code { get; set; }
        public string billing_level { get; set; }
        public string billing_method { get; set; }
        public string labor_allocation_method { get; set; }
        public string overhead_allocation_method { get; set; }
        public string route_service_method { get; set; }
        public string route_type { get; set; }
        public string settlement_level { get; set; }
        public string shipper_type { get; set; }
        public string vehicle_allocation_method { get; set; }


        //public double actual_billing_amount { get; set; }
        //public double actual_cost_allocation { get; set; }
        //public int actual_driver_agent { get; set; }
        //public int actual_miles { get; set; }
        //public double actual_settlement_amount { get; set; }
        //public int actual_stops { get; set; }
        //public int actual_total_pieces { get; set; }
        //public int actual_total_weight { get; set; }
        //public string actual_vehicle { get; set; }
        //public int amazon_order_number { get; set; }
        //public int assigned_driver_agent { get; set; }
        //public string assigned_vehicle { get; set; }
        //public int az_equip1 { get; set; }
        //public int az_equip2 { get; set; }
        //public int az_equip3 { get; set; }

        //public string break_time { get; set; }
        //public bool calc_eta { get; set; }
        //public string close_time { get; set; }
        //public string created_by { get; set; }
        //public string created_date { get; set; }
        //public string created_time { get; set; }
        //public string dispatcher_id { get; set; }
        //public string end_date { get; set; }
        //public string end_location { get; set; }
        //public string end_time { get; set; }
        //public string ending_location { get; set; }
        //public string expire_time { get; set; }
        //public string hours { get; set; }
        //// public List<route_stops> route_stops { get; set; }
        //public double labor_cost { get; set; }
        //public string last_updated_by { get; set; }
        //public string last_updated_date { get; set; }
        //public string last_updated_time { get; set; }
        //public int miles { get; set; }
        //public List<notes> notes { get; set; }
        //public string open_time { get; set; }

        //public double overhead_cost { get; set; }
        //public double per_stop_billing_amount { get; set; }
        //public double per_stop_settlement_amount { get; set; }
        //public string posted_by { get; set; }
        //public string posted_date { get; set; }
        //public int posted_status { get; set; }
        //public string posted_time { get; set; }
        //public string push_services { get; set; }
        //public double route_addon_amount { get; set; }
        //public int route_closed { get; set; }
        //public string route_comments { get; set; }
        //public string route_late_start_code { get; set; }

        //public string rtn_trans_route { get; set; }
        //public int scan_expire_days { get; set; }
        //public int send_to_pt { get; set; }
        //public int service_level { get; set; }
        //public int service_time { get; set; }

        //public string shipper_facility { get; set; }
        //public string shipper_route { get; set; }


        //public string start_date { get; set; }
        //public string start_location { get; set; }
        //public string start_time { get; set; }
        //public string starting_location { get; set; }
        //public int stops { get; set; }
        //public string time_to_reseq { get; set; }
        //public double total_billing_amount { get; set; }
        //public int total_break_minutes { get; set; }
        //public string total_break_time { get; set; }
        //public int total_route_minutes { get; set; }
        //public string total_route_time { get; set; }
        //public double total_settlement_amount { get; set; }
        //public string transfer_to_branch { get; set; }
        //public int transfer_to_company { get; set; }
        //public string transfer_to_shift { get; set; }
        //public int unique_control_id { get; set; }
        //public string updated_by { get; set; }
        //public string updated_date { get; set; }
        //public string updated_time { get; set; }

        //public double vehicle_cost { get; set; }

    }



    public class ResponseRouteHeader
    {
        public object settlement_level_text { get; set; }
        public object settlement_level { get; set; }
        public object route_type_text { get; set; }
        public object route_type { get; set; }
        public List<object> notes { get; set; }
        public object posted_date { get; set; }
        public object end_time { get; set; }
        public object scan_expire_days { get; set; }
        public object actual_stops { get; set; }
        public object service_level { get; set; }
        public object actual_cost_allocation { get; set; }
        public object actual_miles { get; set; }
        public object posted_status { get; set; }
        public object rtn_trans_route { get; set; }
        public object shipper_route { get; set; }
        public object actual_driver_agent { get; set; }
        public object route_addon_amount { get; set; }
        public object open_time { get; set; }
        public object created_by { get; set; }
        public object last_updated_by { get; set; }
        public object az_equip2 { get; set; }
        public object actual_total_pieces { get; set; }
        public object unique_control_id { get; set; }
        public object actual_vehicle { get; set; }
        public object starting_location { get; set; }
        public object per_stop_settlement_amount { get; set; }
        public object ending_location { get; set; }
        public object transfer_to_shift { get; set; }
        public object expire_time { get; set; }
        public object actual_billing_amount { get; set; }
        public object total_route_time { get; set; }
        public object send_to_pt { get; set; }
        public object miles { get; set; }
        public object labor_cost { get; set; }
        public object labor_allocation_method_text { get; set; }
        public object labor_allocation_method { get; set; }
        public object vehicle_cost { get; set; }
        public object company_number_text { get; set; }
        public object company_number { get; set; }
        public object route_code { get; set; }
        public List<object> route_stops { get; set; }
        public object overhead_allocation_method_text { get; set; }
        public object overhead_allocation_method { get; set; }
        public object updated_by { get; set; }
        public object id { get; set; }
        public object dispatcher_id { get; set; }
        public object az_equip3 { get; set; }
        public object posted_time { get; set; }
        public object updated_date { get; set; }
        public object start_time { get; set; }
        public object created_time { get; set; }
        public object stops { get; set; }
        public object posted_by { get; set; }
        public object billing_method_text { get; set; }
        public object billing_method { get; set; }
        public object billing_level_text { get; set; }
        public object billing_level { get; set; }
        public object vehicle_allocation_method_text { get; set; }
        public object vehicle_allocation_method { get; set; }
        public object assigned_driver_agent { get; set; }
        public object shipper_type_text { get; set; }
        public object shipper_type { get; set; }
        public object total_break_time { get; set; }
        public object transfer_to_company { get; set; }
        public object hours { get; set; }
        public object actual_total_weight { get; set; }
        public object branch_id { get; set; }
        public object service_time { get; set; }
        public object close_time { get; set; }
        public object route_comments { get; set; }
        public object total_settlement_amount { get; set; }
        public object route_date { get; set; }
        public object actual_settlement_amount { get; set; }
        public object route_late_start_code { get; set; }
        public object shift_id { get; set; }
        public object start_location { get; set; }
        public object transfer_to_branch { get; set; }
        public object updated_time { get; set; }
        public object route_closed { get; set; }
        public object az_equip1 { get; set; }
        public object time_to_reseq { get; set; }
        public object overhead_cost { get; set; }
        public object assigned_vehicle { get; set; }
        public object per_stop_billing_amount { get; set; }
        public object last_updated_date { get; set; }
        public object total_break_minutes { get; set; }
        public object total_billing_amount { get; set; }
        public object push_services { get; set; }
        public object break_time { get; set; }
        public object calc_eta { get; set; }
        public object route_service_method_text { get; set; }
        public object route_service_method { get; set; }
        public object shipper_facility { get; set; }
        public object start_date { get; set; }
        public object last_updated_time { get; set; }
        public object end_date { get; set; }
        public object created_date { get; set; }
        public object end_location { get; set; }
        public object total_route_minutes { get; set; }
    }
    public class route_stopdetails
    {
        public route_stop route_stop { get; set; }
    }
    public class route_stop
    {
        public string company_number { get; set; }
        public int unique_id { get; set; }
        public string actual_cod_type { get; set; }

        public string callback_required { get; set; }
        public string cod_type { get; set; }
        public int customer_number { get; set; }
        public string origin_code { get; set; }
        //public bool photos_exist { get; set; }
        //public bool posted_status { get; set; }
        public string photos_exist { get; set; }
        public string posted_status { get; set; }
        public string required_signature_type { get; set; }
        public string route_date { get; set; }
        // public bool sent_to_phone { get; set; }
        public string sent_to_phone { get; set; }
        public string stop_type { get; set; }
        public string verification_id_type { get; set; }
        public string address_name { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip_code { get; set; }
        public int service_level { get; set; }
        public string route_code { get; set; }
        public string reference { get; set; }
        public string phone { get; set; }
        public string bol_number { get; set; }
        public string branch_id { get; set; }
        public List<items> items { get; set; }

        public string actual_arrival_time { get; set; }
        public double actual_billing_amt { get; set; }
        public double actual_cod_amt { get; set; }
        public string actual_delivery_date { get; set; }
        public string actual_depart_time { get; set; }
        public double actual_latitude { get; set; }
        public double actual_longitude { get; set; }
        public int actual_pieces { get; set; }
        public double actual_settlement_amt { get; set; }
        public int actual_weight { get; set; }
        public string additional_instructions { get; set; }

        public string addl_charge_code1 { get; set; }
        public string addl_charge_code2 { get; set; }
        public string addl_charge_code3 { get; set; }
        public string addl_charge_code4 { get; set; }
        public string addl_charge_code5 { get; set; }
        public string addl_charge_code6 { get; set; }
        public string addl_charge_code7 { get; set; }
        public string addl_charge_code8 { get; set; }
        public string addl_charge_code9 { get; set; }
        public string addl_charge_code10 { get; set; }
        public string addl_charge_code11 { get; set; }
        public string addl_charge_code12 { get; set; }


        public int addl_charge_occur1 { get; set; }
        public int addl_charge_occur2 { get; set; }
        public int addl_charge_occur3 { get; set; }
        public int addl_charge_occur4 { get; set; }
        public int addl_charge_occur5 { get; set; }
        public int addl_charge_occur6 { get; set; }
        public int addl_charge_occur7 { get; set; }
        public int addl_charge_occur8 { get; set; }
        public int addl_charge_occur9 { get; set; }
        public int addl_charge_occur10 { get; set; }
        public int addl_charge_occur11 { get; set; }
        public int addl_charge_occur12 { get; set; }
        public double addon_billing_amt { get; set; }
        public int address_point { get; set; }
        public int address_point_customer { get; set; }
        public string alt_lookup { get; set; }
        public string arrival_time { get; set; }
      //  public int asn_sent { get; set; }
        public string attention { get; set; }
        public double billing_override_amt { get; set; }


        public string c2_paperwork { get; set; }
        public int cases { get; set; }

        public double cod_amount { get; set; }
        public string cod_check_no { get; set; }
        public string combine_data { get; set; }
        public string comments { get; set; }
      //  public string created_by { get; set; }
       // public string created_date { get; set; }
     //   public string created_time { get; set; }
        public string departure_time { get; set; }
        public string dispatch_zone { get; set; }
        public string driver_app_status { get; set; }
        public string eta { get; set; }
        public string eta_date { get; set; }
        public string exception_code { get; set; }
        public int expected_pieces { get; set; }
        public int expected_weight { get; set; }
        public int height { get; set; }
        public string image_sign_req { get; set; }
        public int insurance_value { get; set; }
        public string invoice_number { get; set; }
        public string item_scans_required { get; set; }

        public string late_notice_date { get; set; }
        public string late_notice_time { get; set; }
        public double latitude { get; set; }
        public int length { get; set; }
        public int loaded_pieces { get; set; }
        public int location_accuracy { get; set; }
        public double longitude { get; set; }
        public int minutes_late { get; set; }
        // public List<notes> notes { get; set; }
        public string ordered_by { get; set; }
        //public int orig_order_number { get; set; }
       // public int original_id { get; set; }
        public double override_settle_percent { get; set; }

        public int phone_ext { get; set; }
       // public string posted_by { get; set; }
     //   public string posted_date { get; set; }
        public string posted_time { get; set; }
        public int pricing_zone { get; set; }
        // public List<progress> progress { get; set; } // read only
     //   public string received_branch { get; set; }
        //public int received_company { get; set; }
        public int received_pieces { get; set; }
       // public string received_route { get; set; }
       // public string received_sequence { get; set; }
      //  public string received_shift { get; set; }
       // public int received_unique_id { get; set; }
      //  public string redelivery { get; set; }
      //  public string @return { get; set; }
        public int return_redel_id { get; set; }
        public string return_redelivery_date { get; set; }
        public string return_redelivery_flag { get; set; }
        public string room { get; set; }
        public int schedule_stop_id { get; set; }



        public int service_time { get; set; }
        public double settlement_override_amt { get; set; }
        public string shift_id { get; set; }
        public string signature { get; set; }
        public string signature_filename { get; set; }
       // public List<signature_images> signature_images { get; set; }
        public string signature_required { get; set; }
        public string special_instructions1 { get; set; }
        public string special_instructions2 { get; set; }
        public string special_instructions3 { get; set; }
        public string special_instructions4 { get; set; }

         public string stop_sequence { get; set; }
        public int times_sent { get; set; }
        public int totes { get; set; }
        public string transfer_to_route { get; set; }
        public string transfer_to_sequence { get; set; }
      //  public string updated_by { get; set; }
        public string updated_by_scanner { get; set; }
      //  public string updated_date { get; set; }
      //  public string updated_time { get; set; }
      //  public string upload_time { get; set; }
        public string vehicle { get; set; }
        public string verification_id_details { get; set; }
        public int width { get; set; }


    }

    public class items
    {
        public int company_number { get; set; }
        public int unique_id { get; set; }
        public string actual_cod_type { get; set; }
        public string barcodes_unique { get; set; }
        public string cod_type { get; set; }
        public string photos_exist { get; set; }
        public string @return { get; set; }
        public int expected_pieces { get; set; }
        public int expected_weight { get; set; }
        public string item_description { get; set; }
        public string item_number { get; set; }
        public string container_id { get; set; }
        public string reference { get; set; }

    }
    public class ResponseRouteStop
    {
        public object room { get; set; }
        public object unique_id { get; set; }
        public object c2_paperwork { get; set; }
        public object company_number_text { get; set; }
        public object company_number { get; set; }
        public object addl_charge_code11 { get; set; }
        public object billing_override_amt { get; set; }
        public object addl_charge_occur1 { get; set; }
        public object updated_time { get; set; }
        public object stop_sequence { get; set; }
        public object phone { get; set; }
        public object city { get; set; }
        public object created_by { get; set; }
        public List<object> signature_images { get; set; }
        public object pricing_zone { get; set; }
        public object signature_filename { get; set; }
        public object addl_charge_code10 { get; set; }
        public object cod_check_no { get; set; }
        public object length { get; set; }
        public object expected_weight { get; set; }
        public object actual_settlement_amt { get; set; }
        public object actual_pieces { get; set; }
        public object updated_date { get; set; }
        public object schedule_stop_id { get; set; }
        public object photos_exist { get; set; }
        public object stop_type_text { get; set; }
        public object stop_type { get; set; }
        public object @return { get; set; }
        public object addl_charge_code6 { get; set; }
        public object dispatch_zone { get; set; }
        public object upload_time { get; set; }
        public object actual_cod_amt { get; set; }
        public object location_accuracy { get; set; }
        public List<Progress> progress { get; set; }
        public object received_route { get; set; }
        public object override_settle_percent { get; set; }
        public object cod_amount { get; set; }
        public object addl_charge_code9 { get; set; }
        public object eta_date { get; set; }
        public object cod_type_text { get; set; }
        public object cod_type { get; set; }
        public object addl_charge_occur3 { get; set; }
        public object reference { get; set; }
        public object sent_to_phone { get; set; }
        public object addl_charge_occur12 { get; set; }
        public object callback_required_text { get; set; }
        public object callback_required { get; set; }
        public object service_level_text { get; set; }
        public object service_level { get; set; }
        public object original_id { get; set; }
        public object width { get; set; }
        public object received_sequence { get; set; }
        public object transfer_to_sequence { get; set; }
        public object cases { get; set; }
        public object times_sent { get; set; }
        public object transfer_to_route { get; set; }
        public object zip_code { get; set; }
        public object settlement_override_amt { get; set; }
        public object driver_app_status_text { get; set; }
        public object driver_app_status { get; set; }
        public object route_code_text { get; set; }
        public object route_code { get; set; }
        public object received_shift { get; set; }
        public object addl_charge_occur6 { get; set; }
        public object addl_charge_occur11 { get; set; }
        public object vehicle { get; set; }
        public object addl_charge_code5 { get; set; }
        public object addl_charge_occur9 { get; set; }
        public object eta { get; set; }
        public object departure_time { get; set; }
        public object combine_data { get; set; }
        public object actual_latitude { get; set; }
        public object posted_by { get; set; }
        public object insurance_value { get; set; }
        public object return_redel_id { get; set; }
        public object addl_charge_code1 { get; set; }
        public object origin_code_text { get; set; }
        public object origin_code { get; set; }
        public object ordered_by { get; set; }
        public object posted_date { get; set; }
        public object actual_billing_amt { get; set; }
        public object created_date { get; set; }
        public object latitude { get; set; }
        public object received_pieces { get; set; }
        public object addl_charge_code7 { get; set; }
        public object totes { get; set; }
        public object asn_sent { get; set; }
        public object comments { get; set; }
        public object verification_id_type_text { get; set; }
        public object verification_id_type { get; set; }
        public object posted_time { get; set; }
        public object item_scans_required { get; set; }
        public object shift_id { get; set; }
        public object addon_billing_amt { get; set; }
        public object actual_delivery_date { get; set; }
        public object id { get; set; }
        public object actual_arrival_time { get; set; }
        public object signature_required { get; set; }
        public object longitude { get; set; }
        public object expected_pieces { get; set; }
        public object loaded_pieces { get; set; }
        public object alt_lookup { get; set; }
        public object customer_number_text { get; set; }
        public object customer_number { get; set; }
        public object created_time { get; set; }
        public object addl_charge_code8 { get; set; }
        public object signature { get; set; }
        public object actual_depart_time { get; set; }
        public object bol_number { get; set; }
        public object actual_cod_type_text { get; set; }
        public object actual_cod_type { get; set; }
        public object invoice_number { get; set; }
        public object branch_id { get; set; }
        public object special_instructions2 { get; set; }
        public object updated_by { get; set; }
        public object verification_id_details { get; set; }
        public object required_signature_type_text { get; set; }
        public object required_signature_type { get; set; }
        public object addl_charge_occur7 { get; set; }
        public object orig_order_number { get; set; }
        public object special_instructions1 { get; set; }
        public List<object> notes { get; set; }
        public object image_sign_req { get; set; }
        public object attention { get; set; }
        public object minutes_late { get; set; }
        public object late_notice_time { get; set; }
        public object received_unique_id { get; set; }
        public object exception_code { get; set; }
        public object addl_charge_code4 { get; set; }
        public object addl_charge_occur4 { get; set; }
        public object redelivery { get; set; }
        public object addl_charge_occur10 { get; set; }
        public object upload_date { get; set; }
        public object special_instructions4 { get; set; }
        public object address_name { get; set; }
        public object addl_charge_occur8 { get; set; }
        public object address_point_customer { get; set; }
        public object received_branch { get; set; }
        public List<object> items { get; set; }
        public object return_redelivery_date { get; set; }
        public object height { get; set; }
        public object actual_longitude { get; set; }
        public object service_time { get; set; }
        public object phone_ext { get; set; }
        public object addl_charge_occur2 { get; set; }
        public object late_notice_date { get; set; }
        public object address { get; set; }
        public object arrival_time { get; set; }
        public object posted_status { get; set; }
        public object route_date { get; set; }
        public object addl_charge_code12 { get; set; }
        public object addl_charge_code3 { get; set; }
        public object return_redelivery_flag_text { get; set; }
        public object return_redelivery_flag { get; set; }
        public object additional_instructions { get; set; }
        public object updated_by_scanner { get; set; }
        public object special_instructions3 { get; set; }
        public object addl_charge_occur5 { get; set; }
        public object address_point { get; set; }
        public object actual_weight { get; set; }
        public object received_company { get; set; }
        public object addl_charge_code2 { get; set; }
        public object state { get; set; }
    }

    public class RouteStopResponseItem
    {
        public object item_number { get; set; }
        public object item_description { get; set; }
        public object reference { get; set; }
        public object rma_route { get; set; }
        public object upload_time { get; set; }
        public object rma_stop_id { get; set; }
        public object width { get; set; }
        public object redelivery { get; set; }
        public object received_pieces { get; set; }
        public object cod_amount { get; set; }
        public object height { get; set; }
        public object comments { get; set; }
        public object actual_pieces { get; set; }
        public object actual_cod_amount { get; set; }
        public object rma_number { get; set; }
        public object manually_updated { get; set; }
        public object unique_id { get; set; }
        public object cod_type_text { get; set; }
        public object cod_type { get; set; }
        public object barcodes_unique { get; set; }
        public object actual_cod_type_text { get; set; }
        public object actual_cod_type { get; set; }
        public object return_redel_seq { get; set; }
        public object expected_pieces { get; set; }
        public object signature { get; set; }
        public object exception_code { get; set; }
        public object company_number_text { get; set; }
        public object company_number { get; set; }
        public object updated_date { get; set; }
        public object expected_weight { get; set; }
        public object created_date { get; set; }
        public object rma_origin { get; set; }
        public object created_by { get; set; }
        public object loaded_pieces { get; set; }
        public object return_redelivery_flag_text { get; set; }
        public object return_redelivery_flag { get; set; }
        public object original_id { get; set; }
        public object container_id { get; set; }
        public object @return { get; set; }
        public object length { get; set; }
        public List<object> notes { get; set; }
        public object actual_weight { get; set; }
        public object updated_by { get; set; }
        public object photos_exist { get; set; }
        public object second_container_id { get; set; }
        public object return_redel_id { get; set; }
        public object asn_sent { get; set; }
        public object actual_departure_time { get; set; }
        public object updated_time { get; set; }
        public object return_redelivery_date { get; set; }
        public object actual_arrival_time { get; set; }
        public object item_sequence { get; set; }
        public object pallet_number { get; set; }
        public object actual_date { get; set; }
        public object insurance_value { get; set; }
        public object created_time { get; set; }
        public object upload_date { get; set; }
        public List<object> scans { get; set; }
        public object id { get; set; }
        public object truck_id { get; set; }
    }

    public class RouteStopResponseProgress
    {
        public object status_time { get; set; }
        public object status_date { get; set; }
        public object status_text { get; set; }
        public object id { get; set; }
    }

    public class RouteStopResponseNote
    {
        public object company_number { get; set; }
        public object company_number_text { get; set; }
        public object entry_time { get; set; }
        public object note_text { get; set; }
        public object item_sequence { get; set; }
        public object entry_date { get; set; }
        public object user_entered { get; set; }
        public object show_to_cust { get; set; }
        public object note_type_text { get; set; }
        public object note_type { get; set; }
        public object unique_id { get; set; }
        public object id { get; set; }
        public object user_id { get; set; }
    }
}
