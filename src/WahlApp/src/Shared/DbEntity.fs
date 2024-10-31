module DbEntity

open System
open Model

type IDb = interface end

type WaehlerDb = { Id: Guid ;Name: string; KandidatId: Guid option; VerteilerId: Guid option } interface IDb

type KandidatDb = { Id: Guid; Name: string } interface IDb

type KandidatenAggDb = { Id: Guid; Anzahl: int } interface IDb

type AuswertungDb = { Kandidaten: KandidatenAggDb list } interface IDb

module Db =
    let private toStatus (waehlerDb:WaehlerDb): WaehlerStatus =
        match waehlerDb.KandidatId, waehlerDb.VerteilerId with
        |Option.None, Option.None -> NichtGewaehlt
        |Some kId , _ ->   kId |> Kandidat.KC |> Gewaehlt
        |_, Some wId ->  wId |> Waehler.WC |> Vertraut


    let ToWaehler (dto: WaehlerDb) : Waehler =
        {
        Id = dto.Id |> Waehler.WC
        Name = dto.Name
        Status = toStatus dto
    }

    let fromStatus (waehler:Waehler) : Guid option* Guid option =
        match waehler.Status with
        | NichtGewaehlt -> None, None
        | Gewaehlt kandidatId -> Some (kandidatId |> Kandidat.Ka), None
        | Vertraut waehlerId -> None, Some (waehlerId |> Waehler.Wa)
    let FromWaehler (dto: Waehler) : WaehlerDb =
        let kandidatId , waehlerId = fromStatus dto
        {
        Id = dto.Id |> Waehler.Wa
        Name = dto.Name
        KandidatId = kandidatId
        VerteilerId = waehlerId
    }

    let ToKandidat (dto: KandidatDb) : Kandidat = {
        Id = dto.Id |> Kandidat.KC
        Name = dto.Name
    }

    let FromKandidat (dto: Kandidat) : KandidatDb = {
        Id = dto.Id |> Kandidat.Ka
        Name = dto.Name
    }