Edit the nearest component test project. Run all component tests for shared fixtures or cross-component changes.
Use MethodName_Should_DoSomething_When_Condition(); AAA flow without comments; English DisplayName.
Mirror every production Blazor component and component base in its project's ComponentTests folder and namespace as <ComponentName>Tests.cs.
Cover each method declared by the component or base class, including lifecycle overrides and private actions, with MethodName_Should_*_When_* tests. Inherited methods belong only in the declaring base's tests; overrides belong in the overriding class's tests. Exercise private methods through observable behavior.
