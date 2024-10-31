module Server.Tests

open Expecto

open Model
open Shared
open Server

let jannisWaehlt kandidatId ={(Waehler.create "Jannis")  with  Status = Gewaehlt kandidatId }
let jannisVertraut verteilerId ={(Waehler.create "Jannis")  with  Status = Vertraut verteilerId }
let muriWaehlt kandidatId ={(Waehler.create "Muri")  with  Status =Gewaehlt kandidatId }
let muriVertraut verteilerId ={(Waehler.create "Muri")  with  Status =Vertraut verteilerId }
let philippVertraut verteilerId ={(Waehler.create "Philipp")  with  Status =Vertraut verteilerId }
let pizza = Kandidat.create "Pizza"
let grillen = Kandidat.create "Grillen"

let auswertung =
    testList "Auswertung" [
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

let vertraute =
    testList "Vertraute" [
        testCase "no waehler"
        <| fun _ ->
            let jannis  = jannisWaehlt pizza.Id
            let waehlers = []
            let result = WahlLogic.getVertrauteFor  waehlers jannis

            let expectedResult = jannis.Id, []
            Expect.equal result expectedResult "Pizza should be counted "
        testCase "only oneself"
        <| fun _ ->
            let jannis  = jannisWaehlt pizza.Id
            let waehlers = [jannis]
            let result = WahlLogic.getVertrauteFor  waehlers jannis

            let expectedResult = jannis.Id, []
            Expect.equal result expectedResult "Pizza should be counted "
        testCase "another vertrauter"
        <| fun _ ->
            let jannis  = jannisWaehlt pizza.Id
            let muri  = muriWaehlt pizza.Id
            let waehlers = [jannis; muri]
            let result = WahlLogic.getVertrauteFor  waehlers jannis

            let expectedResult = jannis.Id, [muri]
            Expect.equal result expectedResult "Pizza should be counted "
        testCase "mu vertraut"
        <| fun _ ->
            let jannis  = jannisWaehlt pizza.Id
            let muri  = muriVertraut jannis.Id
            let waehlers = [jannis; muri]
            let result = WahlLogic.getVertrauteFor  waehlers jannis

            let expectedResult = jannis.Id, []
            Expect.equal result expectedResult "Pizza should be counted "
        testCase "50 percent trust"
        <| fun _ ->
            let jannis  = jannisWaehlt pizza.Id
            let muri  = muriWaehlt pizza.Id
            let philipp  = philippVertraut jannis.Id
            let waehlers = [jannis; muri; philipp]
            let result = WahlLogic.getVertrauteFor  waehlers jannis

            let expectedResult = jannis.Id, [muri]
            Expect.equal result expectedResult "Pizza should be counted "

        testCase "chain"
        <| fun _ ->
            let jannis  = jannisWaehlt pizza.Id
            let muri  = muriVertraut jannis.Id
            let philipp  = philippVertraut muri.Id
            let waehlers = [jannis; muri; philipp]
            let result = WahlLogic.getVertrauteFor  waehlers jannis

            let expectedResult = jannis.Id, []
            Expect.equal result expectedResult "Pizza should be counted "
             ]

let all = testList "All" [ Shared.Tests.shared; auswertung; vertraute ]

[<EntryPoint>]
let main _ = runTestsWithCLIArgs [] [||] all