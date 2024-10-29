module WahlLogic

open Entity
open System


let waehlen (kandidatId, waehlerId: WaehlerId) : Async<Waehler list> = async {
    let waehler = Db.getById WaehlerLogic.colName (waehlerId |> Waehler.Wa)

    let newWaehler = {
        waehler with
            KandidatId = Some kandidatId
            VerteilerId = None
    }

    let asdf = Db.update WaehlerLogic.colName newWaehler
    return! WaehlerLogic.getWaehlers ()
}

let verteilen (verteilerId: WaehlerId, waehlerId: WaehlerId) : Async<Waehler list> = async {
    let waehler = Db.getById WaehlerLogic.colName (waehlerId |> Waehler.Wa)

    let newWaehler = {
        waehler with
            VerteilerId = Some verteilerId
            KandidatId = None
    }

    let asdf = Db.update WaehlerLogic.colName newWaehler
    return! WaehlerLogic.getWaehlers ()
}

type GewaehltW = {
    WaehlerId: WaehlerId
    VerteilerId: WaehlerId
}

type GewaehltK = {
    WaehlerId: WaehlerId
    KandidatId: KandidatId
}

let optKandidat (w: Waehler) : GewaehltK option =
    match w.KandidatId with
    | Some id -> Some { WaehlerId = w.Id; KandidatId = id }
    | None -> None

let optVerteilt (w: Waehler) : GewaehltW option =
    match w.VerteilerId with
    | Some id -> Some { WaehlerId = w.Id; VerteilerId = id }
    | None -> None

let getEntscheider (waehlerList: Waehler list) =
    waehlerList |> List.choose (optKandidat)

let rec rechneEinen (waehlerList: GewaehltW list) (gewaehlt: GewaehltW) : int =
    let anhaenger =
        waehlerList |> List.filter (fun w -> w.VerteilerId = gewaehlt.WaehlerId)

    (List.length anhaenger)
    + 1
    + (anhaenger |> List.map (rechneEinen waehlerList) |> List.sum)

let agg zugeordnetList (e: GewaehltK) : KandidatenAgg =
    let start = {
        VerteilerId = (Waehler.WC Guid.Empty)
        WaehlerId = e.WaehlerId
    }

    let sum = rechneEinen zugeordnetList start

    { Anzahl = sum; Id = e.KandidatId }

let rechne (waehlerList: Waehler list) =
    let zugeordnetList = waehlerList |> List.choose optVerteilt

    waehlerList |> getEntscheider |> List.map (agg zugeordnetList)


// let innerJoin (kandidatList: Kandidat list) (waehlerList: Waehler list) =
//     let grp = waehlerList |> List.choose (optKandidat) |> List.groupBy _.KandidatId

//     [
//         for k in kandidatList do
//             for (id, w) in grp do
//                 if k.Id = id then
//                     yield {
//                         Name = k.Name
//                         Id = k.Id
//                         Anzahl = List.length w
//                     }
//     ]

let getAuswertung (logger) : Async<Auswertung> = async {
    let! waehler = WaehlerLogic.getWaehlers (logger)
    // let! kandidaten = KandidatLogic.getKandidaten ()
    let asdf = rechne waehler //innerJoin kandidaten waehler

    return { Kandidaten = asdf }
}