namespace UserManagement.Service.Results
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }

        public static ValidationResult Success() => new() { IsValid = true };
        public static ValidationResult Fail(string message) => new() { IsValid = false, ErrorMessage = message };
    }
}

