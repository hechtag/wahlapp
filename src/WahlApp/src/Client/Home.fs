module Home

open SAFE
open Shared
open Elmish
open System


type Model = { WahlInput: string; Wahl: Wahl option }

type Msg =
    | SetNameWahl of string
    | CreateWahl of ApiCall<string, Wahl>
    | LoadData of ApiCall<unit, Kandidat list>

let api = Api.makeProxy<IApi> ()

let init () =
    let initialModel = { WahlInput = ""; Wahl = None }

    let initialCmd = LoadData(Start()) |> Cmd.ofMsg

    initialModel, initialCmd

let update message model =
    match message with
    | SetNameWahl value -> { model with WahlInput = value }, Cmd.none
    | LoadData msg ->
        match msg with
        | Start() -> model, Cmd.none
        | Finished(_) -> model, Cmd.none
    | CreateWahl msg ->
        match msg with
        | Start name ->
            let createWahlCmd =
                let wahl = Wahl.create name
                Cmd.OfAsync.perform api.createWahl wahl (Finished >> CreateWahl)

            { model with WahlInput = "" }, createWahlCmd
        | Finished wahl -> { model with Wahl = Some wahl }, Cmd.none

open Feliz

module ViewComponents =
    let createWahl model dispatch =
        Html.div [
            prop.className "flex flex-col sm:flex-row mt-4 gap-4"
            prop.children [
                Html.input [
                    prop.className
                        "shadow appearance-none border rounded w-full py-2 px-3 outline-none focus:ring-2 ring-teal-300 text-grey-darker"
                    prop.value model.WahlInput
                    prop.placeholder "Wahl Name"
                    prop.autoFocus true
                    prop.onChange (SetNameWahl >> dispatch)
                    prop.onKeyPress (fun ev ->
                        if ev.key = "Enter" then
                            dispatch (CreateWahl(Start model.WahlInput)))
                ]
                Html.button [
                    prop.className
                        "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed"
                    prop.disabled (model.WahlInput |> fun i -> String.IsNullOrWhiteSpace i)
                    prop.onClick (fun _ -> dispatch (CreateWahl(Start model.WahlInput)))
                    prop.text "Add"
                ]
                Html.h1 (model.Wahl |> Option.map _.Name |> Option.defaultValue "")
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
            Html.div [
                prop.className "flex flex-col items-center justify-center h-full"
                prop.children [
                    Html.h1 [
                        prop.className "text-center text-5xl font-bold text-white mb-3 rounded-md p-4"
                        prop.text "WahlApp"
                    ]
                    ViewComponents.createWahl model dispatch
                ]
            ]
        ]
    ]