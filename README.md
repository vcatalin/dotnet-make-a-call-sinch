# Make a TTS phone/PSTN call using Dotnet

This project initiates a TTS phone/PSTN call from a Sinch rented number and says something. An important aspect of this project is that the request is authenticated using Request Signing.

## Requirements

- dotnet 6 LTS
- newtonsoft.json NuGet

## Install

- replace the `_from` and `_to` numbers within the `Program.cs` file
_ replace the `_key` and `_secret` values within the `Program.cs` file with the corresponding values of your Voice App
- run `dotnet build` command
- run `dotnet run` command
