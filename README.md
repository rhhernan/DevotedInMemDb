# Devoted Health In Memory Database Technical Exercise

### Github Repo located at: https://github.com/rhhernan/DevotedInMemDb

----

### To build the application
1. Have .NET 6 SDK installed to provide the dotnet cli. The app is built in .NET 6.0
    - https://dotnet.microsoft.com/en-us/download/dotnet/6.0
2. Navigate to the project folder and open a terminal
3. Run the command
    - dotnet build DevotedInMemDb.csproj --runtime {your run time here} --no-self-contained
    - Valid runtimes are:
        - win-x64
        - win-x86
        - linux-x64
        - osx-x64
        - You can find more here: https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
4. The project should be built and you should be able to run it on your machine
    - The dll should be located in folders bin\Debug\net6.x or bin\Release\net6.x depending on your build target
    - You can run it by navigating to the newly built DevotedInMemDb.dll directory and running
        - dotnet ./DevotedInMemDb.dll
5. Run the application and input the commands to continue
6. Type HELP to see what commands the application has
7. You can run the TESTS command to see various unit tests the application has
---
### Application Files
The bulk of the application can be found in the following files
- InMemDb.cs
- Transaction.cs

The other files are supplementary to make things easier or to provide some definitions