using System.Net;
using System.Net.Http;
using System.Text;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicMateAI.Web.Tests.Integrations;

public class IntegrationsPageTests : BunitContext
{
    private static readonly Guid DemoClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public IntegrationsPageTests()
    {
        Services.AddSingleton<ILogger<global::ClinicMateAI.Web.Components.Pages.Integrations>>(
            NullLogger<global::ClinicMateAI.Web.Components.Pages.Integrations>.Instance);
    }

    [Fact]
    public void Integrations_RendersChannelCards_FromApi()
    {
        var handler = new RoutingHttpMessageHandler(new Dictionary<string, HttpResponseMessage>
        {
            [$"/api/integrations/overview?clinicId={DemoClinicId}"] = JsonResponse("""
                [
                  {
                    "channel":"LINE",
                    "status":"Connected",
                    "summary":"Webhook พร้อมรับข้อความ",
                    "lastError":"",
                    "lastVerifiedAtUtc":"2026-05-16T09:30:00Z",
                    "isEnabled":true
                  },
                  {
                    "channel":"Facebook",
                    "status":"PendingVerification",
                    "summary":"รอยืนยันสิทธิ์เพจ",
                    "lastError":"",
                    "lastVerifiedAtUtc":null,
                    "isEnabled":false
                  }
                ]
                """)
        });

        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory(new HttpClient(handler)));

        var cut = Render<global::ClinicMateAI.Web.Components.Pages.Integrations>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("LINE OA");
            cut.Markup.Should().Contain("Facebook Messenger");
            cut.Markup.Should().Contain("Webhook พร้อมรับข้อความ");
            cut.Markup.Should().Contain("รอยืนยันสิทธิ์เพจ");
            cut.Markup.Should().Contain("เชื่อมต่อแล้ว");
            cut.Markup.Should().Contain("ดำเนินการต่อ");
        });
    }

    [Fact]
    public void Integrations_ShowsReconnectAction_WhenChannelRequiresReconnect()
    {
        var handler = new RoutingHttpMessageHandler(new Dictionary<string, HttpResponseMessage>
        {
            [$"/api/integrations/overview?clinicId={DemoClinicId}"] = JsonResponse("""
                [
                  {
                    "channel":"LINE",
                    "status":"Connected",
                    "summary":"พร้อมใช้งาน",
                    "lastError":"",
                    "lastVerifiedAtUtc":"2026-05-16T09:30:00Z",
                    "isEnabled":true
                  },
                  {
                    "channel":"Facebook",
                    "status":"ReconnectRequired",
                    "summary":"สิทธิ์การเชื่อมต่อหมดอายุ",
                    "lastError":"Permission revoked",
                    "lastVerifiedAtUtc":"2026-05-15T08:00:00Z",
                    "isEnabled":false
                  }
                ]
                """)
        });

        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory(new HttpClient(handler)));

        var cut = Render<global::ClinicMateAI.Web.Components.Pages.Integrations>();

         cut.WaitForAssertion(() =>
         {
             cut.Markup.Should().Contain("ต้องเชื่อมต่อใหม่");
             cut.Markup.Should().Contain("เชื่อมต่อใหม่");
             cut.Markup.Should().Contain("สิทธิ์การเชื่อมต่อหมดอายุ");
         });
     }

    [Fact]
    public void Integrations_RendersSecondaryActions_ForConnectedAndProblemStates()
    {
        var handler = new RoutingHttpMessageHandler(new Dictionary<string, HttpResponseMessage>
        {
            [$"/api/integrations/overview?clinicId={DemoClinicId}"] = JsonResponse("""
                [
                  {
                    "channel":"LINE",
                    "status":"Connected",
                    "summary":"พร้อมใช้งาน",
                    "lastError":"",
                    "lastVerifiedAtUtc":"2026-05-16T09:30:00Z",
                    "isEnabled":true
                  },
                  {
                    "channel":"Facebook",
                    "status":"ReconnectRequired",
                    "summary":"สิทธิ์การเชื่อมต่อหมดอายุ",
                    "lastError":"Permission revoked",
                    "lastVerifiedAtUtc":"2026-05-15T08:00:00Z",
                    "isEnabled":false
                  },
                  {
                    "channel":"Google Calendar",
                    "status":"Error",
                    "summary":"เชื่อมต่อไม่สำเร็จ",
                    "lastError":"API unavailable",
                    "lastVerifiedAtUtc":null,
                    "isEnabled":false
                  },
                  {
                    "channel":"Instagram",
                    "status":"PendingVerification",
                    "summary":"รอยืนยันการตั้งค่า",
                    "lastError":"",
                    "lastVerifiedAtUtc":null,
                    "isEnabled":false
                  }
                ]
                """)
        });

        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        }));

        var cut = Render<global::ClinicMateAI.Web.Components.Pages.Integrations>();

        cut.WaitForAssertion(() =>
        {
            var secondaryActions = cut.FindAll(".integration-card__actions .integration-button--ghost")
                .Select(button => button.TextContent.Trim());

            secondaryActions.Should().Equal("แก้ไขการตั้งค่า", "ดูปัญหา", "ดูปัญหา", "ดูขั้นตอน");
        });
    }

    [Fact]
    public void Integrations_SecondaryAction_OpensWizardModal_WithAriaLabelledBy()
    {
        var handler = new RoutingHttpMessageHandler(new Dictionary<string, HttpResponseMessage>
        {
            [$"/api/integrations/overview?clinicId={DemoClinicId}"] = JsonResponse("""
                [
                  {
                    "channel":"LINE",
                    "status":"Connected",
                    "summary":"พร้อมใช้งาน",
                    "lastError":"",
                    "lastVerifiedAtUtc":"2026-05-16T09:30:00Z",
                    "isEnabled":true
                  }
                ]
                """)
        });

        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        }));

        var cut = Render<global::ClinicMateAI.Web.Components.Pages.Integrations>();

        cut.WaitForAssertion(() =>
        {
            cut.Find(".integration-card__actions .integration-button--ghost").TextContent.Trim()
                .Should().Be("แก้ไขการตั้งค่า");
        });

        cut.Find(".integration-card__actions .integration-button--ghost").Click();

        cut.WaitForAssertion(() =>
        {
            var modal = cut.Find(".integration-modal[role='dialog']");
            var titleId = modal.GetAttribute("aria-labelledby");

            titleId.Should().NotBeNullOrWhiteSpace();
            cut.Find($"#{titleId}").TextContent.Should().Contain("LINE OA");
        });
    }

    [Fact]
    public void Integrations_UsesStyledStateShell_ForLoading()
    {
        var handler = new PendingHttpMessageHandler();
        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        }));

        var cut = Render<global::ClinicMateAI.Web.Components.Pages.Integrations>();

        cut.Find(".integration-state").TextContent.Should().Contain("กำลังโหลดข้อมูลการเชื่อมต่อ");
    }

    [Fact]
    public void Integrations_UsesStyledStateShell_ForErrors()
    {
        var handler = new RoutingHttpMessageHandler(new Dictionary<string, HttpResponseMessage>
        {
            [$"/api/integrations/overview?clinicId={DemoClinicId}"] = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        });

        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        }));

        var cut = Render<global::ClinicMateAI.Web.Components.Pages.Integrations>();

        cut.WaitForAssertion(() =>
        {
            var stateShell = cut.Find(".integration-state.integration-state--error");
            stateShell.TextContent.Should().Contain("ไม่สามารถโหลดข้อมูลการเชื่อมต่อได้");
        });
    }

    [Fact]
    public void Integrations_LogsLoadExceptions_WithClinicIdContext()
    {
        var logger = new TestLogger<global::ClinicMateAI.Web.Components.Pages.Integrations>();
        Services.AddSingleton<ILogger<global::ClinicMateAI.Web.Components.Pages.Integrations>>(logger);
        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory(new HttpClient(new ThrowingHttpMessageHandler())
        {
            BaseAddress = new Uri("http://localhost")
        }));

        var cut = Render<global::ClinicMateAI.Web.Components.Pages.Integrations>();

        cut.WaitForAssertion(() =>
        {
            var stateShell = cut.Find(".integration-state.integration-state--error");
            stateShell.TextContent.Should().Contain("ไม่สามารถโหลดข้อมูลการเชื่อมต่อได้");
        });

        logger.Entries.Should().ContainSingle(entry =>
            entry.Level == LogLevel.Error
            && entry.Exception is HttpRequestException
            && entry.Message.Contains(DemoClinicId.ToString()));
    }

    [Fact]
    public void Integrations_LineWizard_CanSaveAndTestSuccessfully()
    {
        var requestLog = new List<LoggedRequest>();
        var handler = new RoutingHttpMessageHandler(request =>
        {
            requestLog.Add(LoggedRequest.From(request));

            var path = request.RequestUri!.PathAndQuery;
            if (path == $"/api/integrations/overview?clinicId={DemoClinicId}"
                && requestLog.Count(x => x.PathAndQuery == path) == 1)
            {
                return JsonResponse("""
                    [
                      {
                        "channel":"LINE",
                        "status":"NotConnected",
                        "summary":"ยังไม่ได้ตั้งค่า LINE OA",
                        "lastError":"",
                        "lastVerifiedAtUtc":null,
                        "isEnabled":false
                      },
                      {
                        "channel":"Facebook",
                        "status":"NotConnected",
                        "summary":"ยังไม่ได้เชื่อมต่อเพจ",
                        "lastError":"",
                        "lastVerifiedAtUtc":null,
                        "isEnabled":false
                      }
                    ]
                    """);
            }

            if (path == $"/api/integrations/overview?clinicId={DemoClinicId}")
            {
                return JsonResponse("""
                    [
                      {
                        "channel":"LINE",
                        "status":"Connected",
                        "summary":"Webhook พร้อมรับข้อความ",
                        "lastError":"",
                        "lastVerifiedAtUtc":"2026-05-16T09:30:00Z",
                        "isEnabled":true
                      },
                      {
                        "channel":"Facebook",
                        "status":"NotConnected",
                        "summary":"ยังไม่ได้เชื่อมต่อเพจ",
                        "lastError":"",
                        "lastVerifiedAtUtc":null,
                        "isEnabled":false
                      }
                    ]
                    """);
            }

            if (path == "/api/integrations/line/save" && request.Method == HttpMethod.Post)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            if (path == "/api/integrations/line/test" && request.Method == HttpMethod.Post)
            {
                return JsonResponse("""{"isSuccess":true,"errorMessage":""}""");
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        }));

        var cut = Render<global::ClinicMateAI.Web.Components.Pages.Integrations>();
        cut.WaitForAssertion(() => cut.Markup.Should().Contain("LINE OA"));

        cut.FindAll(".integration-card__actions .integration-button")
            .First(button => button.TextContent.Trim() == "เชื่อมต่อ")
            .Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Webhook URL");
            cut.Markup.Should().Contain("Channel Secret");
            cut.Markup.Should().Contain("Channel Access Token");
        });

        var inputs = cut.FindAll(".integration-field input");
        inputs[0].Input("line-secret");
        inputs[1].Input("line-access-token");
        cut.FindAll(".integration-modal__actions .integration-button")
            .Single(button => button.TextContent.Trim() == "บันทึกและทดสอบ")
            .Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("เชื่อมต่อ LINE สำเร็จ");
            cut.Markup.Should().Contain("ทดสอบการเชื่อมต่อผ่านแล้ว");
            cut.Markup.Should().Contain("Webhook พร้อมรับข้อความ");
        });

        requestLog.Should().ContainSingle(x => x.PathAndQuery == "/api/integrations/line/save")
            .Which.Body.Should().Contain("\"clinicId\":\"11111111-1111-1111-1111-111111111111\"");
        requestLog.Should().ContainSingle(x => x.PathAndQuery == "/api/integrations/line/save")
            .Which.Body.Should().Contain("\"channelSecret\":\"line-secret\"");
        requestLog.Should().ContainSingle(x => x.PathAndQuery == "/api/integrations/line/test")
            .Which.Body.Should().Contain("\"clinicId\":\"11111111-1111-1111-1111-111111111111\"");
    }

    [Fact]
    public void Integrations_FacebookWizard_CanStartAndCompleteSuccessfully()
    {
        var requestLog = new List<LoggedRequest>();
        var handler = new RoutingHttpMessageHandler(request =>
        {
            requestLog.Add(LoggedRequest.From(request));

            var path = request.RequestUri!.PathAndQuery;
            if (path == $"/api/integrations/overview?clinicId={DemoClinicId}"
                && requestLog.Count(x => x.PathAndQuery == path) == 1)
            {
                return JsonResponse("""
                    [
                      {
                        "channel":"LINE",
                        "status":"NotConnected",
                        "summary":"ยังไม่ได้ตั้งค่า LINE OA",
                        "lastError":"",
                        "lastVerifiedAtUtc":null,
                        "isEnabled":false
                      },
                      {
                        "channel":"Facebook",
                        "status":"NotConnected",
                        "summary":"ยังไม่ได้เชื่อมต่อเพจ",
                        "lastError":"",
                        "lastVerifiedAtUtc":null,
                        "isEnabled":false
                      }
                    ]
                    """);
            }

            if (path == $"/api/integrations/overview?clinicId={DemoClinicId}")
            {
                return JsonResponse("""
                    [
                      {
                        "channel":"LINE",
                        "status":"NotConnected",
                        "summary":"ยังไม่ได้ตั้งค่า LINE OA",
                        "lastError":"",
                        "lastVerifiedAtUtc":null,
                        "isEnabled":false
                      },
                      {
                        "channel":"Facebook",
                        "status":"Connected",
                        "summary":"เชื่อมต่อเพจเรียบร้อย",
                        "lastError":"",
                        "lastVerifiedAtUtc":"2026-05-16T10:00:00Z",
                        "isEnabled":true
                      }
                    ]
                    """);
            }

            if (path == $"/api/integrations/facebook/start?clinicId={DemoClinicId}" && request.Method == HttpMethod.Get)
            {
                return JsonResponse($$"""{"authorizationUrl":"https://facebook.example/connect?clinicId={{DemoClinicId}}"}""");
            }

            if (path == "/api/integrations/facebook/complete" && request.Method == HttpMethod.Post)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory(new HttpClient(handler)));

        var cut = Render<global::ClinicMateAI.Web.Components.Pages.Integrations>();
        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Facebook Messenger"));

        cut.FindAll(".integration-card__actions .integration-button")
            .Last(button => button.TextContent.Trim() == "เชื่อมต่อ")
            .Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("เชื่อมต่อกับ Facebook");
            cut.Markup.Should().Contain("https://facebook.example/connect");
            cut.Markup.Should().Contain("Authorization Code");
        });

        cut.Find(".integration-field input").Input("demo-auth-code");
        cut.FindAll(".integration-modal__actions .integration-button")
            .Single(button => button.TextContent.Trim() == "ยืนยันการเชื่อมต่อ")
            .Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("เชื่อมต่อ Facebook สำเร็จ");
            cut.Markup.Should().Contain("เชื่อมต่อเพจเรียบร้อย");
        });

        requestLog.Should().Contain(x => x.PathAndQuery == $"/api/integrations/facebook/start?clinicId={DemoClinicId}");
        requestLog.Should().ContainSingle(x => x.PathAndQuery == "/api/integrations/facebook/complete")
            .Which.Body.Should().Contain("\"authorizationCode\":\"demo-auth-code\"");
    }

    [Fact]
    public void Integrations_FacebookDetails_DoesNotRestartOauth_ForConnectedChannel()
    {
        var requestLog = new List<LoggedRequest>();
        var handler = new RoutingHttpMessageHandler(request =>
        {
            requestLog.Add(LoggedRequest.From(request));

            var path = request.RequestUri!.PathAndQuery;
            if (path == $"/api/integrations/overview?clinicId={DemoClinicId}")
            {
                return JsonResponse("""
                    [
                      {
                        "channel":"LINE",
                        "status":"NotConnected",
                        "summary":"ยังไม่ได้ตั้งค่า LINE OA",
                        "lastError":"",
                        "lastVerifiedAtUtc":null,
                        "isEnabled":false
                      },
                      {
                        "channel":"Facebook",
                        "status":"Connected",
                        "summary":"เชื่อมต่อเพจเรียบร้อย",
                        "lastError":"",
                        "lastVerifiedAtUtc":"2026-05-16T10:00:00Z",
                        "isEnabled":true
                      }
                    ]
                    """);
            }

            if (path == $"/api/integrations/facebook/start?clinicId={DemoClinicId}")
            {
                return JsonResponse($$"""{"authorizationUrl":"https://facebook.example/connect?clinicId={{DemoClinicId}}"}""");
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory(new HttpClient(handler)));

        var cut = Render<global::ClinicMateAI.Web.Components.Pages.Integrations>();
        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Facebook Messenger"));

        cut.FindAll(".integration-card__actions .integration-button")
            .Single(button => button.TextContent.Trim() == "ดูรายละเอียด")
            .Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("เชื่อมต่อเพจเรียบร้อย");
            cut.Markup.Should().Contain("ลองดึงลิงก์อีกครั้ง");
        });

        requestLog.Should().ContainSingle(x => x.PathAndQuery == $"/api/integrations/overview?clinicId={DemoClinicId}");
        requestLog.Should().NotContain(x => x.PathAndQuery == $"/api/integrations/facebook/start?clinicId={DemoClinicId}");
    }

    [Fact]
    public void Integrations_Css_KeepsGhostButtonsStyled_OnHoverAndActive()
    {
        var cssPath = Path.Combine(GetRepositoryRoot(), "src", "ClinicMateAI.Web", "Components", "Pages", "Integrations.razor.css");
        var css = File.ReadAllText(cssPath);

        css.Should().Contain(".integration-button--ghost:hover");
        css.Should().Contain(".integration-button--ghost:active");
    }

    private static HttpResponseMessage JsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private sealed class TestHttpClientFactory(HttpClient httpClient) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => httpClient;
    }

    private sealed class RoutingHttpMessageHandler : HttpMessageHandler
    {
        private readonly IReadOnlyDictionary<string, HttpResponseMessage>? _responses;
        private readonly Func<HttpRequestMessage, HttpResponseMessage>? _responder;

        public RoutingHttpMessageHandler(IReadOnlyDictionary<string, HttpResponseMessage> responses)
        {
            _responses = responses;
        }

        public RoutingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_responder is not null)
            {
                return Task.FromResult(Clone(_responder(request)));
            }

            var key = request.RequestUri!.PathAndQuery;
            if (_responses is not null && _responses.TryGetValue(key, out var response))
            {
                return Task.FromResult(Clone(response));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        private static HttpResponseMessage Clone(HttpResponseMessage source)
        {
            var clone = new HttpResponseMessage(source.StatusCode);
            if (source.Content is not null)
            {
                var content = source.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                clone.Content = new StringContent(content, Encoding.UTF8, "application/json");
            }

            return clone;
        }
    }

    private sealed record LoggedRequest(string PathAndQuery, string Body)
    {
        public static LoggedRequest From(HttpRequestMessage request)
        {
            var body = request.Content is null
                ? string.Empty
                : request.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return new LoggedRequest(request.RequestUri!.PathAndQuery, body);
        }
    }

    private sealed class PendingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => new TaskCompletionSource<HttpResponseMessage>().Task;
    }

    private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new HttpRequestException("Simulated connection failure");
    }

    private sealed record LogEntry(LogLevel Level, string Message, Exception? Exception);

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ClinicMateAI.sln")))
        {
            directory = directory.Parent;
        }

        directory.Should().NotBeNull("the test should be running from somewhere under the repository root");
        return directory!.FullName;
    }
}
