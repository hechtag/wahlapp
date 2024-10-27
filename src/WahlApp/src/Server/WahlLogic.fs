module WahlLogic

open Shared


let waehlen (kandidatId, waehlerId) : Async<Waehler list> = async {
    let waehler = Db.getById WaehlerLogic.colName waehlerId

    let newWaehler = {
        waehler with
            KandidatId = Some kandidatId
    }

    let asdf = Db.update WaehlerLogic.colName newWaehler
    return! WaehlerLogic.getWaehlers ()
}