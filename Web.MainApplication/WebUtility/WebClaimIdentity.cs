using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace Web.MainApplication.WebUtility
{
    public class WebClaimIdentity : ClaimsIdentity
    {

        public const string RolesClaimType = "http://www.indosuryalife.com/CuttingEdge.Security.Role";
        public const string GroupClaimType = "http://www.indosuryalife.com/CuttingEdge.Security.Group";
        public const string IPClaimType = "http://www.indosuryalife.com/CuttingEdge.Security.IP";

        public const string FullNameType = "http://www.weslyarfansimanjuntak.com/CuttingEdge.Security.FullName";
        public const string Username = "http://www.weslyarfansimanjuntak.com/CuttingEdge.Security.Username";
        public const string MenuString = "http://www.weslyarfansimanjuntak.com/CuttingEdge.Security.MenuString";
        public const string Menu = "http://www.weslyarfansimanjuntak.com/CuttingEdge.Security.Menu";

        public WebClaimIdentity(IEnumerable<Claim> claims, string authenticationType)
                : base(claims, authenticationType: authenticationType)
        {
            var claimUsername = claims.Where(x => x.Type == Username).FirstOrDefault();
            var claimFullName = claims.Where(x => x.Type == FullNameType).FirstOrDefault();
            var claimMenuString = claims.Where(x => x.Type == MenuString).FirstOrDefault();
            AddClaim(claimUsername);
            AddClaim(claimFullName);
            AddClaim(claimMenuString);
        }

        public WebClaimIdentity(IEnumerable<string> roles, IEnumerable<string> groups, string IP, int id)
        {
            AddClaims(from @group in groups select new Claim(GroupClaimType, @group));
            AddClaims(from role in roles select new Claim(RolesClaimType, role));
            AddClaim(new Claim(IPClaimType, IP.ToString()));
        }

        public IEnumerable<string> Roles
        {
            get
            {
                return from claim in FindAll(RolesClaimType) select claim.Value;
            }
        }

        public IEnumerable<string> Groups { get { return from claim in FindAll(GroupClaimType) select claim.Value; } }

        public string IP { get { return FindFirst(IPClaimType).Value; } }
    }
}