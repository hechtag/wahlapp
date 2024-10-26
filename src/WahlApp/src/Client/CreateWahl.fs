module CreateWahl

open Feliz
open Elmish
open SAFE
open Shared


type Model = {
    Waehler: RemoteData<Waehler list>
    Kandidaten: RemoteData<Kandidat list>
}



type Msg = LoadData of ApiCall<unit, Waehler list * Kandidat list>


let init () =
    let initialModel = {
        Waehler = NotStarted
        Kandidaten = NotStarted
    }

    let initialCmd = LoadData(Start()) |> Cmd.ofMsg

    initialModel, initialCmd


let api = Api.makeProxy<IApi> ()

let update msg model =
    match msg with
    | LoadData msg ->
        match msg with
        | Start() ->
            let tasks () = async {
                let! waehler = api.getWaehlers ()
                let! kandidaten = api.getKandidaten ()
                return waehler, kandidaten
            }

            let cmd = Cmd.OfAsync.perform tasks () (Finished >> LoadData)

            {
                model with
                    Waehler = Loading
                    Kandidaten = Loading
            },
            cmd
        | Finished(waehler, kandidaten) ->
            {
                model with
                    Waehler = Loaded waehler
                    Kandidaten = Loaded kandidaten
            },
            Cmd.none

module ViewComponents =
    let waehlerList model dispatch =
        Html.div [
            prop.className "bg-white/80 rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl"
            prop.children [
                Html.h1 [ prop.text "Wähler" ]
                Html.ol [
                    prop.className "list-decimal ml-6"
                    prop.children [
                        match model.Waehler with
                        | NotStarted -> Html.text "Not Started."
                        | Loading -> Html.text "Loading..."
                        | Loaded todos ->
                            for todo in todos do
                                Html.li [ prop.className "my-1"; prop.text todo.Name ]
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
                        | Loaded todos ->
                            for todo in todos do
                                Html.li [ prop.className "my-1"; prop.text todo.Name ]
                    ]
                ]
            ]
        ]

let view model dispatch =
    Html.div [
        prop.children[ViewComponents.waehlerList model dispatch
                      ViewComponents.kandidatenList model dispatch]
    ]