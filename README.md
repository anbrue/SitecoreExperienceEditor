SitecoreExperienceEditor
Holds some enhancements for the Sitecore Experience Editor.

Edit Fields Command 
  You can add the Edit Fields Button from Custom Experience Buttons to your rendering and automatically 
  all datasource fields show up, if you click at it in the editframe. (Or you can pass specific fields)

Edit Rendering Properties Command
  You can add the Edit Rendering Properties Button to your rendering. By click all rendering parameters 
  appear and you can edit them. It is only for having the buttons at the same place, so the editor doesn't 
  have to click More and Edit properties.

Edit Context Item Button
  You can add the the Edit Context Item Button to your Experience Editor Ribbon. Via click all fields of the 
  context item show up in the field editor popup and you are able to edit them.

Steps to do
1 Put in the Foundation.ExperienceEditor Project into your solution
2 Install the packages from /Packages
3 Check the RenderingChromeType.js: if you have customized it already, search for the following lines and move them to your custom file: 

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

Now you can add the Edit Fields and/or Edit Rendering Properties Command  to your Custom Experience Buttons. The Edit Context Item Button is installed via package.
