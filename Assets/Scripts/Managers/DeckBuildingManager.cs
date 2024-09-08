using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeckBuildingManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _warningScreen = null;
    [SerializeField]
    private GameObject _proceedButton = null;

    [SerializeField]
    private List<GameObject> _flemishCards;
    [SerializeField]
    private List<GameObject> _frenchCards;

    private List<GameObject> _deck = new List<GameObject>();
    private List<GameObject> _aiDeck = new List<GameObject>();
    private ManpowerLimit _manpowerLimit;
    private UIDeckbuilding _deckbuildingUI;

    private int _playerInt = -1;
    private int _enemyManpower = 0;
    private GameSides _playerSide = GameSides.Flemish;
    private GameSides _aiSide = GameSides.Flemish;

    private void Awake()
    {
        _enemyManpower = 0;
        _manpowerLimit = FindObjectOfType<ManpowerLimit>();
        _deckbuildingUI = FindObjectOfType<UIDeckbuilding>();
    }

    internal void SetUpSides(int id)
    {
        if (_playerInt >= 0 && _playerInt != id)
        {
            //Reset the Player's deck and UI
            _deckbuildingUI.ResetDeckUI();
            _deck.Clear();
            _manpowerLimit.ResetManpower();
        }

        switch (id)
        {
            case 0:
                _playerSide = GameSides.Flemish;
                _aiSide = GameSides.French;
                break;
            case 1:
                _playerSide = GameSides.French;
                _aiSide = GameSides.Flemish;
                break;
            default:
                _playerSide = GameSides.Flemish;
                _aiSide = GameSides.French;
                break;
        }

        _playerInt = id;
    }

    public void CheckToGoToGame()
    {
        if (_manpowerLimit.CurrentManpower == 20)
        {
            GoToGame();
            AudioManager.instance.PlayButtonBattle();
        }
        else
        {
            ShowWarning();
            AudioManager.instance.PlayButtonForwardClick();
        }
    }

    public void GoToGame()
    {
        if (_manpowerLimit.CurrentManpower < 1)
            return;

        RandomlyMakeAIDeck();

        //Set the deck
        var deck = ScriptableObject.CreateInstance<DeckScriptableObject>();
        deck.Deck = _deck;

        var aiDeck = ScriptableObject.CreateInstance<DeckScriptableObject>();
        aiDeck.Deck = _aiDeck;

        StaticDataHelper.playerDeck = deck;
        StaticDataHelper.playerSide = _playerSide;
        StaticDataHelper.aiSide = _aiSide;
        StaticDataHelper.aiDeck = aiDeck;

        //Go to the main game
        SceneTransitionManager.instance.TransitionToGame();
        AudioManager.instance.PlayGameMusic();
    }

    public void RandomlyFillOutDeck()
    {
        List<GameObject> usedDeck;

        if (_playerSide == GameSides.Flemish)
            usedDeck = _flemishCards;
        else
            usedDeck = _frenchCards;

        while (_manpowerLimit.CurrentManpower != 20)
        {
            int randIndex = UnityEngine.Random.Range(0, usedDeck.Count);
            GameObject randCard = usedDeck[randIndex];

            TroopCard randCardData = randCard.GetComponent<TroopCard>();

            if (randCardData.ManpowerCost + _manpowerLimit.CurrentManpower > 20)
                continue;

            _deck.Add(randCard);
            _manpowerLimit.UpdateManpower(randCardData.ManpowerCost);
        }

        GoToGame();
    }

    private void RandomlyMakeAIDeck()
    {
        List<GameObject> usedDeck;

        if (_playerSide == GameSides.Flemish)
            usedDeck = _frenchCards;
        else
            usedDeck = _flemishCards;

        while (_enemyManpower != 20)
        {
            int randIndex = UnityEngine.Random.Range(0, usedDeck.Count);
            GameObject randCard = usedDeck[randIndex];

            TroopCard randCardData = randCard.GetComponent<TroopCard>();

            if (randCardData.ManpowerCost + _enemyManpower > 20)
                continue;

            _aiDeck.Add(randCard);
            _enemyManpower += randCardData.ManpowerCost;
        }


    }

    private void ShowWarning()
    {
        _warningScreen.SetActive(true);
        if (_manpowerLimit.CurrentManpower < 1)
            _proceedButton.SetActive(false);
        else
            _proceedButton.SetActive(true);
    }

    public void AddCard(GameObject card)
    {
        _deck.Add(card);
    }

    public bool RemoveCard(GameObject card)
    {
        if (!_deck.Contains(card))
            return false;

        _deck.Remove(card);
        return true;
    }
}
