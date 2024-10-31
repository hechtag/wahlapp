open LiteDB
open Model
open DbEntity
open LiteDB
open LiteDB.FSharp
open LiteDB.FSharp.Extensions

// let mapper = FSharpBsonMapper()
use db = new LiteDatabase("simple.db") //, mapper)


let col = db.GetCollection<KandidatDb>("test")
let jeff = Kandidat.create "Jeff" |> Db.FromKandidat
col.Insert(jeff)

let asdf = col.FindAll() |> Seq.toList
asdf |> List.iter (fun j -> printfn "%A" j)