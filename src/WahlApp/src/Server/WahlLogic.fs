module WahlLogic

open DbEntity
open Model
open System


let waehlen (kandidatId: KandidatId, waehlerId : WaehlerId) : Async<WaehlerDb list> = async {
    let waehler = Db.getById WaehlerLogic.colName (waehlerId |> Waehler.Wa) |> Db.ToWaehler

    let newWaehler = { waehler with Status = Gewaehlt kandidatId }

    let asdf = Db.update WaehlerLogic.colName (newWaehler|> Db.FromWaehler)
    return! WaehlerLogic.getWaehlers ()
}

let verteilen (verteilerId: WaehlerId, waehlerId: WaehlerId) : Async<WaehlerDb list> = async {
    let waehler = Db.getById WaehlerLogic.colName (waehlerId |> Waehler.Wa) |> Db.ToWaehler

    let newWaehler = { waehler with Status = Vertraut verteilerId; }

    let asdf = Db.update WaehlerLogic.colName (newWaehler |> Db.FromWaehler)
    return! WaehlerLogic.getWaehlers ()
}

type WaehlerAgg = {Id: WaehlerId; Anzahl: int}
let rec zaehleVertraute (waehler: Waehler list) (waehlerAgg: WaehlerAgg) : WaehlerAgg=
    let vertraute = waehler |> List.filter (Waehler.optBoolV waehlerAgg.Id)
    let fold agg (b:Waehler)  =
        let zwischenAgg =(zaehleVertraute waehler {Id= b.Id; Anzahl = 1 })
        {agg with Anzahl = agg.Anzahl + zwischenAgg.Anzahl }
    vertraute |> List.fold fold waehlerAgg


let rechneTest (kandidatId: KandidatId) (waehler: Waehler list): KandidatenAgg =
    let direkteWaehler = waehler |> List.filter (Waehler.optBoolK kandidatId)
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

let rec collectVertrautenIds (waehlerList: Waehler list) (waehler: Waehler): WaehlerId list=
    let vertraute =waehlerList |> List.filter  (Waehler.optBoolV waehler.Id )
    let vertrIds = vertraute |> List.map _.Id
    vertrIds @ (vertraute |> List.collect (collectVertrautenIds waehlerList))
let getVertrauteFor (waehlerList: Waehler list) (waehler:Waehler) : WaehlerId * Waehler list =
    let ids = collectVertrautenIds waehlerList waehler
    let res = waehlerList
                  |> List.filter (fun w -> w.Id <> waehler.Id)
                  |> List.filter ((fun w -> List.contains w.Id ids ) >> not)

    waehler.Id, res



let getVertraute () :Async<(WaehlerId * Waehler list) list> =
    async {
        let! waehlerDb = WaehlerLogic.getWaehlers ()
        let waehler = waehlerDb |> List.map Db.ToWaehler
        return waehler |> List.map (getVertrauteFor waehler)
    }

