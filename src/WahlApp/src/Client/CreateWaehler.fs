module CreateWaehler

open Feliz
open Elmish
open SAFE
open Shared
open System

type Model = {
    Waehler: RemoteData<Waehler list>
    WaehlerInput: string
}



type Msg =
    | SetInputWaehler of string
    | SaveWaehler of ApiCall<string, Waehler>
    | LoadData of ApiCall<unit, Waehler list>
    | DeleteWaehler of ApiCall<Guid, Waehler list>

let api = Api.makeProxy<IApi> ()

let init () =
    let initialModel = {
        Waehler = NotStarted
        WaehlerInput = ""
    }

    let initialCmd = LoadData(Start()) |> Cmd.ofMsg

    initialModel, initialCmd

let update msg model =
    match msg with
    | SetInputWaehler value -> { model with WaehlerInput = value }, Cmd.none
    | LoadData msg ->
        match msg with
        | Start() ->
            let cmd = Cmd.OfAsync.perform api.getWaehlers () (Finished >> LoadData)

            { model with Waehler = Loading }, cmd
        | Finished(waehler) -> { model with Waehler = Loaded waehler }, Cmd.none
    | SaveWaehler msg ->
        match msg with
        | Start text ->
            let cmd =
                let waehler = Waehler.create text
                Cmd.OfAsync.perform api.addWaehler waehler (Finished >> SaveWaehler)

            { model with WaehlerInput = "" }, cmd
        | Finished waehler ->
            {
                model with
                    Waehler = model.Waehler |> RemoteData.map (fun waehlerList -> waehlerList @ [ waehler ])
            },
            Cmd.none
    | DeleteWaehler msg ->
        match msg with
        | Start id ->
            let cmd = Cmd.OfAsync.perform api.deleteWaehler id (Finished >> DeleteWaehler)
            { model with Waehler = Loading }, cmd
        | Finished list -> { model with Waehler = Loaded list }, Cmd.none

module ViewComponents =
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
                        | Loaded waehlerList ->
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

                                                  Html.button[prop.className
                                                                  "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed"



                                                              prop.onClick (fun _ ->
                                                                  dispatch (DeleteWaehler(Start waehler.Id)))

                                                              prop.text "Delete"]]
                                ]
                    ]
                ]

                waehlerAction model dispatch
            ]
        ]

let view model dispatch =
    ViewComponents.waehlerList model dispatch