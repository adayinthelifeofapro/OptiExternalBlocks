define([
    "dojo/_base/declare",
    "epi-cms/component/ContentNavigationTreeComponentDescriptor"
], function (
    declare,
    ContentNavigationTreeComponentDescriptor
) {
    return declare("opti-external-blocks.ExternalContentTabDescriptor", [ContentNavigationTreeComponentDescriptor], {

        name: "externalContent",
        title: "External Content Blocks",
        description: "Browse and select external content from the Graph API",
        sortOrder: 400,
        iconClass: "epi-iconExternal",

        // Widget to use for this tab
        widgetType: "opti-external-blocks/ExternalContentTab",

        // Settings for the widget
        settings: {
            apiUrl: "/api/optiexternalblocks"
        }
    });
});
