using System.Collections.Generic;

namespace ApiComparer.Lib.Models
{
    public class RequestHttpMethod
    {
        public string Key { get; set; }
        public IEnumerable<RequestHttpMethodParameter> Parameters { get; set; }
        public IEnumerable<ResponseStatusCode> Responses { get; set; }
    }
}