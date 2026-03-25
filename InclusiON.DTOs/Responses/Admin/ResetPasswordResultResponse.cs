namespace InclusiON.DTOs.Responses.Admin
{
    public class ResetPasswordResultResponse
    {
        public string TemporaryPassword { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }
}
