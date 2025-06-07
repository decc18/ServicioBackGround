using ServicioBackground.DTO;
using ServicioBackground.Enum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServicioBackground.Data
{
    public interface ITransaccionRepository
    {
        Task<List<Ticket>> ConsultarTicketsPendientes(EnumEstado estado);

        Task<Ticket> ConsultarTicketPendiente(int IdTicket);

        Task<Tuple<bool, string>> EnviarTicketPendientes(TicketsPendientes tickets);

        Task<List<DtoReintentos>> ConsultarTicketsReintento();
    }
}
