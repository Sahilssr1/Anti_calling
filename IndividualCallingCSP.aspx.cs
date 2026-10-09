using System;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace RiskManagement.SanjivaniBriefing
{
    /// <summary>
    /// Sanjivani calling page — no database, no office dependencies.
    /// Agent enters the customer's mobile number, clicks Call (launches MicroSIP
    /// via the sip: protocol), then presses PLAY in the floating panel so the
    /// customer hears the recorded Sanjivani message first.
    /// </summary>
    public partial class IndividualCallingCSP : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnCall_Click(object sender, EventArgs e)
        {
            string phone = (txtPhone.Text ?? string.Empty).Trim();
            if (!Regex.IsMatch(phone, @"^\d{10}$"))
            {
                lblMsg.Text = "Sahi 10-digit mobile number likho.";
                return;
            }

            // Hand off to MicroSIP through the sip: protocol handler;
            // MicroSIP dials the customer through 192.168.2.150.
            Response.Redirect("sip:0" + phone + "@192.168.2.150?method=call");
        }
    }
}
