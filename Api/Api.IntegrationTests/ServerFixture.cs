using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;

namespace Api.IntegrationTests
{
    public class ServerFixture : IDisposable
    {
        private TestServer _server;
        private IWebHost _downstreamApiHost;

        public ServerFixture()
        {
            Url = $"http://localhost:{GetNextPort()}";
            DownstreamApiUrl = $"http://localhost:{GetNextPort()}";

            StartServer();
        }

        private void StartServer()
        {
            var builder = Program.CreateWebHostBuilder(new string[0])
                            .UseUrls(Url)
                            .ConfigureAppConfiguration(config =>
                            {
                                config.AddInMemoryCollection(GetOverriddenConfig());
                            });

            _server = new TestServer(builder);
            Client = _server.CreateClient();
            Client.BaseAddress = new Uri(Url);
        }

        private Dictionary<string, string> GetOverriddenConfig()
        {
            return new Dictionary<string, string>
            {
                ["DownstreamApi:Url"] = DownstreamApiUrl
            };
        }

        public string Url { get; }
        public string DownstreamApiUrl { get; }
        public HttpClient Client { get; private set; }

        public ServerFixture WithDownstreamApi(RequestDelegate mockServerHandler)
        {
            _downstreamApiHost = HostMockApi(mockServerHandler);
            _downstreamApiHost.StartAsync();
            return this;
        }

        private IWebHost HostMockApi(RequestDelegate mockServerHandler)
        {
            return WebHost.CreateDefaultBuilder()
                            .Configure(p => p.Run(mockServerHandler))
                            .UseUrls(DownstreamApiUrl)
                            .UseKestrel()
                            .Build();
        }

        private int GetNextPort()
        {
            using (var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(new IPEndPoint(IPAddress.IPv6Loopback, 0));
                return ((IPEndPoint)socket.LocalEndPoint).Port;
            }
        }

        public void Dispose()
        {
            _server?.Dispose();
            _server = null;

            _downstreamApiHost?.StopAsync();
            _downstreamApiHost?.Dispose();
            _downstreamApiHost = null;
        }
    }
}
