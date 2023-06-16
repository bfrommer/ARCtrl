﻿namespace ISA

open Fable.Core
open System.Collections.Generic
open FSharpAux

module rec ArcTableAux =

    let getColumnCount (headers:ResizeArray<CompositeHeader>) = 
        headers.Count

    let getRowCount (values:Dictionary<int*int,CompositeCell>) = 
        if values.Count = 0 then 0 else
            values.Keys |> Seq.maxBy snd |> snd |> (+) 1

    module SanityChecks =
        let validateIndex (index:int) (columnCount:int) =
            if index < 0 then failwith "Cannot insert CompositeColumn at index < 0."
            if index > columnCount then failwith $"Specified index is out of table range! Table contains only {columnCount} columns."

        let validateColumn (column:CompositeColumn) = column.validate(true) |> ignore

        let inline validateNoDuplicateUniqueColumns (columns:seq<CompositeColumn>) =
            let duplicates = columns |> Seq.map (fun x -> x.Header) |> ArcTableAux.tryFindDuplicateUniqueInArray
            if not <| List.isEmpty duplicates then
                let baseMsg = "Found duplicate unique columns in `columns`."
                let sb = System.Text.StringBuilder()
                sb.AppendLine(baseMsg) |> ignore
                duplicates |> List.iter (fun (x: {| HeaderType: CompositeHeader; Index1: int; Index2: int |}) -> 
                    sb.AppendLine($"Duplicate `{x.HeaderType}` at index {x.Index1} and {x.Index2}.")
                    |> ignore
                )
                let msg = sb.ToString()
                failwith msg

        let inline validateNoDuplicateUnique (header: CompositeHeader) (columns:seq<CompositeHeader>) =
            match tryFindDuplicateUnique header columns with
            | None -> ()
            | Some i -> failwith $"Invalid input. Tried setting unique header `{header}`, but header of same type already exists at index {i}."

    // TODO: Move to CompositeHeader?
    let (|IsUniqueExistingHeader|_|) existingHeaders (input: CompositeHeader) = 
        match input with
        | CompositeHeader.Parameter _
        | CompositeHeader.Factor _
        | CompositeHeader.Characteristic _
        | CompositeHeader.Component _
        | CompositeHeader.FreeText _        -> None
        // Input and Output does not look very clean :/
        | CompositeHeader.Output _          -> Seq.tryFindIndex (fun h -> match h with | CompositeHeader.Output _ -> true | _ -> false) existingHeaders
        | CompositeHeader.Input _           -> Seq.tryFindIndex (fun h -> match h with | CompositeHeader.Input _ -> true | _ -> false) existingHeaders
        | header                            -> Seq.tryFindIndex (fun h -> h = header) existingHeaders
        
    // TODO: Move to CompositeHeader?
    /// Returns the column index of the duplicate unique column in `existingHeaders`.
    let tryFindDuplicateUnique (newHeader: CompositeHeader) (existingHeaders: seq<CompositeHeader>) = 
        match newHeader with
        | IsUniqueExistingHeader existingHeaders index -> Some index
        | _ -> None

    let inline failWithDuplicateUnique () = 
        failwith "Found duplicate unique columns in `columns`."

    /// Returns the column index of the duplicate unique column in `existingHeaders`.
    let tryFindDuplicateUniqueInArray (existingHeaders: seq<CompositeHeader>) = 
        let rec loop i (duplicateList: {|Index1: int; Index2: int; HeaderType: CompositeHeader|} list) (headerList: CompositeHeader list) =
            match headerList with
            | _ :: [] | [] -> duplicateList
            | header :: tail -> 
                let hasDuplicate = tryFindDuplicateUnique header tail
                let nextDuplicateList = if hasDuplicate.IsSome then {|Index1 = i; Index2 = hasDuplicate.Value; HeaderType = header|}::duplicateList else duplicateList
                loop (i+1) nextDuplicateList tail
        existingHeaders
        |> Seq.filter (fun x -> not x.IsTermColumn)
        |> List.ofSeq
        |> loop 0 []
               

[<AttachMembers>]
type ArcTable = 
    {
        Name : string
        Headers : ResizeArray<CompositeHeader>
        /// Key: Column * Row
        Values : System.Collections.Generic.Dictionary<int*int,CompositeCell>  
    }

    static member create(name, headers, values) =
        {
            Name = name
            Headers = headers
            Values = values

        }


    /// Create ArcTable with empty 'ValueHeader' and 'Values' 
    static member init(name: string) = {
        Name = name
        Headers = ResizeArray<CompositeHeader>()
        Values = System.Collections.Generic.Dictionary<int*int,CompositeCell>()
    }



    /// Returns a cell at given position if it exists, else returns None.
    member this.TryGetCellAt (column: int,row: int) =
        Dictionary.tryFind (column, row) this.Values

    /// Moves a cell horizontally by the given amount. Positive amounts move the cell to the right, negative amounts move the cell to the left.
    member this.MoveCellAtHorizontally (column,row,amount : int) =
        match this.TryGetCellAt(column,row) with
        | Some c -> 
            Dictionary.addOrUpdateInPlace (column+amount,row) c this.Values |> ignore
            Dictionary.remove(column,row) |> ignore
        | None -> ()
    
    member this.SetCellAt(column, row,c : CompositeCell) =
        Dictionary.addOrUpdateInPlace (column,row) c this.Values |> ignore
     
    member this.OverwriteHeader(header: CompositeHeader, index : int) =
        ArcTableAux.SanityChecks.validateIndex index this.ColumnCount
        this.Headers.[index] <- header

    member this.OverwriteColumn(header: CompositeHeader, cells : CompositeCell [], index : int) =
        ArcTableAux.SanityChecks.validateIndex index this.ColumnCount
        this.Headers.[index] <- header
        cells
        |> Array.iteri (fun i c -> this.SetCellAt(index,i,c))
          
    member this.InsertColumn(header: CompositeHeader,cells: CompositeCell [],index: int,?forceReplace: bool) =
        let forceReplace = defaultArg forceReplace false
        ArcTableAux.SanityChecks.validateIndex index this.ColumnCount
        match ArcTableAux.tryFindDuplicateUnique header this.Headers with
        | Some i when forceReplace -> this.OverwriteColumn(header,cells,i)
        | Some _ -> ArcTableAux.failWithDuplicateUnique()
        | None -> 
            for rowI = 0 to this.RowCount - 1 do
                for colI = this.ColumnCount - 1 to index do                
                    this.MoveCellAtHorizontally(colI,rowI,1)
            cells 
            |> Array.iteri (fun rowI c ->               
                this.SetCellAt(index,rowI,c)           
            )
            this.Headers.Insert(index,header)
           
    member this.InsertEmptyColumn(header: CompositeHeader,index: int,?forceReplace: bool) =
        this.InsertColumn(header, [||],index ,?forceReplace = forceReplace)

    member this.AppendColumn(header: CompositeHeader,cells: CompositeCell [],?forceReplace: bool) =
        let forceReplace = defaultArg forceReplace false
        match ArcTableAux.tryFindDuplicateUnique header this.Headers with
        | Some i when forceReplace -> this.OverwriteColumn(header,cells,i)
        | Some _ -> ArcTableAux.failWithDuplicateUnique()
        | None -> 
            cells
            |> Array.iteri (fun i c -> this.SetCellAt(this.ColumnCount,i,c))
            this.Headers.Add(header)

    member this.AppendEmptyColumn(header: CompositeHeader,?forceReplace: bool) =
        this.AppendColumn(header, [||], ?forceReplace = forceReplace)
        
    member this.AddColumn(header: CompositeHeader,?Cells: CompositeCell [], ?Index,?forceReplace: bool) =
        match Cells,Index with
        | Some c, Some i -> this.InsertColumn(header,c,i,?forceReplace = forceReplace)
        | Some c, None -> this.AppendColumn(header,c,?forceReplace = forceReplace)
        | None, Some i -> this.InsertEmptyColumn(header,i,?forceReplace = forceReplace)
        | None,None -> this.AppendEmptyColumn(header,?forceReplace = forceReplace)
        ArcTable.extendBodyCells this
    /////
    //static member private insertRawColumn (newHeader: CompositeHeader) (newCells: CompositeCell []) (index: int) (forceReplace: bool) (table:ArcTable) =
    //    let mutable numberOfNewColumns = 1
    //    let mutable index = index
    //    let hasDuplicateUnique = ArcTableAux.tryFindDuplicateUnique newHeader table.Headers
    //    // implement fail if unique column should be added but exists already
    //    if not forceReplace && hasDuplicateUnique.IsSome then failwith $"Invalid new column `{newHeader}`. Table already contains header of the same type on index `{hasDuplicateUnique.Value}`"
    //    // Example: existingCells contains `Output io` (With io being any IOType) and header is `Output RawDataFile`. This should replace the existing `Output io`.
    //    // In this case the number of new columns drops to 0 and we insert the index of the existing `Output io` column.
    //    if hasDuplicateUnique.IsSome then
    //        numberOfNewColumns <- 0
    //        index <- hasDuplicateUnique.Value
    //    // headers are easily added. Just insert at position of index. This will insert without replace.
    //    let insertNewHeader = 
    //        // If we need to replace, set at index
    //        if hasDuplicateUnique.IsSome then 
    //            table.Headers.[index] <- newHeader
    //        // if not we just insert at index
    //        else
    //            table.Headers.Insert(index, newHeader)
    //    /// For all columns with index >= we need to increase column index by `numberOfNewColumns`.
    //    /// We do this by moving all these columns one to the right with mutable dictionary set logic (cells.[key] <- newValue), 
    //    /// Therefore we need to start with the last column to not overwrite any values we still need to shift
    //    let increaseColIndexForExisting =
    //        /// Get last column index
    //        let lastColumnIndex = table.ColumnCount - 1
    //        /// Get all keys, to map over relevant rows afterwards
    //        let keys = table.Values.Keys
    //        // start with last column index and go down to `index`
    //        for nextColumnIndex in lastColumnIndex .. index do
    //            keys 
    //            |> Seq.filter (fun (c,_) -> c = nextColumnIndex) // only get keys for the relevant column
    //            |> Seq.iter (fun (c_index,r_index)-> // iterate over all keys for the column
    //                let v = table.Values.[(c_index,r_index)] // get the value
    //                /// Remove value. This is necessary in the following scenario:
    //                ///
    //                /// "AddColumn.Existing Table.add less rows, insert at".
    //                ///
    //                /// Assume a table with 5 rows, insert column with 2 cells. All 5 rows at `index` position are shifted +1, but only row 0 and 1 are replaced with new values. 
    //                /// Without explicit removing, row 2..4 would stay as is. 
    //                let rmv_v = table.Values.Remove(c_index,r_index) 
    //                //table.Values.[(c_index + numberOfNewColumns,r_index)] <- v // set the same value for the same row but (c_index + `numberOfNewColumns`)
    //                ()
    //            )
    //    /// Then we can set the new column at `index`
    //    let insertNewColumn =
    //        for rowIndex,v in (Array.indexed newCells) do
    //            let columnIndex = index
    //            table.Values.[(columnIndex,rowIndex)] <- v
    //    ()

    // We need to calculate the max number of rows between the new columns and the existing columns in the table.
    // `maxRows` will be the number of rows all columns must have after adding the new columns.
    // This behaviour should be intuitive for the user, as Excel handles this case in the same way.
    static member private extendBodyCells (table:ArcTable) =
        let maxRows = table.RowCount
        let lastColumnIndex = table.ColumnCount - 1
        /// Get all keys, to map over relevant rows afterwards
        let keys = table.Values.Keys
        // iterate over columns
        for columnIndex in 0 .. lastColumnIndex do
            /// Only get keys for the relevant column
            let colKeys = keys |> Seq.filter (fun (c,_) -> c = columnIndex) |> Set.ofSeq 
            /// Create set of expected keys
            let expectedKeys = Seq.init maxRows (fun i -> columnIndex,i) |> Set.ofSeq 
            /// Get the missing keys
            let missingKeys = Set.difference expectedKeys colKeys 
            // if no missing keys, we are done and skip the rest, if not empty missing keys we ...
            if missingKeys.IsEmpty |> not then
                /// .. first check which empty filler `CompositeCells` we need. 
                ///
                /// We use header to decide between CompositeCell.Term/CompositeCell.Unitized and CompositeCell.FreeText
                let relatedHeader = table.Headers.[columnIndex]
                let empty = 
                    match relatedHeader.IsTermColumn with
                    | false                                 -> CompositeCell.emptyFreeText
                    | true ->
                        /// We use the first cell in the column to decide between CompositeCell.Term and CompositeCell.Unitized
                        ///
                        /// Not sure if we can add a better logic to infer if empty cells should be term or unitized ~Kevin F
                        let tryExistingCell = if colKeys.IsEmpty then None else Some table.Values.[colKeys.MinimumElement]
                        match tryExistingCell with
                        | Some (CompositeCell.Term _) 
                        | None                              -> CompositeCell.emptyTerm
                        | Some (CompositeCell.Unitized _)   -> CompositeCell.emptyUnitized
                        | _                                 -> failwith "[extendBodyCells] This should never happen, IsTermColumn header must be paired with either term or unitized cell."
                for missingColumn,missingRow in missingKeys do
                    table.Values.[(missingColumn,missingRow)] <- empty

    member this.ColumnCount 
        with get() = ArcTableAux.getColumnCount (this.Headers)

    member this.RowCount 
        with get() = ArcTableAux.getRowCount (this.Values)

    member this.Copy() : ArcTable = 
        ArcTable.create(
            this.Name,
            ResizeArray(this.Headers), 
            Dictionary(this.Values)
        )

    //member this.AddColumn (header:CompositeHeader, ?cells: CompositeCell [], ?index: int, ?forceReplace: bool) : unit = 
    //    let index = Option.defaultValue this.ColumnCount index
    //    let cells = Option.defaultValue [||] cells
    //    let forceReplace = Option.defaultValue false forceReplace
    //    // sanity checks
    //    ArcTableAux.SanityChecks.validateIndex index this.ColumnCount
    //    ArcTableAux.SanityChecks.validateColumn(CompositeColumn.create(header, cells))
    //    // 
    //    ArcTable.insertRawColumn header cells index forceReplace this
    //    //ArcTable.extendBodyCells this

    static member addColumn (header: CompositeHeader,cells: CompositeCell [],?Index: int ,?ForceReplace : bool) : (ArcTable -> ArcTable) =
        fun table ->
            let newTable = table.Copy()
            newTable.AddColumn(header, cells, ?Index = Index)
            newTable

    member this.AddColumns (columns: CompositeColumn [], ?index: int, ?forceReplace: bool) : unit = 
        
        let mutable index = Option.defaultValue this.ColumnCount index
        // sanity checks
        ArcTableAux.SanityChecks.validateIndex index this.ColumnCount
        ArcTableAux.SanityChecks.validateNoDuplicateUniqueColumns columns
        columns |> Array.iter (fun x -> ArcTableAux.SanityChecks.validateColumn x)
        
        columns
        |> Array.iteri (fun i col ->    
            let prevHeadersCount = this.Headers.Count
            this.AddColumn(col.Header, col.Cells, Index = index, ?forceReplace = forceReplace)
            // Check if more headers, otherwise `ArcTableAux.insertColumn` replaced a column and we do not need to increase index.
            if this.Headers.Count > prevHeadersCount then index <- index + 1     
        )
        ArcTable.extendBodyCells this



    static member addColumns (columns: CompositeColumn [],?index: int) =
        fun (table:ArcTable) ->
            let newTable = table.Copy()
            newTable.AddColumns(columns, ?index = index)
            newTable

    member this.GetRow(index : int) = 
        if index < this.RowCount then
            [for i = 0 to this.ColumnCount - 1 do this.TryGetCellAt(i, index) |> Option.defaultValue (CompositeCell.FreeText "")]           
        else
            failwithf "Index %i is out of range. RowCount is %i" index this.RowCount
        
    member this.GetColumn(index:int) =
        ArcTableAux.SanityChecks.validateIndex index this.ColumnCount
        let h = this.Headers.[index]
        let cells = 
            this.Values |> Seq.choose (fun x -> 
                match x.Key with
                | col, i when col = index -> Some (i, x.Value)
                | _ -> None
            )
            |> Seq.sortBy fst
            |> Seq.map snd
            |> Array.ofSeq
        CompositeColumn.create(h, cells)

    static member getColumn (index:int) (table:ArcTable) = table.GetColumn(index)

    member this.SetColumn (index:int, column:CompositeColumn) =
        ArcTableAux.SanityChecks.validateIndex index this.ColumnCount
        ArcTableAux.SanityChecks.validateColumn(column)
        /// remove to be replaced header, this is only used to check if any OTHER header is of the same unique type as column.Header
        /// MUST USE "Seq.removeAt" to not remove in mutable object!
        let otherHeaders = this.Headers |> Seq.removeAt index
        ArcTableAux.SanityChecks.validateNoDuplicateUnique column.Header otherHeaders
        let nextHeader = 
            this.Headers.[index] <- column.Header
        let nextBody =
            column.Cells |> Array.iteri (fun i v -> this.Values.[(index,i)] <- v)
        ()
        //{
        //    this with
        //        Headers = nextHeader
        //        Values = nextBody
        //}

    static member setColumn (index:int) (column:CompositeColumn) (table:ArcTable) = table.SetColumn(index, column)
        
    member this.SetHeader (index:int, newHeader: CompositeHeader, ?forceConvertCells: bool) =
        let forceConvertCells = Option.defaultValue false forceConvertCells
        ArcTableAux.SanityChecks.validateIndex index this.ColumnCount
        /// remove to be replaced header, this is only used to check if any OTHER header is of the same unique type as column.Header
        /// MUST USE "Seq.removeAt" to not remove in mutable object!
        let otherHeaders = this.Headers |> Seq.removeAt index
        ArcTableAux.SanityChecks.validateNoDuplicateUnique newHeader otherHeaders
        // Test if column is still valid with new header
        let c = { this.GetColumn(index) with Header = newHeader }
        if c.validate() then
            let setHeader = this.Headers.[index] <- newHeader
            ()
        // if we force convert cells, we want to convert the existing cells to a valid cell type for the new header
        elif forceConvertCells then
            let convertedCells =
                match newHeader with
                | isTerm when newHeader.IsTermColumn -> c.Cells |> Array.map (fun c -> c.ToTermCell())
                | _ -> c.Cells |> Array.map (fun c -> c.ToFreeTextCell())
            let newColumn = CompositeColumn.create(newHeader,convertedCells)
            this.SetColumn(index, newColumn)
        else
            failwith "Tried setting header for column with invalid type of cells. Set `forceConvertCells` flag to automatically convert cells into valid CompositeCell type."

    static member setHeader (index:int) (header:CompositeHeader) (table:ArcTable) = table.SetHeader(index, header)

    static member insertParameterValue (t : ArcTable) (p : ProcessParameterValue) : ArcTable = 
        raise (System.NotImplementedException())

    static member getParameterValues (t : ArcTable) : ProcessParameterValue [] = 
        raise (System.NotImplementedException())

    // no 
    static member addProcess = 
        raise (System.NotImplementedException())

    static member addRow input output values = //yes
        raise (System.NotImplementedException())

    static member getProtocols (t : ArcTable) : Protocol [] = 
        raise (System.NotImplementedException())

    static member getProcesses (t : ArcTable) : Process [] = 
        raise (System.NotImplementedException())

    static member fromProcesses (ps : Process array) : ArcTable = 
        raise (System.NotImplementedException())

    override this.ToString() =
        [
            $"Table: {this.Name}"
            "-------------"
            this.Headers |> Seq.map (fun x -> x.ToString()) |> String.concat "\t|\t"
            for rowI = 0 to this.RowCount-1 do
                this.GetRow(rowI) |> Seq.map (fun x -> x.ToString()) |> String.concat "\t|\t"
        ]
        |> String.concat "\n"