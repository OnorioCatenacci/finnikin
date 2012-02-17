(*
 * Unit tests for finnikin library
 *
 *)
module Finnikin.Test 
    open NUnit.Framework
    open Finnikin
    let defaultTestEnvironmentVariable = "path"
    let defaultWindowsTestService = "W32Time" //Windows Time

    [<Test>]
    let ``Test Get Good Environment Variable``() =
        Assert.IsTrue(Option.isSome(Finnikin.GetEnvironmentVariable defaultTestEnvironmentVariable))

    [<Test>]
    let ``Test Get NonExistent Environment Variable``() =
        Assert.IsTrue(Option.isNone(Finnikin.GetEnvironmentVariable "NonExistent"))
         
    [<Test>]
    let ``Test Get Environment Variable With Empty Name``() =
        Assert.IsTrue(Option.isNone(Finnikin.GetEnvironmentVariable ""))     
        
    [<Test>]
    let ``Test Do Not Overwrite Existing Environment Variable``() =
        let setEnvVarResult = Finnikin.SetEnvironmentVariable false defaultTestEnvironmentVariable "P1"
        Assert.AreEqual(false,setEnvVarResult)

    [<Test>]
    let ``Test Deliberate Overwrite Of Existing Environment Variable``() =
        let currentPath = Finnikin.GetEnvironmentVariable defaultTestEnvironmentVariable
        let cpString = currentPath.Value
        let setEnvVarResult = Finnikin.SetEnvironmentVariable true defaultTestEnvironmentVariable (cpString + ";P1")
        Assert.AreEqual(true,setEnvVarResult)
        let modifiedCP = Finnikin.GetEnvironmentVariable defaultTestEnvironmentVariable
        StringAssert.AreEqualIgnoringCase(cpString + ";P1",modifiedCP.Value)
        //Set the variable back to the original value
        Finnikin.SetEnvironmentVariable true defaultTestEnvironmentVariable cpString |> ignore

    [<Test>]
    let ``Attempt To Overwrite NonExistent Environment Variable``() =
        let setEnvVarResult = Finnikin.SetEnvironmentVariable true "DOESNOTEXIST" "DOESNOTMATTER"
        //Should succeed because it should create the environment variable and set it
        Assert.AreEqual(true,setEnvVarResult)

    [<Test>]
    let ``Attempt To Get A Default Windows Service``() =
        let serviceIsPresent, defaultServiceController = Finnikin.GetService defaultWindowsTestService
        Assert.AreEqual(true,serviceIsPresent)
        Assert.AreEqual(true,Option.isSome(defaultServiceController))

    [<Test>]
    let ``Test That Service Which Should Be Running Is Running``() =
        Assert.AreEqual(true, Finnikin.IsServiceRunning defaultWindowsTestService)
   
    [<EntryPoint>]
    let main args =
        0
            