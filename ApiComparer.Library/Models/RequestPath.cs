using System.Collections.Generic;

namespace ApiComparer.Lib.Models
{
    public class RequestPath
    {
        public string Key { get; set; }
        public IEnumerable<RequestHttpMethod> Methods { get; set; }
    }
}