(*
 * Unit tests for finnikin library
 *
 *)

 namespace Finnikin
    open NUnit.Framework

    module Test =
        [<Test>]
        let ``TestGetEnvironmentVariable``() =
            Assert.IsTrue(Option.isSome(Main.GetEnvironmentVariable "PATH"))

        [<Test>]
        let ``TestGetNonExistentEnvironmentVariable``() =
            Assert.IsTrue(Option.isNone(Main.GetEnvironmentVariable "NonExistent"))
         
        [<Test>]
        let ``TestGetEnvironmentVariableWithEmptyName``() =
            Assert.IsTrue(Option.isNone(Main.GetEnvironmentVariable ""))     
        
        [<Test>]
        let ``TestDoNotOverwriteExistingEnvironmentVariable``() =
            let setEnvVarResult = Main.SetEnvironmentVariable false "PATH" "P1"
            Assert.AreEqual(Finnikin.opResult.Failure,setEnvVarResult)

        [<EntryPoint>]
        let main args =
            0
            