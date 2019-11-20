using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Menus;
using Rbmk.Radish.Services.Broadcasts.Tabs;
using Rbmk.Radish.Services.Broadcasts.Targets;
using Rbmk.Utils.Broadcasts;
using ReactiveUI;
using Rbmk.Utils.Reactive;
using Splat;

namespace Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Nodes
{
    public class ServerNodeController
    {
        private readonly IBroadcastService _broadcastService;
        private readonly ServerNodeModel _model;

        public ServerNodeController(
            ServerNodeModel model)
            : this(
                Locator.Current.GetService<IBroadcastService>())
        {
            _model = model;
        }

        public ServerNodeController(
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
            var menuItems = new List<CommandMenuItemModel>
            {
                new CommandMenuItemModel
                {
                    Name = "SCAN",
                    CommandTemplate = "SCAN 0",
                    SelectCommand = ReactiveCommand.Create<MenuItemModel>(
                        Select, null, RxApp.MainThreadScheduler)
                },
                new CommandMenuItemModel
                {
                    Name = "SCAN MATCH",
                    CommandTemplate = "SCAN 0 MATCH *",
                    SelectCommand = ReactiveCommand.Create<MenuItemModel>(
                        Select, null, RxApp.MainThreadScheduler)
                },
                new CommandMenuItemModel
                {
                    Name = "KEYS",
                    CommandTemplate = "KEYS *",
                    SelectCommand = ReactiveCommand.Create<MenuItemModel>(
                        Select, null, RxApp.MainThreadScheduler)
                }
            };
            
            return menuItems.ToObservable()
                .ToList()
                .Delay(TimeSpan.FromSeconds(1))
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(list =>
                {
                    _model.MenuItems.AddRange(list);
                });
        }
        
        private void Activate()
        {
            _broadcastService.Broadcast(
                new ActivateTargetIntent(_model.ServerInfo));
        }

        private void Select(MenuItemModel menuItem)
        {
            switch (menuItem)
            {
                case CommandMenuItemModel commandMenuItemModel:
                    _broadcastService.Broadcast(
                        new OpenTabIntent(_model.ServerInfo, commandMenuItemModel.CommandTemplate));
                    break;
            }
        }
    }
}