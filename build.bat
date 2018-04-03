echo off
cls

cd Frontend
echo ==  building frontend ==
call npm install
call npm run build

echo == copying to Backend ==
robocopy /S dist ..\Backend\wwwroot


echo == building backend ==
cd ../Backend
dotnet publish -c "Relase" -o ../website
pause