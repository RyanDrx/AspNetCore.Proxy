using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AspNetCore.Proxy.Tests
{
    public class UnitTests
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public UnitTests()
        {
            _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task CanProxyAttributeToTask()
        {
            var response = await _client.GetAsync("api/posts/totask/1");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("sunt aut facere repellat provident occaecati excepturi optio reprehenderit", JObject.Parse(responseString).Value<string>("title"));
        }

        [Fact]
        public async Task CanProxyAttributeToString()
        {
            var response = await _client.GetAsync("api/posts/tostring/1");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("sunt aut facere repellat provident occaecati excepturi optio reprehenderit", JObject.Parse(responseString).Value<string>("title"));
        }

        [Fact]
        public async Task CanProxyAttributePostRequest()
        {
            var content = new StringContent("{\"title\": \"foo\", \"body\": \"bar\", \"userId\": 1}", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("api/posts", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("101", JObject.Parse(responseString).Value<string>("id"));
        }


        [Fact]
        public async Task CanProxyContentHeadersPostRequest()
        {
            var content = "hello world";
            var contentType = "application/xcustom";

            var stringContent = new StringContent(content);
            stringContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            var response = await _client.PostAsync("echo/post", stringContent);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Equal(content, JObject.Parse(responseString).Value<string>("data"));
            Assert.Equal(contentType, JObject.Parse(responseString)["headers"]["content-type"]);
            Assert.Equal(content.Length, JObject.Parse(responseString)["headers"]["content-length"]);
        }


        [Fact]
        public async Task CanProxyAttributePostWithFormRequest()
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string> { { "xyz", "123" }, { "abc", "321" } });
            var response = await _client.PostAsync("api/posts", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseString);

            Assert.Contains("101", json.Value<string>("id"));
            Assert.Equal("123", json["xyz"]);
            Assert.Equal("321", json["abc"]);
        }

        [Fact]
        public async Task CanProxyAttributeCatchAll()
        {
            var response = await _client.GetAsync("api/catchall/posts/1");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("sunt aut facere repellat provident occaecati excepturi optio reprehenderit", JObject.Parse(responseString).Value<string>("title"));
        }

        [Fact]
        public async Task CanProxyMiddlewareWithContextAndArgsToTask()
        {
            var response = await _client.GetAsync("api/comments/contextandargstotask/1");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("id labore ex et quam laborum", JObject.Parse(responseString).Value<string>("name"));
        }

        [Fact]
        public async Task CanProxyMiddlewareWithArgsToTask()
        {
            var response = await _client.GetAsync("api/comments/argstotask/1");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("id labore ex et quam laborum", JObject.Parse(responseString).Value<string>("name"));
        }

        [Fact]
        public async Task CanProxyMiddlewareWithEmptyToTask()
        {
            var response = await _client.GetAsync("api/comments/emptytotask");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("id labore ex et quam laborum", JObject.Parse(responseString).Value<string>("name"));
        }

        [Fact]
        public async Task CanProxyMiddlewareWithContextAndArgsToString()
        {
            var response = await _client.GetAsync("api/comments/contextandargstostring/1");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("id labore ex et quam laborum", JObject.Parse(responseString).Value<string>("name"));
        }

        [Fact]
        public async Task CanProxyMiddlewareWithArgsToString()
        {
            var response = await _client.GetAsync("api/comments/argstostring/1");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("id labore ex et quam laborum", JObject.Parse(responseString).Value<string>("name"));
        }

        [Fact]
        public async Task CanProxyMiddlewareWithEmptyToString()
        {
            var response = await _client.GetAsync("api/comments/emptytostring");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("id labore ex et quam laborum", JObject.Parse(responseString).Value<string>("name"));
        }

        [Fact]
        public async Task CanProxyWithController()
        {
            var response = await _client.GetAsync("api/controller/posts/1");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("sunt aut facere repellat provident occaecati excepturi optio reprehenderit", JObject.Parse(responseString).Value<string>("title"));
        }

        [Fact]
        public async Task CanModifyRequest()
        {
            var response = await _client.GetAsync("api/controller/customrequest/1");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("qui est esse", JObject.Parse(responseString).Value<string>("title"));
        }

        [Fact]
        public async Task CanModifyResponse()
        {
            var response = await _client.GetAsync("api/controller/customresponse/1");
            response.EnsureSuccessStatusCode();
            
            Assert.Equal("It's all greek...er, Latin...to me!", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task CanModifyBadResponse()
        {
            var response = await _client.GetAsync("api/controller/badresponse/1");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            
            Assert.Equal("I tried to proxy, but I chose a bad address, and it is not found.", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task CanGetGeneric502OnFailure()
        {
            var response = await _client.GetAsync("api/controller/fail/1");
            Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        }

        [Fact]
        public async Task CanGetCustomFailure()
        {
            var response = await _client.GetAsync("api/controller/customfail/1");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal("Things borked.", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task CanProxyConcurrentCalls()
        {
            var calls = Enumerable.Range(0, 1000).Select(i =>
            {
                return _client.GetAsync($"api/controller/posts/{i % 100 + 1}");
            });

            Assert.True((await Task.WhenAll(calls)).All(r => r.IsSuccessStatusCode));
        }
    }
}
