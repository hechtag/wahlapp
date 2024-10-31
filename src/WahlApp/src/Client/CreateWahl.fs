module CreateWahl

open Feliz
open Elmish
open SAFE
open Model
open System
open Api
open DbEntity

type Model = {
    Waehler: RemoteData<Waehler list>
    Kandidaten: RemoteData<Kandidat list>
    Auswertung: RemoteData<Auswertung>
    Vertraute: RemoteData< (WaehlerId * Waehler list) list>
}

type Msg =
    | LoadData of ApiCall<unit, WaehlerDb list * KandidatDb list * Auswertung * (WaehlerId * Waehler list) list>
    | Waehlen of ApiCall<KandidatId * WaehlerId, WaehlerDb list * Auswertung * (WaehlerId * Waehler list) list>
    | Verteilen of ApiCall<WaehlerId * WaehlerId, WaehlerDb list * Auswertung * (WaehlerId * Waehler list) list>

let init () =
    let initialModel = {
        Waehler = NotStarted
        Kandidaten = NotStarted
        Auswertung = NotStarted
        Vertraute =  NotStarted
    }

    let initialCmd = LoadData(Start()) |> Cmd.ofMsg

    initialModel, initialCmd


let api = Api.makeProxy<IApi> ()

let refreshPage () = async {
    let! waehler = api.getWaehlers ()
    let! kandidaten = api.getKandidaten ()
    let! auswertung = api.getAuswertung ()
    let! vertraute = api.getVertraute ()
    return waehler, kandidaten, auswertung, vertraute
}

let update msg model =
    match msg with
    | LoadData msg ->
        match msg with
        | Start() ->

            let cmd = Cmd.OfAsync.perform refreshPage () (Finished >> LoadData)

            {
                model with
                    Waehler = Loading
                    Kandidaten = Loading
                    Auswertung = Loading
                    Vertraute = Loading
            },
            cmd
        | Finished(waehler, kandidaten, auswertung, vertraute) ->
            {
                model with
                    Waehler = Loaded (waehler |> List.map Db.ToWaehler)
                    Kandidaten = Loaded(kandidaten |> List.map Db.ToKandidat)
                    Auswertung = Loaded auswertung
                    Vertraute =  Loaded vertraute
            },
            Cmd.none
    | Waehlen msg ->
        match msg with
        | Start(kandidatId, waehlerId) ->

            let tasks () = async {
                let! waehlerList = api.waehlen (kandidatId, waehlerId)
                let! auswertung = api.getAuswertung ()
                let! vertraute = api.getVertraute ()
                return waehlerList, auswertung, vertraute
            }

            let cmd = Cmd.OfAsync.perform tasks () (Finished >> Waehlen)

            { model with Waehler = Loading }, cmd
        | Finished(waehler, auswertung, vertraute) ->
            {
                model with
                    Waehler =  waehler |> List.map Db.ToWaehler |> Loaded
                    Auswertung = Loaded auswertung
                    Vertraute = Loaded vertraute
            },
            Cmd.none
    | Verteilen msg ->
        match msg with
        | Start(verteilerId, waehlerId) ->

            let tasks () = async {
                let! waehlerList = api.verteilen (verteilerId, waehlerId)
                let! auswertung = api.getAuswertung ()
                let! vertraute = api.getVertraute ()

                return waehlerList, auswertung, vertraute
            }

            let cmd = Cmd.OfAsync.perform tasks () (Finished >> Waehlen)

            { model with Waehler = Loading }, cmd
        | Finished(waehler, auswertung, vertraute) ->
            {
                model with
                    Waehler =  waehler |> List.map Db.ToWaehler |> Loaded
                    Auswertung = Loaded auswertung
                    Vertraute = Loaded vertraute
            },
            Cmd.none

let get (kandidatenList: Kandidat list) (kandidatId: KandidatId option) : string =
    kandidatId
    |> Option.bind (fun id -> kandidatenList |> List.tryFind (fun k -> k.Id = id))
    |> Option.map _.Name
    |> Option.defaultValue "nix"

let getVertraute (waehlerId: WaehlerId) (vertrautenList: (WaehlerId * Waehler list) list): Waehler list=
    let list =vertrautenList |> List.find (fun (id,l) -> id = waehlerId)
    list |> snd

module ViewComponents =
    let waehlerList model dispatch =
        Html.div [
            prop.className "bg-white/80 rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl"
            prop.children [
                Html.h1 [ prop.text "Wähler" ]
                Html.ol [
                    prop.className "list-decimal ml-6"
                    prop.children [
                        match model.Waehler, model.Kandidaten, model.Vertraute with
                        | Loaded waehlerList, Loaded kandidatenList, Loaded vertrautenList  ->
                            for waehler in waehlerList do
                                Html.li [
                                    prop.style [
                                        style.display.flex
                                        style.justifyContent.spaceBetween
                                        style.gap 10
                                        style.alignItems.baseline
                                    ]
                                    prop.className "my-1"
                                    prop.children[Html.div[prop.text waehler.Name]

                                                  Html.select [
                                                      prop.onChange (fun (r: string) ->
                                                          dispatch (Waehlen(Start(Kandidat.parse r, waehler.Id))))
                                                      prop.children [
                                                          Html.option [ prop.text "nix"; prop.value (None.ToString()) ]
                                                          for kandidat in kandidatenList do
                                                              Html.option [
                                                                  prop.text kandidat.Name
                                                                  prop.value (kandidat.Id |> Kandidat.Ka)
                                                                  prop.selected (
                                                                      waehler |> Waehler.optBoolK kandidat.Id
                                                                  )
                                                              ]
                                                      ]
                                                  ]

                                                  Html.select [
                                                      prop.onChange (fun (r: string) ->
                                                          dispatch (Verteilen(Start(Waehler.parse r, waehler.Id))))
                                                      prop.children [
                                                          Html.option [ prop.text "nix"; prop.value (None.ToString()) ]
                                                          for wa in vertrautenList |> getVertraute waehler.Id do
                                                              Html.option [
                                                                  prop.text wa.Name
                                                                  prop.value (wa.Id |> Waehler.Wa)
                                                                  prop.selected ( waehler |> Waehler.optBoolV wa.Id )
                                                              ]
                                                      ]
                                                  ]]
                                ]
                        | NotStarted, _, _
                        | _, NotStarted, _
                        | _, _, NotStarted -> Html.text "Not Started."
                        | Loading, _, _
                        | _, Loading, _
                        | _,_, Loading -> Html.text "Loading..."
                    ]
                ]
            ]
        ]
    let auswertung model dispatch =
        Html.div [
            prop.className "bg-white/80 rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl"
            prop.children [
                Html.h1 [ prop.text "Auswertung" ]
                Html.ol [
                    prop.className "list-decimal ml-6"
                    prop.children [
                        match (model.Auswertung, model.Kandidaten) with
                        | Loaded auswertung, Loaded kandidatenList ->
                            for kandidat in auswertung.Kandidaten do
                                Html.li [
                                    prop.style [
                                        style.display.flex
                                        style.justifyContent.spaceBetween
                                        style.gap 10
                                        style.alignItems.baseline
                                    ]
                                    prop.className "my-1"
                                    prop.children[Html.div[prop.text (kandidatenList |> List.find (fun k -> k.Id= kandidat.Id)).Name]
                                                  Html.div [ prop.text (kandidat.Anzahl.ToString()) ]]
                                ]
                        | NotStarted ,_ | _ , NotStarted-> Html.text "Not Started."
                        | Loading , _ | _ , Loading-> Html.text "Loading..."
                    ]
                ]
            ]
        ]

let view model dispatch =
    Html.div [
        prop.children[ViewComponents.waehlerList model dispatch
                      ViewComponents.auswertung model dispatch]
    ]