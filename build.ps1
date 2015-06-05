if (test-path .\build) { ri -r -fo .\build }
mkdir .\build\lib\4.0
msbuild /p:Configuration=Release
cp -r .\MoqBot\bin\Release\MoqBot.dll .\build\lib\4.0
.\Tools\nuget\NuGet.exe pack .\MoqBot.nuspec -BasePath .\build -o .\build
