using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using DataAccessHelpers;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using RiskManagement.Helper;

namespace RiskManagement.SanjivaniBriefing
{
    public partial class IndividualCallingCSP : WebHelperCSP
    {
        protected void grid_RowDataBound(object sender, GridViewRowEventArgs e)
        {

            e.Row.Attributes.Add("onmouseover", "this.originalstyle=this.style.backgroundColor;this.style.backgroundColor='#AAFF00'");


            // when mouse leaves the row, change the bg color to its original value   
            e.Row.Attributes.Add("onmouseout", "this.style.backgroundColor=this.originalstyle;");
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!cbln(Session["loggedin"]))
            {
                Response.Redirect("Default.aspx");
            }
            if (!IsPostBack)
            {
                string id = Request.QueryString["id"];
                Session["callstartedat"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Session["cspid"] = id;
                loadmasterdata(id);
                loaddailygrid(id);
                loadcspimg();
                CheckAppStatus(id);
                //Label1.Text = "Welcome " + cstr(Session["callername"]).ToUpper();
            }
        }
        private void loadmasterdata(string id)
        {
            //query = "select * from bob_data.cspmaster where id=" + id;
            //dt = GetDataTable();
            //new code

            query = "SELECT * FROM bob_data.cspmaster WHERE id = @id";
            _parameters.Clear();
            _parameters.Add("@id", id);
            dt = GetDataTable(_parameters);

            //new code

            DataRow dr = dt.Rows[0];
            lblName.Text = cstr(dr["cspname"]);
            lblCode.Text = cstr(dr["cspcode"]);
            hypMobileNo.Text = cstr(dr["mobileno"]);
            hypcall.Text = cstr(dr["mobileno"]);
            hypcall.NavigateUrl = "tel:0" + cstr(dr["mobileno"]);

            lblAlternate.Text = cstr(dr["alternatenumbers"]);
            lblAlternate1.Text = cstr(dr["alternatenumber2"]);
            lblSupervisorName.Text = cstr(dr["svname"]);
            lblDCName.Text = cstr(dr["dcname"]);
            lblBranch.Text = cstr(dr["linkbranch"]);
            lblSupervisorMobile.Text = getMobileNo(lblSupervisorName.Text);
            lblDCMobileNo.Text = getMobileNo(lblDCName.Text);

            lblDistrict.Text = cstr(dr["cspdistrict"]);
            lblState.Text = cstr(dr["cspstate"]);

            lblAvgBalance.Text = cstr(dr["avgbalance"]);
            lblTotalAccount.Text = cstr(dr["totalaccount"]);
            lblZeroBalance.Text = cstr(dr["zerobalance"]);

            chkDRA.Checked = cbln(dr["dra"]);
            chlLam.Checked = cbln(dr["lam"]);
            chkdrasq.Checked = cbln(dr["drasq"]);
            chkIIBF.Checked = cbln(dr["iibf"]);
            chkiibfsq.Checked = cbln(dr["iibfsq"]);
            chkMicroATM.Checked = cbln(dr["microatm"]);
            chkPassbookPrinter.Checked = cbln(dr["passbookprinter"]);

            ddlMultipleTerminal.SelectedValue = cstr(cint(dr["multipleterminal"]));

            //query = "select * from officestaff where belongsto='D' and description='" + lblDistrict.Text + "'";
            //DataTable dt2 = GetDataTable();

            //new code

            query = "SELECT * FROM officestaff WHERE belongsto = @belongsto AND description = @description";
            _parameters.Clear();
            _parameters.Add("@belongsto", "D");
            _parameters.Add("@description", lblDistrict.Text);  // Ensure lblDistrict.Text is sanitized
            DataTable dt2 = GetDataTable(_parameters);

            //new code


            if (dt2.Rows.Count > 0)
            {
                int idesc = 1;
                foreach (DataRow item in dt2.Rows)
                {
                    addofficestaff(idesc++, item);
                }
            }
            else
            {
                //query = "select * from officestaff where belongsto='S' and description='" + lblState.Text + "'";
                //DataTable t2 = GetDataTable();

                //new code

                query = "SELECT * FROM officestaff WHERE belongsto = @belongsto AND description = @description";
                _parameters.Clear();
                _parameters.Add("@belongsto", "S");
                _parameters.Add("@description", lblState.Text.Trim()); // Ensure data is clean
                DataTable t2 = GetDataTable(_parameters);

                //new code

                if (t2.Rows.Count > 0)
                {
                    int idesc = 1;
                    foreach (DataRow item in t2.Rows)
                    {
                        addofficestaff(idesc++, item);
                    }
                }
            }

            //query = "select '' as `Date`,'' as Caller, callconnected as `Connected`, callremarks as Remarks, callconnectedreason as Reason, callingdate, calledby from cspcallertransaction where cspcode='" + lblCode.Text + "' order by id desc limit 5";
            //DataTable dataTable1 = GetDataTable();
            
            //new code

            query = "SELECT '' AS `Date`, '' AS Caller, callconnected AS `Connected`, " +
        "callremarks AS Remarks, callconnectedreason AS Reason, callingdate, calledby " +
        "FROM cspcallertransaction WHERE cspcode = @cspcode ORDER BY id DESC LIMIT 5";

            _parameters.Clear();
            _parameters.Add("@cspcode", lblCode.Text.Trim()); // Ensure sanitized input
            DataTable dataTable1 = GetDataTable(_parameters);

            //new code

            foreach (DataRow item in dataTable1.Rows)
            {
                item[0] = cdatehalf(item["callingdate"]);
                //query = "select callername from callermaster where id=" + cstr(item["calledby"]);
                //item[1] = cstr(getvalue());

                //new code

                query = "SELECT callername FROM callermaster WHERE id = @id";
                _parameters.Clear();
                _parameters.Add("@id", cstr(item["calledby"])); // Ensure integer conversion
                item[1] = cstr(getValue(_parameters));

                //new code
            }
            DataRow dr1 = dataTable1.NewRow();
            dr1[0] = "Date";
            dr1[1] = "Caller";
            dr1[2] = "Conn";
            dr1[3] = "Remarks";
            dr1[4] = "Reason";
            dataTable1.Rows.InsertAt(dr1, 0);
            grdcallingHistory.DataSource = dataTable1;
            grdcallingHistory.DataBind();


        }

        //private void CheckAppStatus(string cspId)
        //{
        //    appStatus.Checked = false;
        //    lblLoginDateTime.Text = "";

        //    // Get CSP code
        //    query = "SELECT cspcode FROM cspdata.cspmaster WHERE id='" + cspId + "'";
        //    string cspcode = getvalue();

        //    // Check app details
        //    query = "SELECT deviceid, lastloginon FROM cspdata.fedcspdetails WHERE cspid='" + cspcode + "' LIMIT 1";
        //    DataTable dtDevice = GetDataTable();

        //    if (dtDevice == null || dtDevice.Rows.Count == 0)
        //    {
        //        appStatus.Checked = false;
        //        lblLoginDateTime.Text = "App not installed.";
        //        return;
        //    }

        //    DataRow dr = dtDevice.Rows[0];
        //    string deviceId = cstr(dr["deviceid"]);
        //    string lastLogin = cstr(dr["lastloginon"]);

        //    if (string.IsNullOrWhiteSpace(deviceId))
        //    {
        //        appStatus.Checked = false;
        //        lblLoginDateTime.Text = "App not installed.";
        //        return;
        //    }

        //    // Device found
        //    appStatus.Checked = true;
        //    if (!string.IsNullOrWhiteSpace(lastLogin))
        //        lblLoginDateTime.Text = "Last: " + lastLogin;
        //    else
        //        lblLoginDateTime.Text = "Registered, no login.";

        //    appStatus.Enabled = false; // optional: make readonly
        //}

        private void CheckAppStatus(string cspId)
        {
            appStatus.Checked = false;
            lblLoginDateTime.Text = "";

            // Get CSP code
            query = "SELECT cspcode FROM bob_data.cspmaster WHERE id='" + cspId + "'";
            string cspcode = getvalue();

            // Check app details
            query = "SELECT deviceid, lastloginon FROM cspdata.fedcspdetails WHERE cspid='" + cspcode + "' LIMIT 1";
            DataTable dtDevice = GetDataTable();

            if (dtDevice == null || dtDevice.Rows.Count == 0)
            {
                appStatus.Checked = false;
                lblLoginDateTime.Text = "App not installed.";
                return;
            }

            DataRow dr = dtDevice.Rows[0];
            string deviceId = cstr(dr["deviceid"]);
            string lastLogin = cstr(dr["lastloginon"]);

            if (string.IsNullOrWhiteSpace(deviceId))
            {
                appStatus.Checked = false;
                lblLoginDateTime.Text = "App not installed.";
                return;
            }

            // Device found
            appStatus.Checked = true;

            if (!string.IsNullOrWhiteSpace(lastLogin))
            {
                // Convert to date only
                DateTime dt;

                if (DateTime.TryParse(lastLogin, out dt))
                    lblLoginDateTime.Text = dt.ToString("dd/MM/yyyy"); // Only date
                else
                    lblLoginDateTime.Text = lastLogin; // fallback
            }
            else
            {
                lblLoginDateTime.Text = "Registered, no login.";
            }

            appStatus.Enabled = false;
        }


        private void addofficestaff(int idesc, DataRow item)
        {
            switch (idesc)
            {
                case 1:
                    lblstaff1.Text = cstr(item["officestaffname"]);
                    lblMobileStaff1.Text = cstr(item["mobileno"]);
                    tr1.Visible = true;
                    break;

                case 2:
                    lblstaff2.Text = cstr(item["officestaffname"]);
                    lblMobileStaff2.Text = cstr(item["mobileno"]);
                    tr2.Visible = true;
                    break;
                case 3:
                    lblstaff3.Text = cstr(item["officestaffname"]);
                    lblMobileStaff3.Text = cstr(item["mobileno"]);
                    tr3.Visible = true;
                    break;
                case 4:
                    lblstaff4.Text = cstr(item["officestaffname"]);
                    lblMobileStaff4.Text = cstr(item["mobileno"]);
                    tr4.Visible = true;
                    break;
                case 5:
                    lblstaff5.Text = cstr(item["officestaffname"]);
                    lblMobileStaff5.Text = cstr(item["mobileno"]);
                    tr5.Visible = true;
                    break;
                default:
                    break;
            }
        }
        private string getMobileNo(string text)
        {
            //query = "select mobileno from supervisormaster where supervisorname='" + text + "'";
            //try
            //{
            //    return cstr(getvalue());
            //}

            //new code
            query = "SELECT mobileno FROM supervisormaster WHERE supervisorname = @supervisorname";
            _parameters.Clear();
            _parameters.Add("@supervisorname", text.Trim()); // Trim to remove leading/trailing spaces

            try
            {
                return cstr(getValue(_parameters));
            }
            //new code
            catch
            { }
            return "NA";
        }
        private void loaddailygrid(string id)
        {
            //query = "select schemedescription as Schemes from cspmasterschemes where showin in ('D','A')";
            //DataTable dtdaily = GetDataTable();

            //new code
            query = "SELECT schemedescription AS Schemes FROM cspmasterschemes WHERE showin IN (@value1, @value2)";
            _parameters.Clear();
            _parameters.Add("@value1", "D");
            _parameters.Add("@value2", "A");
            DataTable dtdaily = GetDataTable(_parameters);
            //new code

            CreateDailyTable(dtdaily);

            loadDataDaily(dtdaily);

            grdDaily.DataSource = dtdaily;
            grdDaily.DataBind();

            //query = "select schemedescription as Schemes from cspmasterschemes where showin in ('M','A','Y')";
            //DataTable dtMon = GetDataTable();

            //new code
            query = "SELECT schemedescription AS Schemes FROM cspmasterschemes WHERE showin IN (@value1, @value2, @value3)";
            _parameters.Clear();
            _parameters.Add("@value1", "M");
            _parameters.Add("@value2", "A");
            _parameters.Add("@value3", "Y");
            DataTable dtMon = GetDataTable(_parameters);
            //new code

            CreateMonthlyTable(dtMon);
            loadDataMonthly(dtMon);

            // ✅ ADD COMMISSION ROW HERE
            AddCommissionRow(dtMon);

            grdMontly.DataSource = dtMon;
            grdMontly.DataBind();
        }
        string datetofetch, colname;
        private void loadDataDaily(DataTable dtdaily)
        {
            //query = "select * from cspmasterschemes where showin='A'";
            //DataTable dt = GetDataTable();

            //new code
            query = "SELECT * FROM cspmasterschemes WHERE showin = @showin";
            _parameters.Clear();
            _parameters.Add("@showin", "A");
            DataTable dt = GetDataTable(_parameters);
            //new code

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                FillDataDaily(cstr(dt.Rows[i]["schemefieldname"]), i, dtdaily);
            }

        }
        private void loadDataMonthly(DataTable dtdaily)
        {
            //query = "select * from cspmasterschemes where showin in ('A','Y','M')";
            //DataTable dt = GetDataTable();

            //new code
            query = "SELECT * FROM cspmasterschemes WHERE showin IN (@value1, @value2, @value3)";
            _parameters.Clear();
            _parameters.Add("@value1", "A");
            _parameters.Add("@value2", "Y");
            _parameters.Add("@value3", "M");
            DataTable dt = GetDataTable(_parameters);

            //new code

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (cstr(dt.Rows[i]["showin"]) == "Y")
                {
                    FillDataYearly(cstr(dt.Rows[i]["schemefieldname"]), i, dtdaily);
                }
                else
                    FillDataMonthly(cstr(dt.Rows[i]["schemefieldname"]), i, dtdaily);
            }

        }
        private void AddCommissionRow(DataTable dt)
        {
            string connectionString =
                "Server=192.168.2.191;Port=6625;User Id=pnb;Password=Reset@123;";

            string cspcode = string.Empty;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // 1. Get CSP Code
                string cspQuery = "SELECT cspcode FROM otherdatacapture.federated_bob WHERE id = @cspid";
                using (MySqlCommand cmd = new MySqlCommand(cspQuery, con))
                {
                    cmd.Parameters.AddWithValue("@cspid", Session["cspid"]);
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                        cspcode = result.ToString();
                }

                DataRow dr = dt.NewRow();
                dr["Schemes"] = "Commission";

                decimal total = 0;

                // 2. Loop through last 12 months
                for (int i = 12; i > 0; i--)
                {
                    DateTime month = DateTime.Now.AddMonths(-i);
                    string colname = month.ToString("MMM-yy");

                    if (!dt.Columns.Contains(colname))
                    {
                        dt.Columns.Add(colname, typeof(decimal));
                    }

                    DateTime fromDate = new DateTime(month.Year, month.Month, 1);
                    DateTime toDate = fromDate.AddMonths(1);

                    string commissionQuery =
                        @"SELECT 
    IFNULL(
        SUM(
            IFNULL(ekyc_accts_nfunded_commission, 0) +
            IFNULL(ekyc_accts_funded_comm, 0) +
            IFNULL(comm_non_bsbd_acc, 0) +
            IFNULL(comm_fd, 0) +
            IFNULL(comm_rd, 0) +
            IFNULL(comm_pmsby, 0) +
            IFNULL(comm_pmjjby, 0) +
            IFNULL(comm_aadhar, 0) +
            IFNULL(comm_mobile, 0) +
            IFNULL(comm_withdrawal, 0) +
            IFNULL(comm_deposit, 0) +
            IFNULL(comm_fundtransfer, 0) +
            IFNULL(comm_imps, 0) +
            IFNULL(comm_chqbook, 0) +
            IFNULL(comm_rup_deb, 0) +
            IFNULL(comm_bbps, 0) +
            IFNULL(comm_psp, 0) +
            IFNULL(comm_sms, 0) +
            IFNULL(comm_deb_blocked, 0) +
            IFNULL(comm_pmjdyod, 0) +
            IFNULL(comm_neft, 0) +
            IFNULL(inop_acc_comm, 0) +
            IFNULL(deposit, 0) +
            IFNULL(loan_lead, 0) +
            IFNULL(npa_recovery, 0) +
            IFNULL(comm_apy, 0) +
            IFNULL(comm_rekyc, 0) +
            IFNULL(comm_tds_cert, 0) +
            IFNULL(comm_interest_cert, 0) +
            IFNULL(fixed_diff, 0) +
            IFNULL(rekyc_lead_comm, 0)
        ),
    0) AS TOTAL_COMMISSION
FROM new_schema1.bob_bills
WHERE vilid = @vilid
  AND transactionperiod >= @fromDate
  AND transactionperiod < @toDate";

                    using (MySqlCommand cmd = new MySqlCommand(commissionQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@vilid", cspcode);
                        cmd.Parameters.AddWithValue("@fromDate", fromDate);
                        cmd.Parameters.AddWithValue("@toDate", toDate);

                        decimal commission = Convert.ToDecimal(cmd.ExecuteScalar());

                        dr[colname] = commission;
                        total += commission;
                    }
                }

                dr["Total"] = total;
                dt.Rows.Add(dr);
            }
        }
        private void FillDataYearly(string tcolname, int rownoofscheme, DataTable dtdaily)
        {
            //query = "select cspcode from cspmaster where id=" + cstr(Session["cspid"]);
            //string cspcode = getvalue();

            //new code
            query = "SELECT cspcode FROM cspmaster WHERE id = @id";
            _parameters.Clear();
            _parameters.Add("@id", cstr(Session["cspid"])); // Ensure ID is an integer
            string cspcode = getValue(_parameters);

            //new code


            int total = 0;
            for (int i = 12; i > 0; i--)
            {
                datetofetch = DateTime.Today.AddMonths(-i).ToString("yyyy-MM-01");
                colname = DateTime.Now.AddMonths(-i).ToString("MMM-yy");

                //query = "select Sum(" + tcolname + ") from bob_data.csptransactionmonthly where transactionperiod = '" + datetofetch + "'  and cspcode='" + cspcode + "'";

                //new code
                query = $"SELECT SUM({tcolname}) FROM bob_data.csptransactionmonthly WHERE transactionperiod = @datetofetch AND cspcode = @cspcode";
                _parameters.Clear();
                _parameters.Add("@datetofetch", datetofetch);
                _parameters.Add("@cspcode", cspcode);

                //new code
                if (dtdaily.Rows.Count > 0)
                {
                    dtdaily.Rows[rownoofscheme][colname] = getValue(_parameters);
                    total += cint(dtdaily.Rows[rownoofscheme][colname]);
                }
            }
            dtdaily.Rows[rownoofscheme]["Total"] = total;
        }
        private void FillDataMonthly(string tcolname, int rownoofscheme, DataTable dtdaily)
        {
            query = "select cspcode from cspmaster where id=" + cstr(Session["cspid"]);
            string cspcode = getvalue();
            int total = 0;
            for (int i = 12; i > 0; i--)
            {
                datetofetch = DateTime.Today.AddMonths(-i).ToString("yyyy-MM-01");
                colname = DateTime.Now.AddMonths(-i).ToString("MMM-yy");
                //query = "select Sum(" + tcolname + ") from bob_data.csptransactiondaily where transactionperiod between '" + datetofetch + "' and '" + getlastDayofMonth(datetofetch) + "' and cspcode='" + cspcode + "'";

                //new code
                // Construct query securely using parameters
                query = $"SELECT SUM({tcolname}) FROM bob_data.csptransactiondaily WHERE transactionperiod BETWEEN @datetofetch AND @lastDayOfMonth AND cspcode = @cspcode";
                _parameters.Clear();
                _parameters.Add("@datetofetch", Convert.ToDateTime(datetofetch).ToString("yyyy-MM-dd"));
                _parameters.Add("@lastDayOfMonth", Convert.ToDateTime(getlastDayofMonth(datetofetch)).ToString("yyyy-MM-dd"));
                _parameters.Add("@cspcode", cspcode.Trim());
                //new code
                if (dtdaily.Rows.Count > 0)
                {
                    dtdaily.Rows[rownoofscheme][colname] = getValue(_parameters);
                    total += cint(dtdaily.Rows[rownoofscheme][colname]);
                }
            }
            dtdaily.Rows[rownoofscheme]["Total"] = total;
        }
        private string getlastDayofMonth(string datetofetch)
        {
            DateTime dtdate = Convert.ToDateTime(datetofetch);
            int daysinmonth = DateTime.DaysInMonth(dtdate.Year, dtdate.Month);
            return dtdate.ToString("yyyy-MM-" + daysinmonth.ToString());
        }
        private void getFirstLogin(string tcolname, int rownoofscheme, DataTable dtdaily)
        {
            query = "select cspcode from cspmaster where id=" + cstr(Session["cspid"]);
            string cspcode = getvalue();
            int firstlogin = 0;
            int firstlogint = 0;
            for (int i = 30; i > 0; i--)
            {
                datetofetch = DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd");
                colname = DateTime.Now.AddDays(-i).ToString("dd");
                //datetofetch = "2022-05-10";
                //colname = Convert.ToDateTime("2022-05-10").AddDays(-i).ToString("dd");
                query = "select date_format(min(transactiondatetime),'%H%i') as mindate  from bob_data.csptransaction_aeps where transactionperiod = '" + datetofetch + "' and cspid like '" + cspcode + "%'";
                firstlogin = cint(getvalue());
                query = "select date_format(min(transactiondatetime),'%H%i') as mindate  from bob_data.csptransaction_deposit where transactionperiod = '" + datetofetch + "' and cspid like '" + cspcode + "%'";
                firstlogint = cint(getvalue());
                if (firstlogin > firstlogint && firstlogint > 0 || firstlogin == 0)
                {
                    firstlogin = firstlogint;
                }

                query = "select date_format(min(transactiondatetime),'%H%i') as mindate  from bob_data.csptransaction_money where transactionperiod = '" + datetofetch + "' and cspid like '" + cspcode + "%'";
                firstlogint = cint(getvalue());
                if (firstlogin > firstlogint && firstlogint > 0 || firstlogin == 0)
                {
                    firstlogin = firstlogint;
                }

                query = "select date_format(min(transactiondatetime),'%H%i') as mindate  from bob_data.csptransaction_other where transactionperiod = '" + datetofetch + "' and cspid like '" + cspcode + "%'";
                firstlogint = cint(getvalue());
                if (firstlogin > firstlogint && firstlogint > 0 || firstlogin == 0)
                {
                    firstlogin = firstlogint;
                }

                query = "select date_format(min(transactiondatetime),'%H%i') as mindate  from bob_data.csptransaction_withdrawal where transactionperiod = '" + datetofetch + "' and cspid like '" + cspcode + "%'";
                firstlogint = cint(getvalue());
                if (firstlogin > firstlogint && firstlogint > 0 || firstlogin == 0)
                {
                    firstlogin = firstlogint;
                }
                if (firstlogin == 0)
                {
                    dtdaily.Rows[rownoofscheme][colname] = "";
                    continue;
                }
                string stringlogin = cstr(firstlogin).PadLeft(4, '0');

                dtdaily.Rows[rownoofscheme][colname] = stringlogin.Substring(0, 2) + ":" + stringlogin.Substring(2);
            }
            //dtdaily.Rows[rownoofscheme]["Total"] = total;
        }
        private void FillDataDaily(string tcolname, int rownoofscheme, DataTable dtdaily)
        {
            //query = "select cspcode from bob_data.cspmaster where id=" + cstr(Session["cspid"]);
            //string cspcode = getvalue();

            //new code
            query = "SELECT cspcode FROM bob_data.cspmaster WHERE id = @id";
            _parameters.Clear();
            _parameters.Add("@id", cstr(Session["cspid"]));
            string cspcode = getValue(_parameters);
            //new code

            int total = 0;
            for (int i = 28; i > 0; i--)
            {
                datetofetch = DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd");
                colname = DateTime.Now.AddDays(-i).ToString("dd");
                //query = "select sum(" + tcolname + ") as " + tcolname + " from bob_data.csptransactiondaily where transactionperiod = '" + datetofetch + "' and cspcode='" + cspcode + "'";

                //new code
                query = $"SELECT SUM({tcolname}) AS {tcolname} FROM bob_data.csptransactiondaily WHERE transactionperiod = @datetofetch AND cspcode = @cspcode";
                _parameters.Clear();
                _parameters.Add("@datetofetch", datetofetch);
                _parameters.Add("@cspcode", cspcode);
                //new code
                if (dtdaily.Rows.Count > 0)
                {
                    dtdaily.Rows[rownoofscheme][colname] = getValue(_parameters);
                    total += cint(dtdaily.Rows[rownoofscheme][colname]);
                }
            }
            dtdaily.Rows[rownoofscheme]["Total"] = total;
        }
        private void CreateMonthlyTable(DataTable dtMon)
        {
            for (int i = 12; i >= 0; i--)
            {
                string colname = DateTime.Now.AddMonths(-i).ToString("MMM-yy");
                dtMon.Columns.Add(colname);
            }
            dtMon.Columns.Add("Total");
        }
        private void CreateDailyTable(DataTable dt)
        {
            for (int i = 28; i > 0; i--)
            {
                try
                {
                    string colname = DateTime.Now.AddDays(-i).ToString("dd");
                    dt.Columns.Add(colname);
                }
                catch
                {
                    string colname = DateTime.Now.AddDays(-i).ToString("dd-MM");
                    dt.Columns.Add(colname);
                }
            }
            dt.Columns.Add("Total");
        }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (cstr(Session["callername"]) == "")
                {
                    Response.Redirect("Default.aspx");
                }

                //query = "select id from callermaster where username='" + Session["callername"] + "'";

                query = "SELECT id FROM callermaster WHERE username = @username";
                _parameters.Clear();
                _parameters.Add("@username", cstr(Session["callername"]));
                string callerId = getValue(_parameters);

                string callconnected = "Y";
                string callconnectedreason = ddlConnected.SelectedValue;
                if (rbFailed.Checked)
                {
                    callconnected = "N";
                    callconnectedreason = ddlFailed.SelectedValue;
                }
                int calledby = cint(getValue(_parameters));
                double callduration = 0;
                try
                {
                    DateTime startdt = Convert.ToDateTime(Session["callstartedat"]);
                    TimeSpan ts = DateTime.Now - startdt;
                    callduration = ts.TotalSeconds;
                }
                catch
                { }

                int futureid = -1;
                if (onlyheaderFuture.Visible == true)
                {
                    //query = "INSERT INTO `bob_data`.`cspcommittment`(`cspcode`,`transactionperiod`,`schemeAPY`,`schemePMJJBY`,`schemePMSBY`,`nooftransaction`, committedloginon) values ('" + lblCode.Text + "', '" + DateTime.Now.ToString("yyyy-MM-dd") + "', '" + cint(txtAPY.Text) + "', '" + cint(txtPMJJBY.Text) + "', '" + cint(txtPMSBY.Text) + "', '" + cint(txtnooftransaction.Text) + "', '" + txtlogindate.Text + "')";
                    //try
                    //{
                    //    futureid = cint(setvalueauto());
                    //}
                    query = "INSERT INTO `bob_data`.`cspcommittment`(`cspcode`, `transactionperiod`, `schemeAPY`, `schemePMJJBY`, `schemePMSBY`, `nooftransaction`, `committedloginon`) " +
        "VALUES (@cspcode, @transactionperiod, @schemeAPY, @schemePMJJBY, @schemePMSBY, @nooftransaction, @committedloginon)";

                    _parameters.Clear();
                    _parameters.Add("@cspcode", lblCode.Text.Trim());
                    _parameters.Add("@transactionperiod", DateTime.Now.ToString("yyyy-MM-dd"));
                    _parameters.Add("@schemeAPY", cstr(cint(txtAPY.Text)));
                    _parameters.Add("@schemePMJJBY", cstr(cint(txtPMJJBY.Text)));
                    _parameters.Add("@schemePMSBY", cstr(cint(txtPMSBY.Text)));
                    _parameters.Add("@nooftransaction", cstr(cint(txtnooftransaction.Text)));
                    _parameters.Add("@committedloginon", txtlogindate.Text.Trim());

                    try
                    {
                        futureid = cint(setvalueauto(_parameters));
                    }
                    catch
                    { }
                }

                //query = "insert into cspcallertransaction (`cspcode`, `callingdate`,  `calledby`, `callendat`, `callremarks` , callconnected, callconnectedreason, callstartedat, callduration, futurecommittmentid, phoneno ) values ('" + lblCode.Text + "', '" + DateTime.Now.ToString("yyyy-MM-dd") + "', '" + calledby.ToString() + "', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + txtRemarks.Text + "','" + callconnected + "', '" + callconnectedreason + "','" + cstr(Session["callstartedat"]) + "' , " + callduration.ToString("0") + ", " + futureid + ", '" + cstr(Session["mobileno"]) + "')";
                //try
                //{
                //    setvalue();
                //}

                //new code
                query = "INSERT INTO cspcallertransaction (`cspcode`, `callingdate`, `calledby`, `callendat`, `callremarks`, `callconnected`, `callconnectedreason`, `callstartedat`, `callduration`, `futurecommittmentid`, `phoneno`) " +
        "VALUES (@cspcode, @callingdate, @calledby, @callendat, @callremarks, @callconnected, @callconnectedreason, @callstartedat, @callduration, @futurecommittmentid, @phoneno)";

                _parameters.Clear();
                _parameters.Add("@cspcode", lblCode.Text.Trim());
                _parameters.Add("@callingdate", DateTime.Now.ToString("yyyy-MM-dd"));
                _parameters.Add("@calledby", calledby.ToString());
                _parameters.Add("@callendat", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                _parameters.Add("@callremarks", txtRemarks.Text.Trim());
                _parameters.Add("@callconnected", callconnected);
                _parameters.Add("@callconnectedreason", callconnectedreason);
                _parameters.Add("@callstartedat", cstr(Session["callstartedat"]));
                _parameters.Add("@callduration", callduration.ToString("0"));
                _parameters.Add("@futurecommittmentid", cstr(futureid));
                _parameters.Add("@phoneno", cstr(Session["mobileno"]));

                try
                {
                    setValue(_parameters);
                }
                //new code

                catch
                { }
                string strquery = " multipleterminal=" + ddlMultipleTerminal.SelectedValue + ", totaleligibleac=0,`totalaccount`=" + cint(lblTotalAccount.Text) + ", zerobalance =" + cint(lblZeroBalance.Text) + ", avgbalance =" + cint(lblAvgBalance.Text) + ",     `microatm`=" + chkMicroATM.Checked.ToString() + ", passbookprinter =" + chkPassbookPrinter.Checked.ToString() + ",    `iibf`=" + chkIIBF.Checked.ToString() + ", dra =" + chkDRA.Checked.ToString() + ", lam =" + chlLam.Checked.ToString() + ", iibfsq=" + chkiibfsq.Checked.ToString() + " ,    `drasq`=" + chkdrasq.Checked.ToString();

                //query = "update bob_data.cspmaster set lastcalledat ='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', lastcalldate='" + DateTime.Now.ToString("yyyy-MM-dd") + "'," + strquery + "   where cspcode='" + lblCode.Text + "'";
                //try
                //{
                //    setvalue();
                //}

                //new code
                query = "UPDATE bob_data.cspmaster SET lastcalledat = @lastcalledat, lastcalldate = @lastcalldate, " + strquery + " WHERE cspcode = @cspcode";

                _parameters.Clear();
                _parameters.Add("@lastcalledat", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                _parameters.Add("@lastcalldate", DateTime.Now.ToString("yyyy-MM-dd"));
                _parameters.Add("@cspcode", lblCode.Text.Trim()); // Trim to remove any unwanted spaces

                try
                {
                    setValue(_parameters);
                }

                //new code

                catch
                { }

                if (callconnected == "N")
                {
                    //query = "INSERT INTO `bob_data`.`call_queued`(`cspcode`,cspid,`callqueuedtime`,`callqueueddate`,`previousid`,`nth_try`,`callerid`)VALUES('" + lblCode.Text + "','" + cstr(Session["cspid"]) + "', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + DateTime.Now.ToString("yyyy-MM-dd") + "',0,0, '" + calledby.ToString() + "') ";
                    //try
                    //{
                    //    setvalue();
                    //}

                    //new code
                    query = "INSERT INTO `bob_data`.`call_queued`(`cspcode`, `cspid`, `callqueuedtime`, `callqueueddate`, `previousid`, `nth_try`, `callerid`) " +
        "VALUES(@cspcode, @cspid, @callqueuedtime, @callqueueddate, @previousid, @nth_try, @callerid)";

                    _parameters.Clear();
                    _parameters.Add("@cspcode", lblCode.Text.Trim());
                    _parameters.Add("@cspid", cstr(Session["cspid"]));
                    _parameters.Add("@callqueuedtime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    _parameters.Add("@callqueueddate", DateTime.Now.ToString("yyyy-MM-dd"));
                    _parameters.Add("@previousid", cstr(0));
                    _parameters.Add("@nth_try", cstr(0));
                    _parameters.Add("@callerid", calledby.ToString());

                    try
                    {
                        setValue(_parameters);
                    }
                    //new code
                    catch (Exception ex)
                    { }
                }
                Session.Add("callstarted", false);
                imgCall.ImageUrl = "~/img/phone.png";
                Response.Redirect("Calling_Form.aspx");
            }

            catch
            { btnSave.Enabled = true; }
        }
        protected void btnBreak_Click(object sender, EventArgs e)
        {
            btnSave_Click(sender, e);
        }
        protected void imgCall_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                //if (cbln(Session["callstarted"]))
                //{
                //    return;
                //}
                Session.Add("callstarted", true);
                //imgCall.ImageUrl = "~/img/phoneinprogress.png";
                //imgCall.Enabled = false;
                //imgCall.Visible = false;
                //System.Threading.Thread.Sleep(1500);
                Ssl.EnableTrustedHosts();
                string challenge = dochallenge();
                ChallengeInfo.Root challengeinfo = JsonConvert.DeserializeObject<ChallengeInfo.Root>(challenge);
                string hashed = new HashedHelper().GetHashedData(challengeinfo.response.challenge);

                string cookie = dopost(hashed);
                try
                {
                    CookieInfo.Root callCookie = JsonConvert.DeserializeObject<CookieInfo.Root>(cookie);
                    string scookie = callCookie.response.cookie;

                    //query = "select extensionno from bob_data.callermaster where username = '" + Session["callername"] + "'";
                    //string extn = getvalue();

                    //new code
                    query = "SELECT extensionno FROM bob_data.callermaster WHERE username = @username";
                    _parameters.Clear();
                    _parameters.Add("@username", cstr(Session["callername"]));

                    string extn = getValue(_parameters);
                    //new code
                    string callresponse = doCall(scookie, extn, hypMobileNo.Text.Trim());
                }
                catch (Exception ex)
                { }
            }
            catch (Exception ex)
            { }

        }
        string dopost(string token)
        {

            var url = "https://192.168.2.150:8089/api";

            var request = WebRequest.Create(url);
            request.Method = "POST";


            string json = "{ \"request\":{ \"action\":\"login\", \"token\":\"" + token + "\", \"url\":\"https://192.168.2.150:8089/api\", \"user\":\"cdrapi\" }}";

            byte[] byteArray = Encoding.UTF8.GetBytes(json);

            request.ContentType = @"application/json; charset=utf-8";
            request.ContentLength = byteArray.Length;

            var reqStream = request.GetRequestStream();
            reqStream.Write(byteArray, 0, byteArray.Length);

            var response = request.GetResponse();

            var respStream = response.GetResponseStream();

            var reader = new StreamReader(respStream);
            string data = reader.ReadToEnd();

            return data;
        }
        private string doCall(string cookie, string extensionno, string mobileno)
        {
            Session["callstartedat"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Session["mobileno"] = mobileno;
            var url = "https://192.168.2.150:8089/api";

            var request = WebRequest.Create(url);
            request.Method = "POST";

            string json = "{ \"request\":{ \"action\":\"dialOutbound\", \"outbound\":\"0" + mobileno + "\", \"caller\":\"" + extensionno + "\", \"cookie\":\"" + cookie + "\" }}";

            byte[] byteArray = Encoding.UTF8.GetBytes(json);

            request.ContentType = @"application/json; charset=utf-8";
            request.ContentLength = byteArray.Length;

            var reqStream = request.GetRequestStream();
            reqStream.Write(byteArray, 0, byteArray.Length);

            var response = request.GetResponse();

            var respStream = response.GetResponseStream();

            var reader = new StreamReader(respStream);
            string data = reader.ReadToEnd();

            return data;
        }
        private string dochallenge()
        {
            var url = "https://192.168.2.150:8089/api";

            var request = WebRequest.Create(url);
            request.Method = "POST";


            string json = "{ \"request\":{ \"action\":\"challenge\", \"user\":\"cdrapi\", \"version\":\"1.2\" }}";

            byte[] byteArray = Encoding.UTF8.GetBytes(json);

            request.ContentType = @"application/json; charset=utf-8";
            request.ContentLength = byteArray.Length;

            var reqStream = request.GetRequestStream();
            reqStream.Write(byteArray, 0, byteArray.Length);

            var response = request.GetResponse();

            var respStream = response.GetResponseStream();

            var reader = new StreamReader(respStream);
            string data = reader.ReadToEnd();

            return data;
        }
        protected void ddlConnected_SelectedIndexChanged(object sender, EventArgs e)
        {
            onlyheaderFuture.Visible = true;
        }
        protected void rbConnected_CheckedChanged(object sender, EventArgs e)
        {
            if (rbConnected.Checked)
            {
                ddlConnected.Visible = true;
                ddlFailed.Visible = false;
            }
            else
            {
                ddlConnected.Visible = false;
                ddlFailed.Visible = true;
            }
        }
        //protected void imgSVCall_Click(object sender, ImageClickEventArgs e)
        //{
        //    Response.Redirect("CallingFormOthers.aspx?mobileno=" + lblSupervisorMobile.Text + "&ename=" + lblSupervisorName.Text + "&etype=" + "Supervisor");
        //}

        //protected void imgEdit_Click(object sender, ImageClickEventArgs e)
        //{
        //    if (hypMobileNo.Text.Length > 10)
        //    {
        //        return;
        //    }
        //    lblAlternate1.Text = lblAlternate.Text;
        //    lblAlternate.Text = hypMobileNo.Text;
        //    hypMobileNo.Text = "";
        //    hypMobileNo.Enabled = true;
        //    hypMobileNo.ReadOnly = false;

        //    imgPhoneSave.Visible = true;
        //    imgEdit.Visible = false;
        //}
        //protected void imgalternate_Click(object sender, ImageClickEventArgs e)
        //{
        //    try
        //    {
        //        Ssl.EnableTrustedHosts();
        //        string challenge = dochallenge();
        //        ChallengeInfo.Root challengeinfo = JsonConvert.DeserializeObject<ChallengeInfo.Root>(challenge);
        //        string hashed = new HashedHelper().GetHashedData(challengeinfo.response.challenge);

        //        string cookie = dopost(hashed);
        //        try
        //        {
        //            CookieInfo.Root callCookie = JsonConvert.DeserializeObject<CookieInfo.Root>(cookie);
        //            string scookie = callCookie.response.cookie;

        //            query = "select extensionno from callermaster where username = '" + Session["callername"] + "'";
        //            string extn = getvalue();
        //            string callresponse = doCall(scookie, extn, lblAlternate.Text.Trim());
        //        }
        //        catch (Exception ex)
        //        { }

        //    }
        //    catch (Exception ex)
        //    { }
        //}
        protected void imgalternate1_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                Ssl.EnableTrustedHosts();
                string challenge = dochallenge();
                ChallengeInfo.Root challengeinfo = JsonConvert.DeserializeObject<ChallengeInfo.Root>(challenge);
                string hashed = new HashedHelper().GetHashedData(challengeinfo.response.challenge);

                string cookie = dopost(hashed);
                try
                {
                    CookieInfo.Root callCookie = JsonConvert.DeserializeObject<CookieInfo.Root>(cookie);
                    string scookie = callCookie.response.cookie;

                    query = "select extensionno from callermaster where username = '" + Session["callername"] + "'";
                    string extn = getvalue();
                    string callresponse = doCall(scookie, extn, lblAlternate1.Text.Trim());
                }
                catch (Exception ex)
                { }
            }
            catch (Exception ex)
            {
            }
        }
        //protected void imgName_Click(object sender, ImageClickEventArgs e)
        //{
        //    lblName.Visible = false;
        //    txtCSPName.Visible = true;
        //    imgNameSave.Visible = true;
        //    imgName.Visible = false;
        //}
        //protected void imgNameSave_Click(object sender, ImageClickEventArgs e)
        //{

        //    lblName.Visible = true;
        //    txtCSPName.Visible = false;
        //    imgNameSave.Visible = false;
        //    imgName.Visible = false;
        //    if (txtCSPName.Text == "")
        //    {
        //        return;
        //    }
        //    query = "update cspmaster set cspname='" + txtCSPName.Text + "' , cspstatus='ACTIVE' where id=" + cstr(Session["cspid"]);
        //    setvalue();
        //    lblName.Text = txtCSPName.Text;
        //}
        protected void imgstaff1_Click(object sender, ImageClickEventArgs e)
        {

            switch (((ImageButton)sender).ClientID)
            {
                case "imgstaff1":
                    Response.Redirect("CallingFormOthers.aspx?mobileno=" + lblMobileStaff1.Text + "&ename=" + lblstaff1.Text + "&etype=" + "Office Staff");
                    break;
                case "imgstaff2":
                    Response.Redirect("CallingFormOthers.aspx?mobileno=" + lblMobileStaff2.Text + "&ename=" + lblstaff2.Text + "&etype=" + "Office Staff");
                    //callnumber(lblMobileStaff2.Text);
                    break;
                case "imgstaff3":
                    Response.Redirect("CallingFormOthers.aspx?mobileno=" + lblMobileStaff3.Text + "&ename=" + lblstaff3.Text + "&etype=" + "Office Staff");
                    //callnumber(lblMobileStaff3.Text);
                    break;
                case "imgstaff4":
                    //callnumber(lblMobileStaff4.Text);
                    Response.Redirect("CallingFormOthers.aspx?mobileno=" + lblMobileStaff4.Text + "&ename=" + lblstaff4.Text + "&etype=" + "Office Staff");
                    break;
                case "imgstaff5":
                    //callnumber(lblMobileStaff5.Text);
                    Response.Redirect("CallingFormOthers.aspx?mobileno=" + lblMobileStaff5.Text + "&ename=" + lblstaff5.Text + "&etype=" + "Office Staff");
                    break;
                default:
                    break;
            }

        }
        private void callnumber(string phonenumber)
        {
            try
            {
                Ssl.EnableTrustedHosts();
                string challenge = dochallenge();
                ChallengeInfo.Root challengeinfo = JsonConvert.DeserializeObject<ChallengeInfo.Root>(challenge);
                string hashed = new HashedHelper().GetHashedData(challengeinfo.response.challenge);

                string cookie = dopost(hashed);
                try
                {
                    CookieInfo.Root callCookie = JsonConvert.DeserializeObject<CookieInfo.Root>(cookie);
                    string scookie = callCookie.response.cookie;

                    query = "select extensionno from callermaster where username = '" + Session["callername"] + "'";
                    string extn = getvalue();
                    string callresponse = doCall(scookie, extn, phonenumber);
                }
                catch (Exception ex)
                {

                }

            }
            catch (Exception ex)
            {


            }
        }
        //protected void imgPhoneSave_Click(object sender, ImageClickEventArgs e)
        //{
        //    hypMobileNo.Enabled = false;
        //    hypMobileNo.ReadOnly = true;

        //    imgPhoneSave.Visible = false;
        //    imgEdit.Visible = true;
        //    if (hypMobileNo.Text == "")
        //    {
        //        return;
        //    }
        //    query = "update cspmaster set mobileno='" + hypMobileNo.Text + "' where id=" + cstr(Session["cspid"]);
        //    setvalue();
        //    query = "update cspmaster set alternatenumber2='" + lblAlternate1.Text + "' where id=" + cstr(Session["cspid"]);
        //    setvalue();
        //    query = "update cspmaster set alternatenumbers='" + lblAlternate.Text + "' where id=" + cstr(Session["cspid"]);
        //    setvalue();
        //    lblName.Text = txtCSPName.Text;
        //}
        //protected void imgSipCall_Click(object sender, ImageClickEventArgs e)
        //{
        //    //Session.Add("callstarted", false);
        //    //imgCall.ImageUrl = "~/img/phone.png";
        //    Session["callstartedat"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //    Session["mobileno"] = hypMobileNo.Text.Trim();
        //    Response.Redirect("sip:0" + hypMobileNo.Text + "@192.168.2.150?method=call");

        //}

        protected void btnReset_Click(object sender, EventArgs e)
        {
            string id = Request.QueryString["id"];
            //query = "select cspcode from bob_data.cspmaster where id='" + id + "'";
            //string cspcode = getvalue();

            //new code
            query = "SELECT cspcode FROM bob_data.cspmaster WHERE id = @id";
            _parameters.Clear();
            _parameters.Add("@id", id);

            string cspcode = getValue(_parameters);
            //new code

            //query = "update `cspdata`.`fedcspdetails` set csppassword='" + EncryptAesManaged("san123456") + "' where cspid='" + cspcode + "'";
            //setvalue();

            //new code
            query = "UPDATE `cspdata`.`fedcspdetails` SET csppassword = @csppassword WHERE cspid = @cspid";
            _parameters.Clear();
            _parameters.Add("@csppassword", EncryptAesManaged("san123456"));
            _parameters.Add("@cspid", cspcode);

            setValue(_parameters);

            //new code

            Show("Password is Reset. Ask to Change password.");
        }
        protected void lnkSupervisor_Click(object sender, EventArgs e)
        {
            Response.Redirect("CallingFormOthers.aspx?mobileno="
                + lblSupervisorMobile.Text
                + "&ename=" + lblSupervisorName.Text
                + "&etype=Supervisor");
        }



        protected void btnEditName_Click(object sender, EventArgs e)
        {
            lblName.Visible = false;
            txtCSPName.Visible = true;
            btnSaveName.Visible = true;
            btnEditName.Visible = false;
        }

        protected void btnSaveName_Click(object sender, EventArgs e)
        {
            lblName.Visible = true;
            txtCSPName.Visible = false;
            btnSaveName.Visible = false;
            btnEditName.Visible = false;
            if (txtCSPName.Text == "")
            {
                return;
            }
            //query = "update cspmaster set cspname='" + txtCSPName.Text + "' , cspstatus='ACTIVE' where id=" + cstr(Session["cspid"]);
            //setvalue();

            //new code
            query = "UPDATE cspmaster SET cspname = @cspname, cspstatus = 'ACTIVE' WHERE id = @cspid";
            _parameters.Clear();
            _parameters.Add("@cspname", txtCSPName.Text.Trim());
            _parameters.Add("@cspid", cstr(Session["cspid"]));

            setValue(_parameters);
            //new code

            lblName.Text = txtCSPName.Text;
        }

        protected void btnsipcall_Click(object sender, EventArgs e)
        {
            Session["callstartedat"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string mobileNo = hypMobileNo.Text.Trim();
            Session["mobileno"] = mobileNo;

            // Sanjivani auto-call: log the call start to local PostgreSQL.
            // This must never break the call, so failures are swallowed.
            try
            {
                string caller = Convert.ToString(Session["callername"]);
                PgDb.LogCallStart(mobileNo, caller, PgDb.GreetingForNow());
            }
            catch { }

            Response.Redirect("sip:0" + mobileNo + "@192.168.2.150?method=call");
        }

        /// <summary>
        /// Called from the browser (PageMethods) when the Sanjivani audio message
        /// finishes playing to the customer. Marks the latest call_log row.
        /// </summary>
        [WebMethod]
        public static void MarkAudioPlayed(string mobileNo, string callerName)
        {
            try { PgDb.MarkAudioPlayed(mobileNo, callerName); }
            catch { }
        }

        protected void editPhoneNum_Click(object sender, EventArgs e)
        {
            if (hypMobileNo.Text.Length > 10)
            {
                return;
            }
            lblAlternate1.Text = lblAlternate.Text;
            lblAlternate.Text = hypMobileNo.Text;
            hypMobileNo.Text = "";
            hypMobileNo.Enabled = true;
            hypMobileNo.ReadOnly = false;

            btnPhoneSave.Visible = true;
            editPhoneNum.Visible = false;
        }

        protected void btnPhoneSave_Click(object sender, EventArgs e)
        {
            hypMobileNo.Enabled = false;
            hypMobileNo.ReadOnly = true;

            btnPhoneSave.Visible = false;
            editPhoneNum.Visible = true;
            if (hypMobileNo.Text == "")
            {
                return;
            }
            //query = "update cspmaster set mobileno='" + hypMobileNo.Text + "' where id=" + cstr(Session["cspid"]);
            //setvalue();

            //new code
            query = "UPDATE cspmaster SET mobileno = @mobileno WHERE id = @cspid";

            _parameters.Clear();
            _parameters.Add("@mobileno", hypMobileNo.Text.Trim());
            _parameters.Add("@cspid", cstr(Session["cspid"]));

            setValue(_parameters);
            //new code

            //query = "update cspmaster set alternatenumber2='" + lblAlternate1.Text + "' where id=" + cstr(Session["cspid"]);
            //setvalue();

            //new code
            query = "UPDATE cspmaster SET alternatenumber2 = @alternate2 WHERE id = @cspid";

            _parameters.Clear();
            _parameters.Add("@alternate2", lblAlternate1.Text.Trim());
            _parameters.Add("@cspid", cstr(Session["cspid"]));

            setValue(_parameters);
            //new code

            //query = "update cspmaster set alternatenumbers='" + lblAlternate.Text + "' where id=" + cstr(Session["cspid"]);
            //setvalue();

            //new code
            query = "UPDATE cspmaster SET alternatenumbers = @alternate WHERE id = @cspid";

            _parameters.Clear();
            _parameters.Add("@alternate", lblAlternate.Text.Trim());
            _parameters.Add("@cspid", cstr(Session["cspid"]));

            setValue(_parameters);
            //new code

            lblName.Text = txtCSPName.Text;
        }

        protected void btnalternate_Click(object sender, EventArgs e)
        {
            try
            {
                Ssl.EnableTrustedHosts();
                string challenge = dochallenge();
                ChallengeInfo.Root challengeinfo = JsonConvert.DeserializeObject<ChallengeInfo.Root>(challenge);
                string hashed = new HashedHelper().GetHashedData(challengeinfo.response.challenge);

                string cookie = dopost(hashed);
                try
                {
                    CookieInfo.Root callCookie = JsonConvert.DeserializeObject<CookieInfo.Root>(cookie);
                    string scookie = callCookie.response.cookie;

                    //query = "select extensionno from callermaster where username = '" + Session["callername"] + "'";
                    //string extn = getvalue();

                    //new code
                    query = "SELECT extensionno FROM callermaster WHERE username = @username";

                    _parameters.Clear();
                    _parameters.Add("@username", cstr(Session["callername"]));

                    string extn = getValue(_parameters);
                   //new code
                    string callresponse = doCall(scookie, extn, lblAlternate.Text.Trim());
                }
                catch (Exception ex)
                { }

            }
            catch (Exception ex)
            { }
        }

        protected void imgDc_Click(object sender, EventArgs e)
        {
            Response.Redirect("CallingFormOthers.aspx?mobileno=" + lblDCMobileNo.Text + "&ename=" + lblDCName.Text + "&etype=" + "District Coordinator");
        }

        //protected void imgDCCall_Click(object sender, ImageClickEventArgs e)
        //{
        //    Response.Redirect("CallingFormOthers.aspx?mobileno=" + lblDCMobileNo.Text + "&ename=" + lblDCName.Text + "&etype=" + "District Coordinator");
        //    //try
        //    //{
        //    //    Ssl.EnableTrustedHosts();
        //    //    string challenge = dochallenge();
        //    //    ChallengeInfo.Root challengeinfo = JsonConvert.DeserializeObject<ChallengeInfo.Root>(challenge);
        //    //    string hashed = new HashedHelper().GetHashedData(challengeinfo.response.challenge);

        //    //    string cookie = dopost(hashed);
        //    //    try
        //    //    {
        //    //        CookieInfo.Root callCookie = JsonConvert.DeserializeObject<CookieInfo.Root>(cookie);
        //    //        string scookie = callCookie.response.cookie;

        //    //        query = "select extensionno from callermaster where username = '" + Session["callername"] + "'";
        //    //        string extn = getvalue();
        //    //        string callresponse = doCall(scookie, extn, lblDCMobileNo.Text);
        //    //    }
        //    //    catch (Exception ex)
        //    //    {

        //    //    }

        //    //}
        //    //catch (Exception ex)
        //    //{


        //    //}
        //}
        private void loadcspimg()
        {
            string cspCode = lblCode.Text;
            string documenttype = "Photo";
            string defaultImage = "~/img/undraw_profile_2.svg";
            imgCSP.ImageUrl = defaultImage;
            if (string.IsNullOrEmpty(cspCode))
            {
                return;
            }
            //query = "Select documentname from cspdata.fed_documentsmaster where cspcode='" + cspCode + "' and documenttype='" + documenttype + "'";

            //string filename = getvalue();

            //new code

            query = "SELECT documentname FROM cspdata.fed_documentsmaster WHERE cspcode = @cspcode AND documenttype = @documenttype";

            _parameters.Clear();
            _parameters.Add("@cspcode", cspCode);
            _parameters.Add("@documenttype", documenttype);
            //new code

            string filename = getValue(_parameters);





            //string defaultImage = "~/img/undraw_profile_2.svg";


            if (!string.IsNullOrEmpty(filename))
            {
                string externalBaseUrl = "https://dashboard.sanjivani.foundation/Uploads/";
                imgCSP.ImageUrl = externalBaseUrl + filename;
            }
        }
    }
}