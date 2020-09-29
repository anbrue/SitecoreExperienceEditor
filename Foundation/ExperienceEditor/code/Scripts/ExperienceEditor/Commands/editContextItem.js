define(["sitecore", "/-/speak/v1/ExperienceEditor/ExperienceEditor.js"], function (Sitecore, ExperienceEditor)
{
    Sitecore.Commands.editContextItem =
    {
        canExecute: function (context)
        {
            if (!ExperienceEditor.isInMode("edit") || context.currentContext.isFallback || !context.currentContext.canReadLanguage || !context.currentContext.canWriteLanguage)
            {
                return false;
            }

            return true;
        },
        execute: function (context)
        {
            ExperienceEditor.PipelinesUtil.generateRequestProcessor("ExperienceEditor.GenerateFieldEditorUrl", function (response)
            {
                var DialogUrl = response.responseValue.value;
                var dialogFeatures = "dialogHeight: 520px; dialogWidth: 680px; header:Edit context item fields; ";
                ExperienceEditor.Dialogs.showModalDialog(DialogUrl, "", dialogFeatures, null);
            }).execute(context);
        }
    };
});