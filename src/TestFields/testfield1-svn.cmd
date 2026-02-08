svn revert --remove-added -R Temp-svn
rem svn delete Temp-svn\Playground\ProjectMover.TestField1
svn delete Temp-svn\Playground\scratch
rem rmdir /s /q Temp-svn\Playground\ProjectMover.TestField1
rmdir /s /q Temp-svn\Playground\scratch
rem svn mkdir --parents Temp-svn\Playground
rem svn copy ProjectMover.TestField1 Temp-svn\Playground