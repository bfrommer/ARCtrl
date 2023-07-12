namespace ISADotNet.Json.ROCrateContext

module ProtocolParameter =

  let context =
  """
{
  "@context": {
    "sdo": "http://schema.org/",
    "arc": "https://github.com/nfdi4plants/ARC_ontology/blob/main/ARC_v1.1.owl/",
    "ProtocolParameter": "sdo:Thing",
    "ArcProtocolParameter": "arc:protocol_parameter",
    "parameterName": "arc:has_parameter_name"
  }
}
  """