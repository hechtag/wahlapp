module Index

open Elmish
open Feliz
open Feliz.Router

type Url =
    | CreateWahl
    | CreateWaehler
    | CreateKandidat

type Page =
    | CreateWahl of CreateWahl.Model
    | CreateWaehler of CreateWaehler.Model
    | CreateKandidat of CreateKandidat.Model
    | NotFound


type Model = {
    CurrentUrl: Url option
    CurrentPage: Page
}

type Msg =
    | CreateWahlMsg of CreateWahl.Msg
    | CreateWaehlerMsg of CreateWaehler.Msg
    | CreateKandidatMsg of CreateKandidat.Msg
    | UrlChanged of Url option


let tryParseUrl =
    function
    | []
    | [ "create-wahl" ] -> Some Url.CreateWahl
    | [ "create-waehler" ] -> Some Url.CreateWaehler
    | [ "create-kandidat" ] -> Some Url.CreateKandidat
    | _ -> None


let initPage url =
    match url with
    | Some Url.CreateWahl ->
        let model, msg = CreateWahl.init ()

        {
            CurrentUrl = url
            CurrentPage = (Page.CreateWahl model)
        },
        msg |> Cmd.map CreateWahlMsg
    | Some Url.CreateWaehler ->
        let model, msg = CreateWaehler.init ()

        {
            CurrentUrl = url
            CurrentPage = (Page.CreateWaehler model)
        },
        msg |> Cmd.map CreateWaehlerMsg
    | Some Url.CreateKandidat ->
        let model, msg = CreateKandidat.init ()

        {
            CurrentUrl = url
            CurrentPage = (Page.CreateKandidat model)
        },
        msg |> Cmd.map CreateKandidatMsg
    | None ->
        {
            CurrentUrl = url
            CurrentPage = Page.NotFound
        },
        Cmd.none


let init () : Model * Cmd<Msg> =
    Router.currentPath () |> tryParseUrl |> initPage


let update (message: Msg) (model: Model) : Model * Cmd<Msg> =
    match message, model.CurrentPage with
    | CreateWaehlerMsg msg, Page.CreateWaehler pageModel ->
        let newPageModel, newPageMsg = CreateWaehler.update msg pageModel

        {
            model with
                CurrentPage = Page.CreateWaehler newPageModel
        },
        newPageMsg |> Cmd.map CreateWaehlerMsg
    | CreateWaehlerMsg _, _ -> model, Cmd.none
    | CreateKandidatMsg msg, Page.CreateKandidat pageModel ->
        let newPageModel, newPageMsg = CreateKandidat.update msg pageModel

        {
            model with
                CurrentPage = Page.CreateKandidat newPageModel
        },
        newPageMsg |> Cmd.map CreateKandidatMsg
    | CreateKandidatMsg _, _ -> model, Cmd.none
    | CreateWahlMsg msg, Page.CreateWahl pageModel ->
        let newPageModel, newPageMsg = CreateWahl.update msg pageModel

        {
            model with
                CurrentPage = Page.CreateWahl newPageModel
        },
        newPageMsg |> Cmd.map CreateWahlMsg
    | CreateWahlMsg _, _ -> model, Cmd.none
    | UrlChanged urlSegments, _ -> initPage urlSegments



let view (model: Model) (dispatch: Msg -> unit) =
    Html.div [
        prop.children[Nav.view

                      React.router [
                          router.pathMode
                          router.onUrlChanged (tryParseUrl >> UrlChanged >> dispatch)
                          router.children [
                              match model.CurrentPage with
                              | Page.CreateWahl m -> CreateWahl.view m (CreateWahlMsg >> dispatch)
                              | Page.CreateWaehler m -> CreateWaehler.view m (CreateWaehlerMsg >> dispatch)
                              | Page.CreateKandidat m -> CreateKandidat.view m (CreateKandidatMsg >> dispatch)
                              | Page.NotFound -> Html.p "Not found"
                          ]
                      ]]
    ]