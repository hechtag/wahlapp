module Server

open SAFE
open Saturn
open Shared
open System

module Storage =
    let waehlers = ResizeArray [ Waehler.create "Jannis"; Waehler.create "Muri" ]

    let addWaehler waehler =
        if String.IsNullOrWhiteSpace waehler.Name |> not then
            waehlers.Add waehler
            Ok()
        else
            Error "Invalid waehler"

let waehlerApi ctx = {
    getWaehlers = fun () -> async { return Storage.waehlers |> List.ofSeq }
    addWaehler =
        fun todo -> async {
            return
                match Storage.addWaehler todo with
                | Ok() -> todo
                | Error e -> failwith e
        }
}

let webApp = Api.make waehlerApi

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