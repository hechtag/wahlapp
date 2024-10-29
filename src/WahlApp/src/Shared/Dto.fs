module Dto

open System
open Entity



type WaehlerDto = {
    Id: Guid
    Name: string
    KandidatId: Guid option
    VerteilerId: Guid option
}

type KandidatDto = { Id: Guid; Name: string }

type KandidatenAggDto = { Id: Guid; Anzahl: int }

type AuswertungDto = { Kandidaten: KandidatenAggDto list }

module Dto =
    let ToWaehler (dto: WaehlerDto) : Waehler = {
        Id = dto.Id |> Waehler.WC
        Name = dto.Name
        KandidatId = dto.KandidatId |> Option.map Kandidat.KC
        VerteilerId = dto.VerteilerId |> Option.map Waehler.WC
    }

    let FromWaehler (dto: Waehler) : WaehlerDto = {
        Id = dto.Id |> Waehler.Wa
        Name = dto.Name
        KandidatId = dto.KandidatId |> Option.map Kandidat.Ka
        VerteilerId = dto.VerteilerId |> Option.map Waehler.Wa
    }

    let ToKandidat (dto: KandidatDto) : Kandidat = {
        Id = dto.Id |> Kandidat.KC
        Name = dto.Name
    }

    let FromKandidat (dto: Kandidat) : KandidatDto = {
        Id = dto.Id |> Kandidat.Ka
        Name = dto.Name
    }