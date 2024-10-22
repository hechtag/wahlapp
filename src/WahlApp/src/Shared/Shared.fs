namespace Shared

open System

type Waehler = { Id: Guid; Name: string }

module Waehler =
    let create (name: string) = { Id = Guid.NewGuid(); Name = name }

type Kandidat = { Id: Guid; Name: string }

module Kandidat =
    let create (name: string) = { Id = Guid.NewGuid(); Name = name }

type Wahl = { Id: Guid; Name: string }

module Wahl =
    let create (name: string) = { Id = Guid.NewGuid(); Name = name }

type IApi = {
    getKandidaten: unit -> Async<Kandidat list>
    addKandidat: Kandidat -> Async<Kandidat>
    getWaehlers: unit -> Async<Waehler list>
    addWaehler: Waehler -> Async<Waehler>
    createWahl: Wahl -> Async<Wahl>
}