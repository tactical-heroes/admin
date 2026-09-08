Edit the nearest component test project. Run all component tests for shared fixtures or cross-component changes.
Use MethodName_Should_DoSomething_When_Condition(); AAA flow without comments; English DisplayName.
Mirror every production Blazor component and component base in its project's ComponentTests folder and namespace as <ComponentName>Tests.cs.
Cover each declared method, including lifecycle overrides and private actions, with MethodName_Should_*_When_* tests. Use [Trait("Covers", "MethodName")] when an existing behavioral test also exercises another method; inherited behavior belongs in the declaring base's tests.
