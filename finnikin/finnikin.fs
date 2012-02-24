(*
* Finnikin
* Onorio Catenacci
* Finnikin is an F# library to make certain shell operations easier.  Since this is intended to be used from F#, usability from other .Net languages is not a high priority issue.
* v0.3.12 23 February 2012
*)

module Finnikin
    open System
    open System.Diagnostics
    open System.IO
    open System.Security
    open System.ServiceProcess
    
    let private isPosixPlatform =
        match Environment.OSVersion.Platform with
        | PlatformID.Unix | PlatformID.MacOSX -> true
        | _ -> false
    
    //Allow for the fact we may be running on Linux or MacOSX
    let private correctedEnvironmentVariableName (environmentVariableName:string) =
        if isPosixPlatform then
            environmentVariableName.ToUpper()
        else
            environmentVariableName

    ///<Summary>
    /// Convert a normal string to a SecureString
    ///</Summary>
    ///<param name="s">The normal string to convert to a secure string</param>
    let private toSecureString (s:string) =
        use sString = new SecureString()
        [| for i in 0 .. s.Length-1 do yield sString.AppendChar s.[i]|] |> ignore
        sString         

    ///<summary>
    /// Returns an option containing the definition of the environment string if it exists or None otherwise.  NB at least on Mac OSX environment variables are case-sensitive.
    /// That is, path <> PATH and the code doesn't automatically test for all possible cases.
    ///</summary>
    ///<param name="envVarName">Environment variable name to get the definition of</param>
    let GetEnvironmentVariable (environmentVariableName:string) =
        try
            let envVar = Environment.GetEnvironmentVariable(correctedEnvironmentVariableName environmentVariableName)
            match envVar with
            | null -> None
            | _ -> Some(envVar)
        with
        | :? SecurityException -> None

    ///<summary>
    ///Set an environment variable.  The routine checks to see if the environment variable is already set; user passes in a parameter to indicate whether or not the existing
    ///value should be overwritten.
    ///</summary>
    ///<param name="envVarName">Environment variable to set definition for</param>
    ///<param name="envVarValue">Value to set specified environment variable to</param>
    ///<param name="overwrite">If the environment variable is already set should the value be overwritten</param>
    let SetEnvironmentVariable shouldOverwrite environmentVariableName environmentVariableValue =
        let platformCorrectedEnvironmentVariableName = correctedEnvironmentVariableName environmentVariableName
        if (not shouldOverwrite && Option.isSome(GetEnvironmentVariable platformCorrectedEnvironmentVariableName)) then
            false
        else
            try
                Environment.SetEnvironmentVariable (platformCorrectedEnvironmentVariableName, environmentVariableValue)
                true
            with
            | :? SecurityException -> false        
        
    ///<summary>
    /// Returns a sequence containing all the files which match the given spec.  User specifies starting dir and also whether or not they want to recurse to subdirs
    ///</summary>
    ///<param name="baseDir">Directory to start looking for files in</param>
    ///<param name="fileSpec">File mask to match</param>
    ///<param name="recurse">Should the process recurse into subdirectories or not</param>         
    let GetFileList baseDir fileSpec recurse =
        try
            if Directory.Exists(baseDir) then
                let rec getAllFiles baseDir fileSpec =
                    seq{
                        yield! Directory.EnumerateFiles(baseDir, fileSpec)
                        if recurse then
                            for d in Directory.EnumerateDirectories(baseDir) do
                                yield! getAllFiles d fileSpec
                    }
                getAllFiles baseDir fileSpec           
            else
                Seq.empty
        with
        | :? SecurityException | :? UnauthorizedAccessException -> Seq.empty

    ///<summary>
    /// Applies the specified function to all files which match the given spec
    ///</summary>
    ///<param name="baseDir">Directory to start looking for files in</param>
    ///<param name="fileSpec">File mask to match</param>
    ///<param name="recurse">Should the process recurse into subdirectories or not</param>         
    let FileOperation baseDir fileSpec recurse functionToApply =
        GetFileList baseDir fileSpec recurse |> Seq.iter functionToApply

    ///<summary>
    ///Touch a specified file or set of files to modify the datetime for purposes of building
    ///</summary>
    ///<param name="baseDir">Directory to start looking for files in</param>
    ///<param name="fileSpec">File mask to match</param>
    ///<param name="recurse">Should the operation recurse into subdirectories or not</param>
    let Touch baseDir fileSpec recurse =
        FileOperation baseDir fileSpec recurse (fun f -> FileInfo(f).LastAccessTime <- DateTime.Now) 

    ///<summary>
    /// Either returns the named service or returns an indication that the named service was not found
    ///</summary>
    ///<param name="serviceName">The service to find</param>
    let GetService serviceName =
        try
            let servicesList = ServiceController.GetServices()
            let s = servicesList |> Array.find (fun s -> s.ServiceName = serviceName)
            (true,Some(s))
        with
        | :? System.ComponentModel.Win32Exception | :? System.Collections.Generic.KeyNotFoundException -> (false,None) 

    ///<summary>
    /// Check if a service with the specified service name is running or not
    ///</summary>
    ///<param name="serviceName">Service to test</param>
    let IsServiceRunning serviceName =
        try
            let (serviceExists,service) = GetService serviceName
            if serviceExists then
                service.Value.Status = System.ServiceProcess.ServiceControllerStatus.Running
            else 
                false            
        with
        | :? System.ArgumentException -> false
            

    ///<summary>
    /// Attempt to start the named service on this machine. Note that no indication of success or failure is given
    ///</summary>
    ///<param name="serviceName">Service to attempt to start</param>
    let StartService serviceName =
        let (serviceExists,service) = GetService serviceName
        if serviceExists && not(IsServiceRunning(service.Value.ServiceName)) then
            service.Value.Start()

    ///<summary>
    /// Attempt to stop the named service on this machine. Note that no indication of success or failure is given
    ///</summary>
    ///<param name="serviceName">Service to attempt to stop</param>
    let StopService serviceName =
        let (serviceExists,service) = GetService serviceName
        if serviceExists && (IsServiceRunning(service.Value.ServiceName)) && service.Value.CanStop then
            service.Value.Stop()

    ///<summary>
    ///Set all files and directories which match a given spec to read and write
    ///</summary>
    ///<param name="baseDir">Directory to start looking for files in</param>
    ///<param name="fileSpec">File mask to match</param>
    ///<param name="recurse">Should the operation recurse into subdirectories or not</param>
    let SetFilesToReadWrite baseDir fileSpec recurse =
        FileOperation baseDir fileSpec recurse (fun f -> FileInfo(f).Attributes <- FileAttributes.Normal) 

    ///<summary>
    ///Run a process under a different user ID
    ///</summary>
    ///<param name="userID">User ID to start process under</param>
    ///<param name="exeToRun">Name of the executable to run under the different user id</param>
    ///<param name="arguments">Arguments to the executable</param>
    let RunProcessAs userID password exeToRun arguments = 
        let pStartInfo = new ProcessStartInfo(exeToRun,arguments)
        pStartInfo.UserName <- userID
        pStartInfo.Password <- toSecureString(password)
        try
            let p = Process.Start(pStartInfo).WaitForExit
            (true,Some(p))
        with
        | :? SecurityException -> (false, None)



    open System.DirectoryServices.AccountManagement

    ///<Summary>
    ///A structure that encapsulates the details necessary to create a new Windows user
    ///</Summary>
    type windowsUser = {
        userName:string
        password:string
        displayName:string
        description:string
        canChangePassword:bool
        passwordExpires:bool
    }

    ///<Summary>
    ///The call to create a new user in a given domain context. Takes a windowsUser record and a machine name as arguments
    ///</Summary>
    ///<param name="newUserSpec">The windowsUser record that specifies the details of the new user</param>
    ///<param name="machineID">The hostname of the domain in which to create this new user</param>
    let CreateNewWindowsAccount newUserSpec machineID =
        try
            let pc = new PrincipalContext(ContextType.Machine, machineID)
            let mutable user = new UserPrincipal(pc)
            user.SetPassword(newUserSpec.password)
            user.DisplayName <-newUserSpec.displayName
            user.Name <- newUserSpec.userName
            user.Description <- newUserSpec.description
            user.UserCannotChangePassword <- newUserSpec.canChangePassword
            user.PasswordNeverExpires <- newUserSpec.passwordExpires
            user.Save()

            let mutable gp = GroupPrincipal.FindByIdentity(pc, "Users")
            gp.Members.Add(user)
            gp.Save()
            true
        with
            | :? FileNotFoundException -> false
