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
            Html.a [ prop.href "/page"; prop.text "link" ]
        ]
    ]