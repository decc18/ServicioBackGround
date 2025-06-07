using System.Text.Json.Serialization;

namespace ServicioBackground.DTO
{
    public class TokenResponseDto
    {
        public string access_token { get; set; }

        public string token_type { get; set; }

        public int expires_in { get; set; }

        public string scope { get; set; }
    }
}
