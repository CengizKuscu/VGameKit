using MessagePipe;
using VContainer;
using VGameKit.Runtime.UI.Menu;
using VGameKit.Runtime.UI.Menu.Events;

namespace Demo.Runtime
{
    public enum GameMenuName
    {
        MainMenu,
        PauseMenu,
        SettingsMenu,
        LevelComplete
    }

    public class OpenMenuEvent : BaseOpenMenuEvent<GameMenuName, MenuData>
    {
        public OpenMenuEvent(GameMenuName menuName, MenuData menuData) : base(menuName, menuData)
        {
        }
    }
    
    public class CloseMenuEvent : BaseCloseMenuEvent<GameMenuName>
    {
        public CloseMenuEvent(GameMenuName menuName) : base(menuName)
        {
        }
    }
    
    public class CloseOtherMenuEvent : BaseCloseOthersMenuEvent<GameMenuName>
    {
        public GameMenuName[] KeepMenuNames { get; private set; }
        
        public CloseOtherMenuEvent(params GameMenuName[] keepMenuNames) : base(keepMenuNames)
        {
            KeepMenuNames = keepMenuNames;
        }
    }
    
    public class MenuManager : BaseMenuManager<GameMenuName>
    {
        [Inject] private readonly ISubscriber<OpenMenuEvent> _openMenuSubscriber;
        [Inject] private readonly ISubscriber<CloseMenuEvent> _closeMenuSubscriber;
        [Inject] private readonly ISubscriber<CloseOtherMenuEvent> _closeOtherMenuSubscriber;
        
        [Inject] private readonly LevelCompletePresenter _levelCompletePresenter;

        public override void Subscriptions()
        {
            _openMenuSubscriber.Subscribe(e=> OpenMenuHandler(e)).AddTo(_bagBuilder);
            _closeMenuSubscriber.Subscribe(e=> CloseMenuHandler(e)).AddTo(_bagBuilder);
            _closeOtherMenuSubscriber.Subscribe(e=> CloseOtherMenuHandler(e)).AddTo(_bagBuilder);
        }

        protected override void OpenMenu(GameMenuName menuName, MenuData menuData)
        {
            switch (menuName)
            {
                case GameMenuName.LevelComplete:
                    Open<LevelCompletePresenter, LevelCompleteData>(_levelCompletePresenter, menuData);
                    break;
            }
        }
        
        
        private void OpenMenuHandler(OpenMenuEvent openMenuEvent)
        {
            OpenMenu(openMenuEvent.MenuName, openMenuEvent.MenuData);
        }

        private void CloseMenuHandler(CloseMenuEvent closeMenuEvent)
        {
            CloseMenu(closeMenuEvent.MenuName);
        }

        private void CloseOtherMenuHandler(CloseOtherMenuEvent closeOtherMenuEvent)
        {
            CloseOthers(closeOtherMenuEvent.KeepMenuNames);
        }
        
        
    }

    public class LevelCompletePresenter : BaseMenuPresenter<GameMenuName, LevelCompleteData, LevelCompleteView>
    {
        public override MenuMode MenuMode => MenuMode.Additive;
        public override GameMenuName MenuName => GameMenuName.LevelComplete;
    }

    public class LevelCompleteData : MenuData
    {
    }

    public class LevelCompleteView : BaseMenuView
    {
    }
}