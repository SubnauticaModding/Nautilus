# Contributing

## Preferred conventions and code style
* For commiting, we encourage [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/#summary).
* We encourage [access modifiers](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/access-modifiers) to be specified explicitly with no exceptions.
* Keep access modifiers as strict as possible (E.G: Do not default everything to `public` when they're not meant for public access).
* Unity messages (Awake, Start, Update etc..) should always be `private`.
* Remove unused `using` statements. Many IDEs offer shortcuts to perform this action.
* For namespaces, use [File Scoped Namespaces](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/file-scoped-namespaces).

## Setup
1. Download and install [.NET 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
2. Fork & clone the repository
3. Open the solution (.sln) file with your favorite IDE
4. Restore the dependencies by performing a NuGet restore via your IDE or by simply executing `dotnet restore` in a terminal
5. Make your changes, then submit a pull request.

## Submitting a Pull Request
We expect submitted PRs to be functional and thoroughly tested in the game. Please make sure of the following:
* Your changes do not break existing mods. Be it in a new world/save, or existing ones
* Follow our [Pull Request Template](.github/PULL_REQUEST_TEMPLATE.md)
* Run code refactoring automations to match our [preferred styles](#preferred-conventions-and-code-style).

If you are new to pull requests, read through [GitHub's Guide](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests).

## Docs Contribution
If you're looking to contribute to the [docs](https://subnauticamodding.github.io/Nautilus), refer to our [Docs Guidelines](Nautilus/Documentation/README.md).

