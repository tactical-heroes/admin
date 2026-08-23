using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Shared.Ui;
using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui;

public sealed class MudUpdateFormComponentBaseTests
{
    [Fact(DisplayName = "Loads once for the same route parameter and reloads for a new one")]
    public async Task OnParametersSetAsync_Should_LoadOnce_When_IdIsUnchanged()
    {
        var component = new TestComponent();
        Guid firstId = Guid.NewGuid();
        Guid secondId = Guid.NewGuid();

        await component.SetIdAsync(firstId);
        await component.SetIdAsync(firstId);
        await component.SetIdAsync(secondId);

        component.LoadedIds.ShouldBe([firstId, secondId]);
        component.Model.Id.ShouldBe(secondId);
    }

    [Fact(DisplayName = "Shows a load error and allows retrying the same route parameter")]
    public async Task ReloadAsync_Should_ClearError_When_RetrySucceeds()
    {
        var component = new TestComponent
        {
            OnLoad = static (id, _) => Task.FromResult(
                Result.Failure<TestModel>(Error.Failure($"Cannot load {id}."))),
        };
        Guid id = Guid.NewGuid();

        await component.SetIdAsync(id);

        component.Error.ShouldBe($"Cannot load {id}.");

        component.OnLoad = static (loadedId, _) => Task.FromResult(
            Result.Success(new TestModel { Id = loadedId }));
        await component.RetryAsync();

        component.Error.ShouldBeNull();
        component.Model.Id.ShouldBe(id);
    }

    [Fact(DisplayName = "Does not apply an obsolete load after the route parameter changes")]
    public async Task OnParametersSetAsync_Should_IgnoreObsoleteLoad_When_IdChanges()
    {
        var firstLoad = new TaskCompletionSource<Result<TestModel>>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var secondLoad = new TaskCompletionSource<Result<TestModel>>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        Guid firstId = Guid.NewGuid();
        Guid secondId = Guid.NewGuid();
        var component = new TestComponent
        {
            OnLoad = (id, _) => id == firstId ? firstLoad.Task : secondLoad.Task,
        };

        Task firstTask = component.SetIdAsync(firstId);
        Task secondTask = component.SetIdAsync(secondId);

        secondLoad.SetResult(Result.Success(new TestModel { Id = secondId }));
        await secondTask;
        firstLoad.SetResult(Result.Success(new TestModel { Id = firstId }));
        await firstTask;

        component.Model.Id.ShouldBe(secondId);
        component.Loading.ShouldBeFalse();
    }

    [Fact(DisplayName = "Stops loading when the load operation throws")]
    public async Task OnParametersSetAsync_Should_StopLoading_When_LoadThrows()
    {
        var component = new TestComponent
        {
            OnLoad = static (_, _) => throw new InvalidOperationException("Load failed."),
        };

        await Should.ThrowAsync<InvalidOperationException>(
            () => component.SetIdAsync(Guid.NewGuid()));

        component.Loading.ShouldBeFalse();
    }

    private sealed class TestComponent
        : MudUpdateFormComponentBase<TestModel, TestValidator>
    {
        private readonly TestOperations _operations;

        public TestComponent()
            : this(new TestOperations())
        {
        }

        private TestComponent(TestOperations operations)
            : base(
                operations.LoadAsync,
                TestOperations.UpdateAsync,
                "Saved",
                "/items",
                null!,
                null!)
        {
            _operations = operations;
        }

        public Func<Guid, CancellationToken, Task<Result<TestModel>>> OnLoad
        {
            get => _operations.OnLoad;
            set => _operations.OnLoad = value;
        }

        public List<Guid> LoadedIds => _operations.LoadedIds;

        public string? Error => LoadError;

        public bool Loading => IsLoading;

        public Task SetIdAsync(Guid id)
        {
            Id = id;
            return base.OnParametersSetAsync();
        }

        public Task RetryAsync()
        {
            return ReloadAsync();
        }

    }

    private sealed class TestOperations
    {
        public Func<Guid, CancellationToken, Task<Result<TestModel>>> OnLoad { get; set; } =
            static (id, _) => Task.FromResult(Result.Success(new TestModel { Id = id }));

        public List<Guid> LoadedIds { get; } = [];

        public Task<Result<TestModel>> LoadAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            LoadedIds.Add(id);
            return OnLoad(id, cancellationToken);
        }

        public static Task<Result<Guid>> UpdateAsync(
            Guid id,
            TestModel model,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success(id));
        }
    }

    private sealed class TestModel
    {
        public Guid Id { get; set; }
    }

    private sealed class TestValidator : MudFormValidator<TestModel>
    {
    }
}
