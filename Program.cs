using System;
using System.Collections.Generic;
using System.ServiceProcess;

namespace SharpSC
{
    class Program
    {
        static Dictionary<string,string> ParseArgs(string[] args)
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            foreach(string arg in args)
            {
                string[] parts = arg.Split('=');
                if (parts.Length != 2) {
                    Console.WriteLine("[-] Argument with bad format passed, skipping: {0}", arg);
                    continue;
                }
                results[parts[0].ToLower()] = parts[1];
            }
            return results;
        }

        static void Usage()
        {
            string usageString = @"
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
";
            Console.WriteLine(usageString);
        }
        static void Main(string[] args)
        {
            Dictionary<string, string> programArgs = ParseArgs(args);
            if (!programArgs.ContainsKey("action"))
            {
                Console.WriteLine("[-] Must give action argument for service controller to do something. Exiting!");
                Usage();
                Environment.Exit(1);
            }
            string serviceName = "";
            string displayName = "";
            string binpath = "";
            if (programArgs["action"].ToLower() != "query" && !programArgs.ContainsKey("service"))
            {
                Usage();
                Console.WriteLine("[-] Given an action that requires a service, but none was given. Exiting!");
                Environment.Exit(1);
            }
            if (programArgs["action"].ToLower() == "create" && (!programArgs.ContainsKey("service") || !programArgs.ContainsKey("displayname") || !programArgs.ContainsKey("binpath")))
            {
                Usage();
                Console.WriteLine("[-] Given an action that requires a service, but none was given. Exiting!");
                Environment.Exit(1);
            }
            if (programArgs["action"].ToLower() == "delete" && !programArgs.ContainsKey("service"))
            {
                Usage();
                Console.WriteLine("[-] Given an action that requires a service, but none was given. Exiting!");
                Environment.Exit(1);
            }
            if (programArgs.ContainsKey("service"))
            {
                serviceName = programArgs["service"];
            }
            if (programArgs.ContainsKey("displayname"))
            {
                displayName = programArgs["displayname"];
            }
            if (programArgs.ContainsKey("binpath"))
            {
                binpath = programArgs["binpath"];
            }
            string computerName = "localhost";
            if (programArgs.ContainsKey("computername"))
            {
                computerName = programArgs["computername"];
            }
            switch (programArgs["action"])
            {
                case "query":
                    try
                    {
                        var services = ServiceController.GetServices(computerName);
                        if (services.Length > 0)
                        {
                            if (serviceName == "")
                            {
                                Console.WriteLine("[*] Services on {0}:\n", computerName);
                                foreach (var service in services)
                                {
                                    Console.WriteLine("\tDisplayName: {0}", service.DisplayName);
                                    Console.WriteLine("\tServiceName: {0}", service.ServiceName);
                                    Console.WriteLine("\tStatus     : {0}", service.Status);
                                    Console.WriteLine("\tCanStop    : {0}", service.CanStop);
                                    Console.WriteLine();
                                }
                            }
                            else
                            {
                                bool found = false;
                                foreach (var service in services)
                                {
                                    if (service.ServiceName.ToLower() == serviceName.ToLower())
                                    {
                                        found = true;
                                        Console.WriteLine("[+] Service information for {0} on {1}:", service.ServiceName, computerName);
                                        Console.WriteLine("\tDisplayName: {0}", service.DisplayName);
                                        Console.WriteLine("\tServiceName: {0}", service.ServiceName);
                                        Console.WriteLine("\tStatus     : {0}", service.Status);
                                        Console.WriteLine("\tCanStop    : {0}", service.CanStop);
                                        Console.WriteLine();
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    Console.WriteLine("[-] No service with name {0} could be found on {1}", serviceName, computerName);
                                }
                            }
                        }
                    } catch (Exception ex)
                    {
                        Console.WriteLine("[-] Error querying data. Reason: {0}", ex.Message);
                        Console.WriteLine("[-] StackTrace:\n{0}", ex.StackTrace);
                    }
                    break;
                case "create":
                    PInvokeFunctions.InstallService(computerName, serviceName, displayName, binpath);
                    break;
                case "delete":
                    PInvokeFunctions.UninstallService(computerName, serviceName);
                    break;
                case "start":
                    try
                    {
                        var serviceInstance = new ServiceController(serviceName, computerName);
                        if (serviceInstance.Status == ServiceControllerStatus.Running || serviceInstance.Status == ServiceControllerStatus.StartPending)
                        {
                            Console.WriteLine("[-] Service {0} on {1} is already running. Current status: {2}", serviceInstance.ServiceName, computerName, serviceInstance.Status.ToString());
                            Environment.Exit(0);
                        }
                        Console.WriteLine("[*] Attempting to start service {0} on {1}...", serviceInstance.ServiceName, computerName);
                        serviceInstance.Start();
                        serviceInstance.WaitForStatus(ServiceControllerStatus.Running);
                        Console.WriteLine("[+] Successfully started {0} on {1}!", serviceInstance.ServiceName, computerName);
                        Console.WriteLine();
                        Console.WriteLine("\tDisplayName: {0}", serviceInstance.DisplayName);
                        Console.WriteLine("\tServiceName: {0}", serviceInstance.ServiceName);
                        Console.WriteLine("\tStatus     : {0}", serviceInstance.Status);
                        Console.WriteLine("\tCanStop    : {0}", serviceInstance.CanStop);
                    } catch (Exception ex)
                    {
                        Console.WriteLine("[-] Error in attempting to start service {0} on {1}. Reason: {2}", serviceName, computerName, ex.Message);
                        Console.WriteLine("[-] StackTrace:\n{0}", ex.StackTrace);
                    }
                    break;
                case "stop":
                    try
                    {
                        var serviceInstance = new ServiceController(serviceName, computerName);
                        if (serviceInstance.Status == ServiceControllerStatus.Stopped || serviceInstance.Status == ServiceControllerStatus.StopPending)
                        {
                            Console.WriteLine("[-] {0} on {1} is already stopped. Current status: {2}", serviceInstance.ServiceName, computerName, serviceInstance.Status.ToString());
                            Environment.Exit(0);
                        }
                        Console.WriteLine("[*] Attempting to stop service {0} on {1}...", serviceInstance.ServiceName, computerName);
                        serviceInstance.Stop();
                        serviceInstance.WaitForStatus(ServiceControllerStatus.Stopped);
                        Console.WriteLine("[+] Successfully stopped {0} on {1}!", serviceInstance.ServiceName, computerName);
                        Console.WriteLine();
                        Console.WriteLine("\tDisplayName: {0}", serviceInstance.DisplayName);
                        Console.WriteLine("\tServiceName: {0}", serviceInstance.ServiceName);
                        Console.WriteLine("\tStatus     : {0}", serviceInstance.Status);
                        Console.WriteLine("\tCanStop    : {0}", serviceInstance.CanStop);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[-] Error in attempting to start service {0} on {1}. Reason: {2}", serviceName, computerName, ex.Message);
                        Console.WriteLine("[-] StackTrace:\n{0}", ex.StackTrace);
                    }
                    break;
                default:
                    Console.WriteLine("[-] Invalid action specified: {0}", programArgs["action"]);
                    Usage();
                    break;
            }
        }
    }
}
