(*
 * Unit tests for finnikin library
 *
 *)

// namespace Finnikin

    module Finnikin.Test 
        open NUnit.Framework
        open Finnikin

        [<Test>]
        let ``Test Get Good Environment Variable``() =
            Assert.IsTrue(Option.isSome(Finnikin.GetEnvironmentVariable "PATH"))

        [<Test>]
        let ``Test Get NonExistent Environment Variable``() =
            Assert.IsTrue(Option.isNone(Finnikin.GetEnvironmentVariable "NonExistent"))
         
        [<Test>]
        let ``Test Get Environment Variable With Empty Name``() =
            Assert.IsTrue(Option.isNone(Finnikin.GetEnvironmentVariable ""))     
        
        [<Test>]
        let ``Test Do Not Overwrite Existing Environment Variable``() =
            let setEnvVarResult = Finnikin.SetEnvironmentVariable false "PATH" "P1"
            Assert.AreEqual(false,setEnvVarResult)

        [<Test>]
        let ``Test Deliberate Overwrite Of Existing Environment Variable``() =
            let currentPath = Finnikin.GetEnvironmentVariable "PATH"
            let cpString = currentPath.Value
            let setEnvVarResult = Finnikin.SetEnvironmentVariable true "PATH" (cpString + ";P1")
            Assert.AreEqual(true,setEnvVarResult)
            let modifiedCP = Finnikin.GetEnvironmentVariable "PATH"
            StringAssert.AreEqualIgnoringCase(cpString + ";P1",modifiedCP.Value)
            //Set the variable back to the original value
            Finnikin.SetEnvironmentVariable true "PATH" cpString |> ignore

        [<Test>]
        let ``Attempt To Overwrite NonExistent Environment Variable``() =
            let setEnvVarResult = Finnikin.SetEnvironmentVariable true "DOESNOTEXIST" "DOESNOTMATTER"
            //Should succeed because it should create the environment variable and set it
            Assert.AreEqual(true,setEnvVarResult)
        
        [<EntryPoint>]
        let main args =
            0
            