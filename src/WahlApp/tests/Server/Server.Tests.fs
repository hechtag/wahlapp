module Server.Tests

open Expecto

open Model
open Shared
open Server

let jannisWaehlt kandidatId ={(Waehler.create "Jannis")  with  KandidatId =Some kandidatId }
let jannisVertraut verteilerId ={(Waehler.create "Jannis")  with  VerteilerId =Some verteilerId }
let muriWaehlt kandidatId ={(Waehler.create "Muri")  with  KandidatId =Some kandidatId }
let muriVertraut verteilerId ={(Waehler.create "Muri")  with  VerteilerId =Some verteilerId }
let philippVertraut verteilerId ={(Waehler.create "Philipp")  with  VerteilerId =Some verteilerId }
let pizza = Kandidat.create "Pizza"
let grillen = Kandidat.create "Grillen"

let server =
    testList "Server" [
        testCase "single person votes"
        <| fun _ ->
            let waehlers = [jannisWaehlt pizza.Id]
            let result = WahlLogic.rechneTest pizza.Id waehlers

            let expectedResult = {Id = pizza.Id; Anzahl = 1 }
            Expect.equal result expectedResult "Pizza should be counted "

        testCase "one person votes for pizza"
        <| fun _ ->
            let waehler = [jannisWaehlt pizza.Id; muriWaehlt grillen.Id]

            let result =WahlLogic.rechneTest pizza.Id waehler

            let expectedResult = {Id = pizza.Id; Anzahl = 1 }
            Expect.equal result expectedResult "Result should be ok"

        testCase "one person votes for pizza other trusts"
        <| fun _ ->
            let jannis =jannisWaehlt pizza.Id
            let waehler = [jannis; muriVertraut jannis.Id]

            let result =WahlLogic.rechneTest pizza.Id waehler

            let expectedResult = {Id = pizza.Id; Anzahl = 2 }
            Expect.equal result expectedResult "Result should be ok"

        testCase "one votes two trust them"
        <| fun _ ->
            let jannis =jannisWaehlt pizza.Id
            let waehler = [jannis; muriVertraut jannis.Id; philippVertraut jannis.Id]

            let result =WahlLogic.rechneTest pizza.Id waehler

            let expectedResult = {Id = pizza.Id; Anzahl = 3 }
            Expect.equal result expectedResult "Result should be ok"

        testCase "trust chain"
        <| fun _ ->
            let jannis =jannisWaehlt pizza.Id
            let muri =muriVertraut jannis.Id
            let waehler = [jannis; muri; philippVertraut muri.Id]

            let result =WahlLogic.rechneTest pizza.Id waehler

            let expectedResult = {Id = pizza.Id; Anzahl = 3 }
            Expect.equal result expectedResult "Result should be ok"

        testCase "two vote the same"
        <| fun _ ->
            let jannis =jannisWaehlt pizza.Id
            let muri =muriWaehlt pizza.Id
            let waehler = [jannis; muri ]

            let result =WahlLogic.rechneTest pizza.Id waehler

            let expectedResult = {Id = pizza.Id; Anzahl = 2 }
            Expect.equal result expectedResult "Result should be ok"    ]

let all = testList "All" [ Shared.Tests.shared; server ]

[<EntryPoint>]
let main _ = runTestsWithCLIArgs [] [||] all