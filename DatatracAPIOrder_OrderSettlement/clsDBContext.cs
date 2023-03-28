using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatatracAPIOrder_OrderSettlement
{
    class clsDBContext :clsCommon
    {

        public DSResponse GetDeficitWeightRatingDetails(int company, int customerNumber)
        {
            DSResponse objResponse = new DSResponse();
            try
            {
                DataSet dsDtls = new DataSet();

                SqlParameter paramCompany = new SqlParameter("@Company", SqlDbType.Int);
                paramCompany.Value = company;

                SqlParameter paramCustomerNumber = new SqlParameter("@CustomerNumber", SqlDbType.Int);
                paramCustomerNumber.Value = customerNumber;

                dsDtls = SqlHelper.ExecuteDataset(GetConfigValue("DBConnection"), CommandType.StoredProcedure, "USP_S_StoreBand_DeficitWeightRating_Mapping",
                    paramCompany, paramCustomerNumber);
                if (dsDtls.Tables[0].Rows.Count > 0)
                {
                    objResponse.DS = dsDtls;
                    objResponse.dsResp.ResponseVal = true;
                }
                else
                {
                    objResponse.dsResp.ResponseVal = false;
                    objResponse.dsResp.Reason = "Deficit Weight Rating details not found";
                }
            }
            catch (Exception ex)
            {
                objResponse.dsResp.ResponseVal = false;
                objResponse.dsResp.Reason = "Exception -" + ex.Message;
                WriteErrorLog(ex, "GetDeficitWeightRatingDetails");
            }
            return objResponse;
        }

        public DSResponse GetBillingRatesAndPayableRates_CustomerMappingDetails(int company, int customerNumber)
        {
            DSResponse objResponse = new DSResponse();
            try
            {
                DataSet dsDtls = new DataSet();

                SqlParameter paramCompany = new SqlParameter("@Company", SqlDbType.Int);
                paramCompany.Value = company;

                SqlParameter paramCustomerNumber = new SqlParameter("@CustomerNumber", SqlDbType.Int);
                paramCustomerNumber.Value = customerNumber;

                dsDtls = SqlHelper.ExecuteDataset(GetConfigValue("DBConnection"), CommandType.StoredProcedure, "USP_S_BillingRates_PayableRates_CustomerMapping",
                    paramCompany, paramCustomerNumber);
                if (dsDtls.Tables[0].Rows.Count > 0)
                {
                    objResponse.DS = dsDtls;
                    objResponse.dsResp.ResponseVal = true;
                }
                else
                {
                    objResponse.dsResp.ResponseVal = false;
                    objResponse.dsResp.Reason = "Billing Rates And Payable Rates customer mapping details not found";
                }
            }
            catch (Exception ex)
            {
                objResponse.dsResp.ResponseVal = false;
                objResponse.dsResp.Reason = "Exception -" + ex.Message;
                WriteErrorLog(ex, "GetBillingRatesAndPayableRates_CustomerMappingDetails");
            }
            return objResponse;
        }

        public DSResponse GetFSCRates_MappingDetails(int company, int customerNumber)
        {
            DSResponse objResponse = new DSResponse();
            try
            {
                DataSet dsDtls = new DataSet();

                SqlParameter paramCompany = new SqlParameter("@Company", SqlDbType.Int);
                paramCompany.Value = company;

                SqlParameter paramCustomerNumber = new SqlParameter("@CustomerNumber", SqlDbType.Int);
                paramCustomerNumber.Value = customerNumber;

                dsDtls = SqlHelper.ExecuteDataset(GetConfigValue("DBConnection"), CommandType.StoredProcedure, "USP_S_FSCRate_Mapping",
                    paramCompany, paramCustomerNumber);
                if (dsDtls.Tables[0].Rows.Count > 0)
                {
                    objResponse.DS = dsDtls;
                    objResponse.dsResp.ResponseVal = true;
                }
                else
                {
                    objResponse.dsResp.ResponseVal = false;
                    objResponse.dsResp.Reason = "FSC Rates customer mapping details not found";
                }
            }
            catch (Exception ex)
            {
                objResponse.dsResp.ResponseVal = false;
                objResponse.dsResp.Reason = "Exception -" + ex.Message;
                WriteErrorLog(ex, "GetFSCRates_MappingDetails");
            }
            return objResponse;
        }
    }
}
