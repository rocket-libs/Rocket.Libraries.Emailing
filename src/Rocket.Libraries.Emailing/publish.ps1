dotnet test
if($LASTEXITCODE -eq 0)
{
	dotnet build
	dotnet pack
}