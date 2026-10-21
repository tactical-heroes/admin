using System.Net;
using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.DependencyInjection;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.RoleListPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Api;

public sealed class RequiredCollectionMappingTests : IDisposable
{
    private static readonly Guid EntityId = Guid.Parse("01992fa0-0000-7000-8000-000000000001");
    private readonly ResponseHandler _handler = new();
    private readonly ServiceProvider _services;

    public RequiredCollectionMappingTests()
    {
        var services = new ServiceCollection();
        services.AddTacticalHeroesApiClient(
            static _ => new Uri("https://api.example.test"),
            static _ => TimeSpan.FromSeconds(30));
        services.Configure<HttpClientFactoryOptions>("TacticalHeroesApi",
            options => options.HttpMessageHandlerBuilderActions.Add(builder => builder.PrimaryHandler = _handler));
        services.AddIdentityAdminModule();
        _services = services.BuildServiceProvider();
    }

    [Theory(DisplayName = "GetAsync should reject response when required collection is null")]
    [InlineData("user-roles")]
    [InlineData("user-claims")]
    [InlineData("role-claims")]
    [InlineData("users")]
    [InlineData("roles")]
    [InlineData("permissions")]
    public async Task GetAsync_Should_RejectResponse_When_RequiredCollectionIsNull(string mapping)
    {
        _handler.Body = mapping switch
        {
            "user-roles" => """{"email":"user@example.test","userName":"User","isConfirmed":true,"status":"active","roleIds":null,"claims":[]}""",
            "user-claims" => """{"email":"user@example.test","userName":"User","isConfirmed":true,"status":"active","roleIds":[],"claims":null}""",
            "role-claims" => """{"name":"Role","isDefault":false,"claims":null}""",
            "permissions" => """{"type":"permission","items":null}""",
            _ => """{"items":null,"pageNumber":1,"pageSize":20,"totalCount":0,"totalPages":0}""",
        };

        await Should.ThrowAsync<ArgumentNullException>(() => LoadAsync(mapping));
        _handler.RequestCount.ShouldBe(1);
    }

    [Theory(DisplayName = "SaveAsync should reject model before sending when required collection is null")]
    [InlineData("create-user-roles")]
    [InlineData("create-user-claims")]
    [InlineData("update-user-roles")]
    [InlineData("update-user-claims")]
    [InlineData("create-role-claims")]
    [InlineData("update-role-claims")]
    public async Task SaveAsync_Should_RejectModelBeforeSending_When_RequiredCollectionIsNull(string mapping)
    {
        Task Save() => SaveAsync(mapping);

        await Should.ThrowAsync<ArgumentNullException>(Save);
        _handler.RequestCount.ShouldBe(0);
    }

    public void Dispose() => _services.Dispose();

    private Task LoadAsync(string mapping)
    {
        var token = Xunit.TestContext.Current.CancellationToken;
        return mapping switch
        {
            "user-roles" or "user-claims" => _services.GetRequiredService<UpdateUserApi>().GetAsync(EntityId, token),
            "role-claims" => _services.GetRequiredService<UpdateRoleApi>().GetAsync(EntityId, token),
            "users" => _services.GetRequiredService<UserListApi>().GetPageAsync(1, 20, new UserListFilter(), [], token),
            "roles" => _services.GetRequiredService<RoleListApi>().GetPageAsync(1, 20, [], token),
            "permissions" => _services.GetRequiredService<ClaimCatalogApi>().GetPermissionsAsync(token),
            _ => throw new ArgumentOutOfRangeException(nameof(mapping)),
        };
    }

    private Task<Result<Guid>> SaveAsync(string mapping)
    {
        var token = Xunit.TestContext.Current.CancellationToken;
        return mapping switch
        {
            "create-user-roles" => _services.GetRequiredService<CreateUserApi>().CreateAsync(new CreateUserFormModel { RoleIds = null! }, token),
            "create-user-claims" => _services.GetRequiredService<CreateUserApi>().CreateAsync(new CreateUserFormModel { Claims = null! }, token),
            "update-user-roles" => _services.GetRequiredService<UpdateUserApi>().UpdateAsync(EntityId, new UpdateUserFormModel { RoleIds = null! }, token),
            "update-user-claims" => _services.GetRequiredService<UpdateUserApi>().UpdateAsync(EntityId, new UpdateUserFormModel { Claims = null! }, token),
            "create-role-claims" => _services.GetRequiredService<CreateRoleApi>().CreateAsync(new CreateRoleFormModel { Claims = null! }, token),
            "update-role-claims" => _services.GetRequiredService<UpdateRoleApi>().UpdateAsync(EntityId, new UpdateRoleFormModel { Claims = null! }, token),
            _ => throw new ArgumentOutOfRangeException(nameof(mapping)),
        };
    }

    private sealed class ResponseHandler : HttpMessageHandler
    {
        public string Body { get; set; } = "{}";

        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Body, Encoding.UTF8, "application/json"),
            });
        }
    }
}
