module Server

open SAFE
open Saturn
open Shared
open System
open Db
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http
open Giraffe

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



// Your API logic here
let api (ctx: HttpContext) = {
    getWaehlers = WaehlerLogic.getWaehlers
    addWaehler = WaehlerLogic.addWaehler
    deleteWaehler = WaehlerLogic.deleteWaehler

    getKandidaten = KandidatLogic.getKandidaten
    addKandidat = KandidatLogic.addKandidat
    deleteKandidat = KandidatLogic.deleteKandidat

    createWahl =
        fun wahl -> async {
            return
                match Storage.setWahl wahl with
                | Ok() -> wahl
                | Error e -> failwith e

        }
    waehlen = WahlLogic.waehlen
}

let webApp = Api.make api

let app = application {
    use_router webApp
    memory_cache
    use_static "public"
    use_gzip
    logging (fun logger -> logger.SetMinimumLevel LogLevel.Debug |> ignore)
}

[<EntryPoint>]
let main _ =
    run app
    0