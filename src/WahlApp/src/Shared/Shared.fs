namespace Shared

open System

type Waehler = {
    Id: Guid
    Name: string
    KandidatId: Guid option
}

module Waehler =
    let create (name: string) = {
        Id = Guid.NewGuid()
        Name = name
        KandidatId = None
    }

type Kandidat = { Id: Guid; Name: string }

module Kandidat =
    let create (name: string) = { Id = Guid.NewGuid(); Name = name }

type KandidatenAgg = { Id: Guid; Name: string; Anzahl: int }
type Auswertung = { Kandidaten: KandidatenAgg list }



type IApi = {
    getKandidaten: unit -> Async<Kandidat list>
    addKandidat: Kandidat -> Async<Kandidat>
    deleteKandidat: Guid -> Async<Kandidat list>

    getWaehlers: unit -> Async<Waehler list>
    addWaehler: Waehler -> Async<Waehler>
    deleteWaehler: Guid -> Async<Waehler list>

    waehlen: Guid * Guid -> Async<Waehler list>
    getAuswertung: unit -> Async<Auswertung>
}