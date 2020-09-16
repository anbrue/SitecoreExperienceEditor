# SitecoreExperienceEditor
Holds some enhancements for the Sitecore Experience Editor.

# Edit Fields Command 
# Edit Rendering Properties Command

# Put in the Foundation.ExperienceEditor Project into your solution and install the package from /Packages

# Check the RenderingChromeType.js: if you have customized it already, search for the following lines and move them to your custom file: 

line 221-224: 
  /* Added custom command for calling SitecoreCommand from JS*/
  case "chrome:rendering:runcommand":
  this.runcommand(params.command, sender);
  break;

line 228-243
    /* runs the passed Sitecore Command which has to be defines in the Commands.config */
    runcommand: function (commandName, sender)
    {
        Sitecore.PageModes.PageEditor.layoutDefinitionControl().value =
            Sitecore.PageModes.PageEditor.layout().val();

        var controlId = this.controlId();

        if (sender)
        {
            controlId = sender.controlId();
        }

        Sitecore.PageModes.PageEditor.postRequest(
            commandName + "(uniqueId=" + this.uniqueId() + ",controlId=" + controlId + ")");
    },

# Now you can add the Edit Fields or Edit Rendering Properties Command to your Custom Experience Buttons
