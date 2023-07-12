namespace ISADotNet.Json.ROCrateContext

module Data =

  let context =
  """
{
  "@context": {
    "sdo": "http://schema.org/",
    "arc": "https://github.com/nfdi4plants/ARC_ontology/blob/main/ARC_v1.1.owl/",
    "Data": "sdo:MediaObject",
    "ArcData": "arc:Data",
    "type": "arc:data_type",
    "name": "sdo:name",
    "comments": "sdo:disambiguatingDescription"
  }
}
  """