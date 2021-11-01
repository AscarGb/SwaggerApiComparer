namespace ApiComparer.Lib.Models
{
    public class DefinitionProperty
    {
        public string PropertyName { get; set; }
        public string PropertyFormat { get; set; }
        public string PropertyType { get; set; }
        public string PropertyRefType { get; set; }
        public bool Nullable { get; set; }
        public string ArrayRefType { get; set; }
    }
}