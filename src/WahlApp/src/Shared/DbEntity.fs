module DbEntity

open System
open Model

type IDb = interface end

type WaehlerDb = { Id: Guid ;Name: string; KandidatId: Guid option; VerteilerId: Guid option } interface IDb

type KandidatDb = { Id: Guid; Name: string } interface IDb

type KandidatenAggDb = { Id: Guid; Anzahl: int } interface IDb

type AuswertungDb = { Kandidaten: KandidatenAggDb list } interface IDb

module Db =
    let ToWaehler (dto: WaehlerDb) : Waehler = {
        Id = dto.Id |> Waehler.WC
        Name = dto.Name
        KandidatId = dto.KandidatId |> Option.map Kandidat.KC
        VerteilerId = dto.VerteilerId |> Option.map Waehler.WC
    }

    let FromWaehler (dto: Waehler) : WaehlerDb = {
        Id = dto.Id |> Waehler.Wa
        Name = dto.Name
        KandidatId = dto.KandidatId |> Option.map Kandidat.Ka
        VerteilerId = dto.VerteilerId |> Option.map Waehler.Wa
    }

    let ToKandidat (dto: KandidatDb) : Kandidat = {
        Id = dto.Id |> Kandidat.KC
        Name = dto.Name
    }

    let FromKandidat (dto: Kandidat) : KandidatDb = {
        Id = dto.Id |> Kandidat.Ka
        Name = dto.Name
    }