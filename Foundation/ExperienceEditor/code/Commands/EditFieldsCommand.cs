namespace Foundation.ExperienceEditor.Commands
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using Extensions;
    using Sitecore;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Shell.Applications.WebEdit;
    using Sitecore.Shell.Applications.WebEdit.Commands;
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.Web.UI.Sheer;

    /// <summary>
    /// Represents an enhanced verison of <see cref="Sitecore.Shell.Applications.WebEdit.Commands.FieldEditor" />
    /// with getting the fields dynamically.
    /// </summary>
    public class EditFieldsCommand : WebEditCommand
    {
        #region Methods

        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            if (context.Items.Length < 1)
            {
                return;
            }

            Context.ClientPage.Start(
                this,
                "StartFieldEditor",
                new ClientPipelineArgs(context.Parameters)
                {
                    Parameters =
                    {
                        {
                            "uri",
                            context.Items[0].Uri.ToString()
                        }
                    }
                });
        }

        public override CommandState QueryState(CommandContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            return context.Items.Length != 0 &&
                context.Items[0] != null &&
                !context.Items[0].Access.CanWrite()
                    ? CommandState.Disabled
                    : CommandState.Enabled;
        }

        protected void StartFieldEditor(ClientPipelineArgs args)
        {
            NameValueCollection nameValueCollection = HttpContext.Current?.Handler is Page handler
                ? handler.Request.Form
                : null;

            if (nameValueCollection == null)
            {
                return;
            }

            Assert.ArgumentNotNull(args, nameof(args));

            if (!args.IsPostBack)
            {
                SheerResponse.ShowModalDialog(
                    this.GetOptions(args, nameValueCollection).ToUrlString().ToString(),
                    "720",
                    "520",
                    string.Empty,
                    true);

                args.WaitForPostBack();
            }
            else
            {
                if (!args.HasResult)
                {
                    return;
                }

                PageEditFieldEditorOptions
                    .Parse(args.Result)
                    .SetPageEditorFieldValues(nameValueCollection);

                SheerResponse.Eval(
                    "ExperienceEditor.ribbonDocument().querySelector('[data-sc-id=\"QuickSave\"]').click();");
            }
        }

        /// <summary>
        /// Gets all option as base class, only the fields are get via private function, because
        /// they are not passed through pipeline.
        /// </summary>
        /// <param name="args">
        /// The arguments which come from click-command in webedit button item's field "Click".
        /// </param>
        /// <param name="form">
        /// The current form for the current page in which the webedit button is clicked.
        /// </param>
        /// <returns></returns>
        protected PageEditFieldEditorOptions GetOptions(
            ClientPipelineArgs args,
            NameValueCollection form)
        {
            Assert.ArgumentNotNull(args, nameof(args));
            Assert.ArgumentNotNull(form, nameof(form));

            Item datasourceItem = Database.GetItem(ItemUri.Parse(args.Parameters["uri"]));

            Assert.IsNotNull(datasourceItem, "item");

            string commandID = args.Parameters["command"];

            Assert.IsNotNullOrEmpty(commandID, "Field Editor command expects 'command' parameter");

            // Optional field ID whitelist (dash separated)
            string fieldsParameter = args.Parameters["fields"];
            List<ID> fields = this.ParseFieldsParameter(fieldsParameter);

            Item commandItem = Client.CoreDatabase.GetItem(commandID);

            Assert.IsNotNull(commandItem, "command item");

            // if fields is not empty, only specified field ids are displayed
            List<FieldDescriptor> fieldDescriptorList =
                datasourceItem.CreateFieldDescriptors(fields);

            PageEditFieldEditorOptions fieldEditorOptions =
                new PageEditFieldEditorOptions(
                    form,
                    fieldDescriptorList);

            fieldEditorOptions.Title = commandItem["Title"];
            fieldEditorOptions.Icon = commandItem["Icon"];

            return fieldEditorOptions;
        }

        /// <summary>
        /// Parse ID string dash separated field parameter to a ID list
        /// </summary>
        /// <param name="fieldsParameter">The field parameter as string</param>
        /// <returns>The parsed ID list</returns>
        private List<ID> ParseFieldsParameter(string fieldsParameter)
        {
            return string.IsNullOrWhiteSpace(fieldsParameter)
                ? new List<ID>()
                : fieldsParameter.Split('|')
                                 .ToList()
                                 .Select(stringId => ID.Parse(stringId, null))
                                 .Where(id => !ID.IsNullOrEmpty(id))
                                 .ToList();
        }

        #endregion
    }
}