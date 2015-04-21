@echo off

@echo Installing "IIS Client Certificate Mapping Authentication" server role 
powershell -ExecutionPolicy Unrestricted -command "Install-WindowsFeature Web-Cert-Auth"