# Архитектурные тесты

Проект содержит автоматические ограничения на структуру Blazor-приложения,
направление зависимостей Feature-Sliced Design, изоляцию модулей, маршрутизацию
и разделение Razor-разметки, C#-логики и стилей.

Запуск из корня репозитория:

```powershell
dotnet test tests/TacticalHeroes.Admin.ArchitectureTests/TacticalHeroes.Admin.ArchitectureTests.csproj
```

## Структура FSD

В `TacticalHeroes.Admin.Client` разрешены слои `App`, `Pages`, `Widgets`,
`Features` и `Entities`. В модульных Razor Class Libraries разрешены `Pages`,
`Widgets`, `Features` и `Entities`. Технические каталоги сборки, статические
ресурсы и корневые composition-файлы учитываются отдельно.

Направление зависимостей:

```text
App -> Pages -> Widgets -> Features -> Entities -> Api/Shared
```

1. `SourceFolders_Should_UseKnownLayers_When_ApplicationRootsAreScanned` — в
   Client и модулях запрещены альтернативные верхнеуровневые папки с
   исходниками. Новая продуктовая область должна размещаться внутри принятого
   FSD-слоя.

2. `SourceNamespaces_Should_MatchFolders_When_FsdSourcesAreScanned` — namespace
   каждого C#-исходника внутри FSD-слоя должен повторять физический путь файла.
   Директива `@namespace` в Razor-компонентах запрещена, чтобы разметка не могла
   обойти соглашение о структуре.

3. `SourceDependencies_Should_FollowLayerDirection_When_FsdSourcesAreScanned` —
   Client и модули могут зависеть только от нижележащих FSD-слоёв. Обратные
   зависимости, например `Entities -> Features` или `Widgets -> Pages`,
   запрещены.

4. `ModuleSources_Should_NotReferenceOtherModules_When_ModulesAreScanned` —
   исходники одного модуля не должны ссылаться на namespace другого модуля.
   Композиция нескольких модулей выполняется в Client или Host.

5. `PageSources_Should_NotAccessApiAdapters_When_PagesAreScanned` — компоненты
   из `Pages` не обращаются напрямую к entity API adapters и транспортным
   ошибкам. Page отвечает за маршрут и композицию, загрузка списков находится в
   `Widgets`, а пользовательские операции и формы — в `Features`.

6. `ModuleSources_Should_NotDependOnHigherLayers_When_ModulesAreScanned` —
   дополнительная проверка исходников модулей запрещает зависимости на более
   высокие слои по цепочке `Pages -> Widgets -> Features -> Entities`.

## Границы проектов и модулей

Разрешённый граф production-проектов:

```text
TacticalHeroes.Admin.Api       -> ничего
TacticalHeroes.Admin.Shared    -> ничего
Modules/*                      -> Api, Shared
TacticalHeroes.Admin.Client    -> Modules/*, Api, Shared
TacticalHeroes.Admin           -> Client, Modules/*, Shared
```

7. `ProjectReferences_Should_MatchAllowedDependencies_When_ProductionProjectsAreLoaded`
   — полный набор production-проектов и их прямые `ProjectReference` должны
   точно соответствовать разрешённому графу.

8. `ModuleReferences_Should_NotContainModules_When_ModuleProjectsAreLoaded` —
   любой модульный проект, включая добавленный в будущем, не может получить
   прямую ссылку на другой модульный проект.

9. `FoundationSources_Should_NotDependOnHigherProjects_When_SourcesAreScanned`
   — исходники `TacticalHeroes.Admin.Api` и `TacticalHeroes.Admin.Shared` не
   должны зависеть от Client или модулей.

## Маршруты и состояние страниц

10. `ComponentRoutes_Should_UseContracts_When_ApplicationComponentsAreScanned`
    — Razor-компоненты Client и модулей не объявляют сырые `@page` и не содержат
    внутренние URL-строки в `href`, `action` или `NavigateTo`. Маршруты задаются
    через типизированные route contracts.

11. `ListPages_Should_UseQueryParameters_When_ListStateIsDefined` — номер
    страницы и размер страницы списков фракций, ролей и пользователей читаются
    из query string через `SupplyParameterFromQuery`; email-фильтр пользователей
    также является query-параметром. Такое состояние можно восстановить при
    обновлении страницы или передать ссылкой.

## Razor-компоненты

12. `RazorMarkup_Should_NotContainCodeOrStyles_When_SourceIsScanned` — в
    production-файлах `.razor` запрещены блоки `@code`, `@functions`, директивы
    `@inject` и встроенные `<style>`. C#-логика хранится в `.razor.cs`, а
    изолированные стили — в `.razor.css`. Структурная директива Blazor
    `@inherits` разрешена.

13. `RazorCompanions_Should_HaveComponent_When_CompanionsAreScanned` — каждый
    `.razor.cs` должен иметь соответствующий `.razor` и объявлять partial class
    с именем компонента; каждый `.razor.css` также должен принадлежать
    существующему Razor-компоненту.

14. `RazorForms_Should_UseMudForm_When_FormsAreScanned` — интерактивные формы
    в production Razor используют `MudForm`; `EditForm` запрещён. Обычный HTML
    `<form>` допустим для серверной отправки, когда Blazor-валидация не участвует.

15. `RazorImports_Should_UseSeparateBlocks_When_SourceIsScanned` — цельный блок
    директив `@using` отделяется пустой строкой от структурных директив компонента
    и от Razor-разметки. Соседние директивы `@using` остаются одним блоком.

## Единый вид списков

16. `ListSurfaces_Should_UseSharedComponents_When_AdminListsAreScanned` — списки
    фракций, ролей и пользователей используют общий `EntityList` и контейнер
    `EntityRowActions` для произвольной композиции действий строки, не создавая
    собственные таблицы.

17. `ListWidgets_Should_NotExposeIdentifiers_When_AdminListsAreScanned` — в
    пользовательской разметке списков запрещены колонки с техническим ID.

18. `ListWidgets_Should_BindLoadErrors_When_AdminListsAreScanned` — списки
    передают в общий компонент фактическое состояние ошибки загрузки, а не
    строковый литерал.

19. `ListPages_Should_ExposeHeaderAndCreateAction_When_AdminListsAreScanned` —
    каждая страница списка содержит общий заголовок с пояснением и действие
    создания сущности.

## Конфигурационные options

20. `ConfigurationOptions_Should_HaveValidators_When_OptionsAreScanned` — каждый
    конфигурационный options-класс является `sealed`, находится в отдельной
    подпапке `Options/<name>`, совпадает с именем файла и имеет рядом валидатор
    `<OptionsType>Validator`, реализующий `IValidateOptions<T>`.

21. `ConfigurationOptions_Should_ValidateOnStart_When_RegistrationsAreScanned` —
    валидатор каждого options-класса зарегистрирован в DI, а сами настройки
    проверяются через `ValidateOnStart`.

## Модели

22. `ModelSources_Should_UsePropertyBasedClasses_When_ModelFoldersAreScanned` —
    production-исходники в подпапках `Model` не объявляют `record` или primary
    constructors. Формы, фильтры и read-модели используют единые parameterless
    классы с публичными свойствами.

23. `ModelSources_Should_HaveAdjacentValidators_When_ModelTypesAreScanned` —
    каждый тип `*Model` в production-подпапке `Model` имеет рядом валидатор
    `<ModelType>Validator`, наследующий `MudFormValidator<ModelType>`.

## Перечисления

24. `EnumerationMembers_Should_HaveExplicitNumericValues_When_SourceIsScanned`
    — каждый элемент production-enum имеет явно заданное целочисленное значение,
    чтобы добавление и перестановка элементов не меняли существующие значения.

25. `EnumerationMembers_Should_HaveEnglishDisplayNames_When_SourceIsScanned`
    — каждый элемент production-enum имеет непустой английский
    `[Display(Name = "...")]`, пригодный для единообразного отображения в UI.
