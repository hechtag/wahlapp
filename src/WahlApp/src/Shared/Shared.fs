namespace Shared

open System

type Waehler = { Id: Guid; Name: string }

module Waehler =

    let create (name: string) = { Id = Guid.NewGuid(); Name = name }

type IWaehlerApi = {
    getWaehlers: unit -> Async<Waehler list>
    addWaehler: Waehler -> Async<Waehler>
}