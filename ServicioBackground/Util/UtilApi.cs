using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace ServicioBackground.Util
{
    public static class UtilApi
    {
        public static async Task<Tuple<bool, string>> PostBodyAsync(string baseUrl, string requestUri, string jsonContent, AuthenticationHeaderValue authorization)
        {
            using HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
            return await PostBodyAsync(client, requestUri, jsonContent, authorization);
        }

        public static async Task<Tuple<bool, string>> PostBodyAsync(HttpClient client, string requestUri, string jsonContent, AuthenticationHeaderValue authorization)
        {
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            return await PostAsync(client, requestUri, content, authorization);
        }

        public static async Task<Tuple<bool, string>> PostAsync(HttpClient client, string requestUri, HttpContent content, AuthenticationHeaderValue? authorization)
        {
            try
            {
                if (authorization != null)
                    client.DefaultRequestHeaders.Authorization = authorization;

                HttpResponseMessage response = await client.PostAsync(requestUri, content);

                string contentResult = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    
                    if (UtilComun.EsJsonValido(contentResult))
                    {
                        return new Tuple<bool, string>(true, contentResult);
                    }
                    else
                    {
                        var mensaje = new
                        {
                            mensaje = contentResult
                        };

                        string json = JsonConvert.SerializeObject(mensaje);
                        return new Tuple<bool, string>(true, json);
                    }
                }
                else
                {
                    if (UtilComun.EsJsonValido(contentResult))
                    {
                        return new Tuple<bool, string>(false, contentResult);
                    }
                    else 
                    {
                        var mensaje = new
                        {
                            mensaje = contentResult
                        };

                        string json = JsonConvert.SerializeObject(mensaje);
                        return new Tuple<bool, string>(false, json);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        public static async Task<Tuple<bool, string>> PostFormUrlEncodedAsync(
            string baseUrl,
            string requestUri,
            Dictionary<string, string> formData)
        {
            try
            {
                using HttpClient client = new HttpClient
                {
                    BaseAddress = new Uri(baseUrl)
                };

                // Limpia headers previos para evitar conflictos
                client.DefaultRequestHeaders.Clear();

                var content = new FormUrlEncodedContent(formData);

                HttpResponseMessage response = await client.PostAsync(requestUri, content);

                string contentResult = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    if (UtilComun.EsJsonValido(contentResult))
                    {
                        return new Tuple<bool, string>(true, contentResult);
                    }
                    else
                    {
                        var mensaje = new
                        {
                            mensaje = contentResult
                        };

                        string json = JsonConvert.SerializeObject(mensaje);
                        return new Tuple<bool, string>(true, json);
                    }
                }
                else
                {
                    if (UtilComun.EsJsonValido(contentResult))
                    {
                        return new Tuple<bool, string>(false, contentResult);
                    }
                    else
                    {
                        var mensaje = new
                        {
                            mensaje = contentResult
                        };
                        string json = JsonConvert.SerializeObject(mensaje);
                        return new Tuple<bool, string>(false, json);
                    }
                }
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(false, $"Exception: {ex.Message}");
            }
        }
    }
}
