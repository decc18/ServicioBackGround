using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ServicioBackground.Util
{
    public static class UtilComun
    {
        public static bool EsJsonValido(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            texto = texto.Trim();

            // Debe empezar con { o [ para ser JSON válido
            if ((texto.StartsWith("{") && texto.EndsWith("}")) ||
                (texto.StartsWith("[") && texto.EndsWith("]")))
            {
                try
                {
                    var token = JToken.Parse(texto);
                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
            }

            return false;
        }
    }
}
