namespace Shared

open System

type Waehler = { Id: Guid; Name: string }

module Waehler =

    let create (name: string) = { Id = Guid.NewGuid(); Name = name }

type IWaehlerApi = {
    getWaehlers: unit -> Async<Waehler list>
    addWaehler: Waehler -> Async<Waehler>
}



type Kandidat = { Id: Guid; Name: string }

module Kandidat =

    let create (name: string) = { Id = Guid.NewGuid(); Name = name }

type IKandidatApi = {
    getKandidaten: unit -> Async<Kandidat list>
    addKandidat: Kandidat -> Async<Kandidat>
}

type IApi = {
    Kandidat: IKandidatApi
    Waehler: IWaehlerApi
}