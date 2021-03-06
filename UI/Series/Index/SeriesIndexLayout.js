'use strict';
define(
    [
        'marionette',
        'Series/Index/Posters/CollectionView',
        'Series/Index/List/CollectionView',
        'Series/Index/EmptyView',
        'Series/SeriesCollection',
        'Cells/RelativeDateCell',
        'Cells/SeriesTitleCell',
        'Cells/TemplatedCell',
        'Cells/QualityProfileCell',
        'Series/Index/Table/SeriesStatusCell',
        'Series/Index/Table/Row',
        'Shared/Toolbar/ToolbarLayout',
        'Shared/LoadingView'
    ], function (Marionette,
                 PosterCollectionView,
                 ListCollectionView,
                 EmptyView,
                 SeriesCollection,
                 RelativeDateCell,
                 SeriesTitleCell,
                 TemplatedCell,
                 QualityProfileCell,
                 SeriesStatusCell,
                 SeriesIndexRow,
                 ToolbarLayout,
                 LoadingView) {
        return Marionette.Layout.extend({
            template: 'Series/Index/SeriesIndexLayoutTemplate',

            regions: {
                seriesRegion: '#x-series',
                toolbar     : '#x-toolbar'
            },

            columns:
                [
                    {
                        name : 'status',
                        label: '',
                        cell : SeriesStatusCell
                    },
                    {
                        name     : 'title',
                        label    : 'Title',
                        cell     : SeriesTitleCell,
                        cellValue: 'this'
                    },
                    {
                        name : 'seasonCount',
                        label: 'Seasons',
                        cell : 'integer'
                    },
                    {
                        name : 'qualityProfileId',
                        label: 'Quality',
                        cell : QualityProfileCell
                    },
                    {
                        name : 'network',
                        label: 'Network',
                        cell : 'string'
                    },
                    {
                        name : 'nextAiring',
                        label: 'Next Airing',
                        cell : RelativeDateCell
                    },
                    {
                        name    : 'this',
                        label   : 'Episodes',
                        sortable: false,
                        template: 'Series/EpisodeProgressTemplate',
                        cell    : TemplatedCell,
                        className: 'episode-progress-cell'
                    },
                    {
                        name    : 'this',
                        label   : '',
                        sortable: false,
                        template: 'Series/Index/Table/ControlsColumnTemplate',
                        cell    : TemplatedCell
                    }
                ],

            leftSideButtons: {
                type      : 'default',
                storeState: false,
                items     :
                    [
                        {
                            title: 'Add Series',
                            icon : 'icon-plus',
                            route: 'addseries'
                        },
                        {
                            title         : 'RSS Sync',
                            icon          : 'icon-rss',
                            command       : 'rsssync',
                            successMessage: 'RSS Sync Completed',
                            errorMessage  : 'RSS Sync Failed!'
                        },
                        {
                            title         : 'Update Library',
                            icon          : 'icon-refresh',
                            command       : 'refreshseries',
                            successMessage: 'Library was updated!',
                            errorMessage  : 'Library update failed!'
                        }
                    ]
            },

            _showTable: function () {
                this.currentView = new Backgrid.Grid({
                    row       : SeriesIndexRow,
                    collection: SeriesCollection,
                    columns   : this.columns,
                    className : 'table table-hover'
                });

                this._renderView();
                this._fetchCollection();
            },

            _showList: function () {
                this.currentView = new ListCollectionView();
                this._fetchCollection();
            },

            _showPosters: function () {
                this.currentView = new PosterCollectionView();
                this._fetchCollection();
            },


            initialize: function () {
                this.seriesCollection = SeriesCollection;

                this.listenTo(SeriesCollection, 'sync', this._renderView);
                this.listenTo(SeriesCollection, 'remove', this._renderView);
            },


            _renderView: function () {

                if (SeriesCollection.length === 0) {
                    this.seriesRegion.show(new EmptyView());
                    this.toolbar.close();
                }
                else {
                    this.currentView.collection = SeriesCollection;
                    this.seriesRegion.show(this.currentView);

                    this._showToolbar();
                }
            },


            onShow: function () {
                this._showToolbar();
                this._renderView();
                this._fetchCollection();
            },


            _fetchCollection: function () {
                if (SeriesCollection.length === 0) {
                    this.seriesRegion.show(new LoadingView());
                }

                SeriesCollection.fetch();
            },

            _showToolbar: function () {

                if (this.toolbar.currentView) {
                    return;
                }

                var viewButtons = {
                    type         : 'radio',
                    storeState   : true,
                    menuKey      : 'seriesViewMode',
                    defaultAction: 'listView',
                    items        :
                        [
                            {
                                key     : 'posterView',
                                title   : '',
                                tooltip : 'Posters',
                                icon    : 'icon-th-large',
                                callback: this._showPosters
                            },
                            {
                                key     : 'listView',
                                title   : '',
                                tooltip : 'Overview List',
                                icon    : 'icon-th-list',
                                callback: this._showList
                            },
                            {
                                key     : 'tableView',
                                title   : '',
                                tooltip : 'Table',
                                icon    : 'icon-table',
                                callback: this._showTable
                            }
                        ]
                };

                this.toolbar.show(new ToolbarLayout({
                    right  :
                        [
                            viewButtons
                        ],
                    left   :
                        [
                            this.leftSideButtons
                        ],
                    context: this
                }));
            }
        });
    });
