using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;

namespace LetsWork.API.Extensions
{
    public static class HelperExtensions
    {
        public static string GetIdentityResultErrorMessage(this IdentityResult IdentityCreateResult)
        {
            string errorString = string.Join(Environment.NewLine, IdentityCreateResult.Errors.Select(x => x.Description));
            return errorString;
        }
    }
}
