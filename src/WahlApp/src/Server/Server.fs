module Server

open SAFE
open Saturn
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http
open Giraffe
open Api
open DbEntity

// Your API logic here
let api (ctx: HttpContext) = {
    getWaehlers = WaehlerLogic.getWaehlers
    addWaehler = WaehlerLogic.addWaehler
    deleteWaehler = WaehlerLogic.deleteWaehler

    getKandidaten =
        fun a ->
            let logger = ctx.GetLogger()

            try
                logger.LogInformation("-------------------------------------------------------------------------1")
                let res = KandidatLogic.getKandidaten a
                logger.LogInformation("-------------------------------------------------------------------------2")
                res
            with ex ->
                logger.LogError("------------------- AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", ex)

                async.Return []
    addKandidat = KandidatLogic.addKandidat
    deleteKandidat = KandidatLogic.deleteKandidat

    waehlen = WahlLogic.waehlen
    verteilen = WahlLogic.verteilen
    getAuswertung = WahlLogic.getAuswertung
}

let webApp = Api.make api

let app = application {
    use_router webApp
    // memory_cache
    use_static "public"
    use_gzip
    logging (fun logger -> logger.SetMinimumLevel LogLevel.Information |> ignore)
}

[<EntryPoint>]
let main _ =
    run app
    0