using System;
using Microsoft.Extensions.Logging;
using Perpetuum.Log;

namespace Perpetuum.Host
{
    public class HostStateService : IHostStateService
    {
        private static readonly ILogger _logger = Logger.Factory.CreateLogger("HostStateService");

        private HostState _state;

        public HostState State
        {
            get
            {
                return _state;
            }
            set
            {
                if (_state == value)
                    return;

                _state = value;
                OnStateChanged(value);
            }
        }

        public event Action<IHostStateService,HostState> StateChanged;

        private void OnStateChanged(HostState state)
        {
            _logger.LogInformation($">>>> Perpetuum Server State : [{_state}]");
            try
            {
                StateChanged?.Invoke(this,state);
            }
            catch(Exception ex)
            {
                _logger.LogCritical(ex, ex.Message);
            }
        }
    }
}
