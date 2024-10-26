module KandidatLogic

open Shared

let colName = "kandidat"
let getKandidaten () = async { return Db.findAll colName |> List.ofSeq }

let addKandidat kandidat = async {
    return
        match Db.add<Kandidat> colName kandidat with
        | Ok w -> w
        | Error e -> failwith e
}