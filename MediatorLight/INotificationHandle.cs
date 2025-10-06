namespace MediatorLite
{
    public interface INotificationHandle
    {
        public interface INotificationHandler<TNotification>
    where TNotification : INotification
        {
            Task Handle(TNotification notification, CancellationToken cancellationToken);
        }
    }
}
