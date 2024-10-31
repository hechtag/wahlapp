module Nav

open Feliz

let view =
    Html.section [
        prop.style [
            style.height 15
            style.display.flex
            style.justifyContent.flexStart
            style.gap 10
        ]

        prop.children [
            Html.a [ prop.href "/"; prop.text "Home" ]
            Html.a [ prop.href "/create-waehler"; prop.text "create Wähler" ]
            Html.a [ prop.href "/create-wahl"; prop.text "create Wahl" ]
            Html.a [ prop.href "/create-kandidat"; prop.text "create Kandidat" ]
        ]
    ]