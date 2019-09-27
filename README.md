# SharpSC

Simple .NET assembly to perform basic operations with services.

# Usage
```
Usage:
    SharpSC.exe action=ACTION [computername=COMPUTERNAME service=SERVICE displayname=DISPLAYNAME binpath=BINPATH]

Parameters:
    action          Action to perform. Must be one of 'start', 'stop', 'query', 'create', or 'delete' (Required)
                    
                    query:
                        List all services.
                    start:
                        Start the specified service. Requires parameter ""service""
                    stop:
                        Stop the specified service. Requires parameter ""service""
                    create:
                        Create the specified service. Requires parameters ""service"", ""displayname"", and ""binpath"".
                    delete:
                        Delete the specified service. Requires parameters ""service"".

    service         Service name to interact with. Optional for the 'query' action, but mandatory
                    for the 'start', 'stop', 'create', and 'delete' actions.

    displayname     Display name for the service to be created. Only used in the 'create' action.

    binpath         Path to the binary that you wish to create the service for. Only used in the 'create' action.

    computername    Computer to perform the action against. If not provided, localhost is used.

Example:

    Query for IKEEXT on DC01:
        SharpSC.exe action=query computername=dc01 service=ikeext
    
    Create a new service on DC01 with name MyService to start cmd.exe:
        SharpSC.exe action=create computername=dc01 service=MyService displayname=""My Service"" binpath=C:\Windows\System32\cmd.exe
    
    Start MyService on DC01:
        SharpSC.exe action=start computername=dc01 service=MyService

    Stop IKKEXT Service locally:
        SharpSC.exe action=stop service=ikeext

    Delete MyService on DC01
        SharpSC.exe action=delete service=MyService computername=dc01
```