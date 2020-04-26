using LetsWork.Domain.Models;
using System.Threading.Tasks;

namespace LetsWork.Domain.Interfaces.ServiceManagers
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailModel EmailModelObject);
    }
}
