define([
    "dojo/_base/declare",
    "dijit/_WidgetBase",
    "dijit/_Container",
    "epi-cms/component/ContentNavigationTree",
    "opti-external-blocks/ExternalContentTab"
], function (
    declare,
    _WidgetBase,
    _Container,
    ContentNavigationTree,
    ExternalContentTab
) {
    return declare("opti-external-blocks.ExternalContentComponent", [_WidgetBase, _Container], {

        postCreate: function () {
            this.inherited(arguments);

            // Create the external content tab
            var tab = new ExternalContentTab({
                apiUrl: this.apiUrl || "/api/optiexternalblocks"
            });

            this.addChild(tab);
        }
    });
});
