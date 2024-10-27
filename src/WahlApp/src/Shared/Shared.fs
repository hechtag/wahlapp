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

type Wahl = { Id: Guid; Name: string }

module Wahl =
    let create (name: string) = { Id = Guid.NewGuid(); Name = name }

type IApi = {
    getKandidaten: unit -> Async<Kandidat list>
    addKandidat: Kandidat -> Async<Kandidat>
    deleteKandidat: Guid -> Async<Kandidat list>

    getWaehlers: unit -> Async<Waehler list>
    addWaehler: Waehler -> Async<Waehler>
    deleteWaehler: Guid -> Async<Waehler list>

    createWahl: Wahl -> Async<Wahl>
    waehlen: Guid * Guid -> Async<Waehler list>
}