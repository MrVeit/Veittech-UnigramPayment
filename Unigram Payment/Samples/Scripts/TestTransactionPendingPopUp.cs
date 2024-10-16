using UnityEngine;

namespace TestExample.UI.PopUp
{
    public sealed class TestTransactionPendingPopUp : BasePopUp
    {

    }

    public abstract class BasePopUp : MonoBehaviour
    {
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}