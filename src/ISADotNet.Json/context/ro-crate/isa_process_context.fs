namespace ISADotNet.Json.ROCrateContext

module Process =

  let context =
  """
{
  "@context": {
    "sdo": "http://schema.org/",
    "arc": "https://github.com/nfdi4plants/ARC_ontology/blob/main/ARC_v1.1.owl/",
    "Process": "sdo:Thing",
    "ArcProcess": "arc:process_sequence",
    "name": "arc:name",
    "executesProtocol": "arc:executesProtocol",
    "performer": "aec:performer",
    "date": "arc:date",
    "previousProcess": "arc:has_previous_process",
    "nextProcess": "arc:has_next_process",
    "input": "arc:has_input",
    "output": "arc:has_output",
    "comments": "sdo:disambiguatingDescription"
  }
}
  """