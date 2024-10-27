module CreateWahl

open Feliz
open Elmish
open SAFE
open Shared
open System


type Model = {
    Waehler: RemoteData<Waehler list>
    Kandidaten: RemoteData<Kandidat list>
    Auswertung: RemoteData<Auswertung>
}



type Msg =
    | LoadData of ApiCall<unit, Waehler list * Kandidat list * Auswertung>
    | Waehlen of ApiCall<Guid * Guid, Waehler list * Auswertung>


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
                    Waehler = Loaded waehler
                    Kandidaten = Loaded kandidaten
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
                    Waehler = Loaded waehler
                    Auswertung = Loaded auswertung
            },
            Cmd.none

let get (kandidatenList: Kandidat list) (kandidatId: Guid option) : string =
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
                                                          dispatch (Waehlen(Start(Guid.Parse(r), waehler.Id))))
                                                      prop.children [
                                                          Html.option [ prop.text "nix"; prop.value (None.ToString()) ]
                                                          for kandidat in kandidatenList do
                                                              Html.option [
                                                                  prop.text kandidat.Name
                                                                  prop.value kandidat.Id
                                                                  prop.selected (
                                                                      waehler.KandidatId
                                                                      |> Option.map (fun kId -> kandidat.Id = kId)
                                                                      |> Option.defaultValue false
                                                                  )
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
                        match model.Auswertung with
                        | Loaded auswertung ->
                            for kandidat in auswertung.Kandidaten do
                                Html.li [
                                    prop.style [
                                        style.display.flex
                                        style.justifyContent.spaceBetween
                                        style.gap 10
                                        style.alignItems.baseline
                                    ]
                                    prop.className "my-1"
                                    prop.children[Html.div[prop.text kandidat.Name]
                                                  Html.div [ prop.text (kandidat.Anzahl.ToString()) ]]
                                ]
                        | NotStarted -> Html.text "Not Started."
                        | Loading -> Html.text "Loading..."
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