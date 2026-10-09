using System;
using System.Collections.Generic;
using System.Web.UI;

namespace RiskManagement.SanjivaniBriefing
{
    /// <summary>
    /// Standalone stub of the office base page (WebHelperCSP).
    /// The page only needs these members to compile; the Sanjivani flow
    /// (sip: call button + browser audio panel) does not touch the office
    /// database or telephony API, so the stubs below are safe no-ops.
    /// Office-only integrations throw NotImplementedException if ever called.
    /// </summary>
    public class WebHelperCSP : Page
    {
        protected string query;
        protected Dictionary<string, object> _parameters = new Dictionary<string, object>();

        protected string cstr(object value)
        {
            return value == null ? string.Empty : value.ToString();
        }

        protected string getValue(Dictionary<string, object> parameters)
        {
            // Office MySQL helper is not part of the standalone build.
            return string.Empty;
        }

        protected void setValue(Dictionary<string, object> parameters)
        {
            // Office MySQL helper is not part of the standalone build.
        }

        protected string dochallenge()
        {
            throw new NotImplementedException("Office telephony API is not available in the standalone build.");
        }

        protected string dopost(string hashed)
        {
            throw new NotImplementedException("Office telephony API is not available in the standalone build.");
        }
    }
}
