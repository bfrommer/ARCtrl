namespace ISADotNet.Json.ROCrateContext

module FactorValue =

  let context =
  """
{
  "@context": {
    "sdo": "http://schema.org/",
    "arc": "https://github.com/nfdi4plants/ARC_ontology/blob/main/ARC_v1.1.owl/",
    "FactorValue": "sdo:PropertyValue",
    "ArcFactorValue": "arc:factor_value",
    "category": "arc:category",
    "value": "arc:has_value",
    "unit": "arc:has_unit"
  }
}
  """