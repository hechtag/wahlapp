module CreateKandidat

open Feliz
open Elmish
open SAFE
open Shared
open System

type Model = {
    Kandidaten: RemoteData<Kandidat list>
    KandidatInput: string
}

type Msg =
    | LoadData of ApiCall<unit, Kandidat list>
    | SetInputKandidat of string
    | SaveKandidat of ApiCall<string, Kandidat>
    | DeleteKandidat of ApiCall<Guid, Kandidat list>

let api = Api.makeProxy<IApi> ()

let init () =
    let initialModel = {
        Kandidaten = NotStarted
        KandidatInput = ""
    }

    let initialCmd = LoadData(Start()) |> Cmd.ofMsg

    initialModel, initialCmd


let update message model =
    match message with
    | SetInputKandidat value -> { model with KandidatInput = value }, Cmd.none
    | LoadData msg ->
        match msg with
        | Start() ->
            let qwer = Cmd.OfAsync.perform api.getKandidaten () (Finished >> LoadData)

            { model with Kandidaten = Loading }, qwer
        | Finished(kandidaten) ->
            {
                model with
                    Kandidaten = Loaded kandidaten
            },
            Cmd.none
    | SaveKandidat msg ->
        match msg with
        | Start text ->
            let cmd =
                let kandidat = Kandidat.create text
                Cmd.OfAsync.perform api.addKandidat kandidat (Finished >> SaveKandidat)

            { model with KandidatInput = "" }, cmd
        | Finished kandidat ->
            {
                model with
                    Kandidaten = model.Kandidaten |> RemoteData.map (fun kandidaten -> kandidaten @ [ kandidat ])
            },
            Cmd.none
    | DeleteKandidat msg ->
        match msg with
        | Start id ->
            let cmd = Cmd.OfAsync.perform api.deleteKandidat id (Finished >> DeleteKandidat)

            { model with Kandidaten = Loading }, cmd
        | Finished kandidaten ->
            {
                model with
                    Kandidaten = Loaded kandidaten
            },
            Cmd.none

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
                        | Loaded kandidaten ->
                            for kandidat in kandidaten do
                                Html.li [
                                    prop.style [
                                        style.display.flex
                                        style.justifyContent.spaceBetween
                                        style.gap 10
                                        style.alignItems.baseline
                                    ]
                                    prop.className "my-1"
                                    prop.children[Html.div[prop.text kandidat.Name]

                                                  Html.button[prop.className
                                                                  "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed"



                                                              prop.onClick (fun _ ->
                                                                  dispatch (DeleteKandidat(Start kandidat.Id)))

                                                              prop.text "Delete"]]
                                ]
                    ]
                ]

                kandidatenAction model dispatch
            ]
        ]


let view model dispatch =
    ViewComponents.kandidatenList model dispatch