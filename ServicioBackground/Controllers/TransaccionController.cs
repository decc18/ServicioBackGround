using Microsoft.AspNetCore.Mvc;
using ServicioBackground.Data;
using ServicioBackground.DTO;
using ServicioBackground.Logging;

namespace ServicioBackground.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransaccionController : ControllerBase
    {

        private readonly INLogLogger _logger;
        private readonly IConfiguration _configuration;
        private readonly ITransaccionRepository _transaccionRepository;

        public TransaccionController(IConfiguration configuration, INLogLogger logger, ITransaccionRepository transaccionRepository)
        {
            _configuration = configuration;
            _logger = logger;
            _transaccionRepository = transaccionRepository;
        }

        [HttpPost]
        [Route("EnviarTransaccionesPendientes")]
        public IActionResult EnviarTransaccionesPendientes([FromBody] Ticket ticket)
        {
            _logger.Info("PosBackgroundService iniciado.");
            TicketsPendientes ticketPendientes = new TicketsPendientes();
            ticketPendientes.Tickets.Add(ticket);   
            var respuesta = _transaccionRepository.EnviarTicketPendientes(ticketPendientes);
            return StatusCode(StatusCodes.Status200OK, respuesta);
        }

    }
}
