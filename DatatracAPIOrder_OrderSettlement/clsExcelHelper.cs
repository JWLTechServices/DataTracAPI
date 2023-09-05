using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
namespace DatatracAPIOrder_OrderSettlement
{
    class clsExcelHelper
    {
        public static DataSet ImportExcelXLSX(string Filepath, bool hasHeaders, bool FutureMapping = false)
        {
            clsCommon objCommon = new clsCommon();
            DataSet output = new DataSet();
            //try
            //{

            string HDR = (hasHeaders ? "Yes" : "No");
            // string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Filepath + ";Extended Properties=\"Excel 8.0;HDR=" + HDR + ";IMEX=1\"";

            // string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Filepath + ";Extended Properties=\"Excel 8.0;HDR=" + HDR + ";IMEX=1\"";

            //public static string path = @"C:\src\RedirectApplication\RedirectApplication\301s.xlsx";
            //  string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Filepath + ";Extended Properties=Excel 12.0;HDR=" + HDR + ";IMEX=1";
            string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Filepath + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=1\"";

            //string sql = "SELECT * FROM [Template$]";
            //string excelConnection = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Filepath + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1;\"";

            //using (OleDbDataAdapter adaptor = new OleDbDataAdapter(sql, excelConnection))
            //{
            //    DataSet ds = new DataSet();
            //    adaptor.Fill(ds);
            //}

            using (OleDbConnection conn = new OleDbConnection(strConn))
            {
                conn.Open();

                DataTable dt = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] {
                null,
                null,
                null,
                "TABLE"
            });
                int i = 0;
                foreach (DataRow row in dt.Rows)
                {
                    string sheet = string.Empty;
                    if (FutureMapping == true)
                    {
                        sheet = row["TABLE_NAME"].ToString();
                    }
                    else
                    {
                        if (i == 0)
                        {
                            sheet = "Template$"; //row["TABLE_NAME"].ToString();
                        }
                        //else if (i == 1)
                        //{
                        //    sheet = "Dispatch Track$"; //row["TABLE_NAME"].ToString();

                        //}
                    }
                    OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheet + "]", conn);
                    cmd.CommandType = CommandType.Text;


                    DataTable outputTable = new DataTable(sheet);

                    output.Tables.Add(outputTable);

                    OleDbDataAdapter d = new OleDbDataAdapter(cmd);
                    try
                    {



                        d.Fill(outputTable);
                        if (i == 0)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        // objCommon.LogEvents(ex.Message, "ImportExcelXLSX", System.Diagnostics.EventLogEntryType.Error, 1);
                        objCommon.WriteErrorLog(ex);
                    }
                    i++;
                }
            }
            //}
            //catch (Exception ex)
            //{
            //    objCommon.LogEvents(ex.Message, "ImportExcelXLS", System.Diagnostics.EventLogEntryType.Error, 1);
            //}


            if (HDR == "No")
            {
                foreach (DataColumn column in output.Tables[0].Columns)
                {
                    string cName = output.Tables[0].Rows[0][column.ColumnName].ToString();
                    if (!output.Tables[0].Columns.Contains(cName) && cName != "")
                    {
                        column.ColumnName = cName;
                    }
                }
                output.Tables[0].Rows[0].Delete();
                output.Tables[0].AcceptChanges();
            }
            return output;
        }
        public static void ExportDataToXLSX(DataTable dt, string fileName, int val, string datetime)
        {
            clsCommon objCommon = new clsCommon();
            try
            {
                int fileExtPos = fileName.LastIndexOf(".");
                if (fileExtPos >= 0)
                    fileName = fileName.Substring(0, fileExtPos);
                fileName = fileName + "_" + val + "_" + datetime + ".xlsx";

                string attachment = "attachment; filename=" + fileName;
                HttpContext.Current.Response.ClearContent();
                // Response.ClearContent();
                HttpContext.Current.Response.AddHeader("content-disposition", attachment);
                HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
                string tab = "";
                foreach (DataColumn dc in dt.Columns)
                {
                    HttpContext.Current.Response.Write(tab + dc.ColumnName);
                    tab = "\t";
                }
                HttpContext.Current.Response.Write("\n");
                int i;
                foreach (DataRow dr in dt.Rows)
                {
                    tab = "";
                    for (i = 0; i < dt.Columns.Count; i++)
                    {
                        HttpContext.Current.Response.Write(tab + dr[i].ToString());
                        tab = "\t";
                    }
                    HttpContext.Current.Response.Write("\n");
                }
                HttpContext.Current.Response.End();

            }
            catch (Exception ex)
            {
                objCommon.WriteErrorLog(ex);
            }

        }

        public static DataSet ImportExcelXLSXToDataSet_FSCRATES_All(string Filepath, bool hasHeaders)
        {
            clsCommon objCommon = new clsCommon();
            string HDR = (hasHeaders ? "Yes" : "No");

            string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Filepath + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=1\"";

            DataSet output = new DataSet();

            using (OleDbConnection conn = new OleDbConnection(strConn))
            {
                conn.Open();

                System.Data.DataTable dt = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] {
                null,
                null,
                null,
                "TABLE"
            });
                // string sheet_name = "";
                // string sheet = "Sheet1$";

                foreach (DataRow row in dt.Rows)
                {
                    string sheet = row["TABLE_NAME"].ToString();

                    // string sqlSelect = "SELECT * FROM [" + sheet + "] ;";
                    // string sqlSelect = "SELECT * FROM [" + sheet + "]  where [Company]= @company AND [CustomerNumber]=@customernumber AND [IsActive] =@IsActive ";
                    string sqlSelect = "SELECT * FROM [" + sheet + "]  where [IsActive] =@IsActive ";

                    OleDbCommand comm = new OleDbCommand();
                    comm.Connection = conn;
                    // comm.Parameters.AddWithValue("@Company", company);
                    // comm.Parameters.AddWithValue("@CustomerNumber", customernumber);
                    comm.Parameters.AddWithValue("@IsActive", "Y");
                    comm.CommandText = sqlSelect;

                    //  OleDbCommand cmd = new OleDbCommand(sqlSelect, conn);
                    System.Data.DataTable outputTable = new System.Data.DataTable(sheet);
                    output.Tables.Add(outputTable);

                    OleDbDataAdapter d = new OleDbDataAdapter(comm);
                    try
                    {
                        d.Fill(outputTable);
                    }
                    catch (Exception ex)
                    {
                        objCommon.WriteErrorLog(ex, "ImportExcelXLSXToDataSet_FSCRATES");
                    }
                }
            }
            if (HDR == "No")
            {
                if (output != null)
                {
                    if (output.Tables.Count > 0)
                    {
                        if (output.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataColumn column in output.Tables[0].Columns)
                            {
                                string cName = output.Tables[0].Rows[0][column.ColumnName].ToString();
                                if (!output.Tables[0].Columns.Contains(cName) && cName != "")
                                {
                                    column.ColumnName = cName;
                                }
                            }

                            output.Tables[0].Rows[0].Delete();
                            output.Tables[0].AcceptChanges();
                        }
                    }
                    if (output.Tables.Count > 1)
                    {
                        if (output.Tables[1].Rows.Count > 0)
                        {
                            foreach (DataColumn column in output.Tables[1].Columns)
                            {
                                string cName = output.Tables[1].Rows[0][column.ColumnName].ToString();
                                if (!output.Tables[1].Columns.Contains(cName) && cName != "")
                                {
                                    column.ColumnName = cName;
                                }
                            }
                            output.Tables[1].Rows[0].Delete();
                            output.Tables[1].AcceptChanges();
                        }
                    }
                }
            }
            return output;
        }
    }
}
