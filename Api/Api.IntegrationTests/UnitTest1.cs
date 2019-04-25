using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Api.IntegrationTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            RequestDelegate downstreamApiHandler = context =>
            {
                context.Response.StatusCode = 201;
                return context.Response.WriteAsync("");
            };

            using (var server = new ServerFixture().WithDownstreamApi(downstreamApiHandler))
            {
                var response = await server.Client.GetAsync("api/values");

                var body = await response.Content.ReadAsStringAsync();
                Assert.AreEqual("201", body);
            }
        }
    }
}
