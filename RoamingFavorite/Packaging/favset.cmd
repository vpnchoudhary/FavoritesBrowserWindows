@ECHO off

SET _ROAMINGFAVORITESFOLDER=%1
if "%_ROAMINGFAVORITESFOLDER%"=="" SET _ROAMINGFAVORITESFOLDER=%USERPROFILE%\Skydrive\RoamingFavoritesApp\Bookmark

FOR /f "tokens=3" %%f IN ('reg query "HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders" /v Favorites /reg:32') DO (
    SET _FAVORITESFOLDER=%%f
)

ECHO Copying favorites folder from %_FAVORITESFOLDER% to %_ROAMINGFAVORITESFOLDER%....

call robocopy %_FAVORITESFOLDER% %_ROAMINGFAVORITESFOLDER% /MIR >> null

reg add "HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders" /v Favorites /t REG_SZ /d %_ROAMINGFAVORITESFOLDER% /f /reg:32 

ECHO Favorite setup done, please launch Favorites Browser!


