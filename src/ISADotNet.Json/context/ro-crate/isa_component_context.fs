namespace ISADotNet.Json.ROCrateContext

module Component =

  let context =
  """
{
  "@context": {
    "sdo": "http://schema.org/",
    "arc": "https://github.com/nfdi4plants/ARC_ontology/blob/main/ARC_v1.1.owl/",
    "Component": "sdo:Thing",
    "ArcComponent": "arc:component",
    "componentName": "arc:name",
    "componentType": "arc:has_component_type"
  }
}
  """