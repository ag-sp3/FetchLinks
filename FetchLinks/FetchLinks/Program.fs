open System
open System.IO
open System.Text.RegularExpressions
open System.Net

let getHtml url = 
   async {    
        let! rsp = WebRequest.Create(url.ToString()).AsyncGetResponse()
        use reader = new StreamReader(rsp.GetResponseStream())
        return reader.ReadToEndAsync().Result    
    } |> Async.RunSynchronously

let matches pattern prefix postfix raw = 
    Regex.Matches(raw, pattern)|> Seq.cast<System.Text.RegularExpressions.Match> 
    |> Seq.map (fun m -> prefix + m.Groups.[1].Value + postfix) |> Seq.distinct

let dump (s: string seq) = 
    let print2file item = File.AppendAllText(@"C:\Users\alexa\Desktop\to download.txt",item.ToString() + "\r\n")
    let print2screen item = item.ToString() |> Console.WriteLine
    s |> Seq.iter (fun i -> i |> print2file; i |> print2screen )

let getVideoLinksFromSession session = 
    let html = getHtml session
    let pattern = @"href=""(http://video.ch9.ms/ch9/.+?uwp\d.+?high.mp4)"""
    let links = html |> matches pattern "" ""
    let count = links |> Seq.length
    if count > 0 then links |> Seq.head else html |> matches @"href=""(http://video.ch9.ms/ch9/.+?uwp\d.+?mid.mp4)""" "" "" |> Seq.head
         
let getVideoLinksFromPage page = 
    let sessionPattern = @"href\=""(/Series/Windows-10-development-for-absolute-beginners/UWP\-\d{3}\-[^#]+?)"""
    getHtml page |> matches sessionPattern @"https://channel9.msdn.com" "" 
    |> Seq.map(fun s -> s |> getVideoLinksFromSession)

let getSourcePages() = 
    let ranges = seq { 2 .. 8 }
    let head = @"https://channel9.msdn.com/Series/Windows-10-development-for-absolute-beginners"
    let prefix = @"https://channel9.msdn.com/Series/Windows-10-development-for-absolute-beginners?page="
    Seq.append (seq {yield head}) (ranges |> Seq.map (fun i -> prefix + i.ToString()))
    
[<EntryPoint>]
let main argv = 
    let pages = getSourcePages()
    pages |> Seq.map(fun p -> p |> getVideoLinksFromPage) |> Seq.iter(fun p -> p |> dump)    
    Console.WriteLine("All links fetched!"); Console.ReadLine() |> ignore
    0