using System;
using System.Net.Http;
using System.Threading.Tasks;
using ApiComparer.Lib;

namespace ApiComparer
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            using var client = new HttpClient();
            var net48ApiResult = await client.GetStringAsync("http://localhost:8015/swagger/docs/v1");
            var net5ApiResult = await client.GetStringAsync("https://localhost:5001/swagger/v1/swagger.json");

            var paths48 = Net48ApiParser.Create().Parse(net48ApiResult);
            var paths5 = Net5ApiParser.Create().Parse(net5ApiResult);

            var compareResult = SwaggerResultComparer.Create(true).Compare(paths48, paths5);

            foreach (var item in compareResult.CompareDefinitionsResult)
                Console.WriteLine(item);

            foreach (var item in compareResult.CompareRequestPathsResult)
                Console.WriteLine(item);
        }
    }
}