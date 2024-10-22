module Server

open SAFE
open Saturn
open Shared
open System

module Storage =
    let waehlers = ResizeArray [ Waehler.create "Jannis"; Waehler.create "Muri" ]
    let kandidaten = ResizeArray [ Kandidat.create "Jannis"; Kandidat.create "Muri" ]
    let mutable currentWahl = None

    let setWahl wahl =
        currentWahl <- Some wahl
        Ok()

    let addKandidat (kandidat: Kandidat) =
        if String.IsNullOrWhiteSpace kandidat.Name |> not then
            kandidaten.Add kandidat
            Ok()
        else
            Error "Invalid waehler"

    let addWaehler (waehler: Waehler) =
        if String.IsNullOrWhiteSpace waehler.Name |> not then
            waehlers.Add waehler
            Ok()
        else
            Error "Invalid waehler"

let api ctx = {
    getWaehlers = fun () -> async { return Storage.waehlers |> List.ofSeq }
    addWaehler =
        fun todo -> async {
            return
                match Storage.addWaehler todo with
                | Ok() -> todo
                | Error e -> failwith e
        }

    getKandidaten = fun () -> async { return Storage.kandidaten |> List.ofSeq }
    addKandidat =
        fun todo -> async {
            return
                match Storage.addKandidat todo with
                | Ok() -> todo
                | Error e -> failwith e
        }
    createWahl =
        fun wahl -> async {
            return
                match Storage.setWahl wahl with
                | Ok() -> wahl
                | Error e -> failwith e

        }
}

let webApp = Api.make api

let app = application {
    use_router webApp
    memory_cache
    use_static "public"
    use_gzip
}

[<EntryPoint>]
let main _ =
    run app
    0