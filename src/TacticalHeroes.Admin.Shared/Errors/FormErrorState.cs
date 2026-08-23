using MudBlazor;

using PANiXiDA.Core.ResultPattern;

namespace TacticalHeroes.Admin.Shared.Errors;

public sealed class FormErrorState<TModel>
{
    private IReadOnlyDictionary<string, string[]> _errors =
        new Dictionary<string, string[]>();

    public void Clear()
    {
        _errors = new Dictionary<string, string[]>();
    }

    public void Handle(IReadOnlyList<Error> errors, ISnackbar snackbar)
    {
        _errors = ApiErrorMessage.GetFieldErrors<TModel>(errors);
        IReadOnlyList<Error> unhandledErrors =
            ApiErrorMessage.GetUnhandledErrors<TModel>(errors);

        if (unhandledErrors.Count > 0)
        {
            snackbar.Add(ApiErrorMessage.FromErrors(unhandledErrors), Severity.Error);
        }
    }

    public bool HasError(string field)
    {
        return _errors.ContainsKey(field);
    }

    public string? GetError(string field)
    {
        return _errors.TryGetValue(field, out string[]? messages)
            ? string.Join(" ", messages)
            : null;
    }
}
