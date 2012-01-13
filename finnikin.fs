(*
* Finnikin
* Onorio Catenacci
* Finnikin is an F# library to make certain shell operations easier.  Since this is intended to be used from F#, usability from other .Net languages is not a high priority issue.
* Developed with Mono 2.10.6 on a MacOSX 10.7 box.  
* v0.1 10 January 2012
*)

///<summary>
///The finnikin module
///</summary>
namespace finnikin 
    open System
    open System.IO
    open System.Security
 
    module finnikin =
    
        ///<summary>
        /// Returns an option containing the definition of the environment string if it exists or None otherwise.  NB at least on Mac OSX environment variables are case-sensitive.
        /// That is, path <> PATH and the code doesn't automatically test for all possible cases.
        ///</summary>
        ///<param name="envVarName">Environment variable name to get the definition of</param>
        let getEnvironmentVariable envVarName =
            try
                let envVar = Environment.GetEnvironmentVariable(envVarName)
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
        let setEnvironmentVariable overwrite envVarName envVarValue =
            0    
        
        ///<summary>
        /// Check for invalid characters in a file name
        ///</summary>
        ///<param name="fileName">File name to check for validity</param>
        let validateFileName fileNameToValidate =
            not(String.IsNullOrEmpty(fileNameToValidate))
              
        ///<summary>
        /// Returns a sequence containing all the files which match the given spec.  User specifies starting dir and also whether or not they want to recurse to subdirs
        ///</summary>
        ///<param name="baseDir">Directory to start looking for files in</param>
        ///<param name="fileSpec">File mask to match</param>
        ///<param name="recurse">Should the process recurse into subdirectories or not</param>         
        let getFileList baseDir fileSpec recurse =
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
            | :? SecurityException -> Seq.empty
            | :? UnauthorizedAccessException -> Seq.empty

        ///<summary>
        ///Touch a specified file or set of files to modify the datetime for purposes of building
        ///</summary>
        ///<param name="baseDir">Directory to start looking for files in</param>
        ///<param name="fileSpec">File mask to match</param>
        ///<param name="recurse">Should the operation recurse into subdirectories or not</param>
        let touch baseDir fileSpec recurse =
            getFileList baseDir fileSpec recurse |> Seq.iter (fun f -> FileInfo(f).LastAccessTime <- DateTime.Now)