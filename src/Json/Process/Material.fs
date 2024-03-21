namespace ARCtrl.Json

open Thoth.Json.Core

open ARCtrl
open System.IO




module MaterialAttributeValue =
    
    let genID (m:MaterialAttributeValue) : string = 
        match m.ID with
        | Some id -> URI.toString id
        | None -> "#EmptyMaterialAttributeValue"

    let encoder (options : ConverterOptions) (oa : MaterialAttributeValue) = 
        [
            if options.SetID then 
                "@id", Encode.string (oa |> genID)
            else 
                Encode.tryInclude "@id" Encode.string (oa.ID)
            if options.IsJsonLD then 
                "@type", (Encode.list [Encode.string "MaterialAttributeValue"])
                "additionalType", Encode.string "MaterialAttributeValue"
            if options.IsJsonLD then
                if oa.Category.IsSome && oa.Category.Value.CharacteristicType.IsSome then
                    Encode.tryInclude "category" Encode.string (oa.Category.Value.CharacteristicType.Value.Name)
                if oa.Category.IsSome && oa.Category.Value.CharacteristicType.IsSome then
                    Encode.tryInclude "categoryCode" Encode.string (oa.Category.Value.CharacteristicType.Value.TermAccessionNumber)
                if oa.Value.IsSome then "value", Encode.string (oa.ValueText)
                if oa.Value.IsSome && oa.Value.Value.IsAnOntology then
                    Encode.tryInclude "valueCode" Encode.string (oa.Value.Value.AsOntology()).TermAccessionNumber
                if oa.Unit.IsSome then Encode.tryInclude "unit" Encode.string (oa.Unit.Value.Name)
                if oa.Unit.IsSome then Encode.tryInclude "unitCode" Encode.string (oa.Unit.Value.TermAccessionNumber)
            else
                Encode.tryInclude "category" (MaterialAttribute.encoder options) (oa.Category)
                Encode.tryInclude "value" (Value.encoder options) (oa.Value)
                Encode.tryInclude "unit" (OntologyAnnotation.encoder options) (oa.Unit)
            if options.IsJsonLD then 
                "@context", ROCrateContext.MaterialAttributeValue.context_jsonvalue
        ]
        |> Encode.choose
        |> Encode.object

    let decoder (options : ConverterOptions) : Decoder<MaterialAttributeValue> =
        Decode.object (fun get ->
            {
                ID = get.Optional.Field "@id" GDecode.uri
                Category = get.Optional.Field "category" (MaterialAttribute.decoder options)
                Value = get.Optional.Field "value" (Value.decoder options)
                Unit = get.Optional.Field "unit" (OntologyAnnotation.decoder options)
            }
        )

    let fromJsonString (s:string) = 
        GDecode.fromJsonString (decoder (ConverterOptions())) s
    let fromJsonldString (s:string) = 
        GDecode.fromJsonString (decoder (ConverterOptions(IsJsonLD=true))) s

    let toJsonString (m:MaterialAttributeValue) = 
        encoder (ConverterOptions()) m
        |> Encode.toJsonString 2
    
    /// exports in json-ld format
    let toJsonldString (m:MaterialAttributeValue) = 
        encoder (ConverterOptions(SetID=true,IsJsonLD=true)) m
        |> Encode.toJsonString 2

    let toJsonldStringWithContext (a:MaterialAttributeValue) = 
        encoder (ConverterOptions(SetID=true,IsJsonLD=true)) a
        |> Encode.toJsonString 2

    //let fromFile (path : string) = 
    //    File.ReadAllText path 
    //    |> fromString

    //let toFile (path : string) (m:MaterialAttributeValue) = 
    //    File.WriteAllText(path,toString m)


module Material = 
    
    let genID (m:Material) : string = 
        match m.ID with
            | Some id -> id
            | None -> match m.Name with
                        | Some n -> "#Material_" + n.Replace(" ","_")
                        | None -> "#EmptyMaterial"
    
    let rec encoder (options : ConverterOptions) (oa : Material) = 
        [
            if options.SetID then 
                "@id", Encode.string (oa |> genID)
            else 
                Encode.tryInclude "@id" Encode.string (oa.ID)
            if options.IsJsonLD then 
                "@type", (Encode.list [Encode.string "Material"])
            Encode.tryInclude "name" Encode.string (oa.Name)
            Encode.tryInclude "type" (MaterialType.encoder options) (oa.MaterialType)
            Encode.tryIncludeList "characteristics" (MaterialAttributeValue.encoder options) (oa.Characteristics)
            Encode.tryIncludeList "derivesFrom" (encoder options) (oa.DerivesFrom)
            if options.IsJsonLD then 
                "@context", ROCrateContext.Material.context_jsonvalue
        ]
        |> Encode.choose
        |> Encode.object

    let allowedFields = ["@id";"@type";"name";"type";"characteristics";"derivesFrom"; "@context"]

    let rec decoder (options : ConverterOptions) : Decoder<Material> =       
        GDecode.object allowedFields (fun get -> 
            {                       
                ID = get.Optional.Field "@id" GDecode.uri
                Name = get.Optional.Field "name" Decode.string
                MaterialType = get.Optional.Field "type" (MaterialType.decoder options)
                Characteristics = get.Optional.Field "characteristics" (Decode.list (MaterialAttributeValue.decoder options))
                DerivesFrom = get.Optional.Field "derivesFrom" (Decode.list (decoder options))
            }
        )
        
    let fromJsonString (s:string) = 
        GDecode.fromJsonString (decoder (ConverterOptions())) s
    let fromJsonldString (s:string) = 
        GDecode.fromJsonString (decoder (ConverterOptions(IsJsonLD=true))) s

    let toJsonString (m:Material) = 
        encoder (ConverterOptions()) m
        |> Encode.toJsonString 2
    
    /// exports in json-ld format
    let toJsonldString (m:Material) = 
        encoder (ConverterOptions(SetID=true,IsJsonLD=true)) m
        |> Encode.toJsonString 2

    let toJsonldStringWithContext (a:Material) = 
        encoder (ConverterOptions(SetID=true,IsJsonLD=true)) a
        |> Encode.toJsonString 2

    //let fromFile (path : string) = 
    //    File.ReadAllText path 
    //    |> fromString

    //let toFile (path : string) (m:Material) = 
    //    File.WriteAllText(path,toString m)