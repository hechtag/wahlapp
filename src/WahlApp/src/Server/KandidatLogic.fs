module KandidatLogic

open Model
open Microsoft.Extensions.Logging
open DbEntity

let colName = "kandidat"

let getKandidaten () : Async<KandidatDb list> = async {
    return Db.findAll colName |> List.ofSeq

}

let addKandidat kandidat = async {
    return
        match Db.add<KandidatDb> colName kandidat with
        | Ok w -> w //|> Dto.FromKandidat
        | Error e -> failwith e

}

let deleteKandidat (id: KandidatId) : Async<KandidatDb list> = async {
    do Db.delete<KandidatDb> colName (id |> Kandidat.Ka)
    return! getKandidaten ()
}