using UnityEngine;
using UnityEngine.Events;

public class CampfireModel : MonoBehaviour
{
    private HexagonModel _currentHexagonModel = null;
    public HexagonModel CurrentHexagonModel
    {
        get { return _currentHexagonModel; }
        set { _currentHexagonModel = value; }
    }

    private TroopModel _currentDefender = null;

    public TroopModel CurrentDefender
    {
        get { return _currentDefender; }
        set { _currentDefender = value; }
    }

    private CampfireManager _campfireManager = null;
    public CampfireManager CampfireManager
    {
        set { _campfireManager = value; }
    }

    private bool _isDefended = false;
    public bool IsDefended
    {
        get { return _isDefended; }
    }

    private CampfireView _campfireView = null;

    public UnityEvent OnCampsiteCaptured;

    private void Awake()
    {
        _campfireView = GetComponent<CampfireView>();
    }

    public void SetCurrentDefender(TroopModel troop)
    {
        if (!_campfireManager) return;

        //Check if a troop is defending an empty campfire
        if (CurrentDefender == null && troop != null)
        {
            if (troop.HP < 1)
                return;
            _isDefended = true;
            CurrentDefender = troop;

            _campfireManager.UpdateCampfireCaptureCount();
            _campfireManager.CheckToStartCountdown();

            if (CurrentDefender != null)
            {
                _campfireManager.SpawnFlameSymbol(gameObject.transform);
                OnCampsiteCaptured?.Invoke();
            }
        }
        else if (troop == null || troop != CurrentDefender)
        {
            //Check if a troop is taking over a campfire from an enemy
            CurrentDefender = troop;
            _campfireManager.UpdateCampfireCaptureCount();

            _campfireManager.ResetCountdown();
            _campfireManager.CheckToStartCountdown();

            if (CurrentDefender != null)
            {
                _campfireManager.SpawnFlameSymbol(gameObject.transform);
                OnCampsiteCaptured?.Invoke();
            }
        }
    }

    internal void ShowWarning()
    {
        _campfireView.ScaleUpFlame();
    }

    internal void SetToBeDestroyed()
    {
        _campfireView.SpawnSmokeRFX();
    }
}
