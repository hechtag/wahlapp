module Index

open Elmish
open Feliz
open Feliz.Router

type Url = | Home

type Page =
    | Home of Home.Model
    | NotFound


type Model = {
    CurrentUrl: Url option
    CurrentPage: Page
}

type Msg =
    | HomeMsg of Home.Msg
    | UrlChanged of Url option


let tryParseUrl =
    function
    | []
    | [ "home" ] -> Some Url.Home
    | _ -> None


let initPage url =
    match url with
    | Some Url.Home ->
        let page1Model, page1Msg = Home.init ()

        {
            CurrentUrl = url
            CurrentPage = (Page.Home page1Model)
        },
        page1Msg |> Cmd.map HomeMsg
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
            | Page.NotFound -> Html.p "Not found"
        ]
    ]