namespace AISmartReply.Models
{
    public class SmartReplyRequest
    {
        public string CustomerMessage { get; set; } = string.Empty;

        public string Tone { get; set; } = "Professional";
        public string ReplyLength { get; set; } = "Short";
    }
}
