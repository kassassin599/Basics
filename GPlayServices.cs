using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using GooglePlayGames.BasicApi.SavedGame;
using System;

//Written by Naveen annna.
public enum RequestType
{
    Saving = 0,
    Loading = 1
}


public class GPlayServices : MonoBehaviour {
    private static GPlayServices instance;

    public static GPlayServices Instance {
        get {
            return instance;
        }
        set {
            if (instance == null) {
                instance = value;
            }
        }
    }

    public bool EnableCloudSave = true;


    private const string SavedFileName = "SavedGame";
    private const string LeaderboardID = "CgkIiuWi7JMHEAIQAQ";

    public delegate void CoinValueChanged(int value);
    public CoinValueChanged OnCoinValueChanged;

    public int coins = 0;

    public int Coin
    {
        get { return coins; }
        set { coins = value; OnCoinValueChanged(coins); }
    }

    public delegate void LoadGameData(byte[] bytes);
    public event LoadGameData OnSavedGameLoaded;

    public delegate void UserScoreLoaded(int xp);
    public event UserScoreLoaded OnPlayerScoreLoaded;

    private RequestType requestType;
    private byte[] dataToSave;

    private const string authenticateKey = "PlayKey";

    public int score;

    private void Awake() {

        if (Instance) {
            Destroy(this.gameObject);
            Quick.Log("Deleted - Play Services");
        } else {
            Instance = this;
            DontDestroyOnLoad(this);
            Quick.Log("Play service Instance Created");
        }

        //TrackAchievements.OnAchievementUnlocked += OnAchievementUnlocked;

        if (EnableCloudSave) {
            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
           .EnableSavedGames()
           .Build();
            PlayGamesPlatform.InitializeInstance(config);

        }

        if (PlayerPrefs.GetInt(authenticateKey) == 0) {
            // Select the Google Play Games platform as our social platform implementation
            PlayGamesPlatform.Activate();
            Invoke("Authenticate", 2);
        }
    }

    /// <summary>
    /// Called when any achievement gets unlocked form TraceAchievement script
    /// </summary>
    /// <param name="achievement"></param>
    private void OnAchievementUnlocked(Achievement achievement) {
        //Social.ReportProgress(achievement.googleId, 100.0f, (bool success) =>
        //{m
        //});
    }

    /// <summary>
    /// Login
    /// </summary>
    public void Authenticate() {
        Quick.Log("INSIDE AUTHENTICATE");
        if (!Social.localUser.authenticated) {
            Quick.Log("TRYING TO LOGIN USER!!!!!");
            Social.localUser.Authenticate((bool success) => {
                if (success) {
                    LoadDataFromCloud();
                    //LoadPlayerLeaderboardScore();
                    //SubmitScore();
                    Quick.Log("USER LOGGED IN");
                } else {
                    PlayerPrefs.SetInt(authenticateKey, 1);
                    Quick.Log("USER LOGIN FAILED");
                }
            });
        } else {
            LoadDataFromCloud();
            //LoadPlayerLeaderboardScore();
            //SubmitScore();
            Quick.Log("USER ALREADY LOGGED IN");
            //  ((GooglePlayGames.PlayGamesPlatform)Social.Active).SignOut();
        }
    }

    /// <summary>
    /// UI button - Open play service achievement UI
    /// </summary>
    public void ShowAchievements() {
        if (Social.localUser.authenticated) {
            Social.ShowAchievementsUI();
        } else {
            Social.localUser.Authenticate((bool success) => {
                if (success) {
                    Social.ShowAchievementsUI();
                }
            });
        }
    }

    /// <summary>
    /// UI button - Open play service leaderboard UI
    /// </summary>
    public void ShowLeaderboard() {
        if (Social.localUser.authenticated) {
            PlayGamesPlatform.Instance.ShowLeaderboardUI(LeaderboardID);
        } else {
            Social.localUser.Authenticate((bool success) => {
                if (success) {
                    SubmitScore();
                    if (PlayGamesPlatform.Instance == null)
                        Quick.Log("Null Reference is coming over here");
                    PlayGamesPlatform.Instance.ShowLeaderboardUI(LeaderboardID);
                }
            });
        }

    }

    /// <summary>
    /// Submit the total XP to leaderboard 
    /// </summary>
    public void SubmitScore() {
        Quick.Log("SUBMIT SCORE CALLED");
        if (Social.localUser.authenticated) {
            Quick.Log("User authenticated");
            Social.ReportScore(score, LeaderboardID, (bool success) => {
            });
            Quick.Log("LEADERBOARD ID : " + LeaderboardID);
            Quick.Log("Score : " + score);
        }
    }

    public void LoadPlayerLeaderboardScore() {
        if (Social.localUser.authenticated) {
            PlayGamesPlatform.Instance.LoadScores(LeaderboardID, LeaderboardStart.PlayerCentered, 1,
                LeaderboardCollection.Public, LeaderboardTimeSpan.AllTime, PlayerScoreLoaded);
        }
    }

    public void PlayerScoreLoaded(LeaderboardScoreData data) {
        //data.PlayerScore.formattedValue
        int leaderBoardCoin = int.Parse(data.PlayerScore.formattedValue);
    }

    public void SaveDataToCloud(byte[] data) {
        if (!EnableCloudSave)
            return;

        if (Social.localUser.authenticated) {
            requestType = RequestType.Saving;
            dataToSave = data;
            OpenSavedGame(SavedFileName);
        }
    }

    public void LoadDataFromCloud() {
        if (!EnableCloudSave)
            return;

        if (Social.localUser.authenticated) {
            requestType = RequestType.Loading;
            OpenSavedGame(SavedFileName);
        }
    }

    /// <summary>
    /// Open SavedGame 
    /// </summary>
    /// <param name="fileName"></param>
    private void OpenSavedGame(string fileName) {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.OpenWithAutomaticConflictResolution(fileName, DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime, OnSavedGameOpened);
    }

    /// <summary>
    /// Called after opening a SavedGame
    /// </summary>
    /// <param name="status"></param>
    /// <param name="game"></param>
    private void OnSavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata game) {
        if (status == SavedGameRequestStatus.Success) {
            switch (requestType) {
                case RequestType.Saving:
                    TimeSpan timeSpan = new TimeSpan(1, 0, 0);
                    SaveGame(game, dataToSave, timeSpan);
                    break;
                case RequestType.Loading:
                    LoadDataFromMetadata(game);
                    break;
                default:
                    break;
            }

        } else {

        }
    }

    private void SaveGame(ISavedGameMetadata game, byte[] savedData, TimeSpan totalPlayTime) {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

        SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
        builder = builder.WithUpdatedPlayedTime(totalPlayTime);

        SavedGameMetadataUpdate metadataUpdate = builder.Build();
        savedGameClient.CommitUpdate(game, metadataUpdate, savedData, OnSavedGameWritten);
    }

    public void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata game) {
        if (status == SavedGameRequestStatus.Success) {
            Quick.Log("Game Save Data - Saved Successfully");
        } else {
            Quick.Log("Game Save Data - Save Failed");
        }
    }


    private void LoadDataFromMetadata(ISavedGameMetadata game) {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.ReadBinaryData(game, OnSavedGameDataRead);
    }

    public void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] data) {
        if (status == SavedGameRequestStatus.Success) {
            OnSavedGameLoaded?.Invoke(data);
        } else {

        }
    }
}