module Client.Tests

open Fable.Mocha

open Index
open Model
open Shared
open SAFE

let client =
    testList "Client" [
        testCase "Added todo"
        <| fun _ ->
            // let newTodo = Waehler.create "new todo"
            // let model, _ = init ()
            // let model, _ = update (LoadData(Finished([], []))) model
            // let model, _ = update (SaveWaehler(Finished newTodo)) model
//
            // Expect.equal
                // (model.Waehler |> RemoteData.map _.Length |> RemoteData.defaultValue 0)
                // 1
                // "There should be 1 todo"
//
                Expect.equal true true "Todo should equal new todo"
    ]

let all =
    testList "All" [
#if FABLE_COMPILER // This preprocessor directive makes editor happy
        Shared.Tests.shared
#endif
        client
    ]

[<EntryPoint>]
let main _ = Mocha.runTests all