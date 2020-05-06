using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Worms.Server.Auth
{
    internal class LoopbackHttpListener : IDisposable
    {
        private const int _defaultTimeout = 60 * 5;

        private readonly IWebHost _host;
        private readonly TaskCompletionSource<string> _source = new TaskCompletionSource<string>();

        public LoopbackHttpListener(string redirectUri)
        {
            _host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(redirectUri)
                .Configure(Configure)
                .Build();
            _host.Start();
        }

        public void Dispose()
        {
            Task.Run(async () =>
            {
                await Task.Delay(500);
                _host.Dispose();
            });
        }

        private void Configure(IApplicationBuilder app)
        {
            app.Run(async ctx =>
            {
                switch (ctx.Request.Method)
                {
                    case "GET":
                        SetResult(ctx.Request.QueryString.Value, ctx);
                        break;
                    case "POST" when !ctx.Request.ContentType.Equals("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase):
                        ctx.Response.StatusCode = 415;
                        break;
                    case "POST":
                    {
                        using var sr = new StreamReader(ctx.Request.Body, Encoding.UTF8);
                        var body = await sr.ReadToEndAsync();
                        SetResult(body, ctx);
                        break;
                    }
                    default:
                        ctx.Response.StatusCode = 405;
                        break;
                }
            });
        }

        private void SetResult(string value, HttpContext ctx)
        {
            try
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "text/html";
                ctx.Response.WriteAsync("<h1>Successfully logged in.</h1>");
                ctx.Response.Body.Flush();

                _source.TrySetResult(value);
            }
            catch
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.ContentType = "text/html";
                ctx.Response.WriteAsync("<h1>Invalid request.</h1>");
                ctx.Response.Body.Flush();
            }
        }

        public Task<string> WaitForCallbackAsync(int timeoutInSeconds = _defaultTimeout)
        {
            Task.Run(async () =>
            {
                await Task.Delay(timeoutInSeconds * 1000);
                _source.TrySetCanceled();
            });

            return _source.Task;
        }
    }
}
