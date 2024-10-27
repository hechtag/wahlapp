module CreateWahl

open Feliz
open Elmish
open SAFE
open Shared
open System


type Model = {
    Waehler: RemoteData<Waehler list>
    Kandidaten: RemoteData<Kandidat list>
}



type Msg =
    | LoadData of ApiCall<unit, Waehler list * Kandidat list>
    | Waehlen of ApiCall<Guid * Guid, Waehler list>


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
    | Waehlen msg ->
        match msg with
        | Start(kandidatId, waehlerId) ->
            let cmd =
                Cmd.OfAsync.perform api.waehlen (kandidatId, waehlerId) (Finished >> Waehlen)

            { model with Waehler = Loading }, cmd
        | Finished(waehler) -> { model with Waehler = Loaded waehler }, Cmd.none

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
                                                  Html.div[prop.text (get kandidatenList waehler.KandidatId)]

                                                  Html.select [
                                                      prop.onChange (fun (r: string) ->
                                                          dispatch (Waehlen(Start(Guid.Parse(r), waehler.Id))))
                                                      prop.children [
                                                          for kandidat in kandidatenList do
                                                              Html.option [
                                                                  prop.text kandidat.Name
                                                                  prop.value kandidat.Id
                                                              ]
                                                      ]
                                                  ]]
                                ]
                        | NotStarted, _ -> Html.text "Not Started."
                        | _, NotStarted -> Html.text "Not Started."
                        | Loading, _ -> Html.text "Loading..."
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

let view model dispatch =
    Html.div [
        prop.children[ViewComponents.waehlerList model dispatch
                      ViewComponents.kandidatenList model dispatch]
    ]