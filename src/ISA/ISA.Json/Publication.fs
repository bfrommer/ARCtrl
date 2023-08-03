namespace ISA.Json

#if FABLE_COMPILER
open Thoth.Json
#else
open Thoth.Json.Net
#endif
open ISA
open System.IO

module Publication =    
    
    let genID (p:Publication) = 
        match p.DOI with
        | Some doi -> doi
        | None -> match p.PubMedID with
                  | Some id -> id
                  | None -> match p.Title with
                            | Some t -> "#Pub_" + t.Replace(" ","_")
                            | None -> "#EmptyPublication"

    let rec encoder (options : ConverterOptions) (oa : obj) = 
        [
            if options.SetID then "@id", GEncode.string (oa :?> Publication |> genID)
            if options.IncludeType then "@type", GEncode.string "Publication"
            GEncode.tryInclude "pubMedID" GEncode.string (oa |> GEncode.tryGetPropertyValue "PubMedID")
            GEncode.tryInclude "doi" GEncode.string (oa |> GEncode.tryGetPropertyValue "DOI")
            GEncode.tryInclude "authorList" GEncode.string (oa |> GEncode.tryGetPropertyValue "Authors")
            GEncode.tryInclude "title" GEncode.string (oa |> GEncode.tryGetPropertyValue "Title")
            GEncode.tryInclude "status" (OntologyAnnotation.encoder options) (oa |> GEncode.tryGetPropertyValue "Status")
            GEncode.tryInclude "comments" (Comment.encoder options) (oa |> GEncode.tryGetPropertyValue "Comments")
        ]
        |> GEncode.choose
        |> Encode.object

    let rec decoder (options : ConverterOptions) : Decoder<Publication> =
        Decode.object (fun get ->
            {
                PubMedID = get.Optional.Field "pubMedID" GDecode.uri
                DOI = get.Optional.Field "doi" Decode.string
                Authors = get.Optional.Field "authorList" Decode.string
                Title = get.Optional.Field "title" Decode.string
                Status = get.Optional.Field "status" (OntologyAnnotation.decoder options)
                Comments = get.Optional.Field "comments" (Decode.array (Comment.decoder options))
            }
            
        )

    let fromString (s:string) = 
        GDecode.fromString (decoder (ConverterOptions())) s

    let toString (p:Publication) = 
        encoder (ConverterOptions()) p
        |> Encode.toString 2

    /// exports in json-ld format
    let toStringLD (p:Publication) = 
        encoder (ConverterOptions(SetID=true,IncludeType=true)) p
        |> Encode.toString 2

    //let fromFile (path : string) = 
    //    File.ReadAllText path 
    //    |> fromString

    //let toFile (path : string) (p:Publication) = 
    //    File.WriteAllText(path,toString p)