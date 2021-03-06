﻿'use strict';
define(
    [
        'app',
        'marionette',
        'AddSeries/RootFolders/Layout',
        'AddSeries/Existing/CollectionView',
        'AddSeries/AddSeriesView',
        'Quality/QualityProfileCollection',
        'AddSeries/RootFolders/Collection',
        'Series/SeriesCollection'
    ], function (App,
                 Marionette,
                 RootFolderLayout,
                 ExistingSeriesCollectionView,
                 AddSeriesView,
                 QualityProfileCollection,
                 RootFolderCollection,
                 SeriesCollection) {

        return Marionette.Layout.extend({
            template: 'AddSeries/AddSeriesLayoutTemplate',

            regions: {
                workspace: '#add-series-workspace'
            },

            events: {
                'click .x-import': '_importSeries',
                'click .x-add-new': '_addSeries'
            },

            attributes: {
                id: 'add-series-screen'
            },

            initialize: function () {

                SeriesCollection.fetch();
                QualityProfileCollection.fetch();
                RootFolderCollection.fetch();

                this.rootFolderLayout = new RootFolderLayout();
                this.rootFolderLayout.on('folderSelected', this._folderSelected, this);
            },

            onShow: function () {
                this.workspace.show(new AddSeriesView());
            },

            _folderSelected: function (options) {
                App.vent.trigger(App.Commands.CloseModalCommand);

                this.workspace.show(new ExistingSeriesCollectionView({model: options.model}));
            },

            _importSeries: function () {
                App.modalRegion.show(this.rootFolderLayout);
            },

            _addSeries: function () {
                this.workspace.show(new AddSeriesView());
            }
        });
    });

