using System.Net;
using System.Net.Http;
using System.Text;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicMateAI.Web.Tests.Inbox;

public class InboxPageTests : BunitContext
{
    [Fact]
    public void Inbox_LoadsClinicsAndConversations_OnInitialize()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var handler = new RoutingHttpMessageHandler(new Dictionary<string, HttpResponseMessage>
        {
            ["/api/inbox/clinics"] = JsonResponse("""
                [{"clinicId":"11111111-1111-1111-1111-111111111111","name":"Demo Clinic"}]
                """),
            [$"/api/inbox/conversations?clinicId={clinicId}&take=50"] = JsonResponse("""
                [{"conversationId":"22222222-2222-2222-2222-222222222222","channel":"LINE","externalConversationId":"line-1","customerDisplayName":"Customer A"}]
                """)
        });

        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        }));

        var cut = Render<global::ClinicMateAI.Web.Components.Pages.Inbox>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Demo Clinic");
            cut.Markup.Should().Contain("Customer A");
            cut.Markup.Should().NotContain("Failed to load clinics");
        });
    }

    [Fact]
    public void Inbox_ShowsError_WhenClinicLoadFails()
    {
        var handler = new RoutingHttpMessageHandler(new Dictionary<string, HttpResponseMessage>
        {
            ["/api/inbox/clinics"] = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        });

        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        }));

        var cut = Render<global::ClinicMateAI.Web.Components.Pages.Inbox>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Failed to load clinics (500).");
        });
    }

    [Fact]
    public void Inbox_LoadsMessages_WhenConversationSelected()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var conversationId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var handler = new RoutingHttpMessageHandler(new Dictionary<string, HttpResponseMessage>
        {
            ["/api/inbox/clinics"] = JsonResponse("""
                [{"clinicId":"11111111-1111-1111-1111-111111111111","name":"Demo Clinic"}]
                """),
            [$"/api/inbox/conversations?clinicId={clinicId}&take=50"] = JsonResponse("""
                [{"conversationId":"22222222-2222-2222-2222-222222222222","channel":"LINE","externalConversationId":"line-1","customerDisplayName":"Customer A"}]
                """),
            [$"/api/inbox/conversations/{conversationId}/messages?clinicId={clinicId}"] = JsonResponse("""
                [{"senderType":"Customer","text":"Hello","sentAtUtc":"2026-05-15T10:00:00Z"}]
                """)
        });

        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        }));

        var cut = Render<global::ClinicMateAI.Web.Components.Pages.Inbox>();
        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Customer A"));

        cut.FindAll("button.list-group-item-action").First().Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Hello");
        });
    }

    [Fact]
    public void Inbox_ShowsError_WhenTakeIsOutOfRange()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var handler = new RoutingHttpMessageHandler(new Dictionary<string, HttpResponseMessage>
        {
            ["/api/inbox/clinics"] = JsonResponse("""
                [{"clinicId":"11111111-1111-1111-1111-111111111111","name":"Demo Clinic"}]
                """),
            [$"/api/inbox/conversations?clinicId={clinicId}&take=50"] = JsonResponse("[]")
        });

        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        }));

        var cut = Render<global::ClinicMateAI.Web.Components.Pages.Inbox>();
        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Demo Clinic"));

        cut.Find("#takeInput").Change("0");
        cut.Find("button.btn.btn-primary").Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Take must be between 1 and 200.");
        });
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

    private sealed class RoutingHttpMessageHandler(
        IReadOnlyDictionary<string, HttpResponseMessage> responses) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var key = request.RequestUri!.PathAndQuery;
            if (responses.TryGetValue(key, out var response))
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
}
