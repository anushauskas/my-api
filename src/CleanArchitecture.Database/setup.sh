#!/bin/bash
for i in {1..50};
do
    /opt/mssql-tools/bin/sqlcmd -S $TARGET_SERVER_NAME -U $TARGET_USER -P $TARGET_PASSWORD -d master -Q "SELECT @@VERSION"
    if [ $? -eq 0 ]
    then
        echo "setup ready"
        break
    else
        echo "not ready yet..."
        sleep 1
    fi
done

/tools/sqlpackage/sqlpackage /Action:Publish /SourceFile:"CleanArchitecture.Database.dacpac" /Profile:"CleanArchitecture.Local.publish.xml" /TargetUser:$TARGET_USER /TargetPassword:$TARGET_PASSWORD /TargetServerName:$TARGET_SERVER_NAME
/tools/sqlpackage/sqlpackage /Action:Publish /SourceFile:"CleanArchitecture.Database.dacpac" /Profile:"CleanArchitecture.Test.publish.xml" /TargetUser:$TARGET_USER /TargetPassword:$TARGET_PASSWORD /TargetServerName:$TARGET_SERVER_NAME
