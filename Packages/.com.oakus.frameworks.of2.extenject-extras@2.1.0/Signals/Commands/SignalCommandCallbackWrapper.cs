﻿using System;

namespace Zenject
{
    public class SignalCommandCallbackWrapper<TCommand, TSignal> : IDisposable where TCommand : Command<TSignal>
    {
        readonly SignalBus _signalBus;
        readonly TickableManager _tickableManager;
        readonly Pool<TCommand, TSignal> _commandPool;
        readonly Type _signalType;
        readonly object _identifier;

        public SignalCommandCallbackWrapper(
            SignalBindingBindInfo bindInfo,
            Pool<TCommand, TSignal> commandPool,
            SignalBus signalBus,
            TickableManager tickableManager)
        {
            if (bindInfo == null)
                return;
            
            _signalType = bindInfo.SignalType;
            _identifier = bindInfo.Identifier;
            _signalBus = signalBus;
            _tickableManager = tickableManager;
            _commandPool = commandPool;
            
            signalBus.SubscribeId(bindInfo.SignalType, _identifier, OnSignalFired);
        }

        void OnSignalFired(object signal)
        {
            var command = _commandPool.Spawn(_commandPool, (TSignal) signal);
            if (!_tickableManager.Has(command))
                _tickableManager.Add(command);
            command.ExecuteCommand();
        }

        public void Dispose()
        {
            _signalBus.UnsubscribeId(_signalType, _identifier, OnSignalFired);
        }
    }
}