﻿module ARCtrl.Json.ROCrateHelper

open Thoth.Json.Core

open ARCtrl
open System.IO

module Person =

    /// <summary>
    /// This is only used for ro-crate creation. In ISA publication authors are only a string. ro-crate requires person object.
    /// Therefore, we try to split the string by common separators and create a minimal person object for ro-crate.
    /// </summary>
    /// <param name="authorList"></param>
    let authorListStrinEncoder (authorList: string) =
        let tab = "\t"
        let semi = ";"
        let comma = ","
        let separator =
            if authorList.Contains(tab) then tab
            elif authorList.Contains(semi) then semi
            else comma
        let names = authorList.Split([|separator|], System.StringSplitOptions.None)
        let encodeSingle (name:string) =
            [
                "@type", Encode.string "Person"
                Encode.tryInclude "name" Encode.string (Some name)
                "@context", ROCrateContext.Person.contextMinimal_jsonValue
            ]
            |> Encode.choose
            |> Encode.object
        Encode.array (names |> Array.map encodeSingle)
