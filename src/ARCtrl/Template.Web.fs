﻿module ARCtrl.Template.Web

open ARCtrl
open Fable.Core


let getTemplates(url: string option) =
    let defaultURL = @"https://github.com/nfdi4plants/Swate-templates/releases/download/latest/templates.json"
    let url = defaultArg url defaultURL
    async {
        let! jsonString = ARCtrl.WebRequest.downloadFile url
        let mapResult = Json.Templates.fromJsonString jsonString
        return mapResult
    }

#if FABLE_COMPILER_JAVASCRIPT

/// <summary>
/// This class is used to make async functions more accessible from JavaScript.
/// </summary>
[<AttachMembers>]
type JsWeb =
    static member getTemplates(url: string option) =
        async {
            let! templates = getTemplates(url)
            return templates
        }
        |> Async.StartAsPromise
#endif