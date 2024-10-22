module CreateWahl

open Feliz
open Elmish


type Model = { id: string }



type Msg = None
let init what = { id = "" }, Cmd.none
let view model dispatch = Html.div [ prop.text "create Wahl" ]