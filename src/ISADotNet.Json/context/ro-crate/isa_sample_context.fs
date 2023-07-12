namespace ISADotNet.Json.ROCrateContext

module Sample =

  let context =
  """
{
  "@context": {
    "sdo": "http://schema.org/",
    "arc": "https://github.com/nfdi4plants/ARC_ontology/blob/main/ARC_v1.1.owl/",
    "Sample": "sdo:Thing",
    "ArcSample": "arc:sample",
    "name": "arc:name",
    "characteristics": "arc:has_characteristic",
    "factorValues": "arc:has_factor_value",
    "derivesFrom": "arc:derives_from"
  }
}
  """