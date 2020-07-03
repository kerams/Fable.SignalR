﻿namespace SignalRApp

type RandomStringGen () = 
    member _.Gen () =
        let characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
        
        System.Random().Next(0,characters.Length-1)
        |> fun i -> characters.ToCharArray().[i]
        |> string

module SignalRHub =
    open Fable.SignalR
    open FSharp.Control
    open Microsoft.Extensions.DependencyInjection
    open SignalRHub
    open System.Collections.Generic
    
    let update (msg: Action) (stringGen: RandomStringGen) =
        match msg with
        | Action.IncrementCount i -> Response.NewCount(i + 1)
        | Action.DecrementCount i -> Response.NewCount(i - 1)
        | Action.RandomCharacter -> stringGen.Gen() |> Response.RandomCharacter

    let invoke (msg: Action) (services: System.IServiceProvider) =
        services.GetService<RandomStringGen>()
        |> update msg
            
    let send (msg: Action) (hubContext: FableHub<Action,Response>) =
        invoke msg hubContext.Services
        |> hubContext.Clients.Caller.Send

    [<RequireQualifiedAccess>]
    module Stream =
        let sendToClient (msg: StreamFrom.Action) (_: FableHub<Action,Response>) =
            match msg with
            | StreamFrom.Action.GenInts ->
                asyncSeq {
                    for i in [ 1 .. 100 ] do
                        yield StreamFrom.Response.GetInts i
                }
                |> AsyncSeq.toAsyncEnum

        let getFromClient (clientStream: IAsyncEnumerable<StreamTo.Action>) (_: FableHub<Action,Response>) =
            AsyncSeq.ofAsyncEnum clientStream
            |> AsyncSeq.iterAsync (fun _ -> async { return () })
            |> Async.StartAsTask

module SignalRHub2 =
    open Fable.SignalR
    open FSharp.Control
    open SignalRHub
    open System.Collections.Generic

    let update (msg: Action) =
        match msg with
        | Action.IncrementCount i -> Response.NewCount(i + 1)
        | Action.DecrementCount i -> Response.NewCount(i - 1)
        | Action.RandomCharacter ->
            let characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
            
            System.Random().Next(0,characters.Length-1)
            |> fun i -> characters.ToCharArray().[i]
            |> string
            |> Response.RandomCharacter

    let invoke (msg: Action) _ = update msg

    let send (msg: Action) (hubContext: FableHub<Action,Response>) =
        update msg
        |> hubContext.Clients.Caller.Send

    [<RequireQualifiedAccess>]
    module Stream =
        let sendToClient (msg: StreamFrom.Action) (_: FableHub<Action,Response>) =
            match msg with
            | StreamFrom.Action.GenInts ->
                asyncSeq {
                    for i in [ 1 .. 100 ] do
                        yield StreamFrom.Response.GetInts i
                }
                |> AsyncSeq.toAsyncEnum

        let getFromClient (clientStream: IAsyncEnumerable<StreamTo.Action>) (_: FableHub<Action,Response>) =
            AsyncSeq.ofAsyncEnum clientStream
            |> AsyncSeq.iterAsync (fun _ -> async { return () })
            |> Async.StartAsTask
