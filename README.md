![Frame 39266](https://github.com/user-attachments/assets/d742bc73-6b41-491c-9319-d2c3cd38a602)

# Cloning and Setting Up the Gml.Client Project

This guide will help you clone the `Gml.Client` project from GitHub, set up the development environment, and publish the
project.

## Prerequisites

Before you begin, ensure that you have the following software installed on your machine:

- [Git](https://git-scm.com/)
- .NET SDK (version 8.0)
- [JetBrains Rider](https://www.jetbrains.com/rider/)

## Cloning the Repository

1. Open a terminal.
2. Run the following command to clone the repository:

    ```sh
    git clone https://github.com/Gml-Launcher/Gml.Client.git
    ```

3. Navigate to the project directory:

    ```sh
    cd Gml.Client
    ```

## Setting Up the Development Environment

1. Open JetBrains Rider.
2. Open the cloned `Gml.Client` project in Rider:

    - Select `Open` from the welcome screen.
    - Navigate to the `Gml.Client` project directory and select it.

3. After the project is loaded, Rider will restore the necessary dependencies. This may take some time.

## Building the Project

1. Ensure your project target framework is correctly set to .NET 8.0. You can verify and set the target framework in the
   `.csproj` file(s) of your projects:

    ```xml
    <TargetFramework>net8.0</TargetFramework>
    ```

2. Build the project by selecting `Build > Build Solution` from the main menu or by pressing `Ctrl+Shift+B`.

## Running the Project

1. Ensure the correct startup configuration is selected (typically the main executable project).
2. Run the project by selecting `Run > Run` from the main menu or by pressing `Shift+F10`.

## Publishing the Project

1. Open a terminal.
2. Navigate to the project directory if not already there:

    ```sh
    cd Gml.Client
    ```

3. Run the publish command using the .NET CLI:

    ```sh
    dotnet publish -c Release -o ./publish
    ```

   This will publish the project in the `Release` configuration to the `./publish` directory.

## Contributing

If you'd like to contribute to the project, please fork the repository and create a pull request. Make sure your code
adheres to the project's coding standards and passes all the tests.

For any issues or feature requests, you can open an issue on
the [GitHub Issues](https://github.com/Gml-Launcher/Gml.Client/issues) page of the repository.

## Additional Resources

- [JetBrains Rider Documentation](https://www.jetbrains.com/help/rider/Introduction.html)
- [.NET Documentation](https://learn.microsoft.com/en-us/dotnet/)

By following the above steps, you should be able to set up, develop, and publish the `Gml.Client` project successfully.
Happy coding!
