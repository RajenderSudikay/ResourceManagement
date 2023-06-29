namespace ResourceManagement.Models.Email
{
    public class SendEmail
    {
        public string From { get; set; }
        public string To { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string Subject { get; set; }
        public string EmailBody { get; set; }
        public JsonResponseModel JsonResponse { get; set; } = new JsonResponseModel();
        public dynamic inputObject { get; set; }
        public string SpecificUserName { get; set; }
        public string SpecificPassword { get; set; }
    }
}