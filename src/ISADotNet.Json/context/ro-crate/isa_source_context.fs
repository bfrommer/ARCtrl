namespace ISADotNet.Json.ROCrateContext

module Source =

  let context =
  """
{
  "@context": {
    "sdo": "http://schema.org/",
    "arc": "https://github.com/nfdi4plants/ARC_ontology/blob/main/ARC_v1.1.owl/",
    "Source": "sdo:Thing",
    "ArcSource": "arc:source",
    "identifier": "sdo:identifier",
    "name": "arc:name",
    "characteristics": "arc:has_characteristic"
  }
}
  """