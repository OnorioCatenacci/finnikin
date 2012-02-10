(*
 * Unit tests for finnikin library
 *
 *)

 namespace Finnikin
    open NUnit.Framework

    module Test =
        [<Test>]
        let ``Test Get Good Environment Variable``() =
            Assert.IsTrue(Option.isSome(Main.GetEnvironmentVariable "PATH"))

        [<Test>]
        let ``Test Get NonExistent Environment Variable``() =
            Assert.IsTrue(Option.isNone(Main.GetEnvironmentVariable "NonExistent"))
         
        [<Test>]
        let ``Test Get Environment Variable With Empty Name``() =
            Assert.IsTrue(Option.isNone(Main.GetEnvironmentVariable ""))     
        
        [<Test>]
        let ``Test Do Not Overwrite Existing Environment Variable``() =
            let setEnvVarResult = Main.SetEnvironmentVariable false "PATH" "P1"
            Assert.AreEqual(Finnikin.opResult.Failure,setEnvVarResult)

        [<Test>]
        let ``Test Deliberate Overwrite Of Existing Environment Variable``() =
            let currentPath = Main.GetEnvironmentVariable "PATH"
            let cpString = currentPath.Value
            let setEnvVarResult = Main.SetEnvironmentVariable true "PATH" (cpString + ";P1")
            Assert.AreEqual(Finnikin.opResult.Success,setEnvVarResult)
            let modifiedCP = Main.GetEnvironmentVariable "PATH"
            StringAssert.AreEqualIgnoringCase(cpString + ";P1",modifiedCP.Value)
            //Set the variable back to the original value
            Main.SetEnvironmentVariable true "PATH" cpString |> ignore

        [<Test>]
        let ``Attempt To Overwrite NonExistent Environment Variable``() =
            let setEnvVarResult = Main.SetEnvironmentVariable true "DOESNOTEXIST" "DOESNOTMATTER"
            //Should succeed because it should create the environment variable and set it
            Assert.AreEqual(Finnikin.opResult.Success,setEnvVarResult)
        
        [<EntryPoint>]
        let main args =
            0
            