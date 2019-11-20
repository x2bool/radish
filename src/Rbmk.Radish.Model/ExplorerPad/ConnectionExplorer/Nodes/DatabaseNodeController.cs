using System;
using System.Reactive.Disposables;
using Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Menus;
using Rbmk.Radish.Services.Broadcasts.Tabs;
using Rbmk.Radish.Services.Broadcasts.Targets;
using Rbmk.Utils.Broadcasts;
using ReactiveUI;
using Rbmk.Utils.Reactive;
using Splat;

namespace Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Nodes
{
    public class DatabaseNodeController
    {
        private readonly IBroadcastService _broadcastService;
        private readonly DatabaseNodeModel _model;

        public DatabaseNodeController(
            DatabaseNodeModel model)
            : this(
                Locator.Current.GetService<IBroadcastService>())
        {
            _model = model;
        }

        public DatabaseNodeController(
            IBroadcastService broadcastService)
        {
            _broadcastService = broadcastService;
        }

        public IDisposable BindActivateCommand()
        {
            _model.ActivateCommand = ReactiveCommand.Create(
                Activate, null, RxApp.MainThreadScheduler);

            return _model.ActivateCommand
                .SubscribeWithLog();
        }

        public IDisposable BindCommands()
        {   
            _model.MenuItems.Clear();
        
            _model.MenuItems.Add(new CommandMenuItemModel
            {
                Name = "GET",
                CommandTemplate = "GET key",
                SelectCommand = ReactiveCommand.Create<MenuItemModel>(
                    Select, null, RxApp.MainThreadScheduler)
            });
            
            _model.MenuItems.Add(new CommandMenuItemModel
            {
                Name = "MGET",
                CommandTemplate = "MGET key1 key2",
                SelectCommand = ReactiveCommand.Create<MenuItemModel>(
                    Select, null, RxApp.MainThreadScheduler)
            });
            
            _model.MenuItems.Add(new CommandMenuItemModel
            {
                Name = "SET",
                CommandTemplate = "SET key value",
                SelectCommand = ReactiveCommand.Create<MenuItemModel>(
                    Select, null, RxApp.MainThreadScheduler)
            });
            
            _model.MenuItems.Add(new CommandMenuItemModel
            {
                Name = "MSET",
                CommandTemplate = "MSET key1 value1 key2 value2",
                SelectCommand = ReactiveCommand.Create<MenuItemModel>(
                    Select, null, RxApp.MainThreadScheduler)
            });
            
            return Disposable.Empty;
        }

        private void Activate()
        {
            _broadcastService.Broadcast(
                new ActivateTargetIntent(_model.DatabaseInfo));
        }

        private void Select(MenuItemModel menuItem)
        {
            switch (menuItem)
            {   
                case CommandMenuItemModel commandMenuItemModel:
                    _broadcastService.Broadcast(
                        new OpenTabIntent(_model.DatabaseInfo, commandMenuItemModel.CommandTemplate));
                    break;
            }
        }
    }
}