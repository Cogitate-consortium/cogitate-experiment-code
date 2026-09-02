using System;

namespace Experiment.Triggers.Core
{
    /// <summary>
    /// 200621 Dummy class for now
    /// </summary>
    public class TriggerManager : IDisposable
    {
        protected bool isInitialized { get; private set; }
        private Config config;
        private RuntimeConfig runtimeConfig;

        public TriggerManager(Config config, RuntimeConfig runtimeConfig)
        {
            this.config = config;
            this.runtimeConfig = runtimeConfig;
            isInitialized = true;
        }

        public virtual void Dispose()
        {
            isInitialized = false;
        }

        protected bool ShouldSend(TriggerOutEvent eventType)
        {
            return
                runtimeConfig.shouldFire == null || // Either we do not have a checking function
                runtimeConfig.shouldFire(eventType);// Or it's telling us "GO"
        }

        public class RuntimeConfig
        {
            public Func<TriggerOutEvent, bool> shouldFire;

            public RuntimeConfig(Func<TriggerOutEvent, bool> shouldFire)
            {
                this.shouldFire = shouldFire;
            }
        }

        public class Config
        {
            public bool debug;
        }
    }
}