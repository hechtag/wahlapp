open LiteDB
use db = new LiteDatabase("myData.db")

type Test() =
    member val Name = "" with get, set
    override this.ToString() = sprintf "[ %s ]" this.Name

let col = db.GetCollection<Test>("test")
let test = Test()
test.Name <- "test"


col.Insert(test)
test.Name <- test.Name + "2"

// col.Update(test)

let results = col.Find(fun x -> x.Name = "test") |> Seq.toList
printfn "%O" results