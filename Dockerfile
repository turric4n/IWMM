FROM mcr.microsoft.com/dotnet/aspnet:6.0
EXPOSE 80/tcp
WORKDIR /app
COPY . ./
RUN apt-get update && \
    apt-get install -y --allow-unauthenticated libc6-dev libcurl3-gnutls
ENTRYPOINT ["./iwmm", "-c", "config/iwmm.yml"]