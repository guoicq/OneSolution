using System;
using System.Net;

namespace OneSolution.Core.ErrorHandling
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }
    }
}
