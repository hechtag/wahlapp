module Server

open SAFE
open Saturn
open Shared
open System
open Db
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http
open Giraffe

// Your API logic here
let api (ctx: HttpContext) = {
    getWaehlers = WaehlerLogic.getWaehlers
    addWaehler = WaehlerLogic.addWaehler
    deleteWaehler = WaehlerLogic.deleteWaehler

    getKandidaten = KandidatLogic.getKandidaten
    addKandidat = KandidatLogic.addKandidat
    deleteKandidat = KandidatLogic.deleteKandidat

    waehlen = WahlLogic.waehlen
    getAuswertung = WahlLogic.getAuswertung
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