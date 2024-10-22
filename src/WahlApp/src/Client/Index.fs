module Index

open Elmish
open Feliz
open Feliz.Router

type Url =
    | Home
    | CreateWahl

type Page =
    | Home of Home.Model
    | CreateWahl of CreateWahl.Model
    | NotFound


type Model = {
    CurrentUrl: Url option
    CurrentPage: Page
}

type Msg =
    | HomeMsg of Home.Msg
    | CreateWahlMsg of CreateWahl.Msg
    | UrlChanged of Url option


let tryParseUrl =
    function
    | []
    | [ "home" ] -> Some Url.Home
    | [ "create-wahl" ] -> Some Url.CreateWahl
    | _ -> None


let initPage url =
    match url with
    | Some Url.Home ->
        let model, msg = Home.init ()

        {
            CurrentUrl = url
            CurrentPage = (Page.Home model)
        },
        msg |> Cmd.map HomeMsg
    | Some Url.CreateWahl ->
        let model, msg = CreateWahl.init ()

        {
            CurrentUrl = url
            CurrentPage = (Page.CreateWahl model)
        },
        msg |> Cmd.map CreateWahlMsg
    | None ->
        {
            CurrentUrl = url
            CurrentPage = Page.NotFound
        },
        Cmd.none


let init () : Model * Cmd<Msg> =
    Router.currentPath () |> tryParseUrl |> initPage


let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg, model.CurrentPage with
    | HomeMsg page1Msg, Page.Home page1Model ->
        let newPage1Model, newPage1Msg = Home.update page1Msg page1Model

        {
            model with
                CurrentPage = Page.Home newPage1Model
        },
        newPage1Msg |> Cmd.map HomeMsg
    | UrlChanged urlSegments, _ -> initPage urlSegments
    | _ -> model, Cmd.none



let view (model: Model) (dispatch: Msg -> unit) =
    React.router [
        router.pathMode
        router.onUrlChanged (tryParseUrl >> UrlChanged >> dispatch)
        router.children [
            match model.CurrentPage with
            | Page.Home homeModel -> Home.view homeModel (HomeMsg >> dispatch)
            | Page.CreateWahl wahlModel -> CreateWahl.view wahlModel (CreateWahlMsg >> dispatch)
            | Page.NotFound -> Html.p "Not found"
        ]
    ]