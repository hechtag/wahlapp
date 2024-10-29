namespace Entity

open System

type WaehlerId = W of Guid
type KandidatId = K of Guid

type Waehler = {
    Id: WaehlerId
    Name: string
    KandidatId: KandidatId option
    VerteilerId: WaehlerId option
}

module Waehler =
    let create (name: string) = {
        Id = W(Guid.NewGuid())
        Name = name
        KandidatId = None
        VerteilerId = None
    }

    let Wa (W id) = id
    let WC (id: Guid) = W id
    let parse (str: string) = (Guid.Parse str) |> WC

type Kandidat = { Id: KandidatId; Name: string }

module Kandidat =
    let create (name: string) = { Id = K(Guid.NewGuid()); Name = name }
    let Ka (K id) = id
    let KC id = K id
    let parse (str: string) = (Guid.Parse str) |> KC

type KandidatenAgg = { Id: KandidatId; Anzahl: int }

type Auswertung = { Kandidaten: KandidatenAgg list }