module Api

open Entity
open Dto


type IApi = {
    getKandidaten: unit -> Async<KandidatDto list>
    addKandidat: KandidatDto -> Async<KandidatDto>
    deleteKandidat: KandidatId -> Async<KandidatDto list>

    getWaehlers: unit -> Async<Waehler list>
    addWaehler: Waehler -> Async<Waehler>
    deleteWaehler: WaehlerId -> Async<Waehler list>

    waehlen: KandidatId * WaehlerId -> Async<Waehler list>
    verteilen: WaehlerId * WaehlerId -> Async<Waehler list>
    getAuswertung: unit -> Async<Auswertung>
}