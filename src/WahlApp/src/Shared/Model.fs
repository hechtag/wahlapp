namespace Model

open System

type WaehlerId = W of Guid
type KandidatId = K of Guid

type WaehlerStatus = NichtGewaehlt | Gewaehlt of KandidatId |  Vertraut of WaehlerId
type Waehler = {
    Id: WaehlerId
    Name: string
    Status: WaehlerStatus
}

module Waehler =
    let create (name: string) = {
        Id = W(Guid.NewGuid())
        Name = name
        Status =  NichtGewaehlt
    }

    let Wa (W id) = id
    let WC (id: Guid) = W id
    let parse (str: string) = (Guid.Parse str) |> WC


    let optBoolK (k: KandidatId) (w: Waehler)  :bool =
        match w.Status with
        | Gewaehlt id -> id = k
        | Vertraut _ | NichtGewaehlt -> false

    let optBoolV (k: WaehlerId) (w: Waehler)  :bool =
        match w.Status with
        | Vertraut id -> id = k
        | Gewaehlt _ | NichtGewaehlt -> false

type Kandidat = { Id: KandidatId; Name: string }

module Kandidat =
    let create (name: string) = { Id = K(Guid.NewGuid()); Name = name }
    let Ka (K id) = id
    let KC id = K id
    let parse (str: string) = (Guid.Parse str) |> KC

type KandidatenAgg = { Id: KandidatId; Anzahl: int }

type Auswertung = { Kandidaten: KandidatenAgg list }