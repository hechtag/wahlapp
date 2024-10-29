open LiteDB
open Entity
open Dto
open LiteDB
open LiteDB.FSharp
open LiteDB.FSharp.Extensions

// let mapper = FSharpBsonMapper()
use db = new LiteDatabase("simple.db") //, mapper)


let col = db.GetCollection<KandidatDto>("test")
let jeff = Kandidat.create "Jeff" |> Dto.FromKandidat
col.Insert(jeff)

let asdf = col.FindAll() |> Seq.toList
asdf |> List.iter (fun j -> printfn "%A" j)