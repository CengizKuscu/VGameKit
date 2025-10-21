using UnityEngine;
using UnityEngine.UI;
using VGameKit.Runtime.UI.Popup;

namespace Demo.Runtime
{
    public class Popup1 : BasePopup<PopupNames>
    {
        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _continueBtn;

        public override void OnHidePopup()
        {
            _closeBtn.onClick.RemoveListener(OnClose);
            _continueBtn.onClick.RemoveListener(OnClickContinue);
        }

        public override void OnShowBefore()
        {
            _closeBtn.onClick.AddListener(OnClose);
            _continueBtn.onClick.AddListener(OnClickContinue);
        }

        private void OnClose()
        {
            BreakPopupFlow();
        }

        private void OnClickContinue()
        {
            Close();
        }
    }
}