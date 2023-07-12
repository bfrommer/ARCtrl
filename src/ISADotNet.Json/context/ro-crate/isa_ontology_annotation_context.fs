namespace ISADotNet.Json.ROCrateContext

module OntologyAnnotation =

  let context =
  """
{
  "@context": {
    "sdo": "http://schema.org/",
    "OntologyAnnotation": "sdo:DefinedTerm",
    "annotationValue": "sdo:name",
    "termSource": "sdo:inDefinedTermSet",
    "termAccession": "sdo:termCode",
    "comments": "sdo:disambiguatingDescription"
  }
}
  """