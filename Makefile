api/dev:
	dotnet watch run --project ./src/Opengate/Opengate.csproj

api/run:
	dotnet run --project ./src/Opengate/Opengate.csproj

env/gen:
	dotnet anv generate --output ./src/Opengate/
