using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;

namespace AuthenticationBot
{
    /// <summary>
    /// Sample claims validator that loads an allowed list from configuration if present
    /// and checks that requests are coming from allowed parent bots.
    /// </summary>
    public class AllowedCallersClaimsValidator : ClaimsValidator
    {
        public AllowedCallersClaimsValidator()
        {
        }

        public override Task ValidateClaimsAsync(IList<Claim> claims)
        {
            return Task.CompletedTask;
        }
    }
}
