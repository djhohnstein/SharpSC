# SharpSC

Simple .NET assembly to perform basic operations with services.

# Usage
```
Usage:
    SharpSC.exe action=ACTION [computername=COMPUTERNAME service=SERVICE]

Parameters:
    action          Action to perform. Must be one of 'start', 'stop', or 'query' (Required)
    service         Service name to interact with. Optional for the 'query' action, but mandatory
                    for the 'start' and 'stop' actions.
    computername    Computer to perform the action against. If not provided, localhost is used.

Example:

    SharpSC.exe action=query computername=dc01 service=ikeext
```