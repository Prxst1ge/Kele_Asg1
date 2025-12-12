using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions; // Required for ContinueWithOnMainThread
using TMPro; // Required for UI
using UnityEngine.UI;
using UnityEngine.SceneManagement; // To load the AR scene
using System.Collections.Generic;

public class AuthManager : MonoBehaviour
{
    public DatabaseManager dbManager;

    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public TextMeshProUGUI loginFeedbackText;


    public TMP_InputField signupEmailInput;
    public TMP_InputField signupPasswordInput;
    public TextMeshProUGUI signupFeedbackText;
    // Firebase variables
    private FirebaseAuth auth;

    void Start()
    {

        // Making sure code is running
        Debug.Log("AuthManager is running");
        // Initialize Firebase Auth
        auth = FirebaseAuth.DefaultInstance;

        loginFeedbackText.text = "";
        signupFeedbackText.text = "";


    }

    // --- BUTTON FUNCTIONS ---

    public void OnLoginButtonClicked()
    {
        loginFeedbackText.text = "";
        string email = loginEmailInput.text;
        string password = loginPasswordInput.text;

        // Basic validation before sending to server
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            loginFeedbackText.text = "Please enter both email and password.";
            return;
        }

        loginFeedbackText.text = "Logging in...";

        // Call Firebase Login
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted) // Login Failed
            {
                // ERROR HANDLING 
                FirebaseException firebaseEx = task.Exception.GetBaseException() as FirebaseException;
                if (firebaseEx != null)
                {
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                    string friendlyMessage = GetErrorMessage(errorCode);
                    loginFeedbackText.text = friendlyMessage;
                }
                return;
            }
            string currentUserId = task.Result.User.UserId;
            if (auth.CurrentUser != null)
            {
                Debug.Log("User already signed in: " + auth.CurrentUser.UserId);
                dbManager.CheckIfProfileExists(currentUserId, (profileExists) =>
                {
                    if (!profileExists)
                    {
                        // Profile is missing (new user or old user with no data): create the structure
                        dbManager.CreateNewUserProfile(currentUserId, "PineappleTart");
                    }
                });
                LoadGameScene();
            }

            // Success!
            Debug.Log("Login Successful!");

        });
    }

    public void OnSignupButtonClicked()
    {
        signupFeedbackText.text = "";
        string email = signupEmailInput.text;
        string password = signupPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            signupFeedbackText.text = "Please fill in all fields.";
            return;
        }

        signupFeedbackText.text = "Creating Account...";

        // Call Firebase Sign Up
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                // ERROR HANDLING
                FirebaseException firebaseEx = task.Exception.GetBaseException() as FirebaseException;
                if (firebaseEx != null)
                {
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                    string friendlyMessage = GetErrorMessage(errorCode);
                    signupFeedbackText.text = friendlyMessage;
                }
                return;
            }
            if (auth.CurrentUser != null)
            {
                Debug.Log("User already signed in: " + auth.CurrentUser.UserId);
                string newUserId = task.Result.User.UserId;
                dbManager.CreateNewUserProfile(newUserId, "PineappleTart");
                LoadGameScene();
            }


        });
    }

    // --- HELPER FUNCTIONS ---

    private void LoadGameScene()
    {
        // Replace "AR_Scene" with the exact name of your game scene
        SceneManager.LoadScene("MainMenu");
    }

    // This fulfills the "User-friendly error messages" requirement (Week 4 Part 5)
    private string GetErrorMessage(AuthError errorCode)
    {
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                return "Please enter an email address.";
            case AuthError.MissingPassword:
                return "Please enter a password.";
            case AuthError.WrongPassword:
                return "Incorrect password. Please try again.";
            case AuthError.InvalidEmail:
                return "Invalid email format.";
            case AuthError.UserNotFound:
                return "Account not found. Please sign up.";
            case AuthError.EmailAlreadyInUse:
                return "This email is already taken.";
            case AuthError.WeakPassword:
                return "Password is too weak. Use at least 6 characters.";
            case AuthError.InvalidCredential:
                return "Invalid Email or Password.";
            default:
                return "An unknown error occurred.";
        }
    }
}
