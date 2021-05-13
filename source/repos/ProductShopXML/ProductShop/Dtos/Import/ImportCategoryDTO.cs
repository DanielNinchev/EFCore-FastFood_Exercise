using System.Xml.Serialization;

namespace ProductShop.Dtos
{
    [XmlType("Category")]
    public class ImportCategoryDTO
    {
        [XmlElement("name")]
        public string Name { get; set; }
    }
}
