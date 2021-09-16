using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DatatracAPIOrder_OrderSettlement
{
    class clsDatatrac : clsCommon
    {
        public ReturnResponse CallDataTracOrderPostAPI(orderdetails objorderdetails)
        {
            ReturnResponse objresponse = new ReturnResponse();

            string json = string.Empty;
            clsCommon objCommon = new clsCommon();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                    //    string url = objCommon.GetConfigValue("DatatracURL");//"https://login.datatrac.com/rest/order";
                    string url = objCommon.GetConfigValue("DatatracURL") + "/order";
                    client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var Username = objCommon.GetConfigValue("DatatracUserName");
                    var Password = objCommon.GetConfigValue("DatatracPassword");

                    UTF8Encoding utf8 = new UTF8Encoding();

                    byte[] encodedBytes = utf8.GetBytes(Username + ":" + Password);
                    string userCredentialsEncoding = Convert.ToBase64String(encodedBytes);
                    client.DefaultRequestHeaders.Add("Authorization", "Basic " + userCredentialsEncoding);

                    string payload = JsonConvert.SerializeObject(objorderdetails);
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
                            //string strExecutionLogMessage = "Datatrac response failed for the customer reference number " + objorderdetails.order.reference + System.Environment.NewLine;
                            //strExecutionLogMessage += "Request:" + payload + System.Environment.NewLine;
                            //strExecutionLogMessage += "Response:" + objresponse.Reason;
                            //objCommon.WriteExecutionLog(objCommon.GetConfigValue("ExecutionLogFileLocation"), strExecutionLogMessage);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "exception in CallDataTracOrderPostAPI " + ex;
                objresponse.Reason = strExecutionLogMessage;
                objresponse.ResponseVal = false;
                objCommon.WriteExecutionLog(objCommon.GetConfigValue("ExecutionLogFileLocation"), strExecutionLogMessage);
                objCommon.WriteErrorLog(ex, strExecutionLogMessage);

                //objCommon.LogEvents(ex, "CallDataTracOrderPostAPI", System.Diagnostics.EventLogEntryType.Error, 1);

            }

            //objresponse.Reason = "{\"002018704400\": {\"cod_text\": \"No\", \"cod\": \"N\", \"signature\": \"SOF\", \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"add_charge_occur8\": null, \"pickup_airport_code\": null, \"hours\": \"15\", \"roundtrip_actual_depart_time\": null, \"bol_number\": null, \"deliver_address\": \"134 CANTERBURY DR\", \"time_order_entered\": \"08:05\", \"driver2\": null, \"add_charge_amt11\": null, \"rate_buck_amt3\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"pickup_special_instructions3\": null, \"roundtrip_actual_pieces\": null, \"deliver_actual_longitude\": null, \"fuel_price_source\": null, \"roundtrip_wait_time\": null, \"deliver_actual_latitude\": null, \"deliver_special_instructions3\": null, \"deliver_name\": \"MICHAEL BROWN\", \"rate_buck_amt7\": null, \"add_charge_amt3\": null, \"add_charge_code5\": null, \"delivery_airport_code\": null, \"master_airway_bill_number\": null, \"distribution_shift_id\": null, \"deliver_requested_dep_time\": \"17:00\", \"page_number\": 1, \"add_charge_occur9\": null, \"rate_buck_amt5\": null, \"pickup_pricing_zone\": 1, \"frequent_caller_id\": null, \"pickup_actual_latitude\": null, \"pickup_omw_longitude\": null, \"distribution_unique_id\": 0, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"bringg_send_sms\": false, \"pickup_zip\": \"23150\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"previous_ctrl_number\": null, \"roundtrip_sign_req\": false, \"notes\": [], \"actual_miles\": null, \"etrac_number\": null, \"deliver_omw_timestamp\": null, \"deliver_pricing_zone\": 1, \"rate_buck_amt2\": null, \"signature_images\": [], \"deliver_special_instr_long\": null, \"pickup_wait_time\": null, \"dispatch_time\": null, \"delivery_longitude\": -77.31366340, \"deliver_omw_latitude\": null, \"reference_text\": \"2086108801\", \"reference\": \"2086108801\", \"add_charge_occur6\": null, \"additional_drivers\": false, \"cod_accept_cashiers_check\": false, \"hist_inv_date\": null, \"add_charge_amt9\": null, \"delivery_point_customer\": 31025, \"deliver_requested_arr_time\": \"08:00\", \"add_charge_occur10\": null, \"add_charge_amt6\": null, \"rate_chart_used\": 0, \"pickup_sign_req\": true, \"fuel_plan\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"rate_buck_amt11\": null, \"fuel_price_zone\": null, \"add_charge_code11\": null, \"add_charge_amt4\": null, \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"pickup_state\": \"VA\", \"add_charge_amt5\": null, \"az_equip2\": null, \"line_items\": [], \"pickup_special_instructions4\": null, \"deliver_attention\": null, \"deliver_eta_time\": null, \"del_actual_location_accuracy\": null, \"deliver_omw_longitude\": null, \"pickup_name\": \"HUMAN TOUCH\", \"add_charge_code3\": null, \"order_automatically_quoted\": false, \"add_charge_amt7\": null, \"pu_actual_location_accuracy\": null, \"pickup_signature\": \"SOF\", \"original_schedule_number\": null, \"rate_buck_amt10\": 2.16, \"callback_to\": null, \"customers_etrac_partner_id\": \"96609250\", \"ordered_by\": \"RYDER\", \"pu_arrive_notification_sent\": false, \"hist_inv_number\": 0, \"rate_buck_amt9\": null, \"pickup_special_instructions2\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"progress\": [{\"status_date\": \"2021-06-30\", \"status_text\": \"Entered in carrier's system\", \"status_time\": \"08:05:00\"}, {\"status_date\": \"2021-05-10\", \"status_text\": \"Picked up\", \"status_time\": \"08:30:00\"}, {\"status_date\": \"2021-05-10\", \"status_text\": \"Delivered\", \"status_time\": \"08:15:00\"}], \"pickup_phone_ext\": null, \"delivery_address_point_number_text\": \"MICHAEL BROWN\", \"delivery_address_point_number\": 25113, \"deliver_wait_time\": null, \"cod_amount\": null, \"pickup_actual_pieces\": null, \"pickup_eta_time\": null, \"rate_buck_amt1\": 80.00, \"deliver_requested_date\": \"2021-05-10\", \"bringg_order_id\": null, \"roundtrip_signature\": null, \"add_charge_amt12\": null, \"return_svc_level\": null, \"rate_buck_amt4\": null, \"deliver_country\": null, \"add_charge_code2\": null, \"number_of_pieces\": 1, \"push_services\": null, \"add_charge_occur7\": null, \"pickup_actual_arr_time\": \"08:00\", \"deliver_city\": \"STAFFORD\", \"pickup_special_instr_long\": null, \"fuel_miles\": null, \"image_sign_req\": false, \"add_charge_amt10\": null, \"po_number\": null, \"verified_weight\": null, \"add_charge_code1\": null, \"callback_time\": null, \"blocks\": null, \"send_new_order_alert\": false, \"pickup_omw_latitude\": null, \"add_charge_code4\": null, \"pickup_attention\": null, \"rate_buck_amt8\": null, \"pickup_requested_date\": \"2021-05-10\", \"manual_notepad\": false, \"control_number\": 1870440, \"deliver_dispatch_zone\": null, \"service_level_text\": \"White Glove Delivery\", \"service_level\": 58, \"dl_arrive_notification_sent\": false, \"custom_special_instr_long\": null, \"pickup_latitude\": 37.53250820, \"pickup_actual_longitude\": null, \"pickup_city\": \"SANDSTON\", \"deliver_special_instructions2\": null, \"pickup_email_notification_sent\": false, \"add_charge_occur3\": null, \"rate_buck_amt6\": null, \"pickup_requested_arr_time\": \"07:00\", \"pickup_country\": null, \"deliver_room\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"photos_exist\": false, \"rate_special_instructions\": null, \"pickup_omw_timestamp\": null, \"add_charge_amt8\": null, \"deliver_actual_pieces\": null, \"pickup_address_point_number_text\": \"HUMAN TOUCH\", \"pickup_address_point_number\": 19891, \"pickup_longitude\": -77.33035820, \"insurance_amount\": null, \"dispatch_id\": null, \"calc_add_on_chgs\": false, \"deliver_route_code\": null, \"original_ctrl_number\": null, \"pickup_actual_date\": \"2021-05-10\", \"az_equip3\": null, \"date_order_entered\": \"2021-06-30\", \"exception_code\": null, \"deliver_actual_date\": \"2021-05-10\", \"delivery_latitude\": 38.37859180, \"rescheduled_ctrl_number\": null, \"deliver_actual_dep_time\": \"08:15\", \"quote_amount\": null, \"deliver_phone_ext\": null, \"add_charge_code8\": null, \"deliver_phone\": null, \"add_charge_occur11\": null, \"signature_required\": true, \"deliver_eta_date\": null, \"weight\": null, \"add_charge_occur1\": null, \"deliver_special_instructions4\": null, \"ordered_by_phone_number\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"customer_name\": \"MXD/RYDER\", \"csr\": \"DX*\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"invoice_period_end_date\": null, \"add_charge_amt2\": null, \"distribution_branch_id\": null, \"roundtrip_actual_latitude\": null, \"add_charge_code7\": null, \"cod_accept_company_check\": false, \"email_addresses\": null, \"add_charge_code9\": null, \"pickup_route_seq\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"deliver_route_sequence\": null, \"settlements\": [{\"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"vendor_invoice_number\": null, \"date_last_updated\": \"2021-06-30\", \"charge3\": null, \"settlement_bucket6_pct\": null, \"settlement_pct\": 100.00, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"settlement_bucket2_pct\": null, \"pay_chart_used\": null, \"settlement_bucket3_pct\": null, \"order_date\": \"2021-05-10\", \"charge5\": null, \"fuel_price_zone\": null, \"voucher_amount\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"voucher_date\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"settlement_period_end_date\": null, \"charge6\": null, \"fuel_plan\": null, \"charge2\": null, \"vendor_employee_numer\": null, \"control_number\": 1870440, \"settlement_bucket5_pct\": null, \"settlement_bucket4_pct\": null, \"adjustment_type\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"driver_number_text\": \"LAVERT KENDALL MORRIS\", \"driver_number\": 3001, \"time_last_updated\": \"07:05\", \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"agent_etrac_transaction_number\": null, \"agents_etrac_partner_id\": null, \"charge4\": null, \"voucher_number\": null, \"pre_book_percentage\": true, \"id\": \"002018704400D1\", \"charge1\": null, \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"settlement_bucket1_pct\": null, \"fuel_price_source\": null, \"record_type\": 0}], \"add_charge_code10\": null, \"push_partner_order_id\": null, \"edi_acknowledgement_required\": false, \"roundtrip_actual_date\": null, \"pickup_requested_dep_time\": \"09:00\", \"pickup_phone\": null, \"roundtrip_actual_arrival_time\": null, \"pickup_room\": null, \"callback_userid\": null, \"vehicle_type\": null, \"add_charge_occur4\": null, \"add_charge_amt1\": null, \"pickup_dispatch_zone\": null, \"roundtrip_actual_longitude\": null, \"pickup_eta_date\": null, \"pickup_route_code\": null, \"hazmat\": false, \"pickup_special_instructions1\": null, \"total_pages\": 1, \"record_type\": 0, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"pickup_point_customer\": 31025, \"pickup_address\": \"540 EASTPARK CT\", \"callback_date\": null, \"deliver_state\": \"VA\", \"exception_sign_required\": false, \"add_charge_code12\": null, \"holiday_groups\": null, \"az_equip1\": null, \"driver1_text\": \"LAVERT KENDALL MORRIS\", \"driver1\": 3001, \"rate_miles\": null, \"rt_actual_location_accuracy\": null, \"id\": \"002018704400\", \"add_charge_occur5\": null, \"house_airway_bill_number\": null, \"deliver_special_instructions1\": null, \"exception_timestamp\": null, \"add_charge_occur2\": null, \"pickup_actual_dep_time\": \"08:30\", \"deliver_zip\": \"22554\", \"deliver_actual_arr_time\": \"08:00\", \"add_charge_code6\": null, \"add_charge_occur12\": null, \"bringg_last_loc_sent\": null, \"zone_set_used\": 1}}";
            // objresponse.Reason = "{\"002018707560\": {\"cod_text\": \"No\", \"cod\": \"N\", \"signature\": \"SOF\", \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"add_charge_occur8\": null, \"pickup_airport_code\": null, \"hours\": \"15\", \"roundtrip_actual_depart_time\": null, \"bol_number\": null, \"deliver_address\": \"134 CANTERBURY DR\", \"time_order_entered\": \"07:45\", \"driver2\": null, \"add_charge_amt11\": null, \"rate_buck_amt3\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"pickup_special_instructions3\": null, \"roundtrip_actual_pieces\": null, \"deliver_actual_longitude\": null, \"fuel_price_source\": null, \"roundtrip_wait_time\": null, \"deliver_actual_latitude\": null, \"deliver_special_instructions3\": null, \"deliver_name\": \"MICHAEL BROWN\", \"rate_buck_amt7\": null, \"add_charge_amt3\": null, \"add_charge_code5\": null, \"delivery_airport_code\": null, \"master_airway_bill_number\": null, \"distribution_shift_id\": null, \"deliver_requested_dep_time\": \"17:00\", \"page_number\": 1, \"add_charge_occur9\": null, \"rate_buck_amt5\": null, \"pickup_pricing_zone\": 1, \"frequent_caller_id\": null, \"pickup_actual_latitude\": null, \"pickup_omw_longitude\": null, \"distribution_unique_id\": 0, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"bringg_send_sms\": false, \"pickup_zip\": \"23150\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"previous_ctrl_number\": null, \"roundtrip_sign_req\": false, \"notes\": [], \"actual_miles\": null, \"etrac_number\": null, \"deliver_omw_timestamp\": null, \"deliver_pricing_zone\": 1, \"rate_buck_amt2\": null, \"signature_images\": [], \"deliver_special_instr_long\": null, \"pickup_wait_time\": null, \"dispatch_time\": null, \"delivery_longitude\": -77.31366340, \"deliver_omw_latitude\": null, \"reference_text\": \"2086108801\", \"reference\": \"2086108801\", \"add_charge_occur6\": null, \"additional_drivers\": false, \"cod_accept_cashiers_check\": false, \"hist_inv_date\": null, \"add_charge_amt9\": null, \"delivery_point_customer\": 31025, \"deliver_requested_arr_time\": \"08:00\", \"add_charge_occur10\": null, \"add_charge_amt6\": null, \"rate_chart_used\": 0, \"pickup_sign_req\": true, \"fuel_plan\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"rate_buck_amt11\": null, \"fuel_price_zone\": null, \"add_charge_code11\": null, \"add_charge_amt4\": null, \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"pickup_state\": \"VA\", \"add_charge_amt5\": null, \"az_equip2\": null, \"line_items\": [], \"pickup_special_instructions4\": null, \"deliver_attention\": null, \"deliver_eta_time\": null, \"del_actual_location_accuracy\": null, \"deliver_omw_longitude\": null, \"pickup_name\": \"HUMAN TOUCH\", \"add_charge_code3\": null, \"order_automatically_quoted\": false, \"add_charge_amt7\": null, \"pu_actual_location_accuracy\": null, \"pickup_signature\": \"SOF\", \"original_schedule_number\": null, \"rate_buck_amt10\": 2.16, \"callback_to\": null, \"customers_etrac_partner_id\": \"96609250\", \"ordered_by\": \"RYDER\", \"pu_arrive_notification_sent\": false, \"hist_inv_number\": 0, \"rate_buck_amt9\": null, \"pickup_special_instructions2\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"progress\": [{\"status_date\": \"2021-07-01\", \"status_text\": \"Entered in carrier's system\", \"status_time\": \"07:45:00\"}, {\"status_date\": \"2021-05-10\", \"status_text\": \"Picked up\", \"status_time\": \"08:30:00\"}, {\"status_date\": \"2021-05-10\", \"status_text\": \"Delivered\", \"status_time\": \"08:15:00\"}], \"pickup_phone_ext\": null, \"delivery_address_point_number_text\": \"MICHAEL BROWN\", \"delivery_address_point_number\": 25113, \"deliver_wait_time\": null, \"cod_amount\": null, \"pickup_actual_pieces\": null, \"pickup_eta_time\": null, \"rate_buck_amt1\": 80.00, \"deliver_requested_date\": \"2021-05-10\", \"bringg_order_id\": null, \"roundtrip_signature\": null, \"add_charge_amt12\": null, \"return_svc_level\": null, \"rate_buck_amt4\": null, \"deliver_country\": null, \"add_charge_code2\": null, \"number_of_pieces\": 1, \"push_services\": null, \"add_charge_occur7\": null, \"pickup_actual_arr_time\": \"08:00\", \"deliver_city\": \"STAFFORD\", \"pickup_special_instr_long\": null, \"fuel_miles\": null, \"image_sign_req\": false, \"add_charge_amt10\": null, \"po_number\": null, \"verified_weight\": null, \"add_charge_code1\": null, \"callback_time\": null, \"blocks\": null, \"send_new_order_alert\": false, \"pickup_omw_latitude\": null, \"add_charge_code4\": null, \"pickup_attention\": null, \"rate_buck_amt8\": null, \"pickup_requested_date\": \"2021-05-10\", \"manual_notepad\": false, \"control_number\": 1870756, \"deliver_dispatch_zone\": null, \"service_level_text\": \"White Glove Delivery\", \"service_level\": 58, \"dl_arrive_notification_sent\": false, \"custom_special_instr_long\": null, \"pickup_latitude\": 37.53250820, \"pickup_actual_longitude\": null, \"pickup_city\": \"SANDSTON\", \"deliver_special_instructions2\": null, \"pickup_email_notification_sent\": false, \"add_charge_occur3\": null, \"rate_buck_amt6\": null, \"pickup_requested_arr_time\": \"07:00\", \"pickup_country\": null, \"deliver_room\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"photos_exist\": false, \"rate_special_instructions\": null, \"pickup_omw_timestamp\": null, \"add_charge_amt8\": null, \"deliver_actual_pieces\": null, \"pickup_address_point_number_text\": \"HUMAN TOUCH\", \"pickup_address_point_number\": 19891, \"pickup_longitude\": -77.33035820, \"insurance_amount\": null, \"dispatch_id\": null, \"calc_add_on_chgs\": false, \"deliver_route_code\": null, \"original_ctrl_number\": null, \"pickup_actual_date\": \"2021-05-10\", \"az_equip3\": null, \"date_order_entered\": \"2021-07-01\", \"exception_code\": null, \"deliver_actual_date\": \"2021-05-10\", \"delivery_latitude\": 38.37859180, \"rescheduled_ctrl_number\": null, \"deliver_actual_dep_time\": \"08:15\", \"quote_amount\": null, \"deliver_phone_ext\": null, \"add_charge_code8\": null, \"deliver_phone\": null, \"add_charge_occur11\": null, \"signature_required\": true, \"deliver_eta_date\": null, \"weight\": null, \"add_charge_occur1\": null, \"deliver_special_instructions4\": null, \"ordered_by_phone_number\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"customer_name\": \"MXD/RYDER\", \"csr\": \"DX*\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"invoice_period_end_date\": null, \"add_charge_amt2\": null, \"distribution_branch_id\": null, \"roundtrip_actual_latitude\": null, \"add_charge_code7\": null, \"cod_accept_company_check\": false, \"email_addresses\": null, \"add_charge_code9\": null, \"pickup_route_seq\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"deliver_route_sequence\": null, \"settlements\": [{\"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"vendor_invoice_number\": null, \"date_last_updated\": \"2021-07-01\", \"charge3\": null, \"settlement_bucket6_pct\": null, \"settlement_pct\": 100.00, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"settlement_bucket2_pct\": null, \"pay_chart_used\": null, \"settlement_bucket3_pct\": null, \"order_date\": \"2021-05-10\", \"charge5\": null, \"fuel_price_zone\": null, \"voucher_amount\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"voucher_date\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"settlement_period_end_date\": null, \"charge6\": null, \"fuel_plan\": null, \"charge2\": null, \"vendor_employee_numer\": null, \"control_number\": 1870756, \"settlement_bucket5_pct\": null, \"settlement_bucket4_pct\": null, \"adjustment_type\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"driver_number_text\": \"LAVERT KENDALL MORRIS\", \"driver_number\": 3001, \"time_last_updated\": \"06:45\", \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"agent_etrac_transaction_number\": null, \"agents_etrac_partner_id\": null, \"charge4\": null, \"voucher_number\": null, \"pre_book_percentage\": true, \"id\": \"002018707560D1\", \"charge1\": null, \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"settlement_bucket1_pct\": null, \"fuel_price_source\": null, \"record_type\": 0}], \"add_charge_code10\": null, \"push_partner_order_id\": null, \"edi_acknowledgement_required\": false, \"roundtrip_actual_date\": null, \"pickup_requested_dep_time\": \"09:00\", \"pickup_phone\": null, \"roundtrip_actual_arrival_time\": null, \"pickup_room\": null, \"callback_userid\": null, \"vehicle_type\": null, \"add_charge_occur4\": null, \"add_charge_amt1\": null, \"pickup_dispatch_zone\": null, \"roundtrip_actual_longitude\": null, \"pickup_eta_date\": null, \"pickup_route_code\": null, \"hazmat\": false, \"pickup_special_instructions1\": null, \"total_pages\": 1, \"record_type\": 0, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"pickup_point_customer\": 31025, \"pickup_address\": \"540 EASTPARK CT\", \"callback_date\": null, \"deliver_state\": \"VA\", \"exception_sign_required\": false, \"add_charge_code12\": null, \"holiday_groups\": null, \"az_equip1\": null, \"driver1_text\": \"LAVERT KENDALL MORRIS\", \"driver1\": 3001, \"rate_miles\": null, \"rt_actual_location_accuracy\": null, \"id\": \"002018707560\", \"add_charge_occur5\": null, \"house_airway_bill_number\": null, \"deliver_special_instructions1\": null, \"exception_timestamp\": null, \"add_charge_occur2\": null, \"pickup_actual_dep_time\": \"08:30\", \"deliver_zip\": \"22554\", \"deliver_actual_arr_time\": \"08:00\", \"add_charge_code6\": null, \"add_charge_occur12\": null, \"bringg_last_loc_sent\": null, \"zone_set_used\": 1}}";
            // objresponse.Reason = "{\"002018716980\": {\"csr\": \"DX*\", \"cod_text\": \"No\", \"cod\": \"N\", \"edi_acknowledgement_required\": false, \"control_number\": 1871698, \"deliver_requested_arr_time\": \"08:00\", \"return_svc_level\": null, \"pickup_phone_ext\": null, \"customers_etrac_partner_id\": \"96609250\", \"distribution_unique_id\": 0, \"custom_special_instr_long\": null, \"exception_sign_required\": false, \"po_number\": null, \"exception_code\": null, \"add_charge_occur4\": null, \"frequent_caller_id\": null, \"push_services\": null, \"pickup_omw_longitude\": null, \"deliver_special_instructions2\": null, \"deliver_actual_dep_time\": \"08:15\", \"house_airway_bill_number\": null, \"invoice_period_end_date\": null, \"line_items\": [], \"pickup_eta_date\": null, \"dl_arrive_notification_sent\": false, \"hist_inv_date\": null, \"add_charge_occur6\": null, \"add_charge_code12\": null, \"add_charge_occur5\": null, \"pickup_route_seq\": null, \"add_charge_code9\": null, \"callback_time\": null, \"add_charge_amt12\": null, \"add_charge_occur3\": null, \"deliver_eta_date\": null, \"ordered_by\": \"RYDER\", \"deliver_actual_latitude\": null, \"pickup_airport_code\": null, \"rate_buck_amt9\": null, \"deliver_pricing_zone\": 1, \"add_charge_code4\": null, \"rate_buck_amt5\": null, \"total_pages\": 1, \"roundtrip_actual_arrival_time\": null, \"pickup_requested_date\": \"2021-05-10\", \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"pickup_wait_time\": null, \"pickup_special_instructions4\": null, \"deliver_special_instructions1\": null, \"deliver_room\": null, \"roundtrip_actual_date\": null, \"rescheduled_ctrl_number\": null, \"insurance_amount\": null, \"deliver_city\": \"STAFFORD\", \"progress\": [{\"status_text\": \"Entered in carrier's system\", \"status_time\": \"12:27:00\", \"status_date\": \"2021-07-05\"}, {\"status_text\": \"Picked up\", \"status_time\": \"08:30:00\", \"status_date\": \"2021-05-10\"}, {\"status_text\": \"Delivered\", \"status_time\": \"08:15:00\", \"status_date\": \"2021-05-10\"}], \"deliver_eta_time\": null, \"rate_buck_amt2\": null, \"previous_ctrl_number\": null, \"pickup_city\": \"SANDSTON\", \"pickup_special_instructions1\": null, \"deliver_route_sequence\": null, \"pickup_actual_pieces\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"signature_required\": true, \"deliver_actual_date\": \"2021-05-10\", \"callback_userid\": null, \"pickup_requested_dep_time\": \"09:00\", \"cod_accept_cashiers_check\": false, \"signature_images\": [], \"add_charge_amt8\": null, \"add_charge_amt3\": null, \"pickup_zip\": \"23150\", \"original_ctrl_number\": null, \"calc_add_on_chgs\": false, \"original_schedule_number\": null, \"email_addresses\": null, \"pickup_actual_dep_time\": \"08:30\", \"pickup_special_instructions3\": null, \"etrac_number\": null, \"fuel_plan\": null, \"pickup_address_point_number_text\": \"HUMAN TOUCH\", \"pickup_address_point_number\": 19891, \"pickup_latitude\": 37.53250820, \"add_charge_amt5\": null, \"verified_weight\": null, \"pickup_sign_req\": true, \"exception_timestamp\": null, \"add_charge_occur10\": null, \"deliver_special_instructions3\": null, \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"add_charge_code1\": null, \"deliver_name\": \"MICHAEL BROWN\", \"az_equip2\": null, \"rate_buck_amt3\": null, \"bringg_send_sms\": false, \"pickup_actual_date\": \"2021-05-10\", \"page_number\": 1, \"pickup_signature\": \"SOF\", \"bringg_order_id\": null, \"pu_arrive_notification_sent\": false, \"roundtrip_actual_pieces\": null, \"pickup_actual_latitude\": null, \"manual_notepad\": false, \"roundtrip_sign_req\": false, \"deliver_zip\": \"22554\", \"rate_buck_amt8\": null, \"add_charge_occur11\": null, \"holiday_groups\": null, \"delivery_latitude\": 38.37859180, \"rate_buck_amt4\": null, \"pickup_point_customer\": 31025, \"rate_miles\": null, \"pickup_special_instructions2\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"bol_number\": null, \"rate_chart_used\": 0, \"rate_buck_amt6\": null, \"add_charge_occur1\": null, \"deliver_phone\": null, \"rate_buck_amt11\": null, \"deliver_actual_pieces\": null, \"deliver_attention\": null, \"driver1\": null, \"zone_set_used\": 1, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"add_charge_amt4\": null, \"pickup_pricing_zone\": 1, \"pickup_country\": null, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"deliver_omw_timestamp\": null, \"weight\": null, \"fuel_price_source\": null, \"pickup_omw_timestamp\": null, \"add_charge_code6\": null, \"photos_exist\": false, \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"pickup_room\": null, \"deliver_omw_latitude\": null, \"callback_date\": null, \"actual_miles\": null, \"delivery_longitude\": -77.31366340, \"image_sign_req\": false, \"additional_drivers\": false, \"pickup_attention\": null, \"reference_text\": \"DAYA1\", \"reference\": \"DAYA1\", \"roundtrip_wait_time\": null, \"add_charge_code11\": null, \"pickup_special_instr_long\": null, \"add_charge_code3\": null, \"add_charge_amt1\": null, \"callback_to\": null, \"date_order_entered\": \"2021-07-05\", \"rate_special_instructions\": null, \"hist_inv_number\": 0, \"roundtrip_signature\": null, \"bringg_last_loc_sent\": null, \"pickup_route_code\": null, \"pickup_requested_arr_time\": \"07:00\", \"deliver_address\": \"134 CANTERBURY DR\", \"roundtrip_actual_longitude\": null, \"add_charge_occur8\": null, \"delivery_airport_code\": null, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"add_charge_code10\": null, \"customer_name\": \"MXD/RYDER\", \"rate_buck_amt1\": 80.00, \"delivery_address_point_number_text\": \"MICHAEL BROWN\", \"delivery_address_point_number\": 25113, \"cod_amount\": null, \"roundtrip_actual_depart_time\": null, \"pickup_dispatch_zone\": null, \"service_level_text\": \"White Glove Delivery\", \"service_level\": 58, \"deliver_actual_longitude\": null, \"pickup_actual_arr_time\": \"08:00\", \"add_charge_amt6\": null, \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"pickup_longitude\": -77.33035820, \"driver2\": null, \"distribution_branch_id\": null, \"add_charge_amt9\": null, \"add_charge_code8\": null, \"blocks\": null, \"hazmat\": false, \"add_charge_code7\": null, \"deliver_requested_dep_time\": \"17:00\", \"signature\": \"SOF\", \"master_airway_bill_number\": null, \"cod_accept_company_check\": false, \"delivery_point_customer\": 31025, \"add_charge_occur2\": null, \"quote_amount\": null, \"add_charge_code2\": null, \"deliver_requested_date\": \"2021-05-10\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"record_type\": 0, \"deliver_special_instr_long\": null, \"push_partner_order_id\": null, \"pickup_address\": \"540 EASTPARK CT\", \"add_charge_occur9\": null, \"distribution_shift_id\": null, \"settlements\": [], \"hours\": \"15\", \"pu_actual_location_accuracy\": null, \"fuel_price_zone\": null, \"rt_actual_location_accuracy\": null, \"deliver_phone_ext\": null, \"vehicle_type\": null, \"del_actual_location_accuracy\": null, \"ordered_by_phone_number\": null, \"deliver_country\": null, \"az_equip3\": null, \"add_charge_amt7\": null, \"add_charge_amt10\": null, \"notes\": [], \"pickup_eta_time\": null, \"deliver_state\": \"VA\", \"add_charge_code5\": null, \"deliver_wait_time\": null, \"pickup_omw_latitude\": null, \"fuel_miles\": null, \"add_charge_occur12\": null, \"deliver_dispatch_zone\": null, \"rate_buck_amt10\": 2.16, \"order_automatically_quoted\": false, \"deliver_actual_arr_time\": \"08:00\", \"deliver_special_instructions4\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"az_equip1\": null, \"time_order_entered\": \"12:27\", \"rate_buck_amt7\": null, \"roundtrip_actual_latitude\": null, \"add_charge_amt2\": null, \"add_charge_occur7\": null, \"pickup_email_notification_sent\": false, \"dispatch_time\": null, \"pickup_actual_longitude\": null, \"add_charge_amt11\": null, \"pickup_name\": \"HUMAN TOUCH\", \"dispatch_id\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"send_new_order_alert\": false, \"id\": \"002018716980\", \"deliver_omw_longitude\": null, \"pickup_state\": \"VA\", \"deliver_route_code\": null, \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"pickup_phone\": null, \"number_of_pieces\": 1}}";
            // objresponse.ResponseVal = true;
            return objresponse;
        }


        public ReturnResponse CallDataTracOrderSettlementPutAPI(string UniqueId, string jsonreq)
        {
            ReturnResponse objresponse = new ReturnResponse();

            string json = string.Empty; ;
            clsCommon objCommon = new clsCommon();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                    string url = objCommon.GetConfigValue("DatatracURL") + "/order_settlement/" + UniqueId;

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
                    using (var content = new StringContent(payload, Encoding.UTF8, "application/json"))
                    {
                        content.Headers.ContentType.CharSet = "UTF-8";
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        var response = client.PutAsync(url, content).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            objresponse.ResponseVal = true;
                            objresponse.Reason = response.Content.ReadAsStringAsync().Result;
                        }
                        else
                        {
                            objresponse.ResponseVal = false;
                            objresponse.Reason = response.Content.ReadAsStringAsync().Result;
                            //string strExecutionLogMessage = "Datatrac response failed for the unique number  " + UniqueId + System.Environment.NewLine;
                            //strExecutionLogMessage += "Request :" + payload + System.Environment.NewLine;
                            //strExecutionLogMessage += "Response : " + objresponse.Reason;
                            //objCommon.WriteExecutionLog(objCommon.GetConfigValue("ExecutionLogFileLocation"), strExecutionLogMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "Exception in CallDataTracOrderSettlementPutAPI " + ex;
                objresponse.Reason = strExecutionLogMessage;
                objresponse.ResponseVal = false;
                objCommon.WriteExecutionLog(objCommon.GetConfigValue("ExecutionLogFileLocation"), strExecutionLogMessage);
                // objCommon.LogEvents(ex, "CallDataTracOrderSettlementPutAPI", System.Diagnostics.EventLogEntryType.Error, 2);
                objCommon.WriteErrorLog(ex, strExecutionLogMessage);
            }
            return objresponse;
        }


        public ReturnResponse CallDataTracOrderPutAPI(string UniqueId, string jsonreq)
        {
            ReturnResponse objresponse = new ReturnResponse();

            string json = string.Empty;
            clsCommon objCommon = new clsCommon();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                    string url = objCommon.GetConfigValue("DatatracURL") + "/order/" + UniqueId;
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
                    using (var content = new StringContent(payload, Encoding.UTF8, "application/json"))
                    {
                        content.Headers.ContentType.CharSet = "UTF-8";
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        var response = client.PutAsync(url, content).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            objresponse.ResponseVal = true;
                            objresponse.Reason = response.Content.ReadAsStringAsync().Result;

                        }
                        else
                        {
                            objresponse.ResponseVal = false;
                            objresponse.Reason = response.Content.ReadAsStringAsync().Result;
                            //string strExecutionLogMessage = "Datatrac response failed for the customer id " + UniqueId + System.Environment.NewLine;
                            //strExecutionLogMessage += "Request:" + payload + System.Environment.NewLine;
                            //strExecutionLogMessage += "Response:" + objresponse.Reason;
                            //objCommon.WriteExecutionLog(objCommon.GetConfigValue("ExecutionLogFileLocation"), strExecutionLogMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "exception in CallDataTracOrderPutAPI " + ex;
                objresponse.Reason = strExecutionLogMessage;
                objresponse.ResponseVal = false;
                objCommon.WriteExecutionLog(objCommon.GetConfigValue("ExecutionLogFileLocation"), strExecutionLogMessage);
                objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                // objCommon.LogEvents(ex, "CallDataTracOrderPutAPI", System.Diagnostics.EventLogEntryType.Error, 1);

            }

            //objresponse.Reason = "{\"002018704400\": {\"cod_text\": \"No\", \"cod\": \"N\", \"signature\": \"SOF\", \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"add_charge_occur8\": null, \"pickup_airport_code\": null, \"hours\": \"15\", \"roundtrip_actual_depart_time\": null, \"bol_number\": null, \"deliver_address\": \"134 CANTERBURY DR\", \"time_order_entered\": \"08:05\", \"driver2\": null, \"add_charge_amt11\": null, \"rate_buck_amt3\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"pickup_special_instructions3\": null, \"roundtrip_actual_pieces\": null, \"deliver_actual_longitude\": null, \"fuel_price_source\": null, \"roundtrip_wait_time\": null, \"deliver_actual_latitude\": null, \"deliver_special_instructions3\": null, \"deliver_name\": \"MICHAEL BROWN\", \"rate_buck_amt7\": null, \"add_charge_amt3\": null, \"add_charge_code5\": null, \"delivery_airport_code\": null, \"master_airway_bill_number\": null, \"distribution_shift_id\": null, \"deliver_requested_dep_time\": \"17:00\", \"page_number\": 1, \"add_charge_occur9\": null, \"rate_buck_amt5\": null, \"pickup_pricing_zone\": 1, \"frequent_caller_id\": null, \"pickup_actual_latitude\": null, \"pickup_omw_longitude\": null, \"distribution_unique_id\": 0, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"bringg_send_sms\": false, \"pickup_zip\": \"23150\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"previous_ctrl_number\": null, \"roundtrip_sign_req\": false, \"notes\": [], \"actual_miles\": null, \"etrac_number\": null, \"deliver_omw_timestamp\": null, \"deliver_pricing_zone\": 1, \"rate_buck_amt2\": null, \"signature_images\": [], \"deliver_special_instr_long\": null, \"pickup_wait_time\": null, \"dispatch_time\": null, \"delivery_longitude\": -77.31366340, \"deliver_omw_latitude\": null, \"reference_text\": \"2086108801\", \"reference\": \"2086108801\", \"add_charge_occur6\": null, \"additional_drivers\": false, \"cod_accept_cashiers_check\": false, \"hist_inv_date\": null, \"add_charge_amt9\": null, \"delivery_point_customer\": 31025, \"deliver_requested_arr_time\": \"08:00\", \"add_charge_occur10\": null, \"add_charge_amt6\": null, \"rate_chart_used\": 0, \"pickup_sign_req\": true, \"fuel_plan\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"rate_buck_amt11\": null, \"fuel_price_zone\": null, \"add_charge_code11\": null, \"add_charge_amt4\": null, \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"pickup_state\": \"VA\", \"add_charge_amt5\": null, \"az_equip2\": null, \"line_items\": [], \"pickup_special_instructions4\": null, \"deliver_attention\": null, \"deliver_eta_time\": null, \"del_actual_location_accuracy\": null, \"deliver_omw_longitude\": null, \"pickup_name\": \"HUMAN TOUCH\", \"add_charge_code3\": null, \"order_automatically_quoted\": false, \"add_charge_amt7\": null, \"pu_actual_location_accuracy\": null, \"pickup_signature\": \"SOF\", \"original_schedule_number\": null, \"rate_buck_amt10\": 2.16, \"callback_to\": null, \"customers_etrac_partner_id\": \"96609250\", \"ordered_by\": \"RYDER\", \"pu_arrive_notification_sent\": false, \"hist_inv_number\": 0, \"rate_buck_amt9\": null, \"pickup_special_instructions2\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"progress\": [{\"status_date\": \"2021-06-30\", \"status_text\": \"Entered in carrier's system\", \"status_time\": \"08:05:00\"}, {\"status_date\": \"2021-05-10\", \"status_text\": \"Picked up\", \"status_time\": \"08:30:00\"}, {\"status_date\": \"2021-05-10\", \"status_text\": \"Delivered\", \"status_time\": \"08:15:00\"}], \"pickup_phone_ext\": null, \"delivery_address_point_number_text\": \"MICHAEL BROWN\", \"delivery_address_point_number\": 25113, \"deliver_wait_time\": null, \"cod_amount\": null, \"pickup_actual_pieces\": null, \"pickup_eta_time\": null, \"rate_buck_amt1\": 80.00, \"deliver_requested_date\": \"2021-05-10\", \"bringg_order_id\": null, \"roundtrip_signature\": null, \"add_charge_amt12\": null, \"return_svc_level\": null, \"rate_buck_amt4\": null, \"deliver_country\": null, \"add_charge_code2\": null, \"number_of_pieces\": 1, \"push_services\": null, \"add_charge_occur7\": null, \"pickup_actual_arr_time\": \"08:00\", \"deliver_city\": \"STAFFORD\", \"pickup_special_instr_long\": null, \"fuel_miles\": null, \"image_sign_req\": false, \"add_charge_amt10\": null, \"po_number\": null, \"verified_weight\": null, \"add_charge_code1\": null, \"callback_time\": null, \"blocks\": null, \"send_new_order_alert\": false, \"pickup_omw_latitude\": null, \"add_charge_code4\": null, \"pickup_attention\": null, \"rate_buck_amt8\": null, \"pickup_requested_date\": \"2021-05-10\", \"manual_notepad\": false, \"control_number\": 1870440, \"deliver_dispatch_zone\": null, \"service_level_text\": \"White Glove Delivery\", \"service_level\": 58, \"dl_arrive_notification_sent\": false, \"custom_special_instr_long\": null, \"pickup_latitude\": 37.53250820, \"pickup_actual_longitude\": null, \"pickup_city\": \"SANDSTON\", \"deliver_special_instructions2\": null, \"pickup_email_notification_sent\": false, \"add_charge_occur3\": null, \"rate_buck_amt6\": null, \"pickup_requested_arr_time\": \"07:00\", \"pickup_country\": null, \"deliver_room\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"photos_exist\": false, \"rate_special_instructions\": null, \"pickup_omw_timestamp\": null, \"add_charge_amt8\": null, \"deliver_actual_pieces\": null, \"pickup_address_point_number_text\": \"HUMAN TOUCH\", \"pickup_address_point_number\": 19891, \"pickup_longitude\": -77.33035820, \"insurance_amount\": null, \"dispatch_id\": null, \"calc_add_on_chgs\": false, \"deliver_route_code\": null, \"original_ctrl_number\": null, \"pickup_actual_date\": \"2021-05-10\", \"az_equip3\": null, \"date_order_entered\": \"2021-06-30\", \"exception_code\": null, \"deliver_actual_date\": \"2021-05-10\", \"delivery_latitude\": 38.37859180, \"rescheduled_ctrl_number\": null, \"deliver_actual_dep_time\": \"08:15\", \"quote_amount\": null, \"deliver_phone_ext\": null, \"add_charge_code8\": null, \"deliver_phone\": null, \"add_charge_occur11\": null, \"signature_required\": true, \"deliver_eta_date\": null, \"weight\": null, \"add_charge_occur1\": null, \"deliver_special_instructions4\": null, \"ordered_by_phone_number\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"customer_name\": \"MXD/RYDER\", \"csr\": \"DX*\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"invoice_period_end_date\": null, \"add_charge_amt2\": null, \"distribution_branch_id\": null, \"roundtrip_actual_latitude\": null, \"add_charge_code7\": null, \"cod_accept_company_check\": false, \"email_addresses\": null, \"add_charge_code9\": null, \"pickup_route_seq\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"deliver_route_sequence\": null, \"settlements\": [{\"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"vendor_invoice_number\": null, \"date_last_updated\": \"2021-06-30\", \"charge3\": null, \"settlement_bucket6_pct\": null, \"settlement_pct\": 100.00, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"settlement_bucket2_pct\": null, \"pay_chart_used\": null, \"settlement_bucket3_pct\": null, \"order_date\": \"2021-05-10\", \"charge5\": null, \"fuel_price_zone\": null, \"voucher_amount\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"voucher_date\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"settlement_period_end_date\": null, \"charge6\": null, \"fuel_plan\": null, \"charge2\": null, \"vendor_employee_numer\": null, \"control_number\": 1870440, \"settlement_bucket5_pct\": null, \"settlement_bucket4_pct\": null, \"adjustment_type\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"driver_number_text\": \"LAVERT KENDALL MORRIS\", \"driver_number\": 3001, \"time_last_updated\": \"07:05\", \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"agent_etrac_transaction_number\": null, \"agents_etrac_partner_id\": null, \"charge4\": null, \"voucher_number\": null, \"pre_book_percentage\": true, \"id\": \"002018704400D1\", \"charge1\": null, \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"settlement_bucket1_pct\": null, \"fuel_price_source\": null, \"record_type\": 0}], \"add_charge_code10\": null, \"push_partner_order_id\": null, \"edi_acknowledgement_required\": false, \"roundtrip_actual_date\": null, \"pickup_requested_dep_time\": \"09:00\", \"pickup_phone\": null, \"roundtrip_actual_arrival_time\": null, \"pickup_room\": null, \"callback_userid\": null, \"vehicle_type\": null, \"add_charge_occur4\": null, \"add_charge_amt1\": null, \"pickup_dispatch_zone\": null, \"roundtrip_actual_longitude\": null, \"pickup_eta_date\": null, \"pickup_route_code\": null, \"hazmat\": false, \"pickup_special_instructions1\": null, \"total_pages\": 1, \"record_type\": 0, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"pickup_point_customer\": 31025, \"pickup_address\": \"540 EASTPARK CT\", \"callback_date\": null, \"deliver_state\": \"VA\", \"exception_sign_required\": false, \"add_charge_code12\": null, \"holiday_groups\": null, \"az_equip1\": null, \"driver1_text\": \"LAVERT KENDALL MORRIS\", \"driver1\": 3001, \"rate_miles\": null, \"rt_actual_location_accuracy\": null, \"id\": \"002018704400\", \"add_charge_occur5\": null, \"house_airway_bill_number\": null, \"deliver_special_instructions1\": null, \"exception_timestamp\": null, \"add_charge_occur2\": null, \"pickup_actual_dep_time\": \"08:30\", \"deliver_zip\": \"22554\", \"deliver_actual_arr_time\": \"08:00\", \"add_charge_code6\": null, \"add_charge_occur12\": null, \"bringg_last_loc_sent\": null, \"zone_set_used\": 1}}";
            // objresponse.Reason = "{\"002018707560\": {\"cod_text\": \"No\", \"cod\": \"N\", \"signature\": \"SOF\", \"edi_order_accepted_or_rejected_text\": \"\", \"edi_order_accepted_or_rejected\": null, \"add_charge_occur8\": null, \"pickup_airport_code\": null, \"hours\": \"15\", \"roundtrip_actual_depart_time\": null, \"bol_number\": null, \"deliver_address\": \"134 CANTERBURY DR\", \"time_order_entered\": \"07:45\", \"driver2\": null, \"add_charge_amt11\": null, \"rate_buck_amt3\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"pickup_special_instructions3\": null, \"roundtrip_actual_pieces\": null, \"deliver_actual_longitude\": null, \"fuel_price_source\": null, \"roundtrip_wait_time\": null, \"deliver_actual_latitude\": null, \"deliver_special_instructions3\": null, \"deliver_name\": \"MICHAEL BROWN\", \"rate_buck_amt7\": null, \"add_charge_amt3\": null, \"add_charge_code5\": null, \"delivery_airport_code\": null, \"master_airway_bill_number\": null, \"distribution_shift_id\": null, \"deliver_requested_dep_time\": \"17:00\", \"page_number\": 1, \"add_charge_occur9\": null, \"rate_buck_amt5\": null, \"pickup_pricing_zone\": 1, \"frequent_caller_id\": null, \"pickup_actual_latitude\": null, \"pickup_omw_longitude\": null, \"distribution_unique_id\": 0, \"order_type_text\": \"One way\", \"order_type\": \"O\", \"bringg_send_sms\": false, \"pickup_zip\": \"23150\", \"pick_del_trans_flag_text\": \"Transfer\", \"pick_del_trans_flag\": \"T\", \"previous_ctrl_number\": null, \"roundtrip_sign_req\": false, \"notes\": [], \"actual_miles\": null, \"etrac_number\": null, \"deliver_omw_timestamp\": null, \"deliver_pricing_zone\": 1, \"rate_buck_amt2\": null, \"signature_images\": [], \"deliver_special_instr_long\": null, \"pickup_wait_time\": null, \"dispatch_time\": null, \"delivery_longitude\": -77.31366340, \"deliver_omw_latitude\": null, \"reference_text\": \"2086108801\", \"reference\": \"2086108801\", \"add_charge_occur6\": null, \"additional_drivers\": false, \"cod_accept_cashiers_check\": false, \"hist_inv_date\": null, \"add_charge_amt9\": null, \"delivery_point_customer\": 31025, \"deliver_requested_arr_time\": \"08:00\", \"add_charge_occur10\": null, \"add_charge_amt6\": null, \"rate_chart_used\": 0, \"pickup_sign_req\": true, \"fuel_plan\": null, \"customer_number_text\": \"MXD/Ryder\", \"customer_number\": 31025, \"rate_buck_amt11\": null, \"fuel_price_zone\": null, \"add_charge_code11\": null, \"add_charge_amt4\": null, \"customer_type_text\": \"Philadelphia\", \"customer_type\": \"P\", \"pickup_state\": \"VA\", \"add_charge_amt5\": null, \"az_equip2\": null, \"line_items\": [], \"pickup_special_instructions4\": null, \"deliver_attention\": null, \"deliver_eta_time\": null, \"del_actual_location_accuracy\": null, \"deliver_omw_longitude\": null, \"pickup_name\": \"HUMAN TOUCH\", \"add_charge_code3\": null, \"order_automatically_quoted\": false, \"add_charge_amt7\": null, \"pu_actual_location_accuracy\": null, \"pickup_signature\": \"SOF\", \"original_schedule_number\": null, \"rate_buck_amt10\": 2.16, \"callback_to\": null, \"customers_etrac_partner_id\": \"96609250\", \"ordered_by\": \"RYDER\", \"pu_arrive_notification_sent\": false, \"hist_inv_number\": 0, \"rate_buck_amt9\": null, \"pickup_special_instructions2\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"progress\": [{\"status_date\": \"2021-07-01\", \"status_text\": \"Entered in carrier's system\", \"status_time\": \"07:45:00\"}, {\"status_date\": \"2021-05-10\", \"status_text\": \"Picked up\", \"status_time\": \"08:30:00\"}, {\"status_date\": \"2021-05-10\", \"status_text\": \"Delivered\", \"status_time\": \"08:15:00\"}], \"pickup_phone_ext\": null, \"delivery_address_point_number_text\": \"MICHAEL BROWN\", \"delivery_address_point_number\": 25113, \"deliver_wait_time\": null, \"cod_amount\": null, \"pickup_actual_pieces\": null, \"pickup_eta_time\": null, \"rate_buck_amt1\": 80.00, \"deliver_requested_date\": \"2021-05-10\", \"bringg_order_id\": null, \"roundtrip_signature\": null, \"add_charge_amt12\": null, \"return_svc_level\": null, \"rate_buck_amt4\": null, \"deliver_country\": null, \"add_charge_code2\": null, \"number_of_pieces\": 1, \"push_services\": null, \"add_charge_occur7\": null, \"pickup_actual_arr_time\": \"08:00\", \"deliver_city\": \"STAFFORD\", \"pickup_special_instr_long\": null, \"fuel_miles\": null, \"image_sign_req\": false, \"add_charge_amt10\": null, \"po_number\": null, \"verified_weight\": null, \"add_charge_code1\": null, \"callback_time\": null, \"blocks\": null, \"send_new_order_alert\": false, \"pickup_omw_latitude\": null, \"add_charge_code4\": null, \"pickup_attention\": null, \"rate_buck_amt8\": null, \"pickup_requested_date\": \"2021-05-10\", \"manual_notepad\": false, \"control_number\": 1870756, \"deliver_dispatch_zone\": null, \"service_level_text\": \"White Glove Delivery\", \"service_level\": 58, \"dl_arrive_notification_sent\": false, \"custom_special_instr_long\": null, \"pickup_latitude\": 37.53250820, \"pickup_actual_longitude\": null, \"pickup_city\": \"SANDSTON\", \"deliver_special_instructions2\": null, \"pickup_email_notification_sent\": false, \"add_charge_occur3\": null, \"rate_buck_amt6\": null, \"pickup_requested_arr_time\": \"07:00\", \"pickup_country\": null, \"deliver_room\": null, \"status_code_text\": \"Rated\", \"status_code\": \"R\", \"photos_exist\": false, \"rate_special_instructions\": null, \"pickup_omw_timestamp\": null, \"add_charge_amt8\": null, \"deliver_actual_pieces\": null, \"pickup_address_point_number_text\": \"HUMAN TOUCH\", \"pickup_address_point_number\": 19891, \"pickup_longitude\": -77.33035820, \"insurance_amount\": null, \"dispatch_id\": null, \"calc_add_on_chgs\": false, \"deliver_route_code\": null, \"original_ctrl_number\": null, \"pickup_actual_date\": \"2021-05-10\", \"az_equip3\": null, \"date_order_entered\": \"2021-07-01\", \"exception_code\": null, \"deliver_actual_date\": \"2021-05-10\", \"delivery_latitude\": 38.37859180, \"rescheduled_ctrl_number\": null, \"deliver_actual_dep_time\": \"08:15\", \"quote_amount\": null, \"deliver_phone_ext\": null, \"add_charge_code8\": null, \"deliver_phone\": null, \"add_charge_occur11\": null, \"signature_required\": true, \"deliver_eta_date\": null, \"weight\": null, \"add_charge_occur1\": null, \"deliver_special_instructions4\": null, \"ordered_by_phone_number\": null, \"origin_code_text\": \"Web-Carrier API\", \"origin_code\": \"W\", \"customer_name\": \"MXD/RYDER\", \"csr\": \"DX*\", \"powerpage_status_text\": \"\", \"powerpage_status\": \"0\", \"invoice_period_end_date\": null, \"add_charge_amt2\": null, \"distribution_branch_id\": null, \"roundtrip_actual_latitude\": null, \"add_charge_code7\": null, \"cod_accept_company_check\": false, \"email_addresses\": null, \"add_charge_code9\": null, \"pickup_route_seq\": null, \"exception_order_action_text\": \"Close order\", \"exception_order_action\": \"0\", \"deliver_route_sequence\": null, \"settlements\": [{\"driver_company_number_text\": \"JW LOGISTICS EAST REGION\", \"driver_company_number\": 2, \"posting_status_text\": \"Not processed\", \"posting_status\": \"0\", \"vendor_invoice_number\": null, \"date_last_updated\": \"2021-07-01\", \"charge3\": null, \"settlement_bucket6_pct\": null, \"settlement_pct\": 100.00, \"transaction_type_text\": \"Driver\", \"transaction_type\": \"D\", \"settlement_bucket2_pct\": null, \"pay_chart_used\": null, \"settlement_bucket3_pct\": null, \"order_date\": \"2021-05-10\", \"charge5\": null, \"fuel_price_zone\": null, \"voucher_amount\": null, \"fuel_update_freq_text\": \"Weekly\", \"fuel_update_freq\": \"0\", \"voucher_date\": null, \"file_status_text\": \"Order\", \"file_status\": \"O\", \"settlement_period_end_date\": null, \"charge6\": null, \"fuel_plan\": null, \"charge2\": null, \"vendor_employee_numer\": null, \"control_number\": 1870756, \"settlement_bucket5_pct\": null, \"settlement_bucket4_pct\": null, \"adjustment_type\": null, \"company_number_text\": \"JW LOGISTICS EAST REGION\", \"company_number\": 2, \"driver_number_text\": \"LAVERT KENDALL MORRIS\", \"driver_number\": 3001, \"time_last_updated\": \"06:45\", \"agent_accepted_or_rejected_text\": \"\", \"agent_accepted_or_rejected\": null, \"agent_etrac_transaction_number\": null, \"agents_etrac_partner_id\": null, \"charge4\": null, \"voucher_number\": null, \"pre_book_percentage\": true, \"id\": \"002018707560D1\", \"charge1\": null, \"driver_sequence_text\": \"1\", \"driver_sequence\": \"1\", \"settlement_bucket1_pct\": null, \"fuel_price_source\": null, \"record_type\": 0}], \"add_charge_code10\": null, \"push_partner_order_id\": null, \"edi_acknowledgement_required\": false, \"roundtrip_actual_date\": null, \"pickup_requested_dep_time\": \"09:00\", \"pickup_phone\": null, \"roundtrip_actual_arrival_time\": null, \"pickup_room\": null, \"callback_userid\": null, \"vehicle_type\": null, \"add_charge_occur4\": null, \"add_charge_amt1\": null, \"pickup_dispatch_zone\": null, \"roundtrip_actual_longitude\": null, \"pickup_eta_date\": null, \"pickup_route_code\": null, \"hazmat\": false, \"pickup_special_instructions1\": null, \"total_pages\": 1, \"record_type\": 0, \"order_timeliness_text\": \"On time\", \"order_timeliness\": \"2\", \"callback_required_text\": \"No\", \"callback_required\": \"N\", \"pickup_point_customer\": 31025, \"pickup_address\": \"540 EASTPARK CT\", \"callback_date\": null, \"deliver_state\": \"VA\", \"exception_sign_required\": false, \"add_charge_code12\": null, \"holiday_groups\": null, \"az_equip1\": null, \"driver1_text\": \"LAVERT KENDALL MORRIS\", \"driver1\": 3001, \"rate_miles\": null, \"rt_actual_location_accuracy\": null, \"id\": \"002018707560\", \"add_charge_occur5\": null, \"house_airway_bill_number\": null, \"deliver_special_instructions1\": null, \"exception_timestamp\": null, \"add_charge_occur2\": null, \"pickup_actual_dep_time\": \"08:30\", \"deliver_zip\": \"22554\", \"deliver_actual_arr_time\": \"08:00\", \"add_charge_code6\": null, \"add_charge_occur12\": null, \"bringg_last_loc_sent\": null, \"zone_set_used\": 1}}";
            //  objresponse.ResponseVal = true;
            return objresponse;
        }

        public void OrderSettlementPut(string strInputFilePath, string filename, string ReferenceId, string UniqueId, int company_number,
            int control_number, double charge1 = 0, double charge5 = 0, double charge2 = 0, double charge3 = 0, double charge4 = 0, double charge6 = 0)
        {
            string request, strExecutionLogMessage;
            clsCommon objCommon = new clsCommon();
            clsDatatrac clsDatatrac = new clsDatatrac();
            try
            {

                string strExecutionLogFileLocation;
                strExecutionLogFileLocation = objCommon.GetConfigValue("ExecutionLogFileLocation");

                int record_type = 0;
                string transaction_type = "D"; // 
                string driver_sequence = "1";

                //    objorderssettlement.driver_number = 3001;

                //order_settlementdetails objorder_settlementdetails = new order_settlementdetails();
                //objorder_settlementdetails.order_settlement = objorderssettlement;
                clsCommon.ReturnResponse objresponseOrdersettlement = new clsCommon.ReturnResponse();


                string ordersettlementputrequest = null;
                if (company_number != null)
                {
                    ordersettlementputrequest = @"'company_number': " + company_number + ",";
                }
                if (control_number != null)
                {
                    ordersettlementputrequest = ordersettlementputrequest + @"'control_number': " + control_number + ",";
                }
                if (record_type != null)
                {
                    ordersettlementputrequest = ordersettlementputrequest + @"'record_type': " + record_type + ",";
                }
                if (transaction_type != null)
                {
                    ordersettlementputrequest = ordersettlementputrequest + @"'transaction_type': '" + transaction_type + "',";
                }
                if (driver_sequence != null)
                {
                    ordersettlementputrequest = ordersettlementputrequest + @"'driver_sequence': '" + driver_sequence + "',";
                }
                if (charge1 != 0)
                {
                    ordersettlementputrequest = ordersettlementputrequest + @"'charge1': " + charge1 + ",";
                }
                if (charge2 != 0)
                {
                    ordersettlementputrequest = ordersettlementputrequest + @"'charge2': " + charge2 + ",";
                }
                if (charge3 != 0)
                {
                    ordersettlementputrequest = ordersettlementputrequest + @"'charge3': " + charge3 + ",";
                }
                if (charge4 != 0)
                {
                    ordersettlementputrequest = ordersettlementputrequest + @"'charge4': " + charge4 + ",";
                }
                if (charge5 != 0)
                {
                    ordersettlementputrequest = ordersettlementputrequest + @"'charge5': " + charge5 + ",";
                }
                if (charge6 != 0)
                {
                    ordersettlementputrequest = ordersettlementputrequest + @"'charge6': " + charge6 + ",";
                }

                ordersettlementputrequest = @"{" + ordersettlementputrequest + "}";

                string order_settlementObject = @"{'order_settlement': " + ordersettlementputrequest + "}";
                JObject jsonobj = JObject.Parse(order_settlementObject);
                request = jsonobj.ToString();
                //  objresponseOrdersettlement = CallDataTracOrderSettlementPutAPI(UniqueId, order_settlementObject);
                if (objresponseOrdersettlement.ResponseVal)
                {
                    // request = JsonConvert.SerializeObject(objresponseOrdersettlement);
                    strExecutionLogMessage = "OrderSettlementPutAPI Success " + System.Environment.NewLine;
                    strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                    strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                    objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                    DataSet dsOrderSettlementResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                    // objCommon.WriteDatatracResponseToOutputFile(dsOrderSettlementResponse, strInputFilePath, ReferenceId, "S", filename);
                }
                else
                {
                    request = JsonConvert.SerializeObject(objresponseOrdersettlement);
                    strExecutionLogMessage = "OrderSettlementPutAPI Failed " + System.Environment.NewLine;
                    strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                    strExecutionLogMessage += "Response -" + objresponseOrdersettlement.Reason + System.Environment.NewLine;
                    DataSet dsOrderSettlementResponse = objCommon.jsonToDataSet(objresponseOrdersettlement.Reason);
                    // objCommon.WriteDatatracResponseToOutputFile(dsOrderSettlementResponse, strInputFilePath, ReferenceId, "E", filename);
                    objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);

                }
            }
            catch (Exception ex)
            {
                // objCommon.LogEvents(ex, "OrderSettlementPut", System.Diagnostics.EventLogEntryType.Error, 2);
                objCommon.WriteErrorLog(ex);
            }
        }

        public void OrderPut(string strLocationFolder, string ReferenceId, string UniqueId, int company_number,
            int control_number, int driver1, string pickup_zip)
        {
            string request, strExecutionLogMessage;
            clsCommon objCommon = new clsCommon();
            clsDatatrac clsDatatrac = new clsDatatrac();
            try
            {

                string strExecutionLogFileLocation;
                strExecutionLogFileLocation = objCommon.GetConfigValue("ExecutionLogFileLocation");
                //    string orderputrequest = @"{'order': {'driver1': " + objOrder.driver1 + "    }}";
                string orderputrequest = null;
                if (company_number != null)
                {
                    orderputrequest = @"'company_number': " + company_number + ",";
                }
                if (control_number != null)
                {
                    orderputrequest = orderputrequest + @"'control_number': " + control_number + ",";
                }
                if (driver1 != 0)
                {
                    orderputrequest = orderputrequest + @"'driver1': " + driver1 + ",";
                }
                if (pickup_zip != null)
                {
                    orderputrequest = orderputrequest + @"'pickup_zip': " + pickup_zip + ",";
                }




                orderputrequest = @"{" + orderputrequest + "}";

                string orderObject = @"{'order': " + orderputrequest + "}";

                JObject jsonobj = JObject.Parse(orderObject);
                request = jsonobj.ToString();
                clsCommon.ReturnResponse objresponseorderput = new clsCommon.ReturnResponse();
                //    objresponseorderput = clsDatatrac.CallDataTracOrderPutAPI(UniqueId, orderObject);
                if (objresponseorderput.ResponseVal)
                {
                    //request = JsonConvert.SerializeObject(objresponseorderput);
                    strExecutionLogMessage = "OrderPutAPI Success " + System.Environment.NewLine;
                    strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                    strExecutionLogMessage += "Response -" + objresponseorderput.Reason + System.Environment.NewLine;
                    objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                    DataSet dsOrderSettlementResponse = objCommon.jsonToDataSet(objresponseorderput.Reason);
                    // objCommon.WriteDatatracResponseToOutputFile(dsOrderSettlementResponse, strLocationFolder, "Orderput", ReferenceId, @"Order\Update");
                }
                else
                {
                    //request = JsonConvert.SerializeObject(objresponseorderput);
                    strExecutionLogMessage = "OrderPutAPI Failed " + System.Environment.NewLine;
                    strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                    strExecutionLogMessage += "Response -" + objresponseorderput.Reason + System.Environment.NewLine;
                    objCommon.WriteExecutionLog(strExecutionLogFileLocation, strExecutionLogMessage);
                }
            }
            catch (Exception ex)
            {
                //objCommon.LogEvents(ex, "OrderPut", System.Diagnostics.EventLogEntryType.Error, 3);
                objCommon.WriteErrorLog(ex);
            }
        }


        public string GenerateUniqueNumber(int company_number, int control_number, string orderSettlement_UniqueIdSuffix = "")
        {
            string result;
            // result = company_number.ToString().PadLeft(3, '0') + "0" + control_number + "0";
            result = company_number.ToString().PadLeft(3, '0') + control_number.ToString().PadLeft(8, '0') + "0";
            if (orderSettlement_UniqueIdSuffix != "")
            {
                result = result + orderSettlement_UniqueIdSuffix; // "D1";   //get it from excel future mapping
            }
            return result;
        }
    }
    public class order
    {
        public int company_number { get; set; }
        public int customer_number { get; set; }
        public string pickup_requested_date { get; set; }
        public string pickup_requested_arr_time { get; set; }
        public string pickup_name { get; set; }
        public string pickup_address { get; set; }
        public string pickup_city { get; set; }
        public string pickup_state { get; set; }
        public string pickup_zip { get; set; }
        public string deliver_name { get; set; }
        public string deliver_address { get; set; }
        public string deliver_city { get; set; }
        public string deliver_zip { get; set; }
        public string deliver_state { get; set; }
        //   public string deliver_special_instructions1 { get; set; }
        //   public string deliver_special_instructions2 { get; set; }
        public string pick_del_trans_flag { get; set; }
        public string ordered_by { get; set; }
        public int service_level { get; set; }
        public string reference { get; set; }
        //public int weight { get; set; }
        //  public List<notes> notes { get; set; }
        public int driver1 { get; set; }
        public string pickup_requested_dep_time { get; set; }
        public string pickup_actual_arr_time { get; set; }
        public string pickup_actual_dep_time { get; set; }
        public string deliver_requested_arr_time { get; set; }
        public string deliver_requested_dep_time { get; set; }
        public string deliver_actual_arr_time { get; set; }
        public string deliver_actual_dep_time { get; set; }
        public string signature { get; set; }
        public string csr { get; set; }
        public int number_of_pieces { get; set; }
        public string pickup_signature { get; set; }
        public string deliver_requested_date { get; set; }
        public string pickup_actual_date { get; set; }
        public string deliver_actual_date { get; set; }
        public double rate_buck_amt1 { get; set; }
        public double rate_buck_amt3 { get; set; }
        public double rate_buck_amt10 { get; set; }
        public int rate_miles { get; set; }
        // public double add_charge_amt1 { get; set; }
        // public double add_charge_amt5 { get; set; }
        public string bol_number { get; set; }
    }
    public class notes
    {
        public string note_code { get; set; }
        public string note_line { get; set; }
    }
    public class orderdetails
    {
        public order order { get; set; }
    }

    //public class dispatchTrack
    //{
    //    public string Customer { get; set; }
    //    public string Address { get; set; }
    //    public string City { get; set; }
    //    public string State { get; set; }
    //    public double Zip { get; set; }
    //}

    public class order_settlementdetails
    {
        public order_settlement order_settlement { get; set; }
    }
    public class order_settlement
    {
        public string id { get; set; }
        public int company_number { get; set; }
        public int control_number { get; set; }
        public int record_type { get; set; }  //0 
        public string transaction_type { get; set; }  //D
        public string driver_sequence { get; set; } //1
        public double charge1 { get; set; }
        public double charge5 { get; set; }
        // public int driver_number { get; set; }

    }

    public class orderput
    {
        //  public string id { get; set; }
        public int company_number { get; set; }
        public int control_number { get; set; }
        public int driver1 { get; set; }
    }

    public class RootObject
    {
        public string name { get; set; }
        public string type { get; set; }
        public string id { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 

    public class Id
    {
        public object id { get; set; }
        public object company_number { get; set; }
        public object control_number { get; set; }
        public object service_level { get; set; }
        public object customer_number { get; set; }
        public object reference { get; set; }
        public object bol_number { get; set; }
        public object pickup_requested_date { get; set; }
        public object pickup_requested_arr_time { get; set; }
        public object pickup_requested_dep_time { get; set; }
        public object pickup_actual_date { get; set; }
        public object pickup_actual_arr_time { get; set; }
        public object pickup_actual_dep_time { get; set; }
        public object pickup_name { get; set; }
        public object pickup_address { get; set; }
        public object pickup_city { get; set; }
        public object pickup_state { get; set; }
        public object pickup_zip { get; set; }
        public object pickup_signature { get; set; }
        public object deliver_requested_date { get; set; }
        public object deliver_requested_arr_time { get; set; }
        public object deliver_requested_dep_time { get; set; }
        public object deliver_actual_date { get; set; }
        public object deliver_actual_arr_time { get; set; }
        public object deliver_actual_dep_time { get; set; }
        public object deliver_name { get; set; }
        public object deliver_address { get; set; }
        public object deliver_city { get; set; }
        public object deliver_state { get; set; }
        public object deliver_zip { get; set; }
        public object signature { get; set; }
        public object rate_buck_amt1 { get; set; }
        public object rate_buck_amt3 { get; set; }
        public object rate_buck_amt10 { get; set; }
        public object number_of_pieces { get; set; }
        public object deliver_actual_pieces { get; set; }
        public object rate_miles { get; set; }
        public object actual_miles { get; set; }
        public object driver1 { get; set; }
        public object ordered_by { get; set; }
        public object csr { get; set; }
        public object reference_text { get; set; }
        public object roundtrip_actual_date { get; set; }
        public List<object> notes { get; set; }
        public object pickup_phone_ext { get; set; }
        public object holiday_groups { get; set; }
        public object deliver_eta_time { get; set; }
        public object powerpage_status_text { get; set; }
        public object powerpage_status { get; set; }
        public object add_charge_occur4 { get; set; }
       
        public object quote_amount { get; set; }
        public object cod_text { get; set; }
        public object cod { get; set; }
        public object additional_drivers { get; set; }
        public object rescheduled_ctrl_number { get; set; }
        public object edi_order_accepted_or_rejected_text { get; set; }
        public object edi_order_accepted_or_rejected { get; set; }
        public object pickup_actual_pieces { get; set; }
        public object record_type { get; set; }
        public object pickup_special_instr_long { get; set; }
        public object pickup_special_instructions3 { get; set; }
        public object exception_timestamp { get; set; }

        public object house_airway_bill_number { get; set; }
        public object deliver_pricing_zone { get; set; }
        public object total_pages { get; set; }
        public object add_charge_occur11 { get; set; }
        public object deliver_omw_latitude { get; set; }
        public object callback_userid { get; set; }
      
        public object pickup_point_customer { get; set; }
        public object pickup_eta_time { get; set; }
        public object add_charge_occur8 { get; set; }
        public object invoice_period_end_date { get; set; }
        public object pickup_special_instructions1 { get; set; }
        public object rate_buck_amt2 { get; set; }
        public object pickup_special_instructions4 { get; set; }
        public object manual_notepad { get; set; }
        public object edi_acknowledgement_required { get; set; }

        public object ordered_by_phone_number { get; set; }
        public object add_charge_amt12 { get; set; }
        public object delivery_point_customer { get; set; }
       
        public object email_addresses { get; set; }

        public object driver2 { get; set; }
        public List<object> signature_images { get; set; }
        public object rate_buck_amt11 { get; set; }
        public object delivery_latitude { get; set; }
        public object pickup_attention { get; set; }
        public object date_order_entered { get; set; }
        public object vehicle_type { get; set; }
        public object add_charge_amt9 { get; set; }
        public object pickup_phone { get; set; }
       
        public object customers_etrac_partner_id { get; set; }
        public object order_type_text { get; set; }
        public object order_type { get; set; }
        public object dl_arrive_notification_sent { get; set; }
        public object add_charge_code3 { get; set; }
        public object etrac_number { get; set; }

      

        public List<object> line_items { get; set; }
        public object pickup_sign_req { get; set; }
        public object add_charge_code10 { get; set; }
     
        public object fuel_plan { get; set; }
        public object add_charge_amt10 { get; set; }
        public object roundtrip_actual_depart_time { get; set; }
        //  public object control_number { get; set; }
        public object pickup_dispatch_zone { get; set; }
        public object send_new_order_alert { get; set; }
        //  public List<Settlement> settlements { get; set; }
        public object deliver_actual_latitude { get; set; }
        public object fuel_price_zone { get; set; }
        public object verified_weight { get; set; }

        public object pickup_airport_code { get; set; }
        public object dispatch_time { get; set; }
        public object deliver_attention { get; set; }
        public object time_order_entered { get; set; }
        public object rate_buck_amt4 { get; set; }
        public object roundtrip_wait_time { get; set; }
        public object add_charge_amt2 { get; set; }
        public object az_equip3 { get; set; }
        //public List<Progress> progress { get; set; }
        public object page_number { get; set; }
        public object roundtrip_sign_req { get; set; }
        public object add_charge_amt1 { get; set; }
        public object add_charge_code8 { get; set; }
        public object weight { get; set; }
        public object rate_buck_amt6 { get; set; }
        public object customer_type_text { get; set; }
        public object customer_type { get; set; }
        public object bringg_send_sms { get; set; }
        public object exception_order_action_text { get; set; }
        public object exception_order_action { get; set; }
        public object custom_special_instr_long { get; set; }

        public object service_level_text { get; set; }

        public object az_equip1 { get; set; }
        public object add_charge_code4 { get; set; }
        public object bringg_order_id { get; set; }
        public object delivery_address_point_number_text { get; set; }
        public object delivery_address_point_number { get; set; }
        public object pick_del_trans_flag_text { get; set; }
        public object pick_del_trans_flag { get; set; }
        public object deliver_special_instructions1 { get; set; }
        public object pickup_wait_time { get; set; }
        public object add_charge_occur5 { get; set; }
        public object push_partner_order_id { get; set; }
        public object deliver_route_sequence { get; set; }
        public object pickup_country { get; set; }

        public object original_schedule_number { get; set; }
        public object frequent_caller_id { get; set; }
        public object distribution_unique_id { get; set; }
        public object fuel_miles { get; set; }
        public object status_code_text { get; set; }
        public object status_code { get; set; }
        public object rate_buck_amt5 { get; set; }
        public object exception_sign_required { get; set; }
        public object pickup_route_code { get; set; }
        public object deliver_dispatch_zone { get; set; }
        public object delivery_longitude { get; set; }
        public object pickup_pricing_zone { get; set; }
        public object zone_set_used { get; set; }
        public object deliver_special_instructions2 { get; set; }
        public object add_charge_amt3 { get; set; }
        public object deliver_phone { get; set; }
        public object pickup_email_notification_sent { get; set; }
        public object add_charge_occur12 { get; set; }



        public object deliver_actual_longitude { get; set; }
        public object image_sign_req { get; set; }
        public object pickup_eta_date { get; set; }
        public object deliver_phone_ext { get; set; }
        public object pickup_omw_longitude { get; set; }
        public object original_ctrl_number { get; set; }
        public object pickup_special_instructions2 { get; set; }
        public object order_automatically_quoted { get; set; }

       
        public object callback_time { get; set; }
        public object hazmat { get; set; }
        public object distribution_shift_id { get; set; }
        public object pickup_latitude { get; set; }
       
        public object insurance_amount { get; set; }
        public object cod_accept_cashiers_check { get; set; }
        public object add_charge_amt4 { get; set; }
        public object add_charge_code7 { get; set; }
       
        public object cod_accept_company_check { get; set; }
        
        public object previous_ctrl_number { get; set; }
       
        public object deliver_special_instructions3 { get; set; }
        public object rate_buck_amt7 { get; set; }
        public object hist_inv_number { get; set; }
        public object callback_date { get; set; }
        public object deliver_special_instr_long { get; set; }
        public object po_number { get; set; }


      
        public object dispatch_id { get; set; }
        public object photos_exist { get; set; }
        public object pickup_actual_latitude { get; set; }
        public object fuel_update_freq_text { get; set; }
        public object fuel_update_freq { get; set; }
        // public object id { get; set; }
        public object company_number_text { get; set; }
        // public object company_number { get; set; }
        public object del_actual_location_accuracy { get; set; }
        public object add_charge_occur7 { get; set; }
        public object add_charge_occur9 { get; set; }
        public object roundtrip_actual_latitude { get; set; }
        public object add_charge_occur6 { get; set; }
        public object pickup_actual_longitude { get; set; }
        public object pickup_omw_timestamp { get; set; }
        public object bringg_last_loc_sent { get; set; }
        public object add_charge_code5 { get; set; }
        public object deliver_country { get; set; }
        public object master_airway_bill_number { get; set; }
        public object pickup_route_seq { get; set; }
        public object roundtrip_signature { get; set; }
        public object calc_add_on_chgs { get; set; }

        public object cod_amount { get; set; }
        public object add_charge_code12 { get; set; }
        public object rt_actual_location_accuracy { get; set; }
        public object rate_chart_used { get; set; }
        public object pickup_longitude { get; set; }
      
        public object add_charge_amt5 { get; set; }
        public object pu_arrive_notification_sent { get; set; }

        public object order_timeliness_text { get; set; }
        public object order_timeliness { get; set; }
        public object push_services { get; set; }
        public object deliver_eta_date { get; set; }
        public object driver1_text { get; set; }
      
        public object deliver_omw_longitude { get; set; }
        public object deliver_wait_time { get; set; }
        public object pickup_room { get; set; }
        public object deliver_special_instructions4 { get; set; }
        public object add_charge_amt7 { get; set; }
        public object az_equip2 { get; set; }
        public object hours { get; set; }
        public object add_charge_code2 { get; set; }
        public object exception_code { get; set; }
        public object roundtrip_actual_pieces { get; set; }
        public object rate_special_instructions { get; set; }
        public object roundtrip_actual_arrival_time { get; set; }
        public object add_charge_occur1 { get; set; }
        public object origin_code_text { get; set; }
        public object origin_code { get; set; }
        public object delivery_airport_code { get; set; }
        public object distribution_branch_id { get; set; }
        public object hist_inv_date { get; set; }
        public object add_charge_code1 { get; set; }

        public object deliver_route_code { get; set; }
        public object roundtrip_actual_longitude { get; set; }

        public object rate_buck_amt8 { get; set; }
        public object pickup_omw_latitude { get; set; }
        public object deliver_omw_timestamp { get; set; }
        public object rate_buck_amt9 { get; set; }
        public object deliver_room { get; set; }
        public object add_charge_code6 { get; set; }
        public object add_charge_occur3 { get; set; }
        public object blocks { get; set; }
        public object add_charge_code9 { get; set; }
       
        public object add_charge_occur10 { get; set; }
        public object add_charge_code11 { get; set; }
        public object pickup_address_point_number_text { get; set; }
        public object pickup_address_point_number { get; set; }
        public object customer_name { get; set; }
        public object pu_actual_location_accuracy { get; set; }
       
        public object add_charge_amt6 { get; set; }
        public object signature_required { get; set; }
       
        public object add_charge_amt8 { get; set; }
        public object callback_to { get; set; }
        public object fuel_price_source { get; set; }
        public object customer_number_text { get; set; }


        public object callback_required_text { get; set; }
        public object callback_required { get; set; }
        public object return_svc_level { get; set; }
        public object add_charge_amt11 { get; set; }
        public object add_charge_occur2 { get; set; }
    }

    public class Settlement
    {

        public object company_number { get; set; }
        public object control_number { get; set; }
        public object charge1 { get; set; }
        public object charge5 { get; set; }
        public object charge2 { get; set; }
        public object charge3 { get; set; }
        public object charge4 { get; set; }
        public object charge6 { get; set; }
        public object id { get; set; }

        public object company_number_text { get; set; }
        public object settlement_bucket4_pct { get; set; }
        public object date_last_updated { get; set; }
        public object fuel_price_zone { get; set; }
        public object driver_sequence_text { get; set; }
        public object driver_sequence { get; set; }
        public object posting_status_text { get; set; }
        public object posting_status { get; set; }
        public object settlement_period_end_date { get; set; }
        public object time_last_updated { get; set; }
        public object driver_number_text { get; set; }
        public object driver_number { get; set; }
        public object settlement_bucket2_pct { get; set; }
        public object driver_company_number_text { get; set; }
        public object driver_company_number { get; set; }
        public object voucher_date { get; set; }
        public object agent_etrac_transaction_number { get; set; }
        public object settlement_bucket5_pct { get; set; }
        public object record_type { get; set; }
        public object voucher_number { get; set; }
        public object voucher_amount { get; set; }
        public object pay_chart_used { get; set; }
        public object settlement_pct { get; set; }
        public object vendor_invoice_number { get; set; }
        public object settlement_bucket3_pct { get; set; }
        public object fuel_update_freq_text { get; set; }
        public object fuel_update_freq { get; set; }
        public object pre_book_percentage { get; set; }
        public object settlement_bucket6_pct { get; set; }
        public object transaction_type_text { get; set; }
        public object transaction_type { get; set; }
        public object adjustment_type { get; set; }
        public object agents_etrac_partner_id { get; set; }
        public object fuel_plan { get; set; }
        public object fuel_price_source { get; set; }
        public object agent_accepted_or_rejected_text { get; set; }
        public object agent_accepted_or_rejected { get; set; }
        public object file_status_text { get; set; }
        public object file_status { get; set; }
        public object vendor_employee_numer { get; set; }
        public object settlement_bucket1_pct { get; set; }
        public object order_date { get; set; }
    }

    public class Progress
    {
        public object id { get; set; }
        public object status_date { get; set; }
        public object status_text { get; set; }
        public object status_time { get; set; }

    }

    public class ResponseOrderSettlements
    {
        public object company_number { get; set; }
        public object control_number { get; set; }
        public object charge1 { get; set; }
        public object charge5 { get; set; }
        public object charge2 { get; set; }
        public object charge3 { get; set; }
        public object charge4 { get; set; }
        public object charge6 { get; set; }
        public object id { get; set; }
        public object settlement_bucket6_pct { get; set; }
        public object voucher_date { get; set; }

        public object record_type { get; set; }
        public object company_number_text { get; set; }

        public object vendor_employee_numer { get; set; }
        public object driver_sequence_text { get; set; }
        public object driver_sequence { get; set; }
        public object fuel_update_freq_text { get; set; }
        public object fuel_update_freq { get; set; }
        public object driver_number_text { get; set; }
        public object driver_number { get; set; }
        public object date_last_updated { get; set; }
        public object settlement_period_end_date { get; set; }
        public object driver_company_number_text { get; set; }
        public object driver_company_number { get; set; }

        public object posting_status_text { get; set; }
        public object posting_status { get; set; }


        public object fuel_price_zone { get; set; }
        public object agent_etrac_transaction_number { get; set; }
        public object adjustment_type { get; set; }
        public object transaction_type_text { get; set; }
        public object transaction_type { get; set; }
        public object agent_accepted_or_rejected_text { get; set; }
        public object agent_accepted_or_rejected { get; set; }
        public object fuel_price_source { get; set; }


        public object file_status_text { get; set; }
        public object file_status { get; set; }

        public object pre_book_percentage { get; set; }
        public object settlement_bucket5_pct { get; set; }
        public object fuel_plan { get; set; }


        public object order_date { get; set; }
        public object pay_chart_used { get; set; }
        public object settlement_bucket4_pct { get; set; }
        public object voucher_amount { get; set; }
        public object time_last_updated { get; set; }
        public object agents_etrac_partner_id { get; set; }
        public object settlement_bucket1_pct { get; set; }
        public object settlement_bucket3_pct { get; set; }
        public object voucher_number { get; set; }
        public object settlement_pct { get; set; }
        public object settlement_bucket2_pct { get; set; }
        public object vendor_invoice_number { get; set; }

    }

    public class Note
    {
        public object company_number { get; set; }
        public object control_number { get; set; }
        public object id { get; set; }
        public object user_id { get; set; }
        public object note_line { get; set; }
        public object note_code { get; set; }
        public object company_number_text { get; set; }
        public object entry_date { get; set; }
        public object print_on_ticket { get; set; }
        public object show_to_cust { get; set; }
        public object entry_time { get; set; }
    }

    public class ErrorResponse
    {
        public string error { get; set; }
        public string status { get; set; }
        public string code { get; set; }
        public string reference { get; set; }

    }



}
