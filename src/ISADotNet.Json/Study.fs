namespace ISADotNet.Json

#if FABLE_COMPILER
open Thoth.Json
#else
open Thoth.Json.Net
#endif
open ISADotNet
open System.IO
open GEncode

module StudyMaterials = 

    let encoder (options : ConverterOptions) (oa : obj) = 
        [
            tryInclude "sources" (Source.encoder options) (oa |> tryGetPropertyValue "Sources")
            tryInclude "samples" (Sample.encoder options) (oa |> tryGetPropertyValue "Samples")
            tryInclude "otherMaterials" (Material.encoder options) (oa |> tryGetPropertyValue "OtherMaterials")
        ]
        |> GEncode.choose
        |> Encode.object
    
    let decoder (options : ConverterOptions) : Decoder<StudyMaterials> =
        Decode.object (fun get ->
            {
                Sources = get.Optional.Field "sources" (Decode.list (Source.decoder options))
                Samples = get.Optional.Field "samples" (Decode.list (Sample.decoder options))
                OtherMaterials = get.Optional.Field "otherMaterials" (Decode.list (Material.decoder options))
            }
        )


module Study =
    
    let genID (s:Study) = 
        match s.ID with
        | Some id -> URI.toString id
        | None -> match s.FileName with
                  | Some n -> "#Study" + n.Replace(" ","_")
                  | None -> match s.Identifier with
                            | Some id -> "#Study_" + id.Replace(" ","_")
                            | None -> match s.Title with
                                      | Some t -> "#Study_" + t.Replace(" ","_")
                                      | None -> "#EmptyStudy"
    
    let encoder (options : ConverterOptions) (oa : obj) = 
        [
            if options.SetID then "@id", GEncode.string (oa :?> Study |> genID)
                else tryInclude "@id" GEncode.string (oa |> tryGetPropertyValue "ID")
            if options.IncludeType then "@type", ([GEncode.string "Study"; GEncode.string "ArcStudy"] |> Encode.list)
            tryInclude "filename" GEncode.string (oa |> tryGetPropertyValue "FileName")
            tryInclude "identifier" GEncode.string (oa |> tryGetPropertyValue "Identifier")
            tryInclude "title" GEncode.string (oa |> tryGetPropertyValue "Title")
            tryInclude "description" GEncode.string (oa |> tryGetPropertyValue "Description")
            tryInclude "submissionDate" GEncode.string (oa |> tryGetPropertyValue "SubmissionDate")
            tryInclude "publicReleaseDate" GEncode.string (oa |> tryGetPropertyValue "PublicReleaseDate")
            tryInclude "publications" (Publication.encoder options) (oa |> tryGetPropertyValue "Publications")
            tryInclude "people" (Person.encoder options) (oa |> tryGetPropertyValue "Contacts")
            tryInclude "studyDesignDescriptors" (OntologyAnnotation.encoder options) (oa |> tryGetPropertyValue "StudyDesignDescriptors")
            tryInclude "protocols" (Protocol.encoder options) (oa |> tryGetPropertyValue "Protocols")
            if options.IsRoCrate then
                let study = oa:?> Study
                let mat = study.Materials
                match mat with
                | Some m -> tryInclude "samples" (Sample.encoder options) (m |> tryGetPropertyValue "Samples")
                | None -> ()
            if options.IsRoCrate then
                let study = oa:?> Study
                let mat = study.Materials
                match mat with
                | Some m -> tryInclude "sources" (Source.encoder options) (m |> tryGetPropertyValue "Sources")
                | None -> ()
            if options.IsRoCrate then
                let study = oa:?> Study
                let mat = study.Materials
                match mat with
                | Some m -> tryInclude "materials" (Material.encoder options) (m |> tryGetPropertyValue "OtherMaterials")
                | None -> ()
            if not options.IsRoCrate then (tryInclude "materials" (StudyMaterials.encoder options) (oa |> tryGetPropertyValue "Materials"))
            tryInclude "processSequence" (Process.encoder options) (oa |> tryGetPropertyValue "ProcessSequence")
            tryInclude "assays" (Assay.encoder options) (oa |> tryGetPropertyValue "Assays")            
            tryInclude "factors" (Factor.encoder options) (oa |> tryGetPropertyValue "Factors")
            tryInclude "characteristicCategories" (MaterialAttribute.encoder options) (oa |> tryGetPropertyValue "CharacteristicCategories")            
            tryInclude "unitCategories" (OntologyAnnotation.encoder options) (oa |> tryGetPropertyValue "UnitCategories")
            tryInclude "comments" (Comment.encoder options) (oa |> tryGetPropertyValue "Comments")
        ]
        |> GEncode.choose
        |> List.append (if options.IncludeContext then [("@context",Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText("/home/wetzels/arc/ISADotNet_public/src/ISADotNet.Json/context/sdo/isa_study_sdo_context.jsonld")).GetValue("@context"))] else [])
        |> Encode.object

    let decoder (options : ConverterOptions) : Decoder<Study> =
        Decode.object (fun get ->
            {
                ID = get.Optional.Field "@id" GDecode.uri
                FileName = get.Optional.Field "filename" Decode.string
                Identifier = get.Optional.Field "identifier" Decode.string
                Title = get.Optional.Field "title" Decode.string
                Description = get.Optional.Field "description" Decode.string
                SubmissionDate = get.Optional.Field "submissionDate" Decode.string
                PublicReleaseDate = get.Optional.Field "publicReleaseDate" Decode.string
                Publications = get.Optional.Field "publications" (Decode.list (Publication.decoder options))
                Contacts = get.Optional.Field "people" (Decode.list (Person.decoder options))
                StudyDesignDescriptors = get.Optional.Field "studyDesignDescriptors" (Decode.list (OntologyAnnotation.decoder options))
                Protocols = get.Optional.Field "protocols" (Decode.list (Protocol.decoder options))
                Materials = get.Optional.Field "materials" (StudyMaterials.decoder options)
                Assays = get.Optional.Field "assays" (Decode.list (Assay.decoder options))
                Factors = get.Optional.Field "factors" (Decode.list (Factor.decoder options))
                CharacteristicCategories = get.Optional.Field "characteristicCategories" (Decode.list (MaterialAttribute.decoder options))
                UnitCategories = get.Optional.Field "unitCategories" (Decode.list (OntologyAnnotation.decoder options))
                ProcessSequence = get.Optional.Field "processSequence" (Decode.list (Process.decoder options))
                Comments = get.Optional.Field "comments" (Decode.list (Comment.decoder options))
            }
        )

    let fromString (s:string) = 
        GDecode.fromString (decoder (ConverterOptions())) s

    let toString (p:Study) = 
        encoder (ConverterOptions()) p
        |> Encode.toString 2

    /// exports in json-ld format
    let toStringLD (s:Study) = 
        encoder (ConverterOptions(SetID=true,IncludeType=true)) s
        |> Encode.toString 2
    let toStringLDWithContext (a:Study) = 
        encoder (ConverterOptions(SetID=true,IncludeType=true,IncludeContext=true)) a
        |> Encode.toString 2

    //let fromFile (path : string) = 
    //    File.ReadAllText path 
    //    |> fromString

    //let toFile (path : string) (p:Study) = 
    //    File.WriteAllText(path,toString p)