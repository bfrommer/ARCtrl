﻿module Contracts.ArcStudy

open Contract

let tryFromContract (c:Contract) =
    match c with
    | {Operation = READ; DTOType = Some DTOType.ISA_Study; DTO = Some (DTO.Spreadsheet fsworkbook)} ->
        Some fsworkbook
    | _ -> None

open ARCtrl.Path

let (|StudyPath|_|) (input) =
    match input with
    | [|StudiesFolderName; anyStudyName; StudyFileName|] -> 
        let path = FileSystem.Path.combineMany input
        Some path
    | _ -> None