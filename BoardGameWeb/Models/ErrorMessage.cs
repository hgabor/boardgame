using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Level14.BoardGameWeb.Models
{
    public class ErrorMessage
    {
        public readonly int errorCode;
        public readonly string error;
        private ErrorMessage(int code, string message)
        {
            errorCode = code;
            error = message;
        }

        public static readonly ErrorMessage NoApiKey = new ErrorMessage(code: 1, message: "No API key specified");
        public static readonly ErrorMessage InvalidApiKey = new ErrorMessage(code: 2, message: "Invalid API key");
    }
}