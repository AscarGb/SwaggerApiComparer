using System.Collections.Generic;

namespace ApiComparer.Lib.Models
{
    public class RequestPathData
    {
        public IEnumerable<RequestPath> RequestPath { get; set; }
        public IEnumerable<Definition> Definitions { get; set; }
    }
}