namespace Foundation.ExperienceEditor.Models
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Web;
    using Sitecore;
    using Sitecore.Collections;
    using Sitecore.Data;
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Data.Managers;
    using Sitecore.Data.Templates;
    using Sitecore.Diagnostics;
    using Sitecore.Layouts;
    using Sitecore.SecurityModel;
    using Sitecore.Shell.Applications.WebEdit;
    using Sitecore.Text;
    using Sitecore.Web;
    using Sitecore.Web.UI.Sheer;

    /// <summary>
    /// Represents <see cref="Sitecore.Shell.Applications.Layouts.DeviceEditor.RenderingParameters"/>
    /// with some nice modifications :) Unfortenately we had to copy everything because most of 
    /// the properties and methods are private.
    /// </summary>
    public class CustomRenderingParameters
    {
        #region Private Fields 

        /// <summary>
        /// The current pipeline arguments.
        /// </summary>
        private ClientPipelineArgs args;

        /// <summary>
        /// The selected device ID.
        /// </summary>
        private string deviceId;

        /// <summary>
        /// The name of the handle.
        /// </summary>
        private string handleName;

        /// <summary>
        /// The current layout definition.
        /// </summary>
        private LayoutDefinition layoutDefinition;

        #endregion

        #region Public Properties 
        /// <summary>
        /// Gets or sets the args.
        /// </summary>
        /// <value>
        /// The args.
        /// </value>
        public ClientPipelineArgs Args
        {
            get
            {
                return args;
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                args = value;
            }
        }

        /// <summary>
        /// Gets or sets the  item.
        /// </summary>
        /// <value>The item.</value>
        public Item Item
        {
            private get;
            set;
        }

        /// <summary>
        /// Gets or sets the device ID.
        /// </summary>
        /// <value>
        /// The device ID.
        /// </value>
        public string DeviceId
        {
            get
            {
                return deviceId ?? string.Empty;
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                deviceId = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the handle.
        /// </summary>
        /// <value>
        /// The name of the handle.
        /// </value>
        public string HandleName
        {
            get
            {
                return handleName ?? "SC_DEVICEEDITOR";
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                handleName = value;
            }
        }

        /// <summary>
        /// Gets or sets the index of the selected.
        /// </summary>
        /// <value>
        /// The index of the selected.
        /// </value>
        public int SelectedIndex
        {
            get;
            set;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows this instance.
        /// </summary>
        /// <returns>
        /// The boolean.
        /// </returns>
        public bool Show()
        {
            if (Args.IsPostBack)
            {
                if (Args.HasResult)
                {
                    Save();
                }
                return true;
            }
            if (SelectedIndex < 0)
            {
                return true;
            }
            RenderingDefinition renderingDefinition = GetRenderingDefinition();
            if (renderingDefinition == null)
            {
                return true;
            }
            string text = null;
            if (!string.IsNullOrEmpty(renderingDefinition.ItemID))
            {
                Item item = Client.ContentDatabase.GetItem(renderingDefinition.ItemID, Item.Language);
                if (item != null)
                {
                    LinkField linkField = item.Fields["Customize Page"];
                    Assert.IsNotNull(linkField, "linkField");
                    if (!string.IsNullOrEmpty(linkField.Url))
                    {
                        text = linkField.Url;
                    }
                }
            }
            Dictionary<string, string> parameters = GetParameters(renderingDefinition);
            List<FieldDescriptor> fields = GetFields(renderingDefinition, parameters);
            RenderingParametersFieldEditorOptions renderingParametersFieldEditorOptions =
                new RenderingParametersFieldEditorOptions(fields)
                {
                    DialogTitle = "Control Properties",
                    HandleName = HandleName,
                    PreserveSections = true
                };
            SetCustomParameters(renderingDefinition, renderingParametersFieldEditorOptions);
            UrlString urlString;
            if (!string.IsNullOrEmpty(text))
            {
                urlString = new UrlString(text);
                renderingParametersFieldEditorOptions.ToUrlHandle().Add(urlString, HandleName);
            }
            else
            {
                urlString = renderingParametersFieldEditorOptions.ToUrlString();
            }
            SheerResponse.ShowModalDialog(new ModalDialogOptions(urlString.ToString())
            {
                Width = "720",
                Height = "480",
                Response = true,
                Header = renderingParametersFieldEditorOptions.DialogTitle
            });
            args.WaitForPostBack();
            return false;
        }

        #endregion

        #region Private Methods 

        /// <summary>
        /// Sets the custom parameters.
        /// </summary>
        /// <param name="renderingDefinition">The rendering definition.</param>
        /// <param name="options">The options.</param>
        private void SetCustomParameters(
            RenderingDefinition renderingDefinition, RenderingParametersFieldEditorOptions options)
        {
            Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
            Assert.ArgumentNotNull(options, "options");

            Item item = (renderingDefinition.ItemID != null) ?
                Client.ContentDatabase.GetItem(renderingDefinition.ItemID) : null;

            if (item != null)
            {
                options.Parameters["rendering"] = item.Uri.ToString();
            }
            if (Item != null)
            {
                options.Parameters["contentitem"] = Item.Uri.ToString();
            }
            if (WebEditUtil.IsRenderingPersonalized(renderingDefinition))
            {
                options.Parameters["warningtext"] = "There are personalization conditions defined for this control. Changing control properties may effect them.";
            }
            if (!string.IsNullOrEmpty(renderingDefinition.MultiVariateTest))
            {
                options.Parameters["warningtext"] = "There is a multivariate test set up for this control. Changing control properties may effect the test.";
            }
        }

        /// <summary>
        /// Gets the fields.
        /// </summary>
        /// <param name="renderingDefinition">
        /// The rendering definition.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <returns>
        /// The fields.
        /// </returns>
        private List<FieldDescriptor> GetFields(
            RenderingDefinition renderingDefinition,
            Dictionary<string, string> parameters)
        {
            Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
            Assert.ArgumentNotNull(parameters, "parameters");
            List<FieldDescriptor> list = new List<FieldDescriptor>();
            Item standardValuesItem;
            using (new SecurityDisabler())
            {
                standardValuesItem = GetStandardValuesItem(renderingDefinition);
            }
            if (standardValuesItem == null)
            {
                return list;
            }

            FieldCollection fields = standardValuesItem.Fields;
            fields.ReadAll();

            Dictionary<string, string> dictionary = new Dictionary<string, string>(parameters);

            foreach (TemplateField templateField in
                this.GetRelevantTemplateFields(renderingDefinition).ToList())
            {
                Field standardValue = fields.FirstOrDefault(x => x.Name == templateField.Name);

                string value = GetValue(templateField.Name, renderingDefinition, parameters);

                FieldDescriptor item = new FieldDescriptor(standardValuesItem, templateField.Name)
                {
                    Value = (value ?? standardValue.Value),
                    ContainsStandardValue = ((value == null) ? true : false)
                };

                list.Add(item);
                dictionary.Remove(templateField.Name);
            }

            GetAdditionalParameters(list, standardValuesItem, dictionary);

            return list;
        }

        private IEnumerable<TemplateField> GetRelevantTemplateFields(
            RenderingDefinition renderingDefinition)
        {
            Item renderingItem = GetRenderingItem(renderingDefinition);
            string text = renderingItem["Parameters Template"];

            if (string.IsNullOrEmpty(text))
            {
                text = CustomRenderingParameters.StandardRenderingParametersTemplateId.ToString();
            }

            TemplateItem templateItem = renderingItem.Database.GetItem(
                new ID(text), renderingItem.Language);

            Template template = TemplateManager.GetTemplate(
                templateItem.ID, templateItem.Database);

            if (templateItem == null)
            {
                return new List<TemplateField>();
            }

            IEnumerable<TemplateField> relevantFields =
                template.GetFields(true).Where(CustomRenderingParameters.IsDataField);

            return relevantFields;
        }

        /// <summary>
        /// Gets the rendering item.
        /// </summary>
        /// <param name="renderingDefinition">
        /// The rendering definition.
        /// </param>
        /// <returns>
        /// The rendering item.
        /// </returns>
        private Item GetRenderingItem(RenderingDefinition renderingDefinition)
        {
            Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
            string itemID = renderingDefinition.ItemID;
            if (!string.IsNullOrEmpty(itemID))
            {
                return Client.ContentDatabase.GetItem(itemID, Item.Language);
            }
            return null;
        }

        /// <summary>
        /// Gets the standard values item.
        /// </summary>
        /// <param name="renderingDefinition">
        /// The rendering definition.
        /// </param>
        /// <returns>
        /// The standard values item.
        /// </returns>
        private Item GetStandardValuesItem(RenderingDefinition renderingDefinition)
        {
            Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
            Item renderingItem = GetRenderingItem(renderingDefinition);
            if (renderingItem == null)
            {
                return null;
            }
            return RenderingItem.GetStandardValuesItemFromParametersTemplate(renderingItem);
        }

        /// <summary>
        /// Gets the definition.
        /// </summary>
        /// <returns>
        /// The definition.
        /// </returns>
        private LayoutDefinition GetLayoutDefinition()
        {
            if (layoutDefinition == null)
            {
                string sessionString = WebUtil.GetSessionString(HandleName);
                Assert.IsNotNull(sessionString, "sessionValue");
                layoutDefinition = LayoutDefinition.Parse(sessionString);
            }
            return layoutDefinition;
        }

        /// <summary>
        /// Gets the rendering definition.
        /// </summary>
        /// <returns>
        /// The rendering definition.
        /// </returns>
        private RenderingDefinition GetRenderingDefinition()
        {
            ArrayList renderings = GetLayoutDefinition().GetDevice(DeviceId).Renderings;
            if (renderings == null)
            {
                return null;
            }
            return renderings[MainUtil.GetInt(SelectedIndex, 0)] as RenderingDefinition;
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        private void Save()
        {
            RenderingDefinition renderingDefinition = GetRenderingDefinition();
            if (renderingDefinition != null)
            {
                Item standardValuesItem;
                using (new SecurityDisabler())
                {
                    standardValuesItem = GetStandardValuesItem(renderingDefinition);
                }
                if (standardValuesItem != null)
                {
                    UrlString urlString = new UrlString();
                    foreach (FieldDescriptor field in RenderingParametersFieldEditorOptions.Parse(args.Result).Fields)
                    {
                        SetValue(renderingDefinition, urlString, standardValuesItem.Fields[field.FieldID].Name, field.Value);
                    }
                    renderingDefinition.Parameters = urlString.ToString();
                    LayoutDefinition layoutDefinition = GetLayoutDefinition();
                    SetLayoutDefinition(layoutDefinition);
                }
            }
        }

        /// <summary>
        /// Sets the definition.
        /// </summary>
        /// <param name="layout">
        /// The layout.
        /// </param>
        private void SetLayoutDefinition(LayoutDefinition layout)
        {
            Assert.ArgumentNotNull(layout, "layout");
            WebUtil.SetSessionValue(HandleName, layout.ToXml());
        }

        #endregion

        #region Private Static Methods 
        /// <summary>
        /// Gets the additional parameters.
        /// </summary>
        /// <param name="fieldDescriptors">
        /// The field descriptors.
        /// </param>
        /// <param name="standardValues">
        /// The standard values.
        /// </param>
        /// <param name="additionalParameters">
        /// The addtional parameters.
        /// </param>
        private static void GetAdditionalParameters(
            List<FieldDescriptor> fieldDescriptors,
            Item standardValues,
            Dictionary<string, string> additionalParameters)
        {
            Assert.ArgumentNotNull(fieldDescriptors, "fieldDescriptors");
            Assert.ArgumentNotNull(standardValues, "standardValues");
            Assert.ArgumentNotNull(additionalParameters, "additionalParameters");
            string fieldName = "Additional Parameters";
            if (standardValues.Fields[fieldName] != null || additionalParameters.Any())
            {
                UrlString urlString = new UrlString();
                foreach (string key in additionalParameters.Keys)
                {
                    urlString[key] = HttpUtility.UrlDecode(additionalParameters[key]);
                }
                fieldDescriptors.Add(new FieldDescriptor(standardValues, fieldName)
                {
                    Value = urlString.ToString()
                });
            }
        }

        /// <summary>
        /// Gets the caching.
        /// </summary>
        /// <param name="renderingDefinition">
        /// The rendering definition.
        /// </param>
        /// <returns>
        /// The caching.
        /// </returns>
        private static string GetCaching(RenderingDefinition renderingDefinition)
        {
            Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
            return ((renderingDefinition.Cachable == "1") ? "1" : "0") +
                "|" + ((renderingDefinition.ClearOnIndexUpdate == "1") ? "1" : "0") + "|" +
                ((renderingDefinition.VaryByData == "1") ? "1" : "0") + "|" +
                ((renderingDefinition.VaryByDevice == "1") ? "1" : "0") + "|" +
                ((renderingDefinition.VaryByLogin == "1") ? "1" : "0") + "|" +
                ((renderingDefinition.VaryByParameters == "1") ? "1" : "0") + "|" +
                ((renderingDefinition.VaryByQueryString == "1") ? "1" : "0") + "|" +
                ((renderingDefinition.VaryByUser == "1") ? "1" : "0");
        }

        private static readonly ID StandardRenderingParametersTemplateId =
            new ID("{8CA06D6A-B353-44E8-BC31-B528C7306971}");

        public static bool IsDataField(TemplateField templateField)
        {
            Assert.ArgumentNotNull(templateField, "templateField");

            if (templateField.Template.ID !=
                CustomRenderingParameters.StandardRenderingParametersTemplateId)
            {
                return templateField.Template.BaseIDs.Length != 0;
            }

            return false;
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <param name="renderingDefinition">
        /// The rendering definition.
        /// </param>
        /// <returns>
        /// The parameters.
        /// </returns>
        private static Dictionary<string, string> GetParameters(
            RenderingDefinition renderingDefinition)
        {
            Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            NameValueCollection nameValueCollection =
                WebUtil.ParseUrlParameters(renderingDefinition.Parameters ?? string.Empty);

            foreach (string key in nameValueCollection.Keys)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    dictionary[key] = nameValueCollection[key];
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="fieldName">
        /// The name.
        /// </param>
        /// <param name="renderingDefinition">
        /// The rendering definition.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <returns>
        /// The value.
        /// </returns>
        private static string GetValue(
            string fieldName,
            RenderingDefinition renderingDefinition,
            Dictionary<string, string> parameters)
        {
            Assert.ArgumentNotNull(fieldName, "fieldName");
            Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
            Assert.ArgumentNotNull(parameters, "parameters");

            switch (fieldName.ToLowerInvariant())
            {
                case "placeholder":
                    return renderingDefinition.Placeholder ?? string.Empty;
                case "data source":
                    return renderingDefinition.Datasource ?? string.Empty;
                case "caching":
                    return GetCaching(renderingDefinition);
                case "personalization":
                    return renderingDefinition.Conditions ?? string.Empty;
                case "tests":
                    return renderingDefinition.MultiVariateTest ?? string.Empty;
                default:
                    parameters.TryGetValue(fieldName, out string value);
                    return value;
            }
        }

        /// <summary>
        /// Sets the caching.
        /// </summary>
        /// <param name="renderingDefinition">
        /// The rendering definition.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        private static void SetCaching(RenderingDefinition renderingDefinition, string value)
        {
            Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
            Assert.ArgumentNotNull(value, "value");
            if (string.IsNullOrEmpty(value))
            {
                value = "0|0|0|0|0|0|0|0";
            }
            string[] array = value.Split('|');
            Assert.IsTrue(array.Length == 8, "Invalid caching value format");

            renderingDefinition.Cachable =
                ((array[0] == "1") ? "1" : ((renderingDefinition.Cachable != null) ? "0" : null));

            renderingDefinition.ClearOnIndexUpdate = ((array[1] == "1") ?
                "1" : ((renderingDefinition.ClearOnIndexUpdate != null) ? "0" : null));

            renderingDefinition.VaryByData = ((array[2] == "1") ?
                "1" : ((renderingDefinition.VaryByData != null) ? "0" : null));

            renderingDefinition.VaryByDevice = ((array[3] == "1") ?
                "1" : ((renderingDefinition.VaryByDevice != null) ? "0" : null));

            renderingDefinition.VaryByLogin = ((array[4] == "1") ?
                "1" : ((renderingDefinition.VaryByLogin != null) ? "0" : null));

            renderingDefinition.VaryByParameters = ((array[5] == "1") ?
                "1" : ((renderingDefinition.VaryByParameters != null) ? "0" : null));

            renderingDefinition.VaryByQueryString = ((array[6] == "1") ?
                "1" : ((renderingDefinition.VaryByQueryString != null) ? "0" : null));

            renderingDefinition.VaryByUser = ((array[7] == "1") ?
                "1" : ((renderingDefinition.VaryByUser != null) ? "0" : null));
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="renderingDefinition">
        /// The rendering definition.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="fieldName">
        /// Name of the field.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        private static void SetValue(
            RenderingDefinition renderingDefinition,
            UrlString parameters,
            string fieldName,
            string value)
        {
            Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
            Assert.ArgumentNotNull(fieldName, "fieldName");
            Assert.ArgumentNotNull(value, "value");
            Assert.ArgumentNotNull(parameters, "parameters");

            switch (fieldName.ToLowerInvariant())
            {
                case "placeholder":
                    renderingDefinition.Placeholder = value;
                    break;
                case "data source":
                    renderingDefinition.Datasource = value;
                    break;
                case "caching":
                    SetCaching(renderingDefinition, value);
                    break;
                case "personalization":
                    renderingDefinition.Conditions = value;
                    break;
                case "tests":
                    {
                        if (string.IsNullOrEmpty(value))
                        {
                            renderingDefinition.MultiVariateTest = string.Empty;
                        }
                        Item item = Client.ContentDatabase.GetItem(value);
                        if (item != null)
                        {
                            renderingDefinition.MultiVariateTest = item.ID.ToString();
                        }
                        else
                        {
                            renderingDefinition.MultiVariateTest = value;
                        }
                        break;
                    }
                case "additional parameters":
                    {
                        NameValueCollection parameters2 = new UrlString(value).Parameters;
                        parameters.Parameters.Add(parameters2);
                        break;
                    }
                default:
                    parameters[fieldName] = value;
                    break;
            }
        }

        #endregion

    }
}