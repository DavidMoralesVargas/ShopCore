using Tienda_Online.Shared.Respuesta;

namespace Tienda_Online.Backend.Helpers
{
    public interface IMailHelper
    {
        AccionRespuesta<string> SendMail(string toName, string toEmail, string subject, string body);
    }
}
