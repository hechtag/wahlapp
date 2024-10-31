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
}



type Msg =
    | LoadData of ApiCall<unit, WaehlerDb list * KandidatDb list * Auswertung>
    | Waehlen of ApiCall<KandidatId * WaehlerId, WaehlerDb list * Auswertung>
    | Verteilen of ApiCall<WaehlerId * WaehlerId, WaehlerDb list * Auswertung>


let init () =
    let initialModel = {
        Waehler = NotStarted
        Kandidaten = NotStarted
        Auswertung = NotStarted
    }

    let initialCmd = LoadData(Start()) |> Cmd.ofMsg

    initialModel, initialCmd


let api = Api.makeProxy<IApi> ()

let refreshPage () = async {
    let! waehler = api.getWaehlers ()
    let! kandidaten = api.getKandidaten ()
    let! auswertung = api.getAuswertung ()
    return waehler, kandidaten, auswertung
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
            },
            cmd
        | Finished(waehler, kandidaten, auswertung) ->
            {
                model with
                    Waehler = Loaded (waehler |> List.map Db.ToWaehler)
                    Kandidaten = Loaded(kandidaten |> List.map Db.ToKandidat)
                    Auswertung = Loaded auswertung
            },
            Cmd.none
    | Waehlen msg ->
        match msg with
        | Start(kandidatId, waehlerId) ->

            let tasks () = async {
                let! waehlerList = api.waehlen (kandidatId, waehlerId)
                let! auswertung = api.getAuswertung ()
                return waehlerList, auswertung
            }

            let cmd = Cmd.OfAsync.perform tasks () (Finished >> Waehlen)

            { model with Waehler = Loading }, cmd
        | Finished(waehler, auswertung) ->
            {
                model with
                    Waehler =  waehler |> List.map Db.ToWaehler |> Loaded
                    Auswertung = Loaded auswertung
            },
            Cmd.none
    | Verteilen msg ->
        match msg with
        | Start(verteilerId, waehlerId) ->

            let tasks () = async {
                let! waehlerList = api.verteilen (verteilerId, waehlerId)
                let! auswertung = api.getAuswertung ()
                return waehlerList, auswertung
            }

            let cmd = Cmd.OfAsync.perform tasks () (Finished >> Waehlen)

            { model with Waehler = Loading }, cmd
        | Finished(waehler, auswertung) ->
            {
                model with
                    Waehler =  waehler |> List.map Db.ToWaehler |> Loaded
                    Auswertung = Loaded auswertung
            },
            Cmd.none

let get (kandidatenList: Kandidat list) (kandidatId: KandidatId option) : string =
    kandidatId
    |> Option.bind (fun id -> kandidatenList |> List.tryFind (fun k -> k.Id = id))
    |> Option.map _.Name
    |> Option.defaultValue "nix"

module ViewComponents =
    let waehlerList model dispatch =
        Html.div [
            prop.className "bg-white/80 rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl"
            prop.children [
                Html.h1 [ prop.text "Wähler" ]
                Html.ol [
                    prop.className "list-decimal ml-6"
                    prop.children [
                        match model.Waehler, model.Kandidaten with
                        | Loaded waehlerList, Loaded kandidatenList ->
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
                                                          for wa in
                                                              waehlerList |> List.filter (fun w -> w.Id <> waehler.Id) do
                                                              Html.option [
                                                                  prop.text wa.Name
                                                                  prop.value (wa.Id |> Waehler.Wa)
                                                                  prop.selected ( waehler |> Waehler.optBoolV wa.Id )
                                                              ]
                                                      ]
                                                  ]]
                                ]
                        | NotStarted, _
                        | _, NotStarted -> Html.text "Not Started."
                        | Loading, _
                        | _, Loading -> Html.text "Loading..."
                    ]
                ]
            ]
        ]

    let kandidatenList model dispatch =
        Html.div [
            prop.className "bg-white/80 rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl"
            prop.children [
                Html.h1 [ prop.text "Kandidaten" ]
                Html.ol [
                    prop.className "list-decimal ml-6"
                    prop.children [
                        match model.Kandidaten with
                        | NotStarted -> Html.text "Not Started."
                        | Loading -> Html.text "Loading..."
                        | Loaded kandidaten ->
                            for kandidat in kandidaten do
                                Html.li [ prop.className "my-1"; prop.text kandidat.Name ]
                    ]
                ]
            ]
        ]

    let auswertung model dispatch =
        Html.div [
            prop.className "bg-white/80 rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl"
            prop.children [
                Html.h1 [ prop.text "Wähler" ]
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
                      ViewComponents.kandidatenList model dispatch
                      ViewComponents.auswertung model dispatch]
    ]