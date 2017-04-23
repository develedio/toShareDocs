using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Data.SqlClient;
using System.Configuration;
namespace shareDocs.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            DataSet ds = new DataSet();
            if (Request.Files["file"].ContentLength > 0)
            {
                string fileExtension =
                                     System.IO.Path.GetExtension(Request.Files["file"].FileName);

                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    string fileLocation = Server.MapPath("~/Content/") + Request.Files["file"].FileName;
                    if (System.IO.File.Exists(fileLocation))
                    {

                        System.IO.File.Delete(fileLocation);
                    }
                    Request.Files["file"].SaveAs(fileLocation);
                    string excelConnectionString = string.Empty;
                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    //connection String for xls file format.
                    if (fileExtension == ".xls")
                    {
                        excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    }
                    //connection String for xlsx file format.
                    else if (fileExtension == ".xlsx")
                    {

                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    }
                    //Create Connection to Excel work book and add oledb namespace
                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();
                    DataTable dt = new DataTable();

                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dt == null)
                    {
                        return null;
                    }

                    String[] excelSheets = new String[dt.Rows.Count];
                    int t = 0;
                    //excel data saves in temp file here.
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[t] = row["TABLE_NAME"].ToString();
                        t++;
                    }
                    OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);


                    string query = string.Format("Select * from [{0}]", excelSheets[0]);
                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                    {
                        dataAdapter.Fill(ds);
                    }
                    excelConnection.Close(); //ECP
                }
                if (fileExtension.ToString().ToLower().Equals(".xml"))
                {
                    string fileLocation = Server.MapPath("~/Content/") + Request.Files["FileUpload"].FileName;
                    if (System.IO.File.Exists(fileLocation))
                    {
                        System.IO.File.Delete(fileLocation);
                    }

                    Request.Files["FileUpload"].SaveAs(fileLocation);
                    XmlTextReader xmlreader = new XmlTextReader(fileLocation);
                    // DataSet ds = new DataSet();
                    ds.ReadXml(xmlreader);
                    xmlreader.Close();
                }

                //create table 
                string conn = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
                SqlConnection con = new SqlConnection(conn);
                //ECP la primera columna en BD será in entero auto-incrementado
                //ECP la segunda columna debe contener el usuario con el cual se realizará el login, el cual puede ser email, RUT, etc.
                string userTableName = "Person_0001";
                string sql = "CREATE TABLE " + userTableName + " (" + " Id int NOT NULL IDENTITY (1,1) PRIMARY KEY";
                for (int i = 0; i < ds.Tables[0].Columns.Count; i++) {
                    if (!String.IsNullOrEmpty(ds.Tables[0].Columns[i].ColumnName))
                    {
                        sql += "," + ds.Tables[0].Columns[i].ColumnName + " varchar(100) NULL";
                    }
                }                                   
                sql += ")";

                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                
                //grabar los datos

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    //string conn = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
                    con = new SqlConnection(conn);
                    //string query = "Insert into Person(Name,Email,Mobile) Values('" + ds.Tables[0].Rows[i][0].ToString() + "','" + ds.Tables[0].Rows[i][1].ToString() + "','" + ds.Tables[0].Rows[i][2].ToString() + "')";
                    string query = "Insert into " + userTableName + "( ";
                    string colNames = ds.Tables[0].Columns[0].ColumnName;
                    string colValues = "Values('" + ds.Tables[0].Rows[i][0].ToString();
                    for (int j = 1; j < ds.Tables[0].Columns.Count; j++)
                    {
                        colNames  += ","   + ds.Tables[0].Columns[j].ColumnName;
                        colValues += "','" + ds.Tables[0].Rows[i][1].ToString();
                    }

                    colNames += ") ";
                    colValues += "')";

                    query += colNames;
                    query += colValues;

                    con.Open();
                    cmd = new SqlCommand(query, con);
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

            }
            return View();
        }



    }
}
