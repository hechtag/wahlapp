module WaehlerLogic

open System
open DbEntity
open Microsoft.Extensions.Logging
open Model

let colName = "waehler"

let getWaehlers () : Async<WaehlerDb list> = async {
    return Db.findAll colName |> List.ofSeq

}

let addWaehler waehler = async {
    return
        match Db.add<WaehlerDb> colName waehler with
        | Ok w -> w
        | Error e -> failwith e
}

let deleteWaehler (id: WaehlerId) : Async<WaehlerDb list> = async {
    do Db.delete<WaehlerDb> colName (id|> Waehler.Wa)
    return! getWaehlers ()
}