Before contributing, make sure you have [.NET SDK](https://dotnet.microsoft.com/en-us/download) installed, then open a terminal and enter the following command to install docfx:
```powershell
dotnet tool update -g docfx
```
To build the website locally, run the following command:
```powershell
docfx Nautilus/docfx.json --serve
```

The website will then be launched on http://localhost:8080 and you can preview it.

## Contributing
Simply make a PR to the `docs` branch with your changes.
Make sure to add your documentation files (markdown) to the toc.yml appropriate for the type of document you're trying to add as well.  

**Please refrain from pushing C# changes to the `docs` branch.**