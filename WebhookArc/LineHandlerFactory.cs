using Autofac;

namespace WebhookArc
{
    public interface ILineHandlerFactory
    {
        ILineHandler NewInstance();
    }

    public class LineHandlerFactory : ILineHandlerFactory
    {
        private readonly ILifetimeScope lifetimeScope;

        public LineHandlerFactory(ILifetimeScope lifetimeScope)
        {
            this.lifetimeScope = lifetimeScope;
        }

        public ILineHandler NewInstance() => lifetimeScope.Resolve<ILineHandler>();
    }
}
