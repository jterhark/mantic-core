# Prerequisites
- Familiarity with how the Mantic Framework API works
- Familiarity with both the caller and callee code for the API
- Account level access to the `divvy` database on `appdev`

# Preparation
1. Add tests that relate the the feature you have added.
2. Verify ALL tests are passing before continuing.
3. Determine if this is a debug or release build.
4. Increment nuget package version following the incremental versioning guidlines 

# Building Nuget Package
### Debug

1. Clean solution
    ```shell
    dotnet clean
    ```
    
2. Build solution
    ```shell
    dotnet build
    ```
    
3. Publish to nuget package
    ```shell
    dotnet pack --include-source --include-symbols
    ```
    
4. Move the following files from `ManticFramework/bin/Debug` to `S:\SATechnology\Projects\Development\Nuget Packages`
    - `ManticFramework.X.X.X.nupkg`
    - `ManticFramework.X.X.X.symbols.nupkg`
    
### Release

1. Clean solution
    ```shell
    dotnet clean -c Release
    ```

2. build solution
    ```shell
    dotnet build -c Release
    ```

3. publish to nuget package
    ```shell
    dotnet pack -c Release
    ```
 4. Move `ManticFramework/bin/Release/ManticFramework.X.X.X.nupkg` to `S:\SATechnology\Projects\Development\Nuget Packages`