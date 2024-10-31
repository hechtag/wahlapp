module Api

open System
open Model
open DbEntity


type IApi = {
    getKandidaten: unit -> Async<KandidatDb list>
    addKandidat: KandidatDb -> Async<KandidatDb>
    deleteKandidat: KandidatId -> Async<KandidatDb list>

    getWaehlers: unit -> Async<WaehlerDb list>
    addWaehler: WaehlerDb -> Async<WaehlerDb>
    deleteWaehler: WaehlerId -> Async<WaehlerDb list>

    waehlen: KandidatId * WaehlerId -> Async<WaehlerDb list>
    verteilen: WaehlerId * WaehlerId -> Async<WaehlerDb list>
    getAuswertung: unit -> Async<Auswertung>
    getVertraute: unit -> Async<(WaehlerId * Waehler list) list>
}