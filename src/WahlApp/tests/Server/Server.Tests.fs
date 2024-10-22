module Server.Tests

open Expecto

open Shared
open Server

let server =
    testList "Server" [
        testCase "Adding valid Todo"
        <| fun _ ->
            let validTodo = Waehler.create "TODO"
            let expectedResult = Ok()

            let result = Storage.addWaehler validTodo

            Expect.equal result expectedResult "Result should be ok"
            Expect.contains Storage.waehlers validTodo "Storage should contain new todo"
    ]

let all = testList "All" [ Shared.Tests.shared; server ]

[<EntryPoint>]
let main _ = runTestsWithCLIArgs [] [||] all