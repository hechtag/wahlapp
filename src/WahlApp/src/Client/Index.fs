module Index

open Elmish
open SAFE
open Shared
open System

type Model = {
    Waehler: RemoteData<Waehler list>
    Kandidaten: RemoteData<Kandidat list>
    WaehlerInput: string
    KandidatInput: string
}

type Msg =
    | SetInputWaehler of string
    | SetInputKandidat of string
    | LoadData of ApiCall<unit, Waehler list * Kandidat list>
    | SaveWaehler of ApiCall<string, Waehler>
    | SaveKandidat of ApiCall<string, Kandidat>

let waehlerApi = Api.makeProxy<IWaehlerApi> ()
let kandidatApi = Api.makeProxy<IKandidatApi> ()

let init () =
    let initialModel = {
        Waehler = NotStarted
        WaehlerInput = ""
        Kandidaten = NotStarted
        KandidatInput = ""
    }

    let initialCmd = LoadData(Start()) |> Cmd.ofMsg

    initialModel, initialCmd

let update msg model =
    match msg with
    | SetInputWaehler value -> { model with WaehlerInput = value }, Cmd.none
    | SetInputKandidat value -> { model with KandidatInput = value }, Cmd.none
    | LoadData msg ->
        match msg with
        | Start() ->
            let asdf =
                fun () -> async {
                    let! waehler = waehlerApi.getWaehlers ()
                    let! kandidaten = kandidatApi.getKandidaten ()
                    return waehler, kandidaten
                }


            let qwer = Cmd.OfAsync.perform asdf () (Finished >> LoadData)

            {
                model with
                    Waehler = Loading
                    Kandidaten = Loading
            },
            qwer
        | Finished(waehler, kandidaten) ->
            {
                model with
                    Waehler = Loaded waehler
                    Kandidaten = Loaded kandidaten
            },
            Cmd.none
    | SaveWaehler msg ->
        match msg with
        | Start todoText ->
            let saveTodoCmd =
                let todo = Waehler.create todoText
                Cmd.OfAsync.perform waehlerApi.addWaehler todo (Finished >> SaveWaehler)

            { model with WaehlerInput = "" }, saveTodoCmd
        | Finished todo ->
            {
                model with
                    Waehler = model.Waehler |> RemoteData.map (fun todos -> todos @ [ todo ])
            },
            Cmd.none
    | SaveKandidat msg ->
        match msg with
        | Start todoText ->
            let saveTodoCmd =
                let todo = Kandidat.create todoText
                Cmd.OfAsync.perform kandidatApi.addKandidat todo (Finished >> SaveKandidat)

            { model with KandidatInput = "" }, saveTodoCmd
        | Finished todo ->
            {
                model with
                    Kandidaten = model.Kandidaten |> RemoteData.map (fun todos -> todos @ [ todo ])
            },
            Cmd.none

open Feliz

module ViewComponents =
    let kandidatenAction model dispatch =
        Html.div [
            prop.className "flex flex-col sm:flex-row mt-4 gap-4"
            prop.children [
                Html.input [
                    prop.className
                        "shadow appearance-none border rounded w-full py-2 px-3 outline-none focus:ring-2 ring-teal-300 text-grey-darker"
                    prop.value model.KandidatInput
                    prop.placeholder "What needs to be done?"
                    prop.autoFocus true
                    prop.onChange (SetInputKandidat >> dispatch)
                    prop.onKeyPress (fun ev ->
                        if ev.key = "Enter" then
                            dispatch (SaveKandidat(Start model.KandidatInput)))
                ]
                Html.button [
                    prop.className
                        "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed"
                    prop.disabled (model.KandidatInput |> fun i -> String.IsNullOrWhiteSpace i)
                    prop.onClick (fun _ -> dispatch (SaveKandidat(Start model.KandidatInput)))
                    prop.text "Add"
                ]
            ]
        ]

    let waehlerAction model dispatch =
        Html.div [
            prop.className "flex flex-col sm:flex-row mt-4 gap-4"
            prop.children [
                Html.input [
                    prop.className
                        "shadow appearance-none border rounded w-full py-2 px-3 outline-none focus:ring-2 ring-teal-300 text-grey-darker"
                    prop.value model.WaehlerInput
                    prop.placeholder "What needs to be done?"
                    prop.autoFocus true
                    prop.onChange (SetInputWaehler >> dispatch)
                    prop.onKeyPress (fun ev ->
                        if ev.key = "Enter" then
                            dispatch (SaveWaehler(Start model.WaehlerInput)))
                ]
                Html.button [
                    prop.className
                        "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed"
                    prop.disabled (model.WaehlerInput |> fun i -> String.IsNullOrWhiteSpace i)
                    prop.onClick (fun _ -> dispatch (SaveWaehler(Start model.WaehlerInput)))
                    prop.text "Add"
                ]
            ]
        ]

    let kandidatenList model dispatch =
        Html.div [
            prop.className "bg-white/80 rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl"
            prop.children [
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

                kandidatenAction model dispatch
            ]
        ]

    let waehlerList model dispatch =
        Html.div [
            prop.className "bg-white/80 rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl"
            prop.children [
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

                waehlerAction model dispatch
            ]
        ]

let view model dispatch =
    Html.section [
        prop.className "h-screen w-screen"
        prop.style [
            style.backgroundSize "cover"
            style.backgroundImageUrl "https://unsplash.it/1200/900?random"
            style.backgroundPosition "no-repeat center center fixed"
        ]

        prop.children [
            Html.a [
                prop.href "https://safe-stack.github.io/"
                prop.className "absolute block ml-12 h-12 w-12 bg-teal-300 hover:cursor-pointer hover:bg-teal-400"
                prop.children [ Html.img [ prop.src "/favicon.png"; prop.alt "Logo" ] ]
            ]

            Html.div [
                prop.className "flex flex-col items-center justify-center h-full"
                prop.children [
                    Html.h1 [
                        prop.className "text-center text-5xl font-bold text-white mb-3 rounded-md p-4"
                        prop.text "WahlApp"
                    ]
                    ViewComponents.waehlerList model dispatch
                    ViewComponents.kandidatenList model dispatch
                ]
            ]
        ]
    ]