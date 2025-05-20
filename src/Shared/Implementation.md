# Shared Library Implementation

1. Create a .NET class library:
```bash
dotnet new classlib -n Shared
```

2. Define data transfer objects such as `LicenseInfo`, `UserAccount`, and helpers for offline token validation.

3. Unit test the library from the `tests` folder:
```bash
dotnet test
```
The library should remain free of external dependencies so it can be referenced by all other projects in this solution.
