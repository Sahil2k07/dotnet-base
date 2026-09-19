.PHONY: build migrate run start clean

build:
	dotnet build Apps/DotnetBase.Server

migrate:
	cd Apps/DotnetBase.Migrator && dotnet run

run:
	cd Apps/DotnetBase.Server && dotnet run

start: build
	cd Apps/DotnetBase.Server && dotnet run --no-build

clean:
	dotnet clean