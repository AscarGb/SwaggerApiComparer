namespace ApiComparer.Lib.Models
{
    public class RequestHttpMethodParameter
    {
        public string Key { get; set; }
        public string In { get; set; }
        public string ParameterType { get; set; }
        public bool Required { get; set; }
    }
}