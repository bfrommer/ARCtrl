namespace ISADotNet.Json.ROCrateContext

module ProcessParameterValue =

  let context =
  """
{
  "@context": {
    "sdo": "http://schema.org/",
    "arc": "https://github.com/nfdi4plants/ARC_ontology/blob/main/ARC_v1.1.owl/",
    "ProcessParameterValue": "sdo:PropertyValue",
    "ArcProcessParameterValue": "arc:parameter_value",
    "category": "arc:has_category",
    "value": "arc:value",
    "unit": "arc:has_unit"
  }
}
  """