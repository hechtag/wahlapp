module HtmlU

open Feliz


let divT (text: string) = Html.div [ prop.text text ]