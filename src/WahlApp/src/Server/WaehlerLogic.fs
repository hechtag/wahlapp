module WaehlerLogic

open Shared

let colName = "waehler"
let getWaehlers () : Async<Waehler list> = async { return Db.findAll colName |> List.ofSeq }

let addWaehler waehler = async {
    return
        match Db.add<Waehler> colName waehler with
        | Ok w -> w
        | Error e -> failwith e
}

let deleteWaehler id : Async<Waehler list> = async {
    do Db.delete<Waehler> colName id
    return! getWaehlers ()
}