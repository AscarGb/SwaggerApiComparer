using System.Collections.Generic;

namespace ApiComparer.Lib
{
    public class CompareResult
    {
        public List<string> CompareDefinitionsResult { get; set; }
        public List<string> CompareRequestPathsResult { get; set; }
    }
}