using System.Globalization;
using TacticalHeroes.Admin.Api.Serialization;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;
using TacticalHeroes.Admin.Shared.Model;
using ApiClaim = TacticalHeroes.Admin.Api.Generated.Models.Claim;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Users.Api;

public sealed class UsersApi(TacticalHeroesApiClient client)
{
    public async Task<PageResult<UserSummary>> GetPageAsync(
        int pageNumber,
        int pageSize,
        string? email,
        CancellationToken cancellationToken = default)
    {
        var response = await client.Api.V1.Users.GetAsync(
            request =>
            {
                request.QueryParameters.PageNumber = pageNumber.ToString(
                    CultureInfo.InvariantCulture);
                request.QueryParameters.PageSize = pageSize.ToString(
                    CultureInfo.InvariantCulture);
                request.QueryParameters.Email = string.IsNullOrWhiteSpace(email)
                    ? null
                    : email.Trim();
            },
            cancellationToken);

        if (response is null)
        {
            return PageResult<UserSummary>.Empty(pageNumber, pageSize);
        }

        var items = response.Items?
            .Where(user => user.Id.HasValue)
            .Select(user => new UserSummary(
                user.Id!.Value,
                user.Email ?? string.Empty,
                user.UserName ?? string.Empty,
                user.IsConfirmed ?? false,
                user.Status ?? string.Empty,
                user.StatusDisplayName ?? user.Status ?? string.Empty))
            .ToArray() ?? [];

        return new PageResult<UserSummary>(
            items,
            checked((int)Math.Max(response.PageNumber.ToInt64(), pageNumber)),
            checked((int)Math.Max(response.PageSize.ToInt64(), pageSize)),
            response.TotalCount.ToInt64(),
            checked((int)response.TotalPages.ToInt64()));
    }

    public async Task<UserDetails> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var response = await client.Api.V1.Users[id].GetAsync(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The users API returned an empty response.");

        return new UserDetails
        {
            Id = response.Id ?? id,
            Email = response.Email ?? string.Empty,
            UserName = response.UserName ?? string.Empty,
            IsConfirmed = response.IsConfirmed ?? false,
            Status = response.Status ?? string.Empty,
            StatusDisplayName = response.StatusDisplayName ?? response.Status ?? string.Empty,
            Claims = response.Claims?
                .Select(ToClaimValue)
                .ToList() ?? [],
        };
    }

    public async Task<IReadOnlyList<UserStatus>> GetStatusesAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await client.Api.V1.Users.Statuses.GetAsync(
            cancellationToken: cancellationToken);

        return response?
            .Where(status => !string.IsNullOrWhiteSpace(status.Name))
            .Select(status => new UserStatus(
                status.Name!,
                status.DisplayName ?? status.Name!))
            .ToArray() ?? [];
    }

    public async Task<Guid> CreateAsync(
        UserDetails user,
        CancellationToken cancellationToken = default)
    {
        var request = new CreateUserRequest
        {
            Email = user.Email.Trim(),
            UserName = user.UserName.Trim(),
            Password = user.Password,
            IsConfirmed = user.IsConfirmed,
            Status = user.Status,
            Claims = user.Claims.Select(ToApiClaim).ToList(),
        };
        var response = await client.Api.V1.Users.PostAsync(
            request,
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The users API returned an empty response.");

        return response.Id
            ?? throw new InvalidOperationException("The users API did not return the created identifier.");
    }

    public Task UpdateAsync(
        UserDetails user,
        CancellationToken cancellationToken = default)
    {
        if (user.Id == Guid.Empty)
        {
            throw new InvalidOperationException("A user identifier is required for update.");
        }

        var request = new UpdateUserRequest
        {
            Email = user.Email.Trim(),
            UserName = user.UserName.Trim(),
            IsConfirmed = user.IsConfirmed,
            Status = user.Status,
            Claims = user.Claims.Select(ToApiClaim).ToList(),
        };

        return client.Api.V1.Users[user.Id].PutAsync(
            request,
            cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return client.Api.V1.Users[id].DeleteAsync(
            cancellationToken: cancellationToken);
    }

    private static ClaimValue ToClaimValue(ApiClaim claim)
    {
        return new ClaimValue
        {
            Type = claim.Type ?? string.Empty,
            Value = claim.Value ?? string.Empty,
        };
    }

    private static ApiClaim ToApiClaim(ClaimValue claim)
    {
        return new ApiClaim
        {
            Type = claim.Type.Trim(),
            Value = claim.Value.Trim(),
        };
    }
}
