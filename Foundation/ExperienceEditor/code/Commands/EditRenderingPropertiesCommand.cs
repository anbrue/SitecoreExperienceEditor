namespace Foundation.ExperienceEditor.Commands
{
    using Foundation.ExperienceEditor.Models;
    using Sitecore;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Shell.Applications.WebEdit.Commands;
    using Sitecore.Web;
    using Sitecore.Web.UI.Sheer;

    /// <summary>
    /// Represents an enhanced version of 
    /// <see cref="Sitecore.Shell.Applications.WebEdit.Commands.EditRenderingProperties"/>.
    /// </summary>
    public class EditRenderingPropertiesCommand : EditRenderingProperties
    {
        #region Protected Methods 

        /// <summary>
        /// Copied from 
        /// <see cref="Sitecore.Shell.Applications.WebEdit.Commands.EditRenderingProperties"/> class.
        /// </summary>
        /// <param name="context"></param>
        /// <remarks>
        /// Changed <see cref="Sitecore.Shell.Applications.Layouts.DeviceEditor.RenderingParameters"/>
        /// to <see cref="AMRenderingParameters"/> class.
        /// </remarks>
        protected override void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            int @int = MainUtil.GetInt(args.Parameters["selectedindex"], -1);
            if (@int < 0)
            {
                return;
            }
            Item clientContentItem = WebEditUtil.GetClientContentItem(Client.ContentDatabase);
            CustomRenderingParameters renderingParameters = new CustomRenderingParameters
            {
                Args = args,
                DeviceId = args.Parameters["device"],
                SelectedIndex = @int,
                HandleName = args.Parameters["handle"],
                Item = clientContentItem
            };

            if (!renderingParameters.Show())
            {
                return;
            }

            if (args.HasResult)
            {
                string sessionString = WebUtil.GetSessionString(args.Parameters["handle"]);
                sessionString = EditRenderingPropertiesCommand.GetLayout(sessionString);
                SheerResponse.SetAttribute("scLayoutDefinition", "value", sessionString);

                SheerResponse.Eval(
                    "window.parent.Sitecore.PageModes.ChromeManager.handleMessage('chrome:rendering:propertiescompleted');");

                SheerResponse.Eval(
                    "ExperienceEditor.ribbonDocument().querySelector('[data-sc-id=\"QuickSave\"]').click();");
            }
            else
            {
                SheerResponse.SetAttribute("scLayoutDefinition", "value", string.Empty);
            }
            WebUtil.RemoveSessionValue(args.Parameters["handle"]);
        }

        #endregion

        /// <summary>
        /// Copied from 
        /// <see cref="Sitecore.Shell.Applications.WebEdit.Commands.EditRenderingProperties"/> class.
        /// </summary>
        /// <param name="context"></param>
        private static string GetLayout(string layout)
        {
            Assert.ArgumentNotNull(layout, "layout");
            return WebEditUtil.ConvertXMLLayoutToJSON(layout);
        }
    }
}