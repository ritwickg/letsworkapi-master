using Ardalis.GuardClauses;
using System.Net;

namespace LetsWork.Domain.Exceptions
{
    public static class GuardClauseExtensions
    {
        public static void NullItem<T>(this IGuardClause GuardClause, T ItemToCheck) where T : class, new()
        {
            if (ItemToCheck == null)
                throw new HttpException($"{nameof(ItemToCheck)} is empty, please check the request.", HttpStatusCode.BadRequest);
        }

        public static void NullString(this IGuardClause GuardClause, string TestString)
        {
            if(string.IsNullOrEmpty(TestString))
                throw new HttpException($"{nameof(TestString)} is null or an empty string, please check the request.", HttpStatusCode.BadRequest);
        }
    }
}
