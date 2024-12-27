namespace Internet_1.ViewModels
{
    public class ErrorResponse
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}