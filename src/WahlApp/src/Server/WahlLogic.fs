module WahlLogic

open DbEntity
open Model
open System


let waehlen (kandidatId: KandidatId, waehlerId : WaehlerId) : Async<WaehlerDb list> = async {
    let waehler = Db.getById WaehlerLogic.colName (waehlerId |> Waehler.Wa) |> Db.ToWaehler

    let newWaehler = {
        waehler with
            KandidatId = Some kandidatId
            VerteilerId = None
    }

    let asdf = Db.update WaehlerLogic.colName (newWaehler|> Db.FromWaehler)
    return! WaehlerLogic.getWaehlers ()
}

let verteilen (verteilerId: WaehlerId, waehlerId: WaehlerId) : Async<WaehlerDb list> = async {
    let waehler = Db.getById WaehlerLogic.colName (waehlerId |> Waehler.Wa) |> Db.ToWaehler

    let newWaehler = {
        waehler with
            VerteilerId = Some verteilerId
            KandidatId = None
    }

    let asdf = Db.update WaehlerLogic.colName (newWaehler |> Db.FromWaehler)
    return! WaehlerLogic.getWaehlers ()
}

let optBoolK (k: KandidatId) (w: Waehler)  :bool =
    match w.KandidatId with
    | Some id -> id = k
    | None -> false

let optBoolV (k: WaehlerId) (w: Waehler)  :bool =
    match w.VerteilerId with
    | Some id -> id = k
    | None -> false
type WaehlerAgg = {Id: WaehlerId; Anzahl: int}
let rec zaehleVertraute (waehler: Waehler list) (waehlerAgg: WaehlerAgg) : WaehlerAgg=
    let vertraute = waehler |> List.filter (optBoolV waehlerAgg.Id)
    let fold agg (b:Waehler)  =
        let zwischenAgg =(zaehleVertraute waehler {Id= b.Id; Anzahl = 1 })
        {agg with Anzahl = agg.Anzahl + zwischenAgg.Anzahl }
    vertraute |> List.fold fold waehlerAgg


let rechneTest (kandidatId: KandidatId) (waehler: Waehler list): KandidatenAgg =
    let direkteWaehler = waehler |> List.filter (optBoolK kandidatId)
    let fold  (agg:KandidatenAgg) (erster: Waehler)  =
        let zwischenAgg = zaehleVertraute waehler {Id = erster.Id; Anzahl = 1 }
        {agg with Anzahl = agg.Anzahl +  zwischenAgg.Anzahl}
    let vertraute : KandidatenAgg = direkteWaehler |> List.fold  fold {Id =kandidatId; Anzahl = 0 }

    {Id = kandidatId; Anzahl = vertraute.Anzahl }

let getAuswertung ()  : Async<Auswertung> = async {
    let! waehlerDb = WaehlerLogic.getWaehlers ()
    let waehler = waehlerDb|> List.map Db.ToWaehler
    let! kandidatenDb = KandidatLogic.getKandidaten ()
    let kandidaten = kandidatenDb|> List.map Db.ToKandidat

    let asdf = kandidaten |> List.map (fun k ->  rechneTest k.Id waehler)
    return { Kandidaten = asdf }
}


