(*
 * Unit tests for finnikin library
 *
 *)

 namespace Finnikin
    open NUnit.Framework

    module Test =
        [<Test>]
        let ``testGetEnvironment``() =
            Assert.IsTrue(Option.isSome(FinnikinMain.GetEnvironmentVariable "PATH"))

        [<Test>]
        let ``testGetNonExistentEnvironmentVariable``() =
            Assert.IsTrue(Option.isNone(FinnikinMain.GetEnvironmentVariable "NonExistent")) 
        
        [<Test>]
        let ``testOverwriteExistingEnvVar``() =
            let currentPath = FinnikinMain.GetEnvironmentVariable "PATH"
            let setEnvVarResult = FinnikinMain.SetEnvironmentVariable true "PATH" "P1"
            Assert.AreEqual(Finnikin.opResult.Success,setEnvVarResult)
            FinnikinMain.SetEnvironmentVariable true "PATH" currentPath.Value |> ignore

        [<Test>]
        let ``testDoNotOverwriteExistingEnvVar``() =
            let setEnvVarResult = FinnikinMain.SetEnvironmentVariable false "PATH" "P1"
            Assert.AreEqual(Finnikin.opResult.Failure,setEnvVarResult)



        [<EntryPoint>]
        let main args =
            0
            