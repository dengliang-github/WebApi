FROM mcr.microsoft.com/dotnet/nightly/aspnet
COPY . /publish
WORKDIR /publish
#时区设置
RUN /bin/cp /usr/share/zoneinfo/Asia/Shanghai /etc/localtime \
&& echo ‘Asia/Shanghai’ >/etc/timezone

EXPOSE 80
ENTRYPOINT ["dotnet","DotNetCoreWebApi.dll"]