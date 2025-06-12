using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using ServicioBackground.DTO;
using ServicioBackground.Enum;
using ServicioBackground.Logging;
using ServicioBackground.Util;
using System.Data;
using System.Net.Http.Headers;
using System.Text;

namespace ServicioBackground.Data
{
    public class TransaccionRepository : ITransaccionRepository
    {
        private readonly IConfiguration _configuration;
        private readonly INLogLogger _logger;
        // Variables para almacenar el token y su expiración
        private string? _accessToken;
        private DateTime _tokenExpiration = DateTime.MinValue;

        public TransaccionRepository(IConfiguration configuration, INLogLogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public async Task<Tuple<bool, string>> EnviarTicketPendientes(TicketsPendientes tickets)
        {
            string urlBase = _configuration.GetValue("ServicioBackground:UrlBase", "");
            Tuple<bool, string> responsePostCheck = new Tuple<bool, string>(false, string.Empty);
            string token = await GenerarToken(urlBase);
            if (!string.IsNullOrEmpty(token))
            {
                string jsonContent = JsonConvert.SerializeObject(tickets);

                responsePostCheck = await UtilApi.PostBodyAsync(
                    urlBase,
                    "o/rco/tickets/v1.0/tickets/negocios",
                    jsonContent,
                    new AuthenticationHeaderValue("Bearer", token));
                Ticket tocketEnvio = tickets.Tickets.First();
                await InsertarLogTicket(tocketEnvio, responsePostCheck, jsonContent, tocketEnvio.Reintento);
                await ActualizarEstadoTicket(responsePostCheck, tocketEnvio);
            }
            else
                responsePostCheck = new Tuple<bool, string>(false, $"Error al generar token");

            return responsePostCheck;
        }

        public async Task<List<Ticket>> ConsultarTicketsPendientes(EnumEstado estado)
        {
            try
            {
                string cadenaConexion = _configuration.GetValue("ServicioBackground:CadenaConexion", "");
                var connection = new MySqlConnection(cadenaConexion);

                var parametros = new DynamicParameters();
                parametros.Add("p_estado", (int)estado, DbType.Int32);

                var tickets = await connection.QueryAsync<Ticket>(
                    "sp_consultar_tickets_por_estado",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );
                
                if(tickets == null || !tickets.Any())
                    return new List<Ticket>();
                
                if (tickets.Any())
                {
                    foreach (var ticket in tickets)
                    {
                        var parametrosProducto = new DynamicParameters();
                        parametrosProducto.Add("p_ticket_id", (int)ticket.Id, DbType.Int32);

                        var productosBDD = await connection.QueryAsync<Producto>(
                            "sp_consultar_productos_ticket",
                            parametrosProducto,
                            commandType: CommandType.StoredProcedure
                        );
                        if(productosBDD == null || !productosBDD.Any())
                        {
                            _logger.Info($"No se encontraron productos para el ticket {ticket.Id}.");
                            continue;
                        }
                        ticket.Productos.AddRange(productosBDD);
                    }
                }

                return tickets.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error("Ejecución: sp_obtener_transacciones_por_estado", ex);
                throw;
            }
        }

        public async Task<Ticket> ConsultarTicketPendiente(int idTicket)
        {
            try
            {
                _logger.Info("Ejecución: sp_consultar_ticket_por_id");
                string cadenaConexion = _configuration.GetValue("ServicioBackground:CadenaConexion", "");
                var connection = new MySqlConnection(cadenaConexion);

                var parametros = new DynamicParameters();
                parametros.Add("p_id_ticket", idTicket, DbType.Int32);

                var tickets = await connection.QueryAsync<Ticket>(
                    "sp_consultar_ticket_por_id",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                if (tickets == null || !tickets.Any())
                {
                    _logger.Info("No se encontraron transacciones pendientes.");
                    return new Ticket();
                }

                if (tickets.Any())
                {
                    foreach (var ticket in tickets)
                    {
                        var parametrosProducto = new DynamicParameters();
                        parametrosProducto.Add("p_ticket_id", (int)ticket.Id, DbType.Int32);

                        var productosBDD = await connection.QueryAsync<Producto>(
                            "sp_consultar_productos_ticket",
                            parametrosProducto,
                            commandType: CommandType.StoredProcedure
                        );
                        if (productosBDD == null || !productosBDD.Any())
                        {
                            _logger.Info($"No se encontraron productos para el ticket {ticket.Id}.");
                            continue;
                        }
                        ticket.Productos.AddRange(productosBDD);
                    }
                }

                return tickets.First();
            }
            catch (Exception ex)
            {
                _logger.Error("Ejecución: sp_obtener_transacciones_por_estado", ex);
                throw;
            }
        }

        public async Task<List<DtoReintentos>> ConsultarTicketsReintento()
        {
            try
            {
                int reintentos = _configuration.GetValue("ServicioBackground:ReintentosEnvio", 5);
                string cadenaConexion = _configuration.GetValue("ServicioBackground:CadenaConexion", "");
                var connection = new MySqlConnection(cadenaConexion);

                var parametros = new DynamicParameters();
                parametros.Add("p_max_reintentos", reintentos, DbType.Int32);

                var logsEnvio = await connection.QueryAsync<DtoReintentos>(
                    "sp_consultar_logs_envio_api",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                if (logsEnvio == null || !logsEnvio.Any())
                    return new List<DtoReintentos>();

                return logsEnvio.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error("Ejecución: sp_obtener_transacciones_por_estado", ex);
                throw;
            }
        }

        private async Task<string> GenerarToken(string urlBase)
        {
            // Si el token existe y no ha expirado, lo reutilizamos
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiration)
            {
                return _accessToken;
            }

            try
            {
                string clientId = _configuration.GetValue("ServicioBackground:ClientId", "");
                string clientSecret = _configuration.GetValue("ServicioBackground:ClientSecret", "");

                var formData = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", clientId },
                    { "client_secret", clientSecret }
                };

                Tuple<bool, string> responsePostCheck = await UtilApi.PostFormUrlEncodedAsync(
                    urlBase,
                    "o/oauth2/token",
                    formData);

                TokenResponseDto respuesta = JsonConvert.DeserializeObject<TokenResponseDto>(responsePostCheck.Item2) ?? throw new ApplicationException("No se pudo generar token");
                var expiresIn = respuesta.expires_in;

                // Guardar el token y su expiración
                _accessToken = respuesta.access_token;
                _tokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn - 60); // Restar 60s como margen de seguridad

                return _accessToken;
            }
            catch (Exception ex)
            {
                _logger.Error("Error al generar token:", ex);
                return string.Empty;
            }
        }

        public async Task ActualizarEstadoTicket(Tuple<bool, string> respuesta, Ticket ticket)
        {
            string mensaje = string.Empty;  
            if (respuesta.Item1)
            {
                mensaje = $"Ticket {ticket.NoTicket} enviado correctamente.";
                _logger.Info(mensaje);
                ticket.EstadoEnvio = EnumEstado.Enviado;
                ticket.RespuestaServicio = "OK";
            }
            else
            {
                mensaje = $"Error al enviar el ticket {ticket.NoTicket}: {respuesta.Item2}";
                _logger.Error(mensaje);
                ticket.EstadoEnvio = EnumEstado.Error;
                ticket.RespuestaServicio = mensaje;
            }

            string cadenaConexion = _configuration.GetValue("ServicioBackground:CadenaConexion", "");
            var connection = new MySqlConnection(cadenaConexion);

            var parametros = new DynamicParameters();
            parametros.Add("p_id", ticket.Id, DbType.Int32);
            parametros.Add("p_respuesta_servicio", mensaje, DbType.String);
            parametros.Add("p_estado_envio", (int)ticket.EstadoEnvio, DbType.Int32);

            var tickets = await connection.QueryAsync<Ticket>(
                "sp_actualizar_respuesta_servicio",
                parametros,
                commandType: CommandType.StoredProcedure
            );

        }

        public async Task InsertarLogTicket(Ticket ticket, Tuple<bool, string> respuesta,string tramaEnviada, int reintentos)
        {
            try
            {
                string mensaje = string.Empty;
                string cadenaConexion = _configuration.GetValue("ServicioBackground:CadenaConexion", "");
                var connection = new MySqlConnection(cadenaConexion);

                var parametros = new DynamicParameters();
                parametros.Add("p_ticket_id", ticket.Id, DbType.Int32);
                parametros.Add("p_trama_enviada", tramaEnviada, DbType.String);
                parametros.Add("p_respuesta_api", respuesta.Item2, DbType.String);
                parametros.Add("p_numero_reintentos", reintentos, DbType.Int32);
                parametros.Add("p_exito", respuesta.Item1 == true ? 1 : 0, DbType.Int32);

                var tickets = await connection.QueryAsync<Ticket>(
                    "sp_insertar_log_envio_api",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.Error("Error al guardar log envio:", ex);
            }
           

        }


    }
}
