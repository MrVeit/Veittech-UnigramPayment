namespace UnigramPayment.Core.Common
{
    public interface IUnigramPaymentUtilsCallbacks
    {
        delegate void OnTimeTickLoad(long unixTimeTick);

        event OnTimeTickLoad OnTimeTickLoaded;
    }
}