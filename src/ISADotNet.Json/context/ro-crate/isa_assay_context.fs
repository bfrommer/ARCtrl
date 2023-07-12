namespace ISADotNet.Json.ROCrateContext

module Assay =

  let context =
  """
{
  "@context": {
    "sdo": "https://schema.org/",
    "arc": "https://github.com/nfdi4plants/ARC_ontology/blob/main/ARC_v1.1.owl/",
    "Assay": "sdo:Dataset",
    "ArcAssay": "arc:assay",
    "measurementType": "sdo:variableMeasured",
    "technologyType": "sdo:measurementTechnique",
    "technologyPlatform": "sdo:instrument",
    "dataFiles": "sdo:hasPart",

    "materials": "arc:has_assay_material",
    "otherMaterials": "arc:has_assay_material",
    "samples": "arc:has_assay_material",
    "characteristicCategories": "arc:has_characteristic_category",
    "processSequences": "arc:has_process_sequence",
    "unitCategories": "arc:has_unit_category",
    "comments": "sdo:disambiguatingDescription",
    "filename": "sdo:url"
  }
}
  """