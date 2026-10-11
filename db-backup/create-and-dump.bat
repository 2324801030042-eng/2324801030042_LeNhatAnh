@echo off
REM Tao cluster + CSDL lab + backup (chay sau khi PostgreSQL 17 cai xong)
REM Yeu cau: chay voi quyen admin, PostgreSQL bin da co trong PATH
setlocal
set PGDATA=C:\PGDATA
set PGBIN=C:\Program Files\PostgreSQL\17\bin

if not exist "%PGDATA%" (
  "%PGBIN%\initdb.exe" -D "%PGDATA%" -U postgres -E UTF8 --auth=trust
)
"%PGBIN%\pg_ctl.exe" -D "%PGDATA%" -l "%PGDATA%\server.log" start
timeout /t 5 >nul
psql -U postgres -h localhost -f ..\postgres\createdatabases.sql
for %%D in (payments jobs policy pricing) do (
  "%PGBIN%\pg_dump.exe" -U postgres -h localhost -Fp -f lab_netmicro_%%D.sql lab_netmicro_%%D
)
echo OK - backup files:
dir *.sql
