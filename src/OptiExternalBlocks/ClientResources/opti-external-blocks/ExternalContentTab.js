define([
    "dojo/_base/declare",
    "dojo/on",
    "dojo/dom-construct",
    "dojo/dom-class",
    "dojo/request/xhr",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dijit/_WidgetsInTemplateMixin",
    "epi/shell/widget/_ModelBindingMixin"
], function (
    declare,
    on,
    domConstruct,
    domClass,
    xhr,
    _WidgetBase,
    _TemplatedMixin,
    _WidgetsInTemplateMixin,
    _ModelBindingMixin
) {
    return declare("opti-external-blocks.ExternalContentTab", [_WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin, _ModelBindingMixin], {

        templateString: '<div class="external-content-tab">' +
            '<div class="external-content-header">' +
                '<h3>External Content Blocks</h3>' +
            '</div>' +
            '<div class="external-content-type-selector" data-dojo-attach-point="typeSelectorNode"></div>' +
            '<div class="external-content-search">' +
                '<input type="text" data-dojo-attach-point="searchInput" placeholder="Search external content..." class="epi-textbox" />' +
                '<button data-dojo-attach-point="searchButton" class="epi-button">Search</button>' +
            '</div>' +
            '<div class="external-content-results" data-dojo-attach-point="resultsNode"></div>' +
            '<div class="external-content-pagination" data-dojo-attach-point="paginationNode"></div>' +
            '<div class="external-content-preview" data-dojo-attach-point="previewNode"></div>' +
        '</div>',

        baseClass: "external-content-tab",

        apiUrl: "/api/optiexternalblocks",

        templates: [],
        selectedTemplateId: null,
        searchResults: [],
        currentPage: 1,
        pageSize: 20,
        totalCount: 0,
        _selectedItem: null,

        postCreate: function () {
            this.inherited(arguments);
            this._loadStyles();
            this._loadTemplates();
            this._setupEventHandlers();
        },

        _loadStyles: function () {
            // Inject CSS if not already loaded
            var styleId = "opti-external-blocks-styles";
            if (!document.getElementById(styleId)) {
                // Get the module base path by going up from the dojo module folder
                var modulePath = require.toUrl("opti-external-blocks");
                // modulePath points to ClientResources/opti-external-blocks, go up to ClientResources/Styles
                var cssPath = modulePath.replace(/\/opti-external-blocks\/?$/, "/Styles/externalcontent.css");
                domConstruct.create("link", {
                    id: styleId,
                    rel: "stylesheet",
                    type: "text/css",
                    href: cssPath
                }, document.head);
            }
        },

        _setupEventHandlers: function () {
            var self = this;

            on(this.searchButton, "click", function () {
                self._performSearch();
            });

            on(this.searchInput, "keypress", function (e) {
                if (e.keyCode === 13) {
                    self._performSearch();
                }
            });
        },

        _loadTemplates: function () {
            var self = this;

            xhr(this.apiUrl + "/templates", {
                handleAs: "json",
                headers: { "Accept": "application/json" }
            }).then(function (data) {
                self.templates = data;
                self._renderTypeSelector();
            }, function (err) {
                console.error("Error loading templates:", err);
                self._showError("Failed to load content types");
            });
        },

        _renderTypeSelector: function () {
            var self = this;
            domConstruct.empty(this.typeSelectorNode);

            if (this.templates.length === 0) {
                domConstruct.create("p", {
                    innerHTML: "No external content types configured.",
                    className: "external-content-empty"
                }, this.typeSelectorNode);
                return;
            }

            var select = domConstruct.create("select", {
                className: "epi-select external-content-type-select"
            }, this.typeSelectorNode);

            domConstruct.create("option", {
                value: "",
                innerHTML: "Select content type..."
            }, select);

            this.templates.forEach(function (template) {
                domConstruct.create("option", {
                    value: template.id,
                    innerHTML: template.displayName
                }, select);
            });

            on(select, "change", function (e) {
                self.selectedTemplateId = e.target.value || null;
                self.currentPage = 1;
                if (self.selectedTemplateId) {
                    self._performSearch();
                } else {
                    self._clearResults();
                }
            });
        },

        _performSearch: function () {
            if (!this.selectedTemplateId) {
                return;
            }

            var self = this;
            var query = this.searchInput.value || "";

            var url = this.apiUrl + "/search?" +
                "templateId=" + encodeURIComponent(this.selectedTemplateId) +
                "&query=" + encodeURIComponent(query) +
                "&page=" + this.currentPage +
                "&pageSize=" + this.pageSize;

            this._showLoading();

            xhr(url, {
                handleAs: "json",
                headers: { "Accept": "application/json" }
            }).then(function (data) {
                self.searchResults = data.items;
                self.totalCount = data.totalCount;
                self._renderResults();
                self._renderPagination(data.hasMorePages);
            }, function (err) {
                console.error("Error searching content:", err);
                self._showError("Failed to search content");
            });
        },

        _showLoading: function () {
            domConstruct.empty(this.resultsNode);
            domConstruct.create("div", {
                className: "external-content-loading",
                innerHTML: '<span class="epi-loader-small"></span> Loading...'
            }, this.resultsNode);
        },

        _renderResults: function () {
            var self = this;
            domConstruct.empty(this.resultsNode);

            if (this.searchResults.length === 0) {
                domConstruct.create("div", {
                    className: "external-content-empty",
                    innerHTML: "No content found."
                }, this.resultsNode);
                return;
            }

            var list = domConstruct.create("ul", {
                className: "external-content-list"
            }, this.resultsNode);

            // Store items by ID for quick lookup
            this._itemsById = {};

            this.searchResults.forEach(function (item) {
                // Store item by ID
                self._itemsById[item.id] = item;

                var li = domConstruct.create("li", {
                    className: "external-content-item",
                    "data-id": item.id,
                    "data-template-id": self.selectedTemplateId
                }, list);

                if (item.thumbnailUrl) {
                    domConstruct.create("img", {
                        src: item.thumbnailUrl,
                        alt: item.title,
                        className: "external-content-thumb"
                    }, li);
                } else {
                    domConstruct.create("span", {
                        className: "external-content-icon epi-iconDocument"
                    }, li);
                }

                domConstruct.create("span", {
                    innerHTML: item.title,
                    className: "external-content-title"
                }, li);
            });

            // Use event delegation - single click handler on the list
            on(list, "click", function (e) {
                // Find the clicked list item
                var target = e.target;
                while (target && target !== list) {
                    if (domClass.contains(target, "external-content-item")) {
                        var itemId = target.getAttribute("data-id");
                        var item = self._itemsById[itemId];
                        if (item) {
                            self._selectItem(item, target);
                        }
                        break;
                    }
                    target = target.parentNode;
                }
            });
        },

        _renderPagination: function (hasMore) {
            var self = this;
            domConstruct.empty(this.paginationNode);

            var paginationContainer = domConstruct.create("div", {
                className: "external-content-pagination-controls"
            }, this.paginationNode);

            if (this.currentPage > 1) {
                var prevBtn = domConstruct.create("button", {
                    innerHTML: "Previous",
                    className: "epi-button epi-button--small"
                }, paginationContainer);

                on(prevBtn, "click", function () {
                    self.currentPage--;
                    self._performSearch();
                });
            }

            domConstruct.create("span", {
                innerHTML: "Page " + this.currentPage + " (" + this.totalCount + " total)",
                className: "external-content-page-info"
            }, paginationContainer);

            if (hasMore) {
                var nextBtn = domConstruct.create("button", {
                    innerHTML: "Next",
                    className: "epi-button epi-button--small"
                }, paginationContainer);

                on(nextBtn, "click", function () {
                    self.currentPage++;
                    self._performSearch();
                });
            }
        },

        _selectItem: function (item, liElement) {
            // Remove previous selection
            var previousSelected = this.resultsNode.querySelector(".external-content-item--selected");
            if (previousSelected) {
                domClass.remove(previousSelected, "external-content-item--selected");
            }

            // Add selection to clicked item
            if (liElement) {
                domClass.add(liElement, "external-content-item--selected");
            }

            this._selectedItem = item;

            // Load preview
            this._loadPreview(item);
        },

        _loadPreview: function (item) {
            var self = this;
            domConstruct.empty(this.previewNode);

            domConstruct.create("div", {
                className: "external-content-preview-loading",
                innerHTML: "Loading preview..."
            }, this.previewNode);

            var url = this.apiUrl + "/preview/" + this.selectedTemplateId + "/" + encodeURIComponent(item.id);

            xhr(url, {
                handleAs: "json",
                headers: { "Accept": "application/json" }
            }).then(function (data) {
                domConstruct.empty(self.previewNode);

                domConstruct.create("h4", {
                    innerHTML: data.title,
                    className: "external-content-preview-title"
                }, self.previewNode);

                domConstruct.create("div", {
                    innerHTML: data.html,
                    className: "external-content-preview-content"
                }, self.previewNode);

                var insertBtn = domConstruct.create("button", {
                    innerHTML: "Insert Content",
                    className: "epi-button epi-button--primary"
                }, self.previewNode);

                on(insertBtn, "click", function () {
                    self._insertItem(item);
                });
            }, function (err) {
                domConstruct.empty(self.previewNode);
                domConstruct.create("div", {
                    innerHTML: "Failed to load preview",
                    className: "external-content-error"
                }, self.previewNode);
            });
        },

        _insertItem: function (item) {
            var self = this;

            // Show loading state on button
            var insertBtn = this.previewNode.querySelector(".epi-button--primary");
            if (insertBtn) {
                insertBtn.innerHTML = "Creating...";
                insertBtn.disabled = true;
            }

            // Create the block
            xhr.post(this.apiUrl + "/createblock", {
                handleAs: "json",
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },
                data: JSON.stringify({
                    externalId: item.id,
                    templateId: this.selectedTemplateId
                })
            }).then(function (response) {
                self._showInsertFeedback(response.name);
            }, function (err) {
                console.error("Error creating block:", err);
                // Re-enable button on error
                if (insertBtn) {
                    insertBtn.innerHTML = "Insert Content";
                    insertBtn.disabled = false;
                }
                alert("Failed to create content block. Please try again.");
            });
        },

        _showInsertFeedback: function (name) {
            var self = this;

            // Clear the preview panel
            domConstruct.empty(this.previewNode);

            // Remove selection highlight
            var previousSelected = this.resultsNode.querySelector(".external-content-item--selected");
            if (previousSelected) {
                domClass.remove(previousSelected, "external-content-item--selected");
            }

            // Show success message
            domConstruct.create("div", {
                className: "external-content-feedback",
                innerHTML: '<strong>Success!</strong> Block "' + name + '" has been created.<br><small>Find it in the Blocks panel to add to your content area.</small>',
                style: "background: #d4edda; color: #155724; padding: 15px; border-radius: 4px; text-align: center;"
            }, this.previewNode);

            // Clear the selected item
            this._selectedItem = null;

            // Remove feedback after 3 seconds
            setTimeout(function () {
                domConstruct.empty(self.previewNode);
            }, 3000);
        },

        _clearResults: function () {
            domConstruct.empty(this.resultsNode);
            domConstruct.empty(this.paginationNode);
            domConstruct.empty(this.previewNode);
        },

        _showError: function (message) {
            domConstruct.empty(this.resultsNode);
            domConstruct.create("div", {
                innerHTML: message,
                className: "external-content-error"
            }, this.resultsNode);
        }
    });
});
