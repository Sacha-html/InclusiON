namespace InclusiON.DTOs.Responses.Family
{
    /// <summary>
    /// Informacion basica de una persona vinculada a un representante familiar.
    /// </summary>
    public class LinkedPersonInfo
    {
        public Guid PersonId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? DisabilityType { get; set; }
        public bool IsPrimary { get; set; }
        public string? Relationship { get; set; }
    }
}
