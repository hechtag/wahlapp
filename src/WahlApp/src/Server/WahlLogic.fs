module WahlLogic

open Shared
open System


let waehlen (kandidatId, waehlerId) : Async<Waehler list> = async {
    let waehler = Db.getById WaehlerLogic.colName waehlerId

    let newWaehler = {
        waehler with
            KandidatId = Some kandidatId
    }

    let asdf = Db.update WaehlerLogic.colName newWaehler
    return! WaehlerLogic.getWaehlers ()
}

type Gewaehlt = {
    Id: Guid
    Name: string
    KandidatId: Guid
}

let opt (w: Waehler) =
    match w.KandidatId with
    | Some id ->
        Some {
            Id = w.Id
            Name = w.Name
            KandidatId = id
        }
    | None -> None

let innerJoin (kandidatList: Kandidat list) (waehlerList: Waehler list) =
    let grp = waehlerList |> List.choose (opt) |> List.groupBy _.KandidatId

    [
        for k in kandidatList do
            for (id, w) in grp do
                if k.Id = id then
                    yield {
                        Name = k.Name
                        Id = k.Id
                        Anzahl = List.length w
                    }
    ]

let getAuswertung () : Async<Auswertung> = async {
    let! waehler = WaehlerLogic.getWaehlers ()
    let! kandidaten = KandidatLogic.getKandidaten ()
    let asdf = innerJoin kandidaten waehler

    return { Kandidaten = asdf }
}