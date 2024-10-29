module KandidatLogic

open Entity
open Microsoft.Extensions.Logging
open Dto

let colName = "kandidat"

let getKandidaten () : Async<KandidatDto list> = async {
    return Db.findAll colName |> List.ofSeq

}

let addKandidat kandidat = async {
    return
        match Db.add<KandidatDto> colName kandidat with
        | Ok w -> w //|> Dto.FromKandidat
        | Error e -> failwith e

}

let deleteKandidat (id: KandidatId) : Async<KandidatDto list> = async {
    do Db.delete<Kandidat> colName (id |> Kandidat.Ka)
    return! getKandidaten ()
}