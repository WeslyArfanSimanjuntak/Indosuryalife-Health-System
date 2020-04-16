using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace Web.MainApplication.WebUtility
{
    public class WebAppClaimPrincipal : ClaimsPrincipal
    {


        public WebAppClaimPrincipal(WebClaimIdentity identity)
                : base(identity)
        {

        }

        public WebAppClaimPrincipal(ClaimsPrincipal claimsPrincipal)
                : base(claimsPrincipal)
        {

        }
    }
}