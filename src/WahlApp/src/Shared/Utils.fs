module Utils



let mapAndDefault m d o =
    o |> Option.map m |> Option.defaultValue d