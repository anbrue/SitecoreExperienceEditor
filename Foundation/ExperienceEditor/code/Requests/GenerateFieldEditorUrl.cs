namespace Foundation.ExperienceEditor.Requests
{
    using System.Collections.Generic;
    using Foundation.ExperienceEditor.Extensions;
    using Sitecore.Data;
    using Sitecore.ExperienceEditor.Speak.Server.Contexts;
    using Sitecore.ExperienceEditor.Speak.Server.Requests;
    using Sitecore.ExperienceEditor.Speak.Server.Responses;
    using Sitecore.Shell.Applications.ContentEditor;

    /// <summary>
    /// </summary>
    /// <remarks>
    /// How to: http://reyrahadian.com/2015/04/15/sitecore-8-adding-edit-meta-data-button-in-experience-editor/
    /// </remarks>
    public class GenerateFieldEditorUrl : PipelineProcessorRequest<ItemContext>
    {
        #region Public Override Methods

        /// <summary>
        /// Gets the response value for the pipeline to process the editor form.
        /// </summary>
        /// <returns>
        /// Returns the pipeline response with the generated url for the dialog form.
        /// </returns>
        public override PipelineProcessorResponseValue ProcessRequest()
        {
            return new PipelineProcessorResponseValue
            {
                Value = this.GenerateUrl()
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates the url for the editor dialog.
        /// </summary>
        /// <returns>
        /// Returns the url for the dialog form.
        /// </returns>
        public string GenerateUrl()
        {
            List<FieldDescriptor> fieldList = RequestContext.Item.CreateFieldDescriptors();

            FieldEditorOptions fieldeditorOption = new FieldEditorOptions(fieldList)
            {
                PreserveSections = true,
                //Save item when ok button is pressed
                SaveItem = true
            };

            return fieldeditorOption.ToUrlString().ToString();
        }

        #endregion
    }
}