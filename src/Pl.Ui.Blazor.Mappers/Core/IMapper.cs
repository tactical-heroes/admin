namespace Pl.Ui.Blazor.Mappers.Core;

public interface IMapper<TViewModel, TDto>
{
    static abstract TViewModel ToViewModel(TDto dto);
    static abstract IEnumerable<TViewModel> ToViewModel(IEnumerable<TDto> dtos);
    static abstract TDto ToDto(TViewModel viewModel);
    static abstract IEnumerable<TDto> ToDto(IEnumerable<TViewModel> viewModels);
}
