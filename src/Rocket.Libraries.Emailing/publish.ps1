dotnet test
if($LASTEXITCODE -eq 0)
{
	rm -r ./bin
	dotnet build
	dotnet pack
}