FROM mcr.microsoft.com/dotnet/nightly/aspnet
COPY . /publish
WORKDIR /publish
EXPOSE 80
ENTRYPOINT ["dotnet","DotNetCoreWebApi.dll"]