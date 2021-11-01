using System.Collections.Generic;

namespace ApiComparer.Lib.Models
{
    public class Definition
    {
        public string DefinitionType { get; set; }
        public IEnumerable<DefinitionProperty> DefinitionProperties { get; set; }
        public string Name { get; set; }
    }
}