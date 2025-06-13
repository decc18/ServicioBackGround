using Microsoft.Extensions.DependencyInjection;
using ServicioBackground.Data;
using ServicioBackground.DTO;
using ServicioBackground.Enum;
using ServicioBackground.Logging;
using System;

namespace ServicioBackground.BackgroundWorker
{
    public class PosBackgroundServiceReintentos : BackgroundService
    {
        private readonly INLogLogger _logger;
        private readonly IConfiguration _configuration;
        private readonly ITransaccionRepository _transaccionRepository;


        public PosBackgroundServiceReintentos(ITransaccionRepository transaccionRepository, IConfiguration configuration, INLogLogger logger)
        {
            _transaccionRepository = transaccionRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Info("PosBackgroundServiceReintentos iniciado.");
            int enviarPendientesCada = _configuration.GetValue("ServicioBackground:EnviarReintentosPendientesCada", 5);
            var ticketPendiente = new TicketsPendientes();
            Tuple<bool, string> respuesta = new Tuple<bool, string>(false, string.Empty);
            await Task.Delay(enviarPendientesCada * 60000);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    List<DtoReintentos> ticketsReintentos = await _transaccionRepository.ConsultarTicketsReintento();
                    foreach (var item in ticketsReintentos)
                    {
                        Ticket ticket = await _transaccionRepository.ConsultarTicketPendiente(item.IdTicket);
                        ticket.Reintento = item.NumeroReintentos+1;
                        ticketPendiente = new TicketsPendientes();
                        ticketPendiente.Tickets.Add(ticket);
                        respuesta = await _transaccionRepository.EnviarTicketPendientes(ticketPendiente);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error en PosBackgroundService:", ex);
                }
                finally
                {
                    await Task.Delay(enviarPendientesCada * 60000, stoppingToken);
                }
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
