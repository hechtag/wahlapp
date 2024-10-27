module Db

open System
open LiteDB
open Shared

let private dbName = "wahlapp.db"
let private lockObj = obj () // Global lock object

let findAll (collectionName: string) : 'T seq =
    use db = new LiteDatabase(dbName)
    let asdf = db.GetCollection<'T>(collectionName).FindAll() |> Seq.toList
    asdf


let find (collectionName: string) (f: 'T -> bool) : 'T seq =
    use db = new LiteDatabase(dbName)
    let asdf = db.GetCollection<'T>(collectionName).Find(f) |> Seq.toList
    asdf

let getById (collectionName: string) (id: Guid) : 'T =
    use db = new LiteDatabase(dbName)
    let asdf = db.GetCollection<'T>(collectionName).FindById(id)
    asdf

let add<'T> (collectionName: string) (item: 'T) : Result<'T, string> =
    use db = new LiteDatabase(dbName)
    let asdf = db.GetCollection<'T>(collectionName).Insert(item)
    printfn "%O" asdf
    Ok item


let delete<'T> (collectionName: string) (id: Guid) : unit =
    use db = new LiteDatabase(dbName)
    let asdf = db.GetCollection<'T>(collectionName).Delete(id)
    ()

let update (collectionName: string) (toUpdate: 'T) : unit =
    use db = new LiteDatabase(dbName)
    let asdf = db.GetCollection<'T>(collectionName).Update(toUpdate)
    ()


// let test = Test()
// test.Name <- "test"


// col.Insert(test)
// test.Name <- test.Name + "2"

// // col.Update(test)

// let results = col.Find(fun x -> x.Name = "test") |> Seq.toList
// printfn "%O" results