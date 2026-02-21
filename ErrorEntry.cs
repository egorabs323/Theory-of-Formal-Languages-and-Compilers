using System;

namespace YourNamespace
{
    public class ErrorEntry
    {
        public string Timestamp { get; set; }
        public string Level { get; set; }      
        public string Module { get; set; }   
        public string Message { get; set; }   
        public string ErrorCode { get; set; }  

        public ErrorEntry(string level, string module, string message, string errorCode = "")
        {
            Timestamp = DateTime.Now.ToString("HH:mm:ss");
            Level = level;
            Module = module;
            Message = message;
            ErrorCode = errorCode;
        }
    }
}