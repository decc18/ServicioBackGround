using Microsoft.Extensions.DependencyInjection;
using ServicioBackground.Data;
using ServicioBackground.DTO;
using ServicioBackground.Enum;
using ServicioBackground.Logging;
using System;

namespace ServicioBackground.BackgroundWorker
{
    public class PosBackgroundService : BackgroundService
    {
        private readonly INLogLogger _logger;
        private readonly IConfiguration _configuration;
        private readonly ITransaccionRepository _transaccionRepository;

        public PosBackgroundService(ITransaccionRepository transaccionRepository, IConfiguration configuration, INLogLogger logger)
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
            _logger.Info("PosBackgroundService iniciado.");
            int enviarPendientesCada = _configuration.GetValue("ServicioBackground:EnviarPendientesCada", 5);
            string codigoPostal = _configuration.GetValue("ServicioBackground:CodigoPostalTienda", "01120");
            var ticketPendiente = new TicketsPendientes();
            Tuple<bool, string> respuesta = new Tuple<bool, string>(false, string.Empty);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    List<Ticket> tickets = await _transaccionRepository.ConsultarTicketsPendientes(EnumEstado.Insertado);
                    foreach (Ticket ticket in tickets)
                    {
                        ticketPendiente = new TicketsPendientes();
                        ticket.CodigoPostal = codigoPostal;
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
