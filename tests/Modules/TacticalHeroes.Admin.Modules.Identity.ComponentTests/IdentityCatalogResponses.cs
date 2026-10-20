using System.Net;
using System.Text;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests;

internal static class IdentityCatalogResponses
{
    public static readonly Guid RoleId = Guid.Parse("0f4d1e44-f544-4435-a6b0-24b6c4a2ca65");

    public static HttpResponseMessage? Create(HttpRequestMessage request)
    {
        if (request.Method != HttpMethod.Get)
        {
            return null;
        }

        string? json = request.RequestUri?.AbsolutePath switch
        {
            "/api/v1/claims/permissions" => """
                {"type":"permission","items":[
                  {"code":"identity.users.read","displayName":"Просмотр пользователей",
                   "moduleCode":"identity","moduleDisplayName":"Управление доступом","resourceCode":"users","resourceDisplayName":"Пользователи"}
                ]}
                """,
            "/api/v1/roles" => $$"""
                {"items":[{"id":"{{RoleId:D}}","name":"Moderator","isDefault":false}],
                 "pageNumber":1,"pageSize":100,"totalCount":1,"totalPages":1}
                """,
            _ => null,
        };

        return json is null ? null : new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
    }
}
