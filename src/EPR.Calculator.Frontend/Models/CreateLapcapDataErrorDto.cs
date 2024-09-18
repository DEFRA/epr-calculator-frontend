namespace EPR.Calculator.Frontend.Models
{
    public class CreateLapcapDataErrorDto
    {
        public required string Message { get; set; }

        public string? Description { get; set; }

        public required string UniqueReference { get; set; }

        public required string Country { get; set; }

        public required string Material { get; set; }
    }
}
