namespace Foundation.ExperienceEditor.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Data.Managers;
    using Sitecore.Data.Templates;
    using Sitecore.Text;

    /// <summary>
    /// Holds all custom extension for the <see cref="Sitecore.Data.Items.Item"/>.
    /// </summary>
    public static class ItemExtensions
    {
        #region Public Static Methods 

        /// <summary>
        /// Gets the list of custom fields for the current item.
        /// </summary>
        /// <param name="item">
        /// The item for which the custom fields should be retrieved.
        /// </param>
        /// <param name="whitelistFieldIds">Whitelist and filter specified field IDs, if not null or empty</param>
        /// <returns>
        /// Returns the list of sorted custom fields, even in base templates, for the current item.
        /// </returns>
        public static List<FieldDescriptor> CreateFieldDescriptors(this Item item, List<ID> whitelistFieldIds = null)
        {
            Template template = TemplateManager.GetTemplate(
                item.TemplateID, item.Database);

            TemplateField[] allFields = template.GetFields(true);
            IEnumerable<TemplateField> fieldsQuery = allFields.Where(ItemUtil.IsDataField);

            if (whitelistFieldIds != null && whitelistFieldIds.Any())
            {
                fieldsQuery = fieldsQuery.Where(x => whitelistFieldIds.Contains(x.ID));
            }

            List<string> fields = fieldsQuery.OrderBy(x => x.Sortorder)
                 .Select(x => x.Name)
                 .ToList();

            List<FieldDescriptor> fieldList = new List<FieldDescriptor>();
            ListString fieldString = new ListString(fields);

            foreach (string field in new ListString(fieldString))
            {
                fieldList.Add(new FieldDescriptor(item, field));
            }

            return fieldList;
        }

        #endregion
    }
}