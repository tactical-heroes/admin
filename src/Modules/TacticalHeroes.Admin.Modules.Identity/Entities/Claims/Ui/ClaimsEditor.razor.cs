using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Ui;

public partial class ClaimsEditor
{
    [Parameter, EditorRequired]
    public IList<ClaimValue> Claims { get; set; } = [];

    private void AddClaim()
    {
        Claims.Add(new ClaimValue());
    }

    private void RemoveClaim(ClaimValue claim)
    {
        Claims.Remove(claim);
    }
}
