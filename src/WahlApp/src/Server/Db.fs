module Db

open System
open DbEntity
open LiteDB
open Model
open Microsoft.Extensions.Logging

let private dbName = "wahlapp.db"
let private lockObj = obj () // Global lock object

let findAll<'T when 'T :> IDb> (collectionName: string) : 'T seq =
    use db = new LiteDatabase(dbName)
    let asdf = db.GetCollection<'T>(collectionName).FindAll() |> Seq.toList
    asdf


let find<'T when 'T :> IDb> (collectionName: string) (f: 'T -> bool) : 'T seq =
    use db = new LiteDatabase(dbName)
    let asdf = db.GetCollection<'T>(collectionName).Find(f) |> Seq.toList
    asdf

let getById<'T when 'T :> IDb> (collectionName: string) (id: Guid) : 'T =
    use db = new LiteDatabase(dbName)
    let asdf = db.GetCollection<'T>(collectionName).FindById(id)
    asdf

let add<'T when 'T :> IDb> (collectionName: string) (item: 'T) : Result<'T, string> =
    use db = new LiteDatabase(dbName)
    let asdf = db.GetCollection<'T>(collectionName).Insert(item)
    printfn "%O" asdf
    Ok item


let delete<'T when 'T :> IDb> (collectionName: string) (id: Guid) : unit =
    use db = new LiteDatabase(dbName)
    let asdf = db.GetCollection<'T>(collectionName).Delete(id)
    ()

let update<'T when 'T :> IDb> (collectionName: string) (toUpdate: 'T) : unit =
    use db = new LiteDatabase(dbName)
    let asdf = db.GetCollection<'T>(collectionName).Update(toUpdate)
    ()
